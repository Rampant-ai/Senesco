using System;
using System.Collections.Generic;
using System.Windows.Forms;
using log4net;
using Senesco.Client;
using Senesco.Client.Communication;
using Senesco.Client.Events;
using Senesco.Client.Utility;
using System.Windows.Input;

namespace Senesco.Forms
{
   public partial class ChatForm : Form
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(ChatForm));

      private readonly SenescoController m_controller = null;
      private UserListForm m_userListForm = null;
      private ActionQueue m_backgroundActions = new ActionQueue();

      // Required delegates for Invoke calls.
      private delegate void OneStringDelegate(string str);
      private delegate void NoParamDelegate();

      // Current emote state -- controlled by KeyDown and KeyUp events.
      private bool m_emote = false;
      private static List<Keys> s_emoteKeys = new List<Keys>()
      {
         Keys.Alt, Keys.Control, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey
      };

      public ChatForm()
      {
         // Initialize logging system.
         Log.InitLogging();

         // Start up this window.
         InitializeComponent();

         // Restore window position.
         //WindowUtils.RestoreWindowPosition(this, null, WindowUtils.DefaultPosition.GlobalDefault);

         // Create and initialize the controller for all the internal logic.
         m_controller = new SenescoController();
         m_controller.ProgressUpdated += progressUpdated;
         m_controller.Connected += connected;
         m_controller.Disconnected += disconnected;
         m_controller.ChatReceived += chatReceived;
         m_controller.UserListUpdate += userListUpdate;
         //m_controller.PmReceived += pmReceived;
         //m_controller.UserInfoReceived += userInfoReceived;

         // Clear the window title initially.
         SetWindowTitle(null);

         // Synchronize the bookmark menu with the current bookmark files.
         UpdateBookmarks();

         // Process the command-line arguments.
         //ProcessStartup();
      }

      #region Window Control

      private void SetWindowTitle(string message)
      {
         if (this.InvokeRequired)
         {
            this.Invoke(new OneStringDelegate(SetWindowTitle), message);
         }
         else
         {
            // If no message given, just "Senesco".
            if (string.IsNullOrEmpty(message))
               this.Text = "Senesco";
            // Otherwise, suffix the message.
            else
               this.Text = string.Format("Senesco: {0}", message);
         }
      }

      private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
      {
         m_controller.Shutdown();
         Environment.Exit(0);
      }

      #endregion

      #region Bookmarks

      /// <summary>
      /// This method updates the bookmark menus.  Currently, there are two separate menus.
      /// One is for a quick-connect list, and the other is for picking which bookmark to use
      /// to use for the auto-login feature.
      /// </summary>
      public void UpdateBookmarks()
      {
         // Get the full list of bookmark names from the controller.
         List<string> bookmarkNames = m_controller.GetBookmarkNames();
         string autoConnectBookmark = m_controller.GetAutoConnectBookmark();

         // Clear the old list of bookmarks.
         s_log.InfoFormat("Clearing old bookmarks from root MenuItem {0}", m_connectRecent.Name);

         m_connectRecent.DropDownItems.Clear();
         m_autoConnect.DropDownItems.Clear();

         // If the list is no good or empty, disable the root menus.
         if (bookmarkNames == null || bookmarkNames.Count == 0)
         {
            m_connectRecent.Enabled = false;
            m_autoConnect.Enabled = false;
            return;
         }

         // Enable both root menus.
         m_connectRecent.Enabled = true;
         m_autoConnect.Enabled = true;

         // Create each bookmark as a new submenu item.
         s_log.InfoFormat("Updating {0} and {1} with bookmarks", m_connectRecent.Name, m_autoConnect.Name);
         int i = 1;
         foreach (string bookmarkName in bookmarkNames)
         {
            ToolStripMenuItem mi = new ToolStripMenuItem();
            mi.Name = String.Format("bookmark{0}", i);
            mi.Text = bookmarkName;
            mi.Click += new EventHandler(bookmark_Click);
            m_connectRecent.DropDownItems.Add(mi);

            ToolStripMenuItem ac = new ToolStripMenuItem();
            ac.Name = String.Format("autoconnect{0}", i);
            ac.Text = bookmarkName;
            ac.Click += new EventHandler(autoConnect_Click);
            //ac.IsCheckable = true;
            ac.Checked = (autoConnectBookmark == bookmarkName); // Check the one that's currently set.
            m_autoConnect.DropDownItems.Add(ac);

            i++;
         }
      }

      private void bookmark_Click(object sender, EventArgs e)
      {
         ToolStripMenuItem bookmarkItem = sender as ToolStripMenuItem;
         if (bookmarkItem == null)
         {
            s_log.ErrorFormat("Bad sender for bookmark_Click()!");
            return;
         }

         s_log.InfoFormat("Bookmark '{0}' was clicked from menu.", bookmarkItem.Text);

         Server server = m_controller.GetBookmark(bookmarkItem.Text as string);

         if (server == null)
            return;

         // Update the window title.
         SetWindowTitle(String.Format("Connecting to '{0}'...", server.ServerName));

         // Unhide the progress bar.
         m_progressBar.Visible = true;
         m_progressBar.Height = 25;

         // Connect to the server.
         Status result = m_controller.Connect(server);

         // Hide the progress bar when finished.
         m_progressBar.Visible = false;
         m_progressBar.Height = 0;

         // Reset the window title if connecting failed.
         if (result != Status.Success)
         {
            SetWindowTitle(null);
         }
      }

      private void autoConnect_Click(object sender, EventArgs e)
      {
         // Change the auto-connect bookmark to this one (if the selection has changed).
         MenuItem selected = sender as MenuItem;
         if (selected == null)
            return;
         string selectedBookmark = selected.Text as string;

         // If the user clicked the one that's already checked, then remove the auto-connect.
         if (selected.Checked == false)
            RegistryUtils.RemoveAutoConnectBookmark();
         else
         {
            // Set the user's new selection in the registry.
            RegistryUtils.SetAutoConnectBookmark(selectedBookmark);

            // Update the checks to reflect the new selection (like radio buttons).
            foreach (MenuItem mi in m_autoConnect.DropDownItems)
               mi.Checked = (selectedBookmark == mi.Text as string);
         }
      }

      #endregion

      #region Chat

      private void chatEntry_KeyDown(object sender, KeyEventArgs e)
      {
         // Is the emote key down?
         if (s_emoteKeys.Contains(e.KeyCode))
         {
            m_emote = true;
         }

         // If pressing return or enter, submit the chat.
         if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
         {
            TextBox textBox = sender as TextBox;
            string textToSend = textBox.Text;

            // Clear the GUI text box and make sure the newline doesn't show up after.
            textBox.Text = string.Empty;
            e.SuppressKeyPress = true;
            
            // Send the text to the server.
            m_backgroundActions.Add(() => m_controller.SendChat(textToSend, m_emote));
         }
      }

      private void chatEntry_KeyUp(object sender, KeyEventArgs e)
      {
         if (s_emoteKeys.Contains(e.KeyCode))
         {
            m_emote = false;
         }
      }

      #endregion

      #region Controller Event Handlers

      private void progressUpdated(object sender, ProgressUpdatedEventArgs e)
      {
         // Update the progress bar.
         m_progressBar.Value = e.ProgressPercent;
      }

      private void connected(object sender, ConnectedEventArgs e)
      {
         // Update the window title with the connected server name.
         SetWindowTitle(m_controller.ConnectedServerName);

         // Request the user list to populate the user list window.
         m_controller.GetUserList();

         // Open the user list window if necessary.
         OpenUserListForm();

         // Enable the "Disconnect" menu option since we're connected.
         m_disconnectMenu.Enabled = true;
      }

      private void disconnected(object sender, EventArgs e)
      {
         // Disconnect from the server with the given reason.
         m_controller.Disconnect("User Quit.");

         // Clear the window title.
         SetWindowTitle(null);

         // Close and dispose the user list window.
         if (m_userListForm != null)
         {
            m_userListForm.Close();
            m_userListForm = null;
         }

         // Disable the "Disconnect" menu option since no longer connected.
         m_disconnectMenu.Enabled = false;
      }

      private void chatReceived(object sender, ChatReceivedEventArgs e)
      {
         AddChat(e.Text);
      }

      private void AddChat(string chat)
      {
         // Make sure only one thread at a time adds text.
         if (m_chatView.InvokeRequired)
         {
            m_chatView.Invoke(new OneStringDelegate(AddChat), chat);
         }
         else
         {
            lock (m_chatView)
            {
               m_chatView.AppendText(String.Format("\n{0}", chat));
            }
         }
      }

      #endregion

      #region File Menu

      private void connectToolStripMenuItem_Click(object sender, EventArgs e)
      {
         // show connect dialog
      }

      private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
      {
         m_controller.Disconnect("User disconnected");
      }
      
      private void exitToolStripMenuItem_Click(object sender, EventArgs e)
      {
         m_controller.Shutdown();
         Application.Exit();
      }

      #endregion

      #region Config Menu

      private void userSettingsToolStripMenuItem_Click(object sender, EventArgs e)
      {
         // Show user settings
      }

      private void soundsToolStripMenuItem_Click(object sender, EventArgs e)
      {
         // Show sounds window
      }

      #endregion

      #region Chat Menu

      private void openChatLogToolStripMenuItem_Click(object sender, EventArgs e)
      {
         m_controller.OpenChatLog();
      }

      private void revealChatLogToolStripMenuItem_Click(object sender, EventArgs e)
      {
         m_controller.ShowChatLogs();
      }

      #endregion

      #region Window Menu

      private void userListToolStripMenuItem_Click(object sender, EventArgs e)
      {
         // Show user list window if not already visible.
         OpenUserListForm();
      }

      private void OpenUserListForm()
      {
         if (this.InvokeRequired)
         {
            this.Invoke(new NoParamDelegate(OpenUserListForm));
         }
         else
         {
            // Open a UserListWindow if one is not already opened.
            if (m_userListForm == null)
            {
               m_userListForm = new UserListForm(m_controller);
               m_userListForm.Closed += new EventHandler(userListWindowClosed);
               UserListFormUpdate();
               m_userListForm.Show(this);
            }
         }
      }

      private void userListUpdate(object sender, UserListUpdateEventArgs e)
      {
         if (e != null)
         {
            if (m_userListForm.InvokeRequired)
            {
               m_userListForm.Invoke(new NoParamDelegate(UserListFormUpdate));
            }
            else
            {
               UserListFormUpdate();
            }
         }
      }
      
      private void UserListFormUpdate()
      {
         // Update the existing window.
         if (m_userListForm != null)
            m_userListForm.RefreshUserList();
      }

      private void userListWindowClosed(object sender, EventArgs e)
      {
         // The window is closed, null the reference to it.
         m_userListForm = null;
      }

      #endregion

      #region Help Menu

      private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
      {
         // Show the About window
      }

      private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
      {
         m_controller.LaunchWebsite();
      }

      #endregion
   }
}

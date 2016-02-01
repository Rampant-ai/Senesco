using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using Senesco.Client;
using Senesco.Client.Communication;
using Senesco.Client.Events;
using Senesco.Client.Utility;
using Senesco.WPF.Windows.Config;
using Senesco.WPF.Windows.Dialog;
using Senesco.WPF.Windows.Main;

namespace Senesco
{
   public class Presenter
   {
      #region Members and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(Presenter));

      private readonly SenescoController m_controller = null;
      private ChatWindow m_chatWindow = null;
      private UserListWindow m_userListWindow = null;

      private ActionQueue m_backgroundActions = new ActionQueue();
      private Dispatcher m_dispatcher = null;

      public delegate void NoParamDelegate();
      public delegate void OneStringDelegate(string str1);
      public delegate void TwoStringDelegate(string str1, string str2);
      public delegate void PmReceivedDelegate(string sendingNick, int sendingUserId, string message);

      private static List<Key> s_emoteKeys = new List<Key>()
      {
         Key.LeftAlt, Key.RightAlt,
         Key.LeftCtrl, Key.RightCtrl
      };

      private static List<Key> s_submitKeys = new List<Key>()
      {
         Key.Return, Key.Enter
      };

      public Presenter(string[] startupArgs)
      {
         // Initialize logging system.
         Log.InitLogging();

         // Keep a handle to the current main GUI thread, so that we can have proper object
         // ownership when it comes time for this presenter to present new GUI objects.
         m_dispatcher = Dispatcher.CurrentDispatcher;

         // Create and initialize the controller for all the internal logic.
         m_controller = new SenescoController();
         m_controller.ProgressUpdated += progressUpdated;
         m_controller.Connected += connected;
         m_controller.Disconnected += disconnected;
         m_controller.ChatReceived += chatReceived;
         m_controller.UserListUpdate += userListUpdate;
         m_controller.PmReceived += pmReceived;
         m_controller.UserInfoReceived += userInfoReceived;
         m_controller.NewVersionAvailable += newVersionAvailable;

         // Create and show the main chat window.
         // Note that this creator is run in the main GUI thread, so this chat window is properly owned,
         // but other windows we create may be in different threads so we'll have to take extra care.
         m_chatWindow = new ChatWindow(this, m_controller);
         m_chatWindow.Show();

         // Process the command-line arguments.
         ProcessStartup(startupArgs);
      }

      private void ProcessStartup(string[] startupArgs)
      {
         s_log.InfoFormat("Processing startup events.");

         try
         {
            // Do nothing if no arguments.
            if (startupArgs == null || startupArgs.Length == 0)
               return;

            // Dump arguments if debug logging is enabled.
            if (s_log.IsDebugEnabled)
            {
               for (int i = 0; i < startupArgs.Length; i++)
                  s_log.DebugFormat("{0}: {1}", i, startupArgs[i]);
            }

            // Try to parse the first object as a bookmark.  Returns null upon failure.
            Server server = FileUtils.GetBookmark(new FileInfo(startupArgs[0]));

            // Connect to that server.
            if (server != null)
               InitiateConnect(server);
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception processing startup events: {0}", e.Message);
         }
      }

      #endregion

      #region Controller Event Handlers

      private void progressUpdated(object sender, ProgressUpdatedEventArgs e)
      {
         m_chatWindow.ProgressUpdated(e.ProgressPercent);
      }

      private void connected(object sender, ConnectedEventArgs e)
      {
         // Request the user list to populate the user list window.
         m_controller.GetUserList();

         // Update the state of the Chat window.
         m_chatWindow.Connected();

         // Open the user list window if necessary.
         ShowUserListWindow();
      }

      /// <summary>
      /// Event handler when the controller indicates we've become disconnected.
      /// </summary>
      private void disconnected(object sender, EventArgs e)
      {
         // Proceeding like we actively disconnected leaves us in the best state.
         DisconnectedByServer();
      }

      private void chatReceived(object sender, ChatReceivedEventArgs e)
      {
         if (e == null)
            return;

         // Old unformatted way.
         //m_chatWindow.AddChatText(e.Text);

         // Process the text into a fancy formatted paragraph.
         m_chatWindow.AddChatParagraph(e.Text, UrlExpression, LinkClick);
      }

      private void userListUpdate(object sender, UserListUpdateEventArgs e)
      {
         // If the window is open, refresh it.
         if (m_userListWindow != null)
            m_userListWindow.RefreshUserList();
      }

      private void pmReceived(object sender, PrivateMsgEventArgs e)
      {
         m_dispatcher.Invoke(new PmReceivedDelegate(ShowPmReceiveWindow), e.SendingNick, e.SendingUserId, e.Message);
      }

      private void ShowPmReceiveWindow(string sendingNick, int sendingUserId, string message)
      {
         PmReceiveWindow pmWindow = new PmReceiveWindow(m_chatWindow, m_controller, sendingNick, sendingUserId, message);
         pmWindow.Show();
      }

      private void userInfoReceived(object sender, UserInfoEventArgs e)
      {
         m_dispatcher.Invoke(new OneStringDelegate(ShowUserInfoWindow), e.UserInfo);
      }

      private void ShowUserInfoWindow(string userInfo)
      {
         UserInfoWindow uiw = new UserInfoWindow(m_chatWindow, userInfo);
         uiw.Show();
      }

      #endregion

      #region Chat

      /// <summary>
      /// Controls system behavior when an input key is pressed.
      /// </summary>
      internal void KeyPressed(Key key, TextBox textBox)
      {
         // If pressing one of the submit keys, submit the chat.
         if (s_submitKeys.Contains(key))
         {
            string textToSend = textBox.Text;

            // Clear the GUI text box.
            textBox.Text = string.Empty;

            // Are any of the emote key modifiers held down?
            bool emote = false;
            foreach (Key k in s_emoteKeys)
            {
               if (Keyboard.IsKeyDown(k))
                  emote = true;
            }

            // Send the text to the server.
            m_backgroundActions.Add(() => m_controller.SendChat(textToSend, emote));

            // Local echo for testing chat processing without connecting to a server.
            if (Keyboard.IsKeyDown(Key.Pause))
            {
               m_chatWindow.AddChatParagraph(textToSend, UrlExpression, LinkClick);
            }
         }
      }

      /// <summary>
      /// http://geekswithblogs.net/casualjim/archive/2005/12/01/61722.aspx
      /// </summary>
      private static readonly Regex UrlExpression1 = new Regex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");

      /// <summary>
      /// http://daringfireball.net/2010/07/improved_regex_for_matching_urls
      /// </summary>
      private static readonly Regex UrlExpression2 = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");
      private static readonly Regex UrlExpression3 = new Regex(@"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");
      
      /// <summary>
      /// Regular expression for detecting URLs in free text.
      /// </summary>
      private static Regex UrlExpression = UrlExpression2;
      
      /// <summary>
      /// Callback for clicking on a hyperlink.
      /// </summary>
      private void LinkClick(object sender, RoutedEventArgs e)
      {
         Hyperlink link = (Hyperlink)sender;

         string url;
         if (link.NavigateUri == null)
            url = link.Tag as string;
         else
            url = link.NavigateUri.OriginalString;

         m_controller.LaunchURL(url);
      }

      #endregion

      #region User List

      internal void ShowUserListWindow()
      {
         if (m_dispatcher.CheckAccess() == false)
         {
            m_dispatcher.Invoke(new NoParamDelegate(ShowUserListWindow));
         }
         else
         {
            // Open a UserListWindow if one is not already opened.
            if (m_userListWindow == null)
            {
               m_userListWindow = new UserListWindow(m_chatWindow, m_controller);
               m_userListWindow.Closed += new EventHandler(userListWindowClosed);
               m_userListWindow.RefreshUserList();
               m_userListWindow.Show();
            }

            // Activate and focus the window.
            m_userListWindow.Activate();
            m_userListWindow.Focus();
         }
      }

      private void userListWindowClosed(object sender, EventArgs e)
      {
         // The window is closed, null the reference to it.
         m_userListWindow = null;
      }

      #endregion

      #region Connect/Disconnect

      /// <summary>
      /// Method to control behavior of the Connect Dialog.
      /// </summary>
      internal void ShowConnect()
      {
         if (m_dispatcher.CheckAccess() == false)
         {
            m_dispatcher.Invoke(new NoParamDelegate(ShowConnect));
         }
         else
         {
            // Display new connection window.
            ConnectWindow connectWindow = new ConnectWindow(m_chatWindow, m_controller);
            connectWindow.ShowDialog();

            // If the window did not configure a server, do nothing.
            if (connectWindow.ConfiguredServer == null)
            {
               s_log.InfoFormat("Connection window did not configure a server.");
               return;
            }

            InitiateConnect(connectWindow.ConfiguredServer);
         }
      }

      internal void InitiateConnect(string bookmarkName)
      {
         Server server = m_controller.GetBookmark(bookmarkName);
         m_backgroundActions.Add(() => InitiateConnectDelegate(server));
      }

      internal void InitiateConnect(Server server)
      {
         m_backgroundActions.Add(() => InitiateConnectDelegate(server));
      }

      private Status InitiateConnectDelegate(Server server)
      {
         if (server == null)
            return Status.Failure;

         // Update the Chat window state.
         m_chatWindow.SetConnecting(server.ServerName);

         // Connect to the server.
         Status result = m_controller.Connect(server);

         // Hide the progress bar when finished.

         // Reset the window title if connecting failed.
         if (result != Status.Success)
         {
            m_chatWindow.SetConnectFailed();
         }

         return result;
      }

      /// <summary>
      /// Method called to initiate a disconnect.
      /// </summary>
      internal void DisconnectedByUser()
      {
         // Disconnect from the server with the given reason.
         m_backgroundActions.Add(() => m_controller.Disconnect("User Quit."));

         // Update the Chat window state.
         m_chatWindow.Disconnected(true);

         // Close and dispose the user list window.
         if (m_userListWindow != null)
         {
            m_userListWindow.Close();
            m_userListWindow = null;
         }
      }

      /// <summary>
      /// Method called to initiate a disconnect.
      /// </summary>
      internal void DisconnectedByServer()
      {
         if (m_dispatcher.CheckAccess() == false)
         {
            m_dispatcher.Invoke(new NoParamDelegate(DisconnectedByServer));
         }
         else
         {
            // Update the Chat window state.
            m_chatWindow.Disconnected(false);

            // Close and dispose the user list window.
            if (m_userListWindow != null)
            {
               m_userListWindow.Close();
               m_userListWindow = null;
            }
         }
      }

      internal void Exit()
      {
         m_controller.Shutdown();
         if (Application.Current != null)
            Application.Current.Shutdown();
         Environment.Exit(0);
      }

      #endregion

      #region Other Actions

      internal void ShowLogs()
      {
         m_backgroundActions.Add(m_controller.ShowChatLogs);
      }

      internal void OpenLogs()
      {
         m_backgroundActions.Add(m_controller.OpenChatLog);
      }

      internal void ShowAboutBox()
      {
         if (m_dispatcher.CheckAccess() == false)
         {
            m_dispatcher.Invoke(new NoParamDelegate(ShowAboutBox));
         }
         else
         {
            // Show the About window.
            AboutWindow aboutWindow = new AboutWindow(m_chatWindow);
            aboutWindow.Show();
         }
      }

      internal void ShowUserConfig()
      {
         if (m_dispatcher.CheckAccess() == false)
         {
            m_dispatcher.Invoke(new NoParamDelegate(ShowUserConfig));
         }
         else
         {
            // Display user config dialog.
            UserConfig config = new UserConfig(m_chatWindow, m_controller.LocalUser);
            config.ShowDialog();

            // If the user canceled, this will be null.
            User user = config.OutputUser;
            if (user == null)
               return;

            // If the user settings were changed, submit them to the server.
            if (m_controller.LocalUser.SettingsDiffer(user))
               m_controller.SetUserInfo(user);
         }
      }

      internal void ShowSoundConfig()
      {
         if (m_dispatcher.CheckAccess() == false)
         {
            m_dispatcher.Invoke(new NoParamDelegate(ShowSoundConfig));
         }
         else
         {
            // Display sound config dialog.
            SoundsConfig config = new SoundsConfig(m_chatWindow, m_controller.SoundController);
            config.ShowDialog();
         }
      }

      #endregion

      #region New Version Updater

      private void newVersionAvailable(object sender, NewVersionEventArgs e)
      {
         m_dispatcher.Invoke(new TwoStringDelegate(ShowUpgradePrompt), e.CurrentVersion, e.NewVersion);
      }

      private void ShowUpgradePrompt(string currentVersion, string newVersion)
      {
         UpgradePrompt prompt = new UpgradePrompt(m_chatWindow, currentVersion, newVersion);
         if (prompt.ShowDialog() == true)
         {
            m_controller.PerformUpgrade(newVersion);
            Exit();
         }
      }

      #endregion
   }
}

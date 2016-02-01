using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using Senesco.Client;
using Senesco.Client.Communication;
using Senesco.Client.Utility;
using System.Text.RegularExpressions;

namespace Senesco.WPF.Windows.Main
{
   /// <summary>
   /// Interaction logic for ChatWindow.xaml
   /// </summary>
   public partial class ChatWindow : Window, ISenescoWindow
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(ChatWindow));

      private readonly Presenter m_presenter = null;
      private readonly SenescoController m_controller = null;

      private Brush m_progressBarOriginalColor = null;

      // GUI delegate definitions.
      public delegate void NoParamDelegate();
      public delegate void UserListDelegate(List<User> addList, List<User> removeList, bool delta);
      public delegate void ServerDelegate(Server server);
      public delegate void OneIntegerDelegate(int i);
      public delegate void OneBooleanDelegate(bool b);
      public delegate void OneStringDelegate(string str1);
      public delegate void ChatParagraphDelegate(string text, Regex urlExpression, RoutedEventHandler urlHandler);
      public delegate void TwoStringOneIntegerDelegate(string str1, string str2, int i);

      public ChatWindow(Presenter presenter, SenescoController controller)
      {
         m_presenter = presenter;
         m_controller = controller;

         // Start up this window.
         InitializeComponent();

         // Restore window position.
         WindowUtils.RestoreWindowPosition(this, null, WindowUtils.DefaultPosition.GlobalDefault);

         // Clear the window title initially.
         SetWindowTitle(null);

         // Synchronize the bookmark menu with the current bookmark files.
         UpdateBookmarks();
      }

      #region Window Management

      private void SetWindowTitle(string message)
      {
         if (string.IsNullOrEmpty(message))
         {
            // If no message given, just "Senesco".
            this.Title = "Senesco";
         }
         else
         {
            // Otherwise, suffix the message.
            this.Title = string.Format("Senesco: {0}", message);
         }
      }

      public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         WindowUtils.SaveWindowPosition(this);
      }

      public void Window_LocationChanged(object sender, EventArgs e)
      {
         WindowUtils.SaveWindowPosition(this);
      }

      public void SaveWindowPosition()
      {
         ConfigSettings.UserSettings.ChatWindowLeft = this.Left;
         ConfigSettings.UserSettings.ChatWindowTop = this.Top;
         ConfigSettings.UserSettings.ChatWindowWidth = this.Width;
         ConfigSettings.UserSettings.ChatWindowHeight = this.Height;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.ChatWindowLeft;
         this.Top = ConfigSettings.UserSettings.ChatWindowTop;
         this.Width = ConfigSettings.UserSettings.ChatWindowWidth;
         this.Height = ConfigSettings.UserSettings.ChatWindowHeight;
      }

      #endregion

      #region Window Commands

      public static RoutedCommand ConnectCmd = new RoutedCommand();
      public static RoutedCommand ReconnectCmd = new RoutedCommand();
      public static RoutedCommand DisconnectCmd = new RoutedCommand();
      public static RoutedCommand QuitCmd = new RoutedCommand();
      public static RoutedCommand UserConfigCmd = new RoutedCommand();
      public static RoutedCommand SoundConfigCmd = new RoutedCommand();
      public static RoutedCommand OpenLogsCmd = new RoutedCommand();
      public static RoutedCommand RevealLogsCmd = new RoutedCommand();
      public static RoutedCommand UserListCmd = new RoutedCommand();
      public static RoutedCommand AboutCmd = new RoutedCommand();
      public static RoutedCommand WebsiteCmd = new RoutedCommand();

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
         m_connectRecent.Items.Clear();
         m_autoConnect.Items.Clear();

         // If the list is no good or empty, disable the root menus.
         if (bookmarkNames == null || bookmarkNames.Count == 0)
         {
            m_connectRecent.IsEnabled = false;
            m_autoConnect.IsEnabled = false;
            return;
         }

         // Enable both root menus.
         m_connectRecent.IsEnabled = true;
         m_autoConnect.IsEnabled = true;

         // Create each bookmark as a new submenu item.
         s_log.InfoFormat("Updating {0} and {1} with bookmarks", m_connectRecent.Name, m_autoConnect.Name);
         int i = 1;
         foreach (string bookmarkName in bookmarkNames)
         {
            MenuItem mi = new MenuItem();
            mi.Name = String.Format("bookmark{0}", i);
            mi.Header = bookmarkName;
            mi.Click += bookmark_Click;
            m_connectRecent.Items.Add(mi);

            MenuItem ac = new MenuItem();
            ac.Name = String.Format("autoconnect{0}", i);
            ac.Header = bookmarkName;
            ac.Click += autoConnect_Click;
            ac.IsCheckable = true;
            ac.IsChecked = (autoConnectBookmark == bookmarkName); // Check the one that's currently set.
            m_autoConnect.Items.Add(ac);

            i++;
         }
      }

      private void autoConnect_Click(object sender, RoutedEventArgs e)
      {
         // Change the auto-connect bookmark to this one (if the selection has changed).
         MenuItem selected = sender as MenuItem;
         if (selected == null)
            return;
         string selectedBookmark = selected.Header as string;

         // If the user clicked the one that's already checked, then remove the auto-connect.
         if (selected.IsChecked == false)
            RegistryUtils.RemoveAutoConnectBookmark();
         else
         {
            // Set the user's new selection in the registry.
            RegistryUtils.SetAutoConnectBookmark(selectedBookmark);

            // Update the checks to reflect the new selection (like radio buttons).
            foreach (MenuItem mi in m_autoConnect.Items)
               mi.IsChecked = (selectedBookmark == mi.Header as string);
         }
      }

      #endregion

      #region Chat

      private void chatEntry_KeyDown(object sender, KeyEventArgs e)
      {
         m_presenter.KeyPressed(e.Key, sender as TextBox);
      }

      internal void AddChatText(string text)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new OneStringDelegate(AddChatText), text);
         }
         else
         {
            // Old method: Suffix the chat string to the existing paragraph.
            m_chatView.AppendText(String.Format("\n{0}", text));
         }
      }

      internal void AddChatParagraph(string text, Regex urlExpression, RoutedEventHandler urlHandler)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new ChatParagraphDelegate(AddChatParagraph), text, urlExpression, urlHandler);
         }
         else
         {
            // Add the new paragraph to the existing document in the control.
            Paragraph paragraph = ProcessText(text, urlExpression, urlHandler);
            m_chatView.Document.Blocks.Add(paragraph);

            // If the user is at or lower than 80% of the total scrollback, that's
            // pretty near the bottom, so keep the chat scrolled all the way down.
            // This is how we achieve auto-scroll while still allowing the user to
            // scroll up while chat may be actively ongoing.

            // The total height scrollback is "ExtentHeight".  The lowest point
            // currently visible is "VerticalOffset" plus "ViewportHeight".
            if (m_chatView.VerticalOffset + m_chatView.ViewportHeight >= (0.8 * m_chatView.ExtentHeight))
               m_chatView.ScrollToEnd();
         }
      }

      /// <summary>
      /// Helper method to create a Paragraph from a free text string.
      /// The text is parsed for URLs, and each URL is attached to the given callback.
      /// This method is not in the presenter because the Paragraph must be created (owned) by the GUI thread.
      /// </summary>
      private Paragraph ProcessText(string text, Regex urlExpression, RoutedEventHandler urlHandler)
      {
         // Create a new Paragraph for this chat.
         Paragraph paragraph = new Paragraph();

         // Get the URL matches using a regular expression.
         // Intersperse the regular text with the detected URLs.
         int lastIndex = 0;
         foreach (Match match in urlExpression.Matches(text))
         {
            // Copy the regular text substring from the last index to the next URL.
            if (match.Index != lastIndex)
            {
               string regularText = text.Substring(lastIndex, match.Index - lastIndex);
               paragraph.Inlines.Add(new Run(regularText));
            }

            // Create a Hyperlink for the URL match.
            Hyperlink link = new Hyperlink(new Run(match.Value));
            try
            {
               link.NavigateUri = new Uri(match.Value);
            }
            catch
            {
               link.NavigateUri = null;
               link.Tag = match.Value;
            }
            link.Click += urlHandler;
            link.Cursor = Cursors.Arrow;

            // Add the hyperlink to the document.
            paragraph.Inlines.Add(link);

            // Update the last matched position.
            lastIndex = match.Index + match.Length;
         }

         // Copy any regular text left into a final run.
         if (lastIndex < text.Length)
            paragraph.Inlines.Add(new Run(text.Substring(lastIndex)));

         return paragraph;
      }

      #endregion

      #region Connection Controls

      internal void Connected()
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new NoParamDelegate(Connected));
         }
         else
         {
            // Update the window title with the connected server name.
            SetWindowTitle(m_controller.ConnectedServerName);

            // Enable the "Disconnect" menu option since we're connected.
            m_disconnectMenu.IsEnabled = true;

            // Hide the progress bar now that it is completed successfully.
            ProgressBarVisible(false);
         }
      }

      internal void ProgressUpdated(int percent)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new OneIntegerDelegate(ProgressUpdated), percent);
         }
         else
         {
            // Update the progress bar.
            m_progressBar.Value = percent;
         }
      }

      internal void SetConnecting(string serverName)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new OneStringDelegate(SetConnecting), serverName);
         }
         else
         {
            // Update the window title.
            SetWindowTitle(String.Format("Connecting to '{0}'...", serverName));

            // Display the progress bar.
            ProgressBarError(false);
            ProgressBarVisible(true);
         }
      }

      internal void SetConnectFailed()
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new NoParamDelegate(SetConnectFailed));
         }
         else
         {
            ProgressBarError(true);
            SetWindowTitle(null);
         }
      }

      private void ProgressBarVisible(bool visible)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new OneBooleanDelegate(ProgressBarVisible), visible);
         }
         else
         {
            m_progressBar.Value = 0;
            m_progressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
         }
      }

      private void ProgressBarError(bool errorState)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new OneBooleanDelegate(ProgressBarError), errorState);
         }
         else
         {
            // Preserve the original color the first time this comes through.
            if (m_progressBarOriginalColor == null)
            {
               m_progressBarOriginalColor = m_progressBar.Foreground;
            }

            // If there was an error, show a red full progress bar.
            if (errorState)
            {
               m_progressBar.Value = 100;
               m_progressBar.Visibility = Visibility.Visible;
               m_progressBar.Foreground = Brushes.Red;
            }
            else
            {
               m_progressBar.Value = 0;
               m_progressBar.Visibility = Visibility.Collapsed;
               m_progressBar.Foreground = m_progressBarOriginalColor;
            }
         }
      }

      private void bookmark_Click(object sender, RoutedEventArgs e)
      {
         MenuItem bookmarkItem = sender as MenuItem;
         if (bookmarkItem == null)
         {
            s_log.ErrorFormat("Bad sender for bookmark_Click()!");
            return;
         }

         s_log.InfoFormat("Bookmark '{0}' was clicked from menu.", bookmarkItem.Header);

         m_presenter.InitiateConnect(bookmarkItem.Header as string);
      }

      internal void Disconnected(bool byUser)
      {
         if (CheckAccess() == false)
         {
            Dispatcher.Invoke(new OneBooleanDelegate(Disconnected), byUser);
         }
         else
         {
            // Clear the window title.
            SetWindowTitle(null);

            // Disable the "Disconnect" menu option since no longer connected.
            m_disconnectMenu.IsEnabled = false;

            // If the user did not perform the disconnect, that's considered an error state.
            if (byUser == false)
            {
               ProgressBarError(true);
            }
         }
      }

      #endregion

      #region Event Handlers

      private void ConnectHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.ShowConnect();
      }

      private void ReconnectHandler(object sender, RoutedEventArgs e)
      {
         m_controller.SetReconnect(m_reconnect.IsChecked);
      }

      private void DisconnectHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.DisconnectedByUser();
      }

      private void QuitHandler(object sender, EventArgs e)
      {
         m_presenter.Exit();
      }

      private void UserConfigHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.ShowUserConfig();
      }

      private void SoundConfigHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.ShowSoundConfig();
      }

      private void OpenLogsHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.OpenLogs();
      }

      private void RevealLogsHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.ShowLogs();
      }

      private void UserListHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.ShowUserListWindow();
      }

      private void AboutHandler(object sender, RoutedEventArgs e)
      {
         m_presenter.ShowAboutBox();
      }

      private void WebsiteHandler(object sender, RoutedEventArgs e)
      {
         m_controller.LaunchWebsite();
      }

      #endregion
   }
}

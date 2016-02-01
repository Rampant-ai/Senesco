using System;
using System.Windows;
using log4net;
using Senesco.Client;
using Senesco.Client.Communication;
using Senesco.Client.Utility;
using Senesco.WPF.Windows.Main;

namespace Senesco.WPF.Windows.Dialog
{
   /// <summary>
   /// Interaction logic for ConnectWindow.xaml
   /// </summary>
   public partial class ConnectWindow : Window, ISenescoWindow
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(ConnectWindow));

      private SenescoController m_controller = null;

      public Server ConfiguredServer = null;

      public ConnectWindow(Window owner, SenescoController controller)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         m_controller = controller;
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
         ConfigSettings.UserSettings.ConnectWindowLeft = this.Left;
         ConfigSettings.UserSettings.ConnectWindowTop = this.Top;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.ConnectWindowLeft;
         this.Top = ConfigSettings.UserSettings.ConnectWindowTop;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         // Initial control focus.
         if (m_serverName != null)
            m_serverName.Focus();
      }

      private void TextChanged(object sender, RoutedEventArgs e)
      {
         // When any text is changed, re-enable the Save button.
         if (m_saveButton != null && m_saveButton.IsEnabled == false)
            m_saveButton.IsEnabled = true;
      }

      private void CancelButton_Click(object sender, RoutedEventArgs e)
      {
         this.ConfiguredServer = null;
         this.Close();
      }

      private void ConnectButton_Click(object sender, RoutedEventArgs e)
      {
         // Persist the settings and close the window.
         this.ConfiguredServer = MakeServerFromControls();
         this.Close();
      }

      private void SaveButton_Click(object sender, RoutedEventArgs e)
      {
         m_controller.AddBookmark(MakeServerFromControls());

         // Disable this button now that the bookmark is saved.
         m_saveButton.IsEnabled = false;

         // Update the parent window's list of bookmarks.
         ChatWindow chatWindow = this.Owner as ChatWindow;
         if (chatWindow != null)
            chatWindow.UpdateBookmarks();
      }

      private Server MakeServerFromControls()
      {
         try
         {
            Server server = new Server();

            server.ServerName = m_serverName.Text;
            server.Address = m_addressText.Text;
            server.Nick = m_nickText.Text;
            server.Icon = 31337;
            server.LoginName = m_usernameText.Text;
            server.Password = m_passwordText.Password;

            return server;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception setting values from window: {0}", e.Message);
            return null;
         }
      }
   }
}

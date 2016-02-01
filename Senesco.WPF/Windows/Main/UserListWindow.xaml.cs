using System;
using System.Windows;
using System.Windows.Controls;
using Senesco.Client;
using Senesco.Client.Utility;
using Senesco.WPF.Windows.Dialog;

namespace Senesco.WPF.Windows.Main
{
   /// <summary>
   /// Interaction logic for UserListWindow.xaml
   /// </summary>
   public partial class UserListWindow : Window, ISenescoWindow
   {
      private SenescoController m_controller;

      // GUI delegate definitions.
      public delegate Status NoParamStatusDelegate();

      #region Window Management

      public UserListWindow(Window owner, SenescoController controller)
      {
         // Set up this new window.
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.RightOfParent);

         // Configure the internals and user objects.
         m_controller = controller;
         CreateContextMenu(m_userListBox);
         RefreshUserList();
      }

      private void CreateContextMenu(ListBox listBox)
      {
         if (listBox == null || listBox.ContextMenu != null)
            return;
         listBox.ContextMenu = new ContextMenu();

         MenuItem sendPm = new MenuItem();
         sendPm.Header = "Send Private Message";
         sendPm.Click += SendPm_EventHandler;
         listBox.ContextMenu.Items.Add(sendPm);

         MenuItem getUserInfo = new MenuItem();
         getUserInfo.Header = "Get User Info";
         getUserInfo.Click += GetUserInfo_EventHandler;
         listBox.ContextMenu.Items.Add(getUserInfo);
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
         ConfigSettings.UserSettings.UserListWindowLeft = this.Left;
         ConfigSettings.UserSettings.UserListWindowTop = this.Top;
         ConfigSettings.UserSettings.UserListWindowWidth = this.Width;
         ConfigSettings.UserSettings.UserListWindowHeight = this.Height;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.UserListWindowLeft;
         this.Top = ConfigSettings.UserSettings.UserListWindowTop;
         this.Width = ConfigSettings.UserSettings.UserListWindowWidth;
         this.Height = ConfigSettings.UserSettings.UserListWindowHeight;
      }

      #endregion

      public Status RefreshUserList()
      {
         if (CheckAccess() == false)
         {
            return (Status)Dispatcher.Invoke(new NoParamStatusDelegate(RefreshUserList), null);
         }
         else
         {
            // Get the list of users from the model.
            if (m_controller == null)
               return Status.Failure;

            // TODO: make this a simple binding instead.
            m_userListBox.ItemsSource = m_controller.UserList;

            // Only enable the buttons and context menu if there are users.
            bool hasUsers = (m_userListBox.Items.Count > 0);
            m_sendPmButton.IsEnabled = hasUsers;
            m_getUserInfoButton.IsEnabled = hasUsers;
            m_userListBox.ContextMenu.IsEnabled = hasUsers;

            return Status.Success;
         }
      }

      private User GetSelectedUser()
      {
         if (m_userListBox == null)
            return null;
         if (m_userListBox.SelectedItem == null)
            return null;
         return m_userListBox.SelectedItem as User;
      }

      private void SendPm_EventHandler(object sender, RoutedEventArgs e)
      {
         User user = GetSelectedUser();
         if (user == null)
            return;

         Window pm = new PmSendWindow(this, m_controller, user, null);
         pm.Show();
      }

      private void GetUserInfo_EventHandler(object sender, RoutedEventArgs e)
      {
         User user = GetSelectedUser();
         if (user == null)
            return;

         // Send GetUserInfo transaction with the currently selected user.
         m_controller.GetUserInfo(user);
      }
   }
}

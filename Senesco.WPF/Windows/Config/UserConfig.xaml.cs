using System;
using System.Windows;
using Senesco.Client;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Config
{
   /// <summary>
   /// Interaction logic for UserConfig.xaml
   /// </summary>
   public partial class UserConfig : Window, ISenescoWindow
   {
      #region Members and Creator

      public User OutputUser = null;

      public UserConfig(Window owner, User user)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         // Initialize the fields with the given user.
         SetFieldsFromUser(user);
      }

      #endregion

      #region Window Management

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
         ConfigSettings.UserSettings.UserConfigLeft = this.Left;
         ConfigSettings.UserSettings.UserConfigTop = this.Top;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.UserConfigLeft;
         this.Top = ConfigSettings.UserSettings.UserConfigTop;
      }

      #endregion

      #region Data Handling

      private void SetFieldsFromUser(User user)
      {
         if (user == null)
            return;

         m_nick.Text = user.Username;
         m_icon.Text = user.IconId.ToString();
         m_ignorePrivateMsgs.IsChecked = user.IgnorePrivateMsgs;
         m_ignorePrivateChat.IsChecked = user.IgnorePrivateChat;
         m_ignoreAutoResponse.Text = user.IgnoreAutoResponse;
      }

      private User CreateUserFromFields()
      {
         User user = new User();

         user.Username = m_nick.Text;
         user.IgnorePrivateMsgs = m_ignorePrivateMsgs.IsChecked.Value;
         user.IgnorePrivateChat = m_ignorePrivateChat.IsChecked.Value;
         user.IgnoreAutoResponse = m_ignoreAutoResponse.Text;

         // If the icon string doesn't parse, set a value that won't pass validation.
         if (int.TryParse(m_icon.Text, out user.IconId) == false)
            user.IconId = -1;

         return user;
      }

      private void SaveButton_Click(object sender, RoutedEventArgs e)
      {
         // When saving, create the output user for the caller who invoked
         // this window to use as output.
         OutputUser = CreateUserFromFields();

         // If the generated user is valid, we're done.
         if (OutputUser.IsValid())
         {
            this.Close();
            return;
         }
         
         // Don't close the window if the values are invalid.
         //FIXME: use styles to highlight the error fields.
         if (String.IsNullOrEmpty(m_nick.Text))
         {
            m_nick.Text = "unnamed.senesco";
         }
         else
         {
            // If the nick was okay, the icon must be invalid.
            // This will need better checking if there's more extensive validation.
            m_icon.Text = "0";
         }

         OutputUser = null;
      }

      #endregion
   }
}

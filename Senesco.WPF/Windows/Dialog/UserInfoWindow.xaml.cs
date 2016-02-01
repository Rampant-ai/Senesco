using System;
using System.Windows;
using System.Windows.Input;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Dialog
{
   /// <summary>
   /// Interaction logic for UserInfoWindow.xaml
   /// </summary>
   public partial class UserInfoWindow : Window, ISenescoWindow
   {
      public UserInfoWindow(Window owner, string userInfoText)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         m_userInfoText.AppendText(userInfoText);
      }

      private void DismissButton_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
      }

      private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
      {
         // Close the window immediately if Escape is pressed.
         if (Keyboard.IsKeyDown(Key.Escape))
            this.Close();
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
         ConfigSettings.UserSettings.UserInfoWindowLeft = this.Left;
         ConfigSettings.UserSettings.UserInfoWindowTop = this.Top;
         ConfigSettings.UserSettings.UserInfoWindowWidth = this.Width;
         ConfigSettings.UserSettings.UserInfoWindowHeight = this.Height;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.UserInfoWindowLeft;
         this.Top = ConfigSettings.UserSettings.UserInfoWindowTop;
         this.Width = ConfigSettings.UserSettings.UserInfoWindowWidth;
         this.Height = ConfigSettings.UserSettings.UserInfoWindowHeight;
      }
   }
}

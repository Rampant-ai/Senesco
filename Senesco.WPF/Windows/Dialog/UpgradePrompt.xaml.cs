using System;
using System.Windows;
using System.Windows.Input;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Dialog
{
   /// <summary>
   /// Interaction logic for UpgradePrompt.xaml
   /// </summary>
   public partial class UpgradePrompt : Window, ISenescoWindow
   {
      public UpgradePrompt(Window owner, string currentVersion, string newVersion)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         m_promptText.Text = FormatVersionMessage(currentVersion, newVersion);
      }

      private string FormatVersionMessage(string currentVersion, string newVersion)
      {
         return String.Format("A new version is available!\n\nWould you like to upgrade from version {0} to version {1}?",
            currentVersion, newVersion);
      }

      private void YesButton_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }

      private void NoButton_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = false;
      }

      private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
      {
         // Close the window immediately if Escape is pressed.
         if (Keyboard.IsKeyDown(Key.Escape))
         {
            DialogResult = false;
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
         ConfigSettings.UserSettings.UpgradeWindowLeft = this.Left;
         ConfigSettings.UserSettings.UpgradeWindowTop = this.Top;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.UpgradeWindowLeft;
         this.Top = ConfigSettings.UserSettings.UpgradeWindowTop;
      }
   }
}

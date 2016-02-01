using System;
using System.Windows;
using System.Windows.Controls;
using Senesco.Client.Sound;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Config
{
   /// <summary>
   /// Interaction logic for SoundsConfig.xaml
   /// </summary>
   public partial class SoundsConfig : Window, ISenescoWindow
   {
      public SoundsConfig(Window owner, SoundController soundController)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         if (soundController != null)
            DataContext = soundController;
      }

      /// <summary>
      /// When the window is closed, save the changes to the profile currently
      /// being edited.
      /// </summary>
      private void Window_Closed(object sender, EventArgs e)
      {
         // Save any changes to the sound profile.
         SoundController soundController = DataContext as SoundController;
         soundController.CurrentProfile.Save();
      }

      /// <summary>
      /// When the selection is changed, save the changes to the profile we're
      /// switching from.
      /// </summary>
      private void SoundProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         // If no prior selection, do nothing.
         if (e.RemovedItems == null || e.RemovedItems.Count == 0)
            return;

         SoundProfile lastProfile = e.RemovedItems[0] as SoundProfile;
         if (lastProfile == null)
            return;

         lastProfile.Save();
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
         ConfigSettings.UserSettings.SoundsConfigLeft = this.Left;
         ConfigSettings.UserSettings.SoundsConfigTop = this.Top;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.SoundsConfigLeft;
         this.Top = ConfigSettings.UserSettings.SoundsConfigTop;
      }
   }
}

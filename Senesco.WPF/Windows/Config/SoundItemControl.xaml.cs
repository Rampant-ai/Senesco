using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Senesco.Client.Sound;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Config
{
   /// <summary>
   /// Interaction logic for SoundItemControl.xaml
   /// </summary>
   public partial class SoundItemControl : UserControl
   {
      public SoundItemControl()
      {
         InitializeComponent();
      }

      private void SoundItemControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         SoundItem soundItem = DataContext as SoundItem;
         if (soundItem == null)
            return;

         if (soundItem.ReadOnly)
            m_changeFileButton.Visibility = Visibility.Collapsed;
         else
            m_changeFileButton.Visibility = Visibility.Visible;
      }

      private void PlayFile(object sender, RoutedEventArgs e)
      {
         SoundItem soundItem = DataContext as SoundItem;
         if (soundItem == null)
            return;

         SoundUtils.PlaySound(soundItem.Name);
      }

      private void SelectFile(object sender, RoutedEventArgs e)
      {
         SoundItem soundItem = DataContext as SoundItem;
         if (soundItem == null)
            return;

         string oldPath = soundItem.FilePath;

         // Open a dialog to have the user pick a file.
         // NOTE: I made a default filter for WAV files but for all I know the
         // SoundPlayer class will play others.  I added the "All files" filter
         // for this situation.
         OpenFileDialog ofd = new OpenFileDialog();
         ofd.Title = "Select .wav file";
         ofd.Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*";
         ofd.Multiselect = false;
         
         DirectoryInfo di = SoundController.GetSoundProfileDirectory();
         ofd.InitialDirectory = di.FullName;

         bool? result = ofd.ShowDialog();

         // If the user canceled or otherwise exited, do nothing.
         if (result == null || result == false)
            return;

         // Set the new path and submit it for loading.
         soundItem.FilePath = ofd.FileName;
         if (soundItem.Submit() != Status.Success)
         {
            soundItem.FilePath = oldPath;
         }
      }
   }
}

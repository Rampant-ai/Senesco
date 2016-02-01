using System.ComponentModel;
using Senesco.Client.Utility;

namespace Senesco.Client.Sound
{
   public class SoundItem : INotifyPropertyChanged
   {
      #region Fields and Properties
      
      // Internal name used to configure and play the sound.
      private SoundEffect m_name;
      public SoundEffect Name
      {
         get { return m_name; }
         set
         {
            m_name = value;
            NotifyPropertyChanged("Name");
         }
      }

      // User interface label string.
      private string m_label;
      public string Label
      {
         get { return m_label; }
         set
         {
            m_label = value;
            NotifyPropertyChanged("Label");
         }
      }

      // Actual path to the sound file.
      private string m_filePath;
      public string FilePath
      {
         get { return m_filePath; }
         set
         {
            m_filePath = value;
            NotifyPropertyChanged("FilePath");
         }
      }

      // Read-only flag, for allowing user modification.
      private bool m_readOnly;
      public bool ReadOnly
      {
         get { return m_readOnly; }
      }

      #endregion

      #region Creator

      public SoundItem(SoundEffect name, string label, bool readOnly)
      {
         Name = name;
         Label = label;
         m_readOnly = readOnly;
      }

      #endregion

      /// <summary>
      /// Submits the current SoundItem for loading.
      /// </summary>
      /// <returns>True if error, otherwise false.</returns>
      public Status Submit()
      {
         if (string.IsNullOrEmpty(FilePath))
            return Status.Failure;
         return SoundUtils.SetSound(Name, FilePath);
      }

      /// <summary>
      /// Plays this sound if possible.
      /// </summary>
      public void Play()
      {
         SoundUtils.PlaySound(Name);
      }

      #region INotifyPropertyChanged

      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged(string info)
      {
         if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(info));
      }

      #endregion
   }
}

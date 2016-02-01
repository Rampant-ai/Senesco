
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Senesco.Client.Utility;

namespace Senesco.Client.Sound
{
   public class SoundController : INotifyPropertyChanged
   {
      #region Constants

      private const string c_profileDirectory = @"SoundProfiles";
      private const string c_soundFileExtension = @".wav";

      #endregion

      #region Members and Creator

      private object m_profileLock = new object();

      private SoundProfile m_currentProfile = null;
      public SoundProfile CurrentProfile
      {
         get { return m_currentProfile; }
         set
         {
            bool changed = (m_currentProfile != value);

            m_currentProfile = value;

            if (m_currentProfile != null && changed)
            {
               // Load all the new sounds.
               m_currentProfile.SubmitAll();

               // Persist the new selection.
               SaveProfile(m_currentProfile.DisplayName);
            }
            
            NotifyPropertyChanged("CurrentProfile");
         }
      }

      private List<SoundProfile> m_profiles = new List<SoundProfile>();
      public List<SoundProfile> Profiles
      {
         get { return m_profiles; }
         set
         {
            m_profiles = value;
            NotifyPropertyChanged("Profiles");
         }
      }

      public SoundController()
      {
         LoadProfiles();

         RestoreProfileSelection();
      }

      #endregion

      #region Save/Load Profile Selection

      private void RestoreProfileSelection()
      {
         CurrentProfile = FindProfile(ConfigSettings.UserSettings.SoundProfile);
         if (CurrentProfile == null)
            CurrentProfile = FindProfile(SoundProfile.ProfileName_Empty);
      }

      public void SelectProfile(string profileName)
      {
         CurrentProfile = FindProfile(profileName);
      }

      private void SaveProfile(string profileName)
      {
         ConfigSettings.UserSettings.SoundProfile = profileName;
         ConfigSettings.UserSettings.Save();
      }

      private SoundProfile FindProfile(string whichProfile)
      {
         // Linear search to match the profile by name.
         foreach (SoundProfile profile in Profiles)
         {
            // Match by name, ignore case.
            if (string.Compare(whichProfile, profile.DisplayName, true) == 0)
            {
               return profile;
            }
         }
         return null;
      }

      #endregion

      #region Profile File Loading

      private void LoadProfiles()
      {
         lock (m_profileLock)
         {
            // Always start with an empty profile (no sounds).
            AddEmptyProfile();

            // Load user-saved profiles.
            AddUserProfile();

            // Load built-in profiles.
            AddBuiltInProfiles();
         }
      }

      private void AddEmptyProfile()
      {
         SoundProfile emptyProfile = new SoundProfile(true);
         emptyProfile.DisplayName = SoundProfile.ProfileName_Empty;
         Profiles.Add(emptyProfile);
      }

      private void AddUserProfile()
      {
         SoundProfile userProfile = new SoundProfile(false);
         userProfile.DisplayName = SoundProfile.ProfileName_User;
         userProfile.Restore();
         Profiles.Add(userProfile);
      }

      private void AddBuiltInProfiles()
      {
         // Get the sound profile directory.
         DirectoryInfo dirInfo = GetSoundProfileDirectory();
         if (dirInfo == null)
            return;

         // List directories.  Each directory is a profile.
         DirectoryInfo[] profileDirs = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

         // Each filename should match the name from each SoundItem in the profile.
         foreach (DirectoryInfo profileDir in profileDirs)
         {
            SoundProfile soundProfile = CreateProfileFromDirectory(profileDir, true);
            if (soundProfile != null)
               Profiles.Add(soundProfile);
         }
      }

      public static DirectoryInfo GetSoundProfileDirectory()
      {
         // Get executing assembly.
         Assembly assembly = Assembly.GetEntryAssembly();
         string exeDirectory = Path.GetDirectoryName(assembly.Location);
#if DEBUG
         // Go up three directories.
         exeDirectory = Path.Combine(exeDirectory, "..");
         exeDirectory = Path.Combine(exeDirectory, "..");
         exeDirectory = Path.Combine(exeDirectory, "..");
#endif
         string soundProfiles = Path.Combine(exeDirectory, c_profileDirectory);

         // Return null if the directory does not exist.
         DirectoryInfo dirInfo = new DirectoryInfo(soundProfiles);
         if (dirInfo.Exists == false)
            return null;
         return dirInfo;
      }

      private SoundProfile CreateProfileFromDirectory(DirectoryInfo profileDir, bool readOnly)
      {
         SoundProfile profile = new SoundProfile(readOnly);

         // Name the profile after the directory.
         profile.DisplayName = profileDir.Name;

         // Look in this directory for files named the same as the SoundItem.Name
         // property.  We do this by inserting the name between the profile path
         // and the expected sound file extension, then checking if that file exists.
         foreach (SoundItem item in profile.SoundItems)
         {
            string fullPath = Path.Combine(profileDir.FullName, item.Name.ToString());
            fullPath = Path.ChangeExtension(fullPath, c_soundFileExtension);
            if (File.Exists(fullPath))
               item.FilePath = fullPath;
         }

         return profile;
      }

      #endregion

      #region Sound Playing

      public void PlaySound(SoundEffect soundEffect)
      {
         if (CurrentProfile == null)
            return;

         // Mundane linear search to match the Name in the profile.
         // This does a search instead of switch statement so this method is
         // maintenance-free.
         foreach (SoundItem soundItem in CurrentProfile.SoundItems)
         {
            // If this is the one we're looking for, play it.
            // Assumes there's at most one of each Name in the list.
            if (soundItem.Name == soundEffect)
            {
               soundItem.Play();
               return;
            }
         }
      }

      #endregion
      
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

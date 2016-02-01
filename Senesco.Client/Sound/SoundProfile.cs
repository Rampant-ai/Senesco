
using System.Collections.Generic;
using System.ComponentModel;
using Senesco.Client.Utility;

namespace Senesco.Client.Sound
{
   /// <summary>
   /// Represents one complete set of sounds for the application.  There may be
   /// several instances of this class, so that the user can be provided with
   /// a selection of profiles to pick from, or create his own.
   /// </summary>
   public class SoundProfile : INotifyPropertyChanged
   {
      #region Constants

      public const string ProfileName_Empty = @"No Sounds";
      public const string ProfileName_User = @"User Profile";

      #endregion

      private string m_displayName = null;
      public string DisplayName
      {
         get { return m_displayName; }
         set
         {
            m_displayName = value;
            NotifyPropertyChanged("DisplayName");
         }
      }

      private readonly bool m_readOnly;
      public bool ReadOnly
      {
         get { return m_readOnly; }
      }

      public List<SoundItem> SoundItems
      {
         get
         {
            List<SoundItem> soundItems = new List<SoundItem>();
            soundItems.Add(ChatClick);
            soundItems.Add(UserJoin);
            soundItems.Add(UserPart);
            soundItems.Add(PmReceived);
            soundItems.Add(PmSent);
            soundItems.Add(Connected);
            soundItems.Add(Disconnected);
            return soundItems;
         }
      }

      #region The Sound Effects

      private SoundItem m_chatClick = null;
      private SoundItem m_userJoin = null;
      private SoundItem m_userPart = null;
      private SoundItem m_pmReceived = null;
      private SoundItem m_pmSent = null;
      private SoundItem m_connected = null;
      private SoundItem m_disconnected = null;

      public SoundItem ChatClick
      {
         get { return m_chatClick; }
      }

      public SoundItem UserJoin
      {
         get { return m_userJoin; }
      }

      public SoundItem UserPart
      {
         get { return m_userPart; }
      }

      public SoundItem PmReceived
      {
         get { return m_pmReceived; }
      }

      public SoundItem PmSent
      {
         get { return m_pmSent; }
      }

      public SoundItem Connected
      {
         get { return m_connected; }
      }

      public SoundItem Disconnected
      {
         get { return m_disconnected; }
      }

      #endregion

      /// <summary>
      /// Creator
      /// </summary>
      public SoundProfile(bool readOnly)
      {
         m_readOnly = readOnly;

         m_chatClick = new SoundItem(SoundEffect.ChatClick, "Chat Received", m_readOnly);
         m_userJoin = new SoundItem(SoundEffect.UserJoin, "User Joined", m_readOnly);
         m_userPart = new SoundItem(SoundEffect.UserPart, "User Parted", m_readOnly);
         m_pmReceived = new SoundItem(SoundEffect.PmReceived, "PM Received", m_readOnly);
         m_pmSent = new SoundItem(SoundEffect.PmSent, "PM Sent", m_readOnly);
         m_connected = new SoundItem(SoundEffect.Connected, "Connected", m_readOnly);
         m_disconnected = new SoundItem(SoundEffect.Disconnected, "Disconnected", m_readOnly);
      }

      /// <summary>
      /// Helper method to load all loaded sounds.
      /// </summary>
      public void SubmitAll()
      {
         //TODO: better analysis on the threading issues with this.
         SoundUtils.ClearSounds();

         foreach (SoundItem soundItem in SoundItems)
         {
            soundItem.Submit();
         }
      }

      #region Save to/Restore from Config

      /// <summary>
      /// Saves this complete sound profile based on this profile's currently set DisplayName.
      /// </summary>
      public void Save()
      {
         // For now, only allow saving if this is the user profile.
         if (DisplayName != ProfileName_User)
            return;

         foreach (SoundItem item in SoundItems)
         {
            ConfigSettings.UserSettings.SaveSoundPath(GenerateItemSaveKey(item), item.FilePath);
         }
         ConfigSettings.UserSettings.Save();
      }

      /// <summary>
      /// Loads this complete sound profile based on this profile's currently set DisplayName.
      /// </summary>
      public void Restore()
      {
         foreach (SoundItem item in SoundItems)
         {
            item.FilePath = ConfigSettings.UserSettings.GetSoundPath(GenerateItemSaveKey(item));
         }
      }

      /// <summary>
      /// Private helper to consistently generate save keys for each sound, based
      /// on this profile's currently set DisplayName.
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      private string GenerateItemSaveKey(SoundItem item)
      {
         return item.Name.ToString();
         //return String.Format("{0}|{1}", this.DisplayName, item.Name.ToString());
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

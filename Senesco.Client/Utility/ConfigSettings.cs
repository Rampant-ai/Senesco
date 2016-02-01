using System;
using System.Configuration;
using log4net;

namespace Senesco.Client.Utility
{
   /// <summary>
   /// This class is a convenient interface for saving application settings
   /// per-user.  Unfortunately each setting must be explicitly defined, so
   /// saved settings for all user interface implementations will be
   /// represented here.  Fortunately, they are all primitives so the only
   /// pollution is the set of names.
   /// </summary>
   public class ConfigSettings : ApplicationSettingsBase
   {
      #region Properties and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(ConfigSettings));

      public static ConfigSettings UserSettings = new ConfigSettings();

      public ConfigSettings()
      {
         // Make sure we call our SettingsLoaded event handler.
         this.SettingsLoaded += new SettingsLoadedEventHandler(ConfigSettings_SettingsLoaded);
      }

      #endregion

      #region Settings Upgrade

      /// <summary>
      /// By default, this flag is set to TRUE.  Whenever we save the config to file,
      /// it is set to FALSE to indicate we have a valid config file.  Therefore, if
      /// after the config is loaded we find this value still set to TRUE, then we
      /// must have failed loading the last config file.
      /// </summary>
      [UserScopedSetting()]
      [DefaultSettingValue("true")]
      public bool ConfigFileNotFound
      {
         get { return (bool)this["ConfigFileNotFound"]; }
         set { this["ConfigFileNotFound"] = value; }
      }

      /// <summary>
      /// Event handler called when settings are finished being loaded.
      /// Crucially, this method is called even if no saved settings file was found.
      /// </summary>
      void ConfigSettings_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
      {
         // If the flag is still set to true, we didn't find a config file and we
         // should attempt an upgrade from a previous version.
         if (ConfigFileNotFound)
         {
            try
            {
               Upgrade();
            }
            catch (Exception ex)
            {
               s_log.ErrorFormat("Exception performing settings upgrade: {0}", ex.Message);
            }
         }

         // Always save the flag as off so the next time we launch we will know
         // that it came from a saved config file!
         ConfigFileNotFound = false;
      }

      #endregion

      #region Chat Window Coordinates

      [UserScopedSetting()]
      [DefaultSettingValue("20")]
      public double ChatWindowLeft
      {
         get { return (double)this["ChatWindowLeft"]; }
         set { this["ChatWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("20")]
      public double ChatWindowTop
      {
         get { return (double)this["ChatWindowTop"]; }
         set { this["ChatWindowTop"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("550")]
      public double ChatWindowWidth
      {
         get { return (double)this["ChatWindowWidth"]; }
         set { this["ChatWindowWidth"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("350")]
      public double ChatWindowHeight
      {
         get { return (double)this["ChatWindowHeight"]; }
         set { this["ChatWindowHeight"] = value; }
      }

      #endregion

      #region User List Window Coordinates

      [UserScopedSetting()]
      public double UserListWindowLeft
      {
         get { return (double)this["UserListWindowLeft"]; }
         set { this["UserListWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double UserListWindowTop
      {
         get { return (double)this["UserListWindowTop"]; }
         set { this["UserListWindowTop"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("280")]
      public double UserListWindowWidth
      {
         get { return (double)this["UserListWindowWidth"]; }
         set { this["UserListWindowWidth"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("350")]
      public double UserListWindowHeight
      {
         get { return (double)this["UserListWindowHeight"]; }
         set { this["UserListWindowHeight"] = value; }
      }

      #endregion

      #region User Info Window Coordinates

      [UserScopedSetting()]
      public double UserInfoWindowLeft
      {
         get { return (double)this["UserInfoWindowLeft"]; }
         set { this["UserInfoWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double UserInfoWindowTop
      {
         get { return (double)this["UserInfoWindowTop"]; }
         set { this["UserInfoWindowTop"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("300")]
      public double UserInfoWindowWidth
      {
         get { return (double)this["UserInfoWindowWidth"]; }
         set { this["UserInfoWindowWidth"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("400")]
      public double UserInfoWindowHeight
      {
         get { return (double)this["UserInfoWindowHeight"]; }
         set { this["UserInfoWindowHeight"] = value; }
      }

      #endregion

      #region PM Received Coordinates

      [UserScopedSetting()]
      public double PmReceiveWindowLeft
      {
         get { return (double)this["PmReceiveWindowLeft"]; }
         set { this["PmReceiveWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double PmReceiveWindowTop
      {
         get { return (double)this["PmReceiveWindowTop"]; }
         set { this["PmReceiveWindowTop"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("325")]
      public double PmReceiveWindowWidth
      {
         get { return (double)this["PmReceiveWindowWidth"]; }
         set { this["PmReceiveWindowWidth"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("280")]
      public double PmReceiveWindowHeight
      {
         get { return (double)this["PmReceiveWindowHeight"]; }
         set { this["PmReceiveWindowHeight"] = value; }
      }

      #endregion

      #region PM Send Coordinates

      [UserScopedSetting()]
      public double PmSendWindowLeft
      {
         get { return (double)this["PmSendWindowLeft"]; }
         set { this["PmSendWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double PmSendWindowTop
      {
         get { return (double)this["PmSendWindowTop"]; }
         set { this["PmSendWindowTop"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("325")]
      public double PmSendWindowWidth
      {
         get { return (double)this["PmSendWindowWidth"]; }
         set { this["PmSendWindowWidth"] = value; }
      }
      [UserScopedSetting()]
      [DefaultSettingValue("280")]
      public double PmSendWindowHeight
      {
         get { return (double)this["PmSendWindowHeight"]; }
         set { this["PmSendWindowHeight"] = value; }
      }

      #endregion

      #region Connect Window Coordinates

      [UserScopedSetting()]
      public double ConnectWindowLeft
      {
         get { return (double)this["ConnectWindowLeft"]; }
         set { this["ConnectWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double ConnectWindowTop
      {
         get { return (double)this["ConnectWindowTop"]; }
         set { this["ConnectWindowTop"] = value; }
      }

      #endregion

      #region Sounds Config Window Coordinates

      [UserScopedSetting()]
      public double SoundsConfigLeft
      {
         get { return (double)this["SoundsConfigLeft"]; }
         set { this["SoundsConfigLeft"] = value; }
      }
      [UserScopedSetting()]
      public double SoundsConfigTop
      {
         get { return (double)this["SoundsConfigTop"]; }
         set { this["SoundsConfigTop"] = value; }
      }

      #endregion

      #region User Config Window Coordinates

      [UserScopedSetting()]
      public double UserConfigLeft
      {
         get { return (double)this["UserConfigLeft"]; }
         set { this["UserConfigLeft"] = value; }
      }
      [UserScopedSetting()]
      public double UserConfigTop
      {
         get { return (double)this["UserConfigTop"]; }
         set { this["UserConfigTop"] = value; }
      }

      #endregion

      #region About Window Coordinates

      [UserScopedSetting()]
      public double AboutWindowLeft
      {
         get { return (double)this["AboutWindowLeft"]; }
         set { this["AboutWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double AboutWindowTop
      {
         get { return (double)this["AboutWindowTop"]; }
         set { this["AboutWindowTop"] = value; }
      }

      #endregion

      #region Upgrade Window Coordinates

      [UserScopedSetting()]
      public double UpgradeWindowLeft
      {
         get { return (double)this["UpgradeWindowLeft"]; }
         set { this["UpgradeWindowLeft"] = value; }
      }
      [UserScopedSetting()]
      public double UpgradeWindowTop
      {
         get { return (double)this["UpgradeWindowTop"]; }
         set { this["UpgradeWindowTop"] = value; }
      }

      #endregion

      #region Sound Settings

      [UserScopedSetting()]
      public string SoundProfile
      {
         get { return (string)this["SoundProfile"]; }
         set { this["SoundProfile"] = value; }
      }

      // There needs to be a property for each sound path to save, and it should
      // be named after the SoundItem Name property so the reflection works.
      [UserScopedSetting()]
      public string ChatClick { get; set; }
      [UserScopedSetting()]
      public string UserJoin { get; set; }
      [UserScopedSetting()]
      public string UserPart { get; set; }
      [UserScopedSetting()]
      public string PmReceived { get; set; }
      [UserScopedSetting()]
      public string PmSent { get; set; }
      [UserScopedSetting()]
      public string Connected { get; set; }
      [UserScopedSetting()]
      public string Disconnected { get; set; }

      public Status SaveSoundPath(string name, string path)
      {
         if (String.IsNullOrEmpty(name))
            return Status.Failure;

         this[name] = path;
         Save();
         
         return Status.Success;
      }

      public string GetSoundPath(string name)
      {
         try
         {
            return (string)this[name];
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception restoring sound path: {0}", e.Message);
            return null;
         }
      }

      #endregion

      #region Console Settings

      [UserScopedSetting()]
      [DefaultSettingValue("/")]
      public string CommandPrefix
      {
         get { return (string)this["CommandPrefix"]; }
         set { this["CommandPrefix"] = value; }
      }

      #endregion
   }
}

using System;
using System.IO;
using System.Reflection;
using log4net;
using Microsoft.Win32;

namespace Senesco.Client.Utility
{
   public class RegistryUtils
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(RegistryUtils));
      private const string c_autoConnectValueName = "Senesco";

      #region Auto Connection Settings

      internal static string GetAutoConnectBookmark()
      {
         // Construct key path to HKCU > Software > Microsoft > Windows > CurrentVersion > Run
         RegistryKey key = GetCurrentUserRun();
         if (key == null)
            return string.Empty;

         // Get the command from the key value "Senesco".
         string autoConnectCommand = key.GetValue(c_autoConnectValueName) as string;
         if (autoConnectCommand == null)
         {
            s_log.Info("No auto-connect command found in registry.");
            return string.Empty;
         }
         s_log.DebugFormat("Found command in registry: {0}", autoConnectCommand);

         // The command is formatted as: "{Senesco Path}" "{Bookmark}"
         // What we want is the filename at the end of the full path between the 3rd and 4th quotes.
         string[] exploded = autoConnectCommand.Split('"');
         if (exploded.Length < 4)
            return string.Empty;
         string bookmarkPath = exploded[3];
         s_log.DebugFormat("Bookmark path: {0}", bookmarkPath);
         return Path.GetFileNameWithoutExtension(bookmarkPath);
      }

      public static Status SetAutoConnectBookmark(string bookmarkName)
      {
         try
         {
            // Get the appropriate subkey.
            RegistryKey key = GetCurrentUserRun();
            if (key == null)
               return Status.Failure;

            // Get full path of this bookmark.
            string bookmarkPath = FileUtils.GetBookmarkFullPath(bookmarkName);

            // Get full path of this executable.
            string exePath = Assembly.GetEntryAssembly().Location;

            // Format the final registry key value.
            string autoConnectCommand = String.Format(@"""{0}"" ""{1}""", exePath, bookmarkPath);
            s_log.DebugFormat("Constructed auto-connect command: {0}", autoConnectCommand);

            // Create and set the value in the subkey.
            key.SetValue(c_autoConnectValueName, autoConnectCommand, RegistryValueKind.String);

            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error setting auto-connect bookmark: {0}", e.Message);
            return Status.Failure;
         }
      }

      public static Status RemoveAutoConnectBookmark()
      {
         try
         {
            // Get the appropriate subkey.
            RegistryKey key = GetCurrentUserRun();
            if (key == null)
               return Status.Failure;

            // Remove the value completely.
            key.DeleteValue(c_autoConnectValueName);
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error removing auto-connect bookmark: {0}", e.Message);
            return Status.Failure;
         }
      }

      #endregion

      #region Key Getters

      private static RegistryKey GetCurrentUserRun()
      {
         try
         {
            RegistryKey key = Registry.CurrentUser;
            key = key.CreateSubKey("Software");
            key = key.CreateSubKey("Microsoft");
            key = key.CreateSubKey("Windows");
            key = key.CreateSubKey("CurrentVersion");
            key = key.CreateSubKey("Run");
            return key;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Could not get registry key: {0}", e.Message);
            return null;
         }
      }

      #endregion
   }
}

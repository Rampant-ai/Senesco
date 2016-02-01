using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using log4net;
using Senesco.Client.Events;

namespace Senesco.Client.Utility
{
   class AutoUpdate
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(AutoUpdate));

      private const int s_checkInterval = 24 * 1000 * 60 * 60;

      private Timer m_autoUpdateCheckTimer;

      public event EventHandlers.NewVersionDelegate NewVersionAvailable;

      public AutoUpdate()
      {
         m_autoUpdateCheckTimer = new Timer(5 * 1000); // Initial check seconds after launching.
         m_autoUpdateCheckTimer.AutoReset = true;
         m_autoUpdateCheckTimer.Elapsed += UpdateTimer_Elapsed;
         m_autoUpdateCheckTimer.Start();
      }

      private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         // Stop the timer while the update check is performed.
         m_autoUpdateCheckTimer.Stop();
         CheckForUpdate();

         // Make sure the interval is properly set before restarting.
         m_autoUpdateCheckTimer.Interval = s_checkInterval;
         m_autoUpdateCheckTimer.Start();
      }

      /// <summary>
      /// This method is invoked periodically to contact the server and determine if there
      /// is a newer version available.  If so, a controller event is fired with the new version
      /// number.
      /// </summary>
      public void CheckForUpdate()
      {
         try
         {
            // Download the file which contains the latest version number.
            FileInfo fi = DownloadFile.Download("http://rancor.yi.org/Senesco/currentVersion");

            // Read in the data from the file.
            string newVersion = null;
            using (FileStream fs = fi.OpenRead())
            using (StreamReader sr = new StreamReader(fs))
            {
               newVersion = sr.ReadToEnd().Trim();
            }

            // Clean up the temporarily downloaded file.
            fi.Delete();

            // Determine if this version is newer than the current version.
            string currentVersion;
            if (CompareVersion(newVersion, Assembly.GetExecutingAssembly(), out currentVersion) > 0)
            {
               // Fire event if a new version is available.
               if (NewVersionAvailable != null)
                  NewVersionAvailable(this, new NewVersionEventArgs(currentVersion, newVersion));
            }
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception checking for update: {0}\n{1}", e.Message, e.StackTrace);
         }
      }

      /// <summary>
      /// Finds and compares the version number in the given Assembly with the given version string.
      /// </summary>
      private int CompareVersion(string newVersion, Assembly assembly, out string currentVersion)
      {
         currentVersion = null;
         string qualifiedName = assembly.FullName;
         
         string[] attributes = qualifiedName.Split(',');
         foreach (string att in attributes)
         {
            // Only look for "x=y" pairings.
            if (att.Contains("=") == false)
               continue;

            string[] halves = att.Split('=');

            // Require the trimmed first half to be "Version".
            if (String.Compare(halves[0].Trim(), "Version", true) == 0)
            {
               // Normalize the two versions in case one is shorter than the other.
               string v1, v2;
               NormalizeVersions(newVersion, halves[1], out v1, out v2);

               // The normalized version from the current assembly is the one we consider the current version.
               currentVersion = v2;

               // Return the version comparison.
               return CompareVersionStrings(v1, v2);
            }
         }

         throw new Exception("Could not locate version number in current assembly.");
      }

      /// <summary>
      /// In case version strings have a different number of decimals, this routine
      /// normalizes them to have the same number (down to whichever string has fewer).
      /// 
      /// For example: "0.8.2" and "0.8.3.0" will become "0.8.2" and "0.8.3"
      /// </summary>
      private void NormalizeVersions(string v1, string v2, out string v1o, out string v2o)
      {
         string[] v1parts = v1.Split('.');
         string[] v2parts = v2.Split('.');

         int parts = Math.Min(v1parts.Length, v2parts.Length);

         v1o = String.Empty;
         v2o = String.Empty;
         bool first = true;
         for (int i = 0; i < parts; i++)
         {
            if (first)
            {
               first = false;
            }
            else
            {
               v1o += '.';
               v2o += '.';
            }

            v1o += v1parts[i];
            v2o += v2parts[i];
         }
      }

      /// <summary>
      /// Compares two version strings with sequential ordinal comparisons, ie.
      /// Major versions are compared first, then minor versions, then releases, et al.
      /// The number of comparisons performed is limited by the smaller of the two version
      /// places.
      /// </summary>
      private int CompareVersionStrings(string v1, string v2)
      {
         string[] v1parts = v1.Split('.');
         string[] v2parts = v2.Split('.');

         for (int i = 0; i < v1parts.Length; i++)
         {
            int int1 = int.Parse(v1parts[i]);
            int int2 = int.Parse(v2parts[i]);
            if (int1 != int2)
               return int1.CompareTo(int2);
         }
         return 0;
      }

      /// <summary>
      /// This method is invoked by the presentation layer, indicating that either the user or an
      /// automated process is trying to perform the update process now.
      /// </summary>
      /// <param name="version">The update string provided by the "UpdateAvailable" event.</param>
      public void PerformUpdate(string version)
      {
         try
         {
            // Generate the URL from the provided version number.
            // http://rancor.yi.org/Senesco/53cr37/Senesco v0.8.2.zip
            string url = String.Format("http://rancor.yi.org/Senesco/53cr37/Senesco%20v{0}.zip", version);
            int pid = Process.GetCurrentProcess().Id;

            string currentExe = Assembly.GetEntryAssembly().Location;
            string currentInstall = Path.GetDirectoryName(currentExe);
            string exeName = Path.GetFileName(currentExe);

            // Updater.exe  http://rancor.yi.org/Senesco/53cr37/Senesco%20v0.8.2.zip  3360  "C:\Program Files\Senesco\"  "C:\Program Files\Senesco\Senesco.exe"
            string command = Path.Combine(currentInstall, "Updater.exe");
            string parameters = String.Format(@"{0} {1} ""{2}"" ""{3}""", url, pid, currentInstall, currentExe);
            s_log.InfoFormat("Launching:\n  {0}\n  {1}", command, parameters);
            Process.Start(command, parameters);
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Auto-update failed: {0}\n{1}", e.Message, e.StackTrace);
         }
      }
   }
}

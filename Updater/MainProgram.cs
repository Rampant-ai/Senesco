using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;

namespace Updater
{
   class MainProgram
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(MainProgram));

      /// <summary>
      /// Entry point.
      /// </summary>
      static void Main(string[] args)
      {
         try
         {
            Log.InitLogging();

            // Run the update process and pause for a few seconds if no errors.
            Run(args);
         }
         catch (Exception e)
         {
            // Wait for user input if any unhandled exceptions occurred.
            s_log.ErrorFormat("Unhandled exception: {0}", e.Message);
            s_log.ErrorFormat("");
            s_log.ErrorFormat("Press any key to exit.");
            Console.ReadKey(true);
         }
      }

      /// <summary>
      /// Helper "Main" method which can freely throw exceptions.
      /// </summary>
      private static void Run(string[] args)
      {
         if (args.Length != 4)
         {
            s_log.ErrorFormat("Bad arguments:");
            for (int i = 0; i < args.Length; i++)
               s_log.ErrorFormat("   {0}: {1}", i, args[i]);
            return;
         }

         string url = args[0];
         int pid = int.Parse(args[1]);
         string targetDir = args[2];
         string targetExe = args[3];

         // If this executable is running in the target directory, we need to copy the updater
         // to a temporary location and re-launch from there with the same parameters.
         if (SelfAwareness.IsRunningInDirectory(targetDir))
         {
            s_log.InfoFormat("Re-running updater from temporary location...");
            SelfAwareness.CopyAndLaunch(args);
            return;
         }

         s_log.DebugFormat("Updating with these parameters:");
         s_log.DebugFormat("  URL: {0}", url);
         s_log.DebugFormat("  PID: {0}", pid);
         s_log.DebugFormat("  Install Dir: {0}", targetDir);
         s_log.DebugFormat("  Relaunch App: {0}", targetExe);
         s_log.DebugFormat("");

         RunUpdate(url, pid, targetDir, targetExe);

         s_log.InfoFormat("Update successful!");
         Thread.Sleep(5000);
      }

      /// <summary>
      /// Business logic for the update process.
      /// </summary>
      /// <returns>True if the update was successful.</returns>
      private static bool RunUpdate(string url, int pid, string targetDir, string targetExe)
      {
         // Test permissions for install directory.
         s_log.InfoFormat("Testing permissions...");
         if (InstallUpdate.HasFullPermissions(targetDir) == false)
         {
            s_log.ErrorFormat("Update Failed: Insufficient permissions to modify install directory!");
            return false;
         }
         s_log.InfoFormat("Permissions test successful!");

         // Download file from URL to temporary directory.
         s_log.InfoFormat("Downloading update...");
         FileInfo downloadedFile = DownloadFile.Download(url);

         // Unzip file to working directory.
         s_log.InfoFormat("Unzipping update...");
         DirectoryInfo unzippedDir = UnzipFile.Unzip(downloadedFile, true);

         // Wait for pid to exit.
         PidExit.WaitForPid(pid);

         // Perform installation.
         InstallUpdate.Install(unzippedDir.FullName, targetDir);

         // Delete the temporary directory.
         unzippedDir.Delete(true);

         // Launch updated version.
         Process.Start(Path.Combine(targetDir, targetExe));

         return true;
      }
   }
}

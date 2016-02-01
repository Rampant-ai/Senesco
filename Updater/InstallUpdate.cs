using System;
using System.IO;
using log4net;

namespace Updater
{
   class InstallUpdate
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(InstallUpdate));

      public static void Install(string unzipDir, string targetDir)
      {
         // Define and create an empty temporary backup directory.
         string backupPath = TempPath.GenerateTempDirectory("Senesco Upgrade Backup"); // C:\Temp\Senesco Upgrade Backup\
         Directory.CreateDirectory(backupPath);

         try
         {
            // Copy files from TargetDir to a backup directory.
            CopyContents(targetDir, backupPath); // C:\Temp\Senesco Upgrade Backup\ (Senesco.exe)

            // Clean the target directory.
            CleanContents(targetDir);

            // Copy files from UnzipDir to TargetDir.
            CopyContents(unzipDir, targetDir); // C:\Temp\Senesco Unzip\  to  C:\Program Files\Senesco\
         }
         catch (Exception e)
         {
            // If any exceptions, restore backup and rethrow.
            s_log.ErrorFormat("Exception installing update! {0}", e.Message);
            s_log.ErrorFormat("Attempting to restore backup...");

            CopyContents(backupPath, targetDir); // C:\Temp\Senesco Unzip\  to  C:\Program Files\Senesco\

            throw;
         }

         try
         {
            // Delete the backup directory.  If this fails, no big deal.
            Directory.Delete(backupPath, true);
         }
         catch (Exception e)
         {
            s_log.WarnFormat("Could not clean up backup directory: {0}", backupPath);
            s_log.WarnFormat("   Reason: {0}", e.Message);
         }
      }

      /// <summary>
      /// Copies the contents of a directory without modifying the base directory itself.
      /// </summary>
      private static void CopyContents(string fromDir, string toDir)
      {
         DirectoryInfo parentDir = new DirectoryInfo(fromDir);

         s_log.DebugFormat("Copying files:\n  {0}\n  {1}", fromDir, toDir);
         foreach (FileInfo fi in parentDir.EnumerateFiles())
         {
            fi.CopyTo(Path.Combine(toDir, fi.Name), true);
         }

         s_log.DebugFormat("Copying directories:\n  {0}\n  {1}", fromDir, toDir);
         foreach (DirectoryInfo di in parentDir.EnumerateDirectories())
         {
            // Create the target directory, then recursively copy it.
            string targetDir = Path.Combine(toDir, di.Name);
            Directory.CreateDirectory(targetDir);
            CopyContents(Path.Combine(fromDir, di.Name), targetDir);
         }
      }

      /// <summary>
      /// Cleans all files and directories from the given directory.
      /// </summary>
      private static void CleanContents(string targetDir)
      {
         DirectoryInfo dir = new DirectoryInfo(targetDir);

         s_log.DebugFormat("Cleaning files from: {0}", targetDir);
         foreach (FileInfo fi in dir.EnumerateFiles())
         {
            fi.Delete();
         }

         s_log.DebugFormat("Cleaning directories from: {0}", targetDir);
         foreach (DirectoryInfo di in dir.EnumerateDirectories())
         {
            di.Delete(true);
         }
      }

      /// <summary>
      /// Helper method to test permissions in the given directory.  If any permissions are
      /// not available, this method returns false.
      /// </summary>
      /// <param name="targetDir">The directory to test.</param>
      /// <returns>True if the current user has permissions to write to the given directory.</returns>
      public static bool HasFullPermissions(string targetDir)
      {
         try
         {
            // Create and delete a temporary file.
            FileInfo fi = new FileInfo(Path.Combine(targetDir, "test.txt"));

            // Create and write a byte to the file.
            byte testByte = 0x00;
            using (FileStream fs1 = fi.OpenWrite())
            {
               fs1.WriteByte(testByte);
            }

            // Read the byte from the file.
            using (FileStream fs2 = fi.OpenRead())
            {
               if (fs2.ReadByte() != testByte)
                  throw new Exception("Could not read data out of file during permissions test.");
            }

            // Delete the file.
            fi.Delete();

            // No exceptions, all operations were successful.
            return true;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception during permissions test: {0}", e.Message);
            return false;
         }
      }
   }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;

namespace Updater
{
   class SelfAwareness
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(SelfAwareness));

      internal static bool IsRunningInDirectory(string targetDir)
      {
         // Get the current execution directory.
         string executingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

         // Remove any trailing directory delimiters;
         string targetDirCleaned = targetDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

         s_log.DebugFormat("Comparing paths:\n   {0}\n   {1}", executingDir, targetDirCleaned);

         if (String.Compare(executingDir, targetDirCleaned, true) == 0)
            return true;
         else
            return false;
      }

      internal static Process CopyAndLaunch(string[] args)
      {
         // Make a temporary location target.
         string tempUpdaterPath = TempPath.GenerateTempDirectory("Senesco Displaced Updater");
         Directory.CreateDirectory(tempUpdaterPath);

         // Build the source and target paths.
         string exeSource = Assembly.GetExecutingAssembly().Location;
         string exeTarget = Path.Combine(tempUpdaterPath, Path.GetFileName(exeSource));

         // Copy the files to the temporary location.
         string sourcePath = Path.GetDirectoryName(exeSource);
         CopyFile(sourcePath, tempUpdaterPath, "Updater.exe");
         CopyFile(sourcePath, tempUpdaterPath, "Updater.exe.config");
         CopyFile(sourcePath, tempUpdaterPath, "ICSharpCode.SharpZipLib.dll");
         CopyFile(sourcePath, tempUpdaterPath, "log4net.dll");

         // Launch the copied executable with same parameter list.
         string parameters = String.Format(@"{0} {1} ""{2}"" ""{3}""", args[0], args[1], args[2], args[3]);
         s_log.InfoFormat("Launching:  {0}", exeTarget);
         s_log.InfoFormat("Parameters: {0}", parameters);
         return Process.Start(exeTarget, parameters);
      }

      private static void CopyFile(string sourcePath, string tempUpdaterPath, string sourceFile)
      {
         string dllSource = Path.Combine(sourcePath, sourceFile);
         string dllTarget = Path.Combine(tempUpdaterPath, sourceFile);
         File.Copy(dllSource, dllTarget);
      }
   }
}

using System;
using System.IO;
using log4net;

namespace Updater
{
   class TempPath
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(TempPath));

      /// <summary>
      /// Generate a path to a temporary directory that does not currently exist.
      /// </summary>
      /// <param name="label">The desired readable label for the directory name.</param>
      /// <returns>The unique unused path.</returns>
      public static string GenerateTempDirectory(string label)
      {
         string subDir, outDir;
         do
         {
            subDir = String.Format("{0} {1}", label, Guid.NewGuid().ToString());
            outDir = Path.Combine(Path.GetTempPath(), subDir);
         }
         while (Directory.Exists(outDir));

         s_log.DebugFormat("Generated temporary path: {0}", outDir);
         return outDir;
      }
   }
}

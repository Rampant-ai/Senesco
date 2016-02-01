using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Updater
{
   class UnzipFile
   {
      public static DirectoryInfo Unzip(FileInfo downloadedFile, bool deleteZip)
      {
         // Generate a unique output directory.
         string outDir = TempPath.GenerateTempDirectory("Senesco Upgrade Files");
         Directory.CreateDirectory(outDir);

         // Unzip to the target directory.
         using (FileStream fs = downloadedFile.OpenRead())
         {
            byte[] buffer = new byte[4096]; // 4K unzipping buffer
            ZipFile zip = new ZipFile(fs);
            foreach (ZipEntry entry in zip)
            {
               // Ignore directories since we create them as needed from the file paths.
               if (entry.IsFile == false)
                  continue;

               // Relative path of this file.
               String fileName = entry.Name;
               Stream zipStream = zip.GetInputStream(entry);

               // Compile the target unzip full path.
               String targetFilePath = Path.Combine(outDir, fileName);

               // Create target directory if needed.
               string directoryName = Path.GetDirectoryName(targetFilePath);
               if (String.IsNullOrEmpty(directoryName) == false)
               {
                  Directory.CreateDirectory(directoryName);
               }

               // Unzip using the buffer.
               using (FileStream streamWriter = File.Create(targetFilePath))
               {
                  StreamUtils.Copy(zipStream, streamWriter, buffer);
               }
            }
         }

         // If there were no exceptions, delete the zip file.
         if (deleteZip)
         {
            downloadedFile.Delete();
         }

         // If the unzip dir is a single wrapping dir, return the inner dir.
         if (Directory.GetFiles(outDir).Length == 0)
         {
            string[] directories = Directory.GetDirectories(outDir);
            if (directories.Length == 1)
            {
               return new DirectoryInfo(directories[0]);
            }
         }

         return new DirectoryInfo(outDir);
      }
   }
}

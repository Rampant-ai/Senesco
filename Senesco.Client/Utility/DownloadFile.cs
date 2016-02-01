using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace Senesco.Client.Utility
{
   /// <summary>
   /// TODO: This class should be folded into a "Common" library rather than included
   /// with a specific project.
   /// </summary>
   class DownloadFile
   {
      private static bool s_downloadComplete = false;

      public static FileInfo Download(string url)
      {
         int index = url.LastIndexOf('/');
         string webFilename = url.Substring(index + 1, (url.Length - index) - 1);
         string downloadFile = Path.Combine(Path.GetTempPath(), webFilename);

         WebClient wc = new WebClient();
         wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
         wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);

         // Start the download and wait until it is complete.
         ManualResetEvent mre = new ManualResetEvent(false);
         wc.DownloadFileAsync(new Uri(url), downloadFile, mre);
         mre.WaitOne();

         Console.WriteLine("Download complete!");
         return new FileInfo(downloadFile);
      }

      static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
      {
         // This is a cosmetic workaround since the event notifications can fire out of order.
         // This prevents a progress % report from being displayed after "Complete!" is shown.
         if (s_downloadComplete == false)
         {
            Console.WriteLine("Download progress: {0}%", e.ProgressPercentage);
         }
      }

      static void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
      {
         s_downloadComplete = true;

         // Signal the main thread to proceed.
         ManualResetEvent mre = (ManualResetEvent)e.UserState;
         mre.Set();
      }
   }
}

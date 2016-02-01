using System.Diagnostics;
using System.Threading;
using log4net;

namespace Updater
{
   class PidExit
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(PidExit));

      public static void WaitForPid(int pid)
      {
         try
         {
            while (true)
            {
               // This call will throw an exception when the PID is not found.
               Process.GetProcessById(pid);
               s_log.InfoFormat("Waiting for PID {0} to terminate...", pid);
               Thread.Sleep(1000);
            }
         }
         catch { }
      }
   }
}

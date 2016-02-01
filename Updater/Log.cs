using log4net;
using log4net.Config;

namespace Updater
{
   public class Log
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(Log));

      private static bool loggingConfigured = false;

      public static void InitLogging()
      {
         if (loggingConfigured == false)
         {
            XmlConfigurator.Configure();
            s_log.Info("Logging configured!");
            loggingConfigured = true;
         }
         else
         {
            s_log.Debug("Logging already configured!");
         }
      }
   }
}

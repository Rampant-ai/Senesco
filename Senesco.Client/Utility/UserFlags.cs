
namespace Senesco.Client.Utility
{
   class UserFlags
   {
      private const int s_idleFlag = 0x01;
      private const int s_adminFlag = 0x02;

      public static bool IsIdleFlagSet(int flags)
      {
         return (flags & s_idleFlag) > 0;
      }

      public static bool IsAdminFlagSet(int flags)
      {
         return (flags & s_adminFlag) > 0;
      }
   }
}

using System;

namespace Senesco.Client.Events
{
   public class UserInfoEventArgs : EventArgs
   {
      private readonly string m_userInfo;

      public string UserInfo
      {
         get { return m_userInfo; }
      }

      public UserInfoEventArgs(string userInfo)
      {
         m_userInfo = userInfo;
      }
   }
}

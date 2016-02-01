using System;

namespace Senesco.Client.Events
{
   public class PrivateMsgEventArgs : EventArgs
   {
      private readonly string m_sendingNick;
      private readonly int m_sendingUserId;
      private readonly string m_message;

      public string SendingNick
      {
         get { return m_sendingNick; }
      }

      public int SendingUserId
      {
         get { return m_sendingUserId; }
      }

      public string Message
      {
         get { return m_message; }
      }

      public PrivateMsgEventArgs(string sendingNick, int senderUserId, string message)
      {
         m_sendingNick = sendingNick;
         m_sendingUserId = senderUserId;
         m_message = message;
      }
   }
}

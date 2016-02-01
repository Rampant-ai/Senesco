using System;

namespace Senesco.Client.Events
{
   public class DisconnectedEventArgs : EventArgs
   {
      private readonly string m_reason;

      public string Reason
      {
         get { return m_reason; }
      }

      public DisconnectedEventArgs(string reason)
      {
         if (string.IsNullOrEmpty(reason))
            m_reason = "No reason given.";
         else
            m_reason = reason;
      }
   }
}

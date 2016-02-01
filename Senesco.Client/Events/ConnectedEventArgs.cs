using System;

namespace Senesco.Client.Events
{
   public class ConnectedEventArgs : EventArgs
   {
      private readonly string m_message;

      public string Message
      {
         get { return m_message; }
      }

      public ConnectedEventArgs(string message)
      {
         m_message = message;
      }
   }
}

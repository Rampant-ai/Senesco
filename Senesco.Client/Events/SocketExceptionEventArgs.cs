using System;

namespace Senesco.Client.Events
{
   public class SocketExceptionEventArgs : EventArgs
   {
      private readonly Exception m_exception;

      public Exception Exception
      {
         get { return m_exception; }
      }

      public SocketExceptionEventArgs(Exception e)
      {
         m_exception = e;
      }
   }
}

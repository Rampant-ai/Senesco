using System;

namespace Senesco.Client.Events
{
   public class ChatReceivedEventArgs : EventArgs
   {
      private readonly string m_text;
      private readonly int m_window;

      public string Text
      {
         get { return m_text; }
      }

      public int Window
      {
         get { return m_window; }
      }

      public ChatReceivedEventArgs(string text)
      {
         m_text = text;
         m_window = -1;
      }

      public ChatReceivedEventArgs(string text, int window)
      {
         m_text = text;
         m_window = window;
      }
   }
}

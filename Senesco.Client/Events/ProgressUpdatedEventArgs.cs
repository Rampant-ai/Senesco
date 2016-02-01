using System;

namespace Senesco.Client.Events
{
   public class ProgressUpdatedEventArgs : EventArgs
   {
      private readonly string m_eventUpdated;
      private readonly int m_progressPercent;

      public string EventUpdated
      {
         get { return m_eventUpdated; }
      }

      public int ProgressPercent
      {
         get { return m_progressPercent; }
      }

      public ProgressUpdatedEventArgs(string eventUpdated, int progressPercent)
      {
         m_eventUpdated = eventUpdated;
         m_progressPercent = progressPercent;
      }
   }
}

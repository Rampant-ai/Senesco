using System;

namespace Senesco.Client.Events
{
   public class NewVersionEventArgs : EventArgs
   {
      private readonly string m_currentVersion;
      private readonly string m_newVersion;

      public string CurrentVersion
      {
         get { return m_currentVersion; }
      }

      public string NewVersion
      {
         get { return m_newVersion; }
      }

      public NewVersionEventArgs(string currentVersion, string newVersion)
      {
         m_currentVersion = currentVersion;
         m_newVersion = newVersion;
      }
   }
}

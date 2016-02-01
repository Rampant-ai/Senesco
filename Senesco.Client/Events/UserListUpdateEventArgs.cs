using System;
using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Events
{
   public class UserListUpdateEventArgs : EventArgs
   {
      private readonly List<User> m_addList;
      private readonly List<User> m_removeList;
      private readonly bool m_delta;

      public List<User> AddList
      {
         get { return m_addList; }
      }

      public List<User> RemoveList
      {
         get { return m_removeList; }
      }

      public bool Delta
      {
         get { return m_delta; }
      }

      public UserListUpdateEventArgs(List<User> addList, List<User> removeList, bool delta)
      {
         m_addList = addList;
         m_removeList = removeList;
         m_delta = delta;
      }
   }
}

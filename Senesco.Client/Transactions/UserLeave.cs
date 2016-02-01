using log4net;
using Senesco.Client.Transactions.Objects;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions
{
   class UserLeave : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(UserLeave));

      private UserId m_userSocket;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public UserLeave()
         : base("UserLeave")
      {
      }

      public UserLeave(int userSocket)
         : base("UserLeave", false, 0)
      {
         m_userSocket = new UserId(userSocket);
         m_objectList.Add(m_userSocket);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(UserId))
               m_userSocket = obj as UserId;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }

      public User GetUser()
      {
         User user = new User();
         user.UserId = m_userSocket.Value.Value;
         //user.IconId = Icon.Value.Value;
         //user.Username = Nick.Value.Value;
         //user.Flags = UserStatus.Value.Value;
         return user;
      }
   }
}

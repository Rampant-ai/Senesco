using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class ChangeStatus : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(ChangeStatus));

      private Icon m_icon;
      private Nick m_nick;
      private UserStatus m_userStatus;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public ChangeStatus()
         : base("ChangeStatus")
      {
      }

      public ChangeStatus(int icon, string nick, int userStatus)
         : base("ChangeStatus", false, 0)
      {
         m_icon = new Icon(icon);
         m_objectList.Add(m_icon);

         m_nick = new Nick(nick);
         m_objectList.Add(m_nick);

         m_userStatus = new UserStatus(userStatus);
         m_objectList.Add(m_userStatus);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(Icon))
               m_icon = obj as Icon;
            else if (obj.GetType() == typeof(Nick))
               m_nick = obj as Nick;
            else if (obj.GetType() == typeof(UserStatus))
               m_userStatus = obj as UserStatus;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

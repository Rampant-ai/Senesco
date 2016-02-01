using log4net;
using Senesco.Client.Transactions.Objects;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions
{
   class UserChange : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(UserChange));

      public UserId UserId;
      public Icon Icon;
      public Nick Nick;
      public UserStatus UserStatus;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public UserChange()
         : base("UserChange")
      {
      }

      public UserChange(int userId, int icon, string nick, int userStatus)
         : base("UserChange", false, 0)
      {
         UserId = new UserId(userId);
         Icon = new Icon(icon);
         Nick = new Nick(nick);
         UserStatus = new UserStatus(userStatus);

         m_objectList.Add(UserId);
         m_objectList.Add(Icon);
         m_objectList.Add(Nick);
         m_objectList.Add(UserStatus);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(UserId))
               UserId = obj as UserId;
            else if (obj.GetType() == typeof(Icon))
               Icon = obj as Icon;
            else if (obj.GetType() == typeof(Nick))
               Nick = obj as Nick;
            else if (obj.GetType() == typeof(UserStatus))
               UserStatus = obj as UserStatus;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }

      public User GetUser()
      {
         User user = new User();
         user.UserId = UserId.Value.Value;
         user.IconId = Icon.Value.Value;
         user.Username = Nick.Value.Value;
         user.Flags = UserStatus.Value.Value;
         return user;
      }
   }
}

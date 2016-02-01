using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class Login : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(Login));

      private UserName m_login;
      private Password m_password;
      private Nick m_nick;
      private Icon m_icon;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public Login()
         : base("Login")
      {
      }

      public Login(string login, string password, string nick, int icon)
         : base("Login", false, 0)
      {
         m_login = new UserName(login);
         m_password = new Password(password);
         m_nick = new Nick(nick);
         m_icon = new Icon(icon);

         m_objectList.Add(m_login);
         m_objectList.Add(m_password);
         m_objectList.Add(m_nick);
         m_objectList.Add(m_icon);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(UserName))
               m_login = obj as UserName;
            else if (obj.GetType() == typeof(Password))
               m_password = obj as Password;
            else if (obj.GetType() == typeof(Nick))
               m_nick = obj as Nick;
            else if (obj.GetType() == typeof(Icon))
               m_icon = obj as Icon;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

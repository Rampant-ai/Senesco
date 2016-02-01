using System;
using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   public class SetUserInfo : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(GetUserInfo));

      private Nick m_nick;
      private Icon m_icon;
      private UserOptions m_userOptions;
      private AutoResponse m_autoResponse;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public SetUserInfo()
         : base("SetUserInfo")
      {
      }

      public SetUserInfo(string nick,
                         int icon,
                         bool ignorePrivateMsgs,
                         bool ignorePrivateChat,
                         string ignoreAutoResponse)
         : base("SetUserInfo", false, 0)
      {
         m_nick = new Nick(nick);
         m_icon = new Icon(icon);
         m_userOptions = new UserOptions(ignorePrivateMsgs, ignorePrivateChat, ignoreAutoResponse);

         if (String.IsNullOrEmpty(ignoreAutoResponse) == false)
            m_autoResponse = new AutoResponse(ignoreAutoResponse);

         m_objectList.Add(m_nick);
         m_objectList.Add(m_icon);
         m_objectList.Add(m_userOptions);

         if (m_autoResponse != null)
            m_objectList.Add(m_autoResponse);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(Nick))
               m_nick = obj as Nick;
            else if (obj.GetType() == typeof(Icon))
               m_icon = obj as Icon;
            else if (obj.GetType() == typeof(UserOptions))
               m_userOptions = obj as UserOptions;
            else if (obj.GetType() == typeof(AutoResponse))
               m_autoResponse = obj as AutoResponse;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

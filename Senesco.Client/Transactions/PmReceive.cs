using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class PmReceive : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(PmReceive));

      public UserId UserId;
      public Nick Nick;
      public Message Message;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public PmReceive()
         : base("PmReceive")
      {
      }

      public PmReceive(int userId, string nick, string message)
         : base("PmReceive", false, 0)
      {
         UserId = new UserId(userId);
         Nick = new Nick(nick);
         Message = new Message(message);

         m_objectList.Add(UserId);
         m_objectList.Add(Nick);
         m_objectList.Add(Message);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(UserId))
               UserId = obj as UserId;
            else if (obj.GetType() == typeof(Nick))
               Nick = obj as Nick;
            else if (obj.GetType() == typeof(Message))
               Message = obj as Message;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class PmSend : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(PmSend));

      public UserId UserId;
      public Message Message;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public PmSend()
         : base("PmSend")
      {
      }

      public PmSend(int userId, string message)
         : base("PmSend", false, 0)
      {
         UserId = new UserId(userId);
         Message = new Message(message);

         m_objectList.Add(UserId);
         m_objectList.Add(Message);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(UserId))
               UserId = obj as UserId;
            else if (obj.GetType() == typeof(Message))
               Message = obj as Message;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

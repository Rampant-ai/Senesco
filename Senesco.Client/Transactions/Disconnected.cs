using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class Disconnected : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(Disconnected));

      private Message m_message;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public Disconnected()
         : base("Disconnected")
      {
      }

      public Disconnected(int taskNumber, string message)
         : base("Disconnected", false, 0)
      {
         m_message = new Message(message);
         m_objectList.Add(m_message);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(Message))
               m_message = obj as Message;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

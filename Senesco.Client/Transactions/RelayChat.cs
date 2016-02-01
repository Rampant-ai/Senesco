using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class RelayChat : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(RelayChat));

      public Message Message;
      public ChatWindow ChatWindow;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public RelayChat()
         : base("RelayChat")
      {
      }

      public RelayChat(string message, int chatWindow)
         : base("RelayChat", false, 0)
      {
         Message = new Message(message);
         ChatWindow = new ChatWindow(chatWindow);

         m_objectList.Add(Message);
         m_objectList.Add(ChatWindow);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(Message))
               Message = obj as Message;
            else if (obj.GetType() == typeof(ChatWindow))
               ChatWindow = obj as ChatWindow;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class SendChat : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(SendChat));

      public Message Message;
      public ChatWindow ChatWindow;
      public Parameter Parameter;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public SendChat()
         : base("SendChat")
      {
      }

      public SendChat(string message, int? chatWindow, bool emote)
         : base("SendChat", false, 0)
      {
         // Message -- the text to be sent.
         Message = new Message(message);
         m_objectList.Add(Message);
         
         // Emote flag -- if the text is an emote.
         if (emote)
            Parameter = new Parameter(1);
         else
            Parameter = new Parameter(0);
         m_objectList.Add(Parameter);

         // Optional chat window number.
         if (chatWindow != null)
         {
            ChatWindow = new ChatWindow(chatWindow.Value);
            m_objectList.Add(ChatWindow);
         }
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(Message))
               Message = obj as Message;
            else if (obj.GetType() == typeof(ChatWindow))
               ChatWindow = obj as ChatWindow;
            else if (obj.GetType() == typeof(Parameter))
               Parameter = obj as Parameter;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }

      /// <summary>
      /// Returns the list of parameters for 
      /// </summary>
      /// <returns></returns>
      public override object[] GetDelegateArgs()
      {
         return null;
      }
   }
}

using Senesco.Client.Utility;
using Senesco.Client.Transactions;

namespace Senesco.Client.Events
{
   public class EventHandlers
   {
      public delegate void ProgressUpdatedEventHandler(object sender, ProgressUpdatedEventArgs e);
      public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs e);
      public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);
      public delegate void TransactionEventHandler(object sender, TransactionEventArgs e);
      public delegate void SocketExceptionEventHandler(object sender, SocketExceptionEventArgs e);
      public delegate void ChatReceivedEventHandler(object sender, ChatReceivedEventArgs e);
      public delegate void UserListUpdateEventHandler(object sender, UserListUpdateEventArgs e);
      public delegate void PrivateMsgEventHandler(object sender, PrivateMsgEventArgs e);
      public delegate void UserInfoDelegate(object sender, UserInfoEventArgs e);
      public delegate void NewVersionDelegate(object sender, NewVersionEventArgs e);
   }
}

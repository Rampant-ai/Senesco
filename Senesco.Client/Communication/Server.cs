using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Timers;
using log4net;
using Senesco.Client.Events;
using Senesco.Client.Transactions;
using Senesco.Client.Transactions.Objects;
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Communication
{
   [Serializable()]
   public class Server : ISerializable
   {
      #region Members and Creators

      private static readonly ILog s_log = LogManager.GetLogger(typeof(Server));

      public string ServerName;
      public string Address;
      public string LoginName;
      public string Password;
      public string Nick = "unnamed.senesco";
      public int Icon;

      public bool IsConnected
      {
         get { return m_channel == null ? false : m_channel.IsConnected; }
      }
      
      private Connection m_channel = null;
      private Timer m_keepAliveTimer = null;
      private const int c_keepAliveIntervalSeconds = 300; // 5 minutes

      public Server()
      {
      }

      public Server(SerializationInfo info, StreamingContext ctxt)
      {
         this.ServerName = info.GetString("ServerName");
         this.Address = info.GetString("Address");
         this.LoginName = info.GetString("LoginName");
         this.Password = info.GetString("Password");
         this.Nick = info.GetString("Nick");
         this.Icon = info.GetInt32("Icon");
      }
         
      public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
      {
         info.AddValue("ServerName", this.ServerName);
         info.AddValue("Address", this.Address.ToString());
         info.AddValue("LoginName", this.LoginName);
         info.AddValue("Password", this.Password);
         info.AddValue("Nick", this.Nick);
         info.AddValue("Icon", this.Icon);
      }

      public override string ToString()
      {
         bool first = true;
         StringBuilder sb = new StringBuilder();

         if (String.IsNullOrEmpty(ServerName) == false)
         {
            AddComma(ref first, sb);
            sb.Append("Name: ");
            sb.Append(ServerName);
         }

         if (String.IsNullOrEmpty(Address) == false)
         {
            AddComma(ref first, sb);
            sb.Append("Address: ");
            sb.Append(Address);
         }

         if (String.IsNullOrEmpty(LoginName) == false)
         {
            AddComma(ref first, sb);
            sb.Append("LoginName: ");
            sb.Append(LoginName);
         }

         if (String.IsNullOrEmpty(Password) == false)
         {
            AddComma(ref first, sb);
            sb.Append("Password: ");
            for (int i = 0; i < Password.Length; i++)
               sb.Append('*');
         }

         if (String.IsNullOrEmpty(Nick) == false)
         {
            AddComma(ref first, sb);
            sb.Append("Nick: ");
            sb.Append(Nick);
         }

         if (Icon != 0)
         {
            AddComma(ref first, sb);
            sb.Append("Icon: ");
            sb.Append(Icon);
         }

         return sb.ToString();
      }

      private static void AddComma(ref bool first, StringBuilder sb)
      {
         if (first)
            first = false;
         else
            sb.Append(", ");
      }

      #endregion

      #region Connect and Disconnect

      public Status Connect()
      {
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 0));
         Disconnect();

         // Only create a new Connection if we've never made one before.
         // Otherwise we re-use the same socket.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 10));
         if (m_channel == null)
            m_channel = new Connection();

         // Establish TCP connection.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 20));
         if (m_channel.Connect(this) == Status.Failure)
            return Status.Failure;

         // Send Hotline handshake.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 30));
         Package clientHello = new Package(new RawString("TRTPHOTL"), 0, 1, 0, 2);
         if (m_channel.Send(clientHello) == Status.Failure)
            return Status.Failure;

         // Start the socket data reader thread so incoming data will fill our byte queue.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 50));
         m_channel.BeginSocketReceive();

         // Read the handshake response from the server.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 60));
         Package serverHello;
         if (m_channel.ReceiveHandshake(out serverHello) == Status.Failure)
            return Status.Failure;

         // Confirm the correct handshake response.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 70));
         if (serverHello != new Package(new RawString("TRTP"), 0, 0, 0, 0))
         {
            s_log.ErrorFormat("Incorrect handshake received.");
            return Status.Failure;
         }
         s_log.InfoFormat("Correct handshake received!");

         // Specify the delegate to call when a transaction is received.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 80));
         m_channel.TransactionReceived += ReceiveTransaction;
         m_channel.SocketException += HandleSocketException;

         // Start the socket listening for transactions.
         m_channel.ListenForTransactions();

         // Start keep-alive timer.
         OnProgressUpdated(new ProgressUpdatedEventArgs("Connect to Server", 90));
         StartKeepAlive();

         return Status.Success;
      }

      public void Disconnect()
      {
         s_log.Debug("Server.Disconnect() called.");
         if (m_channel != null)
         {
            m_channel.Disconnect();
         }

         if (m_keepAliveTimer != null)
         {
            s_log.Info("Stopping KeepAlive timer.");
            m_keepAliveTimer.Stop();
            m_keepAliveTimer = null;
         }
      }

      #endregion

      #region KeepAlive Timer

      private void StartKeepAlive()
      {
         if (m_keepAliveTimer != null)
         {
            s_log.Info("Stopping previous KeepAlive timer.");
            m_keepAliveTimer.Stop();
         }

         // Create a new KeepAlive timer using the specified interval.
         double seconds = c_keepAliveIntervalSeconds;
         s_log.InfoFormat("Starting new KeepAlive timer that fires every {0} seconds.", seconds);
         m_keepAliveTimer = new Timer(seconds * 1000);
         m_keepAliveTimer.Elapsed += new ElapsedEventHandler(keepAlive_timerElapsed);
         m_keepAliveTimer.AutoReset = true;
         m_keepAliveTimer.Start();
      }

      public void keepAlive_timerElapsed(object sender, ElapsedEventArgs e)
      {
         s_log.Info("KeepAlive timer event elapsed.");

         if (m_channel == null)
            return;

         s_log.Info("Sending KeepAlive transaction.");

         // FIXME: use a smaller transaction.
         Transaction keepAlive = new GetUserList(null);

         m_channel.SendTransaction(keepAlive);
      }

      #endregion

      #region Send and Receive Transaction

      public Status SendTransaction(Transaction tx)
      {
         if (m_channel == null)
         {
            s_log.ErrorFormat("Not connected to server.");
            return Status.Failure;
         }

         if (tx == null)
         {
            s_log.ErrorFormat("Null transaction provided.  Not sending.");
            return Status.Failure;
         }

         // Returns Success if the transaction was successfully queued.
         return m_channel.SendTransaction(tx);
      }

      public void ReceiveTransaction(object sender, TransactionEventArgs e)
      {
         Transaction inTransaction = e.IncomingTransaction;
         Transaction replyTo = e.ReplyToTransaction;

         if (inTransaction == null)
         {
            s_log.ErrorFormat("No incoming transaction!");
            return;
         }

         Type inType = inTransaction.GetType();
         Type replyType = (replyTo == null) ? null : replyTo.GetType();

         s_log.InfoFormat("Received Transaction ID {0} '{1}': ({2})",
                          inTransaction.Id.Value, inTransaction.Name, inType.ToString());

         if (replyTo != null)
         {
            s_log.InfoFormat("In reply to ID {0} '{1}': ({2})",
                             replyTo.Id.Value, replyTo.Name, replyType.ToString());
         }

         // If it has an error code, display the error.
         if (inType == typeof(RelayChat))
            HandleRelayChat(inTransaction as RelayChat);
         else if (inType == typeof(Disconnected))
            HandleDisconnected(inTransaction as Disconnected);
         else if (replyType == typeof(Login))
            HandleLoginResponse(inTransaction);
         else if (replyType == typeof(GetUserList))
            HandleGetUserList(inTransaction);
         else if (inType == typeof(UserChange))
            HandleUserUpdate(inTransaction as UserChange);
         else if (inType == typeof(UserLeave))
            HandleUserLeave(inTransaction as UserLeave);
         else if (inType == typeof(PmReceive))
            HandleReceivePm(inTransaction as PmReceive);
         else if (replyType == typeof(GetUserInfo))
            HandleGotUserInfo(inTransaction);
         else if (inType == typeof(Transaction))
            s_log.WarnFormat("Unsupported Transaction received: {0}", inTransaction.Id.Value);
         else if (inTransaction.ErrorCode > 0)
            HandleError(inTransaction);
         else 
            s_log.WarnFormat("Unsupported Transaction {0} '{1}' received: ({2})",
                             inTransaction.Id.Value, inTransaction.Name, inTransaction.GetType().ToString());

         return;
      }

      #endregion

      #region Transaction Handler Methods
      
      private void HandleError(Transaction inTransaction)
      {
         s_log.InfoFormat("Received error transaction: {0}", inTransaction.ErrorCode);
         ErrorMessage errMsg = (ErrorMessage)ObjectFactory.FindObject(inTransaction.Objects, typeof(ErrorMessage));
         string text = String.Format("*** Error: {0} ***", errMsg.Value.Value);
         OnChatReceived(new ChatReceivedEventArgs(text));
      }

      private void HandleLoginResponse(Transaction loginResponse)
      {
         s_log.InfoFormat("Received login response: {0}", loginResponse.ErrorCode);

         if (loginResponse.ErrorCode != 0)
         {
            OnDisconnected(new DisconnectedEventArgs("Login failed."));
            Disconnect();
         }
         else
         {
            // If the error code is zero, the login was succesful.
            // Some servers return a ServerName object, so print it if found.
            ServerName serverName = (ServerName)ObjectFactory.FindObject(loginResponse.Objects, typeof(ServerName));
            if (ChatReceived != null)
            {
               if (serverName == null)
               {
                  OnConnected(new ConnectedEventArgs("*** Login complete. ***"));
               }
               else
               {
                  string text = String.Format("*** Server Name: {0} ***", serverName.Value.Value);
                  OnConnected(new ConnectedEventArgs(text));
               }
            }
         }
      }

      private void HandleDisconnected(Disconnected disconnected)
      {
         s_log.InfoFormat("Received Disconnected event.");

         // Notify the subscribers of the Disconnected event.
         OnDisconnected(new DisconnectedEventArgs(String.Format("{0}", disconnected.ErrorCode)));

         // Disconnect from the server since it's done talking to us.
         Disconnect();
      }

      private void HandleSocketException(object sender, SocketExceptionEventArgs e)
      {
         s_log.ErrorFormat("Socket threw exception: {0}", e.Exception.Message);

         // Notify the subscribers of the Disconnected event.
         OnDisconnected(new DisconnectedEventArgs(String.Format("{0}", e.Exception.Message)));

         // Clean up the socket management since there's no connected socket anymore.
         Disconnect();
      }

      private void HandleRelayChat(RelayChat relayChat)
      {
         s_log.InfoFormat("Received RelayChat event.");
         if (relayChat == null)
            return;

         // The ChatWindow object is only specified for private chats.
         int chatWindow = -1;
         if (relayChat.ChatWindow != null)
         {
            chatWindow = relayChat.ChatWindow.Value.Value;
            s_log.DebugFormat("Chat is for window {0}.", chatWindow);
         }

         s_log.DebugFormat("Chat text: {0}", relayChat.Message.Value.Value);

         // Submit this transaction's "message" object to the chat delegate.
         OnChatReceived(new ChatReceivedEventArgs(relayChat.Message.Value.Value, chatWindow));
      }

      /// <summary>
      /// Handler for a Transaction response listing the complete userlist.
      /// NOTE: It is not the responsibility of the Server class to decide how
      /// the response data is displayed to the user.  That is, do NOT
      /// automatically make this a chat message!
      /// </summary>
      private void HandleGetUserList(Transaction userList)
      {
         s_log.InfoFormat("Received userlist response.");
         if (userList == null)
            return;

         // Create a new userlist from scratch.
         List<User> addList = new List<User>();

         foreach (HotlineObject obj in userList.Objects)
         {
            UserListEntry ule = obj as UserListEntry;
            if (ule == null)
               continue;

            User user = new User(ule);
            addList.Add(user);
         }

         // Send the new full user list as a non-delta list of "adds".
         OnUserListUpdate(new UserListUpdateEventArgs(addList, null, false));
      }

      private void HandleUserUpdate(UserChange userChange)
      {
         s_log.InfoFormat("Received UserChange transaction.");
         if (userChange == null)
            return;
         
         List<User> addList = new List<User>();
         addList.Add(userChange.GetUser());

         // Raise the event.
         OnUserListUpdate(new UserListUpdateEventArgs(addList, null, true));
      }

      private void HandleUserLeave(UserLeave userLeave)
      {
         s_log.InfoFormat("Received UserLeave transaction.");
         if (userLeave == null)
            return;

         List<User> removeList = new List<User>();
         removeList.Add(userLeave.GetUser());

         // Raise the event.
         OnUserListUpdate(new UserListUpdateEventArgs(null, removeList, true));
      }

      private void HandleGotUserInfo(Transaction userInfoTransaction)
      {
         s_log.InfoFormat("Received GetUserInfo response.");
         if (userInfoTransaction == null)
            return;

         Message msg = (Message)ObjectFactory.FindObject(userInfoTransaction.Objects, typeof(Message));
         Nick nick = (Nick)ObjectFactory.FindObject(userInfoTransaction.Objects, typeof(Nick));

         if (msg == null || nick == null)
            return;

         string formattedMsg = String.Format("*** User Details for {0}: ***\n\n{1}",
                                             nick.Value.Value, msg.Value.Value);

         // Raise the event.
         OnUserInfoReceived(new UserInfoEventArgs(formattedMsg));
      }

      private void HandleReceivePm(PmReceive pmReceive)
      {
         s_log.InfoFormat("Received Private Message transaction.");
         if (pmReceive == null)
            return;

         // Raise the event.
         OnPmReceived(new PrivateMsgEventArgs(pmReceive.Nick.Value.Value, pmReceive.UserId.Value.Value, pmReceive.Message.Value.Value));
      }

      #endregion

      #region Events

      // Define events
      public event EventHandlers.ProgressUpdatedEventHandler ProgressUpdated;
      public event EventHandlers.ConnectedEventHandler Connected;
      public event EventHandlers.DisconnectedEventHandler Disconnected;
      public event EventHandlers.ChatReceivedEventHandler ChatReceived;
      public event EventHandlers.UserListUpdateEventHandler UserListUpdate;
      public event EventHandlers.PrivateMsgEventHandler PmReceived;
      public event EventHandlers.UserInfoDelegate UserInfoReceived;

      // Methods to raise each event
      protected virtual void OnProgressUpdated(ProgressUpdatedEventArgs e)
      {
         if (ProgressUpdated != null)
            ProgressUpdated(this, e);
      }

      protected virtual void OnConnected(ConnectedEventArgs e)
      {
         if (Connected != null)
            Connected(this, e);
      }

      protected virtual void OnDisconnected(DisconnectedEventArgs e)
      {
         if (Disconnected != null)
            Disconnected(this, e);
      }

      protected virtual void OnChatReceived(ChatReceivedEventArgs e)
      {
         if (ChatReceived != null)
            ChatReceived(this, e);
      }

      protected virtual void OnUserListUpdate(UserListUpdateEventArgs e)
      {
         if (UserListUpdate != null)
            UserListUpdate(this, e);
      }

      protected virtual void OnPmReceived(PrivateMsgEventArgs e)
      {
         if (PmReceived != null)
            PmReceived(this, e);
      }

      protected virtual void OnUserInfoReceived(UserInfoEventArgs e)
      {
         if (UserInfoReceived != null)
            UserInfoReceived(this, e);
      }

      #endregion
   }
}

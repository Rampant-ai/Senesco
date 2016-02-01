using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using Senesco.Client.Transactions;
using Senesco.Client.Utility;
using Senesco.Client.Events;

namespace Senesco.Client.Communication
{
   class Connection
   {
      #region Fields and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(Connection));

      private int m_bufferSize = 10240;
      private Server m_server = null;
      private Socket m_socket = null;
      private Thread m_receiveThread = null;
      private Queue<byte> m_receiveQueue = null;
      private AutoResetEvent m_receiveEvent = new AutoResetEvent(false);

      private Thread m_transactionThread = null;
      private bool m_processTransactions = true;

      private Thread m_sendThread = null;
      private Queue<Transaction> m_sendQueue = new Queue<Transaction>();
      private AutoResetEvent m_sendEvent = new AutoResetEvent(false);

      public bool IsConnected
      {
         get { return m_socket == null ? false : m_socket.Connected; }
      }

      /// <summary>
      /// This event is fired when a Transaction is received from the server.
      /// </summary>
      public event EventHandlers.TransactionEventHandler TransactionReceived;
      protected virtual void OnTransactionReceived(TransactionEventArgs e)
      {
         if (TransactionReceived != null)
            TransactionReceived(this, e);
      }

      /// <summary>
      /// This event is fired when the socket throws an exception.
      /// </summary>
      public event EventHandlers.SocketExceptionEventHandler SocketException;
      protected virtual void OnSocketException(SocketExceptionEventArgs e)
      {
         if (SocketException != null)
            SocketException(this, e);
      }

      /// <summary>
      /// Creator
      /// </summary>
      internal Connection()
      {
         m_receiveQueue = new Queue<byte>(m_bufferSize);
      }

      #endregion

      #region Connect and Disconnect

      internal Status Connect(Server target)
      {
         if (target == null)
            return Status.Failure;
         m_server = target;

         try
         {
            // Parse address information from the server target.
            IPAddress ipAddress;
            AddressFamily targetFamily;
            int targetPort;
            if (ParseAddress(target.Address, out ipAddress, out targetFamily, out targetPort) == Status.Failure)
               return Status.Failure;

            // If the socket is already connected, disconnect it.
            if (m_socket != null && m_socket.Connected == true)
               m_socket.Disconnect(true);

            // If the socket has not been created or will be changing address families, create a new socket.
            if (m_socket == null || m_socket.AddressFamily != targetFamily)
            {
               // Release the system resources before we create a new socket.
               if (m_socket != null)
                  m_socket.Close();
               m_socket = new Socket(targetFamily, SocketType.Stream, ProtocolType.Tcp);
            }

            m_socket.Connect(ipAddress, targetPort);
            s_log.InfoFormat("Connected to '{0}' at '{1}' on port {2}", target.ServerName, target.Address, targetPort);

            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception opening socket for {0}: {1}", target.ServerName, e.Message);
            m_server = null;
            return Status.Failure;
         }
      }

      private Status ParseAddress(string input, out IPAddress ipAddress, out AddressFamily targetFamily, out int port)
      {
         // Several cases:
         // 1. Domain name
         // 2. Domain name with port
         // 3. IPv4
         // 4. IPv4 with port
         // 5. IPv6
         // 6. IPv6 with port

         // Pull off the suffixed port, if present.
         // This assumes IPv4 for now.
         port = ParsePort(ref input, AddressFamily.InterNetwork);
         ipAddress = null;
         targetFamily = AddressFamily.Unknown;

         try
         {
            // It appears this call "resolves" IP addresses too, so maybe this is a catch-all solution.
            IPAddress[] addresses = Dns.GetHostAddresses(input);
            foreach (IPAddress address in addresses)
            {
               s_log.DebugFormat("Resolved '{0}' to: {1}", input, address.ToString());
               if (ipAddress == null)
               {
                  s_log.InfoFormat("Using first address: {0}", address.ToString());
                  ipAddress = address;
                  targetFamily = ipAddress.AddressFamily;
               }
            }
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("DNS lookup for '{0}' failed: {1}", input, e.Message);
         }

         // If the DNS lookup worked, we're all done.
         if (ipAddress != null)
            return Status.Success;

         // Maybe it was already an IP address...
         try
         {
            ipAddress = IPAddress.Parse(input);
            targetFamily = ipAddress.AddressFamily;
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Parsing '{0}' as IP Address failed: {1}", input, e.Message);
            return Status.Failure;
         }
      }

      private int ParsePort(ref string address, AddressFamily targetFamily)
      {
         if (targetFamily == AddressFamily.InterNetworkV6)
            throw new NotImplementedException();

         // For IPv4, take the substring after the colon, if any.
         int colonIndex = address.LastIndexOf(':');
         if (colonIndex == -1)
            return 5500;

         // Otherwise we found a colon, so what's after the colon is the port
         // and what's before is the address.
         string portString = address.Substring(colonIndex + 1);
         address = address.Substring(0, colonIndex);

         // If the text after the port is empty, use the default.
         if (String.IsNullOrEmpty(portString))
            return 5500;

         // Parse to an integer.
         return int.Parse(portString);
      }

      internal Status Disconnect()
      {
         // Terminate the socket receive thread.
         if (m_receiveThread != null)
         {
            try { m_receiveThread.Abort(); }
            catch { }
            m_receiveThread = null;
         }

         // Clear the received byte queue.
         if (m_receiveQueue != null)
         {
            m_receiveQueue.Clear();
         }

         // Terminate the socket send thread.
         if (m_sendThread != null)
         {
            try { m_sendThread.Abort(); }
            catch { }
            m_sendThread = null;
         }

         // Clear the send queue of transactions.
         if (m_sendQueue != null)
         {
            m_sendQueue.Clear();
         }

         // Clear all subscriptions.
         this.SocketException = null;
         this.TransactionReceived = null;

         // Rather than abort the Transaction thread, signal it to terminate on the next loop.
         // This is important, because we're likely in that thread right now.
         m_processTransactions = false;

         // Nothing to do?
         if (m_socket == null || m_socket.Connected == false)
            return Status.NoResult;

         // Disconnect the socket.
         try
         {
            m_socket.DisconnectAsync(new SocketAsyncEventArgs() { DisconnectReuseSocket = true });
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception disconnecting socket: {0}", e.Message);
            return Status.Failure;
         }
      }

      #endregion

      #region Send Data

      internal Status Send(Package package)
      {
         if (package == null)
         {
            s_log.ErrorFormat("Cannot send null package.");
            return Status.Failure;
         }

         if (m_socket == null || m_socket.Connected == false)
         {
            s_log.ErrorFormat("Socket not connected!");
            return Status.Failure;
         }

         try
         {
            SendBytes(package.GetBytes());
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error sending Package({0}): {1}", package.GetSize(), e.Message);
         }

         return Status.NoResult;
      }

      internal Status SendTransaction(Transaction transaction)
      {
         //TODO: Check socket state for being disconnected.

         // Validate transaction
         if (transaction == null)
         {
            s_log.ErrorFormat("Cannot send null package.");
            return Status.Failure;
         }

         // Enqueue Package.
         lock (m_sendQueue)
         {
            m_sendQueue.Enqueue(transaction);
         }

         // Notify other thread.
         m_sendEvent.Set();

         // Start other thread if necessary.
         if (m_sendThread == null)
         {
            m_sendThread = new Thread(SendLoop);
            m_sendThread.Name = "Transaction Sending";
            m_sendThread.Start();
         }

         return Status.Success;
      }

      private void SendLoop()
      {
         while (true)
         {
            // Get the next transaction if possible.
            Transaction transaction = null;
            lock (m_sendQueue)
            {
               if (m_sendQueue.Count > 0)
                  transaction = m_sendQueue.Dequeue();
            }

            // If no transaction was in the queue, sleep.
            if (transaction == null)
            {
               m_sendEvent.WaitOne();
            }
            else // Otherwise process the transaction.
            {
               try
               {
                  SendBytes(transaction.GetBytes());
               }
               catch (Exception e)
               {
                  s_log.ErrorFormat("Error sending Transaction '{0}': {1}",
                     (transaction == null) ? "(null)" : transaction.Name,
                     e.Message);
               }
            }
         }
      }

      private void SendBytes(byte[] bytes)
      {
         DataUtils.DumpBytes("Sending bytes:", bytes);

         lock (m_socket)
            m_socket.Send(bytes);
      }

      #endregion

      #region Receive Data

      public void BeginSocketReceive()
      {
         // Make sure the "received data" queue is empty, since it may contain
         // partial data from a prior interrupted connection.
         // No lock required because the other thread must not be active yet.
         m_receiveQueue.Clear();

         StartReceiveThread();
      }

      private void StartReceiveThread()
      {
         if (m_receiveThread != null)
            m_receiveThread.Abort();
         m_receiveThread = new Thread(ReceiveLoop);
         m_receiveThread.Name = "Socket Receiving";
         m_receiveThread.Start();
      }

      private void ReceiveLoop()
      {
         try
         {
            // If there is no connected socket, bail out.
            if (m_socket == null || m_socket.Connected == false)
            {
               s_log.ErrorFormat("Socket not connected!");
               m_receiveThread = null;
               return;
            }

            byte[] receiveBuffer = new byte[m_bufferSize];
            int receivedCount;

            // Listen on the socket indefinitely.
            while (true)
            {
               // Synchronously wait for data.
               receivedCount = m_socket.Receive(receiveBuffer);

               // Sometimes with failed logins there's an endless loop of receiving
               // zero bytes off the socket?
               if (receivedCount == 0)
                  continue;

               // Data has arrived.  Get a thread lock on the byte queue.
               lock (m_receiveQueue)
               {
                  // Enqueue all bytes received.
                  for (int i = 0; i < receivedCount; i++)
                     m_receiveQueue.Enqueue(receiveBuffer[i]);

                  // Notify the receive event that new data is in the queue.
                  m_receiveEvent.Set();
               }
            }
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Socket threw exception: {0}", e.Message);

            // After this exception is handled, the thread will terminate.
            if (m_receiveThread != null)
               m_receiveThread = null;

            if (m_socket != null)
            {
               try { m_socket.Disconnect(false); }
               catch { }
               m_socket = null;
            }

            // Fire the event so the system can react to the loss of connection.
            OnSocketException(new SocketExceptionEventArgs(e));
         }
      }

      /// <summary>
      /// Synchronous method that waits for the specific handshake to arrive
      /// from the socket connection.
      /// </summary>
      /// <param name="serverHello">The received handshake.</param>
      /// <returns>
      /// Status.Failure if the handshake was not received within 30 seconds.
      /// </returns>
      internal Status ReceiveHandshake(out Package serverHello)
      {
         serverHello = null;
         TimeSpan timeout = new TimeSpan(0, 0, 30);

         // Loop indefinitely while waiting for the handshake response.
         // Note that the ResetEvent may wait up to 30 seconds each time a
         // signal is received from the socket's thread.
         while (true)
         {
            lock (m_receiveQueue)
            {
               // Break this loop only if we've received 8 or more bytes.
               if (m_receiveQueue.Count >= 8)
                  break;
            }

            // Wait for "data received" notification from the socket.
            // If this returns false, the wait has timed out.
            if (m_receiveEvent.WaitOne(timeout) == false)
            {
               s_log.ErrorFormat("Timed out waiting for response.");
               return Status.Failure;
            }
         }

         serverHello = new Package();
         List<byte> receivedBytes = new List<byte>();
         lock (m_receiveQueue)
         {
            for (int i = 0; i < 8; i++)
               receivedBytes.Add(m_receiveQueue.Dequeue());
         }
         serverHello.ReceivedBytes(receivedBytes);
         
         return Status.Success;
      }

      internal void ListenForTransactions()
      {
         if (m_transactionThread != null)
            m_transactionThread.Abort();
         m_processTransactions = true;
         m_transactionThread = new Thread(TransactionLoop);
         m_transactionThread.Name = "Transaction Processing";
         m_transactionThread.Start();
      }

      private void TransactionLoop()
      {
         List<byte> potentialTransaction = new List<byte>();
         while (m_processTransactions)
         {
            // Wait indefinitely for notification that the socket has received data.
            m_receiveEvent.WaitOne();

            // Get all available bytes from the socket queue.
            // Append the bytes to the current "partial transaction" in case the socket
            // receives the transaction in pieces.
            lock (m_receiveQueue)
            {
               potentialTransaction.AddRange(m_receiveQueue);
               m_receiveQueue.Clear();
            }

            // Attempt to parse a transaction from the latest list of bytes.
            Transaction transaction, replyTo;
            if (Transaction.TryParse(ref potentialTransaction, out transaction, out replyTo) == Status.Success)
            {
               // If the transaction was successfully parsed, fire the event.
               OnTransactionReceived(new TransactionEventArgs(transaction, replyTo));

               // If there are additional bytes leftover after the transaction we just parsed,
               // then this could be the start of the next transaction, so leave them in the list.
               // However, we also do not want to wait for the next socket receive in case
               // the next transaction is already complete, so notify the event for the next loop.
               if (potentialTransaction != null && potentialTransaction.Count != 0)
                  m_receiveEvent.Set();
            }

            // Otherwise the transaction was not complete, so we just hold the partial byte list
            // for the next try when more data is received on the socket.
         }
      }

      #endregion
   }
}

using System;
using System.Collections.Generic;
using System.Timers;
using log4net;

namespace Senesco.Client.Transactions
{
   class TransactionFactory
   {
      #region Fields and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(TransactionFactory));

      private static bool s_initialized = false;

      static TransactionFactory()
      {
         s_log.DebugFormat("TransactionFactory static constructor called");
         if (s_initialized == false)
         {
            InitializeIdMaps();
            StartCleanupTimer();
            s_initialized = true;
         }
      }

      #endregion

      #region ID Number Management

      private static Dictionary<int, Type> s_idMap = new Dictionary<int, Type>();
      private static Dictionary<Type, int> s_typeMap = new Dictionary<Type, int>();

      private static void InitializeIdMaps()
      {
         s_log.DebugFormat("Initializing TransactionFactory ID lists");

         s_idMap[0] = typeof(Transaction);
         // Note: several transactions use ID 104!
         s_idMap[104] = typeof(PmReceive);      // Receive Private Message
         s_idMap[105] = typeof(SendChat);       // Send chat
         s_idMap[106] = typeof(RelayChat);      // Receive chat
         s_idMap[107] = typeof(Login);          // Send login credentials
         s_idMap[108] = typeof(PmSend);         // Send Private Message
         //s_idMap[109] = typeof(ShowAgreement);  // Show Agreement.
         s_idMap[111] = typeof(Disconnected);   // You have been disconnected.
         s_idMap[300] = typeof(GetUserList);    // Get the list of users.
         s_idMap[301] = typeof(UserChange);     // A user has changed (nick, icon, status).
         s_idMap[302] = typeof(UserLeave);      // A user has left.
         s_idMap[303] = typeof(GetUserInfo);    // Get details for a specific user.
         s_idMap[304] = typeof(SetUserInfo);    // Change user settings (nick, icon, settings).
         //s_idMap[354] = typeof(UserAccess);     // Permissions for the authenticated user.

         // The other map is the exact opposite, which we can generate
         // with this simple loop.
         foreach (KeyValuePair<int, Type> kvp in s_idMap)
            s_typeMap[kvp.Value] = kvp.Key;
      }

      internal static int GetIdByType(Type type)
      {
         return s_typeMap[type];
      }

      #endregion

      #region Factory

      internal static Transaction Create(bool reply, int transactionId, int taskNumber, int errorCode,
                                         byte[] transactionData, out Transaction replyTo)
      {
         replyTo = null;
         try
         {
            // Figure out what type is associated with the given ID number.
            // The default for unknown ID values is the base class.
            Type transType;
            if (s_idMap.TryGetValue(transactionId, out transType) == false)
            {
               s_log.InfoFormat("Unknown Transaction ID {0}", transactionId);
               transType = typeof(Transaction);
            }

            // Create an instance of that Type.
            Transaction transaction = (Transaction)Activator.CreateInstance(transType);

            // Configure the base instance with the fields we have.
            transaction.Configure(reply, transactionId, taskNumber, errorCode);

            // Have the transaction parse its own details.
            transaction.ParseObjects(transactionData);

            // Look up the transaction that this is a reply to.
            replyTo = Transaction.GetSourceTransaction(taskNumber);

            // Return the finished transaction.
            return transaction;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception generating Transaction ID {0}: {1}", transactionId, e.Message);
            return null;
         }
      }

      #endregion

      #region Transaction Management

      // Static list for keeping track of sent tasks.  Server replies don't
      // always have enough stand-alone context, so this is used to look up
      // the original Transaction that the server is replying to.
      private static Dictionary<int, Transaction> s_activeTransactions = new Dictionary<int, Transaction>();

      public static void AddActiveTransaction(int taskNumber, Transaction transaction)
      {
         lock (s_activeTransactions)
         {
            s_activeTransactions.Add(taskNumber, transaction);
         }
      }

      public static Transaction FindActiveTransaction(int taskNumber)
      {
         Transaction transaction;

         lock (s_activeTransactions)
         {
            if (s_activeTransactions.TryGetValue(taskNumber, out transaction) == true)
            {
               // This transaction is completed, so remove it from the list.
               // NOTE: It looks like one outbound Transaction can have multiple replies,
               // like Login, but we can just let ALL transactions get cleaned up with
               // the existing cleanup timer!
               //s_activeTransactions.Remove(taskNumber);
            }
         }

         return transaction;
      }

      #endregion

      #region Cleanup Timer

      private static Timer s_cleanupTimer = null;

      private static void StartCleanupTimer()
      {
         s_log.DebugFormat("Initializing Transaction cleanup timer");

         // Run the timer every minute.
         s_cleanupTimer = new Timer(60000);
         s_cleanupTimer.AutoReset = true;
         s_cleanupTimer.Elapsed += delegate(object sender, ElapsedEventArgs e) { CleanOldTransactions(); };
         s_cleanupTimer.Start();
      }

      private static void CleanOldTransactions()
      {
         s_log.DebugFormat("Searching for old Transactions...");

         // There should be no reason to keep a transaction older than 5 minutes.
         DateTime deleteThreshold = DateTime.Now - new TimeSpan(0, 5, 0);

         // Obtain a lock on the list.
         List<int> toDelete = new List<int>();
         lock (s_activeTransactions)
         {
            // Loop through all items.
            foreach (KeyValuePair<int, Transaction> kvp in s_activeTransactions)
            {
               // If this item is older than the maximum allowed, add the key to the delete list.
               if (kvp.Value.Created < deleteThreshold)
                  toDelete.Add(kvp.Key);
            }

            // Run the delete list after the above loop is done since we can't delete while enumerating.
            s_log.DebugFormat("Removing {0} Transactions...", toDelete.Count);
            foreach (int i in toDelete)
               s_activeTransactions.Remove(i);
         }
      }

      #endregion
   }
}

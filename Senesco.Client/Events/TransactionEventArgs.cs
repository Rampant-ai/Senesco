using System;
using Senesco.Client.Transactions;

namespace Senesco.Client.Events
{
   public class TransactionEventArgs : EventArgs
   {
      private readonly Transaction m_incomingTransaction;
      private readonly Transaction m_replyToTransaction;
      
      public Transaction IncomingTransaction
      {
         get { return m_incomingTransaction; }
      }

      public Transaction ReplyToTransaction
      {
         get { return m_replyToTransaction; }
      }

      public TransactionEventArgs(Transaction incomingTransaction, Transaction replyToTransaction)
      {
         m_incomingTransaction = incomingTransaction;
         m_replyToTransaction = replyToTransaction;
      }
   }
}

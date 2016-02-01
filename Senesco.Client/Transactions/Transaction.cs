using System;
using System.Collections.Generic;
using log4net;
using Senesco.Client.Transactions.Objects;
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions
{
   public class Transaction
   {
      #region Fields and Creators

      private static readonly ILog s_log = LogManager.GetLogger(typeof(Transaction));

      public string Name;
      private Short m_class;
      public Short Id;
      private Long m_taskNumber;
      private Long m_errorCode;

      // Getter for the task number.
      public int TaskNumber
      {
         // Returns zero if set to null.
         //TODO: verify that negative numbers are coming out right.
         get { return (m_taskNumber == null) ? 0 : m_taskNumber.Value; }
      }

      // Getter for the error code.
      public int ErrorCode
      {
         // Returns zero if set to null.
         //TODO: verify that negative numbers are coming out right.
         get { return (m_errorCode == null) ? 0 : m_errorCode.Value; }
      }

      protected List<HotlineObject> m_objectList = new List<HotlineObject>();

      public IEnumerable<HotlineObject> Objects
      {
         get
         {
            if (m_objectList == null)
               return null;
            else
               return m_objectList;
         }
      }

      private DateTime m_createTime;

      public DateTime Created
      {
         get { return m_createTime; }
      }

      /// <summary>
      /// Default constructor for the Activator to use.
      /// This creator does not automatically set a task number because it is
      /// used to create incoming transaction objects, which are read from the
      /// incoming data (so the task number is specified by the server).
      /// </summary>
      public Transaction()
      {
         m_createTime = DateTime.Now;
         Name = "NoName";
      }

      /// <summary>
      /// Simple constructor for default constructors in inherited classes to use.
      /// Basically, this only preserves the name.
      /// </summary>
      public Transaction(string name)
      {
         SetTaskNumber();
         Name = name;
         m_createTime = DateTime.Now;
      }

      public Transaction(string name, bool reply, int errorCode)
      {
         SetTaskNumber();
         Name = name;
         m_createTime = DateTime.Now;

         // Each object that inherits from Transaction must have its ID listed
         // in the TransactionFactory class.
         int id = TransactionFactory.GetIdByType(this.GetType());

         Configure(reply, id, errorCode);
      }

      public void Configure(bool reply, int id, int errorCode)
      {
         Id = new Short(id);
         m_errorCode = new Long(errorCode);

         if (reply)
            m_class = new Short(1);
         else
            m_class = new Short(0);
      }

      public void Configure(bool reply, int id, int taskNumber, int errorCode)
      {
         Id = new Short(id);
         m_errorCode = new Long(errorCode);
         m_taskNumber = new Long(taskNumber);

         if (reply)
            m_class = new Short(1);
         else
            m_class = new Short(0);
      }

      #endregion

      #region Task Management

      // Static counter for giving each Transaction a unique task number.
      // Start at 1000 to make sure we don't confuse it with any other 3-digit
      // ID numbers for the various objects...
      private static int s_taskCounter = 1000;

      private int SetTaskNumber()
      {
         // Get a task number and increment the counter so each number is used only once.
         int taskNumber = s_taskCounter++;

         // Record the task number in active task list.
         TransactionFactory.AddActiveTransaction(taskNumber, this);

         // Set the task number as a Long in this instance.
         m_taskNumber = new Long(taskNumber);

         // Return the assigned integer.
         return taskNumber;
      }

      public static Transaction GetSourceTransaction(int taskNumber)
      {
         // Look up the task number in the list of active tasks.
         Transaction transaction = TransactionFactory.FindActiveTransaction(taskNumber);
         if (transaction != null)
         {
            s_log.InfoFormat("Found active Task Number {0}, type {1}.", taskNumber, transaction.GetType().ToString());
            return transaction;
         }

         s_log.WarnFormat("No active transaction found for Task Number {0}.", taskNumber);
         return null;
      }

      #endregion

      #region Serializing

      public byte[] GetBytes()
      {
         //TODO: also could create these if they're not set, and log warning.
         //if (m_class == null || m_id == null || m_taskNumber == null || m_errorCode == null)
         //{
         //   s_log.ErrorFormat("Required header object is missing.");
         //   return null;
         //}

         List<byte> byteList = new List<byte>();
         byteList.AddRange(m_class.GetBytes());          // Transaction class
         byteList.AddRange(Id.GetBytes());             // Transaction ID
         byteList.AddRange(m_taskNumber.GetBytes());     // Task number
         byteList.AddRange(m_errorCode.GetBytes());      // Error code

         // Construct the data block in a separate list since we need to prefix the count.
         List<byte> dataBlock = new List<byte>();
         dataBlock.AddRange(new Short(m_objectList.Count).GetBytes());
         foreach (HotlineObject obj in m_objectList)
            dataBlock.AddRange(obj.GetBytes());

         // Calculate the total length of the data block.
         Long dataBlockLength = new Long(dataBlock.Count);
         byte[] lengthBytes = dataBlockLength.GetBytes();
         
         // Write the final length to the byte list twice.
         byteList.AddRange(lengthBytes);
         byteList.AddRange(lengthBytes);
         
         // Now include the data block in the final output list.
         byteList.AddRange(dataBlock);

         // Convert the final list to an array.
         return byteList.ToArray();
      }

      #endregion

      #region Deserializing

      public static Status TryParse(ref List<byte> potentialTransaction,
                                    out Transaction outTransaction,
                                    out Transaction replyToTransaction)
      {
         outTransaction = null;
         replyToTransaction = null;

         s_log.InfoFormat("Parsing Transaction from {0} bytes.", potentialTransaction.Count);
         DataUtils.DumpBytes("Parsing received bytes:", potentialTransaction);

         byte[] transArray = potentialTransaction.ToArray();
         int arrayIndex = 0;

         try
         {
            // Parse bytes enough to run the appropriate creator,
            // followed by a generic data object parser for the details.
            // Then a factory should be able to make the right transaction.
            int transactionClass = DataUtils.ReadShort(transArray, ref arrayIndex);
            bool reply = (transactionClass != 0); // If class is "0", this is not a reply.

            int transactionId = DataUtils.ReadShort(transArray, ref arrayIndex);
            int taskNumber = DataUtils.ReadLong(transArray, ref arrayIndex);
            int errorCode = DataUtils.ReadLong(transArray, ref arrayIndex);
            int dataLength1 = DataUtils.ReadLong(transArray, ref arrayIndex); // Total transaction size
            int dataLength2 = DataUtils.ReadLong(transArray, ref arrayIndex); // Size of this part

            s_log.InfoFormat("Parsed header for Transaction ID {0}", transactionId);

            if (errorCode != 0)
               s_log.ErrorFormat("Error code: {0}", errorCode);

            if (dataLength1 != dataLength2)
            {
               s_log.ErrorFormat("Multi-part transactions not supported! ({0},{1})", dataLength1, dataLength2);
               return Status.Failure;
            }

            // Read the exact amount of bytes specified by the header.
            byte[] transactionData = DataUtils.ReadLength(transArray, ref arrayIndex, dataLength1);

            // Shuttle off the Transaction data to the factory for more specific parsing.
            outTransaction = TransactionFactory.Create(reply, transactionId, taskNumber, errorCode,
                                                       transactionData, out replyToTransaction);

            // Any additional data in the array needs to be left in the original list.
            potentialTransaction = DataUtils.CopyRemainder(transArray, ref arrayIndex);

            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception parsing Transaction: {0}", e.Message);
            return Status.Failure;
         }
      }

      public void ParseObjects(byte[] transactionData)
      {
         // The transaction data starts with a Short indicating how many objects there are.
         int index = 0;
         int objectCount = DataUtils.ReadShort(transactionData, ref index);

         // Parse that many objects.
         for (int i = 0; i < objectCount; i++)
         {
            // First read the ID and length as shorts.
            int objectId = DataUtils.ReadShort(transactionData, ref index);
            int length = DataUtils.ReadShort(transactionData, ref index);

            // Copy the object data out of the source so it can be parsed cleanly.
            byte[] objectData = DataUtils.ReadLength(transactionData, ref index, length);

            // Have the ObjectFactory create the object.
            HotlineObject parsedObject = ObjectFactory.Create(objectId, objectData);

            // Add it to the Transaction if it is not null.
            if (parsedObject != null)
               m_objectList.Add(parsedObject);

            // Call this Transaction's overridden process object list method.
            ProcessObjectList();
         }
      }

      #endregion

      #region Virtual Methods

      protected virtual void ProcessObjectList()
      {
         s_log.WarnFormat("ProcessObjectList called from base class.");
      }

      public virtual object[] GetDelegateArgs()
      {
         s_log.WarnFormat("GetDelegateArgs called from base class.");
         return null;
      }

      #endregion
   }
}

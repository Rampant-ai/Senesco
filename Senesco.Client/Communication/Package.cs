using System;
using System.Collections.Generic;
using log4net;
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Communication
{
   class Package
   {
      #region Fields and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(Package));

      private List<byte> m_byteList = new List<byte>();
      private byte[] m_bytes = null;

      public Package()
      { }

      public Package(params object[] args)
      {
         if (args == null || args.Length == 0)
            throw new Exception("No arguments to Package constructor");

         ProcessObjectArray(args);
      }

      #endregion

      #region Encoding helpers

      private void ProcessObjectArray(object[] args)
      {
         foreach (object o in args)
         {
            Type type = o.GetType();
            if (type == typeof(string))
            {
               DataUtils.AddString(o as string, m_byteList, false, true);
            }
            else if (type == typeof(RawString))
            {
               // RawString does not have a byte count prefixed.
               DataUtils.AddString(((RawString)o).Value, m_byteList, false, false);
            }
            else if (type == typeof(EncodedString))
            {
               // Encoded strings are encoded, with a byte count prefix.
               DataUtils.AddString(((EncodedString)o).Value, m_byteList, true, true);
            }
            else if (type == typeof(int))
            {
               DataUtils.AddIntegerRaw((int)o, m_byteList);
            }
            else
            {
               s_log.ErrorFormat("Unexpected object type in Package: {0}", type.ToString());
            }
         }
      }

      public void AddIntegerRaw(int i)
      {
         DataUtils.AddIntegerRaw(i, m_byteList);
      }

      public void AddShort(int i)
      {
         DataUtils.AddShort(i, m_byteList);
      }

      public void AddLong(int i)
      {
         DataUtils.AddLong(i, m_byteList);
      }

      #endregion

      #region Retrieval for use

      public byte[] GetBytes()
      {
         if (m_byteList == null || m_byteList.Count == 0)
         {
            s_log.ErrorFormat("No data to send!");
            return null;
         }

         // Calculate byte array if never done so.
         // This way the work isn't done twice in case we call this twice.
         if (m_bytes == null)
            m_bytes = m_byteList.ToArray();

         return m_bytes;
      }

      public int GetSize()
      {
         if (m_bytes == null)
            return m_bytes.Length;
         return -1;
      }

      #endregion

      #region Incoming Bytes

      public void ReceivedBytes(List<byte> receivedBytes)
      {
         m_byteList = receivedBytes;
      }

      #endregion

      #region Operators

      /// <summary>
      /// Checks that both sides must represent the same bytes in the same order.
      /// </summary>
      public static bool operator ==(Package left, Package right)
      {
         // If both null, they're equal.
         if ((object)left == null && (object)right == null)
            return true;

         // If either is null (but not both), they're not equal.
         if ((object)left == null || (object)right == null)
            return false;

         byte[] leftBytes = left.GetBytes();
         byte[] rightBytes = right.GetBytes();

         // If the arrays are a different length, give up now.
         if (leftBytes.Length != rightBytes.Length)
            return false;

         // Check that every byte is the same, in order.
         for (int i = 0; i < leftBytes.Length; i++)
         {
            if (leftBytes[i] != rightBytes[i])
               return false;
         }

         // All bytes were equal!
         return true;
      }

      public static bool operator !=(Package left, Package right)
      {
         return !(left == right);
      }

      public override bool Equals(object o)
      {
         if (o is Package)
            return (this == (o as Package));
         return false;
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      #endregion
   }
}

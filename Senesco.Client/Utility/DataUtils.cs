using System.Collections;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace Senesco.Client.Utility
{
   class DataUtils
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(DataUtils));

      #region Strings

      //public static void AddString(string s, List<byte> byteList)
      //{
      //   AddString(s, byteList, false, true);
      //}

      public static void AddString(string s, List<byte> byteList, bool encode, bool prefixByteCount)
      {
         if (s == null)
            return;

         byte[] encodedBytes = StringToBytes(s, encode);

         // Prefix the length.
         if (prefixByteCount == true)
            DataUtils.AddShort(encodedBytes.Length, byteList);

         // Add the string bytes.
         byteList.AddRange(encodedBytes);
      }

      private static byte[] StringToBytes(string s, bool encode)
      {
         if (s == null)
            return null;

         // Convert string to bytes.
         char[] chars = s.ToCharArray();
         byte[] bytes = new byte[chars.Length];

         // Some dirt-simple encoding of characters...
         if (encode)
         {
            EncodedString(chars, bytes);
            // No additional character encoding for encoded strings, since we just
            // effectively made our own encoding!
            return bytes;
         }
         else
         {
            RegularString(chars, bytes);
            //bytes = Encoding.Convert(Encoding.ASCII, Encoding.ASCII, bytes);
            return bytes;
         }
      }

      private static string BytesToString(byte[] bytes, ref int index, int count, bool encoded)
      {
         if (bytes == null)
            return null;

         StringBuilder sb = new StringBuilder();
         if (encoded == false)
         {
            //byte[] bytes = Encoding.Convert(Encoding.ASCII, Encoding.ASCII, bytes);
            for (int i = 0; i < count; i++)
               sb.Append((char)bytes[index++]);
         }
         else
         {
            for (int i = 0; i < count; i++)
               sb.Append((char)(0xFF - bytes[index++]));
         }

         return sb.ToString();
      }

      private static void RegularString(char[] chars, byte[] bytes)
      {
         int i = 0;
         foreach (char c in chars)
            bytes[i++] = (byte)c;
      }

      private static void EncodedString(char[] chars, byte[] bytes)
      {
         int i = 0;
         foreach (char c in chars)
            bytes[i++] = (byte)(0xFF - c);
      }

      #endregion Strings

      #region Integers

      /// <summary>
      /// Adds the given integer to the given byte list, represented by a short if the
      /// integer is small or a long if the integer is large or negative.
      /// </summary>
      public static void AddNumber(int i, List<byte> byteList, bool prefixByteCount)
      {
         if (0 <= i && i <= 65535)
         {
            if (prefixByteCount)
               AddShort(2, byteList);
            AddShort(i, byteList);
         }
         else
         {
            if (prefixByteCount)
               AddShort(4, byteList);
            AddLong(i, byteList);
         }
      }

      /// <summary>
      /// Adds the given integer to the given byte list, represented by one byte.
      /// </summary>
      public static void AddIntegerRaw(int i, List<byte> byteList)
      {
         DataUtils.AddIntegerToByteList(i, byteList, 1);
      }

      /// <summary>
      /// Adds the given integer to the given byte list, represented by two bytes.
      /// </summary>
      public static void AddShort(int i, List<byte> byteList)
      {
         DataUtils.AddIntegerToByteList(i, byteList, 2);
      }

      /// <summary>
      /// Adds the given integer to the given byte list, represented by four bytes.
      /// </summary>
      public static void AddLong(int i, List<byte> byteList)
      {
         DataUtils.AddIntegerToByteList(i, byteList, 4);
      }

      /// <summary>
      /// Adds the given integer to the given byte list, represented by a specific number of bytes.
      /// </summary>
      /// <param name="i">The integer to add.</param>
      /// <param name="byteList">The list of bytes to which to add.</param>
      /// <param name="byteCount">The number of bytes to represent the integer.</param>
      private static void AddIntegerToByteList(int i, List<byte> byteList, int byteCount)
      {
         Stack<byte> byteStack = new Stack<byte>(byteCount);
         for (int j = 0; j < byteCount; j++)
         {
            // Mask to get the bottom byte and add it to the stack.
            byteStack.Push((byte)(i & 0xFF));
            // Downshift by one byte for the next loop.
            i >>= 8;
         }

         // Add stack contents to output list. This effectively reverses the byte order.
         byteList.AddRange(byteStack);
      }

      #endregion Integers

      #region Misc Utility

      public static void DumpBytes(string message, IEnumerable list)
      {
         // Skip all this if debug logging is not on.
         if (s_log.IsDebugEnabled == false)
            return;

         if (string.IsNullOrEmpty(message) == false)
            s_log.Debug(message);

         StringBuilder sb = new StringBuilder();
         int count = 0;
         int width = 8;  // FFFF (four character chunks)
         int chunks = 2; // FFFF FFFF (two chunks per line)
         int total = width * chunks;

         foreach (byte b in list)
         {
            sb.AppendFormat("{0:X2}", b);
            count++;
            if (count % width == 0)
               sb.Append(" ");

            // If we've filled a line, print it and reset everything.
            if (count == total)
            {
               s_log.DebugFormat("{0}", sb.ToString());
               sb = new StringBuilder();
               count = 0;
            }
         }

         // Print any remainder.
         if (count > 0)
            s_log.DebugFormat("{0}", sb.ToString());
      }

      public static int TotalBytes(List<byte[]> byteArrays)
      {
         if (byteArrays == null)
            return 0;
         int sum = 0;
         foreach (byte[] array in byteArrays)
            sum += array.Length;
         return sum;
      }

      public static List<byte> CopyRemainder(byte[] transArray, ref int arrayIndex)
      {
         List<byte> potentialTransaction = new List<byte>();
         if (arrayIndex < transArray.Length)
         {
            s_log.DebugFormat("{0} bytes remain in the buffer after parsing Transaction.", transArray.Length - arrayIndex);

            while (arrayIndex < transArray.Length)
               potentialTransaction.Add(transArray[arrayIndex++]);
         }
         return potentialTransaction;
      }

      #endregion

      #region Byte-stream reading

      // Note: These methods do no array bounds checking so that an exception is thrown
      // if the transaction is incomplete.  This is on purpose.

      internal static byte[] ReadLength(byte[] transArray, ref int arrayIndex, int count)
      {
         // The result array will be of this known size.
         byte[] result = new byte[count];
         
         // Copy the relevant part of the array.
         for (int i = 0; i < count; i++)
            result[i] = transArray[arrayIndex++];
         
         // Return the finished array.
         return result;
      }

      internal static int ReadShort(byte[] bytes, ref int index)
      {
         //TODO: parse negative values
         return ReadIntBytes(bytes, ref index, 2);
      }

      internal static int ReadLong(byte[] bytes, ref int index)
      {
         //TODO: parse negative values
         return ReadIntBytes(bytes, ref index, 4);
      }

      internal static int ReadNumber(byte[] bytes, ref int index)
      {
         //TODO: we can only do this if we assume the whole buffer is the Number!
         switch (bytes.Length)
         {
            case 4:
               return ReadLong(bytes, ref index);
            case 2:
               return ReadShort(bytes, ref index);
            default:
               s_log.ErrorFormat("Bad array size in DataUtils.ReadNumber!  Fix this TODO!!");
               return 0;
         }
      }

      private static int ReadIntBytes(byte[] bytes, ref int index, int byteCount)
      {
         int result = 0;
         bool first = true;
         for (int i = 0; i < byteCount; i++)
         {
            // Upshift by one byte, except the first time through.
            if (first)
               first = false;
            else
               result <<= 8;

            result += bytes[index++];
         }
         return result;
      }

      internal static string ReadString(byte[] objectData, ref int index, int count)
      {
         // Read only the specified number of bytes.
         return BytesToString(objectData, ref index, count, false);
      }

      internal static string ReadEncodedString(byte[] objectData, ref int index, int count)
      {
         // Read only the specified number of bytes.
         return BytesToString(objectData, ref index, count, true);
      }

      #endregion
   }
}

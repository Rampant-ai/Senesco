using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects.ObjectData
{
   /// <summary>
   /// The RawString class does not include a character count prefix when it
   /// is converted into bytes for transmitting through the socket.  This class
   /// is only used in certain special cases where the count is not required.
   /// </summary>
   class RawString : IHotlineObjectData
   {
      public string Value;

      public RawString(string s)
      {
         Value = s;
      }

      public byte[] GetBytes()
      {
         List<byte> bytes = new List<byte>();
         DataUtils.AddString(Value, bytes, false, false);
         return bytes.ToArray();
      }
   }
}

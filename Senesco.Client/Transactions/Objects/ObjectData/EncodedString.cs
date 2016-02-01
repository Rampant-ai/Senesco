using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects.ObjectData
{
   class EncodedString : IHotlineObjectData
   {
      public string Value;

      public EncodedString(string s)
      {
         Value = s;
      }

      public byte[] GetBytes()
      {
         List<byte> bytes = new List<byte>();
         DataUtils.AddString(Value, bytes, true, false);
         return bytes.ToArray();
      }
   }
}

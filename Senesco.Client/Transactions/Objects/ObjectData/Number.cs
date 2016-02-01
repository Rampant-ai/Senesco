using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects.ObjectData
{
   class Number : IHotlineObjectData
   {
      public int Value;

      public Number(int i)
      {
         Value = i;
      }

      public byte[] GetBytes()
      {
         List<byte> bytes = new List<byte>();
         DataUtils.AddNumber(Value, bytes, false);
         return bytes.ToArray();
      }
   }
}

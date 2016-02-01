using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects.ObjectData
{
   public class Short : IHotlineObjectData
   {
      public int Value;

      public Short(int i)
      {
         Value = i;
      }

      public byte[] GetBytes()
      {
         List<byte> bytes = new List<byte>();
         DataUtils.AddShort(Value, bytes);
         return bytes.ToArray();
      }
   }
}

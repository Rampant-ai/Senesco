using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects.ObjectData
{
   public class Long : IHotlineObjectData
   {
      public int Value;

      public Long(int i)
      {
         Value = i;
      }

      public byte[] GetBytes()
      {
         List<byte> bytes = new List<byte>();
         DataUtils.AddLong(Value, bytes);
         return bytes.ToArray();
      }
   }
}

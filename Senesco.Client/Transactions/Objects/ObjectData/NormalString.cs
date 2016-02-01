using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects.ObjectData
{
   /// <summary>
   /// The NormalString class is a normal string.  When encoded, it has a prefixed
   /// Short indicating the number of characters.  This is the typical string encoding
   /// that is used most of the time.
   /// </summary>
   public class NormalString : IHotlineObjectData
   {
      public string Value;

      public NormalString(string s)
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

using System;
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class UserOptions : HotlineObject
   {
      public Number Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public UserOptions()
      { }

      public UserOptions(bool ignorePrivateMsgs, bool ignorePrivateChat, string ignoreAutoResponse)
      {
         int bits = 0;

         if (ignorePrivateMsgs)
            bits |= 0x01;
         if (ignorePrivateChat)
            bits |= 0x02;
         if (String.IsNullOrEmpty(ignoreAutoResponse) == false)
            bits |= 0x04;

         Value = new Number(bits);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         int bits = DataUtils.ReadShort(objectData, ref index);

         // This should have some helpers to read the individual bits, but
         // really only the server needs to read the bits from this object.
         Value = new Number(bits);
         this.ObjectDataList.Add(Value);
      }
   }
}

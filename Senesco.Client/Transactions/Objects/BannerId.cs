
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class BannerId : HotlineObject
   {
      public Number Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public BannerId()
      { }

      public BannerId(int icon)
      {
         Value = new Number(icon);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         int icon = DataUtils.ReadNumber(objectData, ref index);

         Value = new Number(icon);
         this.ObjectDataList.Add(Value);
      }
   }
}

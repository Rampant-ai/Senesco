
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class UserId : HotlineObject
   {
      public Number Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public UserId()
      { }

      public UserId(int userSocket)
      {
         Value = new Number(userSocket);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         int userSocket = DataUtils.ReadNumber(objectData, ref index);

         Value = new Number(userSocket);
         this.ObjectDataList.Add(Value);
      }
   }
}

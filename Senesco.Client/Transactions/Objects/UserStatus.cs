
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class UserStatus : HotlineObject
   {
      public Number Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public UserStatus()
      { }

      public UserStatus(int userStatus)
      {
         Value = new Number(userStatus);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         int userStatus = DataUtils.ReadNumber(objectData, ref index);

         Value = new Number(userStatus);
         this.ObjectDataList.Add(Value);
      }
   }
}

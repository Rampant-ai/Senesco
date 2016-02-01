
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class UserName : HotlineObject
   {
      public EncodedString Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public UserName()
      { }

      public UserName(string username)
      {
         Value = new EncodedString(username);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         string username = DataUtils.ReadEncodedString(objectData, ref index, objectData.Length);

         Value = new EncodedString(username);
         this.ObjectDataList.Add(Value);
      }
   }
}

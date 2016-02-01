
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class Password : HotlineObject
   {
      public EncodedString Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public Password()
      { }

      public Password(string password)
      {
         Value = new EncodedString(password);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         string password = DataUtils.ReadEncodedString(objectData, ref index, objectData.Length);

         Value = new EncodedString(password);
         this.ObjectDataList.Add(Value);
      }
   }
}

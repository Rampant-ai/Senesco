
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class ErrorMessage : HotlineObject
   {
      public NormalString Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public ErrorMessage()
      { }

      public ErrorMessage(string message)
      {
         Value = new NormalString(message);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         string message = DataUtils.ReadString(objectData, ref index, objectData.Length);

         Value = new NormalString(message);
         this.ObjectDataList.Add(Value);
      }
   }
}

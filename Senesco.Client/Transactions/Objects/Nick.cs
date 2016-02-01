
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class Nick : HotlineObject
   {
      public NormalString Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public Nick()
      { }

      public Nick(string nick)
      {
         Value = new NormalString(nick);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         string nick = DataUtils.ReadString(objectData, ref index, objectData.Length);

         Value = new NormalString(nick);
         this.ObjectDataList.Add(Value);
      }
   }
}

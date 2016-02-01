
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class ChatWindow : HotlineObject
   {
      public Long Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public ChatWindow()
      { }

      public ChatWindow(int chatWindow)
      {
         Value = new Long(chatWindow);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         int chatWindow = DataUtils.ReadLong(objectData, ref index);

         Value = new Long(chatWindow);
         this.ObjectDataList.Add(Value);
      }
   }
}

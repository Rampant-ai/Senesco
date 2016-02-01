
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   class Parameter : HotlineObject
   {
      public Number Value;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public Parameter()
      { }

      public Parameter(int parameter)
      {
         Value = new Number(parameter);
         this.ObjectDataList.Add(Value);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;
         int parameter = DataUtils.ReadNumber(objectData, ref index);

         Value = new Number(parameter);
         this.ObjectDataList.Add(Value);
      }
   }
}

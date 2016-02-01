using System.Collections.Generic;
using log4net;
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   /// <summary>
   /// This is the base class for Hotline Objects, which are generally composed
   /// of a specific collection of IHotlineObjectData objects.
   /// </summary>
   public class HotlineObject
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(HotlineObject));

      private Short m_id = null;
      public List<IHotlineObjectData> ObjectDataList = new List<IHotlineObjectData>();

      public HotlineObject()
      {
         int id = ObjectFactory.GetIdByType(this.GetType());
         m_id = new Short(id);
      }

      public byte[] GetBytes()
      {
         if (m_id == null)
         {
            s_log.ErrorFormat("Object has null Object ID! ({0})", this.GetType().ToString());
            return null;
         }

         // First get the byte array for each object.
         // We don't mash them together just yet so we can calculate the overall length first.
         List<byte[]> byteArrays = new List<byte[]>();
         if (ObjectDataList != null)
         {
            foreach (IHotlineObjectData objData in ObjectDataList)
               byteArrays.Add(objData.GetBytes());
         }

         // Now smash all the objects together in the correct order.
         List<byte> byteList = new List<byte>();

         // First add the ObjectId as a Short.
         byteList.AddRange(m_id.GetBytes());

         // Next add the object data length as a Short.
         Short Length = new Short(DataUtils.TotalBytes(byteArrays));
         byteList.AddRange(Length.GetBytes());

         // Finally, the object data itself.
         foreach (byte[] byteArray in byteArrays)
            byteList.AddRange(byteArray);

         // Return the final list as an array.
         return byteList.ToArray();
      }

      internal virtual void ParseBytes(byte[] objectData)
      {
         s_log.WarnFormat("Base class attempting to parse byte array of size {0}.",
                          (objectData == null) ? -1 : objectData.Length);
      }
   }
}

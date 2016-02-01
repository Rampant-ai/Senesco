using System;
using System.Collections.Generic;
using log4net;

namespace Senesco.Client.Transactions.Objects
{
   class ObjectFactory
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(ObjectFactory));

      #region Registration

      private static bool s_initialized = false;
      private static object s_initSync = new object();
      private static Dictionary<int, Type> s_idMap = new Dictionary<int, Type>();
      private static Dictionary<Type, int> s_typeMap = new Dictionary<Type, int>();

      private static void Initialize()
      {
         s_idMap[0] = typeof(HotlineObject);
         s_idMap[100] = typeof(ErrorMessage);
         s_idMap[101] = typeof(Message);
         s_idMap[102] = typeof(Nick);
         s_idMap[103] = typeof(UserId);
         s_idMap[104] = typeof(Icon);
         s_idMap[105] = typeof(UserName);
         s_idMap[106] = typeof(Password);
         s_idMap[109] = typeof(Parameter);
         s_idMap[112] = typeof(UserStatus);
         s_idMap[113] = typeof(UserOptions);
         s_idMap[114] = typeof(ChatWindow);
         s_idMap[115] = typeof(Subject);
         s_idMap[160] = typeof(Version);
         s_idMap[161] = typeof(BannerId);
         s_idMap[162] = typeof(ServerName);
         s_idMap[215] = typeof(AutoResponse);
         s_idMap[300] = typeof(UserListEntry);

         // The other map is the exact opposite, which we can generate
         // with this simple loop.
         foreach (KeyValuePair<int, Type> kvp in s_idMap)
            s_typeMap[kvp.Value] = kvp.Key;

         s_initialized = true;
      }

      internal static int GetIdByType(Type type)
      {
         // Make sure multiple threads don't initialize simultaneously.
         lock (s_initSync)
         {
            if (s_initialized == false)
               Initialize();
         }

         return s_typeMap[type];
      }

      #endregion

      internal static HotlineObject Create(int objectId, byte[] objectData)
      {
         try
         {
            // Figure out what type is associated with the given ID number.
            Type objType;
            if (s_idMap.TryGetValue(objectId, out objType) == false)
            {
               s_log.InfoFormat("Unknown Object ID {0}", objectId);
               objType = typeof(HotlineObject);
            }

            // Create an instance of that Type.
            HotlineObject hotlineObject = (HotlineObject)Activator.CreateInstance(objType);

            // Have the object parse its own details from the byte array.
            hotlineObject.ParseBytes(objectData);

            // Return the finished object.
            return hotlineObject;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception generating object ID {0}: {1}", objectId, e.Message);
            return null;
         }
      }

      /// <summary>
      /// Finds an item by type in the given list.  The first matching item is
      /// the one returned.  Use this when only one object of that type is expected.
      /// This lookup is a linear search.
      /// </summary>
      /// <param name="objectEnumerable">The list to search.</param>
      /// <param name="type">The Type of the object to find.</param>
      /// <returns>The first found object with the correct Type, or null.</returns>
      public static HotlineObject FindObject(IEnumerable<HotlineObject> objectEnumerable, Type type)
      {
         if (objectEnumerable == null)
            return null;

         foreach (HotlineObject obj in objectEnumerable)
         {
            if (obj.GetType() == type)
               return obj;
         }
         return null;
      }

      /// <summary>
      /// Finds items by type in the given list.  All matching items are returned.
      /// This lookup is a linear search.
      /// </summary>
      /// <param name="objectEnumerable">The list to search.</param>
      /// <param name="type">The Type of the objects to find.</param>
      /// <returns>The complete list of objects matching the given type, or an empty list.</returns>
      public static List<HotlineObject> FindObjects(IEnumerable<HotlineObject> objectEnumerable, Type type)
      {
         List<HotlineObject> returnList = new List<HotlineObject>();

         if (objectEnumerable == null)
            return returnList;

         foreach (HotlineObject obj in objectEnumerable)
         {
            if (obj.GetType() == type)
               returnList.Add(obj);
         }
         return returnList;
      }
   }
}

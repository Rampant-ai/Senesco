using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class GetUserInfo : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(GetUserInfo));

      private UserId m_userId;

      /// <summary>
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public GetUserInfo()
         : base("GetUserInfo")
      {
      }

      public GetUserInfo(int userId)
         : base("GetUserInfo", false, 0)
      {
         m_userId = new UserId(userId);
         m_objectList.Add(m_userId);
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            if (obj.GetType() == typeof(UserId))
               m_userId = obj as UserId;
            else
               s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

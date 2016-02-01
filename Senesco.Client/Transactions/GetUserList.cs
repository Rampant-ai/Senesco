using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Transactions
{
   class GetUserList : Transaction
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(GetUserList));

      /// <summary>
      /// DO NOT USE THIS OVERLOAD FOR INITIATING THIS TRANSACTION.
      /// Default creator for the Activator to use in the TransactionFactory.
      /// </summary>
      public GetUserList()
         : base("GetUserList")
      {
      }

      /// <summary>
      /// Always use THIS constructor to initiate this transaction type.
      /// The "alwaysNull" parameter is provided to differentiate it from the
      /// parameterless constructor used by the Activator.
      /// </summary>
      /// <param name="alwaysNull">Ignored. Just pass null.</param>
      public GetUserList(object ignored)
         : base("GetUserList", false, 0)
      {
      }

      protected override void ProcessObjectList()
      {
         foreach (HotlineObject obj in m_objectList)
         {
            s_log.ErrorFormat("Unexpected object: {0}", obj.GetType().ToString());
         }
      }
   }
}

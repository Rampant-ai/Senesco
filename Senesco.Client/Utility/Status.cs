
namespace Senesco.Client.Utility
{
   /// <summary>
   /// This class was originally just an enum, but I wrapped up the class as
   /// cleverly as I could to make it behave like an enum, but with an optional
   /// string message that can be included when needed.
   /// 
   /// Always use the public static methods to create these objects, and test
   /// their success/failure properties using the given public static readonly
   /// objects.  Lastly, the "message" field can be used by using the static
   /// method overloads (for Set) and the public Message property (for Get).
   /// </summary>
   public class Status
   {
      #region Public Static Class Interface

      public readonly static Status Failure = new Status(Result.Failure);
      public readonly static Status NoResult = new Status(Result.NoResult);
      public readonly static Status Success = new Status(Result.Success);

      public static Status GetSuccess()
      {
         return new Status(Result.Success);
      }

      public static Status GetSuccess(string message)
      {
         return new Status(Result.Success, message);
      }

      public static Status GetNoResult()
      {
         return new Status(Result.NoResult);
      }

      public static Status GetNoResult(string message)
      {
         return new Status(Result.NoResult, message);
      }

      public static Status GetFailure()
      {
         return new Status(Result.Failure);
      }

      public static Status GetFailure(string message)
      {
         return new Status(Result.Failure, message);
      }

      #endregion

      #region Members and Properties

      private enum Result
      {
         Failure = -1,
         NoResult = 0,
         Success = 1,
      }

      Result m_result = Result.NoResult;
      string m_message = null;

      public string Message
      {
         get { return m_message; }
      }

      #endregion

      #region Creator

      private Status(Result result)
      {
         m_result = result;
      }

      private Status(Result result, string message)
      {
         m_result = result;
         m_message = message;
      }

      #endregion

      #region ToString override

      public override string ToString()
      {
         return m_result.ToString();
      }

      #endregion

      #region Equals operators

      public static bool operator ==(Status left, Status right)
      {
         // Null equals null.
         if (null == (object)left || null == (object)right)
            return true;

         // Any instance does not equal null.
         if (null == (object)left || null == (object)right)
            return false;

         // Two instances are equal only if their results are equal.
         return (left.m_result == right.m_result);
      }

      public static bool operator !=(Status left, Status right)
      {
         return !(left == right);
      }

      public override bool Equals(object obj)
      {
         return (this == (obj as Status));
      }

      public override int GetHashCode()
      {
         return (int)m_result;
      }

      #endregion
   }
}

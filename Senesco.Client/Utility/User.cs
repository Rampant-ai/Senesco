using System;
using log4net;
using Senesco.Client.Transactions.Objects;

namespace Senesco.Client.Utility
{
   public class User : ICloneable
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(User));

      public string Username;
      public int UserId; // Set by the server
      public int IconId;
      public int Flags;  // Set by the server: Admin flag, Idle flag...
      public bool IgnorePrivateMsgs;
      public bool IgnorePrivateChat;
      public string IgnoreAutoResponse;

      public User()
      {
      }

      public User(UserListEntry ule)
      {
         string user = String.Format("({0}:{1}:{2})", ule.Nick.Value, ule.Icon.Value, ule.Socket.Value);
         s_log.Info(user);

         Username = ule.Nick.Value;
         UserId = ule.Socket.Value;
         IconId = ule.Icon.Value;
         Flags = ule.Status.Value; // 2 = admin, 1 = idle
      }

      public object Clone()
      {
         User user = new User();

         // Clone the client fields.
         user.Username = this.Username;
         user.IconId = this.IconId;
         user.IgnorePrivateMsgs = this.IgnorePrivateMsgs;
         user.IgnorePrivateChat = this.IgnorePrivateChat;
         user.IgnoreAutoResponse = this.IgnoreAutoResponse;

         // Clone the server fields too.
         user.UserId = this.UserId;
         user.Flags = this.Flags;

         return user;
      }

      public override string ToString()
      {
         if (String.IsNullOrEmpty(Username))
            return String.Format("{0} ({1}:{2}:{3})", "unnamed", UserId, IconId, Flags);
         else
            return String.Format("{0} ({1}:{2}:{3})", Username, UserId, IconId, Flags);
      }

      /// <summary>
      /// Checks if all values in the User are valid.
      /// </summary>
      /// <returns>True if all values are valid.</returns>
      public bool IsValid(out string error)
      {
         error = null;

         // Non-empty name.
         if (String.IsNullOrEmpty(Username))
         {
            error = "Nick is empty";
            return false;
         }

         // Icon between 0-32767 inclusive.
         if (IconId < 0 || IconId > 32767)
         {
            error = "Icon must be in the range [0,32767]";
            return false;
         }

         return true;
      }

      /// <summary>
      /// Overload without the error string out parameter.
      /// </summary>
      /// <returns>True if all values are valid.</returns>
      public bool IsValid()
      {
         string error;
         return IsValid(out error);
      }

      /// <summary>
      /// The equality operator to determine if two User objects represent the same connected user.
      /// </summary>
      public static bool operator ==(User left, User right)
      {
         // If both null, then they are equal.
         // Cast as object so we don't stack overflow.
         if ((left as object) == null && (right as object) == null)
            return true;

         // If either is null, then they are not equal.
         // Cast as object so we don't stack overflow.
         if ((left as object) == null || (right as object) == null)
            return false;

         // Determine equality strictly by User ID.
         return (left.UserId == right.UserId);
      }

      public static bool operator !=(User left, User right)
      {
         return !(left == right);
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         return (this == (obj as User));
      }

      /// <summary>
      /// Checks if the user settings are different between the current instance
      /// and the given User instance.
      /// </summary>
      /// <param name="user">The User to compare with.</param>
      /// <returns>True if the Users are different.</returns>
      public bool SettingsDiffer(User user)
      {
         // Only check the fields that are configured by the user.
         if (this.Username == user.Username &&
             this.IconId == user.IconId &&
             this.IgnoreAutoResponse == user.IgnoreAutoResponse &&
             this.IgnorePrivateChat == user.IgnorePrivateChat &&
             this.IgnorePrivateMsgs == user.IgnorePrivateMsgs)
         {
            return false;
         }
         else
         {
            return true;
         }
      }
   }
}

using System;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace Senesco.Client.Utility
{
   class UserList
   {
      #region Fields and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(UserList));

      private Dictionary<int, User> m_userLookup = new Dictionary<int, User>();

      public List<User> Users
      {
         get
         {
            s_log.Debug("UserList: Getting Users property");
            lock (m_userLookup)
            {
               return new List<User>(m_userLookup.Values);
            }
         }
      }

      public UserList(List<User> userList)
      {
         if (userList == null)
            return;

         lock (m_userLookup)
         {
            foreach (User user in userList)
               m_userLookup.Add(user.UserId, user);
         }
      }

      #endregion

      #region Add User

      public Status AddUsers(IEnumerable<User> usersToAdd)
      {
         if (usersToAdd == null)
            return Status.NoResult;

         // Process the whole list, but keep track of if any failed.
         bool fail = false;
         foreach (User user in usersToAdd)
         {
            if (AddUser(user) == Status.Failure)
               fail = true;
         }

         if (fail)
            return Status.Failure;
         else
            return Status.Success;
      }

      public Status AddUser(User user)
      {
         if (user == null)
         {
            s_log.ErrorFormat("UserList.AddUser(): User is null.");
            return Status.Failure;
         }

         try
         {
            lock (m_userLookup)
            {
               m_userLookup.Add(user.UserId, user);
            }
            return Status.Success;
         }
         catch (ArgumentException e)
         {
            s_log.ErrorFormat("UserID {0} already in UserLst ({1})", user.UserId, e.Message);
            lock (m_userLookup)
            {
               m_userLookup[user.UserId] = user;
            }
            return Status.NoResult;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception adding User {0}: {1}", user.ToString(), e.Message);
            return Status.Failure;
         }
      }

      #endregion

      #region Remove User

      public Status RemoveUsers(IEnumerable<User> usersToRemove)
      {
         if (usersToRemove == null)
            return Status.NoResult;

         // Process the whole list, but keep track of if any failed.
         bool fail = false;
         foreach (User user in usersToRemove)
         {
            if (RemoveUser(user) == Status.Failure)
               fail = true;
         }

         if (fail)
            return Status.Failure;
         else
            return Status.Success;
      }

      public Status RemoveUser(User user)
      {
         if (user == null)
         {
            s_log.ErrorFormat("UserList.RemoveUser(): User is null.");
            return Status.Failure;
         }

         lock (m_userLookup)
         {
            if (m_userLookup.Remove(user.UserId) == false)
            {
               s_log.ErrorFormat("UserList.RemoveUser(): User not found.");
               return Status.Failure;
            }
         }

         return Status.Success;
      }

      #endregion

      #region Message Formatting

      public string FormatUserChangeMsg(IEnumerable<User> userList)
      {
         if (userList == null)
            return String.Empty;

         // Start the message.
         StringBuilder sb = new StringBuilder();
         //sb.Append("Join/Update: ");

         // Loop over all users in the list (usually just one).
         bool first = true;
         foreach (User user in userList)
         {
            // Is this user already in the userlist?
            User existingUser;
            lock (m_userLookup)
            {
               m_userLookup.TryGetValue(user.UserId, out existingUser);
            }

            // If not, print a "Join" message.
            if (existingUser == null)
            {
               AddComma(sb, ref first);
               sb.Append("Join: ");
               sb.Append(user.ToString());
               continue; // No other changes possible.
            }

            // If the user is already present, figure out what else changed and cite that.

            // Did the user change username?
            if (String.Compare(existingUser.Username, user.Username) != 0)
            {
               AddComma(sb, ref first);
               sb.Append(existingUser.Username);
               sb.Append(" is now known as ");
               sb.Append(user.Username);
            }

            // Did the user change icon?
            if (existingUser.IconId != user.IconId)
            {
               AddComma(sb, ref first);
               sb.Append(existingUser.Username);
               sb.Append(" changed icon from ");
               sb.Append(existingUser.IconId);
               sb.Append(" to ");
               sb.Append(user.IconId);
            }

            // Did the user change flags (idle/admin)?
            if (existingUser.Flags != user.Flags)
            {
               // Calculate which flags changed via XOR.
               int changedFlags = existingUser.Flags ^ user.Flags;

               // Has the idle flag changed?
               if (UserFlags.IsIdleFlagSet(changedFlags))
               {
                  // If the flag is now on, the user became idle.
                  if (UserFlags.IsIdleFlagSet(user.Flags))
                  {
                     AddComma(sb, ref first);
                     sb.Append(existingUser.Username);
                     sb.Append(" is idle");
                  }
                  // Else user became unidle.
                  else
                  {
                     AddComma(sb, ref first);
                     sb.Append(existingUser.Username);
                     sb.Append(" is back");
                  }
               }

               // Has the admin flag changed?
               if (UserFlags.IsAdminFlagSet(changedFlags))
               {
                  // If the flag is now on, the user became an admin.
                  if (UserFlags.IsAdminFlagSet(user.Flags))
                  {
                     AddComma(sb, ref first);
                     sb.Append(existingUser.Username);
                     sb.Append(" is an admin");
                  }
                  // Else user became non-admin.
                  else
                  {
                     AddComma(sb, ref first);
                     sb.Append(existingUser.Username);
                     sb.Append(" is not an admin");
                  }
               }
            }
         }

         return sb.ToString();
      }

      /// <summary>
      /// Small helper method to either toggle off the "first item" flag, or
      /// suffix a comma if it's not the first.
      /// </summary>
      /// <param name="sb">The StringBuilder to suffix to</param>
      /// <param name="first">The "first item" flag</param>
      private static void AddComma(StringBuilder sb, ref bool first)
      {
         if (first == true)
            first = false;
         else
            sb.Append(", ");
      }

      public string FormatUserLeaveMsg(IEnumerable<User> removeList)
      {
         if (removeList == null)
            return String.Empty;

         // Start the message.
         StringBuilder sb = new StringBuilder();
         sb.Append("Part: ");

         // Loop over all users in the list (usually just one).
         bool first = true;
         foreach (User user in removeList)
         {
            if (user == null)
               continue;

            // This is generally called from a Leave message, which generally will
            // only provide users containing the UserId.

            // Get the User object to cite.
            User target = user;
            // If the input username is empty, try to use the one from the current userlist.
            if (String.IsNullOrEmpty(user.Username))
            {
               lock (m_userLookup)
               {
                  if (m_userLookup.TryGetValue(user.UserId, out target) == false)
                     target = user;
               }
            }

            // Cite the found user from the UserList or a placeholder fallback.
            AddComma(sb, ref first);
            if (target != null)
            {
               sb.Append(target.ToString());
            }
            else
            {
               sb.AppendFormat("no nick ({0})", user.UserId);
            }
         }

         return sb.ToString();
      }

      #endregion
   }
}

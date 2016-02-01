using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using log4net;
using log4net.Appender;
using Senesco.Client.Communication;
using Senesco.Client.Events;
using Senesco.Client.Sound;
using Senesco.Client.Transactions;
using Senesco.Client.Utility;

namespace Senesco.Client
{
   public class SenescoController
   {
      #region Fields and Creator

      private static readonly ILog s_log = LogManager.GetLogger(typeof(SenescoController));
      private static readonly ILog s_chatLog = LogManager.GetLogger("ChatLog");

      private AutoUpdate m_autoUpdater = new AutoUpdate();
      private SoundController m_soundController = new SoundController();
      private Server m_server = null;
      private User m_localUser = null;
      private UserList m_userList = null;

      private bool m_shouldReconnect = true;
      private const int c_reconnectIntervalSeconds = 30;  // 30 seconds
      private Timer m_reconnectTimer = null;
      
      public SenescoController()
      {
         m_autoUpdater.NewVersionAvailable += HandleNewVersionAvailable;
      }

      public SoundController SoundController
      {
         get { return m_soundController; }
      }

      public string ConnectedServerName
      {
         get
         {
            if (m_server == null)
               return null;
            return m_server.ServerName;
         }
      }

      public User LocalUser
      {
         get
         {
            if (m_localUser == null)
               return null;
            return (User)m_localUser.Clone();
         }
      }

      public List<User> UserList
      {
         get
         {
            if (m_userList == null)
               return new List<User>();
            return m_userList.Users;
         }
      }

      #endregion

      #region Connect, Disconnect, Shutdown

      public Status Connect(Server server)
      {
         if (server == null)
            return Status.Failure;

         // If connected to another server, disconnect first.
         if (m_server != null)
            m_server.Disconnect();

         // If we're connecting to a new server, set up the delegates.
         // This is typically only the same server during the "auto-reconnect" cycle.
         if (m_server != server)
         {
            m_server = server;

            // Set up the event delegates for this Server connection.
            m_server.ProgressUpdated += HandleProgressUpdated;
            m_server.Connected += HandleConnected;
            m_server.Disconnected += HandleDisconnected;
            m_server.ChatReceived += HandleChatReceived;
            m_server.UserListUpdate += HandleUserListUpdate;
            m_server.PmReceived += HandlePmReceived;
            m_server.UserInfoReceived += HandleUserInfoReceived;
         }

         // Initiate server connection.
         if (m_server.Connect() == Status.Failure)
         {
            string message = String.Format("Connecting to {0} failed.", m_server.Address);
            s_log.ErrorFormat(message);
            LocalChatMessage(message);
            m_server = null;
            return Status.Failure;
         }

         // Print a message indicating successful connection.
         LocalChatMessage(String.Format("Connected to '{0}'", m_server.ServerName));

         // Send initial transactions.
         Login login = new Login(m_server.LoginName,
                                 m_server.Password,
                                 m_server.Nick,
                                 m_server.Icon);
         if (m_server.SendTransaction(login) == Status.Failure)
         {
            string message = String.Format("Sending login to server failed.");
            s_log.ErrorFormat(message);
            LocalChatMessage(message);
            m_server = null;
            return Status.Failure;
         }

         // Store initial user settings.
         m_localUser = new User();
         m_localUser.Username = m_server.Nick;
         m_localUser.IconId = m_server.Icon;

         return Status.Success;
      }

      public Status Disconnect(string reason)
      {
         if (m_server == null)
            return Status.NoResult;

         // Disconnect from the server.
         m_server.Disconnect();

         // Send a notice to the chat window to make it clear you have been disconnected.
         if (string.IsNullOrEmpty(reason))
            reason = "No reason given.";
         LocalChatMessage(String.Format("Disconnected from '{0}': {1}", m_server.ServerName, reason));

         // Play the disconnected sound.
         m_soundController.PlaySound(SoundEffect.Disconnected);

         return Status.Success;
      }

      public void Shutdown()
      {
         Disconnect("User Quit.");
      }

      #endregion

      #region Reconnect Timer

      public void SetReconnect(bool shouldReconnect)
      {
         m_shouldReconnect = shouldReconnect;
      }

      private void StopReconnect()
      {
         // Stop any active reconnect timer.
         if (m_reconnectTimer != null)
         {
            m_reconnectTimer.Stop();
            m_reconnectTimer = null;
         }
      }

      private void StartReconnect()
      {
         if (m_reconnectTimer != null)
         {
            s_log.Info("Stopping previous Reconnect timer.");
            StopReconnect();
         }

         if (m_shouldReconnect == false)
            return;

         // Create a new KeepAlive timer using the specified interval.
         double seconds = c_reconnectIntervalSeconds;
         s_log.InfoFormat("Starting new Reconnect timer that fires every {0} seconds.", seconds);
         m_reconnectTimer = new Timer(seconds * 1000);
         m_reconnectTimer.Elapsed += reconnect_timerElapsed;
         m_reconnectTimer.AutoReset = false;
         m_reconnectTimer.Start();
      }

      public void reconnect_timerElapsed(object sender, ElapsedEventArgs e)
      {
         s_log.Info("Reconnect timer event elapsed.");

         // If the reconnect flag was switched off since the last attempt, or the
         // user possibly reconnected himself, then clean up.
         if (m_shouldReconnect == false || m_server.IsConnected == true)
         {
            StopReconnect();
            return;
         }

         s_log.Info("Attempting Reconnect...");

         // Try reconnecting.
         if (Connect(m_server) == Status.Success)
         {
            // If the reconnect succeeded, stop the reconnect timer!
            StopReconnect();
            return;
         }

         string message = String.Format("Reconnect failed, waiting {0} seconds for next try.", c_reconnectIntervalSeconds);
         OnProgressUpdated(new ProgressUpdatedEventArgs(message, 100));

         // Start the timer again (only fires manually).
         // This is done because the connect attempt above could take a long
         // time to time out, or it might be refused immediately.  We don't
         // want to blindly fire the timer event in case the fire interval is
         // shorter than the timeout case.  That would cause these connect
         // attempts to run concurrently.
         m_reconnectTimer.Start();
      }

      #endregion

      #region Bookmark Wrappers

      // Even though the FileUtils class can be accessed directly, the View
      // should generally use the Controller class for every operation.

      public Status AddBookmark(Server server)
      {
         return FileUtils.AddBookmark(server);
      }

      public Status DeleteBookmark(string bookmarkName)
      {
         return FileUtils.DeleteBookmark(bookmarkName);
      }

      public List<string> GetBookmarkNames()
      {
         return FileUtils.GetBookmarkNames();
      }

      public Server GetBookmark(string serverName)
      {
         return FileUtils.GetBookmark(serverName);
      }

      public string GetAutoConnectBookmark()
      {
         return RegistryUtils.GetAutoConnectBookmark();
      }

      #endregion

      #region Chat System

      /// <summary>
      /// Sends a "local message" to the chat system with some labels and a
      /// timestamp to provide status updates to the user.
      /// </summary>
      /// <param name="message">The message to display.</param>
      private void LocalChatMessage(string message)
      {
         // Include the timestamp by default.
         LocalChatMessage(message, true);
      }

      private void LocalChatMessage(string message, bool includeTimestamp)
      {
         if (String.IsNullOrEmpty(message))
            return;

         // Format the message.
         HandleChatReceived(String.Format("\n<<< {0} >>>", message), -1, true);

         // Include the timestamp if flagged to.
         if (includeTimestamp)
            HandleChatReceived(String.Format("\n<<< {0} >>>", DateTime.Now.ToString()), -1, true);
      }

      /// <summary>
      /// Sends the given chat string to the connected server.
      /// </summary>
      /// <param name="text">The chat string to process and send.</param>
      /// <param name="cursorIndex">
      /// The index position of the cursor, if available.  Specify -1 otherwise.
      /// </param>
      /// <param name="emote">True if the chat should be sent as an emote.</param>
      /// <returns>
      /// Status.Success if all chats were successfully queued.
      /// Status.NoResult if there was no chat to send, or no newlines were present.
      /// Status.Failure if there was chat to send but queueing failed.
      /// </returns>
      public Status SendChat(string text, bool emote)
      {
         if (m_server == null || m_server.IsConnected == false)
         {
            s_log.ErrorFormat("Not sending chat - not connected to server");
            return Status.Failure;
         }

         // Split the string into separate lines, if needed.
         List<string> chats = ProcessChat(text);

         if (chats == null || chats.Count == 0)
         {
            s_log.Debug("No chat to send.");
            return Status.NoResult;
         }

         s_log.Debug("Connected to a server - sending chat");

         // Don't allow multi-line emotes.
         if (chats.Count > 1)
            emote = false;

         // Send each chat in a separate SendChat transaction.
         Status result = Status.NoResult;
         foreach (string chat in chats)
         {
            result = m_server.SendTransaction(new SendChat(chat, null, emote));
            if (result == Status.Failure)
            {
               s_log.ErrorFormat("Sending chat failed: {0}", chat);
               return Status.Failure;
            }
         }
         return result;
      }

      /// <summary>
      /// Splits the chat sequence into lines by splitting on the newline characters.
      /// </summary>
      /// <param name="text">The text to process.</param>
      /// <returns>A list of single lines of chat.</returns>
      private List<string> ProcessChat(string text)
      {
         // Split up the chat into an array of lines.
         string[] delimiters = new string[1] { Environment.NewLine };
         string[] rawLines = text.Split(delimiters, StringSplitOptions.None);

         // Remove the newline characters from each line.
         List<string> finalLines = new List<string>();
         foreach (string rawLine in rawLines)
         {
            string finalLine = rawLine.Replace(Environment.NewLine, String.Empty);
            //string finalLine = rawLine.Trim();
            if (string.IsNullOrEmpty(finalLine) == false)
               finalLines.Add(finalLine);
         }
         return finalLines;
      }

      #endregion

      #region Chat Log Files

      public Status OpenChatLog()
      {
         try
         {
            System.Diagnostics.Process.Start(GetLogFilePath("RollingChatLog"));
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error opening chat log: {0}", e.Message);
            return Status.Failure;
         }
      }

      public Status ShowChatLogs()
      {
         try
         {
            string fullPath = GetLogFilePath("RollingChatLog");
            System.Diagnostics.Process.Start(Path.GetDirectoryName(fullPath));
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error showing chat logs: {0}", e.Message);
            return Status.Failure;
         }
      }

      private string GetLogFilePath(string appenderName)
      {
         // Get the current log file from log4net.
         // Unfortunately, this returns ALL appenders intsead of just the ones for s_chatLog.
         IAppender[] appenders = s_chatLog.Logger.Repository.GetAppenders();
         if (appenders == null)
         {
            s_log.Error("No appenders found for chat log.");
            return string.Empty;
         }

         // Confirm which appender has the correct type and name.
         foreach (IAppender appender in appenders)
         {
            RollingFileAppender rfa = appender as RollingFileAppender;
            if (rfa == null)
               continue;
            if (rfa.Name != appenderName)
               continue;

            // Return the matching filename.
            s_log.InfoFormat("Launching file: {0}", rfa.File);
            return rfa.File;
         }

         s_log.ErrorFormat("No matching appender found.");
         return string.Empty;
      }

      #endregion

      #region User List Interaction

      public Status GetUserList()
      {
         if (m_server == null || m_server.IsConnected == false)
         {
            s_log.ErrorFormat("Cannot get user list: not connected to server.");
            return Status.Failure;
         }

         return m_server.SendTransaction(new GetUserList(null));
      }

      public Status GetUserInfo(User selectedUser)
      {
         if (m_server == null || m_server.IsConnected == false)
         {
            s_log.ErrorFormat("Cannot get user info: not connected to server.");
            return Status.Failure;
         }
         
         if (selectedUser == null)
         {
            s_log.ErrorFormat("Cannot get user info: no user specified.");
            return Status.Failure;
         }

         return m_server.SendTransaction(new GetUserInfo(selectedUser.UserId));
      }

      public Status SendPrivateMessage(User selectedUser, string message)
      {
         if (m_server == null || m_server.IsConnected == false)
         {
            s_log.ErrorFormat("Cannot send PM: not connected to server.");
            return Status.Failure;
         }
         
         if (selectedUser == null)
         {
            s_log.ErrorFormat("Cannot send PM: no user specified.");
            return Status.Failure;
         }

         m_soundController.PlaySound(SoundEffect.PmSent);

         return m_server.SendTransaction(new PmSend(selectedUser.UserId, message));
      }

      /// <summary>
      /// Call this method to update your user settings on the server.  You can
      /// update your nick, icon number, and ignore flags.  For ignoring, you
      /// can either ignore Private Messages or Private Chats, or both.  When
      /// ignoring such private requests, you can also optionally specify an
      /// automatic ignore response, which is handled by the server.
      /// </summary>
      /// <param name="newNick">Your new nick</param>
      /// <param name="newIcon">Your new icon number</param>
      /// <param name="ignorePrivMsgs">If true, ignore PM's</param>
      /// <param name="ignorePrivChat">If true, ignore PC's</param>
      /// <param name="autoIgnoreMsg">
      /// If provided, send this message to the requestor on your behalf when a request is ignored.
      /// </param>
      public Status SetUserInfo(User newUserInfo)
      {
         if (m_server == null || m_server.IsConnected == false)
         {
            string message = "Cannot set user info: not connected to server.";
            s_log.ErrorFormat(message);
            return Status.GetFailure(message);
         }

         string error;
         if (newUserInfo.IsValid(out error) == false)
         {
            string message = String.Format("Cannot set user info: {0}.", error);
            s_log.ErrorFormat(message);
            return Status.GetFailure(message);
         }

         // If not ignoring anything, don't send an ignore message.
         if (newUserInfo.IgnorePrivateMsgs == false && newUserInfo.IgnorePrivateChat == false)
         {
            newUserInfo.IgnoreAutoResponse = null;
         }

         SetUserInfo setUserInfo = new SetUserInfo(
            newUserInfo.Username,
            newUserInfo.IconId,
            newUserInfo.IgnorePrivateMsgs,
            newUserInfo.IgnorePrivateChat,
            newUserInfo.IgnoreAutoResponse);

         Status result = m_server.SendTransaction(setUserInfo);

         // Update the local values, since the server doesn't come back with a response.
         if (result == Status.Success)
            m_localUser = newUserInfo;

         return result;
      }

      #endregion

      #region Help Menu

      public Status LaunchWebsite()
      {
         // Launch the website URL.
         //return LaunchURL("http://rancor.yi.org/Senesco/53cr37/");

         // Launch directly to the changelog embed.
         return LaunchURL("http://rancor.yi.org/Senesco/53cr37/index.php?file=changelog.txt");
      }

      public Status LaunchURL(string url)
      {
         if (String.IsNullOrEmpty(url))
            return Status.Success;

         try
         {
            System.Diagnostics.Process.Start(url);
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error launching URL: {0}", e.Message);
            return Status.Failure;
        }
      }

      #endregion

      // This controller class subscribes to the Server object's events using
      // these event handlers, and then bubble them up to the subscribing client.
      #region Incoming Transaction Event Handlers

      public void HandleChatReceived(object sender, ChatReceivedEventArgs e)
      {
         if (e != null)
         {
            HandleChatReceived(e.Text, e.Window, false);
         }
      }

      public void HandleChatReceived(string chatText, int chatWindow, bool local)
      {
         // Strip exterior newlines so each presenter can handle it as needed.
         chatText = chatText.Trim();
         
         // Play the chat sound if this chat came from the server (not local).
         if (local == false)
            m_soundController.PlaySound(SoundEffect.ChatClick);

         // Raise the ChatReceived event.
         OnChatReceived(new ChatReceivedEventArgs(chatText, chatWindow));

         // Add this text to the chat log.
         // Local client messages are logged under DEBUG, chats under INFO.
         if (local)
            s_chatLog.Debug(chatText);
         else
            s_chatLog.Info(chatText);
      }
      
      public void HandleProgressUpdated(object sender, ProgressUpdatedEventArgs e)
      {
         OnProgressUpdated(e);
      }

      public void HandleConnected(object sender, ConnectedEventArgs e)
      {
         LocalChatMessage(String.Format("{0}", e.Message));

         // Raise the event.
         OnConnected(e);
      }

      public void HandleDisconnected(object sender, DisconnectedEventArgs e)
      {
         // Raise the event.
         OnDisconnected(e);

         // Show a notification in the chat window.
         LocalChatMessage(String.Format("Disconnected from '{0}': {1}", m_server.ServerName, e.Reason));

         // Play the disconnected sound.
         m_soundController.PlaySound(SoundEffect.Disconnected);

         // Start the reconnect timer.
         StartReconnect();
      }

      public void HandleUserListUpdate(object sender, UserListUpdateEventArgs e)
      {
         if (e == null)
            return;

         // Update the user list in the model.
         // If this is not a delta, then we're being given a complete list of users in "addList".
         if (e.Delta == false)
         {
            m_userList = new UserList(e.AddList);
         }
         else
         {
            // This can happen if the UI never requests the userlist prior to
            // other server events happening.
            if (m_userList == null)
               m_userList = new UserList(null);

            // Otherwise, this IS a delta, so update the list appropriately.
            if (e.RemoveList != null && e.RemoveList.Count > 0)
            {
               IEnumerable<User> removes = e.RemoveList;
               LocalChatMessage(m_userList.FormatUserLeaveMsg(removes));
               m_userList.RemoveUsers(removes);
               m_soundController.PlaySound(SoundEffect.UserPart);
            }

            if (e.AddList != null && e.AddList.Count > 0)
            {
               IEnumerable<User> adds = e.AddList;
               LocalChatMessage(m_userList.FormatUserChangeMsg(adds));
               m_userList.AddUsers(adds);
               m_soundController.PlaySound(SoundEffect.UserJoin);
            }
         }

         // Raise the event.
         OnUserListUpdate(e);
      }
      
      public void HandleUserInfoReceived(object sender, UserInfoEventArgs e)
      {
         // Raise the event.
         OnUserInfoReceived(e);
      }
      
      public void HandlePmReceived(object sender, PrivateMsgEventArgs e)
      {
         if (e == null)
            return;

         // Raise the event.
         OnPmReceived(e);

         // Play the private message received sound.
         m_soundController.PlaySound(SoundEffect.PmReceived);
      }

      public void HandleNewVersionAvailable(object sender, NewVersionEventArgs e)
      {
         // Raise the event.
         OnNewVersionAvailable(e);
      }

      #endregion

      // Client implementations subscribe to these events and just wait for
      // them to happen on their own schedule.
      #region Events

      // Define events
      public event EventHandlers.ProgressUpdatedEventHandler ProgressUpdated;
      public event EventHandlers.ConnectedEventHandler Connected;
      public event EventHandlers.DisconnectedEventHandler Disconnected;
      public event EventHandlers.ChatReceivedEventHandler ChatReceived;
      public event EventHandlers.UserListUpdateEventHandler UserListUpdate;
      public event EventHandlers.PrivateMsgEventHandler PmReceived;
      public event EventHandlers.UserInfoDelegate UserInfoReceived;
      public event EventHandlers.NewVersionDelegate NewVersionAvailable;

      // Methods to raise each event
      protected virtual void OnProgressUpdated(ProgressUpdatedEventArgs e)
      {
         if (ProgressUpdated != null)
            ProgressUpdated(this, e);
      }

      protected virtual void OnConnected(ConnectedEventArgs e)
      {
         if (Connected != null)
            Connected(this, e);
      }

      protected virtual void OnDisconnected(DisconnectedEventArgs e)
      {
         if (Disconnected != null)
            Disconnected(this, e);
      }

      protected virtual void OnChatReceived(ChatReceivedEventArgs e)
      {
         if (ChatReceived != null)
            ChatReceived(this, e);
      }

      protected virtual void OnUserListUpdate(UserListUpdateEventArgs e)
      {
         if (UserListUpdate != null)
            UserListUpdate(this, e);
      }

      protected virtual void OnPmReceived(PrivateMsgEventArgs e)
      {
         if (PmReceived != null)
            PmReceived(this, e);
      }

      protected virtual void OnUserInfoReceived(UserInfoEventArgs e)
      {
         if (UserInfoReceived != null)
            UserInfoReceived(this, e);
      }

      protected virtual void OnNewVersionAvailable(NewVersionEventArgs e)
      {
         if (NewVersionAvailable != null)
            NewVersionAvailable(this, e);
      }

      /// <summary>
      /// This method initiates a process to upgrade the application from the web.
      /// It begins a process which will replace the current assemblies, so the
      /// application should be terminated after this method is completed.
      /// </summary>
      /// <param name="newVersion">
      /// The version string provided by the NewVersionAvailable event.
      /// </param>
      public void PerformUpgrade(string newVersion)
      {
         m_autoUpdater.PerformUpdate(newVersion);
      }

      #endregion
   }
}

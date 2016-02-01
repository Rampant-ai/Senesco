using System;
using System.Collections.Generic;
using System.Reflection;
using Senesco.Client;
using Senesco.Client.Communication;
using Senesco.Client.Events;
using Senesco.Client.Utility;

namespace Senesco.Console
{
   class Program
   {
      #region Static Entry Point

      private static Program s_program = new Program();

      public static void Main(string[] args)
      {
         s_program.Start(args);
      }

      #endregion

      private Screen m_screen = new Screen();
      private SenescoController m_controller = new SenescoController();

      private void Start(string[] args)
      {
         // Initialize the Commands class.
         Commands.Initialize();

         // Print version and basic instructions.
         PrintVersion();

         // Subscribe to all of the controller events.
         SubscribeToEvents();

         // Process argument for immediate server connection.
         if (args != null && args.Length >= 1)
         {
            Server server = ParseServer_CommandLine(args);
            ConnectToServer(server);
         }

         // Begin monitoring user input.
         InputLoop();
      }

      #region User Input

      private void InputLoop()
      {
         try
         {
            bool continueLoop = true;
            while (continueLoop)
            {
               // Wait indefinitely for a key press.
               ConsoleKeyInfo input = System.Console.ReadKey(true);

               // Process the keypress against the screen processor.
               string text = m_screen.HandleKey(input);

               // If the screen returned a non-null string, process it.
               if (String.IsNullOrEmpty(text) == false)
                  continueLoop = ProcessInput(text);
            }
         }
         catch (Exception e)
         {
            m_screen.WriteLine(e.Message);
            m_screen.WriteLine(e.StackTrace);
            m_screen.WriteLine("Unhandled exception -- press any key to close");
            System.Console.ReadKey(true);
         }
      }

      /// <summary>
      /// User-entered text command interpretation for the command line.
      /// </summary>
      /// <param name="text">The user-entered text.</param>
      /// <returns>
      /// False if the input resulted in a QUIT command.
      /// True if the program should continue.
      /// </returns>
      private bool ProcessInput(string text)
      {
         // Do nothing if the text is completely empty.
         if (String.IsNullOrEmpty(text))
            return true;

         // Switch on the entered command, if there is one.
         Status result;
         switch (Commands.GetCommand(text, out text))
         {
            case Commands.Command.Connect:
               // Connect command that tries to connect via a bookmark name
               // or manually-entered server details.
               ConnectToServer(ParseServer_UserEntry(text));
               break;

            case Commands.Command.Bookmark:
               SaveBookmark(text);
               break;

            case Commands.Command.Delete:
               DeleteBookmark(text);
               break;

            case Commands.Command.Bookmarks:
               // Get list of local bookmarks.
               List<string> bookmarks = m_controller.GetBookmarkNames();
               m_screen.WriteLine("Bookmarks:");
               foreach (string bookmark in bookmarks)
                  m_screen.WriteLine("   {0}", bookmark);
               break;

            case Commands.Command.Reconnect:
               bool reconnect;
               if (Boolean.TryParse(text, out reconnect) == true)
               {
                  m_controller.SetReconnect(reconnect);
                  m_screen.WriteLine("Reconnect flag set to {0}.", reconnect);
               }
               else
                  m_screen.WriteLine("Value for 'reconnect' not understood.  Use 'true' or 'false'.");
               break;

            case Commands.Command.Disconnect:
               // Disconnect from the current server.
               result = m_controller.Disconnect("User Disconnect");
               if (result == Status.Failure)
                  m_screen.WriteLine("Failed to disconnect");
               break;

            case Commands.Command.Quit:
               // Command to exit the program.  Also attempts a graceful disconnect.
               m_controller.Disconnect("User Quit");
               return false;

            case Commands.Command.Version:
               // Local command to print the current version.
               PrintVersion();
               break;

            case Commands.Command.Help:
               // Local command to print the help text.
               Commands.PrintHelp(m_screen);
               break;

            case Commands.Command.Prefix:
               // Local command to change the command prefix string.
               result = Commands.SetCommandPrefix(text);
               if (result == Status.Failure)
                  m_screen.WriteLine("Could not change command prefix to '{0}': {1}", text, result.Message);
               else
                  m_screen.WriteLine("{0}", result.Message);
               break;

            case Commands.Command.Who:
               // Request user list from server.
               result = m_controller.GetUserList();
               if (result == Status.Failure)
                  m_screen.WriteLine("Failed to get user list");
               break;

            case Commands.Command.Nick:
               ChangeNick(text);
               break;

            case Commands.Command.Icon:
               ChangeIcon(text);
               break;

            case Commands.Command.Emote:
               // Submit text as emote.
               if (m_controller.SendChat(text, true) == Status.Failure)
                  m_screen.WriteLine("Failed to send chat");
               break;

            case Commands.Command.None:
               // Submit text as chat.
               if (m_controller.SendChat(text, false) == Status.Failure)
                  m_screen.WriteLine("Failed to send chat");
               break;

            case Commands.Command.Unknown:
               // The command did not match any of the enum mappings in the Commands class.
               m_screen.WriteLine("Unrecognized command");
               break;

            default:
               // There is a recognized enum value, but none of the cases above matches it!
               m_screen.WriteLine("Unimplemented command");
               break;
         }

         return true;
      }

      #endregion

      #region Command Helpers (Subroutines)

      private void ChangeNick(string text)
      {
         // Get the current user settings.  This returns a Clone.
         User nickChange = m_controller.LocalUser;
         if (nickChange == null)
         {
            m_screen.WriteLine("Cannot change nick yet");
            return;
         }

         // Change only the nick.
         nickChange.Username = text;

         // Submit changed user.
         Status result = m_controller.SetUserInfo(nickChange);
         if (result == Status.Failure)
            m_screen.WriteLine(result.Message);
      }

      private void ChangeIcon(string text)
      {
         // Get the current user settings.  This returns a Clone.
         User iconChange = m_controller.LocalUser;
         if (iconChange == null)
         {
            m_screen.WriteLine("Cannot change icon yet");
            return;
         }

         // Set changed icon.  If the icon text won't parse, do nothing.
         if (int.TryParse(text, out iconChange.IconId) == false)
         {
            m_screen.WriteLine("Could not parse icon number");
            return;
         }

         // Submit changed user.
         Status result = m_controller.SetUserInfo(iconChange);
         if (result == Status.Failure)
            m_screen.WriteLine(result.Message);
      }

      private void PrintVersion()
      {
         try
         {
            m_screen.WriteLine(String.Format("Senesco v{0}",
               Assembly.GetExecutingAssembly().GetName().Version.ToString()));
         }
         catch
         {
            m_screen.WriteLine("Senesco (unable to determine version)");
         }

         m_screen.WriteLine("Type {0}help for a list of commands", Commands.GetCommandPrefix());
      }

      #endregion

      #region Connect To Server Helpers

      private void DeleteBookmark(string text)
      {
         Status result = m_controller.DeleteBookmark(text);
         if (result == Status.Failure)
            m_screen.WriteLine("{0}", result.Message);
         else
            m_screen.WriteLine("Bookmark '{0}' deleted.", text);
      }

      private void SaveBookmark(string text)
      {
         // Parse the parameters into a Server object.
         Server newBookmark;
         Status result = ParseBookmark(text, out newBookmark);
         if (result == Status.Failure)
         {
            m_screen.WriteLine("{0}", result.Message);
            return;
         }

         // Write the server out as a bookmark.
         result = m_controller.AddBookmark(newBookmark);
         if (result == Status.Failure)
            m_screen.WriteLine("Failed to save bookmark '{0}': {1}",
                               newBookmark.ServerName, result.Message);
         else
            m_screen.WriteLine("Bookmark '{0}' saved.", newBookmark.ServerName);
      }

      private Status ParseBookmark(string text, out Server bookmark)
      {
         bookmark = null;
         string[] args = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

         // There must be at least two parameters.
         if (args.Length < 2)
            return Status.GetFailure("A bookmark name and server address are required to create a bookmark.");

         // The first token is the new bookmark name (server name), then normal server parameters.
         string bookmarkName = args[0];

         // Copy the server parameters to a new array so we can re-use the parsing method.
         string[] serverArgs = new string[args.Length - 1];
         for (int i = 1; i < args.Length; i++)
         {
            serverArgs[i - 1] = args[i];
         }

         // Parse the parameters, and set the bookmark name as the server name.
         bookmark = ParseServerArguments(serverArgs);
         bookmark.ServerName = bookmarkName;
         return Status.Success;
      }

      private Server ParseServer_UserEntry(string text)
      {
         // If the whole text equals a bookmark name, use that.
         Server bookmark = m_controller.GetBookmark(text);
         if (bookmark != null)
            return bookmark;

         // If the bookmark name was not found, parse server components.
         string[] args = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         return ParseServerArguments(args);
      }

      private Server ParseServer_CommandLine(string[] args)
      {
         if (args == null || args.Length == 0)
            return null;

         // If there are multiple parameters, parse server components even if
         // the first parameter might match a bookmark.
         if (args.Length > 1)
            return ParseServerArguments(args);

         // If there is only one parameter, try to look it up as a bookmark.
         if (args.Length == 1)
         {
            Server bookmark = m_controller.GetBookmark(args[0]);
            if (bookmark != null)
               return bookmark;
         }
         
         // Else parse a Server with only the address.
         return ParseServerArguments(args);
      }

      private Server ParseServerArguments(string[] args)
      {
         Server server = new Server();

         for (int i = 0; i < 5; i++)
         {
            if (args.Length < i + 1)
               break;

            switch (i)
            {
               case 0:
                  server.Address = args[i];
                  break;
               case 1:
                  server.Nick = args[i];
                  break;
               case 2:
                  server.LoginName = args[i];
                  break;
               case 3:
                  server.Password = args[i];
                  break;
               case 4:
                  int.TryParse(args[i], out server.Icon);
                  break;
            }
         }

         return server;
      }

      private Status ConnectToServer(Server server)
      {
         if (server == null)
            return Status.NoResult;

         m_screen.WriteLine("Connecting to: {0}", server.ToString());

         Status result = m_controller.Connect(server);

         if (result == Status.Failure)
            m_screen.WriteLine("Failed to connect");

         return result;
      }

      #endregion

      #region Event Handling

      private void SubscribeToEvents()
      {
         m_controller.ChatReceived += Event_ChatReceived;
         m_controller.Connected += Event_Connected;
         m_controller.Disconnected += Event_Disconnected;
         m_controller.PmReceived += Event_PmReceived;
         m_controller.ProgressUpdated += Event_ProgressUpdated;
         m_controller.UserInfoReceived += Event_UserInfoReceived;
         m_controller.UserListUpdate += Event_UserListUpdate;
      }

      private void Event_UserListUpdate(object sender, UserListUpdateEventArgs e)
      {
         // Don't do anything in the case of a delta because the controller
         // already analyzes the changes and reports them as local chat text.
         if (e.Delta == true)
            return;

         // Dump complete userlist, because this non-delta response only arrives
         // in response to a "/who" command.
         if (e.AddList != null)
         {
            m_screen.WriteLine("---------------------");
            m_screen.WriteLine("User List:");
            foreach (User user in e.AddList)
            {
               m_screen.WriteLine(String.Format("   {0}", user.ToString()));
            }
            m_screen.WriteLine("---------------------");
         }
      }

      private void Event_UserInfoReceived(object sender, UserInfoEventArgs e)
      {
         m_screen.WriteLine("---------------------");
         m_screen.WriteLine("User Details:");
         m_screen.WriteLine(e.UserInfo.Trim());
         m_screen.WriteLine("---------------------");
      }

      private void Event_ProgressUpdated(object sender, ProgressUpdatedEventArgs e)
      {
         m_screen.WriteLine("{0}: Progress {1}%", e.EventUpdated, e.ProgressPercent);
      }

      private void Event_PmReceived(object sender, PrivateMsgEventArgs e)
      {
         m_screen.WriteLine("Private Message: ({0}) {1}", e.SendingNick, e.Message.Trim());
      }

      private void Event_Disconnected(object sender, DisconnectedEventArgs e)
      {
         m_screen.WriteLine("Disconnected!");
      }

      private void Event_Connected(object sender, ConnectedEventArgs e)
      {
         m_screen.WriteLine("Connected!");

         // Request the userlist immediately.
         // This is handy when first connecting to see who is initially there
         // even without a separate userlist window, and it is also important
         // internally to better handle delta user list updates.
         m_controller.GetUserList();
      }

      private void Event_ChatReceived(object sender, ChatReceivedEventArgs e)
      {
         m_screen.WriteLine(e.Text.Trim());
      }

      #endregion
   }
}

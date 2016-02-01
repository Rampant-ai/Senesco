
using System;
using System.Collections.Generic;
using Senesco.Client.Utility;

namespace Senesco.Console
{
   class Commands
   {
      public enum Command
      {
         None,    // No command was entered (command prefix was not used)
         Unknown, // The command entered was unknown
         Connect,
         Reconnect,
         Bookmark,
         Delete,
         Bookmarks,
         Disconnect,
         Quit,
         Nick,
         Icon,
         Emote,
         Who,
         Version,
         Prefix,
         Help
      }

      private static string s_commandPrefix = "/";
      private static Dictionary<string, Command> s_commandLookup = null;
      private static Dictionary<Command, string> s_commandUsage = null;

      public static void Initialize()
      {
         InitLookup();

         // Restore the last saved command prefix.
         s_commandPrefix = ConfigSettings.UserSettings.CommandPrefix;
      }

      public static Command GetCommand(string text, out string strippedText)
      {
         // If the text does not start with the command prefix, it's not a command.
         if (text.StartsWith(s_commandPrefix) == false)
         {
            strippedText = text;
            return Command.None;
         }

         // Find the end of the command word, either by delimiter or total string
         // length if there's no text after the command.
         int delimiter = text.IndexOf(" ");
         string command;
         if (delimiter == -1)
         {
            strippedText = string.Empty;

            // Remove the prefix, the rest is the command string.
            command = text.Remove(0, s_commandPrefix.Length);
         }
         else
         {
            // If there's anything after the delimiter character, then copy it as
            // the stripped output.
            if (text.Length > delimiter)
               strippedText = text.Substring(delimiter + 1, (text.Length - 1) - delimiter);
            else
               strippedText = string.Empty;

            // The command is the substring after the prefix up to the delimiter.
            command = text.Substring(s_commandPrefix.Length, delimiter - s_commandPrefix.Length);
         }

         return LookupCommand(command);
      }
      
      /// <summary>
      /// Helper method to look up the given command text in the command lookup.
      /// </summary>
      /// <param name="commandText"></param>
      /// <returns></returns>
      private static Command LookupCommand(string commandText)
      {
         Command command;
         if (s_commandLookup.TryGetValue(commandText.ToLower(), out command) == false)
            return Command.Unknown;
         return command;
      }

      /// <summary>
      /// Initialization method for the command lookup dictionary.
      /// This dictionary maps command strings to the internal Command enum.
      /// </summary>
      private static void InitLookup()
      {
         s_commandLookup = new Dictionary<string, Command>();

         // ALL COMMANDS MUST BE FULLY LOWERCASE!
         s_commandLookup.Add("connect", Command.Connect);
         s_commandLookup.Add("server", Command.Connect);
         s_commandLookup.Add("bookmark", Command.Bookmark);
         s_commandLookup.Add("delete", Command.Delete);
         s_commandLookup.Add("bookmarks", Command.Bookmarks);
         s_commandLookup.Add("reconnect", Command.Reconnect);
         s_commandLookup.Add("disconnect", Command.Disconnect);
         s_commandLookup.Add("bye", Command.Disconnect);
         s_commandLookup.Add("quit", Command.Quit);
         s_commandLookup.Add("exit", Command.Quit);
         s_commandLookup.Add("nick", Command.Nick);
         s_commandLookup.Add("icon", Command.Icon);
         s_commandLookup.Add("me", Command.Emote);
         s_commandLookup.Add("who", Command.Who);
         s_commandLookup.Add("version", Command.Version);
         s_commandLookup.Add("help", Command.Help);
         s_commandLookup.Add("prefix", Command.Prefix);

         // If any of these commands have "usage" beyond the command itself,
         // include the usage parameters here.
         s_commandUsage = new Dictionary<Command, string>();
         s_commandUsage.Add(Command.Connect, "<bookmark name> OR <address> [nick] [username] [password] [icon#]");
         s_commandUsage.Add(Command.Bookmark, "<bookmark name> <address> [nick] [username] [password] [icon#]");
         s_commandUsage.Add(Command.Reconnect, "<true or false>");
         s_commandUsage.Add(Command.Delete, "<bookmark name>");
         s_commandUsage.Add(Command.Nick, "<new nick>");
         s_commandUsage.Add(Command.Icon, "<new icon number>");
         s_commandUsage.Add(Command.Emote, "<emote text>");
         s_commandUsage.Add(Command.Prefix, "<new command prefix>");
      }

      /// <summary>
      /// Updates the command-prefix for the interpreter.
      /// </summary>
      /// <returns>True upon error, false if the prefix was updated.</returns>
      public static Status SetCommandPrefix(string prefix)
      {
         if (String.IsNullOrEmpty(prefix))
            return Status.GetFailure("Prefix cannot be empty.");

         if (prefix.Length > 10)
            return Status.GetFailure("Prefix cannot be longer than 10 characters.");

         s_commandPrefix = prefix;
         
         // Save the changed prefix.
         ConfigSettings.UserSettings.CommandPrefix = prefix;
         ConfigSettings.UserSettings.Save();
         
         return Status.GetSuccess(String.Format("Prefix changed to '{0}'.", prefix));
      }

      public static string GetCommandPrefix()
      {
         return s_commandPrefix;
      }

      public static void PrintHelp(Screen screen)
      {
         screen.WriteLine("Commands:");
         foreach (KeyValuePair<string, Command> kvp in s_commandLookup)
         {
            string usage;
            s_commandUsage.TryGetValue(kvp.Value, out usage);

            // If there are no usage parameters for this command, simpler output.
            if (String.IsNullOrEmpty(usage))
               screen.WriteLine("   {0}{1}", s_commandPrefix, kvp.Key);
            else
               screen.WriteLine("   {0}{1} {2}", s_commandPrefix, kvp.Key, usage);
         }
      }

   }
}

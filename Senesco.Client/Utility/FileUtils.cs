using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using Senesco.Client.Communication;

namespace Senesco.Client.Utility
{
   public class FileUtils
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(FileUtils));

      private const string s_bookmarkExtension = ".sbm";

      #region Public Bookmark Management Methods

      /// <summary>
      /// Saves the given Server information into a bookmark file in the
      /// currently configured bookmark directory.
      /// </summary>
      /// <param name="server">The Server object to save.</param>
      /// <returns>Status result code.</returns>
      public static Status AddBookmark(Server server)
      {
         if (server == null)
            return Status.GetFailure("Internal error (null server object provided)");

         string fullPath = null;
         try
         {
            s_log.InfoFormat("Saving bookmark for server '{0}'", server.ServerName);

            string filename = server.ServerName + s_bookmarkExtension;
            fullPath = Path.Combine(GetBookmarkDirectory(), filename);
            if (File.Exists(fullPath))
            {
               s_log.WarnFormat("Bookmark already exists: {0}", fullPath);
               return Status.GetFailure("A bookmark with that name already exists.");
            }
            CustomSerialize(server, fullPath);
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error saving bookmark: {0}", e.Message);
            try { File.Delete(fullPath); }
            catch { }
            return Status.GetFailure(String.Format("Error while saving bookmark: {0}", e.Message));
         }
      }

      /// <summary>
      /// Utility method to delete a bookmark named after the given server name.
      /// </summary>
      /// <param name="bookmarkName">The name of the bookmark to delete.</param>
      /// <returns>Status result code.</returns>
      public static Status DeleteBookmark(string bookmarkName)
      {
         if (String.IsNullOrEmpty(bookmarkName))
            return Status.GetFailure("No bookmark name provided.");

         string fullPath = null;
         try
         {
            s_log.InfoFormat("Deleting bookmark for server '{0}'", bookmarkName);

            string filename = bookmarkName + s_bookmarkExtension;
            fullPath = Path.Combine(GetBookmarkDirectory(), filename);

            if (File.Exists(fullPath) == false)
               return Status.GetFailure(String.Format("Bookmark '{0}' was not found.", bookmarkName));

            File.Delete(fullPath);
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error deleting bookmark: {0}", e.Message);
            return Status.GetFailure(String.Format("An error occurred deleting the bookmark: {0}", e.Message));
         }
      }

      /// <summary>
      /// Utility method to retrieve a bookmark via a FileInfo object.
      /// This method finds and deserializes the saved bookmark file.
      /// </summary>
      /// <param name="bookmarkFile">The bookmark file to deserialize</param>
      /// <returns>The deserialized Server information.</returns>
      public static Server GetBookmark(FileInfo bookmarkFile)
      {
         if (bookmarkFile == null)
            return null;
         return GetBookmarkInternal(bookmarkFile.FullName);
      }

      /// <summary>
      /// Retrieves the server information from the given bookmark name.
      /// This method finds and deserializes the saved bookmark file.
      /// </summary>
      /// <param name="serverName">The bookmark server name to retrieve.</param>
      /// <returns>The deserialized Server information.</returns>
      public static Server GetBookmark(string serverName)
      {
         try
         {
            if (String.IsNullOrEmpty(serverName))
            {
               s_log.ErrorFormat("Tried to retrieve bookmark for empty server name.");
               return null;
            }

            s_log.InfoFormat("Retrieving bookmark for server name '{0}'", serverName);

            // Construct full path to the bookmark file.
            string fullPath = Path.Combine(GetBookmarkDirectory(), serverName + s_bookmarkExtension);

            // Convert bookmark file into a Server.
            return GetBookmarkInternal(fullPath);
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error reading bookmark: {0}", e.Message);
            return null;
         }
      }

      /// <summary>
      /// Helper method to deserialize a saved bookmark from a full path.
      /// </summary>
      /// <param name="fullPath"></param>
      /// <returns></returns>
      private static Server GetBookmarkInternal(string fullPath)
      {
         try
         {
            if (File.Exists(fullPath) == false)
            {
               s_log.ErrorFormat("Bookmark file {0} does not exist.", fullPath);
               return null;
            }

            string fileExtension = Path.GetExtension(fullPath);

            // If the given filename has the "lnk" extension, follow the shortcut then proceed.
            //if (String.Compare(fileExtension, "lnk", true) != 0)


            // If this file does not have the ".sbm" extension, reject it.
            if (String.Compare(fileExtension, s_bookmarkExtension, true) != 0)
            {
               s_log.ErrorFormat("Bookmark files must be have the file extension '{0}': {1}",
                                 s_bookmarkExtension, fullPath);
               return null;
            }

            // Open the file and deserialize the Server object.
            return CustomDeserialize(fullPath);
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error reading bookmark: {0}", e.Message);
            return null;
         }
      }

      /// <summary>
      /// Lists all bookmarks in the currently configured bookmark directory,
      /// without their file extensions.  The returned list is suitable for
      /// display.
      /// </summary>
      /// <returns>The list of bookmark names.</returns>
      public static List<string> GetBookmarkNames()
      {
         try
         {
            s_log.InfoFormat("Listing all bookmarks");

            // List to be returned.
            List<string> bookmarkNames = new List<string>();

            // Filename mask search parameter, wildcard plus our file extension.
            string bookmarkPath = GetBookmarkDirectory();
            string bookmarkMask = "*" + s_bookmarkExtension;

            // Search for the matching filenames.
            string[] foundFiles = Directory.GetFiles(bookmarkPath, bookmarkMask);

            if (foundFiles == null || foundFiles.Length == 0)
            {
               s_log.InfoFormat("No bookmarks found in: {0}", bookmarkPath);
               return null;
            }

            // Use the filenames rather than the serialized name within the file.
            // The user could rename the files, of course, and that's okay since all we have
            // to do is assume that a "connect via bookmark" command is specifying the bookmark
            // filename.
            foreach (string fullPath in foundFiles)
               bookmarkNames.Add(Path.GetFileNameWithoutExtension(fullPath));

            return bookmarkNames;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error listing bookmark names: {0}", e.Message);
            return null;
         }
      }

      /// <summary>
      /// Gets the currently configured bookmark directory, which is defined as:
      /// %APPDATA%/Senesco/Bookmarks/
      /// </summary>
      /// <returns>The expanded (actual) directory path.</returns>
      private static string GetBookmarkDirectory()
      {
         string dir = Path.Combine("%APPDATA%", Path.Combine("Senesco", "Bookmarks"));
         dir  = Environment.ExpandEnvironmentVariables(dir);
         if (Directory.Exists(dir) == false)
            Directory.CreateDirectory(dir);
         return dir;
      }

      /// <summary>
      /// Retrieves the full path for the given bookmark name using the currently
      /// configured bookmark directory.  A warning is logged if the file does not
      /// exist.
      /// </summary>
      /// <param name="bookmarkName">The bookmark name, without extension.</param>
      /// <returns>The full path, including file extension.</returns>
      public static string GetBookmarkFullPath(string bookmarkName)
      {
         string filename = bookmarkName + s_bookmarkExtension;
         string builtPath = Path.Combine(GetBookmarkDirectory(), filename);
         if (File.Exists(builtPath) == false)
            s_log.WarnFormat("Bookmark file does not exist: {0}", builtPath);
         return builtPath;
      }

      #endregion

      #region Serialization Methods

      /// <summary>
      /// Uses [Serializable] attribute to serialize via BinaryFormatter.
      /// </summary>
      //private static void BinarySerialize(Server server, string fullPath)
      //{
      //   using (FileStream fs = new FileStream(fullPath, FileMode.Create))
      //   {
      //      BinaryFormatter serializer = new BinaryFormatter();
      //      serializer.Serialize(fs, server);
      //   }
      //}

      /// <summary>
      /// Uses [Serializable] attribute to deserialize via BinaryFormatter.
      /// </summary>
      //private static Server BinaryDeserialize(string fullPath)
      //{
      //   using (FileStream fs = new FileStream(fullPath, FileMode.Open))
      //   {
      //      BinaryFormatter serializer = new BinaryFormatter();
      //      return (Server)serializer.Deserialize(fs);
      //   }
      //}

      //private static void XmlSerialize(Server server, string fullPath)
      //{
      //   using (FileStream fs = new FileStream(fullPath, FileMode.Create))
      //   {
      //      SoapFormatter serializer = new SoapFormatter();
      //      serializer.Serialize(fs, server);
      //   }
      //}

      //private static Server XmlDeserialize(string fullPath)
      //{
      //   using (FileStream fs = new FileStream(fullPath, FileMode.Open))
      //   {
      //      SoapFormatter serializer = new SoapFormatter();
      //      return (Server)serializer.Deserialize(fs);
      //   }
      //}

      private static void CustomSerialize(Server server, string fullPath)
      {
         using (StreamWriter sw = new StreamWriter(fullPath, false))
         {
            //TODO: write generically via reflection
            sw.WriteLine(String.Format("ServerName={0}",        server.ServerName));
            sw.WriteLine(String.Format("Address={0}",           server.Address));
            sw.WriteLine(String.Format("LoginName={0}",         server.LoginName));
            sw.WriteLine(String.Format("Password={0}",   Encode(server.Password)));
            sw.WriteLine(String.Format("Nick={0}",              server.Nick));
            sw.WriteLine(String.Format("Icon={0}",              server.Icon));
         }
      }

      private static Server CustomDeserialize(string fullPath)
      {
         Server server = new Server();
         using (StreamReader sr = new StreamReader(fullPath))
         {
            //TODO: allow the lines in any order
            server.ServerName =      CopyAfterEquals(sr.ReadLine());
            server.Address =         CopyAfterEquals(sr.ReadLine());
            server.LoginName =       CopyAfterEquals(sr.ReadLine());
            server.Password = Decode(CopyAfterEquals(sr.ReadLine()));
            server.Nick =            CopyAfterEquals(sr.ReadLine());
            server.Icon =  int.Parse(CopyAfterEquals(sr.ReadLine()));
         }
         return server;
      }

      private static string CopyAfterEquals(string line)
      {
         return line.Remove(0, line.IndexOf('=') + 1);
      }

      private static string Encode(string password)
      {
         if (String.IsNullOrEmpty(password))
            return String.Empty;
         return Convert.ToBase64String(Encoding.Unicode.GetBytes(password));
      }

      private static string Decode(string password)
      {
         if (String.IsNullOrEmpty(password))
            return String.Empty;
         return Encoding.Unicode.GetString(Convert.FromBase64String(password));
      }

      #endregion
   }
}

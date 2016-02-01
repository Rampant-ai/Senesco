
using System;
using System.Text;

namespace Senesco.Console
{
   class Screen
   {
      // Each method in this class obtains a thread lock to prevent simultaneous
      // writes to the console.
      private object m_threadLock = new object();

      // This string builder is the user input buffer.
      private StringBuilder m_buffer = new StringBuilder();

      #region User Input

      public string HandleKey(ConsoleKeyInfo input)
      {
         lock (m_threadLock)
         {
            // Ignore blacklisted characters.
            if (IsBlacklisted(input))
               return null;

            switch (input.Key)
            {
               // The enter key submits the input buffer for processing.
               case ConsoleKey.Enter:
                  // Construct the final submitted text to return.
                  string text = m_buffer.ToString();

                  // Clear the buffer and the display.
                  Buffer_Clear(m_buffer);

                  // Return the final contents of the buffer.
                  return text;

               // The delete key removes one character from the end of the input buffer.
               case ConsoleKey.Backspace:
               case ConsoleKey.Delete: //TODO: handle forward-delete and all four arrow keys
                  Buffer_Backspace(m_buffer);
                  return null;

               // All other keys are appended to the input buffer.
               default:
                  Buffer_Append(m_buffer, input.KeyChar);
                  return null;
            }
         }
      }

      /// <summary>
      /// Helper to define which keys are not allowed as user entry.
      /// </summary>
      private bool IsBlacklisted(ConsoleKeyInfo input)
      {
         switch (input.Key)
         {
            case ConsoleKey.Tab:
            case ConsoleKey.RightArrow:
            case ConsoleKey.LeftArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
            case ConsoleKey.Insert:
            case ConsoleKey.PageDown:
            case ConsoleKey.PageUp:
            case ConsoleKey.Home:
            case ConsoleKey.End:
               return true;
         }

         return false;
      }

      #endregion

      #region Console Text Management

      public void Write(string text, params object[] args)
      {
         if (String.IsNullOrEmpty(text))
            return;

         lock (m_threadLock)
         {
            // Remove the currently displayed buffer text.
            Buffer_Erase(m_buffer.Length);

            // Display the new text.
            WriteRaw(text, args);

            // Redraw the text-entry buffer exactly as it was.
            Buffer_Print(m_buffer);
         }
      }

      public void WriteLine(string text, params object[] args)
      {
         if (String.IsNullOrEmpty(text))
            return;

         lock (m_threadLock)
         {
            // Remove the currently displayed buffer text.
            Buffer_Erase(m_buffer.Length);

            // Display the new text.
            WriteLineRaw(text, args);

            // Redraw the text-entry buffer exactly as it was.
            Buffer_Print(m_buffer);
         }
      }

      private void WriteRaw(string text, params object[] args)
      {
         lock (m_threadLock)
         {
            System.Console.Write(String.Format(text, args));
         }
      }

      private void WriteLineRaw(string text, params object[] args)
      {
         lock (m_threadLock)
         {
            System.Console.WriteLine(String.Format(text, args));
         }
      }

      #endregion

      #region Buffer Management

      private void Buffer_Append(StringBuilder buffer, char c)
      {
         lock (m_threadLock)
         {
            buffer.Append(c);
            WriteRaw("{0}", c);
         }
      }

      private void Buffer_Backspace(StringBuilder buffer)
      {
         lock (m_threadLock)
         {
            if (buffer.Length == 0)
               return;

            // Remove the currently displayed buffer text.
            Buffer_Erase(buffer.Length);

            // Remove one character from the buffer.
            buffer.Remove(buffer.Length - 1, 1);

            // Print the whole buffer from scratch.
            Buffer_Print(buffer);
         }
      }

      private void Buffer_Clear(StringBuilder buffer)
      {
         lock (m_threadLock)
         {
            // Remove the currently displayed buffer text.
            Buffer_Erase(buffer.Length);

            // Clear the buffer.
            buffer.Remove(0, buffer.Length);

            // Print the now-empty buffer (just for the prompt).
            Buffer_Print(buffer);
         }
      }

      private void Buffer_Erase(int count)
      {
         lock (m_threadLock)
         {
            // Delete one extra character for the prompt.
            for (int i = 0; i <= count; i++)
               WriteRaw("\b \b");
         }
      }

      private void Buffer_Print(StringBuilder buffer)
      {
         lock (m_threadLock)
         {
            WriteRaw(">{0}", buffer.ToString());
         }
      }

      #endregion
   }
}

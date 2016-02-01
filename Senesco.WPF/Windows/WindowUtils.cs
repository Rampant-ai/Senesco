using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using log4net;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows
{
   class WindowUtils
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(WindowUtils));

      /// <summary>
      /// For consistent configuration of parent/child windows.
      /// </summary>
      public static void ConfigureChildWindow(Window parent, Window child)
      {
         child.ShowInTaskbar = false;
         child.Owner = parent;
      }

      /// <summary>
      /// Default options for window positioning if the saved position cannot be restored.
      /// </summary>
      public enum DefaultPosition
      {
         GlobalDefault,
         CenterOnParent,
         RightOfParent
      };

      /// <summary>
      /// Overload which lets the caller specify the default positioning of the
      /// window if the saved position could not be restored.
      /// </summary>
      public static Status RestoreWindowPosition(Window window, Window parent, DefaultPosition position)
      {
         if (window == null)
            return Status.NoResult;

         // If this window has a saved position, use that.
         ISenescoWindow senescoWindow = window as ISenescoWindow;
         if (senescoWindow != null && RestoreWindowPosition(senescoWindow) == Status.Success)
            return Status.Success;

         // Otherwise move the window to the specified default.
         switch (position)
         {
            case DefaultPosition.GlobalDefault:
               PositionGlobalDefault(window);
               return Status.Success;
            case DefaultPosition.CenterOnParent:
               CenterChildOnParent(parent, window);
               return Status.Success;
            case DefaultPosition.RightOfParent:
               ChildRightSideOfParent(parent, window);
               return Status.Success;
         }
         return Status.NoResult;
      }

      /// <summary>
      /// Restores the last saved window position for this window.
      /// </summary>
      public static Status RestoreWindowPosition(ISenescoWindow window)
      {
         if (window == null)
            return Status.NoResult;

         try
         {
            window.RestoreWindowPosition();
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error restoring window position: {0}", e.Message);
            return Status.Failure;
         }
      }

      /// <summary>
      /// Saves the current position and size of the given window.
      /// </summary>
      public static Status SaveWindowPosition(ISenescoWindow window)
      {
         if (window == null)
            return Status.NoResult;

         try
         {
            window.SaveWindowPosition();
            ConfigSettings.UserSettings.Save();
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Error restoring window position: {0}", e.Message);
            return Status.Failure;
         }
      }

      /// <summary>
      /// Window alignment helper.
      /// </summary>
      /// <param name="window"></param>
      private static void PositionGlobalDefault(Window window)
      {
         window.Left = 50;
         window.Top = 50;
      }

      /// <summary>
      /// Window alignment helper.
      /// </summary>
      private static void CenterChildOnParent(Window parent, Window child)
      {
         if (parent == null || child == null)
            return;

         // Position centered over the userlist.
         child.Left = parent.Left + (parent.Width / 2) - (child.Width / 2);
         child.Top = parent.Top + (parent.Height / 2) - (child.Height / 2);
      }

      /// <summary>
      /// Window alignment helper.
      /// </summary>
      private static void ChildRightSideOfParent(Window parent, Window child)
      {
         if (parent == null || child == null)
            return;

         // Position just off to the right at the same height.
         child.Left = parent.Left + parent.Width + 1;
         child.Top = parent.Top;
      }

      /// <summary>
      /// Helper method to get a single flattened text string from a RichTextBox.
      /// </summary>
      public static string TextFromRichTextBox(RichTextBox rtb)
      {
         if (rtb.Document == null || rtb.Document.Blocks == null)
            return String.Empty;

         StringBuilder sb = new StringBuilder();
         foreach (Block block in rtb.Document.Blocks)
         {
            Paragraph paragraph = block as Paragraph;
            if (paragraph == null)
               continue;

            foreach (Inline inline in paragraph.Inlines)
            {
               Run run = inline as Run;
               if (run == null)
                  continue;

               sb.AppendLine(run.Text);
            }
         }

         return sb.ToString().Trim();
      }
   }
}

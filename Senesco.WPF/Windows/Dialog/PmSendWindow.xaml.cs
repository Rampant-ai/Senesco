using System;
using System.Windows;
using System.Windows.Input;
using Senesco.Client;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Dialog
{
   /// <summary>
   /// Interaction logic for PmSendWindow.xaml
   /// </summary>
   public partial class PmSendWindow : Window, ISenescoWindow
   {
      private SenescoController m_controller;
      private User m_targetUser;

      public PmSendWindow(Window owner, SenescoController controller, User targetUser, string replyText)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         m_controller = controller;
         m_targetUser = targetUser;

         if (m_targetUser != null)
            m_recipientLabel.Content = String.Format("Recipient: {0}", m_targetUser.Username);

         // Hide the quote and splitter grid rows if there is no text to quote.
         if (String.IsNullOrEmpty(replyText))
         {
            m_quoteRow.Height = new GridLength(0);
            m_splitterRow.Height = new GridLength(0);
         }
         else
         {
            // Otherwise put the reply text in its text box.
            m_replyTextBox.AppendText(replyText);
            // And slightly extend the window height.
            //this.Height *= 1.2;
         }
      }

      public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         WindowUtils.SaveWindowPosition(this);
      }

      public void Window_LocationChanged(object sender, EventArgs e)
      {
         WindowUtils.SaveWindowPosition(this);
      }

      public void SaveWindowPosition()
      {
         ConfigSettings.UserSettings.PmSendWindowLeft = this.Left;
         ConfigSettings.UserSettings.PmSendWindowTop = this.Top;
         ConfigSettings.UserSettings.PmSendWindowWidth = this.Width;
         ConfigSettings.UserSettings.PmSendWindowHeight = this.Height;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.PmSendWindowLeft;
         this.Top = ConfigSettings.UserSettings.PmSendWindowTop;
         this.Width = ConfigSettings.UserSettings.PmSendWindowWidth;
         this.Height = ConfigSettings.UserSettings.PmSendWindowHeight;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         // Set initial focus to the text box.
         if (m_pmText != null)
            m_pmText.Focus();
      }

      private void Window_KeyDown(object sender, KeyEventArgs e)
      {
         // Close the window immediately if Escape is pressed.
         if (Keyboard.IsKeyDown(Key.Escape))
            this.Close();
      }

      private void CancelButton_Click(object sender, RoutedEventArgs e)
      {
         // Close the window without doing anything.
         this.Close();
      }

      private void SendButton_Click(object sender, RoutedEventArgs e)
      {
         // Send the private message text entered in this window.
         SendPm_RichTextBox();

         // Close this window.
         this.Close();
      }

      private void SendPm_RichTextBox()
      {
         string message = WindowUtils.TextFromRichTextBox(m_pmText);

         if (m_controller != null && message.Length > 0)
            m_controller.SendPrivateMessage(m_targetUser, message);
      }

      /*
      private void SendPm_TextBox()
      {
         string message = m_pmText.Text;
         if (m_controller != null && message.Length > 0)
            m_controller.SendPrivateMessage(m_targetUser, message);
      }
      */
   }
}

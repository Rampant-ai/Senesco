using System;
using System.Windows;
using System.Windows.Input;
using Senesco.Client;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Dialog
{
   /// <summary>
   /// Interaction logic for PmReceiveWindow.xaml
   /// </summary>
   public partial class PmReceiveWindow : Window, ISenescoWindow
   {
      private Window m_owner;
      private SenescoController m_controller;
      private string m_sendingNick;
      private int m_sendingUserId;

      public PmReceiveWindow(Window owner, SenescoController controller, string sendingNick, int sendingUserId, string message)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         m_owner = owner;
         m_controller = controller;
         m_sendingNick = sendingNick;
         m_sendingUserId = sendingUserId;
         m_senderLabel.Content = String.Format("Sender: {0}", sendingNick);
         m_pmText.AppendText(message);
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
         ConfigSettings.UserSettings.PmReceiveWindowLeft = this.Left;
         ConfigSettings.UserSettings.PmReceiveWindowTop = this.Top;
         ConfigSettings.UserSettings.PmReceiveWindowWidth = this.Width;
         ConfigSettings.UserSettings.PmReceiveWindowHeight = this.Height;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.PmReceiveWindowLeft;
         this.Top = ConfigSettings.UserSettings.PmReceiveWindowTop;
         this.Width = ConfigSettings.UserSettings.PmReceiveWindowWidth;
         this.Height = ConfigSettings.UserSettings.PmReceiveWindowHeight;
      }

      private void Window_KeyDown(object sender, KeyEventArgs e)
      {
         // Close the window immediately if Escape is pressed.
         if (Keyboard.IsKeyDown(Key.Escape))
            this.Close();
      }

      private void DismissButton_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
      }

      private void ReplyButton_Click(object sender, RoutedEventArgs e)
      {
         User replyUser = new User();
         replyUser.UserId = m_sendingUserId;
         replyUser.Username = m_sendingNick;
         string replyText = WindowUtils.TextFromRichTextBox(m_pmText);
         PmSendWindow psw = new PmSendWindow(m_owner, m_controller, replyUser, replyText);
         psw.Show();
         this.Close();
      }
   }
}

using System.Windows.Forms;
using Senesco.Client;
using Senesco.Client.Utility;

namespace Senesco.Forms
{
   public partial class UserListForm : Form
   {
      private SenescoController m_controller;

      public UserListForm(SenescoController controller)
      {
         InitializeComponent();

         m_controller = controller;
      }

      public Status RefreshUserList()
      {
         // Get the list of users from the model.
         if (m_controller == null)
            return Status.Failure;

         // Set the data source to the list of users.
         m_userList.DataSource = m_controller.UserList;

         // Only enable the buttons and context menu if there are users.
         bool hasUsers = (m_userList.Items.Count > 0);
         m_sendPmButton.Enabled = hasUsers;
         m_getUserInfoButton.Enabled = hasUsers;
         //m_userList.ContextMenu.Enabled = hasUsers;

         return Status.Success;
      }

   }
}

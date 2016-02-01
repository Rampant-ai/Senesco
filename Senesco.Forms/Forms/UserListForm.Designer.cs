namespace Senesco.Forms
{
   partial class UserListForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserListForm));
         this.m_userList = new System.Windows.Forms.ListBox();
         this.m_sendPmButton = new System.Windows.Forms.Button();
         this.m_getUserInfoButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // m_userList
         // 
         this.m_userList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.m_userList.FormattingEnabled = true;
         this.m_userList.Location = new System.Drawing.Point(12, 49);
         this.m_userList.Name = "m_userList";
         this.m_userList.Size = new System.Drawing.Size(238, 264);
         this.m_userList.TabIndex = 0;
         // 
         // m_sendPmButton
         // 
         this.m_sendPmButton.Location = new System.Drawing.Point(12, 12);
         this.m_sendPmButton.Name = "m_sendPmButton";
         this.m_sendPmButton.Size = new System.Drawing.Size(75, 23);
         this.m_sendPmButton.TabIndex = 1;
         this.m_sendPmButton.Text = "Send PM";
         this.m_sendPmButton.UseVisualStyleBackColor = true;
         // 
         // m_getUserInfoButton
         // 
         this.m_getUserInfoButton.Location = new System.Drawing.Point(93, 12);
         this.m_getUserInfoButton.Name = "m_getUserInfoButton";
         this.m_getUserInfoButton.Size = new System.Drawing.Size(75, 23);
         this.m_getUserInfoButton.TabIndex = 2;
         this.m_getUserInfoButton.Text = "Get Info";
         this.m_getUserInfoButton.UseVisualStyleBackColor = true;
         // 
         // UserListForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(262, 325);
         this.Controls.Add(this.m_getUserInfoButton);
         this.Controls.Add(this.m_sendPmButton);
         this.Controls.Add(this.m_userList);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "UserListForm";
         this.Text = "Senesco: Users";
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ListBox m_userList;
      private System.Windows.Forms.Button m_sendPmButton;
      private System.Windows.Forms.Button m_getUserInfoButton;
   }
}
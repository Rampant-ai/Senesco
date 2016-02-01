namespace Senesco.Forms
{
   partial class ChatForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.m_connectRecent = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
         this.m_autoConnect = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
         this.m_disconnectMenu = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.configToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.userSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.soundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.chatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.openChatLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.revealChatLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.userListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.websiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
         this.m_chatEntry = new System.Windows.Forms.TextBox();
         this.m_chatView = new System.Windows.Forms.RichTextBox();
         this.m_progressBar = new System.Windows.Forms.ProgressBar();
         this.menuStrip1.SuspendLayout();
         this.tableLayoutPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.configToolStripMenuItem,
            this.chatToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(583, 24);
         this.menuStrip1.TabIndex = 0;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.m_connectRecent,
            this.m_autoConnect,
            this.m_disconnectMenu,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
         this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
         this.fileToolStripMenuItem.Text = "File";
         // 
         // connectToolStripMenuItem
         // 
         this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
         this.connectToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
         this.connectToolStripMenuItem.Text = "Connect...";
         this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
         // 
         // m_connectRecent
         // 
         this.m_connectRecent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
         this.m_connectRecent.Name = "m_connectRecent";
         this.m_connectRecent.Size = new System.Drawing.Size(165, 22);
         this.m_connectRecent.Text = "Connect To";
         // 
         // toolStripMenuItem2
         // 
         this.toolStripMenuItem2.Name = "toolStripMenuItem2";
         this.toolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
         this.toolStripMenuItem2.Text = "toolStripMenuItem2";
         // 
         // m_autoConnect
         // 
         this.m_autoConnect.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4});
         this.m_autoConnect.Name = "m_autoConnect";
         this.m_autoConnect.Size = new System.Drawing.Size(165, 22);
         this.m_autoConnect.Text = "Auto Connect To";
         // 
         // toolStripMenuItem4
         // 
         this.toolStripMenuItem4.Name = "toolStripMenuItem4";
         this.toolStripMenuItem4.Size = new System.Drawing.Size(180, 22);
         this.toolStripMenuItem4.Text = "toolStripMenuItem4";
         // 
         // m_disconnectMenu
         // 
         this.m_disconnectMenu.Name = "m_disconnectMenu";
         this.m_disconnectMenu.Size = new System.Drawing.Size(165, 22);
         this.m_disconnectMenu.Text = "Disconnect";
         this.m_disconnectMenu.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(162, 6);
         // 
         // exitToolStripMenuItem
         // 
         this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
         this.exitToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
         this.exitToolStripMenuItem.Text = "Exit";
         this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
         // 
         // configToolStripMenuItem
         // 
         this.configToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userSettingsToolStripMenuItem,
            this.soundsToolStripMenuItem});
         this.configToolStripMenuItem.Name = "configToolStripMenuItem";
         this.configToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
         this.configToolStripMenuItem.Text = "Config";
         // 
         // userSettingsToolStripMenuItem
         // 
         this.userSettingsToolStripMenuItem.Name = "userSettingsToolStripMenuItem";
         this.userSettingsToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
         this.userSettingsToolStripMenuItem.Text = "User Settings";
         this.userSettingsToolStripMenuItem.Click += new System.EventHandler(this.userSettingsToolStripMenuItem_Click);
         // 
         // soundsToolStripMenuItem
         // 
         this.soundsToolStripMenuItem.Name = "soundsToolStripMenuItem";
         this.soundsToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
         this.soundsToolStripMenuItem.Text = "Sounds";
         this.soundsToolStripMenuItem.Click += new System.EventHandler(this.soundsToolStripMenuItem_Click);
         // 
         // chatToolStripMenuItem
         // 
         this.chatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openChatLogToolStripMenuItem,
            this.revealChatLogToolStripMenuItem});
         this.chatToolStripMenuItem.Name = "chatToolStripMenuItem";
         this.chatToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
         this.chatToolStripMenuItem.Text = "Chat";
         // 
         // openChatLogToolStripMenuItem
         // 
         this.openChatLogToolStripMenuItem.Name = "openChatLogToolStripMenuItem";
         this.openChatLogToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
         this.openChatLogToolStripMenuItem.Text = "Open Chat Log";
         this.openChatLogToolStripMenuItem.Click += new System.EventHandler(this.openChatLogToolStripMenuItem_Click);
         // 
         // revealChatLogToolStripMenuItem
         // 
         this.revealChatLogToolStripMenuItem.Name = "revealChatLogToolStripMenuItem";
         this.revealChatLogToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
         this.revealChatLogToolStripMenuItem.Text = "Reveal Chat Logs";
         this.revealChatLogToolStripMenuItem.Click += new System.EventHandler(this.revealChatLogToolStripMenuItem_Click);
         // 
         // windowToolStripMenuItem
         // 
         this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userListToolStripMenuItem});
         this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
         this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
         this.windowToolStripMenuItem.Text = "Window";
         // 
         // userListToolStripMenuItem
         // 
         this.userListToolStripMenuItem.Name = "userListToolStripMenuItem";
         this.userListToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
         this.userListToolStripMenuItem.Text = "User List";
         this.userListToolStripMenuItem.Click += new System.EventHandler(this.userListToolStripMenuItem_Click);
         // 
         // helpToolStripMenuItem
         // 
         this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.websiteToolStripMenuItem});
         this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
         this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
         this.helpToolStripMenuItem.Text = "Help";
         // 
         // aboutToolStripMenuItem
         // 
         this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
         this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
         this.aboutToolStripMenuItem.Text = "About";
         this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
         // 
         // websiteToolStripMenuItem
         // 
         this.websiteToolStripMenuItem.Name = "websiteToolStripMenuItem";
         this.websiteToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
         this.websiteToolStripMenuItem.Text = "Website";
         this.websiteToolStripMenuItem.Click += new System.EventHandler(this.websiteToolStripMenuItem_Click);
         // 
         // tableLayoutPanel1
         // 
         this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.tableLayoutPanel1.ColumnCount = 1;
         this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
         this.tableLayoutPanel1.Controls.Add(this.m_chatEntry, 0, 1);
         this.tableLayoutPanel1.Controls.Add(this.m_chatView, 0, 0);
         this.tableLayoutPanel1.Controls.Add(this.m_progressBar, 0, 2);
         this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 28);
         this.tableLayoutPanel1.Name = "tableLayoutPanel1";
         this.tableLayoutPanel1.RowCount = 3;
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
         this.tableLayoutPanel1.Size = new System.Drawing.Size(558, 312);
         this.tableLayoutPanel1.TabIndex = 1;
         // 
         // m_chatEntry
         // 
         this.m_chatEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.m_chatEntry.Location = new System.Drawing.Point(3, 228);
         this.m_chatEntry.Multiline = true;
         this.m_chatEntry.Name = "m_chatEntry";
         this.m_chatEntry.Size = new System.Drawing.Size(555, 50);
         this.m_chatEntry.TabIndex = 0;
         this.m_chatEntry.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chatEntry_KeyDown);
         this.m_chatEntry.KeyUp += new System.Windows.Forms.KeyEventHandler(this.chatEntry_KeyUp);
         // 
         // m_chatView
         // 
         this.m_chatView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.m_chatView.Location = new System.Drawing.Point(3, 3);
         this.m_chatView.Name = "m_chatView";
         this.m_chatView.ReadOnly = true;
         this.m_chatView.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
         this.m_chatView.Size = new System.Drawing.Size(555, 219);
         this.m_chatView.TabIndex = 1;
         this.m_chatView.Text = "";
         // 
         // m_progressBar
         // 
         this.m_progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.m_progressBar.Location = new System.Drawing.Point(3, 284);
         this.m_progressBar.Name = "m_progressBar";
         this.m_progressBar.Size = new System.Drawing.Size(555, 25);
         this.m_progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
         this.m_progressBar.TabIndex = 2;
         // 
         // ChatForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(583, 352);
         this.Controls.Add(this.tableLayoutPanel1);
         this.Controls.Add(this.menuStrip1);
         this.DoubleBuffered = true;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "ChatForm";
         this.Text = "Senesco";
         this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChatForm_FormClosed);
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.tableLayoutPanel1.ResumeLayout(false);
         this.tableLayoutPanel1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem configToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem chatToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem openChatLogToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem revealChatLogToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem userListToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem websiteToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem m_connectRecent;
      private System.Windows.Forms.ToolStripMenuItem m_disconnectMenu;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem userSettingsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem soundsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
      private System.Windows.Forms.ToolStripMenuItem m_autoConnect;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
      private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
      private System.Windows.Forms.TextBox m_chatEntry;
      private System.Windows.Forms.RichTextBox m_chatView;
      private System.Windows.Forms.ProgressBar m_progressBar;
   }
}


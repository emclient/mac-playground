namespace MailClient.Protocols.Imap
{
	partial class ImapOptionsControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImapOptionsControl));
			this.group_Server_SpecialFolders = new MailClient.Common.UI.Controls.SeparatorBox();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.labelJunkNames = new System.Windows.Forms.Label();
			this.labelSentName = new System.Windows.Forms.Label();
			this.checkAutodetectFolderNames = new System.Windows.Forms.CheckBox();
			this.labelTrashName = new System.Windows.Forms.Label();
			this.textBoxTrashName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.labelDraftsName = new System.Windows.Forms.Label();
			this.textBoxSentName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.textBoxDraftsName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.textBoxJunkName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.group_Server_Offline = new MailClient.Common.UI.Controls.SeparatorBox();
			this.tableLayoutSynchronization = new System.Windows.Forms.TableLayoutPanel();
			this.checkOffline = new System.Windows.Forms.CheckBox();
			this.checkOfflineAttachments = new System.Windows.Forms.CheckBox();
			this.group_Server_Authentication = new MailClient.Common.UI.Controls.SeparatorBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label_LoginName = new System.Windows.Forms.Label();
			this.label_Password = new System.Windows.Forms.Label();
			this.text_Password = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.text_LoginName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.radio_UseIdentityCredentials = new System.Windows.Forms.RadioButton();
			this.radio_UseAccountCredentials = new System.Windows.Forms.RadioButton();
			this.group_Servers_OutgoingServer = new MailClient.Common.UI.Controls.SeparatorBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.text_Port = new MailClient.Common.UI.Controls.TextBoxEx.NumericTextBoxEx();
			this.combo_Security = new System.Windows.Forms.ComboBox();
			this.label_Security = new System.Windows.Forms.Label();
			this.label_Port = new System.Windows.Forms.Label();
			this.label_Host = new System.Windows.Forms.Label();
			this.text_Host = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.group_Server_SpecialFolders.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.group_Server_Offline.SuspendLayout();
			this.tableLayoutSynchronization.SuspendLayout();
			this.group_Server_Authentication.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.group_Servers_OutgoingServer.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// group_Server_SpecialFolders
			// 
			resources.ApplyResources(this.group_Server_SpecialFolders, "group_Server_SpecialFolders");
			this.group_Server_SpecialFolders.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_Server_SpecialFolders.Controls.Add(this.tableLayoutPanel3);
			this.group_Server_SpecialFolders.Name = "group_Server_SpecialFolders";
			this.group_Server_SpecialFolders.TabStop = false;
			// 
			// tableLayoutPanel3
			// 
			resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
			this.tableLayoutPanel3.Controls.Add(this.labelJunkNames, 0, 4);
			this.tableLayoutPanel3.Controls.Add(this.labelSentName, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.checkAutodetectFolderNames, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.labelTrashName, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.textBoxTrashName, 1, 3);
			this.tableLayoutPanel3.Controls.Add(this.labelDraftsName, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.textBoxSentName, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.textBoxDraftsName, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.textBoxJunkName, 1, 4);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			// 
			// labelJunkNames
			// 
			resources.ApplyResources(this.labelJunkNames, "labelJunkNames");
			this.labelJunkNames.Name = "labelJunkNames";
			// 
			// labelSentName
			// 
			resources.ApplyResources(this.labelSentName, "labelSentName");
			this.labelSentName.Name = "labelSentName";
			// 
			// checkAutodetectFolderNames
			// 
			resources.ApplyResources(this.checkAutodetectFolderNames, "checkAutodetectFolderNames");
			this.tableLayoutPanel3.SetColumnSpan(this.checkAutodetectFolderNames, 2);
			this.checkAutodetectFolderNames.Name = "checkAutodetectFolderNames";
			this.checkAutodetectFolderNames.UseVisualStyleBackColor = true;
			this.checkAutodetectFolderNames.CheckedChanged += new System.EventHandler(this.checkAutodetectFolderNames_CheckedChanged);
			// 
			// labelTrashName
			// 
			resources.ApplyResources(this.labelTrashName, "labelTrashName");
			this.labelTrashName.Name = "labelTrashName";
			// 
			// textBoxTrashName
			// 
			resources.ApplyResources(this.textBoxTrashName, "textBoxTrashName");
			this.textBoxTrashName.Name = "textBoxTrashName";
			// 
			// labelDraftsName
			// 
			resources.ApplyResources(this.labelDraftsName, "labelDraftsName");
			this.labelDraftsName.Name = "labelDraftsName";
			// 
			// textBoxSentName
			// 
			resources.ApplyResources(this.textBoxSentName, "textBoxSentName");
			this.textBoxSentName.Name = "textBoxSentName";
			// 
			// textBoxDraftsName
			// 
			resources.ApplyResources(this.textBoxDraftsName, "textBoxDraftsName");
			this.textBoxDraftsName.Name = "textBoxDraftsName";
			// 
			// textBoxJunkName
			// 
			resources.ApplyResources(this.textBoxJunkName, "textBoxJunkName");
			this.textBoxJunkName.Name = "textBoxJunkName";
			// 
			// group_Server_Offline
			// 
			resources.ApplyResources(this.group_Server_Offline, "group_Server_Offline");
			this.group_Server_Offline.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_Server_Offline.Controls.Add(this.tableLayoutSynchronization);
			this.group_Server_Offline.Name = "group_Server_Offline";
			this.group_Server_Offline.TabStop = false;
			// 
			// tableLayoutSynchronization
			// 
			resources.ApplyResources(this.tableLayoutSynchronization, "tableLayoutSynchronization");
			this.tableLayoutSynchronization.Controls.Add(this.checkOffline, 0, 0);
			this.tableLayoutSynchronization.Controls.Add(this.checkOfflineAttachments, 0, 1);
			this.tableLayoutSynchronization.Name = "tableLayoutSynchronization";
			// 
			// checkOffline
			// 
			resources.ApplyResources(this.checkOffline, "checkOffline");
			this.checkOffline.Name = "checkOffline";
			this.checkOffline.UseVisualStyleBackColor = true;
			this.checkOffline.CheckedChanged += new System.EventHandler(this.checkOffline_CheckedChanged);
			// 
			// checkOfflineAttachments
			// 
			resources.ApplyResources(this.checkOfflineAttachments, "checkOfflineAttachments");
			this.checkOfflineAttachments.Name = "checkOfflineAttachments";
			this.checkOfflineAttachments.UseVisualStyleBackColor = true;
			// 
			// group_Server_Authentication
			// 
			resources.ApplyResources(this.group_Server_Authentication, "group_Server_Authentication");
			this.group_Server_Authentication.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_Server_Authentication.Controls.Add(this.tableLayoutPanel2);
			this.group_Server_Authentication.Controls.Add(this.flowLayoutPanel1);
			this.group_Server_Authentication.Name = "group_Server_Authentication";
			this.group_Server_Authentication.TabStop = false;
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
			this.tableLayoutPanel2.Controls.Add(this.label_LoginName, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.label_Password, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.text_Password, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.text_LoginName, 1, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			// 
			// label_LoginName
			// 
			resources.ApplyResources(this.label_LoginName, "label_LoginName");
			this.label_LoginName.Name = "label_LoginName";
			// 
			// label_Password
			// 
			resources.ApplyResources(this.label_Password, "label_Password");
			this.label_Password.Name = "label_Password";
			// 
			// text_Password
			// 
			resources.ApplyResources(this.text_Password, "text_Password");
			this.text_Password.Name = "text_Password";
			// 
			// text_LoginName
			// 
			resources.ApplyResources(this.text_LoginName, "text_LoginName");
			this.text_LoginName.Name = "text_LoginName";
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			//this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
			resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
			this.flowLayoutPanel1.Controls.Add(this.radio_UseIdentityCredentials);
			this.flowLayoutPanel1.Controls.Add(this.radio_UseAccountCredentials);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			// 
			// radio_UseIdentityCredentials
			// 
			resources.ApplyResources(this.radio_UseIdentityCredentials, "radio_UseIdentityCredentials");
			this.flowLayoutPanel1.SetFlowBreak(this.radio_UseIdentityCredentials, true);
			this.radio_UseIdentityCredentials.Name = "radio_UseIdentityCredentials";
			this.radio_UseIdentityCredentials.TabStop = true;
			this.radio_UseIdentityCredentials.UseVisualStyleBackColor = true;
			this.radio_UseIdentityCredentials.CheckedChanged += new System.EventHandler(this.radio_Authentication_CheckChanged);
			// 
			// radio_UseAccountCredentials
			// 
			resources.ApplyResources(this.radio_UseAccountCredentials, "radio_UseAccountCredentials");
			this.flowLayoutPanel1.SetFlowBreak(this.radio_UseAccountCredentials, true);
			this.radio_UseAccountCredentials.Name = "radio_UseAccountCredentials";
			this.radio_UseAccountCredentials.TabStop = true;
			this.radio_UseAccountCredentials.UseVisualStyleBackColor = true;
			this.radio_UseAccountCredentials.CheckedChanged += new System.EventHandler(this.radio_Authentication_CheckChanged);
			// 
			// group_Servers_OutgoingServer
			// 
			resources.ApplyResources(this.group_Servers_OutgoingServer, "group_Servers_OutgoingServer");
			this.group_Servers_OutgoingServer.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_Servers_OutgoingServer.Controls.Add(this.tableLayoutPanel1);
			this.group_Servers_OutgoingServer.Name = "group_Servers_OutgoingServer";
			this.group_Servers_OutgoingServer.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.text_Port, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.combo_Security, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label_Security, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label_Port, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label_Host, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.text_Host, 1, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// text_Port
			// 
			resources.ApplyResources(this.text_Port, "text_Port");
			this.text_Port.Name = "text_Port";
			// 
			// combo_Security
			// 
			resources.ApplyResources(this.combo_Security, "combo_Security");
			this.combo_Security.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.combo_Security.FormattingEnabled = true;
			this.combo_Security.Items.AddRange(new object[] {
            resources.GetString("combo_Security.Items"),
            resources.GetString("combo_Security.Items1"),
            resources.GetString("combo_Security.Items2"),
            resources.GetString("combo_Security.Items3")});
			this.combo_Security.Name = "combo_Security";
			this.combo_Security.SelectedIndexChanged += new System.EventHandler(this.combo_Security_SelectedIndexChanged);
			// 
			// label_Security
			// 
			resources.ApplyResources(this.label_Security, "label_Security");
			this.label_Security.Name = "label_Security";
			// 
			// label_Port
			// 
			resources.ApplyResources(this.label_Port, "label_Port");
			this.label_Port.Name = "label_Port";
			// 
			// label_Host
			// 
			resources.ApplyResources(this.label_Host, "label_Host");
			this.label_Host.Name = "label_Host";
			// 
			// text_Host
			// 
			resources.ApplyResources(this.text_Host, "text_Host");
			this.text_Host.Name = "text_Host";
			// 
			// ImapOptionsControl
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.group_Server_SpecialFolders);
			this.Controls.Add(this.group_Server_Offline);
			this.Controls.Add(this.group_Server_Authentication);
			this.Controls.Add(this.group_Servers_OutgoingServer);
			this.Name = "ImapOptionsControl";
			this.group_Server_SpecialFolders.ResumeLayout(false);
			this.group_Server_SpecialFolders.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.group_Server_Offline.ResumeLayout(false);
			this.group_Server_Offline.PerformLayout();
			this.tableLayoutSynchronization.ResumeLayout(false);
			this.tableLayoutSynchronization.PerformLayout();
			this.group_Server_Authentication.ResumeLayout(false);
			this.group_Server_Authentication.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.group_Servers_OutgoingServer.ResumeLayout(false);
			this.group_Servers_OutgoingServer.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MailClient.Common.UI.Controls.SeparatorBox group_Servers_OutgoingServer;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private MailClient.Common.UI.Controls.TextBoxEx.NumericTextBoxEx text_Port;
		private System.Windows.Forms.ComboBox combo_Security;
		private System.Windows.Forms.Label label_Security;
		private System.Windows.Forms.Label label_Port;
		private System.Windows.Forms.Label label_Host;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_Host;
		private MailClient.Common.UI.Controls.SeparatorBox group_Server_Authentication;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label label_LoginName;
		private System.Windows.Forms.Label label_Password;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_Password;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_LoginName;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton radio_UseIdentityCredentials;
		private System.Windows.Forms.RadioButton radio_UseAccountCredentials;
		private MailClient.Common.UI.Controls.SeparatorBox group_Server_Offline;
		private System.Windows.Forms.TableLayoutPanel tableLayoutSynchronization;
		private System.Windows.Forms.CheckBox checkOffline;
		private System.Windows.Forms.CheckBox checkOfflineAttachments;
		private MailClient.Common.UI.Controls.SeparatorBox group_Server_SpecialFolders;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.CheckBox checkAutodetectFolderNames;
		private System.Windows.Forms.Label labelTrashName;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx textBoxTrashName;
		private System.Windows.Forms.Label labelJunkNames;
		private System.Windows.Forms.Label labelSentName;
		private System.Windows.Forms.Label labelDraftsName;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx textBoxSentName;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx textBoxDraftsName;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx textBoxJunkName;
	}
}

namespace MailClient.UI.Controls
{
	public class ControlContactTextBox : System.Windows.Forms.TextBox { }
}

namespace MailClient.Accounts.Mail
{
	partial class MailOptionsControl
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MailOptionsControl));
			this.panel_General = new MailClient.Common.UI.Controls.ScrollPanel();
			this.check_IncludeInGlobalOperations = new System.Windows.Forms.CheckBox();
			this.group_Services = new MailClient.Common.UI.Controls.SeparatorBox();
			this.panel_Services_TopMarginPanel = new System.Windows.Forms.Panel();
			//this.dataGrid_Services = new MailClient.UI.Controls.ControlDataGrid.ControlDataGrid();
			this.group_AutoBCC = new MailClient.Common.UI.Controls.SeparatorBox();
			this.table_General_AutoBCC = new System.Windows.Forms.TableLayoutPanel();
			this.label_AutoBCC = new System.Windows.Forms.Label();
			this.text_AutoBCC = new MailClient.UI.Controls.ControlContactTextBox();
			this.group_General_Scheduling = new MailClient.Common.UI.Controls.SeparatorBox();
			this.table_General_DefaultFolders = new System.Windows.Forms.TableLayoutPanel();
			this.text_General_CalendarFolder = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.text_General_ContactFolder = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.button_General_SelectCalendarFolder = new System.Windows.Forms.Button();
			this.label_General_ContactFolder = new System.Windows.Forms.Label();
			this.label_General_CalendarFolder = new System.Windows.Forms.Label();
			this.label_General_TaskFolder = new System.Windows.Forms.Label();
			this.button_General_SelectTaskFolder = new System.Windows.Forms.Button();
			this.button_General_SelectContactFolder = new System.Windows.Forms.Button();
			this.text_General_TaskFolder = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.group_Server_Authentication = new MailClient.Common.UI.Controls.SeparatorBox();
			this.table_General_Authentication = new System.Windows.Forms.TableLayoutPanel();
			this.label_LoginName = new System.Windows.Forms.Label();
			this.label_Password = new System.Windows.Forms.Label();
			this.text_Password = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.text_LoginName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.radio_UseSSO = new System.Windows.Forms.RadioButton();
			this.radio_UseAccountCredentials = new System.Windows.Forms.RadioButton();
			this.group_General_UserInfo = new MailClient.Common.UI.Controls.SeparatorBox();
			this.table_General_UserInformation = new System.Windows.Forms.TableLayoutPanel();
			this.label_General_Name = new System.Windows.Forms.Label();
			this.text_Name = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.label_General_Email = new System.Windows.Forms.Label();
			this.text_Email = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.button_General_Aliases = new System.Windows.Forms.Button();
			this.table_IdentityName = new System.Windows.Forms.TableLayoutPanel();
			this.label_General_AccountName = new System.Windows.Forms.Label();
			this.text_IdentityName = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.controlPanelTabSwitcher1 = new MailClient.Common.UI.Controls.ControlPanelSwitcher.ControlPanelTabSwitcher();
			this.tab_General = new MailClient.Common.UI.Controls.ControlPanelSwitcher.SwitchPanel();
			this.tab_Security = new MailClient.Common.UI.Controls.ControlPanelSwitcher.SwitchPanel();
			this.checkEncryptMessages = new System.Windows.Forms.CheckBox();
			this.checkSignMessages = new System.Windows.Forms.CheckBox();
			this.checkIncludeCertificates = new System.Windows.Forms.CheckBox();
			this.panel_General.SuspendLayout();
			this.group_Services.SuspendLayout();
			this.group_AutoBCC.SuspendLayout();
			this.table_General_AutoBCC.SuspendLayout();
			this.group_General_Scheduling.SuspendLayout();
			this.table_General_DefaultFolders.SuspendLayout();
			this.group_Server_Authentication.SuspendLayout();
			this.table_General_Authentication.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.group_General_UserInfo.SuspendLayout();
			this.table_General_UserInformation.SuspendLayout();
			this.table_IdentityName.SuspendLayout();
			this.controlPanelTabSwitcher1.SuspendLayout();
			this.tab_General.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel_General
			// 
			resources.ApplyResources(this.panel_General, "panel_General");
			this.panel_General.Controls.Add(this.check_IncludeInGlobalOperations);
			this.panel_General.Controls.Add(this.group_Services);
			this.panel_General.Controls.Add(this.group_AutoBCC);
			this.panel_General.Controls.Add(this.group_General_Scheduling);
			this.panel_General.Controls.Add(this.group_Server_Authentication);
			this.panel_General.Controls.Add(this.group_General_UserInfo);
			this.panel_General.Controls.Add(this.table_IdentityName);
			this.panel_General.Name = "panel_General";
			// 
			// check_IncludeInGlobalOperations
			// 
			resources.ApplyResources(this.check_IncludeInGlobalOperations, "check_IncludeInGlobalOperations");
			this.check_IncludeInGlobalOperations.Name = "check_IncludeInGlobalOperations";
			this.check_IncludeInGlobalOperations.UseVisualStyleBackColor = true;
			// 
			// group_Services
			// 
			resources.ApplyResources(this.group_Services, "group_Services");
			this.group_Services.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_Services.Controls.Add(this.panel_Services_TopMarginPanel);
			//this.group_Services.Controls.Add(this.dataGrid_Services);
			this.group_Services.Name = "group_Services";
			this.group_Services.TabStop = false;
			// 
			// panel_Services_TopMarginPanel
			// 
			this.panel_Services_TopMarginPanel.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.panel_Services_TopMarginPanel, "panel_Services_TopMarginPanel");
			this.panel_Services_TopMarginPanel.Name = "panel_Services_TopMarginPanel";
			// 
			// dataGrid_Services
			// 
			//this.dataGrid_Services.AutoHeight = true;
			//this.dataGrid_Services.AutomaticallyResizeColumnsToFit = true;
			//resources.ApplyResources(this.dataGrid_Services, "dataGrid_Services");
			//this.dataGrid_Services.MaxAutoHeightRows = 10;
			//this.dataGrid_Services.Name = "dataGrid_Services";
			//this.dataGrid_Services.RowHeight = 21;
			//this.dataGrid_Services.ShowColumnHeader = false;
			//this.dataGrid_Services.CellClicked += new System.EventHandler<MailClient.UI.Controls.ControlDataGrid.DataGridCellClickedEventArgs>(this.dataGrid_Services_CellClicked);
			//this.dataGrid_Services.CellDoubleClicked += new System.EventHandler<MailClient.UI.Controls.ControlDataGrid.DataGridCellClickedEventArgs>(this.dataGrid_Services_CellDoubleClicked);
			//this.dataGrid_Services.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGrid_Services_KeyDown);
			// 
			// group_AutoBCC
			// 
			resources.ApplyResources(this.group_AutoBCC, "group_AutoBCC");
			this.group_AutoBCC.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_AutoBCC.Controls.Add(this.table_General_AutoBCC);
			this.group_AutoBCC.Name = "group_AutoBCC";
			this.group_AutoBCC.TabStop = false;
			// 
			// table_General_AutoBCC
			// 
			resources.ApplyResources(this.table_General_AutoBCC, "table_General_AutoBCC");
			this.table_General_AutoBCC.Controls.Add(this.label_AutoBCC, 0, 0);
			this.table_General_AutoBCC.Controls.Add(this.text_AutoBCC, 1, 0);
			this.table_General_AutoBCC.Name = "table_General_AutoBCC";
			// 
			// label_AutoBCC
			// 
			resources.ApplyResources(this.label_AutoBCC, "label_AutoBCC");
			this.label_AutoBCC.Name = "label_AutoBCC";
			// 
			// text_AutoBCC
			// 
			resources.ApplyResources(this.text_AutoBCC, "text_AutoBCC");
			//this.text_AutoBCC.MaxEntries = 10;
			//this.text_AutoBCC.Name = "text_AutoBCC";
			//this.text_AutoBCC.TextPadding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			// 
			// group_General_Scheduling
			// 
			resources.ApplyResources(this.group_General_Scheduling, "group_General_Scheduling");
			this.group_General_Scheduling.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_General_Scheduling.Controls.Add(this.table_General_DefaultFolders);
			this.group_General_Scheduling.Name = "group_General_Scheduling";
			this.group_General_Scheduling.TabStop = false;
			// 
			// table_General_DefaultFolders
			// 
			resources.ApplyResources(this.table_General_DefaultFolders, "table_General_DefaultFolders");
			this.table_General_DefaultFolders.Controls.Add(this.text_General_CalendarFolder, 1, 0);
			this.table_General_DefaultFolders.Controls.Add(this.text_General_ContactFolder, 1, 2);
			this.table_General_DefaultFolders.Controls.Add(this.button_General_SelectCalendarFolder, 2, 0);
			this.table_General_DefaultFolders.Controls.Add(this.label_General_ContactFolder, 0, 2);
			this.table_General_DefaultFolders.Controls.Add(this.label_General_CalendarFolder, 0, 0);
			this.table_General_DefaultFolders.Controls.Add(this.label_General_TaskFolder, 0, 1);
			this.table_General_DefaultFolders.Controls.Add(this.button_General_SelectTaskFolder, 2, 1);
			this.table_General_DefaultFolders.Controls.Add(this.button_General_SelectContactFolder, 2, 2);
			this.table_General_DefaultFolders.Controls.Add(this.text_General_TaskFolder, 1, 1);
			this.table_General_DefaultFolders.Name = "table_General_DefaultFolders";
			// 
			// text_General_CalendarFolder
			// 
			resources.ApplyResources(this.text_General_CalendarFolder, "text_General_CalendarFolder");
			this.text_General_CalendarFolder.Name = "text_General_CalendarFolder";
			this.text_General_CalendarFolder.ReadOnly = true;
			// 
			// text_General_ContactFolder
			// 
			resources.ApplyResources(this.text_General_ContactFolder, "text_General_ContactFolder");
			this.text_General_ContactFolder.Name = "text_General_ContactFolder";
			this.text_General_ContactFolder.ReadOnly = true;
			// 
			// button_General_SelectCalendarFolder
			// 
			resources.ApplyResources(this.button_General_SelectCalendarFolder, "button_General_SelectCalendarFolder");
			this.button_General_SelectCalendarFolder.Name = "button_General_SelectCalendarFolder";
			this.button_General_SelectCalendarFolder.UseVisualStyleBackColor = true;
			this.button_General_SelectCalendarFolder.Click += new System.EventHandler(this.button_General_SelectCalendarFolder_Click);
			// 
			// label_General_ContactFolder
			// 
			resources.ApplyResources(this.label_General_ContactFolder, "label_General_ContactFolder");
			this.label_General_ContactFolder.Name = "label_General_ContactFolder";
			// 
			// label_General_CalendarFolder
			// 
			resources.ApplyResources(this.label_General_CalendarFolder, "label_General_CalendarFolder");
			this.label_General_CalendarFolder.Name = "label_General_CalendarFolder";
			// 
			// label_General_TaskFolder
			// 
			resources.ApplyResources(this.label_General_TaskFolder, "label_General_TaskFolder");
			this.label_General_TaskFolder.Name = "label_General_TaskFolder";
			// 
			// button_General_SelectTaskFolder
			// 
			resources.ApplyResources(this.button_General_SelectTaskFolder, "button_General_SelectTaskFolder");
			this.button_General_SelectTaskFolder.Name = "button_General_SelectTaskFolder";
			this.button_General_SelectTaskFolder.UseVisualStyleBackColor = true;
			this.button_General_SelectTaskFolder.Click += new System.EventHandler(this.button_General_SelectTaskFolder_Click);
			// 
			// button_General_SelectContactFolder
			// 
			resources.ApplyResources(this.button_General_SelectContactFolder, "button_General_SelectContactFolder");
			this.button_General_SelectContactFolder.Name = "button_General_SelectContactFolder";
			this.button_General_SelectContactFolder.UseVisualStyleBackColor = true;
			this.button_General_SelectContactFolder.Click += new System.EventHandler(this.button_General_SelectContactFolder_Click);
			// 
			// text_General_TaskFolder
			// 
			resources.ApplyResources(this.text_General_TaskFolder, "text_General_TaskFolder");
			this.text_General_TaskFolder.Name = "text_General_TaskFolder";
			this.text_General_TaskFolder.ReadOnly = true;
			// 
			// group_Server_Authentication
			// 
			resources.ApplyResources(this.group_Server_Authentication, "group_Server_Authentication");
			this.group_Server_Authentication.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_Server_Authentication.Controls.Add(this.table_General_Authentication);
			this.group_Server_Authentication.Controls.Add(this.flowLayoutPanel1);
			this.group_Server_Authentication.Name = "group_Server_Authentication";
			this.group_Server_Authentication.TabStop = false;
			// 
			// table_General_Authentication
			// 
			resources.ApplyResources(this.table_General_Authentication, "table_General_Authentication");
			this.table_General_Authentication.Controls.Add(this.label_LoginName, 0, 0);
			this.table_General_Authentication.Controls.Add(this.label_Password, 0, 1);
			this.table_General_Authentication.Controls.Add(this.text_Password, 1, 1);
			this.table_General_Authentication.Controls.Add(this.text_LoginName, 1, 0);
			this.table_General_Authentication.Name = "table_General_Authentication";
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
			resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
			this.flowLayoutPanel1.Controls.Add(this.radio_UseSSO);
			this.flowLayoutPanel1.Controls.Add(this.radio_UseAccountCredentials);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			// 
			// radio_UseSSO
			// 
			resources.ApplyResources(this.radio_UseSSO, "radio_UseSSO");
			this.flowLayoutPanel1.SetFlowBreak(this.radio_UseSSO, true);
			this.radio_UseSSO.Name = "radio_UseSSO";
			this.radio_UseSSO.TabStop = true;
			this.radio_UseSSO.UseVisualStyleBackColor = true;
			this.radio_UseSSO.CheckedChanged += new System.EventHandler(this.radio_Authentication_CheckChanged);
			// 
			// radio_UseAccountCredentials
			// 
			resources.ApplyResources(this.radio_UseAccountCredentials, "radio_UseAccountCredentials");
			this.flowLayoutPanel1.SetFlowBreak(this.radio_UseAccountCredentials, true);
			this.radio_UseAccountCredentials.Name = "radio_UseAccountCredentials";
			this.radio_UseAccountCredentials.TabStop = true;
			this.radio_UseAccountCredentials.UseVisualStyleBackColor = true;
			// 
			// group_General_UserInfo
			// 
			resources.ApplyResources(this.group_General_UserInfo, "group_General_UserInfo");
			this.group_General_UserInfo.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
			this.group_General_UserInfo.Controls.Add(this.table_General_UserInformation);
			this.group_General_UserInfo.Name = "group_General_UserInfo";
			this.group_General_UserInfo.TabStop = false;
			// 
			// table_General_UserInformation
			// 
			resources.ApplyResources(this.table_General_UserInformation, "table_General_UserInformation");
			this.table_General_UserInformation.Controls.Add(this.label_General_Name, 0, 0);
			this.table_General_UserInformation.Controls.Add(this.text_Name, 1, 0);
			this.table_General_UserInformation.Controls.Add(this.label_General_Email, 0, 1);
			this.table_General_UserInformation.Controls.Add(this.text_Email, 1, 1);
			this.table_General_UserInformation.Controls.Add(this.button_General_Aliases, 2, 1);
			this.table_General_UserInformation.Name = "table_General_UserInformation";
			// 
			// label_General_Name
			// 
			resources.ApplyResources(this.label_General_Name, "label_General_Name");
			this.label_General_Name.Name = "label_General_Name";
			// 
			// text_Name
			// 
			resources.ApplyResources(this.text_Name, "text_Name");
			this.table_General_UserInformation.SetColumnSpan(this.text_Name, 2);
			this.text_Name.Name = "text_Name";
			// 
			// label_General_Email
			// 
			resources.ApplyResources(this.label_General_Email, "label_General_Email");
			this.label_General_Email.Name = "label_General_Email";
			// 
			// text_Email
			// 
			resources.ApplyResources(this.text_Email, "text_Email");
			this.text_Email.Name = "text_Email";
			// 
			// button_General_Aliases
			// 
			resources.ApplyResources(this.button_General_Aliases, "button_General_Aliases");
			this.button_General_Aliases.Name = "button_General_Aliases";
			this.button_General_Aliases.UseVisualStyleBackColor = true;
			this.button_General_Aliases.Click += new System.EventHandler(this.button_General_Aliases_Click);
			// 
			// table_IdentityName
			// 
			resources.ApplyResources(this.table_IdentityName, "table_IdentityName");
			this.table_IdentityName.Controls.Add(this.label_General_AccountName, 0, 0);
			this.table_IdentityName.Controls.Add(this.text_IdentityName, 1, 0);
			this.table_IdentityName.Name = "table_IdentityName";
			// 
			// label_General_AccountName
			// 
			resources.ApplyResources(this.label_General_AccountName, "label_General_AccountName");
			this.label_General_AccountName.Name = "label_General_AccountName";
			// 
			// text_IdentityName
			// 
			resources.ApplyResources(this.text_IdentityName, "text_IdentityName");
			this.text_IdentityName.Name = "text_IdentityName";
			// 
			// controlPanelTabSwitcher1
			// 
			this.controlPanelTabSwitcher1.BorderStyle = ((MailClient.Common.UI.Controls.ControlPanelSwitcher.BorderStyle)((((MailClient.Common.UI.Controls.ControlPanelSwitcher.BorderStyle.Left | MailClient.Common.UI.Controls.ControlPanelSwitcher.BorderStyle.Right)
			| MailClient.Common.UI.Controls.ControlPanelSwitcher.BorderStyle.Top)
			| MailClient.Common.UI.Controls.ControlPanelSwitcher.BorderStyle.Bottom)));
			this.controlPanelTabSwitcher1.Controls.Add(this.tab_General);
			this.controlPanelTabSwitcher1.Controls.Add(this.tab_Security);
			resources.ApplyResources(this.controlPanelTabSwitcher1, "controlPanelTabSwitcher1");
			this.controlPanelTabSwitcher1.Name = "controlPanelTabSwitcher1";
			this.controlPanelTabSwitcher1.TabDisplayStyle = MailClient.Common.UI.Controls.ControlPanelSwitcher.TabDisplayStyle.Text;
			this.controlPanelTabSwitcher1.TabPadding = new System.Windows.Forms.Padding(12, 8, 12, 8);
			this.controlPanelTabSwitcher1.TabSize = new System.Drawing.Size(0, 0);
			this.controlPanelTabSwitcher1.BeforePanelActivated += new System.EventHandler<MailClient.Common.UI.Controls.ControlPanelSwitcher.PanelActivatingEventArgs>(this.ControlPanelTabSwitcher1_BeforePanelActivated);
			// 
			// tab_General
			// 
			this.tab_General.Controls.Add(this.panel_General);
			resources.ApplyResources(this.tab_General, "tab_General");
			this.tab_General.Name = "tab_General";
			// 
			// MailOptionsControl
			// 
			this.Controls.Add(this.controlPanelTabSwitcher1);
			this.Name = "MailOptionsControl";
			resources.ApplyResources(this, "$this");
			this.panel_General.ResumeLayout(false);
			this.panel_General.PerformLayout();
			this.group_Services.ResumeLayout(false);
			this.group_AutoBCC.ResumeLayout(false);
			this.group_AutoBCC.PerformLayout();
			this.table_General_AutoBCC.ResumeLayout(false);
			this.table_General_AutoBCC.PerformLayout();
			this.group_General_Scheduling.ResumeLayout(false);
			this.group_General_Scheduling.PerformLayout();
			this.table_General_DefaultFolders.ResumeLayout(false);
			this.table_General_DefaultFolders.PerformLayout();
			this.group_Server_Authentication.ResumeLayout(false);
			this.group_Server_Authentication.PerformLayout();
			this.table_General_Authentication.ResumeLayout(false);
			this.table_General_Authentication.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.group_General_UserInfo.ResumeLayout(false);
			this.group_General_UserInfo.PerformLayout();
			this.table_General_UserInformation.ResumeLayout(false);
			this.table_General_UserInformation.PerformLayout();
			this.table_IdentityName.ResumeLayout(false);
			this.table_IdentityName.PerformLayout();
			this.controlPanelTabSwitcher1.ResumeLayout(false);
			this.tab_General.ResumeLayout(false);
			this.tab_General.PerformLayout();
			this.tab_Security.ResumeLayout(false);
			this.tab_Security.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private Common.UI.Controls.ScrollPanel panel_General;
		private MailClient.Common.UI.Controls.SeparatorBox group_Services;
		//private UI.Controls.ControlDataGrid.ControlDataGrid dataGrid_Services;
		private MailClient.Common.UI.Controls.SeparatorBox group_AutoBCC;
		private System.Windows.Forms.TableLayoutPanel table_General_AutoBCC;
		private System.Windows.Forms.Label label_AutoBCC;
		private UI.Controls.ControlContactTextBox text_AutoBCC;
		private MailClient.Common.UI.Controls.SeparatorBox group_General_Scheduling;
		private System.Windows.Forms.TableLayoutPanel table_General_DefaultFolders;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_General_CalendarFolder;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_General_ContactFolder;
		private System.Windows.Forms.Button button_General_SelectCalendarFolder;
		private System.Windows.Forms.Label label_General_ContactFolder;
		private System.Windows.Forms.Label label_General_CalendarFolder;
		private System.Windows.Forms.Label label_General_TaskFolder;
		private System.Windows.Forms.Button button_General_SelectTaskFolder;
		private System.Windows.Forms.Button button_General_SelectContactFolder;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_General_TaskFolder;
		private MailClient.Common.UI.Controls.SeparatorBox group_Server_Authentication;
		private System.Windows.Forms.TableLayoutPanel table_General_Authentication;
		private System.Windows.Forms.Label label_LoginName;
		private System.Windows.Forms.Label label_Password;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_Password;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_LoginName;
		private System.Windows.Forms.CheckBox check_IncludeInGlobalOperations;
		private MailClient.Common.UI.Controls.SeparatorBox group_General_UserInfo;
		private System.Windows.Forms.TableLayoutPanel table_General_UserInformation;
		private System.Windows.Forms.Label label_General_Name;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_Name;
		private System.Windows.Forms.Label label_General_Email;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_Email;
		private System.Windows.Forms.Button button_General_Aliases;
		private System.Windows.Forms.TableLayoutPanel table_IdentityName;
		private System.Windows.Forms.Label label_General_AccountName;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx text_IdentityName;
		private Common.UI.Controls.ControlPanelSwitcher.ControlPanelTabSwitcher controlPanelTabSwitcher1;
		private Common.UI.Controls.ControlPanelSwitcher.SwitchPanel tab_General;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton radio_UseSSO;
		private System.Windows.Forms.RadioButton radio_UseAccountCredentials;
		private Common.UI.Controls.ControlPanelSwitcher.SwitchPanel tab_Security;
		private System.Windows.Forms.CheckBox checkEncryptMessages;
		private System.Windows.Forms.CheckBox checkSignMessages;
		private System.Windows.Forms.CheckBox checkIncludeCertificates;
		private System.Windows.Forms.Panel panel_Services_TopMarginPanel;
	}
}

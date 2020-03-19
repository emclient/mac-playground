//using MailClient.Common.UI.Controls;
//using MailClient.UI.Controls;

namespace FormsTest
{
    partial class LayoutForm
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
            this.tableLayoutPanel20 = new System.Windows.Forms.TableLayoutPanel();
            this.group_General_DefaultApp = new MailClient.Common.UI.Controls.SeparatorBox();
            this.button_General_MakeDefault = new System.Windows.Forms.Button();
            this.label_General_ApplicationIsHandler = new System.Windows.Forms.Label();
            this.group_General_SendReceive = new MailClient.Common.UI.Controls.SeparatorBox();
            this.flowLayoutPanel_General_CheckForNew = new System.Windows.Forms.FlowLayoutPanel();
            this.check_General_CheckMessages = new MailClient.UI.Controls.WrappingCheckBox();
            this.numeric_General_CheckInterval = new System.Windows.Forms.NumericUpDown();
            this.label_General_Minutes = new System.Windows.Forms.Label();
            this.check_General_SendAndReceiveAtStartup = new MailClient.UI.Controls.WrappingCheckBox();
            this.group_General_General = new MailClient.Common.UI.Controls.SeparatorBox();
            this.flowLayoutPanel23 = new System.Windows.Forms.FlowLayoutPanel();
            this.check_General_MinimizeToTray = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_CloseToTray = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_RunOnStartup = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_EmptyTrashOnExit = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_ShowGlobalFolders = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_ShowLocalFolders = new MailClient.UI.Controls.WrappingCheckBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.combo_General_AfterStart = new MailClient.Common.UI.Controls.ComboBoxEx();
            this.label_General_AfterStart = new System.Windows.Forms.Label();
            this.button_General_AfterStartFolderBrowse = new System.Windows.Forms.Button();
            this.group_General_ActivityWindow = new MailClient.Common.UI.Controls.SeparatorBox();
            this.check_General_ShowWindowWhenErrorOccurs = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_HideWindowAfterSendAndReceive = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_General_ShowWindowOnSendAndReceive = new MailClient.UI.Controls.WrappingCheckBox();
            this.tableLayoutPanel20.SuspendLayout();
            this.group_General_DefaultApp.SuspendLayout();
            this.group_General_SendReceive.SuspendLayout();
            this.flowLayoutPanel_General_CheckForNew.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_General_CheckInterval)).BeginInit();
            this.group_General_General.SuspendLayout();
            this.flowLayoutPanel23.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.group_General_ActivityWindow.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel20
            // 
            this.tableLayoutPanel20.ColumnCount = 1;
            this.tableLayoutPanel20.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel20.Controls.Add(this.group_General_DefaultApp, 0, 3);
            this.tableLayoutPanel20.Controls.Add(this.group_General_SendReceive, 0, 1);
            this.tableLayoutPanel20.Controls.Add(this.group_General_General, 0, 0);
            this.tableLayoutPanel20.Controls.Add(this.group_General_ActivityWindow, 0, 2);
            this.tableLayoutPanel20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel20.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel20.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel20.Name = "tableLayoutPanel20";
            this.tableLayoutPanel20.RowCount = 4;
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel20.Size = new System.Drawing.Size(484, 361);
            this.tableLayoutPanel20.TabIndex = 1;
            // 
            // group_General_DefaultApp
            // 
            this.group_General_DefaultApp.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
            this.group_General_DefaultApp.Controls.Add(this.button_General_MakeDefault);
            this.group_General_DefaultApp.Controls.Add(this.label_General_ApplicationIsHandler);
            this.group_General_DefaultApp.Dock = System.Windows.Forms.DockStyle.Top;
            this.group_General_DefaultApp.Location = new System.Drawing.Point(3, 384);
            this.group_General_DefaultApp.Name = "group_General_DefaultApp";
            this.group_General_DefaultApp.Padding = new System.Windows.Forms.Padding(10);
            this.group_General_DefaultApp.Size = new System.Drawing.Size(478, 80);
            this.group_General_DefaultApp.TabIndex = 3;
            this.group_General_DefaultApp.TabStop = false;
            this.group_General_DefaultApp.Text = "Default Email Application";
            // 
            // button_General_MakeDefault
            // 
            this.button_General_MakeDefault.AutoSize = true;
            this.button_General_MakeDefault.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button_General_MakeDefault.Location = new System.Drawing.Point(12, 50);
            this.button_General_MakeDefault.Name = "button_General_MakeDefault";
            this.button_General_MakeDefault.Size = new System.Drawing.Size(126, 23);
            this.button_General_MakeDefault.TabIndex = 1;
            this.button_General_MakeDefault.Text = "Make &default";
            this.button_General_MakeDefault.UseVisualStyleBackColor = true;
            // 
            // label_General_ApplicationIsHandler
            // 
            this.label_General_ApplicationIsHandler.AutoSize = true;
            this.label_General_ApplicationIsHandler.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_General_ApplicationIsHandler.Location = new System.Drawing.Point(9, 29);
            this.label_General_ApplicationIsHandler.Name = "label_General_ApplicationIsHandler";
            this.label_General_ApplicationIsHandler.Size = new System.Drawing.Size(139, 13);
            this.label_General_ApplicationIsHandler.TabIndex = 0;
            this.label_General_ApplicationIsHandler.Text = "Default info text placeholder";
            // 
            // group_General_SendReceive
            // 
            this.group_General_SendReceive.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
            this.group_General_SendReceive.Controls.Add(this.flowLayoutPanel_General_CheckForNew);
            this.group_General_SendReceive.Controls.Add(this.check_General_SendAndReceiveAtStartup);
            this.group_General_SendReceive.Dock = System.Windows.Forms.DockStyle.Top;
            this.group_General_SendReceive.Location = new System.Drawing.Point(3, 193);
            this.group_General_SendReceive.Name = "group_General_SendReceive";
            this.group_General_SendReceive.Padding = new System.Windows.Forms.Padding(10);
            this.group_General_SendReceive.Size = new System.Drawing.Size(478, 79);
            this.group_General_SendReceive.TabIndex = 1;
            this.group_General_SendReceive.TabStop = false;
            this.group_General_SendReceive.Text = "Synchronization";
            // 
            // flowLayoutPanel_General_CheckForNew
            // 
            this.flowLayoutPanel_General_CheckForNew.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel_General_CheckForNew.Controls.Add(this.check_General_CheckMessages);
            this.flowLayoutPanel_General_CheckForNew.Controls.Add(this.numeric_General_CheckInterval);
            this.flowLayoutPanel_General_CheckForNew.Controls.Add(this.label_General_Minutes);
            this.flowLayoutPanel_General_CheckForNew.Location = new System.Drawing.Point(13, 53);
            this.flowLayoutPanel_General_CheckForNew.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_General_CheckForNew.Name = "flowLayoutPanel_General_CheckForNew";
            this.flowLayoutPanel_General_CheckForNew.Size = new System.Drawing.Size(458, 25);
            this.flowLayoutPanel_General_CheckForNew.TabIndex = 1;
            // 
            // check_General_CheckMessages
            // 
            this.check_General_CheckMessages.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.check_General_CheckMessages.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_CheckMessages.Location = new System.Drawing.Point(0, 1);
            this.check_General_CheckMessages.Margin = new System.Windows.Forms.Padding(0);
            this.check_General_CheckMessages.Name = "check_General_CheckMessages";
            this.check_General_CheckMessages.Size = new System.Drawing.Size(140, 17);
            this.check_General_CheckMessages.TabIndex = 0;
            this.check_General_CheckMessages.Text = "S&ynchronize items every";
            this.check_General_CheckMessages.UseVisualStyleBackColor = true;
            // 
            // numeric_General_CheckInterval
            // 
            this.numeric_General_CheckInterval.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numeric_General_CheckInterval.Location = new System.Drawing.Point(140, 0);
            this.numeric_General_CheckInterval.Margin = new System.Windows.Forms.Padding(0);
            this.numeric_General_CheckInterval.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numeric_General_CheckInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numeric_General_CheckInterval.Name = "numeric_General_CheckInterval";
            this.numeric_General_CheckInterval.Size = new System.Drawing.Size(41, 20);
            this.numeric_General_CheckInterval.TabIndex = 1;
            this.numeric_General_CheckInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label_General_Minutes
            // 
            this.label_General_Minutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_General_Minutes.AutoSize = true;
            this.label_General_Minutes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_General_Minutes.Location = new System.Drawing.Point(182, 3);
            this.label_General_Minutes.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.label_General_Minutes.Name = "label_General_Minutes";
            this.label_General_Minutes.Size = new System.Drawing.Size(43, 13);
            this.label_General_Minutes.TabIndex = 2;
            this.label_General_Minutes.Text = "minutes";
            // 
            // check_General_SendAndReceiveAtStartup
            // 
            this.check_General_SendAndReceiveAtStartup.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_SendAndReceiveAtStartup.Location = new System.Drawing.Point(13, 32);
            this.check_General_SendAndReceiveAtStartup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.check_General_SendAndReceiveAtStartup.Name = "check_General_SendAndReceiveAtStartup";
            this.check_General_SendAndReceiveAtStartup.Size = new System.Drawing.Size(158, 17);
            this.check_General_SendAndReceiveAtStartup.TabIndex = 0;
            this.check_General_SendAndReceiveAtStartup.Text = "&Synchronize items at startup";
            this.check_General_SendAndReceiveAtStartup.UseVisualStyleBackColor = true;
            // 
            // group_General_General
            // 
            this.group_General_General.AutoSize = true;
			this.group_General_General.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
            this.group_General_General.Controls.Add(this.flowLayoutPanel23);
            this.group_General_General.Dock = System.Windows.Forms.DockStyle.Top;
            this.group_General_General.Location = new System.Drawing.Point(3, 3);
            this.group_General_General.Name = "group_General_General";
            this.group_General_General.Padding = new System.Windows.Forms.Padding(10);
            this.group_General_General.Size = new System.Drawing.Size(478, 184);
            this.group_General_General.TabIndex = 0;
            this.group_General_General.TabStop = false;
            this.group_General_General.Text = "General";
            // 
            // flowLayoutPanel23
            // 
            this.flowLayoutPanel23.AutoSize = true;
            this.flowLayoutPanel23.Controls.Add(this.check_General_MinimizeToTray);
            this.flowLayoutPanel23.Controls.Add(this.check_General_CloseToTray);
            this.flowLayoutPanel23.Controls.Add(this.check_General_RunOnStartup);
            this.flowLayoutPanel23.Controls.Add(this.check_General_EmptyTrashOnExit);
            this.flowLayoutPanel23.Controls.Add(this.check_General_ShowGlobalFolders);
            this.flowLayoutPanel23.Controls.Add(this.check_General_ShowLocalFolders);
            this.flowLayoutPanel23.Controls.Add(this.tableLayoutPanel3);
            this.flowLayoutPanel23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel23.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel23.Location = new System.Drawing.Point(10, 23);
            this.flowLayoutPanel23.Name = "flowLayoutPanel23";
            this.flowLayoutPanel23.Size = new System.Drawing.Size(458, 151);
            this.flowLayoutPanel23.TabIndex = 0;
            // 
            // check_General_MinimizeToTray
            // 
            this.check_General_MinimizeToTray.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_MinimizeToTray.Location = new System.Drawing.Point(3, 2);
            this.check_General_MinimizeToTray.Margin = new System.Windows.Forms.Padding(3, 2, 3, 1);
            this.check_General_MinimizeToTray.Name = "check_General_MinimizeToTray";
            this.check_General_MinimizeToTray.Size = new System.Drawing.Size(152, 17);
            this.check_General_MinimizeToTray.TabIndex = 0;
            this.check_General_MinimizeToTray.Text = "&Minimize application to tray";
            this.check_General_MinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // check_General_CloseToTray
            // 
            this.check_General_CloseToTray.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_CloseToTray.Location = new System.Drawing.Point(3, 23);
            this.check_General_CloseToTray.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.check_General_CloseToTray.Name = "check_General_CloseToTray";
            this.check_General_CloseToTray.Size = new System.Drawing.Size(138, 17);
            this.check_General_CloseToTray.TabIndex = 1;
            this.check_General_CloseToTray.Text = "&Close application to tray";
            this.check_General_CloseToTray.UseVisualStyleBackColor = true;
            // 
            // check_General_RunOnStartup
            // 
            this.check_General_RunOnStartup.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_RunOnStartup.Location = new System.Drawing.Point(3, 44);
            this.check_General_RunOnStartup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.check_General_RunOnStartup.Name = "check_General_RunOnStartup";
            this.check_General_RunOnStartup.Size = new System.Drawing.Size(143, 17);
            this.check_General_RunOnStartup.TabIndex = 2;
            this.check_General_RunOnStartup.Text = "&Run on Windows startup";
            this.check_General_RunOnStartup.UseVisualStyleBackColor = true;
            // 
            // check_General_EmptyTrashOnExit
            // 
            this.check_General_EmptyTrashOnExit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_EmptyTrashOnExit.Location = new System.Drawing.Point(3, 65);
            this.check_General_EmptyTrashOnExit.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.check_General_EmptyTrashOnExit.Name = "check_General_EmptyTrashOnExit";
            this.check_General_EmptyTrashOnExit.Size = new System.Drawing.Size(115, 17);
            this.check_General_EmptyTrashOnExit.TabIndex = 3;
            this.check_General_EmptyTrashOnExit.Text = "&Empty trash on exit";
            this.check_General_EmptyTrashOnExit.UseVisualStyleBackColor = true;
            // 
            // check_General_ShowGlobalFolders
            // 
            this.check_General_ShowGlobalFolders.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_ShowGlobalFolders.Location = new System.Drawing.Point(3, 86);
            this.check_General_ShowGlobalFolders.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.check_General_ShowGlobalFolders.Name = "check_General_ShowGlobalFolders";
            this.check_General_ShowGlobalFolders.Size = new System.Drawing.Size(117, 17);
            this.check_General_ShowGlobalFolders.TabIndex = 4;
            this.check_General_ShowGlobalFolders.Text = "&Show Smart folders";
            this.check_General_ShowGlobalFolders.UseVisualStyleBackColor = true;
            // 
            // check_General_ShowLocalFolders
            // 
            this.check_General_ShowLocalFolders.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_ShowLocalFolders.Location = new System.Drawing.Point(3, 107);
            this.check_General_ShowLocalFolders.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.check_General_ShowLocalFolders.Name = "check_General_ShowLocalFolders";
            this.check_General_ShowLocalFolders.Size = new System.Drawing.Size(116, 17);
            this.check_General_ShowLocalFolders.TabIndex = 5;
            this.check_General_ShowLocalFolders.Text = "&Show Local folders";
            this.check_General_ShowLocalFolders.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 161F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.combo_General_AfterStart, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label_General_AfterStart, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.button_General_AfterStartFolderBrowse, 3, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 126);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(280, 25);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // combo_General_AfterStart
            // 
            this.combo_General_AfterStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo_General_AfterStart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_General_AfterStart.FormattingEnabled = true;
            this.combo_General_AfterStart.Location = new System.Drawing.Point(87, 0);
            this.combo_General_AfterStart.Margin = new System.Windows.Forms.Padding(0);
            this.combo_General_AfterStart.Name = "combo_General_AfterStart";
            this.combo_General_AfterStart.Size = new System.Drawing.Size(161, 21);
            this.combo_General_AfterStart.TabIndex = 1;
            // 
            // label_General_AfterStart
            // 
            this.label_General_AfterStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_General_AfterStart.AutoEllipsis = true;
            this.label_General_AfterStart.AutoSize = true;
            this.label_General_AfterStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_General_AfterStart.Location = new System.Drawing.Point(0, 6);
            this.label_General_AfterStart.Margin = new System.Windows.Forms.Padding(0);
            this.label_General_AfterStart.Name = "label_General_AfterStart";
            this.label_General_AfterStart.Size = new System.Drawing.Size(87, 13);
            this.label_General_AfterStart.TabIndex = 0;
            this.label_General_AfterStart.Text = "Show on startup:";
            // 
            // button_General_AfterStartFolderBrowse
            // 
            this.button_General_AfterStartFolderBrowse.AutoSize = true;
            this.button_General_AfterStartFolderBrowse.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button_General_AfterStartFolderBrowse.Location = new System.Drawing.Point(254, 0);
            this.button_General_AfterStartFolderBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 0, 2);
            this.button_General_AfterStartFolderBrowse.Name = "button_General_AfterStartFolderBrowse";
            this.button_General_AfterStartFolderBrowse.Size = new System.Drawing.Size(26, 23);
            this.button_General_AfterStartFolderBrowse.TabIndex = 3;
            this.button_General_AfterStartFolderBrowse.Text = "…";
            this.button_General_AfterStartFolderBrowse.UseVisualStyleBackColor = true;
            this.button_General_AfterStartFolderBrowse.Visible = false;
            // 
            // group_General_ActivityWindow
            // 
            this.group_General_ActivityWindow.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
            this.group_General_ActivityWindow.Controls.Add(this.check_General_ShowWindowWhenErrorOccurs);
            this.group_General_ActivityWindow.Controls.Add(this.check_General_HideWindowAfterSendAndReceive);
            this.group_General_ActivityWindow.Controls.Add(this.check_General_ShowWindowOnSendAndReceive);
            this.group_General_ActivityWindow.Dock = System.Windows.Forms.DockStyle.Top;
            this.group_General_ActivityWindow.Location = new System.Drawing.Point(3, 278);
            this.group_General_ActivityWindow.Name = "group_General_ActivityWindow";
            this.group_General_ActivityWindow.Padding = new System.Windows.Forms.Padding(10);
            this.group_General_ActivityWindow.Size = new System.Drawing.Size(478, 100);
            this.group_General_ActivityWindow.TabIndex = 2;
            this.group_General_ActivityWindow.TabStop = false;
            this.group_General_ActivityWindow.Text = "Operations Window";
            // 
            // check_General_ShowWindowWhenErrorOccurs
            // 
            this.check_General_ShowWindowWhenErrorOccurs.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_ShowWindowWhenErrorOccurs.Location = new System.Drawing.Point(13, 78);
            this.check_General_ShowWindowWhenErrorOccurs.Name = "check_General_ShowWindowWhenErrorOccurs";
            this.check_General_ShowWindowWhenErrorOccurs.Size = new System.Drawing.Size(195, 17);
            this.check_General_ShowWindowWhenErrorOccurs.TabIndex = 2;
            this.check_General_ShowWindowWhenErrorOccurs.Text = "Show wi&ndow when an error occurs";
            this.check_General_ShowWindowWhenErrorOccurs.UseVisualStyleBackColor = true;
            // 
            // check_General_HideWindowAfterSendAndReceive
            // 
            this.check_General_HideWindowAfterSendAndReceive.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_HideWindowAfterSendAndReceive.Location = new System.Drawing.Point(13, 55);
            this.check_General_HideWindowAfterSendAndReceive.Name = "check_General_HideWindowAfterSendAndReceive";
            this.check_General_HideWindowAfterSendAndReceive.Size = new System.Drawing.Size(252, 17);
            this.check_General_HideWindowAfterSendAndReceive.TabIndex = 1;
            this.check_General_HideWindowAfterSendAndReceive.Text = "Hi&de window when send and receive completes";
            this.check_General_HideWindowAfterSendAndReceive.UseVisualStyleBackColor = true;
            // 
            // check_General_ShowWindowOnSendAndReceive
            // 
            this.check_General_ShowWindowOnSendAndReceive.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.check_General_ShowWindowOnSendAndReceive.Location = new System.Drawing.Point(13, 32);
            this.check_General_ShowWindowOnSendAndReceive.Name = "check_General_ShowWindowOnSendAndReceive";
            this.check_General_ShowWindowOnSendAndReceive.Size = new System.Drawing.Size(192, 17);
            this.check_General_ShowWindowOnSendAndReceive.TabIndex = 0;
            this.check_General_ShowWindowOnSendAndReceive.Text = "Sh&ow window on send and receive";
            this.check_General_ShowWindowOnSendAndReceive.UseVisualStyleBackColor = true;
            // 
            // LayoutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.tableLayoutPanel20);
            this.Name = "LayoutForm";
            this.Text = "LayoutForm";
            this.tableLayoutPanel20.ResumeLayout(false);
            this.tableLayoutPanel20.PerformLayout();
            this.group_General_DefaultApp.ResumeLayout(false);
            this.group_General_DefaultApp.PerformLayout();
            this.group_General_SendReceive.ResumeLayout(false);
            this.group_General_SendReceive.PerformLayout();
            this.flowLayoutPanel_General_CheckForNew.ResumeLayout(false);
            this.flowLayoutPanel_General_CheckForNew.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_General_CheckInterval)).EndInit();
            this.group_General_General.ResumeLayout(false);
            this.group_General_General.PerformLayout();
            this.flowLayoutPanel23.ResumeLayout(false);
            this.flowLayoutPanel23.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.group_General_ActivityWindow.ResumeLayout(false);
            this.group_General_ActivityWindow.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel20;
        private SeparatorBox group_General_DefaultApp;
        private System.Windows.Forms.Button button_General_MakeDefault;
        private System.Windows.Forms.Label label_General_ApplicationIsHandler;
        private SeparatorBox group_General_SendReceive;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_General_CheckForNew;
        private WrappingCheckBox check_General_CheckMessages;
        private System.Windows.Forms.NumericUpDown numeric_General_CheckInterval;
        private System.Windows.Forms.Label label_General_Minutes;
        private WrappingCheckBox check_General_SendAndReceiveAtStartup;
        private SeparatorBox group_General_General;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel23;
        private WrappingCheckBox check_General_MinimizeToTray;
        private WrappingCheckBox check_General_CloseToTray;
        private WrappingCheckBox check_General_RunOnStartup;
        private WrappingCheckBox check_General_EmptyTrashOnExit;
        private WrappingCheckBox check_General_ShowGlobalFolders;
        private WrappingCheckBox check_General_ShowLocalFolders;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private ComboBoxEx combo_General_AfterStart;
        private System.Windows.Forms.Label label_General_AfterStart;
        private System.Windows.Forms.Button button_General_AfterStartFolderBrowse;
        private SeparatorBox group_General_ActivityWindow;
        private WrappingCheckBox check_General_ShowWindowWhenErrorOccurs;
        private WrappingCheckBox check_General_HideWindowAfterSendAndReceive;
        private WrappingCheckBox check_General_ShowWindowOnSendAndReceive;






    }
}
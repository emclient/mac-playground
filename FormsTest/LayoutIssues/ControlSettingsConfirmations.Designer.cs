namespace MailClient.UI.Controls.SettingsControls
{
	partial class ControlSettingsConfirmations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlSettingsConfirmations));
            this.group_Confirmations = new MailClient.Common.UI.Controls.SeparatorBox();
            this.table_Confirmations_JunkInbox = new System.Windows.Forms.TableLayoutPanel();
            this.combo_Confirmations_MoveToJunk = new MailClient.Common.UI.Controls.ComboBoxEx();
            this.label_Confirmations_MoveToJunk = new System.Windows.Forms.Label();
            this.flowLayoutPanel_Confirmations_ = new System.Windows.Forms.FlowLayoutPanel();
            this.check_Confirmations_NotifyLargeMessage = new MailClient.UI.Controls.WrappingCheckBox();
            this.numeric_Confirmations_NotifyLargeMessage = new System.Windows.Forms.NumericUpDown();
            this.label_Confirmations_MB = new System.Windows.Forms.Label();
            this.check_Confirmations_NotifyMissingAttachment = new MailClient.UI.Controls.WrappingCheckBox();
            this.flowLayoutPanel37 = new System.Windows.Forms.FlowLayoutPanel();
            this.check_Confirmations_AskDeletingFolder = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskAfterDroppingFolder = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskDeletingMail = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskDeletingMailPermanently = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskDeletingEvent = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskDeletingTask = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskDeletingContact = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskEmptyingTrash = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskWhenNewDocumentation = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskApplyingRule = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_AskMailModification = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_NotifySubjectNotSet = new MailClient.UI.Controls.WrappingCheckBox();
            this.check_Confirmations_NotifyGroupHeaderSelected = new MailClient.UI.Controls.WrappingCheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.group_Confirmations.SuspendLayout();
            this.table_Confirmations_JunkInbox.SuspendLayout();
            this.flowLayoutPanel_Confirmations_.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_Confirmations_NotifyLargeMessage)).BeginInit();
            this.flowLayoutPanel37.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // group_Confirmations
            // 
            resources.ApplyResources(this.group_Confirmations, "group_Confirmations");
            this.group_Confirmations.BoxStyle = MailClient.Common.UI.Controls.SeparatorBox.Style.Header;
            this.group_Confirmations.Controls.Add(this.table_Confirmations_JunkInbox);
            this.group_Confirmations.Controls.Add(this.flowLayoutPanel_Confirmations_);
            this.group_Confirmations.Controls.Add(this.flowLayoutPanel37);
            this.group_Confirmations.Name = "group_Confirmations";
            this.group_Confirmations.TabStop = false;
            // 
            // table_Confirmations_JunkInbox
            // 
            resources.ApplyResources(this.table_Confirmations_JunkInbox, "table_Confirmations_JunkInbox");
            this.table_Confirmations_JunkInbox.Controls.Add(this.combo_Confirmations_MoveToJunk, 1, 0);
            this.table_Confirmations_JunkInbox.Controls.Add(this.label_Confirmations_MoveToJunk, 0, 0);
            this.table_Confirmations_JunkInbox.Name = "table_Confirmations_JunkInbox";
            // 
            // combo_Confirmations_MoveToJunk
            // 
            resources.ApplyResources(this.combo_Confirmations_MoveToJunk, "combo_Confirmations_MoveToJunk");
            this.combo_Confirmations_MoveToJunk.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_Confirmations_MoveToJunk.Name = "combo_Confirmations_MoveToJunk";
            // 
            // label_Confirmations_MoveToJunk
            // 
            resources.ApplyResources(this.label_Confirmations_MoveToJunk, "label_Confirmations_MoveToJunk");
            this.label_Confirmations_MoveToJunk.Name = "label_Confirmations_MoveToJunk";
            // 
            // flowLayoutPanel_Confirmations_
            // 
            resources.ApplyResources(this.flowLayoutPanel_Confirmations_, "flowLayoutPanel_Confirmations_");
            this.flowLayoutPanel_Confirmations_.Controls.Add(this.check_Confirmations_NotifyLargeMessage);
            this.flowLayoutPanel_Confirmations_.Controls.Add(this.numeric_Confirmations_NotifyLargeMessage);
            this.flowLayoutPanel_Confirmations_.Controls.Add(this.label_Confirmations_MB);
            this.flowLayoutPanel_Confirmations_.Controls.Add(this.check_Confirmations_NotifyMissingAttachment);
            this.flowLayoutPanel_Confirmations_.Name = "flowLayoutPanel_Confirmations_";
            // 
            // check_Confirmations_NotifyLargeMessage
            // 
            resources.ApplyResources(this.check_Confirmations_NotifyLargeMessage, "check_Confirmations_NotifyLargeMessage");
            this.check_Confirmations_NotifyLargeMessage.Name = "check_Confirmations_NotifyLargeMessage";
            this.check_Confirmations_NotifyLargeMessage.UseVisualStyleBackColor = true;
            // 
            // numeric_Confirmations_NotifyLargeMessage
            // 
            resources.ApplyResources(this.numeric_Confirmations_NotifyLargeMessage, "numeric_Confirmations_NotifyLargeMessage");
            this.numeric_Confirmations_NotifyLargeMessage.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numeric_Confirmations_NotifyLargeMessage.Name = "numeric_Confirmations_NotifyLargeMessage";
            this.numeric_Confirmations_NotifyLargeMessage.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label_Confirmations_MB
            // 
            resources.ApplyResources(this.label_Confirmations_MB, "label_Confirmations_MB");
            this.flowLayoutPanel_Confirmations_.SetFlowBreak(this.label_Confirmations_MB, true);
            this.label_Confirmations_MB.Name = "label_Confirmations_MB";
            // 
            // check_Confirmations_NotifyMissingAttachment
            // 
            resources.ApplyResources(this.check_Confirmations_NotifyMissingAttachment, "check_Confirmations_NotifyMissingAttachment");
            this.check_Confirmations_NotifyMissingAttachment.Name = "check_Confirmations_NotifyMissingAttachment";
            this.check_Confirmations_NotifyMissingAttachment.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel37
            // 
            resources.ApplyResources(this.flowLayoutPanel37, "flowLayoutPanel37");
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskDeletingFolder);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskAfterDroppingFolder);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskDeletingMail);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskDeletingMailPermanently);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskDeletingEvent);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskDeletingTask);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskDeletingContact);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskEmptyingTrash);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskWhenNewDocumentation);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskApplyingRule);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_AskMailModification);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_NotifySubjectNotSet);
            this.flowLayoutPanel37.Controls.Add(this.check_Confirmations_NotifyGroupHeaderSelected);
            this.flowLayoutPanel37.Name = "flowLayoutPanel37";
            // 
            // check_Confirmations_AskDeletingFolder
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskDeletingFolder, true);
            resources.ApplyResources(this.check_Confirmations_AskDeletingFolder, "check_Confirmations_AskDeletingFolder");
            this.check_Confirmations_AskDeletingFolder.Name = "check_Confirmations_AskDeletingFolder";
            this.check_Confirmations_AskDeletingFolder.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskAfterDroppingFolder
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskAfterDroppingFolder, true);
            resources.ApplyResources(this.check_Confirmations_AskAfterDroppingFolder, "check_Confirmations_AskAfterDroppingFolder");
            this.check_Confirmations_AskAfterDroppingFolder.Name = "check_Confirmations_AskAfterDroppingFolder";
            this.check_Confirmations_AskAfterDroppingFolder.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskDeletingMail
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskDeletingMail, true);
            resources.ApplyResources(this.check_Confirmations_AskDeletingMail, "check_Confirmations_AskDeletingMail");
            this.check_Confirmations_AskDeletingMail.Name = "check_Confirmations_AskDeletingMail";
            this.check_Confirmations_AskDeletingMail.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskDeletingMailPermanently
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskDeletingMailPermanently, true);
            resources.ApplyResources(this.check_Confirmations_AskDeletingMailPermanently, "check_Confirmations_AskDeletingMailPermanently");
            this.check_Confirmations_AskDeletingMailPermanently.Name = "check_Confirmations_AskDeletingMailPermanently";
            this.check_Confirmations_AskDeletingMailPermanently.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskDeletingEvent
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskDeletingEvent, true);
            resources.ApplyResources(this.check_Confirmations_AskDeletingEvent, "check_Confirmations_AskDeletingEvent");
            this.check_Confirmations_AskDeletingEvent.Name = "check_Confirmations_AskDeletingEvent";
            this.check_Confirmations_AskDeletingEvent.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskDeletingTask
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskDeletingTask, true);
            resources.ApplyResources(this.check_Confirmations_AskDeletingTask, "check_Confirmations_AskDeletingTask");
            this.check_Confirmations_AskDeletingTask.Name = "check_Confirmations_AskDeletingTask";
            this.check_Confirmations_AskDeletingTask.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskDeletingContact
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskDeletingContact, true);
            resources.ApplyResources(this.check_Confirmations_AskDeletingContact, "check_Confirmations_AskDeletingContact");
            this.check_Confirmations_AskDeletingContact.Name = "check_Confirmations_AskDeletingContact";
            this.check_Confirmations_AskDeletingContact.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskEmptyingTrash
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskEmptyingTrash, true);
            resources.ApplyResources(this.check_Confirmations_AskEmptyingTrash, "check_Confirmations_AskEmptyingTrash");
            this.check_Confirmations_AskEmptyingTrash.Name = "check_Confirmations_AskEmptyingTrash";
            this.check_Confirmations_AskEmptyingTrash.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskWhenNewDocumentation
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskWhenNewDocumentation, true);
            resources.ApplyResources(this.check_Confirmations_AskWhenNewDocumentation, "check_Confirmations_AskWhenNewDocumentation");
            this.check_Confirmations_AskWhenNewDocumentation.Name = "check_Confirmations_AskWhenNewDocumentation";
            this.check_Confirmations_AskWhenNewDocumentation.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskApplyingRule
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskApplyingRule, true);
            resources.ApplyResources(this.check_Confirmations_AskApplyingRule, "check_Confirmations_AskApplyingRule");
            this.check_Confirmations_AskApplyingRule.Name = "check_Confirmations_AskApplyingRule";
            this.check_Confirmations_AskApplyingRule.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_AskMailModification
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_AskMailModification, true);
            resources.ApplyResources(this.check_Confirmations_AskMailModification, "check_Confirmations_AskMailModification");
            this.check_Confirmations_AskMailModification.Name = "check_Confirmations_AskMailModification";
            this.check_Confirmations_AskMailModification.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_NotifySubjectNotSet
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_NotifySubjectNotSet, true);
            resources.ApplyResources(this.check_Confirmations_NotifySubjectNotSet, "check_Confirmations_NotifySubjectNotSet");
            this.check_Confirmations_NotifySubjectNotSet.Name = "check_Confirmations_NotifySubjectNotSet";
            this.check_Confirmations_NotifySubjectNotSet.UseVisualStyleBackColor = true;
            // 
            // check_Confirmations_NotifyGroupHeaderSelected
            // 
            this.flowLayoutPanel37.SetFlowBreak(this.check_Confirmations_NotifyGroupHeaderSelected, true);
            resources.ApplyResources(this.check_Confirmations_NotifyGroupHeaderSelected, "check_Confirmations_NotifyGroupHeaderSelected");
            this.check_Confirmations_NotifyGroupHeaderSelected.Name = "check_Confirmations_NotifyGroupHeaderSelected";
            this.check_Confirmations_NotifyGroupHeaderSelected.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.group_Confirmations, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // ControlSettingsConfirmations
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ControlSettingsConfirmations";
            this.group_Confirmations.ResumeLayout(false);
            this.group_Confirmations.PerformLayout();
            this.table_Confirmations_JunkInbox.ResumeLayout(false);
            this.table_Confirmations_JunkInbox.PerformLayout();
            this.flowLayoutPanel_Confirmations_.ResumeLayout(false);
            this.flowLayoutPanel_Confirmations_.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_Confirmations_NotifyLargeMessage)).EndInit();
            this.flowLayoutPanel37.ResumeLayout(false);
            this.flowLayoutPanel37.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Common.UI.Controls.SeparatorBox group_Confirmations;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel37;
		private WrappingCheckBox check_Confirmations_AskDeletingFolder;
		private WrappingCheckBox check_Confirmations_AskAfterDroppingFolder;
		private WrappingCheckBox check_Confirmations_AskDeletingMail;
		private WrappingCheckBox check_Confirmations_AskDeletingMailPermanently;
		private WrappingCheckBox check_Confirmations_AskDeletingEvent;
		private WrappingCheckBox check_Confirmations_AskDeletingTask;
		private WrappingCheckBox check_Confirmations_AskDeletingContact;
		private WrappingCheckBox check_Confirmations_AskEmptyingTrash;
		private WrappingCheckBox check_Confirmations_AskWhenNewDocumentation;
		private WrappingCheckBox check_Confirmations_AskApplyingRule;
		private WrappingCheckBox check_Confirmations_AskMailModification;
		private WrappingCheckBox check_Confirmations_NotifySubjectNotSet;
		private WrappingCheckBox check_Confirmations_NotifyGroupHeaderSelected;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Confirmations_;
		private WrappingCheckBox check_Confirmations_NotifyLargeMessage;
		private System.Windows.Forms.NumericUpDown numeric_Confirmations_NotifyLargeMessage;
		private System.Windows.Forms.Label label_Confirmations_MB;
		private WrappingCheckBox check_Confirmations_NotifyMissingAttachment;
		private System.Windows.Forms.TableLayoutPanel table_Confirmations_JunkInbox;
		private Common.UI.Controls.ComboBoxEx combo_Confirmations_MoveToJunk;
		private System.Windows.Forms.Label label_Confirmations_MoveToJunk;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}

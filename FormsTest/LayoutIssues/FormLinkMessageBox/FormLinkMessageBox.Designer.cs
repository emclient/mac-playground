namespace MailClient.UI.Forms
{
	partial class FormLinkMessageBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLinkMessageBox));
			this.linkLabel1 = new MailClient.Common.UI.Controls.ControlLinkLabel();
			this.buttonOk = new MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// linkLabel1
			// 
			resources.ApplyResources(this.linkLabel1, "linkLabel1");
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.TabStop = true;
			this.linkLabel1.MultiLine = true;
			this.linkLabel1.MaxLines = 20;
			this.linkLabel1.UseLinkColorForText = false;
			this.linkLabel1.LinkSelectionBehavior = Common.UI.Controls.LinkSelectionBehavior.ClickWithLeftLink;
			this.linkLabel1.LinkClicked += new System.EventHandler<MailClient.Common.UI.Controls.ControlLinkClickedEventArgs>(this.linkLabel1_LinkClicked);
			// 
			// buttonOk
			// 
			resources.ApplyResources(this.buttonOk, "buttonOk");
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Style = MailClient.Common.UI.Controls.ControlToolStrip.ButtonDisplayStyle.Text;
			this.buttonOk.ToolStripStyle = false;
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonOk, 0, 1);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// FormLinkMessageBox
			// 
			this.AcceptButton = this.buttonOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.buttonOk;
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormLinkMessageBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MailClient.Common.UI.Controls.ControlLinkLabel linkLabel1;
		private MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton buttonOk;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}
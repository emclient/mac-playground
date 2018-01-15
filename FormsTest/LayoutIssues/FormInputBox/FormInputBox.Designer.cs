namespace MailClient.UI.Forms
{
	partial class FormInputBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInputBox));
			this.labelDescription = new System.Windows.Forms.Label();
			this.buttonCancel = new MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton();
			this.buttonOk = new MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton();
			this.textResult = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelDescription
			// 
			resources.ApplyResources(this.labelDescription, "labelDescription");
			this.labelDescription.Name = "labelDescription";
			// 
			// buttonCancel
			// 
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Style = MailClient.Common.UI.Controls.ControlToolStrip.ButtonDisplayStyle.Text;
			this.buttonCancel.ToolStripStyle = false;
			// 
			// buttonOk
			// 
			resources.ApplyResources(this.buttonOk, "buttonOk");
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.IsHighlighted = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Style = MailClient.Common.UI.Controls.ControlToolStrip.ButtonDisplayStyle.Text;
			this.buttonOk.ToolStripStyle = false;
			// 
			// textResult
			// 
			resources.ApplyResources(this.textResult, "textResult");
			this.textResult.Name = "textResult";
			this.textResult.TextChanged += new System.EventHandler(this.textResult_TextChanged);
			// 
			// flowLayoutPanel1
			// 
			resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
			this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
			this.flowLayoutPanel1.Controls.Add(this.buttonOk);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			// 
			// FormInputBox
			// 
			this.AcceptButton = this.buttonOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.buttonCancel;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.textResult);
			this.Controls.Add(this.labelDescription);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormInputBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Load += new System.EventHandler(this.FormInputBox_Load);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelDescription;
		private MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton buttonCancel;
		private MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton buttonOk;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx textResult;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
	}
}
namespace MailClient.UI.Controls.WizardControls
{
	partial class ControlExpandablePanelAutodiscover
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlExpandablePanelAutodiscover));
			this.textEmail = new MailClient.Common.UI.Controls.ControlTextBox.ControlTextBox();
			this.textPassword = new MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx();
			this.labelCaption = new System.Windows.Forms.Label();
			this.labelEmail = new System.Windows.Forms.Label();
			this.labelPassword = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.stripButton_Manual = new MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton();
			this.stripButton_Autodiscover = new MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton();
			this.labelProgress = new System.Windows.Forms.Label();
			//this.controlWaiting1 = new MailClient.UI.Controls.ControlWaiting();
			this.pictureBoxError = new System.Windows.Forms.PictureBox();
			this.labelError = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxError)).BeginInit();
			this.SuspendLayout();
			// 
			// textEmail
			// 
			resources.ApplyResources(this.textEmail, "textEmail");
			//this.textEmail.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.textEmail.Name = "textEmail";
			// 
			// textPassword
			// 
			resources.ApplyResources(this.textPassword, "textPassword");
			this.textPassword.Name = "textPassword";
			this.textPassword.UseSystemPasswordChar = true;
			// 
			// labelCaption
			// 
			resources.ApplyResources(this.labelCaption, "labelCaption");
			this.labelCaption.AutoEllipsis = true;
			this.labelCaption.Name = "labelCaption";
			// 
			// labelEmail
			// 
			resources.ApplyResources(this.labelEmail, "labelEmail");
			this.labelEmail.Name = "labelEmail";
			// 
			// labelPassword
			// 
			resources.ApplyResources(this.labelPassword, "labelPassword");
			this.labelPassword.Name = "labelPassword";
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.textEmail, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelEmail, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelPassword, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.textPassword, 1, 1);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// flowLayoutPanel1
			// 
			resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
			this.flowLayoutPanel1.Controls.Add(this.stripButton_Manual);
			this.flowLayoutPanel1.Controls.Add(this.stripButton_Autodiscover);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			// 
			// stripButton_Manual
			// 
			this.stripButton_Manual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(181)))), ((int)(((byte)(76)))));
			this.stripButton_Manual.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(181)))), ((int)(((byte)(76)))));
			this.stripButton_Manual.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(62)))));
			this.stripButton_Manual.ColorStyle = MailClient.Common.UI.Controls.ControlToolStrip.Colors.ColorStyle.UseColorsSpecified;
			this.stripButton_Manual.ForeColor = System.Drawing.Color.White;
			this.stripButton_Manual.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(62)))));
			this.stripButton_Manual.HoverForeColor = System.Drawing.Color.White;
			this.stripButton_Manual.IsHighlighted = true;
			resources.ApplyResources(this.stripButton_Manual, "stripButton_Manual");
			this.stripButton_Manual.Name = "stripButton_Manual";
			this.stripButton_Manual.Style = MailClient.Common.UI.Controls.ControlToolStrip.ButtonDisplayStyle.Text;
			this.stripButton_Manual.Click += new System.EventHandler(this.controlButtonManualConfiguration_Click);
			// 
			// stripButton_Autodiscover
			// 
			this.stripButton_Autodiscover.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(181)))), ((int)(((byte)(76)))));
			this.stripButton_Autodiscover.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(181)))), ((int)(((byte)(76)))));
			this.stripButton_Autodiscover.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(62)))));
			this.stripButton_Autodiscover.ColorStyle = MailClient.Common.UI.Controls.ControlToolStrip.Colors.ColorStyle.UseColorsSpecified;
			this.stripButton_Autodiscover.ForeColor = System.Drawing.Color.White;
			this.stripButton_Autodiscover.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(62)))));
			this.stripButton_Autodiscover.HoverForeColor = System.Drawing.Color.White;
			this.stripButton_Autodiscover.IsHighlighted = true;
			resources.ApplyResources(this.stripButton_Autodiscover, "stripButton_Autodiscover");
			this.stripButton_Autodiscover.Name = "stripButton_Autodiscover";
			this.stripButton_Autodiscover.Style = MailClient.Common.UI.Controls.ControlToolStrip.ButtonDisplayStyle.Text;
			this.stripButton_Autodiscover.Click += new System.EventHandler(this.controlButtonAutodiscover_Click);
			// 
			// labelProgress
			// 
			resources.ApplyResources(this.labelProgress, "labelProgress");
			this.labelProgress.Name = "labelProgress";
			// 
			// controlWaiting1
			// 
			/*this.controlWaiting1.BackColor = System.Drawing.Color.Transparent;
			this.controlWaiting1.ImageAlignment = MailClient.UI.Controls.ControlWaiting.ImageAlignmentType.Center;
			resources.ApplyResources(this.controlWaiting1, "controlWaiting1");
			this.controlWaiting1.Name = "controlWaiting1";
			this.controlWaiting1.TabStop = false;*/
			// 
			// pictureBoxError
			// 
			//this.pictureBoxError.Image = global::MailClient.Resources_MultiResImages.Crash_16;
			resources.ApplyResources(this.pictureBoxError, "pictureBoxError");
			this.pictureBoxError.Name = "pictureBoxError";
			this.pictureBoxError.TabStop = false;
			// 
			// labelError
			// 
			resources.ApplyResources(this.labelError, "labelError");
			this.labelError.AutoEllipsis = true;
			this.labelError.ForeColor = System.Drawing.Color.Crimson;
			this.labelError.Name = "labelError";
			// 
			// ControlExpandablePanelAutodiscover
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.pictureBoxError);
			this.Controls.Add(this.labelCaption);
			this.Controls.Add(this.labelError);
			//this.Controls.Add(this.controlWaiting1);
			this.Controls.Add(this.labelProgress);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "ControlExpandablePanelAutodiscover";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxError)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MailClient.Common.UI.Controls.ControlTextBox.ControlTextBox textEmail;
		private MailClient.Common.UI.Controls.TextBoxEx.TextBoxEx textPassword;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Label labelEmail;
		private System.Windows.Forms.Label labelPassword;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label labelProgress;
		//private ControlWaiting controlWaiting1;
		private System.Windows.Forms.PictureBox pictureBoxError;
		private System.Windows.Forms.Label labelError;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private Common.UI.Controls.ControlToolStrip.ControlToolStripButton stripButton_Manual;
		private Common.UI.Controls.ControlToolStrip.ControlToolStripButton stripButton_Autodiscover;
	}
}

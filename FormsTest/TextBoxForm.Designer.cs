namespace FormsTest
{
    partial class TextBoxForm
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
			this.panel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.textbox1 = new System.Windows.Forms.TextBox();
			this.textbox2 = new System.Windows.Forms.TextBox();
			this.textbox3 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(1);
            this.panel1.Size = new System.Drawing.Size(272, 249);
            this.panel1.TabIndex = 0;
			this.panel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;

			// textbox1
			this.textbox1.AutoSize = true;
            this.textbox1.Name = "textbox1";
            this.textbox1.Size = new System.Drawing.Size(71, 13);
			this.textbox1.TabIndex = 0;
			this.textbox1.Font = System.Drawing.SystemFonts.DefaultFont;
			this.textbox1.Text = this.textbox1.Font.ToString();

			// textbox2
			this.textbox2.AutoSize = true;
			this.textbox2.Name = "textbox1";
			this.textbox2.Size = new System.Drawing.Size(71, 13);
			this.textbox2.TabIndex = 1;
			this.textbox2.Font = System.Drawing.SystemFonts.CaptionFont;
			this.textbox2.Text = this.textbox2.Font.ToString();

			// textbox3
			this.textbox3.AutoSize = true;
			this.textbox3.Name = "textbox3";
			this.textbox3.Size = new System.Drawing.Size(71, 13);
			this.textbox3.TabIndex = 2;
			this.textbox3.Font = System.Drawing.SystemFonts.MenuFont;
			this.textbox3.Text = this.textbox3.Font.ToString();

			this.panel1.Controls.Add(textbox1);			this.panel1.Controls.Add(textbox2);
			this.panel1.Controls.Add(textbox3);

			// 
			// DebugLayoutForm4
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DebugLayoutForm4";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.Text = "DebugLayoutForm4";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}
		
		#endregion
		
		private System.Windows.Forms.FlowLayoutPanel panel1;

        private System.Windows.Forms.TextBox textbox1;
        private System.Windows.Forms.TextBox textbox2;
        private System.Windows.Forms.TextBox textbox3;
	}
}
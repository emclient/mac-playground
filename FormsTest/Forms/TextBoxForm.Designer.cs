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
			this.maskedbox = new System.Windows.Forms.MaskedTextBox();
			this.textbox1 = new System.Windows.Forms.TextBox();
			this.textbox2 = new System.Windows.Forms.TextBox();
			this.textbox3 = new System.Windows.Forms.TextBox();
			this.textbox4 = new System.Windows.Forms.TextBox();
			this.textbox5 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // panel1
            // 
			this.panel1.SuspendLayout();
			this.panel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.panel1.AutoSize = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(1);
            this.panel1.Size = new System.Drawing.Size(600, 249);
            this.panel1.TabIndex = 0;

			// maskedbox
			this.maskedbox.AutoSize = true;
            this.maskedbox.Name = "maskedbox";
            this.maskedbox.Size = new System.Drawing.Size(80, 13);
			this.maskedbox.TabIndex = 1;
			this.maskedbox.Font = System.Drawing.SystemFonts.DefaultFont;
			this.maskedbox.Text = this.maskedbox.Font.ToString();

			// textbox1
            this.textbox1.Name = "textbox1";
            this.textbox1.Size = new System.Drawing.Size(600, 22);
			this.textbox1.TabIndex = 2;
			this.textbox1.Font = System.Drawing.SystemFonts.DefaultFont;
			this.textbox1.Text = this.textbox1.Name + ":" + this.textbox1.Font.ToString();

			// textbox2
			this.textbox2.Name = "textbox2";
			this.textbox2.Size = new System.Drawing.Size(600, 13);
			this.textbox2.TabIndex = 3;
			this.textbox2.Font = System.Drawing.SystemFonts.CaptionFont;
			this.textbox2.Text = this.textbox2.Font.ToString();

			// textbox3
			this.textbox3.Name = "textbox3";
			this.textbox3.Size = new System.Drawing.Size(600, 13);
			this.textbox3.TabIndex = 4;
			this.textbox3.Font = System.Drawing.SystemFonts.MenuFont;
			this.textbox3.Text = this.textbox3.Font.ToString();

			// textbox4
			this.textbox4.AutoSize = false;
			this.textbox4.Name = "textbox4";
			this.textbox4.Size = new System.Drawing.Size(250, 200);
			this.textbox4.TabIndex = 5;
			this.textbox4.Font = System.Drawing.SystemFonts.MenuFont;
			this.textbox4.Text = this.textbox3.Font.ToString();
			this.textbox4.Multiline = true;
			this.textbox4.AcceptsTab = true;
			this.textbox4.AcceptsReturn = true;
			this.textbox4.ScrollBars = System.Windows.Forms.ScrollBars.Both;

			// textbox5
			this.textbox5.AutoSize = false;
			this.textbox5.Name = "textbox5";
			this.textbox5.Size = new System.Drawing.Size(450, 160);
			this.textbox5.TabIndex = 6;
			this.textbox5.Font = System.Drawing.SystemFonts.MenuFont;
			this.textbox5.Text = this.textbox3.Font.ToString();
			this.textbox5.Multiline = true;
			this.textbox5.AcceptsTab = false;
			this.textbox5.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;

			this.button1 = new System.Windows.Forms.Button();
			this.button1.Text = "Get Rtf";
			this.button1.Click += new System.EventHandler(this.button1_Click);

			this.panel1.Controls.Add(maskedbox);
			this.panel1.Controls.Add(textbox1);			this.panel1.Controls.Add(textbox2);
			this.panel1.Controls.Add(textbox3);
			this.panel1.Controls.Add(textbox4);
			this.panel1.Controls.Add(textbox5);
			this.panel1.Controls.Add(button1);

			// TextBoxForm
			this.MinimumSize = new System.Drawing.Size(0, 0);
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(640, 560);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Name = "TextBoxForm";
			this.Padding = new System.Windows.Forms.Padding(6);
			this.Text = "TextBoxForm";

			this.panel1.ResumeLayout(true);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		
		#endregion
		
		private System.Windows.Forms.FlowLayoutPanel panel1;

		private System.Windows.Forms.MaskedTextBox maskedbox;
		private System.Windows.Forms.TextBox textbox1;
        private System.Windows.Forms.TextBox textbox2;
        private System.Windows.Forms.TextBox textbox3;
		private System.Windows.Forms.TextBox textbox4;
		private System.Windows.Forms.RichTextBox textbox5;
		private System.Windows.Forms.Button button1;
	}
}
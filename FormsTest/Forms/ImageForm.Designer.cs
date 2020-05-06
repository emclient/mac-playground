namespace FormsTest
{
    partial class ImageForm
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
            this.picturebox1 = new System.Windows.Forms.PictureBox();
            this.picturebox2 = new System.Windows.Forms.PictureBox();
            this.picturebox3 = new System.Windows.Forms.PictureBox();
            this.picturebox4 = new System.Windows.Forms.PictureBox();
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

			// textbox1
			this.picturebox1.AutoSize = true;
            this.picturebox1.Name = "textbox1";
            this.picturebox1.Size = new System.Drawing.Size(100, 100);
			this.picturebox1.TabIndex = 0;
            this.picturebox1.ImageLocation = "Resources/Hotmail.png";
            this.picturebox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;


			// textbox2
			this.picturebox2.AutoSize = true;
			this.picturebox2.Name = "textbox1";
			this.picturebox2.Size = new System.Drawing.Size(100, 100);
			this.picturebox2.TabIndex = 1;
            this.picturebox2.ImageLocation = "Resources/Hotmail_Small.png";
            this.picturebox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;

			// textbox3
			this.picturebox3.AutoSize = true;
			this.picturebox3.Name = "textbox3";
			//this.picturebox3.Size = new System.Drawing.Size(100, 100);
			this.picturebox3.TabIndex = 2;
            //this.picturebox3.ImageLocation = "Resources/iCloud.png";
            this.picturebox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;

			// textbox4
			this.picturebox4.AutoSize = true;
			this.picturebox4.Name = "textbox4";
			this.picturebox4.Size = new System.Drawing.Size(100, 100);
			this.picturebox4.TabIndex = 3;
            //this.picturebox4.ImageLocation = "Resources/iCloud_Small.png";
            this.picturebox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;

			this.panel1.Controls.Add(picturebox1);			this.panel1.Controls.Add(picturebox2);
			this.panel1.Controls.Add(picturebox3);
			this.panel1.Controls.Add(picturebox4);

			// TextBoxForm
			this.MinimumSize = new System.Drawing.Size(0, 0);
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(640, 480);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Name = "ImageBoxForm";
			this.Padding = new System.Windows.Forms.Padding(6);
			this.Text = "ImageBoxForm";

			this.panel1.ResumeLayout(true);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		
		#endregion
		
		private System.Windows.Forms.FlowLayoutPanel panel1;

        private System.Windows.Forms.PictureBox picturebox1;
        private System.Windows.Forms.PictureBox picturebox2;
        private System.Windows.Forms.PictureBox picturebox3;
        private System.Windows.Forms.PictureBox picturebox4;
	}
}
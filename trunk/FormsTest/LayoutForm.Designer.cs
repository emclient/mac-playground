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
            this.separatorBox1 = new FormsTest.SeparatorBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBox1 = new FormsTest.WrappingCheckBox();
            this.checkBox2 = new FormsTest.WrappingCheckBox();
            this.checkBox3 = new FormsTest.WrappingCheckBox();
            this.checkBox4 = new FormsTest.WrappingCheckBox();
            this.checkBox5 = new FormsTest.WrappingCheckBox();
            this.checkBox6 = new FormsTest.WrappingCheckBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label_General_AfterStart = new System.Windows.Forms.Label();
            this.button_General_AfterStartFolderBrowse = new System.Windows.Forms.Button();
            this.separatorBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // separatorBox1
            // 
            this.separatorBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.separatorBox1.BoxStyle = FormsTest.SeparatorBox.Style.Header;
            this.separatorBox1.Controls.Add(this.flowLayoutPanel1);
            this.separatorBox1.Location = new System.Drawing.Point(12, 12);
            this.separatorBox1.Name = "separatorBox1";
            this.separatorBox1.Size = new System.Drawing.Size(460, 337);
            this.separatorBox1.TabIndex = 1;
            this.separatorBox1.TabStop = false;
            this.separatorBox1.Text = "General";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.checkBox1);
            this.flowLayoutPanel1.Controls.Add(this.checkBox2);
            this.flowLayoutPanel1.Controls.Add(this.checkBox3);
            this.flowLayoutPanel1.Controls.Add(this.checkBox4);
            this.flowLayoutPanel1.Controls.Add(this.checkBox5);
            this.flowLayoutPanel1.Controls.Add(this.checkBox6);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel3);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(13, 22);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(434, 302);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(3, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(152, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "&Minimize application to tray";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.Location = new System.Drawing.Point(3, 26);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(138, 17);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "&Close application to tray";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.Location = new System.Drawing.Point(3, 49);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(143, 17);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Text = "&Run on Windows startup";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.Location = new System.Drawing.Point(3, 72);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(115, 17);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.Text = "&Empty trash on exit";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.Location = new System.Drawing.Point(3, 95);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(117, 17);
            this.checkBox5.TabIndex = 4;
            this.checkBox5.Text = "&Show Smart folders";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.Location = new System.Drawing.Point(3, 118);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(116, 17);
            this.checkBox6.TabIndex = 5;
            this.checkBox6.Text = "&Show Local folders";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 161F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.label_General_AfterStart, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.button_General_AfterStartFolderBrowse, 3, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 139);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(280, 25);
            this.tableLayoutPanel3.TabIndex = 7;
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
            // LayoutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.separatorBox1);
            this.Name = "LayoutForm";
            this.Text = "LayoutForm";
            this.separatorBox1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SeparatorBox separatorBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private WrappingCheckBox checkBox1;
        private WrappingCheckBox checkBox2;
        private WrappingCheckBox checkBox3;
        private WrappingCheckBox checkBox4;
        private WrappingCheckBox checkBox5;
        private WrappingCheckBox checkBox6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label_General_AfterStart;
        private System.Windows.Forms.Button button_General_AfterStartFolderBrowse;

    }
}
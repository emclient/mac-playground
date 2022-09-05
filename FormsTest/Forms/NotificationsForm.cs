using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace FormsTest
{
	partial class NotificationsForm : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		FlowLayoutPanel panel1;

		public NotificationsForm()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			components = new Container();

			this.panel1 = new FlowLayoutPanel();
            this.SuspendLayout();

			// panel1
			panel1.SuspendLayout();
			panel1.AutoSize = true;
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Dock = DockStyle.Fill;
			panel1.Name = "panel1";
			panel1.FlowDirection = FlowDirection.TopDown;
			panel1.Padding = new Padding(1);

			var button1 = new Button();
			button1.AutoSize = true;
			button1.Click += button1_Click;
			button1.Text = "Button 1";
			panel1.Controls.Add(button1);

			var button2 = new Button();
			button2.AutoSize = true;
			button2.Click += button2_Click;
			button2.Text = "Button 2";
			panel1.Controls.Add(button2);

			var button3 = new Button();
			button3.AutoSize = true;
			button1.Click += button3_Click;
			button3.Text = "Button 3";
			panel1.Controls.Add(button3);

			// TablesForm
			MinimumSize = new Size(100, 100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(480, 320);
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "TablesForm";
			Padding = new Padding(6);
			Text = "TablesForm";

			panel1.ResumeLayout(true);
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		protected void button1_Click(object? sender, EventArgs e)
		{
		}

		protected void button2_Click(object? sender, EventArgs e)
		{
		}

		protected void button3_Click(object? sender, EventArgs e)
		{
		}
	}
}

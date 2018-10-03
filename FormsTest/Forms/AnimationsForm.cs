using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MailClient.UI.Controls;

namespace FormsTest
{
	public class AnimationsForm : Form
	{
		IContainer components;
		ControlWaiting circle;
		ControlStatusBar bar;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();

			bar = new ControlStatusBar();
			bar.Size = new Size(100, 18);
			bar.SetInfiniteProgress("Synchronizing...");
			bar.Location = Place();
			Controls.Add(bar);

			circle = new ControlWaiting();
			circle.Location = Place();
			circle.Size = new Size(16, 16);
			circle.BackColor = Color.Transparent;
			Controls.Add(circle);

			var a = new Button();
			a.Text = "Circle";
			a.Location = Place();
			a.Click += A_Click;
			Controls.Add(a);

			var b = new Button();
			b.Text = "Bar";
			b.Location = Place();
			b.Click += B_Click;
			Controls.Add(b);

			var c = new Button();
			c.Text = "Invalidate";
			c.Location = Place();
			c.Click += C_Click;
			Controls.Add(c);
		}

		private Point Place()
		{
			var pos = Point.Empty;
			foreach (Control c in Controls)
				pos.Y = Math.Max(pos.Y, c.Bottom);
			return pos + new Size(0, 5);
		}

		void A_Click(object sender, EventArgs e)
		{
			circle.Enabled = !circle.Enabled;
		}

		void B_Click(object sender, EventArgs e)
		{
			if (bar.InfiniteOperationActive)
				bar.StopInfiniteProgress();
			else
				bar.SetInfiniteProgress("Synchronizing...");
		}

		bool inval = true;
		void C_Click(object sender, EventArgs e)
		{
			bar.InvalidationEnabled = inval;
			circle.InvalidationEnabled = inval;
			inval = !inval;
		}

		public AnimationsForm()
		{
			InitializeComponent();
		}
	}
}

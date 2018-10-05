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
		ProgressBar pbar, pbar2;
		ProgressIndicator pind;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();

			var d = new Button();
			d.Text = "Native";
			d.Location = Below();
			d.Click += (sender, evt) => { pbar.Style = pbar.Style == ProgressBarStyle.Marquee ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee; };
			Controls.Add(d);

			pbar = new ProgressBar();
			pbar.Style = ProgressBarStyle.Marquee;
			pbar.Size = new Size(100, 5);
			pbar.Location = Right(d);
			Controls.Add(pbar);

			var f = new Button();
			f.Text = "CALayer";
			f.Location = Below();
			f.Click += (sender, evt) => { pind.Enabled = !pind.Enabled; };
			Controls.Add(f);

			pind = new ProgressIndicator();
			pind.Size = new Size(16, 16);
			pind.Location = Right(f);
			pind.BackColor = Color.White;
			Controls.Add(pind);

			var c = new Button();
			c.Text = "Don't Inval";
			c.Location = Below();
			c.Click += C_Click;
			Controls.Add(c);

			var a = new Button();
			a.Text = "Circle";
			a.Location = Below();
			a.Click += A_Click;
			Controls.Add(a);

			circle = new ControlWaiting();
			circle.Location = Right(a);
			circle.Size = new Size(16, 16);
			circle.BackColor = Color.Transparent;
			Controls.Add(circle);

			var b = new Button();
			b.Text = "Bar";
			b.Click += B_Click;
			b.Location = Below();
			Controls.Add(b);

			bar = new ControlStatusBar();
			bar.Size = new Size(100, 18);
			bar.SetInfiniteProgress("Synchronizing...");
			bar.Location = Right(b);
			Controls.Add(bar);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		private Point Below(Control control = null)
		{
			var pos = new Point(5, 5);
			if (control != null)
				pos = control.Location + new Size(0, control.Height);
			else
				foreach (Control c in Controls)
				{
					pos.Y = Math.Max(pos.Y, c.Bottom);
					pos.X = Math.Min(pos.X, c.Left);
				}
			return pos + new Size(0, 5);
		}

		private Point Right(Control control = null)
		{
			var pos = new Point(5, 5);
			if (control != null)
				pos = control.Location + new Size(control.Width, 0);
			else
				foreach (Control c in Controls)
				{
					pos.X = Math.Max(pos.X, c.Right);
					pos.Y = Math.Min(pos.Y, c.Top);
				}
			return pos + new Size(5, 0);
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
			inval = !inval;
			bar.InvalidationEnabled = inval;
			circle.InvalidationEnabled = inval;
		}

		public AnimationsForm()
		{
			Text = "Animations";
			InitializeComponent();
		}
	}
}

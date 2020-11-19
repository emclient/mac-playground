using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FormsTest.Controls;

namespace FormsTest
{
	public class TextureBrushForm : Form
	{
		IContainer components;
		FlowLayoutPanel panel1;

		List<TextureBrushControl> textBrushControls;

		public TextureBrushForm()
		{
			InitializeComponent();

			Console.WriteLine(ModifierKeys);

			if ((ModifierKeys & Keys.Shift) != 0)
				AddTextureBrushControls();
			else
				AddTextureBrushControl();
		}

		private void InitializeComponent()
		{
			components = new Container();

			panel1 = new FlowLayoutPanel();

			// panel1
			panel1.SuspendLayout();
			panel1.FlowDirection = FlowDirection.TopDown;
			panel1.AutoSize = true;
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Dock = DockStyle.Fill;
			panel1.Location = new Point(6, 6);
			panel1.Name = "panel1";
			panel1.Padding = new Padding(1);
			panel1.Size = new Size(600, 249);
			panel1.TabIndex = 0;
#if MAC
			//panel1.UseNativeControl = true;
#endif
			//panel1.WrapContents = false;
			//panel1.AutoScroll = true;

			// TextureBrushForm
			MinimumSize = new Size(100, 100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(480, 320);
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "TextureBrushForm";
			Padding = new Padding(6);
			Text = "TextureBrushForm";

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

		void AddTextureBrushControls()
		{
			var modes = Enum.GetValues(typeof(System.Drawing.Drawing2D.WrapMode));
			for (int i = 0; i < 20; ++i)
				foreach (System.Drawing.Drawing2D.WrapMode mode in modes)
				{
					var c = new TextureBrushControl();
					c.Name = $"{i}-{(int)mode}";
					c.Size = new Size(2 * 48, 2 * 32);
					c.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;// mode;
					panel1.Controls.Add(c);
				}
		}

		void AddTextureBrushControl()
		{
			var control = new TextureBrushControl2();
			control.Size = new Size(1000, 1000);
			panel1.Controls.Add(control);
		}
	}
}

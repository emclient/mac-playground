using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace FormsTest
{
	partial class ScrollBarsForm : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		Panel mainPanel;
		public ScrollBarsForm()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			components = new Container();

			this.mainPanel = new Panel();
            this.SuspendLayout();

			// mainPanel
			mainPanel.SuspendLayout();
			//mainPanel.FlowDirection = FlowDirection.LeftToRight;
			mainPanel.AutoSize = true;
			mainPanel.BorderStyle = BorderStyle.FixedSingle;
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Name = "panel1";
			mainPanel.Padding = new Padding(1);

			var vbar = new VScrollBar();
			vbar.Dock = DockStyle.Right;
			vbar.Minimum = -1;
			vbar.Maximum = 10;
			vbar.Value = 1;
			vbar.SmallChange = 1;
			vbar.LargeChange = 11;

			var hbar = new HScrollBar();
			hbar.Dock = DockStyle.Bottom;
			hbar.Minimum = 10;
			hbar.Maximum = 20;
			hbar.Value = 10;
			hbar.SmallChange = 1;
			hbar.LargeChange = 5;

			Label vlabel = new Label();
			vlabel.Dock = DockStyle.Top;
			vlabel.AutoSize = true;

			Label hlabel = new Label();
			hlabel.Dock = DockStyle.Top;
			hlabel.AutoSize = true;

			Label vbarValueLabel = new Label();
			vbarValueLabel.Dock = DockStyle.Top;
			vbarValueLabel.AutoSize = true;

			var vvalbox = new TextBox();
			vvalbox.Dock = DockStyle.Top;
			vvalbox.AutoSize = true;
			vvalbox.MinimumSize = new Size(50, 21);
			vvalbox.TextChanged += (s, e) => { if (int.TryParse(vvalbox.Text, out var val) && val >= vbar.Minimum && val <= vbar.Maximum) vbar.Value = val; };

			var hvalbox = new TextBox();
			hvalbox.Dock = DockStyle.Top;
			vvalbox.AutoSize = true;
			hvalbox.MinimumSize = new Size(50, 21);
			hvalbox.TextChanged += (s, e) => { if (int.TryParse(hvalbox.Text, out var val) && val >= hbar.Minimum && val <= hbar.Maximum) hbar.Value = val; };

			mainPanel.Controls.Add(vbar);
			mainPanel.Controls.Add(hbar);
			mainPanel.Controls.Add(vlabel);
			mainPanel.Controls.Add(hlabel);
			mainPanel.Controls.Add(vvalbox);
			mainPanel.Controls.Add(hvalbox);

			vbar.ValueChanged += (s, e) => { vlabel.Text = $"vbar [{vbar.Minimum}, {vbar.Maximum}], largeChange:{vbar.LargeChange}, value:{vbar.Value}"; };
			hbar.ValueChanged += (s, e) => { hlabel.Text = $"hbar [{hbar.Minimum}, {hbar.Maximum}], largeChange:{hbar.LargeChange}, value:{hbar.Value}"; };

			// TablesForm
			MinimumSize = new Size(100, 100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(480, 320);
			Controls.Add(mainPanel);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "TablesForm";
			Padding = new Padding(6);
			Text = "TablesForm";

			mainPanel.ResumeLayout(true);
			ResumeLayout(false);
			PerformLayout();
		}

		private void Vbar_ValueChanged(object sender, EventArgs e) => throw new NotImplementedException();

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

	}
}

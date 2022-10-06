using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace FormsTest
{
	public partial class AutoSizeForm : Form
	{
		FlowLayoutPanel flowPanel;
		Label label;
		TextBox textBox;
		Button button;

		public AutoSizeForm()
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			this.AutoSize = true;
			this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.Text = "AutoSize Form";

			flowPanel = new FlowLayoutPanel();
			flowPanel.AutoSize = true;
			flowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.Controls.Add(flowPanel);

			label = new Label();
			label.Name = "label";
			label.Text = "Text:";
			label.Width = 50;
			label.TextAlign = ContentAlignment.MiddleCenter;
			flowPanel.Controls.Add(label);

			textBox = new TextBox();
			textBox.Name = "textBox";
			textBox.Width = 250;
			flowPanel.Controls.Add(textBox);

			button = new Button();
			button.Name = "button";
			button.Text = "Change direction";
			button.AutoSize = true;
			button.Click += new EventHandler(button_Click);
			flowPanel.Controls.Add(button);
		}

		void button_Click(object sender, EventArgs e)
		{
			flowPanel.FlowDirection = flowPanel.FlowDirection == FlowDirection.TopDown
				? FlowDirection.LeftToRight
				: FlowDirection.TopDown;
				return;

		}
	}
}

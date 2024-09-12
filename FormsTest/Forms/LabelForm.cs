using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Diagnostics;

namespace FormsTest
{
	public class LabelForm : Form
	{
		bool loaded;
		FlowLayoutPanel panel;
		Label textLabel;
		TextBox textBox;
		Label fontLabel;

		Label propertiesLabel;
		FlowLayoutPanel propertiesPanel;
		CheckBox labelExCheckBox;
		CheckBox autoSizeCheckBox;
		CheckBox autoEllipsisCheckBox;
		CheckBox useMnemonicCheckBox;
		CheckBox useCompatibleTextRenderingCheckBox;

		CheckBox noPaddingCheckBox;
		CheckBox noClippingCheckBox;
		CheckBox macTextAlignmentCheckBox;
		List<RadioButton> textAlignRadioButtons;

		NumericUpDown paddingNumericUpDown;



		Font font = new Font("Helvetica", 10, FontStyle.Bold);

		LabelDetailForm detail;

		public LabelForm()
		{
			this.SuspendLayout();

			this.AutoSize = false;
			this.Text = "Draw Text";
			this.Font = new Font("Helvetica", 10, FontStyle.Regular);
			this.Padding = new Padding(12);
			this.Size = new Size(660, 660);

			panel = new FlowLayoutPanel();
			panel.FlowDirection = FlowDirection.TopDown;
			panel.Dock = DockStyle.Fill;
			this.Controls.Add(panel);

			FlowLayoutPanel textPanel;
			textPanel = new FlowLayoutPanel();
			textPanel.AutoSize = true;
			textPanel.FlowDirection = FlowDirection.TopDown;
			panel.Controls.Add(textPanel);

			textLabel = new Label();
			textLabel.Name = "textLabel";
			textLabel.Text = "Text:";
			textLabel.AutoSize = true;
			textPanel.Controls.Add(textLabel);

			textBox = new TextBox();
			textBox.Name = "textBox";
			textBox.Multiline = true;
			textBox.Height = 120;
			textBox.Width = 320;
			textBox.Text = "Švagr & bagr";
			textBox.TextChanged += textBox_textChanged;
			textBox.ScrollBars = ScrollBars.Vertical;
			textPanel.Controls.Add(textBox);

			fontLabel = new Label();
			fontLabel.AutoSize = true;
			fontLabel.Text = font.ToString();
			textPanel.Controls.Add(fontLabel);

			var fontButton = new Button();
			fontButton.Text = "Font";
			fontButton.AutoSize = true;
			fontButton.Click += fontButton_Click;
			textPanel.Controls.Add(fontButton);

			// TextFormatFlags

			propertiesPanel = new FlowLayoutPanel();
			propertiesPanel.FlowDirection = FlowDirection.TopDown;
			propertiesPanel.AutoSize = true;
			panel.Controls.Add(propertiesPanel);

			propertiesLabel = new Label();
			propertiesLabel.Text = "Label Properties:";
			propertiesLabel.AutoSize = true;
			propertiesPanel.Controls.Add(propertiesLabel);

			labelExCheckBox = new CheckBox();
			labelExCheckBox.Text = "LabelEx";
			labelExCheckBox.AutoSize = true;
			labelExCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(labelExCheckBox);

			autoSizeCheckBox = new CheckBox();
			autoSizeCheckBox.Text = "Auto Size";
			autoSizeCheckBox.AutoSize = true;
			autoSizeCheckBox.Checked = true;
			autoSizeCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(autoSizeCheckBox);

			autoEllipsisCheckBox = new CheckBox();
			autoEllipsisCheckBox.Text = "Auto Ellipsis";
			autoEllipsisCheckBox.AutoSize = true;
			autoEllipsisCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(autoEllipsisCheckBox);

			useMnemonicCheckBox = new CheckBox();
			useMnemonicCheckBox.Text = "Use Mnemonic";
			useMnemonicCheckBox.AutoSize = true;
			useMnemonicCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(useMnemonicCheckBox);

			useCompatibleTextRenderingCheckBox = new CheckBox();
			useCompatibleTextRenderingCheckBox.Text = "Use Compatible Text Rendering";
			useCompatibleTextRenderingCheckBox.AutoSize = true;
			useCompatibleTextRenderingCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(useCompatibleTextRenderingCheckBox);

			var exLabel = new Label();
			exLabel.Text = "LabelEx Properties:";
			exLabel.AutoSize = true;
			exLabel.Padding = new Padding(0, 5, 0, 0);
			propertiesPanel.Controls.Add(exLabel);

			noPaddingCheckBox = new CheckBox();
			noPaddingCheckBox.Text = "NoPadding";
			noPaddingCheckBox.AutoSize = true;
			noPaddingCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(noPaddingCheckBox);

			noClippingCheckBox = new CheckBox();
			noClippingCheckBox.Text = "NoClipping";
			noClippingCheckBox.AutoSize = true;
			noClippingCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(noClippingCheckBox);

			macTextAlignmentCheckBox = new CheckBox();
			macTextAlignmentCheckBox.Text = "MacTextAlignment.Right";
			macTextAlignmentCheckBox.AutoSize = true;
			macTextAlignmentCheckBox.CheckedChanged += propertyCheckBox_CheckedChanged;
			propertiesPanel.Controls.Add(macTextAlignmentCheckBox);

			var alignmentLabel = new Label();
			alignmentLabel.Text = "Content Alignment:";
			alignmentLabel.AutoSize = true;
			alignmentLabel.Padding = new Padding(0, 5, 0, 0);
			propertiesPanel.Controls.Add(alignmentLabel);

			textAlignRadioButtons = new List<RadioButton>();
			foreach (string name in Enum.GetNames(typeof(ContentAlignment)))
			{
				var rb = new RadioButton();
				textAlignRadioButtons.Add(rb);
				rb.AutoSize = true;
				rb.Text = name;
				rb.CheckedChanged += propertyCheckBox_CheckedChanged;
				propertiesPanel.Controls.Add(rb);
			}
			textAlignRadioButtons[0].Checked = true;

			var addingLabel = new Label();
			addingLabel.Text = "Padding:";
			addingLabel.AutoSize = true;
			addingLabel.Padding = new Padding(0, 5, 0, 0);
			propertiesPanel.Controls.Add(addingLabel);

			paddingNumericUpDown = new NumericUpDown();
			paddingNumericUpDown.Value = 0;
			paddingNumericUpDown.AutoSize = true;
			paddingNumericUpDown.MaximumSize = new Size(80, 30);
			paddingNumericUpDown.ValueChanged += nud_ValueChanged;
			paddingNumericUpDown.Maximum = 100000;
			propertiesPanel.Controls.Add(paddingNumericUpDown);

			this.ResumeLayout(false);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			detail = new LabelDetailForm();
			detail.Location = new Point(this.Bounds.Right + 30, this.Bounds.Top);
			detail.Show(this);

			loaded = true;
			UpdateDetail();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
		}

		void fontButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new FontDialog())
			{
				dialog.ShowColor = true;
				dialog.ShowEffects = true;
				dialog.Color = Color.Green;
				dialog.Font = font;

				if (DialogResult.OK == dialog.ShowDialog())
				{
					font = dialog.Font;
					fontLabel.Text = font.ToString();
					UpdateDetail();
				}
			}
		}

		void button_Click(object sender, EventArgs e)
		{
			UpdateDetail();
		}

		void textBox_textChanged(object sender, EventArgs e)
		{
			UpdateDetail();
		}

		void propertyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateDetail();
		}

		void nud_ValueChanged(object sender, EventArgs e)
		{
			UpdateDetail();
		}

		void UpdateDetail()
		{
			if (!loaded)
				return;

			Label label = labelExCheckBox.Checked ? new LabelEx() : new Label();

			label.Font = font;
			label.BackColor = Color.Salmon;
			label.Text = textBox.Text;
			label.TextAlign = GetAlignment(textAlignRadioButtons);
			label.AutoEllipsis = autoEllipsisCheckBox.Checked;
			label.UseMnemonic = useMnemonicCheckBox.Checked;
			label.UseCompatibleTextRendering = useCompatibleTextRenderingCheckBox.Checked;
			label.AutoSize = autoSizeCheckBox.Checked;
			label.Dock = label.AutoSize ? DockStyle.None : DockStyle.Fill;
			var p = Convert.ToInt32(paddingNumericUpDown.Value);
			label.Padding = new Padding(p, p, p, p);

			if (label is LabelEx ex)
			{
				ex.NoPadding = noPaddingCheckBox.Checked;
				ex.NoClipping = noClippingCheckBox.Checked;
				ex.MacAlignment = macTextAlignmentCheckBox.Checked ? LabelEx.MacTextAlignment.Right : LabelEx.MacTextAlignment.Left;
			}

			detail.Update(label);
		}

		ContentAlignment GetAlignment(List<RadioButton> buttons)
		{
			var index = GetIndexOfFirstChecked(buttons);
			var values = Enum.GetValues<ContentAlignment>();
			return values[Math.Max(0, index)];
		}

		int GetIndexOfFirstChecked(List<RadioButton> buttons)
		{
			int i;
			for (i = 0; i < buttons.Count; ++i)
				if (buttons[i].Checked)
					return i;
			return -1;
		}
	}

	class LabelDetailForm : Form
	{
		Panel panel;
		Label label;
		Label line;
		Label label2;

		Brush textBrush = new SolidBrush(Color.Black);
		Brush backBrush = new SolidBrush(Color.Beige);

		public LabelDetailForm()
		{
			this.SuspendLayout();

			this.AutoSize = false;
			this.Text = "Labels";
			this.Padding = new Padding(10);

			panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.Paint += panel_Paint;
			panel.Resize += (_, _) => Update(label);
			this.Controls.Add(panel);

			line = new Label();
			line.AutoSize = false;
			line.BorderStyle = BorderStyle.Fixed3D;
			line.Height = 1;
			//line.BackColor = Color.FromArgb(128,128,128,128);
			panel.Controls.Add(line);

			this.ResumeLayout(false);
		}

		public void Update(Label label)
		{
			panel.Invalidate();
			if (this.label != null)
				panel.Controls.Remove(this.label);

			if (label != null)
			{
				panel.Controls.Add(this.label = label);

				panel.Controls.Remove(this.line);
				var b = (int)(label.GetBaselineOffset() + 0.5f);
				line.Bounds = new Rectangle(0, label.Top + b, panel.Width, 1);
				panel.Controls.Add(this.line);

				UpdateLabel2();

				panel.Controls.Remove(this.label);
				panel.Controls.Add(this.label);
			}
		}

		void UpdateLabel2()
		{
			if (label2 != null)
			{
				label2.Parent?.Controls.Remove(label2);
				label2.Dispose();
			}

			label2 = new LabelEx();
			label2.Text = "Švagr & bagr";
			label2.AutoSize = true;
			label2.Padding = new Padding(3,3,3,3);
			label2.TextAlign = ContentAlignment.TopLeft;
			panel.Controls.Add(this.label2);
			label2.Location = new Point(
				panel.Width - label2.Width,
				label.Top + (int)Math.Round((label.GetBaselineOffset()) - (int)Math.Round(label2.GetBaselineOffset())));
		}

		void panel_Paint(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			var clip = e.ClipRectangle;
			g.FillRectangle(backBrush, clip);

			var b = label.GetBaselineOffset();
			using var pen = new Pen(Color.LightGreen);
			e.Graphics.DrawLine(pen, (float)e.ClipRectangle.Left, (float)label.Top + b, (float)e.ClipRectangle.Right, (float)label.Top + b);
		}
	}
}

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace FormsTest
{
	public class DrawStringForm : Form
	{
		bool loaded;
		FlowLayoutPanel panel;
		Label label;
		TextBox textBox;
		Label fontLabel;

		Label alignmentLabel;
		RadioButton alignmentCenterRadioButton;
		RadioButton alignmentNearRadioButton;
		RadioButton alignmentFarRadioButton;

		Label formatFlagsLabel;
		FlowLayoutPanel formatFlagsPanel;
		CheckBox directionRightToLeftCheckBox;
		CheckBox directionVerticalCheckBox;
		CheckBox displayFormatControlCheckBox;
		CheckBox fitBlackBoxCheckBox;
		CheckBox lineLimitCheckBox;
		CheckBox measureTrailingSpacesCheckBox;
		CheckBox noClipCheckBox;
		CheckBox noFontFallbackCheckBox;
		CheckBox noWrapCheckBox;

		// Trimming 
		Label trimmingLabel;
		FlowLayoutPanel trimmingPanel;
		RadioButton trimmingNoneRadioButton;
		RadioButton trimmingCharacterRadioButton;
		RadioButton trimmingWordRadioButton;
		RadioButton trimmingEllipsisCharacter;
		RadioButton trimmingEllipsisWord;
		RadioButton trimmingEllipsisPath;

		NumericUpDown nudWidth;
		NumericUpDown nudHeight;
		Label prefSizeLabel;
	

		Font font = new Font("Helvetica", 10, FontStyle.Bold);

		DrawStringDetailForm detail;

		public DrawStringForm()
		{
			this.SuspendLayout();

			this.AutoSize = false;
			this.Text = "Draw String";
			this.Font = new Font("Helvetica", 10, FontStyle.Regular);
			this.Padding = new Padding(12);
			this.Size = new Size(780, 440);

			panel = new FlowLayoutPanel();
			panel.FlowDirection = FlowDirection.TopDown;
			panel.Dock = DockStyle.Fill;
			this.Controls.Add(panel);

			FlowLayoutPanel textPanel;
			textPanel = new FlowLayoutPanel();
			textPanel.AutoSize = true;
			textPanel.FlowDirection = FlowDirection.TopDown;
			panel.Controls.Add(textPanel);

			label = new Label();
			label.Name = "textLabel";
			label.Text = "Text";
			label.AutoSize = true;
			textPanel.Controls.Add(label);

			textBox = new TextBox();
			textBox.Name = "textBox";
			textBox.Multiline = true;
			textBox.Height = 320;
			textBox.Width = 320;
			textBox.Text = "Encapsulates text layout information (such as alignment, orientation and tab stops) display manipulations (such as ellipsis insertion and national digit substitution) and OpenType features. This class cannot be inherited. \n \n Encapsulates text layout information (such as alignment, orientation and tab stops) display manipulations (such as ellipsis insertion and national digit substitution) and OpenType features. This class cannot be inherited.";
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

			// Alignment

			var alignmentPanel = new FlowLayoutPanel();
			alignmentPanel.AutoSize = true;
			alignmentPanel.FlowDirection = FlowDirection.TopDown;
			panel.Controls.Add(alignmentPanel);

			alignmentLabel = new Label();
			alignmentLabel.Text = "Alignment";
			alignmentLabel.AutoSize = true;
			alignmentPanel.Controls.Add(alignmentLabel);

			alignmentNearRadioButton = new RadioButton();
			alignmentNearRadioButton.AutoSize = true;
			alignmentNearRadioButton.Checked = true;
			alignmentNearRadioButton.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			alignmentNearRadioButton.Text = "Near";
			alignmentPanel.Controls.Add(alignmentNearRadioButton);

			alignmentCenterRadioButton = new RadioButton();
			alignmentCenterRadioButton.AutoSize = true;
			alignmentCenterRadioButton.Text = "Center";
			alignmentCenterRadioButton.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			alignmentPanel.Controls.Add(alignmentCenterRadioButton);

			alignmentFarRadioButton = new RadioButton();
			alignmentFarRadioButton.AutoSize = true;
			alignmentFarRadioButton.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			alignmentFarRadioButton.Text = "Far";
			alignmentPanel.Controls.Add(alignmentFarRadioButton);

			// Trimming 

			trimmingPanel = new FlowLayoutPanel();
			trimmingPanel.FlowDirection = FlowDirection.TopDown;
			trimmingPanel.AutoSize = true;
			panel.Controls.Add(trimmingPanel);

			trimmingLabel = new Label();
			trimmingLabel.Text = "Trimming";
			trimmingLabel.AutoSize = true;
			trimmingPanel.Controls.Add(trimmingLabel);

			trimmingNoneRadioButton = new RadioButton();
			trimmingNoneRadioButton.AutoSize = true;
			trimmingNoneRadioButton.Checked = true;
			trimmingNoneRadioButton.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			trimmingNoneRadioButton.Text = "None";
			trimmingPanel.Controls.Add(trimmingNoneRadioButton);

			trimmingCharacterRadioButton = new RadioButton();
			trimmingCharacterRadioButton.AutoSize = true;
			trimmingCharacterRadioButton.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			trimmingCharacterRadioButton.Text = "Character";
			trimmingPanel.Controls.Add(trimmingCharacterRadioButton);

			trimmingWordRadioButton = new RadioButton();
			trimmingWordRadioButton.AutoSize = true;
			trimmingWordRadioButton.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			trimmingWordRadioButton.Text = "Word";
			trimmingPanel.Controls.Add(trimmingWordRadioButton);

			trimmingEllipsisCharacter = new RadioButton();
			trimmingEllipsisCharacter.AutoSize = true;
			trimmingEllipsisCharacter.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			trimmingEllipsisCharacter.Text = "EllipsisCharacter";
			trimmingPanel.Controls.Add(trimmingEllipsisCharacter);

			trimmingEllipsisWord = new RadioButton();
			trimmingEllipsisWord.AutoSize = true;
			trimmingEllipsisWord.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			trimmingEllipsisWord.Text = "EllipsisWord";
			trimmingPanel.Controls.Add(trimmingEllipsisWord);

			trimmingEllipsisPath = new RadioButton();
			trimmingEllipsisPath.AutoSize = true;
			trimmingEllipsisPath.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			trimmingEllipsisPath.Text = "EllipsisPath";
			trimmingPanel.Controls.Add(trimmingEllipsisPath);

			// Format Flags

			formatFlagsPanel = new FlowLayoutPanel();
			formatFlagsPanel.FlowDirection = FlowDirection.TopDown;
			formatFlagsPanel.AutoSize = true;
			panel.Controls.Add(formatFlagsPanel);

			formatFlagsLabel = new Label();
			formatFlagsLabel.Text = "Format Flags";
			formatFlagsLabel.AutoSize = true;
			formatFlagsPanel.Controls.Add(formatFlagsLabel);

			directionRightToLeftCheckBox = new CheckBox();
			directionRightToLeftCheckBox.Text = "DirectionRightToLeft";
			directionRightToLeftCheckBox.AutoSize = true;
			directionRightToLeftCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(directionRightToLeftCheckBox);

			directionVerticalCheckBox = new CheckBox();
			directionVerticalCheckBox.Text = "DirectionVertical";
			directionVerticalCheckBox.AutoSize = true;
			directionVerticalCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(directionVerticalCheckBox);

			displayFormatControlCheckBox = new CheckBox();
			displayFormatControlCheckBox.Text = "DisplayFormatControl";
			displayFormatControlCheckBox.AutoSize = true;
			displayFormatControlCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(displayFormatControlCheckBox);

			fitBlackBoxCheckBox = new CheckBox();
			fitBlackBoxCheckBox.Text = "FitBlackBox";
			fitBlackBoxCheckBox.AutoSize = true;
			fitBlackBoxCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(fitBlackBoxCheckBox);

			lineLimitCheckBox = new CheckBox();
			lineLimitCheckBox.Text = "LineLimit";
			lineLimitCheckBox.AutoSize = true;
			lineLimitCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(lineLimitCheckBox);

			measureTrailingSpacesCheckBox = new CheckBox();
			measureTrailingSpacesCheckBox.Text = "MeasureTrailingSpaces";
			measureTrailingSpacesCheckBox.AutoSize = true;
			measureTrailingSpacesCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(measureTrailingSpacesCheckBox);

			noClipCheckBox = new CheckBox();
			noClipCheckBox.Text = "NoClip";
			noClipCheckBox.AutoSize = true;
			noClipCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(noClipCheckBox);

			noFontFallbackCheckBox = new CheckBox();
			noFontFallbackCheckBox.Text = "NoFontFallback";
			noFontFallbackCheckBox.AutoSize = true;
			noFontFallbackCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(noFontFallbackCheckBox);

			noWrapCheckBox = new CheckBox();
			noWrapCheckBox.Text = "NoWrap";
			noWrapCheckBox.AutoSize = true;
			noWrapCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(noWrapCheckBox);

			var whPanel = new FlowLayoutPanel();
			whPanel.FlowDirection = FlowDirection.LeftToRight;
			whPanel.AutoSize = true;
			panel.Controls.Add(whPanel);

			var whLabel = new Label();
			whLabel.Text = "Proposed Size:";
			whLabel.AutoSize = true;
			whPanel.Controls.Add(whLabel);

			nudWidth = new NumericUpDown();
			nudWidth.Value = 40;
			nudWidth.AutoSize = true;
			nudWidth.MaximumSize = new Size(80,30);
			nudWidth.ValueChanged += nud_ValueChanged;
			nudWidth.Maximum = 100000;
			whPanel.Controls.Add(nudWidth);

			nudHeight = new NumericUpDown();
			nudHeight.Value = 0;
			nudHeight.AutoSize = true;
			nudHeight.MaximumSize = new Size(80,30);
			nudHeight.ValueChanged += nud_ValueChanged;
			nudHeight.Maximum = 100000;
			whPanel.Controls.Add(nudHeight);

			prefSizeLabel = new Label();
			prefSizeLabel.AutoSize = true;
			panel.Controls.Add(prefSizeLabel);

			this.ResumeLayout(false);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			detail = new DrawStringDetailForm();
			detail.Location = new Point(this.Bounds.Right + 30, this.Bounds.Top);
			detail.Show(this);

			loaded = true;
			UpdateDetail();
			MeasureText();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
		}

		void nud_ValueChanged(object sender, EventArgs e)
		{
			MeasureText();
		}

		void MeasureText()
		{
			var flags = TextFormatFlags.Default | TextFormatFlags.WordBreak;
			var proposed = new Size((int)nudWidth.Value, (int)nudHeight.Value);
			var preferred = TextRenderer.MeasureText(textBox.Text, textBox.Font, proposed, flags);
			prefSizeLabel.Text = $"Measure Text:[{preferred.Width},{preferred.Height}]"; 

			var format = GetFormat();
			using var g = CreateGraphics();
			preferred = g.MeasureString(textBox.Text, font, new SizeF(proposed.Width, proposed.Height), format).ToSize();
			prefSizeLabel.Text += $"\nMesure String:[{preferred.Width},{preferred.Height}]"; 

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
					MeasureText();
				}
			}
		}

		void button_Click(object sender, EventArgs e)
		{
			UpdateDetail();
			MeasureText();
		}

		void textBox_textChanged(object sender, EventArgs e)
		{
			UpdateDetail();
			MeasureText();
		}

		void formatFlagsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateDetail();
			MeasureText();
		}
		void UpdateDetail()
		{
			if (!loaded)
				return;

			var format = GetFormat();
			detail.Update(textBox.Text, format, font);
		}

		StringFormat GetFormat()
		{
			var format = new StringFormat();

			if (alignmentNearRadioButton.Checked) format.Alignment = StringAlignment.Near;
			if (alignmentFarRadioButton.Checked) format.Alignment = StringAlignment.Far;
			if (alignmentCenterRadioButton.Checked) format.Alignment = StringAlignment.Center;
			
			if (directionRightToLeftCheckBox.Checked) format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			if (directionVerticalCheckBox.Checked) format.FormatFlags |= StringFormatFlags.DirectionVertical;
			if (displayFormatControlCheckBox.Checked) format.FormatFlags |= StringFormatFlags.DisplayFormatControl;
			if (fitBlackBoxCheckBox.Checked) format.FormatFlags |= StringFormatFlags.FitBlackBox;
			if (lineLimitCheckBox.Checked) format.FormatFlags |= StringFormatFlags.LineLimit;
			if (measureTrailingSpacesCheckBox.Checked) format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			if (noClipCheckBox.Checked) format.FormatFlags |= StringFormatFlags.NoClip;
			if (noFontFallbackCheckBox.Checked) format.FormatFlags |= StringFormatFlags.NoFontFallback;
			if (noWrapCheckBox.Checked) format.FormatFlags |= StringFormatFlags.NoWrap;

			if (trimmingNoneRadioButton.Checked) format.Trimming = StringTrimming.None;
			if (trimmingCharacterRadioButton.Checked) format.Trimming = StringTrimming.Character;
			if (trimmingWordRadioButton.Checked) format.Trimming = StringTrimming.Word;
			if (trimmingEllipsisCharacter.Checked) format.Trimming = StringTrimming.EllipsisCharacter;
			if (trimmingEllipsisWord.Checked) format.Trimming = StringTrimming.EllipsisWord;
			if (trimmingEllipsisPath.Checked) format.Trimming = StringTrimming.EllipsisPath;

			return format;
		}
	}

	class DrawStringDetailForm : Form
	{
		Panel panel;

		string text = "";
		StringFormat format = new StringFormat();
		Font font = new Font("Helvetica", 10, FontStyle.Bold);
		Brush textBrush = new SolidBrush(Color.Black);
		Brush backBrush = new SolidBrush(Color.Beige);

		public DrawStringDetailForm()
		{
			this.SuspendLayout();

			this.AutoSize = false;
			this.Text = "Draw String";
			this.Padding = new Padding(10);

			panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.Paint += panel_Paint;
			panel.Resize += (_, _) => panel.Invalidate();
			this.Controls.Add(panel);

			this.ResumeLayout(false);
		}

		public void Update(string text, StringFormat format, Font font)
		{
			this.text = text;
			this.format = format;
			this.font = font;

			panel.Invalidate();
		}

		void panel_Paint(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			var clip = e.ClipRectangle;
			g.FillRectangle(backBrush, clip);
			
			g.DrawString(text, font, textBrush, panel.ClientRectangle, format);

			using var pen = new Pen(Color.LightGreen);
			var (a, d, h) = panel.GetFontOffsetsScaled(font);
			e.Graphics.DrawLine(pen, (float)e.ClipRectangle.Left, a, (float)e.ClipRectangle.Right, a);
		}
	}
}

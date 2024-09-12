using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace FormsTest
{
	public class DrawTextForm : Form
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
		CheckBox tffLeftCheckBox;
		CheckBox tffTopCheckBox;
		CheckBox tffDefaultCheckBox;
		CheckBox tffGlyphOverhangPaddingCB;
		CheckBox tffHorizontalCenterCB;
		CheckBox tffRightCB;
		CheckBox tffVerticalCenterCB;
		CheckBox tffBottomCB;
		CheckBox tffWordBreakCB;
		CheckBox tffSingleLineCB;
		CheckBox tffExpandTabsCB;
		CheckBox tffNoClippingCB;
		CheckBox tffExternalLeadingCB;
		CheckBox tffNoPrefixCB;
		CheckBox tffInternalCB;
		CheckBox tffTextBoxControlCB;
		CheckBox tffPathEllipsisCB;
		CheckBox tffEndEllipsisCB;
		CheckBox tffModifyStringCB;
		CheckBox tffRightToLeftCB;
		CheckBox tffWordEllipsisCB;
		CheckBox tffNoFullWidthCharacterBreakCB;
		CheckBox tffHidePrefixCB;
		CheckBox tffPrefixOnlyCB;
		CheckBox tffPreserveGraphicsClippingCB;
		CheckBox tffPreserveGraphicsTranslateTransformCB;
		CheckBox tffLeftAndRightPaddingCB;
		CheckBox tffNoPaddingCB;

		NumericUpDown nudWidth;
		NumericUpDown nudHeight;
		Label prefSizeLabel;
	

		Font font = new Font("Helvetica", 10, FontStyle.Bold);

		DrawTextDetailForm detail;

		public DrawTextForm()
		{
			this.SuspendLayout();

			this.AutoSize = false;
			this.Text = "Draw Text";
			this.Font = new Font("Helvetica", 10, FontStyle.Regular);
			this.Padding = new Padding(12);
			this.Size = new Size(660,840);

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

			// TextFormatFlags

			formatFlagsPanel = new FlowLayoutPanel();
			formatFlagsPanel.FlowDirection = FlowDirection.TopDown;
			formatFlagsPanel.AutoSize = true;
			panel.Controls.Add(formatFlagsPanel);

			formatFlagsLabel = new Label();
			formatFlagsLabel.Text = "Text Format Flags";
			formatFlagsLabel.AutoSize = true;
			formatFlagsPanel.Controls.Add(formatFlagsLabel);

			tffLeftCheckBox = new CheckBox();
			tffLeftCheckBox.Text = "Left";
			tffLeftCheckBox.AutoSize = true;
			tffLeftCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffLeftCheckBox);

			tffTopCheckBox = new CheckBox();
			tffTopCheckBox.Text = "Top";
			tffTopCheckBox.AutoSize = true;
			tffTopCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffTopCheckBox);

			tffDefaultCheckBox = new CheckBox();
			tffDefaultCheckBox.Text = "Default";
			tffDefaultCheckBox.AutoSize = true;
			tffDefaultCheckBox.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffDefaultCheckBox);

			tffGlyphOverhangPaddingCB = new CheckBox();
			tffGlyphOverhangPaddingCB.Text = "GlyphOverhangPadding";
			tffGlyphOverhangPaddingCB.AutoSize = true;
			tffGlyphOverhangPaddingCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffGlyphOverhangPaddingCB);

			tffHorizontalCenterCB = new CheckBox();
			tffHorizontalCenterCB.Text = "HorizontalCenter";
			tffHorizontalCenterCB.AutoSize = true;
			tffHorizontalCenterCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffHorizontalCenterCB);

			tffRightCB = new CheckBox();
			tffRightCB.Text = "Right";
			tffRightCB.AutoSize = true;
			tffRightCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffRightCB);

			tffVerticalCenterCB = new CheckBox();
			tffVerticalCenterCB.Text = "VerticalCenter";
			tffVerticalCenterCB.AutoSize = true;
			tffVerticalCenterCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffVerticalCenterCB);

			tffBottomCB = new CheckBox();
			tffBottomCB.Text = "Bottom";
			tffBottomCB.AutoSize = true;
			tffBottomCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffBottomCB);

			tffWordBreakCB = new CheckBox();
			tffWordBreakCB.Text = "WordBreak";
			tffWordBreakCB.AutoSize = true;
			tffWordBreakCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffWordBreakCB);

			tffSingleLineCB = new CheckBox();
			tffSingleLineCB.Text = "SingleLine";
			tffSingleLineCB.AutoSize = true;
			tffSingleLineCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffSingleLineCB);

			tffExpandTabsCB = new CheckBox();
			tffExpandTabsCB.Text = "ExpandTabs";
			tffExpandTabsCB.AutoSize = true;
			tffExpandTabsCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffExpandTabsCB);

			tffNoClippingCB = new CheckBox();
			tffNoClippingCB.Text = "NoClipping";
			tffNoClippingCB.AutoSize = true;
			tffNoClippingCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffNoClippingCB);

			tffExternalLeadingCB = new CheckBox();
			tffExternalLeadingCB.Text = "ExternalLeading";
			tffExternalLeadingCB.AutoSize = true;
			tffExternalLeadingCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffExternalLeadingCB);

			tffNoPrefixCB = new CheckBox();
			tffNoPrefixCB.Text = "NoPrefix";
			tffNoPrefixCB.AutoSize = true;
			tffNoPrefixCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffNoPrefixCB);

			tffInternalCB = new CheckBox();
			tffInternalCB.Text = "Internal";
			tffInternalCB.AutoSize = true;
			tffInternalCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffInternalCB);

			tffTextBoxControlCB = new CheckBox();
			tffTextBoxControlCB.Text = "TextBoxControl";
			tffTextBoxControlCB.AutoSize = true;
			tffTextBoxControlCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffTextBoxControlCB);

			tffPathEllipsisCB = new CheckBox();
			tffPathEllipsisCB.Text = "PathEllipsis";
			tffPathEllipsisCB.AutoSize = true;
			tffPathEllipsisCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffPathEllipsisCB);

			tffEndEllipsisCB = new CheckBox();
			tffEndEllipsisCB.Text = "EndEllipsis";
			tffEndEllipsisCB.AutoSize = true;
			tffEndEllipsisCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffEndEllipsisCB);

			tffModifyStringCB = new CheckBox();
			tffModifyStringCB.Text = "ModifyString";
			tffModifyStringCB.AutoSize = true;
			tffModifyStringCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffModifyStringCB);

			tffRightToLeftCB = new CheckBox();
			tffRightToLeftCB.Text = "RightToLeft";
			tffRightToLeftCB.AutoSize = true;
			tffRightToLeftCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffRightToLeftCB);

			tffWordEllipsisCB = new CheckBox();
			tffWordEllipsisCB.Text = "WordEllipsis";
			tffWordEllipsisCB.AutoSize = true;
			tffWordEllipsisCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffWordEllipsisCB);

			tffNoFullWidthCharacterBreakCB = new CheckBox();
			tffNoFullWidthCharacterBreakCB.Text = "NoFullWidthCharacterBreak";
			tffNoFullWidthCharacterBreakCB.AutoSize = true;
			tffNoFullWidthCharacterBreakCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffNoFullWidthCharacterBreakCB);

			tffHidePrefixCB = new CheckBox();
			tffHidePrefixCB.Text = "HidePrefix";
			tffHidePrefixCB.AutoSize = true;
			tffHidePrefixCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffHidePrefixCB);

			tffPrefixOnlyCB = new CheckBox();
			tffPrefixOnlyCB.Text = "PrefixOnly";
			tffPrefixOnlyCB.AutoSize = true;
			tffPrefixOnlyCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffPrefixOnlyCB);

			tffPreserveGraphicsClippingCB = new CheckBox();
			tffPreserveGraphicsClippingCB.Text = "PreserveGraphicsClipping";
			tffPreserveGraphicsClippingCB.AutoSize = true;
			tffPreserveGraphicsClippingCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffPreserveGraphicsClippingCB);

			tffPreserveGraphicsTranslateTransformCB = new CheckBox();
			tffPreserveGraphicsTranslateTransformCB.Text = "PreserveGraphicsTranslateTransform";
			tffPreserveGraphicsTranslateTransformCB.AutoSize = true;
			tffPreserveGraphicsTranslateTransformCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffPreserveGraphicsTranslateTransformCB);

			tffLeftAndRightPaddingCB = new CheckBox();
			tffLeftAndRightPaddingCB.Text = "LeftAndRightPadding";
			tffLeftAndRightPaddingCB.AutoSize = true;
			tffLeftAndRightPaddingCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffLeftAndRightPaddingCB);

			tffNoPaddingCB = new CheckBox();
			tffNoPaddingCB.Text = "NoPadding";
			tffNoPaddingCB.AutoSize = true;
			tffNoPaddingCB.CheckedChanged += formatFlagsCheckBox_CheckedChanged;
			formatFlagsPanel.Controls.Add(tffNoPaddingCB);

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

			detail = new DrawTextDetailForm();
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
			var flags = GetTextFormatFlags();
			var proposed = new Size((int)nudWidth.Value, (int)nudHeight.Value);
			var preferred = TextRenderer.MeasureText(textBox.Text, font, proposed, flags);
			prefSizeLabel.Text = $"Measure Text:[{preferred.Width},{preferred.Height}]"; 
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

			var flags = GetTextFormatFlags();
			detail.Update(textBox.Text, flags, font);
		}

		TextFormatFlags GetTextFormatFlags()
		{
			var flags = new TextFormatFlags();

			if (tffLeftCheckBox.Checked) flags |= TextFormatFlags.Left;
			if (tffTopCheckBox.Checked) flags |= TextFormatFlags.Top;
			if (tffDefaultCheckBox.Checked) flags |= TextFormatFlags.Default;
			if (tffGlyphOverhangPaddingCB.Checked) flags |= TextFormatFlags.GlyphOverhangPadding;
			if (tffHorizontalCenterCB.Checked) flags |= TextFormatFlags.HorizontalCenter;
			if (tffRightCB.Checked) flags |= TextFormatFlags.Right;
			if (tffVerticalCenterCB.Checked)  flags |= TextFormatFlags.VerticalCenter;
			if (tffBottomCB.Checked) flags |= TextFormatFlags.Bottom;
			if (tffWordBreakCB.Checked) flags |= TextFormatFlags.WordBreak;
			if (tffSingleLineCB.Checked) flags |= TextFormatFlags.SingleLine;
			if (tffExpandTabsCB.Checked) flags |= TextFormatFlags.ExpandTabs;
			if (tffNoClippingCB.Checked) flags |= TextFormatFlags.NoClipping;
			if (tffExternalLeadingCB.Checked) flags |= TextFormatFlags.ExternalLeading;
			if (tffNoPrefixCB.Checked) flags |= TextFormatFlags.NoPrefix;
			if (tffInternalCB.Checked) flags |= TextFormatFlags.Internal;
			if (tffTextBoxControlCB.Checked) flags |= TextFormatFlags.TextBoxControl;
			if (tffPathEllipsisCB.Checked) flags |= TextFormatFlags.PathEllipsis;
			if (tffEndEllipsisCB.Checked) flags |= TextFormatFlags.EndEllipsis;
			if (tffModifyStringCB.Checked) flags |= TextFormatFlags.ModifyString;
			if (tffRightToLeftCB.Checked) flags |= TextFormatFlags.RightToLeft;
			if (tffWordEllipsisCB.Checked) flags |= TextFormatFlags.WordEllipsis;
			if (tffNoFullWidthCharacterBreakCB.Checked) flags |= TextFormatFlags.NoFullWidthCharacterBreak;
			if (tffHidePrefixCB.Checked) flags |= TextFormatFlags.HidePrefix;
			if (tffPrefixOnlyCB.Checked) flags |= TextFormatFlags.PrefixOnly;
			if (tffPreserveGraphicsClippingCB.Checked) flags |= TextFormatFlags.PreserveGraphicsClipping;
			if (tffPreserveGraphicsTranslateTransformCB.Checked) flags |= TextFormatFlags.PreserveGraphicsTranslateTransform;
			if (tffNoPaddingCB.Checked) flags |= TextFormatFlags.NoPadding;
			if (tffLeftAndRightPaddingCB.Checked) flags |= TextFormatFlags.LeftAndRightPadding;

			return flags;
		}
	}

	class DrawTextDetailForm : Form
	{
		Panel panel;

		string text = "";
		TextFormatFlags flags = new TextFormatFlags();
		Font font = new Font("Helvetica", 10, FontStyle.Bold);
		Brush textBrush = new SolidBrush(Color.Black);
		Brush backBrush = new SolidBrush(Color.Beige);

		public DrawTextDetailForm()
		{
			this.SuspendLayout();

			this.AutoSize = false;
			this.Text = "Draw Text";
			this.Padding = new Padding(10);

			panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.Paint += panel_Paint;
			panel.Resize += (_, _) => panel.Invalidate();
			this.Controls.Add(panel);

			this.ResumeLayout(false);
		}

		public void Update(string text, TextFormatFlags flags, Font font)
		{
			this.text = text;
			this.flags = flags;
			this.font = font;

			panel.Invalidate();
		}

		void panel_Paint(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			var clip = e.ClipRectangle;
			g.FillRectangle(backBrush, clip);
			
			var client = panel.ClientRectangle;
			var size = TextRenderer.MeasureText(g, text, font, client.Size, flags);
			
			var rect = new Rectangle(client.Location, size);
			if (flags.HasFlag(TextFormatFlags.Bottom))
				rect.Y = client.Bottom - rect.Height;
			if (flags.HasFlag(TextFormatFlags.Right))
				rect.X = client.Right - rect.Width;

			using var brush = new SolidBrush(System.Drawing.Color.Salmon);
			g.FillRectangle(brush, rect);

			TextRenderer.DrawText(g, text, font, client, Color.Black, flags);

			using var pen = new Pen(Color.LightGreen);
			var (a, d, h) = panel.GetFontOffsetsScaled(font);
			e.Graphics.DrawLine(pen, (float)rect.Left, rect.Top + a, (float)rect.Right, rect.Top + a);
		}
	}
}

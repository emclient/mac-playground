using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms.Mac;
using System.Windows.Forms.Theming;
using System.Windows.Forms.resources;
using System.Drawing.Mac;
using System.Windows.Forms.Extensions.Drawing;
using System.Windows.Forms.CocoaInternal;
using AppKit;
using Foundation;
using CoreText;
using CoreGraphics;

namespace System.Windows.Forms
{

	internal class SWFCell : NSButtonCell
	{
		public bool isFocused;
		public SWFCell()
		{
		}

		public SWFCell(NSCoder coder) : base(coder)
		{
		}

		public override CGRect TitleRectForBounds(CGRect theRect)
		{
			if (AttributedTitle != null)
			{
				var size = AttributedTitle.Size;
				size.Width += 10;
				theRect.Y = (theRect.Height - size.Height) / 2.0f;
				theRect.Height = size.Height;
				theRect.X = (theRect.Width - size.Width) / 2.0f;
				theRect.Width = size.Width;
				return theRect;
			}

			if (Font != null)
			{
				theRect.Y = (theRect.Height - Font.BoundingRectForFont.Height) / 2.0f;
				theRect.Height = Font.BoundingRectForFont.Height;
			}

			return base.TitleRectForBounds(theRect);
		}

		public override void DrawWithFrame(CGRect cellFrame, NSView inView)
		{
			cellFrame = cellFrame.Move(0, 1);

			if (isFocused) // && !Highlighted)
				DrawFocusRing(cellFrame, inView);

			base.DrawWithFrame(cellFrame, inView);
		}

		public override void DrawFocusRing(CGRect cellFrameMask, NSView inControlView)
		{
			base.DrawFocusRing(cellFrameMask, inControlView);

			// Experiment
			//NSGraphicsContext.CurrentContext.SaveGraphicsState();
			////NSSetFocusRingStyle(NSFocusRingOnly);
			//var fr = this.TitleRectForBounds(cellFrameMask).Inflate(7f, 2f).Move(0, 1);
			//NSColor.SelectedTextBackground.SetStroke();
			//var path = NSBezierPath.FromRoundedRect(fr, 3.5f, 3.5f);
			//path.LineWidth = 1.5f;
			//path.Stroke();
			//NSGraphicsContext.CurrentContext.RestoreGraphicsState();
		}
	}

	internal class SWFPopUpButtonCell : NSPopUpButtonCell
	{
		public SWFPopUpButtonCell()
		{
		}

		public SWFPopUpButtonCell(NSCoder coder) : base(coder)
		{
		}

		public override void DrawWithFrame(CGRect cellFrame, NSView inView)
		{
			cellFrame = cellFrame.Move(-2, 1);
			base.DrawWithFrame(cellFrame, inView);
		}

		public override CGRect TitleRectForBounds(CGRect theRect)
		{
			// FIXME: Calculate the offset from font heights (standard vs used)

			var r = base.TitleRectForBounds(theRect);
			return r.Move(0, -1);
		}
	}

	internal class ThemeMacOS : Theme
	{		
		internal static void NotImplemented(System.Reflection.MethodBase method, object details = null)
		{
			System.Diagnostics.Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name + (details == null ? String.Empty : " (" + details.ToString() + ")"));
		}

		public override Version Version {
			get {
				return new Version(0, 1, 0, 0);
			}
		}

		/* Hardcoded colour values not exposed in the API constants in all configurations */
		protected static readonly Color arrow_color = Color.Black;
		protected static readonly Color pen_ticks_color = Color.Black;
		protected static StringFormat string_format_menu_text;
		protected static StringFormat string_format_menu_shortcut;
		protected static StringFormat string_format_menu_menubar_text;
		static ImageAttributes imagedisabled_attributes;
		Font window_border_font;
		const int SEPARATOR_HEIGHT = 6;
		const int SEPARATOR_MIN_WIDTH = 20;
		const int SM_CXBORDER = 1;
		const int SM_CYBORDER = 1;		
		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tabd
		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items

		#region	Principal Theme Methods
		public ThemeMacOS()
		{			
			ResetDefaults();
		}

		public override void ResetDefaults() {
			defaultWindowBackColor = SystemColors.Window;
			defaultWindowForeColor = SystemColors.WindowText;

			this.ColorWindow = NSColor.TextBackground.ToSDColor();
			this.ColorWindowText = NSColor.Text.ToSDColor();

			window_border_font = null;
			
			/* Menu string formats */
			string_format_menu_text = new StringFormat ();
			string_format_menu_text.LineAlignment = StringAlignment.Center;
			string_format_menu_text.Alignment = StringAlignment.Near;
			string_format_menu_text.HotkeyPrefix = HotkeyPrefix.Show;
			string_format_menu_text.SetTabStops (0f, new float [] { 50f });
			string_format_menu_text.FormatFlags |= StringFormatFlags.NoWrap;

			string_format_menu_shortcut = new StringFormat ();	
			string_format_menu_shortcut.LineAlignment = StringAlignment.Center;
			string_format_menu_shortcut.Alignment = StringAlignment.Far;

			string_format_menu_menubar_text = new StringFormat ();
			string_format_menu_menubar_text.LineAlignment = StringAlignment.Center;
			string_format_menu_menubar_text.Alignment = StringAlignment.Center;
			string_format_menu_menubar_text.HotkeyPrefix = HotkeyPrefix.Show;
		}

		public override bool DoubleBufferingSupported {
			get {return true; }
		}

		public override int HorizontalScrollBarHeight {
			get {
				return XplatUI.HorizontalScrollBarHeight;
			}
		}

		public override int VerticalScrollBarWidth {
			get {
				return XplatUI.VerticalScrollBarWidth;
			}
		}
		
		public override Font WindowBorderFont {
			get {
				return window_border_font ?? (window_border_font = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Bold));
			}
		}

		#endregion	// Principal Theme Methods

		#region	Internal Methods
		protected Brush GetControlBackBrush (Color c) {
			if (c.ToArgb () == DefaultControlBackColor.ToArgb ())
				return SystemBrushes.Control;
			return ResPool.GetSolidBrush (c);
		}

		protected Brush GetControlForeBrush (Color c) {
			if (c.ToArgb () == DefaultControlForeColor.ToArgb ())
				return SystemBrushes.ControlText;
			return ResPool.GetSolidBrush (c);
		}
		#endregion	// Internal Methods

		#region Control
		public override Font GetLinkFont (Control control) 
		{
			return new Font (control.Font.FontFamily, control.Font.Size, control.Font.Style | FontStyle.Underline, control.Font.Unit); 
		}
		#endregion	// Control

		#region OwnerDraw Support
		public  override void DrawOwnerDrawBackground (DrawItemEventArgs e)
		{
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				e.Graphics.FillRectangle (SystemBrushes.Highlight, e.Bounds);
				return;
			}

			e.Graphics.FillRectangle (ResPool.GetSolidBrush(e.BackColor), e.Bounds);
		}

		public  override void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Focus)
				CPDrawFocusRectangle (e.Graphics, e.Bounds, e.ForeColor, e.BackColor);
		}
		#endregion // OwnerDraw Support

		#region Button

		NSButtonCell GetButtonCell(ButtonBase b)
		{
			var cell = new SWFCell();
			cell.AttributedTitle = System.Drawing.Mac.Extensions.GetAttributedString(b.Text, '&', b.font, b.TextAlign);
			cell.Alignment = b.TextAlign.ToNSTextAlignment();
			cell.Highlighted = b.ButtonState == ButtonState.Pushed;
			cell.Bezeled = false;
			cell.Bordered = true;
			cell.BezelStyle = NSBezelStyle.Rounded; // When Rounded is set, the button border gets its own dimensions (smaller than we want)
			cell.Enabled = b.Enabled;
			cell.isFocused = b.Focused;
			if (b.Image != null)
				cell.Image = b.Image.ToNSImage();
			return cell;
		}

		#region Standard Button Style

		public override void DrawButton (Graphics g, Button b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			DrawButtonBackground(g, b, clipRectangle);

			using (var cell = GetButtonCell(b))
			{
				var view = NSView.FocusView();
				var frame = b.ClientRectangle.ToCGRect();
				cell.DrawWithFrame(frame, view);
			}
		}

		public virtual void DrawButtonBackground(Graphics g, ButtonBase button, Rectangle clipArea) 
		{
			var parent = button.Parent;
			if (parent != null)
			{
				var e = new PaintEventArgs(g, new Rectangle(clipArea.X + button.Left, clipArea.Y + button.Top, clipArea.Width, clipArea.Height));
				var state = e.Graphics.Save();
				e.Graphics.TranslateTransform(-button.Left, -button.Top);
				parent.PaintControlBackground(e);
				e.Graphics.Restore(state);
				e.SetGraphics(null);
			}
		}

		public virtual void DrawButtonFocus(Graphics g, Button button)
		{
			ControlPaint.DrawFocusRectangle (g, Rectangle.Inflate (button.ClientRectangle, -4, -4));
		}

		public virtual void DrawButtonImage(Graphics g, ButtonBase button, Rectangle imageBounds)
		{
			if (button.Enabled)
				g.DrawImage (button.Image, imageBounds);
			else
				CPDrawImageDisabled (g, button.Image, imageBounds.Left, imageBounds.Top, ColorControl);
		}

		public virtual void DrawButtonText (Graphics g, ButtonBase button, Rectangle textBounds)
		{
		}

		#endregion

		#region FlatStyle Button Style
		public override void DrawFlatButton(Graphics g, ButtonBase b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			NotImplemented(Reflection.MethodBase.GetCurrentMethod());
		}

		#endregion

		#region Popup Button Style

		SWFPopUpButtonCell GetPopupButtonCell(ComboBox b)
		{
			var cell = new SWFPopUpButtonCell();
			cell.Font = b.Font.ToNSFont();
			cell.Alignment = NSTextAlignment.Natural;
			cell.Highlighted = b.DroppedDown;
			cell.Bezeled = true;
			cell.Bordered = true;
			cell.BezelStyle = NSBezelStyle.Rounded;
			cell.Enabled = b.Enabled;
			cell.PullsDown = true;

			foreach (var item in b.Items)
				cell.AddItem(item.ToString());
			cell.SelectItemAt(b.SelectedIndex);
			cell.SetTitle(b.SelectedItem?.ToString() ?? b.SelectedText);

			return cell;
		}

		NSMenuItem[] GetMenuItems(ComboBox b)
		{
			throw new NotImplementedException();
		}

		public override void DrawPopupButton (Graphics g, Button b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			DrawPopupButtonBackground (g, b, clipRectangle);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawPopupButtonImage (g, b, imageBounds);

			// If we're focused, draw a focus rectangle
			if (b.Focused && b.Enabled && b.ShowFocusCues)
				DrawPopupButtonFocus (g, b);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawPopupButtonText (g, b, textBounds);
		}

		public virtual void DrawPopupButtonBackground (Graphics g, Button button, Rectangle clipArea)
		{
			if (button.Pressed)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Pressed, button.BackColor, button.ForeColor);
			else if (button.Entered)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Entered, button.BackColor, button.ForeColor);
			else if (button.InternalSelected)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Default, button.BackColor, button.ForeColor);
			else if (!button.Enabled)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Disabled, button.BackColor, button.ForeColor);
			else
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Normal, button.BackColor, button.ForeColor);
		}

		public virtual void DrawPopupButtonFocus (Graphics g, Button button)
		{
			// No changes from Standard for image for this theme
			DrawButtonFocus (g, button);
		}

		public virtual void DrawPopupButtonImage (Graphics g, Button button, Rectangle imageBounds)
		{
			// No changes from Standard for image for this theme
			DrawButtonImage (g, button, imageBounds);
		}

		public virtual void DrawPopupButtonText (Graphics g, Button button, Rectangle textBounds)
		{
			// No changes from Standard for image for this theme
			DrawButtonText (g, button, textBounds);
		}
		#endregion

		#region Button Layout Calculations

		public override Size CalculateButtonAutoSize (Button button)
		{
			using (var cell = GetButtonCell(button))
				return cell.CellSize.ToSDSize();
		}

		public override void CalculateButtonTextAndImageLayout (Graphics g, ButtonBase button, out Rectangle textRect, out Rectangle imageRect)
		{
			textRect = Rectangle.Empty;
			imageRect = Rectangle.Empty;
		}

		private void LayoutTextBeforeOrAfterImage (Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, ContentAlignment textAlign, ContentAlignment imageAlign, out Rectangle textRect, out Rectangle imageRect)
		{
			int element_spacing = 0;    // Spacing between the Text and the Image
			int total_width = textSize.Width + element_spacing + imageSize.Width;

			if (!textFirst)
				element_spacing += 2;

			// If the text is too big, chop it down to the size we have available to it
			if (total_width > totalArea.Width)
			{
				textSize.Width = totalArea.Width - element_spacing - imageSize.Width;
				total_width = totalArea.Width;
			}

			int excess_width = totalArea.Width - total_width;
			int offset = 0;

			Rectangle final_text_rect;
			Rectangle final_image_rect;

			HorizontalAlignment h_text = GetHorizontalAlignment(textAlign);
			HorizontalAlignment h_image = GetHorizontalAlignment(imageAlign);

			if (h_image == HorizontalAlignment.Left)
				offset = 0;
			else if (h_image == HorizontalAlignment.Right && h_text == HorizontalAlignment.Right)
				offset = excess_width;
			else if (h_image == HorizontalAlignment.Center && (h_text == HorizontalAlignment.Left || h_text == HorizontalAlignment.Center))
				offset += (int)(excess_width / 3);
			else
				offset += (int)(2 * (excess_width / 3));

			if (textFirst)
			{
				final_text_rect = new Rectangle(totalArea.Left + offset, AlignInRectangle(totalArea, textSize, textAlign).Top, textSize.Width, textSize.Height);
				final_image_rect = new Rectangle(final_text_rect.Right + element_spacing, AlignInRectangle(totalArea, imageSize, imageAlign).Top, imageSize.Width, imageSize.Height);
			}
			else
			{
				final_image_rect = new Rectangle(totalArea.Left + offset, AlignInRectangle(totalArea, imageSize, imageAlign).Top, imageSize.Width, imageSize.Height);
				final_text_rect = new Rectangle(final_image_rect.Right + element_spacing, AlignInRectangle(totalArea, textSize, textAlign).Top, textSize.Width, textSize.Height);
			}

			textRect = final_text_rect;
			imageRect = final_image_rect;
		}

		private void LayoutTextAboveOrBelowImage (Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, ContentAlignment textAlign, ContentAlignment imageAlign, bool displayEllipsis, out Rectangle textRect, out Rectangle imageRect)
		{
			int element_spacing = 0;    // Spacing between the Text and the Image
			int total_height = textSize.Height + element_spacing + imageSize.Height;

			if (textFirst)
				element_spacing += 2;

			if (textSize.Width > totalArea.Width)
				textSize.Width = totalArea.Width;

			// If the there isn't enough room and we're text first, cut out the image
			if (total_height > totalArea.Height && textFirst)
			{
				imageSize = Size.Empty;
				total_height = totalArea.Height;
			}

			int excess_height = totalArea.Height - total_height;
			int offset = 0;

			Rectangle final_text_rect;
			Rectangle final_image_rect;

			VerticalAlignment v_text = GetVerticalAlignment(textAlign);
			VerticalAlignment v_image = GetVerticalAlignment(imageAlign);

			if (v_image == VerticalAlignment.Top)
				offset = 0;
			else if (v_image == VerticalAlignment.Bottom && v_text == VerticalAlignment.Bottom)
				offset = excess_height;
			else if (v_image == VerticalAlignment.Center && (v_text == VerticalAlignment.Top || v_text == VerticalAlignment.Center))
				offset += (int)(excess_height / 3);
			else
				offset += (int)(2 * (excess_height / 3));

			if (textFirst)
			{
				var textHeight = excess_height >= 0 ? totalArea.Height - imageSize.Height - element_spacing : textSize.Height;
				final_text_rect = new Rectangle(AlignInRectangle(totalArea, textSize, textAlign).Left, totalArea.Top + offset, textSize.Width, textHeight);
				final_image_rect = new Rectangle(AlignInRectangle(totalArea, imageSize, imageAlign).Left, final_text_rect.Bottom + element_spacing, imageSize.Width, imageSize.Height);
			}
			else
			{
				final_image_rect = new Rectangle(AlignInRectangle(totalArea, imageSize, imageAlign).Left, totalArea.Top + offset, imageSize.Width, imageSize.Height);
				var textHeight = excess_height >= 0 ? totalArea.Height - final_image_rect.Height : textSize.Height;
				final_text_rect = new Rectangle(AlignInRectangle(totalArea, textSize, textAlign).Left, final_image_rect.Bottom + element_spacing, textSize.Width, textHeight);

				if (final_text_rect.Bottom > totalArea.Bottom)
				{
					final_text_rect.Y -= (final_text_rect.Bottom - totalArea.Bottom);
					if (final_text_rect.Y < totalArea.Top)
						final_text_rect.Y = totalArea.Top;
				}
			}

			if (displayEllipsis)
			{
				// Don't use more space than is available otherwise ellipsis won't show
				if (final_text_rect.Height > totalArea.Bottom)
					final_text_rect.Height = totalArea.Bottom - final_text_rect.Top;
			}

			textRect = final_text_rect;
			imageRect = final_image_rect;
		}
		
		private HorizontalAlignment GetHorizontalAlignment (ContentAlignment align)
		{
			switch (align) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					return HorizontalAlignment.Left;
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					return HorizontalAlignment.Center;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					return HorizontalAlignment.Right;
			}

			return HorizontalAlignment.Left;
		}

		private enum VerticalAlignment
		{
			Top = 0,
			Center = 1,
			Bottom = 2
		}
		
		private VerticalAlignment GetVerticalAlignment (ContentAlignment align)
		{
			switch (align) {
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopRight:
					return VerticalAlignment.Top;
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight:
					return VerticalAlignment.Center;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					return VerticalAlignment.Bottom;
			}

			return VerticalAlignment.Top;
		}

		internal Rectangle AlignInRectangle (Rectangle outer, Size inner, ContentAlignment align)
		{
			int x = 0;
			int y = 0;

			if (align == ContentAlignment.BottomLeft || align == ContentAlignment.MiddleLeft || align == ContentAlignment.TopLeft)
				x = outer.X;
			else if (align == ContentAlignment.BottomCenter || align == ContentAlignment.MiddleCenter || align == ContentAlignment.TopCenter)
				x = Math.Max (outer.X + ((outer.Width - inner.Width) / 2), outer.Left);
			else if (align == ContentAlignment.BottomRight || align == ContentAlignment.MiddleRight || align == ContentAlignment.TopRight)
				x = outer.Right - inner.Width;
			if (align == ContentAlignment.TopCenter || align == ContentAlignment.TopLeft || align == ContentAlignment.TopRight)
				y = outer.Y;
			else if (align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleLeft || align == ContentAlignment.MiddleRight)
				y = outer.Y + (outer.Height - inner.Height) / 2;
			else if (align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomRight || align == ContentAlignment.BottomLeft)
				y = outer.Bottom - inner.Height;

			return new Rectangle (x, y, Math.Min (inner.Width, outer.Width), Math.Min (inner.Height, outer.Height));
		}
		#endregion
		#endregion

		#region ButtonBase
		public override void DrawButtonBase(Graphics g, Rectangle clipRect, ButtonBase b)
		{
			DrawButtonBackground(g, b, clipRect);

			using (var cell = GetButtonCell(b))
			{
				var view = NSView.FocusView();
				var frame = b.ClientRectangle.ToCGRect();
				cell.DrawWithFrame(frame, view);
			}
		}

		protected static bool ShouldPaintFocusRectagle (ButtonBase button)
		{
			return (button.Focused || button.paint_as_acceptbutton) && button.Enabled && button.ShowFocusCues;
		}

		public override Size ButtonBaseDefaultSize {
			get {
				return new Size (75, 23);
			}
		}
		#endregion // ButtonBase

		#region CheckBox

		NSButtonCell sharedCheckBoxCell;
		NSButtonCell SharedCheckBoxCell(CheckBox cb = null)
		{
			if (sharedCheckBoxCell == null)
			{
				sharedCheckBoxCell = new NSButtonCell();
				sharedCheckBoxCell.SetButtonType(NSButtonType.Switch);
				sharedCheckBoxCell.Title = String.Empty;
				sharedCheckBoxCell.Bezeled = false;
				sharedCheckBoxCell.Bordered = false;
			}

			if (cb != null)
			{
				sharedCheckBoxCell.Highlighted = cb.Pressed;
				sharedCheckBoxCell.Bezeled = false;
				sharedCheckBoxCell.Bordered = false;
				sharedCheckBoxCell.State = (cb.CheckState == CheckState.Checked ? NSCellStateValue.On : NSCellStateValue.Off);
				sharedCheckBoxCell.Enabled = cb.Enabled;
			}

			return sharedCheckBoxCell;
		}

		public override void DrawCheckBox (Graphics g, CheckBox cb, Rectangle glyphArea, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			if (cb.FlatStyle == FlatStyle.Flat || cb.FlatStyle == FlatStyle.Popup)
			{
				glyphArea.Height -= 2;
				glyphArea.Width -= 2;
			}

			DrawCheckBoxGlyph(g, cb, glyphArea);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawCheckBoxImage(g, cb, imageBounds);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawCheckBoxText(g, cb, textBounds);
		}

		public virtual void DrawCheckBoxGlyph (Graphics g, CheckBox cb, Rectangle glyphArea)
		{
			DrawCheckBoxGlyph (g, glyphArea, cb.Checked, cb.Focused, cb.Enabled, cb.ShowFocusCues);
		}

		public virtual void DrawCheckBoxGlyph (Graphics g, Rectangle glyphArea, bool check, bool focused, bool enabled, bool showFocusCues)
		{
			var cell = SharedCheckBoxCell();
			cell.State = check ? NSCellStateValue.On : NSCellStateValue.Off;
			cell.Enabled = enabled;

			var frame = glyphArea.ToCGRect();
			var view = NSView.FocusView();

			if (focused && enabled && showFocusCues)
				cell.DrawFocusRing(frame, view);

			cell.DrawWithFrame(frame, view);
		}

		public virtual void DrawCheckBoxFocus (Graphics g, CheckBox cb, Rectangle focusArea)
		{
			ControlPaint.DrawFocusRectangle (g, focusArea);
		}

		public virtual void DrawCheckBoxImage (Graphics g, CheckBox cb, Rectangle imageBounds)
		{
			if (cb.Enabled)
				g.DrawImage (cb.Image, imageBounds);
			else
				CPDrawImageDisabled (g, cb.Image, imageBounds.Left, imageBounds.Top, ColorControl);
		}

		public virtual void DrawCheckBoxText (Graphics g, CheckBox cb, Rectangle textBounds)
		{
			if (cb.Enabled)
				TextRenderer.DrawTextInternal(g, cb.Text, cb.Font, textBounds, cb.ForeColor, cb.TextFormatFlags, cb.UseCompatibleTextRendering);
			else
				DrawStringDisabled20(g, cb.Text, cb.Font, textBounds, cb.BackColor, cb.TextFormatFlags, cb.UseCompatibleTextRendering);
		}

		internal int CheckSize
		{
			get { return SharedCheckBoxCell().CellSize.ToSDSize().Height; }
		}

		internal int RBSize
		{
			get { return GetSharedRadioButtonCell().CellSize.ToSDSize().Height; }
		}

		public override void CalculateCheckBoxTextAndImageLayout (ButtonBase button, Point p, out Rectangle glyphArea, out Rectangle textRectangle, out Rectangle imageRectangle)
		{
			int check_size = button is RadioButton ? RBSize : CheckSize;

			if (button is CheckBox)
				check_size = (button as CheckBox).Appearance == Appearance.Normal ? check_size : 0;

			glyphArea = new Rectangle(button.Padding.Left, button.Padding.Top, check_size, check_size);

			Rectangle content_rect = button.PaddingClientRectangle;
			ContentAlignment align = ContentAlignment.TopLeft;

			if (button is CheckBox)
				align = (button as CheckBox).CheckAlign;
			else if (button is RadioButton)
				align = (button as RadioButton).CheckAlign;

			switch (align)
			{
				case ContentAlignment.BottomCenter:
					glyphArea.Y += content_rect.Height - check_size - 2;
					glyphArea.X += (content_rect.Width - check_size) / 2;
					break;
				case ContentAlignment.BottomLeft:
					glyphArea.Y += content_rect.Height - check_size - 2;
					content_rect.Width -= check_size;
					content_rect.Offset(check_size, 0);
					break;
				case ContentAlignment.BottomRight:
					glyphArea.Y += content_rect.Height - check_size - 2;
					glyphArea.X += content_rect.Width - check_size;
					content_rect.Width -= check_size;
					break;
				case ContentAlignment.MiddleCenter:
					glyphArea.Y += (content_rect.Height - check_size) / 2;
					glyphArea.X += (content_rect.Width - check_size) / 2;
					break;
				case ContentAlignment.MiddleLeft:
					glyphArea.Y += (content_rect.Height - check_size) / 2;
					content_rect.Width -= check_size;
					content_rect.Offset(check_size, 0);
					break;
				case ContentAlignment.MiddleRight:
					glyphArea.Y += (content_rect.Height - check_size) / 2;
					glyphArea.X += content_rect.Width - check_size;
					content_rect.Width -= check_size;
					break;
				case ContentAlignment.TopCenter:
					glyphArea.X += (content_rect.Width - check_size) / 2;
					break;
				case ContentAlignment.TopLeft:
					content_rect.Width -= check_size;
					content_rect.Offset(check_size, 0);
					break;
				case ContentAlignment.TopRight:
					glyphArea.X += content_rect.Width - check_size;
					content_rect.Width -= check_size;
					break;
			}

			Image image = button.Image;
			string text = button.Text;

			Size proposed = Size.Empty;

			// Force wrapping if we aren't AutoSize and our text is too long
			//if (!button.AutoSize) // JV:Condition disabled to fix drawing checkboxes and radiobuttons that need wrapping
			proposed.Width = button.PaddingClientRectangle.Width - glyphArea.Width - 2;

			Size text_size = TextRenderer.MeasureTextInternal(text, button.Font, proposed, button.TextFormatFlags, button.UseCompatibleTextRendering);

			// Text can't be bigger than the content rectangle
			text_size.Height = Math.Min(text_size.Height, content_rect.Height);
			text_size.Width = Math.Min(text_size.Width, content_rect.Width);

			Size image_size = image == null ? Size.Empty : image.Size;

			textRectangle = Rectangle.Empty;
			imageRectangle = Rectangle.Empty;

			switch (button.TextImageRelation)
			{
				case TextImageRelation.Overlay:
					// Text is centered vertically, and 2 pixels to the right
					textRectangle.X = content_rect.Left + 2;
					textRectangle.Y = button.PaddingClientRectangle.Top + ((content_rect.Height - text_size.Height) / 2) - 1;
					textRectangle.Size = text_size;

					glyphArea = AdjustGlyphArea(button, align, glyphArea, content_rect, textRectangle);

					// Image is dependent on ImageAlign
					if (image == null)
						return;

					int image_x = button.PaddingClientRectangle.Left;
					int image_y = button.PaddingClientRectangle.Top;
					int image_height = image.Height;
					int image_width = image.Width;

					switch (button.ImageAlign)
					{
						case System.Drawing.ContentAlignment.TopLeft:
							image_x += 5;
							image_y += 5;
							break;
						case System.Drawing.ContentAlignment.TopCenter:
							image_x += (content_rect.Width - image_width) / 2;
							image_y += 5;
							break;
						case System.Drawing.ContentAlignment.TopRight:
							image_x += content_rect.Width - image_width - 5;
							image_y += 5;
							break;
						case System.Drawing.ContentAlignment.MiddleLeft:
							image_x += 5;
							image_y += (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.MiddleCenter:
							image_x += (content_rect.Width - image_width) / 2;
							image_y += (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.MiddleRight:
							image_x += content_rect.Width - image_width - 4;
							image_y += (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.BottomLeft:
							image_x += 5;
							image_y += content_rect.Height - image_height - 4;
							break;
						case System.Drawing.ContentAlignment.BottomCenter:
							image_x += (content_rect.Width - image_width) / 2;
							image_y += content_rect.Height - image_height - 4;
							break;
						case System.Drawing.ContentAlignment.BottomRight:
							image_x += content_rect.Width - image_width - 4;
							image_y += content_rect.Height - image_height - 4;
							break;
						default:
							image_x += 5;
							image_y += 5;
							break;
					}

					imageRectangle = new Rectangle(image_x + check_size, image_y, image_width, image_height);
					break;
				case TextImageRelation.ImageAboveText:
					content_rect.Inflate(-4, -4);
					LayoutTextAboveOrBelowImage(content_rect, false, text_size, image_size, button.TextAlign, button.ImageAlign, false, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.TextAboveImage:
					content_rect.Inflate(-4, -4);
					LayoutTextAboveOrBelowImage(content_rect, true, text_size, image_size, button.TextAlign, button.ImageAlign, false, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.ImageBeforeText:
					content_rect.Inflate(-4, -4);
					LayoutTextBeforeOrAfterImage(content_rect, false, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.TextBeforeImage:
					content_rect.Inflate(-4, -4);
					LayoutTextBeforeOrAfterImage(content_rect, true, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
			}
		}

		// Adjust vertical glyph position - center it to an appropriate text line
		internal Rectangle AdjustGlyphArea(ButtonBase button, ContentAlignment align, Rectangle glyphArea, Rectangle contentRect, Rectangle textRect)
		{
			var lines = Math.Max(1, textRect.Height / button.Font.GetHeight());
			var lineHeight = (int)button.Font.GetHeight();
			switch (align)
			{
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopRight:
					glyphArea.Y = textRect.Y + (lineHeight - glyphArea.Height) / 2;
					break;
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomRight:
					glyphArea.Y = textRect.Bottom - lineHeight + (lineHeight - glyphArea.Height) / 2;
					break;
			}

			return glyphArea;
		}

		public override Size CalculateCheckBoxAutoSize (CheckBox checkBox)
		{
			Size ret_size = Size.Empty;
			Size text_size = TextRenderer.MeasureTextInternal(checkBox.Text, checkBox.Font, checkBox.UseCompatibleTextRendering);
			Size image_size = checkBox.Image == null ? Size.Empty : checkBox.Image.Size;

			// Pad the text size
			if (checkBox.Text.Length != 0)
			{
				text_size.Height += 4;
				text_size.Width += 4;
			}

			int rbsize = SharedCheckBoxCell().CellSize.ToSDSize().Height;
			text_size.Height = Math.Max(text_size.Height, rbsize);

			switch (checkBox.TextImageRelation)
			{
				case TextImageRelation.Overlay:
					ret_size.Height = Math.Max(checkBox.Text.Length == 0 ? 0 : text_size.Height, image_size.Height);
					ret_size.Width = Math.Max(text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageAboveText:
				case TextImageRelation.TextAboveImage:
					ret_size.Height = text_size.Height + image_size.Height;
					ret_size.Width = Math.Max(text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageBeforeText:
				case TextImageRelation.TextBeforeImage:
					ret_size.Height = Math.Max(text_size.Height, image_size.Height);
					ret_size.Width = text_size.Width + image_size.Width;
					break;
			}

			// Pad the result
			ret_size.Height += (checkBox.Padding.Vertical);
			ret_size.Width += (checkBox.Padding.Horizontal) + CheckSize;

			// There seems to be a minimum height
			if (ret_size.Height == checkBox.Padding.Vertical)
				ret_size.Height += CheckSize;

			return ret_size;
		}

		public override void DrawCheckBox(Graphics dc, Rectangle clip_area, CheckBox checkbox)
		{
			var cell = SharedCheckBoxCell(checkbox);
			if (checkbox.Focused && checkbox.Enabled && checkbox.appearance != Appearance.Button && checkbox.Text != String.Empty && checkbox.ShowFocusCues)
				cell.FocusRingType = NSFocusRingType.Default;
			cell.DrawWithFrame(checkbox.ClientRectangle.ToCGRect(), NSView.FocusView());
		}

		#endregion	// CheckBox
		
		#region CheckedListBox
		
		public override void DrawCheckedListBoxItem (CheckedListBox ctrl, DrawItemEventArgs e)
		{			
			Color back_color, fore_color;
			Rectangle item_rect = e.Bounds;
			ButtonState state;

			// Draw checkbox

			if ((e.State & DrawItemState.Checked) == DrawItemState.Checked) {
				state = ButtonState.Checked;
				if ((e.State & DrawItemState.Inactive) == DrawItemState.Inactive)
					state |= ButtonState.Inactive;
			} else
				state = ButtonState.Normal;

			if (ctrl.ThreeDCheckBoxes == false)
				state |= ButtonState.Flat;

			Rectangle checkbox_rect = new Rectangle (2, (item_rect.Height - CheckSize) / 2, CheckSize, CheckSize);
			Rectangle glyph_rect = new Rectangle(item_rect.X + checkbox_rect.X, item_rect.Y + checkbox_rect.Y, checkbox_rect.Width, checkbox_rect.Height);
			DrawCheckBoxGlyph(e.Graphics, glyph_rect, state.HasFlag(ButtonState.Checked), e.State.HasFlag(DrawItemState.Focus), !state.HasFlag(ButtonState.Inactive), false);

			item_rect.X += checkbox_rect.Right;
			item_rect.Width -= checkbox_rect.Right;
			
			/* Draw text*/
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}
			
			e.Graphics.FillRectangle (ResPool.GetSolidBrush
				(back_color), item_rect);

			e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
				ResPool.GetSolidBrush (fore_color),
				item_rect, ctrl.StringFormat);
					
		}
		
		public override int CheckBoxSize() {
			return CheckSize;
		}
		
		#endregion // CheckedListBox
		
		#region ComboBox		
		public override void DrawComboBoxItem (ComboBox ctrl, DrawItemEventArgs e)
		{
			Color back_color, fore_color;
			Rectangle text_draw = e.Bounds;
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.LineLimit;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}
			
			if (!ctrl.Enabled)
				fore_color = ColorInactiveCaptionText;
							
			e.Graphics.FillRectangle (ResPool.GetSolidBrush (back_color), e.Bounds);

			if (e.Index != -1) {
				e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
					ResPool.GetSolidBrush (fore_color),
					text_draw, string_format);
			}
			
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				CPDrawFocusRectangle (e.Graphics, e.Bounds, fore_color, back_color);
			}

			string_format.Dispose ();
		}
		
		public override void DrawFlatStyleComboButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left + 1, centerY);
			P2=new Point(rect.Right - 1, centerY);
			P3=new Point(centerX, rect.Bottom - 1);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;
			
			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				/* Move away from the shadow */
				arrow[0].X += 1;	arrow[0].Y += 1;
				arrow[1].X += 1;	arrow[1].Y += 1;
				arrow[2].X += 1;	arrow[2].Y += 1;
				
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;

				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}		
		}
		public override void ComboBoxDrawNormalDropDownButton (ComboBox cb, Graphics g, Rectangle clippingArea, Rectangle area, ButtonState state)
		{
			using (var cell = GetPopupButtonCell(cb))
			{
				var view = NSView.FocusView();
				var frame = cb.ClientRectangle.ToCGRect();
				cell.DrawWithFrame(frame, view);
			}

			//CPDrawComboButton (g, area, state);
		}
		public override bool ComboBoxNormalDropDownButtonHasTransparentBackground (ComboBox comboBox, ButtonState state)
		{
			return false; // Prevents ComboBox from drawing the background itself.
		}
		public override bool ComboBoxDropDownButtonHasHotElementStyle (ComboBox comboBox)
		{
			return false;
		}

		public override void ComboBoxDrawBackground (ComboBox comboBox, Graphics g, Rectangle clippingArea, FlatStyle style)
		{
			var parent = comboBox.Parent;
			if (parent != null)
			{
				var e = new PaintEventArgs(g, new Rectangle(clippingArea.X + comboBox.Left, clippingArea.Y + comboBox.Top, clippingArea.Width, clippingArea.Height));
				var state = e.Graphics.Save();
				e.Graphics.TranslateTransform(-comboBox.Left, -comboBox.Top);
				parent.PaintControlBackground(e);
				e.Graphics.Restore(state);
				e.SetGraphics(null);
				return;
			}

			if (!comboBox.Enabled)
				g.FillRectangle (SystemBrushes.Control, comboBox.ClientRectangle);

			if (comboBox.DropDownStyle == ComboBoxStyle.Simple)
				g.FillRectangle (ResPool.GetSolidBrush (comboBox.Parent.BackColor), comboBox.ClientRectangle);

			if (style == FlatStyle.Popup && (comboBox.Entered || comboBox.Focused)) {
				Rectangle area = comboBox.TextArea;
				area.Height -= 1;
				area.Width -= 1;
				g.DrawRectangle (ResPool.GetPen (SystemColors.ControlDark), area);
				g.DrawLine (ResPool.GetPen (SystemColors.ControlDark), comboBox.ButtonArea.X - 1, comboBox.ButtonArea.Top, comboBox.ButtonArea.X - 1, comboBox.ButtonArea.Bottom);
			}
			bool is_flat = style == FlatStyle.Flat || style == FlatStyle.Popup;
			if (!is_flat && clippingArea.IntersectsWith (comboBox.TextArea))
				ControlPaint.DrawBorder3D (g, comboBox.TextArea, Border3DStyle.Sunken);
		}
		public override bool CombBoxBackgroundHasHotElementStyle (ComboBox comboBox)
		{
			return false;
		}
		#endregion ComboBox

		#region DataGridView
		#region DataGridViewHeaderCell
		#region DataGridViewRowHeaderCell
		public override bool DataGridViewRowHeaderCellDrawBackground (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}

		public override bool DataGridViewRowHeaderCellDrawSelectionBackground (DataGridViewRowHeaderCell cell)
		{
			return false;
		}

		public override bool DataGridViewRowHeaderCellDrawBorder (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}
		#endregion

		#region DataGridViewColumnHeaderCell
		public override bool DataGridViewColumnHeaderCellDrawBackground (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}

		public override bool DataGridViewColumnHeaderCellDrawBorder (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}
		#endregion

		public override bool DataGridViewHeaderCellHasPressedStyle  (DataGridView dataGridView)
		{
			return false;
		}

		public override bool DataGridViewHeaderCellHasHotStyle (DataGridView dataGridView)
		{
			return false;
		}
		#endregion
		#endregion

		#region DateTimePicker

		internal Rectangle DateTimePickerClientWithoutStepper(DateTimePicker picker)
		{
			var r = picker.ClientRectangle;
			if (picker.ShowUpDown)
				r.Size = new Size(r.Width - DateTimePicker.up_down_width - 4, r.Height);
			return r;
		}

		protected virtual void DateTimePickerDrawBorder (DateTimePicker picker, Graphics g, Rectangle clippingArea)
		{
			this.CPDrawBorder3D (g, DateTimePickerClientWithoutStepper(picker), Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, SystemColors.Control);
		}

		protected virtual void DateTimePickerDrawDropDownButton (DateTimePicker dateTimePicker, Graphics g, Rectangle clippingArea)
		{
			ButtonState state = dateTimePicker.is_drop_down_visible ? ButtonState.Pushed : ButtonState.Normal;
			g.FillRectangle (ResPool.GetSolidBrush (ColorControl), dateTimePicker.drop_down_arrow_rect);
			this.CPDrawComboButton ( 
			  g, 
			  dateTimePicker.drop_down_arrow_rect, 
			  state);
		}

		public override void DrawDateTimePicker(Graphics dc, Rectangle clip_rectangle, DateTimePicker dtp)
		{
			if (!clip_rectangle.IntersectsWith (dtp.ClientRectangle))
				return;

			// draw the outer border
			DateTimePickerDrawBorder (dtp, dc, clip_rectangle);

			Rectangle button_bounds = DateTimePickerClientWithoutStepper(dtp);
			// deflate by the border width
			if (clip_rectangle.IntersectsWith (dtp.drop_down_arrow_rect)) {
				button_bounds.Inflate (-2,-2);
				if (!dtp.ShowUpDown) {
					DateTimePickerDrawDropDownButton (dtp, dc, clip_rectangle);
				}
			}

			// render the date part
			if (!clip_rectangle.IntersectsWith (dtp.date_area_rect))
				return;

			// fill the background
			dc.FillRectangle (SystemBrushes.Window, dtp.date_area_rect);

			// Update date_area_rect if we are drawing the checkbox
			Rectangle date_area_rect = dtp.date_area_rect;
			if (dtp.ShowCheckBox) {
				Rectangle check_box_rect = dtp.CheckBoxRect;
				date_area_rect.X = date_area_rect.X + check_box_rect.Width + DateTimePicker.check_box_space * 2;
				date_area_rect.Width = date_area_rect.Width - check_box_rect.Width - DateTimePicker.check_box_space * 2;

				ButtonState bs = dtp.Checked ? ButtonState.Checked : ButtonState.Normal;
				CPDrawCheckBox(dc, check_box_rect, bs);

				if (dtp.is_checkbox_selected)
					CPDrawFocusRectangle (dc, check_box_rect, dtp.foreground_color, dtp.background_color);
			}

			// render each text part
			using (StringFormat text_format = StringFormat.GenericTypographic)
			{
				text_format.LineAlignment = StringAlignment.Near;
				text_format.Alignment = StringAlignment.Near;
				text_format.FormatFlags = text_format.FormatFlags | StringFormatFlags.NoWrap;
				text_format.FormatFlags &= ~(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
				text_format.Trimming = StringTrimming.Character;

				var fontHeight = dtp.Font.GetHeight(dc);
				// Calculate the rectangles for each part 
				if (dtp.part_data.Length > 0 && dtp.part_data[0].drawing_rectangle.IsEmpty)
				{
					Graphics gr = dc;
					for (int i = 0; i < dtp.part_data.Length; i++)
					{
						DateTimePicker.PartData fd = dtp.part_data[i];
						RectangleF text_rect = new RectangleF();
						string text = fd.GetText(dtp.Value);
						text_rect.Size = new SizeF(gr.MeasureString(text, dtp.Font, 250, text_format).Width, fontHeight);
						if (!fd.is_literal)
							text_rect.Width = Math.Max (dtp.CalculateMaxWidth(fd.value, gr, text_format), text_rect.Width);

						if (i > 0) {
							text_rect.X = dtp.part_data[i - 1].drawing_rectangle.Right;
						} else {
							text_rect.X = date_area_rect.X;
						}
						text_rect.Inflate (1, 0);
						fd.drawing_rectangle = text_rect;
						fd.drawing_rectangle.Y = dtp.ClientRectangle.Y;
						fd.drawing_rectangle.Height = dtp.ClientRectangle.Height;
					}
				}
				
				// draw the text part
				Brush text_brush = ResPool.GetSolidBrush (dtp.ShowCheckBox && dtp.Checked == false ?
						SystemColors.GrayText : dtp.ForeColor); // Use GrayText if Checked is false
				RectangleF clip_rectangleF = clip_rectangle;

				for (int i = 0; i < dtp.part_data.Length; i++)
				{
					DateTimePicker.PartData fd = dtp.part_data [i];
					string text;

					if (!clip_rectangleF.IntersectsWith (fd.drawing_rectangle))
						continue;

					text = dtp.editing_part_index == i ? dtp.editing_text : fd.GetText (dtp.Value);

					var text_size = new SizeF(dc.MeasureString (text, dtp.Font, 250, text_format).Width, fontHeight);
					var text_position = new PointF(
						(fd.drawing_rectangle.Left + fd.drawing_rectangle.Width / 2) - text_size.Width / 2,
						(fd.drawing_rectangle.Top + fd.drawing_rectangle.Height / 2) - text_size.Height / 2);
					var text_rect = RectangleF.Intersect(new RectangleF(text_position, text_size), date_area_rect);
					
					if (text_rect.IsEmpty)
						break;

					if (text_rect.Right >= date_area_rect.Right)
						text_format.FormatFlags &= ~StringFormatFlags.NoClip;
					else
						text_format.FormatFlags |= StringFormatFlags.NoClip;

					if (fd.Selected && dtp.Focused) {
						dc.FillRectangle (SystemBrushes.Highlight, text_rect);
						dc.DrawString (text, dtp.Font, SystemBrushes.HighlightText, text_rect, text_format);
					
					} else {
						dc.DrawString (text, dtp.Font, text_brush, text_rect, text_format);
					}

					if (fd.drawing_rectangle.Right > date_area_rect.Right)
						break; // the next part would be not be visible, so don't draw anything more.
				}
			}
		}

		public override bool DateTimePickerBorderHasHotElementStyle {
			get {
				return false;
			}
		}

		public override Rectangle DateTimePickerGetDropDownButtonArea (DateTimePicker dateTimePicker)
		{
			Rectangle rect = dateTimePicker.ClientRectangle;
			rect.X = rect.Right - SystemInformation.VerticalScrollBarWidth - 2;
			if (rect.Width > (SystemInformation.VerticalScrollBarWidth + 2)) {
				rect.Width = SystemInformation.VerticalScrollBarWidth;
			} else {
				rect.Width = Math.Max (rect.Width - 2, 0);
			}
			
			rect.Inflate (0, -2);
			return rect;
		}

		public override Rectangle DateTimePickerGetDateArea (DateTimePicker dateTimePicker)
		{
			Rectangle rect = dateTimePicker.ClientRectangle;
			if (dateTimePicker.ShowUpDown) {
				// set the space to the left of the up/down button
				if (rect.Width > (DateTimePicker.up_down_width + 4)) {
					rect.Width -= (DateTimePicker.up_down_width + 4);
				} else {
					rect.Width = 0;
				}
			} else {
				// set the space to the left of the up/down button
				// TODO make this use up down button
				if (rect.Width > (SystemInformation.VerticalScrollBarWidth + 4)) {
					rect.Width -= SystemInformation.VerticalScrollBarWidth;
				} else {
					rect.Width = 0;
				}
			}
			
			rect.Inflate (-2, -2);
			return rect;
		}
		public override bool DateTimePickerDropDownButtonHasHotElementStyle {
			get {
				return false;
			}
		}
		#endregion // DateTimePicker

		#region GroupBox
		public override void DrawGroupBox (Graphics dc,  Rectangle area, GroupBox box) {
			StringFormat	text_format;
			SizeF		size;
			int		width;
			int		y;

			dc.FillRectangle (GetControlBackBrush (box.BackColor), box.ClientRectangle);
			
			text_format = new StringFormat();
			text_format.HotkeyPrefix = HotkeyPrefix.Show;
			text_format.Alignment = StringAlignment.Center;

			size = dc.MeasureString (box.Text, box.Font);
			width = 0;

			if (size.Width > 0) {
				width = ((int) size.Width) + 7;
			
				if (width > box.Width - 16)
					width = box.Width - 16;
			}
			
			y = box.Font.Height / 2 + 1;

			Pen pen = ResPool.GetPen(NSColor.Grid.ToSDColor());

			// Draw group box
			Rectangle rText = new Rectangle(10, 0, width, box.Font.Height);
			Rectangle r = new Rectangle(0, y, box.Width, box.Height - y);
			dc.DrawLine(pen, r.Left, r.Top, 0, r.Bottom - 1); // left
			dc.DrawLine(pen, r.Left, r.Bottom - 1, r.Right-1, r.Bottom - 1); // bottom
			dc.DrawLine(pen, r.Left, r.Top, rText.Left, r.Top); // top-left
			dc.DrawLine(pen, rText.Right, r.Top, r.Right - 1, r.Top); // top-right
			dc.DrawLine(pen, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1); // right

			// Text
			if (box.Text.Length != 0) {
				if (box.Enabled) {
					dc.DrawString(box.Text, box.Font, ResPool.GetSolidBrush(box.ForeColor), rText, text_format);
				} else {
					CPDrawStringDisabled(dc, box.Text, box.Font, box.BackColor, rText, text_format);
				}
			}
			
			text_format.Dispose ();	
		}

		public override Size GroupBoxDefaultSize {
			get {
				return new Size (200,100);
			}
		}
		#endregion

		#region HScrollBar
		public override Size HScrollBarDefaultSize {
			get {
				return new Size (80, this.ScrollBarButtonSize);
			}
		}

		#endregion // HScrollBar

		#region ListBox

		// Background color of the active (focused) highlited item in lists (not text)
		Color? colorHighlightItem = typeof(NSColor).RespondsToSelector("selectedContentBackgroundColor") ? NSColor.SelectedContentBackground.ToSDColor() : (Color?)null;

		internal virtual Color ColorHighlightItem
		{
			get { return colorHighlightItem ?? ColorHighlight; }
			set { colorHighlightItem = value; }
		}

		// Background color of the inactive (not focused) highlited item in lists (not text)
		Color? colorInactiveHighlightItem = typeof(NSColor).RespondsToSelector("unemphasizedSelectedContentBackgroundColor") ? NSColor.UnemphasizedSelectedContentBackground.ToSDColor() : (Color?)null;

		internal virtual Color ColorInactiveHighlightItem
		{
			get { return colorInactiveHighlightItem ?? ColorHighlight; }
			set { colorInactiveHighlightItem = value; }
		}

		// Text color of the active highlited item text in lists (not text)
		Color? colorHighlightItemText = NSColor.SelectedMenuItemText.ToSDColor();

		public virtual Color ColorHighlightItemText
		{
			get { return colorHighlightItemText ?? ColorHighlightText; }
			set { colorHighlightItemText = value; }
		}

		public override void DrawListBoxItem (ListBox ctrl, DrawItemEventArgs e)
		{
			Color back_color, fore_color;

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
					fore_color = ColorHighlightItemText;
					back_color = ColorHighlightItem;
				} else { 
					fore_color = e.ForeColor;
					back_color = ColorInactiveHighlightItem;
				}
			} else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}

			e.Graphics.FillRectangle (ResPool.GetSolidBrush (back_color), e.Bounds);

			e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
					       ResPool.GetSolidBrush (fore_color),
					       e.Bounds, ctrl.StringFormat);
					
			//if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
				//CPDrawFocusRectangle (e.Graphics, e.Bounds, fore_color, back_color);
		}
		
		#endregion ListBox

		#region ListView
		// Drawing
		public override void DrawListViewItems (Graphics dc, Rectangle clip, ListView control)
		{
			bool details = control.View == View.Details;
			int first = control.FirstVisibleIndex;	
			int lastvisibleindex = control.LastVisibleIndex;

			if (control.VirtualMode)
				control.OnCacheVirtualItems (new CacheVirtualItemsEventArgs (first, lastvisibleindex));

			for (int i = first; i <= lastvisibleindex; i++) {					
				ListViewItem item = control.GetItemAtDisplayIndex (i);
				if (clip.IntersectsWith (item.Bounds)) {
					bool owner_draw = false;
					if (control.OwnerDraw)
						owner_draw = DrawListViewItemOwnerDraw (dc, item, i);
					if (!owner_draw)
					{
						DrawListViewItem (dc, control, item);
						if (control.View == View.Details)
							DrawListViewSubItems (dc, control, item);
					}
				}
			}	

			if (control.UsingGroups) {
				// Use InternalCount instead of Count to take into account Default Group as needed
				for (int i = 0; i < control.Groups.InternalCount; i++) {
					ListViewGroup group = control.Groups.GetInternalGroup (i);
					if (group.ItemCount > 0 && clip.IntersectsWith (group.HeaderBounds))
						DrawListViewGroupHeader (dc, control, group);
				}
			}

			ListViewInsertionMark insertion_mark = control.InsertionMark;
			int insertion_mark_index = insertion_mark.Index;
			if (Application.VisualStylesEnabled && insertion_mark.Bounds != Rectangle.Empty &&
					(control.View != View.Details && control.View != View.List) &&
					insertion_mark_index > -1 && insertion_mark_index < control.Items.Count) {

				Brush brush = ResPool.GetSolidBrush (insertion_mark.Color);
				dc.FillRectangle (brush, insertion_mark.Line);
				dc.FillPolygon (brush, insertion_mark.TopTriangle);
				dc.FillPolygon (brush, insertion_mark.BottomTriangle);
			}
			
			// draw the gridlines
			if (details && control.GridLines && !control.UsingGroups) {
				Size control_size = control.ClientSize;
				int top = (control.HeaderStyle == ColumnHeaderStyle.None) ?
					0 : control.header_control.Height;

				// draw vertical gridlines
				foreach (ColumnHeader col in control.Columns) {
					int column_right = col.Rect.Right - control.h_marker;
					dc.DrawLine (SystemPens.Control,
						     column_right, top,
						     column_right, control_size.Height);
				}

				// draw horizontal gridlines
				int item_height = control.ItemSize.Height;
				if (item_height == 0)
					item_height =  control.Font.Height + 2;

				int y = top + item_height - (control.v_marker % item_height); // scroll bar offset
				while (y < control_size.Height) {
					dc.DrawLine (SystemPens.Control, 0, y, control_size.Width, y);
					y += item_height;
				}
			}			
			
			// Draw corner between the two scrollbars
			if (control.h_scroll.Visible == true && control.v_scroll.Visible == true) {
				Rectangle rect = new Rectangle ();
				rect.X = control.h_scroll.Location.X + control.h_scroll.Width;
				rect.Width = control.v_scroll.Width;
				rect.Y = control.v_scroll.Location.Y + control.v_scroll.Height;
				rect.Height = control.h_scroll.Height;
				dc.FillRectangle (SystemBrushes.Control, rect);
			}

			Rectangle box_select_rect = control.item_control.BoxSelectRectangle;
			if (!box_select_rect.Size.IsEmpty)
				dc.DrawRectangle (ResPool.GetDashPen (ColorControlText, DashStyle.Dot), box_select_rect);

		}

		public override void DrawListViewHeader (Graphics dc, Rectangle clip, ListView control)
		{	
			bool details = (control.View == View.Details);
				
			// border is drawn directly in the Paint method
			if (details && control.HeaderStyle != ColumnHeaderStyle.None) {				
				dc.FillRectangle (SystemBrushes.Control,
						  0, 0, control.TotalWidth, control.Font.Height + 5);
				if (control.Columns.Count > 0) {
					foreach (ColumnHeader col in control.Columns) {
						Rectangle rect = col.Rect;
						rect.X -= control.h_marker;

						bool owner_draw = false;
						if (control.OwnerDraw)
							owner_draw = DrawListViewColumnHeaderOwnerDraw (dc, control, col, rect);
						if (owner_draw)
							continue;

						ListViewDrawColumnHeaderBackground (control, col, dc, rect, clip);
						rect.X += 5;
						rect.Width -= 10;
						if (rect.Width <= 0)
							continue;

						int image_index;
						if (control.SmallImageList == null)
							image_index = -1;
						else 
							image_index = col.ImageKey == String.Empty ? col.ImageIndex : control.SmallImageList.Images.IndexOfKey (col.ImageKey);

						if (image_index > -1 && image_index < control.SmallImageList.Images.Count) {
							int image_width = control.SmallImageList.ImageSize.Width + 5;
							int text_width = (int)dc.MeasureString (col.Text, control.Font).Width;
							int x_origin = rect.X;
							int y_origin = rect.Y + ((rect.Height - control.SmallImageList.ImageSize.Height) / 2);

							switch (col.TextAlign) {
								case HorizontalAlignment.Left:
									break;
								case HorizontalAlignment.Right:
									x_origin = rect.Right - (text_width + image_width);
									break;
								case HorizontalAlignment.Center:
									x_origin = (rect.Width - (text_width + image_width)) / 2 + rect.X;
									break;
							}

							if (x_origin < rect.X)
								x_origin = rect.X;

							control.SmallImageList.Draw (dc, new Point (x_origin, y_origin), image_index);
							rect.X += image_width;
							rect.Width -= image_width;
						}

						dc.DrawString (col.Text, control.Font, SystemBrushes.ControlText, rect, col.Format);
					}
					int right = control.GetReorderedColumn (control.Columns.Count - 1).Rect.Right - control.h_marker;
					if (right < control.Right) {
						Rectangle rect = control.Columns [0].Rect;
						rect.X = right;
						rect.Width = control.Right - right;
						ListViewDrawUnusedHeaderBackground (control, dc, rect, clip);
					}
				}
			}
		}

		protected virtual void ListViewDrawColumnHeaderBackground (ListView listView, ColumnHeader columnHeader, Graphics g, Rectangle area, Rectangle clippingArea)
		{
			ButtonState state;
			if (listView.HeaderStyle == ColumnHeaderStyle.Clickable)
				state = columnHeader.Pressed ? ButtonState.Pushed : ButtonState.Normal;
			else
				state = ButtonState.Flat;
			CPDrawButton (g, area, state);
		}
		
		protected virtual void ListViewDrawUnusedHeaderBackground (ListView listView, Graphics g, Rectangle area, Rectangle clippingArea)
		{
			ButtonState state;
			if (listView.HeaderStyle == ColumnHeaderStyle.Clickable)
				state = ButtonState.Normal;
			else
				state = ButtonState.Flat;
			CPDrawButton (g, area, state);
		}

		public override void DrawListViewHeaderDragDetails (Graphics dc, ListView view, ColumnHeader col, int target_x)
		{
			Rectangle rect = col.Rect;
			rect.X -= view.h_marker;
			Color color = Color.FromArgb (0x7f, ColorControlDark.R, ColorControlDark.G, ColorControlDark.B);
			dc.FillRectangle (ResPool.GetSolidBrush (color), rect);
			rect.X += 3;
			rect.Width -= 8;
			if (rect.Width <= 0)
				return;
			color = Color.FromArgb (0x7f, ColorControlText.R, ColorControlText.G, ColorControlText.B);
			dc.DrawString (col.Text, view.Font, ResPool.GetSolidBrush (color), rect, col.Format);
			dc.DrawLine (ResPool.GetSizedPen (ColorHighlight, 2), target_x, 0, target_x, col.Rect.Height);
		}

		protected virtual bool DrawListViewColumnHeaderOwnerDraw (Graphics dc, ListView control, ColumnHeader column, Rectangle bounds)
		{
			ListViewItemStates state = ListViewItemStates.ShowKeyboardCues;
			if (column.Pressed)
				state |= ListViewItemStates.Selected;

			DrawListViewColumnHeaderEventArgs args = new DrawListViewColumnHeaderEventArgs (dc,
					bounds, column.Index, column, state, SystemColors.ControlText, ThemeEngine.Current.ColorControl, DefaultFont);
			control.OnDrawColumnHeader (args);

			return !args.DrawDefault;
		}

		protected virtual bool DrawListViewItemOwnerDraw (Graphics dc, ListViewItem item, int index)
		{
			ListViewItemStates item_state = ListViewItemStates.ShowKeyboardCues;
			if (item.Selected)
				item_state |= ListViewItemStates.Selected;
			if (item.Focused)
				item_state |= ListViewItemStates.Focused;
						
			DrawListViewItemEventArgs args = new DrawListViewItemEventArgs (dc,
					item, item.Bounds, index, item_state);
			item.ListView.OnDrawItem (args);

			if (args.DrawDefault)
				return false;

			if (item.ListView.View == View.Details) {
				int count = Math.Min (item.ListView.Columns.Count, item.SubItems.Count);
				
				// Do system drawing for subitems if no owner draw is done
				for (int j = 0; j < count; j++) {
					if (!DrawListViewSubItemOwnerDraw (dc, item, item_state, j)) {
						if (j == 0) // The first sub item contains the main item semantics
							DrawListViewItem (dc, item.ListView, item);
						else
							DrawListViewSubItem (dc, item.ListView, item, j);
					}
				}
			}
			
			return true;
		}

		protected virtual void DrawListViewItem (Graphics dc, ListView control, ListViewItem item)
		{				
			Rectangle rect_checkrect = item.CheckRectReal;
			Rectangle icon_rect = item.GetBounds (ItemBoundsPortion.Icon);
			Rectangle full_rect = item.GetBounds (ItemBoundsPortion.Entire);
			Rectangle text_rect = item.GetBounds (ItemBoundsPortion.Label);			

			// Tile view doesn't support CheckBoxes
			if (control.CheckBoxes && control.View != View.Tile) {
				if (control.StateImageList == null) {
					// Make sure we've got at least a line width of 1
					int check_wd = Math.Max (3, rect_checkrect.Width / 6);
					int scale = Math.Max (1, rect_checkrect.Width / 12);

					// set the checkbox background
					dc.FillRectangle (SystemBrushes.Window,
							  rect_checkrect);
					// define a rectangle inside the border area
					Rectangle rect = new Rectangle (rect_checkrect.X + 2,
									rect_checkrect.Y + 2,
									rect_checkrect.Width - 4,
									rect_checkrect.Height - 4);
					Pen pen = ResPool.GetSizedPen (this.ColorWindowText, 2);
					dc.DrawRectangle (pen, rect);

					// Need to draw a check-mark
					if (item.Checked) {
						Pen check_pen = ResPool.GetSizedPen (this.ColorWindowText, 1);
						// adjustments to get the check-mark at the right place
						rect.X ++; rect.Y ++;
						// following logic is taken from DrawFrameControl method
						int x_offset = rect.Width / 5;
						int y_offset = rect.Height / 3;
						for (int i = 0; i < check_wd; i++) {
							dc.DrawLine (check_pen, rect.Left + x_offset,
								     rect.Top + y_offset + i,
								     rect.Left + x_offset + 2 * scale,
								     rect.Top + y_offset + 2 * scale + i);
							dc.DrawLine (check_pen,
								     rect.Left + x_offset + 2 * scale,
								     rect.Top + y_offset + 2 * scale + i,
								     rect.Left + x_offset + 6 * scale,
								     rect.Top + y_offset - 2 * scale + i);
						}
					}
				}
				else {
					int simage_idx;
					if (item.Checked)
						simage_idx = control.StateImageList.Images.Count > 1 ? 1 : -1;
					else
						simage_idx = control.StateImageList.Images.Count > 0 ? 0 : -1;

					if (simage_idx > -1)
						control.StateImageList.Draw (dc, rect_checkrect.Location, simage_idx);
				}
			}

			ImageList image_list = control.View == View.LargeIcon || control.View == View.Tile ? control.LargeImageList : control.SmallImageList;
			if (image_list != null) {
				int idx;

				if (item.ImageKey != String.Empty)
					idx = image_list.Images.IndexOfKey (item.ImageKey);
				else
					idx = item.ImageIndex;

				if (idx > -1 && idx < image_list.Images.Count) {
					// Draw a thumbnail image if it exists for a FileViewListViewItem, otherwise draw
					// the standard icon.  See https://bugzilla.xamarin.com/show_bug.cgi?id=28025.
					var fi = item as System.Windows.Forms.FileViewListViewItem;
					if (fi != null && fi.FSEntry != null && fi.FSEntry.Image != null)
						dc.DrawImage(fi.FSEntry.Image, icon_rect);
					else
						image_list.Draw(dc, icon_rect.Location, idx);
				}
			}

			// draw the item text			
			// format for the item text
			StringFormat format = new StringFormat ();
			if (control.View == View.SmallIcon || control.View == View.LargeIcon)
				format.LineAlignment = StringAlignment.Near;
			else
				format.LineAlignment = StringAlignment.Center;
			if (control.View == View.LargeIcon)
				format.Alignment = StringAlignment.Center;
			else
				format.Alignment = StringAlignment.Near;
			
			if (control.LabelWrap && control.View != View.Details && control.View != View.Tile)
				format.FormatFlags = StringFormatFlags.LineLimit;
			else
				format.FormatFlags = StringFormatFlags.NoWrap;

			if ((control.View == View.LargeIcon && !item.Focused) || control.View == View.Details || control.View == View.Tile)
				format.Trimming = StringTrimming.EllipsisCharacter;

			Rectangle highlight_rect = text_rect;
			if (control.View == View.Details) { // Adjustments for Details view
				Size text_size = Size.Ceiling (dc.MeasureString (item.Text, item.Font));

				if (!control.FullRowSelect) // Selection shouldn't be outside the item bounds
					highlight_rect.Width = Math.Min (text_size.Width + 4, text_rect.Width);
			}

			if (item.Selected && control.Focused)
				dc.FillRectangle (SystemBrushes.Highlight, highlight_rect);
			else if (item.Selected && !control.HideSelection)
				dc.FillRectangle (SystemBrushes.Control, highlight_rect);
			else
				dc.FillRectangle (ResPool.GetSolidBrush (item.BackColor), text_rect);
			
			Brush textBrush =
				!control.Enabled ? SystemBrushes.ControlLight :
				(item.Selected && control.Focused) ? SystemBrushes.HighlightText :
				this.ResPool.GetSolidBrush (item.ForeColor);

			// Tile view renders its Text in a different fashion
			if (control.View == View.Tile && Application.VisualStylesEnabled) {
				// Item.Text is drawn using its first subitem's bounds
				dc.DrawString (item.Text, item.Font, textBrush, item.SubItems [0].Bounds, format);

				int count = Math.Min (control.Columns.Count, item.SubItems.Count);
				for (int i = 1; i < count; i++) {
					ListViewItem.ListViewSubItem sub_item = item.SubItems [i];
					if (sub_item.Text == null || sub_item.Text.Length == 0)
						continue;

					Brush itemBrush = item.Selected && control.Focused ? 
						SystemBrushes.HighlightText : GetControlForeBrush (sub_item.ForeColor);
					dc.DrawString (sub_item.Text, sub_item.Font, itemBrush, sub_item.Bounds, format);
				}
			} else
			
			if (item.Text != null && item.Text.Length > 0) {
				Font font = item.Font;

				if (control.HotTracking && item.Hot)
					font = item.HotFont;

				if (item.Selected && control.Focused)
					dc.DrawString (item.Text, font, textBrush, highlight_rect, format);
				else
					dc.DrawString (item.Text, font, textBrush, text_rect, format);
			}

			if (item.Focused && control.Focused) {				
				Rectangle focus_rect = highlight_rect;
				if (control.FullRowSelect && control.View == View.Details) {
					int width = 0;
					foreach (ColumnHeader col in control.Columns)
						width += col.Width;
					focus_rect = new Rectangle (0, full_rect.Y, width, full_rect.Height);
				}
				if (control.ShowFocusCues) {
					if (item.Selected)
						CPDrawFocusRectangle (dc, focus_rect, ColorHighlightText, ColorHighlight);
					else
						CPDrawFocusRectangle (dc, focus_rect, control.ForeColor, control.BackColor);
				}
			}

			format.Dispose ();
		}

		protected virtual void DrawListViewSubItems (Graphics dc, ListView control, ListViewItem item)
		{
			int columns_count = control.Columns.Count;
			int count = Math.Min (item.SubItems.Count, columns_count);
			// 0th item already done (in this case)
			for (int i = 1; i < count; i++)
				DrawListViewSubItem (dc, control, item, i);

			// Fill in selection for remaining columns if Column.Count > SubItems.Count
			Rectangle sub_item_rect = item.GetBounds (ItemBoundsPortion.Label);
			if (item.Selected && (control.Focused || !control.HideSelection) && control.FullRowSelect) {
				for (int index = count; index < columns_count; index++) {
					ColumnHeader col = control.Columns [index];
					sub_item_rect.X = col.Rect.X - control.h_marker;
					sub_item_rect.Width = col.Wd;
					dc.FillRectangle (control.Focused ? SystemBrushes.Highlight : SystemBrushes.Control, 
							sub_item_rect);
				}
			}
		}

		protected virtual void DrawListViewSubItem (Graphics dc, ListView control, ListViewItem item, int index)
		{
			ListViewItem.ListViewSubItem subItem = item.SubItems [index];
			ColumnHeader col = control.Columns [index];
			StringFormat format = new StringFormat ();
			format.Alignment = col.Format.Alignment;
			format.LineAlignment = StringAlignment.Center;
			format.FormatFlags = StringFormatFlags.NoWrap;
			format.Trimming = StringTrimming.EllipsisCharacter;

			Rectangle sub_item_rect = subItem.Bounds;
			Rectangle sub_item_text_rect = sub_item_rect;
			sub_item_text_rect.X += 3;
			sub_item_text_rect.Width -= ListViewItemPaddingWidth;
						
			SolidBrush sub_item_back_br = null;
			SolidBrush sub_item_fore_br = null;
			Font sub_item_font = null;
						
			if (item.UseItemStyleForSubItems) {
				sub_item_back_br = ResPool.GetSolidBrush (item.BackColor);
				sub_item_fore_br = ResPool.GetSolidBrush (item.ForeColor);

				// Hot tracking for subitems only applies when UseStyle is true
				if (control.HotTracking && item.Hot)
					sub_item_font = item.HotFont;
				else
					sub_item_font = item.Font;
			} else {
				sub_item_back_br = ResPool.GetSolidBrush (subItem.BackColor);
				sub_item_fore_br = ResPool.GetSolidBrush (subItem.ForeColor);
				sub_item_font = subItem.Font;
			}
						
			if (item.Selected && (control.Focused || !control.HideSelection) && control.FullRowSelect) {
				Brush bg, text;
				if (control.Focused) {
					bg = SystemBrushes.Highlight;
					text = SystemBrushes.HighlightText;
				} else {
					bg = SystemBrushes.Control;
					text = sub_item_fore_br;
							
				}
							
				dc.FillRectangle (bg, sub_item_rect);
				if (subItem.Text != null && subItem.Text.Length > 0)
					dc.DrawString (subItem.Text, sub_item_font,
							text, sub_item_text_rect, format);
			} else {
				dc.FillRectangle (sub_item_back_br, sub_item_rect);
				if (subItem.Text != null && subItem.Text.Length > 0)
					dc.DrawString (subItem.Text, sub_item_font,
							sub_item_fore_br,
							sub_item_text_rect, format);
			}

			format.Dispose ();
		}

		protected virtual bool DrawListViewSubItemOwnerDraw (Graphics dc, ListViewItem item, ListViewItemStates state, int index)
		{
			ListView control = item.ListView;
			ListViewItem.ListViewSubItem subitem = item.SubItems [index];

			DrawListViewSubItemEventArgs args = new DrawListViewSubItemEventArgs (dc, subitem.Bounds, item, 
					subitem, item.Index, index, control.Columns [index], state);
			control.OnDrawSubItem (args);
			
			return !args.DrawDefault;
		}

		protected virtual void DrawListViewGroupHeader (Graphics dc, ListView control, ListViewGroup group)
		{
			Rectangle text_bounds = group.HeaderBounds;
			Rectangle header_bounds = group.HeaderBounds;
			text_bounds.Offset (8, 0);
			text_bounds.Inflate (-8, 0);
			int text_height = control.Font.Height + 2; // add a tiny padding between the text and the group line

			Font font = new Font (control.Font, control.Font.Style | FontStyle.Bold);
			Brush brush = new LinearGradientBrush (new Point (header_bounds.Left, 0), new Point (header_bounds.Left + ListViewGroupLineWidth, 0), 
					SystemColors.Desktop, Color.White);
			Pen pen = new Pen (brush);

			StringFormat sformat = new StringFormat ();
			switch (group.HeaderAlignment) {
				case HorizontalAlignment.Left:
					sformat.Alignment = StringAlignment.Near;
					break;
				case HorizontalAlignment.Center:
					sformat.Alignment = StringAlignment.Center;
					break;
				case HorizontalAlignment.Right:
					sformat.Alignment = StringAlignment.Far;
					break;
			}

			sformat.LineAlignment = StringAlignment.Near;
			dc.DrawString (group.Header, font, SystemBrushes.ControlText, text_bounds, sformat);
			dc.DrawLine (pen, header_bounds.Left, header_bounds.Top + text_height, header_bounds.Left + ListViewGroupLineWidth, 
					header_bounds.Top + text_height);

			sformat.Dispose ();
			font.Dispose ();
			pen.Dispose ();
			brush.Dispose ();
		}

		public override bool ListViewHasHotHeaderStyle {
			get {
				return false;
			}
		}

		// Sizing
		public override int ListViewGetHeaderHeight (ListView listView, Font font)
		{
			return ListViewGetHeaderHeight (font);
		}

		static int ListViewGetHeaderHeight (Font font)
		{
			return font.Height + 5;
		}

		public static int ListViewGetHeaderHeight ()
		{
			return ListViewGetHeaderHeight (ThemeEngine.Current.DefaultFont);
		}

		public override Size ListViewCheckBoxSize {
			get { return new Size (16, 16); }
		}

		public override int ListViewColumnHeaderHeight {
			get { return 16; }
		}

		public override int ListViewDefaultColumnWidth {
			get { return 60; }
		}

		public override int ListViewVerticalSpacing {
			get { return 22; }
		}

		public override int ListViewEmptyColumnWidth {
			get { return 10; }
		}

		public override int ListViewHorizontalSpacing {
			get { return 4; }
		}

		public override int ListViewItemPaddingWidth {
			get { return 6; }
		}

		public override Size ListViewDefaultSize {
			get { return new Size (121, 97); }
		}

		public override int ListViewGroupHeight { 
			get { return 20; }
		}

		public int ListViewGroupLineWidth {
			get { return 200; }
		}

		public override int ListViewTileWidthFactor {
			get { return 22; }
		}

		public override int ListViewTileHeightFactor {
			get { return 3; }
		}
		#endregion	// ListView

		#region MonthCalendar

		// draw the month calendar
		public override void DrawMonthCalendar(Graphics dc, Rectangle clip_rectangle, MonthCalendar mc) 
		{
			Rectangle client_rectangle = mc.ClientRectangle;
			Size month_size = mc.SingleMonthSize;
			// cache local copies of Marshal-by-ref internal members (gets around error CS0197)
			Size calendar_spacing = (Size)((object)mc.calendar_spacing);
			Size date_cell_size = (Size)((object)mc.date_cell_size);
			
			// draw the singlecalendars
			int x_offset = 1;
			int y_offset = 1;
			// adjust for the position of the specific month
			for (int i=0; i < mc.CalendarDimensions.Height; i++) 
			{
				if (i > 0) 
				{
					y_offset += month_size.Height + calendar_spacing.Height;
				}
				// now adjust for x position	
				for (int j=0; j < mc.CalendarDimensions.Width; j++) 
				{
					if (j > 0) 
					{
						x_offset += month_size.Width + calendar_spacing.Width;
					} 
					else 
					{
						x_offset = 1;
					}

					Rectangle month_rect = new Rectangle (x_offset, y_offset, month_size.Width, month_size.Height);
					if (month_rect.IntersectsWith (clip_rectangle)) {
						DrawSingleMonth (
							dc,
							clip_rectangle,
							month_rect,
							mc,
							i,
							j);
					}
				}
			}
			
			Rectangle bottom_rect = new Rectangle (
						client_rectangle.X,
						Math.Max(client_rectangle.Bottom - date_cell_size.Height - 3, 0),
						client_rectangle.Width,
						date_cell_size.Height + 2);
			// draw the today date if it's set
			if (mc.ShowToday && bottom_rect.IntersectsWith (clip_rectangle)) 
			{
				dc.FillRectangle (GetControlBackBrush (mc.BackColor), bottom_rect);
				if (mc.ShowToday) {
					// draw the vertical divider
					int col_offset = (mc.ShowWeekNumbers) ? 1 : 0;
					int vert_divider_y = bottom_rect.Top;
					dc.DrawLine(
						ResPool.GetPen(mc.TitleBackColor),
						client_rectangle.X + (col_offset * date_cell_size.Width) + mc.divider_line_offset,
						client_rectangle.Y + vert_divider_y,
						client_rectangle.Right - mc.divider_line_offset,
						client_rectangle.Y + vert_divider_y);

					int today_offset = 5;
					if (mc.ShowTodayCircle) 
					{
						Rectangle today_circle_rect = new Rectangle(
							client_rectangle.X + 5,
							Math.Max(client_rectangle.Bottom - date_cell_size.Height, 0),
							date_cell_size.Width,
							date_cell_size.Height);
						today_circle_rect.Inflate(-2, -2);
						DrawTodayCircle (dc, today_circle_rect);
						today_offset += date_cell_size.Width + 5;
					}
					// draw today's date
					StringFormat text_format = new StringFormat();
					text_format.LineAlignment = StringAlignment.Center;
					text_format.Alignment = StringAlignment.Near;
					Rectangle today_rect = new Rectangle (
							today_offset + client_rectangle.X,
							Math.Max(client_rectangle.Bottom - date_cell_size.Height, 0),
							Math.Max(client_rectangle.Width - 2 - today_offset, 0),
							date_cell_size.Height - 2);
					string today = DateTime.Now.ToSafeString("d");
					string label = Strings.ResourceManager.GetString("MonthCalToday", DateTimeUtility.PreferredCulture);
					dc.DrawString (label + ": " + today, mc.bold_font, GetControlForeBrush (mc.ForeColor), today_rect, text_format);
					text_format.Dispose ();
				}				
			}
			
			Brush border_brush;
			
			if (mc.owner == null)
				border_brush = GetControlBackBrush(mc.BackColor);
			else
				border_brush = SystemBrushes.ActiveBorder;

			// finally paint the borders of the calendars as required
			for (int i = 0; i <= mc.CalendarDimensions.Width; i++) {
				if (i == 0 && clip_rectangle.X == client_rectangle.X) {
					dc.FillRectangle (border_brush, client_rectangle.X, client_rectangle.Y, 1, client_rectangle.Height);
				} else if (i == mc.CalendarDimensions.Width && clip_rectangle.Right == client_rectangle.Right) {
					dc.FillRectangle (border_brush, client_rectangle.Right - 1, client_rectangle.Y, 1, client_rectangle.Height);
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X + (month_size.Width*i) + (calendar_spacing.Width * (i-1)) + 1,
						client_rectangle.Y,
						calendar_spacing.Width,
						client_rectangle.Height);
					if (i < mc.CalendarDimensions.Width && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (border_brush, rect);
					}
				}
			}
			for (int i = 0; i <= mc.CalendarDimensions.Height; i++) {
				if (i == 0 && clip_rectangle.Y == client_rectangle.Y) {
					dc.FillRectangle (border_brush, client_rectangle.X, client_rectangle.Y, client_rectangle.Width, 1);
				} else if (i == mc.CalendarDimensions.Height && clip_rectangle.Bottom == client_rectangle.Bottom) {
					dc.FillRectangle (border_brush, client_rectangle.X, client_rectangle.Bottom - 1, client_rectangle.Width, 1);
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X,
						client_rectangle.Y + (month_size.Height*i) + (calendar_spacing.Height*(i-1)) + 1,
						client_rectangle.Width,
						calendar_spacing.Height);
					if (i < mc.CalendarDimensions.Height && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (border_brush, rect);
					}
				}
			}
			
			// draw the drop down border if need
			if (mc.owner != null) {
				Rectangle bounds = mc.ClientRectangle;
				if (clip_rectangle.Contains (mc.Location)) {
					// find out if top or left line to draw
					if(clip_rectangle.Contains (new Point (bounds.Left, bounds.Bottom))) {
					
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Y, bounds.X, bounds.Bottom-1);
					}
					if(clip_rectangle.Contains (new Point (bounds.Right, bounds.Y))) {
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Y, bounds.Right-1, bounds.Y);
					}
				}
				if (clip_rectangle.Contains (new Point(bounds.Right, bounds.Bottom))) {
					// find out if bottom or right line to draw
					if(clip_rectangle.Contains (new Point (bounds.Left, bounds.Bottom))) {
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1);
					}
					if(clip_rectangle.Contains (new Point (bounds.Right, bounds.Y))) {
						dc.DrawLine (SystemPens.ControlText, bounds.Right-1, bounds.Y, bounds.Right-1, bounds.Bottom-1);
					}
				}
			}
		}

		// darws a single part of the month calendar (with one month)
		private void DrawSingleMonth(Graphics dc, Rectangle clip_rectangle, Rectangle rectangle, MonthCalendar mc, int row, int col) 
		{
			// cache local copies of Marshal-by-ref internal members (gets around error CS0197)
			Size title_size = (Size)((object)mc.title_size);
			Size date_cell_size = (Size)((object)mc.date_cell_size);
			DateTime current_month = (DateTime)((object)mc.current_month);
			DateTime sunday = new DateTime(2006, 10, 1);

			// draw the title back ground
			DateTime this_month = current_month.AddMonths (row*mc.CalendarDimensions.Width+col);
			Rectangle title_rect = new Rectangle(rectangle.X, rectangle.Y, title_size.Width, title_size.Height);
			if (title_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), title_rect);

				Rectangle year_rect, upRect, downRect;
				mc.GetYearNameRectangles(title_rect, row * mc.CalendarDimensions.Width + col, out year_rect, out upRect, out downRect);
				year_rect.Inflate(1, 0);
				if (mc.ShowYearUpDown)
					dc.FillRoundRect(ResPool.GetSolidBrush(SystemColors.Control), year_rect, 3);

				// draw the title				
				string title_text = this_month.ToSafeString ("MMMM yyyy");
				dc.DrawString (title_text, mc.bold_font, ResPool.GetSolidBrush (mc.TitleForeColor), title_rect, mc.centered_format);

				if (mc.ShowYearUpDown) {
					upRect.Inflate(-(upRect.Width - upRect.Height) / 2, -2);
					dc.FillTriangle(GetArrowBrush(mc, mc.IsYearGoingUp), upRect, 0);
					//using (var pen = CreateArrowPen(mc, mc.IsYearGoingUp))
					//	dc.DrawArrow(pen, upRect, 0);

					downRect.Inflate(-(downRect.Width - downRect.Height) / 2, -2);
					dc.FillTriangle(GetArrowBrush(mc, mc.IsYearGoingDown), downRect, 180);
					//using (var pen = CreateArrowPen(mc, mc.IsYearGoingDown))
					//	dc.DrawArrow(pen, downRect, 180);
				}

				// draw previous and next buttons if it's time
				if (row == 0 && col == 0) 
				{
					// draw previous button
					DrawMonthCalendarButton (
						dc,
						rectangle,
						mc,
						title_size,
						mc.button_x_offset,
						(System.Drawing.Size)((object)mc.button_size),
						true);
				}
				if (row == 0 && col == mc.CalendarDimensions.Width-1) 
				{
					// draw next button
					DrawMonthCalendarButton (
						dc,
						rectangle,
						mc,
						title_size,
						mc.button_x_offset,
						(System.Drawing.Size)((object)mc.button_size),
						false);
				}
			}
			
			// set the week offset and draw week nums if needed
			int col_offset = (mc.ShowWeekNumbers) ? 1 : 0;
			Rectangle day_name_rect = new Rectangle(
				rectangle.X,
				rectangle.Y + title_size.Height,
				(7 + col_offset) * date_cell_size.Width,
				date_cell_size.Height);
			if (day_name_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (GetControlBackBrush (mc.BackColor), day_name_rect);
				// draw the day names 
				DayOfWeek first_day_of_week = mc.GetDayOfWeek(mc.FirstDayOfWeek);
				for (int i=0; i < 7; i++) 
				{
					int position = i - (int) first_day_of_week;
					if (position < 0) 
					{
						position = 7 + position;
					}
					// draw it
					Rectangle day_rect = new Rectangle(
						day_name_rect.X + ((i + col_offset)* date_cell_size.Width),
						day_name_rect.Y,
						date_cell_size.Width,
						date_cell_size.Height);
					dc.DrawString (sunday.AddDays (i + (int) first_day_of_week).ToSafeString ("ddd"), mc.Font, ResPool.GetSolidBrush (SystemColors.ControlText), day_rect, mc.centered_format);
				}
				
				// draw the vertical divider
				int vert_divider_y = Math.Max(title_size.Height+ date_cell_size.Height-1, 0);
				dc.DrawLine(
					ResPool.GetPen(mc.TitleBackColor),
					rectangle.X + (col_offset * date_cell_size.Width) + mc.divider_line_offset,
					rectangle.Y + vert_divider_y,
					rectangle.Right - mc.divider_line_offset,
					rectangle.Y + vert_divider_y);
			}


			// draw the actual date items in the grid (including the week numbers)
			Rectangle date_rect = new Rectangle (
				rectangle.X,
				rectangle.Y + title_size.Height + date_cell_size.Height,
				date_cell_size.Width,
				date_cell_size.Height);
			int month_row_count = 0;
			bool draw_week_num_divider = false;
			DateTime current_date = mc.GetFirstDateInMonthGrid ( new DateTime (this_month.Year, this_month.Month, 1));
			for (int i=0; i < 6; i++) 
			{
				// establish if this row is in our clip_area
				Rectangle row_rect = new Rectangle (
					rectangle.X,
					rectangle.Y + title_size.Height + (date_cell_size.Height * (i+1)),
					date_cell_size.Width * 7,
					date_cell_size.Height);
				if (mc.ShowWeekNumbers) {
					row_rect.Width += date_cell_size.Width;
				}
		
				bool draw_row = row_rect.IntersectsWith (clip_rectangle);
				if (draw_row) {
					dc.FillRectangle (GetControlBackBrush (mc.BackColor), row_rect);
				}
				// establish if this is a valid week to draw
				if (mc.IsValidWeekToDraw (this_month, current_date, row, col)) {
					month_row_count = i;
				}
				
				// draw the week number if required
				if (mc.ShowWeekNumbers && month_row_count == i) {
					if (!draw_week_num_divider) {
						draw_week_num_divider = draw_row;
					}
					// get the week for this row
					int week = mc.GetWeekOfYear (current_date);	

					if (draw_row) {
						dc.DrawString (
							week.ToString(),
							mc.Font,
							ResPool.GetSolidBrush (mc.TitleBackColor),
							date_rect,
							mc.centered_format);
					}
					date_rect.Offset(date_cell_size.Width, 0);
				}
								
				// only draw the days if we have to
				if(month_row_count == i) {
					for (int j=0; j < 7; j++) 
					{
						if (draw_row) {
							DrawMonthCalendarDate (
								dc,
								date_rect,
								mc,
								current_date,
								this_month,
								row,
								col);
						}

						// move the day on
						current_date = current_date.AddDays(1);
						date_rect.Offset(date_cell_size.Width, 0);
					}

					// shift the rectangle down one row
					int offset = (mc.ShowWeekNumbers) ? -8 : -7;
					date_rect.Offset(offset*date_cell_size.Width, date_cell_size.Height);
				}
			}

			// month_row_count is zero based, so add one
			month_row_count++;

			// draw week numbers if required
			if (draw_week_num_divider) {
				col_offset = 1;
				dc.DrawLine (
					ResPool.GetPen (mc.ForeColor),
					rectangle.X + date_cell_size.Width - 1,
					rectangle.Y + title_size.Height + date_cell_size.Height + mc.divider_line_offset,
					rectangle.X + date_cell_size.Width - 1,
					rectangle.Y + title_size.Height + date_cell_size.Height + (month_row_count * date_cell_size.Height) - mc.divider_line_offset);
			}
		}

		private Brush GetArrowBrush(MonthCalendar mc, bool highlighted)
		{
			var color = mc.TitleForeColor.ToNSColor();
			color = highlighted ? color.ShadowWithLevel(0.2f) : color;
			return ResPool.GetSolidBrush(color.ToSDColor());
		}

		private Pen CreateArrowPen(MonthCalendar mc, bool highlighted)
		{
			var color = mc.TitleForeColor.ToNSColor();
			color = highlighted ? color.ShadowWithLevel(0.2f) : color;
			var pen = new Pen(color.ToSDColor(), 1.5f);
			pen.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Triangle);
			return pen;
		}

		// draws the previous or next button
		private void DrawMonthCalendarButton (Graphics dc, Rectangle rectangle, MonthCalendar mc, Size title_size, int x_offset, Size button_size, bool is_previous) 
		{
			// prepare the button
			if (is_previous) 
			{
				Rectangle button_rect = new Rectangle (
					rectangle.X + 1 + x_offset,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));
				button_rect.Inflate(-3, -3);

				var color = mc.TitleForeColor.ToNSColor();
				color = mc.is_previous_clicked ? color.ShadowWithLevel(0.2f) : color;
				var brush = ResPool.GetSolidBrush(color.ToSDColor());
				dc.FillEquilateralTriangle(brush, button_rect, -90);
			}
			else
			{
				Rectangle button_rect = new Rectangle (
					rectangle.Right - 1 - x_offset - button_size.Width,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));
				button_rect.Inflate(-3, -3);

				var color = mc.TitleForeColor.ToNSColor();
				color = mc.is_next_clicked ? color.ShadowWithLevel(0.2f) : color;
				var brush = ResPool.GetSolidBrush(color.ToSDColor());
				dc.FillEquilateralTriangle(brush, button_rect, 90);
			}
		}

		// draws one day in the calendar grid
		private void DrawMonthCalendarDate (Graphics dc, Rectangle rectangle, MonthCalendar mc,	DateTime date, DateTime month, int row, int col) {
			const int inflate = -2;
			const int cornerRadius = 3;
			Color date_color = mc.ForeColor;
			Rectangle interior = Rectangle.Inflate(rectangle, inflate, inflate);

			// find out if we are the lead of the first calendar or the trail of the last calendar						
			if (date.Year != month.Year || date.Month != month.Month) {
				DateTime check_date = month.AddMonths (-1);
				// check if it's the month before 
				if (check_date.Year == date.Year && check_date.Month == date.Month && row == 0 && col == 0) {
					date_color = mc.TrailingForeColor;
				} else {
					// check if it's the month after
					check_date = month.AddMonths (1);
					if (check_date.Year == date.Year && check_date.Month == date.Month && row == mc.CalendarDimensions.Height-1 && col == mc.CalendarDimensions.Width-1) {
						date_color = mc.TrailingForeColor;
					} else {
						return;
					}
				}
			} else {
				date_color = mc.ForeColor;
			}

			Color selectionBackColor = SystemColors.Highlight;
			Color selectionForeColor = SystemColors.HighlightText;

			if (date == mc.SelectionStart.Date && date == mc.SelectionEnd.Date) {
				// see if the date is in the start of selection
				date_color = selectionForeColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate (rectangle, inflate, inflate);
				dc.FillRoundRect(ResPool.GetSolidBrush(selectionBackColor), selection_rect, cornerRadius);
			} else if (date == mc.SelectionStart.Date) {
				// see if the date is in the start of selection
				date_color = selectionForeColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = rectangle.Inflate(inflate, inflate, 0, inflate);
				dc.FillRoundRect(ResPool.GetSolidBrush (selectionBackColor), selection_rect, cornerRadius, 0, 0, cornerRadius);
			} else if (date == mc.SelectionEnd.Date) {
				// see if it is the end of selection
				date_color = selectionForeColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = rectangle.Inflate(0, inflate, inflate, inflate);
				dc.FillRoundRect(ResPool.GetSolidBrush(selectionBackColor), selection_rect, 0, cornerRadius, cornerRadius, 0);
			}
			else if (date > mc.SelectionStart.Date && date < mc.SelectionEnd.Date) {
				// now see if it's in the middle
				date_color = selectionForeColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate (rectangle, 0, inflate);
				dc.FillRectangle (ResPool.GetSolidBrush (selectionBackColor), selection_rect);
			}

			// establish if it's a bolded font
			Font font = mc.IsBoldedDate (date) ? mc.bold_font : mc.Font;

			// just draw the date now
			dc.DrawString (date.Day.ToString(), font, ResPool.GetSolidBrush (date_color), rectangle, mc.centered_format);

			// today circle if needed
			if (mc.ShowTodayCircle && date == DateTime.Now.Date) {
				DrawTodayCircle (dc, interior);
			}

			// draw the selection grid
			//if (mc.is_date_clicked && mc.clicked_date == date) {
			//	Pen pen = ResPool.GetDashPen (Color.Black, DashStyle.Dot);
			//	dc.DrawRectangle (pen, interior);
			//}
		}

		private void DrawTodayCircle (Graphics dc, Rectangle rectangle) {

			const int cornerRadius = 3;
			const float strokeWidth = 1f;
			var r = rectangle.ToRectangleF();
			r.Width -= 1; r.Height -= 1;
			var circle_color = typeof(NSColor).GetClass().RespondsToSelector(new ObjCRuntime.Selector("controlAccentColor"))
				? NSColor.ControlAccent.ToSDColor() : Color.FromArgb(10, 96, 254);
			using (Pen pen = new Pen(circle_color, strokeWidth))
				dc.DrawRoundRect(pen, r, cornerRadius);
		}

		#endregion 	// MonthCalendar

		#region Panel
		public override Size PanelDefaultSize {
			get {
				return new Size (200, 100);
			}
		}
		#endregion	// Panel

		#region PictureBox
		public override void DrawPictureBox (Graphics dc, Rectangle clip, PictureBox pb) {
			Rectangle client = pb.ClientRectangle;

			client = new Rectangle (client.Left + pb.Padding.Left, client.Top + pb.Padding.Top, client.Width - pb.Padding.Horizontal, client.Height - pb.Padding.Vertical);

			// FIXME - instead of drawing the whole picturebox every time
			// intersect the clip rectangle with the drawn picture and only draw what's needed,
			// Also, we only need a background fill where no image goes
			if (pb.Image != null) {
				switch (pb.SizeMode) {
				case PictureBoxSizeMode.StretchImage:
					dc.DrawImage (pb.Image, client.Left, client.Top, client.Width, client.Height);
					break;

				case PictureBoxSizeMode.CenterImage:
					dc.DrawImage (pb.Image, (client.Width / 2) - (pb.Image.Width / 2), (client.Height / 2) - (pb.Image.Height / 2));
					break;

				case PictureBoxSizeMode.Zoom:
					Size image_size;
					
					if (((float)pb.Image.Width / (float)pb.Image.Height) >= ((float)client.Width / (float)client.Height))
						image_size = new Size (client.Width, (pb.Image.Height * client.Width) / pb.Image.Width);
					else
						image_size = new Size ((pb.Image.Width * client.Height) / pb.Image.Height, client.Height);

					dc.DrawImage (pb.Image, (client.Width / 2) - (image_size.Width / 2), (client.Height / 2) - (image_size.Height / 2), image_size.Width, image_size.Height);
					break;

				default:
					// Normal, AutoSize
					dc.DrawImage (pb.Image, client.Left, client.Top, pb.Image.Width, pb.Image.Height);
					break;
				}

				return;
			}
		}

		public override Size PictureBoxDefaultSize {
			get {
				return new Size (100, 50);
			}
		}
		#endregion	// PictureBox

		#region PrintPreviewControl
		public override int PrintPreviewControlPadding {
			get { return 8; }
		}

		public override Size PrintPreviewControlGetPageSize (PrintPreviewControl preview)
		{
			int page_width, page_height;
			int padding = PrintPreviewControlPadding;
			PreviewPageInfo[] pis = preview.page_infos;

			if (preview.AutoZoom) {
				int height_available = preview.ClientRectangle.Height - (preview.Rows) * padding - 2 * padding;
				int width_available = preview.ClientRectangle.Width - (preview.Columns - 1) * padding - 2 * padding;

				float image_ratio = (float)pis[0].Image.Width / pis[0].Image.Height;

				/* try to lay things out using the width to determine the size */
				page_width = width_available / preview.Columns;
				page_height = (int)(page_width / image_ratio);

				/* does the height fit? */
				if (page_height * (preview.Rows + 1) > height_available) {
					/* no, lay things out via the height */
					page_height = height_available / (preview.Rows + 1);
					page_width = (int)(page_height * image_ratio);
				}
			}
			else {
				page_width = (int)(pis[0].Image.Width * preview.Zoom);
				page_height = (int)(pis[0].Image.Height * preview.Zoom);
			}

			return new Size (page_width, page_height);
		}

		public override void PrintPreviewControlPaint (PaintEventArgs pe, PrintPreviewControl preview, Size page_size)
		{
			int padding = 8;
			PreviewPageInfo[] pis = preview.page_infos;
			if (pis == null)
				return;

			int page_x, page_y;

			int width = page_size.Width * preview.Columns + padding * (preview.Columns - 1) + 2 * padding;
			int height = page_size.Height * (preview.Rows + 1) + padding * preview.Rows + 2 * padding;

			Rectangle viewport = preview.ViewPort;

			pe.Graphics.Clip = new Region (viewport);

			/* center things if we can */
			int off_x = viewport.Width / 2 - width / 2;
			if (off_x < 0) off_x = 0;
			int off_y = viewport.Height / 2 - height / 2;
			if (off_y < 0) off_y = 0;

			page_y = off_y + padding - preview.vbar_value;

			int p = preview.StartPage;
			for (int py = 0; py < preview.Rows + 1; py ++) {
				page_x = off_x + padding - preview.hbar_value;
				for (int px = 0; px < preview.Columns; px ++) {
					if (p >= pis.Length)
						return;
					Image image = preview.image_cache[p];
					if (image == null)
						image = pis[p].Image;
					Rectangle dest = new Rectangle (new Point (page_x, page_y), page_size);

					pe.Graphics.DrawImage (image, dest, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				
					page_x += padding + page_size.Width;
					p++;
				}
				page_y += padding + page_size.Height;
			}
		}
		#endregion      // PrintPreviewControl

		#region ProgressBar
		public override void DrawProgressBar (Graphics dc, Rectangle clip_rect, ProgressBar ctrl) 
		{
			#if !MACOS_THEME
			Rectangle client_area = ctrl.client_area;
			
			/* Draw border */
			CPDrawBorder3D (dc, ctrl.ClientRectangle, Border3DStyle.SunkenOuter, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom & ~Border3DSide.Middle, ColorControl);
			
			/* Draw Blocks */
			int draw_mode = 0;
			int max_blocks = int.MaxValue;
			int start_pixel = client_area.X;
			draw_mode = (int) ctrl.Style;

			switch (draw_mode) {
			case 1: { // Continuous
				int pixels_to_draw;
				pixels_to_draw = (int)(client_area.Width * ((double)(ctrl.Value - ctrl.Minimum) / (double)(Math.Max(ctrl.Maximum - ctrl.Minimum, 1))));
				dc.FillRectangle (ResPool.GetSolidBrush (ctrl.ForeColor), new Rectangle (client_area.X, client_area.Y, pixels_to_draw, client_area.Height));
				break;
			}
			case 2: // Marquee
				if (XplatUI.ThemesEnabled) {
					int ms_diff = (int) (DateTime.Now - ctrl.start).TotalMilliseconds;
					double percent_done = (double) ms_diff / ProgressBarMarqueeSpeedScaling 
						% (double)ctrl.MarqueeAnimationSpeed / (double)ctrl.MarqueeAnimationSpeed;
					max_blocks = 5;
					start_pixel = client_area.X + (int) (client_area.Width * percent_done);
				}
				
				goto case 0;
			case 0:
			default:  // Blocks
				Rectangle block_rect;
				int space_betweenblocks = ProgressBarChunkSpacing;
				int block_width;
				int increment;
				int barpos_pixels;
				int block_count = 0;
				
				block_width = ProgressBarGetChunkSize (client_area.Height);
				block_width = Math.Max (block_width, 0); // block_width is used to break out the loop below, it must be >= 0!
				barpos_pixels = (int)(((double)(ctrl.Value - ctrl.Minimum) * client_area.Width) / (Math.Max (ctrl.Maximum - ctrl.Minimum, 1)));
				increment = block_width + space_betweenblocks;
				
				block_rect = new Rectangle (start_pixel, client_area.Y, block_width, client_area.Height);
				while (true) {
					if (max_blocks != int.MaxValue) {
						if (block_count >= max_blocks)
							break;
						if (block_rect.X > client_area.Width)
							block_rect.X -= client_area.Width;
					} else {
						if ((block_rect.X - client_area.X) >= barpos_pixels)
							break;
					}
					
					if (clip_rect.IntersectsWith (block_rect) == true) {				
						dc.FillRectangle (ResPool.GetSolidBrush (ctrl.ForeColor), block_rect);
					}				
					
					block_rect.X  += increment;
					block_count++;
				}
				break;
			}
			#endif
		}
		
		public const int ProgressBarChunkSpacing = 2;

		public static int ProgressBarGetChunkSize ()
		{
			return ProgressBarGetChunkSize (ProgressBarDefaultHeight);
		}
		
		static int ProgressBarGetChunkSize (int progressBarClientAreaHeight)
		{
			int size = (progressBarClientAreaHeight * 2) / 3;
			return size;
		}

		const int ProgressBarDefaultHeight = 23;

		public override Size ProgressBarDefaultSize {
			get {
				return new Size (100, ProgressBarDefaultHeight);
			}
		}

		public const double ProgressBarMarqueeSpeedScaling = 15;

		#endregion	// ProgressBar

		#region RadioButton
		public override void DrawRadioButton (Graphics dc, Rectangle clip_rectangle, RadioButton rb) {

			if (rb.Focused && rb.Enabled && rb.appearance != Appearance.Button && rb.Text != String.Empty && rb.ShowFocusCues) {
				//SizeF text_size = dc.MeasureString (rb.Text, rb.Font);
				
				//Rectangle focus_rect = Rectangle.Empty;
				//focus_rect.X = text_rectangle.X;
				//focus_rect.Y = (int)((text_rectangle.Height - text_size.Height) / 2);
				//focus_rect.Size = text_size.ToSize ();

				//RadioButton_DrawFocus (rb, dc, focus_rect);
			}
			
		}

		public override Size RadioButtonDefaultSize {
			get {
				return new Size (104,32);
			}
		}

		NSButtonCell sharedRBCell;
		NSButtonCell SharedRadioButtonCell { get { return GetSharedRadioButtonCell(); } }

		internal virtual NSButtonCell GetSharedRadioButtonCell(RadioButton rb = null)
		{
			if (sharedRBCell == null)
			{
				sharedRBCell = new NSButtonCell();
				sharedRBCell.SetButtonType(NSButtonType.Radio);
				sharedRBCell.Title = string.Empty;
				sharedRBCell.Bezeled = false;
				sharedRBCell.Bordered = false;
			}

			if (rb != null)
			{
				sharedRBCell.Alignment = rb.TextAlign.ToNSTextAlignment();
				sharedRBCell.Highlighted = rb.Pressed;
				sharedRBCell.State = rb.Checked ? NSCellStateValue.On : NSCellStateValue.Off;
				sharedRBCell.Enabled = rb.Enabled;
			}
			return sharedRBCell;
		}

		public override Size CalculateRadioButtonAutoSize(RadioButton rb)
		{
			Size ret_size = Size.Empty;
			Size text_size = TextRenderer.MeasureTextInternal(rb.Text, rb.Font, rb.UseCompatibleTextRendering);
			Size image_size = rb.Image == null ? Size.Empty : rb.Image.Size;

			// Pad the text size
			if (rb.Text.Length != 0)
			{
				text_size.Height += 4;
				text_size.Width += 4;
			}

			text_size.Height = Math.Max(text_size.Height, RBSize);

			switch (rb.TextImageRelation)
			{
				case TextImageRelation.Overlay:
					ret_size.Height = Math.Max(rb.Text.Length == 0 ? 0 : text_size.Height, image_size.Height);
					ret_size.Width = Math.Max(text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageAboveText:
				case TextImageRelation.TextAboveImage:
					ret_size.Height = text_size.Height + image_size.Height;
					ret_size.Width = Math.Max(text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageBeforeText:
				case TextImageRelation.TextBeforeImage:
					ret_size.Height = Math.Max(text_size.Height, image_size.Height);
					ret_size.Width = text_size.Width + image_size.Width;
					break;
			}

			// Pad the result
			ret_size.Height += (rb.Padding.Vertical);
			ret_size.Width += (rb.Padding.Horizontal) + RBSize;

			// There seems to be a minimum height
			if (ret_size.Height == rb.Padding.Vertical)
				ret_size.Height += RBSize;

			return ret_size;
		}

		public override void DrawRadioButton (Graphics g, RadioButton rb, Rectangle glyphArea, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			if (rb.FlatStyle == FlatStyle.Flat || rb.FlatStyle == FlatStyle.Popup)
			{
				glyphArea.Height -= 2;
				glyphArea.Width -= 2;
			}

			DrawRadioButtonGlyph(g, rb, glyphArea);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawRadioButtonImage(g, rb, imageBounds);

			//if (rb.Focused && rb.Enabled && rb.ShowFocusCues && textBounds.Size != Size.Empty)
				//DrawRadioButtonFocus(g, rb, textBounds);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawRadioButtonText(g, rb, textBounds);

			//var cell = GetSharedRadioButtonCell(rb);
			//cell.DrawWithFrame(rb.ClientRectangle.ToCGRect(), NSView.FocusView());
		}

		public virtual void DrawRadioButtonGlyph (Graphics g, RadioButton rb, Rectangle glyphArea)
		{
			var cell = GetSharedRadioButtonCell(rb);
			var frame = glyphArea.ToCGRect();
			var view = NSView.FocusView();

			if (rb.Focused && rb.Enabled && rb.ShowFocusCues)
				cell.DrawFocusRing(frame, view);

			cell.DrawWithFrame(frame, view);
		}

		public virtual void DrawRadioButtonFocus (Graphics g, RadioButton rb, Rectangle focusArea)
		{
			ControlPaint.DrawFocusRectangle (g, focusArea);
		}

		public virtual void DrawRadioButtonImage (Graphics g, RadioButton rb, Rectangle imageBounds)
		{
			if (rb.Enabled)
				g.DrawImage(rb.Image, imageBounds);
			else
				CPDrawImageDisabled(g, rb.Image, imageBounds.Left, imageBounds.Top, ColorControl);
		}

		public virtual void DrawRadioButtonText (Graphics g, RadioButton rb, Rectangle textBounds)
		{
			if (rb.Enabled)
				TextRenderer.DrawTextInternal(g, rb.Text, rb.Font, textBounds, rb.ForeColor, rb.TextFormatFlags, rb.UseCompatibleTextRendering);
			else
				DrawStringDisabled20(g, rb.Text, rb.Font, textBounds, rb.BackColor, rb.TextFormatFlags, rb.UseCompatibleTextRendering);
		}

		public override void CalculateRadioButtonTextAndImageLayout (ButtonBase b, Point offset, out Rectangle glyphArea, out Rectangle textRectangle, out Rectangle imageRectangle)
		{
			CalculateCheckBoxTextAndImageLayout (b, offset, out glyphArea, out textRectangle, out imageRectangle);
		}
		#endregion	// RadioButton

		#region ScrollBar
		public override void DrawScrollBar (Graphics dc, Rectangle clip, ScrollBar bar)
		{
#if !MACOS_THEME
			int		scrollbutton_width = bar.scrollbutton_width;
			int		scrollbutton_height = bar.scrollbutton_height;
			Rectangle	first_arrow_area;
			Rectangle	second_arrow_area;			
			Rectangle	thumb_pos;
			
			thumb_pos = bar.ThumbPos;

			if (bar.vert) {
				first_arrow_area = new Rectangle(0, 0, bar.Width, scrollbutton_height);
				bar.FirstArrowArea = first_arrow_area;

				second_arrow_area = new Rectangle(0, bar.ClientRectangle.Height - scrollbutton_height, bar.Width, scrollbutton_height);
				bar.SecondArrowArea = second_arrow_area;

				thumb_pos.Width = bar.Width;
				bar.ThumbPos = thumb_pos;

				Brush VerticalBrush;
				/* Background, upper track */
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Backwards)
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle UpperTrack = new Rectangle (0, 0, bar.ClientRectangle.Width, bar.ThumbPos.Bottom);
				if (clip.IntersectsWith (UpperTrack))
					dc.FillRectangle (VerticalBrush, UpperTrack);

				/* Background, lower track */
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Forward)
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle LowerTrack = new Rectangle (0, bar.ThumbPos.Bottom, bar.ClientRectangle.Width, bar.ClientRectangle.Height - bar.ThumbPos.Bottom);
				if (clip.IntersectsWith (LowerTrack))
					dc.FillRectangle (VerticalBrush, LowerTrack);

				/* Buttons */
				if (clip.IntersectsWith (first_arrow_area))
					CPDrawScrollButton (dc, first_arrow_area, ScrollButton.Up, bar.firstbutton_state);
				if (clip.IntersectsWith (second_arrow_area))
					CPDrawScrollButton (dc, second_arrow_area, ScrollButton.Down, bar.secondbutton_state);
			} else {
				first_arrow_area = new Rectangle(0, 0, scrollbutton_width, bar.Height);
				bar.FirstArrowArea = first_arrow_area;

				second_arrow_area = new Rectangle (bar.ClientRectangle.Width - scrollbutton_width, 0, scrollbutton_width, bar.Height);
				bar.SecondArrowArea = second_arrow_area;

				thumb_pos.Height = bar.Height;
				bar.ThumbPos = thumb_pos;

				Brush HorizontalBrush;
				//Background, left track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Backwards)
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle LeftTrack = new Rectangle (0, 0, bar.ThumbPos.Right, bar.ClientRectangle.Height);
				if (clip.IntersectsWith (LeftTrack))
					dc.FillRectangle (HorizontalBrush, LeftTrack);

				//Background, right track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Forward)
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle RightTrack = new Rectangle (bar.ThumbPos.Right, 0, bar.ClientRectangle.Width - bar.ThumbPos.Right, bar.ClientRectangle.Height);
				if (clip.IntersectsWith (RightTrack))
					dc.FillRectangle (HorizontalBrush, RightTrack);

				/* Buttons */
				if (clip.IntersectsWith (first_arrow_area))
					CPDrawScrollButton (dc, first_arrow_area, ScrollButton.Left, bar.firstbutton_state);
				if (clip.IntersectsWith (second_arrow_area))
					CPDrawScrollButton (dc, second_arrow_area, ScrollButton.Right, bar.secondbutton_state);
			}

			/* Thumb */
			ScrollBar_DrawThumb(bar, thumb_pos, clip, dc);				
#endif
		}

		protected virtual void ScrollBar_DrawThumb(ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc)
		{
			if (bar.Enabled && thumb_pos.Width > 0 && thumb_pos.Height > 0 && clip.IntersectsWith(thumb_pos))
				DrawScrollButtonPrimitive(dc, thumb_pos, ButtonState.Normal);
		}

		public override int ScrollBarButtonSize {
			get { return (int)NSScroller.GetScrollerWidth(NSControlSize.Regular, NSScrollerStyle.Legacy); }
		}

		public override bool ScrollBarHasHotElementStyles {
			get {
				return false;
			}
		}

		public override bool ScrollBarHasPressedThumbStyle {
			get { 
				return false;
			}
		}

		public override bool ScrollBarHasHoverArrowButtonStyle {
			get {
				return false;
			}
		}
		#endregion	// ScrollBar

		#region TabControl

		#region TabControl settings

		public override Size TabControlDefaultItemSize {
			get { return ThemeElements.CurrentTheme.TabControlPainter.DefaultItemSize; }
		}

		public override Point TabControlDefaultPadding {
			get { return ThemeElements.CurrentTheme.TabControlPainter.DefaultPadding; }
		}

		public override int TabControlMinimumTabWidth {
			get { return ThemeElements.CurrentTheme.TabControlPainter.MinimumTabWidth; }
		}

		public override Rectangle TabControlSelectedDelta {
			get { return ThemeElements.CurrentTheme.TabControlPainter.SelectedTabDelta; }
		}

		public override int TabControlSelectedSpacing {
			get { return ThemeElements.CurrentTheme.TabControlPainter.SelectedSpacing; }
		}
		
		public override int TabPanelOffsetX {
			get { return ThemeElements.CurrentTheme.TabControlPainter.TabPanelOffset.X; }
		}
		
		public override int TabPanelOffsetY {
			get { return ThemeElements.CurrentTheme.TabControlPainter.TabPanelOffset.Y; }
		}

		public override int TabControlColSpacing {
			get { return ThemeElements.CurrentTheme.TabControlPainter.ColSpacing; }
		}

		public override Point TabControlImagePadding {
			get { return ThemeElements.CurrentTheme.TabControlPainter.ImagePadding; }
		}

		public override int TabControlScrollerWidth {
			get {return ThemeElements.CurrentTheme.TabControlPainter.ScrollerWidth; }
		}


		public override Size TabControlGetSpacing (TabControl tab) 
		{
			try {
				return ThemeElements.CurrentTheme.TabControlPainter.RowSpacing (tab);
			} catch {
				throw new Exception ("Invalid Appearance value: " + tab.Appearance);
			}
		}
		#endregion

		public override void DrawTabControl (Graphics dc, Rectangle area, TabControl tab)
		{
			ThemeElements.CurrentTheme.TabControlPainter.Draw (dc, area, tab);
		}

		public override Rectangle TabControlGetDisplayRectangle (TabControl tab)
		{
			return ThemeElements.CurrentTheme.TabControlPainter.GetDisplayRectangle (tab);
		}

		public override Rectangle TabControlGetPanelRect (TabControl tab)
		{
			return ThemeElements.CurrentTheme.TabControlPainter.GetTabPanelRect (tab);
		}

		#endregion

		#region TextBox
		public override void TextBoxBaseFillBackground (TextBoxBase textBoxBase, Graphics g, Rectangle clippingArea)
		{
			if (textBoxBase.backcolor_set || (textBoxBase.Enabled && !textBoxBase.ReadOnly)) {
				g.FillRectangle(ResPool.GetSolidBrush(textBoxBase.BackColor), clippingArea);
			} else {
				g.FillRectangle(ResPool.GetSolidBrush(ColorControl), clippingArea);
			}
		}

		public override bool TextBoxBaseHandleWmNcPaint (TextBoxBase textBoxBase, ref Message m)
		{
			return false;
		}

		public override bool TextBoxBaseShouldPaintBackground (TextBoxBase textBoxBase)
		{
			return true;
		}
		#endregion

		#region ToolTip
		public override void DrawToolTip(Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control)
		{
			ToolTipDrawBackground (dc, clip_rectangle, control);

			TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.HorizontalCenter;

			Color foreground = control.ForeColor;
			if (control.title.Length > 0) {
				Font bold_font = new Font (control.Font, control.Font.Style | FontStyle.Bold);
				TextRenderer.DrawTextInternal (dc, control.title, bold_font, control.title_rect,
						foreground, flags, false);
				bold_font.Dispose ();
			}

			if (control.icon != null)
				dc.DrawIcon (control.icon, control.icon_rect);

			TextRenderer.DrawTextInternal (dc, control.Text, control.Font, control.text_rect, foreground, flags, false);
		}

		protected virtual void ToolTipDrawBackground (Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control)
		{
			Brush back_brush = ResPool.GetSolidBrush (control.BackColor);
			dc.FillRectangle (back_brush, control.ClientRectangle);
			dc.DrawRectangle (SystemPens.WindowFrame, 0, 0, control.Width - 1, control.Height - 1);
		}

		public override Size ToolTipSize(ToolTip.ToolTipWindow tt, string text)
		{
			Size size = TextRenderer.MeasureTextInternal (text, tt.Font, false);
			size.Width += 4;
			size.Height += 3;
			Rectangle text_rect = new Rectangle (Point.Empty, size);
			text_rect.Inflate (-2, -1);
			tt.text_rect = text_rect;
			tt.icon_rect = tt.title_rect = Rectangle.Empty;

			Size title_size = Size.Empty;
			if (tt.title.Length > 0) {
				Font bold_font = new Font (tt.Font, tt.Font.Style | FontStyle.Bold);
				title_size = TextRenderer.MeasureTextInternal (tt.title, bold_font, false);
				bold_font.Dispose ();
			}

			Size icon_size = Size.Empty;
			if (tt.icon != null)
				icon_size = new Size (size.Height, size.Height);

			if (icon_size != Size.Empty || title_size != Size.Empty) {
				int padding = 8;
				int top_area_width = 0;
				int top_area_height = icon_size.Height > title_size.Height ? icon_size.Height : title_size.Height;
				Size text_size = size;
				Point location = new Point (padding, padding);

				if (icon_size != Size.Empty) {
					tt.icon_rect = new Rectangle (location, icon_size);
					top_area_width = icon_size.Width + padding;
				}

				if (title_size != Size.Empty) {
					Rectangle title_rect = new Rectangle (location, new Size (title_size.Width, top_area_height));
					if (icon_size != Size.Empty)
						title_rect.X += icon_size.Width + padding;

					tt.title_rect = title_rect;
					top_area_width += title_size.Width;
				}

				tt.text_rect = new Rectangle (new Point (location.X, location.Y + top_area_height + padding),
						text_size);

				size.Height += padding + top_area_height;
				if (top_area_width > size.Width)
					size.Width = top_area_width;

				// margins
				size.Width += padding * 2;
				size.Height += padding * 2;
			}

			return size;
		}
		
		public override bool ToolTipTransparentBackground {
			get {
				return false;
			}
 		}
		#endregion	// ToolTip

		#region BalloonWindow
		NotifyIcon.BalloonWindow balloon_window;
		
		public override void ShowBalloonWindow (IntPtr handle, int timeout, string title, string text, ToolTipIcon icon)
		{
			Control control = Control.FromHandle(handle);
			
			if (control == null)
				return;

			if (balloon_window != null) {
				balloon_window.Close ();
				balloon_window.Dispose ();
			}

			balloon_window = new NotifyIcon.BalloonWindow (handle);
			balloon_window.Title = title;
			balloon_window.Text = text;
			balloon_window.Icon = icon;
			balloon_window.Timeout = timeout;
			balloon_window.Show ();
		}

		public override void HideBalloonWindow (IntPtr handle)
		{
			if (balloon_window == null || balloon_window.OwnerHandle != handle)
				return;

			balloon_window.Close ();
			balloon_window.Dispose ();
			balloon_window = null;
		}

		private const int balloon_iconsize = 16;
		private const int balloon_bordersize = 8; 
		
		public override void DrawBalloonWindow (Graphics dc, Rectangle clip, NotifyIcon.BalloonWindow control) 
		{
			Brush solidbrush = ResPool.GetSolidBrush (this.ColorInfoText);
			Rectangle rect = control.ClientRectangle;
			int iconsize = (control.Icon == ToolTipIcon.None) ? 0 : balloon_iconsize;
			
			// Rectangle borders and background.
			dc.FillRectangle (ResPool.GetSolidBrush (ColorInfo), rect);
			dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame), 0, 0, rect.Width - 1, rect.Height - 1);

			// Icon
			Image image;
			switch (control.Icon) {
				case ToolTipIcon.Info: {
					image = ThemeEngine.Current.Images(UIIcon.MessageBoxInfo, balloon_iconsize);
					break;
				}

				case ToolTipIcon.Warning: {
					image = ThemeEngine.Current.Images(UIIcon.MessageBoxError, balloon_iconsize);
					break;
				}

				case ToolTipIcon.Error: {
					image = ThemeEngine.Current.Images(UIIcon.MessageBoxWarning, balloon_iconsize);
					break;
				}
				
				default: {
					image = null;
					break;
				}
			}

			if (control.Icon != ToolTipIcon.None)
				dc.DrawImage (image, new Rectangle (balloon_bordersize, balloon_bordersize, iconsize, iconsize));
			
			// Title
			Rectangle titlerect = new Rectangle (rect.X + balloon_bordersize + iconsize + (iconsize > 0 ? balloon_bordersize : 0), 
												rect.Y + balloon_bordersize, 
												rect.Width - ((3 * balloon_bordersize) + iconsize), 
												rect.Height - (2 * balloon_bordersize));
			
			Font titlefont = new Font (control.Font.FontFamily, control.Font.Size, control.Font.Style | FontStyle.Bold, control.Font.Unit);
			dc.DrawString (control.Title, titlefont, solidbrush, titlerect, control.Format);
			
			// Text
			Rectangle textrect = new Rectangle (rect.X + balloon_bordersize, 
												rect.Y + balloon_bordersize, 
												rect.Width - (2 * balloon_bordersize), 
												rect.Height - (2 * balloon_bordersize));

			StringFormat textformat = control.Format;
			textformat.LineAlignment = StringAlignment.Far;
			dc.DrawString (control.Text, control.Font, solidbrush, textrect, textformat);
		}

		public override Rectangle BalloonWindowRect (NotifyIcon.BalloonWindow control)
		{
			Rectangle deskrect = Screen.GetWorkingArea (control);
			SizeF maxsize = new SizeF (250, 200);

			SizeF titlesize = TextRenderer.MeasureString (control.Title, control.Font, maxsize, control.Format);
			SizeF textsize = TextRenderer.MeasureString (control.Text, control.Font, maxsize, control.Format);
			
			if (titlesize.Height < balloon_iconsize)
				titlesize.Height = balloon_iconsize;
			
			Rectangle rect = new Rectangle ();
			rect.Height = (int) (titlesize.Height + textsize.Height + (3 * balloon_bordersize));
			rect.Width = (int) ((titlesize.Width > textsize.Width) ? titlesize.Width : textsize.Width) + (2 * balloon_bordersize);
			rect.X = deskrect.Width - rect.Width - 2;
			rect.Y = deskrect.Height - rect.Height - 2;
			
			return rect;
		}
		#endregion	// BalloonWindow

		#region	TrackBar
		public override int TrackBarValueFromMousePosition (int x, int y, TrackBar tb)
		{
			int result = tb.Value;
			int value_pos = tb.Value;
			float pixels_betweenticks;
			Rectangle thumb_pos = Rectangle.Empty, thumb_area = Rectangle.Empty;
			Point channel_startpoint = Point.Empty, na_point = Point.Empty;
			
			GetTrackBarDrawingInfo (tb, out pixels_betweenticks, out thumb_area, out thumb_pos, out channel_startpoint, out na_point, out na_point);
			
			/* Convert thumb position from mouse position to value*/
			if (tb.Orientation == Orientation.Vertical) {
				value_pos = (int)Math.Round (((thumb_area.Bottom - y - (float)thumb_pos.Height / 2) / (float)pixels_betweenticks), 0);

				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
				else if (value_pos + tb.Minimum < tb.Minimum)
					value_pos = 0;

				result = value_pos + tb.Minimum;
			} else {
				value_pos = (int)Math.Round (((x - channel_startpoint.X - (float)thumb_pos.Width / 2) / (float) pixels_betweenticks), 0);

				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
				else if (value_pos + tb.Minimum < tb.Minimum)
					value_pos = 0;

				result = value_pos + tb.Minimum;
			}
			
			return result;
		}
		
		private void GetTrackBarDrawingInfo (TrackBar tb, out float pixels_betweenticks, out Rectangle thumb_area, out Rectangle thumb_pos, out Point channel_startpoint, out Point bottomtick_startpoint, out Point toptick_startpoint)
		{
			thumb_area = Rectangle.Empty;
			thumb_pos = Rectangle.Empty;
			
			if (tb.Orientation == Orientation.Vertical) {
				toptick_startpoint = new Point ();
				bottomtick_startpoint = new Point ();
				channel_startpoint = new Point ();
				float pixel_len;
				const int space_from_right = 8;
				const int space_from_left = 8;
				const int space_from_bottom = 11;
				Rectangle area = tb.ClientRectangle;

				switch (tb.TickStyle) {
				case TickStyle.BottomRight:
				case TickStyle.None:
					channel_startpoint.Y = 8;
					channel_startpoint.X = 9;
					bottomtick_startpoint.Y = 13;
					bottomtick_startpoint.X = 24;
					break;
				case TickStyle.TopLeft:
					channel_startpoint.Y = 8;
					channel_startpoint.X = 19;
					toptick_startpoint.Y = 13;
					toptick_startpoint.X = 8;
					break;
				case TickStyle.Both:
					channel_startpoint.Y = 8;
					channel_startpoint.X = 18;
					bottomtick_startpoint.Y = 13;
					bottomtick_startpoint.X = 32;
					toptick_startpoint.Y = 13;
					toptick_startpoint.X = 8;
					break;
				default:
					break;
				}

				thumb_area.X = area.X + channel_startpoint.X;
				thumb_area.Y = area.Y + channel_startpoint.Y;
				thumb_area.Height = area.Height - space_from_right - space_from_left;
				thumb_area.Width = TrackBarVerticalTrackWidth;

				pixel_len = thumb_area.Height - 11;
				if (tb.Maximum == tb.Minimum) {
					pixels_betweenticks = 0;
				} else {
					pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
				}

				thumb_pos.Y = thumb_area.Bottom - space_from_bottom - (int)(pixels_betweenticks * (float)(tb.Value - tb.Minimum));
			} else {	
				toptick_startpoint = new Point ();
				bottomtick_startpoint = new Point ();
				channel_startpoint = new Point ();
				float pixel_len;
				const int space_from_right = 8;
				const int space_from_left = 8;
				Rectangle area = tb.ClientRectangle;
							
				switch (tb.TickStyle) {
				case TickStyle.BottomRight:
				case TickStyle.None:
					channel_startpoint.X = 8;
					channel_startpoint.Y = 9;
					bottomtick_startpoint.X = 13;
					bottomtick_startpoint.Y = 24;				
					break;
				case TickStyle.TopLeft:
					channel_startpoint.X = 8;
					channel_startpoint.Y = 19;
					toptick_startpoint.X = 13;
					toptick_startpoint.Y = 8;
					break;
				case TickStyle.Both:
					channel_startpoint.X = 8;
					channel_startpoint.Y = 18;	
					bottomtick_startpoint.X = 13;
					bottomtick_startpoint.Y = 32;				
					toptick_startpoint.X = 13;
					toptick_startpoint.Y = 8;				
					break;
				default:
					break;
				}
							
				thumb_area.X = area.X + channel_startpoint.X;
				thumb_area.Y = area.Y + channel_startpoint.Y;
				thumb_area.Width = area.Width - space_from_right - space_from_left;
				thumb_area.Height = TrackBarHorizontalTrackHeight;

				pixel_len = thumb_area.Width - 11;
				if (tb.Maximum == tb.Minimum) {
					pixels_betweenticks = 0;
				} else {
					pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
				}

				thumb_pos.X = channel_startpoint.X + (int)(pixels_betweenticks * (float) (tb.Value - tb.Minimum));
			}

			thumb_pos.Size = TrackBarGetThumbSize (tb);
		}

		protected virtual Size TrackBarGetThumbSize (TrackBar trackBar)
		{
			return TrackBarGetThumbSize ();
		}

		public static Size TrackBarGetThumbSize ()
		{
			/* Draw thumb fixed 10x22 size */
			return new Size (10, 22);
		}

		public const int TrackBarVerticalTrackWidth = 4;

		public const int TrackBarHorizontalTrackHeight = 4;

		#region Ticks
		protected interface ITrackBarTickPainter
		{
			void Paint (float x1, float y1, float x2, float y2);
		}

		class TrackBarTickPainter : ITrackBarTickPainter
		{
			readonly Graphics g;
			readonly Pen pen;
			public TrackBarTickPainter (Graphics g, Pen pen)
			{
				this.g = g;
				this.pen = pen;
			}
			public void Paint (float x1, float y1, float x2, float y2)
			{
				g.DrawLine (pen, x1, y1, x2, y2);
			}
		}
		protected virtual ITrackBarTickPainter GetTrackBarTickPainter (Graphics g)
		{
			return new TrackBarTickPainter (g, ResPool.GetPen (pen_ticks_color));
		}
		#endregion

		#region DrawTrackBar_Vertical
		private void DrawTrackBar_Vertical (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area,  Brush br_thumb,
			float ticks, int value_pos, bool mouse_value) {			

			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			Rectangle area = tb.ClientRectangle;
			
			GetTrackBarDrawingInfo (tb, out pixels_betweenticks, out thumb_area, out thumb_pos, out channel_startpoint, out bottomtick_startpoint, out toptick_startpoint);

			#region Track
			TrackBarDrawVerticalTrack (dc, thumb_area, channel_startpoint, clip_rectangle);
			#endregion

			#region Thumb
			switch (tb.TickStyle) 	{
			case TickStyle.BottomRight:
			case TickStyle.None:
				thumb_pos.X = channel_startpoint.X - 8;
				TrackBarDrawVerticalThumbRight (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			case TickStyle.TopLeft:
				thumb_pos.X = channel_startpoint.X - 10;
				TrackBarDrawVerticalThumbLeft (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			default:
				thumb_pos.X = area.X + 10;
				TrackBarDrawVerticalThumb (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			}
			#endregion

			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			thumb_area.X = thumb_pos.X;
			thumb_area.Y = channel_startpoint.Y;
			thumb_area.Width = thumb_pos.Height;

			#region Ticks
			if (pixels_betweenticks <= 0)
				return;
			if (tb.TickStyle == TickStyle.None)
				return;
			Region outside = new Region (area);
			outside.Exclude (thumb_area);			
			
			if (outside.IsVisible (clip_rectangle)) {
				ITrackBarTickPainter tick_painter = TrackBarGetVerticalTickPainter (dc);

				if ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight) {
					float x = area.X + bottomtick_startpoint.X;
					for (float inc = 0; inc < pixel_len + 1; inc += pixels_betweenticks) 	{
						float y = area.Y + bottomtick_startpoint.Y + inc;
						tick_painter.Paint (
							x, y,
							x + (inc == 0 || inc + pixels_betweenticks >= pixel_len + 1 ? 3 : 2), y);
					}
				}

				if ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft) {
					float x = area.X + toptick_startpoint.X; 
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						float y = area.Y + toptick_startpoint.Y + inc;
						tick_painter.Paint (
							x - (inc == 0 || inc + pixels_betweenticks >= pixel_len + 1 ? 3 : 2), y,
							x, y);
					}			
				}
			}
			
			outside.Dispose ();
			#endregion
		}

		#region Track
		protected virtual void TrackBarDrawVerticalTrack (Graphics dc, Rectangle thumb_area, Point channel_startpoint, Rectangle clippingArea)
		{
			dc.FillRectangle (SystemBrushes.ControlDark, channel_startpoint.X, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (SystemBrushes.ControlDarkDark, channel_startpoint.X + 1, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (SystemBrushes.ControlLight, channel_startpoint.X + 3, channel_startpoint.Y,
				1, thumb_area.Height);
		}
		#endregion

		#region Thumb
		protected virtual void TrackBarDrawVerticalThumbRight (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 16, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X + 16, thumb_pos.Y, thumb_pos.X + 16 + 4, thumb_pos.Y + 4);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X + 15, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X + 16, thumb_pos.Y + 9, thumb_pos.X + 16 + 4, thumb_pos.Y + 9 - 4);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X + 16, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X + 16, thumb_pos.Y + 10, thumb_pos.X + 16 + 5, thumb_pos.Y + 10 - 5);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 16, 8);
			dc.FillRectangle (br_thumb, thumb_pos.X + 17, thumb_pos.Y + 2, 1, 6);
			dc.FillRectangle (br_thumb, thumb_pos.X + 18, thumb_pos.Y + 3, 1, 4);
			dc.FillRectangle (br_thumb, thumb_pos.X + 19, thumb_pos.Y + 4, 1, 2);
		}

		protected virtual void TrackBarDrawVerticalThumbLeft (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X + 4 + 16, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 4);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 9, thumb_pos.X + 4 + 16, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 9, thumb_pos.X, thumb_pos.Y + 5);
			dc.DrawLine (pen, thumb_pos.X + 19, thumb_pos.Y + 9, thumb_pos.X + 19, thumb_pos.Y + 1);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 10, thumb_pos.X + 4 + 16, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 10, thumb_pos.X - 1, thumb_pos.Y + 5);
			dc.DrawLine (pen, thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X + 20, thumb_pos.Y + 10);

			dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 15, 8);
			dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 1, 6);
			dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 1, 4);
			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 1, 2);
		}

		protected virtual void TrackBarDrawVerticalThumb (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 19, thumb_pos.Y);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X + 19, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 1, thumb_pos.X + 19, thumb_pos.Y + 8);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X + 20, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X + 20, thumb_pos.Y + 9);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 18, 8);
		}
		#endregion

		#region Ticks
		protected virtual ITrackBarTickPainter TrackBarGetVerticalTickPainter (Graphics g)
		{
			return GetTrackBarTickPainter (g);
		}
		#endregion
		#endregion

		#region DrawTrackBar_Horizontal
		/* 
			Horizontal trackbar 
		  
			Does not matter the size of the control, Win32 always draws:
				- Ticks starting from pixel 13, 8
				- Channel starting at pos 8, 19 and ends at Width - 8
				- Autosize makes always the control 45 pixels high
				- Ticks are draw at (channel.Witdh - 10) / (Maximum - Minimum)
				
		*/
		private void DrawTrackBar_Horizontal (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area, Brush br_thumb,
			float ticks, int value_pos, bool mouse_value) {			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			Rectangle area = tb.ClientRectangle;
			
			GetTrackBarDrawingInfo (tb , out pixels_betweenticks, out thumb_area, out thumb_pos, out channel_startpoint, out bottomtick_startpoint, out toptick_startpoint);

			#region Track
			TrackBarDrawHorizontalTrack (dc, thumb_area, channel_startpoint, clip_rectangle);
			#endregion

			#region Thumb
			switch (tb.TickStyle) {
			case TickStyle.BottomRight:
			case TickStyle.None:
				thumb_pos.Y = channel_startpoint.Y - 8;
				TrackBarDrawHorizontalThumbBottom (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			case TickStyle.TopLeft:
				thumb_pos.Y = channel_startpoint.Y - 10;
				TrackBarDrawHorizontalThumbTop (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			default:
				thumb_pos.Y = area.Y + 10;
				TrackBarDrawHorizontalThumb (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			}
			#endregion

			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ticks;

			thumb_area.Y = thumb_pos.Y;
			thumb_area.X = channel_startpoint.X;
			thumb_area.Height = thumb_pos.Height;
			#region Ticks
			if (pixels_betweenticks <= 0)
				return;
			if (tb.TickStyle == TickStyle.None)
				return;
			Region outside = new Region (area);
			outside.Exclude (thumb_area);

			if (outside.IsVisible (clip_rectangle)) {
				ITrackBarTickPainter tick_painter = TrackBarGetHorizontalTickPainter (dc);

				if ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight) {
					float y = area.Y + bottomtick_startpoint.Y;
					for (float inc = 0; inc < pixel_len + 1; inc += pixels_betweenticks) {					
						float x = area.X + bottomtick_startpoint.X + inc;
						tick_painter.Paint (
							x, y, 
							x, y + (inc == 0 || inc + pixels_betweenticks >= pixel_len + 1 ? 3 : 2));
					}
				}

				if ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft) {
					float y = area.Y + toptick_startpoint.Y;
					for (float inc = 0; inc < pixel_len + 1; inc += pixels_betweenticks) {					
						float x = area.X + toptick_startpoint.X + inc;
						tick_painter.Paint (
							x, y - (inc == 0 || (inc + pixels_betweenticks) >= pixel_len + 1 ? 3 : 2), 
							x, y);
					}			
				}
			}
			
			outside.Dispose ();
			#endregion
		}

		#region Track
		protected virtual void TrackBarDrawHorizontalTrack (Graphics dc, Rectangle thumb_area, Point channel_startpoint, Rectangle clippingArea)
		{
			dc.FillRectangle (SystemBrushes.ControlDark, channel_startpoint.X, channel_startpoint.Y,
				thumb_area.Width, 1);

			dc.FillRectangle (SystemBrushes.ControlDarkDark, channel_startpoint.X, channel_startpoint.Y + 1,
				thumb_area.Width, 1);

			dc.FillRectangle (SystemBrushes.ControlLight, channel_startpoint.X, channel_startpoint.Y + 3,
				thumb_area.Width, 1);
		}
		#endregion

		#region Thumb
		protected virtual void TrackBarDrawHorizontalThumbBottom (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 16);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 16, thumb_pos.X + 4, thumb_pos.Y + 16 + 4);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 15);
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 16, thumb_pos.X + 9 - 4, thumb_pos.Y + 16 + 4);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y + 16);
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 16, thumb_pos.X + 10 - 5, thumb_pos.Y + 16 + 5);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 16);
			dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 17, 6, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 18, 4, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 19, 2, 1);
		}

		protected virtual void TrackBarDrawHorizontalThumbTop (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X, thumb_pos.Y + 4 + 16);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X + 4, thumb_pos.Y);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 9, thumb_pos.Y + 4 + 16);
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 19, thumb_pos.X + 1, thumb_pos.Y + 19);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 10, thumb_pos.Y + 4 + 16);
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y - 1);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 10, thumb_pos.Y + 20);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 8, 15);
			dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 6, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 4, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 2, 1);
		}

		protected virtual void TrackBarDrawHorizontalThumb (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 9, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 19);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 19);
			dc.DrawLine (pen, thumb_pos.X + 1, thumb_pos.Y + 10, thumb_pos.X + 8, thumb_pos.Y + 19);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y + 20);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 9, thumb_pos.Y + 20);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 18);
		}
		#endregion

		#region Ticks
		protected virtual ITrackBarTickPainter TrackBarGetHorizontalTickPainter (Graphics g)
		{
			return GetTrackBarTickPainter (g);
		}
		#endregion
		#endregion

		public override void DrawTrackBar (Graphics dc, Rectangle clip_rectangle, TrackBar tb) 
		{
			Brush		br_thumb;
			int		value_pos;
			bool		mouse_value;
			float		ticks = (tb.Maximum - tb.Minimum) / tb.tickFrequency; /* N of ticks draw*/
			Rectangle	area;
			Rectangle	thumb_pos = tb.ThumbPos;
			Rectangle	thumb_area = tb.ThumbArea;
			
			if (tb.thumb_pressed) {
				value_pos = tb.thumb_mouseclick;
				mouse_value = true;
			} else {
				value_pos = tb.Value - tb.Minimum;
				mouse_value = false;
			}

			area = tb.ClientRectangle;

			if (!tb.Enabled) {
				br_thumb = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLightLight, ColorControlLight);
			} else if (tb.thumb_pressed == true) {
				br_thumb = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl);
			} else {
				br_thumb = SystemBrushes.Control;
			}

			
			/* Control Background */
			if (tb.BackColor.ToArgb () == DefaultControlBackColor.ToArgb ()) {
				dc.FillRectangle (SystemBrushes.Control, clip_rectangle);
			} else {
				dc.FillRectangle (ResPool.GetSolidBrush (tb.BackColor), clip_rectangle);
			}
			
			if (tb.Focused) {
				CPDrawFocusRectangle(dc, area, tb.ForeColor, tb.BackColor);
			}

			if (tb.Orientation == Orientation.Vertical) {
				DrawTrackBar_Vertical (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			
			} else {
				DrawTrackBar_Horizontal (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			}

			tb.ThumbPos = thumb_pos;
			tb.ThumbArea = thumb_area;
		}

		public override Size TrackBarDefaultSize {
			get {
				return new Size (104, 42);
			}
		}

		public override bool TrackBarHasHotThumbStyle {
			get {
				return false;
			}
		}
		#endregion	// TrackBar

		#region UpDownBase
		public override void UpDownBaseDrawButton (Graphics g, Rectangle bounds, bool top, VisualStyles.PushButtonState state)
		{
			ControlPaint.DrawScrollButton (g, bounds, top ? ScrollButton.Up : ScrollButton.Down, state == VisualStyles.PushButtonState.Pressed ? ButtonState.Pushed : ButtonState.Normal);
		}

		public override bool UpDownBaseHasHotButtonStyle {
			get {
				return false;
			}
		}
		#endregion

		#region	VScrollBar
		public override Size VScrollBarDefaultSize {
			get {
				return new Size (this.ScrollBarButtonSize, 80);
			}
		}
		#endregion	// VScrollBar

		#region TreeView
		public override Size TreeViewDefaultSize {
			get {
				return new Size (121, 97);
			}
		}

		public override void TreeViewDrawNodePlusMinus (TreeView treeView, TreeNode node, Graphics dc, int x, int middle)
		{
			int height = treeView.ActualItemHeight - 2;
			dc.FillRectangle (ResPool.GetSolidBrush (treeView.BackColor), (x + 4) - (height / 2), node.GetY() + 1, height, height);
			
			dc.DrawRectangle (SystemPens.ControlDarkDark, x, middle - 4, 8, 8);

			if (node.IsExpanded) {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
			} else {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
				dc.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
			}
		}
		#endregion

		#region Managed window
		public override int ManagedWindowTitleBarHeight (InternalWindowManager wm)
		{
			if (wm.IsToolWindow && !wm.IsMinimized)
				return SystemInformation.ToolWindowCaptionHeight;
			if (wm.Form.FormBorderStyle == FormBorderStyle.None)
				return 0;
			return SystemInformation.CaptionHeight;
		}

		public override int ManagedWindowBorderWidth (InternalWindowManager wm)
		{
			if ((wm.IsToolWindow && wm.form.FormBorderStyle == FormBorderStyle.FixedToolWindow) ||
				wm.IsMinimized)
				return 3;
			else
				return 4;
		}

		public override int ManagedWindowIconWidth (InternalWindowManager wm)
		{
			return ManagedWindowTitleBarHeight (wm) - 5;
		}

		public override void ManagedWindowSetButtonLocations (InternalWindowManager wm)
		{
			TitleButtons buttons = wm.TitleButtons;
			Form form = wm.form;
			
			buttons.HelpButton.Visible = form.HelpButton;
			
			foreach (TitleButton button in buttons) {
				button.Visible = false;
			}
			
			switch (form.FormBorderStyle) {
			case FormBorderStyle.None:
				if (form.WindowState != FormWindowState.Normal)
					goto case FormBorderStyle.Sizable;
				break;
			case FormBorderStyle.FixedToolWindow:
			case FormBorderStyle.SizableToolWindow:
				buttons.CloseButton.Visible = true;
				if (form.WindowState != FormWindowState.Normal)
					goto case FormBorderStyle.Sizable;
				break;
			case FormBorderStyle.FixedSingle:
			case FormBorderStyle.Fixed3D:
			case FormBorderStyle.FixedDialog:
			case FormBorderStyle.Sizable:
				switch (form.WindowState) {
					case FormWindowState.Normal:
						buttons.MinimizeButton.Visible = true;
						buttons.MaximizeButton.Visible = true;
						buttons.RestoreButton.Visible = false;
						break;
					case FormWindowState.Maximized:
						buttons.MinimizeButton.Visible = true;
						buttons.MaximizeButton.Visible = false;
						buttons.RestoreButton.Visible = true;
						break;
					case FormWindowState.Minimized:
						buttons.MinimizeButton.Visible = false;
						buttons.MaximizeButton.Visible = true;
						buttons.RestoreButton.Visible = true;
						break;
				}
				buttons.CloseButton.Visible = true;
				break;
			}

			// Respect MinimizeBox/MaximizeBox
			if (form.MinimizeBox == false && form.MaximizeBox == false) {
				buttons.MinimizeButton.Visible = false;
				buttons.MaximizeButton.Visible = false;
			} else if (form.MinimizeBox == false)
				buttons.MinimizeButton.State = ButtonState.Inactive;
			else if (form.MaximizeBox == false)
				buttons.MaximizeButton.State = ButtonState.Inactive;

			int bw = ManagedWindowBorderWidth (wm);
			Size btsize = ManagedWindowButtonSize (wm);
			int btw = btsize.Width;
			int bth = btsize.Height;
			int top = bw + 2;
			int left = form.Width - bw - btw - ManagedWindowSpacingAfterLastTitleButton;
			
			if ((!wm.IsToolWindow || wm.IsMinimized) && wm.HasBorders) {
				buttons.CloseButton.Rectangle = new Rectangle (left, top, btw, bth);
				left -= 2 + btw;
				
				if (buttons.MaximizeButton.Visible) {
					buttons.MaximizeButton.Rectangle = new Rectangle (left, top, btw, bth);
					left -= 2 + btw;
				} 
				if (buttons.RestoreButton.Visible) {
					buttons.RestoreButton.Rectangle = new Rectangle (left, top, btw, bth);
					left -= 2 + btw;
				}

				buttons.MinimizeButton.Rectangle = new Rectangle (left, top, btw, bth);
				left -= 2 + btw;
			} else if (wm.IsToolWindow) {
				buttons.CloseButton.Rectangle = new Rectangle (left, top, btw, bth);
				left -= 2 + btw;
			}
		}

		protected virtual Rectangle ManagedWindowDrawTitleBarAndBorders (Graphics dc, Rectangle clip, InternalWindowManager wm)
		{
			Form form = wm.Form;
			int tbheight = ManagedWindowTitleBarHeight (wm);
			int bdwidth = ManagedWindowBorderWidth (wm);
			Color titlebar_color = Color.FromArgb (255, 10, 36, 106);
			Color titlebar_color2 = Color.FromArgb (255, 166, 202, 240);
			Color color = ThemeEngine.Current.ColorControlDark;
			Color color2 = Color.FromArgb (255, 192, 192, 192);

			Pen pen = ResPool.GetPen (ColorControl);
			Rectangle borders = new Rectangle (0, 0, form.Width, form.Height);
			ControlPaint.DrawBorder3D (dc, borders, Border3DStyle.Raised);
			// The 3d border is only 2 pixels wide, so we draw the innermost pixels ourselves
			borders = new Rectangle (2, 2, form.Width - 5, form.Height - 5);
			for (int i = 2; i < bdwidth; i++) {
				dc.DrawRectangle (pen, borders);
				borders.Inflate (-1, -1);
			}				


			bool draw_titlebar_enabled = false;
			if (wm.Form.Parent != null && wm.Form.Parent is Form) {
				draw_titlebar_enabled = false;
			} else if (wm.IsActive && !wm.IsMaximized) {
				draw_titlebar_enabled = true;
			}
			if (draw_titlebar_enabled) {
				color = titlebar_color;
				color2 = titlebar_color2;
			}

			Rectangle tb = new Rectangle (bdwidth, bdwidth, form.Width - (bdwidth * 2), tbheight - 1);

			// HACK: For now always draw the titlebar until we get updates better
			if (tb.Width > 0 && tb.Height > 0) {
				using (System.Drawing.Drawing2D.LinearGradientBrush gradient = new LinearGradientBrush (tb, color, color2, LinearGradientMode.Horizontal))
				{
					dc.FillRectangle (gradient, tb);
				}	
			}
			
			if (!wm.IsMinimized)
				// Draw the line just beneath the title bar
				dc.DrawLine (ResPool.GetPen (SystemColors.Control), bdwidth,
						tbheight + bdwidth - 1, form.Width - bdwidth - 1,
						tbheight + bdwidth - 1);
			return tb;
		}

		public override void DrawManagedWindowDecorations (Graphics dc, Rectangle clip, InternalWindowManager wm)
		{
#if debug
			Console.WriteLine (DateTime.Now.ToLongTimeString () + " DrawManagedWindowDecorations");
			dc.FillRectangle (Brushes.Black, clip);
#endif
			Rectangle tb = ManagedWindowDrawTitleBarAndBorders (dc, clip, wm);

			Form form = wm.Form;
			if (wm.ShowIcon) {
				Rectangle icon = ManagedWindowGetTitleBarIconArea (wm);
				if (icon.IntersectsWith (clip))
					dc.DrawIcon (form.Icon, icon);
				const int SpacingBetweenIconAndCaption = 2;
				tb.Width -= icon.Right + SpacingBetweenIconAndCaption - tb.X ;
				tb.X = icon.Right + SpacingBetweenIconAndCaption;
			}
			
			foreach (TitleButton button in wm.TitleButtons.AllButtons) {
				tb.Width -= Math.Max (0, tb.Right - DrawTitleButton (dc, button, clip, form));
			}
			const int SpacingBetweenCaptionAndLeftMostButton = 3;
			tb.Width -= SpacingBetweenCaptionAndLeftMostButton;

			string window_caption = form.Text;
			window_caption = window_caption.Replace (Environment.NewLine, string.Empty);

			if (window_caption != null && window_caption != string.Empty) {
				StringFormat format = new StringFormat ();
				format.FormatFlags = StringFormatFlags.NoWrap;
				format.Trimming = StringTrimming.EllipsisCharacter;
				format.LineAlignment = StringAlignment.Center;

				if (tb.IntersectsWith (clip))
					dc.DrawString (window_caption, WindowBorderFont,
						ThemeEngine.Current.ResPool.GetSolidBrush (Color.White),
						tb, format);
			}
		}

		public override Size ManagedWindowButtonSize (InternalWindowManager wm)
		{
			int height = ManagedWindowTitleBarHeight (wm);
			if (!wm.IsMaximized && !wm.IsMinimized) {
				if (wm.IsToolWindow)
					return new Size (SystemInformation.ToolWindowCaptionButtonSize.Width - 2,
							height - 5);
				if (wm.Form.FormBorderStyle == FormBorderStyle.None)
					return Size.Empty;
			} else
				height = SystemInformation.CaptionHeight;

			return new Size (SystemInformation.CaptionButtonSize.Width - 2,
					height - 5);
		}

		private int DrawTitleButton (Graphics dc, TitleButton button, Rectangle clip, Form form)
		{
			if (!button.Visible) {
				return int.MaxValue;
			}
			
			if (button.Rectangle.IntersectsWith (clip)) {
				ManagedWindowDrawTitleButton (dc, button, clip, form);
			}
			return button.Rectangle.Left;
		}

		protected virtual void ManagedWindowDrawTitleButton (Graphics dc, TitleButton button, Rectangle clip, Form form)
		{
			dc.FillRectangle (SystemBrushes.Control, button.Rectangle);

			ControlPaint.DrawCaptionButton (dc, button.Rectangle,
					button.Caption, button.State);
		}

		public override Rectangle ManagedWindowGetTitleBarIconArea (InternalWindowManager wm)
		{
			int bw = ManagedWindowBorderWidth (wm);
			return new Rectangle (bw + 3, bw + 2, wm.IconWidth, wm.IconWidth);
		}

		public override Size ManagedWindowGetMenuButtonSize (InternalWindowManager wm)
		{
			Size result = SystemInformation.MenuButtonSize;
			result.Width -= 2;
			result.Height -= 4;
			return result;
		}

		public override bool ManagedWindowTitleButtonHasHotElementStyle (TitleButton button, Form form)
		{
			return false;
		}

		public override void ManagedWindowDrawMenuButton (Graphics dc, TitleButton button, Rectangle clip, InternalWindowManager wm)
		{
			dc.FillRectangle (SystemBrushes.Control, button.Rectangle);
			ControlPaint.DrawCaptionButton (dc, button.Rectangle,
					button.Caption, button.State);
		}

		public override void ManagedWindowOnSizeInitializedOrChanged (Form form)
		{
		}
		#endregion

		#region ControlPaint
		public override void CPDrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle) {
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public override void CPDrawBorder (Graphics graphics, RectangleF bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle) {
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
			CPDrawBorder3D(graphics, rectangle, style, sides, SystemColors.WindowFrame);
		}

		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color)
		{
			Rectangle rect = new Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			bool is_ColorControl = control_color.ToArgb () == ColorControl.ToArgb () ? true : false;
			
			if ((style & Border3DStyle.Adjust) != 0) {
				rect.Y -= 2;
				rect.X -= 2;
				rect.Width += 4;
				rect.Height += 4;
			}
			
			Pen pen = ResPool.GetPen(control_color);

			if ((sides & Border3DSide.Middle) != 0) {
				Brush brush = is_ColorControl ? SystemBrushes.Control : ResPool.GetSolidBrush (control_color);
				graphics.FillRectangle (brush, rect);
			}
			
			if ((sides & Border3DSide.Left) != 0) {
				graphics.DrawLine(pen, rect.Left, rect.Bottom - 2, rect.Left, rect.Top);
			}
			
			if ((sides & Border3DSide.Top) != 0) {
				graphics.DrawLine(pen, rect.Left, rect.Top, rect.Right - 2, rect.Top);
			}
			
			if ((sides & Border3DSide.Right) != 0) {
				graphics.DrawLine(pen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
			}
			
			if ((sides & Border3DSide.Bottom) != 0) {
				graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
			}
		}

		public override void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			CPDrawButtonInternal (dc, rectangle, state, SystemPens.ControlDarkDark, SystemPens.ControlDark, SystemPens.ControlLightLight);
		}

		private void CPDrawButtonInternal (Graphics dc, Rectangle rectangle, ButtonState state, Pen DarkPen, Pen NormalPen, Pen LightPen)
		{
			dc.FillRectangle (SystemBrushes.Control, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);
			
			if ((state & ButtonState.All) == ButtonState.All || ((state & ButtonState.Checked) == ButtonState.Checked && (state & ButtonState.Flat) == ButtonState.Flat)) {
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl), rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4);
				
				dc.DrawRectangle (SystemPens.ControlDark, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
			} else
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				dc.DrawRectangle (SystemPens.ControlDark, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
			} else
			if ((state & ButtonState.Checked) == ButtonState.Checked) {
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl), rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4);
				
				Pen pen = DarkPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				
				pen = NormalPen;
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 3);
				dc.DrawLine (pen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
				
				pen = LightPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
				dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1);
			} else
			if (((state & ButtonState.Pushed) == ButtonState.Pushed) && ((state & ButtonState.Normal) == ButtonState.Normal)) {
				Pen pen = DarkPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				
				pen = NormalPen;
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 3);
				dc.DrawLine (pen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
				
				pen = LightPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
				dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1);
			} else
			if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Normal) == ButtonState.Normal)) {
				Pen pen = LightPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
				
				pen = NormalPen;
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 2);
				dc.DrawLine (pen, rectangle.Right - 2, rectangle.Y + 1, rectangle.Right - 2, rectangle.Bottom - 3);
				
				pen = DarkPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 2);
			}
		}


		public override void CPDrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state) {
			Rectangle	captionRect;
			int			lineWidth;

			CPDrawButtonInternal (graphics, rectangle, state, SystemPens.ControlDarkDark, SystemPens.ControlDark, SystemPens.ControlLightLight);

			if (rectangle.Width<rectangle.Height) {
				captionRect=new Rectangle(rectangle.X+1, rectangle.Y+rectangle.Height/2-rectangle.Width/2+1, rectangle.Width-4, rectangle.Width-4);
			} else {
				captionRect=new Rectangle(rectangle.X+rectangle.Width/2-rectangle.Height/2+1, rectangle.Y+1, rectangle.Height-4, rectangle.Height-4);
			}

			if ((state & ButtonState.Pushed)!=0) {
				captionRect=new Rectangle(rectangle.X+2, rectangle.Y+2, rectangle.Width-3, rectangle.Height-3);
			}

			/* Make sure we've got at least a line width of 1 */
			lineWidth=Math.Max(1, captionRect.Width/7);

			switch(button) {
			case CaptionButton.Close: {
				Pen	pen;

				if ((state & ButtonState.Inactive)!=0) {
					pen = ResPool.GetSizedPen (ColorControlLight, lineWidth);
					DrawCaptionHelper(graphics, ColorControlLight, pen, lineWidth, 1, captionRect, button);

					pen = ResPool.GetSizedPen (ColorControlDark, lineWidth);
					DrawCaptionHelper(graphics, ColorControlDark, pen, lineWidth, 0, captionRect, button);
					return;
				} else {
					pen = ResPool.GetSizedPen (ColorControlText, lineWidth);
					DrawCaptionHelper(graphics, ColorControlText, pen, lineWidth, 0, captionRect, button);
					return;
				}
			}

			case CaptionButton.Help:
			case CaptionButton.Maximize:
			case CaptionButton.Minimize:
			case CaptionButton.Restore: {
				if ((state & ButtonState.Inactive)!=0) {
					DrawCaptionHelper(graphics, ColorControlLight, SystemPens.ControlLightLight, lineWidth, 1, captionRect, button);

					DrawCaptionHelper(graphics, ColorControlDark, SystemPens.ControlDark, lineWidth, 0, captionRect, button);
					return;
				} else {
					DrawCaptionHelper(graphics, ColorControlText, SystemPens.ControlText, lineWidth, 0, captionRect, button);
					return;
				}
			}
			}
		}

		class LocalGraphicsContext : IDisposable
		{
			NSGraphicsContext savedContext;
			CGContext cgContext;

			public LocalGraphicsContext(Graphics dc)
			{
				savedContext = NSGraphicsContext.CurrentContext;
				cgContext = dc.ToCGContext();
				NSGraphicsContext.CurrentContext = NSGraphicsContext.FromGraphicsPort(cgContext, true);
				cgContext.SaveState();
			}

			public void Dispose()
			{
				cgContext.RestoreState();
				if (savedContext != null)
					NSGraphicsContext.CurrentContext = savedContext;
			}
		}

		class NSFlippedView : NSView
		{
			public NSFlippedView(CGRect frame)
				: base(frame)
			{
			}

			public override bool IsFlipped
			{
				get
				{
					return true;
				}
			}
		}

		public override void CPDrawCheckBox (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if ((state & ButtonState.Flat) != 0)
				CPDrawCheckBoxInternal(dc, rectangle, state, false /* mixed */);
			else
				DrawCheckBoxNative(dc, rectangle, state);
		}
			
		internal virtual void DrawCheckBoxNative(Graphics dc, Rectangle rectangle, ButtonState state, bool mixed = false)
		{
			using (var cell = new NSButtonCell()) {
				cell.SetButtonType(NSButtonType.Switch);
				cell.AttributedTitle = new NSAttributedString();
				cell.Alignment = NSTextAlignment.Left;
				cell.Highlighted = (state & ButtonState.Pushed) != 0;
				cell.Bezeled = false;
				cell.Bordered = false;
				cell.SetCellAttribute(NSCellAttribute.CellAllowsMixedState, 1);
				cell.State = mixed ? NSCellStateValue.Mixed : (state & ButtonState.Checked) != 0 ? NSCellStateValue.On : NSCellStateValue.Off;
				cell.Enabled = (state & ButtonState.Inactive) == 0;

				var frame = rectangle.ToCGRect();
				var bounds = new CGRect(CGPoint.Empty, cell.CellSize);
				var imgRect = cell.ImageRectForBounds(bounds);
				frame = frame.Move(bounds.X - imgRect.X - 1, bounds.Y - imgRect.Y);

				using (var view = new NSFlippedView(frame))
				using (new LocalGraphicsContext(dc))
					cell.DrawWithFrame(frame, view);
			}
		}

		private void CPDrawCheckBoxInternal (Graphics dc, Rectangle rectangle, ButtonState state, bool mixed)
		{
			Pen check_pen = (mixed) ? Pens.Gray : Pens.Black;
			
			Rectangle cb_rect = new Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			
			if ((state & ButtonState.All) == ButtonState.All) {
				cb_rect.X += 1;
				cb_rect.Y += 1;
				cb_rect.Width -= 1;
				cb_rect.Height -= 1;
				
				dc.FillRectangle (SystemBrushes.Control, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				dc.DrawRectangle (SystemPens.ControlDark, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				
				check_pen = SystemPens.ControlDark;
			} else
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				cb_rect.X += 1;
				cb_rect.Y += 1;
				cb_rect.Width -= 1;
				cb_rect.Height -= 1;
				
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					dc.FillRectangle (SystemBrushes.ControlLight, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				else
					dc.FillRectangle (Brushes.White, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				dc.DrawRectangle (SystemPens.ControlDark, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
			} else {
				cb_rect.X += 1;
				cb_rect.Y += 1;
				cb_rect.Width -= 1;
				cb_rect.Height -= 1;
				
				int check_box_visible_size = (cb_rect.Height > cb_rect.Width) ? cb_rect.Width : cb_rect.Height;
				
				int x_pos = Math.Max (0, cb_rect.X + (cb_rect.Width / 2) - check_box_visible_size / 2);
				int y_pos = Math.Max (0, cb_rect.Y + (cb_rect.Height / 2) - check_box_visible_size / 2);
				
				Rectangle rect = new Rectangle (x_pos, y_pos, check_box_visible_size, check_box_visible_size);
				
				if (((state & ButtonState.Pushed) == ButtonState.Pushed) || ((state & ButtonState.Inactive) == ButtonState.Inactive)) {
					dc.FillRectangle (SystemBrushes.Control, rect.X + 2, rect.Y + 2, rect.Width - 3, rect.Height - 3);
				} else
					dc.FillRectangle (SystemBrushes.ControlLightLight, rect.X + 2, rect.Y + 2, rect.Width - 3, rect.Height - 3);
				
				Pen pen = SystemPens.ControlDark;
				dc.DrawLine (pen, rect.X, rect.Y, rect.X, rect.Bottom - 1);
				dc.DrawLine (pen, rect.X + 1, rect.Y, rect.Right - 1, rect.Y);
				
				pen = SystemPens.ControlDarkDark;
				dc.DrawLine (pen, rect.X + 1, rect.Y + 1, rect.X + 1, rect.Bottom - 2);
				dc.DrawLine (pen, rect.X + 2, rect.Y + 1, rect.Right - 2, rect.Y + 1);
				
				pen = SystemPens.ControlLightLight;
				dc.DrawLine (pen, rect.Right, rect.Y, rect.Right, rect.Bottom);
				dc.DrawLine (pen, rect.X, rect.Bottom, rect.Right, rect.Bottom);

				pen = SystemPens.Control;
				dc.DrawLine (pen, rect.X + 1, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				dc.DrawLine (pen, rect.Right - 1, rect.Y + 1, rect.Right - 1, rect.Bottom - 1);
				
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					check_pen = SystemPens.ControlDark;
			}
			
			if ((state & ButtonState.Checked) == ButtonState.Checked) {
				int check_size = (cb_rect.Height > cb_rect.Width) ? cb_rect.Width / 2: cb_rect.Height / 2;
				
				if (check_size < 7) {
					float lineWidth = Math.Max (3, check_size / 3);
					float scale = Math.Max (1, check_size / 9);
					
					RectangleF rect = new RectangleF (cb_rect.X + (cb_rect.Width - check_size) / 2, cb_rect.Y + (cb_rect.Height - check_size) / 2, check_size, check_size);

					float top = rect.Top + lineWidth / 2;
					for (int i = 0; i < lineWidth; i++) {
						dc.DrawLine (check_pen, rect.Left, top + i, rect.Left + 2 * scale, top + 2 * scale + i);
						dc.DrawLine(check_pen, rect.Left + 2 * scale, top + 2 * scale + i, rect.Right - 1, top - 2 * scale + i);
					}
				} else {
					int lineWidth = Math.Max (3, check_size / 3) + 1;
					
					int x_half = cb_rect.Width / 2;
					int y_half = cb_rect.Height / 2;
					
					Rectangle rect = new Rectangle (cb_rect.X + x_half - (check_size / 2) - 1, cb_rect.Y + y_half - (check_size / 2), check_size, check_size);
					
					int gradient_left = check_size / 3;
					int gradient_right = check_size - gradient_left - 1;
					
					for (int i = 0; i < lineWidth; i++) {
						dc.DrawLine (check_pen, rect.X, rect.Bottom - 1 - gradient_left - i, rect.X + gradient_left, rect.Bottom - 1 - i);
						dc.DrawLine (check_pen, rect.X + gradient_left, rect.Bottom - 1 - i, rect.Right - 1, rect.Bottom - i  - 1 - gradient_right);
					}
				}
			}
		}

		public override void CPDrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			if ((state & ButtonState.Checked)!=0) {
				graphics.FillRectangle(ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLightLight, ColorControlLight),rectangle);				
			}

			if ((state & ButtonState.Flat)!=0) {
				ControlPaint.DrawBorder(graphics, rectangle, ColorControlDark, ButtonBorderStyle.Solid);
			} else {
				if ((state & (ButtonState.Pushed | ButtonState.Checked))!=0) {
					// this needs to render like a pushed button - jba
					// CPDrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
					Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
					graphics.DrawRectangle (SystemPens.ControlDark, trace_rectangle);
				} else {
					CPDrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
				}
			}

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left, centerY);
			P2=new Point(rect.Right, centerY);
			P3=new Point(centerX, rect.Bottom);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;
			
			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				/* Move away from the shadow */
				arrow[0].X += 1;	arrow[0].Y += 1;
				arrow[1].X += 1;	arrow[1].Y += 1;
				arrow[2].X += 1;	arrow[2].Y += 1;
				
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;

				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}


		public override void CPDrawContainerGrabHandle (Graphics graphics, Rectangle bounds)
		{
			Pen			pen	= Pens.Black;
			Rectangle	rect	= new Rectangle (bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);	// Dunno why, but MS does it that way, too
			int			X;
			int			Y;
			
			graphics.FillRectangle (SystemBrushes.ControlLightLight, rect);
			graphics.DrawRectangle (pen, rect);
			
			X = rect.X + rect.Width / 2;
			Y = rect.Y + rect.Height / 2;
			
			/* Draw the cross */
			graphics.DrawLine (pen, X, rect.Y + 2, X, rect.Bottom - 2);
			graphics.DrawLine (pen, rect.X + 2, Y, rect.Right - 2, Y);
			
			/* Draw 'arrows' for vertical lines */
			graphics.DrawLine (pen, X - 1, rect.Y + 3, X + 1, rect.Y + 3);
			graphics.DrawLine (pen, X - 1, rect.Bottom - 3, X + 1, rect.Bottom - 3);
			
			/* Draw 'arrows' for horizontal lines */
			graphics.DrawLine (pen, rect.X + 3, Y - 1, rect.X + 3, Y + 1);
			graphics.DrawLine (pen, rect.Right - 3, Y - 1, rect.Right - 3, Y + 1);
		}

		public virtual void DrawFlatStyleFocusRectangle (Graphics graphics, Rectangle rectangle, ButtonBase button, Color foreColor, Color backColor) {
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			
			Color outerColor = foreColor;
			// adjust focus color according to the flatstyle
			if (button.FlatStyle == FlatStyle.Popup && !button.is_pressed) {
				outerColor = (backColor.ToArgb () == ColorControl.ToArgb ()) ? ControlPaint.Dark(ColorControl) : ColorControlText;				
			}
			
			// draw the outer rectangle
			graphics.DrawRectangle (ResPool.GetPen (outerColor), trace_rectangle);			
			
			// draw the inner rectangle						
			if (button.FlatStyle == FlatStyle.Popup) {
				DrawInnerFocusRectangle (graphics, Rectangle.Inflate (rectangle, -4, -4), backColor);
			} else {
				// draw a flat inner rectangle
				Pen pen = ResPool.GetPen (ControlPaint.LightLight (backColor));
				graphics.DrawRectangle(pen, Rectangle.Inflate (trace_rectangle, -4, -4));				
			}
		}
		
		public virtual void DrawInnerFocusRectangle(Graphics graphics, Rectangle rectangle, Color backColor)
		{	
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			
//#if NotUntilCairoIsFixed
			Color colorBackInverted = Color.FromArgb (255 - backColor.R, 255 - backColor.G, 255 - backColor.B);
			DashStyle oldStyle; // used for caching old penstyle
			Pen pen = ResPool.GetPen (colorBackInverted);

			oldStyle = pen.DashStyle; 
			pen.DashStyle = DashStyle.Dot;

			graphics.DrawRectangle (pen, trace_rectangle);
			pen.DashStyle = oldStyle;
//#else
//			CPDrawFocusRectangle(graphics, trace_rectangle, Color.Wheat, backColor);
//#endif
		}
				

		public override void CPDrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) 
		{
			// Do nothing, we make use of native focus ring.
		}
		
		public override void CPDrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled)
		{
			Brush	sb;
			Pen pen;
			
			if (primary == true) {
				pen = Pens.Black;
				if (enabled == true) {
					sb = Brushes.White;
				} else {
					sb = SystemBrushes.Control;
				}
			} else {
				pen = Pens.White;
				if (enabled == true) {
					sb = Brushes.Black;
				} else {
					sb = SystemBrushes.Control;
				}
			}
			graphics.FillRectangle (sb, rectangle);
			graphics.DrawRectangle (pen, rectangle);			
		}


		public override void CPDrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor) {
			Color	foreColor;
			int	h;
			int	b;
			int	s;

			ControlPaint.Color2HBS(backColor, out h, out b, out s);
			
			if (b>127) {
				foreColor=Color.Black;
			} else {
				foreColor=Color.White;
			}

			// still not perfect. it seems that ms calculates the position of the first dot or line

			using (Pen pen = new Pen (foreColor)) {
				pen.DashPattern = new float [] {1.0f, pixelsBetweenDots.Width - 1};
				
				for (int y = area.Top; y < area.Bottom; y += pixelsBetweenDots.Height)
					graphics.DrawLine (pen, area.X, y, area.Right - 1, y);
			}
		}

		public override void CPDrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background) {
			/*
				Microsoft seems to ignore the background and simply make
				the image grayscale. At least when having > 256 colors on
				the display.
			*/
			
			if (imagedisabled_attributes == null) {				
				imagedisabled_attributes = new ImageAttributes ();
				ColorMatrix colorMatrix=new ColorMatrix(new float[][] {
					  // This table would create a perfect grayscale image, based on luminance
					  //				new float[]{0.3f,0.3f,0.3f,0,0},
					  //				new float[]{0.59f,0.59f,0.59f,0,0},
					  //				new float[]{0.11f,0.11f,0.11f,0,0},
					  //				new float[]{0,0,0,1,0,0},
					  //				new float[]{0,0,0,0,1,0},
					  //				new float[]{0,0,0,0,0,1}
		
					  // This table generates a image that is grayscaled and then
					  // brightened up. Seems to match MS close enough.
					  new float[]{0.2f,0.2f,0.2f,0,0},
					  new float[]{0.41f,0.41f,0.41f,0,0},
					  new float[]{0.11f,0.11f,0.11f,0,0},
					  new float[]{0.15f,0.15f,0.15f,1,0,0},
					  new float[]{0.15f,0.15f,0.15f,0,1,0},
					  new float[]{0.15f,0.15f,0.15f,0,0,1}
				  });
				  
				 imagedisabled_attributes.SetColorMatrix (colorMatrix);
			}
			
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imagedisabled_attributes);
			
		}


		public override void CPDrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary) {
			Pen	penBorder;
			Pen	penInside;

			if (primary) {
				penBorder = ResPool.GetSizedPen (Color.White, 2);
				penInside = ResPool.GetPen (Color.Black);
			} else {
				penBorder = ResPool.GetSizedPen (Color.Black, 2);
				penInside = ResPool.GetPen (Color.White);
			}
			penBorder.Alignment=PenAlignment.Inset;
			penInside.Alignment=PenAlignment.Inset;

			graphics.DrawRectangle(penBorder, rectangle);
			graphics.DrawRectangle(penInside, rectangle.X+2, rectangle.Y+2, rectangle.Width-5, rectangle.Height-5);
		}


		public override void CPDrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph, Color color, Color backColor) {
			Rectangle	rect;
			int			lineWidth;

			if (backColor != Color.Empty)
				graphics.FillRectangle (ResPool.GetSolidBrush (backColor), rectangle);
				
			Brush brush = ResPool.GetSolidBrush (color);

			switch(glyph) {
			case MenuGlyph.Arrow: {
				float height = rectangle.Height * 0.7f;
				float width  = height / 2.0f;
				
				PointF ddCenter = new PointF (rectangle.X + ((rectangle.Width-width) / 2.0f), rectangle.Y + (rectangle.Height / 2.0f));

				PointF [] vertices = new PointF [3];
				vertices [0].X = ddCenter.X;
				vertices [0].Y = ddCenter.Y - (height / 2.0f);
				vertices [1].X = ddCenter.X;
				vertices [1].Y = ddCenter.Y + (height / 2.0f);
				vertices [2].X = ddCenter.X + width + 0.1f;
				vertices [2].Y = ddCenter.Y;
				
				graphics.FillPolygon (brush, vertices);

				return;
			}

			case MenuGlyph.Bullet: {
				
				lineWidth=Math.Max(2, rectangle.Width/3);
				rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);
				
				graphics.FillEllipse(brush, rect);
				
				return;
			}

			case MenuGlyph.Checkmark: {
				
				Pen pen = ResPool.GetPen (color);
				lineWidth = Math.Max (2, rectangle.Width / 6);
				rect = new Rectangle(rectangle.X + lineWidth, rectangle.Y + lineWidth, rectangle.Width - lineWidth * 2, rectangle.Height- lineWidth * 2);

				int Scale = Math.Max (1, rectangle.Width / 12);
				int top = (rect.Y + lineWidth + ((rect.Height - ((2 * Scale) + lineWidth)) / 2));

				for (int i=0; i<lineWidth; i++) {
					graphics.DrawLine (pen, rect.Left+lineWidth/2, top+i, rect.Left+lineWidth/2+2*Scale, top+2*Scale+i);
					graphics.DrawLine (pen, rect.Left+lineWidth/2+2*Scale, top+2*Scale+i, rect.Left+lineWidth/2+6*Scale, top-2*Scale+i);
				}
				return;
			}
			}

		}

		public override void CPDrawMixedCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			if ((state & ButtonState.Flat) != 0)
				CPDrawCheckBoxInternal(graphics, rectangle, state, true /* mixed */);
			else
				DrawCheckBoxNative(graphics, rectangle, state, true);
		}

		public override void CPDrawRadioButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			CPColor cpcolor = ResPool.GetCPColor (ColorControl);
			
			Color dot_color = Color.Black;
			
			Color top_left_outer = Color.Black;
			Color top_left_inner = Color.Black;
			Color bottom_right_outer = Color.Black;
			Color bottom_right_inner = Color.Black;
			
			int ellipse_diameter = (rectangle.Width > rectangle.Height) ? (int)(rectangle.Height  * 0.9f) : (int)(rectangle.Width * 0.9f);
			int radius = ellipse_diameter / 2;
			
			Rectangle rb_rect = new Rectangle (rectangle.X + (rectangle.Width / 2) - radius, rectangle.Y + (rectangle.Height / 2) - radius, ellipse_diameter, ellipse_diameter);
			
			Brush brush = null;
			
			if ((state & ButtonState.All) == ButtonState.All) {
				brush = SystemBrushes.Control;
				dot_color = cpcolor.Dark;
			} else
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Pushed) == ButtonState.Pushed))
					brush = SystemBrushes.Control;
				else
					brush = SystemBrushes.ControlLightLight;
			} else {
				if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Pushed) == ButtonState.Pushed))
					brush = SystemBrushes.Control;
				else
					brush = SystemBrushes.ControlLightLight;
				
				top_left_outer = cpcolor.Dark;
				top_left_inner = cpcolor.DarkDark;
				bottom_right_outer = cpcolor.Light;
				bottom_right_inner = Color.Transparent;
				
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					dot_color = cpcolor.Dark;
			}
			
			dc.FillEllipse (brush, rb_rect.X + 1, rb_rect.Y + 1, ellipse_diameter - 1, ellipse_diameter - 1);
			
			int line_width = Math.Max (1, (int)(ellipse_diameter * 0.08f));
			
			dc.DrawArc (ResPool.GetSizedPen (top_left_outer, line_width), rb_rect, 135.0f, 180.0f);
			dc.DrawArc (ResPool.GetSizedPen (top_left_inner, line_width), Rectangle.Inflate (rb_rect, -line_width, -line_width), 135.0f, 180.0f);
			dc.DrawArc (ResPool.GetSizedPen (bottom_right_outer, line_width), rb_rect, 315.0f, 180.0f);
			
			if (bottom_right_inner != Color.Transparent)
				dc.DrawArc (ResPool.GetSizedPen (bottom_right_inner, line_width), Rectangle.Inflate (rb_rect, -line_width, -line_width), 315.0f, 180.0f);
			else
				using (Pen h_pen = new Pen (SystemColors.Control, line_width)) {
					dc.DrawArc (h_pen, Rectangle.Inflate (rb_rect, -line_width, -line_width), 315.0f, 180.0f);
				}
			
			if ((state & ButtonState.Checked) == ButtonState.Checked) {
				int inflate = line_width * 4;
				Rectangle tmp = Rectangle.Inflate (rb_rect, -inflate, -inflate);
				if (rectangle.Height >  13) {
					tmp.X += 1;
					tmp.Y += 1;
					tmp.Height -= 1;
					dc.FillEllipse (ResPool.GetSolidBrush (dot_color), tmp);
				} else {
					Pen pen = ResPool.GetPen (dot_color);
					dc.DrawLine (pen, tmp.X, tmp.Y + (tmp.Height / 2), tmp.Right, tmp.Y + (tmp.Height / 2));
					dc.DrawLine (pen, tmp.X, tmp.Y + (tmp.Height / 2) + 1, tmp.Right, tmp.Y + (tmp.Height / 2) + 1);
					
					dc.DrawLine (pen, tmp.X + (tmp.Width / 2), tmp.Y, tmp.X + (tmp.Width / 2), tmp.Bottom);
					dc.DrawLine (pen, tmp.X + (tmp.Width / 2) + 1, tmp.Y, tmp.X + (tmp.Width / 2) + 1, tmp.Bottom);
				}
			}
		}

		public override void CPDrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {

		}


		public override void CPDrawReversibleLine (Point start, Point end, Color backColor) {

		}


		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state)
		{
			DrawScrollButtonPrimitive (dc, area, state);
			
			bool fill_rect = true;
			int offset = 0;
			
			if ((state & ButtonState.Pushed) != 0)
				offset = 1;
			
			// skip the border
			Rectangle rect = new Rectangle (area.X + 2 + offset, area.Y + 2 + offset, area.Width - 4, area.Height - 4);
			
			Point [] arrow = new Point [3];
			for (int i = 0; i < 3; i++)
				arrow [i] = new Point ();
			
			Pen pen = SystemPens.ControlText;
			
			if ((state & ButtonState.Inactive) != 0) {
				pen = SystemPens.ControlDark;
			}
			
			switch (type) {
				default:
				case ScrollButton.Down:
					int x_middle = (int)Math.Round (rect.Width / 2.0f) - 1;
					int y_middle = (int)Math.Round (rect.Height / 2.0f) - 1;
					if (x_middle == 1)
						x_middle = 2;
					
					int triangle_height;
					
					if (rect.Height < 8) {
						triangle_height = 2;
						fill_rect = false;
					} else if (rect.Height == 11) {
						triangle_height = 3;
					} else {
						triangle_height = (int)Math.Round (rect.Height / 3.0f);
					}
					
					arrow [0].X = rect.X + x_middle;
					arrow [0].Y = rect.Y + y_middle + triangle_height / 2;
					
					arrow [1].X = arrow [0].X + triangle_height - 1;
					arrow [1].Y = arrow [0].Y - triangle_height + 1;
					arrow [2].X = arrow [0].X - triangle_height + 1;
					arrow [2].Y = arrow [1].Y;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X + 1, arrow [1].Y + 1, arrow [0].X + 1, arrow [0].Y + 1);
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X, arrow [1].Y + 1, arrow [0].X + 1, arrow [0].Y);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [0].Y - arrow [1].Y; i++) {
							dc.DrawLine (pen, arrow [1].X, arrow [1].Y + i, arrow [2].X, arrow [1].Y + i);
							arrow [1].X -= 1;
							arrow [2].X += 1;
						}
					}
					break;
					
				case ScrollButton.Up:
					x_middle = (int)Math.Round (rect.Width / 2.0f) - 1;
					y_middle = (int)Math.Round (rect.Height / 2.0f);
					if (x_middle == 1)
						x_middle = 2;
					
					if (y_middle == 1)
						y_middle = 2;
					
					if (rect.Height < 8) {
						triangle_height = 2;
						fill_rect = false;
					} else if (rect.Height == 11) {
						triangle_height = 3;
					} else {
						triangle_height = (int)Math.Round (rect.Height / 3.0f);
					}
					
					arrow [0].X = rect.X + x_middle;
					arrow [0].Y = rect.Y + y_middle - triangle_height / 2;
					
					arrow [1].X = arrow [0].X + triangle_height - 1;
					arrow [1].Y = arrow [0].Y + triangle_height - 1;
					arrow [2].X = arrow [0].X - triangle_height + 1;
					arrow [2].Y = arrow [1].Y;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X + 1, arrow [1].Y + 1, arrow [2].X + 1, arrow [1].Y + 1);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [1].Y - arrow [0].Y; i++) {
							dc.DrawLine (pen, arrow [2].X, arrow [1].Y - i, arrow [1].X, arrow [1].Y - i);
							arrow [1].X -= 1;
							arrow [2].X += 1;
						}
					}
					break;
					
				case ScrollButton.Left:
					y_middle = (int)Math.Round (rect.Height / 2.0f) - 1;
					if (y_middle == 1)
						y_middle = 2;
					
					int triangle_width;
					
					if (rect.Width < 8) {
						triangle_width = 2;
						fill_rect = false;
					} else if (rect.Width == 11) {
						triangle_width = 3;
					} else {
						triangle_width = (int)Math.Round (rect.Width / 3.0f);
					}
					
					arrow [0].X = rect.Left + triangle_width - 1;
					arrow [0].Y = rect.Y + y_middle;
					
					if (arrow [0].X - 1 == rect.X)
						arrow [0].X += 1;
					
					arrow [1].X = arrow [0].X + triangle_width - 1;
					arrow [1].Y = arrow [0].Y - triangle_width + 1;
					arrow [2].X = arrow [1].X;
					arrow [2].Y = arrow [0].Y + triangle_width - 1;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X + 1, arrow [1].Y + 1, arrow [2].X + 1, arrow [2].Y + 1);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [2].X - arrow [0].X; i++) {
							dc.DrawLine (pen, arrow [2].X - i, arrow [1].Y, arrow [2].X - i, arrow [2].Y);
							arrow [1].Y += 1;
							arrow [2].Y -= 1;
						}
					}
					break;
					
				case ScrollButton.Right:
					y_middle = (int)Math.Round (rect.Height / 2.0f) - 1;
					if (y_middle == 1)
						y_middle = 2;
					
					if (rect.Width < 8) {
						triangle_width = 2;
						fill_rect = false;
					} else if (rect.Width == 11) {
						triangle_width = 3;
					} else {
						triangle_width = (int)Math.Round (rect.Width / 3.0f);
					}
					
					arrow [0].X = rect.Right - triangle_width - 1;
					arrow [0].Y = rect.Y + y_middle;
					
					if (arrow [0].X - 1 == rect.X)
						arrow [0].X += 1;
					
					arrow [1].X = arrow [0].X - triangle_width + 1;
					arrow [1].Y = arrow [0].Y - triangle_width + 1;
					arrow [2].X = arrow [1].X;
					arrow [2].Y = arrow [0].Y + triangle_width - 1;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [0].X + 1, arrow [0].Y + 1, arrow [2].X + 1, arrow [2].Y + 1);
						dc.DrawLine (SystemPens.ControlLightLight, arrow [0].X, arrow [0].Y + 1, arrow [2].X + 1, arrow [2].Y);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [0].X - arrow [1].X; i++) {
							dc.DrawLine (pen, arrow [2].X + i, arrow [1].Y, arrow [2].X + i, arrow [2].Y);
							arrow [1].Y += 1;
							arrow [2].Y -= 1;
						}
					}
					break;
			}
		}

		public  override void CPDrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor) {

		}


		public override void CPDrawSizeGrip (Graphics dc, Color backColor, Rectangle bounds)
		{
			Pen pen_dark = ResPool.GetPen(ControlPaint.Dark(backColor));
			Pen pen_light_light = ResPool.GetPen(ControlPaint.LightLight(backColor));
			
			for (int i = 2; i < bounds.Width - 2; i += 4) {
				dc.DrawLine (pen_light_light, bounds.X + i, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + i - 1);
				dc.DrawLine (pen_dark, bounds.X + i + 1, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + i);
				dc.DrawLine (pen_dark, bounds.X + i + 2, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + i + 1);
			}
		}

		private void DrawStringDisabled20 (Graphics g, string s, Font font, Rectangle layoutRectangle, Color color, TextFormatFlags flags, bool useDrawString)
		{
			CPColor cpcolor = ResPool.GetCPColor (color);

			layoutRectangle.Offset (1, 1);
			TextRenderer.DrawTextInternal (g, s, font, layoutRectangle, cpcolor.LightLight, flags, useDrawString);

			layoutRectangle.Offset (-1, -1);
			TextRenderer.DrawTextInternal (g, s, font, layoutRectangle, cpcolor.Dark, flags, useDrawString);
		}

		public  override void CPDrawStringDisabled (Graphics dc, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format)
		{
			dc.DrawString(s, font, ResPool.GetSolidBrush(SystemColors.GrayText), layoutRectangle, format);
		}

		public override void CPDrawStringDisabled (IDeviceContext dc, string s, Font font, Color color, Rectangle layoutRectangle, TextFormatFlags format)
		{
			TextRenderer.DrawText (dc, s, font, layoutRectangle, SystemColors.GrayText, format);
		}

		public override void CPDrawVisualStyleBorder (Graphics graphics, Rectangle bounds)
		{
			graphics.DrawRectangle (SystemPens.ControlDarkDark, bounds);
		}

		private static void DrawBorderInternal (Graphics graphics, int startX, int startY, int endX, int endY,
			int width, Color color, ButtonBorderStyle style, Border3DSide side) 
		{
			DrawBorderInternal (graphics, (float) startX, (float) startY, (float) endX, (float) endY, 
				width, color, style, side);
		}

		private static void DrawBorderInternal (Graphics graphics, float startX, float startY, float endX, float endY,
			int width, Color color, ButtonBorderStyle style, Border3DSide side) {

			Pen pen = null;

			switch (style) {
			case ButtonBorderStyle.Solid:
			case ButtonBorderStyle.Inset:
			case ButtonBorderStyle.Outset:
					pen = ThemeEngine.Current.ResPool.GetDashPen (color, DashStyle.Solid);
					break;
			case ButtonBorderStyle.Dashed:
					pen = ThemeEngine.Current.ResPool.GetDashPen (color, DashStyle.Dash);
					break;
			case ButtonBorderStyle.Dotted:
					pen = ThemeEngine.Current.ResPool.GetDashPen (color, DashStyle.Dot);
					break;
			default:
			case ButtonBorderStyle.None:
					return;
			}

			switch(style) {
			case ButtonBorderStyle.Outset: {
				Color		colorGrade;
				int		hue, brightness, saturation;
				int		brightnessSteps;
				int		brightnessDownSteps;

				ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

				brightnessDownSteps=brightness/width;
				if (brightness>127) {
					brightnessSteps=Math.Max(6, (160-brightness)/width);
				} else {
					brightnessSteps=(127-brightness)/width;
				}

				for (int i=0; i<width; i++) {
					switch(side) {
					case Border3DSide.Left:	{
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
						break;
					}

					case Border3DSide.Right: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
						break;
					}

					case Border3DSide.Top: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
						break;
					}

					case Border3DSide.Bottom: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
						break;
					}
					}
				}
				break;
			}

			case ButtonBorderStyle.Inset: {
				Color		colorGrade;
				int		hue, brightness, saturation;
				int		brightnessSteps;
				int		brightnessDownSteps;

				ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

				brightnessDownSteps=brightness/width;
				if (brightness>127) {
					brightnessSteps=Math.Max(6, (160-brightness)/width);
				} else {
					brightnessSteps=(127-brightness)/width;
				}

				for (int i=0; i<width; i++) {
					switch(side) {
					case Border3DSide.Left:	{
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
						break;
					}

					case Border3DSide.Right: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
						break;
					}

					case Border3DSide.Top: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
						break;
					}

					case Border3DSide.Bottom: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
						break;
					}
					}
				}
				break;
			}

				/*
					I decided to have the for-loop duplicated for speed reasons;
					that way we only have to switch once (as opposed to have the
					for-loop around the switch)
				*/
			default: {
				switch(side) {
				case Border3DSide.Left:	{
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
					}
					break;
				}

				case Border3DSide.Right: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
					}
					break;
				}

				case Border3DSide.Top: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
					}
					break;
				}

				case Border3DSide.Bottom: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
					}
					break;
				}
				}
				break;
			}
			}
		}

		/*
			This function actually draws the various caption elements.
			This way we can scale them nicely, no matter what size, and they
			still look like MS's scaled caption buttons. (as opposed to scaling a bitmap)
		*/

		private void DrawCaptionHelper(Graphics graphics, Color color, Pen pen, int lineWidth, int shift, Rectangle captionRect, CaptionButton button) {
			switch(button) {
			case CaptionButton.Close: {
				if (lineWidth<2) {
					graphics.DrawLine(pen, captionRect.Left+2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
					graphics.DrawLine(pen, captionRect.Right-2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
				}

				graphics.DrawLine(pen, captionRect.Left+2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
				graphics.DrawLine(pen, captionRect.Right-2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
				return;
			}

			case CaptionButton.Help: {
				StringFormat	sf = new StringFormat();				
				Font				font = new Font("Microsoft Sans Serif", captionRect.Height, FontStyle.Bold, GraphicsUnit.Pixel);

				sf.Alignment=StringAlignment.Center;
				sf.LineAlignment=StringAlignment.Center;


				graphics.DrawString("?", font, ResPool.GetSolidBrush (color), captionRect.X+captionRect.Width/2+shift, captionRect.Y+captionRect.Height/2+shift+lineWidth/2, sf);

				sf.Dispose();				
				font.Dispose();

				return;
			}

			case CaptionButton.Maximize: {
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+2*lineWidth+shift+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift+i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
				}
				return;
			}

			case CaptionButton.Minimize: {
				/* Bottom line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth+shift, captionRect.Bottom-lineWidth+shift-i);
				}
				return;
			}

			case CaptionButton.Restore: {
				/** First 'window' **/
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift, captionRect.Top+2*lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift-i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+4*lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+5*lineWidth-lineWidth/2+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i);
				}

				/** Second 'window' **/
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+4*lineWidth+shift+1-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+4*lineWidth+shift+1-i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+4*lineWidth+shift+1, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Top+4*lineWidth+shift+1, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Bottom-lineWidth+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
				}

				return;
			}

			}
		}

		/* Generic scroll button */
		public void DrawScrollButtonPrimitive (Graphics dc, Rectangle area, ButtonState state) {
			if ((state & ButtonState.Pushed) == ButtonState.Pushed) {
				dc.FillRectangle (SystemBrushes.Control, area.X + 1,
					area.Y + 1, area.Width - 2 , area.Height - 2);

				dc.DrawRectangle (SystemPens.ControlDark, area.X,
					area.Y, area.Width, area.Height);

				return;
			}			
	
			Brush sb_control = SystemBrushes.Control;
			Brush sb_lightlight = SystemBrushes.ControlLightLight;
			Brush sb_dark = SystemBrushes.ControlDark;
			Brush sb_darkdark = SystemBrushes.ControlDarkDark;
			
			dc.FillRectangle (sb_control, area.X, area.Y, area.Width, 1);
			dc.FillRectangle (sb_control, area.X, area.Y, 1, area.Height);

			dc.FillRectangle (sb_lightlight, area.X + 1, area.Y + 1, area.Width - 1, 1);
			dc.FillRectangle (sb_lightlight, area.X + 1, area.Y + 2, 1,
				area.Height - 4);
			
			dc.FillRectangle (sb_dark, area.X + 1, area.Y + area.Height - 2,
				area.Width - 2, 1);

			dc.FillRectangle (sb_darkdark, area.X, area.Y + area.Height -1,
				area.Width , 1);

			dc.FillRectangle (sb_dark, area.X + area.Width - 2,
				area.Y + 1, 1, area.Height -3);

			dc.FillRectangle (sb_darkdark, area.X + area.Width -1,
				area.Y, 1, area.Height - 1);

			dc.FillRectangle (sb_control, area.X + 2,
				area.Y + 2, area.Width - 4, area.Height - 4);
			
		}
		
		public override void CPDrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style) {
			switch (border_style){
			case BorderStyle.Fixed3D:
				dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X, area.Y, area.X +area.Width, area.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X, area.Y, area.X, area.Y + area.Height);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X , area.Y + area.Height - 1, area.X + area.Width , 
					area.Y + area.Height - 1);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X + area.Width -1 , area.Y, area.X + area.Width -1, 
					area.Y + area.Height);

				dc.DrawLine (ResPool.GetPen (ColorActiveBorder), area.X + 1, area.Bottom - 2, area.Right - 2, area.Bottom - 2);
				dc.DrawLine (ResPool.GetPen (ColorActiveBorder), area.Right - 2, area.Top + 1, area.Right - 2, area.Bottom - 2);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), area.X + 1, area.Top + 1, area.X + 1, area.Bottom - 3);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), area.X + 1, area.Top + 1, area.Right - 3, area.Top + 1);
				break;
			case BorderStyle.FixedSingle:
				dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame), area.X, area.Y, area.Width - 1, area.Height - 1);
				break;
			case BorderStyle.None:
			default:
				break;
			}
			
		}
		#endregion	// ControlPaint


	} //class
}

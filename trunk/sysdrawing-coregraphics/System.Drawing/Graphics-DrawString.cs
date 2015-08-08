using System;
using System.Text;

#if MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreText;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreText;
#endif

using nfloat = System.Single;
using NMath = System.Math;

namespace System.Drawing
{
	public partial class Graphics 
	{

		public Region[] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			if ((text == null) || (text.Length == 0))
				return new Region [0];

			if (font == null)
				throw new ArgumentNullException ("font");

			if (stringFormat == null)
				throw new ArgumentException ("stringFormat");

			throw new NotImplementedException ();
		}

		public SizeF MeasureString (string text, Font font)
		{
			return MeasureString (text, font, SizeF.Empty);
		}

		public SizeF MeasureString (string textg, Font font, int width)
		{
			return MeasureString (textg, font, new SizeF (width, Int32.MaxValue));
		}

		public SizeF MeasureString (string textg, Font font, SizeF layoutArea)
		{
			return MeasureString (textg, font, layoutArea, StringFormat.GenericDefault);
		}

		public SizeF MeasureString (string text, Font font, PointF point, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat)
		{
			int charactersFitted, linesFilled;
			return MeasureString(text, font, layoutArea, stringFormat, out charactersFitted, out linesFilled);
		}

		public SizeF MeasureString (string text, Font font, int width, StringFormat stringFormat)
		{
			return MeasureString(text, font, new SizeF(width, 0), stringFormat);
		}

		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, 
		                            out int charactersFitted, out int linesFilled)
		{	
			charactersFitted = 0;
			linesFilled = 0;

			if (String.IsNullOrEmpty(text))
				return SizeF.Empty;

			if (font == null)
				throw new ArgumentNullException ("font");

			// As per documentation 
			// https://developer.apple.com/library/mac/#documentation/GraphicsImaging/Conceptual/drawingwithquartz2d/dq_text/dq_text.html#//apple_ref/doc/uid/TP30001066-CH213-TPXREF101
			// 
			// * Note * Not sure if we should save off the graphic state, set context transform to identity
			//  and restore state to do the measurement.  Something to be looked into.
			//			context.TextPosition = rect.Location;
			//			var startPos = context.TextPosition;
			//			context.SelectFont ( font.nativeFont.PostScriptName,
			//			                    font.SizeInPoints,
			//			                    CGTextEncoding.MacRoman);
			//			context.SetTextDrawingMode(CGTextDrawingMode.Invisible); 
			//			
			//			context.SetCharacterSpacing(1); 
			//			var textMatrix = CGAffineTransform.MakeScale(1f,-1f);
			//			context.TextMatrix = textMatrix;
			//
			//			context.ShowTextAtPoint(rect.X, rect.Y, textg, textg.Length); 
			//			var endPos = context.TextPosition;
			//
			//			var measure = new SizeF(endPos.X - startPos.X, font.nativeFont.CapHeightMetric);

			var atts = buildAttributedString(text, font, stringFormat);

			var measure = SizeF.Empty;
			// Calculate the lines
			int start = 0;
			int length = (int)atts.Length;

			var typesetter = new CTTypesetter(atts);

			if ((stringFormat.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				layoutArea = new SizeF (layoutArea.Height, layoutArea.Width);
			}

			while (start < length) {
				int count = typesetter.SuggestLineBreak (start, layoutArea.Width == 0 ? 8388608 : layoutArea.Width);
				var line = typesetter.GetLine (new NSRange(start, count));

				// Create and initialize some values from the bounds.
				nfloat ascent;
				nfloat descent;
				nfloat leading;
				var lineWidth = line.GetTypographicBounds (out ascent, out descent, out leading);

				measure.Height += (float)NMath.Ceiling (ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
				if (lineWidth > measure.Width)
					measure.Width = (float)NMath.Ceiling (lineWidth);

				line.Dispose ();
				start += count;
				linesFilled++;
			}

			if ((stringFormat.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				return new SizeF (measure.Height, measure.Width);
			}

			return measure;
		}

		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format = null)
		{
			DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0, 0), format);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format = null)
		{
			DrawString (s, font, brush, new RectangleF(x, y, 0, 0), format);
		}

		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format = null)
		{
			if (font == null)
				throw new ArgumentNullException ("font");
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (s == null || s.Length == 0)
				return;

			if (format == null) 
			{
				format = StringFormat.GenericDefault;
			}

			// TODO: Take into consideration units

			// Not sure we need the Save and Restore around this yet.
			context.SaveState();

			// TextMatrix is not part of the Graphics State and Restore 
			var saveMatrix = context.TextMatrix;
			bool layoutAvailable = true;

			//			context.SelectFont ( font.nativeFont.PostScriptName,
			//			                    font.SizeInPoints,
			//			                    CGTextEncoding.MacRoman);
			//
			//			context.SetCharacterSpacing(1);
			//			context.SetTextDrawingMode(CGTextDrawingMode.Fill); // 5
			//			
			//			// Setup both the stroke and the fill ?
			//			brush.Setup(this, true);
			//			brush.Setup(this, false);
			//
			//			var textMatrix = font.nativeFont.Matrix;
			//
			//			textMatrix.Scale(1,-1);
			//			context.TextMatrix = textMatrix;
			//
			//			context.ShowTextAtPoint(layoutRectangle.X, 
			//			                        layoutRectangle.Y + font.nativeFont.CapHeightMetric, s); 
			//
			//
			// First we call the brush with a fill of false so the brush can setup the stroke color
			// that the text will be using.
			// For LinearGradientBrush this will setup a TransparentLayer so the gradient can
			// be filled at the end.  See comments.
			brush.Setup(this, false); // Stroke

			// I think we only Fill the text with no Stroke surrounding
			context.SetTextDrawingMode(CGTextDrawingMode.Fill);

			context.SetAllowsAntialiasing (true);
			context.SetAllowsFontSmoothing (true);
			context.SetAllowsSubpixelPositioning (true);
			context.SetAllowsFontSubpixelQuantization (true);

			context.SetShouldAntialias (true);
			context.SetShouldSmoothFonts (true);
			context.SetShouldSubpixelPositionFonts (true);
			context.ShouldSubpixelQuantizeFonts(true);

			var attributedString = buildAttributedString(s, font, format, lastBrushColor);

			// Work out the geometry
			RectangleF insetBounds = layoutRectangle;
			if (insetBounds.Size == SizeF.Empty)
			{
				insetBounds.Width = (float)boundingBox.Width;
				insetBounds.Height = (float)boundingBox.Height;
				layoutAvailable = false;

				if (format.LineAlignment != StringAlignment.Near) 
				{
					insetBounds.Size = MeasureString (s, font);
				}
			}

			PointF textPosition = new PointF(insetBounds.X + .5f, insetBounds.Y + .5f);

			float boundsWidth;
			float boundsHeight;

			// Calculate the lines
			int start = 0;
			int length = (int)attributedString.Length;
			float baselineOffset = 0;

			var typesetter = new CTTypesetter(attributedString);

			// If we are drawing vertial direction then we need to rotate our context transform by 90 degrees
			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				//textMatrix.Rotate (ConversionHelpers.DegreesToRadians (90));
				//var verticalOffset = 0.0f;
				baselineOffset = 0;
				while (start < length) {
					int count = typesetter.SuggestLineBreak (start, insetBounds.Height);
					var line = typesetter.GetLine (new NSRange (start, count));

					// Create and initialize some values from the bounds.
					nfloat ascent;
					nfloat descent;
					nfloat leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);
					baselineOffset += (float)NMath.Ceiling (ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
					line.Dispose ();
					start += count;
				}
				context.TranslateCTM (layoutRectangle.X, layoutRectangle.Y);
				context.RotateCTM (ConversionHelpers.DegreesToRadians (90));
				context.TranslateCTM (-layoutRectangle.X, -layoutRectangle.Y);
				if (layoutAvailable)
					context.TranslateCTM (0, -layoutRectangle.Width);
				else
					context.TranslateCTM (0, -baselineOffset);
				boundsWidth = insetBounds.Height;
				boundsHeight = insetBounds.Width;
				start = 0;
			} else {
				// First we need to calculate the offset for Vertical Alignment if we 
				// are using anything but Top
				if (layoutAvailable && format.LineAlignment != StringAlignment.Near) {
					while (start < length) {
						int count = typesetter.SuggestLineBreak (start, insetBounds.Width);
						var line = typesetter.GetLine (new NSRange(start, count));

						// Create and initialize some values from the bounds.
						nfloat ascent;
						nfloat descent;
						nfloat leading;
						line.GetTypographicBounds (out ascent, out descent, out leading);
						baselineOffset += (float)NMath.Ceiling (ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
						line.Dispose ();
						start += count;
					}

					start = 0;
				}

				boundsWidth = insetBounds.Width;
				boundsHeight = insetBounds.Height;
			}

			start = 0;
			while (start < length && textPosition.Y < insetBounds.Bottom)
			{

				// Now we ask the typesetter to break off a line for us.
				// This also will take into account line feeds embedded in the text.
				//  Example: "This is text \n with a line feed embedded inside it"
				int count = typesetter.SuggestLineBreak(start, boundsWidth);
				var line = typesetter.GetLine(new NSRange(start, count));

				// Create and initialize some values from the bounds.
				nfloat ascent;
				nfloat descent;
				nfloat leading;
				double lineWidth = line.GetTypographicBounds(out ascent, out descent, out leading);

				// Calculate the string format if need be
				var penFlushness = 0.0f;
				if (format != null) 
				{
					if (layoutAvailable) 
					{
						if (format.Alignment == StringAlignment.Far)
							penFlushness = (float)line.GetPenOffsetForFlush(1.0f, boundsWidth);
						else if (format.Alignment == StringAlignment.Center)
							penFlushness = (float)line.GetPenOffsetForFlush(0.5f, boundsWidth);
					}
					else 
					{
						// We were only passed in a point so we need to format based
						// on the point.
						if (format.Alignment == StringAlignment.Far)
							penFlushness -= (float)lineWidth;
						else if (format.Alignment == StringAlignment.Center)
							penFlushness -= (float)lineWidth / 2.0f;


					}


				}

				// initialize our Text Matrix or we could get trash in here
				var textMatrix = new CGAffineTransform (
					1, 0, 0, -1, 0, ascent);

				if (format.LineAlignment == StringAlignment.Near)
					textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y); 
				if (format.LineAlignment == StringAlignment.Center) 
				{
					if (layoutAvailable)
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y + ((boundsHeight / 2) - (baselineOffset / 2)));
					else
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y - ((boundsHeight / 2) - (baselineOffset / 2)));
				}

				if (format.LineAlignment == StringAlignment.Far) 
				{
					if (layoutAvailable)
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y + ((boundsHeight) - (baselineOffset)));
					else
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y - ((boundsHeight) - (baselineOffset)));
				}

				context.TextMatrix = textMatrix;

				// and draw the line
				line.Draw(context);

				// Move the index beyond the line break.
				start += count;
				textPosition.Y += (float)NMath.Ceiling(ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
				line.Dispose();

			}

			// Now we call the brush with a fill of true so the brush can do the fill if need be 
			// For LinearGradientBrush this will draw the Gradient and end the TransparentLayer.
			// See comments.
			brush.Setup(this, true); // Fill

			context.TextMatrix = saveMatrix;
			context.RestoreState();
		}	

		private NSMutableAttributedString buildAttributedString(string text, Font font, StringFormat format = null, 
			Color? fontColor=null) 
		{
			// Create a new attributed string definition
			var ctAttributes = new CTStringAttributes ();

			// Font attribute
			ctAttributes.Font = font.nativeFont;
			// -- end font 

			if (format != null && (format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) 
			{
				//ctAttributes.VerticalForms = true;

			}

			if (fontColor.HasValue) {

				// Font color
				var fc = fontColor.Value;
				var cgColor = new CGColor(fc.R / 255f, 
					fc.G / 255f,
					fc.B / 255f,
					fc.A / 255f);

				ctAttributes.ForegroundColor = cgColor;
				ctAttributes.ForegroundColorFromContext = false;
				// -- end font Color
			}

			if (font.Underline) {
				// Underline
				ctAttributes.UnderlineStyle = CTUnderlineStyle.Single;
				// --- end underline
			}


			if (font.Strikeout) {
				// StrikeThrough
#if MONOMAC
				int single = (int)MonoMac.AppKit.NSUnderlineStyle.Single;
				int solid = (int)MonoMac.AppKit.NSUnderlinePattern.Solid;
				var attss = single | solid;
				ctAttributes.UnderlineStyleValue = attss;
#else
				ctAttributes.UnderlineStyleValue = 1;
#endif

				// --- end StrikeThrough
			}


			var alignment = CTTextAlignment.Left;
			var alignmentSettings = new CTParagraphStyleSettings();
			alignmentSettings.Alignment = alignment;
			var paragraphStyle = new CTParagraphStyle(alignmentSettings);

			ctAttributes.ParagraphStyle = paragraphStyle;
			// end text alignment

			NSMutableAttributedString atts;

			if (format == null || format.HotkeyPrefix == System.Drawing.Text.HotkeyPrefix.None) {
				atts = new NSMutableAttributedString (text, ctAttributes.Dictionary);
			} else {
				var sb = new StringBuilder ();
				bool wasHotkey = false;
				foreach (var c in text) {
					if (wasHotkey || c != '&') {
						sb.Append (c);
						wasHotkey = false;
					} else if (c == '&') {
						wasHotkey = true;
					}	
				}

				atts = new NSMutableAttributedString (sb.ToString(), ctAttributes.Dictionary);

				// Underline
				if (format.HotkeyPrefix == System.Drawing.Text.HotkeyPrefix.Show) {
					int index = 0;
					foreach (var c in text) {
						if (wasHotkey) {
							atts.AddAttributes (
								new CTStringAttributes () { UnderlineStyle = CTUnderlineStyle.Single },
								new NSRange (index, 1));
							index++;
							wasHotkey = false;
						} else if (c == '&') {
							wasHotkey = true;
						} else {
							index++;
						}
					}
				}
			}

			return atts;
		}
	}
}


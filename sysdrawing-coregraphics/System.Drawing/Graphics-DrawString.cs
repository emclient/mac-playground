using System;
using System.Text;
using System.Collections.Generic;

#if XAMARINMAC
using CoreGraphics;
using Foundation;
using CoreText;
#elif MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.CoreText;
using MonoMac.Foundation;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreText;
#endif

#if MAC64
using nfloat = System.Double;
#else
using nfloat = System.Single;
#endif
using NMath = System.Math;

namespace System.Drawing
{
	public partial class Graphics 
	{
		static SizeF maxSize = new SizeF(float.MaxValue, float.MaxValue);

		public Region[] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			if ((text == null) || (text.Length == 0))
				return new Region [0];

			if (font == null)
				throw new ArgumentNullException ("font");

			if (stringFormat == null)
				throw new ArgumentException ("stringFormat");

			// FIXME
			int n = stringFormat.measurableCharacterRanges != null ? stringFormat.measurableCharacterRanges.Length : 0;
			Region[] regions = new Region[n];
			for (int i = 0; i < n; ++i)
				regions[i] = new Region(); //layoutRect);

			return regions;
		}

		public SizeF MeasureString (string text, Font font)
		{
			return MeasureString (text, font, maxSize);
		}

		public SizeF MeasureString (string textg, Font font, int width)
		{
			return MeasureString (textg, font, new SizeF (width, float.MaxValue));
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
			return MeasureString(text, font, new SizeF(width, float.MaxValue), stringFormat);
		}

		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, 
		                            out int charactersFitted, out int linesFilled)
		{	
			charactersFitted = 0;
			linesFilled = 0;

			if (font == null)
				throw new ArgumentNullException("font");
			if (String.IsNullOrEmpty(text))
				return SizeF.Empty;

			var atts = buildAttributedString(text, font, stringFormat);
			var measure = SizeF.Empty;

			if ((stringFormat.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				layoutArea = new SizeF(layoutArea.Height, layoutArea.Width);
			}

			float baselineOffset = (float)NMath.Ceiling(font.nativeFont.AscentMetric + font.nativeFont.DescentMetric + 1);
			float lineHeight = (float)NMath.Ceiling(font.nativeFont.AscentMetric + font.nativeFont.DescentMetric + font.nativeFont.LeadingMetric + 1);
			var lines = CreateLines(font, atts, layoutArea, stringFormat, baselineOffset, lineHeight);
			foreach (var line in lines)
			{
				if (line != null)
				{
					nfloat ascent, descent, leading;
					var lineWidth = line.GetTypographicBounds(out ascent, out descent, out leading);
					if ((stringFormat.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
						lineWidth += line.TrailingWhitespaceWidth;
					measure.Width = Math.Max(measure.Width, (float)NMath.Ceiling((float)lineWidth));
					charactersFitted += (int)line.StringRange.Length;
					line.Dispose();
				}
				measure.Height += lineHeight;
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

		private static CTLine EllipsisToken(Font font, StringFormat format)
		{
			var attrStr = buildAttributedString("\u2026", font, format, null);
			return new CTLine (attrStr);
		}

		private static List<CTLine> CreateLines (Font font, NSAttributedString attributedString, SizeF layoutBox, StringFormat format, float baselineOffset, float lineHeight)
		{
			bool noWrap = (format.FormatFlags & StringFormatFlags.NoWrap) != 0;
			bool wholeLines = (format.FormatFlags & StringFormatFlags.LineLimit) != 0;
			using (var typesetter = new CTTypesetter(attributedString))
			{
				var lines = new List<CTLine>();
				int start = 0;
				int length = (int)attributedString.Length;
				float y = 0;
				while (start < length && y < layoutBox.Height && (!wholeLines || y + baselineOffset <= layoutBox.Height))
				{
					if (format.Trimming != StringTrimming.None)
					{
						// Keep the last line in full when trimming is enabled
						bool lastLine;
						if (!wholeLines)
							lastLine = y + lineHeight >= layoutBox.Height;
						else
							lastLine = y + lineHeight + baselineOffset > layoutBox.Height;
						if (lastLine)
							noWrap = true;
					}

					// Now we ask the typesetter to break off a line for us.
					// This also will take into account line feeds embedded in the text.
					//  Example: "This is text \n with a line feed embedded inside it"
					var count = (int)typesetter.SuggestLineBreak(start, noWrap ? double.MaxValue : layoutBox.Width);

					// Note: trimming may return a null line i.e. not enough space for any characters
					var line = typesetter.GetLine(new NSRange(start, count));
					switch (format.Trimming)
					{
						case StringTrimming.Character:
							using (var oldLine = line)
								line = line.GetTruncatedLine(layoutBox.Width, CTLineTruncation.End, null);
							break;
						case StringTrimming.EllipsisWord: // Fall thru for now
						case StringTrimming.EllipsisCharacter:
							using (var oldLine = line)
							using (CTLine ellipsisToken = EllipsisToken(font, format))
								line = line.GetTruncatedLine(layoutBox.Width, CTLineTruncation.End, ellipsisToken);
							break;
						case StringTrimming.EllipsisPath:
							using (var oldLine = line)
							using (CTLine ellipsisToken = EllipsisToken(font, format))
								line = line.GetTruncatedLine(layoutBox.Width, CTLineTruncation.Middle, ellipsisToken);
							break;
					}

					lines.Add(line);
					start += (int)count;
					y += (float)lineHeight;
				}
				return lines;
			}
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
				format = StringFormat.GenericDefault;

			// TODO: Take into consideration units

			// Not sure we need the Save and Restore around this yet.
			context.SaveState();

			// TextMatrix is not part of the Graphics State and Restore 
			var saveMatrix = context.TextMatrix;
			bool layoutAvailable = true;

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
			}

			float boundsWidth;
			float boundsHeight;

			float baselineOffset = (float)NMath.Ceiling(font.nativeFont.AscentMetric + font.nativeFont.DescentMetric + 1);
			float lineHeight = (float)NMath.Ceiling(font.nativeFont.AscentMetric + font.nativeFont.DescentMetric + font.nativeFont.LeadingMetric + 1);
			List<CTLine> lines = new List<CTLine>();

			// Calculate the lines
			// If we are drawing vertical direction then we need to rotate our context transform by 90 degrees
			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				// Swap out the width and height and calculate the lines
				lines = CreateLines(font, attributedString, new SizeF(insetBounds.Height, insetBounds.Width), format, baselineOffset, lineHeight);
				//textMatrix.Rotate (ConversionHelpers.DegreesToRadians (90));
				context.TranslateCTM (layoutRectangle.X, layoutRectangle.Y);
				context.RotateCTM (ConversionHelpers.DegreesToRadians (90));
				context.TranslateCTM (-layoutRectangle.X, -layoutRectangle.Y);
				if (layoutAvailable)
					context.TranslateCTM (0, -layoutRectangle.Width);
				else
					context.TranslateCTM (0, -baselineOffset);
				boundsWidth = insetBounds.Height;
				boundsHeight = insetBounds.Width;
			} else {
				lines = CreateLines(font, attributedString, insetBounds.Size, format, baselineOffset, lineHeight);
				boundsWidth = insetBounds.Width;
				boundsHeight = insetBounds.Height;
			}

			if (format.LineAlignment != StringAlignment.Near && !layoutAvailable) {
				float maxLineWidth = 0;
				foreach (var line in lines) {
					if (line != null) {
						nfloat ascent, descent, leading;
						var lineWidth = line.GetTypographicBounds(out ascent, out descent, out leading);
						if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
							lineWidth += line.TrailingWhitespaceWidth;
						maxLineWidth = Math.Max(maxLineWidth, (float)NMath.Ceiling((float)lineWidth));
					}
				}
				boundsWidth = maxLineWidth;
			}

			int currentLine = 0;
			PointF textPosition = new PointF(insetBounds.X + .5f, insetBounds.Y + .5f);
			foreach (var line in lines)
			{
				// Make sure that the line still exists, it may be null if it's too small to render any text and wants StringTrimming
				if (line != null)
				{
					nfloat ascent, descent, leading;
					var lineWidth = line.GetTypographicBounds(out ascent, out descent, out leading);

					// Calculate the string format if need be
					var penFlushness = 0.0f;
					if (layoutAvailable)
					{
						if (format.Alignment == StringAlignment.Far)
							penFlushness = (float)line.GetPenOffsetForFlush(1.0f, boundsWidth);
						else if (format.Alignment == StringAlignment.Center)
							penFlushness = (float)line.GetPenOffsetForFlush(0.5f, boundsWidth);
						if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
						{
							if (format.Alignment == StringAlignment.Far)
								penFlushness -= (float)line.TrailingWhitespaceWidth;
							else if (format.Alignment == StringAlignment.Center)
								penFlushness -= (float)line.TrailingWhitespaceWidth * 0.5f;
						}
					}
					else
					{
						// We were only passed in a point so we need to format based on the point.
						if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
							lineWidth += line.TrailingWhitespaceWidth;
						if (format.Alignment == StringAlignment.Far)
							penFlushness -= (float)lineWidth;
						else if (format.Alignment == StringAlignment.Center)
							penFlushness -= (float)lineWidth / 2.0f;
					}

					// initialize our Text Matrix or we could get trash in here
					nfloat x = textPosition.X + penFlushness;
					nfloat y = textPosition.Y;
					switch (format.LineAlignment)
					{
						case StringAlignment.Center:
							y += (layoutAvailable ? 1 : -1) * ((boundsHeight / 2) - (baselineOffset / 2));
							break;
						case StringAlignment.Far:
							y += (layoutAvailable ? 1 : -1) * ((boundsHeight) - (baselineOffset));
							break;
					}

					var textMatrix = new CGAffineTransform(1, 0, 0, -1, 0, ascent);
					textMatrix.Translate(x, y);
					context.TextMatrix = textMatrix;

					line.Draw (context);
					line.Dispose ();

					// Currently we support strikethrough only at the font level (for a whole text, not for ranges). It seems CoreText does not handle it for us.
					if (font.Strikeout)
					{
						var sy = y + ascent - font.nativeFont.XHeightMetric / (nfloat)2.0;
						context.MoveTo(x, sy);
						context.AddLineToPoint(x + (nfloat)lineWidth, sy);
						context.SetStrokeColor(lastBrushColor.ToCGColor());
						context.SetLineWidth(1);
						context.StrokePath();
					}
				}

				// Move the index beyond the line break.
				textPosition.Y += lineHeight;
				++currentLine;
			}

			// Now we call the brush with a fill of true so the brush can do the fill if need be 
			// For LinearGradientBrush this will draw the Gradient and end the TransparentLayer.
			// See comments.
			brush.Setup(this, true); // Fill

			context.TextMatrix = saveMatrix;
			context.RestoreState();
		}	

		private static NSMutableAttributedString buildAttributedString(string text, Font font, StringFormat format = null, Color? fontColor = null) 
		{
			var ctAttributes = new CTStringAttributes ();
			ctAttributes.Font = font.nativeFont;

			if (format != null && (format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) 
			{
				//ctAttributes.VerticalForms = true;
			}

			if (fontColor.HasValue)
			{
				ctAttributes.ForegroundColor = fontColor.Value.ToCGColor();
				ctAttributes.ForegroundColorFromContext = false;
			}

			if (font.Underline)
			{
				ctAttributes.UnderlineStyle = CTUnderlineStyle.Single;
			}

			//if (font.Strikeout)
			//{
			//	// Not used by CoreText, we have to process it ourselves
			//	ctAttributes.Dictionary.SetValueForKey(NSNumber.FromInt32(1), NSAttributedString.StrikethroughStyleAttributeName);
			//	ctAttributes.Dictionary.SetValueForKey(new NSObject(new CGColor(0.0f, 1.0f).Handle), NSAttributedString.StrikethroughColorAttributeName);
			//}

			var alignment = CTTextAlignment.Left;
			var alignmentSettings = new CTParagraphStyleSettings();
			alignmentSettings.Alignment = alignment;
			var paragraphStyle = new CTParagraphStyle(alignmentSettings);

			ctAttributes.ParagraphStyle = paragraphStyle;
			// end text alignment

			text = text.Replace("\0", "");
			if (format == null || format.HotkeyPrefix == System.Drawing.Text.HotkeyPrefix.None)
				return new NSMutableAttributedString (text, ctAttributes.Dictionary);
			
			var sb = new StringBuilder ();
			bool wasHotkey = false;

			int end = text.Length - 1;
			for (int i = 0; i < text.Length; ++i) {
				char c = text[i];
				if (wasHotkey || c != '&' || i == end) {
					sb.Append (c);
					wasHotkey = false;
				} else if (c == '&') {
					wasHotkey = true;
				}	
			}

			var atts = new NSMutableAttributedString (sb.ToString(), ctAttributes.Dictionary);

			// Underline
			wasHotkey = false;
			if (format.HotkeyPrefix == System.Drawing.Text.HotkeyPrefix.Show) {
				int index = 0;
				for (int i = 0; i < text.Length; ++i) {
					char c = text[i];
					if (wasHotkey) {
						atts.AddAttributes (
							new CTStringAttributes () { UnderlineStyle = CTUnderlineStyle.Single },
							new NSRange (index, 1));
						index++;
						wasHotkey = false;
					} else if (c == '&' && i != end && text[i+1] != '&') {
						wasHotkey = true;
					} else {
						index++;
					}
				}
			}

			//if (font.Strikeout)
			//{
			//	// Not used by CoreText, we have to process it ourselves
			//	atts.AddAttribute(NSAttributedString.StrikethroughStyleAttributeName, new NSNumber(1), new NSRange(0, atts.Length));
			//	if (fontColor.HasValue)
			//		atts.AddAttribute(NSAttributedString.StrikethroughColorAttributeName, new NSObject(fontColor.Value.ToCGColor().Handle), new NSRange(0, atts.Length));
			//}

			return atts;
		}
	}
}


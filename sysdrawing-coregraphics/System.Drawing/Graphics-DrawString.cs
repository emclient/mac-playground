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

			float lineHeight = (float)NMath.Ceiling(font.nativeFont.AscentMetric + font.nativeFont.DescentMetric + font.nativeFont.LeadingMetric + 1);
			var lines = CreateLines(font, atts, layoutArea, stringFormat, lineHeight);
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

		private CTLine EllipsisToken(Font font, StringFormat format)
		{
            var attrStr = buildAttributedString("\u2026", font, format, lastBrushColor);
			return new CTLine (attrStr);
		}

		private List<CTLine> CreateLines (Font font, NSAttributedString attributedString, SizeF layoutBox, StringFormat format, float lineHeight)
		{
			bool noWrap = (format.FormatFlags & StringFormatFlags.NoWrap) != 0;
			bool wholeLines = (format.FormatFlags & StringFormatFlags.LineLimit) != 0;
			using (var typesetter = new CTTypesetter(attributedString))
			{
				var lines = new List<CTLine>();
				int start = 0;
				int length = (int)attributedString.Length;
				float y = 0;
				while (start < length && y < layoutBox.Height && (!wholeLines || y + lineHeight <= layoutBox.Height))
				{
					if (format.Trimming != StringTrimming.None)
					{
						// Keep the last line in full when trimming is enabled
						bool lastLine;
						if (!wholeLines)
							lastLine = y + lineHeight >= layoutBox.Height;
						else
							lastLine = y + lineHeight + lineHeight > layoutBox.Height;
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

			// First we call the brush with a fill of false so the brush can setup the stroke color
			// that the text will be using.
			// For LinearGradientBrush this will setup a TransparentLayer so the gradient can
			// be filled at the end.  See comments.
			brush.Setup(this, false); // Stroke

            var savedSmoothingMode = SmoothingMode;
            SmoothingMode = Drawing2D.SmoothingMode.Default;

            var attributedString = buildAttributedString(s, font, format, lastBrushColor);

			// Work out the geometry
			bool layoutAvailable = true;
			RectangleF insetBounds = layoutRectangle;
			if (insetBounds.Size == SizeF.Empty)
			{
				insetBounds.Width = (float)boundingBox.Width;
				insetBounds.Height = (float)boundingBox.Height;
				layoutAvailable = false;
			}

			float boundsWidth;
			float boundsHeight;

			float lineHeight = (float)NMath.Ceiling(font.nativeFont.AscentMetric + font.nativeFont.DescentMetric + font.nativeFont.LeadingMetric + 1);
			List<CTLine> lines = new List<CTLine>();
            CGAffineTransform verticalMatrix = default(CGAffineTransform);

			// Calculate the lines
			// If we are drawing vertical direction then we need to rotate our context transform by 90 degrees
			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				// Swap out the width and height and calculate the lines
				lines = CreateLines(font, attributedString, new SizeF(insetBounds.Height, insetBounds.Width), format, lineHeight);
				verticalMatrix = CGAffineTransform.MakeTranslation(-layoutRectangle.X, -layoutRectangle.Y);
                verticalMatrix.Rotate((float)NMath.PI * 0.5f);
				verticalMatrix.Translate(layoutRectangle.X, layoutRectangle.Y);
				if (layoutAvailable)
					verticalMatrix.Translate(layoutRectangle.Width, 0);
				else
                    verticalMatrix.Translate((lines.Count * lineHeight), 0);
                context.ConcatCTM(verticalMatrix);
				boundsWidth = insetBounds.Height;
				boundsHeight = insetBounds.Width;
			} else {
				lines = CreateLines(font, attributedString, insetBounds.Size, format, lineHeight);
				boundsWidth = insetBounds.Width;
				boundsHeight = insetBounds.Height;
			}

			PointF textPosition = new PointF(insetBounds.X + .5f, insetBounds.Y + .5f);
			if (layoutAvailable)
			{
				if (format.LineAlignment == StringAlignment.Far)
					textPosition.Y += boundsHeight - (lines.Count * lineHeight);
				else if (format.LineAlignment == StringAlignment.Center)
                    textPosition.Y += (boundsHeight - (lines.Count * lineHeight)) / 2;
			}
			else
			{
                // Precalculate maximum width to allow for aligning lines
                if (format.Alignment != StringAlignment.Near) {
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
				if (format.LineAlignment == StringAlignment.Far)
					textPosition.Y -= lineHeight * lines.Count;
				else if (format.LineAlignment == StringAlignment.Center)
					textPosition.Y -= (lineHeight * lines.Count) / 2;
			}

			foreach (var line in lines)
			{
				// Make sure that the line still exists, it may be null if it's too small to render any text and wants StringTrimming
				if (line != null)
				{
					nfloat ascent, descent, leading;
					var lineWidth = line.GetTypographicBounds(out ascent, out descent, out leading);

					// Calculate the string format if need be
					nfloat x = textPosition.X;
					if (layoutAvailable)
					{
						if (format.Alignment == StringAlignment.Far)
							x += (float)line.GetPenOffsetForFlush(1.0f, boundsWidth);
						else if (format.Alignment == StringAlignment.Center)
							x += (float)line.GetPenOffsetForFlush(0.5f, boundsWidth);
						if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
						{
							if (format.Alignment == StringAlignment.Far)
								x -= (float)line.TrailingWhitespaceWidth;
							else if (format.Alignment == StringAlignment.Center)
								x -= (float)line.TrailingWhitespaceWidth * 0.5f;
						}
					}
					else
					{
						// We were only passed in a point so we need to format based on the point.
						if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
							lineWidth += line.TrailingWhitespaceWidth;
						if (format.Alignment == StringAlignment.Far)
							x -= (float)lineWidth;
						else if (format.Alignment == StringAlignment.Center)
							x -= (float)lineWidth / 2.0f;
					}

                    // initialize our Text Matrix or we could get trash in here
                    context.TextMatrix = new CGAffineTransform(1, 0, 0, -1, x, textPosition.Y + font.nativeFont.AscentMetric);
					line.Draw (context);
					line.Dispose ();

					// Currently we support strikethrough only at the font level (for a whole text, not for ranges). It seems CoreText does not handle it for us.
					if (font.Strikeout)
					{
						var sy = textPosition.Y + ascent - font.nativeFont.XHeightMetric / (nfloat)2.0;
						context.MoveTo(x, sy);
						context.AddLineToPoint(x + (nfloat)lineWidth, sy);
						context.SetStrokeColor(lastBrushColor.ToCGColor());
						context.SetLineWidth(1);
						context.StrokePath();
					}
				}

				// Move the index beyond the line break.
				textPosition.Y += lineHeight;
			}

			// Now we call the brush with a fill of true so the brush can do the fill if need be 
			// For LinearGradientBrush this will draw the Gradient and end the TransparentLayer.
			// See comments.
			brush.Setup(this, true); // Fill

            if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
                context.ConcatCTM(verticalMatrix.Invert());
			SmoothingMode = savedSmoothingMode;
		}	

        private static NSAttributedString buildAttributedString(string text, Font font, StringFormat format = null, Color? fontColor = null) 
		{
			var ctAttributes = new CTStringAttributes ();
			ctAttributes.Font = font.nativeFont;
			if (fontColor.HasValue)
			{
				ctAttributes.ForegroundColor = fontColor.Value.ToCGColor();
				ctAttributes.ForegroundColorFromContext = false;
			}
			if (font.Underline)
			{
				ctAttributes.UnderlineStyle = CTUnderlineStyle.Single;
			}
            // font.Strikeout - Not used by CoreText, we have to process it ourselves

            if (text.IndexOf('\0') >= 0)
                text = text.Replace("\0", "");
			if (format == null || format.HotkeyPrefix == System.Drawing.Text.HotkeyPrefix.None)
				return new NSAttributedString (text, ctAttributes.Dictionary);
			
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

			return atts;
		}
	}
}


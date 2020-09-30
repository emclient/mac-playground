using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Mac;
using System.Text;

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

		const int DrawStringCacheCapacity = 2000;
		static DrawStringCache DrawStringCache = new DrawStringCache(DrawStringCacheCapacity);
		static MeasureStringCache MeasureStringCache = new MeasureStringCache(DrawStringCacheCapacity);

		public Region[] MeasureCharacterRanges(string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			if ((text == null) || (text.Length == 0))
				return new Region[0];

			if (font == null)
				throw new ArgumentNullException("font");

			if (stringFormat == null)
				throw new ArgumentException("stringFormat");

			// FIXME
			int n = stringFormat.measurableCharacterRanges != null ? stringFormat.measurableCharacterRanges.Length : 0;
			Region[] regions = new Region[n];
			for (int i = 0; i < n; ++i)
				regions[i] = new Region(); //layoutRect);

			return regions;
		}

		public SizeF MeasureString(string text, Font font)
		{
			return MeasureString(text, font, maxSize, StringFormat.GenericDefault, out int charactersFitted, out int linesFilled);
		}

		public SizeF MeasureString(string text, Font font, int width)
		{
			return MeasureString(text, font, new SizeF(width, float.MaxValue), StringFormat.GenericDefault, out int charactersFitted, out int linesFilled);
		}

		public SizeF MeasureString(string text, Font font, SizeF area)
		{
			return MeasureString(text, font, area, StringFormat.GenericDefault, out int charactersFitted, out int linesFilled);
		}

		public SizeF MeasureString(string text, Font font, PointF point, StringFormat stringFormat)
		{
			throw new NotImplementedException();
		}

		public SizeF MeasureString(string text, Font font, SizeF area, StringFormat stringFormat)
		{
			return MeasureString(text, font, area, stringFormat, out int charactersFitted, out int linesFilled);
		}

		public SizeF MeasureString(string text, Font font, int width, StringFormat stringFormat)
		{
			return MeasureString(text, font, new SizeF(width, float.MaxValue), stringFormat, out int charactersFitted, out int linesFilled);
		}

		public SizeF MeasureString(string text, Font font, SizeF area, StringFormat format, out int charactersFitted, out int linesFilled)
		{
#if DEBUG
			++measureStringCount;
			measureStringStopWatch.Start();
			var result = MeasureStringInternal(text, font, area, format ?? StringFormat.GenericDefault, out charactersFitted, out linesFilled);
			measureStringStopWatch.Stop();

			if (measureStringCount % 1000 == 0)
			{
				Console.WriteLine($"MeasureString1k: {measureStringStopWatch.Elapsed.TotalSeconds}s");
				measureStringStopWatch.Reset();
			}
			return result;
		}

		static long measureStringCount = 0;
		static Diagnostics.Stopwatch measureStringStopWatch = new Diagnostics.Stopwatch();

		public SizeF MeasureStringInternal(string text, Font font, SizeF area, StringFormat format, out int charactersFitted, out int linesFilled)
		{
#endif
			if (text == null) {
				charactersFitted = linesFilled = 0;
				return SizeF.Empty;
			}

			if (font == null)
				font = SystemFonts.DefaultFont;

			var c = MeasureStringCache.GetOrCreate(text, font, area, format ?? StringFormat.GenericDefault, CreateMeasureStringCacheEntry);
			charactersFitted = c.charactersFitted;
			linesFilled = c.linesFilled;
			return c.measure;
		}

		internal MeasureStringCache.Entry CreateMeasureStringCacheEntry(string text, Font font, SizeF area, StringFormat format)
		{
			if (area.Width > 0 && Math.Abs(area.Height) < float.Epsilon)
				area.Height = float.MaxValue;
			var e = CreateMeasureStringCacheEntryCore(text, font, area, format);
			if (Math.Abs(e.measure.Width) < float.Epsilon && e.measure.Height > 0) // Nothing can fit => measure again with unlimited space.
				e = CreateMeasureStringCacheEntryCore(text, font, new SizeF(float.MaxValue, float.MaxValue), format);
			return e;
		}

		internal MeasureStringCache.Entry CreateMeasureStringCacheEntryCore(string text, Font font, SizeF area, StringFormat format)
		{
			var c = new MeasureStringCache.Entry(text, font, area, format);
			if (String.IsNullOrEmpty(text))
				return c;

			var atts = buildAttributedString(text, font, format);

			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
				area = new SizeF(area.Height, area.Width);

			float lineHeight = font.nativeFont.GetLineHeight();
			var lines = CreateLines(font, atts, area, format, lineHeight);
			foreach (var line in lines)
			{
				if (line != null)
				{
					var lineWidth = (StringFormatFlags.FitBlackBox & format.FormatFlags) != 0
						? line.GetBounds(CTLineBoundsOptions.UseOpticalBounds).Width
						: line.GetTypographicBounds(out _, out _, out _);
					if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
						lineWidth += line.TrailingWhitespaceWidth;
					c.measure.Width = Math.Max(c.measure.Width, (float)NMath.Ceiling((float)lineWidth));
					c.charactersFitted += (int)line.StringRange.Length;
					line.Dispose();
				}
				c.measure.Height += lineHeight;
				c.linesFilled++;
			}

			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
				c.measure = new SizeF(c.measure.Height, c.measure.Width);

			return c;
		}

		public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format = null)
		{
			DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0, 0), format);
		}

		public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format = null)
		{
			DrawString(s, font, brush, new RectangleF(x, y, 0, 0), format);
		}

		private CTLine EllipsisToken(Font font, StringFormat format)
		{
			var attrStr = buildAttributedString("\u2026", font, format, lastBrushColor);
			return new CTLine(attrStr);
		}

		private List<CTLine> CreateLines(Font font, NSAttributedString attributedString, SizeF layoutBox, StringFormat format, float lineHeight)
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
								line = line.GetTruncatedLine(noWrap ? nfloat.MaxValue : layoutBox.Width, CTLineTruncation.End, null);
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

		public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format = null)
		{
#if DEBUG
			++drawStringCount;
			drawStringStopWatch.Start();
			DrawStringInternal(s, font, brush, layoutRectangle, format);
			drawStringStopWatch.Stop();

			if (drawStringCount % 1000 == 0)
			{
				Console.WriteLine($"DrawString1k: {drawStringStopWatch.Elapsed.TotalSeconds}s");
				drawStringStopWatch.Reset();
			}
		}

		static long drawStringCount = 0;
		static Diagnostics.Stopwatch drawStringStopWatch = new Diagnostics.Stopwatch();

		internal void DrawStringInternal(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format = null)
		{
#endif
			if (String.IsNullOrEmpty(s))
				return;

			var c = DrawStringCache.GetOrCreate(s, font, brush, layoutRectangle, format ?? StringFormat.GenericDefault, CreateCacheEntry);
			DrawString(c, layoutRectangle.Location);
		}

		internal DrawStringCache.Entry CreateCacheEntry(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			if (font == null)
				throw new ArgumentNullException (nameof(font));
			if (brush == null)
				throw new ArgumentNullException (nameof(brush));

			layoutRectangle.Location = PointF.Empty;
			var c = new DrawStringCache.Entry(s, font, brush, layoutRectangle, format);

			brush.Setup(this, false); // Stroke
			var attributedString = buildAttributedString(s, font, format, lastBrushColor);

			// Work out the geometry
			c.layoutAvailable = true;
			var insetBounds = layoutRectangle;
			if (insetBounds.Size == SizeF.Empty)
			{
				insetBounds.Width = float.MaxValue;
				insetBounds.Height = float.MaxValue;
				c.layoutAvailable = false;
			}

			c.lineHeight = font.nativeFont.GetLineHeight();
			c.lines = new List<CTLine>();
            c.verticalMatrix = default(CGAffineTransform);

			// Calculate the lines
			// If we are drawing vertical direction then we need to rotate our context transform by 90 degrees
			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) {
				// Swap out the width and height and calculate the lines
				c.lines = CreateLines(font, attributedString, new SizeF(insetBounds.Height, insetBounds.Width), format, c.lineHeight);
				c.boundsWidth = insetBounds.Height;
				c.boundsHeight = insetBounds.Width;
			} else {
				c.lines = CreateLines(font, attributedString, insetBounds.Size, format, c.lineHeight);
				c.boundsWidth = insetBounds.Width;
				c.boundsHeight = insetBounds.Height;
			}

			c.textPosition = new PointF(insetBounds.X + .5f, insetBounds.Y + .5f);
			if (c.layoutAvailable)
			{
				if (format.LineAlignment == StringAlignment.Far)
					c.textPosition.Y += c.boundsHeight - (c.lines.Count * c.lineHeight);
				else if (format.LineAlignment == StringAlignment.Center)
					c.textPosition.Y += (c.boundsHeight - (c.lines.Count * c.lineHeight)) / 2;
			}
			else
			{
                // Precalculate maximum width to allow for aligning lines
                if (format.Alignment != StringAlignment.Near) {
                    float maxLineWidth = 0;
					foreach (var line in c.lines) {
                        if (line != null) {
							var lineWidth = (StringFormatFlags.FitBlackBox & format.FormatFlags) != 0
								? line.GetBounds(CTLineBoundsOptions.UseOpticalBounds).Width
								: line.GetTypographicBounds(out _, out _, out _);
                            if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
                                lineWidth += line.TrailingWhitespaceWidth;
                            maxLineWidth = Math.Max(maxLineWidth, (float)NMath.Ceiling((float)lineWidth));
                        }
                    }
					c.boundsWidth = maxLineWidth;
                }
				if (format.LineAlignment == StringAlignment.Far)
					c.textPosition.Y -= c.lineHeight * c.lines.Count;
				else if (format.LineAlignment == StringAlignment.Center)
					c.textPosition.Y -= (c.lineHeight * c.lines.Count) / 2;
			}
			return c;
		}

		internal void DrawString(DrawStringCache.Entry c, PointF offset)
		{
			// TODO: Consider units

			// First we call the brush with a fill of false so the brush can setup the stroke color
			// that the text will be using.
			// For LinearGradientBrush this will setup a TransparentLayer so the gradient can
			// be filled at the end.  See comments.

			var savedSmoothingMode = SmoothingMode;
			SmoothingMode = SmoothingMode.Default;

			c.brush.Setup(this, false); // Stroke

			if ((c.format.FormatFlags & StringFormatFlags.NoClip) == 0 && c.layoutAvailable)
			{
				context.SaveState();
				if ((c.format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
					context.ClipToRect(new CGRect(offset.X, offset.Y, c.boundsHeight, c.boundsWidth));
				else
					context.ClipToRect(new CGRect(offset.X, offset.Y, c.boundsWidth, c.boundsHeight));
			}

			if ((c.format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
			{
				c.verticalMatrix = CGAffineTransform.MakeTranslation(-offset.X, -offset.Y);
				c.verticalMatrix.Rotate((float)NMath.PI * 0.5f);
				c.verticalMatrix.Translate(offset.X, offset.Y);
				if (c.layoutAvailable)
					c.verticalMatrix.Translate(c.layoutRectangle.Width, 0);
				else
					c.verticalMatrix.Translate((c.lines.Count * c.lineHeight), 0);

				context.ConcatCTM(c.verticalMatrix);
			}

			var textPosition = new PointF(c.textPosition.X + offset.X, c.textPosition.Y + offset.Y); // Let's not change pre-calculated value
			foreach (var line in c.lines)
			{
				// Make sure that the line still exists, it may be null if it's too small to render any text and wants StringTrimming
				if (line != null)
				{
					nfloat ascent = 0;
					double lineWidth = ((StringFormatFlags.FitBlackBox & c.format.FormatFlags) != 0)
						? line.GetBounds(CTLineBoundsOptions.UseOpticalBounds).Width
						: line.GetTypographicBounds(out ascent, out _, out _);
					// Calculate the string format if need be
					nfloat x = textPosition.X;
					if (c.layoutAvailable)
					{
						if (c.format.Alignment == StringAlignment.Far)
							x += (float)line.GetPenOffsetForFlush(1.0f, c.boundsWidth);
						else if (c.format.Alignment == StringAlignment.Center)
							x += (float)line.GetPenOffsetForFlush(0.5f, c.boundsWidth);
						if ((c.format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
						{
							if (c.format.Alignment == StringAlignment.Far)
								x -= (float)line.TrailingWhitespaceWidth;
							else if (c.format.Alignment == StringAlignment.Center)
								x -= (float)line.TrailingWhitespaceWidth * 0.5f;
						}
					}
					else
					{
						// We were only passed in a point so we need to format based on the point.
						if ((c.format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
							lineWidth += line.TrailingWhitespaceWidth;
						if (c.format.Alignment == StringAlignment.Far)
							x -= (float)lineWidth;
						else if (c.format.Alignment == StringAlignment.Center)
							x -= (float)lineWidth / 2.0f;
					}

                    // initialize our Text Matrix or we could get trash in here
                    context.TextMatrix = new CGAffineTransform(1, 0, 0, -1, x, textPosition.Y + c.font.nativeFont.AscentMetric);
					line.Draw (context);
					//line.Dispose ();

					// Currently we support strikethrough only at the font level (for a whole text, not for ranges). It seems CoreText does not handle it for us.
					if (c.font.Strikeout)
					{
						var sy = textPosition.Y + ascent - c.font.nativeFont.XHeightMetric / (nfloat)2.0;
						context.MoveTo(x, sy);
						context.AddLineToPoint(x + (nfloat)lineWidth, sy);
						context.SetStrokeColor(lastBrushColor);
						context.SetLineWidth(1);
						context.StrokePath();
					}
				}

				// Move the index beyond the line break.
				textPosition.Y += c.lineHeight;
			}

			if ((c.format.FormatFlags & StringFormatFlags.NoClip) == 0 && c.layoutAvailable)
				context.RestoreState();
			else if ((c.format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
                context.ConcatCTM(c.verticalMatrix.Invert());

			// Now we call the brush with a fill of true so the brush can do the fill if need be 
			// For LinearGradientBrush this will draw the Gradient and end the TransparentLayer.
			// See comments.
			c.brush.Setup(this, true); // Fill

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


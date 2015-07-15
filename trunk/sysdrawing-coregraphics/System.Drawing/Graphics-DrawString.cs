using System;

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
			return MeasureString (textg, font, new RectangleF (0, 0, width, Int32.MaxValue));
		}

		public SizeF MeasureString (string textg, Font font, SizeF layoutArea)
		{
			return MeasureString (textg, font, new RectangleF (new PointF (0, 0), layoutArea));
		}

		public SizeF MeasureString (string text, Font font, PointF point, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		public SizeF MeasureString (string textg, Font font, RectangleF rect)
		{

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

			var atts = buildAttributedString(textg, font);

			var measure = SizeF.Empty;
			// Calculate the lines
			int start = 0;
			int length = atts.Length;

			var typesetter = new CTTypesetter(atts);

			while (start < length) {
				int count = typesetter.SuggestLineBreak (start, 8388608);
				var line = typesetter.GetLine (new NSRange(start, count));

				// Create and initialize some values from the bounds.
				float ascent;
				float descent;
				float leading;
				var lineWidth = line.GetTypographicBounds (out ascent, out descent, out leading);

				measure.Height += (float)Math.Ceiling (ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
				if (lineWidth > measure.Width)
					measure.Width = (float)lineWidth;

				line.Dispose ();
				start += count;
			}

			return measure;
		}

		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		public SizeF MeasureString (string text, Font font, int width, StringFormat format)
		{
			throw new NotImplementedException ();
		}

		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, 
		                            out int charactersFitted, out int linesFilled)
		{	
			charactersFitted = 0;
			linesFilled = 0;

			if ((text == null) || (text.Length == 0))
				return SizeF.Empty;

			if (font == null)
				throw new ArgumentNullException ("font");

			throw new NotImplementedException ();
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

			var attributedString = buildAttributedString(s, font, format, lastBrushColor);

			// Work out the geometry
			RectangleF insetBounds = layoutRectangle;
			if (insetBounds.Size == SizeF.Empty)
			{
				insetBounds.Width = boundingBox.Width;
				insetBounds.Height = boundingBox.Height;
				layoutAvailable = false;

				if (format.LineAlignment != StringAlignment.Near) 
				{
					insetBounds.Size = MeasureString (s, font);
				}


			}

			PointF textPosition = new PointF(insetBounds.X,
			                                 insetBounds.Y);

			float boundsWidth = insetBounds.Width;

			// Calculate the lines
			int start = 0;
			int length = attributedString.Length;
			float baselineOffset = 0;

			var typesetter = new CTTypesetter(attributedString);

			// First we need to calculate the offset for Vertical Alignment if we 
			// are using anything but Top
			if (layoutAvailable && format.LineAlignment != StringAlignment.Near) {
				while (start < length) {
					int count = typesetter.SuggestLineBreak (start, boundsWidth);
					var line = typesetter.GetLine (new NSRange(start, count));

					// Create and initialize some values from the bounds.
					float ascent;
					float descent;
					float leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);
					baselineOffset += (float)Math.Ceiling (ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
					line.Dispose ();
					start += count;
				}

				start = 0;
			}

			// If we are drawing vertial direction then we need to rotate our context transform by 90 degrees
			if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical) 
			{
				//textMatrix.Rotate (ConversionHelpers.DegreesToRadians (90));
				var verticalOffset = 0.0f;
				while (start < length) {
					int count = typesetter.SuggestLineBreak (start, boundsWidth);
					var line = typesetter.GetLine (new NSRange(start, count));

					// Create and initialize some values from the bounds.
					float ascent;
					float descent;
					float leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);
					verticalOffset += (float)Math.Ceiling (ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
					line.Dispose ();
					start += count;
				}
				context.TranslateCTM (layoutRectangle.X, layoutRectangle.Y);
				context.RotateCTM (ConversionHelpers.DegreesToRadians (90));
				context.TranslateCTM (-layoutRectangle.X, -layoutRectangle.Y);
				context.TranslateCTM (0, -verticalOffset);
				start = 0;
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
				float ascent;
				float descent;
				float leading;
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
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y + ((insetBounds.Height / 2) - (baselineOffset / 2)));
					else
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y - ((insetBounds.Height / 2) - (baselineOffset / 2)));
				}

				if (format.LineAlignment == StringAlignment.Far) 
				{
					if (layoutAvailable)
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y + ((insetBounds.Height) - (baselineOffset)));
					else
						textMatrix.Translate (penFlushness + textPosition.X, textPosition.Y - ((insetBounds.Height) - (baselineOffset)));
				}

				context.TextMatrix = textMatrix;

				// and draw the line
				line.Draw(context);

				// Move the index beyond the line break.
				start += count;
				textPosition.Y += (float)Math.Ceiling(ascent + descent + leading + 1); // +1 matches best to CTFramesetter's behavior  
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
#if MONOMAC
				int single = (int)MonoMac.AppKit.NSUnderlineStyle.Single;
				int solid = (int)MonoMac.AppKit.NSUnderlinePattern.Solid;
				var attss = single | solid;
				ctAttributes.UnderlineStyleValue = attss;
#else
				ctAttributes.UnderlineStyleValue = 1;
#endif
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

			NSMutableAttributedString atts = 
				new NSMutableAttributedString(text,ctAttributes.Dictionary);

			return atts;

		}


	}
}


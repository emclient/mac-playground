using System.Text;
using System.Diagnostics;
using System.Collections;
#if __MACOS__
using AppKit;
#elif __IOS__
using UIKit;
#endif
using CoreGraphics;
using CoreText;
using Foundation;
using System.Drawing.Printing;

namespace System.Drawing.Mac
{
	public static class Extensions
	{
		public static int ToInt(this nfloat f)
		{
			return (int)(f >= int.MaxValue ? int.MaxValue : f <= int.MinValue ? int.MinValue : Math.Round(f));
		}

		public static CGRect ToCGRect(this Rectangle r)
		{
			return new CGRect(r.X, r.Y, r.Width, r.Height);
		}

		public static CGRect ToCGRect(this RectangleF r)
		{
			return new CGRect(r.X, r.Y, r.Width, r.Height);
		}

		public static Rectangle ToRectangle(this CGRect r)
		{
			return new Rectangle(r.X.ToInt(), r.Y.ToInt(), r.Width.ToInt(), r.Height.ToInt());
		}

		public static CGRect Flipped(this CGRect r, nfloat height)
		{
			return new CGRect(r.X, height - r.Y, r.Width, r.Height);
		}

		public static CGRect Inflate(this CGRect r, float w, float h)
		{
			return new CGRect(r.X - w, r.Y - h, r.Width + w + w, r.Height + h + h);
		}

		public static CGRect Move(this CGRect r, nfloat dx, nfloat dy)
		{
			return new CGRect(r.X + dx, r.Y + dy, r.Width, r.Height);
		}

		public static CGRect Move(this CGRect r, float dx, float dy)
		{
			return new CGRect(r.X + dx, r.Y + dy, r.Width, r.Height);
		}

		public static Rectangle Move(this Rectangle r, int x, int y)
		{
			return new Rectangle(r.X + x, r.Y + y, r.Width, r.Height);
		}

		public static CGPoint Move(this CGPoint p, nfloat x, nfloat y)
		{
			return new CGPoint(p.X + x, p.Y + y);
		}

		public static CGRect CenterIn(this CGRect self, CGRect other)
		{
			return new CGRect(other.Left + (other.Width - self.Width) / 2f, other.Top + (other.Height - self.Height) /2f, self.Width, self.Height);
		}

		public static CGSize ToCGSize(this Size s)
		{
			return new CGSize(s.Width, s.Height);
		}

		public static Size ToSDSize(this CGSize s)
		{
			return new Size(s.Width.ToInt(), s.Height.ToInt());
		}

		public static Point ToSDPoint(this CGPoint p)
		{
			return new Point(p.X.ToInt(), p.Y.ToInt());
		}

		public static CGPoint Flipped(this CGPoint p, nfloat height)
		{
			return new CGPoint(p.X, height - p.Y);
		}

		public static CGSize Inflate(this CGSize s, float w, float h)
		{
			return new CGSize(s.Width + w, s.Height + h);
		}

		public static CTFont ToCTFont(this Font f)
		{
			return f.nativeFont;
		}

		public static float GetLineHeight(this CTFont font)
		{
			// https://stackoverflow.com/questions/5511830/how-does-line-spacing-work-in-core-text-and-why-is-it-different-from-nslayoutm
			//var ascent = (double)font.AscentMetric;
			//var descent = (double)font.DescentMetric;
			//var leading = (double)font.LeadingMetric;
			//leading = leading < 0 ? 0.0 : Math.Floor(leading + 0.5);
			//var lineHeight = Math.Floor(ascent + 0.5) + Math.Floor(descent + 0.5) + leading;
			//var ascenderDelta = leading > 0 ? 0.0 : Math.Floor(0.2 * lineHeight + 0.5);
			//return (float)(lineHeight + ascenderDelta);

			return (float)NMath.Ceiling(font.AscentMetric + font.DescentMetric + font.LeadingMetric + 1);
		}

		public static CTTextAlignment ToCTTextAlignment(this ContentAlignment a)
		{
			switch (a)
			{
				default:
					return CTTextAlignment.Left;

				case ContentAlignment.TopLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.BottomLeft:
					return CTTextAlignment.Left;

				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					return CTTextAlignment.Right;

				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					return CTTextAlignment.Center;
			}
		}

		public static bool IsBold(this FontStyle style)
		{
			return 0 != (style & FontStyle.Bold);
		}

		public static bool IsItalic(this FontStyle style)
		{
			return 0 != (style & FontStyle.Italic);
		}

		public static bool IsBold(this CTFontSymbolicTraits traits)
		{
			return 0 != (traits & CTFontSymbolicTraits.Bold);
		}

		public static bool IsItalic(this CTFontSymbolicTraits traits)
		{
			return 0 != (traits & CTFontSymbolicTraits.Italic);
		}

#if __MACOS__
		public static CGSize DeviceDPI(this NSScreen screen)
		{
			var desc = screen.DeviceDescription;
			if (desc != null)
				if (desc["NSDeviceResolution"] is NSValue value)
					return value.CGSizeValue;

			Debug.Assert(false, $"Failed to get screen resolution for '{screen.LocalizedName}'");
			return new CGSize(72, 72);
		}

		public static NSFont ToNSFont(this Font f)
		{
			return ObjCRuntime.Runtime.GetNSObject(f.nativeFont.Handle) as NSFont;
		}

		public static CTFont ToCTFont(this NSFont f)
		{
			// CTFont and NSFont are toll-free bridged
			return (CTFont)Activator.CreateInstance(
				typeof(CTFont),
				Reflection.BindingFlags.NonPublic | Reflection.BindingFlags.Instance,
				null,
				new object[] { f.Handle },
				null);
		}

		public static NSTextAlignment ToNSTextAlignment(this ContentAlignment a)
		{
			return (NSTextAlignment)ToCTTextAlignment(a);
		}

		public static NSImage ToNSImage(this Image image)
		{
			if (image.NativeCGImage != null)
				return new NSImage(image.NativeCGImage, CGSize.Empty);

			return new NSImage(image.ToNSData(Imaging.ImageFormat.Png));
		}

		public static CGContext CGContext(this NSGraphicsContext context)
		{
			return context.CGContext;
		}

		public static Bitmap ToBitmap(this NSImage self)
		{
			return self.ToCGImage()?.ToBitmap();
		}

		public static CGImage ToCGImage(this NSImage self)
		{
			var image = self?.CGImage;
			if (image == null)
			{
				var rect = new CGRect(CGPoint.Empty, self.Size);
				image = self.AsCGImage(ref rect, null, null);
			}
			return image;
		}

		public static NSAttributedString GetAttributedString(this string s, char? hotKey = '&', Font font = null, ContentAlignment? alignment = null)
		{
			var attributes = new NSMutableDictionary();

			if (font != null)
			{
				var uiFont = ObjCRuntime.Runtime.GetNSObject(font.nativeFont.Handle) as NSFont;
				attributes.Add(NSStringAttributeKey.Font, uiFont);
			}

			if (alignment.HasValue)
			{
				var style = new NSMutableParagraphStyle();
				style.Alignment = alignment.Value.ToNSTextAlignment();

				attributes.Add(NSStringAttributeKey.ParagraphStyle, style);
			}

			int hotKeyIndex = -1;
			if (hotKey.HasValue)
			{
				hotKeyIndex = s.IndexOf(hotKey.Value);
				if (hotKeyIndex != -1)
					s = s.Substring(0, hotKeyIndex) + s.Substring(1 + hotKeyIndex);
			}

			var astr = new NSMutableAttributedString(s, attributes);
			if (hotKeyIndex != -1 && astr.Length > hotKeyIndex)
				astr.AddAttribute(NSStringAttributeKey.UnderlineStyle, new NSNumber((int)NSUnderlineStyle.Single), new NSRange(hotKeyIndex, 1));

			return astr;
		}
#endif

		public static NSData ToNSData(this Image image, Imaging.ImageFormat format)
		{
			using (var stream = new IO.MemoryStream())
			{
				image.Save(stream, format);
				return NSData.FromArray(stream.ToArray());
			}
		}

		public static Bitmap ToBitmap(this CGImage cgImage)
		{
			return new Bitmap(cgImage);
		}

		public static CGBitmapContext ToCGBitmapContext(this Bitmap bitmap)
		{
			return bitmap.GetRenderableContext();
		}
		
		public static CGImage ToCGImage(this Image image)
		{
			return image.NativeCGImage;
		}

		public static IList GetRepresentations(this Image image)
		{
			return image.representations;
		}

		public static bool ContainsRepresentation(this Image image, Size size)
		{
			return image.GetRepresentation(size).Size == size;
		}

		public static Image GetRepresentation(this Image image, Size size)
		{
			var best = image;
			if (image.representations != null)
				foreach (Image rep in image.representations)
					if (rep.Size == size)
						return rep;
					else if (Math.Abs(rep.Height - size.Height) < Math.Abs(best.Height - size.Height))
						best = rep;
			return best;
		}

		public static void AddRepresentation(this Image image, Image rep)
		{
			if (image.representations == null)
				image.representations = new ArrayList();

			int i = image.IndexOfRepresentation(rep.Size);
			if (i >= 0)
				image.representations[i] = rep;
			else
				image.representations.Add(rep);
		}

		public static int IndexOfRepresentation(this Image image, Size size)
		{
			if (image.representations != null)
				for (int i = 0; i < image.representations.Count; ++i)
					if (((Image)image.representations[i]).Size == size)
						return i;
			return -1;
		}

		public static CGContext ToCGContext(this Graphics graphics)
		{
			return graphics.context;
		}

#if __MACOS__
		public static PageSettings ToPageSettings(this NSPrintInfo printInfo)
		{
			return new PageSettings(printInfo);
		}
#endif
	}
}

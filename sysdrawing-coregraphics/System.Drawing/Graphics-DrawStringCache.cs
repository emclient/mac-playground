using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using CoreGraphics;
using CoreText;
using Foundation;

namespace System.Drawing
{
	internal class DrawStringCache
	{
		public class Entry
		{
			public string s;
			public Font font;
			public Brush brush;
			public RectangleF layoutRectangle;
			public StringFormat format;

			public bool layoutAvailable;
			public RectangleF insetBounds;
			public float boundsWidth;
			public float boundsHeight;
			public float lineHeight;
			public List<CTLine> lines;
			public CGAffineTransform verticalMatrix;
			public NSAttributedString attributedString;
			public PointF textPosition;

			public Entry(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
			{
				this.s = s;
				this.font = font;
				this.brush = brush;
				this.layoutRectangle = layoutRectangle;
				this.format = format;
			}

			public bool ConformsTo(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
			{
				return
					this.s.Equals(s)
						&& this.font.Equals(font)
						&& this.brush.Equals(brush)
						&& this.layoutRectangle.Size.Equals(layoutRectangle.Size)
						&& this.format.Equals(format);
			}
		}

		public delegate DrawStringCache.Entry CreateDrawStringCacheEntryDelegate(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format);

		protected LurchTable<string, Entry> lurch;
		protected bool enabled;

#if DEBUG
		int total = 0;
		int miss = 0;
#endif

		public DrawStringCache(int capacity, bool enabled = true)
		{
			this.enabled = enabled;
			lurch = new LurchTable<string, Entry>(capacity);
		}

		public string GetKey(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			var fnt = $"{font.Name }|{font.Size}|{(font.Italic ? '1' : '0')}{(font.Underline ? '1' : '0')}{(font.Strikeout ? '1' : '0')}";
			var bsh = "";
			if (brush is SolidBrush sb)
				bsh = $"SB{ToString(sb.Color)}";
			else if (brush is HatchBrush hb)
				bsh = $"HB{hb.BackgroundColor}|{ToString(hb.ForegroundColor)}|{ToString(hb.BackgroundColor)}";
			else if (brush is TextureBrush tb)
				bsh = $"TB{tb.Image.Size.Width:X2}x{tb.Image.Size.Width:X2}|{tb.Image.GetHashCode():X2}";
			else if (brush is PathGradientBrush pgb)
				bsh = $"PGB{pgb.GetHashCode():X2}";
			else if (brush is LinearGradientBrush lgb)
				bsh = $"LGB{lgb.LinearColors.Length:X2}{lgb.GetHashCode():X2}";

			var fmt = $"{format.FormatFlags}|{format.HotkeyPrefix}|{format.Alignment}|{format.LineAlignment}|{format.Trimming}";
			return $"{s}|{layoutRectangle.Width},{layoutRectangle.Height}|{fnt}|{format}|{bsh}";
		}

		internal string ToString(Color c)
		{
			return $"{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
		}

		public Entry GetOrCreate(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format, CreateDrawStringCacheEntryDelegate createEntryDelegate)
		{
#if DEBUG
			if (total % 1000 == 0)
			{
				Console.WriteLine($"hit:{total - miss}, miss:{miss}");
				total = miss = 0;
			}

			++total;
#endif

			if (!enabled)
			{
				++miss;
				return createEntryDelegate(s, font, brush, layoutRectangle, format);
			}
			
			var key = GetKey(s, font, brush, layoutRectangle, format);
			if (lurch.TryGetValue(key, out Entry c))
				if (c.ConformsTo(s, font, brush, layoutRectangle, format))
					return c;

#if DEBUG
			++miss;
#endif
			return lurch[key] = createEntryDelegate(s, font, brush, layoutRectangle, format);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Foundation;
using CoreGraphics;
using CoreText;

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

			// Outputs
			public bool layoutAvailable;
			public float boundsWidth;
			public float boundsHeight;
			public float lineHeight;
			public List<CTLine> lines;
			public CGAffineTransform verticalMatrix;
			public PointF textPosition;

			public Entry(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
			{
				this.s = s;
				this.font = (Font)font.Clone(); // An owner could possibly call Dispose()!
				this.brush = (Brush)brush.Clone();
				this.layoutRectangle = layoutRectangle;
				this.format = (StringFormat)format.Clone();
			}

			public bool ConformsTo(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
			{
				return
					Equals(this.s, s)
						&& this.font.Equals(font)
						&& this.brush.Equals(brush)
						&& this.layoutRectangle.Size.Equals(layoutRectangle.Size)
						&& this.format.Equals(format);
			}
		}

		public delegate Entry CreateEntryDelegate(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format);

		protected LruCache<string, Entry> lurch;
		protected bool enabled;

#if DEBUG
		int total = 0;
		int miss = 0;
#endif

#if __MACOS__
		List<NSObject> observers;

		List<NSObject> AddObservers()
		{
			var observers = new List<NSObject>();
			observers.Add(NSNotificationCenter.DefaultCenter.AddObserver(new NSString("EffectiveAppearanceDidChange"), (_) => Clear()));
			observers.Add(NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSSystemColorsDidChangeNotification"), (_) => Clear()));
			observers.Add(NSDistributedNotificationCenter.DefaultCenter.AddObserver(new NSString("AppleInterfaceThemeChangedNotification"), (_) => Clear()));
			return observers;
		}
#else
		static List<NSObject> AddObservers()
		{
			return new List<NSObject>();
		}
#endif

		public DrawStringCache(int capacity, bool enabled = true)
		{
			this.enabled = enabled;
			lurch = new LruCache<string, Entry>(capacity);
			AddObservers();
		}

		public void Clear()
		{
			lurch.Clear();
		}

		public string GetKey(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			font ??= SystemFonts.DefaultFont;
			var fnt = $"{font.Name}|{font.Size}|{font.Italic.ToStr()}{font.Underline.ToStr()}{font.Strikeout.ToStr()}";
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

			var fmt = $"{(int)format.FormatFlags}|{(int)format.HotkeyPrefix}|{(int)format.Alignment}|{(int)format.LineAlignment}|{(int)format.Trimming}";
			return $"{s}|{layoutRectangle.Width.ToStr()},{layoutRectangle.Height.ToStr()}|{fnt}|{fmt}|{bsh}";
		}

		internal string ToString(Color c)
		{
			return $"{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
		}

		public Entry GetOrCreate(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format, CreateEntryDelegate createEntryDelegate)
		{
			if (!enabled)
				return createEntryDelegate(s, font, brush, layoutRectangle, format);
			
#if DEBUG
			if (total % 1000 == 0)
			{
				Console.WriteLine($"draw cache hit:{total - miss}, miss:{miss}, size:{lurch.Count}");
				total = miss = 0;
			}

			++total;
#endif

			var key = GetKey(s, font, brush, layoutRectangle, format);
			if (lurch.TryGetValue(key, out Entry c))
				if (c.ConformsTo(s, font, brush, layoutRectangle, format))
					return c;

#if DEBUG
			++miss;
#endif

			var entry = createEntryDelegate(s, font, brush, layoutRectangle, format);
			lurch.Set(key, entry);
			return entry;
		}
	}

	internal class MeasureStringCache
	{
		public class Entry
		{
			public string text;
			public Font font;
			public SizeF layoutArea;
			public StringFormat format;

			// Outputs
			public int charactersFitted = 0;
			public int linesFilled = 0;
			public SizeF measure = SizeF.Empty;

			public Entry(string text, Font font, SizeF layoutArea, StringFormat format)
			{
				this.text = text;
				this.font = font ?? SystemFonts.DefaultFont;
				this.layoutArea = layoutArea;
				this.format = format;
			}

			public bool ConformsTo(string text, Font font, SizeF layoutArea, StringFormat format)
			{
				return
					Equals(this.text, text)
						&& this.font.Equals(font ?? SystemFonts.DefaultFont)
						&& this.layoutArea.Equals(layoutArea)
						&& this.format.Equals(format);
			}
		}

		public delegate Entry CreateEntryDelegate(string s, Font font, SizeF layoutArea, StringFormat format);

		protected LruCache<string, Entry> lurch;
		protected bool enabled;

#if DEBUG
		int total = 0;
		int miss = 0;
#endif

		public MeasureStringCache(int capacity, bool enabled = true)
		{
			this.enabled = enabled;
			lurch = new LruCache<string, Entry>(capacity);
		}

		public string GetKey(string s, Font font, SizeF layoutArea, StringFormat format)
		{
			font ??= SystemFonts.DefaultFont;
			return s
				+ "|" + layoutArea.Width.ToStr() + "|" + layoutArea.Height.ToStr()
				+ "|" + font.Name + "|" + font.Size.ToString() + "|" + font.Italic.ToStr() + font.Underline.ToStr() + font.Strikeout.ToStr()
				+ "|" + ((int)format.FormatFlags).ToString() + "|" + ((int)format.HotkeyPrefix).ToString() + "|" + ((int)format.Alignment).ToString() + "|" + ((int)format.LineAlignment).ToString() + "|" + ((int)format.Trimming).ToString();
		}

		internal Entry GetOrCreate(string text, Font font, SizeF layoutArea, StringFormat format, CreateEntryDelegate createEntryDelegate)
		{
			if (!enabled)
				return createEntryDelegate(text, font, layoutArea, format);

#if DEBUG
			if (total % 1000 == 0)
			{
				Console.WriteLine($"measure cache hit:{total - miss}, miss:{miss}, size:{lurch.Count}");
				total = miss = 0;
			}

			++total;
#endif

			var key = GetKey(text, font, layoutArea, format);
			if (lurch.TryGetValue(key, out Entry c))
				if (c.ConformsTo(text, font, layoutArea, format))
					return c;

#if DEBUG
			++miss;
#endif
			var entry = createEntryDelegate(text, font, layoutArea, format);
			lurch.Set(key, entry);
			return entry;
		}
	}

	static class Extensions {
		const float big = 1e07f;
		public static string ToStr(this float v) => v > big ? "-" : v.ToString();
		public static string ToStr(this bool v) => v ? "1" : "0";
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using MailClient.Collections;

namespace MailClient.Common.UI
{
	public static class FontCache
	{
		private class SemiboldFontCacheEntry
		{
			private string fontName;
			private float emSize;

			public SemiboldFontCacheEntry(string name, float emSize)
			{
				fontName = name;
				this.emSize = emSize;
			}

			public override bool Equals(object obj)
			{
				SemiboldFontCacheEntry cacheEntry = obj as SemiboldFontCacheEntry;
				if (cacheEntry != null)
				{
					return cacheEntry.emSize == this.emSize && cacheEntry.fontName == this.fontName;
				}
				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return fontName.GetHashCode() ^ emSize.GetHashCode();
			}
		}

		private class FontCacheInternal
		{
			private Font baseFont;
			private float emSize;
			private FontStyle style;
			private FontFamily family;
			private GraphicsUnit unit = GraphicsUnit.Display;

			public FontCacheInternal(Font baseFont, FontStyle style) :
				this(baseFont, baseFont.SizeInPoints, style)
			{
			}

			public FontCacheInternal(Font baseFont, float emSize, FontStyle style)
			{
				this.baseFont = baseFont;
				this.emSize = emSize;
				this.style = style;
			}

			public FontCacheInternal(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			{
				this.family = family;
				this.emSize = emSize;
				this.style = style;
				this.unit = unit;
			}

			public override bool Equals(object obj)
			{
				FontCacheInternal cacheEntry = obj as FontCacheInternal;
				if (cacheEntry != null)
				{
					if (this.baseFont != null)
						return cacheEntry.emSize == this.emSize && this.baseFont.Equals(cacheEntry.baseFont) && this.style == cacheEntry.style && this.unit == cacheEntry.unit;
					else if (this.family != null)
						return cacheEntry.emSize == this.emSize && this.family.Equals(cacheEntry.family) && this.style == cacheEntry.style && this.unit == cacheEntry.unit;

					return false;
				}

				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				if (this.baseFont != null)
					return this.baseFont.GetHashCode() ^ this.emSize.GetHashCode() ^ this.style.GetHashCode();
				else if (this.family != null)
					return this.family.GetHashCode() ^ this.emSize.GetHashCode() ^ this.style.GetHashCode();

				return base.GetHashCode();
			}
		}

		//private static WeakDictionary<SemiboldFontCacheEntry, Font> semiboldCache = new WeakDictionary<SemiboldFontCacheEntry, Font>(16);
		//private static WeakDictionary<FontCacheInternal, Font> fontCache = new WeakDictionary<FontCacheInternal, Font>(64);

		private static Dictionary<SemiboldFontCacheEntry, Font> semiboldCache = new Dictionary<SemiboldFontCacheEntry, Font>(16);
		private static Dictionary<FontCacheInternal, Font> fontCache = new Dictionary<FontCacheInternal, Font>(64);

		public static Font CreateFont(Font baseFont, FontStyle style)
		{
			FontCacheInternal key = new FontCacheInternal(baseFont, style);
			Font outFont;
			if (fontCache.TryGetValue(key, out outFont))
			{
				return outFont;
			}

			try
			{
				Font f = new Font(baseFont, style);
				fontCache.Add(key, f);
				return f;
			}
			catch (ArgumentException)
			{
				return (Font)baseFont.Clone();
			}
		}

		public static Font CreateFont(Font baseFont, FontStyleEx style)
		{
			try
			{
				if (style == FontStyleEx.Semibold)
				{
					string semiboldFont;
					if (SemiboldFontExists(baseFont.Name, out semiboldFont))
						return CreateFont(semiboldFont, baseFont.Size);
					else
						return (Font)baseFont.Clone();
				}
				else
					return CreateFont(baseFont, (FontStyle)style);
			}
			catch (ArgumentException)
			{
				return (Font)baseFont.Clone();
			}
		}

		public static Font CreateFont(string fontFamily, float emSize)
		{
			return CreateFont(fontFamily, emSize, FontStyle.Regular);
		}

		public static Font CreateFont(string fontFamily, float emSize, FontStyle style)
		{
			try
			{
				FontFamily ff = new FontFamily(fontFamily);
				
				int oldCacheCount = fontCache.Count;
				Font font = CreateFont(ff, emSize, style);

				// if we don't use the fontFamily as a key into the cache dictionary, we can dispose it
				if (fontCache.Count == oldCacheCount)
					ff.Dispose();

				return font;
			}
			catch (ArgumentException)
			{
				try
				{
					return new Font(SystemFonts.MessageBoxFont.FontFamily, emSize, style);
				}
				catch (ArgumentException)
				{
					return SystemFonts.MessageBoxFont;
				}
			}
		}

		public static Font CreateFont(FontFamily fontFamily, float emSize)
		{
			return CreateFont(fontFamily, emSize, FontStyle.Regular);
		}

		public static Font CreateFont(FontFamily fontFamily, float emSize, FontStyle style, GraphicsUnit unit = GraphicsUnit.Point)
		{
			if (fontFamily.IsStyleAvailable(style))
			{
				FontCacheInternal key = new FontCacheInternal(fontFamily, emSize, style, unit);
				Font outFont;
				if (fontCache.TryGetValue(key, out outFont))
				{
					return outFont;
				}

				Font f = new Font(fontFamily, emSize, style, unit);
				fontCache.Add(key, f);
				return f;
			}

			return SystemFonts.MessageBoxFont;
		}

		public static Font CreateFontFromString(string fontString)
		{
			try
			{
				if (fontString.Contains(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator))
					return (Font)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font)).ConvertFromString(fontString);
				else
				{
					if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator == ",")
						fontString = fontString.Replace(';', ',');
					else
						fontString = fontString.Replace(',', ';');

					return (Font)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font)).ConvertFromString(fontString);
				}
			}
			catch (ArgumentException)
			{
				//try to parse the string manually
				fontString = fontString.Trim();

				string fontFamily;
				float fontSize;
				FontStyle style = FontStyle.Regular;

				int index = fontString.IndexOf(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
				if (index > 0)
				{
					fontFamily = fontString.Substring(0, index);
					string fontStringEnd = fontString.Substring(index + 1).TrimStart().ToLower();

					index = fontStringEnd.IndexOf(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);

					string sizeString = index > 0 ? fontStringEnd.Substring(0, index) : fontStringEnd;
					if (!float.TryParse(sizeString.TrimEnd('p', 't', ' '), out fontSize))
						fontSize = SystemFonts.MessageBoxFont.Size;

					if (fontStringEnd.Contains("italic"))
						style |= FontStyle.Italic;

					if (fontStringEnd.Contains("bold"))
						style |= FontStyle.Bold;
				}
				else
				{
					fontFamily = fontString;
					fontSize = SystemFonts.MessageBoxFont.Size;
				}

				return (Font)CreateFont(fontFamily, fontSize, style).Clone();
			}
		}

		public static bool SemiboldFontExists(string fontName, out string semiboldFontName)
		{
			semiboldFontName = "";

			try
			{
				using (FontFamily family = new FontFamily(fontName + " Semibold"))
				{
					semiboldFontName = family.Name;
					return true;
				}
			}
			catch (Exception)
			{ }

			return false;
		}

		public static Font TryCreateSemiboldFont(Font font)
		{
			return TryCreateSemiboldFont(font.Name, font.SizeInPoints);
		}

		public static Font TryCreateSemiboldFont(string fontName, float emSize)
		{
			SemiboldFontCacheEntry key = new SemiboldFontCacheEntry(fontName, emSize);
			Font outFont;
			if (semiboldCache.TryGetValue(key, out outFont))
			{
				return outFont;
			}

			try
			{
				FontFamily family = new FontFamily(fontName + " Semibold");
				Font f = new Font(family, emSize);
				semiboldCache.Add(key, f);
				return f;
			}
			catch (Exception)
			{ }

			return CreateFont(fontName, emSize, FontStyle.Bold);
		}
	}
}

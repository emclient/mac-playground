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

namespace System.Drawing.Mac
{
	public static class ColorExtensions
	{
#if __MACOS__
        static Color textColor;
        static Color textBackgroundColor;

        static ColorExtensions()
        {
            var colorConstructor = typeof(System.Drawing.Color).GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new System.Type[] { typeof(long), typeof(short), typeof(string), typeof(System.Drawing.KnownColor) },
                null);
            textColor = (Color)colorConstructor.Invoke(new object[] {
                NSColor.Text.ToArgb(),
                (short)10,
                "textColor",
                (KnownColor)0});
            textBackgroundColor = (Color)colorConstructor.Invoke(new object[] {
                NSColor.TextBackground.ToArgb(),
                (short)10,
                "textBackgroundColor",
                (KnownColor)0});
        }

		public static CGColor ToCGColor(this Color c)
		{
			return ToNSColor(c).CGColor;
		}

		public static NSColor ToNSColor(this Color c)
		{
			if (c.IsSystemColor) {
				switch (c.ToKnownColor()) {
					case KnownColor.ActiveBorder: return NSColor.WindowFrame;
					case KnownColor.ActiveCaption: return NSColor.Grid;
					case KnownColor.ActiveCaptionText: return NSColor.HeaderText;
					// KnownColor.AppWorkspace
					case KnownColor.Control: return NSColor.Control;
					case KnownColor.ControlText: return NSColor.ControlText;
					case KnownColor.ControlDark: return NSColor.ControlShadow;
					case KnownColor.ControlDarkDark: return NSColor.ControlDarkShadow;
					case KnownColor.ControlLight: return NSColor.ControlHighlight;
					case KnownColor.ControlLightLight: return NSColor.ControlBackground;
					// KnownColor.Desktop
					case KnownColor.GrayText: return NSColor.DisabledControlText;
					case KnownColor.Highlight: return NSColor.SelectedTextBackground;
					case KnownColor.HighlightText: return NSColor.SelectedText;
					// KnownColor.HotTrack
					// KnownColor.InactiveBorder
					// KnownColor.InactiveCaption
					// KnownColor.InactiveCaptionText
					// KnownColor.Info
					// KnownColor.InfoText
					// KnownColor.Menu
					// KnownColor.MenuText
					case KnownColor.ScrollBar: return NSColor.ScrollBar;
					case KnownColor.Window: return NSColor.WindowBackground;
					case KnownColor.WindowText: return NSColor.WindowFrameText;
					case KnownColor.WindowFrame: return NSColor.WindowFrame;
					case KnownColor.ButtonFace: return NSColor.Control;
					case KnownColor.ButtonHighlight: return NSColor.ControlHighlight;
					case KnownColor.ButtonShadow: return NSColor.ControlShadow;
				}
			}

			if (c.IsNamedColor) {
                if (c.Name == textBackgroundColor.Name) {
				    return NSColor.TextBackground;
                }
			}

			return ToNSColor(c.ToArgb());
		}

		public static NSColor ToNSColor(this int value)
		{
			value.ToArgb(out int a, out int r, out int g, out int b);
			return NSColor.FromDeviceRgba(r, g, b, a);
		}

		// The following methods are here because equivalent Xamarin stubs have redundant EnsureUIThread assertions that make them unusable for us.
		// NOTE: Fixed with https://github.com/xamarin/xamarin-macios/commit/0fb7498209cf988f0d60d5a1e8dcd060951f1549, kept for compatibility
		public static NSColorSpace GenericRgbColorSpace() => NSColorSpace.GenericRGBColorSpace;
		public static NSColorSpace SRGBColorSpace() => NSColorSpace.SRGBColorSpace;

		public static int ToArgb(this NSColor color)
		{
			color.ToArgb(out int a, out int r, out int g, out int b);
			return (int)((uint)a << 24) + (r << 16) + (g << 8) + b;
		}

		public static bool ToArgb(this NSColor color, out int a, out int r, out int g, out int b)
		{
			var result = ToArgb(color, out float af, out float rf, out float gf, out float bf);
			a = (int)Math.Round(af * 255);
			r = (int)Math.Round(rf * 255);
			g = (int)Math.Round(gf * 255);
			b = (int)Math.Round(bf * 255);
			return result;
		}

		public static bool ToArgb(this NSColor color, out float a, out float r, out float g, out float b)
		{
			var cgc = color.CGColor; // 10.8+
			if (cgc != null)
			{
				if (cgc.NumberOfComponents == 4 && cgc.ColorSpace.Name == CGColorSpaceNames.SRGB)
				{
					r = (float)cgc.Components[0];
					g = (float)cgc.Components[1];
					b = (float)cgc.Components[2];
					a = (float)cgc.Components[3];
					return true;
				}

				if (cgc.NumberOfComponents == 2 && cgc.ColorSpace.Name == CGColorSpaceNames.LinearGray)
				{
					a = (float)cgc.Components[1];
					r = g = b = (float)cgc.Components[0];
					return true;
				}
			}

			if (color.UsingColorSpace(SRGBColorSpace()) is NSColor rgba)
			{
				rgba.GetRgba(out nfloat nr, out nfloat ng, out nfloat nb, out nfloat na);
				a = (float)na;
				r = (float)nr;
				g = (float)ng;
				b = (float)nb;
				return true;
			}

			a = r = g = b = 0;
			return false;
		}

		public static Color ToSDColor(this NSColor color)
		{
			if (color.Type == NSColorType.Catalog && color.CatalogNameComponent == "System") {
				return color.ColorNameComponent switch {
					// FIXME: Reverse of ToNSColor, not always correct
					//"windowFrameColor" => SystemColors.ActiveBorder,
					"gridColor" => SystemColors.ActiveCaption,
					"headerTextColor" => SystemColors.ActiveCaptionText,
					"textColor" => textColor,
					"textBackgroundColor" => textBackgroundColor,
					"controlColor" => SystemColors.Control,
					"controlTextColor" => SystemColors.ControlText, 
					"controlShadowColor" => SystemColors.ControlDark, 
					"controlDarkShadowColor"=> SystemColors.ControlDarkDark, 
					"controlHighlightColor" => SystemColors.ControlLight, 
					"controlBackgroundColor" => SystemColors.ControlLightLight, 
					"disabledControlTextColor" => SystemColors.GrayText, 
					"selectedTextBackgroundColor" => SystemColors.Highlight, 
					"selectedTextColor" => SystemColors.HighlightText, 
					"scrollBarColor" => SystemColors.ScrollBar, 
					"windowBackgroundColor" => SystemColors.Window, 
					"windowFrameTextColor" => SystemColors.WindowText, 
					"windowFrameColor" => SystemColors.WindowFrame,
					/*"controlColor" => SystemColors.ButtonFace, 
					"controlHighlightColor" => SystemColors.ButtonHighlight, 
					"controlShadowColor" => SystemColors.ButtonShadow, */
					_ => Color.FromArgb(color.ToArgb())
				};
			}
			return Color.FromArgb(color.ToArgb());
		}

		public static uint ToUArgb(this NSColor color)
		{
			return (uint)color.ToArgb();
		}
#elif __IOS__
		public static CGColor ToCGColor(this Color c)
		{
			return ToUIColor(c).CGColor;
		}

		public static UIColor ToUIColor(this Color c)
		{
			return ToUIColor(c.ToArgb());
		}

		public static UIColor ToUIColor(this int value)
		{
			value.ToArgb(out int a, out int r, out int g, out int b);
			return UIColor.FromRGBA(r, g, b, a);
		}
#endif

		public static void ToArgb(this int argb, out int a, out int r, out int g, out int b)
		{
			a = (byte)(argb >> 24);
			r = (byte)(argb >> 16);
			g = (byte)(argb >> 8);
			b = (byte)argb;
		}

		public static void ToArgb(this int argb, out nfloat a, out nfloat r, out nfloat g, out nfloat b)
		{
			a = ((byte)(argb >> 24)) / 255f;
			r = ((byte)(argb >> 16)) / 255f;
			g = ((byte)(argb >> 8)) / 255f;
			b = ((byte)argb) / 255f;
		}

		public static CGColor ToCGColor(this int value)
		{
			value.ToArgb(out nfloat a, out nfloat r, out nfloat g, out nfloat b);
			return new CGColor(r, g, b, a);
		}

		public static void SetStrokeColor(this CGContext context, Color color)
		{
			context.SetStrokeColor(ToCGColor(color));
		}

		public static void SetFillColor(this CGContext context, Color color)
		{
			context.SetFillColor(ToCGColor(color));
		}
	}
}

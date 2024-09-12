using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FormsTest
{
	public static class CommonExtension
	{
		private const float defaultDpi = 96f;
		private static SizeF osScaleFactor;
		private static SizeF osDpi;

		static CommonExtension()
		{
			osDpi = getDPI();
			osScaleFactor = new SizeF(osDpi.Width / defaultDpi, osDpi.Height / defaultDpi);
		}

		public static string Dump(this KeyEventArgs e)
		{
			return string.Format(
				"{{code={0}, value={1}, data={2}, shift={3}, alt={4}, ctrl={5}, suppress={6}}}",
				e.KeyCode, e.KeyValue, e.KeyData, e.Shift, e.Alt, e.Control, e.SuppressKeyPress);
		}

		public static string Dump(this KeyPressEventArgs e)
		{
			return string.Format("{{char={0}}}", e.KeyChar);
		}

		public static Rectangle Inset(this Rectangle r, Padding m)
		{
			return r.Inset(m.Left, m.Top, m.Right, m.Bottom);
		}

		public static Rectangle Inset(this Rectangle r, int left, int top, int right, int bottom)
		{
			return new Rectangle(r.X + left, r.Y + top, r.Width - right - left, r.Height - bottom - top);
		}

		public static T EnsureNotNull<T>(this T? obj)
		{
			Debug.Assert(obj is not null);
			return obj;
		}

		public static float GetBaselineOffset(this Label label)
		{
			var (ascent, _, _) = label.GetFontOffsetsScaled();
			return label.Padding.Top + ascent + label.GetVerticalAlignmentOffset();
		}

		public static float GetVerticalAlignmentOffset(this Label label)
		{
			if (!label.AutoSize)
			{
				switch (label.TextAlign)
				{
					case ContentAlignment.MiddleLeft:
					case ContentAlignment.MiddleRight:
					case ContentAlignment.MiddleCenter:
						{
							var size = label.GetPreferredSize(label.Size);
							var height = size.Height;
							return (label.Height - height) / 2;
						}
					case ContentAlignment.BottomLeft:
					case ContentAlignment.BottomRight:
					case ContentAlignment.BottomCenter:
						{
							var size = label.GetPreferredSize(label.Size);
							var height = size.Height;
							return label.Height - height;
						}
				}
			}

			return 0;
		}

#if MAC
		public static (float asc, float desc, float lineHeight) GetOffsets(this Font font, float scaleFactor = 1.0f)
		{
			System.Drawing.Mac.Extensions.GetOffsets(font, out var a, out var d, out var h);
			return (a * scaleFactor, d * scaleFactor, h * scaleFactor);
		}

		public static (float ascent, float descent, float lineSpacing) GetFontOffsetsScaled(this Control control, Font font)
		{
			System.Drawing.Mac.Extensions.GetOffsets(font, out var a, out var d, out var h);
			return (a, d, h);
		}

		public static (float ascent, float descent, float lineSpacing) GetFontOffsetsScaled(this Control control)
		{
			System.Drawing.Mac.Extensions.GetOffsets(control.Font, out var a, out var d, out var h);
			return (a, d, h);
		}
#else
		public static (float asc, float desc, float lineHeight) GetOffsets(this Font font, float scaleFactor = 1.0f)
		{
			FontFamily ff = font.FontFamily;
			float lineSpace = ff.GetLineSpacing(font.Style);
			float ascent = fontHeight * ff.GetCellAscent(font.Style) / lineSpace;
			float descent = fontHeight * ff.GetCellDescent(font.Style) / lineSpace;
			float lineHeight = fontHeight;
			return (ascent, descent, fontHeight);
		}

		public static (float ascent, float descent, float lineSpacing) GetFontOffsetsScaled(this Control control, Font font)
		{
			return font.GetOffsets(control.GetScaleFactor().Height);
		}

		public static (float ascent, float descent, float lineSpacing) GetFontOffsetsScaled(this Control control)
		{
			return control.Font.GetOffsets(control.GetScaleFactor().Height);
		}
#endif

		public static SizeF OSScaleFactor
		{
			get
			{
				return osScaleFactor;
			}
		}

		public static SizeF OSDpi
		{
			get
			{
				return osDpi;
			}
		}


		public static float Scale(this Control ctrl, Graphics gr, float value)
		{
			return Scale(GetScaleFactor(ctrl), gr, value);
		}

		public static double Scale(this Control ctrl, Graphics gr, double value)
		{
			return Scale(GetScaleFactor(ctrl), gr, value);
		}

		public static int Scale(this Control ctrl, Graphics gr, int value)
		{
			return Scale(GetScaleFactor(ctrl), gr, value);
		}

		public static SizeF Scale(this Control ctrl, Graphics gr, SizeF value)
		{
			return Scale(GetScaleFactor(ctrl), gr, value);
		}

		public static Size Scale(this Control ctrl, Graphics gr, Size value)
		{
			return Scale(GetScaleFactor(ctrl), gr, value);
		}

		public static Point Scale(this Control ctrl, Graphics gr, Point value)
		{
			return Scale(GetScaleFactor(ctrl), gr, value);
		}

		public static float Scale(SizeF scaleFactor, Graphics gr, float value)
		{
			return (value * scaleFactor.Width / getGraphicsScaleFactor(gr));
		}

		public static double Scale(SizeF scaleFactor, Graphics gr, double value)
		{
			return (value * scaleFactor.Width / getGraphicsScaleFactor(gr));
		}

		public static int Scale(SizeF scaleFactor, Graphics gr, int value)
		{
			return (int)Math.Round(value * scaleFactor.Width / getGraphicsScaleFactor(gr));
		}

		public static SizeF Scale(SizeF scaleFactor, Graphics gr, SizeF value)
		{
			return new SizeF(value.Width * scaleFactor.Width / getGraphicsScaleFactor(gr), value.Height * scaleFactor.Height / getGraphicsScaleFactor(gr));
		}

		public static Size Scale(SizeF scaleFactor, Graphics gr, Size value)
		{
			return new Size((int)Math.Round(value.Width * scaleFactor.Width / getGraphicsScaleFactor(gr)), (int)Math.Round(value.Height * scaleFactor.Height / getGraphicsScaleFactor(gr)));
		}

		public static Point Scale(SizeF scaleFactor, Graphics gr, Point value)
		{
			return new Point((int)Math.Round(value.X * scaleFactor.Width / getGraphicsScaleFactor(gr)), (int)Math.Round(value.Y * scaleFactor.Height / getGraphicsScaleFactor(gr)));
		}


		public static float Scale(this Control? ctrl, float value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static double Scale(this Control? ctrl, double value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static int Scale(this Control? ctrl, int value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static SizeF Scale(this Control? ctrl, SizeF value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static Size Scale(this Control? ctrl, Size value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static Point Scale(this Control? ctrl, Point value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static Padding Scale(this Control? ctrl, Padding value)
		{
			return Scale(GetScaleFactor(ctrl), value);
		}

		public static float Scale(this SizeF scaleFactor, float value)
		{
			return (value * scaleFactor.Width);
		}

		public static double Scale(this SizeF scaleFactor, double value)
		{
			return (value * scaleFactor.Width);
		}

		public static int Scale(this SizeF scaleFactor, int value)
		{
			return (int)Math.Round((value * scaleFactor.Width));
		}

		public static SizeF Scale(this SizeF scaleFactor, SizeF value)
		{
			return new SizeF(value.Width * scaleFactor.Width, value.Height * scaleFactor.Height);
		}

		public static Size Scale(this SizeF scaleFactor, Size value)
		{
			return new Size((int)Math.Round(value.Width * scaleFactor.Width), (int)Math.Round(value.Height * scaleFactor.Height));
		}

		public static Point Scale(this SizeF scaleFactor, Point value)
		{
			return new Point((int)Math.Round(value.X * scaleFactor.Width), (int)Math.Round(value.Y * scaleFactor.Height));
		}

		public static Padding Scale(this SizeF scaleFactor, Padding value)
		{
			return new Padding(
				Scale(scaleFactor, value.Left),
				Scale(scaleFactor, value.Top),
				Scale(scaleFactor, value.Right),
				Scale(scaleFactor, value.Bottom));
		}

		public static SizeF GetScaleFactor(this Control? ctrl)
		{
#if !MAC
			if (ctrl != null)
			{
				System.Diagnostics.Debug.Assert(!ctrl.InvokeRequired, "GetScaleFactor() must be called on UI thread");

				var factor = getControlDeviceDpi(ctrl) / 96f;
				return new SizeF(factor, factor);
			}
#endif
			return OSScaleFactor;
		}

		/// <summary>
		/// Gets the relative scaling factor of a control compared to OS dpi
		/// </summary>
		/// <param name="ctrl"></param>
		/// <returns></returns>
		public static SizeF GetScaleFactorRelative(this Control? ctrl)
		{
#if !MAC
			if (ctrl != null)
			{
				System.Diagnostics.Debug.Assert(!ctrl.InvokeRequired, "GetScaleFactor() must be called on UI thread");

				var factor = getControlDeviceDpi(ctrl) / osDpi.Width;
				return new SizeF(factor, factor);
			}
#endif
			return new Size(1, 1);
		}

		static float getControlDeviceDpi(this Control? ctrl)
		{
#if !MAC
			Control? topLevel;
			// do
			// {
			// 	topLevel = ctrl?.TopLevelControl;
			// 	if (topLevel is PopupWindowHelper popupWindowHelper)
			// 	{
			// 		ctrl = popupWindowHelper.SourceControl;
			// 	}
			// } while (topLevel is PopupWindowHelper);

			if (ctrl is object)
				return ctrl.DeviceDpi;
#endif

			return osDpi.Width;
		}

		// public static Font ScaleFont(Control ctrl, Font font)
		// {
		// 	SizeF scaleFactor = GetScaleFactor(ctrl);
		// 	return ScaleFont(scaleFactor, font);
		// }

		// public static Font ScaleFont(SizeF scaleFactor, Font font)
		// {
		// 	if (scaleFactor.Width != OSScaleFactor.Width)
		// 	{
		// 		// round the font size to 1 decimal point
		// 		float fontSize = font.Size * scaleFactor.Height / OSScaleFactor.Height;
		// 		fontSize = (float)Math.Round(fontSize * 10) / 10;

		// 		return FontCache.CreateFont(font.FontFamily, fontSize, font.Style);
		// 	}
		// 	return font;
		// }

		private static SizeF getDPI()
		{
			if (Environment.OSVersion.Platform == PlatformID.MacOSX ||
				Environment.OSVersion.Platform == PlatformID.Unix)
				return new SizeF(96, 96); ;

			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
				return new SizeF(graphics.DpiX, graphics.DpiY);
		}

		private static float getGraphicsScaleFactor(this Graphics gr)
		{
			return gr.DpiX / defaultDpi;
		}
	}

}

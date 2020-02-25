using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
#elif XAMARINMAC
using AppKit;
using CoreGraphics;
#endif

namespace WinApi
{
    public static partial class Win32
    {
		internal class SelectedObjects
		{
			public NSFont font = NSFont.SystemFontOfSize(NSFont.SystemFontSize);
			// TODO: Add more objects
		}

		internal static Dictionary<IntPtr, SelectedObjects> selectedObjects = new Dictionary<IntPtr, SelectedObjects>();
		internal static Dictionary<NSFont, List<WCRANGE>> unicodeRangesByFont = new Dictionary<NSFont, List<WCRANGE>>();


        // TODO
        public static bool ExtTextOut(IntPtr hdc, int X, int Y, uint fuOptions, [In] ref RECT lprc, string lpString, int cbCount, IntPtr lpDx)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static int SetBkColor(IntPtr hdc, int clr)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static int SetBkMode(IntPtr hdc, int iBkMode)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

		public static IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj)
		{
			var prev = IntPtr.Zero;
			if (!selectedObjects.TryGetValue(hdc, out SelectedObjects objects))
			{
				objects = new SelectedObjects();
				selectedObjects.Add(hdc, objects);
			}

			var obj = ObjCRuntime.Runtime.GetNSObject(hgdiobj);
			if (obj is NSFont font)
			{
				prev = objects.font?.Handle ?? IntPtr.Zero;
				objects.font = font;
			}

			// else if (obj is NSColor color) {
			// TODO: Add more object types
			//}

			return prev;
        }

        public static bool DeleteObject(IntPtr hObject)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return true;
        }

        public static IntPtr GetStockObject(StockObjects fnObject)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return true;
        }

        public static uint SetTextColor(IntPtr hdc, int crColor)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static uint SetTextAlign(IntPtr hdc, uint fmode)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static bool GetTextExtentExPoint(IntPtr hDC, string lpszStr, int cchString, int nMaxExtent, out int lpnFit, int[] alpDx, out Size lpSize)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            lpnFit = 0;
            lpSize = new Size();
            return false;
        }

        public static IntPtr CreateCompatibleDC(IntPtr hdc)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static bool DeleteDC(IntPtr hdc)
        {
            NotImplemented(MethodBase.GetCurrentMethod());
            return true;
        }

		public static bool OffsetViewportOrgEx(IntPtr hdc, int nXOffset, int nYOffset, out POINT lpPoint)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			lpPoint = new POINT();
			return false;
		}

		public static int SelectClipRgn(IntPtr hdc, IntPtr hrgn)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int IntersectClipRect(IntPtr hdc, int l, int t, int r, int b)
		{
			using (var g = Graphics.FromHdc(hdc))
				g?.SetClip(Rectangle.FromLTRB(l, t, r, b));
			return 0;
		}

		public static bool GradientFill(IntPtr hdc, TRIVERTEX[] pVertex, uint dwNumVertex, GRADIENT_RECT[] pMesh, uint dwNumMesh, uint dwMode)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
		}

		public static IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitCount, short[] lpBits)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static IntPtr CreateBrushIndirect(LOGBRUSH lpLogBrush)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static bool PatBlt(IntPtr hdc, int left, int top, int width, int height, int rop)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
		}

		internal static NSFont GetSelectedFont(IntPtr hdc)
		{
			return selectedObjects.TryGetValue(hdc, out SelectedObjects objects) ? objects.font : null;
		}

		public static uint GetFontUnicodeRanges(IntPtr hdc, IntPtr lpgs)
		{
			var ranges = selectedObjects.TryGetValue(hdc, out SelectedObjects objects) && objects.font != null
				? GetFontUnicodeRanges(objects.font)
				: new List<WCRANGE>();

			int glyphsetSize = 16; //Marshal.SizeOf(typeof(GLYPHSET)) - 4;
			int size = glyphsetSize + ranges.Count * Marshal.SizeOf(typeof(WCRANGE));
			if (lpgs == IntPtr.Zero)
				return (uint)size;

			var totalGlyphs = 0;
			foreach (var range in ranges)
				totalGlyphs += range.cGlyphs;

			Marshal.WriteInt32(lpgs, 0, size); 				//glyphset.cbThis = (ulong)size;
			Marshal.WriteInt32(lpgs, 4, 0);					//glyphset.flAccel = 0;
			Marshal.WriteInt32(lpgs, 8, totalGlyphs);   	//glyphset.cGlyphsSupported = (ulong)totalGlyphs;
			Marshal.WriteInt32(lpgs, 12, ranges.Count);    //glyphset.cRanges = (ulong)ranges.Length;

			for (var i = 0; i < ranges.Count; ++i) {
				Marshal.WriteInt16(lpgs, 16 + i * 4, ranges[i].wcLow);
				Marshal.WriteInt16(lpgs, 18 + i * 4, (short)ranges[i].cGlyphs);
			}

			return (uint)size;
		}

		public static int GetDeviceCaps(IntPtr hdc, int nIndex)
		{
			// FIXME: How to getscreen from Graphics/NSGraphicsContext/hdc?
			//var g = Graphics.FromHdc(hdc);
			//var o = g.nativeObject;
			var screen = NSScreen.MainScreen;

			var cap = (DeviceCap)nIndex;
			switch (cap)
			{
				case DeviceCap.VERTRES: // Logical screen height
					return (int)screen.Frame.Height;
				case DeviceCap.DESKTOPVERTRES: // Physical screen height
					return (int)(screen.Frame.Height * screen.BackingScaleFactor);
				default: throw new NotImplementedException($"GetDeviceCaps({cap}");
			}
		}

		internal static List<WCRANGE> GetFontUnicodeRanges(NSFont font)
		{
			if (unicodeRangesByFont.TryGetValue(font, out List<WCRANGE> ranges))
				return ranges;

			var bytes = new byte[4];
			int a = -1, b = -1;

			ranges = new List<WCRANGE>();

			var charset = font.CoveredCharacterSet;
			for (byte plane = 0; plane <= 16; plane++)
			{
				if (charset.HasMemberInPlane(plane))
				{
					var plane32u = (UInt32)plane;
					for (var c = plane32u << 16; c < (plane32u + 1) << 16; c++)
						if (charset.Contains(c))
							GetFontUnicodeRanges_Next((int)c, ref a, ref b, ref ranges);
				}
			}

			GetFontUnicodeRanges_Next(-1, ref a, ref b, ref ranges);

			unicodeRangesByFont.Add(font, ranges);
			return ranges;
		}

		static bool GetFontUnicodeRanges_Next(int c, ref int a, ref int b, ref List<WCRANGE> ranges)
		{
			if (c == -1)
			{
				if (a != -1)
				{
					ranges.Add(new WCRANGE { wcLow = (char)a, cGlyphs = (ushort)(1 + b - a) });
					a = b = c;
					return true;
				}
			}
			else if (a == -1)
				a = b = c;
			else
				if (b == c - 1)
				b = c;
			else
			{
				ranges.Add(new WCRANGE { wcLow = (char)a, cGlyphs = (ushort)(1 + b - a) });
				a = b = c;
				return true;
			}

			return false;
		}

	}
}

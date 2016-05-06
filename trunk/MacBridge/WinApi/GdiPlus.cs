using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinApi
{
    public static partial class Win32
    {
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
            NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
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

		public static int IntersectClipRect(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
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
	}
}

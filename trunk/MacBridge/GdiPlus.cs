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
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static int SetBkColor(IntPtr hdc, int clr)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static int SetBkMode(IntPtr hdc, int iBkMode)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static bool DeleteObject(IntPtr hObject)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public static IntPtr GetStockObject(StockObjects fnObject)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public static uint SetTextColor(IntPtr hdc, int crColor)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static uint SetTextAlign(IntPtr hdc, uint fmode)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static bool GetTextExtentExPoint(IntPtr hDC, string lpszStr, int cchString, int nMaxExtent, out int lpnFit, int[] alpDx, out Size lpSize)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            lpnFit = 0;
            lpSize = new Size();
            return false;
        }

        public static IntPtr CreateCompatibleDC(IntPtr hdc)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }
    }
}

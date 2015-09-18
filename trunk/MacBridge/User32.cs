using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WinApi
{
    public static partial class Win32
    {
        const string NotImplemented = "Win32 method not implemented on Mac: ";

        public static IntPtr WindowFromPoint(POINT p)
        {
            //TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static IntPtr GetWindow(IntPtr hWnd, uint uCmd)
        {
            //TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static void ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
        }

        public static void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags)
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
        }

        public static int DestroyIcon(IntPtr hIcon)
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public static IntPtr SetFocus(IntPtr hWnd)
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static IntPtr GetFocus()
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        public static bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i)
        {
            // TODO;
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
        {
            // TODO;
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static IntPtr GetForegroundWindow()
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static IntPtr GetDesktopWindow()
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static IntPtr GetShellWindow()
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static int GetWindowRect(IntPtr hwnd, out RECT rc)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            rc = new RECT();
            return 0;
        }

        static bool FlashWindow(IntPtr hwnd, bool bInvert)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public static IntPtr GetWindowDC(IntPtr handle)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static IntPtr ReleaseDC(IntPtr handle, IntPtr hDC)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static int GetClassName(IntPtr hwnd, char[] className, int maxCount)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static IntPtr GetWindow(IntPtr hwnd, int uCmd)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static bool IsWindowVisible(IntPtr hwnd)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static int GetClientRect(IntPtr hwnd, ref RECT lpRect)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static int GetClientRect(IntPtr hwnd, [In, Out] ref Rectangle rect)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static bool UpdateWindow(IntPtr hwnd)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static bool InvalidateRect(IntPtr hwnd, ref Rectangle rect, bool bErase)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static bool ValidateRect(IntPtr hwnd, ref Rectangle rect)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static bool GetWindowRect(IntPtr hWnd, [In, Out] ref Rectangle rect)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public static IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static int GetWindowLongPtr32(IntPtr hWnd, int nIndex)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }

        public static int EnableMenuItem(IntPtr hMenu, SC uIDEnableItem, MF uEnable)
        {
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return 0;
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace WinApi
{
    public static class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        public static IntPtr WindowFromPoint(POINT p)
        {
            //TODO:
            return IntPtr.Zero;
        }

        public static IntPtr GetWindow(IntPtr hWnd, uint uCmd)
        {
            //TODO:
            return IntPtr.Zero;
        }

        public static void ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            // TODO:
        }

        public static void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags)
        {
            // TODO:
        }

        public static int DestroyIcon(IntPtr hIcon)
        {
            // TODO:
            return 1;
        }

        public static bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            // TODO:
            return true;
        }

        public static IntPtr SetFocus(IntPtr hWnd)
        {
            // TODO:
            return IntPtr.Zero;
        }

        public static IntPtr GetFocus()
        {
            // TODO:
            return IntPtr.Zero;
        }

        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        public static bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i)
        {
            // TODO;
            return false;
        }

        public static int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
        {
            // TODO;
            return 0;
        }
    }
}


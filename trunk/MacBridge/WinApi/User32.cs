using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MonoMac.AppKit;
using MacBridge.CoreGraphics;
using System.Windows.Forms.CocoaInternal;

namespace WinApi
{
    public static partial class Win32
    {
		internal static void NotImplemented(MethodBase method, object details = null)
		{
			Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name + (details == null ? String.Empty : " (" + details.ToString() + ")"));
		}

        public static IntPtr WindowFromPoint(POINT p)
        {
            Size displaySize;
            XplatUI.GetDisplaySize(out displaySize);
            p.Y = displaySize.Height - p.Y;

            var screenLocation = p.ToCGPoint();

            int wnum = NSWindow.WindowNumberAtPoint(screenLocation, 0);
            var window = NSApplication.SharedApplication.WindowWithWindowNumber(wnum);
            if (window != null)
            {
                var windowLocation = window.ConvertScreenToBase(screenLocation);
                //var contentViewLocation = window.ContentView.ConvertPointFromView(windowLocation, null);
                var view = window.ContentView.HitTest(windowLocation);

                if (view != null)
                {
                    var hwnd = Hwnd.GetHandleFromWindow(view.Handle);
                    if (hwnd != IntPtr.Zero)
                        return hwnd;
                }
            }

            return IntPtr.Zero;
        }

        public static IntPtr GetWindow(IntPtr hWnd, uint uCmd)
        {
            //TODO:
            NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static void ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            // TODO:
			NotImplemented(MethodBase.GetCurrentMethod());
        }

        public static void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags)
        {
            // TODO: Handle all remaining flags

            //if (0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOZORDER))
            //XplatUI.SetZOrder(hWnd, hWndInsertAfter, false, false);

            int x_, y_, w_, h_, clw_, clh_;
            XplatUI.GetWindowPos(hWnd, false, out x_, out y_, out w_, out h_, out clw_, out clh_);
           
            bool move = 0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOMOVE);
            if (!move)
            {
                x = x_;
                y = y_;
            }

            bool size = 0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOSIZE);
            if (!size)
            {
                cx = w_;
                cy = h_;
            }

            if (move || size)
                XplatUI.SetWindowPos(hWnd, x, y, cx, cy);

            bool show = 0 != (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_SHOWWINDOW);
            bool hide = 0 != (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_HIDEWINDOW);
            bool activate = 0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOACTIVATE);

            if (show || hide || activate)
                XplatUI.SetVisible(hWnd, show || !hide, activate);

            bool redraw = 0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOREDRAW);
            if (redraw)
                XplatUI.UpdateWindow(hWnd);
        }

        public static int DestroyIcon(IntPtr hIcon)
        {
            // TODO:
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            XplatUI.PostMessage(hWnd, (Msg)msg, wParam, lParam);
            return true;
        }

        public static bool SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            XplatUI.SendMessage(hWnd, (Msg)msg, wParam, lParam);
            return true;
        }

        public static IntPtr SetFocus(IntPtr hWnd)
        {
            var prev = XplatUI.GetFocus();
            XplatUI.SetFocus(hWnd);
            return prev;
        }

        public static IntPtr GetFocus()
        {
            return XplatUI.GetFocus();
        }

        public delegate bool EnumWindowProc(IntPtr hWnd,IntPtr parameter);

        public static bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i)
        {
            // TODO;
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
        {
            // TODO;
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

		public static bool AllowSetForegroundWindow(int dwProcessId)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
		}

		public static IntPtr GetForegroundWindow()
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static IntPtr GetDesktopWindow()
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static IntPtr GetShellWindow()
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static int GetWindowRect(IntPtr hwnd, out RECT rc)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            rc = new RECT();
            return 0;
        }

        // Apple says:
        // Don't flash the taskbar button if the only thing the user has to do is activate the program,
        // read a message, or see a change in status.
        public static bool FlashWindow(IntPtr hwnd, bool bInvert)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return true;
        }

        public static IntPtr GetWindowDC(IntPtr handle)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static IntPtr ReleaseDC(IntPtr handle, IntPtr hDC)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static int GetClassName(IntPtr hwnd, char[] className, int maxCount)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static IntPtr GetWindow(IntPtr hwnd, int uCmd)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static bool IsWindowVisible(IntPtr hwnd)
        {
            return XplatUI.IsVisible(hwnd);
        }

        public static int GetClientRect(IntPtr hwnd, ref RECT lpRect)
        {
            var window = Hwnd.ObjectFromHandle(hwnd);
            lpRect = new RECT(window.ClientRect);
            return 1;
        }

        public static int GetClientRect(IntPtr hwnd, [In, Out] ref Rectangle rect)
        {
            var window = Hwnd.ObjectFromHandle(hwnd);
            rect = window.ClientRect;
            return 1;
        }

        public static bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint)
        {
            XplatUI.SetWindowPos(hwnd, X, Y, nWidth, nHeight);
            if (bRepaint)
                XplatUI.UpdateWindow(hwnd);
            return true;
        }

        public static bool UpdateWindow(IntPtr hwnd)
        {
            XplatUI.UpdateWindow(hwnd);
            return true;
        }

        public static bool InvalidateRect(IntPtr hwnd, ref Rectangle rect, bool bErase)
        {
            XplatUI.Invalidate(hwnd, rect, bErase);
            return true;
        }

        public static bool ValidateRect(IntPtr hwnd, ref Rectangle rect)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return true;
        }

        public static bool GetWindowRect(IntPtr hWnd, [In, Out] ref Rectangle rect)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

		public static int GetWindowLongPtr32(IntPtr hWnd, GWL nIndex)
		{
			switch (nIndex)
			{
				case GWL.ID: return GetWindowIdentifier(hWnd);
				case GWL.STYLE: return GetWindowStyle(hWnd);
				case GWL.EXSTYLE: return GetWindowExStyle(hWnd);
				case GWL.WNDPROC:
				case GWL.HINSTANCE:
				case GWL.HWNDPARENT:
				case GWL.USERDATA:
				case GWL.DLGPROC:
				case GWL.USER:
				case GWL.MSGRESULT:
				default:
					break;
			}

			// TODO: Implement remaining options
			NotImplemented(MethodBase.GetCurrentMethod(), nIndex);
			return 0;
		}

        public static IntPtr GetWindowLongPtr64(IntPtr hWnd, GWL nIndex)
        {
			return new IntPtr(GetWindowLongPtr32(hWnd, nIndex));
        }

		public static IntPtr SetWindowLongPtr64(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static int SetWindowLongPtr32(IntPtr hWnd, GWL nIndex, int dwNewLong)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static int EnableMenuItem(IntPtr hMenu, SC uIDEnableItem, MF uEnable)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static IntPtr SetCursor(IntPtr hcur)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static IntPtr LoadCursor(IntPtr hInstcance, SystemCursor hcur)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, BlendFlags dwFlags)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static bool EnableWindow(IntPtr hWnd, bool bEnable)
        {
            XplatUI.EnableWindow(hWnd, bEnable);
            return true;
        }

        public static IntPtr SetActiveWindow(IntPtr handle)
        {
            var prev = XplatUI.GetActive();
            XplatUI.Activate(handle);
            return prev;
        }

        public static bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT wp)
        {
            // TODO: Add support for min & max states

            SetWindowPos(hWnd, IntPtr.Zero,
                wp.rcNormalPosition.left,
                wp.rcNormalPosition.top,
                wp.rcNormalPosition.right - wp.rcNormalPosition.left,
                wp.rcNormalPosition.bottom - wp.rcNormalPosition.top,
                (uint)wp.flags
            );

            return true;
        }

        public static int ScrollWindowEx(IntPtr hWnd, int dx, int dy, IntPtr prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, uint flags)
        {
            var control = Control.FromHandle(hWnd);
            var rect = ((RECT)Marshal.PtrToStructure(prcScroll, typeof(RECT))).ToRectangle();

            // Let's change origin of every NSView whose control's frame intersects with a given rect.
            foreach (Control child in control.Controls)
            {
                if (child.Bounds.IntersectsWith(rect))
                {
                    var window = Hwnd.ObjectFromHandle(child.Handle);
                    var view = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(window.ClientWindow);
                    var origin = view.Bounds.Origin;
                    origin.Y -= dy;
                    origin.X += dx;
                    view.SetBoundsOrigin(origin);
                }
            }
            return 1;
        }

        public static int GetScrollPos(IntPtr hWnd, SB nBar)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static int SetScrollPos(IntPtr hWnd, SB nBar, int nPos, bool bRedraw)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return 0;
        }

        public static bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl)
        {
            // TODO: Add support for min & max states

            lpwndpl = new WINDOWPLACEMENT();

            var nsobject = MonoMac.ObjCRuntime.Runtime.GetNSObject(hWnd);
            if (nsobject == null)
                return false;

            var monoView = nsobject as MonoView;
            if (monoView == null)
                return false;

            var hwnd = Hwnd.ObjectFromHandle(hWnd);
            var window = monoView.Window;
            var isTopLevelView = window.ContentView.Handle == hwnd.WholeWindow;

            var rScreen = isTopLevelView
                ? window.Frame // if it's top level view, then we have to use window's frame, because of the caption
                : monoView.Window.ConvertRectToScreen(monoView.ConvertRectToBase(monoView.Frame));

            Size displaySize;
            XplatUI.GetDisplaySize(out displaySize);

            lpwndpl.rcNormalPosition = new RECT(
                (int)rScreen.Left,
                (int)(displaySize.Height - (rScreen.Top + rScreen.Height)),
                (int)rScreen.Right,
                (int)(displaySize.Height - rScreen.Top)
            );

            return true;
        }

        #region Gestures

        public static bool SetGestureConfig(IntPtr hWnd, int dwReserved, int cIDs, GESTURECONFIG[] pGestureConfig, int cbSize)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static bool GetGestureInfo(IntPtr hGestureInfo, ref GESTUREINFO pGestureInfo)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static bool RegisterTouchWindow(System.IntPtr hWnd, ulong ulFlags)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static bool GetTouchInputInfo(System.IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
            return false;
        }

        public static void CloseTouchInputHandle(System.IntPtr lParam)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
        }

        public static void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
        }

        #endregion

        #region Keyboard

        public static uint MapVirtualKey(uint uCode, uint uMapType)
        {
            // TODO
            return 0;
        }

        public static int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, 
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff, uint wFlags)
        {
            // TODO
            return 0;
        }

        public static bool GetKeyboardState(byte[] lpKeyState)
        {
            // TODO
            return false;
        }

        #endregion // Keyboard

		// Internal

		public static int GetWindowIdentifier(IntPtr hWnd)
		{
			NSView view = ((NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hWnd));
			if (view is MonoContentView)
				return view.Window.WindowNumber;
			return 0;
		}

		public static int GetWindowStyle(IntPtr hWnd)
		{
			WS style = 0;

			Hwnd hwnd = Hwnd.ObjectFromHandle(hWnd);
			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			NSWindow winWrap = vuWrap.Window;

			if ((hwnd.initial_style & WindowStyles.WS_POPUP) != 0)
				style |= WS.POPUP;
			if ((hwnd.initial_style & WindowStyles.WS_OVERLAPPED) != 0)
				style |= WS.OVERLAPPED;
			if (vuWrap.Superview != null)
				style |= WS.CHILD;
			if (vuWrap is MonoContentView && winWrap.ParentWindow != null)
				style |= WS.CHILD;
			if (winWrap.IsMiniaturized)
				style |= WS.MINIMIZE;
			if (winWrap.IsZoomed)
				style |= WS.MAXIMIZE;
			if (!vuWrap.Hidden)
				style |= WS.VISIBLE;
			if (!hwnd.Enabled)
				style |= WS.DISABLED;
			if ((hwnd.initial_style & WindowStyles.WS_BORDER) != 0)
				style |= WS.BORDER;
			if ((hwnd.initial_style & WindowStyles.WS_DLGFRAME) != 0)
				style |= WS.DLGFRAME;
			if ((hwnd.initial_style & WindowStyles.WS_CAPTION) != 0)
				style |= WS.CAPTION;
			if ((hwnd.initial_style & WindowStyles.WS_HSCROLL) != 0)
				style |= WS.HSCROLL;
			if ((hwnd.initial_style & WindowStyles.WS_VSCROLL) != 0)
				style |= WS.VSCROLL;

			return (int)style;
		}

		public static int GetWindowExStyle(IntPtr hWnd)
		{
			WS style = 0;
			// TODO: Implement
			return (int)style;
		}
    }
}

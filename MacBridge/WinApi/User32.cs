using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Mac;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using MacApi;
#if XAMARINMAC
using AppKit;
using Foundation;
using CoreGraphics;
#else
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using ObjCRuntime = MonoMac.ObjCRuntime;
using MacApi.CoreGraphics;
#endif

namespace WinApi
{
	public static partial class Win32
	{
		// This value is cached, because the XplatUICocoa driver also caches it.
		// If we did it different way the XplatUICocoa did, calculations here and there
		// would differ after changing resolution or switching monitors.
		internal static Size screenSize = GetScreenSize();

		internal static Size GetScreenSize()
		{
			Size size;
			XplatUI.GetDisplaySize(out size);
			return size;
		}

        public static IntPtr WindowFromPoint(POINT p)
        {
            p.Y = screenSize.Height - p.Y;
            var screenLocation = new CGPoint(p.X, p.Y);

			var wnum = NSWindow.WindowNumberAtPoint(screenLocation, 0);
            var window = NSApplication.SharedApplication.WindowWithWindowNumber(wnum);
            if (window != null)
            {
                var windowLocation = window.ConvertScreenToBase(screenLocation);
				var view = HitTestIgnoringGrab(window, windowLocation);

				// Embedded native control? => Find MonoView parent
				while (view != null && !(view is MonoView))
					view = view.Superview;

				if (view != null)
					return view.Handle;
            }

            return IntPtr.Zero;
        }

		internal static NSView HitTestIgnoringGrab(NSWindow window, CGPoint point)
		{
			return window.ContentView.HitTest(point);
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

            if (0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOZORDER))
            	XplatUI.SetZOrder(hWnd, hWndInsertAfter, true, false);

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

            bool visible = XplatUI.IsVisible(hWnd);
            
            bool show = 0 != (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_SHOWWINDOW);
            bool hide = 0 != (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_HIDEWINDOW);
            bool activate = 0 == (uFlags & (uint)XplatUIWin32.SetWindowPosFlags.SWP_NOACTIVATE);

			if ((show && !visible))
                XplatUI.SetVisible(hWnd, true, activate);
			else if ((hide && visible))
				XplatUI.SetVisible(hWnd, false, false);

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

        public static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            return XplatUI.SendMessage(hWnd, (Msg)msg, wParam, lParam);
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

		public static int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int GetWindowTextLength(IntPtr hWnd)
		{
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
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd);
			if (vuWrap is IClientView)
				lpRect = new RECT(((IClientView)vuWrap).ClientBounds.ToRectangle());
			else
				lpRect = new RECT(0, 0, (int)vuWrap.Frame.Width, (int)vuWrap.Frame.Height);
            return 1;
        }

        public static int GetClientRect(IntPtr hwnd, [In, Out] ref Rectangle rect)
        {
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd);
			if (vuWrap is IClientView)
				rect = ((IClientView)vuWrap).ClientBounds.ToRectangle();
			else
				rect = new Rectangle(0, 0, (int)vuWrap.Frame.Width, (int)vuWrap.Frame.Height);
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
			switch (nIndex)
			{
				case GWL.EXSTYLE:
					return SetWindowExStyle(hWnd, dwNewLong);
				default:
					break;
			}

			NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static int SetWindowLongPtr32(IntPtr hWnd, GWL nIndex, int dwNewLong)
        {
            return SetWindowLongPtr64(hWnd, nIndex, new IntPtr(dwNewLong)).ToInt32();
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
			var rect = prcScroll != IntPtr.Zero ? ((RECT)Marshal.PtrToStructure(prcScroll, typeof(RECT))).ToRectangle() : control.Bounds;
			var iflags = (int)flags;

			// FIXME:
			// We're ignoring prcClip

			// Let's change origin of every NSView whose control's frame intersects with a given rect.
			if (0 != (iflags & Win32.SW_SCROLLCHILDREN))
			{
				foreach (Control child in control.Controls)
				{
					if (child.Bounds.IntersectsWith(rect))
					{
						var b = child.Bounds;
						XplatUI.SetWindowPos(child.Handle, b.X + dx, b.Y + dy, b.Width, b.Height);
					}
				}
			}

			if (0 != (iflags & Win32.SW_INVALIDATE))
			{
				// NTH: invalidate just the regionn
				control.Invalidate(false);
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

            var nsobject = ObjCRuntime.Runtime.GetNSObject(hWnd);
            if (nsobject == null)
                return false;

            var monoView = nsobject as MonoView;
            if (monoView == null)
                return false;

            var window = monoView.Window;
			var isTopLevelView = window.ContentView.Handle == hWnd;

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

        public static bool RegisterTouchWindow(System.IntPtr hWnd, int ulFlags)
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
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
        }

        public static int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, 
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff, uint wFlags)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
        }

        public static bool GetKeyboardState(byte[] lpKeyState)
        {
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
        }

		public static IntPtr GetKeyboardLayout(int dwLayout)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		#endregion // Keyboard

		// Internal

		public static int GetWindowIdentifier(IntPtr hWnd)
		{
			NSView view = ((NSView)ObjCRuntime.Runtime.GetNSObject(hWnd));
			if (view is MonoContentView)
				return (int)view.Window.WindowNumber;
			return 0;
		}

		public static int GetWindowStyle(IntPtr hWnd)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hWnd);
			NSWindow winWrap = vuWrap.Window;

			if (vuWrap is MonoView)
				return (int)((MonoView)vuWrap).Style;

			WS style = 0;
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

			return (int)style;
		}

		public static int GetWindowExStyle(IntPtr hWnd)
		{
			WS style = 0;
			NotImplemented(MethodBase.GetCurrentMethod());
			return (int)style;
		}

		internal static IntPtr SetWindowExStyle(IntPtr hWnd, IntPtr dwNewLong)
		{
			IntPtr result = IntPtr.Zero;
			if (ObjCRuntime.Runtime.GetNSObject(hWnd) is NSObject o)
			{
				if (o is MonoView mview)
				{
					result = (IntPtr)mview.ExStyle;
					mview.ExStyle = (WindowExStyles)dwNewLong;
				}
				else if (o is MonoWindow mwin)
				{
					if (mwin.ContentView is MonoView cview)
					{
						result = (IntPtr)cview.ExStyle;
						cview.ExStyle = (WindowExStyles)dwNewLong;
					}
				}
			}
			return result;
		}

		#region Graphics
		public static int DrawTextW(IntPtr hDC, string lpszString, int nCount, ref RECT lpRect, int nFormat)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int DrawTextA(IntPtr hDC, byte[] lpszString, int byteCount, ref RECT lpRect, int nFormat)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		#endregion

		public static uint GetGuiResources(IntPtr hProcess, uint uiFlags)
		{
			// TODOO
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static IntPtr CreatePopupMenu()
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static bool DestroyMenu(IntPtr hMenu)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return true;
		}

		public static int GetMenuDefaultItem(IntPtr hMenu, uint fByPos, uint gmdiFlags)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		/// <summary>
		/// Synthesizes keystrokes, mouse motions, and button clicks.
		/// </summary>
		public static uint SendInput(uint nInputs, INPUT[] inputs, int cbSize)
		{
			foreach (var input in inputs)
				SendInput(input);

			return 0;
		}

		public static short VkKeyScan(char ch)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static short GetKeyState(int vKey)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static bool SetCursorPos(int x, int y)
		{
			NSApplication.SharedApplication.InvokeOnMainThread(() =>
			{
			CGPoint p = new CGPoint(x, y);
				var e = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.MouseMoved, p, (CGMouseButton)0);
				CGEventPost(CGEventTapLocation.HID, e);
				CFRelease(e);
			});

			return true;
		}

		public static bool GetCursorPos(out Point p)
		{
			CGPoint q = CGPoint.Empty;
			NSApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var e = CGEventCreate(IntPtr.Zero);
				q = CGEventGetLocation(e);
				CFRelease(e);
			});
			p = new Point((int)q.X, (int)q.Y);
			return true;
		}

		internal static void SendInput(INPUT input)
		{
			switch (input.Type)
			{
				case InputType.MOUSE: SendMouse(input); break;
				case InputType.KEYBOARD: SendKey(input); break;
				case InputType.HARDWARE: SendHardware(input); break;
			}
		}

		internal static void SendMouse(INPUT input)
		{
			NSApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var e = CGEventCreate(IntPtr.Zero);
				var q = CGEventGetLocation(e);
				CFRelease(e);

				var p = new CGPoint(q.X + input.Data.Mouse.X, q.Y + input.Data.Mouse.Y);

				CGEventType type;
				CGMouseButton button;
				ToTypeAndButton(input.Data.Mouse.Flags, out type, out button);
				e = CGEventCreateMouseEvent(IntPtr.Zero, type, p, button);
				CGEventPost(CGEventTapLocation.HID, e);
				CFRelease(e);
			});
		}

		class EventEntry { public CGEventType type; public CGMouseButton button; }
		static readonly Dictionary<MOUSEEVENTF, EventEntry> EventTypes = CreateEventTypesDict();
		static Dictionary<MOUSEEVENTF, EventEntry> CreateEventTypesDict()
		{
			var d = new Dictionary<MOUSEEVENTF, EventEntry>();
			d.Add(MOUSEEVENTF.LEFTDOWN, new EventEntry { type = CGEventType.LeftMouseDown, button = CGMouseButton.Left });
			d.Add(MOUSEEVENTF.LEFTUP, new EventEntry { type = CGEventType.LeftMouseUp, button = CGMouseButton.Left });
			d.Add(MOUSEEVENTF.RIGHTDOWN, new EventEntry { type = CGEventType.RightMouseDown, button = CGMouseButton.Right });
			d.Add(MOUSEEVENTF.RIGHTUP, new EventEntry { type = CGEventType.RightMouseUp, button = CGMouseButton.Right });
			d.Add(MOUSEEVENTF.MIDDLEDOWN, new EventEntry { type = CGEventType.OtherMouseDown, button = CGMouseButton.Center });
			d.Add(MOUSEEVENTF.MIDDLEUP, new EventEntry { type = CGEventType.OtherMouseUp, button = CGMouseButton.Center });
			d.Add(MOUSEEVENTF.MOVE, new EventEntry { type = CGEventType.MouseMoved });
			d.Add(MOUSEEVENTF.HWHEEL, new EventEntry { type = CGEventType.ScrollWheel });
			return d;
		}

		internal static void ToTypeAndButton(MOUSEEVENTF flags, out CGEventType type, out CGMouseButton button)
		{
			EventEntry e;
			if (EventTypes.TryGetValue((MOUSEEVENTF)((uint)flags & 0x1FFFF), out e))
			{
				type = e.type;
				button = e.button;
			}
			else
			{
				type = CGEventType.Null;
				button = 0;
			}
		}

		internal static void SendKey(INPUT input)
		{
			NSApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var strokes = ToKeyStrokes(input);
				foreach (var stroke in strokes)
				{
					var e = CGEventCreateKeyboardEvent(IntPtr.Zero, stroke.KeyCode, stroke.Down);
					CGEventPost(CGEventTapLocation.HID, e);
					CFRelease(e);
				}
			});
		}

		internal class KeyStroke
		{
			public ushort KeyCode = 0;
			public bool Down = true;
		}
			                                                   
		internal static IEnumerable<KeyStroke> ToKeyStrokes(INPUT input)
		{
			return new KeyStroke[] { };
		}

		internal static void SendHardware(INPUT input)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		//https://github.com/xamarin/xamarin-macios/blob/master/src/CoreGraphics/CGEvent.cs

		[DllImport(Constants.ApplicationServicesCoreGraphicsLibrary)]
		internal extern static IntPtr CGEventCreate(IntPtr source);

		[DllImport(Constants.ApplicationServicesCoreGraphicsLibrary)]
		internal extern static CGPoint CGEventGetLocation(IntPtr handle);

		[DllImport(Constants.ApplicationServicesCoreGraphicsLibrary)]
		internal extern static IntPtr CGEventCreateMouseEvent(IntPtr source, CGEventType mouseType, CGPoint mouseCursorPosition, CGMouseButton mouseButton);

		[DllImport(Constants.ApplicationServicesCoreGraphicsLibrary)]
		internal extern static IntPtr CGEventCreateKeyboardEvent(IntPtr source, ushort virtualKey, bool keyDown);

		[DllImport(Constants.ApplicationServicesCoreGraphicsLibrary)]
		internal extern static void CGEventPost(CGEventTapLocation location, IntPtr handle);

		[DllImport(Constants.CoreFoundationLibrary)]
		internal static extern void CFRelease(IntPtr obj);

		public static int GetSystemMetrics(SystemMetric smIndex)
		{
			switch (smIndex)
			{
				case SystemMetric.SM_MAXIMUMTOUCHES: return 5;
				default: throw new NotImplementedException($"GetSystemMetrics({smIndex})");
			}
		}

		public static bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason)
		{
			return true;
		}

		public static bool ShutdownBlockReasonDestroy(IntPtr hWnd)
		{
			return true;
		}

		public static bool IsHungAppWindow(IntPtr hwnd)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
		}


		public static IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, DeviceContextValues flags)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			lpdwProcessId = 0u;
			return 0u;
		}

		public static IntPtr MonitorFromRect(ref RECT lprc, int dwFlags)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}
	}
}
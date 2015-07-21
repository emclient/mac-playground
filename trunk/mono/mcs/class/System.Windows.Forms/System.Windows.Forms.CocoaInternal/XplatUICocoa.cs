//
//EventHandler.cs
// 
//Author:
//	Lee Andrus <landrus2@by-rite.net>
//
//Copyright (c) 2009-2010 Lee Andrus
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//

//
//This document was originally created as a copy of XplatUICarbon.cs 
//and retains many features thereof.
//

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@novell.com>
//
//

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MonoMac.Foundation;
using Cocoa = System.Windows.Forms.CocoaInternal;

/// Cocoa Version
using MonoMac.AppKit;
using NSPoint = System.Drawing.PointF;
using NSRect = System.Drawing.RectangleF;
using System.Windows.Forms.CocoaInternal;


namespace System.Windows.Forms {

	namespace CocoaInternal {
		internal delegate Rectangle [] HwndDelegate (IntPtr handle);
	}

	internal class XplatUICocoa : XplatUIDriver {
		#region Local Variables
		// General driver variables
		private static XplatUICocoa Instance;
		private static int RefCount;
		private static bool themes_enabled;

		// Internal members available to the event handler sub-system
		internal static IntPtr FocusWindow;
		internal static IntPtr ActiveWindow;
		internal static NSWindow ReverseWindow;
		internal static NSWindow CaretWindow;

		internal static Hwnd MouseHwnd;

		internal static MouseButtons MouseState;
		internal static Cocoa.Hover Hover;

		internal static Cocoa.HwndDelegate HwndDelegate = new Cocoa.HwndDelegate (GetClippingRectangles);
		// Instance members
		internal Point mouse_position;

		// Event handlers
		internal Cocoa.ApplicationHandler ApplicationHandler;
		internal Cocoa.ControlHandler ControlHandler;
//		internal Cocoa.HIObjectHandler HIObjectHandler;
		internal Cocoa.KeyboardHandler KeyboardHandler;
		internal Cocoa.MouseHandler MouseHandler;
		internal Cocoa.WindowHandler WindowHandler;
		
		// Cocoa Specific
		internal static GrabStruct Grab;
		internal static Cocoa.Caret Caret;
		private static Cocoa.Dnd Dnd;
		private static Hashtable WindowMapping;
		private static Hashtable HandleMapping;
//		private static IntPtr FosterParent;
//		private static IntPtr Subclass;
		private static int MenuBarHeight;
		internal static ArrayList UtilityWindows;
		internal static readonly Stack<IntPtr> ModalSessions = new Stack<IntPtr>();
		internal float screenHeight;

		// Message loop
		private static Queue MessageQueue;
		private static bool GetMessageResult;

		private static bool ReverseWindowMapped;

		// Timers
		private ArrayList TimerList;
		private static bool in_doevents;
		
		static readonly object instancelock = new object ();
		static readonly object queuelock = new object ();
		#endregion Local Variables
		
		#region Constructors
		private XplatUICocoa() {

			RefCount = 0;
			TimerList = new ArrayList ();
			in_doevents = false;
			MessageQueue = new Queue ();
			
			Initialize ();
		}

		~XplatUICocoa() {
			// FIXME: Clean up the FosterParent here.
		}
		#endregion

		#region Singleton specific code
		public static XplatUICocoa GetInstance() {
			lock (instancelock) {
				if (Instance == null) {
					NSApplication.Init ();
					NSApplication.InitDrawingBridge ();

					Instance = new XplatUICocoa ();
				}
				RefCount++;
			}
			return Instance;
		}

		public int Reference {
			get {
				return RefCount;
			}
		}
		#endregion

		#region Internal methods
		internal void AddExpose (Hwnd hwnd, bool client, NSRect nsrect)
		{
			float height = client ? hwnd.ClientRect.Height : hwnd.Height;
			Rectangle mrect = NativeToMonoFramed (nsrect, height);
			AddExpose (hwnd, client, mrect);
		}

		internal void AddExpose (Hwnd hwnd, bool client, Rectangle rect) {
			AddExpose (hwnd, client, (int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height);
		}

		internal void FlushQueue () {
			CheckTimers (DateTime.UtcNow);
			lock (queuelock) {
				while (MessageQueue.Count > 0) {
					object queueobj = MessageQueue.Dequeue ();
					if (queueobj is GCHandle) {
						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)queueobj);
					} else {
						MSG msg = (MSG)queueobj;
						NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
					}
				}
			}
		}

		internal static Rectangle [] GetClippingRectangles (IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null)
				return null;
 			if (hwnd.Handle != handle)
				return new Rectangle [] {hwnd.ClientRect};

			return (Rectangle []) hwnd.GetClippingRectangles ().ToArray (typeof (Rectangle));
		}

		internal IntPtr GetMousewParam(int Delta) {
			int	 result = 0;

			if ((MouseState & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((MouseState & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((MouseState & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			if ((MouseState & MouseButtons.XButton1) != 0) {
				result |= (int)MsgButtons.MK_XBUTTON1;
			}

			if ((MouseState & MouseButtons.XButton2) != 0) {
				result |= (int)MsgButtons.MK_XBUTTON2;
			}

			Keys mods = ModifierKeys;
			if ((mods & Keys.Control) != 0) {
				result |= (int)MsgButtons.MK_CONTROL;
			}

			if ((mods & Keys.Shift) != 0) {
				result |= (int)MsgButtons.MK_SHIFT;
			}

			result |= Delta << 16;

			return (IntPtr)result;
		}

		internal IntPtr HandleToWindow (IntPtr handle) {
			if (WindowMapping [handle] != null)
				return (IntPtr) WindowMapping [handle];
			return IntPtr.Zero;
		}

		internal IntPtr WindowToHandle (IntPtr winRef) {
			if (HandleMapping [winRef] != null)
				return (IntPtr) HandleMapping [winRef];
			return IntPtr.Zero;
		}

		internal void Initialize ()
		{
			// Cache main screen height for flipping screen coordinates.
			{
				Size size;
				GetDisplaySize (out size);
				screenHeight = size.Height;
			}

			// Initialize the event handlers	
			Cocoa.EventHandler.Driver = this;
			ApplicationHandler = new Cocoa.ApplicationHandler (this);
			ControlHandler = new Cocoa.ControlHandler (this);
			KeyboardHandler = new Cocoa.KeyboardHandler (this);
			MouseHandler = new Cocoa.MouseHandler (this);
			WindowHandler = new Cocoa.WindowHandler (this);

			// Initilize the mouse controls
			Hover.Interval = 500;
			Hover.Timer = new Timer ();
			Hover.Timer.Enabled = false;
			Hover.Timer.Interval = Hover.Interval;
			Hover.Timer.Tick += new EventHandler (HoverCallback);
			Hover.X = -1;
			Hover.Y = -1;
			MouseState = MouseButtons.None;
			mouse_position = Point.Empty;

			// Initialize the Caret
			Caret.Timer = new Timer ();
			Caret.Timer.Interval = 500;
			Caret.Timer.Tick += new EventHandler (CaretCallback);

			// Initialize the D&D
			Dnd = new Cocoa.Dnd (); 
			
			// Initialize the Cocoa Specific stuff
			WindowMapping = new Hashtable ();
			HandleMapping = new Hashtable ();
			UtilityWindows = new ArrayList ();

//			// Initialize the FosterParent
			NSRect rect = NSRect.Empty;
			Cocoa.ProcessSerialNumber psn = new Cocoa.ProcessSerialNumber();

			GetCurrentProcess( ref psn );
			TransformProcessType (ref psn, 1);
			SetFrontProcess (ref psn);
			NSProcessInfo.ProcessInfo.ProcessName = Application.ProductName;
//
//			HIObjectRegisterSubclass (__CFStringMakeConstantString ("com.novell.mwfview"), __CFStringMakeConstantString ("com.apple.hiview"), 0, Cocoa.EventHandler.EventHandlerDelegate, (uint)Cocoa.EventHandler.HIObjectEvents.Length, Cocoa.EventHandler.HIObjectEvents, IntPtr.Zero, ref Subclass);
//
//			Cocoa.EventHandler.InstallApplicationHandler ();
//
//			CreateNewWindow (Cocoa.WindowClass.kDocumentWindowClass, Cocoa.WindowAttributes.kWindowStandardHandlerAttribute | Cocoa.WindowAttributes.kWindowCloseBoxAttribute | Cocoa.WindowAttributes.kWindowFullZoomAttribute | Cocoa.WindowAttributes.kWindowCollapseBoxAttribute | Cocoa.WindowAttributes.kWindowResizableAttribute | Cocoa.WindowAttributes.kWindowCompositingAttribute, ref rect, ref FosterParent);
//			
//			CreateNewWindow (Cocoa.WindowClass.kOverlayWindowClass, Cocoa.WindowAttributes.kWindowNoUpdatesAttribute | Cocoa.WindowAttributes.kWindowNoActivatesAttribute, ref rect, ref ReverseWindow);
			ReverseWindow = new NSWindow(rect, NSWindowStyle.Borderless, NSBackingStore.Buffered, true);
//			CreateNewWindow (Cocoa.WindowClass.kOverlayWindowClass, Cocoa.WindowAttributes.kWindowNoUpdatesAttribute | Cocoa.WindowAttributes.kWindowNoActivatesAttribute, ref rect, ref CaretWindow);
			CaretWindow = new NSWindow(rect, NSWindowStyle.Borderless, NSBackingStore.Buffered, true);
//
//			// Get some values about bar heights
//			Cocoa.Rect structRect = new Cocoa.Rect ();
//			Cocoa.Rect contentRect = new Cocoa.Rect ();
//			GetWindowBounds (FosterParent, 32, ref structRect);
//			GetWindowBounds (FosterParent, 33, ref contentRect);

			NSApplication NSApp = NSApplication.SharedApplication;
//			Console.WriteLine ("{0}", NSApp);
			NSMenu mainMenu = NSApp.MainMenu;
			#if DriverDebug
				Console.WriteLine ("{0}", mainMenu);
			#endif
			bool tempMenu = true;
			if (null == mainMenu) {
//				Console.WriteLine ("mainMenu is null.");
			} else {
				tempMenu = false;
			}

			/*if (tempMenu) {
				mainMenu = new NSMenu();
				NSApp.MainMenu = mainMenu;
			}
			MenuBarHeight = (int) mainMenu.MenuBarHeight;*/
//			Console.WriteLine ("MenuBarHeight = {0}", MenuBarHeight);
//			Console.WriteLine ("{0}", mainMenu.Description);

			// JV
//			if (tempMenu)
//				NSApp.MainMenu = null;

			// Focus
			FocusWindow = IntPtr.Zero;

			// Message loop
			GetMessageResult = true;

			ReverseWindowMapped = false;
		}

		internal void PerformNCCalc (Hwnd hwnd)
		{
			XplatUIWin32.NCCALCSIZE_PARAMS  ncp;
			IntPtr ptr;
			Rectangle rect;

			rect = new Rectangle (0, 0, hwnd.Width, hwnd.Height);

//FIXME! Should not reference Win32 variant here or NEED to do so.
			ncp = new XplatUIWin32.NCCALCSIZE_PARAMS ();
			ptr = Marshal.AllocHGlobal (Marshal.SizeOf (ncp));

			ncp.rgrc1.left = rect.Left;
			ncp.rgrc1.top = rect.Top;
			ncp.rgrc1.right = rect.Right;
			ncp.rgrc1.bottom = rect.Bottom;

			Marshal.StructureToPtr (ncp, ptr, true);
			NativeWindow.WndProc (hwnd.Handle, Msg.WM_NCCALCSIZE, (IntPtr) 1, ptr);
			ncp = (XplatUIWin32.NCCALCSIZE_PARAMS) Marshal.PtrToStructure (ptr, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));
			Marshal.FreeHGlobal(ptr);

			rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			hwnd.ClientRect = rect;

			rect = TranslateClientRectangleToQuartzClientRectangle (hwnd);

			if (hwnd.visible) {
				NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);
				NSRect cr = MonoToNativeFramed (rect, vuWrap.Superview.Frame.Height);
				vuWrap.Frame = cr;
			}

			AddExpose (hwnd, false, 0, 0, hwnd.Width, hwnd.Height);
		}

		internal void ScreenToClientWindow (IntPtr handle, ref NSPoint point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView viewWrapper = null;
			if (null != hwnd)
				viewWrapper = MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ScreenToClientWindow() requires NSView*");

			NSWindow windowWrapper = viewWrapper.Window;
			if (windowWrapper == null) {
				point = new NSPoint (0, 0);
				return;
			}

			point = windowWrapper.ConvertScreenToBase (point);
			point = viewWrapper.ConvertPointFromView (point, null);

//			/*TODO? if (windowWrapper.contentView() != viewWrapper) */ {
//				Point clientOrigin = hwnd.client_rectangle.Location;
//				point.x -= clientOrigin.X;
//				point.y -= clientOrigin.Y;
//			}
		}

		internal void ClientWindowToScreen (IntPtr handle, ref NSPoint point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView viewWrapper = null;
			if (null != hwnd)
				viewWrapper = MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ClientWindowToScreen() requires NSView*");

			NSWindow windowWrapper = viewWrapper.Window;

//			/*TODO? if (windowWrapper.contentView() != viewWrapper) */ {
//				Point clientOrigin = hwnd.client_rectangle.Location;
//				point.x += clientOrigin.X;
//				point.y += clientOrigin.Y;
//			}

			point = viewWrapper.ConvertPointToView (point, null);
			point = windowWrapper.ConvertBaseToScreen (point);
		}

		internal static Rectangle TranslateClientRectangleToQuartzClientRectangle (Hwnd hwnd) {
			return TranslateClientRectangleToQuartzClientRectangle (hwnd, Control.FromHandle (hwnd.Handle));
		}

		internal static Rectangle TranslateClientRectangleToQuartzClientRectangle (Hwnd hwnd, Control ctrl) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Rectangle rect = hwnd.ClientRect;
			Form form = ctrl as Form;
			CreateParams cp = null;

			if (form != null)
				cp = form.GetCreateParams ();

			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Rectangle qrect = rect;
				
				qrect.Y -= borders.top;
				qrect.X -= borders.left;
				qrect.Width += borders.left + borders.right;
				qrect.Height += borders.top + borders.bottom;
				
				rect = qrect;
			}
			
			if (rect.Width < 1 || rect.Height < 1) {
				rect.Width = 1;
				rect.Height = 1;
				rect.X = -5;
				rect.Y = -5;
			}
			
			return rect;
		}

		internal static Size TranslateWindowSizeToQuartzWindowSize (CreateParams cp) {
			return TranslateWindowSizeToQuartzWindowSize (cp, new Size (cp.Width, cp.Height));
		}

		internal static Size TranslateWindowSizeToQuartzWindowSize (CreateParams cp, Size size) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size qsize = size;

				qsize.Width -= borders.left + borders.right;
				qsize.Height -= borders.top + borders.bottom; 
				
				size = qsize;
			}

			if (size.Height == 0)
				size.Height = 1;
			if (size.Width == 0)
				size.Width = 1;
			return size;
		}
			
		internal static Size TranslateQuartzWindowSizeToWindowSize (CreateParams cp, int width, int height) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Size size = new Size (width, height);
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size qsize = size;

				qsize.Width += borders.left + borders.right;
				qsize.Height += borders.top + borders.bottom;
				
				size = qsize;
			}

			return size;
		}

		internal void EnqueueMessage (MSG msg) {
			lock (queuelock) {
				MessageQueue.Enqueue (msg);
			}
		}

		#region Reversible regions
		/* 
		 * Quartz has no concept of XOR drawing due to its compositing nature
		 * We fake this by mapping a overlay window on the first draw and mapping it on the second.
		 * This has some issues with it because its POSSIBLE for ControlPaint.DrawReversible* to actually
		 * reverse two regions at once.  We dont do this in MWF, but this behaviour woudn't work.
		 * We could in theory cache the Rectangle/Color combination to handle this behaviour.
		 *
		 * PROBLEMS: This has some flicker / banding
		 */
		internal void SizeWindow (Rectangle rect, IntPtr window)
		{
			NSWindow winWrap = (NSWindow) MonoMac.ObjCRuntime.Runtime.GetNSObject (window);
			NSRect qrect = MonoToNativeScreen (rect);
			qrect = winWrap.FrameRectFor (qrect);
			winWrap.SetFrame (qrect, false);
#if DriverDebug
			Console.WriteLine ("SizeWindow ({0}, {1}) : {2}", rect, winWrap, qrect);
#endif
		}

		internal void PositionWindowInClient (Rectangle rect, NSWindow window, IntPtr handle)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);

//			/*TODO? if (winWrap.contentView() != vuWrap) */ {
//				rect.Location += (Size) hwnd.client_rectangle.Location;
//			}

			NSRect nsrect = MonoToNativeFramed (rect, vuWrap.Frame.Size.Height);

			NSPoint location = nsrect.Location;
            ClientWindowToScreen (handle, ref location);
			nsrect.Location = location;
			nsrect = window.FrameRectFor (nsrect);
			window.SetFrame (nsrect, false);
#if DriverDebug
			Console.WriteLine ("PositionWindowInClient ({0}, {1}) : {2}", rect, window, nsrect);
#endif
		}
		#endregion Reversible regions

		/*internal static NSString __CFStringMakeConstantString (string cString)
		{
            return NSString.Create (cString);
		}*/

//		internal static void CFRelease (NSObject wHnd)
//		{
//			wHnd.release ();
//		}

		internal NSPoint MonoToNativeScreen (Point monoPoint)
		{
			return MonoToNativeFramed (monoPoint, screenHeight);
		}

		internal NSPoint MonoToNativeFramed (Point monoPoint, float frameHeight)
		{
			return new NSPoint (monoPoint.X, frameHeight - monoPoint.Y);
		}

		internal Point NativeToMonoScreen (NSPoint nativePoint)
		{
			return NativeToMonoFramed (nativePoint, screenHeight);
		}

		internal Point NativeToMonoFramed (NSPoint nativePoint, float frameHeight)
		{
			return new Point ((int) nativePoint.X, (int) (frameHeight - nativePoint.Y));
		}

		internal NSRect MonoToNativeScreen (Rectangle monoRect)
		{
			return MonoToNativeFramed (monoRect, screenHeight);
		}

		internal NSRect MonoToNativeFramed (Rectangle monoRect, float frameHeight)
		{
			return new NSRect(monoRect.Left, frameHeight - monoRect.Bottom, monoRect.Width, monoRect.Height);
		}

		internal Rectangle NativeToMonoScreen (NSRect nativeRect)
		{
			return NativeToMonoFramed (nativeRect, screenHeight);
		}

		internal Rectangle NativeToMonoFramed (NSRect nativeRect, float frameHeight)
		{
			return new Rectangle ((int) nativeRect.Left, (int) (frameHeight - nativeRect.Bottom), 
						(int) nativeRect.Size.Width, (int) nativeRect.Size.Height);
		}

		internal void HwndPositionFromNative (Hwnd hwnd)
		{
			if (hwnd.zero_sized)
				return;

			NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.WholeWindow);
			NSView clientVuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);
			NSRect nsrect = vuWrap.Frame;
			Rectangle mrect;

			bool top = null != WindowMapping [hwnd.Handle];
			if (top) {
				NSWindow winWrap = vuWrap.Window;
				nsrect.Location = winWrap.ConvertBaseToScreen (nsrect.Location);
				nsrect.Size = TranslateQuartzWindowSizeToWindowSize (Control.FromHandle (hwnd.Handle).GetCreateParams (), (int)nsrect.Width, (int)nsrect.Height);
				mrect = NativeToMonoScreen (nsrect);
			} else {
				NSView superVuWrap = vuWrap.Superview;
//				Hwnd parent = hwnd.Parent;

				mrect = NativeToMonoFramed (nsrect, superVuWrap.Frame.Size.Height);

//				if (null != parent) {
//					Point clientOffset = parent.ClientRect.Location;
//					mrect.X -= clientOffset.X;
//					mrect.Y -= clientOffset.Y;
//				}
			}


			bool moved = hwnd.X != mrect.X || hwnd.Y != mrect.Y;
			if (moved || hwnd.Width != mrect.Width || hwnd.Height != mrect.Height) {
				hwnd.X = mrect.X;
				hwnd.Y = mrect.Y;
				hwnd.Width = mrect.Width;
				hwnd.Height = mrect.Height;

				if (top && moved)
					SetCaretPos (hwnd.Handle, Caret.X, Caret.Y);
			}
#if DriverDebug
			Console.WriteLine ("HwndPositionFromNative ({0}) : {1}", hwnd, mrect);
#endif
		}
		#endregion Internal methods
		
		#region Callbacks
		private void CaretCallback (object sender, EventArgs e) {
			if (Caret.Paused) {
				return;
			}

			if (!Caret.On) {
				ShowCaret ();
			} else {
				HideCaret ();
			}
		}
		
		private void HoverCallback (object sender, EventArgs e) {
			if ((Hover.X == mouse_position.X) && (Hover.Y == mouse_position.Y)) {
				MSG msg = new MSG ();
				msg.hwnd = Hover.Hwnd;
				msg.message = Msg.WM_MOUSEHOVER;
				msg.wParam = GetMousewParam (0);
				msg.lParam = (IntPtr)((ushort)Hover.X << 16 | (ushort)Hover.X);
				EnqueueMessage (msg);
			}
		}
		#endregion
		
		#region Private Methods
		private Point ConvertScreenPointToClient (IntPtr handle, Point point)
		{
			NSPoint nspoint = MonoToNativeScreen (point);

			ScreenToClientWindow (handle, ref nspoint);

			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);
			point = NativeToMonoFramed (nspoint, vuWrap.Frame.Size.Height);

//			/*TODO? if (winWrap.contentView() != vuWrap) */ {
//				point -= (Size) hwnd.client_rectangle.Location;
//			}

			return point;
		}

		private Point ConvertClientPointToScreen (IntPtr handle, Point point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);

//			/*TODO? if (winWrap.contentView() != vuWrap) */ {
//				point += (Size) hwnd.client_rectangle.Location;
//			}


			NSPoint nspoint = MonoToNativeFramed (point, vuWrap.Frame.Size.Height);

			ClientWindowToScreen (handle, ref nspoint);
			point = NativeToMonoScreen (nspoint);

			return point;
		}

		private Point ConvertScreenPointToNonClient (IntPtr handle, Point point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView viewWrapper = null;
			if (null != hwnd)
				viewWrapper = MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.WholeWindow) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ConvertScreenPointToNonClient() requires NSView*");

			NSPoint native_point = MonoToNativeScreen (point);
			NSWindow windowWrapper = viewWrapper.Window;

			native_point = windowWrapper.ConvertScreenToBase(native_point);
			native_point = viewWrapper.ConvertPointFromView (native_point, null);

			Point converted_point = NativeToMonoFramed (native_point, viewWrapper.Frame.Size.Height);

			return converted_point;
		}

		private Point ConvertNonClientPointToScreen (IntPtr handle, Point point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView viewWrapper = null;
			if (null != hwnd)
				viewWrapper = MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.WholeWindow) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ConvertScreenPointToNonClient() requires NSView*");

			NSPoint native_point = MonoToNativeFramed (point, viewWrapper.Frame.Size.Height);
			NSWindow windowWrapper = viewWrapper.Window;

			native_point = viewWrapper.ConvertPointToView (native_point, null);
			native_point = windowWrapper.ConvertBaseToScreen(native_point);

			Point converted_point = NativeToMonoScreen (native_point);

			return converted_point;
		}

		private Point ConvertWindowPointToScreen (IntPtr macWindow, Point point)
		{
			NSWindow windowWrapper = MonoMac.ObjCRuntime.Runtime.GetNSObject (macWindow) as NSWindow;
			if (windowWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ConvertWindowPointToScreen() requires NSWindow*");

			NSPoint native_point = MonoToNativeFramed (point, windowWrapper.Frame.Size.Height);

			native_point = windowWrapper.ConvertBaseToScreen(native_point);

			Point converted_point = NativeToMonoScreen (native_point);

			return converted_point;
		}

		private double NextTimeout () {
			DateTime now = DateTime.UtcNow;
			int timeout = 0x7FFFFFF;
			lock (TimerList) {
				foreach (Timer timer in TimerList) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0)
						return 0;
					if (next < timeout)
						timeout = next;
				}
			}
			if (timeout < Timer.Minimum)
				timeout = Timer.Minimum;

			return (double)((double)timeout/1000);
		}
		
		private void CheckTimers (DateTime now) {
			lock (TimerList) {
				int count = TimerList.Count;
				if (count == 0)
					return;
				for (int i = 0; i < TimerList.Count; i++) {
					Timer timer = (Timer) TimerList [i];
					if (timer.Enabled && timer.Expires <= now) {
						// Timer ticks:
						//  - Before MainForm.OnLoad if DoEvents () is called.
						//  - After MainForm.OnLoad if not.
						//
						if (in_doevents ||
						    (Application.MWFThread.Current.Context != null && 
						     Application.MWFThread.Current.Context.MainForm != null && 
						     Application.MWFThread.Current.Context.MainForm.IsLoaded)) {
							timer.FireTick ();
							timer.Update (now);
						}
					}
				}
			}
		}

		private readonly string NSDefaultRunLoopMode = "kCFRunLoopDefaultMode";
		private readonly NSDate distantFuture = NSDate.DistantFuture;

		private bool PumpNativeEvent (bool wait)
		{
			NSDate timeout = NSDate.DistantPast;
				;
			NSApplication NSApp = NSApplication.SharedApplication;
			CheckTimers (DateTime.UtcNow);

			if (wait) 
				if (TimerList.Count == 0)
					timeout = distantFuture;
				else
					timeout = NSDate.FromTimeIntervalSinceNow (NextTimeout ());

			NSEvent evtRef = NSApp.NextEvent (NSEventMask.AnyEvent, timeout, NSDefaultRunLoopMode, true);
			if (evtRef == null)
				return false;

			if (null == Cocoa.EventHandler.EventHandlerDelegate || 
					Cocoa.EventHandledBy.NativeOS == (Cocoa.EventHandledBy.NativeOS & 
					Cocoa.EventHandler.EventHandlerDelegate (evtRef, evtRef, null)))
				NSApp.SendEvent (evtRef);

			return true;
		}

		private void WaitForHwndMessage (Hwnd hwnd, Msg message) {
			MSG msg = new MSG ();

			bool done = false;
			do {
				if (! PeekMessage (null, ref msg, IntPtr.Zero, 0, 0, (uint) PeekMessageFlags.PM_REMOVE))
					break;

				if ((Msg) msg.message == Msg.WM_QUIT) {
					PostQuitMessage (0);
					break;
				}

				if (msg.hwnd == hwnd.Handle) {
					if ((Msg)msg.message == message)
						break;
					else if ((Msg)msg.message == Msg.WM_DESTROY)
						done = true;
				}

				TranslateMessage (ref msg);
				DispatchMessage (ref msg);
			} while (!done);
		}

		private void SendParentNotify(IntPtr child, Msg cause, int x, int y) {
			Hwnd hwnd;
			
			if (child == IntPtr.Zero) {
				return;
			}
			
			hwnd = Hwnd.GetObjectFromWindow (child);
			
			if (hwnd == null) {
				return;
			}
			
			if (hwnd.Handle == IntPtr.Zero) {
				return;
			}
			
			if (ExStyleSet ((int) hwnd.initial_ex_style, WindowExStyles.WS_EX_NOPARENTNOTIFY)) {
				return;
			}
			
			if (hwnd.Parent == null) {
				return;
			}
			
			if (hwnd.Parent.Handle == IntPtr.Zero) {
				return;
			}

			if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY) {
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), child);
			} else {
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
			}
			
			SendParentNotify (hwnd.Parent.Handle, cause, x, y);
		}

		private bool StyleSet (int s, WindowStyles ws) {
			return (s & (int)ws) == (int)ws;
		}

		private bool ExStyleSet (int ex, WindowExStyles exws) {
			return (ex & (int)exws) == (int)exws;
		}

		private void DeriveStyles(int Style, int ExStyle, out FormBorderStyle border_style, out bool border_static, out TitleStyle title_style, out int caption_height, out int tool_caption_height) {

			caption_height = 0;
			tool_caption_height = 0;
			border_static = false;

			if (StyleSet (Style, WindowStyles.WS_CHILD)) {
				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
				} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
					border_static = true;
				} else if (!StyleSet (Style, WindowStyles.WS_BORDER)) {
					border_style = FormBorderStyle.None;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;
				
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					caption_height = 0;
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 0;

					if (StyleSet (Style, WindowStyles.WS_OVERLAPPEDWINDOW) ||
						ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = (FormBorderStyle) 0xFFFF;
					} else {
						border_style = FormBorderStyle.None;
					}
				}

			} else {
				title_style = TitleStyle.None;
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				border_style = FormBorderStyle.None;

				if (StyleSet (Style, WindowStyles.WS_THICKFRAME)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = FormBorderStyle.SizableToolWindow;
					} else {
						border_style = FormBorderStyle.Sizable;
					}
				} else {
					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
							border_static = true;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
							border_style = FormBorderStyle.FixedDialog;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
							border_style = FormBorderStyle.FixedToolWindow;
						} else if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					} else {
						if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					}
				}
			}
		}
		
		private void SetHwndStyles (Hwnd hwnd, CreateParams cp) {
			DeriveStyles (cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.border_static, out hwnd.title_style, 
					out hwnd.caption_height, out hwnd.tool_caption_height);
		}
		
		private void ShowCaret () {
			if (Caret.On)
				return;
			Caret.On = true;
			CaretWindow.OrderFront (CaretWindow);
			Graphics g = Graphics.FromHwnd ((IntPtr) CaretWindow.Handle);

			g.FillRectangle (new SolidBrush (Color.Black), new Rectangle (0, 0, Caret.Width, Caret.Height));

			g.Dispose ();
		}

		private void HideCaret () {
			if (!Caret.On)
				return;
			Caret.On = false;
			CaretWindow.OrderOut (CaretWindow);
		}
		
		private void AccumulateDestroyedHandles (Control c, ArrayList list) {
			if (c != null) {
				Control[] controls = c.Controls.GetAllControls ();

				if (c.IsHandleCreated && !c.IsDisposed) {
					Hwnd hwnd = Hwnd.ObjectFromHandle (c.Handle);

					list.Add (hwnd);
					CleanupCachedWindows (hwnd);
				}

				for (int  i = 0; i < controls.Length; i ++) {
					AccumulateDestroyedHandles (controls[i], list);
				}
			}
		}

		private void CleanupCachedWindows (Hwnd hwnd)
		{
			if (ActiveWindow == hwnd.Handle) {
				SendMessage (hwnd.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
				ActiveWindow = IntPtr.Zero;
			}

			if (FocusWindow == hwnd.Handle) {
				SendMessage (hwnd.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
				FocusWindow = IntPtr.Zero;
			}

			if (Grab.Hwnd == hwnd.Handle) {
				Grab.Hwnd = IntPtr.Zero;
				Grab.Confined = false;
			}

			DestroyCaret (hwnd.Handle);
		}

		private void AddExpose (Hwnd hwnd, bool client, int x, int y, int width, int height) {
			// Don't waste time
			if ((hwnd == null) || (x > hwnd.Width) || (y > hwnd.Height) || ((x + width) < 0) || ((y + height) < 0)) {
				return;
			}

			// Keep the invalid area as small as needed
			if ((x + width) > hwnd.width) {
				width = hwnd.width - x;
			}

			if ((y + height) > hwnd.height) {
				height = hwnd.height - y;
			}

			if (client) {
				hwnd.AddInvalidArea (x, y, width, height);
				if (!hwnd.expose_pending && hwnd.visible) {
					MSG msg = new MSG ();
					msg.message = Msg.WM_PAINT;
					msg.hwnd = hwnd.Handle;
					EnqueueMessage (msg);
					hwnd.expose_pending = true;
				}
			} else {
				hwnd.AddNcInvalidArea (x, y, width, height);
				if (!hwnd.nc_expose_pending && hwnd.visible) {
					MSG msg = new MSG ();
					Region rgn = new Region (hwnd.Invalid);
					IntPtr hrgn = rgn.GetHrgn (null); // Graphics object isn't needed
					msg.message = Msg.WM_NCPAINT;
					msg.wParam = hrgn == IntPtr.Zero ? (IntPtr) 1 : hrgn;
					msg.refobject = rgn;
					msg.hwnd = hwnd.Handle;
					EnqueueMessage (msg);
					hwnd.nc_expose_pending = true;
				}
			}
		}

		private  NSMenu BuildNativeSurrogate (Menu guestMenu)
		{
			NSMenu hostMenu = null != guestMenu.Name ? new NSMenu (guestMenu.Name) : new NSMenu ();

			// In Mono, a sub-Menu is also a MenuItem object. In Cocoa, they are separate objects.
			// Let a sub-Menu keep the NSMenuItem* as its Handle.
			//if (IntPtr.Zero == guestMenu.Handle || ! (guestMenu is MenuItem))
			//	guestMenu.menu_handle = (IntPtr) hostMenu;

			foreach (MenuItem guestItem in guestMenu.MenuItems) {
				string text = guestItem.Text;
				NSMenuItem hostItem = 
					"-" != text ? new Cocoa.MonoMenuItem (guestItem) : NSMenuItem.SeparatorItem;
				hostMenu.AddItem (hostItem);

				if (guestItem.IsParent)
					hostItem.Submenu = BuildNativeSurrogate ((Menu) guestItem);
			}
			return hostMenu;
		}

		private  void HwndPositionToNative (Hwnd hwnd)
		{
			if (hwnd.zero_sized)
				return;

			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			Rectangle mrect = new Rectangle (hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
			NSRect nsrect;

			if (WindowMapping [hwnd.Handle] != null) {
				nsrect = MonoToNativeScreen (mrect);

				NSWindow winWrap = vuWrap.Window;
				nsrect = winWrap.FrameRectFor (nsrect);

				if (winWrap.Frame != nsrect) {
					winWrap.SetFrame (nsrect, false);
					SetCaretPos (hwnd.Handle, Caret.X, Caret.Y);
				}
			} else {
				NSView superVuWrap = vuWrap.Superview;
//				Hwnd parent = hwnd.Parent;

//				if (null != parent) {
//					Point clientOffset = parent.ClientRect.Location;
//					mrect.X += clientOffset.X;
//					mrect.Y += clientOffset.Y;
//				}

				if (superVuWrap != null)
					nsrect = MonoToNativeFramed (mrect, superVuWrap.Frame.Size.Height);
				else
					nsrect = mrect;
				if (vuWrap.Frame != nsrect) {
					vuWrap.Frame = nsrect;
				}
			}
#if DriverDebug
			Console.WriteLine ("HwndToNative ({0}) : {1}", hwnd, nsrect);
#endif
			NSView clientVuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.ClientWindow);
			nsrect = MonoToNativeFramed (hwnd.ClientRect, nsrect.Size.Height);
			clientVuWrap.Frame = nsrect;
		}
		#endregion Private Methods

		#region Override Methods XplatUIDriver
		internal override void RaiseIdle (EventArgs e)
		{
			if (Idle != null)
				Idle (this, e);
		}

		internal override IntPtr InitializeDriver() {
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver (IntPtr token)
		{
			Cocoa.Pasteboard.Application.ReleaseGlobally ();
		}

		internal override void EnableThemes() {
			themes_enabled = true;
		}

		internal override void Activate (IntPtr handle)
		{
			if (ActiveWindow != handle) {
				Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
				NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
				NSWindow winWrap = vuWrap.Window;
				winWrap.MakeKeyAndOrderFront (vuWrap);
				ActiveWindow = handle;

				Menu menu = hwnd.Menu;
				NSMenu hostMenu = null;
				if (null != menu)
					hostMenu = (NSMenu)MonoMac.ObjCRuntime.Runtime.GetNSObject(menu.Handle);
#if DriverDebug
				//Console.WriteLine ("Activate ({0}) {1}", hwnd, hostMenu.Description);
#endif
				// JV
				//NSApplication.SharedApplication.MainMenu = hostMenu;
			}
		}

		internal override void AudibleAlert(AlertType alert)
		{
			//AlertSoundPlay();
		}

		internal override void BeginMoveResize(IntPtr handle)
		{
		}

		internal override void CaretVisible (IntPtr hwnd, bool visible) {
			if (Caret.Hwnd == hwnd) {
				if (visible) {
					if (Caret.Visible < 1) {
						Caret.Visible++;
						Caret.On = false;
						if (Caret.Visible == 1) {
							ShowCaret ();
							Caret.Timer.Start ();
						}
					}
				} else {
					Caret.Visible--;
					if (Caret.Visible == 0) {
						Caret.Timer.Stop ();
						HideCaret ();
					}
				}
			}
		}
		
		internal override bool CalculateWindowRect (ref Rectangle ClientRect, CreateParams cp, Menu menu, 
							    out Rectangle WindowRect) {
			WindowRect = Hwnd.GetWindowRectangle (cp, menu, ClientRect);
			return true;
		}

		internal override void ClientToScreen (IntPtr handle, ref int x, ref int y)
		{
			Point point = new Point (x, y);
			point = ConvertClientPointToScreen (handle, point);

			x = (int) point.X;
			y = (int) point.Y;
		}

		internal override void MenuToScreen (IntPtr handle, ref int x, ref int y)
		{
//			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = GetMenuOrigin (handle);
			point.X += x;
			point.Y += y;
			point = ConvertNonClientPointToScreen (handle, point);

			x = point.X;
			y = point.Y;
		}

		internal override int[] ClipboardAvailableFormats (IntPtr handle) {
			ArrayList list = new ArrayList ();
			DataFormats.Format f = DataFormats.Format.List;

			while (f != null) {
				list.Add (f.Id);
				f = f.Next;
			}

			return (int []) list.ToArray (typeof (int));
		}

		internal override void ClipboardClose (IntPtr handle) {
		}

		[MonoTODO ("Map our internal formats to the right os code where we can")]
		internal override int ClipboardGetID(IntPtr handle, string format)
		{
			//return (int) (IntPtr) __CFStringMakeConstantString (format);
			return 0;
		}

		internal override IntPtr ClipboardOpen(bool primary_selection) {
			if (primary_selection)
				return Cocoa.Pasteboard.Primary.Handle;
			return Cocoa.Pasteboard.Application.Handle;
		}

		internal override object ClipboardRetrieve (IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			return Cocoa.Pasteboard.Retrieve ((NSPasteboard)MonoMac.ObjCRuntime.Runtime.GetNSObject(handle), type);
		}

		internal override void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter, bool copy)
		{
			Cocoa.Pasteboard.Store ((NSPasteboard)MonoMac.ObjCRuntime.Runtime.GetNSObject(handle), obj, type);
		}
		
		internal override void CreateCaret (IntPtr hwnd, int width, int height) {
			if (Caret.Hwnd != IntPtr.Zero)
				DestroyCaret (Caret.Hwnd);

			Caret.Hwnd = hwnd;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = 0;
			Caret.On = false;
		}

		internal override IntPtr CreateWindow (CreateParams cp)
		{
			Hwnd hwnd;
			Hwnd parent_hwnd = null;
			int X;
			int Y;
			int Width;
			int Height;
//			IntPtr ParentHandle;
			IntPtr WindowHandle;
			IntPtr wholeHandle;
			IntPtr clientHandle;
//			IntPtr WholeWindowTracking;
//			IntPtr ClientWindowTracking;

			hwnd = new Hwnd ();

			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;
//			ParentHandle = IntPtr.Zero;
			WindowHandle = IntPtr.Zero;
			wholeHandle = IntPtr.Zero;
			clientHandle = IntPtr.Zero;
//			WholeWindowTracking = IntPtr.Zero;
//			ClientWindowTracking = IntPtr.Zero;
			NSView ParentWrapper = null;  // If any
			NSWindow windowWrapper = null;

			if (Width < 1) Width = 1;
			if (Height < 1) Height = 1;

			if (cp.Parent != IntPtr.Zero) {
				parent_hwnd = Hwnd.ObjectFromHandle (cp.Parent);
//				ParentHandle = parent_hwnd.client_window;
				ParentWrapper = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(parent_hwnd.ClientWindow);
				if (StyleSet (cp.Style, WindowStyles.WS_CHILD))
					windowWrapper = ParentWrapper.Window;
//				wholeHandle = parent_hwnd.WholeWindow;
			}
//			else {
//				if (StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
//					HIViewFindByID (HIViewGetRoot (FosterParent), new Cocoa.HIViewID (Cocoa.EventHandler.kEventClassWindow, 1), ref ParentHandle);
//				}
//			}

			Point next;
			if (cp.control is Form) {
				next = Hwnd.GetNextStackedFormLocation (cp, parent_hwnd);
				X = next.X;
				Y = next.Y;
			}

			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.Parent = Hwnd.ObjectFromHandle (cp.Parent);
			hwnd.initial_style = cp.WindowStyle;
			hwnd.initial_ex_style = cp.WindowExStyle;
			hwnd.visible = false;

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				hwnd.enabled = false;
			}

			clientHandle = IntPtr.Zero;

			Size QWindowSize = TranslateWindowSizeToQuartzWindowSize (cp);
			Rectangle mWholeRect = new Rectangle (new Point (X, Y), QWindowSize);
			NSRect WholeRect;
			if (StyleSet (cp.Style, WindowStyles.WS_CHILD) && null != parent_hwnd)
			{
//				mWholeRect.Location += (Size) parent_hwnd.ClientRect.Location;
				WholeRect = MonoToNativeFramed (mWholeRect, ParentWrapper.Frame.Size.Height);
			} else {
				WholeRect = MonoToNativeScreen (mWholeRect);
			}

			NSView viewWrapper = new Cocoa.MonoView (this, WholeRect);
			wholeHandle = (IntPtr) viewWrapper.Handle;

			SetHwndStyles(hwnd, cp);
/* FIXME */
			if (!StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
//				IntPtr WindowView = IntPtr.Zero;
//				IntPtr GrowBox = IntPtr.Zero;
//				Cocoa.WindowClass windowklass = Cocoa.WindowClass.kOverlayWindowClass;
				NSWindowStyle attributes = NSWindowStyle.Borderless;
				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX)) {
					attributes |= NSWindowStyle.Miniaturizable | NSWindowStyle.Titled;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX)) {
					attributes |= NSWindowStyle.Resizable | NSWindowStyle.Titled;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU)) {
					attributes |= NSWindowStyle.Closable | NSWindowStyle.Titled;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
					attributes |= NSWindowStyle.Titled;
				}
//				if (hwnd.border_style == FormBorderStyle.FixedToolWindow) {
//					windowklass = Cocoa.WindowClass.kUtilityWindowClass;
//				} else if (hwnd.border_style == FormBorderStyle.SizableToolWindow) {
//					attributes |= Cocoa.WindowAttributes.kWindowResizableAttribute;
//					windowklass = Cocoa.WindowClass.kUtilityWindowClass;
//				}
//				if (windowklass == Cocoa.WindowClass.kOverlayWindowClass) {
//					attributes = Cocoa.WindowAttributes.kWindowCompositingAttribute | Cocoa.WindowAttributes.kWindowStandardHandlerAttribute;
//				}
//				attributes |= Cocoa.WindowAttributes.kWindowLiveResizeAttribute;

//				Cocoa.Rect rect = new Cocoa.Rect ();
//				if (StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
//					SetRect (ref rect, (short)X, (short)(Y), (short)(X + QWindowSize.Width), (short)(Y + QWindowSize.Height));
//				} else {
//					SetRect (ref rect, (short)X, (short)(Y + MenuBarHeight), (short)(X + QWindowSize.Width), (short)(Y + MenuBarHeight + QWindowSize.Height));
//				}

//				CreateNewWindow (windowklass, attributes, ref rect, ref WindowHandle);
//
//				HIViewFindByID (HIViewGetRoot (WindowHandle), new Cocoa.HIViewID (Cocoa.EventHandler.kEventClassWindow, 1), ref WindowView);
//				HIViewFindByID (HIViewGetRoot (WindowHandle), new Cocoa.HIViewID (Cocoa.EventHandler.kEventClassWindow, 7), ref GrowBox);
//				HIGrowBoxViewSetTransparent (GrowBox, true);
//				SetAutomaticControlDragTrackingEnabledForWindow (, true);
//				ParentHandle = WindowView;
				windowWrapper = new MonoWindow(WholeRect, attributes, NSBackingStore.Buffered, true);
				WindowHandle = (IntPtr) windowWrapper.Handle;
//				wholeHandle = WindowHandle;
				windowWrapper.ContentView = viewWrapper;
				//WholeRect = windowWrapper.FrameRectFor (windowWrapper.Frame);
				/*Rectangle realFrame = NativeToMonoScreen (WholeRect);
				hwnd.x = X = realFrame.X;
				hwnd.y = Y = realFrame.Y;
				hwnd.width = Width = realFrame.Width;
				hwnd.height = Height = realFrame.Height;*/

				//				Cocoa.EventHandler.InstallWindowHandler (WindowHandle);
				windowWrapper.WeakDelegate = windowWrapper;
				//				ClientWrapper.addTrackingRect_owner_userData_assumeInside (rect, ClientWrapper, 0, false);

				if (StyleSet (cp.Style, WindowStyles.WS_POPUP))
					windowWrapper.Level = NSWindowLevel.PopUpMenu;
				if (ParentWrapper != null)
					ParentWrapper.Window.AddChildWindow (windowWrapper, NSWindowOrderingMode.Above);

//				NSButton cb = windowWrapper.standardWindowButton (Enums.NSWindowCloseButton);
//				if (! NSObject.IsNullOrNil (cb)) {
//					// cb.setAction ("performClose:");
//					// cb.setTarget (viewWrapper);
//					Console.Error.WriteLine ("New window {0}.{1} ()", cb.target (), cb.action ());
//				}
			}else if (ParentWrapper != null) {
				ParentWrapper.AddSubview (viewWrapper);
			}

//			Cocoa.EventHandler.InstallControlHandler (wholeHandle);
//			Cocoa.EventHandler.InstallControlHandler (clientHandle);

			// Enable embedding on controls
//			HIViewChangeFeatures (wholeHandle, 1<<1, 0);
//			HIViewChangeFeatures (clientHandle, 1<<1, 0);

//			HIViewNewTrackingArea (wholeHandle, IntPtr.Zero, (UInt64)wholeHandle, ref WholeWindowTracking);
			Rectangle QClientRect = TranslateClientRectangleToQuartzClientRectangle (hwnd, cp.control);
			NSRect ClientRect = MonoToNativeFramed (QClientRect, WholeRect.Size.Height);
			NSView clientWrapper = new Cocoa.MonoView (this, ClientRect);
			clientHandle = (IntPtr) clientWrapper.Handle;

			hwnd.WholeWindow = wholeHandle;
			hwnd.ClientWindow = clientHandle;
//			hwnd.UserData = viewWrapper;

			viewWrapper.AddSubview (clientWrapper);
			viewWrapper.AddTrackingRect (WholeRect, viewWrapper, IntPtr.Zero, false);
			clientWrapper.AddTrackingRect(ClientRect, clientWrapper, IntPtr.Zero, false);
//			Cocoa.HIRect WholeRect;
//			if (WindowHandle != IntPtr.Zero) {
//				WholeRect = new Cocoa.HIRect (0, 0, QWindowSize.Width, QWindowSize.Height);
//			} else {
//				WholeRect = new Cocoa.HIRect (X, Y, QWindowSize.Width, QWindowSize.Height);
//			}
//			HIViewSetFrame (wholeHandle, ref WholeRect);
//			HIViewSetFrame (clientHandle, ref ClientRect);

			if (WindowHandle != IntPtr.Zero) {
				WindowMapping [hwnd.Handle] = WindowHandle;
				HandleMapping [WindowHandle] = hwnd.Handle;
				if (hwnd.border_style == FormBorderStyle.FixedToolWindow || 
				    hwnd.border_style == FormBorderStyle.SizableToolWindow) {
					UtilityWindows.Add (windowWrapper);
				}
			}

			// Assign handle to control's native window before sending messages.
			if (null != cp.control) {
				cp.control.window.AssignHandle (hwnd.Handle);
#if DriverDebug
				if ("StackTraceMe" == cp.control.Name)
					Console.WriteLine ("{0}", new StackTrace (true));
#endif
			}

//			Dnd.SetAllowDrop (hwnd, true);

			Text (hwnd.Handle, cp.Caption);
			
			SendMessage (hwnd.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			SendParentNotify (hwnd.Handle, Msg.WM_CREATE, int.MaxValue, int.MaxValue);

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				if (WindowHandle != IntPtr.Zero) {
					if (Control.FromHandle(hwnd.Handle) is Form) {
						Form f = Control.FromHandle(hwnd.Handle) as Form;
						if (f.WindowState == FormWindowState.Normal) {
							SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
						}
					}
					windowWrapper.OrderFront (viewWrapper);
					//WaitForHwndMessage (hwnd, Msg.WM_SHOWWINDOW);
				}

				viewWrapper.Hidden = false;
				clientWrapper.Hidden = false;
				hwnd.visible = true;
				if (! (Control.FromHandle (hwnd.Handle) is Form)) {
					SendMessage (hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
				}
			}

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState (hwnd.Handle, FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState (hwnd.Handle, FormWindowState.Maximized);
			}

			InvalidateNC (hwnd.Handle);
			if (cp.Width > 0 && cp.Height > 0)
				Invalidate (hwnd.Handle, Rectangle.Empty, true);

			return hwnd.Handle;
		}

		internal override IntPtr CreateWindow (IntPtr Parent, int X, int Y, int Width, int Height) {
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName= XplatUI.GetDefaultClassName(GetType());
			create_params.ClassStyle = 0;
			create_params.ExStyle=0;
			create_params.Parent=IntPtr.Zero;
			create_params.Param=0;

			return CreateWindow(create_params);
		}

		internal override Bitmap DefineStdCursorBitmap (StdCursor id) {
			return Cocoa.Cursor.DefineStdCursorBitmap (id);
		}

		internal override IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, 
							int xHotSpot, int yHotSpot) {
			return Cocoa.Cursor.DefineCursor (bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);
		}
		
		internal override IntPtr DefineStdCursor (StdCursor id) {
			return Cocoa.Cursor.DefineStdCursor (id);
		}
		
		internal override IntPtr DefWndProc (ref Message msg) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.HWnd);
			switch ((Msg) msg.Msg) {
				case Msg.WM_QUIT: {
					if (WindowMapping [hwnd.Handle] != null)

						Exit ();
					break;
				}
				case Msg.WM_PAINT: {
					hwnd.expose_pending = false;
					break;
				}
				case Msg.WM_NCPAINT: {
					hwnd.nc_expose_pending = false;
					break;
				}  
				case Msg.WM_NCCALCSIZE: {
					if (msg.WParam == (IntPtr)1) {
						XplatUIWin32.NCCALCSIZE_PARAMS ncp;
						ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)
							Marshal.PtrToStructure (msg.LParam, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

						// Add all the stuff X is supposed to draw.
						Control ctrl = Control.FromHandle (hwnd.Handle);
						if (ctrl != null) {
							Hwnd.Borders rect = Hwnd.GetBorders (ctrl.GetCreateParams (), null);

							ncp.rgrc1.top += rect.top;
							ncp.rgrc1.bottom -= rect.bottom;
							ncp.rgrc1.left += rect.left;
							ncp.rgrc1.right -= rect.right;

							Marshal.StructureToPtr (ncp, msg.LParam, true);
						}
					}
					break;
				}
				case Msg.WM_SETCURSOR: {
					// Pass to parent window first
					while ((null != hwnd.parent) && (IntPtr.Zero == msg.Result)) {
						hwnd = hwnd.parent;
						msg.Result = 
							NativeWindow.WndProc (hwnd.Handle, Msg.WM_SETCURSOR, msg.HWnd, msg.LParam);
					}

					if (IntPtr.Zero == msg.Result) {
						IntPtr handle;

						switch ((HitTest) (msg.LParam.ToInt32 () & 0xffff)) {
							case HitTest.HTBOTTOM:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTBORDER:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTBOTTOMLEFT:	handle = Cursors.SizeNESW.handle; break;
							case HitTest.HTBOTTOMRIGHT:	handle = Cursors.SizeNWSE.handle; break;
							case HitTest.HTERROR:
								if ((msg.LParam.ToInt32() >> 16) == (int)Msg.WM_LBUTTONDOWN) {
									//FIXME: AudibleAlert();
								}
								handle = Cursors.Default.handle;
								break;

							case HitTest.HTHELP:		handle = Cursors.Help.handle; break;
							case HitTest.HTLEFT:		handle = Cursors.SizeWE.handle; break;
							case HitTest.HTRIGHT:		handle = Cursors.SizeWE.handle; break;
							case HitTest.HTTOP:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTTOPLEFT:		handle = Cursors.SizeNWSE.handle; break;
							case HitTest.HTTOPRIGHT:	handle = Cursors.SizeNESW.handle; break;

							#if SameAsDefault
							case HitTest.HTGROWBOX:
							case HitTest.HTSIZE:
							case HitTest.HTZOOM:
							case HitTest.HTVSCROLL:
							case HitTest.HTSYSMENU:
							case HitTest.HTREDUCE:
							case HitTest.HTNOWHERE:
							case HitTest.HTMAXBUTTON:
							case HitTest.HTMINBUTTON:
							case HitTest.HTMENU:
							case HitTest.HSCROLL:
							case HitTest.HTBOTTOM:
							case HitTest.HTCAPTION:
							case HitTest.HTCLIENT:
							case HitTest.HTCLOSE:
							#endif
							default: handle = Cursors.Default.handle; break;
						}
						SetCursor (msg.HWnd, handle);
					}
					return (IntPtr) 1;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret (IntPtr hwnd) {
			if (Caret.Hwnd == hwnd) {
				if (Caret.Visible == 1) {
					Caret.Timer.Stop ();
					HideCaret ();
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = 0;
				Caret.On = false;
			}
		}
		
		[MonoTODO]
		internal override void DestroyCursor (IntPtr cursor) {
			throw new NotImplementedException ();
		}
	
		internal override void DestroyWindow (IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle (handle);

			if (null == hwnd) {
				return;
			}
				
			SendParentNotify (hwnd.Handle, Msg.WM_DESTROY, int.MaxValue, int.MaxValue);
				
			CleanupCachedWindows (hwnd);

			ArrayList windows = new ArrayList ();

			AccumulateDestroyedHandles (Control.ControlNativeWindow.ControlFromHandle (hwnd.Handle), windows);

			foreach (Hwnd h in windows) {
				SendMessage (h.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
				h.zombie = true;
			}

			foreach (Hwnd h in windows) {
				h.UserData = null;
				object wh = WindowMapping [h.Handle];
				if (null != wh) { 
					NSWindow winWrap = (NSWindow)MonoMac.ObjCRuntime.Runtime.GetNSObject((IntPtr) wh);
					winWrap.Close ();
					WindowMapping.Remove (h.Handle);
					HandleMapping.Remove ((IntPtr) wh);
				} else {
					NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(h.WholeWindow);
					vuWrap.RemoveFromSuperviewWithoutNeedingDisplay ();
				}

				h.Dispose ();
			}
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DoEvents() {
                        MSG     msg = new MSG ();

			in_doevents = true;
			while (PeekMessage (null, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
                                TranslateMessage (ref msg);
                                DispatchMessage (ref msg);
                        }
			in_doevents = false;

		}

		internal override void EnableWindow (IntPtr handle, bool Enable) {
			//Like X11 we need not do anything here
		}

		internal override void EndLoop (Thread thread) {
		}

		internal void Exit () {
			GetMessageResult = false;
		}
		
		internal override IntPtr GetActive () {
			return ActiveWindow;
		}

		internal override Region GetClipRegion (IntPtr hwnd) {
			return null;
		}

		[MonoTODO]
		internal override void GetCursorInfo (IntPtr cursor, out int width, out int height, out int hotspot_x, 
							out int hotspot_y)
		{
			width = 12;
			height = 12;
			hotspot_x = 0;
			hotspot_y = 0;
		}

		internal override void GetDisplaySize (out Size size)
		{
			// NSScreen.mainScreen () returns the screen the the user is currently interacting with.
			// To get the screen identified by CGMainDisplayID (), get the 0th element of this array.
			var screens = NSScreen.Screens;
			if (screens != null && 0 < screens.Length) {
				NSScreen screenWrap = (NSScreen) screens[0];
				NSRect bounds = screenWrap.Frame;
				size = new Size ((int) bounds.Size.Width, (int) bounds.Size.Height);
			} else {
				size = Size.Empty;
			}
		}

		internal override IntPtr GetParent (IntPtr handle)
		{
			Hwnd	hwnd = Hwnd.ObjectFromHandle (handle);

			if (null != hwnd && null != hwnd.Parent) {
				return hwnd.Parent.Handle;
			}
			return IntPtr.Zero;
		}

		internal override IntPtr GetPreviousWindow (IntPtr handle)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			NSView superWrap = vuWrap.Superview;
			if (superWrap == null)
				return IntPtr.Zero;

			NSView[] subsWrap = superWrap.Subviews;
			int index = Array.IndexOf (subsWrap, vuWrap);
			if (1 > index)
				return IntPtr.Zero;

			return (IntPtr) subsWrap[index - 1].Handle;
		}
		
		internal override void GetCursorPos (IntPtr handle, out int x, out int y)
		{
			NSPoint nspt = NSEvent.CurrentMouseLocation;
			Point pt = NativeToMonoScreen (nspt);
			x = (int) pt.X;
			y = (int) pt.Y;
		}

		internal override IntPtr GetFocus() {
			return FocusWindow;
		}

		
		internal override bool GetFontMetrics (Graphics g, Font font, out int ascent, out int descent) {
			FontFamily ff = font.FontFamily;
			ascent = ff.GetCellAscent (font.Style);
			descent = ff.GetCellDescent (font.Style);
			return true;
		}
		
		internal override Point GetMenuOrigin (IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}

		internal override bool GetMessage (object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax)
		{
			CheckTimers (DateTime.UtcNow);
			bool pumpedNativeEvent = PumpNativeEvent (false);
			int count = 0;

			do {
				CheckTimers (DateTime.UtcNow);

				lock (queuelock) {
					count = MessageQueue.Count;
					if (0 < count) {
						object queueobj;
						queueobj = MessageQueue.Dequeue ();

						if (! (queueobj is GCHandle)) {
							msg = (MSG) queueobj;
							break;
						}

						XplatUIDriverSupport.ExecuteClientMessage ((GCHandle) queueobj);
					}
				}

				bool atIdle = ! pumpedNativeEvent && 0 >= count;
				if (atIdle) 
					RaiseIdle (EventArgs.Empty);

				pumpedNativeEvent = PumpNativeEvent (atIdle);
			} while (GetMessageResult);

			return GetMessageResult;
		}

		[MonoTODO]
		internal override bool GetText (IntPtr handle, out string text) {
			throw new NotImplementedException ();
		}

		internal override void GetWindowPos (IntPtr handle, bool is_toplevel, out int x, out int y, out int width, 
							out int height, out int client_width, out int client_height)
		{
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				x = hwnd.x;
				y = hwnd.y;
				width = hwnd.width;
				height = hwnd.height;

				PerformNCCalc(hwnd);

				client_width = hwnd.ClientRect.Width;
				client_height = hwnd.ClientRect.Height;

				return;
			}

			// Should we throw an exception or fail silently?
			// throw new ArgumentException("Called with an invalid window handle", "handle");

			x = 0;
			y = 0;
			width = 0;
			height = 0;
			client_width = 0;
			client_height = 0;
		}

		internal override FormWindowState GetWindowState (IntPtr handle)
		{
			Hwnd		hwnd	= Hwnd.ObjectFromHandle (handle);
			NSView		vuWrap	= (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			NSWindow	winWrap	= vuWrap.Window;

			if (winWrap.IsMiniaturized)
				return FormWindowState.Minimized;
			if (winWrap.IsZoomed)
				return FormWindowState.Maximized;

			return FormWindowState.Normal;
		}

		internal override void GrabInfo (out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea) {
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}
		
		internal override void GrabWindow (IntPtr handle, IntPtr confine_to_handle) {
			Grab.Hwnd = handle;
			Grab.Confined = confine_to_handle != IntPtr.Zero;
			/* FIXME: Set the Grab.Area */
		}
		
		internal override void UngrabWindow (IntPtr hwnd) {
			bool was_grabbed = Grab.Hwnd != IntPtr.Zero;

			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;

			if (was_grabbed) {
				// lparam should be the handle to the window gaining the mouse capture,
				// but we dont have that information like X11.
				// Also only generate WM_CAPTURECHANGED if the window actually was grabbed.
				SendMessage (hwnd, Msg.WM_CAPTURECHANGED, IntPtr.Zero, IntPtr.Zero);
			}
		}
		
		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}
		
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (clear) {
				AddExpose (hwnd, true, 0, 0, hwnd.Width, hwnd.Height);
			} else {
				AddExpose (hwnd, true, rc.X, rc.Y, rc.Width, rc.Height);
			} 
		}

		internal override void InvalidateNC (IntPtr handle)
		{
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			AddExpose (hwnd, false, 0, 0, hwnd.Width, hwnd.Height); 
		}
		
		internal override bool IsEnabled(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).Enabled;
		}
		
		internal override bool IsVisible(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).visible;
		}
		
		internal override void KillTimer(Timer timer) {
			lock (TimerList) {
				TimerList.Remove(timer);
			}
		}


		internal override void OverrideCursor (IntPtr cursor) {
		}

		internal override PaintEventArgs PaintEventStart (ref Message msg, IntPtr handle, bool client)
		{
			PaintEventArgs	paint_event;
			Hwnd		hwnd;
			Hwnd		paint_hwnd; 

			hwnd = Hwnd.ObjectFromHandle (msg.HWnd);
			if (msg.HWnd == handle) {
				paint_hwnd = hwnd;
			} else {
				paint_hwnd = Hwnd.ObjectFromHandle (handle);
			}

			Control control = Control.FromHandle (paint_hwnd.Handle);
			if (null != control && "StackTraceMe" == control.Name) {
				Console.WriteLine ("PaintEventStart ({0}, 0x{1:X} [{2}] [{3}], {4}) ", 
							msg, handle.ToInt32 (), paint_hwnd, control, client);
				Console.WriteLine (" {0}", new StackTrace (1, true));
			}

			if (Caret.Visible == 1) {
				Caret.Paused = true;
				HideCaret();
			}

			Graphics dc;

			if (client) {
				dc = Graphics.FromHwnd (paint_hwnd.ClientWindow);


				if (null == dc)
					return null;

				Region clip_region = new Region ();
				clip_region.MakeEmpty ();

				foreach (Rectangle r in hwnd.ClipRectangles) {
					clip_region.Union (r);
				}

				if (hwnd.UserClip != null) {
					clip_region.Intersect (hwnd.UserClip);
				}

//				// Translate & clip drawing to the client area.
//				Rectangle clientRect = hwnd.ClientRect;
//				dc.TranslateTransform (clientRect.X, clientRect.Y);
//				clientRect.Location = Point.Empty;
//				clip_region.Intersect (clientRect);

				// FIXME: Clip region is hosed
				dc.Clip = clip_region;
				paint_event = new PaintEventArgs (dc, hwnd.Invalid);
				//hwnd.expose_pending = false;
				hwnd.ClearInvalidArea ();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			} else {
				dc = Graphics.FromHwnd (paint_hwnd.WholeWindow);

				if (null == dc)
					return null;

				if (!hwnd.NCInvalid.IsEmpty) {
					// FIXME: Clip region is hosed
					dc.SetClip (hwnd.NCInvalid);
					paint_event = new PaintEventArgs (dc, hwnd.NCInvalid);
				} else {
					paint_event = new PaintEventArgs (dc, new Rectangle (0, 0, hwnd.width, hwnd.height));
				}

//				// Clip drawing to exclude the client area.
//				dc.ExcludeClip (hwnd.ClientRect);

				//hwnd.nc_expose_pending = false;
				hwnd.ClearNcInvalidArea ();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			}

			return paint_event;
		}

		internal override void PaintEventEnd (ref Message msg, IntPtr handle, bool client)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			// FIXME: Pop is causing invalid stack ops sometimes; race condition?
			try {
				Graphics dc = (Graphics)hwnd.drawing_stack.Pop();
				dc.Flush ();
				dc.Dispose ();
			
				PaintEventArgs pe = (PaintEventArgs)hwnd.drawing_stack.Pop();
				pe.SetGraphics (null);
				pe.Dispose ();  
			} catch {}

			if (Caret.Visible == 1) {
				ShowCaret();
				Caret.Paused = false;
			}
		}

		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, 
							int wFilterMin, int wFilterMax, uint flags)
		{
			bool pumpedNativeEvent = true;
			int count = 0;
			bool peeking = 0 == ((uint) PeekMessageFlags.PM_REMOVE & flags);

			do {
				CheckTimers (DateTime.UtcNow);
				pumpedNativeEvent = PumpNativeEvent (false);

				lock (queuelock) {
					count = MessageQueue.Count;
					if (0 >= count)
						continue;

					object queueobj;
					if (peeking)
						queueobj = MessageQueue.Peek ();
					else
						queueobj = MessageQueue.Dequeue ();

					if (queueobj is GCHandle) {
						if (peeking)
							queueobj = MessageQueue.Dequeue ();

						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)queueobj);
						continue;
					}

					msg = (MSG)queueobj;
					return true;
				}
			} while (pumpedNativeEvent || 0 < count);

			return false;
		}

		internal override bool PostMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			MSG msg = new MSG ();
			msg.hwnd = hwnd;
			msg.message = message;
			msg.wParam = wParam;
			msg.lParam = lParam;
			EnqueueMessage (msg);
			return true;
		}

		internal override void PostQuitMessage (int exitCode)
		{
			NSWindow winWrap = NSApplication.SharedApplication.MainWindow;
			if (winWrap != null) {
				object vuHndl = HandleMapping [(IntPtr) winWrap.Handle];
				if (null != vuHndl)
					PostMessage ((IntPtr) vuHndl, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
			}

			PostMessage (IntPtr.Zero, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
		}

		internal override void RequestAdditionalWM_NCMessages (IntPtr hwnd, bool hover, bool leave) {
		}

		internal override void RequestNCRecalc (IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null) {
				return;
			}

			PerformNCCalc (hwnd);
			SendMessage (handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateNC (handle);
		}

		[MonoTODO]
		internal override void ResetMouseHover (IntPtr handle) {
			throw new NotImplementedException ();
		}

		internal override void ScreenToClient (IntPtr handle, ref int x, ref int y)
		{
			Point point = new Point (x, y);
			point = ConvertScreenPointToClient (handle, point);

			x = (int) point.X;
			y = (int) point.Y;
		}

		internal override void ScreenToMenu (IntPtr handle, ref int x, ref int y)
		{
//			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertScreenPointToNonClient (handle, new Point (x, y));
			Point mo = GetMenuOrigin (handle);
			point -= (Size) mo;

			x = point.X;
			y = point.Y;
		}

		internal override void ScrollWindow (IntPtr handle, Rectangle area, int XAmount, int YAmount, bool clear) {
			/*
			 * This used to use a HIViewScrollRect but this causes issues with the fact that we dont coalesce
			 * updates properly with our short-circuiting of the window manager.  For now we'll do a less
			 * efficient invalidation of the entire handle which appears to fix the problem
			 * see bug #381084
			 */
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Invalidate (handle, new Rectangle (0, 0, hwnd.Width, hwnd.Height), false);
		}
		
		
		internal override void ScrollWindow (IntPtr handle, int XAmount, int YAmount, bool clear) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Invalidate (handle, new Rectangle (0, 0, hwnd.Width, hwnd.Height), false);
		}
		
		[MonoTODO]
		internal override void SendAsyncMethod (AsyncMethodData method) {
			// Fake async
			lock (queuelock) {
				MessageQueue.Enqueue (GCHandle.Alloc (method));
			}
		}

		[MonoTODO]
		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			return NativeWindow.WndProc (hwnd, message, wParam, lParam);
		}
		
		internal override int SendInput (IntPtr hwnd, Queue keys) {
			return 0;
		}

		internal override void SetCaretPos (IntPtr handle, int x, int y)
		{
			if (IntPtr.Zero != handle && handle == Caret.Hwnd) {
				Caret.X = x;
				Caret.Y = y;
//				ClientToScreen (handle, ref x, ref y);
//				CaretWindow.setFrame_display (new NSRect (x, y, Caret.Width, Caret.Height), 0 < Caret.Visible);
				PositionWindowInClient (Caret.rect, CaretWindow, handle);
				Caret.Timer.Stop ();
				HideCaret ();
				if (0 < Caret.Visible) {
					ShowCaret ();
					Caret.Timer.Start ();
				}
			}
		}

		internal override void SetClipRegion (IntPtr hwnd, Region region) {
			throw new NotImplementedException();
		}
		
		internal override void SetCursor (IntPtr window, IntPtr cursor) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (window);

			hwnd.Cursor = cursor;
		}

		internal override void SetCursorPos (IntPtr handle, int x, int y)
		{
			var screens = NSScreen.Screens;

			if (screens != null && 0 < screens.Length) {
				NSScreen screenWrap = (NSScreen)screens[0];
				NSDictionary description = screenWrap.DeviceDescription;
				NSNumber screenNumber = (NSNumber) description["NSScreenNumber"];
// FIXME: Find a Cocoa way to do this.
				CGDisplayMoveCursorToPoint (screenNumber.UnsignedIntegerValue, new Cocoa.CGPoint (x, y));
			}
		}

		internal override void SetFocus (IntPtr handle) {
			if (FocusWindow != IntPtr.Zero) {
				PostMessage (FocusWindow, Msg.WM_KILLFOCUS, handle, IntPtr.Zero);
			}
			PostMessage (handle, Msg.WM_SETFOCUS, FocusWindow, IntPtr.Zero);
			FocusWindow = handle;
		}

		internal override void SetIcon (IntPtr handle, Icon icon)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			// FIXME: we need to map the icon for active window switches
			if (WindowMapping [hwnd.Handle] != null) {
				if (icon == null) { 
					NSApplication.SharedApplication.ApplicationIconImage = null;
				} else {
					Bitmap		bitmap;
					int		size;
					int		index;
	
					bitmap = new Bitmap (128, 128);
					using (Graphics g = Graphics.FromImage (bitmap)) {
						g.DrawImage (icon.ToBitmap (), 0, 0, 128, 128);
					}
					index = 0;
					size = bitmap.Width * bitmap.Height * 4;
					byte[]		bytes = new byte[size];
	
					for (int y = 0; y < bitmap.Height; ++y) {
						for (int x = 0; x < bitmap.Width; ++x) {
							Color pixel = bitmap.GetPixel (x, y);
							bytes[index++] = pixel.A;
							bytes[index++] = pixel.R;
							bytes[index++] = pixel.G;
							bytes[index++] = pixel.B;
						}
					}

					NSData data = NSData.FromArray (bytes);
					NSImage image = new NSImage(data);
					NSApplication.SharedApplication.ApplicationIconImage = image;
				}
			}
		}

		
		internal override void SetModal (IntPtr handle, bool Modal)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			NSWindow winWrap = vuWrap.Window;
			if (Modal)
				ModalSessions.Push (NSApplication.SharedApplication.BeginModalSession (winWrap));
			else
				NSApplication.SharedApplication.EndModalSession (ModalSessions.Pop ());
			return;
		}

		internal override IntPtr SetParent (IntPtr handle, IntPtr parent)
		{
//			IntPtr ParentHandle = IntPtr.Zero;
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Hwnd newParent = Hwnd.ObjectFromHandle (parent);
			NSView newParentWrap = null != newParent ? (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(newParent.ClientWindow) : null;
			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			NSWindow winWrap = (NSWindow) vuWrap.Window;

			if (winWrap != null && winWrap.ContentView == vuWrap) {
				NSWindow parentWinWrap = winWrap.ParentWindow;
				if (parentWinWrap != null)
					parentWinWrap.RemoveChildWindow (winWrap);

				hwnd.Parent = newParent;
				if (newParentWrap != null) {
					parentWinWrap = newParentWrap.Window;
					if (parentWinWrap != null)
						parentWinWrap.AddChildWindow (winWrap, NSWindowOrderingMode.Above);
				}
			} else {
				bool adoption = vuWrap.Superview != null;
				if (adoption) {
					vuWrap.Retain ();
					vuWrap.RemoveFromSuperview ();
				}

				hwnd.Parent = newParent;
				if (newParentWrap != null) {
					newParentWrap.AddSubview (vuWrap);
					vuWrap.Frame = MonoToNativeFramed (new Rectangle (hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height), newParentWrap.Frame.Height);
					if (adoption)
						vuWrap.Release ();
				}
			}

			return IntPtr.Zero;
		}

		internal override void SetTimer (Timer timer) {
			lock (TimerList) {
				TimerList.Add (timer);
			}
		}

		internal override bool SetTopmost (IntPtr hWnd, bool Enabled)
		{
			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hWnd);
			NSWindow winWrap = vuWrap.Window;
			if (winWrap != null)
				winWrap.Level = Enabled ? NSWindowLevel.Floating : NSWindowLevel.Normal;
			return true;
		}

		internal override bool SetOwner (IntPtr hWnd, IntPtr hWndOwner) {
			// TODO: Set window owner. 
			return true;
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
			NSWindow winWrap = vuWrap.Window;
//			object window = WindowMapping [hwnd.Handle];
			if (winWrap == null || vuWrap.Superview != null)
				vuWrap.Hidden = !visible;
			else if (visible)
				winWrap.OrderFront (winWrap);
			else
				winWrap.OrderOut (winWrap);
			
			if (visible)
				SendMessage (handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

			hwnd.visible = visible;
			hwnd.Mapped = true;
			return true;
		}

		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			Dnd.SetAllowDrop (Hwnd.ObjectFromHandle (handle), value);
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data, DragDropEffects allowed_effects)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd == null)
				throw new ArgumentException ("Attempt to begin drag from invalid window handle (" + 
								handle.ToInt32 () + ").");

			return Dnd.StartDrag (hwnd.ClientWindow, data, allowed_effects);
		}

		internal override void SetBorderStyle (IntPtr handle, FormBorderStyle border_style) {
			Form form = Control.FromHandle (handle) as Form;
			if (form != null && form.window_manager == null && (border_style == FormBorderStyle.FixedToolWindow ||
				border_style == FormBorderStyle.SizableToolWindow)) {
				form.window_manager = new ToolWindowManager (form);
			}

			RequestNCRecalc (handle);
		}

		internal override void SetMenu (IntPtr handle, Menu menu) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			hwnd.menu = menu;

			NSMenu hostMenu = null;
			if (null != menu) {
//				menu.Wnd.Name = "StackTraceMe";
				if( IntPtr.Zero == menu.Handle)
					hostMenu = BuildNativeSurrogate (menu);
			}

			if (GetActive () == handle) {
				if (null == hostMenu && null != menu && IntPtr.Zero != menu.Handle)
					hostMenu = (NSMenu) MonoMac.ObjCRuntime.Runtime.GetNSObject (menu.Handle);
				#if DriverDebug
					Console.WriteLine ("SetMenu ({0}, {1}) : {2}", hwnd, menu, 
							null != hostMenu ? hostMenu.Description : null);
				#endif

				NSApplication.SharedApplication.MainMenu = hostMenu;
			}

			RequestNCRecalc (handle);
		}
		
		internal override void SetWindowMinMax (IntPtr handle, Rectangle maximized, Size min, Size max) {
		}

		internal override void SetWindowPos (IntPtr handle, int x, int y, int width, int height)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null) {
				return;
			}

			// Win32 automatically changes negative width/height to 0.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;
				
			// X requires a sanity check for width & height; otherwise it dies
			if (hwnd.zero_sized && width > 0 && height > 0) {
				if (hwnd.visible) {
					NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
					vuWrap.Hidden = false;
				}
				hwnd.zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				hwnd.zero_sized = true;
				NSView vuWrap = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
				vuWrap.Hidden = true;
			}

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && (hwnd.width == width) && (hwnd.height == height)) {
				return;
			}

			if (hwnd.Parent != null)
				AddExpose (hwnd.Parent, true, hwnd.x, hwnd.y, hwnd.width, hwnd.height);

			hwnd.x = x;
			hwnd.y = y;
			hwnd.width = width;
			hwnd.height = height;

			if (! hwnd.zero_sized) {
				HwndPositionToNative (hwnd);
				SendMessage (hwnd.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

#if DriverDebug
				Console.WriteLine ("SetWindowPos ({0}, {1}, {2}, {3}, {4})", hwnd, x, y, width, height);
#endif

				PerformNCCalc (hwnd);
			}
		}
		
		internal override void SetWindowState (IntPtr handle, FormWindowState state)
		{
			Hwnd 		hwnd 	= Hwnd.ObjectFromHandle (handle);
			NSView		vuWrap	= (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.WholeWindow);
			NSWindow	winWrap	=  vuWrap.Window;

			switch (state) {
				case FormWindowState.Minimized: {
					winWrap.Miniaturize (vuWrap);
					break;
				}
				case FormWindowState.Normal: {
					if ( winWrap.IsMiniaturized )
						winWrap.Deminiaturize (vuWrap);
					else if (winWrap.IsZoomed )
						winWrap.Zoom (vuWrap);
					break;
				}
				case FormWindowState.Maximized: {
//					Form form = Control.FromHandle (hwnd.Handle) as Form;
//					if (form != null && form.FormBorderStyle == FormBorderStyle.None) {
//						Cocoa.Rect rect = new Cocoa.Rect ();
//						Cocoa.HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
//						SetRect (ref rect, (short)0, (short)0, (short)bounds.size.width, (short)bounds.size.height);
//						SetWindowBounds ((IntPtr) WindowMapping [hwnd.Handle], 33, ref rect);
//						HIViewSetFrame (hwnd.whole_window, ref bounds);
//					} else {
//						ZoomWindow (window, 8, false);
//					}
					if (winWrap.IsMiniaturized )
						winWrap.Deminiaturize (vuWrap);
					if (! winWrap.IsZoomed )
						winWrap.Zoom (vuWrap);
					break;
				}
			}
		}

		[MonoTODO ("How to set these attributes on a NSWindow after init?")]
		internal override void SetWindowStyle (IntPtr handle, CreateParams cp)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			SetHwndStyles(hwnd, cp);
			
			if (WindowMapping [hwnd.Handle] != null) {
				Cocoa.WindowAttributes attributes = Cocoa.WindowAttributes.kWindowCompositingAttribute | Cocoa.WindowAttributes.kWindowStandardHandlerAttribute;
				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) { 
					attributes |= Cocoa.WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					attributes |= Cocoa.WindowAttributes.kWindowResizableAttribute | Cocoa.WindowAttributes.kWindowHorizontalZoomAttribute | Cocoa.WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					attributes |= Cocoa.WindowAttributes.kWindowCloseBoxAttribute;
				}
				if ((cp.ExStyle & ((int)WindowExStyles.WS_EX_TOOLWINDOW)) != 0) {
					attributes = Cocoa.WindowAttributes.kWindowStandardHandlerAttribute | Cocoa.WindowAttributes.kWindowCompositingAttribute;
				}
				attributes |= Cocoa.WindowAttributes.kWindowLiveResizeAttribute;

//				Cocoa.WindowAttributes outAttributes = Cocoa.WindowAttributes.kWindowNoAttributes;
// FIXME: How to set these on a NSWindow after init?
//				GetWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], ref outAttributes);
//				ChangeWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], attributes, outAttributes);
			}
		}

		internal override void SetWindowTransparency (IntPtr handle, double transparency, Color key) {
		}

		internal override double GetWindowTransparency (IntPtr handle)
		{
			return 1.0;
		}

		internal override TransparencySupport SupportsTransparency () {
			return TransparencySupport.None;
		}

		internal override bool SetZOrder (IntPtr handle, IntPtr after_handle, bool Top, bool Bottom)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Hwnd afterHwnd = Hwnd.ObjectFromHandle (after_handle);
			NSView itVuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.WholeWindow);
			NSView afterVuWrap = null;
			NSView itSuperVuWrap = itVuWrap.Superview;
			bool results = true;
#if DriverDebug
				Console.WriteLine ("SetZOrder ({0}, {1}, {2}, {3})", hwnd, afterHwnd, Top, Bottom);
#endif

			if (IntPtr.Zero != after_handle) {
				if (null != afterHwnd)
					afterVuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (afterHwnd.WholeWindow);
				if (afterVuWrap == null) {
					return false;	// Bad after_handle.
				}
			}

			if (itSuperVuWrap != null) {
				if (afterVuWrap != null) {
					if (itSuperVuWrap == afterVuWrap || 
					    itVuWrap.AncestorSharedWithView(afterVuWrap) != itSuperVuWrap) 
						return false;	// after_handle is ineligible NSView.

					// If After is a subview of Its sibling, find the sibling.
					NSView afterSuperVuWrap = null;
					while ( itSuperVuWrap != (afterSuperVuWrap = afterVuWrap.Superview))
						afterVuWrap = afterSuperVuWrap;
				}

				// Copy the array because it needs to be mutable and because it is volitile.
				List<NSView> subviews = new List<NSView> (itSuperVuWrap.Subviews);
				int oldIndex = subviews.IndexOf (itVuWrap);

				if (Bottom) {
					subviews.Insert (0, itVuWrap);
					++oldIndex;
				} else if (!Top && afterVuWrap != null) {
					int newIndex = subviews.IndexOf (afterVuWrap);
					subviews.Insert (newIndex, itVuWrap);
					if (newIndex < oldIndex)
						++oldIndex;
				} else {
					subviews.Add (itVuWrap);
				}

				if (results) {
					// Remove it from its original slot.
					subviews.RemoveAt (oldIndex);
					itSuperVuWrap.Subviews = subviews.ToArray ();
				}

				return results;
			}

			// Top level/Content View: Reorder the Window.
			NSWindow itWinWrap = itVuWrap.Window;

			if (itWinWrap != null) {
				if (Bottom) {
					itWinWrap.OrderWindow (NSWindowOrderingMode.Below, 0);
				} else if (!Top && afterVuWrap != null) {
					NSWindow afterWinWrap = afterVuWrap.Window;
					if (itWinWrap == afterWinWrap)
						return false;	// Cannot reorder relative to self.
					itWinWrap.OrderWindow (NSWindowOrderingMode.Below, afterVuWrap.Window.WindowNumber);
				} else {
					itWinWrap.OrderWindow (NSWindowOrderingMode.Above, 0);
				}
			}

			return true;
		}

		internal override void ShowCursor (bool show)
		{
			if (show)
				NSCursor.Unhide ();
			else
				NSCursor.Hide ();
		}

		internal override object StartLoop (Thread thread) {
			return new object ();
		}

		[MonoTODO]
		internal override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon)
		{
			throw new NotImplementedException ();
		}

		internal override bool Text (IntPtr handle, string text)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			object nswindowPtr = WindowMapping [hwnd.Handle];

			if (null != nswindowPtr) {
				NSWindow winWrap = (NSWindow) MonoMac.ObjCRuntime.Runtime.GetNSObject ((IntPtr) nswindowPtr);
				winWrap.Title = text;
			}
			else {
				// Just mark it for redisplay. The generic Mono code will redraw it using the text it stores.
				NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);
				vuWrap.NeedsDisplay = true;
			}

//			SetControlTitleWithCFString (hwnd.whole_window, __CFStringMakeConstantString (text));
//			SetControlTitleWithCFString (hwnd.client_window, __CFStringMakeConstantString (text));
			return true;
		}

		internal override void UpdateWindow (IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);

			if (!hwnd.visible || vuWrap.IsHiddenOrHasHiddenAncestor || NSRect.Empty == vuWrap.VisibleRect())
				return;


			SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
		}

		internal override bool TranslateMessage (ref MSG msg) {
			return Cocoa.EventHandler.TranslateMessage (ref msg);
		}

		#region Reversible regions
		/* 
		 * Quartz has no concept of XOR drawing due to its compositing nature
		 * We fake this by mapping a overlay window on the first draw and mapping it on the second.
		 * This has some issues with it because its POSSIBLE for ControlPaint.DrawReversible* to actually
		 * reverse two regions at once.  We dont do this in MWF, but this behaviour woudn't work.
		 * We could in theory cache the Rectangle/Color combination to handle this behaviour.
		 *
		 * PROBLEMS: This has some flicker / banding
		 */
		internal override void DrawReversibleLine (Point start, Point end, Color backColor) {
//			throw new NotImplementedException();
		}

		internal override void FillReversibleRectangle (Rectangle rectangle, Color backColor) {
//			throw new NotImplementedException();
		}

		internal override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {
//			throw new NotImplementedException();
		}

		internal override void DrawReversibleRectangle (IntPtr handle, Rectangle rect, int line_width)
		{

			if (ReverseWindowMapped) {
				ReverseWindow.OrderOut (ReverseWindow);
				ReverseWindowMapped = false;
			} else {
//				Rectangle size_rect = rect;
//				ClientToScreen (handle, ref rect.Location);
				PositionWindowInClient (rect, ReverseWindow, handle);

//				ReverseWindow.setFrame_display (size_rect, false);
//				ReverseWindow.orderFront (ReverseWindow);

				rect.Location = Point.Empty;

				Graphics g = Graphics.FromHwnd ((IntPtr) ReverseWindow.Handle);

				for (int i = 0; i < line_width; i++) {
					rect.Width -= 1;
					rect.Height -= 1;
					g.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (Color.Black), rect);
					rect.X += 1;
					rect.Y += 1;
				}

				g.Flush ();
				g.Dispose ();
				
				ReverseWindowMapped = true;
			}
		}

		#endregion Reversible regions

		internal override SizeF GetAutoScaleSize (Font font) {
			Graphics        g;
			float           width;
			string          magic_string = "The quick brown fox jumped over the lazy dog.";
			double          magic_number = 44.549996948242189;

			g = Graphics.FromImage (new Bitmap (1, 1));

			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF(width, font.Height);
		}

		internal override Point MousePosition {
			get {
				mouse_position = NativeToMonoScreen (NSEvent.CurrentMouseLocation);
				return mouse_position;
			}
		}
		#endregion Override Methods XplatUIDriver


		#region Override Properties XplatUIDriver
		internal override int KeyboardSpeed { get{ throw new NotImplementedException(); } } 
		internal override int KeyboardDelay { get{ throw new NotImplementedException(); } } 

		internal override int CaptionHeight {
			get {
				return 19;
			}
		}

		internal override  Size CursorSize { get{ throw new NotImplementedException(); } }
		internal override  bool DragFullWindows { get{ throw new NotImplementedException(); } }
		internal override  Size DragSize {
			get {
				return new Size(4, 4);
			}
		}

		internal override  Size FrameBorderSize {
			get {
				return new Size (2, 2);
			}
		}

		internal override  Size IconSize { get{ throw new NotImplementedException(); } }
		internal override  Size MaxWindowTrackSize { get{ throw new NotImplementedException(); } }
		internal override bool MenuAccessKeysUnderlined {
			get {
				return false;
			}
		}
		internal override Size MinimizedWindowSpacingSize { get{ throw new NotImplementedException(); } }

		internal override Size MinimumWindowSize {
			get {
				return new Size (110, 22);
			}
		}

		internal override Keys ModifierKeys {
			get {
				return KeyboardHandler.ModifierKeys;
			}
		}
		internal override Size SmallIconSize { get{ throw new NotImplementedException(); } }
		internal override int MouseButtonCount { get{ throw new NotImplementedException(); } }
		internal override bool MouseButtonsSwapped { get{ throw new NotImplementedException(); } }
		internal override bool MouseWheelPresent { get{ throw new NotImplementedException(); } }

		internal override MouseButtons MouseButtons {
			get {
				return MouseState;
			}
		}

		internal override Rectangle VirtualScreen {
			get {
				return WorkingArea;
			}
		}

		internal override Rectangle WorkingArea { 
			get { 
				Rectangle rect = Rectangle.Empty;
				// NSScreen.mainScreen () returns the screen the the user is currently interacting with.
				// To get the screen identified by CGMainDisplayID (), get the 0th element of this array.
				var screens = NSScreen.Screens;

				if (screens != null && 0 < screens.Length) {
					NSScreen screenWrap = (NSScreen) screens[0];
					NSRect bounds = screenWrap.VisibleFrame;
					rect = NativeToMonoScreen (bounds);
				}

				return rect;
			}
		}

		[MonoTODO]
		internal override Screen[] AllScreens
		{
			get
			{
				return null;
			}
		}

		internal override bool ThemesEnabled {
			get {
				return XplatUICocoa.themes_enabled;
			}
		}
 

		// Event Handlers
		internal override event EventHandler Idle;
		#endregion Override properties XplatUIDriver

		#region Process imports
		[DllImport("/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices")]
		extern static int GetCurrentProcess (ref Cocoa.ProcessSerialNumber psn);
		[DllImport("/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices")]
		extern static int TransformProcessType (ref Cocoa.ProcessSerialNumber psn, uint type);
		[DllImport("/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices")]
		extern static int SetFrontProcess (ref Cocoa.ProcessSerialNumber psn);
		#endregion
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void CGDisplayMoveCursorToPoint (UInt32 display, Cocoa.CGPoint point);
	}
}

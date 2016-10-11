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

using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Cocoa = System.Windows.Forms.CocoaInternal;

/// Cocoa Version
#if XAMARINMAC
using Foundation;
using AppKit;
using System.Windows.Forms.CocoaInternal;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Windows.Forms.CocoaInternal;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif
#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
using NSSize = System.Drawing.SizeF;
#else
#if XAMARINMAC
using CGPoint = CoreGraphics.CGPoint;
using NSRect = CoreGraphics.CGRect;
using NSPoint = CoreGraphics.CGPoint;
using NSSize = CoreGraphics.CGSize;
#elif MONOMAC
using CGPoint = MonoMac.CoreGraphics.CGPoint;
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
using NSSize = MonoMac.CoreGraphics.CGSize;
#endif
#endif

#if MAC64
using nint = System.Int64;
using nfloat = System.Double;
#else
using nint = System.Int32;
using nfloat = System.Single;
#endif

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
		internal static IntPtr ActiveWindow;
		internal static NSWindow ReverseWindow;
		internal static NSView CaretView;

		// Instance members
		internal static NSEventModifierMask key_modifiers = 0;
		internal bool translate_modifier = false;

		// Event handlers
		private Cocoa.MonoApplicationDelegate applicationDelegate;
		
		// Cocoa Specific
		internal static GrabStruct Grab;
		internal static Cocoa.Caret Caret;
		internal static ArrayList UtilityWindows;
		internal static readonly Stack<IntPtr> ModalSessions = new Stack<IntPtr>();
		internal float screenHeight;

		private Dictionary<Thread, Stack<NSAutoreleasePool>> pools = new Dictionary<Thread, Stack<NSAutoreleasePool>>();

		// Message loop
		private static bool GetMessageResult;

		private static bool ReverseWindowMapped;

		static readonly object instancelock = new object ();

		static Queue<String> charsQueue = new Queue<string>();
		internal const int NSEventTypeWindowsMessage = 12345;

#endregion Local Variables
		
#region Constructors
		private XplatUICocoa() {

			RefCount = 0;

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
					try { NSApplication.InitDrawingBridge (); }
					catch (NullReferenceException) { }

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

		internal void Initialize ()
		{
			// Cache main screen height for flipping screen coordinates.
			Size size;
			GetDisplaySize (out size);
			screenHeight = size.Height;

			// Initialize the event handlers
			applicationDelegate = new Cocoa.MonoApplicationDelegate (this);
			NSApplication.SharedApplication.Delegate = applicationDelegate;

			// Initialize the Caret
			Caret.Timer = new Timer ();
			Caret.Timer.Interval = 500;
			Caret.Timer.Tick += new EventHandler (CaretCallback);

			// Initialize the Cocoa Specific stuff
			UtilityWindows = new ArrayList ();

			// Transform to foreground process
			if (NSApplication.SharedApplication.ActivationPolicy != NSApplicationActivationPolicy.Regular)
			{
				NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
				NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
				NSProcessInfo.ProcessInfo.ProcessName = Application.ProductName;
			}

			ReverseWindow = new NSWindow(NSRect.Empty, NSWindowStyle.Borderless, NSBackingStore.Buffered, true);
			CaretView = new NSView(NSRect.Empty);
			CaretView.WantsLayer = true;
			CaretView.Layer.BackgroundColor = NSColor.Black.CGColor;

			// Message loop
			GetMessageResult = true;

			ReverseWindowMapped = false;
		}

		internal void PerformNCCalc (Hwnd hwnd)
		{
			//FIXME! Should not reference Win32 variant here or NEED to do so.
			XplatUIWin32.NCCALCSIZE_PARAMS ncp = new XplatUIWin32.NCCALCSIZE_PARAMS ();
			IntPtr ptr = Marshal.AllocHGlobal (Marshal.SizeOf (ncp));

			Rectangle rect = new Rectangle(0, 0, hwnd.Width, hwnd.Height);
			ncp.rgrc1.left = rect.Left;
			ncp.rgrc1.top = rect.Top;
			ncp.rgrc1.right = rect.Right;
			ncp.rgrc1.bottom = rect.Bottom;

			Marshal.StructureToPtr (ncp, ptr, true);
			NativeWindow.WndProc (hwnd.Handle, Msg.WM_NCCALCSIZE, (IntPtr) 1, ptr);
			ncp = (XplatUIWin32.NCCALCSIZE_PARAMS) Marshal.PtrToStructure (ptr, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));
			Marshal.FreeHGlobal(ptr);

			rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			// For top-level windows the client area position is calculated with the window title, but the actual view is already
			// adjusted for that.
			if (hwnd.Parent == null)
				rect = new Rectangle (new Point (0, 0), rect.Size);
			hwnd.ClientRect = rect;

			// Update subview locations
			var vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd.Handle);
			foreach (var subView in vuWrap.Subviews)
			{
				var subViewHwnd = Hwnd.ObjectFromHandle(subView.Handle);
				if (subViewHwnd != null)
					HwndPositionToNative(subViewHwnd);
			}
		}

		internal void ScreenToClientWindow (IntPtr handle, ref NSPoint point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ScreenToClientWindow() requires NSView*");

			NSWindow windowWrapper = viewWrapper.Window;
			if (windowWrapper == null) {
				point = new NSPoint (0, 0);
				return;
			}

			point = windowWrapper.ConvertScreenToBase (point);
			point = viewWrapper.ConvertPointFromView (point, null);
			point.X -= hwnd.ClientRect.X;
			point.Y -= hwnd.ClientRect.Y;
		}

		internal void ClientWindowToScreen (IntPtr handle, ref NSPoint point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ClientWindowToScreen() requires NSView*");

			NSWindow windowWrapper = viewWrapper.Window;
			if (viewWrapper != null && windowWrapper != null) {
				point.X += hwnd.ClientRect.X;
				point.Y += hwnd.ClientRect.Y;
				point = viewWrapper.ConvertPointToView (point, null);
				point = windowWrapper.ConvertBaseToScreen (point);
			}
		}

		internal void EnqueueMessage (MSG msg) {
			NSApplication.SharedApplication.PostEvent(msg.ToNSEvent(), false);
		}

		internal void PositionWindowInClient (Rectangle rect, NSWindow window, IntPtr handle)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			rect.X += hwnd.ClientRect.X;
			rect.Y += hwnd.ClientRect.Y;

			NSRect nsrect = MonoToNativeFramed (rect, vuWrap.Frame.Size.Height);

			NSPoint location = nsrect.Location;
            ClientWindowToScreen (handle, ref location);
			nsrect = new NSRect(location, nsrect.Size);
			nsrect = window.FrameRectFor (nsrect);
			window.SetFrame (nsrect, false);
#if DriverDebug
			Console.WriteLine ("PositionWindowInClient ({0}, {1}) : {2}", rect, window, nsrect);
#endif
		}

		internal NSPoint MonoToNativeScreen (Point monoPoint)
		{
			return new NSPoint (monoPoint.X, screenHeight - monoPoint.Y);
		}

		internal NSPoint MonoToNativeFramed (Point monoPoint, nfloat frameHeight)
		{
			return new NSPoint (monoPoint.X, monoPoint.Y);
		}

		internal Point NativeToMonoScreen (NSPoint nativePoint)
		{
			return new Point ((int) nativePoint.X, (int) (screenHeight - nativePoint.Y));
		}

		internal Point NativeToMonoFramed (NSPoint nativePoint, nfloat frameHeight)
		{
			return new Point ((int) nativePoint.X, (int) (nativePoint.Y));
		}

		internal NSRect MonoToNativeScreen (Rectangle monoRect)
		{
			return new NSRect(monoRect.Left, screenHeight - monoRect.Bottom, monoRect.Width, monoRect.Height);
		}

		internal NSRect MonoToNativeFramed (Rectangle monoRect, nfloat frameHeight)
		{
			return new NSRect(monoRect.Left, monoRect.Top, monoRect.Width, monoRect.Height);
		}

		internal Rectangle NativeToMonoScreen (NSRect nativeRect)
		{
			return new Rectangle ((int) nativeRect.Left, (int) (screenHeight - nativeRect.Bottom), 
				(int) nativeRect.Size.Width, (int) nativeRect.Size.Height);
		}

		internal Rectangle NativeToMonoFramed (NSRect nativeRect, nfloat frameHeight)
		{
			return new Rectangle ((int) nativeRect.Left, (int) nativeRect.Top, 
						(int) nativeRect.Size.Width, (int) nativeRect.Size.Height);
		}

		internal void HwndPositionFromNative (Hwnd hwnd)
		{
			if (hwnd.zero_sized)
				return;

			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (hwnd.Handle);
			NSWindow winWrap = vuWrap.Window;
			NSRect nsrect = vuWrap.Frame;
			Rectangle mrect;

			bool top = winWrap != null && winWrap.ContentView == vuWrap;
			if (top) {
				var size = winWrap.Frame.Size;
				nsrect = new NSRect(
					winWrap.ConvertBaseToScreen (nsrect.Location),
					new NSSize(size.Width, size.Height));
				mrect = NativeToMonoScreen (nsrect);
			} else {
				NSView superVuWrap = vuWrap.Superview;
				mrect = NativeToMonoFramed (nsrect, superVuWrap.Frame.Size.Height);
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
#endregion
		
#region Private Methods
		private Point ConvertScreenPointToClient (IntPtr handle, Point point)
		{
			NSPoint nspoint = MonoToNativeScreen (point);

			ScreenToClientWindow (handle, ref nspoint);

			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			point = NativeToMonoFramed (nspoint, vuWrap.Frame.Size.Height);
			point.X -= hwnd.ClientRect.X;
			point.Y -= hwnd.ClientRect.Y;
			return point;
		}

		private Point ConvertClientPointToScreen (IntPtr handle, Point point)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			if (vuWrap == null)
				return Point.Empty;
			point.X += hwnd.ClientRect.X;
			point.Y += hwnd.ClientRect.Y;

			NSPoint nspoint = MonoToNativeFramed (point, vuWrap.Frame.Size.Height);

			ClientWindowToScreen (handle, ref nspoint);
			point = NativeToMonoScreen (nspoint);

			return point;
		}

		private Point ConvertScreenPointToNonClient (IntPtr handle, Point point)
		{
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
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
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ConvertScreenPointToNonClient() requires NSView*");

			NSPoint native_point = MonoToNativeFramed (point, viewWrapper.Frame.Size.Height);
			NSWindow windowWrapper = viewWrapper.Window;

			native_point = viewWrapper.ConvertPointToView (native_point, null);
			native_point = windowWrapper.ConvertBaseToScreen(native_point);

			Point converted_point = NativeToMonoScreen (native_point);

			return converted_point;
		}

		private bool PumpNativeEvent (bool wait, ref MSG msg, bool dequeue = true)
		{
			msg.message = Msg.WM_NULL;

			NSDate timeout = wait ? NSDate.DistantFuture : NSDate.DistantPast;
			NSApplication NSApp = NSApplication.SharedApplication;
			NSEvent evtRef = NSApp.NextEvent (NSEventMask.AnyEvent, timeout, NSRunLoop.NSDefaultRunLoopMode, dequeue);
			if (evtRef == null)
				return false;

			// Is it Windows message?
			if (evtRef.Type == NSEventType.ApplicationDefined && evtRef.Subtype == NSEventTypeWindowsMessage) {
				msg = evtRef.ToMSG();
				return true;
			}

			if (dequeue) {
				NSApp.SendEvent(evtRef);
				NSApp.UpdateWindows();
			}

			return true;
		}

		private void SendParentNotify(IntPtr child, Msg cause, int x, int y) {
			if (child == IntPtr.Zero)
				return;
			
			Hwnd hwnd = Hwnd.GetObjectFromWindow(child);
			if (hwnd == null || hwnd.Handle == IntPtr.Zero)
				return;
			
			if (ExStyleSet((int) hwnd.initial_ex_style, WindowExStyles.WS_EX_NOPARENTNOTIFY))
				return;
			
			if (hwnd.Parent == null || hwnd.Parent.Handle == IntPtr.Zero)
				return;

			if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY)
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), child);
			else
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
			
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
			if (Caret.On || Caret.Hwnd == IntPtr.Zero)
				return;

			Caret.On = true;
			CaretView.Hidden = false;
		}

		private void HideCaret () {
			if (!Caret.On)
				return;
			Caret.On = false;
			CaretView.Hidden = true;

			// FIXME: Remove this when TextBox (LineTag) rendering is fixed
			CaretView.Superview.NeedsDisplay = true;
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

			if (GetFocus() == hwnd.Handle) {
				SendMessage (hwnd.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
			}

			if (Grab.Hwnd == hwnd.Handle) {
				Grab.Hwnd = IntPtr.Zero;
				Grab.Confined = false;
			}

			DestroyCaret (hwnd.Handle);
		}

		private  void HwndPositionToNative (Hwnd hwnd)
		{
			if (hwnd.zero_sized)
				return;

			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd.Handle);
			NSWindow winWrap = vuWrap.Window;
			Rectangle mrect = new Rectangle (hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
			NSRect nsrect;

			if (winWrap != null && winWrap.ContentView == vuWrap)
			{
				nsrect = MonoToNativeScreen(mrect);
				if (winWrap.Frame != nsrect)
				{
					winWrap.SetFrame(nsrect, false);
					SetCaretPos(hwnd.Handle, Caret.X, Caret.Y);
				}
			}
			else {
				NSView superVuWrap = vuWrap.Superview;
				Hwnd parent = hwnd.Parent;

				if (superVuWrap != null) {
						mrect.Y += (int)superVuWrap.AlignmentRectInsets.Top;
						mrect.X += (int)superVuWrap.AlignmentRectInsets.Left;
						nsrect = MonoToNativeFramed(mrect, superVuWrap.Frame.Size.Height);
				} else
					nsrect = new NSRect(mrect.X, mrect.Y, mrect.Width, mrect.Height);

				if (vuWrap.Frame != nsrect)
					vuWrap.Frame = nsrect;
			}
#if DriverDebug
			Console.WriteLine ("HwndToNative ({0}) : {1}", hwnd, nsrect);
#endif
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
			if (ActiveWindow != handle && handle != IntPtr.Zero)
			{
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				if (vuWrap != null)
				{
					NSWindow winWrap = vuWrap.Window;
					if (winWrap != null)
					{
						winWrap.MakeKeyAndOrderFront (vuWrap);
					}
				}
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
			if (StyleSet(cp.Style, WindowStyles.WS_CHILD)) {
				WindowRect = Hwnd.GetWindowRectangle (cp, menu, ClientRect);
			} else {				
				var nsrect = NSWindow.FrameRectFor (MonoToNativeFramed(ClientRect, ClientRect.Height), StyleFromCreateParams (cp));
				// RGS TEST
				if (0 != ClientRect.Width && 0 != ClientRect.Height) { // The 0x0 sizes are calculations to update only the content area
					if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
						Debug.WriteLine ("{0} WxH: {1}x{2} -> {3}x{4}", new object[] {
							cp.Caption,
							ClientRect.Width,
							ClientRect.Height,
							nsrect.Width,
							nsrect.Height
						});
					} else {
//						Debug.WriteLine ("CalculateWindowRect untitled window");
					}
				}
				// RGS TEST

				WindowRect = new Rectangle((int)nsrect.X, (int)nsrect.Y, (int)nsrect.Width, (int)nsrect.Height);
			}
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
			return Cocoa.Pasteboard.GetAvailableFormats((NSPasteboard)ObjCRuntime.Runtime.GetNSObject(handle));
		}

		internal override void ClipboardClose (IntPtr handle) {
		}

		[MonoTODO ("Map our internal formats to the right os code where we can")]
		internal override int ClipboardGetID(IntPtr handle, string format)
		{
			DataFormats.Format f = DataFormats.Format.Find(format);
			return f != null ? f.Id : DataFormats.Format.Count + 1;
		}

		internal override IntPtr ClipboardOpen(bool primary_selection) {
			if (primary_selection)
				return Pasteboard.Primary != null ? Pasteboard.Primary.Handle : IntPtr.Zero;
			return Pasteboard.Application != null ? Pasteboard.Application.Handle : IntPtr.Zero;
		}

		internal override object ClipboardRetrieve (IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			return Cocoa.Pasteboard.Retrieve ((NSPasteboard)ObjCRuntime.Runtime.GetNSObject(handle), type);
		}

		internal override void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter, bool copy)
		{
			Cocoa.Pasteboard.Store ((NSPasteboard)ObjCRuntime.Runtime.GetNSObject(handle), obj, type);
		}
		
		internal override void CreateCaret (IntPtr hwnd, int width, int height) {
			if (Caret.Hwnd != IntPtr.Zero)
				DestroyCaret (Caret.Hwnd);

			Caret.Hwnd = hwnd;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = 0;
			Caret.On = false;

			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd);
			if (CaretView.Superview != vuWrap)
			{
				if (CaretView.Superview != null)
					CaretView.RemoveFromSuperview();

				if (vuWrap != null)
					vuWrap.AddSubview(CaretView);
			}
		}

		private NSWindowStyle StyleFromCreateParams(CreateParams cp)
		{
			NSWindowStyle attributes = NSWindowStyle.Borderless;
			if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
				attributes = NSWindowStyle.Titled;
				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX))
					attributes |= NSWindowStyle.Miniaturizable;
				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX))
					attributes |= NSWindowStyle.Resizable;
				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU))
					attributes |= NSWindowStyle.Closable;
			}
			return attributes;
		}

		internal override IntPtr CreateWindow (CreateParams cp)
		{
			Hwnd hwnd = new Hwnd();
			Hwnd parent_hwnd = null;
			int X = cp.X;
			int Y = cp.Y;

			int Width = Math.Max(1, cp.Width);
			int Height = Math.Max(1, cp.Height);
			IntPtr WindowHandle = IntPtr.Zero;
			IntPtr wholeHandle = IntPtr.Zero;

			NSView ParentWrapper = null;  // If any
			NSWindow windowWrapper = null;
			NSView viewWrapper =  null;

			bool isTopLevel = !StyleSet(cp.Style, WindowStyles.WS_CHILD);

			if (cp.Parent != IntPtr.Zero) {
				parent_hwnd = Hwnd.ObjectFromHandle (cp.Parent);
				ParentWrapper = (NSView)ObjCRuntime.Runtime.GetNSObject(parent_hwnd.Handle);
				if (!isTopLevel)
					windowWrapper = ParentWrapper.Window;
			}

			Point next;
			if (X == int.MinValue || Y == int.MinValue) {
				next = Hwnd.GetNextStackedFormLocation (cp, parent_hwnd);
				if (X == int.MinValue)
					X = next.X;
				if (Y == int.MinValue)
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
			//hwnd.ClientRect = new Rectangle(0, 0, Width, Height);

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				hwnd.enabled = false;
			}

			Rectangle mWholeRect = new Rectangle (new Point (X, Y), new Size(Width, Height));
			NSRect WholeRect;
			if (!isTopLevel && null != parent_hwnd) {
				mWholeRect.X += (int)ParentWrapper.AlignmentRectInsets.Left;
				mWholeRect.Y += (int)ParentWrapper.AlignmentRectInsets.Top;
				WholeRect = MonoToNativeFramed (mWholeRect, ParentWrapper.Frame.Size.Height);
			} else {
				WholeRect = MonoToNativeScreen (mWholeRect);
			}
				
			SetHwndStyles(hwnd, cp);
			// FIXME

			if (isTopLevel) {
				NSWindowStyle attributes = StyleFromCreateParams(cp);
				//SetAutomaticControlDragTrackingEnabledForWindow (, true);
				//ParentHandle = WindowView;
				WholeRect = NSWindow.ContentRectFor(WholeRect, attributes);
				windowWrapper = new MonoWindow(WholeRect, attributes, NSBackingStore.Buffered, true, this);
				WindowHandle = (IntPtr) windowWrapper.Handle;
				windowWrapper.WeakDelegate = windowWrapper;

				if ((cp.ClassStyle & 0x20000) != 0) // CS_DROPSHADOW
					windowWrapper.HasShadow = true;

				viewWrapper = new Cocoa.MonoContentView (this, WholeRect, hwnd);
				wholeHandle = (IntPtr) viewWrapper.Handle;
				windowWrapper.ContentView = viewWrapper;
				windowWrapper.InitialFirstResponder = viewWrapper;
				windowWrapper.SetOneShot(true);

				if (StyleSet (cp.Style, WindowStyles.WS_POPUP))
					windowWrapper.Level = NSWindowLevel.PopUpMenu;
				if (ParentWrapper != null)
					ParentWrapper.Window.AddChildWindow (windowWrapper, NSWindowOrderingMode.Above);
			} else {
				viewWrapper = new Cocoa.MonoView (this, WholeRect, hwnd);
				wholeHandle = (IntPtr) viewWrapper.Handle;
				if (ParentWrapper != null)
					ParentWrapper.AddSubview (viewWrapper);
			}

			viewWrapper.AutoresizesSubviews = false;

			hwnd.ClientWindow = wholeHandle;

			if (WindowHandle != IntPtr.Zero) {
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
			
			SendMessage (hwnd.Handle, Msg.WM_NCCREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			PerformNCCalc (hwnd);

			SendMessage (hwnd.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			SendParentNotify (hwnd.Handle, Msg.WM_CREATE, int.MaxValue, int.MaxValue);

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				if (WindowHandle != IntPtr.Zero) {
					if (Control.FromHandle(hwnd.Handle) is Form) {
						Form f = Control.FromHandle(hwnd.Handle) as Form;
						if (f.WindowState == FormWindowState.Normal) {
							SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
						}
						if (f.ActivateOnShow)
							windowWrapper.MakeKeyAndOrderFront (viewWrapper);
						else
							windowWrapper.OrderFront (viewWrapper);
					}
				}

				hwnd.visible = true;
				hwnd.mapped = true;
				if (! (Control.FromHandle (hwnd.Handle) is Form)) {
					SendMessage (hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
				}
			}
			else
			{
				if (!isTopLevel)
					viewWrapper.Hidden = true;
			}

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState (hwnd.Handle, FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState (hwnd.Handle, FormWindowState.Maximized);
			}

			hwnd.UserData = new object[] { windowWrapper, viewWrapper };

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
				case Msg.WM_IME_COMPOSITION:
					for (string s = PopChars(); s.Length != 0; s = PopChars())
						foreach (char c in s)
							SendMessage (msg.HWnd, Msg.WM_IME_CHAR, (IntPtr) c, msg.LParam);
					break;
				case Msg.WM_IME_CHAR:
					// On Windows API it sends two WM_CHAR messages for each byte, but
					// I wonder if it is worthy to emulate it (also no idea how to 
					// reconstruct those bytes into chars).
					SendMessage (msg.HWnd, Msg.WM_CHAR, msg.WParam, msg.LParam);
					return IntPtr.Zero;
				case Msg.WM_QUIT:
					Exit ();
					break;
				case Msg.WM_PAINT:
					hwnd.expose_pending = false;
					break;
				case Msg.WM_NCPAINT:
					hwnd.nc_expose_pending = false;
					break;
				case Msg.WM_NCCALCSIZE: {
					if (msg.WParam == (IntPtr)1) {
						// Add all the stuff X is supposed to draw.
						Control ctrl = Control.FromHandle (hwnd.Handle);
						if (ctrl != null) {
							XplatUIWin32.NCCALCSIZE_PARAMS ncp;
							ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)
								Marshal.PtrToStructure (msg.LParam, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

							if (ctrl is Form) {
								var cp = ctrl.GetCreateParams ();
								var frameRect = MonoToNativeScreen (new Rectangle (
									                ncp.rgrc1.left,
									                ncp.rgrc1.top, 
									                ncp.rgrc1.right - ncp.rgrc1.left,
									                ncp.rgrc1.bottom - ncp.rgrc1.top));
								var contentRect = NativeToMonoScreen (NSWindow.ContentRectFor (frameRect, StyleFromCreateParams (cp)));
								ncp.rgrc1.left = contentRect.Left;
								ncp.rgrc1.top = contentRect.Top;
								ncp.rgrc1.right = contentRect.Right;
								ncp.rgrc1.bottom = contentRect.Bottom;
							} else {
								Hwnd.Borders rect = Hwnd.GetBorders (ctrl.GetCreateParams (), null);
								ncp.rgrc1.top += rect.top;
								ncp.rgrc1.bottom -= rect.bottom;
								ncp.rgrc1.left += rect.left;
								ncp.rgrc1.right -= rect.right;
							}

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
				case Msg.WM_MOUSEWHEEL:
				{
					if ((null != hwnd.Parent) && (IntPtr.Zero != msg.Result))
						msg.Result = NativeWindow.WndProc(hwnd.Parent.Handle, (Msg)msg.Msg, msg.WParam, msg.LParam);
					return (IntPtr)msg.Result;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret (IntPtr hwnd) {
			//Console.WriteLine("DestroyCaret(" + DebugUtility.ControlInfo(hwnd) + ", destroying:" + (Caret.Hwnd == hwnd) + ")");
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
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
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
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(h.Handle);
				NSWindow winWrap = vuWrap.Window;
				if (winWrap != null && winWrap.ContentView == vuWrap) { 
					if (hwnd.border_style == FormBorderStyle.FixedToolWindow ||
						hwnd.border_style == FormBorderStyle.SizableToolWindow)
					{
						UtilityWindows.Remove(winWrap);
					}

					winWrap.ReleasedWhenClosed = true;
					winWrap.Close ();
					NSApplication.SharedApplication.RemoveWindowsItem(winWrap);
				} else {
					vuWrap.RemoveFromSuperviewWithoutNeedingDisplay ();
				}

				h.UserData = null;
				h.Dispose ();
			}
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DoEvents() {
            MSG msg = new MSG ();

			while (PeekMessage (null, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
                var message = Message.Create(msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);
                if (Application.FilterMessage (ref message))
                    continue;
                TranslateMessage (ref msg);
                DispatchMessage (ref msg);
            }
		}

		internal override void EnableWindow (IntPtr handle, bool Enable) {
			//Like X11 we need not do anything here
		}

		internal override void EndLoop (Thread thread) {
			var stack = pools[thread];
			stack.Pop().Dispose();
			if (stack.Count == 0)
				pools.Remove(thread);
		}

		internal void Exit () {
			GetMessageResult = false;
			NSApplication.SharedApplication.Delegate = null;
		}
		
		internal override IntPtr GetActive () {
			return ActiveWindow;
		}

		internal override Region GetClipRegion (IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				return hwnd.UserClip;
			}
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
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
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
			if (ActiveWindow != IntPtr.Zero) {
				NSView activeWindowWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (ActiveWindow);
				MonoContentView contentView = activeWindowWrap.Window.FirstResponder as MonoContentView;
				if (contentView != null)
					return contentView.FocusHandle;
			}
			return IntPtr.Zero;
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
			bool wait = false;

			while (GetMessageResult) {
				while (PumpNativeEvent(wait, ref msg)) {
					if (msg.message != Msg.WM_NULL)
						return true;
					wait = false;
				}
			
				RaiseIdle(EventArgs.Empty);
				wait = true;
			}

			return false;
		}

		[MonoTODO]
		internal override bool GetText (IntPtr handle, out string text) {
			throw new NotImplementedException ();
		}

		internal override void GetWindowPos (IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

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

			x = y = 0;
			width = height = 0;
			client_width = client_height = 0;
		}

		internal override FormWindowState GetWindowState(IntPtr handle)
		{
			// FIXME: This Check is here because of DbRepair/DbBackup.
			// It should be probably handled there - but on Windows it's OK.
			if (NSThread.IsMain)
				return GetWindowStateInternal(handle);

			FormWindowState state = FormWindowState.Normal;
			NSApplication.SharedApplication.InvokeOnMainThread(delegate { state = GetWindowStateInternal(handle); });
			return state;
		}

		internal FormWindowState GetWindowStateInternal(IntPtr handle)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			NSWindow winWrap = vuWrap.Window;

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
			if (handle == IntPtr.Zero)
				return;

			Grab.Hwnd = handle;
			Grab.Confined = confine_to_handle != IntPtr.Zero;
			// FIXME: Set the Grab.Area
		}
		
		internal override void UngrabWindow (IntPtr hwnd) {
			if (Grab.Hwnd != hwnd) {
				Console.WriteLine("Unpaired ungrab!");
				return;
			}

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
		
		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd.visible)
			{
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				rc.X += hwnd.ClientRect.X;
				rc.Y += hwnd.ClientRect.Y;
				vuWrap.SetNeedsDisplayInRect(MonoToNativeFramed(rc, vuWrap.Frame.Height));
			}
		}

		internal override void InvalidateNC(IntPtr handle)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd.visible)
			{
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				vuWrap.NeedsDisplay = true;
			}
		}
		
		internal override bool IsEnabled(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).Enabled;
		}
		
		internal override bool IsVisible(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).visible;
		}
		
		internal override void KillTimer(Timer timer) {
			if (timer.window != IntPtr.Zero) {
				NSTimer nstimer = (NSTimer)ObjCRuntime.Runtime.GetNSObject(timer.window);
				nstimer.Invalidate();
				//nstimer.Release();
				timer.window = IntPtr.Zero;
			}
		}

		internal override void OverrideCursor (IntPtr cursor) {
			Cocoa.Cursor.SetCursor (cursor);
		}

		internal override PaintEventArgs PaintEventStart (ref Message msg, IntPtr handle, bool client)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.HWnd);
			Graphics dc;
			PaintEventArgs paint_event;

			if (client) {
				dc = Graphics.FromHwnd (handle);
				if (null == dc)
					return null;

				if (hwnd.UserClip != null) {
					dc.Clip = hwnd.UserClip;
				}

				paint_event = new PaintEventArgs (dc, hwnd.Invalid);
				hwnd.ClearInvalidArea ();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			} else {
				dc = Graphics.FromHwnd (handle, false);

				if (null == dc)
					return null;

				paint_event = new PaintEventArgs (dc, new Rectangle (0, 0, hwnd.width, hwnd.height));

				hwnd.ClearNcInvalidArea ();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			}

			return paint_event;
		}

		internal override void PaintEventEnd (ref Message msg, IntPtr handle, bool client)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

			// FIXME: Pop is causing invalid stack ops sometimes; race condition?
			try {
				Graphics dc = (Graphics)hwnd.drawing_stack.Pop();
				dc.Flush ();
				dc.Dispose ();
			
				PaintEventArgs pe = (PaintEventArgs)hwnd.drawing_stack.Pop();
				pe.SetGraphics (null);
				pe.Dispose ();  
			} catch {
			}
		}

		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
		{
			bool peeking = 0 == ((uint) PeekMessageFlags.PM_REMOVE & flags);
			PumpNativeEvent (false, ref msg, false);
			if (msg.message != Msg.WM_NULL && wFilterMin <= (int)msg.message && (int)msg.message <= wFilterMax && (hWnd == IntPtr.Zero || hWnd == msg.hwnd)) {
				if (!peeking) {
					PumpNativeEvent (false, ref msg, true); // Pop
				}
				return true;
			}
			return false;
		}

		internal override bool PostMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam)
		{
			MSG msg = new MSG
			{
				hwnd = hwnd,
				message = message,
				wParam = wParam,
				lParam = lParam
			};
			EnqueueMessage (msg);
			return true;
		}

		internal override void PostQuitMessage (int exitCode)
		{
			NSWindow winWrap = NSApplication.SharedApplication.MainWindow;
			if (winWrap != null)
				PostMessage (winWrap.ContentView.Handle, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
			PostMessage (IntPtr.Zero, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
		}

		internal override void RequestAdditionalWM_NCMessages (IntPtr hwnd, bool hover, bool leave) {
		}

		internal override void RequestNCRecalc (IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

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
		
		internal override void SendAsyncMethod (AsyncMethodData method) {
			var handle = GCHandle.Alloc (method);
			NSApplication.SharedApplication.BeginInvokeOnMainThread (delegate {
				XplatUIDriverSupport.ExecuteClientMessage(handle);
			});
		}

		internal override IntPtr SendMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam)
		{
			IntPtr result = IntPtr.Zero;
			if (NSThread.IsMain)
			{
				result = Application.SendMessage(hwnd, message, wParam, lParam);
			}
			else
			{
				NSApplication.SharedApplication.InvokeOnMainThread(delegate
				{
					result = Application.SendMessage(hwnd, message, wParam, lParam);
				});
			}

			return result;
		}

		internal override int SendInput(IntPtr hwnd, Queue keys)
		{
			int result = 0;
			if (NSThread.IsMain)
				 result = SendInputInternal(hwnd, keys);
			else
				NSApplication.SharedApplication.InvokeOnMainThread(() => { 
					result = SendInputInternal(hwnd, keys);
			});
			return result;
		}

		int SendInputInternal(IntPtr hwnd, Queue keys)
		{

			while (keys.Count != 0)
			{
				var msg = (MSG)keys.Dequeue();
				var e = CocoaInternal.KeysConverter.ConvertKeyEvent(hwnd, msg, ref key_modifiers);

				if (e != null)
					NSApplication.SharedApplication.PostEvent(e, false);
			}

			return 0;
		}

		internal override void SetCaretPos (IntPtr handle, int x, int y)
		{
			if (IntPtr.Zero != handle && handle == Caret.Hwnd) {
				Caret.X = x;
				Caret.Y = y;

				CaretView.Frame = new NSRect(Caret.rect.X + (int)CaretView.Superview.AlignmentRectInsets.Left, Caret.rect.Top + (int)CaretView.Superview.AlignmentRectInsets.Top, Caret.rect.Width, Caret.rect.Height);

				Caret.Timer.Stop ();
				HideCaret ();
				if (0 < Caret.Visible) {
					ShowCaret ();
					Caret.Timer.Start ();
				}
			}
		}

		internal override void SetClipRegion (IntPtr handle, Region region)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				hwnd.UserClip = region;
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject (handle);
				if (vuWrap != null && vuWrap.Window != null && vuWrap.Window.ContentView == vuWrap) {
					if (region != null) {
						vuWrap.Window.BackgroundColor = NSColor.Clear;
						vuWrap.Window.IsOpaque = false;
					}
					else {
						vuWrap.Window.BackgroundColor = NSColor.WindowBackground;
						vuWrap.Window.IsOpaque = true;
					}
					vuWrap.Display();
					if (vuWrap.Window.HasShadow)
						vuWrap.Window.InvalidateShadow();
				}
			}
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
				CGDisplayMoveCursorToPoint (screenNumber.UInt32Value, new NSPoint (x, y));
			}
		}

		internal override void SetFocus (IntPtr handle)
		{
			if (ActiveWindow != IntPtr.Zero) {
				var activeWindowWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(ActiveWindow);
				var activeContentView = activeWindowWrap.Window.FirstResponder as MonoContentView;
				IntPtr prevFocusHandle = IntPtr.Zero;
				if (activeContentView != null && activeContentView.FocusHandle != IntPtr.Zero)
					prevFocusHandle = activeContentView.FocusHandle;
				if (prevFocusHandle == handle)
					return;

				var view = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				var window = view != null ? view.Window : null;
				if (window == null)
					return;
				
				var contentView = window.ContentView as MonoContentView;
				var differentWindows = contentView.Handle != ActiveWindow;

				// ToolStripDropDown and other controls can be placed not in the active window,
				// but in it's own NSWindow (that cannot become key nor active), so that it can overlap the active window.
				// In this case, we have to save the original active window's focus handle.
				// We save it to the unused FocusHandle property of the ContentView of the dropdown's NSWindow.
				if (differentWindows)
					contentView.FocusHandle = prevFocusHandle;

				activeContentView = (MonoContentView)activeWindowWrap.Window.ContentView;
				activeContentView.FocusHandle = handle;

				if (activeWindowWrap.Window.FirstResponder != activeContentView)
					activeWindowWrap.Window.MakeFirstResponder(activeContentView);

				if (prevFocusHandle != IntPtr.Zero)
					SendMessage(prevFocusHandle, Msg.WM_KILLFOCUS, handle, IntPtr.Zero);

				if (handle != IntPtr.Zero && GetFocus() == handle)
					SendMessage(handle, Msg.WM_SETFOCUS, prevFocusHandle, IntPtr.Zero);
			}
		}

		internal override void SetIcon (IntPtr handle, Icon icon)
		{
			#if MONOMAC // The native mac application package already contains the app icon.
			ApplicationContext context = Application.MWFThread.Current.Context;
			if ( context != null && context.MainForm != null && context.MainForm.Handle == handle) {
				if (icon == null) { 
					NSApplication.SharedApplication.ApplicationIconImage = null;
				} else {
					var bitmap = new Bitmap (128, 128);
					using (Graphics g = Graphics.FromImage (bitmap)) {
						g.DrawIcon (new Icon(icon, 128, 128), new Rectangle(0, 0, 128, 128));
					}

					var stream = new System.IO.MemoryStream();
					bitmap.Save (stream, System.Drawing.Imaging.ImageFormat.Png);

					NSData data = NSData.FromArray(stream.ToArray());
					NSImage image = new NSImage (data);
					NSApplication.SharedApplication.ApplicationIconImage = image;
				}
			}
			#endif
		}
		
		internal override void SetModal (IntPtr handle, bool Modal)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			NSWindow winWrap = vuWrap.Window;
			if (Modal)
			{
				IntPtr session = NSApplication.SharedApplication.BeginModalSession(winWrap);
				ModalSessions.Push(session);
			}
			else
				NSApplication.SharedApplication.EndModalSession (ModalSessions.Pop ());
		}

		internal override IntPtr SetParent (IntPtr handle, IntPtr parent)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Hwnd newParent = Hwnd.ObjectFromHandle (parent);
			NSView newParentWrap = null != newParent ? (NSView)ObjCRuntime.Runtime.GetNSObject(parent) : null;
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
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
					//vuWrap.Retain ();
					vuWrap.RemoveFromSuperview ();
				}

				hwnd.Parent = newParent;
				if (newParentWrap != null) {
					newParentWrap.AddSubview (vuWrap);
					vuWrap.Frame = MonoToNativeFramed (new Rectangle (hwnd.X + newParent.ClientRect.X, hwnd.Y + newParent.ClientRect.Y, hwnd.Width, hwnd.Height), newParentWrap.Frame.Height);
					//if (adoption)
					//	vuWrap.Release ();
				}
			}

			return IntPtr.Zero;
		}

		#if XAMARINMAC
		internal override void SetTimer(Timer timer)
		{
			if (timer.window != IntPtr.Zero)
				KillTimer(timer);

			var nstimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromMilliseconds(timer.Interval), (t) =>
			{
				if (NSThread.IsMain)
					timer.FireTick();
				else
					NSApplication.SharedApplication.InvokeOnMainThread(timer.FireTick);
			});
			//nstimer.Retain();
			timer.window = nstimer.Handle;
		}
		#elif MONOMAC
		internal override void SetTimer (Timer timer) {
			if (timer.window != IntPtr.Zero)
				KillTimer(timer);

			var nstimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromMilliseconds(timer.Interval), () => {
				if (NSThread.IsMain)
					timer.FireTick();
				else 
					NSApplication.SharedApplication.InvokeOnMainThread(timer.FireTick);
			});
			nstimer.Retain();
			timer.window = nstimer.Handle;
		}
		#endif

		internal override bool SetTopmost (IntPtr hWnd, bool Enabled)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hWnd);
			NSWindow winWrap = vuWrap.Window;
			if (winWrap != null)
				winWrap.Level = Enabled ? NSWindowLevel.ModalPanel : NSWindowLevel.Normal;
			return true;
		}

		internal override bool SetOwner (IntPtr hWnd, IntPtr hWndOwner) {
			// TODO: Handle other cases, where objects are not top level windows but views.

			if (hWnd == IntPtr.Zero || hWndOwner == IntPtr.Zero)
				return false;

			MonoWindow winWrap = ((NSView)ObjCRuntime.Runtime.GetNSObject(hWnd))?.Window as MonoWindow;
			NSWindow winOwnerWrap = ((NSView)ObjCRuntime.Runtime.GetNSObject(hWndOwner))?.Window;

			if (winWrap != null && winWrap != winOwnerWrap)
			{
				winWrap.owner = winOwnerWrap;

				if (winWrap.ParentWindow != null)
					winWrap.ParentWindow.RemoveChildWindow(winWrap);

				// If not visible, do not call AddChildWindow now, because it would immediately show the child window.
				var hwnd = Hwnd.ObjectFromHandle(hWnd);
				var visible = hwnd != null && hwnd.Visible;
				if (winOwnerWrap != null && visible)
					winOwnerWrap.AddChildWindow(winWrap, NSWindowOrderingMode.Above);
			}

			return true;
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			if (!visible && Grab.Hwnd == handle)
				UngrabWindow(handle);

			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			NSWindow winWrap = vuWrap.Window;
			if (winWrap != null && winWrap.ContentView == vuWrap) {
				if (visible)
				{
					if (Control.FromHandle(handle).ActivateOnShow)
						winWrap.MakeKeyAndOrderFront(winWrap);
					else
						winWrap.OrderFront(winWrap);

					// See SetOwner
					var monoWin = winWrap as MonoWindow;
					if (monoWin != null && monoWin.owner != null && monoWin.owner != monoWin.ParentWindow)
						monoWin.owner.AddChildWindow(monoWin, NSWindowOrderingMode.Above);
				}
				else
				{
					MoveFocusOutside(handle, winWrap);
					winWrap.OrderOut(winWrap);
				}
			} else {
				if (!visible && winWrap != null)
				{
					NSView activeWindowWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(ActiveWindow);
					if (activeWindowWrap != null && activeWindowWrap.Window == winWrap)
					{
						MonoContentView contentView = winWrap.FirstResponder as MonoContentView;
						if (contentView != null)
						{
							IntPtr focusHandle = contentView.FocusHandle;
							if (focusHandle != IntPtr.Zero)
								SendMessage(focusHandle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
						}
					}
				}

				vuWrap.Hidden = !visible;
			}

			if (visible)
				SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

			hwnd.visible = visible;
			hwnd.Mapped = true;
			return true;
		}

		internal void MoveFocusOutside(IntPtr handle, NSWindow winWrap)
		{
			// Do nothing if focus is not inside the hierarchy.
			var ctrl = Control.FromHandle(handle);
			var focused = Control.FromHandle(GetFocus());
			if (ctrl == null || focused == null || !ctrl.Contains(focused))
				return;
			
			// Find owning NSWindow and it's content view
			var view = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			var window = view != null ? view.Window : null;
			if (window == null)
				return;
			var contentView = window.ContentView as MonoContentView;

			// Find active NSWindow and its content view
			var activeView = (NSView)ObjCRuntime.Runtime.GetNSObject(ActiveWindow);
			if (activeView == null)
				return; // Should not happen
			var activeContentView = activeView.Window.ContentView as MonoContentView;

			// Check if our control is in a separate NSWindow (ToolStripDropDownd etc)
			if (contentView != activeContentView)
			{
				var savedFocus = contentView.FocusHandle;
				contentView.FocusHandle = IntPtr.Zero;
				SetFocus(savedFocus);
				return;
			}

			var winParent = winWrap.ParentWindow as MonoWindow;
			if (winParent != null)
			{
				var form = focused.FindForm();
				if (form == null)
				{
					var parent = Control.FromHandle(winParent.ContentView.Handle);
					if (parent != null)
						form = parent.FindForm();
				}
				if (form != null)
				{
					var focus = winParent.FindSuperControl(ctrl);
					if (focus == null)
						focus = winParent.FindNextControl(form, ctrl);
					SetFocus(focus != null ? focus.Handle : IntPtr.Zero);
				}
			}
		}

		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			//Dnd.SetAllowDrop (Hwnd.ObjectFromHandle (handle), value);
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data, DragDropEffects allowed_effects)
		{
			/*Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd == null)
				throw new ArgumentException ("Attempt to begin drag from invalid window handle (" + 
								handle.ToInt32 () + ").");

			return Dnd.StartDrag (hwnd.ClientWindow, data, allowed_effects);*/
			//throw new NotImplementedException ();
			return DragDropEffects.None;
		}

		internal override void SetBorderStyle (IntPtr handle, FormBorderStyle border_style) {
			Form form = Control.FromHandle (handle) as Form;
			if (form != null && form.window_manager == null && (border_style == FormBorderStyle.FixedToolWindow ||
				border_style == FormBorderStyle.SizableToolWindow)) {
				form.window_manager = new ToolWindowManager (form);
			}

			if (form != null && form.FormBorderStyle != border_style)
				RequestNCRecalc (handle);
		}

		internal override void SetMenu (IntPtr handle, Menu menu) {
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.menu = menu;

			RequestNCRecalc(handle);
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
					NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
					vuWrap.Hidden = false;
				}
				hwnd.zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				hwnd.zero_sized = true;
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				vuWrap.Hidden = true;
			}

			bool nomove = hwnd.x == x && hwnd.y == y;
			bool nosize = hwnd.width == width && hwnd.height == height;

			// Save a server roundtrip (and prevent a feedback loop)
			if (nomove && nosize)
				return;

			hwnd.x = x;
			hwnd.y = y;
			hwnd.width = width;
			hwnd.height = height;

			if (! hwnd.zero_sized) {
				HwndPositionToNative (hwnd);

#if DriverDebug
				NSView		vuWrap	= (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
				NSWindow	winWrap	= vuWrap.Window;
				if ( null != winWrap )
					Console.WriteLine ("{0} SetWindowPos( {1}, {2}, WxH: {3} x {4} )", winWrap.Title, x, y, width, height);
#endif

				PerformNCCalc (hwnd);

                var wp = new WINDOWPOS
                {
                    hwnd = handle,
                    hwndAfter = IntPtr.Zero,
                    x = x,
                    y = y,
                    cx = width,
                    cy = height,
                    flags = (uint)(XplatUIWin32.SetWindowPosFlags.SWP_NOZORDER
							| XplatUIWin32.SetWindowPosFlags.SWP_NOACTIVATE
							| XplatUIWin32.SetWindowPosFlags.SWP_NOOWNERZORDER
							| XplatUIWin32.SetWindowPosFlags.SWP_NOREPOSITION
							| (nomove ? XplatUIWin32.SetWindowPosFlags.SWP_NOMOVE : 0)
							| (nosize ? XplatUIWin32.SetWindowPosFlags.SWP_NOSIZE : 0)
					),
                };
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(wp));
                Marshal.StructureToPtr(wp, ptr, true);

                SendMessage(hwnd.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, ptr);

                Marshal.FreeHGlobal(ptr);
			}
		}
		
		internal override void SetWindowState (IntPtr handle, FormWindowState state)
		{
			NSView		vuWrap	= (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			NSWindow	winWrap	=  vuWrap.Window;

			switch (state) {
				case FormWindowState.Minimized: {
					winWrap.Miniaturize (vuWrap);
					break;
				}
				case FormWindowState.Normal: {
					if (winWrap.IsMiniaturized)
						winWrap.Deminiaturize (vuWrap);
					else if (winWrap.IsZoomed)
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

		internal override void SetWindowStyle (IntPtr handle, CreateParams cp)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			SetHwndStyles(hwnd, cp);
			if (vuWrap.Window != null && vuWrap.Window.ContentView == vuWrap)
				vuWrap.Window.StyleMask = StyleFromCreateParams(cp);
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
			NSView itVuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			NSView afterVuWrap = null;
			NSView itSuperVuWrap = itVuWrap.Superview;
			bool results = true;

#if DriverDebug
			Console.WriteLine ("SetZOrder ({0}, {1}, {2}, {3})", hwnd, afterHwnd, Top, Bottom);
#endif

			if (IntPtr.Zero != after_handle) {
				afterVuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (after_handle);
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
			Stack<NSAutoreleasePool> stack;
			if (!pools.TryGetValue(thread, out stack))
				pools.Add(thread, stack = new Stack<NSAutoreleasePool>());
			stack.Push(new NSAutoreleasePool());
			return new object ();
		}

		[MonoTODO]
		internal override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			//throw new NotImplementedException();
			tt = null;
			return false;
		}

		[MonoTODO]
		internal override bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			//throw new NotImplementedException();
			tt = null;
			return false;
		}

		[MonoTODO]
		internal override void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			//throw new NotImplementedException();
		}

		[MonoTODO]
		internal override void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon)
		{
			//throw new NotImplementedException ();
		}

		internal override bool Text (IntPtr handle, string text)
		{
			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			if (vuWrap.Window != null && vuWrap.Window.ContentView == vuWrap)
				vuWrap.Window.Title = text;
			else
				vuWrap.NeedsDisplay = true;
			return true;
		}

		internal override void UpdateWindow (IntPtr handle)
		{
			//NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			//vuWrap.DisplayIfNeeded();
		}

		internal override bool TranslateMessage(ref MSG msg) {
			bool result = false;

			if (!result)
				result = TranslateKeyMessage(ref msg);
			if (!result)
				result = TranslateMouseMessage(ref msg);

			return result;
		}

		internal virtual bool TranslateKeyMessage (ref MSG msg) {
			bool sent = true;
			switch (msg.message)
			{
				case Msg.WM_KEYDOWN:
					for (string s = PopChars(); s.Length != 0; s = PopChars())
						foreach(var c in s)
							PostMessage(msg.hwnd, Msg.WM_CHAR, (IntPtr)c, msg.lParam);
					break;
				case Msg.WM_SYSKEYDOWN:
					for (string s = PopChars(); s.Length != 0; s = PopChars())
						foreach (var c in s)
							PostMessage(msg.hwnd, Msg.WM_SYSCHAR, (IntPtr)c, msg.lParam);
					break;
				case Msg.WM_KEYUP:
				case Msg.WM_SYSKEYUP:
					// Just return true, according to the docs
					break;
				default:
					sent = false;
					break;
			}

			return sent;
		}
			
		internal virtual bool TranslateMouseMessage (ref MSG msg) {
			return false;
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

			using (g = Graphics.FromImage (new Bitmap (1, 1))) {
				width = (float)(g.MeasureString (magic_string, font).Width / magic_number);
				return new SizeF (width, font.Height);
			}
		}

		internal override Point MousePosition {
			get {
				return NativeToMonoScreen (NSEvent.CurrentMouseLocation);
			}
		}
#endregion Override Methods XplatUIDriver


#region Override Properties XplatUIDriver

		// MSDN: The keyboard repeat-speed setting, from 0 (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second).
		internal override int KeyboardSpeed
		{ 
			get
			{
				var defaults = NSUserDefaults.StandardUserDefaults;
				var repeat = Math.Max(defaults.IntForKey("KeyRepeat"), 2); //2 ~ 30ms, 1 ~ 15mss
				return ToWindowsKeyboardSpeed((int)repeat);
			}
		}

		// MSDN: The keyboard repeat-delay setting, from 0 (approximately 250 millisecond delay) through 3 (approximately 1 second delay).
		internal override int KeyboardDelay {
			get
			{
				var defaults = NSUserDefaults.StandardUserDefaults;
				var delay = defaults.IntForKey("InitialKeyRepeat");
				return ToWindowsKeyboardSpeed((int)delay);
			}
		}

		internal int ToWindowsKeyboardSpeed(int repeat)
		{
			double ms = 15.0 * repeat;
			double rps = 1000.0 / ms;
			const double a = 31.0 / (30.0 - 2.5);
			const double b = -a * 2.5;
			double speed = a * rps + b;
			return (int)Math.Round(speed);
		}

		internal override int CaptionHeight {
			get {
				return 19;
			}
		}

		internal override  Size CursorSize { get{ throw new NotImplementedException(); } }
		internal override  bool DragFullWindows { get{ return true; } }
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
				Keys keys = Keys.None;
				if ((NSEventModifierMask.ShiftKeyMask & key_modifiers) != 0)        { keys |= Keys.Shift; }
				if ((NSEventModifierMask.CommandKeyMask & key_modifiers) != 0)      { keys |= Keys.Alt; }
				if ((NSEventModifierMask.ControlKeyMask & key_modifiers) != 0)      { keys |= Keys.Control; }
				return keys;
			}
		}
		internal override Size SmallIconSize { get{ throw new NotImplementedException(); } }
		internal override int MouseButtonCount { get{ throw new NotImplementedException(); } }
		internal override bool MouseButtonsSwapped { get{ throw new NotImplementedException(); } }
		internal override bool MouseWheelPresent { get{ throw new NotImplementedException(); } }

		internal override MouseButtons MouseButtons {
			get {
				MouseButtons result = MouseButtons.None;
				var mouseButtons = NSEvent.CurrentPressedMouseButtons;
				if ((mouseButtons & 1) != 0)
					result |= MouseButtons.Left;
				if ((mouseButtons & 2) != 0)
					result |= MouseButtons.Right;
				if ((mouseButtons & 4) != 0)
					result |= MouseButtons.Middle;
				if ((mouseButtons & 8) != 0)
					result |= MouseButtons.XButton1;
				if ((mouseButtons & 16) != 0)
					result |= MouseButtons.XButton2;
				return result;
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

		internal override Screen[] AllScreens
		{
			get
			{
				List<Screen> screens = new List<Screen>();
				int index = 0;
				foreach (var nsScreen in NSScreen.Screens) {
					Rectangle frame = NativeToMonoScreen (nsScreen.Frame);
					Rectangle visibleFrame = NativeToMonoScreen (nsScreen.VisibleFrame);
					screens.Add(new Screen(index == 0, (index + 1).ToString(), frame, visibleFrame));
					index++;
				}
				return screens.ToArray();
			}
		}

		internal override bool ThemesEnabled {
			get {
				return XplatUICocoa.themes_enabled;
			}
		}

		internal static void PushChars(string chars)
		{
			lock (charsQueue) {
				charsQueue.Enqueue (chars);
			}
		}

		internal static string PopChars()
		{
			lock (charsQueue) {
				return charsQueue.Count > 0 ? charsQueue.Dequeue() : String.Empty;
			}
		}
			
		// Event Handlers
		internal override event EventHandler Idle;
#endregion Override properties XplatUIDriver

		[DllImport("/System/Library/Frameworks/CoreGraphics.framework/Versions/Current/CoreGraphics")]
		extern static void CGDisplayMoveCursorToPoint (UInt32 display, NSPoint point);

	}
	// Windows / Native messaging support

	internal static class NSEventExtension {
		internal static MSG ToMSG(this NSEvent e) {
			var addr = new IntPtr(e.Data1);
			try {
				var handle = GCHandle.FromIntPtr (addr);
				var msg = (MSG)handle.Target;
				handle.Free ();
				return msg;
			} 
			catch (Exception x) 
			{
				Debug.WriteLine ("Failed decoding MSG from NSEvent: " + x.ToString());
				return new MSG ();
			}
		}
	}

	internal static class MSGExtension {
		public static NSEvent ToNSEvent(this MSG msg) {
			var handle = GCHandle.Alloc(msg);
			var ptr = (int)GCHandle.ToIntPtr (handle).ToInt64();

			return NSEvent.OtherEvent (NSEventType.ApplicationDefined, CGPoint.Empty, 0, NSDate.Now.SecondsSinceReferenceDate, 0, null, XplatUICocoa.NSEventTypeWindowsMessage, ptr, 0);
		}
	}
}

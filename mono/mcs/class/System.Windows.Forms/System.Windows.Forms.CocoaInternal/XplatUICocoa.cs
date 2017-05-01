﻿//
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
using System.Drawing.Mac;
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
using CoreGraphics;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Windows.Forms.CocoaInternal;
using MonoMac.CoreGraphics;
using ObjCRuntime = MonoMac.ObjCRuntime;
using nint = System.Int32;
using nfloat = System.Single;
#endif

namespace System.Windows.Forms {

	internal class XplatUICocoa : XplatUIDriver {
#region Local Variables
		// General driver variables
		private static XplatUICocoa Instance;
		private static int RefCount;
		private static bool themes_enabled;

		// Internal members available to the event handler sub-system
		internal static NSWindow ReverseWindow;
		internal static NSView CaretView;

		// Instance members
		internal static NSEventModifierMask key_modifiers = 0;
		internal bool translate_modifier = false;

		// Event handlers
		private Cocoa.MonoApplicationDelegate applicationDelegate;
		
		// Cocoa Specific
		internal GrabStruct Grab;
		internal Cocoa.Caret Caret;
		internal readonly Stack<IntPtr> ModalSessions = new Stack<IntPtr>();
		internal float screenHeight;

		private Dictionary<Thread, Stack<NSAutoreleasePool>> pools = new Dictionary<Thread, Stack<NSAutoreleasePool>>();

		// Message loop
		private static bool GetMessageResult;
#if XAMARINMAC
		System.Reflection.MethodInfo NextEventMethod;
#endif
		private static bool ReverseWindowMapped;

		static readonly object instancelock = new object ();

		internal const int NSEventTypeWindowsMessage = 12345;

		private CGPoint nextWindowLocation;

		private Dictionary<IntPtr, object> keepAlivePool = new Dictionary<IntPtr, object>();

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
			// Initialize the event handlers
			applicationDelegate = new Cocoa.MonoApplicationDelegate (this);
			NSApplication.SharedApplication.Delegate = applicationDelegate;

			// Make sure the Apple event handlers get registered
			using (new NSAutoreleasePool())
				NSApplication.SharedApplication.FinishLaunching();

			// Cache main screen height for flipping screen coordinates.
			Size size;
			GetDisplaySize(out size);
			screenHeight = size.Height;

			// Initialize the Caret
			Caret.Timer = new Timer ();
			Caret.Timer.Interval = 500;
			Caret.Timer.Tick += new EventHandler (CaretCallback);

			// Initialize the Cocoa Specific stuff
			ReverseWindow = new NSWindow(CGRect.Empty, NSWindowStyle.Borderless, NSBackingStore.Buffered, true);
			CaretView = new NSView(CGRect.Empty);
			CaretView.WantsLayer = true;
			CaretView.Layer.BackgroundColor = NSColor.Black.CGColor;

			// Message loop
			GetMessageResult = true;

			ReverseWindowMapped = false;

			nextWindowLocation = CGPoint.Empty;
		}

		internal void ScreenToClientWindow (IntPtr handle, ref CGPoint point)
		{
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ScreenToClientWindow() requires NSView*");

			NSWindow windowWrapper = viewWrapper.Window;
			if (windowWrapper == null) {
				point = new CGPoint (0, 0);
				return;
			}

			point = windowWrapper.ConvertScreenToBase (point);
			point = viewWrapper.ConvertPointFromView (point, null);
			if (viewWrapper is IClientView) {
				point.X -= ((IClientView)viewWrapper).ClientBounds.X;
				point.Y -= ((IClientView)viewWrapper).ClientBounds.Y;
			}
		}

		internal void ClientWindowToScreen (IntPtr handle, ref CGPoint point)
		{
			NSView viewWrapper = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			NSWindow windowWrapper = viewWrapper.Window;
			if (viewWrapper != null && windowWrapper != null) {
				if (viewWrapper is IClientView) {
					point.X += ((IClientView)viewWrapper).ClientBounds.X;
					point.Y += ((IClientView)viewWrapper).ClientBounds.Y;
				}
				point = viewWrapper.ConvertPointToView (point, null);
				point = windowWrapper.ConvertBaseToScreen (point);
			}
		}

		internal void EnqueueMessage (MSG msg) {
			NSApplication.SharedApplication.PostEvent(msg.ToNSEvent(), false);
		}

		internal void PositionWindowInClient (Rectangle rect, NSWindow window, IntPtr handle)
		{
			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			/*if (vuWrap is IClientView)
			{
				rect.X += (int)((IClientView)vuWrap).ClientBounds.X;
				rect.Y += (int)((IClientView)vuWrap).ClientBounds.Y;
			}*/

			CGRect nsrect = MonoToNativeFramed (rect, vuWrap);

			CGPoint location = nsrect.Location;
            ClientWindowToScreen (handle, ref location);
			nsrect = new CGRect(location, nsrect.Size);
			nsrect = window.FrameRectFor (nsrect);
			window.SetFrame (nsrect, false);
#if DriverDebug
			Console.WriteLine ("PositionWindowInClient ({0}, {1}) : {2}", rect, window, nsrect);
#endif
		}

		internal CGPoint MonoToNativeScreen (Point monoPoint)
		{
			return new CGPoint (monoPoint.X, screenHeight - monoPoint.Y);
		}

		internal CGPoint MonoToNativeFramed (Point monoPoint, NSView view)
		{
			if (view.IsFlipped) {
				return new CGPoint(monoPoint.X, monoPoint.Y);
			} else {
				return new CGPoint(monoPoint.X, view.Frame.Height - monoPoint.Y);
			}
		}

		internal Point NativeToMonoScreen (CGPoint nativePoint)
		{
			return new Point ((int) nativePoint.X, (int) (screenHeight - nativePoint.Y));
		}

		internal Point NativeToMonoFramed (CGPoint nativePoint, NSView view)
		{
			if (view.IsFlipped) {
				return new Point((int)nativePoint.X, (int)nativePoint.Y);
			} else {
				return new Point((int)nativePoint.X, (int)(view.Frame.Height - nativePoint.Y));
			}
		}

		internal CGRect MonoToNativeScreen (Rectangle monoRect)
		{
			return new CGRect(monoRect.Left, screenHeight - monoRect.Bottom, monoRect.Width, monoRect.Height);
		}

		internal CGRect MonoToNativeFramed (Rectangle monoRect, NSView view)
		{
			if (view.IsFlipped) {
				return new CGRect(monoRect.X, monoRect.Y, monoRect.Width, monoRect.Height);
			} else {
				return new CGRect(monoRect.X, view.Frame.Height - monoRect.Y, monoRect.Width, monoRect.Height);
			}
		}

		internal Rectangle NativeToMonoScreen (CGRect nativeRect)
		{
			return new Rectangle ((int) nativeRect.Left, (int) (screenHeight - nativeRect.Bottom), 
				(int) nativeRect.Size.Width, (int) nativeRect.Size.Height);
		}

		internal Rectangle NativeToMonoFramed (CGRect nativeRect, NSView view)
		{
			if (view.IsFlipped) {
				return new Rectangle((int)nativeRect.X, (int)nativeRect.Y, (int)nativeRect.Width, (int)nativeRect.Height);
			} else {
				return new Rectangle((int)nativeRect.X, (int)(view.Frame.Height - nativeRect.Y), (int)nativeRect.Width, (int)nativeRect.Height);
			}
		}
#endregion
		
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
			CGPoint nspoint = MonoToNativeScreen (point);

			ScreenToClientWindow (handle, ref nspoint);

			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			point = NativeToMonoFramed (nspoint, vuWrap);
			if (vuWrap is IClientView)
			{
				point.X -= (int)((IClientView)vuWrap).ClientBounds.X;
				point.Y -= (int)((IClientView)vuWrap).ClientBounds.Y;
			}
			return point;
		}

		private Point ConvertClientPointToScreen (IntPtr handle, Point point)
		{
			NSView vuWrap = (NSView) ObjCRuntime.Runtime.GetNSObject (handle);
			if (vuWrap == null)
				return Point.Empty;
			/*if (vuWrap is IClientView)
			{
				point.X += (int)((IClientView)vuWrap).ClientBounds.X;
				point.Y += (int)((IClientView)vuWrap).ClientBounds.Y;
			}*/

			CGPoint nspoint = MonoToNativeFramed (point, vuWrap);

			ClientWindowToScreen (handle, ref nspoint);
			point = NativeToMonoScreen (nspoint);

			return point;
		}

		private Point ConvertScreenPointToNonClient (IntPtr handle, Point point)
		{
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ConvertScreenPointToNonClient() requires NSView*");

			CGPoint native_point = MonoToNativeScreen (point);
			NSWindow windowWrapper = viewWrapper.Window;

			native_point = windowWrapper.ConvertScreenToBase(native_point);
			native_point = viewWrapper.ConvertPointFromView (native_point, null);

			Point converted_point = NativeToMonoFramed (native_point, viewWrapper);

			return converted_point;
		}

		private Point ConvertNonClientPointToScreen (IntPtr handle, Point point)
		{
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject (handle) as NSView;
			if (viewWrapper == null)
				throw new ArgumentException ("XplatUICocoa.ConvertScreenPointToNonClient() requires NSView*");

			CGPoint native_point = MonoToNativeFramed (point, viewWrapper);
			NSWindow windowWrapper = viewWrapper.Window;

			native_point = viewWrapper.ConvertPointToView (native_point, null);
			native_point = windowWrapper.ConvertBaseToScreen(native_point);

			Point converted_point = NativeToMonoScreen (native_point);

			return converted_point;
		}

		private static bool IsMouseEvent (NSEventType type)
		{
			switch (type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseUp:
				case NSEventType.LeftMouseDragged:
				case NSEventType.RightMouseDragged:
				case NSEventType.OtherMouseDragged:
				case NSEventType.ScrollWheel:
				case NSEventType.MouseMoved:
					return true;
			}
			return false;
		}

		private bool PumpNativeEvent (bool wait, ref MSG msg, bool dequeue = true)
		{
			msg.message = Msg.WM_NULL;

			NSDate timeout = wait ? NSDate.DistantFuture : NSDate.DistantPast;
			NSApplication NSApp = NSApplication.SharedApplication;

			NSEvent evtRef;
#if XAMARINMAC
			evtRef = (NSEvent) this.NextEventMethod.Invoke (NSApp, new object[] { (nuint)0xffffffffffffffff, timeout, (string)NSRunLoop.NSDefaultRunLoopMode, dequeue });
#else
			evtRef = NSApp.NextEvent (NSEventMask.AnyEvent, timeout, NSRunLoop.NSDefaultRunLoopMode, dequeue);
#endif
			if (evtRef == null)
				return false;

			// Is it Windows message?
			if (evtRef.Type == NSEventType.ApplicationDefined && evtRef.Subtype == NSEventTypeWindowsMessage) {
				msg = evtRef.ToMSG();
				return true;
			}

			if (dequeue) {
				if (Grab.Hwnd != IntPtr.Zero && IsMouseEvent(evtRef.Type) && evtRef.Window != null) {
					var grabView = (NSView)ObjCRuntime.Runtime.GetNSObject(Grab.Hwnd);
					if (grabView.Window.WindowNumber != evtRef.WindowNumber && evtRef.Type != NSEventType.ScrollWheel)
					{
						evtRef = NSEvent.MouseEvent(
							evtRef.Type,
							grabView.Window.ConvertScreenToBase(evtRef.Window.ConvertBaseToScreen(evtRef.LocationInWindow)),
							evtRef.ModifierFlags,
							evtRef.Timestamp,
							grabView.Window.WindowNumber,
							null,
							0,
							evtRef.ClickCount,
							evtRef.Pressure);
					}
					switch (evtRef.Type)
					{
						case NSEventType.LeftMouseDown: grabView.MouseDown(evtRef); break;
						case NSEventType.RightMouseDown: grabView.RightMouseDown(evtRef); break;
						case NSEventType.OtherMouseDown: grabView.OtherMouseDown(evtRef); break;
						case NSEventType.LeftMouseUp: grabView.MouseUp(evtRef); break;
						case NSEventType.RightMouseUp: grabView.RightMouseUp(evtRef); break;
						case NSEventType.OtherMouseUp: grabView.OtherMouseUp(evtRef); break;
						case NSEventType.LeftMouseDragged: grabView.MouseDragged(evtRef); break;
						case NSEventType.RightMouseDragged: grabView.RightMouseDragged(evtRef); break;
						case NSEventType.OtherMouseDragged: grabView.OtherMouseDragged(evtRef); break;
						case NSEventType.ScrollWheel: grabView.ScrollWheel(evtRef); break;
						case NSEventType.BeginGesture: grabView.BeginGestureWithEvent(evtRef); break;
						case NSEventType.EndGesture: grabView.EndGestureWithEvent(evtRef); break;
						case NSEventType.MouseMoved: grabView.MouseMoved(evtRef); break;
					}
				} else {
					NSApp.SendEvent(evtRef);
				}
				NSApp.UpdateWindows();
			}

			return true;
		}

		private void SendParentNotify(IntPtr child, Msg cause, int x, int y) {
			if (child == IntPtr.Zero)
				return;
			
			NSView viewWrapper = ObjCRuntime.Runtime.GetNSObject(child) as NSView;
			if (viewWrapper == null)
				return;
			if (viewWrapper is MonoView && ((MonoView)viewWrapper).ExStyle.HasFlag(WindowExStyles.WS_EX_NOPARENTNOTIFY))
				return;			
			if (viewWrapper.Superview == null || !(viewWrapper.Superview is MonoView))
				return;

			if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY)
				SendMessage(viewWrapper.Superview.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), child);
			else
				SendMessage(viewWrapper.Superview.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
			
			SendParentNotify (viewWrapper.Superview.Handle, cause, x, y);
		}

		private bool StyleSet (int s, WindowStyles ws) {
			return (s & (int)ws) == (int)ws;
		}

		private bool ExStyleSet (int ex, WindowExStyles exws) {
			return (ex & (int)exws) == (int)exws;
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
					//Hwnd hwnd = Hwnd.ObjectFromHandle (c.Handle);

					list.Add (c.Handle);
					CleanupCachedWindows (c.Handle);
				}

				for (int  i = 0; i < controls.Length; i ++) {
					AccumulateDestroyedHandles (controls[i], list);
				}
			}
		}

		private void CleanupCachedWindows(IntPtr handle)
		{
			if (GetFocus() == handle) {
				SendMessage (handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
			}

			if (Grab.Hwnd == handle) {
				Grab.Hwnd = IntPtr.Zero;
				Grab.Confined = false;
			}

			DestroyCaret (handle);
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

		internal override void Activate(IntPtr handle)
		{
			if (handle != IntPtr.Zero) {
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				if (vuWrap != null && vuWrap.Window != null && !vuWrap.Window.IsKeyWindow) {
					vuWrap.Window.MakeKeyAndOrderFront(vuWrap);
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
			if (cp.WindowStyle.HasFlag(WindowStyles.WS_CHILD)) {
				WindowRect = ClientRect;
			} else {
				var nsrect = NSWindow.FrameRectFor (MonoToNativeScreen(ClientRect), NSStyleFromStyle (cp.WindowStyle));
				WindowRect = NativeToMonoScreen(nsrect);
				if (menu != null) {
					WindowRect.Y -= MenuHeight;
					WindowRect.Height += MenuHeight;
				}
			}

			if (cp.WindowStyle.HasFlag(WindowStyles.WS_CHILD) || !cp.WindowStyle.HasFlag(WindowStyles.WS_CAPTION)) {
				if (cp.WindowExStyle.HasFlag(WindowExStyles.WS_EX_CLIENTEDGE) || cp.WindowExStyle.HasFlag(WindowExStyles.WS_EX_STATICEDGE))
					WindowRect = Rectangle.Inflate(ClientRect, Border3DSize.Width, Border3DSize.Height);
				else if (cp.WindowStyle.HasFlag(WindowStyles.WS_BORDER))
					WindowRect = Rectangle.Inflate(ClientRect, BorderSize.Width, BorderSize.Height);
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

		private NSWindowStyle NSStyleFromStyle(WindowStyles style)
		{
			NSWindowStyle attributes = NSWindowStyle.Borderless;
			if (style.HasFlag(WindowStyles.WS_CAPTION)) {
				attributes = NSWindowStyle.Titled;
				if (style.HasFlag(WindowStyles.WS_MINIMIZEBOX))
					attributes |= NSWindowStyle.Miniaturizable;
				if (style.HasFlag(WindowStyles.WS_THICKFRAME))
					attributes |= NSWindowStyle.Resizable;
				if (style.HasFlag(WindowStyles.WS_SYSMENU))
					attributes |= NSWindowStyle.Closable;
			}
			return attributes;
		}

		/*private bool IsUtilityWindow(NSView view)
		{
			var monoView = view as MonoView;
			if (monoView == null || view.Window == null || view.Window.ContentView != view)
				return false;
			if (!monoView.ExStyle.HasFlag(WindowExStyles.WS_EX_TOOLWINDOW))
				return false;
			if (monoView.Style.HasFlag(WindowStyles.WS_THICKFRAME))
				return true;
			if (monoView.Style.HasFlag(WindowStyles.WS_CAPTION))
				return true;
			return false;
		}*/

		private CGRect GetAlignmentRectForFrame(NSView view, CGRect rect)
		{
			if (view.Superview != null)
				return view.GetAlignmentRectForFrame(rect);

			// GetAlignmentRectForFrame on non-parented view always works on non-flipped coordinates
			var insets = view.AlignmentRectInsets;
			rect.X += insets.Left;
			rect.Y += insets.Top;
			rect.Width -= insets.Left + insets.Right;
			rect.Height -= insets.Top + insets.Bottom;

			return rect;
		}

		private CGRect GetFrameForAlignmentRect(NSView view, CGRect rect)
		{
			if (view.Superview != null)
				return view.GetFrameForAlignmentRect(rect);

			// GetFrameForAlignmentRect on non-parented view always works on non-flipped coordinates
			var insets = view.AlignmentRectInsets;
			rect.X -= insets.Left;
			rect.Y -= insets.Top;
			rect.Width += insets.Left + insets.Right;
			rect.Height += insets.Top + insets.Bottom;

			return rect;
		}

		internal override IntPtr CreateWindow (CreateParams cp)
		{
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
				ParentWrapper = (NSView)ObjCRuntime.Runtime.GetNSObject(cp.Parent);
				if (!isTopLevel)
					windowWrapper = ParentWrapper.Window;
			}

			bool cascade = false;
			if (X == int.MinValue || Y == int.MinValue) {
				if (isTopLevel) {
					X = (int)this.nextWindowLocation.X;
					Y = (int)this.nextWindowLocation.Y;
					cascade = true;
				} else {
					X = 0;
					Y = 0;
				}
			}

			Rectangle mWholeRect = new Rectangle (new Point (X, Y), new Size(Width, Height));
			CGRect WholeRect;
			if (!isTopLevel) {
				if (ParentWrapper != null) {
					var clientView = ParentWrapper as IClientView;
					if (clientView != null) {
						mWholeRect.X += (int)clientView.ClientBounds.X;
						mWholeRect.Y += (int)clientView.ClientBounds.Y;
					}
					WholeRect = MonoToNativeFramed (mWholeRect, ParentWrapper);
				} else {
					WholeRect = mWholeRect.ToCGRect();
				}
			} else {
				WholeRect = MonoToNativeScreen (mWholeRect);
			}
				
			if (isTopLevel) {
				NSWindowStyle attributes = NSStyleFromStyle(cp.WindowStyle);
				//SetAutomaticControlDragTrackingEnabledForWindow (, true);
				//ParentHandle = WindowView;
				WholeRect = NSWindow.ContentRectFor(WholeRect, attributes);
				windowWrapper = new MonoWindow(WholeRect, attributes, NSBackingStore.Buffered, true, this);
				WindowHandle = (IntPtr) windowWrapper.Handle;
				windowWrapper.WeakDelegate = windowWrapper;

				if ((cp.ClassStyle & 0x20000) != 0) // CS_DROPSHADOW
					windowWrapper.HasShadow = true;
				windowWrapper.CollectionBehavior = !cp.WindowStyle.HasFlag(WindowStyles.WS_MAXIMIZEBOX) ? NSWindowCollectionBehavior.FullScreenAuxiliary : NSWindowCollectionBehavior.Default;

				viewWrapper = new Cocoa.MonoContentView(this, WholeRect, cp.WindowStyle, cp.WindowExStyle);
				viewWrapper.AutoresizesSubviews = false;
				wholeHandle = (IntPtr)viewWrapper.Handle;
				windowWrapper.ContentView = viewWrapper;
				windowWrapper.InitialFirstResponder = viewWrapper;
				windowWrapper.SetOneShot(true);

				if (StyleSet(cp.Style, WindowStyles.WS_POPUP))
					windowWrapper.Level = NSWindowLevel.PopUpMenu;
				if (ParentWrapper != null)
					ParentWrapper.Window.AddChildWindow (windowWrapper, NSWindowOrderingMode.Above);

				if (cascade)
					this.nextWindowLocation = windowWrapper.CascadeTopLeftFromPoint(this.nextWindowLocation);
			} else {
				if (cp.control is IMacNativeControl) {
					var nativeView = ((IMacNativeControl)cp.control).CreateView();
					if (nativeView != null)
					{
						viewWrapper = nativeView;
						nativeView.Frame = GetFrameForAlignmentRect(nativeView, WholeRect);
					}
				}
				if (viewWrapper == null) {
					viewWrapper = new Cocoa.MonoView(this, WholeRect, cp.WindowStyle, cp.WindowExStyle);
					viewWrapper.AutoresizesSubviews = false;
				}
				wholeHandle = (IntPtr)viewWrapper.Handle;
				if (ParentWrapper != null)
					ParentWrapper.AddSubview(viewWrapper);
			}

			// Assign handle to control's native window before sending messages.
			if (null != cp.control) {
				cp.control.window.AssignHandle(viewWrapper.Handle);
#if DriverDebug
				if ("StackTraceMe" == cp.control.Name)
					Console.WriteLine ("{0}", new StackTrace (true));
#endif
			}

//			Dnd.SetAllowDrop (hwnd, true);

			Text (viewWrapper.Handle, cp.Caption);
			EnableWindow (viewWrapper.Handle, !StyleSet(cp.Style, WindowStyles.WS_DISABLED));
			
			SendMessage (viewWrapper.Handle, Msg.WM_NCCREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			if (viewWrapper is MonoView)
				((MonoView)viewWrapper).PerformNCCalc(viewWrapper.Frame.Size);

			SendMessage (viewWrapper.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			SendParentNotify (viewWrapper.Handle, Msg.WM_CREATE, int.MaxValue, int.MaxValue);

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				if (WindowHandle != IntPtr.Zero) {
					Form f = Control.FromHandle(viewWrapper.Handle) as Form;
					if (f != null) {
						if (f.WindowState == FormWindowState.Normal) {
							SendMessage(viewWrapper.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
						}
						if (f.ActivateOnShow)
							windowWrapper.MakeKeyAndOrderFront (viewWrapper);
						else
							windowWrapper.OrderFront (viewWrapper);
					}
				}

				if (!(Control.FromHandle(viewWrapper.Handle) is Form)) {
					SendMessage(viewWrapper.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
				}
			}
			else
			{
				if (!isTopLevel)
					viewWrapper.Hidden = true;
			}

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState(viewWrapper.Handle, FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState(viewWrapper.Handle, FormWindowState.Maximized);
			}

			keepAlivePool.Add(viewWrapper.Handle, viewWrapper);
			if (isTopLevel)
				keepAlivePool.Add(windowWrapper.Handle, windowWrapper);

			return viewWrapper.Handle;
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
			switch ((Msg) msg.Msg) {
				case Msg.WM_IME_COMPOSITION:
					SendMessage(msg.HWnd, Msg.WM_CHAR, msg.WParam, msg.LParam);
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
				case Msg.WM_NCCALCSIZE: {
					if (msg.WParam == (IntPtr)1) {
						MonoView monoView = ObjCRuntime.Runtime.GetNSObject(msg.HWnd) as MonoView;						
						if (monoView != null) {
							XplatUIWin32.NCCALCSIZE_PARAMS ncp;
							ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (msg.LParam, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

							var windowRect = new Rectangle(
								ncp.rgrc1.left,
								ncp.rgrc1.top,
								ncp.rgrc1.right - ncp.rgrc1.left,
								ncp.rgrc1.bottom - ncp.rgrc1.top);
							var clientRect = windowRect;
							if (monoView.Style.HasFlag(WindowStyles.WS_CHILD) || !monoView.Style.HasFlag(WindowStyles.WS_CAPTION)) {
								if (monoView.ExStyle.HasFlag(WindowExStyles.WS_EX_CLIENTEDGE) || monoView.ExStyle.HasFlag(WindowExStyles.WS_EX_STATICEDGE))
									clientRect.Inflate(-Border3DSize.Width, -Border3DSize.Height);
								else if (monoView.Style.HasFlag(WindowStyles.WS_BORDER))
									clientRect.Inflate(-BorderSize.Width, -BorderSize.Height);
							}
							ncp.rgrc1.left = clientRect.Left;
							ncp.rgrc1.top = clientRect.Top;
							ncp.rgrc1.right = clientRect.Right;
							ncp.rgrc1.bottom = clientRect.Bottom;

							Marshal.StructureToPtr (ncp, msg.LParam, true);
						}
					}
					break;
				}
				/*case Msg.WM_SETCURSOR: {
					// Pass to parent window first
					IntPtr hWndParent;
					if (IntPtr.Zero != msg.Result && (hWndParent = GetParent(msg.HWnd, false)) != IntPtr.Zero)
						msg.Result = NativeWindow.WndProc(hWndParent, (Msg)msg.Msg, msg.WParam, msg.LParam);

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
				}*/
				case Msg.WM_MOUSEWHEEL:
				{
					IntPtr hWndParent;
					if (IntPtr.Zero != msg.Result && (hWndParent = GetParent(msg.HWnd, false)) != IntPtr.Zero)
						msg.Result = NativeWindow.WndProc(hWndParent, (Msg)msg.Msg, msg.WParam, msg.LParam);
					return (IntPtr)msg.Result;
				}
				case Msg.WM_CANCELMODE:
				{
					if (Grab.Hwnd != IntPtr.Zero) {
						UngrabWindow(Grab.Hwnd);
					}
					break;
				}
				case Msg.WM_NCPAINT:
				{
					var monoView = ObjCRuntime.Runtime.GetNSObject(msg.HWnd) as MonoView;
					if (monoView != null)
						monoView.DrawBorders();	
					break;
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
			SendParentNotify (handle, Msg.WM_DESTROY, int.MaxValue, int.MaxValue);
				
			CleanupCachedWindows (handle);

			ArrayList windows = new ArrayList ();

			AccumulateDestroyedHandles (Control.ControlNativeWindow.ControlFromHandle (handle), windows);

			foreach (IntPtr h in windows) {
				SendMessage (h, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
			}

			foreach (IntPtr h in windows) {
				NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(h);
				NSWindow winWrap = vuWrap.Window;
				if (winWrap != null && winWrap.ContentView == vuWrap) { 
					winWrap.ReleasedWhenClosed = true;
					winWrap.Close ();
					NSApplication.SharedApplication.RemoveWindowsItem(winWrap);
					keepAlivePool.Remove(winWrap.Handle);
				} else {
					vuWrap.RemoveFromSuperviewWithoutNeedingDisplay ();
				}
				keepAlivePool.Remove(vuWrap.Handle);
			}
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DoEvents() {
            MSG msg = new MSG ();
			while (PeekMessage (null, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				Application.SendMessage(ref msg);
            }
		}

		internal override void EnableWindow (IntPtr handle, bool Enable) {
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			if (vuWrap is MonoView)
				((MonoView)vuWrap).Enabled = Enable;
			if (vuWrap is NSControl)
				((NSControl)vuWrap).Enabled = Enable;
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
			// NSApplication.SharedApplication.Terminate(this);
		}

		internal override IntPtr GetActive() {
			var keyWindow = NSApplication.SharedApplication.KeyWindow;
			return keyWindow == null ? IntPtr.Zero : keyWindow.ContentView.Handle;
		}

		internal override Region GetClipRegion (IntPtr handle) {
			MonoView vuWrap = ObjCRuntime.Runtime.GetNSObject(handle) as MonoView;
			if (vuWrap != null)
				return vuWrap.UserClip;
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
				CGRect bounds = screenWrap.Frame;
				size = new Size ((int) bounds.Size.Width, (int) bounds.Size.Height);
			} else {
				size = Size.Empty;
			}
		}

		internal override IntPtr GetParent(IntPtr handle, bool with_owner)
		{
			if (handle == IntPtr.Zero)
				return IntPtr.Zero;
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			MonoWindow monoWindow;
			if (vuWrap.Window != null && vuWrap == vuWrap.Window.ContentView)
				return with_owner && (monoWindow = vuWrap.Window as MonoWindow) != null && monoWindow.Owner != null ? monoWindow.Owner.ContentView.Handle : IntPtr.Zero;
			if (vuWrap.Superview != null)
				return vuWrap.Superview.Handle;
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
			CGPoint nspt = NSEvent.CurrentMouseLocation;
			Point pt = NativeToMonoScreen (nspt);
			x = (int) pt.X;
			y = (int) pt.Y;
		}

		internal override IntPtr GetFocus() {
			var keyWindow = NSApplication.SharedApplication.KeyWindow;
			if (keyWindow != null) {
				var responder = keyWindow.FirstResponder;
				if (responder is NSView)
					return responder.Handle;
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
			return Point.Empty;
		}

		internal override bool GetMessage (object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax)
		{
			bool wait = false;

			while (GetMessageResult) {
				while (PumpNativeEvent(wait, ref msg)) {
					return true;
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
			NSView view = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);

			if (view is MonoContentView) {
				var frame = NativeToMonoScreen(view.Window.Frame);
				x = frame.X;
				y = frame.Y;
				width = frame.Width;
				height = frame.Height;
			} else {
				var viewFrame = GetAlignmentRectForFrame(view, view.Frame);
				var frame = view.Superview != null ? NativeToMonoFramed(viewFrame, view.Superview) : viewFrame.ToRectangle();
				x = frame.X;
				y = frame.Y;
				if (view.Superview != null && view.Superview is IClientView)
				{
					x -= (int)((IClientView)view.Superview).ClientBounds.X;
					y -= (int)((IClientView)view.Superview).ClientBounds.Y;
				}
				width = frame.Width;
				height = frame.Height;
			}

			if (view is IClientView) {
				client_width = (int)((IClientView)view).ClientBounds.Width;
				client_height = (int)((IClientView)view).ClientBounds.Height;
			} else {
				client_width = width;
				client_height = height;
			}
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
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			if (!vuWrap.Hidden)
			{
				if (vuWrap is IClientView)
				{
					rc.X += (int)((IClientView)vuWrap).ClientBounds.X;
					rc.Y += (int)((IClientView)vuWrap).ClientBounds.Y;
				}
				vuWrap.SetNeedsDisplayInRect(MonoToNativeFramed(rc, vuWrap));
			}
		}

		internal override void InvalidateNC(IntPtr handle)
		{
            MonoView vuWrap = ObjCRuntime.Runtime.GetNSObject(handle) as MonoView;
            if (vuWrap != null && !vuWrap.Hidden && vuWrap.ClientBounds != vuWrap.Bounds)
				vuWrap.NeedsDisplay = true;
		}
		
		internal override bool IsEnabled(IntPtr handle) {
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			if (vuWrap is MonoView)
				return ((MonoView)vuWrap).Enabled;
			if (vuWrap is NSControl)
				return ((NSControl)vuWrap).Enabled;
			return true;
		}
		
		internal override bool IsVisible(IntPtr handle) {
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			return !vuWrap.Hidden;
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

		internal override bool UserClipWontExposeParent {
			get {
				return false;
			}
		}

		internal override PaintEventArgs PaintEventStart (ref Message msg, IntPtr handle, bool client)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			Graphics dc;
			PaintEventArgs paint_event;

			if (client) {
				dc = Graphics.FromHwnd (handle);
				if (null == dc)
					return null;

				Rectangle dirtyRectangle;
				var monoView = vuWrap as MonoView;
				if (monoView != null) {
					if (monoView.UserClip != null) {
						dc.Clip = monoView.UserClip;
					}
					var clientBounds = monoView.ClientBounds;
					dirtyRectangle = monoView.DirtyRectangle ?? NativeToMonoFramed(clientBounds, monoView);
					dirtyRectangle.Offset(-(int)clientBounds.X, -(int)clientBounds.Y);
				} else {
					dirtyRectangle = NativeToMonoFramed(vuWrap.Frame, vuWrap);
				}

				paint_event = new PaintEventArgs(dc, dirtyRectangle);
			} else {
				dc = Graphics.FromHwnd (handle, false);

				if (null == dc)
					return null;

				paint_event = new PaintEventArgs(dc, vuWrap.Bounds.ToRectangle());
			}

			return paint_event;
		}

		internal override void PaintEventEnd (ref Message msg, IntPtr handle, bool client, PaintEventArgs pevent)
		{
			if (pevent.Graphics != null)
				pevent.Graphics.Dispose();
			pevent.SetGraphics(null);
			pevent.Dispose();
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
			var view = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			if (view is MonoView)
				((MonoView)view).PerformNCCalc(view.Frame.Size);
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
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			if (!vuWrap.Hidden)
				vuWrap.NeedsDisplay = true;
		}
		
		
		internal override void ScrollWindow (IntPtr handle, int XAmount, int YAmount, bool clear) {
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			if (!vuWrap.Hidden)
				vuWrap.NeedsDisplay = true;
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

				if (CaretView.Superview is IClientView) {
					var clientBounds = ((IClientView)CaretView.Superview).ClientBounds;
					CaretView.Frame = Caret.rect.Move((int)clientBounds.X, (int)clientBounds.Y).ToCGRect();
				} else {
					CaretView.Frame = Caret.rect.ToCGRect();
				}

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
			MonoView vuWrap = ObjCRuntime.Runtime.GetNSObject (handle) as MonoView;
			if (vuWrap != null) {
				vuWrap.UserClip = region;
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
			var vuWrap = ObjCRuntime.Runtime.GetNSObject(window) as MonoView;
			if (vuWrap != null && vuWrap.Cursor != cursor) {
				vuWrap.Cursor = cursor;
				if (vuWrap == WindowsEventResponder.mouseView) {
					OverrideCursor(cursor);
				}
			}
		}

		internal override void SetCursorPos (IntPtr handle, int x, int y)
		{
			var screens = NSScreen.Screens;
			if (screens != null && 0 < screens.Length) {
				NSScreen screenWrap = (NSScreen)screens[0];
				NSDictionary description = screenWrap.DeviceDescription;
				NSNumber screenNumber = (NSNumber) description["NSScreenNumber"];
				// FIXME: Find a Cocoa way to do this.
				CGDisplayMoveCursorToPoint (screenNumber.UInt32Value, new CGPoint (x, y));
			}
		}

		private IntPtr GetSWFFirstResponder(NSWindow window)
		{
			if (window.FirstResponder is NSView) {
				var control = Control.FromChildHandle(window.FirstResponder.Handle);
				if (control != null)
					return control.window.Handle;
			}
			return IntPtr.Zero;
		}

		internal override void SetFocus(IntPtr handle)
		{
			var keyWindow = NSApplication.SharedApplication.KeyWindow;
			if (handle == IntPtr.Zero)
			{
				if (keyWindow != null)
					keyWindow.MakeFirstResponder(null);
			}
			else
			{
				var view = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
				if (view.Window == null)
					return;

				if (view != null && keyWindow != view.Window)
					view.Window.MakeKeyAndOrderFront(view.Window);

				// Sometimes the FirstResponder is some deeply nested native view (eg. inside WebView). When the
				// active window is changed to other one and then back, Form.WmActivate tries to restore focus to the
				// the ActiveControl. That will be the most nested Control that System.Windows.Forms know about, but
				// usually not the same one as FirstResponder. We tried to check if FirstResponder is one of those
				// nested views and skip the focus change if it is essentially trying to focus the control that is
				// already focused.
				if (GetSWFFirstResponder(view.Window) != view.Handle)
				{
					if (view is MonoView)
						((MonoView)view).inSetFocus = true;
					view.Window.MakeFirstResponder(view);
					if (view is MonoView)
						((MonoView)view).inSetFocus = false;
				}
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
			NSView newParentWrap = IntPtr.Zero != parent ? (NSView)ObjCRuntime.Runtime.GetNSObject(parent) : null;
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			NSWindow winWrap = (NSWindow) vuWrap.Window;

			if (winWrap != null && winWrap.ContentView == vuWrap) {
				// SetParent for windows is not supported
			} else if (vuWrap.Superview != newParentWrap) {
				bool adoption = vuWrap.Superview != null;
				if (adoption) {
					if (vuWrap.Superview is IClientView) {
						vuWrap.SetFrameOrigin(new CGPoint(
							vuWrap.Frame.X - (int)((IClientView)vuWrap.Superview).ClientBounds.X,
							vuWrap.Frame.Y - (int)((IClientView)vuWrap.Superview).ClientBounds.Y));
					}
					vuWrap.RemoveFromSuperview ();
				}

				if (newParentWrap != null) {
					if (newParentWrap is IClientView) {
						vuWrap.SetFrameOrigin(new CGPoint(
							vuWrap.Frame.X + (int)((IClientView)newParentWrap).ClientBounds.X,
							vuWrap.Frame.Y + (int)((IClientView)newParentWrap).ClientBounds.Y));
					}
					newParentWrap.AddSubview (vuWrap);
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
					FireTick(timer);
				else
					NSApplication.SharedApplication.InvokeOnMainThread(() => { FireTick(timer); });
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
					FireTick(timer);
				else 
					NSApplication.SharedApplication.InvokeOnMainThread(() => { FireTick(timer); });
			});
			nstimer.Retain();
			timer.window = nstimer.Handle;
		}
#endif

		internal void FireTick(Timer timer)
		{
			try
			{
				timer.FireTick();
			}
			catch (Exception e)
			{
				Debug.WriteLine("Unhadled exception in Timer.FireTick(): " + e.ToString()); 
			}
		}

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
				if (winWrap.ParentWindow != null)
					winWrap.ParentWindow.RemoveChildWindow(winWrap);
				winWrap.Owner = winOwnerWrap;
				// If not visible, do not call AddChildWindow now, because it would immediately show the child window.
				if (winOwnerWrap != null && winWrap.IsVisible)
					winOwnerWrap.AddChildWindow(winWrap, NSWindowOrderingMode.Above);				
			}

			return true;
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			if (!visible && Grab.Hwnd != IntPtr.Zero) {
				for (var hwndCheck = Grab.Hwnd; hwndCheck != IntPtr.Zero; hwndCheck = GetParent(hwndCheck, false)) {
					if (hwndCheck == handle) {
						UngrabWindow(Grab.Hwnd);
						break;
					}
				}
			}

			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			NSWindow winWrap = vuWrap.Window;
			MonoWindow monoWindow;
			if (winWrap != null && winWrap.ContentView == vuWrap) {
				if (visible) {
					if ((monoWindow = winWrap as MonoWindow) != null && monoWindow.Owner != null)
						monoWindow.Owner.AddChildWindow(winWrap, NSWindowOrderingMode.Above);
					if (Control.FromHandle(handle).ActivateOnShow)
						winWrap.MakeKeyAndOrderFront(winWrap);
					else
						winWrap.OrderFront(winWrap);
				} else {
					winWrap.OrderOut(winWrap);
					if ((monoWindow = winWrap as MonoWindow) != null && monoWindow.Owner != null)
						monoWindow.Owner.RemoveChildWindow(winWrap);
				}
			} else {
				// AppKit sets the FirstResponder to null when hiding a view that is
				// first responder or contains a subview that is first responder. We
				// want to override the behavior to set the parent as first responder.
				bool fixFocus = winWrap != null && winWrap.FirstResponder is NSView;
				vuWrap.Hidden = !visible;
				if (fixFocus && !(winWrap.FirstResponder is NSView))
					winWrap.MakeFirstResponder(vuWrap.Superview ?? winWrap.ContentView);
			}

			if (visible)
				SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

			return true;
		}

		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			if (value)
				Debug.WriteLine("SetAllowDrop not implemented");
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data, DragDropEffects allowed_effects)
		{
			Debug.WriteLine("StartDrag not implemented");
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
			RequestNCRecalc(handle);
		}
		
		internal override void SetWindowMinMax (IntPtr handle, Rectangle maximized, Size min, Size max) {
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
			vuWrap.Window.MinSize = min.ToCGSize();
			if (max.IsEmpty)
				vuWrap.Window.MaxSize = new CGSize(float.MaxValue, float.MaxValue);
			else
				vuWrap.Window.MaxSize = max.ToCGSize();
		}

		internal override void SetWindowPos (IntPtr handle, int x, int y, int width, int height)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);

			// Win32 automatically changes negative width/height to 0.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;

			int old_x, old_y, old_width, old_height, old_client_width, old_client_height;

			GetWindowPos(handle, false, out old_x, out old_y, out old_width, out old_height, out old_client_width, out old_client_height);

			bool nomove = old_x == x && old_y == y;
			bool nosize = old_width == width && old_height == height;

			// Save a server roundtrip (and prevent a feedback loop)
			if (nomove && nosize)
				return;

			NSWindow winWrap = vuWrap.Window;
			Rectangle mrect = new Rectangle(x, y, width, height);
			CGRect nsrect;

			if (winWrap != null && winWrap.ContentView == vuWrap) {
				nsrect = MonoToNativeScreen(mrect);
				if (winWrap.Frame != nsrect) {
					winWrap.SetFrame(nsrect, false);
					SetCaretPos(handle, Caret.X, Caret.Y);
				}
			}
			else
			{
				NSView superVuWrap = vuWrap.Superview;
				if (superVuWrap != null) {
					var clientView = superVuWrap as IClientView;
					if (clientView != null) {
						mrect.Y += (int)clientView.ClientBounds.Top;
						mrect.X += (int)clientView.ClientBounds.Left;
					}
					nsrect = MonoToNativeFramed(mrect, superVuWrap);
				} else
					nsrect = new CGRect(mrect.X, mrect.Y, mrect.Width, mrect.Height);
				nsrect = GetFrameForAlignmentRect(vuWrap, nsrect);
				if (vuWrap.Frame != nsrect)
					vuWrap.Frame = nsrect;
			}

#if DriverDebug
			if ( null != winWrap )
				Console.WriteLine ("{0} SetWindowPos( {1}, {2}, WxH: {3} x {4} )", winWrap.Title, x, y, width, height);
#endif

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
			SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, ptr);
			Marshal.FreeHGlobal(ptr);
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
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(handle);

			var monoView = vuWrap as MonoView;
			if (monoView != null) {
				monoView.Style = cp.WindowStyle;
				monoView.ExStyle = cp.WindowExStyle;
				monoView.PerformNCCalc(monoView.Frame.Size);
			}
				
			if (vuWrap.Window != null && vuWrap.Window.ContentView == vuWrap) {
				vuWrap.Window.StyleMask = NSStyleFromStyle(cp.WindowStyle) | (vuWrap.Window.StyleMask & NSWindowStyle.FullScreenWindow);
				vuWrap.Window.CollectionBehavior = !cp.WindowStyle.HasFlag(WindowStyles.WS_MAXIMIZEBOX) ? NSWindowCollectionBehavior.FullScreenAuxiliary : NSWindowCollectionBehavior.Default;
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
			// Transform to foreground process
			if (NSApplication.SharedApplication.ActivationPolicy != NSApplicationActivationPolicy.Regular)
			{
				NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
				NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
				NSProcessInfo.ProcessInfo.ProcessName = Application.ProductName;
			}

			Stack<NSAutoreleasePool> stack;
			if (!pools.TryGetValue(thread, out stack))
				pools.Add(thread, stack = new Stack<NSAutoreleasePool>());
			stack.Push(new NSAutoreleasePool());

#if XAMARINMAC
			this.NextEventMethod = typeof(NSApplication).GetMethod("NextEvent", Reflection.BindingFlags.Instance | Reflection.BindingFlags.NonPublic);
#endif

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

		// TranslateMessage produces WM_CHAR messages only for keys that are mapped to ASCII characters by the keyboard driver.
		internal virtual bool TranslateKeyMessage (ref MSG msg) {
			bool sent = true;
			switch (msg.message)
			{
				case Msg.WM_KEYDOWN:
				case Msg.WM_SYSKEYDOWN:
					// Posting WM_CHAR moved to MonoContentView
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
				if ((NSEventModifierMask.CommandKeyMask & key_modifiers) != 0)      { keys |= Keys.Cmd; }
				if ((NSEventModifierMask.AlternateKeyMask & key_modifiers) != 0)	{ keys |= Keys.Alt; }
				if ((NSEventModifierMask.ControlKeyMask & key_modifiers) != 0)      { keys |= Keys.Control; }
				return keys;
			}
		}
		internal override Size SmallIconSize { get{ return new Size(16, 16); } }
		internal override int MouseButtonCount { get{ return 3; } }
		internal override bool MouseButtonsSwapped { get{ return false; } }
		internal override bool MouseWheelPresent { get{ return true; } }

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
					CGRect bounds = screenWrap.VisibleFrame;
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

		internal override Size Border3DSize
		{
			get
			{
#if MACOS_THEME
				return new Size(1, 1);
#else
				return new Size(2, 2);
#endif
			}
		}

		// Event Handlers
		internal override event EventHandler Idle;
			#endregion Override properties XplatUIDriver

		[DllImport("/System/Library/Frameworks/CoreGraphics.framework/Versions/Current/CoreGraphics")]
		extern static void CGDisplayMoveCursorToPoint (UInt32 display, CGPoint point);

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

		public static bool HasExtendedCharFlag(this MSG msg)
		{
			return ((int)msg.lParam & (1 << 24)) != 0;
		}
	}
}

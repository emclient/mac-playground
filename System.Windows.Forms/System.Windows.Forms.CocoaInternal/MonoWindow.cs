﻿using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using Foundation;
using AppKit;
using ObjCRuntime;
using NSPoint = CoreGraphics.CGPoint;
using NSRect = CoreGraphics.CGRect;
using NSSize = CoreGraphics.CGSize;
using NSKey = MacApi.AppKit.NSKey;
using System.Windows.Forms.Mac;

namespace System.Windows.Forms.CocoaInternal
{
	class MonoWindow : NSWindow
	{
		XplatUICocoa driver;
		IntPtr prevFocus;
		bool dragging;
		bool disposed;
		bool key;

		public MonoWindow(IntPtr handle) : base(handle)
		{
		}

		//[Export ("initWithContentRect:styleMask:backing:defer:"), CompilerGenerated]
		internal MonoWindow(NSRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, XplatUICocoa driver)
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			this.driver = driver;
			this.ReleasedWhenClosed = true;

			// Disable tabbing on Sierra until we properly support it
			var setTabbingModeSelector = new ObjCRuntime.Selector("setTabbingMode:");
			if (this.RespondsToSelector(setTabbingModeSelector))
				this.SetValueForKey(NSNumber.FromInt32(2), new NSString("tabbingMode"));

			RegisterEventHandlers();
		}

		void RegisterEventHandlers()
		{
			WillClose += WindowWillClose;
			WindowShouldClose += ShouldClose;
			WillResize += WindowWillResize;
			WillStartLiveResize += WindowWillStartLiveResize;
			DidEndLiveResize += WindowDidEndLiveResize;
			DidMove += WindowDidMove;
		}

		void UnregisterEventHandlers()
		{
			WillClose -= WindowWillClose;
			WindowShouldClose -= ShouldClose;
			WillResize -= WindowWillResize;
			WillStartLiveResize -= WindowWillStartLiveResize;
			DidEndLiveResize -= WindowDidEndLiveResize;
			DidMove -= WindowDidMove;
		}

		internal virtual void WindowWillClose(object sender, EventArgs e)
		{
			UnregisterEventHandlers();
		}

		public override bool MakeFirstResponder(NSResponder aResponder)
		{
			var ok = base.MakeFirstResponder(aResponder);
			var next = FirstResponder;
			if (ok)
			{
				var c = XplatUICocoa.GetHandle(next);
				if (prevFocus != c && !(next is MonoWindow))
				{
					if (prevFocus != IntPtr.Zero)
						driver.SendMessage(prevFocus, Msg.WM_KILLFOCUS, c, IntPtr.Zero);

					// If sending WM_SETFOCUS causes another immediate change of focus, we need to have prevFocus updated
					var oldPrefFocus = prevFocus;
					prevFocus = c;

					if (c != IntPtr.Zero)
						driver.SendMessage(c, Msg.WM_SETFOCUS, oldPrefFocus, IntPtr.Zero);
				}
				else
				{
					prevFocus = c;
				}
			}
			return ok;
		}

		public override bool CanBecomeKeyWindow
		{
			get
			{
				if (ContentView is MonoView monoView)
				{
					if (IsKeyWindow)
						return true;
					if (lastEventType == NSEventType.LeftMouseDown && (mouseActivate == MouseActivate.MA_NOACTIVATE || mouseActivate == MouseActivate.MA_NOACTIVATEANDEAT))
						return false;
					if (0 != (monoView.ExStyle & WindowExStyles.WS_EX_NOACTIVATE))
						return false;
				}
				return true;
			}
		}

		public override bool CanBecomeMainWindow
		{
			get
			{
				return CanBecomeKeyWindow && ParentWindow == null;
			}
		}

		MouseActivate mouseActivate = MouseActivate.MA_ACTIVATE;
		NSEventType lastEventType = NSEventType.ApplicationDefined;
		NSEvent currentEvent;

		public override void SendEvent(NSEvent theEvent)
		{
			DebugUtility.WriteInfoIfChanged(theEvent);

			// See notes in IsKeyWindow below.
			if (disposed)
				return;

			lastEventType = theEvent.Type;
			currentEvent = theEvent;

			switch (theEvent.Type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
				case NSEventType.BeginGesture:
					if (PreprocessMouseDown(theEvent))
						return;
					break;
				case NSEventType.LeftMouseDragged:
					if (PreProcessMouseDragged(theEvent))
						return;
					break;
				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseUp:
					if (PreProcessMouseUp(theEvent))
						return;
					break;
				case NSEventType.ScrollWheel:
					if (PreprocessScrollWheel(theEvent))
						return;
					break;
				case NSEventType.KeyUp:
				case NSEventType.KeyDown:
					// Emulation of ToolStrip's modal filter
					if (Application.KeyboardCapture is ToolStripDropDown capture)
					{
						var resp = FirstResponder;
						var h = resp?.Handle ?? IntPtr.Zero;
						if (h != IntPtr.Zero)
							if (!ToolStripManager.IsChildOfActiveToolStrip(h))
								resp = capture.Handle.AsNSObject<NSResponder>();

						if (resp != null)
						{
							if (theEvent.Type == NSEventType.KeyDown)
								resp.KeyDown(theEvent);
							else
								resp.KeyUp(theEvent);
							return;
						}
					}

					// Deliver key messages also to SWF controls that are wrappers of native controls.
					// This gives them a the chance to handle special keys
					if (!FirstResponder.IsSwfControl() && !(FirstResponder is WindowsEventResponder))
					{
						var control = Control.FromChildHandle(FirstResponder.Handle);
						if (control != null && control.Handle.ToNSObject() is NSView obj)
						{
							theEvent.ToKeyMsg(out Msg msg, out IntPtr wParam, out IntPtr lParam);
							if (theEvent.KeyCode != (int)NSKey.Escape) // Preserve ESC for closing IME window 
							if (IntPtr.Zero != driver.SendMessage(control.Handle, msg, wParam, lParam))
								return;
						}
					}

					break;
			}

			base.SendEvent(theEvent);
			currentEvent = null;
		}

		// Returns true if processing should continue (base.SendEvent should be called), false if not
		protected virtual bool PreprocessMouseDown(NSEvent e)
		{
			bool hitTestCalled = false;
			NSView hitTestView = null;

			if (ToolStripManager.activeToolStrips.Count != 0)
			{
				hitTestCalled = true;
				hitTestView = (ContentView.Superview ?? ContentView).HitTest(e.LocationInWindow);
				if (hitTestView != null && !ToolStripManager.IsChildOfActiveToolStrip(hitTestView.Handle))
					ToolStripManager.FireAppClicked();
			}

			if (!hitTestCalled)
				hitTestView = (ContentView.Superview ?? ContentView).HitTest(e.LocationInWindow);
			var hitTestHandle = hitTestView?.Handle ?? IntPtr.Zero;
			var hitTestControl = Control.FromChildHandle(hitTestHandle);
			var hitTestControlHandle = hitTestControl?.Handle ?? IntPtr.Zero;

			if (e.Type == NSEventType.LeftMouseDown)
			{
				var topLevelParent = IntPtr.Zero; // FIXME
				mouseActivate = (MouseActivate)driver.SendMessage(ContentView.Handle, Msg.WM_MOUSEACTIVATE, topLevelParent, hitTestControlHandle).ToInt32();
				if (mouseActivate == MouseActivate.MA_NOACTIVATEANDEAT)// || mouseActivate == MouseActivate.MA_ACTIVATEANDEAT)
					return true;
			}

			if (hitTestControlHandle == IntPtr.Zero)
			{
				OnNcMouseDown(e);
			}
			else
			{
				if (!hitTestView.IsSwfControl())
				{
					var p = driver.NativeToMonoScreen(this.ConvertPointToScreenSafe(e.LocationInWindow));
					driver.SendParentNotify(hitTestControlHandle, Msg.WM_LBUTTONDOWN, p.X, p.Y);
				}
			}

			return false;
		}

		protected virtual bool PreProcessMouseDragged(NSEvent e)
		{
			if (!dragging)
			{
				dragging = true;
				OnBeginDragging(e);
			}

			OnDragging(e);

			return false;
		}

		protected virtual bool PreProcessMouseUp(NSEvent e)
		{
			if (dragging)
				OnEndDragging(e);

			var view = (ContentView.Superview ?? ContentView).HitTest(e.LocationInWindow);
			if (view != null && !view.IsSwfControl() && Control.FromChildHandle(view.Handle) == null)
				OnNcMouseUp(e);

			return false;
		}

		protected virtual bool PreprocessScrollWheel(NSEvent e)
		{
			var view = (ContentView.Superview ?? ContentView).HitTest(e.LocationInWindow);
			if (view != null && !view.IsSwfControl() && Control.FromChildHandle(view.Handle) is Control control)
			{
				// This is to allow SWF wrappers of native views to handle messages before the native view swallows them (WebView, for example).

				var p = driver.NativeToMonoScreen(this.ConvertPointToScreenSafe(e.LocationInWindow));
				if (Math.Abs(e.ScrollingDeltaY - nfloat.Epsilon) > 0)
				{
					var delta = e.ScaledAndQuantizedDeltaY();
					var wParam = (IntPtr)(((int)e.ModifiersToWParam() & 0xFFFF) | (delta << 16));
					var lParam = (IntPtr)((p.X & 0xFFFF) | (p.Y << 16));
					var msg = new MSG { hwnd = control.Handle, message = Msg.WM_MOUSEWHEEL, wParam = wParam, lParam = lParam };
					if (Application.SendMessage(ref msg, out var drop, out var _).ToInt32() == 0 || drop)
						return true;
				}

				if (Math.Abs(e.ScrollingDeltaX - nfloat.Epsilon) > 0)
				{
					int delta = e.ScaledAndQuantizedDeltaX();
					var wParam = (IntPtr)(((int)e.ModifiersToWParam() & 0xFFFF) | (delta << 16));
					var lParam = (IntPtr)((p.X & 0xFFFF) | (p.Y << 16));
					var msg = new MSG { hwnd = control.Handle, message = Msg.WM_MOUSEHWHEEL, wParam = wParam, lParam = lParam };
					if (Application.SendMessage(ref msg, out var drop, out var _).ToInt32() == 0 || drop)
						return true;
				}
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;

			disposed = true;

			base.Dispose(disposing);
		}

		~MonoWindow()
		{
			Dispose(false);
		}
/*
 * 
 *	Commented out until we figure out why it causes crashes on some systems (seen on Mojave).
 *
 * 
		public override bool IsKeyWindow
		{
			get
			{
				// NOTE:
				// The 'disposed' protection is necessary, because the native NSWindow lasts longer than the Form.
				// The NSWindow is NOT destroyed in DestroyHandle(), it's just closed, and the final "release"
				// message is sent *afterwards* (even if releasedWhenClosed is true)!
				// In the mean time (before or during final "release"), the "isKeyWindow" message
				// is sent by the Cocoa framework, to eventually make another view the "key".
				// Since the Mono bridge between native NSWindow and managed Form objects still exists at the moment,
				// this "IsKeyWindow" c# getter gets invoked, regardless the fact that it is already disposed.

				if (disposed)
					return key;

				// This allows WebView to change cursor when hovering over DOM nodes even if it's window is not key (pop-ups etc).
				return base.IsKeyWindow || IsDeliveringMouseMovedToWebHTMLView();
			}
		}
*/
		protected bool IsDeliveringMouseMovedToWebHTMLView()
		{
			if (currentEvent?.Type == NSEventType.MouseMoved)
			{
				var view = (ContentView.Superview ?? ContentView).HitTest(currentEvent.LocationInWindow);
				return view?.Class.Handle == WebHTMLViewClassHandle;
			}
			return false;
		}

		static IntPtr WebHTMLViewClassHandle = MacApi.LibObjc.objc_getClass("WebHTMLView");

		public override void OrderWindow(NSWindowOrderingMode place, nint relativeTo)
		{
			var wasVisible = IsVisible;
			base.OrderWindow(place, relativeTo);

			if (place == NSWindowOrderingMode.Out)
			{
				// Remove the window from Windows menu
				NSApplication.SharedApplication.RemoveWindowsItem(this);
			}

			if (wasVisible != IsVisible)
				driver.SendMessage(ContentView.Handle, Msg.WM_SHOWWINDOW, (IntPtr)(wasVisible ? 0 : 1), IntPtr.Zero);
		}

		internal virtual bool ShouldClose(NSObject sender)
		{
			if (Control.FromHandle(ContentView.Handle) is Form form)
				form.Close(); // Sets CloseReason, among other things
			else
				driver.SendMessage(ContentView.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			return false;
		}

		internal virtual unsafe NSSize WindowWillResize(NSWindow sender, NSSize size)
		{
			var rect = new XplatUIWin32.RECT[] { new XplatUIWin32.RECT(0, 0, (int)size.Width, (int)size.Height) };

			//FIXME - deduce WMSZ
			var wParam = new IntPtr(8); //WMSZ_BOTTOMRIGHT;

			fixed (void* ptr = &rect[0])
				Application.SendMessage(ContentView.Handle, Msg.WM_SIZING, wParam, new IntPtr(ptr));

			return new NSSize(rect[0].Width, rect[0].Height);
		}

		internal virtual void WindowWillStartLiveResize(object sender, EventArgs e)
		{
			NativeWindow.WndProc(ContentView.Handle, Msg.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
		}

		internal virtual void WindowDidEndLiveResize(object sender, EventArgs e)
		{
			NativeWindow.WndProc(ContentView.Handle, Msg.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
		}

		internal virtual void WindowDidMove(object sender, EventArgs e)
		{
			driver.SendMessage(ContentView?.Handle ?? IntPtr.Zero, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
		}

		protected virtual void OnNcMouseDown(NSEvent e)
		{
			var p = driver.NativeToMonoScreen(Frame.Location);
			var lParam = (IntPtr)((p.Y << 16) | (int)(short)p.X);
			var wParam = (IntPtr)(e.ModifierFlags.ToWParam() | e.ButtonNumberToWParam());
			Application.SendMessage(ContentView.Handle, Msg.WM_NCLBUTTONDOWN.AdjustForButton(e), wParam, lParam);
		}

		protected virtual void OnNcMouseUp(NSEvent e)
		{
			var p = driver.NativeToMonoScreen(Frame.Location);
			var lParam = (IntPtr)((p.Y << 16) | (int)(short)p.X);
			var wParam = (IntPtr)(e.ModifierFlags.ToWParam() | e.ButtonNumberToWParam());
			Application.SendMessage(ContentView.Handle, Msg.WM_NCLBUTTONUP.AdjustForButton(e), wParam, lParam);
		}

		protected virtual void OnBeginDragging(NSEvent e)
		{
		}

		protected virtual unsafe void OnDragging(NSEvent e)
		{
			var p = driver.NativeToMonoScreen(Frame.Location);
			var rect = new XplatUIWin32.RECT[] { new XplatUIWin32.RECT(p.X, p.Y, p.X + (int)Frame.Width, p.Y + (int)Frame.Height) };
			fixed (void* r = &rect[0])
				driver.SendMessage(ContentView?.Handle ?? IntPtr.Zero, Msg.WM_MOVING, IntPtr.Zero, new IntPtr(r));
		}

		protected virtual void OnEndDragging(NSEvent e)
		{
			dragging = false;
			var p = driver.NativeToMonoScreen(this.Frame.Location);
			var lParam = (p.Y << 16) | (short)p.X;
			driver.SendMessage(ContentView?.Handle ?? IntPtr.Zero, Msg.WM_MOVE, IntPtr.Zero, (IntPtr)lParam);
		}

		public override void BecomeKeyWindow()
		{
			base.BecomeKeyWindow();

			if (CanBecomeMainWindow)
				MakeMainWindow();

			// FIXME: Set LParam
			//driver.SendMessage(ContentView.Handle, Msg.WM_NCACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, (IntPtr)(-1));
			driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);

			if ((key = IsKeyWindow) && FirstResponder != null) // For the case that previous WM_ACTIVATE in fact did not activate this window.
			{
				if (FirstResponder == this)
					MakeFirstResponder(ContentView); // InitialResponder?
				else
					driver.SendMessage(FirstResponder.Handle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
			}

			/*foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && !utility_window.IsVisible)
					utility_window.OrderFront(utility_window);
			}*/
		}

		public override void ResignKeyWindow()
		{
			var newKeyWindow = NSApplication.SharedApplication.KeyWindow;

			base.ResignKeyWindow();

			//driver.SendMessage(ContentView.Handle, Msg.WM_NCACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, (IntPtr)(-1));
			driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, newKeyWindow != null ? newKeyWindow.ContentView.Handle : IntPtr.Zero);

			if (!(key = IsKeyWindow) && FirstResponder != null) // For the case that previous WM_ACTIVATE in fact did not deactivate this window.
				driver.SendMessage(FirstResponder.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);

			/*foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && utility_window.IsVisible)
					utility_window.OrderOut(utility_window);
			}*/
		}

		internal virtual void ActivateNextWindow()
		{
			var windows = NSApplication.SharedApplication.OrderedWindows();
			foreach (var window in windows)
			{
				if (window is MonoWindow && window != this && window.IsVisible && !window.IsMiniaturized && !window.IsSheet && window.CanBecomeKeyWindow)
				{
					window.MakeKeyWindow();
					break;
				}
			}
		}

		static internal List<NSWindow> GetOrderedWindowList()
		{
#if MAC
			var list = new List<NSWindow>();
			NSApplication.SharedApplication.EnumerateWindows(NSWindowListOptions.OrderedFrontToBack, (NSWindow window, ref bool stop) =>
			{
				list.Add(window);
			});
			return list;
#else
			var numbers = WindowNumbersWithOptions(NSWindowNumberListOptions.AllApplication | NSWindowNumberListOptions.AllSpaces);
			var windows = NSApplication.SharedApplication.Windows;
			var winByNum = new Dictionary<long, NSWindow>(windows.Length);

			foreach (var window in windows)
				winByNum[(long)window.WindowNumber] = window;

			var sorted = new List<NSWindow>(windows.Length);

			for (int i = 0; i < numbers.Count; ++i)
			{
				var handle = numbers.ValueAt((uint)i);
				var number = new NSNumber(handle);

				NSWindow window;
				if (number != null && winByNum.TryGetValue(number.Int64Value, out window))
					sorted.Add(window);
			}
			return sorted;
#endif
		}

		public NSWindow Owner { get; set; }
	}
}


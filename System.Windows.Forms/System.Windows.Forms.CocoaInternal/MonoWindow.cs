using System.Drawing.Mac;
using System.Collections.Generic;
using System.Windows.Forms.Mac;
using Foundation;
using AppKit;
using ObjCRuntime;
using NSRect = CoreGraphics.CGRect;
using NSSize = CoreGraphics.CGSize;

namespace System.Windows.Forms.CocoaInternal
{
	class MonoWindow : NSWindow
	{
		XplatUICocoa driver;
		bool dragging;
		bool disposed;
		bool key = false;
		bool isPanel;
		int dpi;
		bool? movable;

		public MonoWindow(NativeHandle handle) : base(handle)
		{
		}

		//[Export ("initWithContentRect:styleMask:backing:defer:"), CompilerGenerated]
		internal MonoWindow(NSRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, XplatUICocoa driver, bool isPanel = false)
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			this.driver = driver;
			this.ReleasedWhenClosed = false;
			this.IsPanel = isPanel;
			this.dpi = (int)this.DeviceDPI().Width;

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
			DidMiniaturize += WindowDidMiniaturize;
			DidDeminiaturize += WindowDidDeminiaturize;
			DidChangeBackingProperties += WindowDidChangeBackingProperties;
			WillEnterFullScreen += WindowWillEnterFullScreen;
			DidFailToEnterFullScreen += WindowDidFailToEnterFullScreen;
			DidExitFullScreen += WindowDidExitFullScreen;
		}

		void UnregisterEventHandlers()
		{
			WillClose -= WindowWillClose;
			WindowShouldClose -= ShouldClose;
			WillResize -= WindowWillResize;
			WillStartLiveResize -= WindowWillStartLiveResize;
			DidEndLiveResize -= WindowDidEndLiveResize;
			DidMove -= WindowDidMove;
			DidMiniaturize -= WindowDidMiniaturize;
			DidDeminiaturize -= WindowDidDeminiaturize;
			DidChangeBackingProperties -= WindowDidChangeBackingProperties;
			WillEnterFullScreen -= WindowWillEnterFullScreen;
			DidFailToEnterFullScreen -= WindowDidFailToEnterFullScreen;
			DidExitFullScreen -= WindowDidExitFullScreen;
		}

		internal virtual void WindowWillClose(object sender, EventArgs e)
		{
			UnregisterEventHandlers();
		}

		public override bool MakeFirstResponder(NSResponder first)
		{
			if (IsNoActivate || IsMouseActivateNoActivate)
				return false;

			// Prevent SWF controls based on native controls to gain focus if not appropriate.
			// The AcceptsFirstResponder property would have to be overriden, which is not always possible.
			if (first != null && first is not MonoView)
				if (Control.FromChildHandle(first.Handle) is Control c)
					if (!c.CanFocus)
						return false;

			var ok = base.MakeFirstResponder(first);
			if (ok)
			{
				var hwnd = XplatUICocoa.GetHandle(first);
				if (driver.FocusHwnd != hwnd)
				{
					if (driver.FocusHwnd != IntPtr.Zero)
						driver.SendMessage(driver.FocusHwnd, Msg.WM_KILLFOCUS, hwnd, IntPtr.Zero);

					if (first is not MonoWindow) {
						// If sending WM_SETFOCUS causes another immediate change of focus, we need to have prev focus updated
						var prev = driver.FocusHwnd;
						driver.FocusHwnd = hwnd;

						if (hwnd != IntPtr.Zero)
							driver.SendMessage(hwnd, Msg.WM_SETFOCUS, prev, IntPtr.Zero);
					}
					else 
					{
						driver.FocusHwnd = IntPtr.Zero; // We only killed the focus
					}
				}
				else
				{
					driver.FocusHwnd = hwnd;
				}
			}
			return ok;
		}

		public override bool CanBecomeKeyWindow
		{
			get
			{
				if (IsNoActivate || IsMouseActivateNoActivate)
					return false;
				return true;
			}
		}

		public override bool CanBecomeMainWindow
		{
			get
			{
				return !IsPanel && CanBecomeKeyWindow && ParentWindow == null;
			}
		}

		MouseActivate mouseActivate = MouseActivate.MA_ACTIVATE;
		NSEventType currentEventType = 0;

		public override void SendEvent(NSEvent theEvent)
		{
			DebugUtility.WriteInfoIfChanged(theEvent);

			// See notes in IsKeyWindow below.
			if (disposed)
				return;

			if (!IsVisible && theEvent.IsMouse(out var _))
				return;

			currentEventType = theEvent.Type;

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
					// Deliver key messages also to SWF controls that are wrappers of native controls.
					// This gives them a the chance to handle special keys
					if (!FirstResponder.IsSwfControl() && !(FirstResponder is WindowsEventResponder))
					{
						var control = Control.FromChildHandle(FirstResponder.Handle);
						if (control != null && control.Handle.ToNSObject() is NSView obj)
						{
							if (theEvent.ToKeyMsg(out Msg msg, out IntPtr wParam, out IntPtr lParam))
							{
								if (theEvent.KeyCode == (int)NSKey.Escape)
									break; // Preserve ESC for closing IME window 
								if (IntPtr.Zero == driver.SendMessage(control.Handle, msg, wParam, lParam))
									return; // Processed by a SWF parent
							}
						}
					}

					break;
			}

			base.SendEvent(theEvent);
			currentEventType = 0;
		}

		// Returns true if processing should continue (base.SendEvent should be called), false if not
		internal virtual bool PreprocessMouseDown(NSEvent e)
		{
			var hitTestView = (ContentView.Superview ?? ContentView).HitTest(e.LocationInWindow);
			var hitTestHandle = hitTestView?.Handle ?? IntPtr.Zero;
			var hitTestControl = Control.FromChildHandle(hitTestHandle);
			var hitTestControlHandle = hitTestControl?.Handle ?? IntPtr.Zero;

			if (e.Type == NSEventType.LeftMouseDown && hitTestControlHandle != IntPtr.Zero && e.ClickCount == 1)
			{
				var topLevelParent = hitTestControl?.TopLevelControl?.Handle ?? IntPtr.Zero;
				var lParam = new IntPtr(((int)(Msg.WM_LBUTTONDOWN) << 16) | (int)HitTest.HTCLIENT);
				mouseActivate = (MouseActivate)driver.SendMessage(hitTestControlHandle, Msg.WM_MOUSEACTIVATE, topLevelParent, lParam);
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
					NSApplication.SharedApplication.BeginInvokeOnMainThread( () => 
					{
						for (var v = hitTestView; v != null; v = v.Superview)
						{
							if (v.Superview != null && v.Superview.IsSwfControl())
							{
								var p = driver.NativeToMonoScreen(this.ConvertPointToScreenSafe(e.LocationInWindow));
								driver.SendParentNotify(v.Handle, Msg.WM_LBUTTONDOWN, p.X, p.Y);
								break;
							}
						}
					});
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
			Control capture = Application.KeyboardCapture;
			Control target = Control.FromChildHandle(view?.Handle ?? NativeHandle.Zero);
			var control = capture ?? target;
			if (view != null && !view.IsSwfControl() && control != null)
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

			// Don't deliver scrollwheel messages to controls outside of the current toolstrip (if any)
			if (capture != null && !capture.Contains(target))
				return true;

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
*/ 
		public override bool IsKeyWindow
		{
			get
			{
				// This allows panels (inactive popups) to update mouse cursor when hovering over controls
				if (IsPanel)
					return true;

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
				return base.IsKeyWindow;
			}
		}

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

		internal virtual void WindowDidMiniaturize(object sender, EventArgs e)
		{
			var size = this.Frame.Size.ToSDSize();
			var lParam = (IntPtr)((size.Height << 16) | (int)(short)size.Width);
			driver.SendMessage(ContentView?.Handle ?? IntPtr.Zero, Msg.WM_SIZE, (IntPtr)SIZE.SIZE_MINIMIZED, IntPtr.Zero);
		}

		internal virtual void WindowDidDeminiaturize(object sender, EventArgs e)
		{
			var size = this.Frame.Size.ToSDSize();
			var lParam = (IntPtr)((size.Height << 16) | (int)(short)size.Width);
			driver.SendMessage(ContentView?.Handle ?? IntPtr.Zero, Msg.WM_SIZE, (IntPtr)SIZE.SIZE_RESTORED, lParam);
		}

		internal virtual void WindowWillEnterFullScreen(object sender, EventArgs e)
		{
			// Let's temporarily change the IsMovable property
			// -----------------------------------------------
			// This is a workaround for an Apple's bug, where the window will be misplaced
			// if it is not movable, if it is in fullscreen mode and if it gets moved to another
			// display (when the containing display gets disconnected).
			movable = IsMovable;
			IsMovable = true;
		}

		internal virtual void WindowDidFailToEnterFullScreen(object sender, EventArgs e)
		{
			RestoreIsMovable();
		}

		internal virtual void WindowDidExitFullScreen(object sender, EventArgs e)
		{
			RestoreIsMovable();
		}

		void RestoreIsMovable()
		{
			if (movable.HasValue) {
				IsMovable = movable.Value;
				movable = null;
			}
		}

		public override void SetFrame(NSRect rect, bool display)
		{
			base.SetFrame(rect, display);
		}

		public override void SetFrame(NSRect rect, bool display, bool animate)
		{
			base.SetFrame(rect, display, animate);
		}

		unsafe internal virtual void WindowDidChangeBackingProperties(object sender, EventArgs e)
		{
			var dpi = (int)this.DeviceDPI().Width;
			if (this.dpi != dpi)
			{
				this.dpi = dpi;

				ContentView.TraverseSubviews((view) => Application.SendMessage(view.Handle, Msg.WM_DPICHANGED_BEFOREPARENT, IntPtr.Zero, IntPtr.Zero));
				
				var wParam = (IntPtr)((dpi & 0xFFFF) | (dpi << 16));
				var rect = new XplatUIWin32.RECT[] { XplatUIWin32.RECT.FromRectangle(Frame.ToRectangle()) };
				var size = rect[0].Size;

				var result = Application.SendMessage(ContentView.Handle, Msg.WM_GETDPISCALEDSIZE, wParam, Control.MakeParam(size.Width, size.Height));
				if (result != 0) 
				{
					rect[0].right = rect[0].left + Control.LowOrder(result);
					rect[0].bottom = rect[0].top + Control.HighOrder(result);
				}

				fixed (void* ptr = &rect[0])
					Application.SendMessage(ContentView.Handle, Msg.WM_DPICHANGED, wParam, new IntPtr(ptr));

				ContentView.TraverseSubviews((view) => {
					Application.SendMessage(view.Handle, Msg.WM_DPICHANGED_AFTERPARENT, IntPtr.Zero, IntPtr.Zero);
					view.NeedsDisplay = true;
				});
			}
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

		bool IsNoActivate => ContentView is MonoView view && view.ExStyle.HasFlag(WindowExStyles.WS_EX_NOACTIVATE);

		bool IsMouseActivateNoActivate
			=> currentEventType == NSEventType.LeftMouseDown
			&& (mouseActivate == MouseActivate.MA_NOACTIVATE || mouseActivate == MouseActivate.MA_NOACTIVATEANDEAT)
			&& driver.setFocusCount == 0; // Allow explicit SetFocus() calls during left mouse down

		public override void BecomeKeyWindow()
		{
			base.BecomeKeyWindow();

			if (CanBecomeMainWindow)
				MakeMainWindow();

			if (!IsNoActivate && !IsMouseActivateNoActivate)
			{
				// FIXME: Set LParam
				//driver.SendMessage(ContentView.Handle, Msg.WM_NCACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, (IntPtr)(-1));
				driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
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

			if (FirstResponder is MonoView || FirstResponder is WindowsEventResponder)
				MakeFirstResponder(null); // Sends WM_KILLFOCUS

			base.ResignKeyWindow();

			//driver.SendMessage(ContentView.Handle, Msg.WM_NCACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, (IntPtr)(-1));
			driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, newKeyWindow != null ? newKeyWindow.ContentView.Handle : IntPtr.Zero);

			/*foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && utility_window.IsVisible)
					utility_window.OrderOut(utility_window);
			}*/
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

		public virtual bool IsPanel
		{
			get { return isPanel; }
			set { isPanel = value; }
		}
	}

	public static class MonoWindowExtensions {
		public static void ActivateNextWindow(this NSWindow me, bool onlyForm = false)
		{
			var windows = NSApplication.SharedApplication.OrderedWindows();
			foreach (var window in windows)
			{
				if (window != me && window.IsVisible && !window.IsMiniaturized && !window.IsSheet && window.CanBecomeKeyWindow)
				{
					if (onlyForm && !(window is MonoWindow))
						continue;

					window.MakeKeyWindow();
					break;
				}
			}
		}
	}
}


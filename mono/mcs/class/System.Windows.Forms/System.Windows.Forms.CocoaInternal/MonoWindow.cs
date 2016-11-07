using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

#if XAMARINMAC
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#endif

#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
using NSSize = System.Drawing.SizeF;
using nint = System.Int32;
#else
#if XAMARINMAC
using NSRect = CoreGraphics.CGRect;
using NSPoint = CoreGraphics.CGPoint;
using NSSize = CoreGraphics.CGSize;
using nint = System.Int64;
#elif MONOMAC
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
using NSSize = MonoMac.CoreGraphics.CGSize;
using nint = System.Int32;
#endif
#endif

namespace System.Windows.Forms.CocoaInternal
{
	internal class MonoWindow : NSWindow
	{
		internal XplatUICocoa driver;
		internal NSWindow owner;

		const int TabKey = 9;
		const int ShiftTabKey = 25;

		public MonoWindow (IntPtr handle) : base(handle)
		{
		}
			
		//[Export ("initWithContentRect:styleMask:backing:defer:"), CompilerGenerated]
		internal MonoWindow (NSRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, XplatUICocoa driver) 
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			this.driver = driver;
		}

		public override bool CanBecomeMainWindow
		{
			get { return CanBecomeKeyWindow; }
		}

		public override void SendEvent(NSEvent theEvent)
		{
			var monoContentView = (MonoContentView)ContentView;
			var hitTestView = monoContentView.hitTestResult;

			//Console.WriteLine(hitTestView?.GetType()?.Name);

			switch (theEvent.Type)
			{
				// Support for closing menus, dropdowns etc when clicking 'outside'.
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
				case NSEventType.BeginGesture:
					if (hitTestView == null)
						OnNonClientClick(theEvent);
					else if (!(hitTestView is MonoView))
						OnNativeControlClick(theEvent);
					break;

				// Prevent native controls (webview etc) from eating certain mouse messages for MWF controls
				case NSEventType.MouseMoved:
				case NSEventType.MouseEntered:
				case NSEventType.MouseExited:
					if (FirstResponder is NSControl && hitTestView is MonoView)
					{
						monoContentView.ProcessMouseEvent(theEvent);
						return;
					}
					break;
				
				// Handles tab and shift+tab keys so that switching focus works for both MWF and native controls.
				// Handles ESC key as well (forwards it to the parent control, so that closing window works)
				// - Both tasks should handled better, this is a workaround rather than solution.
				// - This way you can't type TAB char inside web view - it always focuses next control
				// - NSControl should be asked if it's ready to resign the 1st responder
				case NSEventType.KeyDown:
					if (FirstResponder is NSControl && theEvent.Characters.Length > 0)
					{
						int c = (int)theEvent.Characters[0];
						if (c == ShiftTabKey || c == TabKey)
						{
							monoContentView.FocusHandle = FindNextControl(FirstResponder as NSView, c == TabKey);
							MakeFirstResponder(monoContentView);
							return;
						}

						if (theEvent.KeyCode == (ushort)NSKey.Escape)
						{
							var control = FindContainingControl(FirstResponder as NSView);
							if (control != null)
							{
								var focus = monoContentView.FocusHandle;
								monoContentView.FocusHandle = control.Handle;
								monoContentView.ProcessKeyPress(theEvent);
								monoContentView.FocusHandle = focus;
								return;
							}
						}
					}
					break;

				case NSEventType.FlagsChanged:
					if (FirstResponder is NSControl)
						monoContentView.ProcessModifiers(theEvent);
					break;
			}

			base.SendEvent(theEvent);
		}

		protected virtual void OnNonClientClick(NSEvent e)
		{
			// FIXME - Send WM_NCLBUTTONDOWN etc instead of the following line?
			ToolStripManager.FireAppClicked();
		}

		protected virtual void OnNativeControlClick(NSEvent e)
		{
			// FIXME
			ToolStripManager.FireAppClicked();
		}

		[Export("windowShouldClose:")]
		internal virtual bool shouldClose (NSObject sender)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			var form = Control.FromHandle(hwnd.Handle) as Form;
			if (form != null)
				form.Close(); // Sets CloseReason, among other things
			else
				driver.SendMessage (hwnd.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			return false;
		}

		public override bool CanBecomeKeyWindow
		{
			get
			{
				var hwnd = Hwnd.GetObjectFromWindow(this.ContentView.Handle);
				if (hwnd.zombie)
					return false;
				return base.CanBecomeKeyWindow;
			}
		}

		public override void OrderWindow(NSWindowOrderingMode place, nint relativeTo)
		{
			base.OrderWindow(place, relativeTo);

			if (place == NSWindowOrderingMode.Out && IsKeyWindow)
				NSApplication.SharedApplication.BeginInvokeOnMainThread(() => { ActivateNextWindow(); });
		}

		[Export ("windowWillResize:toSize:")]
		internal virtual NSSize willResize(NSWindow sender, NSSize toFrameSize)
		{
			ToolStripManager.FireAppClicked();

			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);

			var rect = new XplatUIWin32.RECT ();
			rect.left = rect.top = 0;
			rect.right = (int)toFrameSize.Width;
			rect.bottom = (int)toFrameSize.Height;
			IntPtr lpRect = Marshal.AllocHGlobal (Marshal.SizeOf (rect));
			Marshal.StructureToPtr(rect, lpRect, false);

			//FIXME - deduce WMSZ
			IntPtr wParam = new IntPtr(8); //WMSZ_BOTTOMRIGHT;

			NativeWindow.WndProc (hwnd.Handle, Msg.WM_SIZING, wParam, lpRect);
			var rect2 = (Rectangle)Marshal.PtrToStructure (lpRect, typeof(Rectangle));
			toFrameSize.Width = rect2.Width;
			toFrameSize.Height = rect2.Height;	

			Marshal.FreeHGlobal (lpRect);

			return toFrameSize;
		}

		[Export ("windowDidResize:")]
		internal virtual void windowDidResize (NSNotification notification)
		{
			// resizeWinForm, invalidate and update?
			resizeWinForm(Hwnd.GetObjectFromWindow(this.ContentView.Handle));
		}

		[Export ("windowWillStartLiveResize:")]
		internal virtual void windowWillStartLiveResize(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			NativeWindow.WndProc (hwnd.Handle, Msg.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
		}

		[Export ("windowDidEndLiveResize:")]
		internal virtual void windowDidEndLiveResize(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);

			resizeWinForm (hwnd);

			NativeWindow.WndProc (hwnd.Handle, Msg.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
		}

		[Export ("windowDidMove:")]
		internal virtual void windowDidMove(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			resizeWinForm (hwnd);
		}

		[Export ("windowDidChangeScreen:")]
		internal virtual void windowDidChangeScreen(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			resizeWinForm (hwnd);
		}

		[Export("windowDidBecomeKey:")]
		internal virtual void windowDidBecomeKey(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow(this.ContentView.Handle);
			XplatUICocoa.ActiveWindow = hwnd.Handle;

			// Activating the window when the FirstReceiver was a native control would result in selecting next control
			if (FirstResponder is MonoContentView || FirstResponder is MonoWindow || FirstResponder == null)
			{
				driver.SendMessage(hwnd.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
			}
			else
			{
				// Setting IsActive causes invoking OnActivate, which is necessary for refreshing menus.
				var form = Control.FromHandle(ContentView.Handle)?.FindForm();
				if (form != null)
					form.IsActive = true;
			}

			var cv = (MonoContentView)ContentView;
			if (cv.FocusHandle != IntPtr.Zero)
				driver.SendMessage(cv.FocusHandle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && !utility_window.IsVisible)
					utility_window.OrderFront(utility_window);
			}
		}

		[Export ("windowDidResignKey:")]
		internal virtual void windowDidResignKey(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			driver.SendMessage (hwnd.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
			if (XplatUICocoa.ActiveWindow == hwnd.Handle)
				XplatUICocoa.ActiveWindow = IntPtr.Zero;

			var cv = (MonoContentView)ContentView;
			if (cv.FocusHandle != IntPtr.Zero)
				driver.SendMessage(cv.FocusHandle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows) {
				if (utility_window != this && utility_window.IsVisible)
					utility_window.OrderOut (utility_window);
			}
		}

		// TODO: expanding, collapsing

		internal virtual void resizeWinForm()
		{
			resizeWinForm(Hwnd.GetObjectFromWindow(this.ContentView.Handle));
		}

		// Tells win form to update it's content
		internal virtual void resizeWinForm(Hwnd contentViewHandle)
		{
			driver.HwndPositionFromNative(contentViewHandle);
			driver.SendMessage (contentViewHandle.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
		}

		internal virtual void ActivateNextWindow()
		{
			var windows = GetOrderedWindowList();
			foreach (var window in windows)
			{
				if (window is MonoWindow && window != this && window.IsVisible && !window.IsMiniaturized && !window.IsSheet && window.CanBecomeKeyWindow && !window.IsKeyWindow)
				{
					window.MakeKeyAndOrderFront(this);
					break;
				}
			}
		}

		static internal List<NSWindow> GetOrderedWindowList()
		{
			var numbers = WindowNumbersWithOptions(NSWindowNumberListOptions.AllApplication | NSWindowNumberListOptions.AllSpaces);
			var windows = NSApplication.SharedApplication.Windows;
			var winByNum = new Dictionary<long, NSWindow>(windows.Length);

			foreach (var window in windows)
				winByNum[(long)window.WindowNumber] = window;

			var sorted = new List<NSWindow>(windows.Length);

#if XAMARINMAC
			for (nuint i = 0; i < numbers.Count; ++i)
			{
				var handle = numbers.ValueAt(i);
				var number = (NSNumber)Activator.CreateInstance(
					typeof(NSNumber),
					Reflection.BindingFlags.NonPublic | Reflection.BindingFlags.Instance,
					null,
					new object[] { handle },
					null);

				NSWindow window;
				if (number != null && winByNum.TryGetValue(number.Int64Value, out window))
					sorted.Add(window);
			}
#else
			for (int i = 0; i < numbers.Count; ++i)
			{
				var handle = numbers.ValueAt((uint)i);
				var number = new NSNumber(handle);

				NSWindow window;
				if (number != null && winByNum.TryGetValue(number.Int64Value, out window))
					sorted.Add(window);
			}
#endif

			return sorted;
		}

		#region Support for focusing native and MWF controls by Tab/ShiftTab

		IntPtr FindNextControl(NSView view, bool forward)
		{
			Control control = FindContainingControl(view);
			Control root = FindRootControl(control);

			if (root != null)
			{
				var next = forward ? FindNextControl(root, control) : FindPrevControl(root, control);
				if (next != null)
					return next.Handle;
			}

			return IntPtr.Zero;
		}

		Control FindContainingControl(NSView view)
		{
			while (view != null)
			{
				var control = Control.FromHandle(view.Handle);
				if (control != null)
					return control;
				view = view.Superview;
			}
			return null;
		}

		Control FindRootControl(Control control)
		{
			Control root;
			for (root = control; control != null; control = control.Parent)
				root = control;

			return root;
		}

		public Control FindSuperControl(Control control)
		{
			var next = control;
			while (next != null)
			{
				if (next.CanSelect && next.TabStop)
					return next;

				next = next.Parent;
			}
			return null;
		}

		public Control FindNextControl(Control top, Control control)
		{
			var next = control;
			bool wrap = true;
			do
			{
				next = top.GetNextControl(next, true);
				if (next == null)
				{
					if (wrap)
					{
						wrap = false;
						continue;
					}
					break;
				}

				if (next.CanSelect && next.TabStop && !next.Contains(control))
					return next;

			} while (next != control);

			return null;
		}

		public Control FindPrevControl(Control top, Control ctl)
		{
			bool wrap = true;
			Control next = ctl, prev = null;
			do
			{
				next = top.GetNextControl(next, true);
				if (next == null)
				{
					if (wrap)
					{
						wrap = false;
						continue;
					}
					break;
				}

				if (next.CanSelect && next.TabStop && !next.Contains(ctl))
					prev = next;

			} while (next != ctl);

			return prev;
		}

		#endregion
	}
}


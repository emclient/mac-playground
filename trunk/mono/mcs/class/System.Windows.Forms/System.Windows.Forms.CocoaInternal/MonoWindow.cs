using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
#else
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
#endif
using System.Diagnostics;

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
				
				// Handle tab and shift+tab keys so that switching focus works for both MWF and native controls 
				case NSEventType.KeyDown:
					if (FirstResponder is NSControl)
					{
						int c = (int)theEvent.Characters[0];
						if (c == ShiftTabKey || c == TabKey)
						{
							monoContentView.FocusHandle = FindNextControl(FirstResponder as NSView, c == 9);
							MakeFirstResponder(monoContentView);
							return;
						}
					}
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

		[Export("windowWillClose:")]
		internal virtual bool willClose (NSObject sender)
		{
			ActivateNextWindow();
			return true;
		}

		public override void OrderOut(NSObject sender)
		{
			base.OrderOut(sender);
			ActivateNextWindow();
		}

		public override void OrderWindow(NSWindowOrderingMode place, int relativeTo)
		{
			base.OrderWindow(place, relativeTo);
			if (place == NSWindowOrderingMode.Out)
				ActivateNextWindow();
		}

		[Export ("windowWillResize:toSize:")]
		internal virtual SizeF willResize (NSWindow sender, SizeF toFrameSize)
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

		[Export ("windowDidBecomeKey:")]
		internal virtual void windowDidBecomeKey(NSNotification notification)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			XplatUICocoa.ActiveWindow = hwnd.Handle;
			driver.SendMessage (hwnd.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);

			var cv = (MonoContentView)ContentView;
			if (cv.FocusHandle != IntPtr.Zero)
				driver.SendMessage(cv.FocusHandle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows) {
				if (utility_window != this && ! utility_window.IsVisible)
					utility_window.OrderFront (utility_window);
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

		NSResponder prevFirstResponder = null;
		[Export ("windowDidUpdate:")]
		internal virtual void windowDidUpdate (NSNotification notification)
		{
			if (prevFirstResponder != FirstResponder)
			{
				prevFirstResponder = FirstResponder;
			}
		}
	
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
				if (window is MonoWindow && window != this && window.IsVisible && !window.IsMiniaturized && !window.IsSheet)
				{
					window.MakeKeyAndOrderFront(this);
					break;
				}
			}
		}

		static internal List<NSWindow> GetOrderedWindowList()
		{
			var numbers = NSWindow.WindowNumbersWithOptions(NSWindowNumberListOptions.AllApplication | NSWindowNumberListOptions.AllSpaces);
			var windows = NSApplication.SharedApplication.Windows;
			var winByNum = new Dictionary<int, NSWindow>(windows.Length);

			foreach (var window in windows)
				winByNum[window.WindowNumber] = window;

			var sorted = new List<NSWindow>(windows.Length);

			for (uint i = 0; i < numbers.Count; ++i)
			{
				var handle = numbers.ValueAt((uint)i);
				var number = new NSNumber(handle);

				NSWindow window;
				if (number != null && winByNum.TryGetValue(number.IntValue, out window))
					sorted.Add(window);
			}

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

				if (next.CanSelect && next.TabStop)
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

				if (next.CanSelect && next.TabStop)
					prev = next;

			} while (next != ctl);

			return prev;
		}

		#endregion
	}
}


﻿using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

#if XAMARINMAC
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif

#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSSize = System.Drawing.SizeF;
using nint = System.Int32;
#else
#if XAMARINMAC
using NSRect = CoreGraphics.CGRect;
using NSSize = CoreGraphics.CGSize;
#elif MONOMAC
using NSRect = MonoMac.CoreGraphics.CGRect;
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
		internal NSResponder savedReponder;

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

		public override bool MakeFirstResponder(NSResponder aResponder)
		{
			if (FirstResponder == aResponder)
				return true;
			
			var focusView = FirstResponder as MonoView;
			var newFocusView = aResponder as MonoView;
			if (base.MakeFirstResponder(aResponder))
			{
				if (focusView != null)
					driver.SendMessage(focusView.Handle, Msg.WM_KILLFOCUS, newFocusView != null ? newFocusView.Handle : IntPtr.Zero, IntPtr.Zero);
				if (newFocusView != null && FirstResponder == aResponder)
					driver.SendMessage(newFocusView.Handle, Msg.WM_SETFOCUS, focusView != null ? focusView.Handle : IntPtr.Zero, IntPtr.Zero);

				// If the newly focused control is not MonoView then it must be some embedded native control. Try
				// to update the ActiveControl chain in Form to match it using similar approach as in Control.WmSetFocus. 
				if (newFocusView == null)
				{
					for (var view = aResponder as NSView; view != null; view = view.Superview)
					{
						var wrapperControl = Control.FromChildHandle(view.Handle);
						if (wrapperControl != null)
						{
							(wrapperControl ?? wrapperControl.Parent).Select(wrapperControl);
							break;
						}
					}
				}

				return true;
			}

			return false;
		}

		public override bool CanBecomeKeyWindow
		{
			get
			{
				var hwnd = Hwnd.GetObjectFromWindow(this.ContentView.Handle);
				if (hwnd.zombie)
					return false;
				return true;
			}
		}

		public override bool CanBecomeMainWindow
		{
			get
			{
				return CanBecomeKeyWindow && owner == null;
			}
		}

		public override void SendEvent(NSEvent theEvent)
		{
			var monoContentView = (MonoContentView)ContentView;

			switch (theEvent.Type)
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
				case NSEventType.BeginGesture:
				case NSEventType.EndGesture:	
				case NSEventType.MouseMoved:
					if (XplatUICocoa.Grab.Hwnd != IntPtr.Zero) {
						var grabView = (NSView) ObjCRuntime.Runtime.GetNSObject(XplatUICocoa.Grab.Hwnd);
						switch (theEvent.Type) {
							case NSEventType.LeftMouseDown: grabView.MouseDown(theEvent); break;
							case NSEventType.RightMouseDown: grabView.RightMouseDown(theEvent); break;
							case NSEventType.OtherMouseDown: grabView.OtherMouseDown(theEvent); break;
							case NSEventType.LeftMouseUp: grabView.MouseUp(theEvent); break;
							case NSEventType.RightMouseUp: grabView.RightMouseUp(theEvent); break;
							case NSEventType.OtherMouseUp: grabView.OtherMouseUp(theEvent); break;
							case NSEventType.LeftMouseDragged: grabView.MouseDragged(theEvent); break;
							case NSEventType.RightMouseDragged: grabView.RightMouseDragged(theEvent); break;
							case NSEventType.OtherMouseDragged: grabView.OtherMouseDragged(theEvent); break;
							case NSEventType.ScrollWheel: grabView.ScrollWheel(theEvent); break;
							case NSEventType.BeginGesture: grabView.BeginGestureWithEvent(theEvent); break;
							case NSEventType.EndGesture: grabView.EndGestureWithEvent(theEvent); break;
							case NSEventType.MouseMoved: grabView.MouseMoved(theEvent); break;
						}
						return;
					}
					if (theEvent.Type == NSEventType.LeftMouseDown ||
						theEvent.Type == NSEventType.RightMouseDown ||
						theEvent.Type == NSEventType.OtherMouseDown ||
						theEvent.Type == NSEventType.BeginGesture) {
						var hitTest = ContentView.HitTest(theEvent.LocationInWindow);
						if (!(hitTest is MonoView)) {
							// Make sure any popup menus are closed when clicking on embedded NSView.
							ToolStripManager.FireAppClicked();
						} 
					}
					break;
			}

			base.SendEvent(theEvent);
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

		public override void OrderWindow(NSWindowOrderingMode place, nint relativeTo)
		{
			base.OrderWindow(place, relativeTo);

			if (place == NSWindowOrderingMode.Out && IsKeyWindow)
				NSApplication.SharedApplication.BeginInvokeOnMainThread(() => {
					try { ActivateNextWindow(); }
					catch (Exception e) { Debug.WriteLine("Failed async call ActivateNextWindow(: " + e); }
				});
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

		public override void BecomeKeyWindow()
		{
			base.BecomeKeyWindow();

			if (CanBecomeMainWindow)
				MakeMainWindow();

			// FIXME: Set LParam
			driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && !utility_window.IsVisible)
					utility_window.OrderFront(utility_window);
			}
		}

		public override void ResignKeyWindow()
		{
			var newKeyWindow = NSApplication.SharedApplication.KeyWindow;

			base.ResignKeyWindow();

			driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, newKeyWindow != null ? newKeyWindow.ContentView.Handle : IntPtr.Zero);

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && utility_window.IsVisible)
					utility_window.OrderOut(utility_window);
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
	}
}


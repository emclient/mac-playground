﻿﻿using System.Drawing;
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
using System.Windows.Forms.Mac;
#elif MONOMAC
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSSize = MonoMac.CoreGraphics.CGSize;
using nint = System.Int32;
using System.Windows.Forms.Mac;
#endif
#endif

namespace System.Windows.Forms.CocoaInternal
{
	class MonoWindow : NSWindow
	{
		private XplatUICocoa driver;

		public MonoWindow(IntPtr handle) : base(handle)
		{
		}

		//[Export ("initWithContentRect:styleMask:backing:defer:"), CompilerGenerated]
		internal MonoWindow(NSRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, XplatUICocoa driver)
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			this.driver = driver;

			// Disable tabbing on Sierra until we properly support it
			var setTabbingModeSelector = new ObjCRuntime.Selector("setTabbingMode:");
			if (this.RespondsToSelector(setTabbingModeSelector))
				this.SetValueForKey(NSNumber.FromInt32(2), new NSString("tabbingMode"));
		}

		public override bool MakeFirstResponder(NSResponder aResponder)
		{
			var prevFirstResponder = FirstResponder;
			if (base.MakeFirstResponder(aResponder) && IsKeyWindow && prevFirstResponder != FirstResponder)
			{
				var prev = GetHandle(prevFirstResponder);
				var next = GetHandle(aResponder);

				if (prev != IntPtr.Zero)
					driver.SendMessage(prev, Msg.WM_KILLFOCUS, next, IntPtr.Zero);
				if (next != IntPtr.Zero && FirstResponder == aResponder)
					driver.SendMessage(next, Msg.WM_SETFOCUS, prev, IntPtr.Zero);

				// If the newly focused control is not a MWF control's view, then it must be some embedded native control. Try
				// to update the ActiveControl chain in Form to match it using similar approach as in Control.WmSetFocus. 
				var wrapperControl = Control.FromChildHandle(next);
				if (wrapperControl != null && wrapperControl.Handle != next)
					(wrapperControl.Parent ?? wrapperControl).Select(wrapperControl);

				return true;
			}

			return false;
		}

		static IntPtr GetHandle(NSObject o)
		{
			if (o is NSView view)
				return view.Handle;
			//if (o is MonoWindow mwin && mwin.ContentView != null)
				//return mwin.ContentView.Handle;
			if (o is WindowsEventResponder wer)
				return wer.view.Handle;
			return IntPtr.Zero;
		}

		public override bool CanBecomeKeyWindow
		{
			get
			{
				if (ContentView is MonoView monoView)
				{
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

		IntPtr hitTestHandle = IntPtr.Zero;
		MouseActivate mouseActivate = MouseActivate.MA_ACTIVATE;
		NSEventType lastEventType = NSEventType.ApplicationDefined;

		public override void SendEvent(NSEvent theEvent)
		{
			DebugUtility.WriteInfoIfChanged(theEvent);

			lastEventType = theEvent.Type;
			var monoContentView = (MonoContentView)ContentView;

			switch (theEvent.Type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
				case NSEventType.BeginGesture:
					hitTestHandle = (ContentView.Superview ?? ContentView).HitTest(theEvent.LocationInWindow)?.Handle ?? IntPtr.Zero;
					if (!ToolStripManager.IsChildOfActiveToolStrip(hitTestHandle))
						ToolStripManager.FireAppClicked();
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
					break;
			}

			if (theEvent.Type == NSEventType.LeftMouseDown)
			{
				var topLevelParent = IntPtr.Zero; // FIXME
				mouseActivate = (MouseActivate)driver.SendMessage(ContentView.Handle, Msg.WM_MOUSEACTIVATE, topLevelParent, hitTestHandle).ToInt32();
				if (mouseActivate == MouseActivate.MA_NOACTIVATEANDEAT)// || mouseActivate == MouseActivate.MA_ACTIVATEANDEAT)
					return;
			}

			base.SendEvent(theEvent);
		}

		[Export("windowShouldClose:")]
		internal virtual bool shouldClose(NSObject sender)
		{
			if (Control.FromHandle(ContentView.Handle) is Form form)
			{
				// If this is the main form, close all the other forms first, to prevent uninitializing errors.
				if (Application.OpenForms[0].Handle == ContentView.Handle)
				{
					var openForms = new List<Form>();
					foreach (Form f in Application.OpenForms)
						if (f.Handle != ContentView.Handle)
							openForms.Add(f);
					
					foreach (var f in openForms)
						f.Close();
				}

				form.Close(); // Sets CloseReason, among other things
			}
			else
				driver.SendMessage(ContentView.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			return false;
		}

		public override void OrderWindow(NSWindowOrderingMode place, nint relativeTo)
		{
			base.OrderWindow(place, relativeTo);

			if (place == NSWindowOrderingMode.Out)
			{
				// Remove the window from Windows menu
				NSApplication.SharedApplication.RemoveWindowsItem(this);

				if (IsKeyWindow)
				{
					NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
					{
						try { ActivateNextWindow(); }
						catch (Exception e) { Debug.WriteLine("Failed async call ActivateNextWindow(: " + e); }
					});
				}
			}
		}

		public override void SetFrame(NSRect frameRect, bool display)
		{
			base.SetFrame(frameRect, display);
			driver.SendMessage(ContentView?.Handle ?? IntPtr.Zero, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("windowWillResize:toSize:")]
		internal virtual NSSize willResize(NSWindow sender, NSSize toFrameSize)
		{
			var rect = new XplatUIWin32.RECT(0, 0, (int)toFrameSize.Width, (int)toFrameSize.Height);
			IntPtr lpRect = Marshal.AllocHGlobal(Marshal.SizeOf(rect));
			Marshal.StructureToPtr(rect, lpRect, false);

			//FIXME - deduce WMSZ
			IntPtr wParam = new IntPtr(8); //WMSZ_BOTTOMRIGHT;

			NativeWindow.WndProc(ContentView.Handle, Msg.WM_SIZING, wParam, lpRect);
			var rect2 = (Rectangle)Marshal.PtrToStructure(lpRect, typeof(Rectangle));
			toFrameSize.Width = rect2.Width;
			toFrameSize.Height = rect2.Height;

			Marshal.FreeHGlobal(lpRect);

			return toFrameSize;
		}

		[Export("windowDidResize:")]
		internal virtual void windowDidResize(NSNotification notification)
		{
			// resizeWinForm, invalidate and update?
			resizeWinForm();
		}

		[Export("windowWillStartLiveResize:")]
		internal virtual void windowWillStartLiveResize(NSNotification notification)
		{
			//var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			NativeWindow.WndProc(ContentView.Handle, Msg.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("windowDidEndLiveResize:")]
		internal virtual void windowDidEndLiveResize(NSNotification notification)
		{
			//var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			resizeWinForm();
			NativeWindow.WndProc(ContentView.Handle, Msg.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("windowDidMove:")]
		internal virtual void windowDidMove(NSNotification notification)
		{
			//var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			//if (hwnd != null)
			//resizeWinForm();
		}

		[Export("windowDidChangeScreen:")]
		internal virtual void windowDidChangeScreen(NSNotification notification)
		{
			BeginInvokeOnMainThread((Action)delegate { resizeWinForm(); });
		}

#if XAMARINMAC
		public override void AddTitlebarAccessoryViewController(NSTitlebarAccessoryViewController childViewController)
		{
			base.AddTitlebarAccessoryViewController(childViewController);
			resizeWinForm();
		}

		public override void RemoveTitlebarAccessoryViewControllerAtIndex(nint index)
		{
			base.RemoveTitlebarAccessoryViewControllerAtIndex(index);
			resizeWinForm();
		}
#endif

		public override void BecomeKeyWindow()
		{
			base.BecomeKeyWindow();

			if (CanBecomeMainWindow)
				MakeMainWindow();

			// FIXME: Set LParam
			//driver.SendMessage(ContentView.Handle, Msg.WM_NCACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, (IntPtr)(-1));
			driver.SendMessage(ContentView.Handle, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);

			if (IsKeyWindow && FirstResponder != null) // For the case that previous WM_ACTIVATE in fact did not activate this window.
				driver.SendMessage(FirstResponder.Handle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);

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

			if (!IsKeyWindow && FirstResponder != null) // For the case that previous WM_ACTIVATE in fact did not deactivate this window.
				driver.SendMessage(FirstResponder.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);

			/*foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
			{
				if (utility_window != this && utility_window.IsVisible)
					utility_window.OrderOut(utility_window);
			}*/
		}

		// TODO: expanding, collapsing

		internal virtual void resizeWinForm()
		{
			/*	resizeWinForm(Hwnd.GetObjectFromWindow(this.ContentView.Handle));
			}

			// Tells win form to update it's content
			internal virtual void resizeWinForm(Hwnd contentViewHandle)
			{*/
			//driver.HwndPositionFromNative(ContentView.Handle);
			driver.RequestNCRecalc(ContentView.Handle);
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
#if XAMARINMAC
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


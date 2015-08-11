using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
#else
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	internal class MonoWindow : NSWindow
	{
		internal XplatUICocoa driver;

		public MonoWindow (IntPtr handle) : base(handle)
		{
		}
			
		//[Export ("initWithContentRect:styleMask:backing:defer:"), CompilerGenerated]
		internal MonoWindow (NSRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, XplatUICocoa driver) 
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			this.driver = driver;
			this.AcceptsMouseMovedEvents = true;
		}

		public override bool CanBecomeMainWindow
		{
			get
			{
				return base.CanBecomeKeyWindow;
			}
		}

		[Export("windowShouldClose:")]
		internal virtual bool shouldClose (NSObject sender)
		{
			var hwnd = Hwnd.GetObjectFromWindow (this.ContentView.Handle);
			driver.SendMessage (hwnd.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			return false;
		}

		[Export("windowWillClose:")]
		internal virtual bool willClose (NSObject sender)
		{
			// TODO: Send WillClose .NET event?
			return true;
		}

		[Export ("windowWillResize:toSize:")]
		internal virtual SizeF willResize (NSWindow sender, SizeF toFrameSize)
		{
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
			driver.SendMessage (hwnd.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
			XplatUICocoa.ActiveWindow = hwnd.Handle;

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
			
			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows) {
				if (utility_window != this && utility_window.IsVisible)
					utility_window.OrderOut (utility_window);
			}
		}

		// TODO: expanding, collapsing

//		[Export ("windowDidUpdate:")]
//		internal virtual void windowDidUpdate (NSNotification notification)
//		{
//		}
	
		internal virtual void resizeWinForm()
		{
			resizeWinForm(Hwnd.GetObjectFromWindow(this.ContentView.Handle));
		}

		// Tells win form to update it's content
		internal virtual void resizeWinForm(Hwnd contentViewHandle)
		{
			//var f = /*this.ConvertRectToScreen(*/this.Frame/*)*/;
			//var r = driver.NativeToMonoScreen(f);
			//XplatUI.SetWindowPos (contentViewHandle.Handle,  (int)r.Left, (int)r.Top, (int)r.Width, (int)r.Height);
			driver.HwndPositionFromNative(contentViewHandle);
			driver.PerformNCCalc (contentViewHandle);
			driver.SendMessage (contentViewHandle.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
		}
	}
}


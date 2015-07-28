using System;
using MonoMac.AppKit;

namespace System.Windows.Forms.CocoaInternal
{
	class MonoApplicationDelegate : NSApplicationDelegate
	{
		XplatUICocoa driver;

		public MonoApplicationDelegate (XplatUICocoa driver)
		{
			this.driver = driver;
		}

		public override void DidBecomeActive (MonoMac.Foundation.NSNotification notification)
		{
			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
				if (!utility_window.IsVisible)
					utility_window.OrderFront (utility_window);

			driver.PostMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
		}

		public override void DidResignActive (MonoMac.Foundation.NSNotification notification)
		{
			IntPtr focusWindow = driver.GetFocus ();
			if (focusWindow != IntPtr.Zero)
				driver.SendMessage (focusWindow, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);

			if (XplatUICocoa.Grab.Hwnd != IntPtr.Zero) {
				driver.SendMessage (Hwnd.ObjectFromHandle (XplatUICocoa.Grab.Hwnd).Handle, 
					Msg.WM_LBUTTONDOWN, (IntPtr)MsgButtons.MK_LBUTTON, 
					(IntPtr) (driver.MousePosition.X << 16 | driver.MousePosition.Y));
			}

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
				if (utility_window.IsVisible)
					utility_window.OrderOut (utility_window);

			driver.PostMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
		}
	}
}


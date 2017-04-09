using System.Collections.Generic;

#if XAMARINMAC
using AppKit;
using Foundation;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	internal class MonoApplicationDelegate : NSApplicationDelegate
	{
		XplatUICocoa driver;
		const long ENDSESSION_LOGOFF = 0x80000000;

		internal static bool IsGoingToPowerOff = false;
		internal static DateTime IsGoingToPowerOfTime = DateTime.MinValue;
		internal const double IsGoingToPowerOfMaxDelay = 30; // To pseudo-handle the case when shutdown is cancelled by another app.

		public MonoApplicationDelegate (XplatUICocoa driver)
		{
			this.driver = driver;

			NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.WillPowerOffNotification, WillPowerOff, null);
			// FIXME: NSMenuDidBeginTrackingNotification should send WM_CANCELMODE
		}

		protected virtual void WillPowerOff(NSNotification n)
		{
			IsGoingToPowerOff = true;
			IsGoingToPowerOfTime = DateTime.Now;
		}

		public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
		{
			bool shouldTerminate = true;

			if (IsGoingToPowerOff && (DateTime.Now - IsGoingToPowerOfTime).TotalSeconds < IsGoingToPowerOfMaxDelay)
			{
				IsGoingToPowerOff = false; // For the case the shutdown is going to be cancelled

				foreach (var window in NSApplication.SharedApplication.Windows)
					if (window.ContentView is MonoContentView)
						if (IntPtr.Zero == XplatUI.SendMessage(window.ContentView.Handle, Msg.WM_QUERYENDSESSION, (IntPtr)1, (IntPtr)ENDSESSION_LOGOFF))
							shouldTerminate = false;
			
				if (!shouldTerminate)
					return NSApplicationTerminateReply.Cancel;

				foreach (var window in NSApplication.SharedApplication.Windows)
					if (window.ContentView is MonoContentView)
						XplatUI.SendMessage(window.ContentView.Handle, Msg.WM_ENDSESSION, (IntPtr)1, (IntPtr)ENDSESSION_LOGOFF);
			}

			return NSApplicationTerminateReply.Now;
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
		}

		public override void DidBecomeActive (NSNotification notification)
		{
			driver.SendMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
		}

		public override void WillResignActive (NSNotification notification)
		{
			if (driver.Grab.Hwnd != IntPtr.Zero) {
				driver.SendMessage(driver.Grab.Hwnd, Msg.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
				driver.Grab.Hwnd = IntPtr.Zero;
			}
			driver.SendMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
		}
	}
}


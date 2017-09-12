using System.Collections.Generic;
using System.Runtime.InteropServices;

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
		internal bool DeactivateAppOnDraggingEnded = false;
		internal NSMenu dockMenu;

		internal MonoApplicationDelegate (XplatUICocoa driver)
		{
			this.driver = driver;
			this.dockMenu = new NSMenu();

			XplatUICocoa.DraggingEnded += XplatUICocoa_DraggingEnded;

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
			if (XplatUICocoa.DraggedData == null)
				DoActivateApp();
			else
				DeactivateAppOnDraggingEnded = false;

		}

		public override void WillResignActive (NSNotification notification)
		{
			if (XplatUICocoa.DraggedData == null) // Do not perform deactivation if dragging in progress
				DoDeactivateApp();
			else
				DeactivateAppOnDraggingEnded = true;
		}

		void DoActivateApp()
		{
			driver.SendMessage(XplatUI.GetActive(), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
		}

		void DoDeactivateApp()
		{
			if (driver.Grab.Hwnd != IntPtr.Zero)
			{
				driver.SendMessage(driver.Grab.Hwnd, Msg.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
				driver.Grab.Hwnd = IntPtr.Zero;
			}
			driver.SendMessage(XplatUI.GetActive(), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
		}

		void XplatUICocoa_DraggingEnded(object sender, EventArgs e)
		{
			if (DeactivateAppOnDraggingEnded)
			{
				DeactivateAppOnDraggingEnded = false;
				DoDeactivateApp();
			}
		}

		public override bool OpenFile(NSApplication sender, string filename)
		{
			return TryOpenFiles(new string[] { filename });
		}

		public override void OpenFiles(NSApplication sender, string[] filenames)
		{
			TryOpenFiles(filenames);
		}

		internal bool TryOpenFiles(string[] filenames)
		{
			GCHandle gch = new GCHandle();
			try
			{
				gch = GCHandle.Alloc(filenames);
				var result = driver.SendMessage(XplatUI.GetActive(), Msg.WM_OPEN_FILES, IntPtr.Zero, GCHandle.ToIntPtr(gch));
				return result == IntPtr.Zero;
			}
			catch (Exception e)
			{
				DebugHelper.WriteLine($"Failed opening file(s) [{String.Join(",", filenames)}]: {e}");
				return false;
			}
			finally
			{
				if (gch.IsAllocated)
					gch.Free();
			}
		}

		public override NSMenu ApplicationDockMenu(NSApplication sender)
		{
			GCHandle gch = new GCHandle();
			try
			{
				gch = GCHandle.Alloc(dockMenu);
				driver.SendMessage(XplatUI.GetActive(), Msg.WM_DOCK_MENU, IntPtr.Zero, GCHandle.ToIntPtr(gch));
			}
			catch (Exception e)
			{
				DebugHelper.WriteLine($"Exception in WM_DOCK_MENU handler: {e}");
			}
			finally
			{
				if (gch.IsAllocated)
					gch.Free();
			}
			return dockMenu;
		}
	}
}


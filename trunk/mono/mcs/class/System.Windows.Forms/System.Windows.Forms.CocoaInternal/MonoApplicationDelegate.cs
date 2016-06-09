using System.Collections.Generic;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

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

				var forms = GetOpenForms();
				foreach (var form in forms)
					if (!form.IsDisposed && form.IsHandleCreated)
						if (IntPtr.Zero == XplatUI.SendMessage(form.Handle, Msg.WM_QUERYENDSESSION, (IntPtr)1, (IntPtr)ENDSESSION_LOGOFF))
							shouldTerminate = false;
			
				if (!shouldTerminate)
					return NSApplicationTerminateReply.Cancel;

				forms = GetOpenForms();
				foreach (var form in forms)
					if (!form.IsDisposed && form.IsHandleCreated)
						XplatUI.SendMessage(form.Handle, Msg.WM_ENDSESSION, (IntPtr)1, (IntPtr)ENDSESSION_LOGOFF);
			}

			return NSApplicationTerminateReply.Now;
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
		}

		public override void DidBecomeActive (NSNotification notification)
		{
			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
				if (!utility_window.IsVisible)
					utility_window.OrderFront (utility_window);

			driver.SendMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
		}

		public override void WillResignActive (NSNotification notification)
		{
			IntPtr focusWindow = driver.GetFocus ();
			if (focusWindow != IntPtr.Zero)
				driver.SendMessage (focusWindow, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);

			if (XplatUICocoa.Grab.Hwnd != IntPtr.Zero) {
				driver.SendMessage (Hwnd.ObjectFromHandle (XplatUICocoa.Grab.Hwnd).Handle, 
					Msg.WM_LBUTTONUP, (IntPtr)MsgButtons.MK_LBUTTON, 
					(IntPtr) (driver.MousePosition.X << 16 | driver.MousePosition.Y));
			}

			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
				if (utility_window.IsVisible)
					utility_window.OrderOut (utility_window);

			driver.SendMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
		}

		void CreateMenu()
		{
			NSMenu mainMenu = new NSMenu();
			NSMenu appMenu = new NSMenu(Application.ProductName);
			NSMenuItem quitItem = appMenu.AddItem("Quit " + Application.ProductName, new Selector("terminate:"), "q");
			quitItem.KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask;// | NSEventModifierMask.AlternateKeyMask;
			NSMenuItem appItem = new NSMenuItem();
			appItem.Title = Application.ProductName;
			appItem.Submenu = appMenu;
			mainMenu.AddItem(appItem);

			NSMenu windowMenu = new NSMenu();
			windowMenu.Title = "Window";
			windowMenu.AddItem("Minimize", new Selector("performMiniaturize:"), "");
			windowMenu.AddItem("Zoom", new Selector("performZoom:"), "");
			windowMenu.AddItem(NSMenuItem.SeparatorItem);
			windowMenu.AddItem("Bring All to Front", new Selector("arrangeInFront:"), "");
			NSMenuItem windowItem = new NSMenuItem();
			windowItem.Submenu = windowMenu;
			mainMenu.AddItem(windowItem);

			NSApplication.SharedApplication.MainMenu = mainMenu;
			NSApplication.SharedApplication.WindowsMenu = windowMenu;
		}

		public static List<Form> GetOpenForms()
		{
			var forms = new List<Form>(Application.OpenForms.Count);
			foreach (Form f in Application.OpenForms)
				forms.Add(f);
			return forms;
		}
	}
}


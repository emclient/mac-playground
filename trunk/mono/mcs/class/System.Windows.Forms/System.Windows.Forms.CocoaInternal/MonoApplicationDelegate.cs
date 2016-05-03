using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace System.Windows.Forms.CocoaInternal
{
	class MonoApplicationDelegate : NSApplicationDelegate
	{
		XplatUICocoa driver;

		public MonoApplicationDelegate (XplatUICocoa driver)
		{
			this.driver = driver;
		}

		public override void DidBecomeActive (NSNotification notification)
		{
			foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
				if (!utility_window.IsVisible)
					utility_window.OrderFront (utility_window);

			driver.SendMessage (XplatUI.GetActive (), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);

			CreateMenu();
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
	}
}


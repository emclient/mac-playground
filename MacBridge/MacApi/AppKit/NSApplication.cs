using System;

#if MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#elif XAMARINMAC
using Foundation;
using AppKit;
#endif

namespace MacBridge.AppKit
{
	public static class NSApplicationEx
	{
		public static bool Activate(int pid, int wait = 5000)
		{
			NSRunningApplication app = null;
			for (; wait > 0; wait -= 100)
			{
				if (app == null)
					app = NSRunningApplication.GetRunningApplication(pid);

				if (app != null)
					if (app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows))
						return true;
					
				System.Threading.Thread.Sleep(100);
			}
			return false;
		}

		public static void ActivateCurrent()
		{
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
		}
	}
}

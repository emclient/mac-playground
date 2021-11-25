using System;
using Foundation;
using AppKit;

namespace MacApi.AppKit
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

				if (app != null && app.FinishedLaunching)
					if (app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows))
						return true;
					
				NSThread.SleepFor(0.1);
			}
			return false;
		}

		public static void ActivateCurrent()
		{
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
		}
	}
}

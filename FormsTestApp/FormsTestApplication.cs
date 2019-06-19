#if __MACOS__

using System;
using System.Windows.Forms;
using AppKit;
using Foundation;

namespace FormsTest.App
{
	// This is a test if the mechanism for creating the principal class usng Info.plist works

	public class FormsTestApplication : NSApplication
	{
		public FormsTestApplication(IntPtr handle) : base(handle)
		{
		}

		[Export("isRunning")]
		public bool IsRunning
		{
			get { return Application.MessageLoop; }
		}
	}
}

#endif

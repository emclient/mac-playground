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
    public static class NSWorkspaceEx
    {
        public static int LaunchApplicationForPath(string path, string[] args, bool activate = true)
        {
            var options = new NSWorkspaceLaunchOptions();
            var configuration = new NSDictionary();
			var encodedPath = System.Net.WebUtility.UrlEncode(path);
            var url = new NSUrl("file://" + encodedPath);

            NSError error = null;
#if MONOMAC
            var app = NSWorkspace.SharedWorkspace.LaunchApplication(url, options, configuration, error);
#else
            var app = NSWorkspace.SharedWorkspace.OpenURL(url, options, configuration, out error);
#endif
            if (error != null)
                throw new ApplicationException("NSWorkspace failed to open URL: " + url);

            if (app == null)
                return 0;

            if (activate)
                app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            return app.ProcessIdentifier;
        }
    }
}

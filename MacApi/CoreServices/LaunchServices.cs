using System;
using System.Runtime.InteropServices;
#if XAMARINMAC
using Foundation;
#elif MONOMAC
using MonoMac.Foundation;
#endif

namespace MacApi.CoreServices
{
	public class LaunchServices
	{
#if !XAMARINMAC
		class NSUrlHelper : NSUrl
		{
			public NSUrlHelper (IntPtr handle) : base(handle)
			{
			}
		}

        public static string GetDefaultHandlerForUrlScheme(string urlScheme)
        {
            var cfHandler = LSCopyDefaultHandlerForURLScheme(new NSString(urlScheme).Handle);
            return NSString.FromHandle(cfHandler);
        }

        public static int SetDefaultHandlerForUrlScheme(string urlScheme, string handlerBundleID)
        {
            return LSSetDefaultHandlerForURLScheme(new NSString(urlScheme).Handle, new NSString(handlerBundleID).Handle);
        }

		[DllImport(Constants.CoreServicesLibrary)]
        internal extern static IntPtr LSCopyDefaultHandlerForURLScheme(IntPtr cfStringUrlScheme);

        [DllImport(Constants.CoreServicesLibrary)]
        internal extern static int LSSetDefaultHandlerForURLScheme(IntPtr urlScheme, IntPtr handlerBundleID);
#endif
	}
}

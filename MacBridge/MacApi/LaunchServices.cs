using System;
using System.Runtime.InteropServices;
#if XAMARINMAC
using Foundation;
using CoreFoundation;
#else
using MonoMac.Foundation;
using MonoMac.CoreFoundation;
#endif

namespace MacBridge.LaunchServices
{
    public partial class LS {

		class NSUrlHelper : NSUrl
		{
			public NSUrlHelper (IntPtr handle) : base(handle)
			{
			}
		}

        public static NSUrl CopyDefaultApplicationUrlForUrl(NSUrl url, int lsRolesMask)
		{
            var cfErrorHandle = IntPtr.Zero;
            var hAppUrl = LSCopyDefaultApplicationURLForURL(url.Handle, lsRolesMask, ref cfErrorHandle);
            if (cfErrorHandle != IntPtr.Zero)
                throw CFException.FromCFError(cfErrorHandle);
            return new NSUrlHelper(hAppUrl);
        }

        public static string CopyDefaultHandlerForURLScheme(string urlScheme)
        {
            var cfHandler = LSCopyDefaultHandlerForURLScheme(new NSString(urlScheme).Handle);
            return NSString.FromHandle(cfHandler);
        }

        public static int SetDefaultHandlerForURLScheme(string urlScheme, string handlerBundleID)
        {
            return LSSetDefaultHandlerForURLScheme(new NSString(urlScheme).Handle, new NSString(handlerBundleID).Handle);
        }

        public static int RegisterURL(NSUrl url, bool update)
        {
            return LSRegisterURL(url.Handle, update);
        }

        #region Native API

        internal const string LaunchServicesDll = "/system/Library/frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices";

        [DllImport(LaunchServicesDll)]
        internal extern static IntPtr LSCopyDefaultHandlerForURLScheme(IntPtr cfStringUrlScheme);

        [DllImport(LaunchServicesDll)]
        internal extern static int LSSetDefaultHandlerForURLScheme(IntPtr urlScheme, IntPtr handlerBundleID);

        [DllImport(LaunchServicesDll)]
        internal extern static IntPtr LSCopyDefaultApplicationURLForURL(IntPtr cfUrl, int lsRolesMask, ref IntPtr cfError);

        [DllImport(LaunchServicesDll)]
        internal extern static int LSRegisterURL(IntPtr CFUrl, bool update);

        #endregion //Native API
    }

    public partial class UTType
    {
        public static string CreatePreferredIdentifierForTag(string path)
        {
            var ext = new NSString(path).PathExtension;
            var tagname = new NSString(kUTTagClassFilenameExtension);
            var huti = UTTypeCreatePreferredIdentifierForTag(tagname.Handle, ext.Handle, IntPtr.Zero);
            return NSString.FromHandle(huti);
        }

        #region Native API

        const string kUTTagClassFilenameExtension = "public.filename-extension";

        [DllImport(LS.LaunchServicesDll)]
        extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);

        #endregion // Native API
    }
}

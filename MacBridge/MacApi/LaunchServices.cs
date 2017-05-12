#if !XAMARINMAC
using System;
using System.Runtime.InteropServices;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;

namespace MacBridge.LaunchServices
{
    public partial class LaunchServices {

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
    }

    public partial class UTType
    {
        public static string CreatePreferredIdentifier(string tagClass, string tag, string conformingToUti)
        {
			var a = NSString.CreateNative(tagClass);
			var b = NSString.CreateNative(tag);
			var c = NSString.CreateNative(conformingToUti);
			var ret = NSString.FromHandle(UTTypeCreatePreferredIdentifierForTag(a, b, c));
			NSString.ReleaseNative(a);
			NSString.ReleaseNative(b);
			NSString.ReleaseNative(c);
			return ret;
        }

		public static string GetPreferredTag(string uti, string tagClass)
		{
			if (uti == null)
				throw new ArgumentNullException("uti");
			if (tagClass == null)
				throw new ArgumentNullException("tagClass");

			var a = NSString.CreateNative(uti);
			var b = NSString.CreateNative(tagClass);
			var ret = NSString.FromHandle(UTTypeCopyPreferredTagWithClass(a, b));
			NSString.ReleaseNative(a);
			NSString.ReleaseNative(b);
			return ret;
		}

		public const string TagClassFilenameExtension = "public.filename-extension";

		[DllImport(Constants.CoreServicesLibrary)]
        extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
		[DllImport(Constants.CoreServicesLibrary)]
		extern static IntPtr UTTypeCopyPreferredTagWithClass(IntPtr uti, IntPtr tagClass);
    }
}
#endif

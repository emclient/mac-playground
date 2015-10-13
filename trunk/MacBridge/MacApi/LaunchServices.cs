using System;
using System.Runtime.InteropServices;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;

namespace MacBridge.LaunchServices
{
    public partial class LS {

        public static NSUrl DefaultApplicationUrlForUrl(NSUrl url, int lsRolesMask) {
            var cfErrorHandle = IntPtr.Zero;
            var hAppUrl = LSCopyDefaultApplicationURLForURL(url.Handle, lsRolesMask, ref cfErrorHandle);
            if (cfErrorHandle != IntPtr.Zero)
                throw CFException.FromCFError(cfErrorHandle);
            return new NSUrl(hAppUrl);
        }

        public static int SetDefaultHandlerForURLScheme(string urlScheme, string handlerBundleID)
        {
            return LSSetDefaultHandlerForURLScheme(new NSString(urlScheme).Handle, new NSString(handlerBundleID).Handle);
        }

        #region Native API

        internal const string LaunchServicesDll = "/system/Library/frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices";

        [DllImport(LaunchServicesDll)]
        internal extern static int LSSetDefaultHandlerForURLScheme(IntPtr urlScheme, IntPtr handlerBundleID);

        [DllImport(LaunchServicesDll)]
        internal extern static IntPtr LSCopyDefaultApplicationURLForURL(IntPtr cfUrl, int lsRolesMask, ref IntPtr cfError);

        [DllImport(LaunchServicesDll)]
        internal extern static int LSRegisterURL(IntPtr CFUrl, bool update);

        //func LSCopyDefaultApplicationURLForContentType(_ inContentType: CFString, _ inRoleMask: LSRolesMask, _ outError: UnsafeMutablePointer<Unmanaged<CFError>?>) -> Unmanaged<CFURL>?
        //func LSCopyApplicationURLsForURL(_ inURL: CFURL, _ inRoleMask: LSRolesMask) -> Unmanaged<CFArray>?
        //func LSOpenCFURLRef(_ inURL: CFURL, _ outLaunchedURL: UnsafeMutablePointer<Unmanaged<CFURL>?>) -> OSStatus
        //func LSRegisterURL(_ inURL: CFURL, _ inUpdate: Bool) -> OSStatus
        //func LSSetDefaultHandlerForURLScheme(_ inURLScheme: CFString, _ inHandlerBundleID: CFString) -> OSStatus
        //func LSCopyDefaultApplicationURLForContentType(_ inContentType: CFString, _ inRoleMask: LSRolesMask, _ outError: UnsafeMutablePointer<Unmanaged<CFError>?>) -> Unmanaged<CFURL>?

        //LSOpenFromURLSpec
        //LSOpenFromRefSpec // Swiss Army Knife

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


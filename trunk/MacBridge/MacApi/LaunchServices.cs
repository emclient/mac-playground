using System;
using System.Runtime.InteropServices;
using MonoMac.Foundation;

namespace MacBridge.LaunchServices
{
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

        const string LaunchServicesDll = "/system/Library/frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices";
        const string kUTTagClassFilenameExtension = "public.filename-extension";

        [DllImport(LaunchServicesDll)]
        extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);

        #endregion // Native API
    }
}


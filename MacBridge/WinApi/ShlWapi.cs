using System;
#if XAMARINMAC
using Foundation;
#else
using MonoMac.Foundation;
#endif
using System.Runtime.InteropServices;

namespace WinApi
{
    public static partial class Win32
    {
        const string NSURLVolumeIsLocalKey = "NSURLVolumeIsLocalKey";
        const string NSURLFileSizeKey = "NSURLFileSizeKey";

        public static bool PathIsNetworkPath(string path)
        {
            var url = NSUrl.FromFilename(path);
            var keys = new NSString[] { new NSString(NSURLVolumeIsLocalKey) };
            NSError error = null;

            NSDictionary d = url.GetResourceValues(keys, out error);
            if (error != null || d == null)
                throw new ApplicationException("Can't determine if the path is a network path");

            NSObject o;
            if (d.TryGetValue(new NSString(NSURLVolumeIsLocalKey), out o))
                if (o is NSNumber)
                    return !((NSNumber)(o)).BoolValue;
            
            return false;
        }
    }
}


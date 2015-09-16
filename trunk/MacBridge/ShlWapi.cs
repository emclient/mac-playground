using System;
using MonoMac.Foundation;
using System.Runtime.InteropServices;

namespace MacBridge
{
    public static class ShlWapi
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


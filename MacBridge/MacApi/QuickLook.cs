using System;
using System.Runtime.InteropServices;
#if XAMARINMAC
using CoreGraphics;
using Foundation;
using CoreFoundation;
#else
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;
#endif

namespace MacBridge.QuickLook
{
    public static partial class QLThumbnailImage
    {
        public static CGImage Create(NSUrl url, CGSize maxThumbnailSize, bool asIcon)
        {
            var allocator = CFAllocator.Default;
            var dict = NSDictionary.FromObjectAndKey (new NSNumber (asIcon), new NSString ("kQLThumbnailOptionIconModeKey"));
            var cgImageHandle = QLThumbnailImageCreate(allocator.Handle, url.Handle, maxThumbnailSize, dict.Handle);
            return cgImageHandle != IntPtr.Zero ? new CGImage(cgImageHandle) : null;
        }

        const string QuickLookDll = "/system/Library/frameworks/QuickLook.framework/QuickLook";

        [DllImport(QuickLookDll)]
        private extern static IntPtr QLThumbnailImageCreate(IntPtr allocator, IntPtr url, CGSize maxThumbnailSize, IntPtr dictionary); 
    }
}


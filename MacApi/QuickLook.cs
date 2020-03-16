#if !XAMARINMAC
using System;
using System.Runtime.InteropServices;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;

namespace MacApi.QuickLook
{
    public static partial class QLThumbnailImage
    {
		// QuickLook.framework/Versions/A/Headers/QLThumbnailImage.h
		// note: documented as Thread-Safe
		[DllImport(Constants.QuickLookLibrary)]
		extern static /* CGImageRef */ IntPtr QLThumbnailImageCreate(/* CFAllocatorRef */ IntPtr allocator, /* CFUrlRef */ IntPtr url, CGSize maxThumbnailSize, /* CFDictionaryRef */ IntPtr options);

		public static CGImage Create(NSUrl url, CGSize maxThumbnailSize, float scaleFactor = 1, bool iconMode = false)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			NSMutableDictionary dictionary = null;

			if (scaleFactor != 1 && iconMode != false)
			{
				dictionary = new NSMutableDictionary();
				dictionary.SetValueForKey((NSNumber)scaleFactor, new NSString("kQLThumbnailOptionScaleFactorKey"));
				dictionary.SetValueForKey(new NSNumber(iconMode), new NSString("kQLThumbnailOptionIconModeKey"));
			}

			var handle = QLThumbnailImageCreate(IntPtr.Zero, url.Handle, maxThumbnailSize, dictionary == null ? IntPtr.Zero : dictionary.Handle);
			GC.KeepAlive(dictionary);
			if (handle != IntPtr.Zero)
				return new CGImage(handle);

			return null;
		}
    }
}
#endif

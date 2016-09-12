using System;
using System.Drawing;
using System.IO;
using WinApi;

#if XAMARINMAC
using AppKit;
using CoreGraphics;
using Foundation;
using CGImage = CoreGraphics.CGImage;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using CGImage = MonoMac.CoreGraphics.CGImage;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.AppKit;
using CGImage = MonoTouch.CoreGraphics.CGImage;
#endif

namespace MacBridge.CoreGraphics
{
	public static class CGImageExtensions
	{
		public static Bitmap ToBitmap(this CGImage cgImage)
		{
			return new Bitmap (cgImage);
		}

		public static NSImage ToNSImage(this Image image)
		{
			var stream = new MemoryStream();
			image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

			var data = NSData.FromArray(stream.ToArray());
			return new NSImage(data);
		}
	}

    public static class WinApiExtensions 
    {
        public static CGPoint ToCGPoint(this POINT p)
        {
            return new CGPoint(p.X, p.Y);
        }
    }
}


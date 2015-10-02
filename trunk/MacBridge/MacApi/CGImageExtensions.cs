using System;
using System.Drawing;
using MonoMac.CoreGraphics;
using WinApi;

namespace MacBridge.CoreGraphics
{
	public static class CGImageExtensions
	{
		public static Bitmap ToBitmap(this CGImage cgImage)
		{
			return new Bitmap (cgImage);
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


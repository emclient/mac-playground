using System;
using System.Drawing;
using MonoMac.CoreGraphics;

namespace MacBridge.CoreGraphics
{
	public static class CGImageExtensions
	{
		public static Bitmap ToBitmap(this CGImage cgImage)
		{
			return new Bitmap (cgImage);
		}
	}
}


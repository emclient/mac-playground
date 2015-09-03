using System;
using NUnit.Framework;
using MonoMac.CoreGraphics;
using MacBridge.CoreGraphics;
using System.Diagnostics;

namespace MacBridge.Test
{
	[TestFixture ()]
	public class Test
	{
		[SetUp]
		public void Setup()
		{
			Trace.Listeners.Add (new ConsoleTraceListener (true));
		}

		[Test ()]
		public void TestCreateBitmapFromCGImage ()
		{
			// Load CGImage
			var path =  @"../../data/SetupLogo.jpg";
			Debug.WriteLine (System.IO.Path.GetFullPath (path));
			var provider = new CGDataProvider (path);
			var cgImage = CGImage.FromJPEG(provider, null, false, CGColorRenderingIntent.Default);

			// Convert it to System.Drawing.Bitmap using Bitmap's internal constructor
			var bitmap = cgImage.ToBitmap ();

			Assert.AreEqual (cgImage.Width, bitmap.Width);
		}
	}
}


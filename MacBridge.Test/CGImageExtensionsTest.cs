using System;
using System.Diagnostics;
using System.Drawing.Mac;
using NUnit.Framework;
#if XAMARINMAC
using CoreGraphics;
#elif MONOMAC
using MonoMac.CoreGraphics;
#endif


namespace MacBridge.Test
{
	[TestFixture ()]
	public class CGImageExtensionsTest
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


using System;
using System.Diagnostics;
using ObjCRuntime;

#if XAMARINMAC
using Foundation;
using AppKit;
using System.Windows.Forms.CocoaInternal;
using CoreGraphics;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Windows.Forms.CocoaInternal;
using MonoMac.CoreGraphics;
using nint = System.Int32;
using nfloat = System.Single;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	public class Troubleshooter
	{
		const string Separator = "----------------------------------------------------------------";

		public static void Initialize()
		{
			var args = NSProcessInfo.ProcessInfo.Arguments;

			if (args.Contains("-doNotThrowOnInitFailure"))
				ObjCRuntime.Class.ThrowOnInitFailure = false;

			// Troubleshooting NSUrl crashes
			if (args.Contains("-wrapNSUrlInitWithString"))
				urlSwizzle = new Swizzle<InitWithStringDelegate>(new Class("NSURL"), Selector.GetHandle("initWithString:"), NSUrlInitWithString);
				//urlSwizzle = new Swizzle<InitWithStringDelegate>(typeof(NSUrl), "initWithString:", NSUrlInitWithString);

			if (args.Contains("-wrapNSDictionaryInitWithObjectsForKeysCount"))
				initDictSwizzle = new Swizzle<InitDictWithObjectsForKeysCountDelegate>(typeof(NSDictionary), "dictionaryWithObjects:forKeys:count:", InitDictWithObjectsForKeysCount, true);

			//-[WebCoreResourceHandleAsOperationQueueDelegate connection:willSendRequest:redirectResponse:] + 53
			if (args.Contains("-wrapConnectionWillSendRequestRedirectResponse"))
				connectionSwizzle = new Swizzle<ConnectionWillSendRequestRedirectResponseDelegate>(new Class("WebCoreResourceHandleAsOperationQueueDelegate"), Selector.GetHandle("connection:willSendRequest:redirectResponse:"), ConnectionWillSendRequestRedirectResponse);
		}

		// NSUrlInitWithString --------------

		delegate IntPtr InitWithStringDelegate(IntPtr @this, IntPtr @selector, IntPtr @string);
		static Swizzle<InitWithStringDelegate> urlSwizzle;

		static IntPtr NSUrlInitWithString(IntPtr @this, IntPtr @selector, IntPtr @string)
		{
			var o = ObjCRuntime.Runtime.GetNSObject(@string);
			Console.WriteLine(Separator);
			Console.WriteLine($"# NSUrl({o.ToStr()})");

			var result = IntPtr.Zero;
			using (var unswizzle = urlSwizzle.Restore())
				result = unswizzle.Delegate(@this, @selector, @string);

			if (result == IntPtr.Zero)
			{
				Console.WriteLine($"initWithString: failed!");
				Console.WriteLine(new StackTrace());
				return new NSUrl("").Handle;
			}

			return result;
		}

		// [__NSPlaceholderDictionary initWithObjects:forKeys:count:] "attempt to insert nil object from objects[0]"
		// [NSDictionary dictionaryWithObjects:forKeys:count:]

		public delegate IntPtr InitDictWithObjectsForKeysCountDelegate(IntPtr @this, IntPtr selector, IntPtr a, IntPtr b, Int32 count);
		static Swizzle<InitDictWithObjectsForKeysCountDelegate> initDictSwizzle;

		public static IntPtr InitDictWithObjectsForKeysCount(IntPtr @this, IntPtr selector, IntPtr objects, IntPtr keys, Int32 count)
		{
			Console.WriteLine(Separator);
			Console.WriteLine($"# dictionaryWithObjects:{objects}, forKeys:{keys}, count:{count}");
			Console.WriteLine(new StackTrace());

			var result = IntPtr.Zero;
			using (var orig = initDictSwizzle.Restore())
				result = orig.Delegate(@this, selector, objects, keys, count);

			return result;
		}

		// -------------

		public delegate IntPtr ConnectionWillSendRequestRedirectResponseDelegate(IntPtr @this, IntPtr selector, IntPtr a, IntPtr b, IntPtr c);
		static Swizzle<ConnectionWillSendRequestRedirectResponseDelegate> connectionSwizzle;

		public static IntPtr ConnectionWillSendRequestRedirectResponse(IntPtr @this, IntPtr selector, IntPtr connectionPtr, IntPtr requestPtr, IntPtr redirectResponsePtr)
		{
			Console.WriteLine(Separator);
			Console.WriteLine($"# connection:willSendRequest:redirectResponse:");
			Console.WriteLine(new StackTrace());

			var connection = ObjCRuntime.Runtime.GetNSObject<NSUrlConnection>(connectionPtr);
			var request = ObjCRuntime.Runtime.GetNSObject<NSUrlRequest>(requestPtr);
			var response = ObjCRuntime.Runtime.GetNSObject<NSUrlResponse>(redirectResponsePtr);

			Console.WriteLine($"request.Url={request.Url.ToStr()}");
			Console.WriteLine($"request.MainDocumentURL={request.MainDocumentURL.ToStr()}");
			if (response != null)
			{
				Console.WriteLine($"redirectResponse.Url={response.Url.ToStr()}");
				Console.WriteLine($"redirectResponse.MimeType={response.MimeType}");
			}

			var result = IntPtr.Zero;
			using (var orig = connectionSwizzle.Restore())
				result = orig.Delegate(@this, selector, connectionPtr, requestPtr, redirectResponsePtr);

			if (result == IntPtr.Zero)
				Console.WriteLine("response:<null>");
			else if (result != requestPtr)
			{
				var resp = ObjCRuntime.Runtime.GetNSObject<NSUrlRequest>(result);
				if (resp != null)
				{
					Console.WriteLine($"response.Url={request.Url.ToStr()}");
					Console.WriteLine($"response.MainDocumentURL={request.MainDocumentURL.ToStr()}");
				}
			}


			return result;
		}
	}

	static class TroubleshooterExtensions
	{
		public static bool Contains(this string[] @this, string value)
		{
			return -1 != Array.IndexOf(@this, value);
		}

		public static string ToStr(this object @this)
		{ 
			return @this != null ? $"\"{@this.ToString()}\"" : "<null>";
		}
	}
}

using System;
using System.Diagnostics;
using ObjCRuntime;
using Foundation;
using AppKit;
using System.Windows.Forms.CocoaInternal;
using CoreGraphics;

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

			// setControlSizeSwizzle = new Swizzle<SetControlSizeDelegate>(new Class("NSScroller"), Selector.GetHandle("setControlSize:"), SetControlSize);
			// initWithFrameSwizzle = new Swizzle<InitWithFrameDelegate>(new Class("NSScroller"), Selector.GetHandle("initWithFrame:"), InitWithFrame);
			// addSubviewSwizzle = new Swizzle<AddSubviewDelegate>(new Class("NSView"), Selector.GetHandle("addSubview:"), AddSubview);
		}

		// NSView addSubview: --------------

		delegate void AddSubviewDelegate(IntPtr @this, IntPtr @selector, IntPtr @view);
		static Swizzle<AddSubviewDelegate> addSubviewSwizzle;

		static void AddSubview(IntPtr thisPtr, IntPtr selectorPtr, IntPtr viewPtr)
		{
			var @this = ObjCRuntime.Runtime.GetNSObject(@thisPtr);
			var view = ObjCRuntime.Runtime.GetNSObject(@viewPtr);

			if (view is NSScroller scroller)
			{
				Console.WriteLine(Separator);
				Console.WriteLine("AddSubview(NSScroller)");
				Console.WriteLine($" - {@this.GetType().Name}");
			}

			using (var unswizzle = addSubviewSwizzle.Restore())
				unswizzle.Delegate(@thisPtr, @selectorPtr, viewPtr);

			// if (ObjCRuntime.Runtime.GetNSObject(@this) is NSScroller scroller)
			// 	scroller.ControlSize = NSControlSize.Large;
		}
		
		// NSScroller initWithFrame --------------

		delegate IntPtr InitWithFrameDelegate(IntPtr @this, IntPtr @selector, CGRect rect);
		static Swizzle<InitWithFrameDelegate> initWithFrameSwizzle;

		static IntPtr InitWithFrame(IntPtr @this, IntPtr @selector, CGRect frame)
		{
			Console.WriteLine(Separator);
			Console.WriteLine($"#NSScroller.InitWithFrame({frame})");

			var result = IntPtr.Zero;
			using (var unswizzle = initWithFrameSwizzle.Restore())
				result = unswizzle.Delegate(@this, @selector, frame);

			if (ObjCRuntime.Runtime.GetNSObject(@this) is NSScroller scroller)
			 	scroller.ControlSize = NSControlSize.Small;

			return result;
		}

		// NSScroller setControlSize --------------

		delegate void SetControlSizeDelegate(IntPtr @this, IntPtr @selector, nint style);
		static Swizzle<SetControlSizeDelegate> setControlSizeSwizzle;

		static void SetControlSize(IntPtr @this, IntPtr @selector, nint size)
		{
			var nsize = (AppKit.NSControlSize)size;
			Console.WriteLine(Separator);
			Console.WriteLine($"# SetControlSize({nsize})");

			using (var unswizzle = setControlSizeSwizzle.Restore())
				unswizzle.Delegate(@this, @selector, size);
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

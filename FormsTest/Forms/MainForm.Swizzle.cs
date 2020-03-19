# if MAC
using System;
using System.Windows.Forms;
using Foundation;
using ObjCRuntime;

namespace FormsTest
{
	public partial class MainForm
	{
		// Swizzling instance method

		public delegate IntPtr NativeEventHandler(IntPtr @this, IntPtr selector, IntPtr a, IntPtr b);
		Swizzle<NativeEventHandler> swizzle;

		void SwizzleDict()
		{
			var selName = "initWithObjects:forKeys:";
			using (swizzle = new Swizzle<NativeEventHandler>(typeof(NSDictionary), selName, HijackedInitWithObjectsForKeys))
			{
				var type = typeof(NSDictionary);
				var d = new NSDictionary((NSString)"key1", (NSString)"obj1", (NSString)"key2", (NSString)"obj2");
			}
		}

		public IntPtr HijackedInitWithObjectsForKeys(IntPtr @this, IntPtr selector, IntPtr objects, IntPtr keys)
		{
			Console.WriteLine("HijackedInitWithObjectsForKeys");
			var objArray = Runtime.GetNSObject(objects);
			var keyArray = Runtime.GetNSObject(keys);

			var result = IntPtr.Zero;
			using (var orig = swizzle.Restore())
				result = orig.Delegate(@this, selector, objects, keys);

			return result;
		}

		// Swizzling class method

		public delegate IntPtr NativeEventHandlerCls(IntPtr @this, IntPtr selector, IntPtr a, IntPtr b, Int32 count);
		Swizzle<NativeEventHandlerCls> swizzleCls;

		void SwizzleCls()
		{
			var selName = "dictionaryWithObjects:forKeys:count:";
			using (swizzleCls = new Swizzle<NativeEventHandlerCls>(typeof(NSDictionary), selName, HijackedDictionaryWithObjectsForKeysCount, true))
			{
				var classHandle = Class.GetHandle(typeof(NSDictionary));
				var keys = IntPtr.Zero;
				var objects = IntPtr.Zero;
				var sel = new Selector(selName);
				var instanceHandle = MacApi.LibObjc.IntPtr_objc_msgSend_IntPtr_IntPtr_Int32(classHandle, sel.Handle, IntPtr.Zero, IntPtr.Zero, 0);
			}
		}

		public IntPtr HijackedDictionaryWithObjectsForKeysCount(IntPtr @this, IntPtr selector, IntPtr objects, IntPtr keys, Int32 count)
		{
			Console.WriteLine("HijackedDictionaryWithObjectsForKeysCount");

			var result = IntPtr.Zero;
			using (var orig = swizzleCls.Restore())
				result = orig.Delegate(@this, selector, objects, keys, count);

			return result;
		}
	}
}
#endif
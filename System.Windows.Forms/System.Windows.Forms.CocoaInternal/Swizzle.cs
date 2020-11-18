using System.Runtime.InteropServices;
using System.Windows.Forms.Mac;
using MacApi;


#if XAMARINMAC
using ObjCRuntime;
using Foundation;
#elif MONOMAC
using MonoMac;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
#endif

namespace System.Windows.Forms
{
	// This is a helper class that lets you redirect any ObjC instance method into your C# delegate.
	// From within your delegate, you can invoke the original implementation.
	//
	// Sample: Hijacking 'mouseDown:' method
	//
	//	public delegate void NativeEventHandler(IntPtr a, IntPtr b, IntPtr c);
	//
	//	class MyClass
	//	{ 
	//		Swizzle<NativeEventHandler> swizzle;
	//
	//		public MyClass()
	//		{
	//			swizzle = new Swizzle<NativeEventHandler>(typeof(NSView), "mouseDown:", HijackedMouseDown);
	//		}
	//
	//		void HijackedMouseDown(IntPtr @this, IntPtr selector, IntPtr theEvent)
	//		{
	//			Console.WriteLine("MouseDown(view:{0}, event:{1})", @this, new NSEvent(theEvent));
	//
	//			using (var orig = swizzle.Restore())
	//				orig.Delegate(@this, selector, theEvent);
	//		}
	//	}

	public class Swizzle<TDelegate> : IDisposable where TDelegate : class
	{
		protected IntPtr originalMethod;
		protected IntPtr originalImpl;
		protected IntPtr victimSel;
		protected IntPtr newImpl;
		protected bool isClassMethod;

		protected TDelegate dlg; // Your delegate

		public Swizzle(Class victim, IntPtr selector, TDelegate del, bool isClassMethod = false)
		{
		    dlg = del;
			victimSel = selector;

			originalMethod = isClassMethod ? LibObjc.class_getClassMethod(victim.Handle, victimSel) : LibObjc.class_getInstanceMethod(victim.Handle, victimSel);
			originalImpl = LibObjc.method_getImplementation(originalMethod);

			newImpl = Marshal.GetFunctionPointerForDelegate(del as System.Delegate);
			LibObjc.method_setImplementation(originalMethod, newImpl);
		}

		public Swizzle(Type victim, string selector, TDelegate del, bool isClassMethod = false)
			: this(victim.GetClass(), Selector.GetHandle(selector), del, isClassMethod)
		{
		}

		public Swizzle(NSObject victim, string selector, TDelegate del, bool isClassMethod = false)
			: this(victim.Class, Selector.GetHandle(selector), del, isClassMethod)
		{
		}

		public Unswizzle Restore()
		{
			return new Unswizzle(this);
		}

		public virtual void Dispose()
		{
			LibObjc.method_setImplementation(originalMethod, originalImpl);
		}

		// Use this class to call the original implementation
		public class Unswizzle : IDisposable
		{
			Swizzle<TDelegate> swizzle;
			public Unswizzle(Swizzle<TDelegate> swizzle)
			{
				this.swizzle = swizzle;
				LibObjc.method_setImplementation(swizzle.originalMethod, swizzle.originalImpl);
			}

			public TDelegate Delegate
			{
				get 
				{
					return Marshal.GetDelegateForFunctionPointer(swizzle.originalImpl, swizzle.dlg.GetType()) as TDelegate;
				}
			}

			public void Dispose()
			{
				LibObjc.method_setImplementation(swizzle.originalMethod, swizzle.newImpl);
				swizzle = null;
			}
		}
	}
}


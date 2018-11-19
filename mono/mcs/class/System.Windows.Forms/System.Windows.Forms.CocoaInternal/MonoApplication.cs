#if XAMARINMAC
using AppKit;
using Foundation;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	[Register("MonoApplication")]
	internal class MonoApplication : NSApplication
	{
		public static MonoApplication CreateShared()
		{
			var classHandle = Class.GetHandle(typeof(MonoApplication));
			var selector = new Selector("sharedApplication");
			var instanceHandle = MacApi.LibObjc.IntPtr_objc_msgSend(classHandle, selector.Handle);
			return new MonoApplication(instanceHandle);
		}

		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		protected internal MonoApplication(IntPtr handle) : base(handle)
		{
		}

		// This is necessary for proper window behavior in case we do not use "run" method, which is our case
		[Export("isRunning")]
		public bool IsRunning
		{
			get { return Application.MessageLoop; }
		}
	}
}

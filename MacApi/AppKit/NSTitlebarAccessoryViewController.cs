#if MONOMAC using System; using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime; 
namespace MacApi.AppKit { 	[Register("NSTitlebarAccessoryViewController", true)] 	public class NSTitlebarAccessoryViewController : NSViewController 	{ 		static readonly IntPtr classHandle = Class.GetHandle("NSTitlebarAccessoryViewController"); 		public override IntPtr ClassHandle { get { return classHandle; } }  		static readonly IntPtr selLayoutAttributeHandle = Selector.GetHandle("layoutAttribute"); 		static readonly IntPtr selSetLayoutAttributeHandle = Selector.GetHandle("setLayoutAttribute:");  		public virtual NSLayoutAttribute LayoutAttribute
		{ 			[Export("layoutAttribute")] 			get { return (NSLayoutAttribute)Messaging.int_objc_msgSend(Handle, selLayoutAttributeHandle); }
			[Export("setLayoutAttribute:")]
			set { Messaging.void_objc_msgSend_int(Handle, selSetLayoutAttributeHandle, (int)value); } 		}

		static readonly IntPtr selFullScreenMinHeight = Selector.GetHandle("fullScreenMinHeight");
		static readonly IntPtr selSetFullScreenMinHeight = Selector.GetHandle("setFullScreenMinHeight:");
 		public virtual float FullScreenMinHeight
		{
			[Export("fullScreenMinHeight")] 			get { return Messaging.float_objc_msgSend(Handle, selFullScreenMinHeight); } 			[Export("setFullScreenMinHeight:")] 			set { Messaging.void_objc_msgSend_float(Handle, selSetFullScreenMinHeight, value); }
		}
	}  	public static class NSTitleBarAccesoryViewControllerExtension 	{ 		static readonly IntPtr selAddTitlebarAccessoryViewController = Selector.GetHandle("addTitlebarAccessoryViewController:");

		[Export("addTitlebarAccessoryViewController:")] 		public static void AddTitlebarAccessoryViewController(this NSWindow self, NSTitlebarAccessoryViewController controller) 		{
			Messaging.void_objc_msgSend_IntPtr(self.Handle, selAddTitlebarAccessoryViewController, controller.Handle); 		} 	} }  #endif
using System;
using Foundation;
using ObjCRuntime;
using System.Runtime.InteropServices;

namespace MacApi.Foundation
{
	// https://developer.apple.com/documentation/foundation/nsbackgroundactivityscheduler?language=objc

	[Register("BackgroundActivityScheduler")]
	public class NSBackgroundActivityScheduler : NSObject
	{
		readonly static string className = "NSBackgroundActivityScheduler";
		readonly static IntPtr selAlloc = Selector.GetHandle("alloc");
		readonly static IntPtr selInitWithIdentifier = Selector.GetHandle("initWithIdentifier:");
		readonly static IntPtr selRepeats = Selector.GetHandle("repeats");
		readonly static IntPtr selSetRepeats = Selector.GetHandle("setRepeats:");
		readonly static IntPtr selInterval = Selector.GetHandle("interval");
		readonly static IntPtr selSetInterval = Selector.GetHandle("setInterval:");
		readonly static IntPtr selTolerance = Selector.GetHandle("tolerance");
		readonly static IntPtr selSetTolerance = Selector.GetHandle("setTolerance:");
		readonly static IntPtr selQualityOfService = Selector.GetHandle("qualityOfService");
		readonly static IntPtr selSetQualityOfService = Selector.GetHandle("setQualityOfService:");
		readonly static IntPtr selScheduleWithBlock = Selector.GetHandle("scheduleWithBlock:");
		readonly static IntPtr selShouldDefer = Selector.GetHandle("shouldDefer");
		readonly static IntPtr selInvalidate = Selector.GetHandle("invalidate");

		public enum Result
		{
			Finished = 1,
			Deferred = 2
		}

		public enum QoS
		{
			UserInteractive = 33,
			UserInitiated = 25,
			Utility = 17,
			Background = 9,
			Default = -1
		}

		public NSBackgroundActivityScheduler(string identifier)
			: base(LibObjc.IntPtr_objc_msgSend_IntPtr(
				Class.GetHandle(className), selAlloc, new NSString(identifier).Handle))
		{
			LibObjc.void_objc_msgSend_IntPtr(Handle, selInitWithIdentifier, new NSString(identifier).Handle);
		}

		public virtual bool Repeats
		{
			get { return LibObjc.bool_objc_msgSend(Handle, selRepeats); }
			set { LibObjc.void_objc_msgSend_Bool(Handle, selSetRepeats, value); }
		}

		public virtual double Interval
		{
			get { return LibObjc.double_objc_msgSend(Handle, selInterval); }
			set { LibObjc.void_objc_msgSend_Double(Handle, selSetInterval, value); }
		}

		public virtual double Tolerance
		{
			get { return LibObjc.double_objc_msgSend(Handle, selTolerance); }
			set { LibObjc.void_objc_msgSend_Double(Handle, selSetTolerance, value); }
		}

		public virtual QoS QualityOfService
		{
			get { return (QoS)LibObjc.IntPtr_objc_msgSend(Handle, selQualityOfService); }
			set { LibObjc.void_objc_msgSend_IntPtr(Handle, selSetQualityOfService, (IntPtr)value); }
		}

		public virtual bool ShouldDefer
		{
			get { return LibObjc.bool_objc_msgSend(Handle, selShouldDefer); }
		}

		public virtual void Invalidate()
		{
			LibObjc.void_objc_msgSend(Handle, selInvalidate);
		}

		public delegate void CompletionHandler(Result result);
		public delegate void ScheduleBlock(CompletionHandler completion);

		public virtual void Schedule(ScheduleBlock block)
		{
			if (block == null)
				throw new ArgumentNullException(nameof(block));

			var blocklit = new BlockLiteral();
			blocklit.SetupBlock(scheduleBlockProxy, block);
			try
			{
				unsafe
				{
					var blockref = &blocklit;
					LibObjc.void_objc_msgSend_IntPtr(this.Handle, selScheduleWithBlock, (IntPtr)blockref);
				}
			}
			finally
			{
				blocklit.CleanupBlock();
			}
		}

		delegate void ScheduleBlockProxy(IntPtr blockLiteral, IntPtr callback);
		static readonly ScheduleBlockProxy scheduleBlockProxy = ScheduleBlockWrapper;

		[MonoPInvokeCallback(typeof(ScheduleBlockProxy))]
		static void ScheduleBlockWrapper(IntPtr blockptr, IntPtr callback)
		{
			var completion = (CompletionHandler) ((Result result) => {
				InvokeNativeCompletionBlock(callback, (int)result);
			});

			BlockLiteral.GetTarget<ScheduleBlock>(blockptr)?.Invoke(completion);
		}

		// https://forums.xamarin.com/discussion/40246/binding-methods-with-block-parameters

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void NativeCompletionHandlerDelegate(IntPtr block, int arg);

		static void InvokeNativeCompletionBlock(IntPtr block, int value)
		{
			unsafe
			{
				var literal = (BlockLiteral*)block;
				var copy = LibObjc._Block_copy(block);
				var invoker = literal->GetDelegateForBlock<NativeCompletionHandlerDelegate>();
				invoker(copy, value);
				LibObjc._Block_release(copy);
			}
		}
	}
}

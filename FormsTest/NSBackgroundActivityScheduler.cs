using System;
using Foundation;
using ObjCRuntime;
using System.Windows.Forms.Mac;
using MacApi;

public class NSBackgroundActivityScheduler : NSObject
{
	readonly static string className = "NSBackgroundActivityScheduler";
	readonly static Selector selAlloc = new Selector("alloc");
	readonly static Selector selInitWithIdentifier = new Selector("initWithIdentifier:");
	readonly static Selector selRepeats = new Selector("repeats");
	readonly static Selector selSetRepeats = new Selector("setRepeats:");
	readonly static Selector selInterval = new Selector("interval");
	readonly static Selector selSetInterval = new Selector("setInterval:");
	readonly static Selector selScheduleWithBlock = new Selector("scheduleWithBlock:");

	//[Export("initWithIdentifier:")]
	public NSBackgroundActivityScheduler(string identifier)
		: base(LibObjc.IntPtr_objc_msgSend_IntPtr(
			new Class(className).Handle, selAlloc.Handle, new NSString(identifier).Handle))
	{
		LibObjc.void_objc_msgSend_IntPtr(Handle, selInitWithIdentifier.Handle, new NSString(identifier).Handle);
	}

	public bool Repeats
	{
		get { return LibObjc.bool_objc_msgSend(Handle, selRepeats.Handle); }
		set { LibObjc.void_objc_msgSend_Bool(Handle, selSetRepeats.Handle, value); }
	}

	public double Interval
	{
		get { return LibObjc.double_objc_msgSend(Handle, selInterval.Handle); }
		set { LibObjc.void_objc_msgSend_Double(Handle, selSetInterval.Handle, value); }
	}

	public delegate void ScheduleCallback(IntPtr completionHandler); 
	public void Schedule(ScheduleCallback callback)
	{
		if (callback == null)
			throw new ArgumentNullException("callback");
		unsafe
		{
			BlockLiteral* block_ptr_handler;
			BlockLiteral block_handler;
			block_handler = new BlockLiteral();
			block_ptr_handler = &block_handler;
			block_handler.SetupBlockUnsafe(static_handler, callback);

			//NativePInvoke((void*)block_ptr_handler);
			SetupHandler(block_ptr_handler);
			block_ptr_handler->CleanupBlock();
		}
	}

	public unsafe delegate void ScheduleCallbackProxy(BlockLiteral* blockLiteral, IntPtr arg1); 	static unsafe readonly ScheduleCallbackProxy static_handler = TrampolineHandler;

	[MonoPInvokeCallback(typeof(ScheduleCallbackProxy))]
	static unsafe void TrampolineHandler(BlockLiteral* block, IntPtr completionHandlerPtr)
	{
		var callback = (ScheduleCallback)(block->Target);
		if (callback != null)
			callback(completionHandlerPtr);
	}  	unsafe void SetupHandler(BlockLiteral* block_ptr_handler)
	{
		LibObjc.IntPtr_objc_msgSend_IntPtr(Handle, selScheduleWithBlock.Handle, new IntPtr(block_ptr_handler));
	} }

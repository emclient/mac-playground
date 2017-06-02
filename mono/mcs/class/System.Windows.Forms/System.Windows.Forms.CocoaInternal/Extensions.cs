#if MONOMAC
using ObjCRuntime = MonoMac.ObjCRuntime;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using System.Drawing;
#elif XAMARINMAC
using System;
using System.Runtime.InteropServices;
using AppKit;
using Foundation;
#endif

namespace System.Windows.Forms.Mac
{
	public static class Extensions
	{
		static DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static NSDate ToNSDate(this DateTime datetime)
		{
			return NSDate.FromTimeIntervalSinceReferenceDate((datetime.ToUniversalTime() - reference).TotalSeconds);
		}

		public static DateTime ToDateTime(this NSDate date)
		{
			return reference.AddSeconds(date.SecondsSinceReferenceDate).ToLocalTime();
		}

		public static NSWindow[] OrderedWindows(this NSApplication self)
		{
			var selector = new ObjCRuntime.Selector("orderedWindows");
			var ptr = ObjCRuntime.Messaging.IntPtr_objc_msgSend(self.Handle, selector.Handle);
			var array = NSArray.ArrayFromHandle<NSWindow>(ptr);
			return array;
		}

		public static NSDragOperation ToDragOperation(this DragDropEffects e)
		{
			var o = NSDragOperation.None;
			if ((e & DragDropEffects.Copy) != 0)
				o |= NSDragOperation.Copy;
			if ((e & DragDropEffects.Link) != 0)
				o |= NSDragOperation.Link;
			if ((e & DragDropEffects.Move) != 0)
				o |= NSDragOperation.Move;
			return o;
		}

		public static DragDropEffects ToDragDropEffects(this NSDragOperation o)
		{
			var e = DragDropEffects.None;
			if ((o & NSDragOperation.Copy) != 0)
				e |= DragDropEffects.Copy;
			if ((o & NSDragOperation.Link) != 0)
				e |= DragDropEffects.Link;
			if ((o & NSDragOperation.Move) != 0)
				e |= DragDropEffects.Move;
			return e;
		}

		public static Keys ToKeys(this NSEventModifierMask modifiers)
		{
			Keys keys = Keys.None;
			if ((NSEventModifierMask.ShiftKeyMask & modifiers) != 0) { keys |= Keys.Shift; }
			if ((NSEventModifierMask.CommandKeyMask & modifiers) != 0) { keys |= Keys.Cmd; }
			if ((NSEventModifierMask.AlternateKeyMask & modifiers) != 0) { keys |= Keys.Alt; }
			if ((NSEventModifierMask.ControlKeyMask & modifiers) != 0) { keys |= Keys.Control; }
			return keys;
		}

#if MONOMAC

		public static CGSize SizeThatFits(this NSControl self, CGSize proposedSize)
		{
			var selector = new ObjCRuntime.Selector("sizeThatFits:");
			var size = ObjCRuntime.Messaging.CGSize_objc_msgSend_CGSize(self.Handle, selector.Handle, proposedSize);
			return size;
		}

		public static NSPasteboardWriting AsPasteboardWriting(this NSObject self)
		{
			return new NSPasteboardWriting(self.Handle);
		}

		public static NSPasteboardWriting AsPasteboardWriting(this String self)
		{
			return new NSPasteboardWriting(((NSString)self).Handle);
		}
#elif XAMARINMAC
		public static INSPasteboardWriting AsPasteboardWriting(this NSObject self)
		{
			return (INSPasteboardWriting)self;
		}

		public static INSPasteboardWriting AsPasteboardWriting(this String self)
		{
			return (INSPasteboardWriting)(NSString)self;
		}
#endif
	}
}

#if XAMARINMAC

namespace ObjCRuntime {
	public static class Messaging {

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

	}
}

#endif
#if MONOMAC
using ObjCRuntime = MonoMac.ObjCRuntime;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#elif XAMARINMAC
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

#if MONOMAC
		public static CGSize SizeThatFits(this NSControl self, CGSize proposedSize)
		{
			var selector = new ObjCRuntime.Selector("sizeThatFits:");
			var size = ObjCRuntime.Messaging.CGSize_objc_msgSend_CGSize(self.Handle, selector.Handle, proposedSize);
			return size;
		}
#endif
	}
}

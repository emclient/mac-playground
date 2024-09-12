using System;
using System.Runtime.InteropServices;
using CoreFoundation;
using Foundation;
using ObjCRuntime;

namespace MacApi.CoreGraphics
{
	public class Quartz
	{
		public static bool IsScreenLocked()
		{
			var h = CGSessionCopyCurrentDictionary();
			if (h == IntPtr.Zero)
				throw new ApplicationException("Not running within Quartz GUI session");

			using (var d = Runtime.GetINativeObject<NSDictionary>(h, true))
			{
				var kCGSSessionScreenIsLocked = new CFString("CGSSessionScreenIsLocked");
				var locked = d["CGSSessionScreenIsLocked"] is NSNumber num ? num.BoolValue : false;
				return locked;
			}
		}

		[DllImport(Constants.CoreGraphicsLibrary)]
		extern public static IntPtr CGSessionCopyCurrentDictionary();
	}
}

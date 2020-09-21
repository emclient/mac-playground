using System;
using System.Runtime.InteropServices;
using MacApi.CoreFoundation;

namespace MacApi.CoreGraphics
{
	public class Quartz
	{
		public static bool IsScreenLocked()
		{
			var h = CGSessionCopyCurrentDictionary();
			if (h == IntPtr.Zero)
				throw new ApplicationException("Not running within Quartz GUI session");

			using (var d = new CFDictionary(h, true))
			{
				var kCGSSessionScreenIsLocked = new CFString("CGSSessionScreenIsLocked");
				var locked = CFDictionary.GetBooleanValue(d.Handle, kCGSSessionScreenIsLocked.Handle);
				return locked;
			}
		}

		[DllImport(Constants.CoreGraphicsLibrary)]
		extern public static IntPtr CGSessionCopyCurrentDictionary();
	}
}

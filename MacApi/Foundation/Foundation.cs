using System;
using Foundation;

namespace MacApi
{
	public static partial class FoundationStatic
	{
		internal const string Dll = "/System/Library/Frameworks/Foundation.framework/Foundation";

		public static IntPtr Handle(this string s)
		{
			return s != null ? ((NSString)s).Handle : IntPtr.Zero; 
		}
	}
}

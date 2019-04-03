using System;
using ObjCRuntime;

namespace MacApi
{
	public class Libraries
	{
		public static class CoreFoundation
		{
			public static readonly IntPtr Handle = Dlfcn.dlopen(Constants.CoreFoundationLibrary, 0);
		}
	}
}

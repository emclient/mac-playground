using System;
using System.Runtime.CompilerServices;

#if XAMARINMAC
using Foundation;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#endif

namespace MacApi.PrintCore
{
	public class PMPrintCoreBase : NSObject
	{
		internal PMPrintCoreBase()
		{
		}

		internal PMPrintCoreBase(IntPtr handle)
		{
		}

		internal PMPrintCoreBase(IntPtr handle, bool owns)
		{
		}
	}
}

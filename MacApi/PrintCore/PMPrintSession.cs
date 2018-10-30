using System;
using System.Runtime.InteropServices;
using Foundation;
using PrintCore;

namespace MacApi.PrintCore
{
	public static class PMPrintSessionExtensions
	{
		public static int SetDestination(this PMPrintSession session, PMPrintSettings settings, PMDestinationType destType, string format, NSUrl destination)
		{
			return PMSessionSetDestination(session.Handle, settings.Handle, destType, new NSString(format).Handle, destination.Handle);
		}

		#region internals

		[DllImport(Constants.PrintCoreLibrary)]
		internal static extern int PMSessionSetDestination(IntPtr printSession, IntPtr printSettings, PMDestinationType destType, IntPtr strDestFormat, IntPtr urlDestLocation);

		#endregion
	}
}

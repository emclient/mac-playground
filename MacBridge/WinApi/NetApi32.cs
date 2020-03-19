using System;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		public static int NetGetJoinInformation(string server, out IntPtr domain, out NetJoinStatus status)
		{
			// TODO: https://stackoverflow.com/questions/14904749/mac-osx-determing-whether-user-account-is-an-active-directory-user-vs-local-us/15284509#15284509
			NotImplemented(MethodBase.GetCurrentMethod());
			domain = IntPtr.Zero;
			status = NetJoinStatus.NetSetupUnknownStatus;
			return 0;
		}

		public static int NetApiBufferFree(IntPtr Buffer)
		{
			return 0;
		}

		public enum NetJoinStatus
		{
			NetSetupUnknownStatus = 0,
			NetSetupUnjoined,
			NetSetupWorkgroupName,
			NetSetupDomainName
		}
	}
}

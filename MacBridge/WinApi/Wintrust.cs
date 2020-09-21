using System;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		public static uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}
	}
}

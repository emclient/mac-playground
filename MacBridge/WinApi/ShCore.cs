using System;
using System.Reflection;

namespace WinApi
{
	public partial class Win32
	{
		public static int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			dpiX = 0;
			dpiY = 0;
			return -1;
		}
	}
}

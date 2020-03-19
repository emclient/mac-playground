using System;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		public static void DwmGetColorizationParameters(ref DWMCOLORIZATIONPARAMS parms)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public static int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int DwmIsCompositionEnabled(out bool enabled)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			enabled = false;
			return 0;
		}
	}
}

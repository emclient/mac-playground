using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		public static Int32 GetThemeColor(IntPtr hTheme, int iPartId, int iStateId, int iPropId, out COLORREF pColor)
		{
			NotImplemented(MethodBase.GetCurrentMethod());

			var c = SystemColors.ButtonFace;
			pColor.R = c.R;
			pColor.G = c.G;
			pColor.B = c.B;
			return 0;
		}

		public static IntPtr OpenThemeData(IntPtr hWnd, String classList)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return (IntPtr)0;
		}

		public static Int32 GetThemeEnumValue(IntPtr hTheme, int iPartId, int iStateId, int iPropId, out int piVal)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			piVal = 0;
			return 0;
		}

		public static Int32 GetThemeBitmap(IntPtr hTheme, int iPartId, int iStateId, int iPropId, uint dwFlags, out IntPtr phBitmap)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			phBitmap = IntPtr.Zero;
			return 0;
		}

		public static Int32 CloseThemeData(IntPtr hTheme)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static bool IsThemeActive()
		{
			return false;
		}


		public static void SetWindowTheme(IntPtr hwnd, String pszSubAppName, String pszSubIdList)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}
	}
}

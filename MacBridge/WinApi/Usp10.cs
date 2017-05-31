using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinApi
{
	public static partial class Win32
	{
		public static bool ScriptGetProperties([Out] out IntPtr ppSp, [Out] out int piNumScripts)
		{
			ppSp = IntPtr.Zero;
			piNumScripts = 0;
			NotImplemented(MethodBase.GetCurrentMethod());
			return true;
		}
	}
}

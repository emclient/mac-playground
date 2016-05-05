using System;
using System.Diagnostics;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		internal static void NotImplemented(MethodBase method, object details = null)
		{
			Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name + (details == null ? String.Empty : " (" + details.ToString() + ")"));
		}
	}
}

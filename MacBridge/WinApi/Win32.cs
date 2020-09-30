using System;
using System.Diagnostics;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{

		[Conditional("DEBUG")]
		internal static void NotImplemented(MethodBase method, object details = null)
		{
			Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name + (details == null ? String.Empty : " (" + details.ToString() + ")"));
		}

		/// <summary>
		/// Returns true iff the HRESULT is a success code.
		/// </summary>
		/// <param name="hr">HRESULT to check.</param>
		/// <returns>True iff a success code.</returns>
		public static bool SUCCEEDED(int hr)
		{
			return (0 <= hr);
		}
	}
}

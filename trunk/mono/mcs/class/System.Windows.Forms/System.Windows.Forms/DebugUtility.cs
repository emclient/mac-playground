using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	internal static class DebugUtility
	{
		internal static String ControlInfo(IntPtr handle)
		{
			var ctrl = Control.FromHandle(handle);
			return ctrl == null ? "null" : ctrl.GetType().Name + ", hwnd=" + handle + "visible=" + ctrl.Visible;
		}
	}
}


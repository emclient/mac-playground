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

		internal static String ControlInfo(MonoMac.AppKit.NSView view)
		{
			if (view == null)
				return "null";

			if (view is CocoaInternal.MonoView)
			{
				var ctrl = Control.FromHandle(view.Handle);
				if (ctrl != null)
					return ctrl.GetType().Name + ", hwnd=" + view.Handle + "visible=" + ctrl.Visible + " frame=" +view.Frame ;
			}

			return view.GetType().Name;
		}
	}
}


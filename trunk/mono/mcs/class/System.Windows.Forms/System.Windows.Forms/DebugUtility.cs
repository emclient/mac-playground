using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	internal static class DebugUtility
	{
		internal static String ControlInfo(Control ctrl)
		{
			return ctrl == null ? "null" : ctrl.GetType().Name + ", visible=" + ctrl.Visible + ", WxH=" + ctrl.Size.Width + "x" + ctrl.Size.Height;
		}

		internal static String ControlInfo(IntPtr handle)
		{
			return ControlInfo(Control.FromHandle(handle));
		}

		internal static String ControlInfo(MonoMac.AppKit.NSView view)
		{
			var ctrl = ControlFromView(view);
			if (ctrl != null)
				return ControlInfo(ctrl);
			if (view != null)
				return view.GetType().Name;
			return "null";
		}

		static String lastInfo;
		internal static void WriteInfoIfChanged(MonoMac.AppKit.NSView view)
		{
			string info = ControlInfo(view);
			if (info != lastInfo)
				Console.WriteLine(lastInfo = info);
		}

		internal static void WriteInfoIfChanged(IntPtr handle)
		{
			var c = Control.FromHandle(handle);
			string info = ControlInfo(handle);
			if (info != lastInfo)
				Console.WriteLine(lastInfo = info);
		}

		internal static Control ControlFromView(MonoMac.AppKit.NSView view)
		{
			if (view != null && view is CocoaInternal.MonoView)
				return Control.FromHandle(view.Handle);
			return null;
		}
	}
}


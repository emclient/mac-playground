using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;

#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif

namespace System.Windows.Forms
{
	public static class DebugUtility
	{
		public static bool SwitchingConversations = false;

		public static String FieldName(Control ctrl)
		{
			if (ctrl == null || !ctrl.IsHandleCreated)
				return "null";
			var field = ReverseLookupField(ctrl);
			return field?.Name ?? ctrl.FindForm()?.Name ?? "<?>";
		}

		public static String ControlInfo(Control ctrl)
		{
			if (ctrl == null || !ctrl.IsHandleCreated)
				return "null";

			NSView view = ClientViewFromControl(ctrl);
			return ctrl.GetType().Name + ", visible=" + ctrl.Visible + ", WxH=" + ctrl.Size.Width + "x" + ctrl.Size.Height
				       + ", XY=" + ctrl.Location.X + ";" + ctrl.Location.Y + ", text=\"" + ctrl.Text + ", view.IsHidden=" + view.Hidden + ", view.IsHiddenOrHasHiddenAncestor=" + view.IsHiddenOrHasHiddenAncestor
				      + "\"" + GetFieldInfo(ctrl);
		}

		public static String ControlInfo(IntPtr handle)
		{
			return ControlInfo(Control.FromHandle(handle));
		}

		public static String ControlInfo(NSView view)
		{
			var ctrl = ControlFromView(view);
			if (ctrl != null)
				return ControlInfo(ctrl);
			if (view != null)
				return view.GetType().Name;
			return "null";
		}

		static String lastInfo;
		public static void WriteInfoIfChanged(NSView view, string prefix = null)
		{
			string info = ControlInfo(view);
			if (info != lastInfo)
				Console.WriteLine((prefix ?? String.Empty) + (lastInfo = info));
		}

		public static void WriteInfoIfChanged(IntPtr handle, string prefix = null)
		{
			//var c = Control.FromHandle(handle);
			string info = ControlInfo(handle);
			if (info != lastInfo)
				Console.WriteLine((prefix ?? String.Empty) + (lastInfo = info));
		}

		public static Control ControlFromView(NSView view)
		{
			if (view != null && view is CocoaInternal.MonoView)
				return Control.FromHandle(view.Handle);
			return null;
		}

		public static NSView ClientViewFromControl(Control control)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(control.Handle);
			return (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd.ClientWindow);
		}

		public static NSView WholeViewFromControl(Control control)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(control.Handle);
			return (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
		}

		public static string GetFieldInfo(Control control, string indent = "  ", string newLine = null)
		{
			newLine = newLine ?? Environment.NewLine.ToString();

			var text = "";
			for (var c = control; c != null; c = c.Parent)
			{
				var field = ReverseLookupField(c);
				if (field == null)
					break;

				text = newLine + indent + field.Name + ":" + c.GetType().Name +  text;
			}

			var form = control.FindForm();
			if (form != null)
				text = newLine + indent + form.GetType().Name + text;

			return text;
		}

		public static FieldInfo ReverseLookupField(Control control)
		{
			for (var parent = control.Parent; parent != null; parent = parent.Parent)
			{
				var field = FindField(parent, control);
				if (field != null)
					return field;
			}
			return null;
		}

		public static FieldInfo FindField(Control parent, Control control)
		{
			if (parent == null || control == null || !parent.IsHandleCreated || !control.IsHandleCreated)
				return null;

			var type = parent.GetType();
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			foreach (var field in fields)
			{
				var value = field.GetValue(parent);
				if (value != null && Object.ReferenceEquals(value, control))
					return field;
			}

			return null;
		}
	}
}


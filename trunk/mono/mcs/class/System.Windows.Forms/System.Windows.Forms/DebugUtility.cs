﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using MonoMac.AppKit;

namespace System.Windows.Forms
{
	public static class DebugUtility
	{
		public static bool SwitchingConversations = false;

		public static String ControlInfo(Control ctrl)
		{
			if (ctrl == null)
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

		public static String ControlInfo(MonoMac.AppKit.NSView view)
		{
			var ctrl = ControlFromView(view);
			if (ctrl != null)
				return ControlInfo(ctrl);
			if (view != null)
				return view.GetType().Name;
			return "null";
		}

		static String lastInfo;
		public static void WriteInfoIfChanged(MonoMac.AppKit.NSView view)
		{
			string info = ControlInfo(view);
			if (info != lastInfo)
				Console.WriteLine(lastInfo = info);
		}

		public static void WriteInfoIfChanged(IntPtr handle)
		{
			//var c = Control.FromHandle(handle);
			string info = ControlInfo(handle);
			if (info != lastInfo)
				Console.WriteLine(lastInfo = info);
		}

		public static Control ControlFromView(MonoMac.AppKit.NSView view)
		{
			if (view != null && view is CocoaInternal.MonoView)
				return Control.FromHandle(view.Handle);
			return null;
		}

		public static NSView ClientViewFromControl(Control control)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(control.Handle);
			return (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.ClientWindow);
		}

		public static NSView WholeViewFromControl(Control control)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle(control.Handle);
			return (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(hwnd.WholeWindow);
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
			if (parent == null || control == null)
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


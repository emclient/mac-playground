using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
//using MailClient.Common.UI.Controls.ControlTextBox;
//using MailClient.Common.UI.Controls;


namespace FormsTest
{
	public static class ControlDebugUtils
	{
		public static IEnumerable<Control> ControlTreeEnumeration(this Control rootControl)
		{
			yield return rootControl;
			foreach (var childControl in rootControl.Controls.Cast<Control>().SelectMany(x => x.ControlTreeEnumeration()))
			{
				yield return childControl;
			}
		}

//		public static void ForAllControls(this Control rootControl, Action<Control> action)
//		{
//			foreach(var control in rootControl.EnumerateControlsTree()) {
//				action(control);
//			}
//		}

		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (var control in enumeration) {
				action( control );
			}
		}

		public static void DebugAll<T>(this IEnumerable<T> enumeration) where T : Control
		{
			enumeration.ForEach(x => x.MouseEnter += delegate { x.WriteControlInfo(); x.BackColor = Color.LightCyan; } );
			enumeration.ForEach(x => { var backColor = x.BackColor; x.MouseLeave += delegate { x.BackColor = backColor; }; } );
			//Console.WriteLine ("----- Initial Layout -----");
			//enumeration.ForEach(x => { /*x.WriteControlLayoutInfo();*/ x.Layout += delegate { x.WriteControlLayoutInfo(); }; } );
			//Console.WriteLine ("--------------------------");
		}

		public static void DebugAllControls(this Control rootControl)
		{
			rootControl.ControlTreeEnumeration().OfType<Control>().DebugAll();
		}

		public static void DebugAllPanels(this Control rootControl)
		{
			rootControl.ControlTreeEnumeration().OfType<Panel>().DebugAll();
		}

		public static void DebugNamedControl(this Control rootControl, string name)
		{
			rootControl.ControlTreeEnumeration().Where(x => x.Name == name).DebugAll();
		}

		public static void DebugNamedControls(this Control rootControl, string[] names)
		{
			rootControl.ControlTreeEnumeration().Where(x => names.Contains(x.Name)).DebugAll();
		}

		public static void DebugFromNamedControl(this Control rootControl, string name)
		{
			foreach (var control in rootControl.ControlTreeEnumeration().Where(x => x.Name == name)) {
				control.DebugAllControls();
			}
		}

		public static void DebugLayout<T>(this IEnumerable<T> enumeration) where T : Control
		{
			Console.WriteLine ("----- Debug Layout -----");
			enumeration.ForEach(x => { x.WriteControlLayoutInfo(); } );
			Console.WriteLine ("------------------------");
		}

		public static void DebugLayoutControls(this Control rootControl)
		{
			rootControl.ControlTreeEnumeration().DebugLayout();
		}

		public static void DebugLayoutNamedControls(this Control rootControl, string[] names)
		{
			rootControl.ControlTreeEnumeration().Where(x => names.Contains(x.Name)).DebugLayout();
		}

		public static void WriteControlTypeAndName(this Control control)
		{
			var info = control.GetType().Name + " '" + control.Name + "'";
			Console.WriteLine (info);
		}

		public static void WriteControlLocationAndSize(this Control control)
		{
			var info = " Location " + control.Location;
			Console.WriteLine (info);
			info = " Size " + control.Size;
			Console.WriteLine (info);
			info = " ClientSize " + control.ClientSize;
			Console.WriteLine (info);
//			if (control is ControlTextBox) {
//				ControlTextBox textBox = (ControlTextBox)control;
//				using (Graphics graphics = textBox.CreateGraphics ()) {
//					var stringSize = Size.Ceiling( graphics.MeasureString (/*textBox.Text*/"AǜŬŮǛbjIMpz", textBox.Font) );
//					info = " Font.Height " + textBox.Font.Height + " Font.Size " + textBox.Font.Size + " MeasureString " + stringSize.Height;
//				}
//				Console.WriteLine (info);
//			}

//			if (control.Name == "tableLayout_Participants" || control.Name == "controlDisplayContactContainer" || ( control is SafeScalingPanel && control.Parent.Name == "controlDisplayContactContainer" )) {
//				Console.WriteLine ( "     ---------- " + control.GetType().Name);
//				foreach ( Control cntl in control.Controls) {
//					WriteControlTypeAndName (cntl);
//					WriteControlLocationAndSize (cntl);
//				}
//				Console.WriteLine ("     ---------- " + control.GetType().Name);
//			}
		}

		public static void WriteControlPaddingAndMargin(this Control control)
		{
			var info = " Padding " + control.Padding;
			Console.WriteLine (info);
			info = " Margin " + control.Margin;
			Console.WriteLine (info);
		}

		public static void WriteControlInfo(this Control control)
		{
			WriteControlTypeAndName (control);
			WriteControlLocationAndSize (control);
			WriteControlPaddingAndMargin (control);
		}

		public static void WriteControlLayoutInfo(this Control control)
		{
			WriteControlTypeAndName (control);
			WriteControlLocationAndSize (control);
		}
	}
}


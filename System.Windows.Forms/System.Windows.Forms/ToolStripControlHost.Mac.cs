using System;
using System.Drawing;
using AppKit;

namespace System.Windows.Forms
{
	public partial class ToolStripControlHost
	{
		internal protected override NSMenuItem ToNSMenuItem()
		{
			var nsMenuItem = base.ToNSMenuItem();
			Control.Size = Control.GetPreferredSize(Size.Empty);
			Control.BackColor = Color.Transparent;
			nsMenuItem.View = (NSView)ObjCRuntime.Runtime.GetNSObject(Control.Handle);
			return nsMenuItem;
		}
	}
}

using System;
using System.Drawing;
#if XAMARINMAC
using AppKit;
#else
using MonoMac.AppKit;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif

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

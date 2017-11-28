using System;
using System.Drawing.Mac;
#if XAMARINMAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms
{
	public partial class ToolStripMenuItem
	{
		protected internal override NSMenuItem ToNSMenuItem()
		{
			var nsMenuItem = base.ToNSMenuItem();
			if (HasDropDownItems)
				nsMenuItem.Submenu = DropDown.ToNSMenu();
			if (nsMenuItem.Image == null && ShowMargin)
			{
				if (CheckState == CheckState.Checked)
					nsMenuItem.State = NSCellStateValue.On;
				else if (CheckState == CheckState.Indeterminate)
					nsMenuItem.State = NSCellStateValue.Mixed;
			}
			return nsMenuItem;
		}
	}
}

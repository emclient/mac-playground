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

			if (PaintCheck)
				if (CheckState == CheckState.Checked)
					nsMenuItem.State = NSCellStateValue.On;
				else if (CheckState == CheckState.Indeterminate)
					nsMenuItem.State = NSCellStateValue.Mixed;

			return nsMenuItem;
		}

		internal bool ShowCheckMargin
		{
			get { return Owner is ToolStripDropDownMenu menu && menu.ShowCheckMargin; }
		}

		internal bool ShowImageMargin
		{
			get { return Owner is ToolStripDropDownMenu menu && menu.ShowImageMargin; }
		}

		internal bool PaintCheck
		{
			get { return ShowCheckMargin || ShowImageMargin; }
		}

		internal bool PaintImage
		{
			get { return ShowImageMargin; }
		}
	}
}

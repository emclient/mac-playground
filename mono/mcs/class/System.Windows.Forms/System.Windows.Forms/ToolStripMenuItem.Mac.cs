using System;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;
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
			nsMenuItem.Submenu = HasDropDownItems ? DropDown.ToNSMenu() : null;
			nsMenuItem.State = PaintCheck ? CheckState.ToCellState() : NSCellStateValue.Off;

			CheckedChanged += (sender, e) => { nsMenuItem.State = PaintCheck ? CheckState.ToCellState() : NSCellStateValue.Off; };

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

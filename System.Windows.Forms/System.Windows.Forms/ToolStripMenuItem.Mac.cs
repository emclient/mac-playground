using System;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;
using AppKit;

namespace System.Windows.Forms
{
	public partial class ToolStripMenuItem
	{
		NSMenuItem nsMenuItem;
		protected internal override NSMenuItem ToNSMenuItem()
		{
			if (nsMenuItem == null)
			{
				nsMenuItem = base.ToNSMenuItem();
				Disposed += this_Disposed;
				CheckedChanged += this_CheckedChanged;
			}

			nsMenuItem.Submenu = HasDropDownItems ? DropDown.ToNSMenu() : null;
			nsMenuItem.State = PaintCheck ? CheckState.ToCellState() : NSCellStateValue.Off;

			if (ShortcutKeys.ToKeyEquivalentAndModifiers(out var equivalent, out var modifiers)
			|| (ShortcutKeyDisplayString?.ToKeyEquivalentAndModifiers(out equivalent, out modifiers) ?? false))
			{
				nsMenuItem.KeyEquivalent = equivalent;
				nsMenuItem.KeyEquivalentModifierMask = modifiers;
			}
			return nsMenuItem;
		}
		void this_CheckedChanged(object sender, EventArgs e)
		{
			nsMenuItem.State = PaintCheck ? CheckState.ToCellState() : NSCellStateValue.Off;
		}

		void this_Disposed(object sender, EventArgs e)
		{
			Disposed -= this_Disposed;
			CheckedChanged -= this_CheckedChanged;
			nsMenuItem = null;
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

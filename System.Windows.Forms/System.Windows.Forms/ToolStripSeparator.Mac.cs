using System;
using AppKit;

namespace System.Windows.Forms
{
	public partial class ToolStripSeparator
	{
		internal protected override NSMenuItem ToNSMenuItem()
		{
			return NSMenuItem.SeparatorItem;
		}
	}
}

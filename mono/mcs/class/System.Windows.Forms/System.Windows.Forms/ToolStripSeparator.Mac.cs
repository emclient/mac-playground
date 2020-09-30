using System;
#if XAMARINMAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

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

using System;
using System.Drawing.Mac;
#if XAMARINMAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms
{
	public partial class ToolStripItem
	{
		protected internal virtual NSMenuItem ToNSMenuItem()
		{
			var nsMenuItem = new NSMenuItem();
			if (DisplayStyle == ToolStripItemDisplayStyle.ImageAndText || DisplayStyle == ToolStripItemDisplayStyle.Text)
				nsMenuItem.Title = (Text ?? String.Empty).Replace("&", "");
			nsMenuItem.Enabled = Enabled;
			nsMenuItem.Hidden = !InternalVisible;
			if (Image != null && (DisplayStyle == ToolStripItemDisplayStyle.ImageAndText || DisplayStyle == ToolStripItemDisplayStyle.Image))
				nsMenuItem.Image = Image.ToNSImage();
			// TODO: Checked, submenu
			nsMenuItem.Activated += (sender, e) => this.OnClick(e);
			return nsMenuItem;
		}
	}
}

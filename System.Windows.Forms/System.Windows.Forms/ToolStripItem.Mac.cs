﻿using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;
using AppKit;
using CoreGraphics;

namespace System.Windows.Forms
{
	public partial class ToolStripItem
	{
		private NSMenuItem nsMenuItem;
		
		protected internal virtual NSMenuItem ToNSMenuItem()
		{
			if (nsMenuItem == null)
			{
				nsMenuItem = new NSMenuItem();

				Disposed += this_Disposed;
				VisibleChanged += this_VisibleChanged;
				EnabledChanged += this_EnabledChanged;
				TextChanged += this_TextChanged;
			}

			if (DisplayStyle == ToolStripItemDisplayStyle.ImageAndText || DisplayStyle == ToolStripItemDisplayStyle.Text)
				nsMenuItem.Title = (Text ?? String.Empty).Replace("&", "");
			nsMenuItem.Enabled = Enabled;
			nsMenuItem.Hidden = !InternalVisible;
			if (Image != null && (DisplayStyle == ToolStripItemDisplayStyle.ImageAndText || DisplayStyle == ToolStripItemDisplayStyle.Image))
			{
				var size = AdjustImageSize(new CGSize(Image.Width, Image.Width));
				var variation = GetRepresentationForDevice(Image, size.ToSDSize());
				nsMenuItem.Image = variation.ToNSImage();
				nsMenuItem.Image.Size = size;
			}
			else
			{
				nsMenuItem.Image = null;
			}

			return nsMenuItem;
		}

		void this_Disposed(object sender, EventArgs e)
		{
			Disposed -= this_Disposed;
			
			if (nsMenuItem != null)
			{
				VisibleChanged -= this_VisibleChanged;
				EnabledChanged -= this_EnabledChanged;
				TextChanged -= this_TextChanged;

				nsMenuItem.Menu?.RemoveItem(nsMenuItem);

				nsMenuItem.Dispose();
				nsMenuItem = null;
			}
		}

		void this_VisibleChanged(object sender, EventArgs e) => nsMenuItem.Hidden = !InternalVisible;
		void this_EnabledChanged(object sender, EventArgs e) => nsMenuItem.Enabled = Enabled;
		void this_TextChanged(object sender, EventArgs e) => nsMenuItem.Title = (Text ?? String.Empty).Replace("&", "");

		protected virtual CGSize AdjustImageSize(CGSize size)
		{
			// Use the size reduced by a suitable integer, if the result's height is close enough to 16.
			// Otherwise, scale 'size' so that the result's height equals to 16.

			int k = Math.Max(1, (int)(size.Height / 16));
			for (int i = k; i <= 1 + k; ++i)
				if (Math.Abs(size.Height / i - 16) <= 2)
					return new CGSize(size.Height / i, size.Width / i);

			return new CGSize(size.Width * 16 / size.Height, 16);
		}

		private Image GetRepresentationForDevice(Image image, Size size)
		{
			var dpi = Parent?.GetDeviceDpi() ?? new Size(72, 72);
			var scale = (float)dpi.Width / 72f;
			var devSize = new Size((int)(scale * size.Width), (int)(scale * size.Height));
			return image.GetRepresentation(devSize);
		}
	}
}

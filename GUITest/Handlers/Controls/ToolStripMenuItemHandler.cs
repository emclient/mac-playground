using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GUITest.Handlers.Controls
{
	class ToolStripMenuItemHandler
	{
		private static Rectangle GetCenterOfToolStripMenuItem(ToolStripMenuItem control)
		{
			return control.Owner.RectangleToScreen(control.Bounds);
		}

		public static void ThrowIfNotVisible(ToolStripItem control, string message = "Control is not visible")
		{
			if (!control.Visible)
				throw new ApplicationException(message);
		}

		public static void Click(ToolStripMenuItem control)
		{
			Mouse.Click(Mouse.ClickType.Left, GetCenterOfToolStripMenuItem, ThrowIfNotVisible, control);
		}
	}
}

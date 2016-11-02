using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using MailClient.UI.Controls;
using MailClient.UI.Controls.ControlDataGrid;
using MailClient.UI.Controls.ToolStripControls;

namespace GUITest.Handlers.Controls
{
	class ControlHandler
	{
		private static Rectangle GetCenterOfControl(Control control)
		{
			return control.RectangleToScreen(control.ClientRectangle);
		}

		public static void ThrowIfNotVisible(Control control, string message = "Control is not visible")
		{
			if (!control.Visible)
				throw new ApplicationException(message);
		}

		public static void Click(Control control)
		{
			Mouse.Click(Mouse.ClickType.Left, GetCenterOfControl, ThrowIfNotVisible, control);
		}

		public static void RightClick(Control control)
		{
			Mouse.Click(Mouse.ClickType.Right, GetCenterOfControl, ThrowIfNotVisible, control);
		}

		public static void DoubleClick(Control control)
		{
			Mouse.Click(Mouse.ClickType.DoubleClick, GetCenterOfControl, ThrowIfNotVisible, control);
		}
	}
}

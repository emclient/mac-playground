using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using MailClient.Common.UI.Controls.ControlPanelSwitcher;

namespace GUITest.Handlers.Controls
{
	public static class ControlPanelTabSwitcherHandler
	{
		private static Rectangle GetCenterOfControlPanelTabSwitcherTab(ControlPanelTabSwitcherWithTabIndex control)
		{
			Rectangle relativeTabRectangle = control.PanelTabSwitcher.GetTabRectangleForPanel(control.PanelTabSwitcher.Controls[control.TabIndex] as MailClient.Common.UI.Controls.ControlPanelSwitcher.SwitchPanel);
			return control.PanelTabSwitcher.RectangleToScreen(relativeTabRectangle);
		}

		public static void ThrowIfNotVisible(ControlPanelTabSwitcherWithTabIndex control, string message = "Control is not visible")
		{
			if (!control.PanelTabSwitcher.Visible)
				throw new ApplicationException(message);
		}

		public static void Click(ControlPanelTabSwitcherWithTabIndex control)
		{
			Mouse.Click(Mouse.ClickType.Left, GetCenterOfControlPanelTabSwitcherTab, ThrowIfNotVisible, control);
		}
	}

	public class ControlPanelTabSwitcherWithTabIndex
	{
		public ControlPanelTabSwitcher PanelTabSwitcher;
		public int TabIndex;

		public ControlPanelTabSwitcherWithTabIndex(ControlPanelTabSwitcher panelTabSwitcher, int tabIndex)
		{
			PanelTabSwitcher = panelTabSwitcher;
			TabIndex = tabIndex;
		}
	}
}

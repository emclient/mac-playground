using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using MailClient.UI.Controls;
using MailClient.UI.Controls.ControlDataGrid;
using MailClient.UI.Controls.ToolStripControls;
using MailClient.Common.UI.Controls.ControlToolStrip;
using MailClient.Common.UI.Controls.ControlTextBox;

namespace GUITest
{
	public static class Utils
	{
		private static T GetNthGenericControl<T>(Control parent, int n) where T : Control
		{
			int i = 0;
			foreach (var item in parent.Controls)
			{
				if (item is T)
				{
					i++;
					if (i == n)
					{
						return item as T;
					}
				}
			}
			return null;
		}

		public static ControlDataGrid GetDropdownControlDataGrid(ComboBoxWithDataGrid comboBox)
		{
			return GetNthGenericControl<ControlDataGrid>((DropDownPanel)((ToolStripControlHost)comboBox.PopupHelper.Items[0]).Control, 1);
		}

		public static ControlTextBox GetNthControlTextBox(Control parent, int n)
		{
			return GetNthGenericControl<ControlTextBox>(parent, n) as ControlTextBox;
		}

		public static Control GetNthRemovableControlEmbedded(Control parent, int n)
		{
			return GetNthGenericControl<RemovableControl>(parent, n).EmbeddedControl;
		}

		public static ControlToolStripButton GetNthControlToolStripButton(Control parent, int n)
		{
			return GetNthGenericControl<ControlToolStripButton>(parent, n) as ControlToolStripButton;
		}
	}
	
	public class ControlDataGridWithPosition
	{
		public ControlDataGrid dataGrid;
		public int row;

		public ControlDataGridWithPosition(ControlDataGrid dataGrid, int row)
		{
			this.dataGrid = dataGrid;
			this.row = row;
		}
	}
}

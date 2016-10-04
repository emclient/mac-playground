using System;
using System.Collections.Generic;
using System.Text;

using MailClient.UI.Controls.ControlDataGrid;

namespace GUITest
{
	public static class Utils
	{
		public static ControlDataGrid GetDropdownControlDataGrid(MailClient.UI.Controls.ComboBoxWithDataGrid comboBox)
		{
			return (ControlDataGrid)((MailClient.UI.Controls.ToolStripControls.DropDownPanel)((System.Windows.Forms.ToolStripControlHost)comboBox.PopupHelper.Items[0]).Control).Controls[0];
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

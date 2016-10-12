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
		public static ControlDataGrid GetDropdownControlDataGrid(ComboBoxWithDataGrid comboBox)
		{
			return (ControlDataGrid)((DropDownPanel)((ToolStripControlHost)comboBox.PopupHelper.Items[0]).Control).Controls[0];
		}

		public static int GetRowOfTheCategory(ControlDataGrid controlDataGrid, string category)
		{
			foreach (MailClient.UI.Categories.UICategory item in controlDataGrid.DataSource)
			{
				if (item.CategoryName.Equals(category))
				{
					return controlDataGrid.DataSource.IndexOf(item);
				}
			}
			return -1;
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

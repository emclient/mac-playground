using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using MailClient.UI.Controls;
using MailClient.UI.Controls.ControlDataGrid;
using MailClient.UI.Controls.ToolStripControls;

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
			if (controlDataGrid.DataSource?[0] is MailClient.UI.Categories.UICategory)
				foreach (MailClient.UI.Categories.UICategory item in controlDataGrid.DataSource)
					if (item.CategoryName.Equals(category))
						return controlDataGrid.DataSource.IndexOf(item);

			throw new ApplicationException();
		}

		public static int GetRowOfTheContactsList(ControlDataGrid controlDataGrid, string name)
		{
			if (controlDataGrid.DataSource?[0] is MailClient.Storage.Application.Contact.ContactItem)
				foreach (MailClient.Storage.Application.Contact.ContactItem item in controlDataGrid.DataSource)
					if (item.Name.DisplayName.Equals(name))
						return controlDataGrid.DataSource.IndexOf(item);

			throw new ApplicationException();
		}
	}
	
	public class ControlDataGridWithPosition
	{
		public ControlDataGrid DataGrid;
		public int Row;

		public ControlDataGridWithPosition(ControlDataGrid dataGrid, int row)
		{
			this.DataGrid = dataGrid;
			this.Row = row;
		}
	}
}

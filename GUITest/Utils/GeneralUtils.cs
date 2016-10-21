using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using MailClient.UI.Controls;
using MailClient.UI.Controls.ControlDataGrid;
using MailClient.UI.Controls.ToolStripControls;

namespace GUITest
{
	public static class GeneralUtils
	{
		public static ControlDataGrid GetDropdownControlDataGrid(ComboBoxWithDataGrid comboBox)
		{
			return (ControlDataGrid)((DropDownPanel)((ToolStripControlHost)comboBox.PopupHelper.Items[0]).Control).Controls[0];
		}

		public static int GetRowOf<TItem>(ControlDataGrid controlDataGrid, string itemString, Func<TItem, string> getName)
		{
			if (controlDataGrid.DataSource?[0] is TItem)
				foreach (TItem item in controlDataGrid.DataSource)
					if (getName(item).Equals(itemString))
						return controlDataGrid.DataSource.IndexOf(item);

			throw new ApplicationException("Item with provided name was not found in the data");
		}

		public static int GetRowOfTheCategory(ControlDataGrid controlDataGrid, string category)
		{
			return GetRowOf<MailClient.UI.Categories.UICategory>(controlDataGrid, category, x => x.CategoryName);
		}

		public static int GetRowOfTheContactName(ControlDataGrid controlDataGrid, string name)
		{
			return GetRowOf<MailClient.Storage.Application.Contact.ContactItem>(controlDataGrid, name, x => x.Name.DisplayName);
		}
	}
	
	public class ControlDataGridWithSourceIndex
	{
		public ControlDataGrid DataGrid;
		public int Row;

		public ControlDataGridWithSourceIndex(ControlDataGrid dataGrid, int row)
		{
			this.DataGrid = dataGrid;
			this.Row = row;
		}
	}
}

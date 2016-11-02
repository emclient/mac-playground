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
	public static class ControlDataGridHandler
	{
		private static Rectangle GetCenterOfControlDataGridRow(ControlDataGridWithSourceIndex control)
		{
			Rectangle relativeRowRectangle = control.DataGrid.GetRowBoundaries(control.DataGrid.GetRowFromDataSourceIndex(control.Row, true));
			return control.DataGrid.RectangleToScreen(relativeRowRectangle);
		}

		public static void ThrowIfNotVisible(ControlDataGridWithSourceIndex control, string message = "Control is not visible")
		{
			if (!control.DataGrid.Visible)
				throw new ApplicationException(message);
		}

		public static void Click(ControlDataGrid control, int row)
		{
			Mouse.Click(Mouse.ClickType.Left, GetCenterOfControlDataGridRow, ThrowIfNotVisible, new ControlDataGridWithSourceIndex(control, row));
		}

		public static void RightClick(ControlDataGrid control, int row)
		{
			Mouse.Click(Mouse.ClickType.Right, GetCenterOfControlDataGridRow, ThrowIfNotVisible, new ControlDataGridWithSourceIndex(control, row));
		}

		public static void DoubleClick(ControlDataGrid control, int row)
		{
			Mouse.Click(Mouse.ClickType.DoubleClick, GetCenterOfControlDataGridRow, ThrowIfNotVisible, new ControlDataGridWithSourceIndex(control, row));
		}

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

		public static int GetRowOfTheContactName(ControlDataGrid controlDataGrid, string displayName)
		{
			return GetRowOf<MailClient.Storage.Application.Contact.ContactItem>(controlDataGrid, displayName, x => x.Name.DisplayName);
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

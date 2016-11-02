using System;
using System.Collections.Generic;
using System.Text;

using GUITest.Handlers.Controls;

namespace GUITest.Handlers.Forms
{
	public static class FormMainHandler
	{
		internal static void SwitchToContactsModule()
		{
			ControlHandler.Click(UI.TryGetMember("formMain.leftSpine1.controlSidebarBoxContacts"));
		}

		internal static void AddNewItemInCurrentModule()
		{
			ControlHandler.Click(UI.TryGetMember("formMain.stripButton_New"));
		}

		internal static void OpenContactDetailFromContactsList(string contactDisplayName)
		{
			var controlContactsDataGrid = (UI.TryGetSubcontrol(UI.TryGetMember("formMain.panelContactsList"), "controlContacts") as MailClient.UI.Controls.ControlContacts).DataGrid;
			ControlDataGridHandler.DoubleClick(controlContactsDataGrid, ControlDataGridHandler.GetRowOfTheContactName(controlContactsDataGrid, contactDisplayName));
		}
	}
}

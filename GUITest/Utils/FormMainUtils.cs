using System;
using System.Collections.Generic;
using System.Text;

namespace GUITest
{
	public static class FormMainUtils
	{
		internal static void SwitchToContactsModule()
		{
			UI.Mouse.Click(UI.TryGetMember("formMain.leftSpine1.controlSidebarBoxContacts"));
		}

		internal static void AddNewItemInCurrentModule()
		{
			UI.Mouse.Click(UI.TryGetMember("formMain.stripButton_New"));
		}

		internal static void OpenContactDetailFromContactsList(string contactDisplayName)
		{
			var controlContactsDataGrid = (UI.TryGetSubcontrol(UI.TryGetMember("formMain.panelContactsList"), "controlContacts") as MailClient.UI.Controls.ControlContacts).DataGrid;
			UI.Mouse.DoubleClick(controlContactsDataGrid, GeneralUtils.GetRowOfTheContactName(controlContactsDataGrid, contactDisplayName));
		}
	}
}

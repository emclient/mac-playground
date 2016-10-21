using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using MailClient.UI.Controls;

namespace GUITest
{
	public static class FormContactUtils
	{
		internal static void AddNewEmail(ContactMailType type, string address, string displayAs = null)
		{
			UI.Mouse.Click(UI.TryGetMember("formContact.toolStripButton_AddEmail"));
			switch (type)
			{
				default:
				case ContactMailType.Email:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_AddEmail_Email"));
					break;
				case ContactMailType.Work:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_AddEmail_Work"));
					break;
				case ContactMailType.Home:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_AddEmail_Home"));
					break;
			}
			var contactInfoPanelControls = UI.TryGetMember("formContact.tableLayoutPanel_Overview_Left").Controls;
			for (int i = contactInfoPanelControls.Count - 1; i >= 0; i--)
			{
				if (contactInfoPanelControls[i].Name.Equals("text_Email_Email"))
				{
					UI.Mouse.Click(contactInfoPanelControls[i]);
					UI.Type(address);
					if (displayAs != null)
					{
						UI.Mouse.Click(contactInfoPanelControls[i + 3]);
						UI.Mouse.Click(contactInfoPanelControls[i + 2]);
						UI.Type(displayAs);
					}
					break;
				}
			}
		}

		internal static void AddNewTelephone(ContactPhoneType type, string number)
		{
			UI.Mouse.Click(UI.TryGetMember("formContact.toolStripButton_AddPhone"));
			switch (type)
			{
				case ContactPhoneType.Work:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Work"));
					break;
				case ContactPhoneType.Fax:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Fax"));
					break;
				case ContactPhoneType.Car:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Car"));
					break;
				case ContactPhoneType.Company:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Company"));
					break;
				case ContactPhoneType.Home:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Home"));
					break;
				case ContactPhoneType.FaxHome:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_FaxHome"));
					break;
				default:
				case ContactPhoneType.Mobile:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Mobile"));
					break;
				case ContactPhoneType.Other:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Other"));
					break;
				case ContactPhoneType.OtherFax:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_OtherFax"));
					break;
				case ContactPhoneType.Pager:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Pager"));
					break;
				case ContactPhoneType.ISDN:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_ISDN"));
					break;
			}
			var contactInfoPanelControls = UI.TryGetMember("formContact.tableLayoutPanel_Overview_Left").Controls;
			for (int i = contactInfoPanelControls.Count - 1; i >= 0; i--)
			{
				if (contactInfoPanelControls[i].Name.Equals("text_Phone"))
				{
					UI.Mouse.Click(contactInfoPanelControls[i]);
					UI.Type(number);
					break;
				}
			}
		}

		internal static void SetCategory(ComboBoxCategory categoryComboBox, string categoryName)
		{
			ComboBoxCategory combo = UI.TryGetMember<ComboBoxCategory>("formContact.combo_Overview_Category");
			UI.Mouse.Click(categoryComboBox);
			var controlDataGrid = GeneralUtils.GetDropdownControlDataGrid(categoryComboBox);
			UI.Mouse.Click(controlDataGrid, GeneralUtils.GetRowOfTheCategory(controlDataGrid, categoryName));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using MailClient.UI.Controls;

namespace GUITest
{
	public static class FormContactUtils
	{
		internal static void AddNewEmail(ContactMail mail)
		{
			UI.Mouse.Click(UI.TryGetMember("formContact.toolStripButton_AddEmail"));
			switch (mail.Type)
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
					UI.Type(mail.Address);
					if (mail.DisplayAs != null)
					{
						UI.Mouse.Click(contactInfoPanelControls[i + 3]);
						UI.Mouse.Click(contactInfoPanelControls[i + 2]);
						UI.Type(mail.DisplayAs);
					}
					break;
				}
			}
		}

		internal static void AddNewTelephone(ContactTelephone telephone)
		{
			UI.Mouse.Click(UI.TryGetMember("formContact.toolStripButton_AddPhone"));
			switch (telephone.Type)
			{
				case ContactTelephoneType.Work:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Work"));
					break;
				case ContactTelephoneType.Fax:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Fax"));
					break;
				case ContactTelephoneType.Car:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Car"));
					break;
				case ContactTelephoneType.Company:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Company"));
					break;
				case ContactTelephoneType.Home:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Home"));
					break;
				case ContactTelephoneType.FaxHome:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_FaxHome"));
					break;
				default:
				case ContactTelephoneType.Mobile:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Mobile"));
					break;
				case ContactTelephoneType.Other:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Other"));
					break;
				case ContactTelephoneType.OtherFax:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_OtherFax"));
					break;
				case ContactTelephoneType.Pager:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_Pager"));
					break;
				case ContactTelephoneType.ISDN:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_PhonePurpose_ISDN"));
					break;
			}
			var contactInfoPanelControls = UI.TryGetMember("formContact.tableLayoutPanel_Overview_Left").Controls;
			for (int i = contactInfoPanelControls.Count - 1; i >= 0; i--)
			{
				if (contactInfoPanelControls[i].Name.Equals("text_Phone"))
				{
					UI.Mouse.Click(contactInfoPanelControls[i]);
					UI.Type(telephone.Number);
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

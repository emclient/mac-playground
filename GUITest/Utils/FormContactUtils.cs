using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Forms;

using MailClient.UI.Controls;

namespace GUITest
{
	public static class FormContactUtils
	{
		private enum FormContactTabs
		{
			Overview,
			Details,
			Certificate
		}

		private static void SwitchToTab(FormContactTabs desiredTab)
		{
			var tabSwitcher = UI.TryGetMember("formContact.controlPanelTabSwitcher1") as MailClient.Common.UI.Controls.ControlPanelSwitcher.ControlPanelTabSwitcher;
			if (tabSwitcher.SelectedIndex != (int)desiredTab)
				UI.Mouse.Click(new ControlPanelTabSwitcherWithTabIndex(tabSwitcher, (int)desiredTab));
		}

		internal static void SetName(string contactName)
		{
			SwitchToTab(FormContactTabs.Overview);
			UI.Mouse.Click(UI.TryGetMember("formContact.text_Overview_FullName"));
			UI.Type(contactName);
		}

		internal static void SetCompany(string companyName)
		{
			SwitchToTab(FormContactTabs.Overview);
			UI.Mouse.Click(UI.TryGetMember("formContact.text_Overview_Company"));
			UI.Type(companyName);
		}

		internal static void SetCategory(string categoryName)
		{
			SwitchToTab(FormContactTabs.Overview);
			ComboBoxCategory categoryComboBox = UI.TryGetMember<ComboBoxCategory>("formContact.combo_Overview_Category");
			UI.Mouse.Click(categoryComboBox);
			var controlDataGrid = GeneralUtils.GetDropdownControlDataGrid(categoryComboBox);
			UI.Mouse.Click(controlDataGrid, GeneralUtils.GetRowOfTheCategory(controlDataGrid, categoryName));
		}

		internal static void AddNewEmail(ContactMail mail)
		{
			SwitchToTab(FormContactTabs.Overview);
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
			SwitchToTab(FormContactTabs.Overview);
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

		internal static void AddNewAddress(ContactAddress address)
		{
			SwitchToTab(FormContactTabs.Overview);
			UI.Mouse.Click(UI.TryGetMember("formContact.toolStripButton_AddAddress"));
			switch (address.Type)
			{
				default:
				case ContactAddressType.Work:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_AddAddress_Work"));
					break;
				case ContactAddressType.Home:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_AddAddress_Home"));
					break;
				case ContactAddressType.Other:
					UI.Mouse.Click(UI.TryGetMember<ToolStripMenuItem>("formContact.menuItem_AddAddress_Other"));
					break;
			}
			var newlyCreatedAddressPanel = UI.TryGetMember("formContact.tableLayoutPanel_Overview_Right").Controls.OfType<Control>().Last();
			bool showMoreButtonClicked = false;

			if (!string.IsNullOrEmpty(address.Street))
			{
				UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "text_Address_Street"));
				UI.Type(address.Street);
			}
			if (!string.IsNullOrEmpty(address.City))
			{
				UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "text_Address_City"));
				UI.Type(address.City);
			}
			if (!string.IsNullOrEmpty(address.ZIP))
			{
				UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "text_Address_Zip"));
				UI.Type(address.ZIP);
			}
			if (!string.IsNullOrEmpty(address.State))
			{
				if (!showMoreButtonClicked)
				{
					showMoreButtonClicked = true;
					UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "button_Address_More"));
				}
				UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "text_Address_County"));
				UI.Type(address.State);
			}
			if (!string.IsNullOrEmpty(address.Country))
			{
				UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "text_Address_Country"));
				UI.Type(address.Country);
			}
			if (!string.IsNullOrEmpty(address.GPS))
			{
				if (!showMoreButtonClicked)
				{
					showMoreButtonClicked = true;
					UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "button_Address_More"));
				}
				UI.Mouse.Click(UI.TryGetSubcontrol(newlyCreatedAddressPanel, "text_Address_GPS"));
				UI.Type(address.GPS);
			}
		}

		internal static void Save()
		{
			UI.Mouse.Click(UI.TryGetMember("formContact.stripButton_Save"));
		}

		internal static void Cancel()
		{
			UI.Mouse.Click(UI.TryGetMember("formContact.stripButton_Cancel"));
		}
	}
}

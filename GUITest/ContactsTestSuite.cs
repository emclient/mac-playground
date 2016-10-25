using System;
using System.Windows.Forms;
using System.Threading;
using NUnit.Framework;

// Controls used in tests
using MailClient.UI.Controls;

using MailClient.Storage.Data;

namespace GUITest
{
	[TestFixture]
	class ContactsTestSuite
	{
		[Test]
		public void CreateNewContactTest()
		{
			Thread.Sleep(2000);

			UI.Mouse.Click(UI.TryGetMember("formMain.leftSpine1.controlSidebarBoxContacts"));
			UI.Mouse.Click(UI.TryGetMember("formMain.stripButton_New"));

			FormContactUtils.SetName(string.Format("{0} {1}", Contact.TestContact.Name.FirstName, Contact.TestContact.Name.LastName));
			FormContactUtils.SetCompany(Contact.TestContact.Company);
			FormContactUtils.SetCategory(Contact.TestContact.Category);

			FormContactUtils.AddNewEmail(Contact.TestContact.Mails[0]);
			FormContactUtils.AddNewEmail(Contact.TestContact.Mails[1]);
			FormContactUtils.AddNewEmail(Contact.TestContact.Mails[2]);
			FormContactUtils.AddNewTelephone(Contact.TestContact.Telephones[0]);
			FormContactUtils.AddNewTelephone(Contact.TestContact.Telephones[1]);

			// Cancel - TEMP
			UI.Mouse.Click(UI.TryGetMember("formContact.stripButton_Cancel"));
			// Save
			//UI.Mouse.Click(UI.TryGetControl("formContact.stripButton_Save"));

			Thread.Sleep(2000);

			//var taskForm = UI.WaitForForm("taskForm");
			//Assert.IsNull(taskForm);
			//MainTestSuite.ThrowIfNull(taskForm, "Confirmation dialog hasn't appeared.");
			//var noButton = UI.FindControl(taskForm, "No");
			//MainTestSuite.ThrowIfNull(noButton, "'No' button not found.");
			//UI.Mouse.Click(noButton);
		}

		//[Test]
		public void EditContactTest()
		{
			Thread.Sleep(2000);

			UI.Mouse.Click(UI.TryGetMember("formMain.leftSpine1.controlSidebarBoxContacts"));
			UI.Mouse.Click(UI.TryGetMember("formMain.stripButton_CustomView"));

			var controlContactsDataGrid = (UI.TryGetSubcontrol(UI.TryGetMember("formMain.panelContactsList"), "controlContacts") as ControlContacts).DataGrid;
			//string name = ((controlContactsDataGrid.DataSource[5] as MailClient.Storage.Application.Contact.ContactItem).Name as MailClient.Contact.ContactName).DisplayName;
			UI.Mouse.DoubleClick(controlContactsDataGrid, GeneralUtils.GetRowOfTheContactName(controlContactsDataGrid, Contact.TestContact.Name.DisplayName));

			Assert.IsNotNull(UI.WaitForForm("formContact"));
			UI.Mouse.Click(UI.TryGetMember("formContact.stripButton_Cancel"));

			//UI.Mouse.Click(UI.TryGetControl("formMain.panelContactsList"));
			//UI.Mouse.Click(UI.TryGetControl("formMain.stripButton_BusinessCards"));
			//UI.Mouse.Click(UI.TryGetControl("formMain.panelContacts"));
			Thread.Sleep(2000);
		}

		private void DbConnection()
		{
			//DataStore DataStore = new DataStore(MainClass.DbLocationPath, 0);
			//KeyValueStore Settings = new KeyValueStore(DataStore, "settings.dat", "GeneralSettings");
			//MailClient.Security.MasterPasswordManager MasterPasswordManager = new MailClient.Security.MasterPasswordManager(Settings);
			//MailClient.Protocols.InteractionControllerFactory ControllerFactory = new MailClient.Protocols.InteractionControllerFactory();
			//MailClient.Accounts.AccountManager AccountManager = new MailClient.Accounts.AccountManager(DataStore, Settings.GetValue("online", true), ControllerFactory, MasterPasswordManager);
		}
	}
}

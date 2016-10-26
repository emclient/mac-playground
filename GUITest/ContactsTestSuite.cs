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

			FormMainUtils.SwitchToContactsModule();
			FormMainUtils.AddNewItemInCurrentModule();

			FormContactUtils.SetName(string.Format("{0} {1}", Contact.TestContact.Name.FirstName, Contact.TestContact.Name.LastName));
			FormContactUtils.SetCompany(Contact.TestContact.Company);
			FormContactUtils.SetCategory(Contact.TestContact.Category);

			FormContactUtils.AddNewEmail(Contact.TestContact.Mails[0]);
			FormContactUtils.AddNewEmail(Contact.TestContact.Mails[1]);
			FormContactUtils.AddNewEmail(Contact.TestContact.Mails[2]);
			FormContactUtils.AddNewTelephone(Contact.TestContact.Telephones[0]);
			FormContactUtils.AddNewTelephone(Contact.TestContact.Telephones[1]);

			FormContactUtils.AddNewAddress(Contact.TestContact.Addresses[0]);

			// Cancel - TEMP
			FormContactUtils.Cancel();

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

			FormMainUtils.SwitchToContactsModule();
			UI.Mouse.Click(UI.TryGetMember("formMain.stripButton_CustomView"));

			FormMainUtils.OpenContactDetailFromContactsList(Contact.TestContact.Name.DisplayName);

			// Cancel - TEMP
			FormContactUtils.Cancel();

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

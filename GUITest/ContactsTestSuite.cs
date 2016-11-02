using System;
using System.Windows.Forms;
using System.Threading;
using NUnit.Framework;

using MailClient.Storage.Data;
using GUITest.Handlers.Forms;

namespace GUITest
{
	[TestFixture]
	class ContactsTestSuite
	{
		[Test]
		public void CreateNewContactTest()
		{
			Thread.Sleep(2000);

			FormMainHandler.SwitchToContactsModule();
			FormMainHandler.AddNewItemInCurrentModule();

			FormContactHandler.SetName(string.Format("{0} {1}", Contact.TestContact.Name.FirstName, Contact.TestContact.Name.LastName));
			FormContactHandler.SetCompany(Contact.TestContact.Company);
			FormContactHandler.SetCategory(Contact.TestContact.Category);

			FormContactHandler.AddNewEmail(Contact.TestContact.Mails[0]);
			FormContactHandler.AddNewEmail(Contact.TestContact.Mails[1]);
			FormContactHandler.AddNewEmail(Contact.TestContact.Mails[2]);
			FormContactHandler.AddNewTelephone(Contact.TestContact.Telephones[0]);
			FormContactHandler.AddNewTelephone(Contact.TestContact.Telephones[1]);

			FormContactHandler.AddNewAddress(Contact.TestContact.Addresses[0]);

			// Cancel - TEMP
			FormContactHandler.Cancel();

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

			FormMainHandler.SwitchToContactsModule();
			Handlers.Controls.ControlHandler.Click(UI.TryGetMember("formMain.stripButton_CustomView"));

			FormMainHandler.OpenContactDetailFromContactsList(Contact.TestContact.Name.DisplayName);

			// Cancel - TEMP
			FormContactHandler.Cancel();

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

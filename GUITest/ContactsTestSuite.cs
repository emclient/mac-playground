using System;
using System.Windows.Forms;
using System.Threading;
using NUnit.Framework;

// Controls used in tests
using MailClient.Common.UI.Controls.ControlToolStrip;
using MailClient.UI.Controls.ControlSidebar;
using MailClient.Common.UI.Controls.ControlTextBox;
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
			UI.Mouse.Click(UI.TryGetControl<ControlSidebarBoxContactsPanel>("formMain.leftSpine1.controlSidebarBoxContacts"));
			UI.Mouse.Click(UI.TryGetControl<ControlToolStripButton>("formMain.stripButton_New"));

			var formContact = UI.WaitForForm("formContact");
			MainTestSuite.ThrowIfNull(formContact, "Failed opening formContact");

			// Name, Surname
			UI.Mouse.Click(UI.TryGetControl<ControlTextBox>("formContact.text_Overview_FullName"));
			UI.Type("GUITest_Name GUITest_Surname");
			// Company
			UI.Mouse.Click(UI.TryGetControl<ControlTextBox>("formContact.text_Overview_Company"));
			UI.Type("GUITest_Company");
			// Category combobox
			ComboBoxCategory combo = UI.TryGetControl<ComboBoxCategory>("formContact.combo_Overview_Category");
			UI.Mouse.Click(combo);
			UI.Mouse.Click(Utils.GetDropdownControlDataGrid(combo), 2);

			UI.Mouse.Click(UI.TryGetControl<ControlToolStripButton>("formContact.stripButton_Cancel"));

			var taskForm = UI.WaitForForm("taskForm");
			Assert.IsNull(taskForm);
			//MainTestSuite.ThrowIfNull(taskForm, "Confirmation dialog hasn't appeared.");
			//Thread.Sleep(2000);

			//var noButton = UI.FindControl(taskForm, "No");
			//MainTestSuite.ThrowIfNull(noButton, "'No' button not found.");
			//UI.Mouse.Click(noButton);
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

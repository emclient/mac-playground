using MailClient.Collections;
using MailClient.Common.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MailClient.Accounts.Mail
{
	public partial class MailOptionsControl : OptionsControl
	//, IAccountConfigurationSource
	{
		public interface IAccount
		{
		}

		public enum AccountType
		{
			Mail,
			Contact,
			Calendar,
			//Task = FolderType.Task,
			Chat,
			Category,
		}

		public enum ArchivingScope
		{
		}

		public interface IAccountConfiguration : INotifyPropertyChanged
		{
			void BeginPriorityUpdate();
			void EndPriorityUpdate();

			string AccountName { get; set; }

			string Username { get; set; }
			string Password { get; set; }
			bool PasswordIsSet { get; set; }

			//IAccount GetAccount(Storage.Data.DataStore dataStore, IBindingAccount bindingAccount, IInteractionControllerFactory controllerFactory);

			string ConfigurationUID { get; }
			bool Enabled { get; set; }
			bool DisabledByRestriction { get; set; }
			int Priority { get; set; }
			bool EnableLog { get; set; }

			AccountType AccountType { get; }
			string ProtocolName { get; }
		}

		public interface IBindingAccountConfiguration : IAccountConfiguration
		{
			IBindableList<IAccountConfiguration> AccountConfigurations { get; }
		}

		public interface IAccountWithArchivingConfiguration : IAccountConfiguration
		{
			ArchivingScope ArchivingScope { get; set; }
		}

		public class MailAccount
		{
			public IAccountConfiguration Configuration = new MailAccountConfiguration();
			public MailAccountConfiguration MailConfiguration = new MailAccountConfiguration();
		}

		public class MailAccountConfiguration : IAccountWithArchivingConfiguration
		{
			public string AccountName { get; set; } = "Test Account Name";
			public string ProviderName { get; set; } = "Provider Name";
			public MailAddress EmailAddress { get; set; } = new MailAddress();
			public string Username { get; set; } = "Username";
			public string Password { get; set; } = "Password";

			public bool AutoBcc { get; set; }
			public bool UseAliasesFromSubaccounts { get; set; }

			public bool IncludeInGlobalOperations { get; set; }
			public bool SignMessages { get; set; }
			public bool EncryptMessages { get; set; }
			public bool IncludeCertificates { get; set; }
			public bool UseSeparateFolderTree { get; set; }

			public Guid SignatureForNew { get; set; } = new Guid();
			public Guid SignatureForReplies { get; set; } = new Guid();
			public Guid SignatureForForwards { get; set; } = new Guid();
			public Guid TemplateForNew { get; set; } = new Guid();
			public Guid TemplateForReplies { get; set; } = new Guid();
			public Guid TemplateForForwards { get; set; } = new Guid();

			public ArchivingScope ArchivingScope { get; set; } = new ArchivingScope();
			public bool PasswordIsSet { get; set; } = true;
			public string ConfigurationUID { get; set; } = new Guid().ToString();
			public bool Enabled { get; set; } = true;
			public bool DisabledByRestriction { get; set; } = false;
			public int Priority { get; set; } = 1;
			public bool EnableLog { get; set; } = true;

			public AccountType AccountType { get; set; } = AccountType.Mail;
			public string ProtocolName { get; set; } = "Protocol Name";
			public Guid securityProfile { get; set; } = new Guid();

			//public ArchivingScope archivingScope;

			public MailAddress[] Aliases = new MailAddress[0];
			public event PropertyChangedEventHandler PropertyChanged;
			public void BeginPriorityUpdate() { }
			public void EndPriorityUpdate() { }
		}

		public class Folder
		{
		}

		public class MailAddress
		{
			public string Address = "test@emclient.com";
			public string Name = "Test Account";
		}

		public class ControlAccountDiagnostics
		{
			public DockStyle Dock = DockStyle.Fill;
			public Color BackColor { get; set; } = SystemColors.Window;
			public bool AutoSize { get; set; } = true;
			public AutoSizeMode AutoSizeMode = AutoSizeMode.GrowAndShrink;
		}

		public class OAuthHelper
		{
		}

		private readonly MailAccount account;

		private Folder selectedCalendarFolder;
		private Folder selectedTaskFolder;
		private Folder selectedContactFolder;

		private MailAddress[] aliases;
		private bool useAliasesFromSubaccounts;

		private readonly Dictionary<IAccountConfiguration, OptionsControl> optionControls = new Dictionary<IAccountConfiguration, OptionsControl>();

		private Common.UI.Controls.ControlPanelSwitcher.SwitchPanel testerPage;
		private ControlAccountDiagnostics tester;
		private List<CheckBox> logCheckBoxes;
		private int lastSelectedTab = 0;

		private Bitmap bitmapCheck;
		private Bitmap bitmapCheckOff;

		private OAuthHelper oauthHelper;

		public MailOptionsControl(MailAccount account)
		{
			account = account ?? new MailAccount();

			InitializeComponent();
			this.account = account;

			account.Configuration.PropertyChanged += Configuration_PropertyChanged;

			this.text_IdentityName.Text = account.MailConfiguration.AccountName;
			this.text_Email.Text = account.MailConfiguration.EmailAddress.Address;
			this.text_Name.Text = account.MailConfiguration.EmailAddress.Name;

			LoadAuthenticationSettings(account.MailConfiguration);

			this.check_IncludeInGlobalOperations.Checked = account.MailConfiguration.IncludeInGlobalOperations;
			this.checkEncryptMessages.Checked = account.MailConfiguration.EncryptMessages;
			this.checkIncludeCertificates.Checked = account.MailConfiguration.IncludeCertificates;
			this.checkSignMessages.Checked = account.MailConfiguration.SignMessages;

			aliases = account.MailConfiguration.Aliases;
			useAliasesFromSubaccounts = account.MailConfiguration.UseAliasesFromSubaccounts;

			//this.text_AutoBCC.AddMailAddresses(account.MailConfiguration.AutoBcc, false);

			//OAuthHelper.TryCreate(account.MailConfiguration.ProviderName, out oauthHelper);
			if (oauthHelper != null)
				group_Server_Authentication.Visible = false;

			//Accounts_CollectionChanged(null, new NotifyCollectionChangedEventArgs<IAccount>(NotifyCollectionChangedAction.Add, account.Accounts.ToList()));
			//account.Accounts.CollectionChanged += Accounts_CollectionChanged;

			// Account Tester
			testerPage = new Common.UI.Controls.ControlPanelSwitcher.SwitchPanel();
			testerPage.Text = "Account Tester"; // Resources.Accounts.Mail_base.Diagnostics;
			testerPage.Padding = this.panel_General.Padding;

			// FlowLayoutPanel inside tableLayout - it doesn't work if we only dock FlowLayout to bottom, so we have to 'wrap' it this way
			TableLayoutPanel tablePanelOuter = new TableLayoutPanel();
			tablePanelOuter.ColumnCount = tablePanelOuter.RowCount = 1;
			tablePanelOuter.AutoSize = true;
			tablePanelOuter.Dock = DockStyle.Bottom;
			testerPage.Controls.Add(tablePanelOuter);

			FlowLayoutPanel flowPanel = new FlowLayoutPanel();
			flowPanel.FlowDirection = FlowDirection.TopDown;
			flowPanel.AutoSize = true;
			flowPanel.Dock = DockStyle.Fill;
			flowPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
			tablePanelOuter.Controls.Add(flowPanel);

			Label loggingLabel = new Label();
			loggingLabel.Text = "Enable logs"; //Resources.Accounts.Mail_base.EnableLogs;
			loggingLabel.AutoSize = true;
			loggingLabel.Padding = new System.Windows.Forms.Padding(0);
			loggingLabel.Margin = new System.Windows.Forms.Padding(3, loggingLabel.Margin.Top, 0, 3);
			flowPanel.Controls.Add(loggingLabel);
			logCheckBoxes = new List<CheckBox>();
			//foreach (IAccount subaccount in account.Accounts)
			//{
			//	if (subaccount is Licensing.LicensingAccount)
			//		continue;
			//	IAccount currentAccount = subaccount;
			//	CheckBox cb = new CheckBox();
			//	cb.Text = currentAccount.Configuration.ProtocolName;
			//	cb.Checked = currentAccount.Configuration.EnableLog;
			//	cb.Tag = subaccount;
			//	cb.Margin = new System.Windows.Forms.Padding(18, 5, 3, 0);
			//	cb.AutoSize = true;
			//	flowPanel.Controls.Add(cb);
			//	logCheckBoxes.Add(cb);
			//}

			tester = new ControlAccountDiagnostics(); //this);
			tester.Dock = DockStyle.Fill;
			tester.BackColor = Color.Transparent;
			tester.AutoSize = true;
			tester.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			//testerPage.Controls.Add(tester);

			controlPanelTabSwitcher1.Controls.Add(testerPage);

			using (Graphics sg = this.CreateGraphics())
			{
				Size size = CheckBoxRenderer.GetGlyphSize(sg, CheckBoxState.CheckedNormal);
				bitmapCheck = new Bitmap(size.Width, size.Height);
				using (Graphics g = Graphics.FromImage(bitmapCheck))
					CheckBoxRenderer.DrawCheckBox(g, new Point(0, 0), CheckBoxState.CheckedNormal);
				size = CheckBoxRenderer.GetGlyphSize(sg, CheckBoxState.UncheckedNormal);
				bitmapCheckOff = new Bitmap(size.Width, size.Height);
				using (Graphics g = Graphics.FromImage(bitmapCheckOff))
					CheckBoxRenderer.DrawCheckBox(g, new Point(0, 0), CheckBoxState.UncheckedNormal);
			}

			//DataGridColumn column;
			//dataGrid_Services.BeginUpdate();

			//column = new DataGridColumn("", 50);
			//column.EditMode = EditModeType.ReadOnly;
			//column.DisplayMode = DataGridColumn.DisplayModeType.ImageOnly;
			//column.HorizontalAlign = HorizontalAlignment.Center;
			//column.ContentHorizontalAlign = HorizontalAlignment.Center;
			//column.ContentVerticalAlign = MailClient.UI.Controls.ControlDataGrid.VerticalAlignment.Center;
			//column.SetBindingImage("Enabled", bitmapCheck, bitmapCheckOff);
			//column.UseFixedWidth = true;
			//column.FixedWidth = 32;
			//dataGrid_Services.Columns.Add(column);

			//column = new DataGridColumn("", 50);
			//column.UseFixedWidth = false;
			//column.EditMode = EditModeType.ReadOnly;
			//column.Binding = "ProtocolName";
			//dataGrid_Services.Columns.Add(column);

			//var servicesList = new BindableList<IAccountConfiguration>(account.MailConfiguration.AccountConfigurations.Where(a => !(a is Licensing.LicensingAccountConfiguration)).ToList());
			//dataGrid_Services.SetDataSource(servicesList);

			//dataGrid_Services.EndUpdate();


			// Security tab
			// TODO: set visibility if certificate is found
			//tab_Security.Visible = false;
		}

		private void LoadAuthenticationSettings(MailAccountConfiguration configuration)
		{
			//if (Utils.DomainUtils.IsJoinedToDomain())
			//{
			//	switch (configuration.Authentication)
			//	{
			//		case AuthenticationSource.Stored:
			//			this.radio_UseAccountCredentials.Checked = true;
			//			break;
			//		case AuthenticationSource.SSO:
			//			this.radio_UseSSO.Checked = true;
			//			break;
			//	}
			//}
			//else
			{
				this.radio_UseAccountCredentials.Checked = true;
				this.radio_UseAccountCredentials.Visible = false;
				this.radio_UseSSO.Visible = false;
			}

			this.text_LoginName.Text = configuration.Username;
			this.text_Password.Text = configuration.Password;
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			AlignTableLayoutPanels(table_General_UserInformation, table_General_Authentication, table_General_AutoBCC, table_General_DefaultFolders, table_IdentityName);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			AlignTableLayoutPanels(table_General_UserInformation, table_General_Authentication, table_General_AutoBCC, table_General_DefaultFolders, table_IdentityName);
		}

		private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs<IAccount> e)
		{
			this.SafeBeginInvoke((MethodInvoker)delegate
			{
				//if (e.NewItems != null)
				//{
				//	foreach (var subaccount in e.NewItems)
				//	{
				//		int index = account.Accounts.Where(a => !(a is Licensing.LicensingAccount || a is Protocols.Gdata.GdataAccount || a is Protocols.Gdata.GdataSettingsAccount)).TakeWhile(a => a != subaccount).Count();
				//		var subaccountConfiguration = subaccount.Configuration;
				//		OptionsControl control = (new Protocols.OptionsControlFactory()).GetOptionsControl(subaccount, oauthHelper != null);
				//		if (control != null)
				//		{
				//			string pageName = subaccountConfiguration.ProtocolName;
				//			Common.UI.Controls.ControlPanelSwitcher.SwitchPanel page = new Common.UI.Controls.ControlPanelSwitcher.SwitchPanel();
				//			page.Text = pageName;
				//			page.Name = subaccountConfiguration.ConfigurationUID;
				//			page.Padding = this.tab_General.Padding;
				//			control.Dock = DockStyle.Fill;
				//			control.BackColor = Color.Transparent;
				//			page.Controls.Add(control);
				//			controlPanelTabSwitcher1.Controls.Add(page);
				//			controlPanelTabSwitcher1.Controls.SetChildIndex(page, index + 1);
				//			optionControls.Add(subaccountConfiguration, control);
				//		}
				//	}
				//}

				//if (e.OldItems != null)
				//{
				//	foreach (var subaccount in e.OldItems)
				//	{
				//		var subaccountConfiguration = subaccount.Configuration;
				//		OptionsControl control;
				//		if (optionControls.TryGetValue(subaccountConfiguration, out control))
				//		{
				//			control.Close(DialogResult.Cancel);
				//			optionControls.Remove(subaccountConfiguration);
				//			controlPanelTabSwitcher1.Controls.RemoveByKey(subaccountConfiguration.ConfigurationUID);
				//		}
				//	}
				//}

				//RefreshDefaultFolders();
			});
		}

		//private void dataGrid_Services_CellClicked(object sender, DataGridCellClickedEventArgs e)
		//{
		//	if (e.Column.Index == 0)
		//	{
		//		bool wasDefaultFoldersVisible = table_General_DefaultFolders.Visible;
		//		IAccountConfiguration item = (IAccountConfiguration)dataGrid_Services.DataSource[e.RowIndex];
		//		if (item != null && CanChangeEnabled(item))
		//			item.Enabled = !item.Enabled;
		//		dataGrid_Services.Invalidate();

		//		//if some controls were added when enabling/disabling account (ie POP3), scroll to the bottom
		//	}
		//}

		//private void dataGrid_Services_CellDoubleClicked(object sender, DataGridCellClickedEventArgs e)
		//{
		//	IAccountConfiguration item = (IAccountConfiguration)dataGrid_Services.DataSource[e.RowIndex];
		//	if (item != null && CanChangeEnabled(item))
		//		item.Enabled = !item.Enabled;
		//	dataGrid_Services.Invalidate();
		//}

		private void dataGrid_Services_KeyDown(object sender, KeyEventArgs e)
		{
			//if (e.KeyCode == Keys.Space)
			//{
			//	foreach (IAccountConfiguration item in dataGrid_Services.SelectedItems)
			//		if (CanChangeEnabled(item))
			//			item.Enabled = !item.Enabled;
			//	dataGrid_Services.Invalidate();
			//}
		}

		private bool CanChangeEnabled(IAccountConfiguration item)
		{
			if (!item.Enabled)
				return true;
			//if (Program.AccountManager.DefaultLocalAccount == this.account)
			//	return (item.AccountType & AccountType.Mail) == 0;
			return true;
		}

		private void RefreshDefaultFolders()
		{
			bool displayAny = false;

			//if (account.Accounts.Any(a => (a.AccountType & AccountType.Mail) != 0))
			//{
			//	List<IAccountWithFolder> schedulingSubaccounts = account.Accounts.OfType<IAccountWithFolder>().ToList();

			//	selectedCalendarFolder = Program.AccountManager.GetFolder(account.MailConfiguration.DefaultSchedulingFolderPath);
			//	if (selectedCalendarFolder != null || !schedulingSubaccounts.Any(a => (a.AccountType & AccountType.Calendar) != 0 && a.GetHomeSet(FolderType.Event).Count() != 0))
			//	{
			//		text_General_CalendarFolder.Text = selectedCalendarFolder == null ? string.Empty : selectedCalendarFolder.DisplayPath;
			//		label_General_CalendarFolder.Visible = true;
			//		text_General_CalendarFolder.Visible = true;
			//		button_General_SelectCalendarFolder.Visible = true;
			//		displayAny = true;
			//	}
			//	else
			//	{
			//		label_General_CalendarFolder.Visible = false;
			//		text_General_CalendarFolder.Visible = false;
			//		button_General_SelectCalendarFolder.Visible = false;
			//	}

			//	selectedTaskFolder = Program.AccountManager.GetFolder(account.MailConfiguration.DefaultTaskFolderPath);
			//	if (selectedTaskFolder != null || !schedulingSubaccounts.Any(a => (a.AccountType & AccountType.Calendar) != 0 && a.GetHomeSet(FolderType.Task).Count() != 0))
			//	{
			//		text_General_TaskFolder.Text = selectedTaskFolder == null ? string.Empty : selectedTaskFolder.DisplayPath;
			//		label_General_TaskFolder.Visible = true;
			//		text_General_TaskFolder.Visible = true;
			//		button_General_SelectTaskFolder.Visible = true;
			//		displayAny = true;
			//	}
			//	else
			//	{
			//		label_General_TaskFolder.Visible = false;
			//		text_General_TaskFolder.Visible = false;
			//		button_General_SelectTaskFolder.Visible = false;
			//	}

			//	selectedContactFolder = Program.AccountManager.GetFolder(account.MailConfiguration.DefaultContactFolderPath);
			//	if (selectedContactFolder != null || !account.Accounts.OfType<IAccountWithFolder>().Any(a => (a.AccountType & AccountType.Contact) != 0 && a.GetHomeSet(FolderType.Contact).Count() != 0))
			//	{
			//		text_General_ContactFolder.Text = selectedContactFolder == null ? string.Empty : selectedContactFolder.DisplayPath;
			//		label_General_ContactFolder.Visible = true;
			//		text_General_ContactFolder.Visible = true;
			//		button_General_SelectContactFolder.Visible = true;
			//		displayAny = true;
			//	}
			//	else
			//	{
			//		label_General_ContactFolder.Visible = false;
			//		text_General_ContactFolder.Visible = false;
			//		button_General_SelectContactFolder.Visible = false;
			//	}
			//}

			group_General_Scheduling.Visible = displayAny;
		}

		private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.SafeBeginInvoke((MethodInvoker)delegate
			{
				if (e.PropertyName.Equals("Username"))
					this.text_LoginName.Text = this.account.Configuration.Username;
				else if (e.PropertyName.Equals("Password"))
					this.text_Password.Text = this.account.Configuration.Password;
			});
		}

		public override void Close(DialogResult result)
		{
			//account.Configuration.PropertyChanged -= Configuration_PropertyChanged;
			//account.Accounts.CollectionChanged -= Accounts_CollectionChanged;

			//if (result == DialogResult.OK)
			//{
			//	// maintain integrity with GetDummyAccount()
			//	account.MailConfiguration.AccountName = this.text_IdentityName.Text;
			//	account.MailConfiguration.EmailAddress = new MailAddress(this.text_Name.Text, this.text_Email.Text);

			//	bool credentialsChanged =
			//		(account.MailConfiguration.Authentication == AuthenticationSource.SSO) != this.radio_UseSSO.Checked ||
			//		account.MailConfiguration.Username != this.text_LoginName.Text ||
			//		account.MailConfiguration.Password != this.text_Password.Text ||
			//		account.MailConfiguration.PasswordIsSet != !String.IsNullOrEmpty(this.text_Password.Text);

			//	account.MailConfiguration.Authentication =
			//		this.radio_UseSSO.Checked ? AuthenticationSource.SSO : AuthenticationSource.Stored;
			//	account.MailConfiguration.Username = this.text_LoginName.Text;
			//	account.MailConfiguration.Password = this.text_Password.Text;
			//	account.MailConfiguration.PasswordIsSet = !String.IsNullOrEmpty(this.text_Password.Text);
			//	account.MailConfiguration.DefaultSchedulingFolderPath = selectedCalendarFolder == null ? null : selectedCalendarFolder.Path;
			//	account.MailConfiguration.DefaultTaskFolderPath = selectedTaskFolder == null ? null : selectedTaskFolder.Path;
			//	account.MailConfiguration.DefaultContactFolderPath = selectedContactFolder == null ? null : selectedContactFolder.Path;

			//	account.MailConfiguration.Aliases = aliases;
			//	account.MailConfiguration.UseAliasesFromSubaccounts = useAliasesFromSubaccounts;

			//	List<MailAddress> autoBccAddressList = new List<MailAddress>();
			//	foreach (ControlContactTextBox.ContactWithAddress contact in this.text_AutoBCC.GetContactsWithAddresses())
			//		autoBccAddressList.Add(contact.Address);
			//	account.MailConfiguration.AutoBcc = autoBccAddressList.ToArray();

			//	account.MailConfiguration.IncludeInGlobalOperations = this.check_IncludeInGlobalOperations.Checked;
			//	account.MailConfiguration.EncryptMessages = this.checkEncryptMessages.Checked;
			//	account.MailConfiguration.IncludeCertificates = this.checkIncludeCertificates.Checked;
			//	account.MailConfiguration.SignMessages = this.checkSignMessages.Checked;

			//	foreach (IAccount subaccount in account.Accounts)
			//		subaccount.Configuration.AccountName = this.text_IdentityName.Text;

			//	bool anyLogChanged = false;
			//	foreach (CheckBox checkBox in logCheckBoxes)
			//	{
			//		IAccount subaccount = (IAccount)checkBox.Tag;
			//		anyLogChanged |= subaccount.Configuration.EnableLog != checkBox.Checked;
			//		subaccount.Configuration.EnableLog = checkBox.Checked;
			//	}
			//	if (anyLogChanged)
			//		Program.Settings.SetValue("LastDisableLoggingAskedTicks", DateTime.UtcNow.Ticks);

			//	if (anyLogChanged || credentialsChanged)
			//	{
			//		if (account.IsOnline)
			//		{
			//			account.GoOffline(OfflineReason.ConfigurationChanged);
			//			account.GoOnline();
			//		}
			//	}
			//}

			//foreach (var configurationAndControl in this.optionControls)
			//	configurationAndControl.Value.Close(result);
		}

		public IAccountConfiguration GetDummyAccount()
		{
			//var accountConfigurations = new List<IBindableAccountConfiguration>();
			//foreach (OptionsControl control in this.optionControls.Values)
			//	if (control is IAccountConfigurationSource)
			//		accountConfigurations.Add((IBindableAccountConfiguration)((IAccountConfigurationSource)control).AccountConfiguration);

			//// maintain integrity with Close(DialogResult result) and ApplyChanges(IAccount account)
			//MailAccountConfiguration dummyAccount = new MailAccountConfiguration(
			//	this.text_IdentityName.Text,
			//	this.text_Name.Text,
			//	this.text_Email.Text,
			//	AuthenticationSource.Stored,
			//	this.text_LoginName.Text,
			//	this.text_Password.Text,
			//	!String.IsNullOrEmpty(this.text_Password.Text),
			//	true,
			//	this.check_IncludeInGlobalOperations.Checked,
			//	account.Configuration.Priority);

			//dummyAccount.DefaultSchedulingFolderPath = selectedCalendarFolder == null ? null : selectedCalendarFolder.Path;
			//dummyAccount.DefaultTaskFolderPath = selectedTaskFolder == null ? null : selectedTaskFolder.Path;
			//dummyAccount.DefaultContactFolderPath = selectedContactFolder == null ? null : selectedContactFolder.Path;
			//dummyAccount.Aliases = aliases;
			//dummyAccount.UseAliasesFromSubaccounts = useAliasesFromSubaccounts;
			//dummyAccount.EnableLog = account.Configuration.EnableLog;

			//foreach (var subaccountConfiguration in accountConfigurations)
			//	subaccountConfiguration.AccountName = this.text_IdentityName.Text;

			//List<MailAddress> autoBccAddressList = new List<MailAddress>();
			//foreach (ControlContactTextBox.ContactWithAddress contact in this.text_AutoBCC.GetContactsWithAddresses())
			//	autoBccAddressList.Add(contact.Address);
			//dummyAccount.AutoBcc = autoBccAddressList.ToArray();

			//foreach (var accountConfiguration in accountConfigurations)
			//	dummyAccount.AccountConfigurations.Add(accountConfiguration);
			//accountConfigurations.ForEach(a => a.BindingConfigurationUID = dummyAccount.ConfigurationUID);

			//return dummyAccount;
			return null;
		}

		public override bool ValidateInput()
		{
			//if (text_IdentityName.Text.Trim() == string.Empty)
			//{
			//	MailClient.Common.UI.MessageBox.Show(Forms.PleaseFillInAccountName, Forms.AccountNameMustBeFilled, MessageBoxButtons.OK, MessageBoxIcon.Error);
			//	controlPanelTabSwitcher1.ActivatePanel(0);
			//	text_IdentityName.Focus();
			//	return false;
			//}

			//foreach (OptionsControl control in this.optionControls.Values)
			//{
			//	if (!control.ValidateInput())
			//	{
			//		for (var c = control.Parent; c != null; c = c.Parent)
			//		{
			//			if (c is Common.UI.Controls.ControlPanelSwitcher.SwitchPanel)
			//			{
			//				controlPanelTabSwitcher1.ActivatePanel((Common.UI.Controls.ControlPanelSwitcher.SwitchPanel)c);
			//				break;
			//			}
			//		}

			//		return false;
			//	}
			//}

			return true;
		}

		private void button_General_SelectCalendarFolder_Click(object sender, EventArgs e)
		{
			//Folder newFolder;
			//if (MailAccount.ShowDefaultFolderDialog(this.TopLevelControl, account, selectedCalendarFolder, FolderType.Event,
			//	Resources.Accounts_base.TheFolderYouSelected, out newFolder))
			//{
			//	selectedCalendarFolder = newFolder;
			//	if (selectedCalendarFolder != null)
			//		text_General_CalendarFolder.Text = selectedCalendarFolder.DisplayPath;
			//	else
			//		text_General_CalendarFolder.Text = string.Empty;
			//}
		}

		private void button_General_SelectTaskFolder_Click(object sender, EventArgs e)
		{
			//Folder newFolder;
			//if (MailAccount.ShowDefaultFolderDialog(this.TopLevelControl, account, selectedTaskFolder, FolderType.Task,
			//	Resources.Accounts_base.TheFolderYouSelectedTask, out newFolder))
			//{
			//	selectedTaskFolder = newFolder;
			//	if (selectedTaskFolder != null)
			//		text_General_TaskFolder.Text = selectedTaskFolder.DisplayPath;
			//	else
			//		text_General_TaskFolder.Text = string.Empty;
			//}
		}

		private void button_General_SelectContactFolder_Click(object sender, EventArgs e)
		{
			//Folder newFolder;
			//if (MailAccount.ShowDefaultFolderDialog(this.TopLevelControl, account, selectedContactFolder, FolderType.Contact,
			//	Resources.Accounts_base.TheFolderYouSelected, out newFolder))
			//{
			//	selectedContactFolder = newFolder;
			//	if (selectedContactFolder != null)
			//		text_General_ContactFolder.Text = selectedContactFolder.DisplayPath;
			//	else
			//		text_General_ContactFolder.Text = string.Empty;
			//}
		}

		public bool CanBeSetDefault()
		{
			bool hasActiveReceiver = false;
			bool hasActiveSender = false;

			//foreach (var subaccount in account.Accounts)
			//{
			//	if (subaccount is IReceiveAccount<Storage.Application.Mail.MailItem>)
			//		hasActiveReceiver = true;
			//	if ((subaccount is ISynchronizableAccount) && (subaccount.AccountType & AccountType.Mail) != 0)
			//		hasActiveReceiver = true;
			//	if (subaccount is ISendAccount<Storage.Application.Mail.MailItem>)
			//		hasActiveSender = true;
			//}

			return hasActiveReceiver && hasActiveSender;
		}

		private void button_General_Aliases_Click(object sender, EventArgs e)
		{
			//using (var formAliases = new FormAliases(aliases, useAliasesFromSubaccounts, account.MailAddressesFromSubaccounts))
			//{
			//	if (formAliases.ShowDialog(this.FindForm()) == DialogResult.OK)
			//	{
			//		aliases = formAliases.Addresses;
			//		useAliasesFromSubaccounts = formAliases.UseAliasesFromSubaccounts;
			//	}
			//}
		}

		private void ControlPanelTabSwitcher1_BeforePanelActivated(object sender, Common.UI.Controls.ControlPanelSwitcher.PanelActivatingEventArgs e)
		{
			//if (e.Panel == testerPage)
			//{
			//	if (ValidateInput())
			//		tester.Active = true;
			//	else
			//		controlPanelTabSwitcher1.ActivatePanel(lastSelectedTab);
			//}
			//else
			//{
			//	tester.Active = false;
			//	lastSelectedTab = controlPanelTabSwitcher1.SelectedIndex;
			//}
		}

		#region IAccountSource Members

		public IAccountConfiguration AccountConfiguration
		{
			get { return GetDummyAccount(); }
		}

		public event EventHandler AccountChanged { add { } remove { } }

		public void ApplyChanges(IAccountConfiguration accountConfigurations)
		{
			//// maintain integrity with GetDummyAccount()
			//MailAccountConfiguration configuration = (MailAccountConfiguration)accountConfigurations;

			//this.text_IdentityName.Text = configuration.AccountName;
			//this.text_Name.Text = configuration.EmailAddress.Name;
			//this.text_Email.Text = configuration.EmailAddress.Address;

			//LoadAuthenticationSettings(configuration);

			//this.check_IncludeInGlobalOperations.Checked = configuration.IncludeInGlobalOperations;
			//this.checkEncryptMessages.Checked = configuration.EncryptMessages;
			//this.checkIncludeCertificates.Checked = configuration.IncludeCertificates;
			//this.checkSignMessages.Checked = configuration.SignMessages;

			//selectedCalendarFolder = Program.AccountManager.GetFolder(configuration.DefaultSchedulingFolderPath);
			//text_General_CalendarFolder.Text = selectedCalendarFolder == null ? string.Empty : selectedCalendarFolder.DisplayPath;
			//selectedTaskFolder = Program.AccountManager.GetFolder(configuration.DefaultTaskFolderPath);
			//text_General_TaskFolder.Text = selectedTaskFolder == null ? string.Empty : selectedTaskFolder.DisplayPath;
			//selectedContactFolder = Program.AccountManager.GetFolder(configuration.DefaultContactFolderPath);
			//text_General_ContactFolder.Text = selectedContactFolder == null ? string.Empty : selectedContactFolder.DisplayPath;

			//aliases = configuration.Aliases;
			//useAliasesFromSubaccounts = configuration.UseAliasesFromSubaccounts;
			//this.text_AutoBCC.Text = string.Empty;
			//this.text_AutoBCC.AddMailAddresses(configuration.AutoBcc, false);

			//int index = 0;
			//foreach (OptionsControl control in this.optionControls.Values)
			//	if (control is IAccountConfigurationSource)
			//		((IAccountConfigurationSource)control).ApplyChanges(configuration.AccountConfigurations[index++]);
		}

		#endregion

		private void radio_Authentication_CheckChanged(object sender, EventArgs e)
		{
			this.text_LoginName.Enabled = this.radio_UseAccountCredentials.Checked;
			this.text_Password.Enabled = this.radio_UseAccountCredentials.Checked;
		}
	}
}

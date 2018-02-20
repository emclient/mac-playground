using System;
using System.ComponentModel;
using System.Windows.Forms;
//using MailClient.Accounts;
//using MailClient.Common.UI;
//using MailClient.Storage.Application;
using System.Net;
//using MailClient.UI.Controls;

namespace MailClient.Protocols.Imap
{
	partial class ImapOptionsControl : OptionsControl
	//, IAccountConfigurationSource
	{
		//private ImapAccount account;
		//private ImapAccountConfiguration accountConfiguration;
		private bool updatingComboFromCode = false;

/*		public ImapOptionsControl(ImapAccountConfiguration accountConfiguration, ImapAccount account, bool hideCredentials)
		{
			InitializeComponent();
			this.account = account;
			this.accountConfiguration = accountConfiguration;

			accountConfiguration.PropertyChanged += Configuration_PropertyChanged;

			this.text_Host.Text = accountConfiguration.Host;
			this.text_Port.Text = accountConfiguration.Port.ToString();
			updatingComboFromCode = true;
			this.combo_Security.SelectedIndex = this.TlsTypeToIndex(accountConfiguration.SSL);
			updatingComboFromCode = false;

			switch (accountConfiguration.AccountCredentialsModel)
			{
				//case CredentialsModelTypes.Anonymous: this.radio_Anonymous.Checked = true; break;
				case CredentialsModelTypes.UseBindingAccount: this.radio_UseIdentityCredentials.Checked = true; break;
				case CredentialsModelTypes.UseAccountDefaults: this.radio_UseAccountCredentials.Checked = true; break;
			}
			this.text_LoginName.Text = accountConfiguration.Username;
			this.text_Password.Text = accountConfiguration.Password;
			if (hideCredentials)
				group_Server_Authentication.Visible = false;

			if (account != null)
			{
				Folder folder = account.AccountFolder;
				switch (folder == null ? OfflineSynchronizationMode.None : folder.OfflineSynchronizationMode)
				{
					case OfflineSynchronizationMode.Body:
						this.checkOffline.Checked = true;
						this.checkOfflineAttachments.Checked = false;
						break;
					case OfflineSynchronizationMode.BodyAndAttachments:
						this.checkOffline.Checked = true;
						this.checkOfflineAttachments.Checked = true;
						break;
					default:
						this.checkOffline.Checked = false;
						this.checkOfflineAttachments.Checked = false;
						this.checkOfflineAttachments.Enabled = false;
						break;
				}

				bool isFallback;
				checkAutodetectFolderNames.Checked = accountConfiguration.AutodetectFolderNames;
				textBoxSentName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Sent, out isFallback);
				textBoxDraftsName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Draft, out isFallback);
				textBoxTrashName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Trash, out isFallback);
				textBoxJunkName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Spam, out isFallback);
			}
			else
			{
				group_Server_Offline.Visible = false;
				group_Server_SpecialFolders.Visible = false;
			}
		}
*/
		public ImapOptionsControl()
		{
			InitializeComponent();

			//this.account = account;
			//this.accountConfiguration = accountConfiguration;

			//accountConfiguration.PropertyChanged += Configuration_PropertyChanged;

			this.text_Host.Text = "host"; //accountConfiguration.Host;
			this.text_Port.Text = "port"; //accountConfiguration.Port.ToString();
			updatingComboFromCode = true;
			this.combo_Security.SelectedIndex = 0;// this.TlsTypeToIndex(accountConfiguration.SSL);
			updatingComboFromCode = false;

			//switch (accountConfiguration.AccountCredentialsModel)
			//{
			//	//case CredentialsModelTypes.Anonymous: this.radio_Anonymous.Checked = true; break;
			//	case CredentialsModelTypes.UseBindingAccount: this.radio_UseIdentityCredentials.Checked = true; break;
			//	case CredentialsModelTypes.UseAccountDefaults: this.radio_UseAccountCredentials.Checked = true; break;
			//}
			this.radio_UseIdentityCredentials.Checked = true;
			this.radio_UseAccountCredentials.Checked = false;

			this.text_LoginName.Text = "username"; //accountConfiguration.Username;
			this.text_Password.Text = "password"; //accountConfiguration.Password;

			//if (hideCredentials)
			//	group_Server_Authentication.Visible = false;

			//if (account != null)
			//{
			//	Folder folder = account.AccountFolder;
			//	switch (folder == null ? OfflineSynchronizationMode.None : folder.OfflineSynchronizationMode)
			//	{
			//		case OfflineSynchronizationMode.Body:
						this.checkOffline.Checked = true;
						this.checkOfflineAttachments.Checked = false;
				//		break;
				//	case OfflineSynchronizationMode.BodyAndAttachments:
				//		this.checkOffline.Checked = true;
				//		this.checkOfflineAttachments.Checked = true;
				//		break;
				//	default:
				//		this.checkOffline.Checked = false;
				//		this.checkOfflineAttachments.Checked = false;
				//		this.checkOfflineAttachments.Enabled = false;
				//		break;
				//}

				//bool isFallback;
				//checkAutodetectFolderNames.Checked = accountConfiguration.AutodetectFolderNames;
				//textBoxSentName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Sent, out isFallback);
				//textBoxDraftsName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Draft, out isFallback);
				//textBoxTrashName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Trash, out isFallback);
				//textBoxJunkName.Text = account.GetSpecialFolderImapPath(SpecialFolderType.Spam, out isFallback);
			//}
			//else
			//{
			//	group_Server_Offline.Visible = false;
			//	group_Server_SpecialFolders.Visible = false;
			//}
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			AlignTableLayoutPanels(tableLayoutPanel1, tableLayoutPanel2, tableLayoutPanel3);
		}
		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			AlignTableLayoutPanels(tableLayoutPanel1, tableLayoutPanel2, tableLayoutPanel3);
		}

		private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.SafeBeginInvoke((MethodInvoker)delegate
			{
				//if (e.PropertyName.Equals("Username"))
				//	this.text_LoginName.Text = this.accountConfiguration.Username;
				//else if (e.PropertyName.Equals("Password"))
				//	this.text_Password.Text = this.accountConfiguration.Password;
			}, null);
		}

		public override void Close(DialogResult result)
		{
/*			accountConfiguration.PropertyChanged -= Configuration_PropertyChanged;

			if (result == DialogResult.OK)
			{
				// maintain integrity with GetDummyAccount()
				int port = Convert.ToInt32(this.text_Port.Text);
				TlsType tlsType = IndexToTlsType(this.combo_Security.SelectedIndex);
				CredentialsModelTypes accountCredentialsModel =
					this.radio_UseIdentityCredentials.Checked ? CredentialsModelTypes.UseBindingAccount :
					CredentialsModelTypes.UseAccountDefaults;

				bool anythingChanged =
					accountConfiguration.Host != this.text_Host.Text ||
					accountConfiguration.Port != port ||
					accountConfiguration.SSL != tlsType ||
					accountConfiguration.AccountCredentialsModel != accountCredentialsModel ||
					accountConfiguration.Username != this.text_LoginName.Text ||
					accountConfiguration.Password != this.text_Password.Text ||
					accountConfiguration.PasswordIsSet != !String.IsNullOrEmpty(this.text_Password.Text);

				if (!anythingChanged && account != null)
				{
					bool isFallback;

					anythingChanged =
						(account.AccountFolder.OfflineSynchronizationMode != OfflineSynchronizationMode.None) != this.checkOffline.Checked ||
						(account.AccountFolder.OfflineSynchronizationMode == OfflineSynchronizationMode.BodyAndAttachments) != this.checkOfflineAttachments.Checked ||
						accountConfiguration.AutodetectFolderNames != checkAutodetectFolderNames.Checked ||
						account.GetSpecialFolderImapPath(SpecialFolderType.Sent, out isFallback) != textBoxSentName.Text ||
						account.GetSpecialFolderImapPath(SpecialFolderType.Draft, out isFallback) != textBoxDraftsName.Text ||
						account.GetSpecialFolderImapPath(SpecialFolderType.Trash, out isFallback) != textBoxTrashName.Text ||
						account.GetSpecialFolderImapPath(SpecialFolderType.Spam, out isFallback) != textBoxJunkName.Text;
				}

				if (anythingChanged)
				{
					bool wasOnline = false;
					if (account != null && account.IsOnline)
					{
						account.GoOffline(OfflineReason.ConfigurationChanged);
						wasOnline = true;
					}

					accountConfiguration.Host = this.text_Host.Text;
					accountConfiguration.Port = Convert.ToInt32(this.text_Port.Text);
					accountConfiguration.SSL = IndexToTlsType(this.combo_Security.SelectedIndex);
					accountConfiguration.AccountCredentialsModel = accountCredentialsModel;
					accountConfiguration.Username = this.text_LoginName.Text;
					accountConfiguration.Password = this.text_Password.Text;
					accountConfiguration.PasswordIsSet = !String.IsNullOrEmpty(this.text_Password.Text);

					if (account != null)
					{
						accountConfiguration.AutodetectFolderNames = checkAutodetectFolderNames.Checked;
						if (!checkAutodetectFolderNames.Checked)
						{
							account.ChangeSpecialFolder(SpecialFolderType.Sent, textBoxSentName.Text, false);
							account.ChangeSpecialFolder(SpecialFolderType.Draft, textBoxDraftsName.Text, false);
							account.ChangeSpecialFolder(SpecialFolderType.Trash, textBoxTrashName.Text, false);
							account.ChangeSpecialFolder(SpecialFolderType.Spam, textBoxJunkName.Text, false);
						}

						Folder folder = account.AccountFolder;
						if (this.checkOffline.Checked && this.checkOfflineAttachments.Checked)
						{
							folder.OfflineSynchronizationMode = OfflineSynchronizationMode.BodyAndAttachments;
							folder.OfflineSynchronizationScope = OfflineSynchronizationScope.FolderAndSubfolders;
						}
						else if (this.checkOffline.Checked)
						{
							folder.OfflineSynchronizationMode = OfflineSynchronizationMode.Body;
							folder.OfflineSynchronizationScope = OfflineSynchronizationScope.FolderAndSubfolders;
						}
						else
						{
							folder.OfflineSynchronizationMode = OfflineSynchronizationMode.None;
							folder.OfflineSynchronizationScope = OfflineSynchronizationScope.Folder;
						}
					}

					if (wasOnline)
						account.GoOnline();
				}
			}
*/		}

		public event EventHandler AccountChanged { add { } remove { } }

/*		public IAccountConfiguration AccountConfiguration
		{
			get
			{
				// maintain integrity with Close(DialogResult result) and ApplyChanges(IAccount account)
				int port = Convert.ToInt32(this.text_Port.Text);
				TlsType tlsType = IndexToTlsType(this.combo_Security.SelectedIndex);
				CredentialsModelTypes accountCredentialsModel =
					//this.radio_Anonymous.Checked ? CredentialsModelTypes.Anonymous :
					this.radio_UseIdentityCredentials.Checked ? CredentialsModelTypes.UseBindingAccount :
					CredentialsModelTypes.UseAccountDefaults;

				var dummyAccount = new ImapAccountConfiguration(
					string.Empty,
					this.text_LoginName.Text,
					this.text_Password.Text,
					!String.IsNullOrEmpty(this.text_Password.Text),
					true,
					accountConfiguration.Priority,
					this.text_Host.Text,
					port,
					tlsType,
					accountCredentialsModel,
					new NotificationSettingCollection());
				dummyAccount.EnableLog = accountConfiguration.EnableLog;

				return dummyAccount;
			}
		}

		public void ApplyChanges(IAccountConfiguration accountConfiguration)
		{
			// maintain integrity with GetDummyAccount()
			ImapAccountConfiguration configuration = (ImapAccountConfiguration)accountConfiguration;

			this.text_Host.Text = configuration.Host;
			this.text_Port.Text = configuration.Port.ToString();
			updatingComboFromCode = true;
			this.combo_Security.SelectedIndex = this.TlsTypeToIndex(configuration.SSL);
			updatingComboFromCode = false;

			switch (configuration.AccountCredentialsModel)
			{
				case CredentialsModelTypes.UseBindingAccount: this.radio_UseIdentityCredentials.Checked = true; break;
				case CredentialsModelTypes.UseAccountDefaults: this.radio_UseAccountCredentials.Checked = true; break;
			}
			this.text_LoginName.Text = configuration.Username;
			this.text_Password.Text = configuration.Password;
		}

		public override bool ValidateInput()
		{
			int port;
			if (!Int32.TryParse(text_Port.Text, out port) || port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
			{
				MailClient.Common.UI.MessageBox.Show(MailClient.Resources.UI.Forms.IncomingPortMustBeNumerical, MailClient.Resources.UI.Forms.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				text_Port.Focus();
				return false;
			}
			return true;
		}

*/

		private void radio_Authentication_CheckChanged(object sender, EventArgs e)
		{
			this.text_LoginName.Enabled = this.radio_UseAccountCredentials.Checked;
			this.text_Password.Enabled = this.radio_UseAccountCredentials.Checked;
		}

/*		private int TlsTypeToIndex(TlsType type)
		{
			switch (type)
			{
				case TlsType.NoTls:
					return 0;
				case TlsType.UseTlsAlways:
					return 2;
				case TlsType.UseTlsLegacy:
				case TlsType.UseTlsIfAvailable:
					return 1;
				case TlsType.UseTlsOnSpecialPort:
					return 3;
			}

			return 0;
		}

		private TlsType IndexToTlsType(int index)
		{
			switch (index)
			{
				case 0:
					return TlsType.NoTls;
				case 1:
					return TlsType.UseTlsIfAvailable;
				case 2:
					return TlsType.UseTlsAlways;
				case 3:
					return TlsType.UseTlsOnSpecialPort;
			}

			return TlsType.NoTls;
		}
*/
		private void combo_Security_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!updatingComboFromCode)
			{
				if (combo_Security.SelectedIndex >= 0 && combo_Security.SelectedIndex < 3 && text_Port.Text == "993")
				{
					text_Port.Text = "143";
				}
				else if (combo_Security.SelectedIndex == 3 && text_Port.Text == "143")
				{
					text_Port.Text = "993";
				}
			}
		}

		private void checkOffline_CheckedChanged(object sender, EventArgs e)
		{
			this.checkOfflineAttachments.Enabled = this.checkOffline.Checked;
		}

		private void checkAutodetectFolderNames_CheckedChanged(object sender, EventArgs e)
		{
			textBoxDraftsName.Enabled = textBoxJunkName.Enabled = textBoxSentName.Enabled = textBoxTrashName.Enabled =
				!checkAutodetectFolderNames.Checked;
		}
	}
}

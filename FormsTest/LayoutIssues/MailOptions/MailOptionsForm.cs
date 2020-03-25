using System;
using System.Windows.Forms;

namespace FormsTest
{
	public partial class MailOptionsForm : Form
	{
		private MailClient.Accounts.Mail.MailOptionsControl mailOptionsControl;

		public MailOptionsForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (mailOptionsControl == null)
			{
				mailOptionsControl = new MailClient.Accounts.Mail.MailOptionsControl(null);
				this.Controls.Add(mailOptionsControl);
				mailOptionsControl.Dock = DockStyle.Fill;
			}
		}
	}
}

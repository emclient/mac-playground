using System;
using System.Windows.Forms;

namespace FormsTest
{
	public partial class ImapOptionsForm : MailClient.Common.UI.Forms.ThemeForm
	{
		private MailClient.Protocols.Imap.ImapOptionsControl imapOptionsControl;

		public ImapOptionsForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (imapOptionsControl == null)
			{
				panel1.Hide();
				//panel1.SuspendLayout();

				imapOptionsControl = new MailClient.Protocols.Imap.ImapOptionsControl();
				//imapOptionsControl.Size = panel1.ClientSize;
				this.Controls.Add(imapOptionsControl);
				imapOptionsControl.Dock = DockStyle.Fill;

				//panel1.ResumeLayout();
			}
		}
	}
}

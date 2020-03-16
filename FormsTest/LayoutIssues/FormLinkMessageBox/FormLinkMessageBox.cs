using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MailClient.UI.Forms
{
	public partial class FormLinkMessageBox : Common.UI.Forms.BorderlessForm
	{
		public FormLinkMessageBox()
		{
			InitializeComponent();
		}

		public FormLinkMessageBox(string title, string text)
 			: this()
		{
			this.Text = title;
			linkLabel1.Text = text;

			/*foreach (Match m in HtmlConversion.HtmlConversion.LinkDetectionRegularExpression.Matches(text))
			{
				this.linkLabel1.Links.Add(new Common.UI.Controls.Link(m.Index, m.Length, m.Value));
			}*/
		}

		private void linkLabel1_LinkClicked(object sender, Common.UI.Controls.ControlLinkClickedEventArgs e)
		{
			try
			{
				Process.Start((string)e.Link.LinkData);
			}
			catch
			{
			}
		}
	}
}
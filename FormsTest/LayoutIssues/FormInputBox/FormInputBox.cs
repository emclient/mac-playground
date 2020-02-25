using System;
using System.Windows.Forms;

namespace MailClient.UI.Forms
{
	public partial class FormInputBox : Form // BorderlessBaseForm
	{
		public event EventHandler ResultChanged;
		public string Result
		{
			get
			{
				return textResult.Text;
			}
		}

		protected void OnResultChanged(EventArgs e)
		{
			if (ResultChanged != null)
				ResultChanged(this, e);
		}

		public bool ReadOnly
		{
			get
			{
				return textResult.ReadOnly;
			}
			set
			{
				textResult.ReadOnly = value;
			}
		}
		public bool ShowCancel
		{
			get
			{
				return buttonCancel.Visible;
			}
			set
			{
				buttonCancel.Visible = value;
			}
		}
		public bool EnabledOk
		{
			get { return buttonOk.Enabled; }
			set { buttonOk.Enabled = value; }
		}

		public FormInputBox(string windowText, string description, string defaultText)
		{
			InitializeComponent();
			Text = windowText;
			labelDescription.Text = description;
			textResult.Text = defaultText;
		}

		private void FormInputBox_Load(object sender, EventArgs e)
		{
			if (labelDescription.Bounds.Right > this.ClientSize.Width)
				this.ClientSize = new System.Drawing.Size(labelDescription.Bounds.Right + 5, this.ClientSize.Height);
		}

		private void textResult_TextChanged(object sender, EventArgs e)
		{
			OnResultChanged(e);
		}
	}
}

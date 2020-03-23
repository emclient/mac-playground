using System;
using System.Drawing;
using System.Windows.Forms;

namespace MailClient.UI.Controls.WizardControls
{
	public partial class ControlExpandablePanelAutodiscover : ControlExpandablePanel
	{
		public event EventHandler StartAutodiscover;
		public event EventHandler StopAutodiscover;
		public event EventHandler ManualConfiguration;
		private string originalCaption;
		private bool loaded;

		public enum AutodiscoverState { Initial, Running, Aborted, Failed, Unauthorized, DomainNotFound }

		private AutodiscoverState autodiscoverState = AutodiscoverState.Initial;

		public string Email
		{
			get { return textEmail.Text; }
		}

		public string Password
		{
			get { return textPassword.Visible ? textPassword.Text : null; }
		}


		public ControlExpandablePanelAutodiscover()
		{
			InitializeComponent();

			originalCaption = labelCaption.Text;
			SetState(AutodiscoverState.Initial);

			textEmail.GotFocus += new EventHandler(text_GotFocus);
			textPassword.GotFocus += new EventHandler(text_GotFocus);

			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Absolute;
			tableLayoutPanel1.ColumnStyles[0].Width = Math.Max(
				labelEmail.PreferredWidth + labelEmail.Margin.Horizontal,
				labelPassword.PreferredWidth + labelPassword.Margin.Horizontal);
			tableLayoutPanel1.ResumeLayout();

			loaded = true;
		}

		void text_GotFocus(object sender, EventArgs e)
		{
			//base.OnGotFocus(e);
		}

		public void SetProgressCaption(string text)
		{
			labelProgress.Text = text;
		}

		public void SetState(AutodiscoverState state)
		{
			autodiscoverState = state;

			bool running = false;
			bool firstPassword = false;
			/*switch (state)
			{
				case AutodiscoverState.Aborted:
					labelError.Text = Resources.UI.Forms.AutodiscoveryAborted;
					stripButton_Autodiscover.Text = Resources.UI.Controls_base.TryAgain;
					break;
				case AutodiscoverState.DomainNotFound:
					labelError.Text = Resources.UI.Forms.DomainNameCannotBeVerified;
					stripButton_Autodiscover.Text = Resources.UI.Controls_base.TryAgain;
					break;
				case AutodiscoverState.Failed:
					labelError.Text = Resources.UI.Forms.CorrectSettingsCouldNotBeDetermined;
					stripButton_Autodiscover.Text = Resources.UI.Controls_base.TryAgain;
					break;
				case AutodiscoverState.Initial:
					labelCaption.Text = originalCaption;
					stripButton_Autodiscover.Text = Resources.UI.Controls_base.StartNow;
					break;
				case AutodiscoverState.Running:
					running = true;
					stripButton_Autodiscover.Text = Resources.UI.Controls_base.Cancel;
					break;
				case AutodiscoverState.Unauthorized:
					if (labelPassword.Visible == false)
					{
						labelCaption.Text = Resources.UI.Forms.AutodiscoverPasswordNeeded;
						stripButton_Autodiscover.Text = Resources.UI.Forms.AutodiscoverContinue;
						firstPassword = true;
						tableLayoutPanel1.SetRow(flowLayoutPanel1, 1);
					}
					else
					{
						labelError.Text = Resources.UI.Forms.ServerFoundAuthenticationFailed;
						stripButton_Autodiscover.Text = Resources.UI.Controls_base.TryAgain;
					}
					labelPassword.Visible = textPassword.Visible = true;
					break;

				default: throw new NotImplementedException();
			}*/

			stripButton_Manual.Left = stripButton_Autodiscover.Right + 5;
			stripButton_Manual.Visible = state == AutodiscoverState.Failed;

			if ((state == AutodiscoverState.Initial || firstPassword) && !running)
			{
				labelCaption.Visible = true;
				labelError.Visible = pictureBoxError.Visible = labelProgress.Visible = /*controlWaiting1.Visible =*/ false;
			}
			else
			{
				labelCaption.Visible = false;
				labelError.Visible = pictureBoxError.Visible = !running;
				labelProgress.Visible = /*controlWaiting1.Visible =*/ running;
			}

			textEmail.Enabled = textPassword.Enabled = !running;

			if (state == AutodiscoverState.Unauthorized)
				textPassword.Select();
		}

		public void SetAutocompleteSource(string[] suggestions)
		{
			//textEmail.AutoCompleteCustomSource.AddRange(suggestions);
		}

		private void controlButtonAutodiscover_Click(object sender, EventArgs e)
		{
			if (autodiscoverState == AutodiscoverState.Running)
			{
				if (StopAutodiscover != null)
					StopAutodiscover(this, e);
			}
			else
			{
				if (StartAutodiscover != null)
					StartAutodiscover(this, e);
			}
		}

		private void controlButtonManualConfiguration_Click(object sender, EventArgs e)
		{
			if (ManualConfiguration != null)
				ManualConfiguration(this, e);
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			if (loaded)
			{
				labelCaption.Height = labelError.Height = labelProgress.Height = this.Font.Height * 2;
				tableLayoutPanel1.Top = labelError.Bottom + 2;
			}
		}
	}
}

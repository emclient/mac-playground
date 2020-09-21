using System;
using System.Windows.Forms;

namespace MailClient.UI.Controls.SettingsControls
{
	public partial class ControlSettingsSpellChecker : UserControl
	{
		public ControlSettingsSpellChecker()
		{
			InitializeComponent();

			group_Spell_IMSpellChecker.AutoSize = true;
			group_Spell_MailSpellChecker.AutoSize = true;
			group_Spell_DefaultSpellChecker.AutoSize = true;
		}

		private void button_Spell_DownloadMore_Click(object sender, EventArgs e)
		{
		}
	}
}

#if MONOMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using MonoMac.AppKit;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class OpenFileDialog : FileDialog
	{
		#region properies

		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		[DefaultValue(false)]
		public bool Multiselect {get; set; }

		#endregion //properies

		public override void Reset()
		{
			FileNames = new string[] {};
			FileName = String.Empty;
		}

		protected override bool RunDialog (IntPtr hwndOwner)
		{
			return DialogResult.OK == ShowDialog();
		}

		public new DialogResult ShowDialog()
		{
			using (var context = new ModalDialogContext())
			{
				var panel = new NSOpenPanel();
				panel.AllowsMultipleSelection = Multiselect;
				panel.ResolvesAliases = true;

				if (NSPanelButtonType.Ok == (NSPanelButtonType)panel.RunModal ()) {
					FileNames = GetFileNames(panel);
					FileName = FileNames.Length > 0 ? FileNames[0] : String.Empty;
					return DialogResult.OK;
				}

				return DialogResult.Cancel;
			}
		}

		protected virtual string[] GetFileNames(NSOpenPanel panel)
		{
			string[] filenames = new string[panel.Urls.Length];
			for (int i = 0; i < filenames.Length; ++i)
				filenames[i] = panel.Urls [i].Path;
			return filenames;
		}
	}
}

#endif //MAC
#if MONOMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using MonoMac.AppKit;

namespace System.Windows.Forms
{
	public class SaveFileDialog : FileDialog
	{
		public string Description { get; set; }

		// Gets or sets the root folder where the browsing starts from.
		public Environment.SpecialFolder RootFolder { get; set; }

		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		// Gets or sets a value indicating whether the New Folder button appears in the folder browser dialog box.
		public bool ShowNewFolderButton	{ get; set; }

		public override void Reset()
		{
			SelectedPath = String.Empty;
			Description = String.Empty;
			RootFolder = Environment.SpecialFolder.Desktop;
		}

		protected override bool RunDialog (IntPtr hwndOwner)
		{
			return DialogResult.OK == ShowDialog();
		}

		public new DialogResult ShowDialog ()
		{
			using (var context = new ModalDialogContext ()) {
				var panel = new MonoMac.AppKit.NSSavePanel ();
				if (NSPanelButtonType.Ok == (NSPanelButtonType)panel.RunModal ()) {
					SelectedPath = panel.Url.Path;
					return DialogResult.OK;
				}

				return DialogResult.Cancel;
			}
		}
	}
}

#endif //MONOMAC
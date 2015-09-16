#if MONOMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using MonoMac.AppKit;
using MonoMac.Foundation;

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
			using (var context = new ModalDialogContext ()) {
				var panel = new MonoMac.AppKit.NSSavePanel ();

				if (!String.IsNullOrWhiteSpace(InitialDirectory) && System.IO.Directory.Exists (InitialDirectory))
					panel.DirectoryUrl = NSUrl.FromFilename (InitialDirectory);

				if (!String.IsNullOrWhiteSpace(FileName))
					panel.NameFieldStringValue = FileName;

				if (NSPanelButtonType.Ok != (NSPanelButtonType)panel.RunModal ())
					return false;

				FileName = SelectedPath = panel.Url.Path;
				return true;
			}
		}
	}
}

#endif //MONOMAC
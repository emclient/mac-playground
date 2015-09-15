#if MONOMAC

using System;
using System.ComponentModel;
using System.Windows.Forms;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Windows.Forms.CocoaInternal;

namespace System.Windows.Forms
{
	public class FolderBrowserDialog : CommonDialog
	{
		#region implemented abstract members of CommonDialog

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

		#endregion

		public new DialogResult ShowDialog()
		{
			using (var context = new ModalDialogContext()) {

				var panel = new MonoMac.AppKit.NSOpenPanel();
				panel.CanChooseFiles = false;
				panel.CanChooseDirectories = true;
				panel.AllowsMultipleSelection = false;
				panel.ResolvesAliases = true;
				panel.Title = Description ?? String.Empty;
				if (!String.IsNullOrWhiteSpace (SelectedPath) && System.IO.Directory.Exists (SelectedPath) && IsSubfolderOf (SelectedPath, RootFolder))
					panel.DirectoryUrl = NSUrl.FromFilename (SelectedPath);

				if (NSPanelButtonType.Ok == (NSPanelButtonType)panel.RunModal ()) {
					SelectedPath = panel.Url.Path;
					return DialogResult.OK;
				}

				return DialogResult.Cancel;
			}
		}

		public string Description { get; set; }

		// Gets or sets the root folder where the browsing starts from.
		public Environment.SpecialFolder RootFolder { get; set; }

		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		// Gets or sets a value indicating whether the New Folder button appears in the folder browser dialog box.
		public bool ShowNewFolderButton	{ get; set; }

		protected virtual bool IsSubfolderOf (string subpath, Environment.SpecialFolder root)
		{
			// TODO:
			return true;
		}
	}
}

#endif // MONOMAC

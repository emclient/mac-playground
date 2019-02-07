#if MACDIALOGS

using System;
using System.ComponentModel;
using System.Windows.Forms;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARINMAC
using AppKit;
using Foundation;
#endif

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

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			using (var context = new ModalDialogContext())
			{
				var panel = new NSOpenPanel();
				panel.CanChooseFiles = false;
				panel.CanChooseDirectories = true;
				panel.CanCreateDirectories = ShowNewFolderButton;
				panel.AllowsMultipleSelection = false;
				panel.ResolvesAliases = true;
				panel.Title = Description ?? String.Empty;

				if (Prompt != null)
					panel.Prompt = Prompt;

				if (!String.IsNullOrWhiteSpace(SelectedPath) && System.IO.Directory.Exists(SelectedPath) && IsSubfolderOf(SelectedPath, RootFolder))
					panel.DirectoryUrl = NSUrl.FromFilename(SelectedPath);

				if (NSPanelButtonType.Ok != (NSPanelButtonType)(int)panel.RunModal())
					return false;

				SelectedPath = panel.Url.Path;
				return true;
			}
		}

		#endregion

		internal string Prompt { get; set; }

		public string Description { get; set; }

		// Gets or sets the root folder where the browsing starts from.
		public Environment.SpecialFolder RootFolder { get; set; }

		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		// Gets or sets a value indicating whether the New Folder button appears in the folder browser dialog box.
		public bool ShowNewFolderButton { get; set; } = true;

		protected virtual bool IsSubfolderOf(string subpath, Environment.SpecialFolder root)
		{
			// TODO:
			return true;
		}
	}
}

#endif // MACDIALOGS

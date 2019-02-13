#if MONOMAC || XAMARINMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using System.IO;

#if XAMARINMAC
using AppKit;
using Foundation;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#endif

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
			base.Reset();

			SelectedPath = String.Empty;
			Description = String.Empty;
			RootFolder = Environment.SpecialFolder.Desktop;
		}

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				using (var context = new ModalDialogContext())
				{
					var panel = new NSSavePanel();

					if (!String.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
					{
						panel.DirectoryUrl = NSUrl.FromFilename(InitialDirectory);
						currentDirectory = InitialDirectory;
					}

					if (!String.IsNullOrWhiteSpace(FileName))
						panel.NameFieldStringValue = FileName;

					if (NSPanelButtonType.Ok != (NSPanelButtonType)(int)panel.RunModal())
						return false;

					FileName = SelectedPath = panel.Url.Path;

					return true;
				}
			}
			finally
			{
				if (RestoreDirectory && currentDirectory != null && Directory.Exists(currentDirectory))
					Environment.CurrentDirectory = currentDirectory;
			}
		}

		public Stream OpenFile()
		{
			if (FileName == null)
				throw new ArgumentNullException("OpenFile", "FileName is null");

			Stream retValue;

			try
			{
				retValue = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite);
			}
			catch (Exception)
			{
				retValue = null;
			}

			return retValue;
		}
	}
}

#endif //MONOMAC
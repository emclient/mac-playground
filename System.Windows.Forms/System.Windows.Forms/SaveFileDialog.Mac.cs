#if MONOMAC || XAMARINMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

#if XAMARINMAC
using AppKit;
using Foundation;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#endif

namespace System.Windows.Forms
{
	[Designer("System.Windows.Forms.Design.SaveFileDialogDesigner, " + Consts.AssemblySystem_Design)]
	public class SaveFileDialog : FileDialog
	{
		public string Description { get; set; }

		// Gets or sets the root folder where the browsing starts from.
		public Environment.SpecialFolder RootFolder { get; set; }

		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		// Gets or sets a value indicating whether the New Folder button appears in the folder browser dialog box.
		public bool ShowNewFolderButton { get; set; }

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

					panel.CanSelectHiddenExtension = true;
					panel.ExtensionHidden = false;

					if (!String.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
					{
						panel.DirectoryUrl = NSUrl.FromFilename(InitialDirectory);
						currentDirectory = InitialDirectory;
					}

					if (!String.IsNullOrEmpty(Filter))
						ApplyFilter(panel, Filter);

					if (!String.IsNullOrWhiteSpace(FileName))
						panel.NameFieldStringValue = AdjustExtensionToMatchFilter(FileName, panel.AllowedFileTypes);

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

		protected virtual string AdjustExtensionToMatchFilter(string filename, IList<string> extensions)
		{
			if (string.IsNullOrEmpty(filename) || extensions == null || extensions.Count == 0)
				return filename;

			var ext = Path.GetExtension(filename).Replace(".", "");
			foreach (var val in extensions)
				if (ext.Equals(val, StringComparison.OrdinalIgnoreCase))
					return filename;

			return $"{filename.TrimEnd('.')}.{extensions[0]}";
		}
	}
}

#endif //MONOMAC
#if MONOMAC || XAMARINMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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

					if (!String.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
					{
						panel.DirectoryUrl = NSUrl.FromFilename(InitialDirectory);
						currentDirectory = InitialDirectory;
					}

					if (!String.IsNullOrWhiteSpace(FileName))
						panel.NameFieldStringValue = FileName;

					if (!String.IsNullOrEmpty(Filter))
						ApplyFilter(panel, Filter);

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

		internal void ApplyFilter(NSSavePanel panel, string filter)
		{
			var interlaced = filter.Split('|');
			if (interlaced.Length == 0)
				return;

			var groups = interlaced.Where((value, index) => index % 2 != 0).ToList();
			var extensions = new List<string>();
			foreach (var group in groups)
			{
				var items = group.Split(';');
				foreach (var item in items)
				{
					var position = item.LastIndexOf('.');
					var extension = position < 0 ? item : item.Substring(1 + position);

					if (!String.IsNullOrWhiteSpace(extension))
					{
						if ("*".Equals(extension, StringComparison.InvariantCulture))
							panel.AllowsOtherFileTypes = true;
						else
							extensions.Add(extension);
					}
				}
			}

			if (extensions.Count != 0)
				panel.AllowedFileTypes = extensions.ToArray();
		}
	}
}

#endif //MONOMAC
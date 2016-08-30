#if MONOMAC || XAMARINMAC

using System;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;
using MonoMac.AppKit;
using System.ComponentModel;
using MonoMac.Foundation;
using System.IO;

namespace System.Windows.Forms
{
	public class OpenFileDialog : FileDialog
	{
		#region properies

		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		[DefaultValue(false)]
		public bool Multiselect { get; set; }

		#endregion //properies

		public override void Reset()
		{
			base.Reset();

			FileNames = new string[] { };
			FileName = String.Empty;
		}

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				using (var context = new ModalDialogContext())
				{
					var panel = new NSOpenPanel();
					panel.AllowsMultipleSelection = Multiselect;
					panel.ResolvesAliases = true;

					if (!String.IsNullOrWhiteSpace(InitialDirectory) && System.IO.Directory.Exists(InitialDirectory))
						panel.DirectoryUrl = NSUrl.FromFilename(InitialDirectory);

					if (!String.IsNullOrWhiteSpace(FileName))
						panel.NameFieldStringValue = FileName;

					if (NSPanelButtonType.Ok != (NSPanelButtonType)panel.RunModal())
						return false;

					FileNames = GetFileNames(panel);
					FileName = SelectedPath = FileNames.Length > 0 ? FileNames[0] : String.Empty;
					return true;
				}
			}
			finally
			{
				if (RestoreDirectory && currentDirectory != null && Directory.Exists(currentDirectory))
					Environment.CurrentDirectory = currentDirectory;
			}
		}

		protected virtual string[] GetFileNames(NSOpenPanel panel)
		{
			string[] filenames = new string[panel.Urls.Length];
			for (int i = 0; i < filenames.Length; ++i)
				filenames[i] = panel.Urls[i].Path;
			return filenames;
		}
	}
}

#endif //MAC
using System.Windows.Forms.CocoaInternal;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using AppKit;
using Foundation;
using ObjCRuntime;
using MacApi;

namespace System.Windows.Forms
{
	public class OpenFileDialog : FileDialog
	{
		// Gets or sets the path selected by the user.
		public string SelectedPath { get; set; }

		[DefaultValue(false)]
		public bool Multiselect { get; set; }

		internal override NSSavePanel Panel { get { return panel; } }

		internal NSOpenPanel panel;

		public override void Reset()
		{
			base.Reset();

			FileNames = new string[] { };
			FileName = String.Empty;
		}

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			string currentDirectory = null;
			try { currentDirectory = Environment.CurrentDirectory; } catch { }

			try
			{
				using (var context = new ModalDialogContext())
				{
					using var panel = this.panel = new NSOpenPanel();
					
					panel.AllowsMultipleSelection = Multiselect;
					panel.ResolvesAliases = true;

					if (!String.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
						panel.DirectoryUrl = NSUrl.FromFilename(InitialDirectory);

					if (!String.IsNullOrEmpty(Filter))
						ApplyFilter(panel, Filter);

					if (!String.IsNullOrWhiteSpace(FileName))
						panel.NameFieldStringValue = FileName;

					if (NSModalResponse.OK != (NSModalResponse)(int)panel.RunModal())
						return false;

					FileNames = GetFileNames(panel);
					FileName = SelectedPath = FileNames.Length > 0 ? FileNames[0] : String.Empty;
					return true;
				}
			}
			finally
			{
				if (RestoreDirectory && currentDirectory != null && Directory.Exists(currentDirectory))
					try { Environment.CurrentDirectory = currentDirectory; } catch { }

				panel = null;					
			}
		}

		protected virtual string[] GetFileNames(NSOpenPanel panel)
		{
			var filenames = new List<string>();
			var urls = panel.Urls;
			foreach (var url in urls)
				if (url.Path != null)
					filenames.Add(url.Path);
			return filenames.ToArray();
		}

		public Stream OpenFile()
		{
			if (FileName.Length == 0)
				throw new ArgumentNullException("OpenFile", "FileName is null");

			return new FileStream(FileName, FileMode.Open, FileAccess.Read);
		}

		internal override void ApplyFilter(NSSavePanel panel, string filter)
		{
			base.ApplyFilter(panel, filter);

			if (this.panel?.AccessoryView != null)
				this.panel.AccessoryViewDisclosed(true);
		}
	}

	static class OpenPanelExtensions
	{
		public static bool AccessoryViewDisclosed(this NSOpenPanel panel, bool? value = null)
		{
			var sel = Selector.GetHandle("isAccessoryViewDisclosed");
			var result = LibObjc.bool_objc_msgSend(panel.Handle, sel);

			if (value.HasValue)
			{
				sel = Selector.GetHandle("setAccessoryViewDisclosed:");
				LibObjc.void_objc_msgSend_Bool(panel.Handle, sel, value.Value);
			}

			return result;
		}
	}
}

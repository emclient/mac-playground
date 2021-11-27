using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms.Mac;
using AppKit;
using UniformTypeIdentifiers;

namespace System.Windows.Forms
{
	[DefaultProperty ("FileName")]
	[DefaultEvent ("FileOk")]
	public abstract class FileDialog : CommonDialog
	{
		protected static readonly object EventFileOk = new object ();

		[DefaultValue(true)]
		public bool AddExtension { get; set; }

		[DefaultValue (true)]
		public bool AutoUpgradeEnabled  { get; set; }

		[DefaultValue(false)]
		public virtual bool CheckFileExists { get; set; }

		[DefaultValue(true)]
		public bool CheckPathExists  { get; set; }

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public FileDialogCustomPlacesCollection CustomPlaces { get; protected set; }

		[DefaultValue("")]
		public string DefaultExt { get; set; }
	
		[DefaultValue(true)]
		public bool DereferenceLinks { get; set; }

		[DefaultValue("")]
		public string FileName { get; set; }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] FileNames { get; set; }

		[DefaultValue("")]
		[Localizable(true)]
		public string Filter { get; set; }

		[DefaultValue(1)]
		public int FilterIndex { get; set; }

		[DefaultValue("")]
		public string InitialDirectory { get; set; }

		[DefaultValue(false)]
		public bool RestoreDirectory { get; set; }

		[DefaultValue(false)]
		public bool ShowHelp { get; set; }

		[DefaultValue(false)]
		public bool SupportMultiDottedExtensions { get; set; }

		[DefaultValue("")]
		[Localizable(true)]
		public string Title { get; set; }

		[DefaultValue(true)]
		public bool ValidateNames { get; set; }

		public override void Reset ()
		{
			AddExtension = true;
			CheckFileExists = false;
			CheckPathExists = true;
			DefaultExt = null;
			DereferenceLinks = true;
			FileName = null;
			Filter = String.Empty;
			FilterIndex = 1;
			InitialDirectory = null;
			RestoreDirectory = false;
			SupportMultiDottedExtensions = false;
			ShowHelp = false;
			Title = null;
			ValidateNames = true;

			UpdateFilters ();
		}

		public override string ToString ()
		{
			return String.Format("{0}: Title: {1}, FileName: {2}", base.ToString (), Title, FileName);
		}

		public event CancelEventHandler FileOk {
			add { Events.AddHandler (EventFileOk, value); }
			remove { Events.RemoveHandler (EventFileOk, value); }
		}

		private void UpdateFilters ()
		{
		}

		internal void ApplyFilter(NSSavePanel panel, string filter)
		{
			if (ExtractContentTypes(filter) is UTType[] contentTypes && panel.SupportsAllowedContentTypes())
			{
				panel.AllowedContentTypes = contentTypes;
				return;
			}

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
							panel.AllowsOtherFileTypes  = true;
						else
							extensions.Add(extension);
					}
				}
			}

			if (extensions.Count != 0)
				panel.AllowedFileTypes = extensions.ToArray();
		}

		static readonly string ContentTypesPrefix = "ContentTypes:";

		internal UTType[] ExtractContentTypes(string filter)
		{
			if (filter != null && filter.StartsWith(ContentTypesPrefix))
			{
				var utts = new List<UTType>();
				foreach (var type in filter.Substring(ContentTypesPrefix.Length).Split(";"))
					if (UTType.CreateFromIdentifier(type) is UTType utt)
						utts.Add(utt);
				return utts.ToArray();
			}
			return null;
		}
	}
}

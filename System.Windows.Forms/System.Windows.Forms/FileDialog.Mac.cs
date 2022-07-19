using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms.Mac;
using AppKit;
using Foundation;
using UniformTypeIdentifiers;
using MacApi;
using ObjCRuntime;
using CoreGraphics;

namespace System.Windows.Forms
{
	[DefaultProperty ("FileName")]
	[DefaultEvent ("FileOk")]
	public abstract class FileDialog : CommonDialog
	{
		internal class FilterItem
		{
			public string Label { get; set; }
			public string[] Extensions;
		}

		protected static readonly object EventFileOk = new object ();
		
		internal List<FilterItem> FilterItems { get; set; }

		// Accessory view items:
		internal NSStackView stack;
		internal NSTextField label;
		internal NSPopUpButton popup;

		internal abstract NSSavePanel Panel { get; }
		
		static readonly CGSize defaultViewSize = new CGSize(200, 80);

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

		internal virtual void ApplyFilter(NSSavePanel panel, string filter)
		{
			panel.AccessoryView = null;
			FilterItems = null;
			
			var items = new List<FilterItem>();

			if (panel.RespondsToSelector(new Selector("allowedContentTypes")))
			if (ExtractContentTypes(filter) is UTType[] contentTypes && panel.SupportsAllowedContentTypes())
			{
				panel.AllowedContentTypes = contentTypes;
				return;
			}

			var interlaced = filter.Split('|');
			for (int i = 0; i < interlaced.Length / 2; ++i)
			{
				var item = new FilterItem {
					Label = interlaced[2 * i],
					Extensions = ExtractExtensions(interlaced[1 + 2 * i])
				};
				items.Add(item);
			}

			this.FilterItems = items;

			if (items != null)
				panel.AccessoryView = CreateFileTypeAccessoryView(items);

			var index = Math.Max(0, FilterIndex - 1);
			if (index >= 0 && index < items.Count)
			{
				popup.SelectItem(index);
				SelectFilterItem(index);
			}
		}

		internal virtual string[] ExtractExtensions(string csv, char separator = ';')
		{
			var extensions = new List<string>();
			var items = csv.Split(separator);
			foreach (var item in items)
			{
				var position = item.LastIndexOf('.');
				var extension = position < 0 ? item : item.Substring(1 + position);
				extensions.Add(extension);
			}
			return extensions.ToArray();
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

		internal NSView CreateFileTypeAccessoryView(IList<FilterItem> items)
		{
			var vert = new NSStackView(new CGRect(CGPoint.Empty, defaultViewSize));
			vert.Orientation = NSUserInterfaceLayoutOrientation.Vertical;
			vert.Alignment = NSLayoutAttribute.CenterX;
			vert.EdgeInsets = new NSEdgeInsets(10, 10, 10, 10);

			var stack = new NSStackView(new CGRect(CGPoint.Empty, defaultViewSize));
			stack.Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
			stack.Alignment = NSLayoutAttribute.Baseline;
			vert.AddView(stack, NSStackViewGravity.Leading);

			var label = new NSTextField();
			label.Bezeled = false;
			label.DrawsBackground = false;
			label.Editable = false;
			label.Selectable = false;
			label.StringValue = "Format:";
			//stack.AddView(label, NSStackViewGravity.Leading);

			popup = new NSPopUpButton();
			popup.BezelStyle = NSBezelStyle.Rounded;
			popup.Alignment = NSTextAlignment.Natural;
			popup.Activated += OnFormatPopupActivated;
			stack.AddView(popup, NSStackViewGravity.Leading);

			foreach (var item in items)
			 	popup.AddItem(item.Label);

			return vert;
		}

		internal virtual void SelectFilterItem(int index)
		{
			var items = FilterItems;
			if (items != null && index >= 0 && index < items.Count)
			{
				var extensions = items[index].Extensions;
				if (extensions != null)
				{
					var asterisk = Array.IndexOf(extensions, "*") != -1;
					if (extensions.Length == 1 && asterisk)
					{
						// Xamarin's AllowedFileTypes throws when passing null here:
						var selector = Selector.GetHandle("setAllowedFileTypes:");
						LibObjc.void_objc_msgSend_IntPtr(Panel.Handle, selector, IntPtr.Zero);
						Panel.AllowsOtherFileTypes  = true;
					}
					else if (extensions.Length > 0)
					{
						Panel.AllowedFileTypes = extensions; // Changes the extension in the text field
						Panel.AllowsOtherFileTypes = asterisk;
					}
				}
			}
		}

		internal virtual void OnFormatPopupActivated(object sender, EventArgs e)
		{
			if (sender is NSPopUpButton popup)
			{
				var index = (int)popup.IndexOfSelectedItem;
				SelectFilterItem(index);
			}
		}
	}
}

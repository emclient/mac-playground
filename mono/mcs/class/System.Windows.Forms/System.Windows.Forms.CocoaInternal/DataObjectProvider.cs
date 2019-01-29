using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Mac;
using System.Drawing.Imaging;
using System.Windows.Forms.Mac;

using MacApi.LaunchServices;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARINMAC
using AppKit;
using Foundation;
#endif


namespace System.Windows.Forms.CocoaInternal
{

	internal class DataObjectProvider : NSPasteboardItemDataProvider
	{
		IDataObject data;

#if MONOMAC
		public override bool ConformsToProtocol(IntPtr protocol)
		{
			if ("NSPasteboardItemDataProvider" == NSString.FromHandle(Mac.Extensions.NSStringFromProtocol(protocol)))
				return true;

			return base.ConformsToProtocol(protocol);
		}
#endif

		public DataObjectProvider(IDataObject data)
		{
			this.data = new DataObjectWrapper(data);
		}

		string webArchiveDynamicType = null;
		internal string WebArchiveDynamicType
		{
			get
			{
				if (null == webArchiveDynamicType)
					webArchiveDynamicType = CreateDynamicTypeFor(Pasteboard.NSPasteboardTypeWebArchive);
				return webArchiveDynamicType;
			}
		}

		public string[] Types
		{
			get
			{
				var types = new HashSet<string>();
				foreach (var format in data.GetFormats())
				{
					switch (format)
					{
						case DataFormats.Text:
						case DataFormats.UnicodeText:
							types.Add(Pasteboard.NSPasteboardTypeText);
							break;
						case DataFormats.Html:
							types.Add(Pasteboard.NSPasteboardTypeHTML);
							//types.Add(WebArchiveDynamicType);
							types.Add(Pasteboard.NSPasteboardTypeRTF);
							break;
						case DataFormats.Bitmap:
							types.Add(Pasteboard.NSPasteboardTypePNG);
							types.Add(Pasteboard.NSPasteboardTypeTIFF);
							break;
					}
				}
				var array = new string[types.Count];
				types.CopyTo(array);
				return array;
			}
		}

		#region NSPasteboardDataProvider

		public override void ProvideDataForType(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			switch (type)
			{
				case Pasteboard.NSPasteboardTypeText:
					ProvideText(pboard, item, type);
					break;
				case Pasteboard.NSPasteboardTypeHTML:
					ProvideHtml(pboard, item, type);
					break;
				case Pasteboard.NSPasteboardTypeWebArchive:
					ProvideWebArchive(pboard, item, type);
					break;
				case Pasteboard.NSPasteboardTypeRTF:
					ProvideRtf(pboard, item, type);
					break;
				case Pasteboard.NSPasteboardTypeTIFF:
					ProvideTiff(pboard, item, type);
					break;
				case Pasteboard.NSPasteboardTypePNG:
					ProvidePng(pboard, item, type);
					break;
				default:
					if (WebArchiveDynamicType == type)
						ProvideWebArchive(pboard, item, type);
					break;
			}
		}

		public override void FinishedWithDataProvider(NSPasteboard pasteboard)
		{
			Wipe();
		}

		public void Wipe()
		{
			// Get rid of possible references to whatever that cannot be read during app terminaion.
			if (data is DataObjectWrapper wrapper)
				wrapper.Wipe();
			else if (!(data is DataObject))
				data = new DataObject();
		}

		#endregion

		#region internals

		protected void ProvideText(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			var text = (data.GetData(DataFormats.Text) ?? data.GetData(DataFormats.UnicodeText)) as string;
			if (text != null)
				item.SetStringForType(text, type);
		}

		protected void ProvideHtml(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			var s = GetHtmlWithoutMetadata();
			if (s != null)
				item.SetStringForType(s, Pasteboard.NSPasteboardTypeHTML);
		}

		protected void ProvideWebArchive(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			// This method does not seem to work any more. Disabled by not returning appropriate type in Types getter.
			var s = GetHtmlWithoutMetadata();
			if (s == null)
				return;

			var mainRsrc = new NSDictionary
			{
				["WebResourceData"] = NSData.FromString(s, NSStringEncoding.UTF8),
				["WebResourceFrameName"] = (NSString)"",
				["WebResourceMIMEType"] = (NSString)"text/html",
				["WebResourceTextEncodingName"] = (NSString)"UTF-8",
				["WebResourceURL"] = (NSString)"about:blank"
			};

			var container = new NSDictionary
			{
				["WebMainResource"] = mainRsrc
			};

			var nsdata = NSPropertyListSerialization.DataWithPropertyList(container, NSPropertyListFormat.Xml, out NSError error);
			var archive = NSString.FromData(nsdata, NSStringEncoding.UTF8);
			item.SetDataForType(nsdata, Pasteboard.NSPasteboardTypeWebArchive);
		}

		protected void ProvideRtf(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			var s = GetHtmlWithoutMetadata();
			if (s == null)
				return;

			var nsdata = NSData.FromString(s, NSStringEncoding.UTF8);
			var options = new NSMutableDictionary
			{
				[Pasteboard.NSDocumentTypeDocumentAttribute] = (NSString)Pasteboard.NSHTMLTextDocumentType,
				[Pasteboard.NSCharacterEncodingDocumentAttribute] = new NSNumber((ulong)NSStringEncoding.UTF8)
			};
			var rtf = new NSAttributedString(nsdata, options, out NSDictionary attributes, out NSError error);

			options[Pasteboard.NSDocumentTypeDocumentAttribute] = (NSString)Pasteboard.NSRTFTextDocumentType;
			nsdata = rtf.GetData(new NSRange(0, rtf.Length), options, out error);
			item.SetDataForType(nsdata, Pasteboard.NSPasteboardTypeRTF);
		}

		protected string CreateDynamicTypeFor(string type)
		{
			return UTType.CreatePreferredIdentifier(UTType.kUTTagClassNSPboardType, Pasteboard.NSPasteboardTypeWebArchive, UTType.kUTTypeData);
		}

		protected void ProvideTiff(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			if (data.GetData(DataFormats.Bitmap) is Image image)
				item.SetDataForType(image.ToNSData(ImageFormat.Tiff), Pasteboard.NSPasteboardTypeTIFF);
		}

		protected void ProvidePng(NSPasteboard pboard, NSPasteboardItem item, string type)
		{
			if (data.GetData(DataFormats.Bitmap) is Image image)
				item.SetDataForType(image.ToNSData(ImageFormat.Png), Pasteboard.NSPasteboardTypePNG);
		}

		protected string GetHtmlWithoutMetadata()
		{
			return data.GetData(DataFormats.Html) is string s ? HtmlClip.RemoveHeader(s) : null;
		}

		#endregion
	}
}

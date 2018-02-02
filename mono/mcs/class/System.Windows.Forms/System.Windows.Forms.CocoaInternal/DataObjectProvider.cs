using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Mac;
using System.Drawing.Imaging;

using MacBridge.LaunchServices;

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

		bool finished = false;
		public override void FinishedWithDataProvider(NSPasteboard pasteboard)
		{
			finished = true;
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

			var mainRsrc = new NSMutableDictionary();
			mainRsrc["WebResourceData"] = NSData.FromString(s, NSStringEncoding.UTF8);
			mainRsrc["WebResourceFrameName"] = (NSString)"";
			mainRsrc["WebResourceMIMEType"] = (NSString)"text/html";
			mainRsrc["WebResourceTextEncodingName"] = (NSString)"UTF-8";
			mainRsrc["WebResourceURL"] = (NSString)"about:blank";

			var container = new NSMutableDictionary();
			container["WebMainResource"] = mainRsrc;

			var nsdata = NSPropertyListSerialization.DataWithPropertyList(container, NSPropertyListFormat.Xml, out NSError error);
			var archive = NSString.FromData(nsdata, NSStringEncoding.UTF8);
			item.SetDataForType(nsdata, Pasteboard.NSPasteboardTypeWebArchive);
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

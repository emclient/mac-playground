//
//Pasteboard.cs
// 
//Author:
//	Lee Andrus <landrus2@by-rite.net>
//
//Copyright (c) 2009-2010 Lee Andrus
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//

//
//This document was originally created as a copy of a document in 
//System.Windows.Forms.CarbonInternal and retains many features thereof.
//

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Geoff Norton (gnorton@novell.com)
//
//

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Mac;
using System.Drawing.Imaging;
using System.Windows.Forms.Extensions.IO;

#if XAMARINMAC
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms.CocoaInternal {
	internal static class Pasteboard {
		internal const string internal_format = "com.novell.mono.mwf.pasteboard";
		internal const string serialized_format = "com.novell.mono.mwf.pasteboard.WindowsForms10PersistentObject";

		internal const string NSDocumentTypeDocumentAttribute = "DocumentType";
		internal const string NSCharacterEncodingDocumentAttribute = "CharacterEncoding";
		internal const string NSHTMLTextDocumentType = "NSHTML";
		internal const string NSRTFTextDocumentType = "NSRTF";
		internal const string NSPasteboardTypeURL = "public.url";
		internal const string NSPasteboardTypeUrlName = "public.url-name";
		internal const string NSPasteboardTypeRTF = "public.rtf";
		internal const string NSPasteboardTypeText = "public.utf8-plain-text";
		internal const string NSPasteboardTypeHTML = "public.html";
		internal const string NSPasteboardTypePNG = "public.png";
		internal const string NSPasteboardTypeTIFF = "public.tiff";

		internal const string UniformResourceLocatorW = "UniformResourceLocatorW";

		internal static Dictionary<string, object> managed = new Dictionary<string, object>();

		static Pasteboard ()
		{
		}

		internal static object Retrieve(NSPasteboard pboard, int id)
		{
			var name = DataFormats.GetFormat(id)?.Name ?? String.Empty;
			if (name == Clipboard.IDataObjectFormat)
			{
				var native = null == pboard.GetStringForType(Clipboard.IDataObjectFormat);
				if (native)
					return new DataObjectPasteboard(pboard);

				if (managed.TryGetValue(name, out object value))
					return value is IDataObject idata ? new DataObjectWrapper(idata) : value;
			}

			return null;
		}

		internal static void Store(NSPasteboard pboard, object data, int id)
		{
			if (id == 0)
			{
				pboard.ClearContents();
				managed.Clear();
				return;
			}
			if (data == null)
				return;

			var name = DataFormats.GetFormat(id)?.Name ?? String.Empty;
			switch (name)
			{
				case Clipboard.IDataObjectFormat:
					managed[name] = data;
					pboard.SetStringForType(name, name); // Set flag that clipboard contains our data

					//TODO: Replace with data provider, so that we can work efficiently, on-demand
					Store(pboard, (IDataObject)data);
					break;
			}
		}

		internal static void Store(NSPasteboard pboard, IDataObject data)
		{
			data = new DataObjectWrapper(data);
			foreach (var format in data.GetFormats())
			{
				switch (format)
				{
					case DataFormats.Text:
					case DataFormats.UnicodeText:
						pboard.SetStringForType(data.GetData(format) as string, NSPasteboardTypeText);
						break;
					case DataFormats.Html:
						SetHTMLData(pboard, data.GetData(format));
						break;
					case DataFormats.Bitmap:
						if (data.GetData(format) is Image image)
						{
							pboard.SetDataForType(image.ToNSData(ImageFormat.Png), NSPasteboardTypePNG);
							pboard.SetDataForType(image.ToNSData(ImageFormat.Tiff), NSPasteboardTypeTIFF);
						}
						break;
				}
			}
		}

		static void SetHTMLData(NSPasteboard pboard, object data)
		{
			if (!(data is string s))
				return;

			s = HtmlClip.RemoveHeader(s);

			// Add as HTML
			pboard.SetStringForType(s, NSPasteboardTypeHTML);

			// Add as Apple HTML Web Archive
			var mainRsrc = new NSMutableDictionary();
			mainRsrc[(NSString)"WebResourceData"] = NSData.FromString(s, NSStringEncoding.UTF8);
			mainRsrc[(NSString)"WebResourceFrameName"] = (NSString)"";
			mainRsrc[(NSString)"WebResourceMIMEType"] = (NSString)"text/html";
			mainRsrc[(NSString)"WebResourceTextEncodingName"] = (NSString)"UTF-8";
			mainRsrc[(NSString)"WebResourceURL"] = (NSString)"about:blank";

			var container = new NSMutableDictionary();
			container[(NSString)"WebMainResource"] = mainRsrc;

			var nsdata = NSPropertyListSerialization.DataWithPropertyList(container, NSPropertyListFormat.Xml, out NSError error);
			var archive = NSString.FromData(nsdata, NSStringEncoding.UTF8);
			pboard.SetDataForType(nsdata, (NSString)"Apple Web Archive pasteboard type");

			// Add as RTF
			//nsdata = NSData.FromString(s, NSStringEncoding.UTF8);
			//var options = new NSMutableDictionary();
			//options[(NSString)NSDocumentTypeDocumentAttribute] = (NSString)NSHTMLTextDocumentType;
			//options[(NSString)NSCharacterEncodingDocumentAttribute] = new NSNumber((ulong)NSStringEncoding.UTF8);
			//var rtf = new NSAttributedString(nsdata, options, out NSDictionary attributes, out NSError error);

			//options.Clear();
			//options[(NSString)NSDocumentTypeDocumentAttribute] = (NSString)NSRTFTextDocumentType;
			//options[(NSString)NSCharacterEncodingDocumentAttribute] = new NSNumber((ulong)NSStringEncoding.UTF8);
			//nsdata = rtf.GetData(new NSRange(0, rtf.Length), options, out error);
			//pboard.SetDataForType(nsdata, kUTTypeRTF);
		}

		// Retrieves array of identifiers of available formats.
		internal static int[] GetAvailableFormats(NSPasteboard pboard)
		{
			var ids = new List<int>();

			var native = null == pboard.GetStringForType(Clipboard.IDataObjectFormat);
			if (native)
			{
				// Data in the pasteboard comes from another application
				var names = DataObjectPasteboard.GetFormats(pboard);
				foreach (var name in names)
					ids.Add(DataFormats.Format.Add(name).Id);
			}
			else
			{
				// We provided the data for the pasteboard
				if (managed.TryGetValue(Clipboard.IDataObjectFormat, out object obj) && obj is IDataObject idata)
					foreach (var fmt in idata.GetFormats())
						ids.Add(DataFormats.Format.Add(fmt).Id);

				foreach(var key in managed.Keys)
					ids.Add(DataFormats.Format.Add(key).Id);
			}

			return ids.ToArray();
		}

		internal static NSPasteboard Primary {
			get { return NSPasteboard.GeneralPasteboard; }
		}
		
		internal static NSPasteboard Application {
			get { return NSPasteboard.GeneralPasteboard; }
		}
	}
}

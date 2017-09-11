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


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

#if XAMARINMAC
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms.CocoaInternal {
	internal class Pasteboard {
		private static NSPasteboard primary_pbref;
		private static NSPasteboard app_pbref;

		internal static readonly string internal_format;
		internal static readonly string serialized_format;

		internal const string fmt_public_utf8_plain_text = "public.utf8-plain-text";

		internal const string NSDocumentTypeDocumentAttribute = "DocumentType";
		internal const string NSCharacterEncodingDocumentAttribute = "CharacterEncoding";
		internal const string NSHTMLTextDocumentType = "NSHTML";
		internal const string NSRTFTextDocumentType = "NSRTF";
		internal const string kUTTypeRTF = "public.rtf";
		internal const string NSPasteboardTypeHTML = "public.html";

		static Pasteboard ()
		{
			primary_pbref = NSPasteboard.GeneralPasteboard;
			app_pbref = NSPasteboard.CreateWithUniqueName();
#if DriverDebug
			Console.WriteLine ("primary_pbref = {0}, app_pbref = {1}", primary_pbref, app_pbref);
#endif
			internal_format = "com.novell.mono.mwf.pasteboard";
			serialized_format = "com.novell.mono.mwf.pasteboard.WindowsForms10PersistentObject";
		}

		internal static object Retrieve(NSPasteboard pboard, int id)
		{
			var name = DataFormats.GetFormat(id)?.Name;
			switch (name)
			{
				// TODO: Add support for other types
				case DataFormats.Text:
					return pboard.GetStringForType(fmt_public_utf8_plain_text);
				case DataFormats.Html:
					return GetHTML(pboard);
				case DataFormats.Rtf:
					return GetRTF(pboard);
			}

			return null;
		}

		internal static void Store(NSPasteboard pboard, object data, int id)
		{
			if (id == 0)
			{
				pboard.ClearContents();
				return;
			}

			var name = DataFormats.GetFormat(id)?.Name;
			switch (name)
			{
				// TODO: Add support for other types
				case DataFormats.Text:
					pboard.SetStringForType(data.ToString(), fmt_public_utf8_plain_text);
					break;
				case DataFormats.Html:
					SetHTMLData(pboard, data);
					break;
			}
		}

		static object GetHTML(NSPasteboard pboard)
		{
			var data = pboard.GetDataForType(NSPasteboardTypeHTML);
			if (data != null)
			{
				var html = data.ToString(NSStringEncoding.UTF8);
				if (html == null)
					html = data.ToString(NSStringEncoding.Unicode);
				if (html != null)
				{
					var mshtml = AddMSClipboardMetadata(html.ToString());
					return new MemoryStream(Encoding.UTF8.GetBytes(mshtml));
				}
			}
			return null;
		}

		static object GetRTF(NSPasteboard pboard)
		{
			var data = pboard.GetDataForType(kUTTypeRTF);
			if (data != null)
				return data.ToString(NSStringEncoding.UTF8).ToString();
			return null;
		}

		static void SetHTMLData(NSPasteboard pboard, object data)
		{
			if (data is MemoryStream stream)
			{
				var s = Encoding.UTF8.GetString(stream.ToArray());
				if (!String.IsNullOrEmpty(s))
				{
					// Remove MS header, if any
					int index = s.IndexOf("<!DOCTYPE", StringComparison.InvariantCulture);
					if (index > 0)
						s = s.Substring(index);

					// Add as HTML
					//var nsdata = NSData.FromString(s, NSStringEncoding.Unicode);
					var nsdata = NSData.FromString(s, NSStringEncoding.UTF8);
					pboard.SetDataForType(nsdata, NSPasteboardTypeHTML);

					// Add as RTF
					nsdata = NSData.FromString(s, NSStringEncoding.UTF8);
					var options = new NSMutableDictionary();
					options[(NSString)NSDocumentTypeDocumentAttribute] = (NSString)NSHTMLTextDocumentType;
					options[(NSString)NSCharacterEncodingDocumentAttribute] = new NSNumber((ulong)NSStringEncoding.UTF8);
					var rtf = new NSAttributedString(nsdata, options, out NSDictionary attributes, out NSError error);

					options.Clear();
					options[(NSString)NSDocumentTypeDocumentAttribute] = (NSString)NSRTFTextDocumentType;
					nsdata = rtf.GetData(new NSRange(0, rtf.Length), options, out error);
					pboard.SetDataForType(nsdata, kUTTypeRTF);
				}
			}
		}

		internal static bool SetStringForType(NSPasteboard pboard, object data, string type)
		{
			string s = null;
			if (data is MemoryStream stream)
				using (var reader = new StreamReader(stream))
					s = reader.ReadToEnd();

			if (String.IsNullOrEmpty(s))
				return false;

			pboard.SetStringForType(s, type);
			return true;
		}

		// Original (mono) version of Retrieve method - it might help us in the future
		internal static object RetrieveOrig (NSPasteboard pbref, int key)
		{
			DataFormats.Format keyFormat = DataFormats.GetFormat (key);
			string keyString;
			if (null != keyFormat)
				keyString = keyFormat.Name;
			//FIXME: We should translate key to native, but we're cheating for now
			keyString = internal_format;

			NSData pbdata = pbref.GetDataForType (internal_format);
			if (pbdata != null) {
				// Use Int64 in case IntPtr is that size in 64-bit applications.
				//GCHandle handle = (GCHandle) (IntPtr) BitConverter.ToInt64 (pbdata.bytes (), 0);
				//return handle.Target;
				// FIXME
				return null;
			}

			pbdata = pbref.GetDataForType (serialized_format);
			if (null != pbdata) {
				object data = null;
				using (var stream = pbdata.AsStream())
				{
					if (stream.Length > 0)
					{
						stream.Seek(0, 0);
						BinaryFormatter bf = new BinaryFormatter();
						data = bf.Deserialize(stream);
					}
				}
				return data;
			}

			pbdata = pbref.GetDataForType (keyString);
			if (null != pbdata) {
				//FIXME: convert data from native format.
				Console.WriteLine ("FIXME: Pasteboard data of type '{1}' ignored.", keyString);
				// return convertedData;
			}

			return null;
		}

		internal static void StoreOrig (NSPasteboard pbref, object data, int key)
		{
			// Free any GCHandle already on the pasteboard.
			NSData pbdata = pbref.GetDataForType (internal_format);
			if (pbdata != null) {
				// Use Int64 in case IntPtr is that size in 64-bit applications.
				//GCHandle handle = (GCHandle) (IntPtr) BitConverter.ToInt64 (pbdata.bytes (), 0);
				//handle.Free ();
				// FIXME
			}

			List<string> types = new List<string> { internal_format };

			if (data is ISerializable)
				types.Add (serialized_format);

			DataFormats.Format keyFormat = DataFormats.GetFormat(key);
			string keyString;
			if (null != keyFormat) {
				keyString = keyFormat.Name;
				//FIXME: We should translate key to native, but we're cheating for now
//				types.addObject (serialized_format);
			}

			pbref.DeclareTypes (types.ToArray(), null);

			IntPtr gcdata = (IntPtr) GCHandle.Alloc (data);
			// Use Int64 in case IntPtr is that size in 64-bit applications.
			byte[] byteArray = BitConverter.GetBytes (gcdata.ToInt64 ());
			pbdata = NSData.FromArray (byteArray);
			pbref.SetDataForType (pbdata, internal_format);

			if (data is ISerializable) {
				MemoryStream stream = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();

				bf.Serialize (stream, data);
				byteArray = stream.GetBuffer ();
				pbdata = NSData.FromArray(byteArray);
				pbref.SetDataForType (pbdata, serialized_format);
			}

//			if (null != keyString) {
//				//FIXME: Convert data to native type.
//				pbref.setData_forType (convertedData, keyString);
//			}
		}

		// Retrieves array of identifiers of available formats. Adds equivalent MWF identifiers.
		internal static int[] GetAvailableFormats(NSPasteboard pboard)
		{
			var ids = new List<int>();
			foreach (var type in pboard.Types)
				AppendTypeIDs(pboard, ids, type);
			return ids.ToArray();
		}

		internal static void AppendTypeIDs(NSPasteboard pboard, List<int> ids, string type)
		{
			var usedIDs = new HashSet<string>();

			switch (type)
			{
				// TODO: Add more identifiers - for images, sounds etc.
				case fmt_public_utf8_plain_text:
					ids.Add(DataFormats.Format.Add(DataFormats.Text).Id);
					break;
				case NSPasteboardTypeHTML:
					ids.Add(DataFormats.Format.Add(DataFormats.Html).Id);
					break;
				case kUTTypeRTF:
					ids.Add(DataFormats.Format.Add(DataFormats.Rtf).Id);
					break;
			}

			// We are mixing MWF pasteboard format identifiers with OSX identifiers (UTIs). Not necessary, maybe useful sometime.
			ids.Add(DataFormats.Format.Add(type).Id);
		}

		internal static NSPasteboard Primary {
			get { return primary_pbref; }
		}
		
		internal static NSPasteboard Application {
			get { return primary_pbref; }
			//get { return app_pbref; }
		}

		internal static string AddMSClipboardMetadata(string html)
		{
			var sb = new StringBuilder();
			sb.AppendLine(Header);
			sb.AppendLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");

			// if given html already provided the fragments we won't add them
			int fragmentStart, fragmentEnd;
			int fragmentStartIdx = html.IndexOf(StartFragment, StringComparison.OrdinalIgnoreCase);
			int fragmentEndIdx = html.LastIndexOf(EndFragment, StringComparison.OrdinalIgnoreCase);

			// if html tag is missing add it surrounding the given html (critical)
			int htmlOpenIdx = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
			int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf('>', htmlOpenIdx) + 1 : -1;
			int htmlCloseIdx = html.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);

			if (fragmentStartIdx < 0 && fragmentEndIdx < 0)
			{
				int bodyOpenIdx = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
				int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf('>', bodyOpenIdx) + 1 : -1;

				if (htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0)
				{
					// the given html doesn't contain html or body tags so we need to add them and place start/end fragments around the given html only
					sb.Append("<html><body>");
					sb.Append(StartFragment);
					fragmentStart = GetByteCount(sb);
					sb.Append(html);
					fragmentEnd = GetByteCount(sb);
					sb.Append(EndFragment);
					sb.Append("</body></html>");
				}
				else
				{
					// insert start/end fragments in the proper place (related to html/body tags if exists) so the paste will work correctly
					int bodyCloseIdx = html.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);

					if (htmlOpenEndIdx < 0)
						sb.Append("<html>");
					else
						sb.Append(html, 0, htmlOpenEndIdx);

					if (bodyOpenEndIdx > -1)
						sb.Append(html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0, bodyOpenEndIdx - (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0));

					sb.Append(StartFragment);
					fragmentStart = GetByteCount(sb);

					var innerHtmlStart = bodyOpenEndIdx > -1 ? bodyOpenEndIdx : (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0);
					var innerHtmlEnd = bodyCloseIdx > -1 ? bodyCloseIdx : (htmlCloseIdx > -1 ? htmlCloseIdx : html.Length);
					sb.Append(html, innerHtmlStart, innerHtmlEnd - innerHtmlStart);

					fragmentEnd = GetByteCount(sb);
					sb.Append(EndFragment);

					if (innerHtmlEnd < html.Length)
						sb.Append(html, innerHtmlEnd, html.Length - innerHtmlEnd);

					if (htmlCloseIdx < 0)
						sb.Append("</html>");
				}
			}
			else
			{
				// handle html with existing start\end fragments just need to calculate the correct bytes offset (surround with html tag if missing)
				if (htmlOpenEndIdx < 0)
					sb.Append("<html>");
				int start = GetByteCount(sb);
				sb.Append(html);
				fragmentStart = start + GetByteCount(sb, start, start + fragmentStartIdx) + StartFragment.Length;
				fragmentEnd = start + GetByteCount(sb, start, start + fragmentEndIdx);
				if (htmlCloseIdx < 0)
					sb.Append("</html>");
			}

			// Back-patch offsets (scan only the header part for performance)
			sb.Replace("<<<<<<<<1", Header.Length.ToString("D9"), 0, Header.Length);
			sb.Replace("<<<<<<<<2", GetByteCount(sb).ToString("D9"), 0, Header.Length);
			sb.Replace("<<<<<<<<3", fragmentStart.ToString("D9"), 0, Header.Length);
			sb.Replace("<<<<<<<<4", fragmentEnd.ToString("D9"), 0, Header.Length);

			return sb.ToString();
		}

		static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
		{
			int count = 0;
			char[] byteCount = new char[1];
			end = end > -1 ? end : sb.Length;
			for (int i = start; i < end; i++)
			{
				byteCount[0] = sb[i];
				count += Encoding.UTF8.GetByteCount(byteCount);
			}
			return count;
		}

		const string Header = "Version:0.9\n\r\nStartHTML:<<<<<<<<1\r\nEndHTML:<<<<<<<<2\r\nStartFragment:<<<<<<<<3\r\nEndFragment:<<<<<<<<4\r\nStartSelection:<<<<<<<<3\r\nEndSelection:<<<<<<<<4";
		const string StartFragment = "<!--StartFragment-->";
		const string EndFragment = @"<!--EndFragment-->";
	}
}

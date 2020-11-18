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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms.Extensions.IO;

namespace System.Windows.Forms {
	public sealed class Clipboard {

		internal const string IDataObjectFormat = "com.novell.mono.mwf.pasteboard.IDataObject";

		#region Local Variables
		#endregion	// Local Variables

		#region Constructors
		private Clipboard() {
		}
		#endregion	// Constructors

		#region	Private Methods
		private static bool ConvertToClipboardData(ref int type, object obj, out byte[] data) {
			data = null;
			return false;
		}

		private static bool ConvertFromClipboardData(int type, IntPtr data, out object obj) {
			obj = null;
			if (data == IntPtr.Zero) {
				return false;
			}
			return false;
		}
		#endregion	// Private Methods

		#region Public Static Methods
		public static void Clear ()
		{
			IntPtr clipboard_handle;

			clipboard_handle = XplatUI.ClipboardOpen (false);
			XplatUI.ClipboardStore (clipboard_handle, null, 0, null, false);
		}

		public static bool ContainsAudio ()
		{
			return ClipboardContainsFormat (DataFormats.WaveAudio);
		}
		
		public static bool ContainsData (string format)
		{
			return ClipboardContainsFormat (format);
		}
		
		public static bool ContainsFileDropList ()
		{
			return ClipboardContainsFormat (DataFormats.FileDrop);
		}
		
		public static bool ContainsImage ()
		{
			return ClipboardContainsFormat (DataFormats.Bitmap);
		}
		
		public static bool ContainsText ()
		{
			return ClipboardContainsFormat (DataFormats.Text, DataFormats.UnicodeText);
		}
		
		public static bool ContainsText (TextDataFormat format)
		{
			switch (format) {
				case TextDataFormat.Text:
					return ClipboardContainsFormat (DataFormats.Text);
				case TextDataFormat.UnicodeText:
					return ClipboardContainsFormat (DataFormats.UnicodeText);
				case TextDataFormat.Rtf:
					return ClipboardContainsFormat (DataFormats.Rtf);
				case TextDataFormat.Html:
					return ClipboardContainsFormat (DataFormats.Html);
				case TextDataFormat.CommaSeparatedValue:
					return ClipboardContainsFormat (DataFormats.CommaSeparatedValue);
			}
			
			return false;
		}
		
		public static Stream GetAudioStream ()
		{
			IDataObject data = GetDataObject ();

			if (data == null)
				return null;

			return (Stream)data.GetData (DataFormats.WaveAudio, true);
		}
		
		public static Object GetData (string format)
		{
			IDataObject data = GetDataObject ();

			if (data == null)
				return null;

			return data.GetData (format, true);
		}

		public static IDataObject GetDataObject ()
		{
			return GetDataObject (false);
		}

		public static StringCollection GetFileDropList ()
		{
			IDataObject data = GetDataObject ();

			if (data == null)
				return null;

			return (StringCollection)data.GetData (DataFormats.FileDrop, true);
		}
		
		public static Image GetImage ()
		{
			IDataObject data = GetDataObject ();

			if (data == null)
				return null;

			return (Image)data.GetData (DataFormats.Bitmap, true);
		}
		
		public static string GetText ()
		{
			return GetText (TextDataFormat.UnicodeText);
		}
		
		public static string GetText (TextDataFormat format)
		{
			if (!Enum.IsDefined (typeof (TextDataFormat), format))
				throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for TextDataFormat", format));
				
			IDataObject data = GetDataObject ();
			
			if (data == null)
				return string.Empty;
				
			string retval;
			
			switch (format) {
				case TextDataFormat.Text:
				default:
					retval = (string)data.GetData (DataFormats.Text, true);
					break;
				case TextDataFormat.UnicodeText:
					retval = (string)data.GetData (DataFormats.UnicodeText, true);
					break;
				case TextDataFormat.Rtf:
					retval = (string)data.GetData (DataFormats.Rtf, true);
					break;
				case TextDataFormat.Html:
					retval = (string)data.GetData (DataFormats.Html, true);
					break;
				case TextDataFormat.CommaSeparatedValue:
					retval = (string)data.GetData (DataFormats.CommaSeparatedValue, true);
					break;
			}
			
			return retval == null ? string.Empty : retval;
		}
		
		public static void SetAudio (byte[] audioBytes)
		{
			if (audioBytes == null)
				throw new ArgumentNullException ("audioBytes");
				
			MemoryStream ms = new MemoryStream (audioBytes);
			
			SetAudio (ms);
		}
		
		public static void SetAudio (Stream audioStream)
		{
			if (audioStream == null)
				throw new ArgumentNullException ("audioStream");

			SetData (DataFormats.WaveAudio, audioStream);
		}

		public static void SetData (string format, Object data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
				
			DataObject data_object = new DataObject (format, data);
			SetDataObject (data_object);
		}

		public static void SetDataObject(object data) {
			SetDataObject(data, false);  // MSDN says default behavior is to place non-persistent data to clipboard
		}

		public static void SetDataObject(object data, bool copy) {
			SetDataObject(data, copy, 10, 100);   // MSDN says default behavior is to try 10 times with 100 ms delay
		}

		internal static void SetDataObjectImpl(object data, bool copy) {
			var converter = new XplatUI.ObjectToClipboard(ConvertToClipboardData);

			var clipboard_handle = XplatUI.ClipboardOpen(false);
			XplatUI.ClipboardStore(clipboard_handle, null, 0, null, copy);	// Empty clipboard

			if (data is IDataObject idata) {
				XplatUI.ClipboardStore(clipboard_handle, idata, DataFormats.GetFormat(IDataObjectFormat).Id, converter, copy);
			} else {
				var format = DataFormats.Format.Find(data.GetType().FullName);
				var id = (format != null) && (format.Name != DataFormats.StringFormat) ? format.Id : -1;
				XplatUI.ClipboardStore(clipboard_handle, data, id, converter, copy);
			}

			XplatUI.ClipboardClose(clipboard_handle);
		}

		static bool IsDataSerializable (object obj)
		{
			if (obj is ISerializable)
				return true;

			AttributeCollection attrs = TypeDescriptor.GetAttributes (obj);
			return attrs [typeof (SerializableAttribute)] != null;
		}

		public static void SetDataObject(object data, bool copy, int retryTimes, int retryDelay)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			if (retryTimes < 0)
				throw new ArgumentOutOfRangeException("retryTimes");
			if (retryDelay < 0)
				throw new ArgumentOutOfRangeException("retryDelay");

			// MS implementation actually puts data to clipboard even when retryTimes == 0
			bool retry = true;
			do
			{
				retry = false;
				--retryTimes;
				try
				{
					SetDataObjectImpl(data, copy);
				} catch (ExternalException) {
					if (retryTimes <= 0)
						throw;
					retry = true;
					Threading.Thread.Sleep(retryDelay);
				}
			} while (retry && retryTimes > 0);
		}

		[MonoInternalNote ("Needs additional checks for valid paths, see MSDN")]
		public static void SetFileDropList (StringCollection filePaths)
		{
			if (filePaths == null)
				throw new ArgumentNullException ("filePaths");
				
			SetData (DataFormats.FileDrop, filePaths);
		}
		
		public static void SetImage (Image image)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			
			SetData (DataFormats.Bitmap, image);
		}
		
		public static void SetText (string text)
		{
			if (string.IsNullOrEmpty (text))
				throw new ArgumentNullException ("text");
				
			SetData (DataFormats.UnicodeText, text);
		}
		
		public static void SetText (string text, TextDataFormat format)
		{
			if (string.IsNullOrEmpty (text))
				throw new ArgumentNullException ("text");
			if (!Enum.IsDefined (typeof (TextDataFormat), format))
				throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for TextDataFormat", format));

			switch (format) {
				case TextDataFormat.Text:
					SetData (DataFormats.Text, text);
					break;
				case TextDataFormat.UnicodeText:
					SetData (DataFormats.UnicodeText, text);
					break;
				case TextDataFormat.Rtf:
					SetData (DataFormats.Rtf, text);
					break;
				case TextDataFormat.Html:
					SetData (DataFormats.Html, text);
					break;
				case TextDataFormat.CommaSeparatedValue:
					SetData (DataFormats.CommaSeparatedValue, text);
					break;
			}
		}
		#endregion	// Public Static Methods

		#region Internal Static Methods
		internal static IDataObject GetDataObject(bool primary_selection)
		{
			IDataObject data = null;

			var clipboard_handle = XplatUI.ClipboardOpen(primary_selection);

			var formats = XplatUI.ClipboardAvailableFormats(clipboard_handle);
			if (formats != null)
			{
				var format = DataFormats.GetFormat(IDataObjectFormat).Id;
				var converter = new XplatUI.ClipboardToObject(ConvertFromClipboardData);
				if (XplatUI.ClipboardRetrieve(clipboard_handle, format, converter) is IDataObject idata)
					data = idata;
			}

			XplatUI.ClipboardClose (clipboard_handle);

			return data ?? new DataObject();
		}
		
		internal static bool ClipboardContainsFormat (params string[] formats)
		{
			var clipboard_handle = XplatUI.ClipboardOpen(false);
			var native_formats = XplatUI.ClipboardAvailableFormats(clipboard_handle);
			
			if (native_formats == null)
				return false;
				
			foreach (int i in native_formats) {
				// We might get a format we don't understand or know
				var item_format = DataFormats.GetFormat(i);
				if (item_format != null)
					if (((IList)formats).Contains(item_format.Name))
						return true;
			}
				
			return false;
		}
		#endregion
	}
}

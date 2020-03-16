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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Forms
{
	public class DataFormats
	{
		public class Format
		{
			static readonly object lockobj = new object ();
			
			private static Format formats;
			private static Dictionary<int, Format> byId = new Dictionary<int, DataFormats.Format>();
			private static Dictionary<string, Format> byName = new Dictionary<string, DataFormats.Format>();

			private string name;
			private int id;
			private Format next;
			internal bool is_serializable;

			public Format (string name, int id)
			{
				this.name = name;
				this.id = id;
				
				lock (lockobj) {
					if (formats == null)
						formats = this;
					else {
						Format f = formats;
						while (f.next != null)
							f = f.next;
						f.next = this;
					}

					byId[id] = this;
					byName[name] = this;
				}
			}

			#region Public Instance Properties
			public int Id {
				get {
					return this.id;
				}
			}

			public string Name {
				get {
					return this.name;
				}
			}

			internal Format Next {
				get {
					return this.next;
				}
			}
			#endregion	// Public Instance Properties

			#region Private Methods
			internal static Format Add (string name)
			{
				Format f;

				f = Find (name);
				if (f == null) {
					IntPtr cliphandle;

					cliphandle = XplatUI.ClipboardOpen (false);
					f = new Format (name, XplatUI.ClipboardGetID (cliphandle, name));
					XplatUI.ClipboardClose (cliphandle);
				}
				return f;
			}

			internal static Format Add (int id) {
				Format f;

				f = Find (id);
				if (f == null)
					f = new Format("Format" + id.ToString(), id);
				return f;
			}

			internal static Format Find (int id) {
				Format value;
				byId.TryGetValue(id, out value);
				return value;
			}

			internal static Format Find (string name) {
				Format value;
				byName.TryGetValue(name, out value);
				return value;
			}

			internal static Format List {
				get {
					return formats;
				}
			}

			internal static int Count
			{
				get { return byId.Count; }
			}

			#endregion	// Private Methods
		}
		
		private DataFormats ()
		{
		}
		
		#region Public Static Fields
		public const string Bitmap				= "Bitmap";
		public const string CommaSeparatedValue	= "Csv";
		public const string Dib					= "DeviceIndependentBitmap";
		public const string Dif					= "DataInterchangeFormat";
		public const string EnhancedMetafile	= "EnhancedMetafile";
		public const string FileDrop			= "FileDrop";
		public const string Html				= "HTML Format";
		public const string Locale				= "Locale";
		public const string MetafilePict		= "MetaFilePict";
		public const string OemText				= "OEMText";
		public const string Palette				= "Palette";
		public const string PenData				= "PenData";
		public const string Riff				= "RiffAudio";
		public const string Rtf					= "Rich Text Format";
		public const string Serializable		= "WindowsForms10PersistentObject";
		public const string StringFormat		= "System.String";
		public const string SymbolicLink		= "SymbolicLink";
		public const string Text				= "Text";
		public const string Tiff				= "Tiff";
		public const string UnicodeText			= "UnicodeText";
		public const string WaveAudio			= "WaveAudio";
		#endregion	// Public Static Fields

		internal const string HtmlStream		= "Html Format";

		private static object lock_object = new object ();
		private static bool initialized;

		// we don't want to force the creation of a new format
		internal static bool ContainsFormat (int id)
		{
			lock (lock_object) {
				if (!initialized)
					Init ();

				return Format.Find (id) != null;
			}
		}

		public static Format GetFormat (int id)
		{
			lock (lock_object) {
				if (!initialized)
					Init ();
				return Format.Find (id);
			}
		}

		public static Format GetFormat (string format)
		{
			lock (lock_object) {
				if (!initialized)
					Init ();
				return Format.Add (format);
			}
		}



		// Assumes we are locked on the lock_object when it is called
		private static void Init ()
		{
			if (initialized)
				return;
			IntPtr cliphandle = XplatUI.ClipboardOpen(false);

			new Format (Text, XplatUI.ClipboardGetID (cliphandle, Text));
			new Format (Bitmap, XplatUI.ClipboardGetID (cliphandle, Bitmap));
			new Format (MetafilePict, XplatUI.ClipboardGetID (cliphandle, MetafilePict));
			new Format (SymbolicLink, XplatUI.ClipboardGetID (cliphandle, SymbolicLink));
			new Format (Dif, XplatUI.ClipboardGetID (cliphandle, Dif)) ;
			new Format (Tiff, XplatUI.ClipboardGetID (cliphandle, Tiff));
			new Format (OemText, XplatUI.ClipboardGetID (cliphandle, OemText));
			new Format (Dib, XplatUI.ClipboardGetID (cliphandle, Dib));
			new Format (Palette, XplatUI.ClipboardGetID (cliphandle, Palette));
			new Format (PenData, XplatUI.ClipboardGetID (cliphandle, PenData));
			new Format (Riff, XplatUI.ClipboardGetID (cliphandle, Riff));
			new Format (WaveAudio, XplatUI.ClipboardGetID (cliphandle, WaveAudio));
			new Format (UnicodeText, XplatUI.ClipboardGetID (cliphandle, UnicodeText));
			new Format (EnhancedMetafile, XplatUI.ClipboardGetID (cliphandle, EnhancedMetafile));
			new Format (FileDrop, XplatUI.ClipboardGetID (cliphandle, FileDrop));
			new Format (Locale, XplatUI.ClipboardGetID (cliphandle, Locale));
			new Format (CommaSeparatedValue, XplatUI.ClipboardGetID (cliphandle, CommaSeparatedValue));
			new Format (Html, XplatUI.ClipboardGetID (cliphandle, Html));
			new Format (Rtf, XplatUI.ClipboardGetID (cliphandle, Rtf));
			new Format (Serializable, XplatUI.ClipboardGetID (cliphandle, Serializable));
			new Format (StringFormat, XplatUI.ClipboardGetID (cliphandle, StringFormat));

			XplatUI.ClipboardClose (cliphandle);

			initialized = true;
		}
	}
}

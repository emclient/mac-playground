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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Collections.Generic;

namespace System.Windows.Forms.CocoaInternal {
	internal class Pasteboard {
		private static NSPasteboard primary_pbref;
		private static NSPasteboard app_pbref;

		internal static readonly string internal_format;
		internal static readonly string serialized_format;

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

		internal static object Retrieve (NSPasteboard pbref, int key)
		{
//			UInt32 count = 0;

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

		internal static void Store (NSPasteboard pbref, object data, int key)
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

		internal static NSPasteboard Primary {
			get { return primary_pbref; }
		}
		
		internal static NSPasteboard Application {
			get { return app_pbref; }
		}

//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern IntPtr CFDataCreate (IntPtr allocator, ref IntPtr buf, Int32 length);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern IntPtr CFDataGetBytePtr (IntPtr data);

//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int PasteboardClear (IntPtr pbref);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int PasteboardCreate (IntPtr str, ref IntPtr pbref);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int PasteboardCopyItemFlavorData (IntPtr pbref, UInt32 itemid, UInt32 key, ref IntPtr data);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int PasteboardGetItemCount (IntPtr pbref, ref UInt32 count);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int PasteboardGetItemIdentifier (IntPtr pbref, UInt32 itemindex, ref UInt32 itemid);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int PasteboardPutItemFlavor (IntPtr pbref, UInt32 itemid, UInt32 key, IntPtr data, UInt32 flags);
	}
}

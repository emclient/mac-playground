//
//Dnd.cs
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
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using MonoMac.Foundation;
using MonoMac.AppKit;
using NSPoint = System.Drawing.PointF;

namespace System.Windows.Forms.CocoaInternal {
	internal delegate void DragTrackingDelegate (short message, IntPtr window, IntPtr data, IntPtr dragref);

	internal class Dnd
	{
		internal const uint kEventParamDragRef = 1685217639; 
		internal const uint typeDragRef = 1685217639;

		internal const uint typeMono = 1836019311;
		internal const uint typeMonoSerializedObject = 1836279154;

		private static DragDropEffects effects = DragDropEffects.None;
		private static DragTrackingDelegate DragTrackingHandler = new DragTrackingDelegate (TrackingCallback);
		private readonly string[] defaultPBTypesForDragReceivers;

		static Dnd () {
			InstallTrackingHandler (DragTrackingHandler, IntPtr.Zero, IntPtr.Zero);
		}

		internal Dnd ()
		{
			defaultPBTypesForDragReceivers = new[] { NSPasteboard.NSMultipleTextSelectionType,
				NSPasteboard.NSStringType, NSPasteboard.NSFilenamesType, NSPasteboard.NSHtmlType,
				NSPasteboard.NSUrlType, //NSPasteboard.NSInkTextType,
				Pasteboard.internal_format, Pasteboard.serialized_format };
		}

		internal static void TrackingCallback (short message, IntPtr window, IntPtr data, IntPtr dragref) {
			XplatUICocoa.GetInstance ().FlushQueue ();
		}
	
		internal static DragDropEffects DragActionsToEffects (UInt32 actions) {
			DragDropEffects effects = DragDropEffects.None;
			if ((actions & 1) != 0)
				effects |= DragDropEffects.Copy;
			if ((actions & (1 << 4)) != 0)
				effects |= DragDropEffects.Move;
			if ((actions & 0xFFFFFFFF) != 0)
				effects |= DragDropEffects.All;

			return effects;
		}

		internal static IDataObject DragToDataObject (NSPasteboard pasteboard)
		{
			var flavors = pasteboard.Types;
			ArrayList flavorlist = new ArrayList ();

			foreach (string flavorref in flavors) {
				uint flavor_counter = 1;
				FlavorHandler flavor = new FlavorHandler (pasteboard, flavorref, flavor_counter);
				if (flavor.Supported)
					flavorlist.Add (flavor);
			}

			if (flavorlist.Count > 0)
				return ((FlavorHandler) flavorlist [0]).Convert (flavorlist);

			return new DataObject ();
		}

		internal static bool HandleEvent (NSObject callref, NSEvent eventref, NSView vuWrap, uint kind, ref MSG msg)
		{
			Control control;
			IDataObject data;
			DragEventArgs drag_event;
			DragDropEffects allowed;
			Point point;
			UInt32 actions = 0;
			NSPasteboard pasteboard = NSPasteboard.FromName (NSPasteboard.NSDragPasteboardName);
			IntPtr handle = (IntPtr) vuWrap.Handle;
			Hwnd hwnd = Hwnd.ObjectFromWindow (handle);

			if (hwnd == null || hwnd.Handle != handle)
				return false;

//			GetEventParameter (eventref, kEventParamDragRef, typeDragRef, IntPtr.Zero, (uint) Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref pasteboard);
			point = XplatUI.State.MousePosition;
			//GetDragAllowableActions (pasteboard, ref actions);
			control = Control.FromHandle (hwnd.Handle);
			allowed = DragActionsToEffects (actions);
			data = DragToDataObject (pasteboard);
			drag_event = new DragEventArgs (data, 0, (int) point.X, (int) point.Y, allowed, DragDropEffects.None);

			switch (kind) {
				case ControlHandler.kEventControlDragEnter: {
					bool accept = control.AllowDrop;
					//SetEventParameter (eventref, ControlHandler.kEventParamControlLikesDrag, ControlHandler.typeBoolean, (uint)Marshal.SizeOf (typeof (bool)), ref accept);

					control.DndEnter (drag_event);
					effects = drag_event.Effect;
					return false;
				}
				case ControlHandler.kEventControlDragWithin:
					control.DndOver (drag_event);
					effects = drag_event.Effect;
					break;
				case ControlHandler.kEventControlDragLeave:
					control.DndLeave (drag_event);
					break;
				case ControlHandler.kEventControlDragReceive:
					control.DndDrop (drag_event);
					break;
			}
			return true;
		}

		public void SetAllowDrop (Hwnd hwnd, bool allow)
		{
			if (hwnd.allow_drop == allow)
				return;

			hwnd.allow_drop = allow;

			NSView vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);
			if (allow)
				vuWrap.RegisterForDraggedTypes (defaultPBTypesForDragReceivers);
			else
				vuWrap.UnregisterDraggedTypes ();
		}

		public void SendDrop (IntPtr handle, IntPtr from, IntPtr time) {
		}

		public DragDropEffects StartDrag (IntPtr handle, object data, DragDropEffects allowed_effects) {
			IntPtr dragref = IntPtr.Zero;
			EventRecord eventrecord = new EventRecord ();

			effects = DragDropEffects.None;

			NewDrag (ref dragref);
			eventrecord.mouse = NSEvent.CurrentMouseLocation;
			StoreObjectInDrag (handle, dragref, data);

			TrackDrag (dragref, ref eventrecord, IntPtr.Zero);

			DisposeDrag (dragref);

			return effects;
		}

		public void StoreObjectInDrag (IntPtr handle, IntPtr dragref, object data) {
			IntPtr type = IntPtr.Zero;
			IntPtr dataptr = IntPtr.Zero;
			Int32 size = 0;

			if (data is string) {
				// implement me
				throw new NotSupportedException ("Implement me.");
			} else if (data is ISerializable) {
				MemoryStream stream = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();

				bf.Serialize (stream, data);

				dataptr = Marshal.AllocHGlobal ((int) stream.Length);
				stream.Seek (0, 0);

				for (int i = 0; i < stream.Length; i++) {
					Marshal.WriteByte (dataptr, i, (byte) stream.ReadByte ());
				}
				
				type = (IntPtr) typeMonoSerializedObject;
				size = (int) stream.Length;
			} else {
				dataptr = (IntPtr) GCHandle.Alloc (data);

				type = (IntPtr) typeMono;
				size = Marshal.SizeOf (typeof (IntPtr));
			}

			AddDragItemFlavor (dragref, handle, type, ref dataptr, size, 1 << 0);
		}

		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		static extern int InstallTrackingHandler (DragTrackingDelegate callback, IntPtr window, IntPtr data);

//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref IntPtr data);
		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		static extern int SetEventParameter (IntPtr eventref, uint name, uint type, uint size, ref bool data);

//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int SetControlDragTrackingEnabled (IntPtr view, bool enabled);
		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		extern static int AddDragItemFlavor (IntPtr dragref, IntPtr itemref, IntPtr flavortype, ref IntPtr data, Int32 size, UInt32 flags);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int CountDragItems (IntPtr dragref, ref UInt32 count);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int CountDragItemFlavors (IntPtr dragref, IntPtr itemref, ref UInt32 count);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int GetDragItemReferenceNumber (IntPtr dragref, UInt32 index, ref IntPtr itemref);
		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		extern static int NewDrag (ref IntPtr dragref);
		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		extern static int TrackDrag (IntPtr dragref, ref EventRecord eventrecord, IntPtr region);
		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		extern static int DisposeDrag (IntPtr dragref);
		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
		extern static int GetDragAllowableActions (IntPtr dragref, ref UInt32 actions);
	}

	internal struct EventRecord {
		internal UInt16 what;
		internal UInt32 message;
		internal UInt32 when;
		internal NSPoint mouse;
		internal UInt16 modifiers;
	}

	internal class FlavorHandler {
		internal string flavorref;
		internal NSPasteboard pasteboard;
//		internal IntPtr itemref;
		internal Int32 size;
//		internal UInt32 flags;
		internal byte [] data;
//		internal string fourcc;

		internal FlavorHandler (NSPasteboard pasteboard, string flavorref, uint counter)
		{
//			GetFlavorType (dragref, flavorref, counter, ref flavorref);
//			GetFlavorFlags (dragref, flavorref, flavorref, ref flags);
//			byte [] fourcc_b = BitConverter.GetBytes ((Int32)flavorref);
//			this.fourcc = String.Format ("{0}{1}{2}{3}", (char)fourcc_b [3], (char)fourcc_b [2], (char)fourcc_b [1], (char)fourcc_b [0]);
			this.pasteboard = pasteboard;
			this.flavorref = flavorref;

			this.GetData ();
		}

		internal void GetData ()
		{
			NSData pbdata = pasteboard.GetDataForType (flavorref);
			//data = pbdata.bytes (); -- FIXME
		}

		internal string DataString {
			get { return Encoding.Default.GetString (this.data); }
		}
		
		internal byte[] DataArray {
			get { return this.data; }
		}

		internal IntPtr DataPtr {
			get { return (IntPtr) BitConverter.ToInt32 (this.data, 0); }
		}

		internal bool Supported {
			get {
				return flavorref == NSPasteboard.NSUrlType || flavorref == Pasteboard.internal_format ||
					flavorref == Pasteboard.serialized_format;
			}
		}

		internal IDataObject Convert (ArrayList flavorlist)
		{
			if (flavorref == NSPasteboard.NSUrlType)
					return ConvertToFileDrop (flavorlist);
			if (flavorref == Pasteboard.internal_format)
					return ConvertToObject (flavorlist);
			if (flavorref == Pasteboard.serialized_format)
					return DeserializeObject (flavorlist);

			return new DataObject ();
		}

		internal IDataObject DeserializeObject (ArrayList flavorlist) {
			MemoryStream stream = new MemoryStream (this.DataArray);

			if (stream.Length == 0)
				return new DataObject ();

			stream.Seek (0, 0);
			BinaryFormatter bf = new BinaryFormatter ();
			object dataq = bf.Deserialize (stream);
			IDataObject data = dataq as IDataObject;
			if (null == data)
				data = new DataObject (dataq);

			return data;
		}

		internal IDataObject ConvertToObject (ArrayList flavorlist) {
			DataObject data = new DataObject ();
			
			foreach (FlavorHandler flavor in flavorlist) {
				GCHandle handle = (GCHandle) flavor.DataPtr;

				data.SetData (handle.Target);
			}
			
			return data;
		}

		internal IDataObject ConvertToFileDrop (ArrayList flavorlist) {
			DataObject data = new DataObject ();
			ArrayList uri_list = new ArrayList ();

			foreach (FlavorHandler flavor in flavorlist) {
				try {
					uri_list.Add (new Uri (flavor.DataString).LocalPath);
				} catch { }
			}

			string [] l = (string []) uri_list.ToArray (typeof (string));
			if (l.Length < 1)
				return data;
			data.SetData (DataFormats.FileDrop, l);
			data.SetData ("FileName", l [0]);
			data.SetData ("FileNameW", l [0]);
			
			return data;
		}

		public override string ToString ()
		{
			return flavorref;
		}

//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int GetFlavorDataSize (IntPtr dragref, IntPtr itemref, IntPtr flavorref, ref Int32 size);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int GetFlavorData (IntPtr dragref, IntPtr itemref, IntPtr flavorref, [In, Out] byte[] data, ref Int32 size, UInt32 offset);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int GetFlavorFlags (IntPtr dragref, IntPtr itemref, IntPtr flavorref, ref UInt32 flags);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		extern static int GetFlavorType (IntPtr dragref, IntPtr itemref, UInt32 index, ref IntPtr flavor);
	}
}

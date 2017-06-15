﻿using System.Windows.Forms.CocoaInternal;
using System.Windows.Forms.Mac;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

#if XAMARINMAC
using AppKit;
using Foundation;
using CoreGraphics;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif

namespace System.Windows.Forms
{
	internal partial class XplatUICocoa
	{
		internal const string IDataObjectPboardType = "mwf.idataobject";

		internal const string UTTypeData = "public.data";
		internal const string UTTypeFileUrl = "public.file-url";
		internal const string UTTypeItem = "public.item";
		internal const string UTTypeImage = "public.image";
		internal const string UTTypeAudio = "public.audio";
		internal const string UTTypeVideo = "public.video";
		internal const string UTTypeUTF8PlainText = "public.utf8-plain-text";
		internal const string UTTypeEmailMessage = "public.email-message";
		internal const string UTTypeVCard = "public.vcard";
		internal const string UTTypeContact = "public.contact";
		internal const string UTTypeToDoItem = "public.to-do-item";
		internal const string UTTypeCalendarEvent = "public.calendar-event";
		internal const string UTTypePDF = "com.adobe.pdf";

		internal const string PasteboardTypeFileURLPromise = "com.apple.pasteboard.promised-file-url";
		internal const string PasteboardTypeFilePromiseContent = "com.apple.pasteboard.promised-file-content-type";

		internal const string NSStringPboardType = "NSStringPboardType";
		internal const string NSFilenamesPboardType = "NSFilenamesPboardType";
		internal const string NSFilesPromisePboardType = "NSFilesPromisePboardType";

		internal const string CFSTR_FILEDESCRIPTORW = "FileGroupDescriptorW";
		internal const string CFSTR_FILECONTENTS = "FileContents";

		internal static object DraggedData = null;
		internal static DragDropEffects DraggingAllowedEffects = DragDropEffects.None;
		internal static DragDropEffects DraggingEffects = DragDropEffects.None;
		internal static event EventHandler DraggingEnded;

		internal DraggingSource draggingSource = new DraggingSource();
		internal FileProvider dndFileProvider;
		internal string[] dndFilenames;
		internal int dndCurrentFileIndex;

		internal NSEvent lastMouseEvent = null;

		internal override void SetAllowDrop(IntPtr handle, bool value)
		{
			if (ObjCRuntime.Runtime.GetNSObject(handle) is MonoView view)
			{
				if (value)
					view.RegisterForDraggedTypes(new string[] { IDataObjectPboardType, UTTypeItem });//, NSPasteboard.NSStringType });
				else
					view.UnregisterDraggedTypes();
			}
		}

		internal override DragDropEffects StartDrag(IntPtr handle, object data, DragDropEffects allowedEffects)
		{
			if (ObjCRuntime.Runtime.GetNSObject(handle) is MonoView view)
			{
				if (Grab.Hwnd != IntPtr.Zero)
					UngrabWindow(Grab.Hwnd);

				var items = CreateDraggingItems(view, DraggedData = data);
				if (items != null && items.Length != 0)
				{
					view.BeginDraggingSession(items, lastMouseEvent, draggingSource);
					DraggingAllowedEffects = allowedEffects;
					DraggingEffects = DragDropEffects.None;
				}
			}

			return allowedEffects;
		}

		internal virtual NSDraggingItem[] CreateDraggingItems(NSView view, object data)
		{
			var maxSize = new CGSize(320, 240);
			var size = ScaleToFit(view.Bounds.Size, maxSize);
			var location = view.ConvertPointFromView(lastMouseEvent.LocationInWindow, null);
			var bounds = new CGRect(location.Move(-4, -4), size);

			var items = new List<NSDraggingItem>();

			DraggedData = (data is IDataObject) ? data : new DataObject(data);

			if (data is IDataObject idata)
			{
				if (idata.GetDataPresent(CFSTR_FILEDESCRIPTORW))
					foreach (var promise in CreateFilePromises(idata))
						items.Add(new NSDraggingItem(promise.AsPasteboardWriting()));
			}

			if (data is String text)
			{
				var pbitem = NewPasteboardItem();
				pbitem.SetStringForType(text, UTTypeUTF8PlainText);
				var item = new NSDraggingItem(pbitem.AsPasteboardWriting());
				items.Add(item);
			}

			var snapshot = TakeSnapshot(view); // FIXME
			foreach (var item in items)
				item.SetDraggingFrame(bounds, snapshot);

			return items.ToArray();
		}

		internal NSPasteboardItem NewPasteboardItem()
		{
			var item = new NSPasteboardItem();
			item.SetDataForType(new NSData(), IDataObjectPboardType); // Tells NSMonoView to look for XplatIUCocoa.DraggedData
			return item;
		}

		internal List<NSPasteboardItem> CreateFilePromises(IDataObject idata)
		{
			var items = new List<NSPasteboardItem>();
			dndFilenames = GetFilenames(idata);

			if (dndFilenames.Length > 0)
			{
				dndFileProvider = new FileProvider(this);
				foreach (var filename in dndFilenames)
				{
					var item = NewPasteboardItem();
					item.SetDataProviderForTypes(dndFileProvider, new string[] { PasteboardTypeFileURLPromise });
					items.Add(item);
				}
			}
			return items;
		}

		private string ContentTypeFromFilename(string filename)
		{
			var extension = IO.Path.GetExtension(filename).Replace(".", "").ToLower();
			switch (extension)
			{
				case "eml": return UTTypeEmailMessage;
				case "vcf": return UTTypeContact;
				case "ics": return UTTypeCalendarEvent;
				case "bmp": case "gif": case "ico": case "jpg": case "jpeg": case "pict":
				case "png": case "tiff": case "raw": return UTTypeImage;
				case "mpg": case "mpeg": case "mp4": case "mkv": case "avi": case "wmv": case "3gp": return UTTypeVideo;
				case "mp3": case "wma": return UTTypeAudio;
				case "pdf": return UTTypePDF;
				default: return UTTypeData;
			}
		}

		// Reads filenames from Win32.FILEGROUPDESCRIPTORW structure
		internal string[] GetFilenames(IDataObject idata)
		{
			if (idata != null && idata.GetData(CFSTR_FILEDESCRIPTORW) is Stream stream)
			{
				using (var reader = new BinaryReader(stream))
				{
					var count = reader.ReadInt32();
					var firstItemOffset = stream.Position;
					var itemSize = (stream.Length - firstItemOffset) / count;
					const int filenameOffset = 72;
					const int filenameFieldLength = 520;

					var filenames = new string[count];
					for (int i = 0; i < count; ++i)
					{
						stream.Position = firstItemOffset + i * itemSize + filenameOffset;
						var bytes = reader.ReadBytes(filenameFieldLength);
						var filename = Encoding.Unicode.GetString(bytes, 0, bytes.GetUnicodeStringLength());
						filenames[i] = filename;
					}

					return filenames;
				}
			}
			return new string[] { };
		}

		internal void ProvideDataForType(NSPasteboard pasteboard, NSPasteboardItem item, string type)
		{
			if (type == PasteboardTypeFileURLPromise && DraggedData is IDataObject idata)
			{
				var location = pasteboard.GetStringForType("com.apple.pastelocation");
				var folder = new NSUrl(location).Path;

				if (DraggedData is Runtime.InteropServices.ComTypes.IDataObject cdata)
				{
					var filename = dndFilenames[dndCurrentFileIndex];
					var unique = GenerateUniqueFilename(folder, filename);
					var path = Path.Combine(folder, unique);
					var stream = GetStream(cdata, dndCurrentFileIndex);
					using (var outputStream = File.Create(path))
						stream.CopyTo(outputStream);
				}
			}
		}

		internal IStream GetStream(System.Runtime.InteropServices.ComTypes.IDataObject cdata, int index)
		{
			var format = new FORMATETC
			{
				cfFormat = (short)DataFormats.GetFormat(CFSTR_FILECONTENTS).Id,
				dwAspect = DVASPECT.DVASPECT_CONTENT,
				tymed = TYMED.TYMED_ISTREAM,
				lindex = index
			};

			cdata.GetData(ref format, out STGMEDIUM medium);

			if (medium.tymed == format.tymed)
				if (Marshal.GetObjectForIUnknown(medium.unionmember) is IStream stream)
					return stream;

			return null;
		}

		internal static string GenerateUniqueFilename(string folder, string filename)
		{
			string name = Path.GetFileNameWithoutExtension(filename);
			string ext = Path.GetExtension(filename);
			string suffix = "";
			for (int i = 1; i < int.MaxValue; ++i)
			{
				var unique = name + suffix + ext;
				if (!File.Exists(Path.Combine(folder, unique)))
					return unique;
				suffix = $" {i}";
			}

			throw new ApplicationException("Failed to create unique filename");
		}

		internal void FinishedWithDataProvider(NSPasteboard pasteboard)
		{
			//dndFileProvider = null;
			//dndFilenames = null;
		}

		internal static NSImage TakeSnapshot(NSView view)
		{
			var b = view.BitmapImageRepForCachingDisplayInRect(view.Bounds);
			view.CacheDisplay(view.Bounds, b);

			var i = new NSImage(view.Bounds.Size);
			i.AddRepresentation(b);

			return i;
		}

		internal static CGSize ScaleToFit(CGSize val, CGSize max)
		{
			var kw = max.Width / val.Width;
			var kh = max.Height / val.Height;

			var k = Math.Min(kw, kh);
			k = Math.Min(k, 1.0f);

			return new CGSize(k * val.Width, k * val.Height);
		}

		internal static NSImage ResizeImage(NSImage image, CGSize size)
		{
			var resized = new NSImage(size);
			resized.LockFocus();
			NSGraphicsContext.CurrentContext.ImageInterpolation = NSImageInterpolation.High;
			image.DrawInRect(new CGRect(0, 0, size.Width, size.Height), new CGRect(CGPoint.Empty, image.Size), NSCompositingOperation.Copy, 1.0f);
			resized.UnlockFocus();
			return resized;
		}

		internal class DraggingSource : NSDraggingSource
		{
			public override void DraggedImageBeganAt(NSImage image, CGPoint screenPoint)
			{
				//Console.WriteLine("DraggingSource.DraggedImageBeganAt");
			}

			public override void DraggedImageEndedAtOperation(NSImage image, CGPoint screenPoint, NSDragOperation operation)
			{
				//Console.WriteLine($"MonoDraggingSource.DraggedImageEndedAtOperation({screenPoint.X},{screenPoint.Y},{operation}");

				XplatUICocoa.DraggedData = null;
				XplatUICocoa.DraggingEffects = operation.ToDragDropEffects();
				XplatUICocoa.DraggingEnded(this, new EventArgs());
			}

			public override void DraggedImageMovedTo(NSImage image, CGPoint screenPoint)
			{
				// Jde pouzit napr. k vypoctu polohy v textu pro vlozeni
				//Console.WriteLine("DraggedImageMovedTo");
			}

			// This would be called only if we started dragging with DragPromisedFilesOfTypes() or if we called it ourselves
			public override string[] NamesOfPromisedFilesDroppedAtDestination(NSUrl dropDestination)
			{
				//Console.WriteLine("MonoDraggingSource.NamesOfPromisedFilesDroppedAtDestination");
				return new string[] { };
			}

			public override NSDragOperation DraggingSourceOperationMaskForLocal(bool flag)
			{
				return NSDragOperation.Copy;
			}

		}

#if XAMARINMAC

		internal class FileProvider : NSPasteboardItemDataProvider
		{
			XplatUICocoa driver;

			public FileProvider(XplatUICocoa driver)
			{
				this.driver = driver;
				driver.dndCurrentFileIndex = 0;
			}

			public override void FinishedWithDataProvider(NSPasteboard pasteboard)
			{
				//Console.WriteLine("FileProvider.FinishedWithDataProvider");
				driver.FinishedWithDataProvider(pasteboard);
			}

			public override void ProvideDataForType(NSPasteboard pasteboard, NSPasteboardItem item, string type)
			{
				driver.ProvideDataForType(pasteboard, item, type);
				driver.dndCurrentFileIndex += 1;
			}
		}

#elif MONOMAC

		internal class FileProvider : NSObject
		{
			XplatUICocoa driver;

			public string[] Filenames;
			public int CurrentIndex;

			public FileProvider(XplatUICocoa driver)
			{
				this.driver = driver;
				driver.dndCurrentFileIndex = 0;
			}

			public override bool ConformsToProtocol(IntPtr protocol)
			{
				//Console.WriteLine(NSString.FromHandle(Extensions.NSStringFromProtocol(protocol)));
				if ("NSPasteboardItemDataProvider" == NSString.FromHandle(Extensions.NSStringFromProtocol(protocol)))
					return true;

				return base.ConformsToProtocol(protocol);
			}

			[Export("pasteboardFinishedWithDataProvider:")]
			public virtual void FinishedWithDataProvider(NSPasteboard pasteboard)
			{
				//Console.WriteLine("FileProvider.FinishedWithDataProvider");
				driver.FinishedWithDataProvider(pasteboard);
			}

			// Using IntPtr instead of NSPasteboardItem prevents crashes in Marshaller under MonoMac.
			[Export("pasteboard:item:provideDataForType:")]
			public virtual void ProvideDataForType(NSPasteboard pasteboard, IntPtr hItem, string type)
			{
				var obj = ObjCRuntime.Runtime.GetNSObject(hItem);
				var item = obj is NSPasteboardItem ? (NSPasteboardItem)obj : new NSPasteboardItem(hItem);

				//Console.WriteLine($"FileProvider.ProvideDataForType({pasteboard.GetType().Name},{item.GetType().Name},{type.GetType().Name}/{type})");
				driver.ProvideDataForType(pasteboard, item, type);
				driver.dndCurrentFileIndex += 1;
			}
		}
#endif
	}
}

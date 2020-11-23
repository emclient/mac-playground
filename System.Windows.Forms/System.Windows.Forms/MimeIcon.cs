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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//
//  Alexander Olk	alex.olk@googlemail.com
//

// use
// public static int GetIconIndexForFile( string full_filename )
// public static int GetIconIndexForMimeType( string mime_type )
// to get the image index in MimeIconEngine.SmallIcons and MimeIconEngine.LargeIcons

using System;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Windows.Forms
{
	internal enum MimeExtensionHandlerStatus
	{
		OK,
		ERROR
	}
	
	internal enum EPlatformHandler
	{
		Default,
		GNOME
	}
	
	internal class ResourceImageLoader
	{
		static Assembly assembly = typeof (ResourceImageLoader).Assembly;
		
		static internal Bitmap Get (string name)
		{
			Stream stream = assembly.GetManifestResourceStream (name);

			if (stream == null) {
				Console.WriteLine ("Failed to read {0}", name);
				return null;
			}
				
			return new Bitmap (stream);
		}
		
		static internal Icon GetIcon (string name)
		{
			Stream stream = assembly.GetManifestResourceStream (name);

			if (stream == null) {
				Console.WriteLine ("Failed to read {0}", name);
				return null;
			}

			return new Icon (stream);
		}
	}
	
	internal class MimeIconEngine
	{
		public static ImageList SmallIcons = new ImageList();
		public static ImageList LargeIcons = new ImageList();
		
		private static EPlatformHandler platform = EPlatformHandler.Default;
		
		internal static Hashtable MimeIconIndex = new Hashtable ();
		
		private static PlatformMimeIconHandler platformMimeHandler = null;
		
		private static object lock_object = new Object();
		
		static MimeIconEngine ()
		{
			SmallIcons.ColorDepth = ColorDepth.Depth32Bit;
			SmallIcons.TransparentColor = Color.Transparent;
			LargeIcons.ColorDepth = ColorDepth.Depth32Bit;
			LargeIcons.TransparentColor = Color.Transparent;

			SmallIcons.ImageSize = new Size (16, 16);
			LargeIcons.ImageSize = new Size (48, 48);
				
			platformMimeHandler = new PlatformDefaultHandler ();
			platformMimeHandler.Start ();
		}
		
		public static int GetIconIndexForFile (string full_filename)
		{
			lock (lock_object) {
				if (platform == EPlatformHandler.Default) {
					return (int)MimeIconIndex ["unknown/unknown"];
				}
				
				string mime_type = Mime.GetMimeTypeForFile (full_filename);
				
				object oindex = GetIconIndex (mime_type);
				
				// not found, add it
				if (oindex == null) {
					int index = full_filename.IndexOf (':');
					
					if (index > 1) {
						oindex = MimeIconIndex ["unknown/unknown"];
						
					} else {
						oindex = platformMimeHandler.AddAndGetIconIndex (full_filename, mime_type);
						
						// sanity check
						if (oindex == null)
							oindex = MimeIconIndex ["unknown/unknown"];
					}
				}
				
				return (int)oindex;
			}
		}
		
		public static int GetIconIndexForMimeType (string mime_type)
		{
			lock (lock_object) {
				if (platform == EPlatformHandler.Default) {
					if (mime_type == "inode/directory") {
						return (int)MimeIconIndex ["inode/directory"];
					} else {
						return (int)MimeIconIndex ["unknown/unknown"];
					}
				}
				
				object oindex = GetIconIndex (mime_type);
				
				// not found, add it
				if (oindex == null) {
					oindex = platformMimeHandler.AddAndGetIconIndex (mime_type);
					
					// sanity check
					if (oindex == null)
						oindex = MimeIconIndex ["unknown/unknown"];
				}
				
				return (int)oindex;
			}
		}
		
		public static Image GetIconForMimeTypeAndSize (string mime_type, Size size)
		{
			lock (lock_object) {
				object oindex = GetIconIndex (mime_type);
				
				Bitmap bmp = new Bitmap (LargeIcons.Images [(int)oindex], size);
				
				return bmp;
			}
		}
		
		internal static void AddIconByImage (string mime_type, Image image)
		{
			int index = SmallIcons.Images.Add (image, Color.Transparent);
			LargeIcons.Images.Add (image, Color.Transparent);
			
			MimeIconIndex.Add (mime_type, index);
		}
		
		private static object GetIconIndex (string mime_type)
		{
			object oindex = null;
			
			if (mime_type != null) {
				// first check if mime_type is available in the mimetype/icon hashtable
				oindex = MimeIconIndex [mime_type];
				
				if (oindex == null) {
					// it is not available, check if an alias exist for mime_type
					string alias = Mime.GetMimeAlias (mime_type);
					
					if (alias != null) {
						string[] split = alias.Split (new char [] { ',' });
						
						for (int i = 0; i < split.Length; i++) {
							oindex = MimeIconIndex [split [i]];
							
							if (oindex != null)
								return oindex;
						}
					}
					
					// if oindex is still null check if mime_type is a sub class of an other mime type
					string sub_class = Mime.SubClasses [mime_type];
					
					if (sub_class != null) {
						oindex = MimeIconIndex [sub_class];
						
						if (oindex != null)
							return oindex;
					}
					
					// last check, see if we find an entry for the main mime type class
					string mime_class_main = mime_type.Substring (0, mime_type.IndexOf ('/'));
					return MimeIconIndex [mime_class_main];
				}
			}
			
			return oindex;
		}
	}
	
	internal abstract class PlatformMimeIconHandler
	{
		protected MimeExtensionHandlerStatus mimeExtensionHandlerStatus = MimeExtensionHandlerStatus.OK;
		
		public MimeExtensionHandlerStatus MimeExtensionHandlerStatus {
			get {
				return mimeExtensionHandlerStatus;
			}
		}
		
		public abstract MimeExtensionHandlerStatus Start ();
		
		public virtual object AddAndGetIconIndex (string filename, string mime_type)
		{
			return null;
		}
		
		public virtual object AddAndGetIconIndex (string mime_type)
		{
			return null;
		}
	}
	
	internal class PlatformDefaultHandler : PlatformMimeIconHandler
	{
		public override MimeExtensionHandlerStatus Start ()
		{
			MimeIconEngine.AddIconByImage ("inode/directory",  ResourceImageLoader.Get ("folder.png"));
			MimeIconEngine.AddIconByImage ("unknown/unknown",  ResourceImageLoader.Get ("text-x-generic.png"));
			MimeIconEngine.AddIconByImage ("desktop/desktop",  ResourceImageLoader.Get ("user-desktop.png"));
			MimeIconEngine.AddIconByImage ("directory/home",  ResourceImageLoader.Get ("user-home.png"));
			
			MimeIconEngine.AddIconByImage ("network/network",  ResourceImageLoader.Get ("folder-remote.png"));
			MimeIconEngine.AddIconByImage ("recently/recently",  ResourceImageLoader.Get ("document-open.png"));
			MimeIconEngine.AddIconByImage ("workplace/workplace",  ResourceImageLoader.Get ("computer.png"));
			
			return MimeExtensionHandlerStatus.OK; // return always ok
		}
	}
}


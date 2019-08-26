//
// System.Drawing.Image.cs
//
// Authors: 	Christian Meyer (Christian.Meyer@cs.tum.edu)
// 		Alexandre Pigolkine (pigolkine@gmx.de)
//		Jordi Mas i Hernandez (jordi@ximian.com)
//		Sanjay Gupta (gsanjay@novell.com)
//		Ravindra (rkumar@novell.com)
//		Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
// Copyright 2011-2013 Xamarin Inc.
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

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

#if XAMARINMAC
using CoreGraphics;
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace System.Drawing {
	
	[Serializable]
	[TypeConverter (typeof (ImageConverter))]
	public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable {

		public delegate bool GetThumbnailImageAbort();

		// This is obtained from a Bitmap
		// Right now that is all we support
		internal CGImage NativeCGImage;
		protected ColorPalette palette;

		// This is obtained from a PDF file.  Not supported right now.
		internal CGPDFDocument nativeMetafile;
		internal CGPDFPage nativeMetafilePage;
		string tag = string.Empty;
		internal SizeF physicalSize;

		internal CGAffineTransform imageTransform;
		protected ImageFlags pixelFlags;


		~Image ()
		{
			Dispose (false);
			GC.SuppressFinalize (this);
		}
		
		[DefaultValue (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Height {
			get {
				if (NativeCGImage != null)
					return (int)NativeCGImage.Height;
				else if (nativeMetafilePage != null)
					return (int)nativeMetafilePage.GetBoxRect(CGPDFBox.Media).Height;
				else
					return 0;
			}
		}
		
		public PixelFormat PixelFormat {
			get {		
				var b = this as Bitmap;
				return b == null ? 0 : b.pixelFormat;
			}
		}
		
		public ImageFormat RawFormat {
			get {

				var b = this as Bitmap;
				return b == null ? new ImageFormat(new Guid()) : b.rawFormat;		
			}
		}
		
		[DefaultValue (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Width {
			get {
				if (NativeCGImage != null)
					return (int)NativeCGImage.Width;
				else if (nativeMetafilePage != null)
					return (int)nativeMetafilePage.GetBoxRect(CGPDFBox.Media).Width;
				else
					return 0;
			}
		}

		/// <summary>
		/// Gets the horizontal resolution, in pixels per inch, of this Image.
		/// </summary>
		/// <value>The horizontal resolution.</value>
		public float HorizontalResolution 
		{ 
			get { 
				var b = this as Bitmap;
				return b == null ? 0 : b.dpiWidth;			
			}

		}

		/// <summary>
		/// Gets the vertical resolution, in pixels per inch, of this Image.
		/// </summary>
		/// <value>The vertical resolution.</value>
		public float VerticalResolution 
		{ 
			get { 				
				var b = this as Bitmap;
				return b == null ? 0 : b.dpiHeight;	 
			}
		}

		public Size Size 
		{ 
			get {
				if (this.nativeMetafilePage != null) {
					var cgsize = this.nativeMetafilePage.GetBoxRect(CGPDFBox.Media);
					return new Size((int)cgsize.Width, (int)cgsize.Height);
				}
				var b = this as Bitmap;
				return b == null ? Size.Empty : b.imageSize;	 
			}
		}

		/// <summary>
		/// Gets the attribute pixel flags representing the image data.
		/// </summary>
		/// <value>The bitwise integer of the ImageFlags combinations.</value>
		[BrowsableAttribute(false)]
		public int Flags 
		{ 
			get { return (int)pixelFlags; } 
		}

		/// <summary>
		/// Gets the width and height of this image.
		/// </summary>
		/// <value>A SizeF structure that represents the width and height of this Image.</value>
		public SizeF PhysicalDimension
		{
			get { 
				var b = this as Bitmap;
				return b == null ? SizeF.Empty : b.physicalDimension;	 			
			}
		}
			
		[Browsable (false)]
		public Guid[] FrameDimensionsList {
			get {
				return new Guid[] { FrameDimension.Time.Guid };
			}
		}

		public int GetFrameCount (FrameDimension dimension)
		{
			return 1;
		}

		public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
		{
			if (frameIndex != 1)
				throw new NotImplementedException ();
			return frameIndex;		
		}

		public PropertyItem GetPropertyItem(int propid)
		{
			if (propid == 0x5100) // Frame delay
				return new PropertyItem();
			throw new NotImplementedException ();
		}

		public ColorPalette Palette {
			get { return palette; }
			set
			{
				if ((PixelFormat & PixelFormat.Indexed) != 0 && palette.Entries.Length == value.Entries.Length) {
					palette = value;

					// Update CGImage
					byte[] paletteEntries = new byte[palette.Entries.Length * 3];
					int index = 0;
					foreach (var entry in palette.Entries) {
						paletteEntries [index++] = entry.R;
						paletteEntries [index++] = entry.G;
						paletteEntries [index++] = entry.B;
					}
					
					NativeCGImage = NativeCGImage.WithColorSpace (CGColorSpace.CreateIndexed (CGColorSpace.CreateDeviceRGB (), palette.Entries.Length - 1, paletteEntries));
				}
			}
		}

		/// <summary>
		/// Creates an exact copy of this Image.
		/// </summary>
		public object Clone ()
		{
			return new Bitmap (this);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// TODO
		}
		
		public static Image FromStream (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			return new Bitmap(stream);
		}

		public static Image FromStream (Stream stream, bool useIcm)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			return new Bitmap(stream, useIcm);
		}

		public void Save(Stream stream, ImageFormat format)
		{
			if (this is Bitmap b)
				b.Save(stream, format);
			else
				using (b = new Bitmap(this))
					b.Save(stream, format);
		}

		public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters parameters)
		{
			if (this is Bitmap b)
				b.Save(stream, encoder, parameters);
			else
				using (b = new Bitmap(this))
					b.Save(stream, encoder, parameters);
		}

		public void Save(string path, ImageCodecInfo encoder, EncoderParameters parameters)
		{
			if (this is Bitmap b)
				b.Save(path, encoder, parameters);
			else
				using (b = new Bitmap(this))
					b.Save(path, encoder, parameters);
		}

		public void Save (Stream stream)
		{
			Save (stream, RawFormat);
		}
		
		public void Save (string filename, ImageFormat format)
		{
			if (this is Bitmap b)
				b.Save(filename, format);
			else
				using (b = new Bitmap(this))
					b.Save(filename, format);
		}

		public void Save (string filename)
		{
			if (this is Bitmap b)
				b.Save(filename);
			else
				using (b = new Bitmap(this))
					b.Save(filename);
		}
			
		public static Bitmap FromHbitmap (IntPtr handle)
		{
			throw new NotImplementedException ();
		}

		public static Bitmap FromHbitmap (IntPtr handle, IntPtr palette)
		{
			throw new NotImplementedException ();
		}

		public static Bitmap FromFile (string filename)
		{
			return new Bitmap(filename);
		}
		
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext context)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				// Icon is a decoder-only codec
				if (RawFormat.Equals (ImageFormat.Icon)) {
					Save (ms, ImageFormat.Png);
				} else {
					Save (ms, RawFormat);
				}
				si.AddValue ("Data", ms.ToArray ());
			}
		}

		[TypeConverterAttribute(typeof(StringConverter))]
		//[BindableAttribute(true)]
		public Object Tag { 
			get { 
				return tag;
			}
				
			set{
				tag = value.ToString();
			}
		}

		/// <summary>
		/// Gets the bounds of the image in the GraphicUnit variable passed.
		/// 
		/// This does not convert the bound into the GraphicsUnit passed but
		/// will tell you what the bounds are based on the unit that is provided
		/// on its return in the referenced pageUnit variable.
		/// </summary>
		/// <returns>The bounds.</returns>
		/// <param name="pageUnit">Page unit.</param>
		public RectangleF GetBounds(ref GraphicsUnit pageUnit)
		{

			var b = this as Bitmap;
			if (b == null)
				return RectangleF.Empty;

			// Right now we only have bitmaps of images so we will default
			// this to Pixel
			pageUnit = GraphicsUnit.Pixel;
			return new RectangleF(new PointF(0,0),b.physicalDimension);

		}

		/// <summary>
		/// Rotates and or flips the image base on the RotateFlipTyp value passed.
		/// </summary>
		/// <param name="rotateFlipType">Rotate flip type.</param>
		public void RotateFlip(RotateFlipType rotateFlipType)
		{
			var b = this as Bitmap;
			if (b == null)
				return;

			b.RotateFlip (rotateFlipType);
		}

		/// <summary>
		/// Gets the size of the pixel format in number of bits per pixel.
		/// </summary>
		/// <returns>The pixel format size.</returns>
		/// <param name="pixfmt">Pixfmt.</param>
		public static int GetPixelFormatSize(PixelFormat pixfmt)
		{
			return ((int)pixfmt >> 8) & 0xff;
		}

		/// <summary>
		/// Determines whether the pixelFormat passed contains alpha information.
		/// </summary>
		/// <returns><c>true</c> if the pixelFormat contain alpha information; otherwise, <c>false</c>.</returns>
		/// <param name="pixelFormat">Pixel format.</param>
		public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
		{
			return (pixfmt & PixelFormat.Alpha) != 0;

		}

		public static bool IsExtendedPixelFormat(PixelFormat pixfmt)
		{
			return (pixfmt & PixelFormat.Indexed) != 0;
		}

		public static bool IsCanonicalPixelFormat(PixelFormat pixfmt)
		{
			return (pixfmt & PixelFormat.Canonical) != 0;
		}

		public static bool IsExtendedPixelFormat1(PixelFormat pixfmt)
		{
			return (pixfmt & PixelFormat.Extended) != 0;
		}

		/// <summary>
		/// Gets the number of components for the pixel format.
		/// </summary>
		/// <returns>The format components.</returns>
		/// <param name="pixfmt">Pixfmt.</param>
		internal static int GetBitsPerPixel(PixelFormat pixfmt)
		{
			return (((int)pixfmt >> 8) & 0xff);
		}

		public Image GetThumbnailImage (int thumbWidth, int thumbHeight, Image.GetThumbnailImageAbort callback, IntPtr callbackData)
		{
			if ((thumbWidth <= 0) || (thumbHeight <= 0))
				throw new OutOfMemoryException ("Invalid thumbnail size");

			Image ThumbNail = new Bitmap (thumbWidth, thumbHeight);

			using (Graphics g = Graphics.FromImage (ThumbNail)) {
				g.DrawImage(this, new RectangleF(0, 0, thumbWidth, thumbHeight), new Rectangle(0, 0, this.Width, this.Height), GraphicsUnit.Pixel);
			}

			return ThumbNail;
		}
	}
}

//
// System.Drawing.Bitmap.cs
//
// Copyright (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2011-2013 Xamarin Inc.
//
// Authors: 
//	Alexandre Pigolkine (pigolkine@gmx.de)
//	Christian Meyer (Christian.Meyer@cs.tum.edu)
//	Miguel de Icaza (miguel@ximian.com)
//	Jordi Mas i Hernandez (jmas@softcatala.org)
//	Ravindra (rkumar@novell.com)
//	Sebastien Pouliot  <sebastien@xamarin.com>
//	Kenneth J. Pouncey  <kjpou@pt.lu>
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
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections.Generic;

#if MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ImageIO;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ImageIO;
using MonoTouch.MobileCoreServices;
#endif

namespace System.Drawing {
	
	[Serializable]
	public sealed class Bitmap : Image {
		// if null, we created the bitmap in memory, otherwise, the backing file.
		internal IntPtr bitmapBlock;

		internal CGBitmapContext cachedContext;

		// we will default this to one for now until we get some tests for other image types
		internal int frameCount = 1;

		internal PixelFormat pixelFormat;
		internal float dpiWidth = 0;
		internal float dpiHeight = 0;
		internal Size imageSize = Size.Empty;
		internal SizeF physicalDimension = SizeF.Empty;
		internal ImageFormat rawFormat;

		private CGDataProvider dataProvider;

		public Bitmap (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException ("Value can not be null");

			try 
			{
				// Use Image IO
				dataProvider = new CGDataProvider(filename);
				if (dataProvider == null)
					throw new FileNotFoundException ("File {0} not found.", filename);

				InitializeImageFrame (0);
			}
			catch (Exception) 
			{
				throw new FileNotFoundException ("File {0} not found.", filename);
			}


		}

		public Bitmap (Stream stream, bool useIcm)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value can not be null");

			// false: stream is owned by user code
			//nativeObject = InitFromStream (stream);
			// TODO
			// Use Image IO
			byte[] buffer;
			using(var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				buffer = memoryStream.ToArray();
			}

			dataProvider = new CGDataProvider(buffer, 0, buffer.Length);

			InitializeImageFrame (0);
		}

		public Bitmap (int width, int height) : 
			this (width, height, PixelFormat.Format32bppArgb)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="System.Drawing.Bitmap"/> class from the specified existing image..
		/// </summary>
		/// <param name="image">Image.</param>
		public Bitmap (Image image) :
			this (image, image.Width, image.Height)
		{

		}

		public Bitmap (Image original, int width, int height) : 
			this (width, height, PixelFormat.Format32bppArgb)
		{
			using (Graphics graphics = Graphics.FromImage (this)) {
				graphics.DrawImage (original, 0, 0, width, height);
			}
		}

		public Bitmap (Image original, Size newSize) : 
			this (newSize.Width, newSize.Height, PixelFormat.Format32bppArgb)
		{
			using (Graphics graphics = Graphics.FromImage (this)) {
				graphics.DrawImage (original, 0, 0, newSize.Width, newSize.Height);
			}
		}

		public Bitmap (int width, int height, PixelFormat format)
		{
			imageTransform = new CGAffineTransform(1, 0, 0, -1, 0, height);

			int bitsPerComponent, bytesPerRow;
			CGColorSpace colorSpace;
			CGBitmapFlags bitmapInfo;
			bool premultiplied = false;
			int bitsPerPixel = 0;

			pixelFormat = format;

			// Don't forget to set the Image width and height for size.
			imageSize.Width = width;
			imageSize.Height = height;

			switch (format){
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.DontCare:
				premultiplied = true;
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				bitmapInfo = CGBitmapFlags.PremultipliedFirst;
				break;
			case PixelFormat.Format32bppArgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				bitmapInfo = CGBitmapFlags.PremultipliedFirst;
				break;
			case PixelFormat.Format32bppRgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				bitmapInfo = CGBitmapFlags.NoneSkipLast;
				break;
			case PixelFormat.Format24bppRgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				bitmapInfo = CGBitmapFlags.NoneSkipLast;
				break;
			default:
				throw new Exception ("Format not supported: " + format);
			}
			bytesPerRow = width * bitsPerPixel/bitsPerComponent;
			int size = bytesPerRow * height;

			bitmapBlock = Marshal.AllocHGlobal (size);
			var bitmap = new CGBitmapContext (bitmapBlock, 
			                              width, height, 
			                              bitsPerComponent, 
			                              bytesPerRow,
			                              colorSpace,
			                              bitmapInfo);
			// This works for now but we need to look into initializing the memory area itself
			// TODO: Look at what we should do if the image does not have alpha channel
			bitmap.ClearRect (new RectangleF (0,0,width,height));

			var provider = new CGDataProvider (bitmapBlock, size, true);
			NativeCGImage = new CGImage (width, height, bitsPerComponent, bitsPerPixel, bytesPerRow, colorSpace, bitmapInfo, provider, null, false, CGColorRenderingIntent.Default);

			dpiWidth = dpiHeight = ConversionHelpers.MS_DPI;
			physicalDimension.Width = width;
			physicalDimension.Height = height;


			// The physical size may be off on certain implementations.  For instance the dpiWidth and dpiHeight 
			// are read using integers in core graphics but in windows it is a float.
			// For example:
			// coregraphics dpiWidth = 24 as integer
			// windows dpiWidth = 24.999935 as float
			// this gives a few pixels difference when calculating the physical size.
			// 256 * 96 / 24 = 1024
			// 256 * 96 / 24.999935 = 983.04
			//
			// https://bugzilla.xamarin.com/show_bug.cgi?id=14365
			// PR: https://github.com/mono/maccore/pull/57
			//

			physicalSize = new SizeF (physicalDimension.Width, physicalDimension.Height);
			physicalSize.Width *= ConversionHelpers.MS_DPI / dpiWidth;
			physicalSize.Height *= ConversionHelpers.MS_DPI / dpiHeight;

			rawFormat = ImageFormat.MemoryBmp;
			pixelFormat = format;

			if (premultiplied) { // make compiler happy
			}
		}

		private void InitializeImageFrame(int frame)
		{
			imageTransform = CGAffineTransform.MakeIdentity();

			SetImageInformation (frame);
			var cg = CGImageSource.FromDataProvider(dataProvider).CreateImage(frame, null);
			imageTransform = new CGAffineTransform(1, 0, 0, -1, 0, cg.Height);
			//InitWithCGImage (cg);
			NativeCGImage = cg;

			GuessPixelFormat ();
		}

		private void GuessPixelFormat()
		{
			bool hasAlpha;
			CGColorSpace colorSpace;
			int bitsPerComponent;
			bool premultiplied = false;
			int bitsPerPixel = 0;
			CGImageAlphaInfo alphaInfo;

			var image = NativeCGImage;

			if (image == null) {
				throw new ArgumentException (" image is invalid! " );
			}

			alphaInfo = image.AlphaInfo;
			hasAlpha = ((alphaInfo == CGImageAlphaInfo.PremultipliedLast) || (alphaInfo == CGImageAlphaInfo.PremultipliedFirst) || (alphaInfo == CGImageAlphaInfo.Last) || (alphaInfo == CGImageAlphaInfo.First) ? true : false);

			imageSize.Width = image.Width;
			imageSize.Height = image.Height;

			// Not sure yet if we need to keep the original image information
			// before we change it internally.  TODO look at what windows does
			// and follow that.
			bitsPerComponent = image.BitsPerComponent;
			bitsPerPixel = image.BitsPerPixel;

			colorSpace = image.ColorSpace;

			if (colorSpace != null)
			{
				if (colorSpace.Model == CGColorSpaceModel.RGB) {
					if (bitsPerPixel == 32) {
						if (hasAlpha) {
							if (alphaInfo == CGImageAlphaInfo.PremultipliedFirst) 
							{
								premultiplied = true;
								pixelFormat = PixelFormat.Format32bppPArgb;
							}

							if (alphaInfo == CGImageAlphaInfo.First)
								pixelFormat = PixelFormat.Format32bppArgb;

							if (alphaInfo == CGImageAlphaInfo.Last)
								pixelFormat = PixelFormat.Format32bppRgb;

							if (alphaInfo == CGImageAlphaInfo.PremultipliedLast) 
							{
								premultiplied = true;
								pixelFormat = PixelFormat.Format32bppRgb;
							}


						} else {
							pixelFormat = PixelFormat.Format24bppRgb;
						}
					} else {
						// Right now microsoft looks like it is using Format32bppRGB for other
						// need more test cases to verify
						pixelFormat = PixelFormat.Format32bppArgb;
					}
				} else {
					// Right now microsoft looks like it is using Format32bppRGB for other
					// MonoChrome is set to 32bpppArgb
					// need more test cases to verify
					pixelFormat = PixelFormat.Format32bppArgb;
				}

			}
			else
			{
				// need more test cases to verify
				pixelFormat = PixelFormat.Format32bppArgb;

			}

			if (premultiplied) { // make compiler happy
			}
		}


		private void SetImageInformation(int frame)
		{
			var imageSource = CGImageSource.FromDataProvider (dataProvider);

			frameCount = imageSource.ImageCount;

			var properties = imageSource.GetProperties (frame, null);

			// This needs to be incorporated in frame information later
			// as well as during the clone methods.
#if MONOTOUCH
			// https://bugzilla.xamarin.com/show_bug.cgi?id=14365
			// PR: https://github.com/mono/maccore/pull/57
			// we need to keep using the obsolete version until the problem is fixed in MT
			dpiWidth =  properties.DPIWidth != null ? (float)properties.DPIWidth : ConversionHelpers.MS_DPI;
			dpiHeight = properties.DPIWidth != null ? (float)properties.DPIHeight : ConversionHelpers.MS_DPI;

#else

			dpiWidth =  properties.DPIWidthF != null ? (float)properties.DPIWidthF : ConversionHelpers.MS_DPI;
			dpiHeight = properties.DPIWidthF != null ? (float)properties.DPIHeightF : ConversionHelpers.MS_DPI;
#endif
			physicalDimension.Width = (float)properties.PixelWidth;
			physicalDimension.Height = (float)properties.PixelHeight;


			// The physical size may be off on certain implementations.  For instance the dpiWidth and dpiHeight 
			// are read using integers in core graphics but in windows it is a float.
			// For example:
			// coregraphics dpiWidth = 24 as integer
			// windows dpiWidth = 24.999935 as float
			// this gives a few pixels difference when calculating the physical size.
			// 256 * 96 / 24 = 1024
			// 256 * 96 / 24.999935 = 983.04
			//
			// https://bugzilla.xamarin.com/show_bug.cgi?id=14365
			// PR: https://github.com/mono/maccore/pull/57
			//

			physicalSize = new SizeF (physicalDimension.Width, physicalDimension.Height);
			physicalSize.Width *= ConversionHelpers.MS_DPI / dpiWidth;
			physicalSize.Height *= ConversionHelpers.MS_DPI / dpiHeight;

			// Set the raw image format
			// We will use the UTI from the image source
			switch (imageSource.TypeIdentifier) 
			{
			case "public.png":
				rawFormat = ImageFormat.Png;
				break;
			case "com.microsoft.bmp":
				rawFormat = ImageFormat.Bmp;
				break;
			case "com.compuserve.gif":
				rawFormat = ImageFormat.Gif;
				break;
			case "public.jpeg":
				rawFormat = ImageFormat.Jpeg;
				break;
			case "public.tiff":
				rawFormat = ImageFormat.Tiff;
				break;
			case "com.microsoft.ico":
				rawFormat = ImageFormat.Icon;
				break;
			case "com.adobe.pdf":
				rawFormat = ImageFormat.Wmf;
				break;
			default:
				rawFormat = ImageFormat.Png;
				break;
			}

		}

		private void InitWithCGImage (CGImage image)
		{
			int	width, height;
			CGBitmapContext bitmap = null;
			bool hasAlpha;
			CGImageAlphaInfo alphaInfo;
			CGColorSpace colorSpace;
			int bitsPerComponent, bytesPerRow;
			CGBitmapFlags bitmapInfo;
			bool premultiplied = false;
			int bitsPerPixel = 0;

			if (image == null) {
				throw new ArgumentException (" image is invalid! " );
			}

			alphaInfo = image.AlphaInfo;
			hasAlpha = ((alphaInfo == CGImageAlphaInfo.PremultipliedLast) || (alphaInfo == CGImageAlphaInfo.PremultipliedFirst) || (alphaInfo == CGImageAlphaInfo.Last) || (alphaInfo == CGImageAlphaInfo.First) ? true : false);
			
			imageSize.Width = image.Width;
			imageSize.Height = image.Height;
			
			width = image.Width;
			height = image.Height;

			// Not sure yet if we need to keep the original image information
			// before we change it internally.  TODO look at what windows does
			// and follow that.
			bitmapInfo = image.BitmapInfo;
			bitsPerComponent = image.BitsPerComponent;
			bitsPerPixel = image.BitsPerPixel;
			bytesPerRow = width * bitsPerPixel/bitsPerComponent;
			int size = bytesPerRow * height;
			
			colorSpace = image.ColorSpace;

			// Right now internally we represent the images all the same
			// I left the call here just in case we find that this is not
			// possible.  Read the comments for non alpha images.
			if(colorSpace != null) {
				if( hasAlpha ) {
					premultiplied = true;
					colorSpace = CGColorSpace.CreateDeviceRGB ();
					bitsPerComponent = 8;
					bitsPerPixel = 32;
					bitmapInfo = CGBitmapFlags.PremultipliedLast;
				}
				else
				{
					// even for images without alpha we will internally 
					// represent them as RGB with alpha.  There were problems
					// if we do not do it this way and creating a bitmap context.
					// The images were not drawing correctly and tearing.  Also
					// creating a Graphics to draw on was a nightmare.  This
					// should probably be looked into or maybe it is ok and we
					// can continue representing internally with this representation
					premultiplied = true;
					colorSpace = CGColorSpace.CreateDeviceRGB ();
					bitsPerComponent = 8;
					bitsPerPixel = 32;
					bitmapInfo = CGBitmapFlags.NoneSkipLast;
				}
			} else {
				premultiplied = true;
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				bitmapInfo = CGBitmapFlags.NoneSkipLast;
			}

			bytesPerRow = width * bitsPerPixel/bitsPerComponent;
			size = bytesPerRow * height;

			bitmapBlock = Marshal.AllocHGlobal (size);
			bitmap = new CGBitmapContext (bitmapBlock, 
			                              width, height, 
			                              bitsPerComponent, 
			                              bytesPerRow,
			                              colorSpace,
			                              bitmapInfo);

			bitmap.ClearRect (new RectangleF (0,0,width,height));

			// We need to flip the Y axis to go from right handed to lefted handed coordinate system
			var transform = new CGAffineTransform(1, 0, 0, -1, 0, image.Height);
			bitmap.ConcatCTM(transform);

			bitmap.DrawImage(new RectangleF (0, 0, image.Width, image.Height), image);

			var provider = new CGDataProvider (bitmapBlock, size, true);
			NativeCGImage = new CGImage (width, height, bitsPerComponent, 
			                             bitsPerPixel, bytesPerRow, 
			                             colorSpace,
			                             bitmapInfo,
			                             provider, null, true, image.RenderingIntent);

			colorSpace.Dispose();
			bitmap.Dispose();

			if (premultiplied) {} // make compiler happy
		}

		internal CGBitmapContext GetRenderableContext()
		{

			if (cachedContext != null && cachedContext.Handle != IntPtr.Zero)
				return cachedContext;

			var format = GetBestSupportedFormat (pixelFormat);
			var bitmapContext = CreateCompatibleBitmapContext (NativeCGImage.Width, NativeCGImage.Height, format);

			bitmapContext.DrawImage (new RectangleF (0, 0, NativeCGImage.Width, NativeCGImage.Height), NativeCGImage);

			int size = bitmapContext.BytesPerRow * bitmapContext.Height;
			var provider = new CGDataProvider (bitmapContext.Data, size, true);

			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			NativeCGImage = new CGImage (bitmapContext.Width, bitmapContext.Height, bitmapContext.BitsPerComponent, 
			                             bitmapContext.BitsPerPixel, bitmapContext.BytesPerRow, 
			                             colorSpace,
			                             bitmapContext.AlphaInfo,
			                             provider, null, true, CGColorRenderingIntent.Default);
			colorSpace.Dispose ();
			cachedContext = bitmapContext;

			return cachedContext;
		}

		internal new void RotateFlip (RotateFlipType rotateFlipType)
		{

			CGAffineTransform rotateFlip = CGAffineTransform.MakeIdentity();

			int width, height;
			width = NativeCGImage.Width;
			height = NativeCGImage.Height;

			switch (rotateFlipType) 
			{
				//			case RotateFlipType.RotateNoneFlipNone:
				//			//case RotateFlipType.Rotate180FlipXY:
				//				rotateFlip = GeomUtilities.CreateRotateFlipTransform (b.Width, b.Height, 0, false, false);
				//				break;
				case RotateFlipType.Rotate90FlipNone:
				//case RotateFlipType.Rotate270FlipXY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 90, false, false);
				break;
				case RotateFlipType.Rotate180FlipNone:
				//case RotateFlipType.RotateNoneFlipXY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 0, true, true);
				break;
				case RotateFlipType.Rotate270FlipNone:
				//case RotateFlipType.Rotate90FlipXY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 270, false, false);
				break;
				case RotateFlipType.RotateNoneFlipX:
				//case RotateFlipType.Rotate180FlipY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 0, true, false);
				break;
				case RotateFlipType.Rotate90FlipX:
				//case RotateFlipType.Rotate270FlipY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 90, true, false);
				break;
				case RotateFlipType.Rotate180FlipX:
				//case RotateFlipType.RotateNoneFlipY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 0, false, true);
				break;
				case RotateFlipType.Rotate270FlipX:
				//case RotateFlipType.Rotate90FlipY:
				rotateFlip = GeomUtilities.CreateRotateFlipTransform (ref width, ref height, 270, true, false);
				break;
			}

			var format = GetBestSupportedFormat (pixelFormat);
			var bitmapContext = CreateCompatibleBitmapContext (width, height, format);

			bitmapContext.ConcatCTM (rotateFlip);

			bitmapContext.DrawImage (new RectangleF (0, 0, NativeCGImage.Width, NativeCGImage.Height), NativeCGImage);

			int size = bitmapContext.BytesPerRow * bitmapContext.Height;
			var provider = new CGDataProvider (bitmapContext.Data, size, true);

			// If the width or height is not the seme we need to switch the dpiHeight and dpiWidth
			// We should be able to get around this with set resolution later.
			if (NativeCGImage.Width != width || NativeCGImage.Height != height)
			{
				var temp = dpiWidth;
				dpiHeight = dpiWidth;
				dpiWidth = temp;
			}

			NativeCGImage = new CGImage (bitmapContext.Width, bitmapContext.Height, bitmapContext.BitsPerComponent, 
			                             bitmapContext.BitsPerPixel, bitmapContext.BytesPerRow, 
			                             bitmapContext.ColorSpace,
			                             bitmapContext.AlphaInfo,
			                             provider, null, true, CGColorRenderingIntent.Default);


			physicalDimension.Width = (float)width;
			physicalDimension.Height = (float)height;

			physicalSize = new SizeF (physicalDimension.Width, physicalDimension.Height);
			physicalSize.Width *= ConversionHelpers.MS_DPI / dpiWidth;
			physicalSize.Height *= ConversionHelpers.MS_DPI / dpiHeight;

			// In windows the RawFormat is changed to MemoryBmp to show that the image has changed.
			rawFormat = ImageFormat.MemoryBmp;

			// Set our transform for this image for the new height
			imageTransform = new CGAffineTransform(1, 0, 0, -1, 0, height);

		}

		private PixelFormat GetBestSupportedFormat (PixelFormat pixelFormat)
		{
			switch (pixelFormat) 
			{
			case PixelFormat.Format32bppArgb:
				return PixelFormat.Format32bppArgb;
			case PixelFormat.Format32bppPArgb:
				return PixelFormat.Format32bppPArgb;
			case PixelFormat.Format32bppRgb:
				return PixelFormat.Format32bppRgb;
			case PixelFormat.Format24bppRgb:
				return PixelFormat.Format24bppRgb;
			default:
				return PixelFormat.Format32bppArgb;
			}

		}

		private CGBitmapContext CreateCompatibleBitmapContext(int width, int height, PixelFormat pixelFormat)
		{
			int bitsPerComponent, bytesPerRow;
			CGColorSpace colorSpace;
			CGImageAlphaInfo alphaInfo;
			bool premultiplied = false;
			int bitsPerPixel = 0;

			// CoreGraphics only supports a few options so we have to make do with what we have
			// https://developer.apple.com/library/mac/qa/qa1037/_index.html
			switch (pixelFormat)
			{
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.DontCare:
				premultiplied = true;
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			case PixelFormat.Format32bppArgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			case PixelFormat.Format32bppRgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			case PixelFormat.Format24bppRgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			default:
				throw new Exception ("Format not supported: " + pixelFormat);
			}

			bytesPerRow = width * bitsPerPixel/bitsPerComponent;
			int size = bytesPerRow * height;

			var bitmapBlock = Marshal.AllocHGlobal (size);
			var bitmap = new CGBitmapContext (bitmapBlock, 
			                                  width, height, 
			                                  bitsPerComponent, 
			                                  bytesPerRow,
			                                  colorSpace,
											  alphaInfo);

			bitmap.ClearRect (new RectangleF (0,0,width,height));

			//colorSpace.Dispose ();
			if (premultiplied) {} // make compiler happy

			return bitmap;
		}

		private CGBitmapContext CreateCompatibleBitmapContext(int width, int height, PixelFormat pixelFormat, IntPtr pixelData)
		{
			int bitsPerComponent, bytesPerRow;
			CGColorSpace colorSpace;
			CGImageAlphaInfo alphaInfo;
			bool premultiplied = false;
			int bitsPerPixel = 0;

			// CoreGraphics only supports a few options so we have to make do with what we have
			// https://developer.apple.com/library/mac/qa/qa1037/_index.html
			switch (pixelFormat)
			{
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.DontCare:
				premultiplied = true;
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			case PixelFormat.Format32bppArgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			case PixelFormat.Format32bppRgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			case PixelFormat.Format24bppRgb:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				bitsPerComponent = 8;
				bitsPerPixel = 32;
				alphaInfo = CGImageAlphaInfo.PremultipliedLast;
				break;
			default:
				throw new Exception ("Format not supported: " + pixelFormat);
			}

			bytesPerRow = width * bitsPerPixel/bitsPerComponent;
			//int size = bytesPerRow * height;

			var bitmap = new CGBitmapContext (pixelData, 
			                                  width, height, 
			                                  bitsPerComponent, 
			                                  bytesPerRow,
			                                  colorSpace,
			                                  alphaInfo);

			colorSpace.Dispose ();

			if (premultiplied) {} // make compiler happy

			return bitmap;
		}

		/*
		  * perform an in-place swap from Quadrant 1 to Quadrant III format
		  * (upside-down PostScript/GL to right side up QD/CG raster format)
		  * We do this in-place, which requires more copying, but will touch
		  * only half the pages.  (Display grabs are BIG!)
		  *
		  * Pixel reformatting may optionally be done here if needed.
		*/
		private void flipImageYAxis (IntPtr source, IntPtr dest, int stride, int height, int size)
		{
			
			long top, bottom;
			byte[] buffer;
			long topP;
			long bottomP;
			long rowBytes;
			
			top = 0;
			bottom = height - 1;
			rowBytes = stride;
			
			var mData = new byte[size];
			Marshal.Copy(source, mData, 0, size);
			
			buffer = new byte[rowBytes];
			
			while (top < bottom) {
				topP = top * rowBytes;
				bottomP = bottom * rowBytes;
				
				/*
				 * Save and swap scanlines.
				 *
				 * This code does a simple in-place exchange with a temp buffer.
				 * If you need to reformat the pixels, replace the first two Array.Copy
				 * calls with your own custom pixel reformatter.
				 */
				Array.Copy (mData, topP, buffer, 0, rowBytes);
				Array.Copy (mData, bottomP, mData, topP, rowBytes);
				Array.Copy (buffer, 0, mData, bottomP, rowBytes);
				
				++top;
				--bottom;
				
			}
			
			Marshal.Copy(mData, 0, dest, size);
			
		}


		/*
		  * perform an in-place swap from Quadrant 1 to Quadrant III format
		  * (upside-down PostScript/GL to right side up QD/CG raster format)
		  * We do this in-place, which requires more copying, but will touch
		  * only half the pages.  (Display grabs are BIG!)
		  *
		  * Pixel reformatting may optionally be done here if needed.
		  * 
		  * NOTE: Not used right now
		*/
		private void flipImageYAxis (int width, int height, int size)
		{
			
			long top, bottom;
			byte[] buffer;
			long topP;
			long bottomP;
			long rowBytes;
			
			top = 0;
			bottom = height - 1;
			rowBytes = width;

			var mData = new byte[size];
			Marshal.Copy(bitmapBlock, mData, 0, size);

			buffer = new byte[rowBytes];
			
			while (top < bottom) {
				topP = top * rowBytes;
				bottomP = bottom * rowBytes;
				
				/*
				 * Save and swap scanlines.
				 *
				 * This code does a simple in-place exchange with a temp buffer.
				 * If you need to reformat the pixels, replace the first two Array.Copy
				 * calls with your own custom pixel reformatter.
				 */
				Array.Copy (mData, topP, buffer, 0, rowBytes);
				Array.Copy (mData, bottomP, mData, topP, rowBytes);
				Array.Copy (buffer, 0, mData, bottomP, rowBytes);
				
				++top;
				--bottom;
				
			}

			Marshal.Copy(mData, 0, bitmapBlock, size);

		}

		/// <summary>
		/// Creates a copy of the section of this Bitmap defined by Rectangle structure and with a specified PixelFormat enumeration.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="pixelFormat">Pixel format.</param>
		public Bitmap Clone (Rectangle rect, PixelFormat pixelFormat)
		{
			if (rect.Width == 0 || rect.Height == 0)
				throw new ArgumentException ("Width or Height of rect is 0.");

			var width = rect.Width;
			var height = rect.Height;

			var tmpImg = new Bitmap (width, height, pixelFormat);

			using (Graphics g = Graphics.FromImage (tmpImg)) {
				g.DrawImage (this, new Rectangle(0,0, width, height), rect, GraphicsUnit.Pixel );
			}
			return tmpImg;
		}


		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (NativeCGImage != null){
					NativeCGImage.Dispose ();
					NativeCGImage = null;
				}
				//Marshal.FreeHGlobal (bitmapBlock);
				bitmapBlock = IntPtr.Zero;
				Console.WriteLine("Bitmap Dispose");
			}
			base.Dispose (disposing);
		}
		
		public Color GetPixel (int x, int y)
		{
			if (x < 0 || x > NativeCGImage.Width - 1)
				throw new InvalidEnumArgumentException ("Parameter must be positive and < Width.");
			if (y < 0 || y > NativeCGImage.Height - 1)
				throw new InvalidEnumArgumentException ("Parameter must be positive and < Height.");

			// Need more tests to see if we need this call.
			MakeSureWeHaveAnAlphaChannel ();

			// We are going to cheat here and instead of reading the bytes of the original image
			// parsing from there a pixel and converting to a format we will just create 
			// a 1 x 1 image of the pixel that we want.  I am supposing this should be really
			// fast.
			var pixelImage = NativeCGImage.WithImageInRect(new RectangleF(x,y,1,1));

			var pData = pixelImage.DataProvider;
			var nData = pData.CopyData ();

			// We may have to parse out the bytes with 4 or 3 bytes per pixel later.
			var pixelColor = Color.FromArgb(nData[3], nData[0], nData[1], nData[2]);
			pixelImage.Dispose ();

			return pixelColor;
		}

		public void SetPixel (int x, int y, Color color)
		{
			if (x < 0 || x > NativeCGImage.Width - 1)
				throw new InvalidEnumArgumentException ("Parameter must be positive and < Width.");
			if (y < 0 || y > NativeCGImage.Height - 1)
				throw new InvalidEnumArgumentException ("Parameter must be positive and < Height.");


			MakeSureWeHaveAnAlphaChannel ();

			// We are going to cheat here by drawing directly to the cached context that is 
			// associated to the image.  This way we do not have to play with pixels and offsets
			// to change the data.  If this proves to be non performant then we will change it later.
			cachedContext.SaveState ();
			cachedContext.ConcatCTM (cachedContext.GetCTM ().Invert ());
			cachedContext.ConcatCTM (imageTransform);
			cachedContext.SetFillColor(color.ToCGColor());
			cachedContext.FillRect (new RectangleF(x,y, 1,1));
			cachedContext.FillPath ();
			cachedContext.RestoreState();

		}


		public void SetResolution (float xDpi, float yDpi)
		{
			throw new NotImplementedException ();
		}

		public void MakeTransparent() 
		{
			// Todo: Instead of passing white here we need to read
			// the Lower-Left pixel of image.  Found this by
			// placing a color in each corner until MakeTransparent 
			// did not work.
			//      bitmap.SetPixel(0, bitmap.Height - 1, Color.Magenta);ß

			MakeTransparent (GetPixel(0, Height-1));

		}

		public void MakeTransparent(Color transparentColor)
		{

			MakeSureWeHaveAnAlphaChannel ();


			var colorValues = transparentColor.ElementsRGBA ();

			// Lock the bitmap's bits.  
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			var bmpData = LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
				             pixelFormat);

			var alpha = Color.Transparent.A;
			var red = Color.Transparent.R;
			var green = Color.Transparent.G;
			var blue = Color.Transparent.B;

			byte alphar = Color.Transparent.A;
			byte redr = Color.Transparent.R;
			byte greenr = Color.Transparent.G;
			byte bluer = Color.Transparent.B;
		
			bool match = false;

			var pixelSize = GetPixelFormatComponents (pixelFormat);
			unsafe 
			{
				for (int y=0; y<Height; y++) {
					byte* row = (byte*)bmpData.Scan0 + (y * bmpData.Stride);
					for (int x=0; x<bmpData.Stride; x=x+pixelSize) {

						redr = row [x + 2];;
						greenr = row [x + 1];
						bluer = row [x];
						alphar = row [x + 3];

						match = false;

						if (row [x + 2] == colorValues [0] && row [x + 1] == colorValues [1]
							&& row [x] == colorValues [2] && row [x + 3] == colorValues [3]) 
							match = true;


						if (match) 
						{
							// we process bgra
							row [x] = blue;
							row [x + 1] = green;
							row [x + 2] = red;

							if (pixelSize == 4)
								row [x + 3] = alpha;
						}
					}

				}
			}

			// Unlock the bits.
			UnlockBits(bmpData);
		}

		private void MakeSureWeHaveAnAlphaChannel ()
		{

			// Initialize our prmultiplied tables.
			if (!ConversionHelpers.sTablesInitialized)
				ConversionHelpers.CalculateTables ();

			var alphaInfo = NativeCGImage.AlphaInfo;
//			var hasAlpha = ((alphaInfo == CGImageAlphaInfo.PremultipliedLast) 
//			                || (alphaInfo == CGImageAlphaInfo.PremultipliedFirst) 
//			                || (alphaInfo == CGImageAlphaInfo.Last) 
//			                || (alphaInfo == CGImageAlphaInfo.First) ? true : false);

			if (cachedContext != null && cachedContext.Handle != IntPtr.Zero) 
			{
				return;
			}

			// set our pixel format
			pixelFormat = PixelFormat.Format32bppArgb;
			// and mark the rawformat as from memory
			rawFormat = ImageFormat.MemoryBmp;

			//format = GetBestSupportedFormat (pixelFormat);
			cachedContext = CreateCompatibleBitmapContext (NativeCGImage.Width, NativeCGImage.Height, pixelFormat);

			// Fill our pixel data with the actual image information
			cachedContext.DrawImage (new RectangleF (0, 0, NativeCGImage.Width, NativeCGImage.Height), NativeCGImage);

			// Dispose of the prevous image that is allocated.
			NativeCGImage.Dispose ();

			// Get a reference to the pixel data
			bitmapBlock = cachedContext.Data;
			int size = cachedContext.BytesPerRow * cachedContext.Height;
			var provider = new CGDataProvider (cachedContext.Data, size, true);

			// Get the image from the bitmap context.
			//NativeCGImage = bitmapContext.ToImage ();
			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			NativeCGImage = new CGImage (cachedContext.Width, cachedContext.Height, cachedContext.BitsPerComponent, 
			                             cachedContext.BitsPerPixel, cachedContext.BytesPerRow, 
			                             colorSpace,
			                             cachedContext.AlphaInfo,
			                             provider, null, true, CGColorRenderingIntent.Default);
			colorSpace.Dispose ();

		}


		public void Save (string path, ImageFormat format)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			if (NativeCGImage == null)
				throw new ObjectDisposedException ("cgimage");

			// With MonoTouch we can use UTType from CoreMobileServices but since
			// MonoMac does not have that yet (or at least can not find it) I will 
			// use the string version of those for now.  I did not want to add another
			// #if #else in here.


			// for now we will just default this to png
			var typeIdentifier = "public.png";

			// Get the correct type identifier
			if (format == ImageFormat.Bmp)
				typeIdentifier = "com.microsoft.bmp";
//			else if (format == ImageFormat.Emf)
//				typeIdentifier = "image/emf";
//			else if (format == ImageFormat.Exif)
//				typeIdentifier = "image/exif";
			else if (format == ImageFormat.Gif)
				typeIdentifier = "com.compuserve.gif";
			else if (format == ImageFormat.Icon)
				typeIdentifier = "com.microsoft.ico";
			else if (format == ImageFormat.Jpeg)
				typeIdentifier = "public.jpeg";
			else if (format == ImageFormat.Png)
				typeIdentifier = "public.png";
			else if (format == ImageFormat.Tiff)
				typeIdentifier = "public.tiff";
			else if (format == ImageFormat.Wmf)
				typeIdentifier = "com.adobe.pdf";

			// Not sure what this is yet
			else if (format == ImageFormat.MemoryBmp)
				throw new NotImplementedException("ImageFormat.MemoryBmp not supported");

			// Obtain a URL file path to be passed
			NSUrl url = NSUrl.FromFilename(path);

			// * NOTE * we only support one image for right now.

			// Create an image destination that saves into the path that is passed in
			CGImageDestination dest = CGImageDestination.FromUrl (url, typeIdentifier, frameCount, null); 

			// Add an image to the destination
			dest.AddImage(NativeCGImage, null);

			// Finish the export
			bool success = dest.Close ();
			if (success == false) {
				//Console.WriteLine("did not work");
			} else {
				//Console.WriteLine("did work: " + path);
			}
			dest.Dispose();
			dest = null;

		}

		public new void Save (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			var format = ImageFormat.Png;
			
			var p = path.LastIndexOf (".");
			if (p != -1 && p < path.Length){
				switch (path.Substring (p + 1)){
				case "png": break;
				case "jpg": format = ImageFormat.Jpeg; break;
				case "tiff": format = ImageFormat.Tiff; break;
				case "bmp": format = ImageFormat.Bmp; break;
				}
			}
			Save (path, format);
		}
		public BitmapData LockBits (RectangleF rect, ImageLockMode flags, PixelFormat pixelFormat)
		{

			BitmapData bitmapData = new BitmapData ();

			if (!ConversionHelpers.sTablesInitialized)
				ConversionHelpers.CalculateTables ();

			// Calculate our strides
			int srcStride = (int)rect.Width * (NativeCGImage.BitsPerPixel / NativeCGImage.BitsPerComponent);

			int numOfComponents = GetPixelFormatComponents(pixelFormat);
			int stride = (int)rect.Width * numOfComponents;

			// Calculate our lengths
			int srcScanLength  = (int)(Math.Abs(srcStride) * rect.Height);
			int scanLength = (int)(Math.Abs(stride) * rect.Height);

			// Declare an array to hold the scan bytes of the bitmap. 
			byte[] scan0 = new byte[scanLength];
			pinnedScanArray = GCHandle.Alloc(scan0, GCHandleType.Pinned);
			bitmapData.Scan0 = pinnedScanArray.AddrOfPinnedObject();

			byte[] srcScan0 = new byte[srcScanLength];

			IntPtr ptr = bitmapBlock;
			if (ptr == IntPtr.Zero) 
			{
				var pData = NativeCGImage.DataProvider;
				var nData = pData.CopyData ();
				ptr = nData.Bytes;
			}
			// Copy the RGB values into the scan array.
			System.Runtime.InteropServices.Marshal.Copy(ptr, srcScan0, 0, srcScanLength);

			if (numOfComponents == 4)
				Convert_P_RGBA_8888_To_BGRA_8888 (ref scan0, srcScan0);
			else
				Convert_P_RGBA_8888_To_BGR_888 (ref scan0, srcScan0);

			// We need to support sub rectangles.
			if (rect != new RectangleF (new PointF (0, 0), physicalDimension)) 
			{
				throw new NotImplementedException("Sub rectangles of bitmaps not supported yet.");
			} 
			else 
			{
				bitmapData.Height = (int)rect.Height;
				bitmapData.Width = (int)rect.Width;
				bitmapData.PixelFormat = pixelFormat;

				bitmapData.Stride = stride;
			}

			return bitmapData;
		}

		[DllImport( "msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false )]
		public static extern unsafe void memcpy( byte* dest, byte* src, int count );

		static unsafe void RectangularCopy( byte* dstScanLine, byte* srcScanLine, int dstStride, int srcStride, int width, int height, int sizeOfPixel )
		{
			byte* srcRow = srcScanLine;
			byte* dstRow = dstScanLine;
			for( int y = 0; y < height; ++y ) {
				memcpy( dstRow, srcRow, sizeOfPixel * width );
				srcRow += srcStride;
				dstRow += dstStride;
			}
		}

		GCHandle pinnedScanArray;

		ImageLockMode bitsLockMode = 0;

		public void UnlockBits (BitmapData data)
		{

			if (bitsLockMode == ImageLockMode.ReadOnly)
			{
				pinnedScanArray.Free ();
				bitsLockMode = 0;
				return;
			}

			//int destStride = data.Width * (NativeCGImage.BitsPerPixel / NativeCGImage.BitsPerComponent);
			int destStride = data.Stride;

			// Declare our size 
			var scanLength  = destStride * Height;

			// This is fine here for now until we support other formats but right now it is RGBA
			var pixelSize = GetPixelFormatComponents (data.PixelFormat);

			if (pixelSize == 4)
				Convert_BGRA_8888_To_P_RGBA_8888 (data.Scan0, bitmapBlock, scanLength);
			else
				Convert_BGR_888_To_P_RGBA_8888 (data.Scan0, bitmapBlock, scanLength);

			// Create a bitmap context from the pixel data
			var bitmapContext = CreateCompatibleBitmapContext (data.Width, data.Height, data.PixelFormat, bitmapBlock);

			// Dispose of the prevous image that is allocated.
			NativeCGImage.Dispose ();

			// Get the image from the bitmap context.
			NativeCGImage = bitmapContext.ToImage ();

			// Dispose of the bitmap context as it is no longer needed
			bitmapContext.Dispose ();

			// we need to free our pointer
			pinnedScanArray.Free();
			bitsLockMode = 0;

		}

		// Our internal format is pre-multiplied alpha
		void Convert_P_RGBA_8888_To_BGRA_8888(ref byte[] scanLine, byte[] source)
		{
			byte temp = 0;
			byte alpha = 0;
			for (int x = 0; x < source.Length; x+=4) 
			{
				alpha = source [x + 3];  // Save off alpha
				temp = source [x];  // save off red

				if (alpha < 255) {
					scanLine [x] = ConversionHelpers.UnpremultiplyValue (alpha, source [x + 2]);  // move blue to red
					scanLine [x + 1] = ConversionHelpers.UnpremultiplyValue (alpha, source [x + 1]);
					scanLine [x + 2] = ConversionHelpers.UnpremultiplyValue (alpha, temp);	// move the red to green
				} else {
					scanLine [x] = source [x + 2];  // move blue to red
					scanLine [x + 1] = source [x + 1];
					scanLine [x + 2] = temp;  // move the red to green
				}
//				var red = source [x];
//				var green = source [x + 1];
//				var blue = source [x + 1];


				scanLine [x + 3] = alpha;
				// Now we do the cha cha cha
			}
		}

		// Our internal format is pre-multiplied alpha
		void Convert_P_RGBA_8888_To_BGR_888(ref byte[] scanLine, byte[] source)
		{
			byte temp = 0;
			byte alpha = 0;
			for (int x = 0, y=0; x < source.Length; x+=4, y+=3) 
			{
				alpha = source [x + 3];  // Save off alpha
				temp = source [x];  // save off red

				scanLine [y] = ConversionHelpers.PremultiplyValue(alpha,source [x + 2]);  // move blue to red
				scanLine [y + 1] = ConversionHelpers.PremultiplyValue(alpha,source [x + 1]);
				scanLine [y + 2] = ConversionHelpers.PremultiplyValue(alpha,temp);	// move the red to green
				// Now we do the cha cha cha
			}
		}

		// Our internal format is pre-multiplied alpha
		void Convert_BGRA_8888_To_P_RGBA_8888(IntPtr source, IntPtr destination, int scanLength)
		{

			unsafe 
			{
				byte* src = (byte*)source;
				byte* dest = (byte*)destination;

				byte temp = 0;
				byte alpha = 0;

				for (int sd = 0; sd < scanLength; sd+=4) 
				{
					alpha = src [sd + 3];
					temp = src [sd];  // save off blue
					dest [sd] = ConversionHelpers.PremultiplyValue(alpha, src [sd + 2]);  // move red back
					dest [sd + 1] = ConversionHelpers.PremultiplyValue(alpha, src [sd + 1]);
					dest [sd + 2] = ConversionHelpers.PremultiplyValue(alpha, temp);
					dest [sd + 3] = alpha;

				}
			}

		}

		// Our internal format is pre-multiplied alpha
		void Convert_BGR_888_To_P_RGBA_8888(IntPtr source, IntPtr destination, int scanLength)
		{

			unsafe 
			{
				byte* src = (byte*)source;
				byte* dest = (byte*)destination;

				byte temp = 0;
				byte alpha = 0;

				for (int sourceOffset = 0, destinationOffset = 0; sourceOffset < scanLength; sourceOffset+=3, destinationOffset+=4) 
				{
					alpha = 255;
					temp = src [sourceOffset];  // save off blue
					dest [destinationOffset] = (byte)(src [sourceOffset + 2]);  // move red back
					dest [destinationOffset + 1] = (byte)(src [sourceOffset + 1]);
					dest [destinationOffset + 2] = (byte)(temp);
					dest [destinationOffset + 3] = alpha;
				}
			}

		}

	}
}

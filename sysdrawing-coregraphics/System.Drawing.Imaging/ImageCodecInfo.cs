//
// System.Drawing.Imaging.ImageCodecInfo.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004,2006,2007 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace System.Drawing.Imaging
{

	public sealed class ImageCodecInfo
	{
		internal ImageCodecInfo()
		{
		}

		// properties
		internal ImageFormat Format { get; set; } // This is a workaround

		public Guid Clsid { get; set; }
		public string CodecName { get; set; }
		public string DllName { get; set; }
		public string FilenameExtension { get; set; }
		public ImageCodecFlags Flags { get; set; }
		public string FormatDescription { get; set; }
		public Guid FormatID { get; set; }
		public string MimeType { get; set; }
		[CLSCompliant(false)]
		public byte[][] SignatureMasks { get; set; }
		[CLSCompliant(false)]
		public byte[][] SignaturePatterns { get; set; }
		public int Version { get; set; }


		// methods		
		public static ImageCodecInfo[] GetImageDecoders()
		{
			Console.WriteLine("Not implemented: ImageCodecInfo.GetImageDecoders()");
			return new ImageCodecInfo[] { };
		}

		public static ImageCodecInfo[] GetImageEncoders()
		{
			// Workaround - formats supported by Bitmap.Save(), as saving falls back to it.
			return new ImageCodecInfo[] { 
				new ImageCodecInfo { Format = ImageFormat.Gif, FormatID = ImageFormat.Gif.Guid, CodecName="Gif codec" },
				new ImageCodecInfo { Format = ImageFormat.Icon, FormatID = ImageFormat.Icon.Guid, CodecName="Icon codec" },
				new ImageCodecInfo { Format = ImageFormat.Jpeg, FormatID = ImageFormat.Jpeg.Guid, CodecName="Jpeg codec" },
				new ImageCodecInfo { Format = ImageFormat.Png, FormatID = ImageFormat.Png.Guid, CodecName="Png codec" },
				new ImageCodecInfo { Format = ImageFormat.Tiff, FormatID = ImageFormat.Tiff.Guid, CodecName="Tiff codec" },
				new ImageCodecInfo { Format = ImageFormat.Wmf, FormatID = ImageFormat.Wmf.Guid, CodecName="Pdf codec" },
			};
		}
	}
}

//
// System.Drawing.Imaging.Metafile.cs
//
// Authors:
//	Christian Meyer, eMail: Christian.Meyer@cs.tum.edu
//	Dennis Hayes (dennish@raytek.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[Serializable]
	[Editor("System.Drawing.Design.MetafileEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
	        "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[System.Runtime.CompilerServices.TypeForwardedFrom("System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public sealed class Metafile : Image {

		internal Metafile (IntPtr ptr)
		{
			throw new PlatformNotSupportedException ();
		}

		internal Metafile (IntPtr ptr, Stream stream)
		{
			throw new PlatformNotSupportedException ();
		}

		public Metafile (Stream stream) 
		{
			if (stream == null)
				throw new ArgumentException ("stream");

			throw new PlatformNotSupportedException ();
		}

		public Metafile (string filename) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");
			if (filename.Length == 0)
				throw new ArgumentException ("filename");

			throw new PlatformNotSupportedException ();
		}

		public Metafile (IntPtr henhmetafile, bool deleteEmf) 
		{
			throw new PlatformNotSupportedException ();
		}

		public Metafile (IntPtr referenceHdc, EmfType emfType) :
			this (referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, emfType, null)
		{
		}

		public Metafile (IntPtr referenceHdc, Rectangle frameRect) :
			this (referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr referenceHdc, RectangleF frameRect) :
			this (referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader) 
		{
			throw new PlatformNotSupportedException ();
		}

		public Metafile (Stream stream, IntPtr referenceHdc) :
			this (stream, referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc) :
			this (fileName, referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr referenceHdc, EmfType emfType, string description) :
			this (referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, emfType, description)
		{
		}

		public Metafile (IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) :
			this (referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) :
			this (referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader, bool deleteWmf) 
		{
			throw new PlatformNotSupportedException ();
		}

		public Metafile (Stream stream, IntPtr referenceHdc, EmfType type) :
			this (stream, referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, type, null)
		{
		}

		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect) :
			this (stream, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect) :
			this (stream, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc, EmfType type) :
			this (fileName, referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, type, null)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect) :
			this (fileName, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}
		
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect) :
			this (fileName, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) :
			this (referenceHdc, frameRect, frameUnit, type, null)
		{
		}

		public Metafile (IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) :
			this (referenceHdc, frameRect, frameUnit, type, null)
		{
		}

		public Metafile (Stream stream, IntPtr referenceHdc, EmfType type, string description) :
			this (stream, referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, type, description)
		{
		}

		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) :
			this (stream, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) :
			this (stream, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc, EmfType type, string description) :
			this (fileName, referenceHdc, new RectangleF (), MetafileFrameUnit.GdiCompatible, type, description)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) :
			this (fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
		{
		}
		
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) :
			this (fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
		{
		}

		public Metafile (IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type,
			string description)
		{
			throw new PlatformNotSupportedException ();
		}

		public Metafile (IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type,
			string description)
		{
			throw new PlatformNotSupportedException ();
		}

		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit,
			EmfType type) : this (stream, referenceHdc, frameRect, frameUnit, type, null)
		{
		}

		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit,
			EmfType type) : this (stream, referenceHdc, frameRect, frameUnit, type, null)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit,
			EmfType type) : this (fileName, referenceHdc, frameRect, frameUnit, type, null)
		{
		}
		
		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit,
			string description) : this (fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, description)
		{
		}

		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit,
			EmfType type) : this (fileName, referenceHdc, frameRect, frameUnit, type, null)
		{
		}
		
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, 
			string description) : this (fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual,
			description) 
		{
		}
		
		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, 
			EmfType type, string description) 
		{
			if (stream == null)
				throw new NullReferenceException ("stream");

			throw new PlatformNotSupportedException ();
		}

		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, 
			EmfType type, string description) 
		{
			if (stream == null)
				throw new NullReferenceException ("stream");

			throw new PlatformNotSupportedException ();
		}

		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, 
			EmfType type, string description) 
		{
			throw new PlatformNotSupportedException ();
		}
		
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit,
			EmfType type, string description) 
		{
			throw new PlatformNotSupportedException ();
		}

		// methods

		public IntPtr GetHenhmetafile ()
		{
			throw new PlatformNotSupportedException ();
		}

		public MetafileHeader GetMetafileHeader ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static MetafileHeader GetMetafileHeader (IntPtr henhmetafile)
		{
			throw new PlatformNotSupportedException ();
		}

		public static MetafileHeader GetMetafileHeader (Stream stream)
		{
			throw new PlatformNotSupportedException ();
		}

		public static MetafileHeader GetMetafileHeader (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			throw new PlatformNotSupportedException ();
		}

		public static MetafileHeader GetMetafileHeader (IntPtr henhmetafile, WmfPlaceableFileHeader wmfHeader)
		{
			throw new PlatformNotSupportedException ();
		}

		public void PlayRecord (EmfPlusRecordType recordType, int flags, int dataSize, byte[] data)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

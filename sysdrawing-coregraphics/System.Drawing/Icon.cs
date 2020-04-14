//
// System.Drawing.Icon.cs
//
// Authors:
//   Gary Barnett (gary.barnett.mono@gmail.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//   Peter Dennis Bartok (pbartok@novell.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004-2008 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Runtime.InteropServices;
#if XAMARINMAC
using ImageIO;
using CoreGraphics;
using Foundation;
#else
using MonoMac.ImageIO;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#endif

namespace System.Drawing
{
	[Serializable]	
	[Editor ("System.Drawing.Design.IconEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter(typeof(IconConverter))]	
	public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		private ReferencedImageData imageData;
		private CGImage image;
		internal bool undisposable;

		public Icon (Icon original, int width, int height)
			: this (original, new Size (width, height))
		{
		}
		
		public Icon (Icon original, Size size)
		{
			if (original == null)
				throw new ArgumentException ("original");

			// The original icon was loaded from multi-image file.
			this.imageData = original.imageData.Acquire();
			if (!LoadImageWithSize(size))
				this.image = original.image.Clone();
		}
		
		public Icon (Stream stream) : this (stream, 32, 32) 
		{
		}
		
		public Icon (Stream stream, Size size) : this(stream, size.Width, size.Height)
		{
		}

		public Icon (Stream stream, int width, int height)
		{
			InitFromStreamWithSize (stream, width, height);
		}
		
		public Icon (string fileName)
		{
			using (FileStream fs = File.OpenRead (fileName)) {
				InitFromStreamWithSize (fs, 32, 32);
			}
		}
		
		public Icon (Type type, string resource)
		{
			if (resource == null)
				throw new ArgumentException ("resource");
			
			using (Stream s = type.Assembly.GetManifestResourceStream (type, resource)) {
				if (s == null) {
					string msg = Locale.GetText ("Resource '{0}' was not found.", resource);
					throw new FileNotFoundException (msg);
				}
				InitFromStreamWithSize (s, 32, 32);		// 32x32 is default
			}
		}
		
		private Icon (SerializationInfo info, StreamingContext context)
		{
			MemoryStream dataStream = null;
			int width=0;
			int height=0;
			foreach (SerializationEntry serEnum in info) {
				if (String.Compare(serEnum.Name, "IconData", true) == 0) {
					dataStream = new MemoryStream ((byte []) serEnum.Value);
				}
				if (String.Compare(serEnum.Name, "IconSize", true) == 0) {
					Size iconSize = (Size) serEnum.Value;
					width = iconSize.Width;
					height = iconSize.Height;
				}
			}
			if (dataStream != null) {
				dataStream.Seek (0, SeekOrigin.Begin);
				InitFromStreamWithSize (dataStream, width, height);
			}
		}

		void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
		{
			MemoryStream ms = new MemoryStream ();
			Save (ms);
			si.AddValue ("IconSize", this.Size, typeof (Size));
			si.AddValue ("IconData", ms.ToArray ());
		}
		
		public Icon (string fileName, int width, int height)
		{
			using (FileStream fs = File.OpenRead (fileName)) {
				InitFromStreamWithSize (fs, width, height);
			}
		}
		
		public Icon (string fileName, Size size)
		{
			using (FileStream fs = File.OpenRead (fileName)) {
				InitFromStreamWithSize (fs, size.Width, size.Height);
			}
		}
		
		public void Dispose ()
		{
			// SystemIcons requires this
			if (undisposable)
				return;

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.image != null)
			{
				this.image.Dispose();
				this.image = null;
			}
			if (this.imageData != null)
			{
				this.imageData.Release();
				this.imageData = null;
			}
		}

		public object Clone ()
		{
			return new Icon (this, Size);
		}
		
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static Icon FromHandle (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("handle");
			throw new NotImplementedException();
		}


		public void Save (Stream outputStream)
		{
			if (outputStream == null)
				throw new NullReferenceException ("outputStream");

			using (var stream = this.imageData.Data.AsStream())
				stream.CopyTo(outputStream);
		}
			
		public Bitmap ToBitmap ()
		{
			return new Bitmap(image);
		}

		public override string ToString ()
		{
			//is this correct, this is what returned by .Net
			return "<Icon>";			
		}
		
		[Browsable (false)]
		public IntPtr Handle {
			get {
				return IntPtr.Zero;
			}
		}

		[Browsable (false)]
		public int Height {
			get {
				return (int)image.Height;
			}
		}
		
		public Size Size {
			get {
				return new Size(Width, Height);
			}
		}
		
		[Browsable (false)]
		public int Width {
			get {
				return (int)image.Width;
			}
		}
		
		~Icon ()
		{
			Dispose (false);
		}
		
		private void InitFromStreamWithSize (Stream stream, int width, int height)
		{
			var data = NSData.FromStream(stream);
			this.imageData = new ReferencedImageData(data, CGImageSource.FromData(data));
			if (!LoadImageWithSize(new Size(width, height)))
				throw new ArgumentOutOfRangeException("stream");
		}

		private bool LoadImageWithSize(Size size)
		{
			var imageSource = imageData.ImageSource;
			int bestIndex = (int)imageSource.ImageCount; // intentionally out of range
			int highestGoodIndex = -1;
			Size bestSize = Size.Empty;
			for (int imageIndex = 0; imageIndex < imageSource.ImageCount; imageIndex++)
			{
				var properties = imageSource.GetPropertiesSafe(imageIndex);
				if (properties != null && properties.PixelWidth.Value <= size.Width && properties.PixelHeight.Value <= size.Height)
				{
					if (properties.PixelWidth.Value > bestSize.Width || properties.PixelHeight.Value > bestSize.Height)
					{
						bestSize = new Size(properties.PixelWidth.Value, properties.PixelHeight.Value);
						bestIndex = imageIndex;
					}
					highestGoodIndex = imageIndex;
				}
				if (bestSize == size)
					break;
			}

			if (bestIndex >= imageSource.ImageCount)
			{
				if (highestGoodIndex >= 0)
					bestIndex = highestGoodIndex;
				else
					return false;
			}

			this.image = imageSource.CreateImage(bestIndex, null);
			return this.image != null;
		}

		public static Icon ExtractAssociatedIcon(string filePath)
		{
            return SystemIcons.WinLogo;
			//throw new NotImplementedException ();
		}

		class ReferencedImageData
		{
			public NSData Data { get; private set; }
			public CGImageSource ImageSource { get; private set; }
			private int referenceCount;

			public ReferencedImageData(NSData data, CGImageSource imageSource)
			{
				this.Data = data;
				this.ImageSource = imageSource;
				this.referenceCount = 1;
			}

			public void Release()
			{
				if (--this.referenceCount == 0)
				{
					if (this.ImageSource != null)
					{
						this.ImageSource.Dispose();
						this.ImageSource = null;
					}
					if (this.Data != null)
					{
						this.Data.Dispose();
						this.Data = null;
					}
				}
			}

			public ReferencedImageData Acquire()
			{
				this.referenceCount++;
				return this;
			}
		}
	}

	static class CGImageSourceExtensions
	{
		// Does not throw, but returns null if properties can't be retrieved. 
		public static CoreGraphics.CGImageProperties GetPropertiesSafe(this CGImageSource imageSource, int imageIndex)
		{
			var ptr = CGImageSourceCopyPropertiesAtIndex(imageSource.Handle, imageIndex, IntPtr.Zero);
			if (ptr == IntPtr.Zero)
				return null;

			var dictionary = ObjCRuntime.Runtime.GetNSObject<NSDictionary>(ptr, true);
			return new CoreGraphics.CGImageProperties(dictionary);
		}

		[DllImport("/System/Library/Frameworks/ApplicationServices.framework/Versions/A/Frameworks/CoreGraphics.framework/CoreGraphics")]
		internal extern static IntPtr CGImageSourceCopyPropertiesAtIndex(IntPtr cgImageSource, nint index, IntPtr cfDictionaryOptions); // -> CFDictionaryRef
	}
}

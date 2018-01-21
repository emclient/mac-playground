//
// System.Drawing.PageSettings.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Diagnostics;
using System.Reflection;

namespace System.Drawing.Printing {
	
	public class PageSettings : ICloneable {

		public PageSettings ()
			: this (new PrinterSettings())
		{
		}

		public PageSettings (PrinterSettings printerSettings)
		{
			this.Margins = new Margins(10, 10, 10, 10);
			this.PaperSize = printerSettings.PaperSizes[0];
		}

		public Rectangle Bounds
		{
			get
			{
				// FIXME
				//NotImplemented(MethodBase.GetCurrentMethod());
				return GetBounds(null);
			}
		}

        public float HardMarginX { get; protected set; }

        public float HardMarginY { get; protected set; }

		public bool Color { get; set; }

		public bool Landscape { get; set; }

		public PaperSize PaperSize { get; set; }

		public Margins Margins { get; set; }

		public PrinterSettings PrinterSettings { get; set; }		

		public PaperSource PaperSource { get; set; }

		public RectangleF PrintableArea
		{ 
			get
			{
				// FIXME
				NotImplemented(MethodBase.GetCurrentMethod());
				return RectangleF.Empty;
			}
		}

		public PrinterResolution PrinterResolution { get; set; }

		public void CopyToHdevmode(IntPtr hdevmode)
		{
			// FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetHdevmode(IntPtr hdevmode)
		{
			// FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public object Clone()
		{
			// FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
			return new PageSettings();
		}

		internal Rectangle GetBounds(object modeHandle)
		{
			// FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
			return new Rectangle(0, 0, PaperSize.Width, PaperSize.Height);
		}

		internal static void NotImplemented(MethodBase method)
		{
			Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name);
		}
	}
}
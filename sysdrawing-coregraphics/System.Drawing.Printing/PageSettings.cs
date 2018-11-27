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
using System.Drawing.Mac;
#if XAMARINMAC
using AppKit;
using CoreGraphics;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
#endif

namespace System.Drawing.Printing {
	
	public class PageSettings : ICloneable {

		internal NSPrintInfo print_info;
		private PaperSize paper_size;

		public PageSettings ()
			: this (new PrinterSettings())
		{
		}

		public PageSettings (PrinterSettings printerSettings)
		{
			this.PrinterSettings = printerSettings;
			print_info = new NSPrintInfo(NSPrintInfo.SharedPrintInfo.Dictionary);
			print_info.Printer = PrinterWithNameOrDefaultPrinter(printerSettings.PrinterName);
			paper_size = new PaperSize(print_info.PaperName, (int)print_info.PaperSize.Width, (int)print_info.PaperSize.Height);
		}

		private PageSettings (PageSettings pageSettings)
		{
			this.PrinterSettings = pageSettings.PrinterSettings;
			print_info = new NSPrintInfo(pageSettings.print_info.Dictionary);
			print_info.Printer = PrinterWithNameOrDefaultPrinter(PrinterSettings.PrinterName);
			paper_size = pageSettings.PaperSize;
		}

		internal static NSPrinter PrinterWithNameOrDefaultPrinter(string printerName)
		{
			NSPrinter printer = null;
			if (!string.IsNullOrEmpty(printerName))
				try { printer = NSPrinter.PrinterWithName(printerName); } catch { }
			return printer ?? new NSPrinter();
		}

		public Rectangle Bounds
		{
			get {
				return print_info.ImageablePageBounds.ToRectangle();
				//return new Rectangle(0, 0, PaperSize.Width, PaperSize.Height);
			}
		}

        public float HardMarginX { get; protected set; }

        public float HardMarginY { get; protected set; }

		public bool Color { get; set; }

		public bool Landscape
		{
			get {
				return print_info.Orientation == NSPrintingOrientation.Landscape;
			}
			set {
				print_info.Orientation = value ? NSPrintingOrientation.Landscape : NSPrintingOrientation.Portrait;
			}
		}

		public PaperSize PaperSize
		{
			get { return paper_size; }
			set {
				paper_size = value;
				print_info.PaperName = paper_size.PaperName;
				print_info.PaperSize = new CGSize(paper_size.Width, paper_size.Height);
			}
		}

		public Margins Margins
		{
			get {
				return new Margins((int)print_info.LeftMargin, (int)print_info.RightMargin, (int)print_info.TopMargin, (int)print_info.BottomMargin);
			}
			set {
				print_info.LeftMargin = value.Left;
				print_info.RightMargin = value.Right;
				print_info.TopMargin = value.Top;
				print_info.BottomMargin = value.Bottom;
			}
		}

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
			return new PageSettings(this);
		}

		internal static void NotImplemented(MethodBase method)
		{
			Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name);
		}
	}
}
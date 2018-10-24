//
// System.Drawing.PrinterSettings.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//
// (C) 2002 Ximian, Inc
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#if XAMARINMAC
using AppKit;
using MacApi.PrintCore;
using PrintCore;
#elif MONOMAC
using MonoMac.AppKit;
#endif

namespace System.Drawing.Printing {
	
	[Serializable]
	public class PrinterSettings : ICloneable {
		private int from_page;
		private int to_page;
		private int minimum_page;
		private int maximum_page;
		private short copies;
		private PrintRange print_range;
		private string printer_name;
		internal PMPrinter printer;
		internal PageSettings page_settings;

		public PrinterSettings ()
		{
			printer = PMPrinterEx.DefaultPrinter();
			printer_name = printer?.Name;
			page_settings = new PageSettings(this);
			//PaperSizes = new PaperSizeCollection(new[] { new PaperSize("Letter", (int)(8.5f * 72f), (int)(11f * 72f)) });
			PaperSources = new PaperSourceCollection(new PaperSource[] { });
		}
		
		public object Clone ()
		{
			// FIXME
			return new PrinterSettings ();
		}

		public bool IsValid {
			get { return printer != null; }
		}

		public int FromPage
		{
			get { return from_page; }
			set {
				if (value < 0)
					throw new ArgumentException ("The value of the FromPage property is less than zero");

				from_page = value;
			}
		}

		public int ToPage
		{
			get { return to_page; }
			set {
				if (value < 0)
					throw new ArgumentException ("The value of the FromPage property is less than zero");

				to_page = value;
			}
		}

		public int MaximumPage
		{
			get { return maximum_page; }
			set {
				// This not documented but behaves like MinimumPage
				if (value < 0)
					throw new ArgumentException ("The value of the MaximumPage property is less than zero");

				maximum_page = value;
			}
		}

		public int MinimumPage
		{
			get { return minimum_page; }
			set {
				if (value < 0)
					throw new ArgumentException ("The value of the MaximumPage property is less than zero");

				minimum_page = value;
			}
		}

		public short Copies
		{
			get { return copies; }
			set { 
				if (value < 0)
					throw new ArgumentException ("The value of the Copies property is less than zero.");

				copies = value;
			}
		}

		public PrintRange PrintRange
		{
			get { return print_range; }
			set
			{
				if (value != PrintRange.AllPages && value != PrintRange.Selection &&
					value != PrintRange.SomePages)
					throw new InvalidEnumArgumentException("The value of the PrintRange property is not one of the PrintRange values");

				print_range = value;
			}
		}

		public string PrinterName
		{
			get { return printer_name; }
			set {
				printer_name = value;
				printer = PMPrinterEx.PrinterWithName(value);
			}
		}

		public bool PrintToFile { get; set; }
		public string PrintFileName { get; set; }
		public bool CanDuplex { get; internal set; }
		public Duplex Duplex { get; internal set; }
		public bool Collate { get; set; }
		public bool IsPlotter { get; set; }
		public int LandscapeAngle { get; internal set; }
		public bool SupportsColor { get; internal set; }
		public bool PrintDialogDisplayed { get; set; }
		public PaperSourceCollection PaperSources { get; set; }

		public PaperSizeCollection PaperSizes {
			get {
				var sizes = new PaperSizeCollection();
				if (printer != null)
					foreach (var paper in printer.PaperList)
						sizes.Add(new PaperSize(paper.GetLocalizedName(printer), (int)paper.Width, (int)paper.Height));
				return sizes;
			}
		}
		public PrinterResolutionCollection PrinterResolutions { get; internal set; }
		public object printer_capabilities { get; internal set; }

		public PageSettings DefaultPageSettings
		{
			get {
				return page_settings;
			}
		}

		public static PrinterSettings.StringCollection InstalledPrinters
		{
			get { return new StringCollection(NSPrinter.PrinterNames); }
		}

		public class PaperSourceCollection : ICollection, IEnumerable
		{
			ArrayList _PaperSources = new ArrayList();

			public PaperSourceCollection(PaperSource[] array) {
				foreach (PaperSource ps in array)
					_PaperSources.Add(ps);
			}

			public int Count { get { return _PaperSources.Count; } }
			int ICollection.Count { get { return _PaperSources.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }			

			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Add (PaperSource paperSource) { return _PaperSources.Add (paperSource); }
			public void CopyTo (PaperSource[] paperSources, int index)  { throw new NotImplementedException (); }

			public virtual PaperSource this[int index] {
				get { return _PaperSources[index] as PaperSource; }
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PaperSources.GetEnumerator();
			}

			public IEnumerator GetEnumerator()
			{
				return _PaperSources.GetEnumerator();
			}

			void ICollection.CopyTo(Array array, int index)
			{
				_PaperSources.CopyTo(array, index);
			}

			internal void Clear ()
			{ 
				_PaperSources.Clear (); 
			}
		}

		public class PaperSizeCollection : ICollection, IEnumerable
		{
			ArrayList _PaperSizes = new ArrayList();

			public PaperSizeCollection(PaperSize[] array) {
				foreach (PaperSize ps in array)
					_PaperSizes.Add(ps);
			}

			internal PaperSizeCollection()
			{
			}

			public int Count { get { return _PaperSizes.Count; } }
			int ICollection.Count { get { return _PaperSizes.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }			
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Add (PaperSize paperSize) {return _PaperSizes.Add (paperSize); }	
			public void CopyTo (PaperSize[] paperSizes, int index) {throw new NotImplementedException (); }			

			public virtual PaperSize this[int index] {
				get { return _PaperSizes[index] as PaperSize; }
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PaperSizes.GetEnumerator();
			}

			public IEnumerator GetEnumerator()
			{
				return _PaperSizes.GetEnumerator();
			}

			void ICollection.CopyTo(Array array, int index)
			{
				_PaperSizes.CopyTo(array, index);
			}

			internal void Clear ()
			{ 
				_PaperSizes.Clear (); 
			}
		}

		public class PrinterResolutionCollection : ICollection, IEnumerable
		{
			ArrayList _PrinterResolutions = new ArrayList();

			public PrinterResolutionCollection(PrinterResolution[] array) {
				foreach (PrinterResolution pr in array)
					_PrinterResolutions.Add(pr);
			}

			public int Count { get { return _PrinterResolutions.Count; } }
			int ICollection.Count { get { return _PrinterResolutions.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }			
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Add (PrinterResolution printerResolution) { return _PrinterResolutions.Add (printerResolution); }
			public void CopyTo (PrinterResolution[] printerResolutions, int index) {throw new NotImplementedException (); }

			public virtual PrinterResolution this[int index] {
				get { return _PrinterResolutions[index] as PrinterResolution; }
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PrinterResolutions.GetEnumerator();
			}

			public IEnumerator GetEnumerator()
			{
				return _PrinterResolutions.GetEnumerator();
			}

			void ICollection.CopyTo(Array array, int index)
			{
				_PrinterResolutions.CopyTo(array, index);
			}

			internal void Clear ()
			{ 
				_PrinterResolutions.Clear (); 
			}
		}
		public class StringCollection : ICollection, IEnumerable
		{
			ArrayList _Strings = new ArrayList();

			public StringCollection(string[] array) {
				foreach (string s in array)
					_Strings.Add(s);
			}

			public int Count { get { return _Strings.Count; } }
			int ICollection.Count { get { return _Strings.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }

			public virtual string this[int index] {
				get { return _Strings[index] as string; }
			}
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Add (string value) { return _Strings.Add (value); }
			public void CopyTo (string[] strings, int index) {throw new NotImplementedException (); }      			

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _Strings.GetEnumerator();
			}

			public IEnumerator GetEnumerator()
			{
				return _Strings.GetEnumerator();
			}

			void ICollection.CopyTo(Array array, int index)
			{
				_Strings.CopyTo(array, index);
			}			
		}
	}
}
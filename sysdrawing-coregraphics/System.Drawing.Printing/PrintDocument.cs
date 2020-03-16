//
// System.Drawing.PrintDocument.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//
// (C) 2002 Ximian, Inc
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

using System.ComponentModel;

namespace System.Drawing.Printing {
	
	[DefaultEvent ("PrintPage"), DefaultProperty ("DocumentName")]
	[ToolboxItemFilter ("System.Drawing.Printing", ToolboxItemFilterType.Allow)]
	public class PrintDocument : Component {
		
		public event PrintEventHandler BeginPrint;
		public event PrintEventHandler EndPrint;
		public event PrintPageEventHandler PrintPage;
		public event QueryPageSettingsEventHandler QueryPageSettings;

		public PrintDocument ()
		{
			DefaultPageSettings = new PageSettings ();
			PrinterSettings = new PrinterSettings ();
			PrintController = new StandardPrintController();
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public PageSettings DefaultPageSettings { get; set; }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public PrinterSettings PrinterSettings { get; set; }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public PrintController PrintController { get; set; }

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public bool OriginAtMargins { get; set; }

		public string DocumentName { get; set; }

		internal void _OnBeginPrint(PrintEventArgs e)
		{
			OnBeginPrint(e);
		}

		internal void _OnEndPrint(PrintEventArgs e)
		{
			OnEndPrint(e);
		}

		internal void _OnPrintPage(PrintPageEventArgs e)
		{
			OnPrintPage(e);
		}

		internal void _OnQueryPageSettings(QueryPageSettingsEventArgs e)
		{
			OnQueryPageSettings(e);
		}

		protected virtual void OnBeginPrint(PrintEventArgs e)
		{
			if (BeginPrint != null)
				BeginPrint(this, e);
		}

		protected virtual void OnEndPrint(PrintEventArgs e)
		{
			if (EndPrint != null)
				EndPrint(this, e);
		}

		protected virtual void OnPrintPage(PrintPageEventArgs e)
		{
			if (PrintPage != null)
				PrintPage(this, e);
		}

		protected virtual void OnQueryPageSettings(QueryPageSettingsEventArgs e)
		{
			if (QueryPageSettings != null)
				QueryPageSettings(this, e);
		}

		public void Print()
		{
			//if (!this.PrinterSettings.IsDefaultPrinter && !this.PrinterSettings.PrintDialogDisplayed)
			//{
			//	IntSecurity.AllPrinting.Demand();
			//}
			PrintController controller = PrintController;
			controller.Print(this);
		}

		public override string ToString()
		{
			return "[PrintDocument " + DocumentName + "]";
		}
	}
}
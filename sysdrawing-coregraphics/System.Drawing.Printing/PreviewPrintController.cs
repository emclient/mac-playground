//
// System.Drawing.PreviewPrintController.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Mac;
#if XAMARINMAC
using CoreGraphics;
using Foundation;
#elif MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#endif

namespace System.Drawing.Printing
{
	public class PreviewPrintController : PrintController
	{
		//bool useantialias;
		//ArrayList pageInfoList;
		NSMutableData previewData;
		CGContextPDF context;

		public PreviewPrintController()
		{
			//pageInfoList = new ArrayList ();
		}

		public override bool IsPreview { 
			get { return true; }
		}

		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			context.EndPage();
		}

		public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
		{
			if (!document.PrinterSettings.IsValid)
				throw new InvalidPrinterException(document.PrinterSettings);
		
			/* maybe we should reuse the images, and clear them? */
			//foreach (PreviewPageInfo pi in pageInfoList)
			//	pi.Image.Dispose ();

			//pageInfoList.Clear ();

			previewData = new NSMutableData();
#if XAMARINMAC
			context = new CGContextPDF(new CGDataConsumer(previewData));
#elif MONOMAC
			context = new CGContextPDF(new CGDataConsumer(previewData), new CGRect(), new CGPDFInfo());
#endif
		}

		public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
		{
			context.Dispose();
		}

		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			var page_rect = e.PageSettings.Bounds.ToCGRect();
			context.BeginPage(page_rect);
			context.SetFillColor(1f, 1f);
			context.FillRect(page_rect);
			return new Graphics(context, false);
		}
		
		public virtual bool UseAntiAlias {
			get;
			set;
		}

		public PreviewPageInfo [] GetPreviewPageInfo()
		{
#if XAMARINMAC
			if (previewData != null) {
				var pdfDocument = new CGPDFDocument(new CGDataProvider(previewData));
				List<PreviewPageInfo> pi = new List<PreviewPageInfo>();
				for (int pageNo = 1; pageNo <= pdfDocument.Pages; pageNo++) {
					var page = pdfDocument.GetPage(pageNo);
					var mediaBox = page.GetBoxRect(CGPDFBox.Media);
					pi.Add(new PreviewPageInfo(new PDFImage(pdfDocument, page), new Size((int)mediaBox.Width, (int)mediaBox.Height)));
				}
				return pi.ToArray();
			}
#endif
			return new PreviewPageInfo[0];
		}

		class PDFImage : Image {
			public PDFImage (CGPDFDocument document, CGPDFPage page)
			{
				this.nativeMetafile = document;
				this.nativeMetafilePage = page;
			}
		}
	}
}

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace FormsTest
{
	public partial class MainForm
	{
		PrintPreviewDialog printPreviewDialog;
		PrintDocument printDocument;

        const string printDocumentContents = "This is to be printed\nLine 2\nAnotherLine";

        // SWF Preview dialog
        public void PrintPreview()
		{

			printDocument = new PrintDocument();
            printDocument.PrintPage += printDocument_PrintPage;

            printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.PrintPreviewControl.Zoom = 2;

            printPreviewDialog.ShowDialog();
        }

        // Native print dialog
        void ShowPrintDialog()
        {

            //https://github.com/brunophilipe/Noto/blob/master/Noto/View%20Controllers/Printing/PrintingView.swift
            //https://nshipster.com/uiprintinteractioncontroller/
            //https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/Printing/osxp_printingapi/osxp_printingapi.html#//apple_ref/doc/uid/10000083i-CH2-SW2
            //https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/Printing/osxp_printapps/osxp_printapps.html#//apple_ref/doc/uid/20000861-BAJBFGED
            //https://developer.apple.com/documentation/appkit/nsprintinfo?language=objc


            var document = new PrintDocument();
            document.PrintPage += printDocument_PrintPage;
            var dialog = new PrintDialog();
            dialog.Document = document;
            dialog.ShowDialog(this);
        }

        void printDocument_PrintPage(object sender, PrintPageEventArgs e)
		{
            var stringToPrint = printDocumentContents;

            int charactersOnPage = 0;
            int linesPerPage = 0;

            // Sets the value of charactersOnPage to the number of characters
            // of stringToPrint that will fit within the bounds of the page.
            e.Graphics.MeasureString(stringToPrint, this.Font,
                e.MarginBounds.Size, StringFormat.GenericTypographic,
                out charactersOnPage, out linesPerPage);

            // Draws the string within the bounds of the page.
            e.Graphics.DrawString(stringToPrint, this.Font, Brushes.Black,
            e.MarginBounds, StringFormat.GenericTypographic);

            // Remove the portion of the string that has been printed.
            stringToPrint = stringToPrint.Substring(charactersOnPage);

            // Check to see if more pages are to be printed.
            e.HasMorePages = (stringToPrint.Length > 0);

            // If there are no more pages, reset the string to be printed.
            if (!e.HasMorePages)
                stringToPrint = printDocumentContents;
        }
    }
}

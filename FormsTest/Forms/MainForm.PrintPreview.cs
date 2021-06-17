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
        string printDocumentContents;

        public void PrintPreview()
		{
            printDocumentContents = "This is to be printed\nLine 2\nAnotherLine";

			printDocument = new PrintDocument();
            printDocument.PrintPage += printDocument_PrintPage;

            printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = printDocument;
            //printPreviewDialog.PrintPreviewControl.Zoom = 2;

            printPreviewDialog.ShowDialog();
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

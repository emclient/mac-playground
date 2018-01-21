using System;
#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
#endif

namespace System.Drawing.Printing
{
	public class StandardPrintController : PrintController
	{
		IntPtr sessionHandle;

		public StandardPrintController()
		{
		}

		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			//SysPrn.GlobalService.EndPage(e.GraphicsContext);
			PMSessionEndPageNoDialog(sessionHandle);
		}

		public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
		{
			var result = PMCreateSession(out sessionHandle);
			// TODO: Set correct printer!
			result = PMSessionBeginCGDocumentNoDialog(
				sessionHandle,
				NSPrintInfo.SharedPrintInfo.GetPMPrintSettings(),
				NSPrintInfo.SharedPrintInfo.GetPMPageFormat());
			//IntPtr dc = SysPrn.GlobalService.CreateGraphicsContext(document.PrinterSettings, document.DefaultPageSettings);
			//e.GraphicsContext = new GraphicsPrinter(null, dc);
			//SysPrn.GlobalService.StartDoc(e.GraphicsContext, document.DocumentName, string.Empty);
		}

		public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
		{
			//SysPrn.GlobalService.EndDoc(e.GraphicsContext);
			PMSessionEndDocumentNoDialog(sessionHandle);
			PMRelease(sessionHandle);
		}

		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			var result = PMSessionBeginPageNoDialog(sessionHandle, IntPtr.Zero, IntPtr.Zero);
			IntPtr contextHandle;
			PMSessionGetCGGraphicsContext(sessionHandle, out contextHandle);
			e.SetGraphics(new Graphics(new CoreGraphics.CGContext(contextHandle), false));
			return e.Graphics;
			//SysPrn.GlobalService.StartPage(e.GraphicsContext);
			//return e.Graphics;
		}

		public const string PrintCoreLibrary = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/PrintCore.framework/PrintCore";

		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMCreateSession(out IntPtr sessionHandle);
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMRelease(IntPtr handle);
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMSessionGetCGGraphicsContext(IntPtr sessionHandle, out /* CGContextRef */ IntPtr contextHandle);
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMSessionBeginCGDocumentNoDialog(IntPtr sessionHandle, IntPtr printSettingsHandle, IntPtr pageFormatHandle);
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMSessionEndDocumentNoDialog(IntPtr sessionHandle);
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMSessionBeginPageNoDialog(IntPtr sessionHandle, IntPtr pageFormat, IntPtr pageFrame);
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMSessionEndPageNoDialog(IntPtr sessionHandle);
	}
}

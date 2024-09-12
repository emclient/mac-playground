using System;
using AppKit;
using PrintCore;
using CoreFoundation;
using CoreGraphics;
using System.Diagnostics;
using System.Reflection;

namespace System.Drawing.Printing
{
	public class StandardPrintController : PrintController
	{
		IntPtr sessionHandle;

		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			PMSessionEndPageNoDialog(sessionHandle);
		}

		public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
		{
			var printSettings = new PMPrintSettings();
			printSettings.Collate = document.PrinterSettings.Collate;
			switch (document.PrinterSettings.Duplex) {
				case Duplex.Simplex: printSettings.DuplexMode = PMDuplexMode.None; break;
				case Duplex.Vertical: printSettings.DuplexMode = PMDuplexMode.Tumble; break;
				case Duplex.Horizontal: printSettings.DuplexMode = PMDuplexMode.NoTumble; break;
			}
			printSettings.SetPageRange((uint)document.PrinterSettings.MinimumPage, (uint)document.PrinterSettings.MaximumPage);
			printSettings.FirstPage = (uint)document.PrinterSettings.FromPage;
			printSettings.LastPage = (uint)document.PrinterSettings.ToPage;
			if (document.DocumentName != null) {
				using (var jobName = new CFString(document.DocumentName))
					PMPrintSettingsSetJobName(printSettings.Handle, jobName.Handle);
			}
			var printInfo = document.DefaultPageSettings.print_info;
			printInfo.Printer = NSPrinter.PrinterWithName(document.PrinterSettings.PrinterName);

			sessionHandle = GetPMPrintSession(printInfo);
			var result = PMSessionBeginCGDocumentNoDialog(sessionHandle, printSettings.Handle, GetPMPageFormat(printInfo));
		}

		public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
		{
			PMSessionEndDocumentNoDialog(sessionHandle);
		}

		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			var result = PMSessionBeginPageNoDialog(
				sessionHandle,
				e.PageSettings == null || e.PageSettings == document.DefaultPageSettings ? IntPtr.Zero : GetPMPageFormat(e.PageSettings.print_info),
				IntPtr.Zero);
			PMSessionGetCGGraphicsContext(sessionHandle, out var contextHandle);
			e.SetGraphics(Graphics.FromHdc(contextHandle));
			return e.Graphics;
		}

		private static IntPtr GetPMPrintSession(NSPrintInfo printInfo)
		{
			var m = printInfo.GetType().GetMethod("GetPMPrintSession", BindingFlags.Instance | BindingFlags.NonPublic);
			Debug.Assert(m != null);
			return m == null ? IntPtr.Zero : (IntPtr)m.Invoke(printInfo, Array.Empty<object>());
		}

		private static IntPtr GetPMPageFormat(NSPrintInfo printInfo)
		{
			var m = printInfo.GetType().GetMethod("GetPMPageFormat", BindingFlags.Instance | BindingFlags.NonPublic);
			Debug.Assert(m != null);
			return m == null ? IntPtr.Zero : (IntPtr)m.Invoke(printInfo, Array.Empty<object>());
		}

		public const string PrintCoreLibrary = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/PrintCore.framework/PrintCore";

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
		[Runtime.InteropServices.DllImport(PrintCoreLibrary)]
		static extern uint PMPrintSettingsSetJobName(IntPtr printSettingsHandle, IntPtr jobNameHandle);
	}
}

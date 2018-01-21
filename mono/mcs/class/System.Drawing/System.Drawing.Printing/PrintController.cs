//
// System.Drawing.PrintController.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System.Drawing.Printing {

	public abstract class PrintController {
		/*
		internal sealed class SafeDeviceModeHandle : SafeHandle
		{ 
			private SafeDeviceModeHandle()
				: base(IntPtr.Zero, true)
			{
			}

			internal SafeDeviceModeHandle(IntPtr handle)
				: base(IntPtr.Zero, true)  // "true" means "owns the handle"
			{
				SetHandle(handle);
			}

			protected override bool ReleaseHandle()
			{
				if (!IsInvalid)
				{
					//FIXME:
					//SafeNativeMethods.GlobalFree(new HandleRef(this, handle));
				}
				handle = IntPtr.Zero;

				return true;
			}

			public static implicit operator IntPtr(SafeDeviceModeHandle handle)
			{
				return (handle == null) ? IntPtr.Zero : handle.handle;
			}

			public static explicit operator SafeDeviceModeHandle(IntPtr handle)
			{
				return new SafeDeviceModeHandle(handle);
			}

			public override bool IsInvalid
			{
				get { return handle == IntPtr.Zero; }
			}
		}

		internal SafeDeviceModeHandle modeHandle = null;*/

		public virtual bool IsPreview { 
			get { return false; }
		}
		public virtual void OnEndPage (PrintDocument document, PrintPageEventArgs e)
		{
		}

		public virtual void OnStartPrint (PrintDocument document, PrintEventArgs e)
		{
		}

		public virtual void OnEndPrint (PrintDocument document, PrintEventArgs e)
		{
		}

		public virtual Graphics OnStartPage (PrintDocument document, PrintPageEventArgs e)
		{
			return null;
		}

		[ResourceExposure(ResourceScope.Process)]
		[ResourceConsumption(ResourceScope.Process)]
		internal void Print(PrintDocument document)
		{
			//IntSecurity.SafePrinting.Demand();
			// Most of the printing security is left to the individual print controller

			//
			// Get the PrintAction for this event
			PrintAction printAction;
			if (IsPreview)
			{
				printAction = PrintAction.PrintToPreview;
			}
			else {
				printAction = document.PrinterSettings.PrintToFile ? PrintAction.PrintToFile : PrintAction.PrintToPrinter;
			}

			// Check that user has permission to print to this particular printer
			PrintEventArgs printEvent = new PrintEventArgs(printAction);
			document._OnBeginPrint(printEvent);
			if (printEvent.Cancel)
			{
				document._OnEndPrint(printEvent);
				return;
			}

			OnStartPrint(document, printEvent);
			if (printEvent.Cancel)
			{
				document._OnEndPrint(printEvent);
				OnEndPrint(document, printEvent);
				return;
			}

			bool canceled = true;

			try
			{
				// To enable optimization of the preview dialog, add the following to the config file:
				// <runtime >
				//     <!-- AppContextSwitchOverrides values are in the form of 'key1=true|false;key2=true|false  -->
				//     <AppContextSwitchOverrides value = "Switch.System.Drawing.Printing.OptimizePrintPreview=true" />
				// </runtime >
				//canceled = LocalAppContextSwitches.OptimizePrintPreview ? PrintLoopOptimized(document) : PrintLoop(document);
				canceled = PrintLoop(document);
			}
			finally
			{
				try
				{
					try
					{
						document._OnEndPrint(printEvent);
						printEvent.Cancel = canceled | printEvent.Cancel;
					}
					finally
					{
						OnEndPrint(document, printEvent);
					}
				}
				finally
				{
					//if (!IntSecurity.HasPermission(IntSecurity.AllPrinting))
					//{
						// Ensure programs with SafePrinting only get to print once for each time they
						// throw up the PrintDialog.
						//IntSecurity.AllPrinting.Assert();
						document.PrinterSettings.PrintDialogDisplayed = false;
					//}
				}
			}
		}

		private bool PrintLoop(PrintDocument document)
		{
			QueryPageSettingsEventArgs queryEvent = new QueryPageSettingsEventArgs((PageSettings)document.DefaultPageSettings.Clone());
			for (;;)
			{
				document._OnQueryPageSettings(queryEvent);
				if (queryEvent.Cancel)
				{
					return true;
				}

				PrintPageEventArgs pageEvent = CreatePrintPageEvent(queryEvent.PageSettings);
				Graphics graphics = OnStartPage(document, pageEvent);
				pageEvent.SetGraphics(graphics);

				try
				{
					document._OnPrintPage(pageEvent);
					OnEndPage(document, pageEvent);
				}
				finally
				{
					pageEvent.Dispose();
				}

				if (pageEvent.Cancel)
				{
					return true;
				}
				else if (!pageEvent.HasMorePages)
				{
					return false;
				}
				else {
					// loop
				}
			}
		}

		private bool PrintLoopOptimized(PrintDocument document)
		{
			PrintPageEventArgs pageEvent = null;
			PageSettings documentPageSettings = (PageSettings)document.DefaultPageSettings.Clone();
			QueryPageSettingsEventArgs queryEvent = new QueryPageSettingsEventArgs(documentPageSettings);
			for (;;)
			{
				queryEvent.PageSettingsChanged = false;
				document._OnQueryPageSettings(queryEvent);
				if (queryEvent.Cancel)
				{
					return true;
				}

				if (!queryEvent.PageSettingsChanged)
				{
					// QueryPageSettings event handler did not change the page settings,
					// thus we use default page settings from the document object.
					if (pageEvent == null)
					{
						pageEvent = CreatePrintPageEvent(queryEvent.PageSettings);
					}
					else {
						// This is not the first page and the settings had not changed since the previous page, 
						// thus don't re-apply them.
						pageEvent.CopySettingsToDevMode = false;
					}

					Graphics graphics = OnStartPage(document, pageEvent);
					pageEvent.SetGraphics(graphics);
				}
				else {
					// Page settings were customized, so use the customized ones in the start page event.
					pageEvent = CreatePrintPageEvent(queryEvent.PageSettings);
					Graphics graphics = OnStartPage(document, pageEvent);
					pageEvent.SetGraphics(graphics);
				}

				try
				{
					document._OnPrintPage(pageEvent);
					OnEndPage(document, pageEvent);
				}
				finally
				{
					pageEvent.Graphics.Dispose();
					pageEvent.SetGraphics(null);
				}

				if (pageEvent.Cancel)
				{
					return true;
				}
				else if (!pageEvent.HasMorePages)
				{
					return false;
				}
			}
		}

		private PrintPageEventArgs CreatePrintPageEvent(PageSettings pageSettings)
		{
			//IntSecurity.AllPrintingAndUnmanagedCode.Assert();
			//Debug.Assert((modeHandle != null), "modeHandle is null.  Someone must have forgot to call base.StartPrint");
			Rectangle pageBounds = pageSettings.Bounds;
			Rectangle marginBounds = new Rectangle(pageSettings.Margins.Left,
												   pageSettings.Margins.Top,
												   pageBounds.Width - (pageSettings.Margins.Left + pageSettings.Margins.Right),
												   pageBounds.Height - (pageSettings.Margins.Top + pageSettings.Margins.Bottom));

			PrintPageEventArgs pageEvent = new PrintPageEventArgs(null, marginBounds, pageBounds, pageSettings);
			return pageEvent;
		}
	}
}

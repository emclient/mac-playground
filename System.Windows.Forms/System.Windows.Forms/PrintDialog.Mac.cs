using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Mac;
using System.Drawing.Printing;
using System.Windows.Forms.CocoaInternal;

namespace System.Windows.Forms
{
	[Designer ("System.Windows.Forms.Design.PrintDialogDesigner, " + Consts.AssemblySystem_Design)]
	[DefaultProperty ("Document")]
	public sealed class PrintDialog : CommonDialog
	{
		PrintDocument document;
		PrinterSettings settings;
		bool pageSettingsTransferred;

		public PrintDialog()
		{
			Reset();
		}

		public override void Reset()
		{
			settings = null;
			AllowPrintToFile = true;
			AllowSelection = false;
			AllowSomePages = false;
			PrintToFile = false;
			ShowHelp = false;
			ShowNetwork = true;
		}

		[DefaultValue(false)]
		public bool AllowCurrentPage
		{
			get;
			set;
		}

		[DefaultValue(true)]
		public bool AllowPrintToFile
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool AllowSelection
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool AllowSomePages
		{
			get;
			set;
		}

		[DefaultValue(null)]
		public PrintDocument Document
		{
			get
			{
				return document;
			}

			set
			{
				document = value;
				settings = (value == null) ? new PrinterSettings() : value.PrinterSettings;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings
		{
			get
			{
				if (settings == null)
					settings = new PrinterSettings();
				return settings;
			}

			set
			{
				if (value != null && value == settings)
					return;

				settings = (value == null) ? new PrinterSettings() : value;
				document = null;
			}
		}

		[DefaultValue(false)]
		public bool PrintToFile
		{
			get;
			set;
		}

		[DefaultValue(true)]
		public bool ShowNetwork
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool ShowHelp
		{
			get;
			set;
		}

		[MonoTODO("Stub, not implemented, will always use default dialog")]
		[DefaultValue(false)]
		public bool UseEXDialog
		{
			get;
			set;
		}

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			try
			{
				document.QueryPageSettings += Document_QueryPageSettings;

				var info = NSPrintInfo.SharedPrintInfo ?? new NSPrintInfo();
				var control = new PrintControl(document);
				control.CreateControl();

				var operation = NSPrintOperation.FromView(ObjCRuntime.Runtime.GetNSObject<NSView>(control.Handle));

				operation.ShowsPrintPanel = true;
				if (operation.PrintPanel == null)
					operation.PrintPanel = new NSPrintPanel();

				operation.PrintInfo = info;

				var panel = operation.PrintPanel;

				using (var context = new ModalDialogContext())
				{
					panel.Options |=
						NSPrintPanelOptions.ShowsCopies
						| (AllowSomePages ? NSPrintPanelOptions.ShowsPageRange : 0)
						| NSPrintPanelOptions.ShowsPaperSize
						| NSPrintPanelOptions.ShowsOrientation
						//| NSPrintPanelOptions.ShowsScaling
						| (AllowSelection ? NSPrintPanelOptions.ShowsPrintSelection : 0)
						| NSPrintPanelOptions.ShowsPageSetupAccessory
						| NSPrintPanelOptions.ShowsPreview;

					/*return*/ operation.RunOperation();
					return false; // Prevent printing again after closing the dialog (Windows standard print dialog only modifies printing options)
				}
			}
			finally
			{
				document.QueryPageSettings -= Document_QueryPageSettings;
			}
		}

		private void Document_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
		{
			if (NSPrintOperation.CurrentOperation is NSPrintOperation printOperation)
			{
				if (pageSettingsTransferred)
					e.PageSettings = printOperation.PrintInfo.ToPageSettings();
				else
				{
					pageSettingsTransferred = true;

					var info = printOperation.PrintInfo;
					var margins = e.PageSettings.Margins;
					info.LeftMargin = margins.Left;
					info.RightMargin = margins.Right;
					info.TopMargin = margins.Top;
					info.BottomMargin = margins.Bottom;

					var printer = NSPrinter.PrinterWithName(e.PageSettings.PrinterSettings.PrinterName);
					if (printer != null)
						info.Printer = printer;

					info.PaperSize = new CGSize(e.PageSettings.PaperSize.Width, e.PageSettings.PaperSize.Height);

					info.Orientation = e.PageSettings.Landscape ? NSPrintingOrientation.Landscape : NSPrintingOrientation.Portrait;
				}
			}
		}
	}

	class PrintControl : Control, IMacNativeControl
	{
		PrintView view;
		PrintDocument document;

		Size pageSize;
		PreviewPageInfo[] pageInfos;
		PreviewPrintController controller = new PreviewPrintController();

		int currentPage;

		public PrintControl(PrintDocument document)
		{
			this.document = document;
			Padding = Padding.Empty;
		}

		public NSView CreateView()
		{
			view = new PrintView();
			view.KnowsPageRangeHandler = view_KnowsPageRange;
			view.RectForPageHandler = view_RectForPage;
			return view;
		}

		bool view_KnowsPageRange(ref NSRange range)
		{
			InvalidatePreview();
			GeneratePreview();

			Size = GetPageSize();
			range = new NSRange(1, pageInfos.Length);

			return true;
		}

		CGRect view_RectForPage(nint page)
		{
			currentPage = (int)page;
			return new CGRect(CGPoint.Empty, pageSize.ToCGSize());
		}

		public int Columns { get; set; } = 1;
		public int Rows { get; set; } = 1;
		public int StartPage { get; set; } = 0;
		public double Zoom { get; set; } = 1;
		public bool AutoZoom { get; set; } = true;

		protected override void OnPaint(PaintEventArgs pevent)
		{
			if (pageInfos == null)
				GeneratePreview();

			if (pageInfos == null)
				return;

			int p = currentPage - 1;
			if (p < 0 || p >= pageInfos.Length)
				return;

			var image = pageInfos[p].Image;
			var dest = new Rectangle(Point.Empty, pageSize);
			pevent.Graphics.DrawImage(image, dest, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		protected void GeneratePreview()
		{
			if (document == null)
				return;

			if (pageInfos == null)
			{
				var oldController = document.PrintController;
				try
				{
					document.PrintController = controller;

					pageSize = GetPageSize();
					document.Print();
					pageInfos = controller.GetPreviewPageInfo();
				}
				catch (Exception e)
				{
					Diagnostics.Debug.WriteLine(e);
					pageInfos = new PreviewPageInfo[0];
				}
				finally
				{
					document.PrintController = oldController;
				}
			}
		}

		public Size GetPageSize()
		{
			return (NSPrintOperation.CurrentOperation?.PrintInfo ?? NSPrintInfo.SharedPrintInfo).PaperSize.ToSDSize();
		}

		public void InvalidatePreview()
		{
			if (pageInfos != null)
			{
				for (int i = 0; i < pageInfos.Length; i++)
					if (pageInfos[i].Image != null)
						pageInfos[i].Image.Dispose();
				pageInfos = null;
			}
		}
	}

	class PrintView : MonoView
	{
		public PrintView() : this(CGRect.Empty) { }
		public PrintView(CGRect frame) : this(frame, 0, 0) { }
		public PrintView(CGRect frame, WindowStyles style, WindowExStyles exStyle) : base(XplatUICocoa.GetInstance(), frame, style, exStyle) { }
		public PrintView(NativeHandle handle) : base(handle) { }

		public delegate bool KnowsPageRangeDelegate(ref NSRange range);
		public delegate CGRect RectForPageDelegate(nint pageNumber);

		public KnowsPageRangeDelegate KnowsPageRangeHandler { get; set; }
		public RectForPageDelegate RectForPageHandler { get; set; }

		public override bool KnowsPageRange(ref NSRange range)
		{
			return KnowsPageRangeHandler != null ? KnowsPageRangeHandler.Invoke(ref range) : base.KnowsPageRange(ref range);
		}

		public override CGRect RectForPage(nint pageNumber)
		{
			return RectForPageHandler != null ? RectForPageHandler.Invoke(pageNumber) : base.RectForPage(pageNumber);
		}
	}
}

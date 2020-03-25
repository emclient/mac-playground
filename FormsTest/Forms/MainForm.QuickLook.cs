#if MAC
using System;
using AppKit;
using QuickLookUI;

namespace FormsTest
{
	public partial class MainForm
	{
		class QLDelegate : QLPreviewPanelDelegate
		{
		}

		class QLDataSource : QLPreviewPanelDataSource
		{
			public override nint NumberOfPreviewItemsInPreviewPanel(QLPreviewPanel panel)
			{
				return 1;
			}

			public override IQLPreviewItem PreviewItemAtIndex(QLPreviewPanel panel, nint index)
			{
				return null;
			}
		}

		void ToggleQuickLookPanel()
		{
			if (QLPreviewPanel.SharedPreviewPanelExists() && QLPreviewPanel.SharedPreviewPanel().IsVisible)
			{
				QLPreviewPanel.SharedPreviewPanel().OrderOut(NSApplication.SharedApplication);
			}
			else
			{
				var panel = QLPreviewPanel.SharedPreviewPanel();
				panel.DataSource = new QLDataSource();
				panel.Delegate = new QLDelegate();
				panel.MakeKeyAndOrderFront(NSApplication.SharedApplication);
			}
		}
	}
}
#endif

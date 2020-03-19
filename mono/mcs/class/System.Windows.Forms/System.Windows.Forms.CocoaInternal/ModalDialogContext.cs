using System;


#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	public class ModalDialogContext : IDisposable
	{
		NSWindow keyWindow;
		NSResponder firstResponder;

		public ModalDialogContext()
		{
			keyWindow = NSApplication.SharedApplication.KeyWindow;
			if (keyWindow != null)
				firstResponder = keyWindow.FirstResponder;
		}

		public void Dispose()
		{
			if (keyWindow != null)
			{
				if (keyWindow.CanBecomeKeyWindow)
					keyWindow.MakeKeyAndOrderFront(null);

				if (firstResponder != null && firstResponder.AcceptsFirstResponder())
					keyWindow.MakeFirstResponder(firstResponder);
			}
		}
	}
}

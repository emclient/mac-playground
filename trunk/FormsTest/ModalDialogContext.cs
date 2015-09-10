using System;
using MonoMac.AppKit;
using System.Windows.Forms;

namespace FormsTest
{
	public class ModalDialogContext : IDisposable
	{
		NSWindow keyWindow;
		NSResponder firstResponder;

		public ModalDialogContext() {
			keyWindow = NSApplication.SharedApplication.KeyWindow;
			if (keyWindow != null)
				firstResponder = keyWindow.FirstResponder;
		}

		public void Dispose ()
		{
			if (keyWindow != null) {
				if (keyWindow.CanBecomeKeyWindow) {
					try {
						keyWindow.BecomeKeyWindow ();
					} catch {
					}
				}
				
				if (firstResponder != null)
					firstResponder.BecomeFirstResponder();
			}
		}
	}
}


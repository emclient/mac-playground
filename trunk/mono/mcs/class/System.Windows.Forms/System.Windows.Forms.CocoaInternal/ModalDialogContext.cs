#if MONOMAC

using System;
using MonoMac.AppKit;

namespace System.Windows.Forms.CocoaInternal
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

#endif // MONOMAC
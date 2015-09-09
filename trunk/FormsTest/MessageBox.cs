#if MAC

using System;
using System.Windows.Forms;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;

namespace FormsTest
{
	public class MessageBox {

		static string SystemIconsFolderPath = "/System/Library/CoreServices/CoreTypes.bundle/Contents/Resources/";
		static string AlertStopIconFilename = "AlertStopIcon";
		static string AliasBadgeIconFilename = "AliasBadgeIcon";
		static string AlertNoteIconFilename = "AlertNoteIcon";
		static string ToolbarInfoIconFilename = "ToolbarInfo";

		private class Context : IDisposable
		{
			NSWindow keyWindow;
			NSResponder firstResponder;

			public Context() {
				keyWindow = NSApplication.SharedApplication.KeyWindow;
				if (keyWindow != null)
					firstResponder = keyWindow.FirstResponder;
			}

			public void Dispose ()
			{
				if (keyWindow != null) {
					keyWindow.BecomeKeyWindow();
					if (firstResponder != null)
						firstResponder.BecomeFirstResponder();
				}
			}
		}

		const MessageBoxButtons Ok = MessageBoxButtons.OK;
		const MessageBoxIcon NoIcon = MessageBoxIcon.None;

		private MessageBox ()
		{
		}

		public static DialogResult Show (string text)
		{
			return Show (null, text, null, Ok, NoIcon, null, null, null, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text)
		{
			return Show (owner, text, null, Ok, NoIcon, null, null, null, null, null);
		}

		public static DialogResult Show (string text, string caption)
		{
			return Show (null, text, caption, Ok, NoIcon, null, null, null, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons)
		{
			return Show (null, text, caption, buttons, NoIcon, null, null, null, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
		{
			return Show (owner, text, caption, buttons, NoIcon, null, null, null, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return Show (owner, text, caption, buttons, icon, null, null, null, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption)
		{
			return Show (owner, text, caption, Ok, NoIcon, null, null, null, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return Show (null, text, caption, buttons, icon, null, null, null, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return Show (null, text, caption, buttons, icon, defaultButton, null, null, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return Show (owner, text, caption, buttons, icon, defaultButton, null, null, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			return Show (null, text, caption, buttons, icon, defaultButton, options, null, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			return Show (owner, text, caption, buttons, icon, defaultButton, options, null, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, bool displayHelpButton)
		{
			// TODO: displayHelpButton
			return Show (null, text, caption, buttons, icon, defaultButton, options, null, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath)
		{
			return Show (null, text, caption, buttons, icon, defaultButton, options, helpFilePath, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, string keyword)
		{
			// TODO: keyword
			return Show (null, text, caption, buttons, icon, defaultButton, options, helpFilePath, null, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator)
		{
			return Show (null, text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator, null);
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator, object param)
		{
			return Show (null, text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator, param);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath)
		{
			return Show (owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, string keyword)
		{
			return Show (owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, null, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator)
		{
			return Show (owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator, null);
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator, object param)
		{
			return Show (owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator, param);
		}

		internal static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons? buttons, MessageBoxIcon? icon, MessageBoxDefaultButton? defaultButton, MessageBoxOptions? options, string helpFilePath, HelpNavigator? navigator, object param)
		{
			using (var c = new Context ()) {
				var alert = new NSAlert ();

				if (text != null)
					alert.InformativeText = text;

				if (caption != null)
					alert.MessageText = caption;

				AddButtons (alert, buttons);
				AddIcon (alert, icon);

				alert.RunModal ();
				return new DialogResult ();
			}
		}

		internal static void AddButtons (NSAlert alert, MessageBoxButtons? buttons)
		{
			if (!buttons.HasValue)
				return;

			switch (buttons.Value)
			{
			case MessageBoxButtons.AbortRetryIgnore:
				alert.AddButton (Loc ("Abort"));
				alert.AddButton (Loc ("Retry"));
				alert.AddButton (Loc ("Ignore"));
				break;
			case MessageBoxButtons.OK:
				alert.AddButton (Loc ("OK"));
				break;
			case MessageBoxButtons.OKCancel:
				alert.AddButton (Loc ("OK"));
				alert.AddButton (Loc ("Cancel"));
				break;
			case MessageBoxButtons.RetryCancel:
				alert.AddButton (Loc ("Retry"));
				alert.AddButton (Loc ("Cancel"));
				break;
			case MessageBoxButtons.YesNo:
				alert.AddButton (Loc ("Yes"));
				alert.AddButton (Loc ("No"));
				break;
			case MessageBoxButtons.YesNoCancel:
				alert.AddButton (Loc ("Yes"));
				alert.AddButton (Loc ("No"));
				alert.AddButton (Loc ("Cancel"));
				break;
			}
		}

		internal static void AddIcon (NSAlert alert, MessageBoxIcon? icon)
		{
			if (!icon.HasValue)
				return;

			switch (icon.Value) {
			case MessageBoxIcon.None:
//				alert.AlertStyle = NSAlertStyle.Informational;
				break;
//			case MessageBoxIcon.Error:
//			case MessageBoxIcon.Hand:
			case MessageBoxIcon.Stop:
				var ico = SystemIconWithApplicationIconBadge (AlertStopIconFilename);
				if (ico != null)
					alert.Icon = ico;
				else
					alert.AlertStyle = NSAlertStyle.Critical;
				break;
			case MessageBoxIcon.Question:
				break;
//			case MessageBoxIcon.Exclamation:
			case MessageBoxIcon.Warning:
				alert.AlertStyle = NSAlertStyle.Critical;
//				alert.AlertStyle = NSAlertStyle.Warning; // Warning style is just application icon
				break;
//			case MessageBoxIcon.Asterisk:
			case MessageBoxIcon.Information:
				alert.AlertStyle = NSAlertStyle.Informational;
				break;
			}
		}

		internal static string Loc(string key)
		{
			// TODO: Localize
			return key;
		}

		internal static NSImage SystemIconWithApplicationIconBadge(string systemIconFileName)
		{
			return MessageBox.SystemIconWithBadge (systemIconFileName, NSApplication.SharedApplication.ApplicationIconImage);
		}

		internal static NSImage SystemIconWithBadge(string systemIconFileName, NSImage badge)
		{
			return IconWithBadge(SystemIcon(systemIconFileName), badge);
		}

		internal static NSImage IconWithBadge(NSImage icon, NSImage badge)
		{
			if (icon == null)
				return null;

			if (badge == null)
				return icon;

			var result = (NSImage)icon.Copy();
			result.LockFocus();
			NSGraphicsContext.CurrentContext.ImageInterpolation = NSImageInterpolation.High;
			var dstRect = new CGRect(icon.Size.Width / 2, 0, icon.Size.Width / 2, icon.Size.Height / 2);
			var srcRect = new CGRect (0, 0, badge.Size.Width, badge.Size.Height);
			badge.DrawInRect(dstRect, srcRect, NSCompositingOperation.SourceOver, 1.0f);
			result.UnlockFocus();
			return result;
		}

		internal static NSImage SystemIcon(string filename)
		{
			if (filename.IndexOf (".") == -1)
				filename += ".icns";
			
			var path = System.IO.Path.Combine (SystemIconsFolderPath, filename);
			if (!NSFileManager.DefaultManager.FileExists (path))
				return null;

			var url = NSUrl.FromFilename (path);
			return new NSImage (url);
		}
	}
}

#endif // MAC

#if MONOMAC || XAMARINMAC

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.CocoaInternal;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
#elif XAMARINMAC
using AppKit;
using Foundation;
using CoreGraphics;
#endif

using R = System.Windows.Forms.MessageBoxFormResourcesMac;

namespace System.Windows.Forms
{
	internal class MessageBoxFormMac
	{
		public static bool UseWinStyleIcons = true;

		static string SystemIconsFolderPath = "/System/Library/CoreServices/CoreTypes.bundle/Contents/Resources/";
		static string AlertStopIconFilename = "AlertStopIcon";
		static string ToolbarInfoIconFilename = "ToolbarInfo";
		static string GenericQuestionMarkIcon = "GenericQuestionMarkIcon";
//		static string AliasBadgeIconFilename = "AliasBadgeIcon";
//		static string AlertNoteIconFilename = "AlertNoteIcon";

		const MessageBoxButtons Ok = MessageBoxButtons.OK;
		const MessageBoxIcon NoIcon = MessageBoxIcon.None;

		NSAlert alert;
		List<ButtonDef> buttons;
		IWin32Window owner;

		class ButtonDef {
			public DialogResult Result;
			public string Text;
		}

		protected MessageBoxFormMac ()
		{
		}

		internal MessageBoxFormMac(IWin32Window owner, string text, string caption, MessageBoxButtons? buttons = null, 
			MessageBoxIcon? icon = null, MessageBoxDefaultButton? defaultButton = null, MessageBoxOptions? options = null, 
			bool diplayHelpButton = false, string helpFilePath = null, HelpNavigator? navigator = null, object param = null)
		{
			this.owner = owner;
			this.buttons = new List<ButtonDef>();
			alert = new NSAlert ();

			if (text != null)
				alert.InformativeText = text;

			if (caption != null)
				alert.MessageText = caption;

			AddButtons(buttons, defaultButton);
			AddIcon(icon);
		}

		internal DialogResult RunDialog()
		{
//			if (owner != null)
//				TopMost = owner.TopMost;
//			XplatUI.AudibleAlert (alert_type);

			if (owner != null && owner.Handle != IntPtr.Zero) {
				// TODO
				//alert.RunSheetModal(window);
			}

			using (var c = new ModalDialogContext ()) {
				int index = (int)alert.RunModal() - (int)NSAlertButtonReturn.First;
				if (index >= buttons.Count)
					return DialogResult.Cancel;
				return buttons[index].Result;
			}
		}

		internal void AddButtons (MessageBoxButtons? buttons, MessageBoxDefaultButton? defaultButton)
		{
			if (!buttons.HasValue)
				return;

			switch (buttons.Value) {
			case MessageBoxButtons.AbortRetryIgnore:
				AddButton(DialogResult.Abort, R.Abort);
				AddButton(DialogResult.Retry, R.Retry);
				AddButton(DialogResult.Ignore, R.Ignore);
				break;
			case MessageBoxButtons.OK:
				AddButton(DialogResult.OK, R.OK);
				break;
			case MessageBoxButtons.OKCancel:
				AddButton(DialogResult.OK, R.OK);
				AddButton(DialogResult.Cancel, R.Cancel);
				break;
			case MessageBoxButtons.RetryCancel:
				AddButton(DialogResult.Retry, R.Retry);
				AddButton(DialogResult.Cancel, R.Cancel);
				break;
			case MessageBoxButtons.YesNo:
				AddButton(DialogResult.Yes, R.Yes);
				AddButton(DialogResult.No, R.No);
				break;
			case MessageBoxButtons.YesNoCancel:
				AddButton(DialogResult.Yes, R.Yes);
				AddButton(DialogResult.No, R.No);
				AddButton(DialogResult.Cancel, R.Cancel);
				break;
			default:
				AddButton(DialogResult.OK, R.OK);
				break;
			}

			// On Mac, the default button is always the first one => Move the default button to front.
			if (defaultButton.HasValue) {
				switch (defaultButton.Value) {
				case MessageBoxDefaultButton.Button1:
					break;
				case MessageBoxDefaultButton.Button2:
					MoveButtonToFront(1);
					break;
				case MessageBoxDefaultButton.Button3:
					MoveButtonToFront(2);
					break;
				}
			}

			DoAddButtons();
		}

		protected void MoveButtonToFront(int index)
		{
			if (buttons.Count > index) {
				var item = this.buttons[index];
				buttons.RemoveAt(index);
				buttons.Insert(0, item);
			}
		}

		protected void AddButton (DialogResult result, string text)
		{
			buttons.Add(new ButtonDef { Result = result, Text = text });
		}

		protected void DoAddButtons()
		{
			foreach (var button in buttons)
				alert.AddButton(button.Text);
		}

		protected void AddIcon(MessageBoxIcon? icon)
		{
			if (UseWinStyleIcons)
				AddIcon_Forms(icon);
			else
				AddIcon_Cocoa(icon);
		}

		// On Mac, alerts contain yellow triangle with app icon badge, or app icon only. There is nothing
		protected void AddIcon_Cocoa (MessageBoxIcon? icon)
		{
			if (!icon.HasValue)
				return;

			switch (icon.Value) {
			//case MessageBoxIcon.Exclamation:
			case MessageBoxIcon.Warning:
			case MessageBoxIcon.Question:
			case MessageBoxIcon.None:
			//alert.AlertStyle = NSAlertStyle.Informational;
				break;
			//case MessageBoxIcon.Error:
			//case MessageBoxIcon.Hand:
			case MessageBoxIcon.Stop:
				alert.AlertStyle = NSAlertStyle.Critical;
				break;
			//case MessageBoxIcon.Asterisk:
			case MessageBoxIcon.Information:
				alert.AlertStyle = NSAlertStyle.Informational;
				break;
			}
		}

		protected void AddIcon_Forms (MessageBoxIcon? iconType)
		{
			if (!iconType.HasValue)
				return;

			switch (iconType.Value) {
			case MessageBoxIcon.None:
				break;
			//case MessageBoxIcon.Error:
			//case MessageBoxIcon.Hand:
			case MessageBoxIcon.Stop:
				{
					AddIconOrSetStyle(SystemIconWithAppBadge (AlertStopIconFilename), NSAlertStyle.Critical);
					break;
				}
			case MessageBoxIcon.Question:
				AddIconOrSetStyle(SystemIconWithAppBadge (GenericQuestionMarkIcon), NSAlertStyle.Informational);
				break;
			//case MessageBoxIcon.Exclamation:
			case MessageBoxIcon.Warning:
				alert.AlertStyle = NSAlertStyle.Critical; // Yellow triangle
				break;
			//case MessageBoxIcon.Asterisk:
			case MessageBoxIcon.Information:
				{
					AddIconOrSetStyle(SystemIconWithAppBadge (ToolbarInfoIconFilename), NSAlertStyle.Critical);
					break;
				}
			}
		}

		protected void AddIconOrSetStyle (NSImage icon, NSAlertStyle style)
		{
			if (icon != null)
				alert.Icon = icon;
			else
				alert.AlertStyle = NSAlertStyle.Critical;
		}

		public void SetHelpData (string path, string keyword, HelpNavigator navigator, object param)
		{
			HelpFilePath = path;
			HelpKeyword = keyword;
			HelpNavigator = navigator;
			HelpParam = param;
		}

		internal string HelpFilePath { get; private set; }

		internal string HelpKeyword { get; private set; }

		internal HelpNavigator HelpNavigator { get; private set; }

		internal object HelpParam { get; private set; }

		internal static NSImage SystemIconWithAppBadge (string systemIconFileName)
		{
			return SystemIconWithBadge (systemIconFileName, NSApplication.SharedApplication.ApplicationIconImage);
		}

		internal static NSImage SystemIconWithBadge (string systemIconFileName, NSImage badge)
		{
			return IconWithBadge (SystemIcon (systemIconFileName), badge);
		}

		/// <summary>
		/// Combines two images into a new one, so that the "badge" image overlays the bottom-right quarter of the other image.
		/// </summary>
		/// <returns>The with badge.</returns>
		/// <param name="icon">Background of the new image</param>
		/// <param name="badge">Overlay image</param>
		internal static NSImage IconWithBadge (NSImage icon, NSImage badge)
		{
			if (icon == null)
				return null;

			if (badge == null)
				return icon;

			var result = (NSImage)icon.Copy ();
			result.LockFocus ();
			NSGraphicsContext.CurrentContext.ImageInterpolation = NSImageInterpolation.High;
			var dstRect = new CGRect (icon.Size.Width / 2, 0, icon.Size.Width / 2, icon.Size.Height / 2);
			var srcRect = new CGRect (0, 0, badge.Size.Width, badge.Size.Height);
			badge.DrawInRect (dstRect, srcRect, NSCompositingOperation.SourceOver, 1.0f);
			result.UnlockFocus ();
			return result;
		}

		/// <summary>
		/// Loads a given icon from icns file in the system bundle.
		/// </summary>
		/// <returns>The icon or nil if it does not exist.</returns>
		/// <param name="filename">Filename. With or without extension (.icns will be added if necessary).</param>
		internal static NSImage SystemIcon (string filename)
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

#endif
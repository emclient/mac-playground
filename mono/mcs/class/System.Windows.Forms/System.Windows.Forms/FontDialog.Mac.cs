#if MONOMAC || XAMARINMAC

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Mac;
using System.Security.Permissions;

#if XAMARINMAC
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#endif

using Strings = System.Windows.Forms.resources.Strings;

namespace System.Windows.Forms
{
	[DefaultEvent("Apply")]
	[DefaultProperty("Font")]
	public class FontDialog : CommonDialog
	{
		protected static readonly object EventApply;

		[DefaultValue(true)]
		public bool AllowScriptChange
		{
			get; set;
		}

		[DefaultValue(true)]
		public bool AllowSimulations
		{
			get; set;
		}

		[DefaultValue(true)]
		public bool AllowVectorFonts
		{
			get; set;
		}

		[DefaultValue(true)]
		public bool AllowVerticalFonts
		{
			get; set;
		}

		[DefaultValue(typeof(Color), "Black")]
		public Color Color
		{
			get; set;
		}

		[DefaultValue(false)]
		public bool FixedPitchOnly
		{
			get; set;
		}

		Font font;
		public Font Font
		{
			get
			{
				if (font == null)
					font = NSFont.SystemFontOfSize(NSFont.SystemFontSize).ToSDFont();
				return font;
			}
			set
			{
				font = value;
			}
		}

		[DefaultValue(false)]
		public bool FontMustExist
		{
			get; set;
		}

		[DefaultValue(0)]
		public int MaxSize
		{
			get; set;
		}

		[DefaultValue(0)]
		public int MinSize
		{
			get; set;
		}

		protected int Options
		{
			get; set;
		}

		[DefaultValue(false)]
		public bool ScriptsOnly
		{
			get; set;
		}

		[DefaultValue(false)]
		public bool ShowApply
		{
			get; set;
		}

		[DefaultValue(false)]
		public bool ShowColor
		{
			get; set;
		}

		[DefaultValue(true)]
		public bool ShowEffects
		{
			get; set;
		}

		[DefaultValue(false)]
		public bool ShowHelp
		{
			get; set;
		}

		public event EventHandler Apply
		{
			add { }
			remove { }
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			return IntPtr.Zero;
		}

		protected virtual void OnApply(EventArgs e)
		{
		}

		public override void Reset()
		{
		}

		protected override bool RunDialog(IntPtr hWndOwner)
		{
			var controller = new FontPanelController(this);

			if (DialogResult.Cancel == controller.RunModal())
				return false;

			try
			{
				var attributes = controller.SelectedAttributes;

				if (controller.SelectedFont != null)
					this.Font = controller.SelectedFont.ToSDFont(controller.SelectedAttributes);

				if (controller.SelectedForeColor != null)
					this.Color = controller.SelectedForeColor.ToSDColor();

				//if (controller.SelectedBackColor != null)
				//	this.BackColor = controller.SelectedBackColor.ToSDColor();

				return true;
			}
			catch
			{
			}

			return false;
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}

	internal static class AttributeKeys
	{
		public readonly static NSString NSStrikethrough = new NSString("NSStrikethrough");
		public readonly static NSString NSUnderline = new NSString("NSUnderline");
		public readonly static NSString NSColor = new NSString("NSColor");
	}

	internal class FontPanelController : NSWindowDelegate
	{
		NSFont font;
		NSFontPanel panel;
		FontDialog owner;

		public DialogResult Result = DialogResult.None;
		public NSFont SelectedFont;
		public NSColor SelectedForeColor;
		public NSColor SelectedBackColor;
		public NSMutableDictionary SelectedAttributes;

		public FontPanelController(FontDialog dialog)
		{
			owner = dialog;
			font = dialog.Font.ToNSFont();

			SelectedAttributes = new NSMutableDictionary();
		}

		public DialogResult RunModal()
		{
			panel = NSFontManager.SharedFontManager.FontPanel(true);
			var prevAccessory = panel.AccessoryView;
			var prevDelegate = panel.Delegate;

			var accessoryView = new ModalFontPanelAccessoryView();
			accessoryView.Initialize(panel, this);
			panel.AccessoryView = accessoryView;
			panel.Delegate = this;

			// Preset font:
			panel.SetPanelFont(owner.Font.ToNSFont(), false);

			//Preset color and other attributes:
			if (owner.Color != null)
			{
				SelectedForeColor = owner.Color.ToNSColor();
				SelectedAttributes[AttributeKeys.NSColor] = owner.Color.ToNSColor();
			}

			if (owner.Font != null)
			{
				if (owner.Font.Underline)
					SelectedAttributes[AttributeKeys.NSUnderline] = new NSNumber(true);
				if (owner.Font.Strikeout)
					SelectedAttributes[AttributeKeys.NSStrikethrough] = new NSNumber(true);
			}

			NSFontManager.SharedFontManager.SetSelectedAttributes(SelectedAttributes, false);

			NSApplication.SharedApplication.RunModalForWindow(panel);

			panel.Delegate = prevDelegate;
			panel.AccessoryView = prevAccessory;

			return Result;
		}

		public override void WillClose(NSNotification notification)
		{
			NSApplication.SharedApplication.StopModal();
		}

		[Export("changeFont:")]
		public void ChangeFont(NSObject sender)
		{
			SelectedFont = NSFontManager.SharedFontManager.ConvertFont(font);
			//NSFontManager.SharedFontManager.SetSelectedFont(SelectedFont);
			panel.SetPanelFont(SelectedFont, false);
		}

		[Export("validModesForFontPanel:")]
		NSFontPanelMode ValidModesForFontPanel(NSFontPanel fontPanel)
		{
			var mode = NSFontPanelMode.FaceMask | NSFontPanelMode.SizeMask | NSFontPanelMode.CollectionMask;

			if (owner.ShowColor)
				mode |= NSFontPanelMode.TextColorEffectMask;

			if (owner.ShowEffects)
			{
				mode |= NSFontPanelMode.StrikethroughEffectMask;
				mode |= NSFontPanelMode.UnderlineEffectMask;
			}

			return mode;
		}

		[Export("systemFont:")]
		public void SystemFontClicked(NSObject sender)
		{
			var size = NSFontManager.SharedFontManager.ConvertFont(font).PointSize;
			SelectedFont = NSFont.SystemFontOfSize(size);
			panel.SetPanelFont(SelectedFont, false);
		}

		[Export("choose:")]
		public void Choose(NSObject sender)
		{
			Result = DialogResult.OK;
			panel.Close();
		}

		[Export("cancel:")]
		public void Cancel(NSObject sender)
		{
			Result = DialogResult.Cancel;
			panel.Close();
		}

		[Export("setColor:forAttribute:")]
		public void SetColorForAttribute(NSColor color, NSString attribute)
		{
			if (AttributeKeys.NSColor.Equals(attribute))
				SelectedForeColor = color;

			//if (AttributeKeys.NSDocumentColor.Equals(attribute))
			//	SelectedBackColor = color;

			SelectedAttributes.SetValueForKey(color, attribute);
			NSFontManager.SharedFontManager.SetSelectedAttributes(SelectedAttributes, false);
		}

		[Export("changeAttributes:")]
		public void ChangeAttributes(NSObject sender)
		{
			var sel = new Selector("convertAttributes:");
			if (sender.RespondsToSelector(sel))
			{
				var ptr = MacApi.LibObjc.IntPtr_objc_msgSend_IntPtr(sender.Handle, sel.Handle, (SelectedAttributes ?? new NSDictionary()).Handle);
				if (ObjCRuntime.Runtime.GetNSObject(ptr) is NSDictionary attrs)
				{
					SelectedAttributes = (NSMutableDictionary)attrs.MutableCopy();
					NSFontManager.SharedFontManager.SetSelectedAttributes(attrs, false);
				}

			}
		}
	}

	internal class ModalFontPanelAccessoryView : NSView
	{
		NSBox line;
		NSButton cancelButton, chooseButton, systemFontButton;

		public virtual ModalFontPanelAccessoryView Initialize(NSFontPanel panel, NSObject target)
		{
			var width = panel.Frame.Size.Width;

			line = new NSBox();
			line.Title = String.Empty;
			line.BoxType = NSBoxType.NSBoxSeparator;
			line.BorderColor = NSColor.DarkGray;
			AddSubview(line);

			cancelButton = new NSButton();
			cancelButton.Title = Strings.CancelButtonTitle;
			cancelButton.Action = new Selector("cancel:");
			cancelButton.SetButtonType(NSButtonType.MomentaryPushIn);
			cancelButton.BezelStyle = NSBezelStyle.Rounded;
			cancelButton.Target = target;
			AddSubview(cancelButton);

			chooseButton = new NSButton();
			chooseButton.Title = Strings.ChooseButtonTitle;
			chooseButton.Action = new Selector("choose:");
			chooseButton.SetButtonType(NSButtonType.MomentaryPushIn);
			chooseButton.BezelStyle = NSBezelStyle.Rounded;
			chooseButton.Target = target;
			AddSubview(chooseButton);

			systemFontButton = new NSButton();
			systemFontButton.Title = Strings.SystemFontButtonTitle;
			systemFontButton.Action = new Selector("systemFont:");
			systemFontButton.SetButtonType(NSButtonType.MomentaryPushIn);
			systemFontButton.BezelStyle = NSBezelStyle.Rounded;
			systemFontButton.Target = target;
			AddSubview(systemFontButton);

			var padding = new CGSize(10, 10);
			chooseButton.SizeToFit();
			var height = chooseButton.Frame.Height + padding.Height + padding.Height;

			SetFrameSize(new CGSize(panel.Frame.Size.Width, height));
			return this;
		}

		public static ModalFontPanelAccessoryView Create(NSFontPanel panel, NSObject target)
		{
			return new ModalFontPanelAccessoryView().Initialize(panel, target);
		}

		public override void Layout()
		{
			base.Layout();

			var width = this.Bounds.Size.Width;

			var padding = new CGSize(10, 10);
			var p = new CGPoint(width - padding.Width, 0);

			cancelButton.SizeToFit();
			p = p.Move(-cancelButton.Frame.Size.Width, padding.Height);
			cancelButton.SetFrameOrigin(p);

			chooseButton.SizeToFit();
			p = p.Move(-chooseButton.Frame.Size.Width, 0);
			chooseButton.SetFrameOrigin(p);

			p.X = padding.Width;
			systemFontButton.SizeToFit();
			systemFontButton.SetFrameOrigin(p);

			line.Frame = new CGRect(0, Bounds.Size.Height - 1, width, 1);
		}
	}

	static class FontDialogExtensions
	{
		public static Font ToSDFont(this NSFont font, NSDictionary attributes = null)
		{
			var descriptor = font.FontDescriptor;
			var traits = (NSFontTraitMask)descriptor.SymbolicTraits;
			var style = (FontStyle)0;
			if (0 != (traits & NSFontTraitMask.Bold)) style |= FontStyle.Bold;
			if (0 != (traits & NSFontTraitMask.Italic)) style |= FontStyle.Italic;

			if (attributes != null)
			{
				if (attributes.ContainsKey(AttributeKeys.NSStrikethrough))
					style |= FontStyle.Strikeout;
				if (attributes.ContainsKey(AttributeKeys.NSUnderline))
					style |= FontStyle.Underline;
			}

			var size = (float)font.PointSize;
			var unit = GraphicsUnit.Pixel;
			return new Font(font.FamilyName, size, style, unit);
		}
	}
}
#endif

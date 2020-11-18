#if MACOS_THEME
using System;
using System.Drawing;
using System.Drawing.Mac;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#elif XAMARINMAC
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
#endif

namespace System.Windows.Forms
{
	public abstract partial class TextBoxBase
	{
		internal class TextBoxBase_NSSecureTextField : TextBoxBase_NSTextField
		{
			public TextBoxBase_NSSecureTextField(TextBoxBase owner) : base(owner)
			{
			}

			public override NSView CreateView()
			{
				var text = owner.Text;
				var size = owner.Bounds.Size;
				textField = new NSSecureTextField(new CGRect(0, 0, size.Width, size.Height));

				textField.Changed += TextFieldChanged;
				textField.DoCommandBySelector = DoCommandBySelector;

				ApplyBorderStyle(owner.BorderStyle);
				ApplyForeColor(owner.ForeColor, owner.forecolor_set);
				ApplyBackColor(owner.BackColor, owner.backcolor_set);
				ApplyAlignment(owner.alignment);
				ApplyFont(owner.Font);
				ApplyScrollbars(owner.scrollbars);
				ApplyReadOnly(owner.read_only);
				ApplyEnabled(owner.is_enabled);
				ApplyText(text);

				return textField;
			}

			public override void Copy()
			{
			}

			public override void Cut()
			{
			}
		}
	}
}

#endif //MACOS_THEME

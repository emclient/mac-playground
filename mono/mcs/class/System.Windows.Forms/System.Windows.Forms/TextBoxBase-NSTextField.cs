#if MACOS_THEME
using System;
using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;
using System.Reflection;

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
		internal class TextBoxBase_NSTextField : ITextBoxBaseImp
		{
			protected TextBoxBase owner;
			protected NSTextField textField;
			protected NSTextFieldDelegate textFieldDelegate;
			protected string text;

			public TextBoxBase_NSTextField(TextBoxBase owner)
			{
				this.owner = owner;
			}

			public virtual NSView CreateView()
			{
				var text = owner.Text;
				var size = owner.Bounds.Size;
				textField = new NSTextField(new CGRect(0, 0, size.Width, size.Height));
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

			public virtual string Rtf { get { return null; } set { } }

			public virtual string Text
			{
				get { return textField != null ? textField.StringValue : this.text; }
				set
				{
					if (textField != null)
						ApplyText(value);
					else
						this.text = value;
				}
			}

			public virtual void ApplyReadOnly(bool value)
			{
				if (textField != null)
					textField.Editable = !value;
			}

			public virtual void ApplyEnabled(bool value)
			{
				if (textField != null)
					textField.Enabled = value;
			}

			public Size IntrinsicContentSize
			{
				get { return textField?.IntrinsicContentSize.ToSDSize() ?? new Size(100, 20); }
			}

			public virtual void AppendText(string value)
			{
				if (textField == null)
					text = text == null ? value : text + value ?? String.Empty;
				else
					textField.StringValue = textField.StringValue == null ? value : textField.StringValue + value ?? String.Empty;
			}

			public virtual void ApplyFont(Font font)
			{
				if (textField != null)
					textField.Font = font.ToNSFont();
			}

			public virtual void ApplyBorderStyle(BorderStyle value)
			{
				if (textField == null)
					return;

				switch (value)
				{
					case BorderStyle.None:
						textField.Bezeled = false;
						break;
					case BorderStyle.FixedSingle:
					case BorderStyle.Fixed3D:
						textField.BezelStyle = NSTextFieldBezelStyle.Square;
						textField.Bezeled = true;
						break;
				}
			}

			public virtual void ApplyForeColor(Color value, bool apply)
			{
				if (textField != null && apply)
					textField.TextColor = value.ToNSColor();
			}

			public virtual void ApplyBackColor(Color value, bool apply)
			{
				if (textField != null && apply)
					textField.BackgroundColor = value.ToNSColor();
			}

			public void ApplyAlignment(HorizontalAlignment value)
			{
				if (textField != null)
					textField.Alignment = value.ToNSTextAlignment();
			}

			public virtual void ApplyScrollbars(RichTextBoxScrollBars scrollbars)
			{
				// No scrollbars in this implementation
			}

			public virtual void SelectAll()
			{
				if (textField != null)
					textField.SelectText(textField);
			}

			public virtual void SelectAllNoScroll()
			{
				// FIXME
				if (textField != null)
					textField.SelectText(textField);
			}

			public virtual void Copy()
			{
				if (textField != null && !(textField is NSSecureTextField))
					textField.SendAction(new Selector("copy:"), textField);
			}

			public virtual void Cut()
			{
				if (textField != null && !(textField is NSSecureTextField))
					textField.SendAction(new Selector("cut:"), textField);
			}

			public virtual void Paste()
			{
				if (textField != null)
					textField.SendAction(new Selector("paste:"), textField);
			}

			public virtual bool CanUndo()
			{
				Control.NotImplemented(MethodBase.GetCurrentMethod(), text);
				return textField != null; // FIXME
			}

			public virtual bool CanRedo()
			{
				Control.NotImplemented(MethodBase.GetCurrentMethod(), text);
				return textField != null; // FIXME
			}

			public virtual bool Undo()
			{
				// FIXME
				Control.NotImplemented(MethodBase.GetCurrentMethod(), text);
				return false;
			}

			public virtual bool Redo()
			{
				// FIXME
				Control.NotImplemented(MethodBase.GetCurrentMethod(), text);
				return false;
			}

			internal virtual void ApplyText(string value)
			{
				if (textField != null && value != null)
					textField.StringValue = value;
			}

			protected virtual void TextFieldChanged(object sender, EventArgs e)
			{
				owner.OnTextUpdate();
				owner.OnTextChanged(e);
			}

			#region NSTextFieldDelegate

			internal virtual bool DoCommandBySelector(NSControl control, NSTextView textView, Selector selector)
			{
				switch (selector.Name)
				{
					case "insertTab:":
					case "insertBacktab:":
						textField.SendWmKey(VirtualKeys.VK_TAB, IntPtr.Zero);
						return true;
					case "insertNewline:":
						textField.SendWmKey(VirtualKeys.VK_RETURN, IntPtr.Zero);
						return true;
					case "cancelOperation:":
						textField.SendWmKey(VirtualKeys.VK_ESCAPE, IntPtr.Zero);
						return true;
				}
				return false;
			}

			#endregion //NSTextFieldDelegate
		}
	}
}

#endif //MACOS_THEME

#if MACOS_THEME
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Mac;
using System.Linq;
using System.Reflection;
using System.Windows.Forms.Mac;

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
			protected Formatter formatter;

			public TextBoxBase_NSTextField(TextBoxBase owner)
			{
				this.owner = owner;
			}

			public virtual NSView CreateView()
			{
				text = owner.Text;

				var size = owner.Bounds.Size;
				textField = new NSTextField(new CGRect(0, 0, size.Width, size.Height));
				textField.TextShouldBeginEditing = TextFieldShouldBeginEditing;
				textField.Changed += TextFieldChanged;
				textField.DoCommandBySelector = DoCommandBySelector;
				textField.GetCompletions = TextFieldGetCompletions;
				textField.Formatter = formatter = new Formatter(this);

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
				get { return text; }
				set
				{
					this.text = value;
					if (textField != null)
						ApplyText(value);
				}
			}

			public virtual void ApplyReadOnly(bool value)
			{
				// Now handled by TextFieldShouldBeginEditing(), which let's the user to select, copy and lookup content.

				//if (textField != null)
					//textField.Editable = !value;
			}

			public virtual void ApplyEnabled(bool value)
			{
				if (textField != null)
					textField.Enabled = value;
			}

			public virtual Size IntrinsicContentSize
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

			public virtual void ApplyAlignment(HorizontalAlignment value)
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
				if (textField.Window.FirstResponder is NSTextView textView && textField.Contains(textView))
					NSApplication.SharedApplication.SendAction(new Selector("undo:"), textView, NSApplication.SharedApplication);
				return true;
			}

			public virtual bool Redo()
			{
				if (textField.Window.FirstResponder is NSTextView textView && textField.Contains(textView))
					NSApplication.SharedApplication.SendAction(new Selector("redo:"), textView, NSApplication.SharedApplication);
				return true;
			}

			internal virtual void ApplyText(string value)
			{
				if (textField != null && value != null)
					textField.StringValue = value;
			}

			internal virtual void ApplyFormatter(NSFormatter formatter)
			{
				if (textField != null && formatter != null)
					textField.Formatter = formatter;
			}

			bool completing = false;
			protected virtual void TextFieldChanged(object sender, EventArgs e)
			{
				this.text = textField.StringValue;

				owner.OnTextUpdate();
				owner.OnTextChanged(e);

				Complete();
			}

			internal virtual void Complete()
			{
				if (!completing 
				    && owner.auto_complete_mode != AutoCompleteMode.None && owner.auto_complete_source == AutoCompleteSource.CustomSource && owner.auto_complete_custom_source != null
					&& textField.StringValue.Length > 0 && owner.auto_complete_custom_source.Count != 0
				    && textField.Window.FirstResponder is NSTextView textView)
				{
						completing = true;
						textView.Complete(textField);
						completing = false;
				}
			}

			internal virtual string FormatterPreprocessText(string value)
			{
				if (owner.preprocessText!= null)
					return owner.preprocessText(value, this.text);
				return value;
			}

			#region NSTextFieldDelegate

			internal virtual bool DoCommandBySelector(NSControl control, NSTextView textView, Selector selector)
			{
				switch (selector.Name)
				{
					case "insertTab:":
					case "insertBacktab:":
						SendWmKey(VirtualKeys.VK_TAB, IntPtr.Zero);
						return true;
					case "insertNewline:":
						SendWmKey(VirtualKeys.VK_RETURN, IntPtr.Zero);
						return true;
					case "cancelOperation:":
						SendWmKey(VirtualKeys.VK_ESCAPE, IntPtr.Zero);
						return true;
				}
				return false;
			}

			internal virtual bool TextFieldShouldBeginEditing(NSControl control, NSText fieldEditor)
			{
				return !owner.ReadOnly && owner.Enabled;
			}

			internal string[] TextFieldGetCompletions(NSControl control, NSTextView textView, string[] words, NSRange charRange, ref nint index)
			{
				var prefix = textView.String?.Substring(0, (int)charRange.Location + (int)charRange.Length) ?? String.Empty;

				var completions = new List<string>();
				foreach(string s in owner.auto_complete_custom_source)
					if (s.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
						completions.Add(s.Substring((int)charRange.Location));

				index = -1;
				return completions.Distinct().OrderBy(x => x).ToArray();
			}

			internal virtual void SendWmKey(VirtualKeys key, IntPtr lParam)
			{
				XplatUI.SendMessage(owner.Handle, Msg.WM_KEYDOWN, (IntPtr)key, lParam);
				XplatUI.SendMessage(owner.Handle, Msg.WM_KEYUP, (IntPtr)key, lParam);
			}

			#endregion //NSTextFieldDelegate
		}

		internal class Formatter : NSFormatter
		{
			TextBoxBase_NSTextField owner;

			public Formatter(TextBoxBase_NSTextField owner)
			{
				this.owner = owner;
			}

			public override string StringFor(NSObject value)
			{
				return owner.FormatterPreprocessText(value?.ToString() ?? null);
			}

			public override bool GetObjectValue(out NSObject obj, string str, out NSString error)
			{
				try
				{
					obj = (NSString)owner.FormatterPreprocessText(str ?? String.Empty);
					error = null;
					return true;
				}
				catch (Exception e)
				{
					obj = (NSString)string.Empty;
					error = (NSString)e.Message;	
					return false;
				}
			}
		}
	}
}

#endif //MACOS_THEME

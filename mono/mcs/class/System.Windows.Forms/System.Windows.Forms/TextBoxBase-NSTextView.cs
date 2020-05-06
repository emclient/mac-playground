#if MACOS_THEME
using System;
using System.Windows.Forms.Mac;
using System.Drawing.Mac;
using System.Drawing;

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
		internal class TextBoxBase_NSTextView : ITextBoxBaseImp
		{
			private const string NSForegroundColorAttributeName = "NSColor";
			private const string NSBackgroundColorAttributeName = "NSBackgroundColor";

			TextBoxBase owner;
			NSScrollView scrollView;
			NSTextView textView;

			string initialText;

			public TextBoxBase_NSTextView(TextBoxBase owner)
			{
				this.owner = owner;
			}

			public virtual NSView CreateView()
			{
				var text = owner.Text;
				scrollView = new NSScrollView(owner.Bounds.ToCGRect());

				var contentSize = scrollView.ContentSize;
				textView = new NSTextView(new CGRect(0, 0, contentSize.Width, contentSize.Height));
				//textView.MinSize = new CGSize(MinimumSize.Width, MinimumSize.Height);
				//textView.MaxSize = new CGSize(MaximumSize.Width, MaximumSize.Height);
				textView.VerticallyResizable = true;
				textView.HorizontallyResizable = true;
				textView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
				textView.TextContainer.ContainerSize = new CGSize(contentSize.Width, float.MaxValue);
				textView.TextContainer.WidthTracksTextView = true;
				textView.RichText = owner.richtext;
				textView.ShouldUpdateTouchBarItemIdentifiers = TextViewUpdateTouchBarItemIdentifiers;

				if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(10,12,1)))
					textView.AutomaticTextCompletionEnabled = true;
				
				textView.LinkClicked = TextViewLinkClicked;

				textView.TextDidChange += TextViewTextDidChange;
				textView.DoCommandBySelector = TextViewDoCommandBySelector;
				textView.AllowsUndo(true);

				scrollView.DocumentView = textView;

				ApplyBorderStyle(owner.BorderStyle);
				ApplyForeColor(owner.ForeColor, owner.forecolor_set);
				ApplyBackColor(owner.BackColor, owner.backcolor_set);
				ApplyAlignment(owner.alignment);
				ApplyFont(owner.Font);
				ApplyScrollbars(owner.scrollbars);
				ApplyReadOnly(owner.read_only);
				ApplyEnabled(owner.Enabled);
				ApplyText(text);

				return scrollView;
			}

			public virtual void Release()
			{
				if (textView != null)
				{
					textView.LinkClicked = null;
					textView.TextDidChange -= TextViewTextDidChange;
					textView.DoCommandBySelector = null;
					textView = null;
					scrollView = null;
					owner = null;
				}
			}

			public virtual void AppendText(string text)
			{
				if (textView == null)
					initialText = initialText == null ? text : initialText + text;
				else
					textView.TextStorage.Append(new NSAttributedString(text));
			}

			public virtual string Rtf {
				get
				{
					var str = textView?.AttributedString;
					if (str != null)
					{
						var options = new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.RTF, StringEncoding = NSStringEncoding.UTF8 };
						var data = str.GetData(new NSRange(0, str.Length), options, out NSError error);
						if (error == null && data != null)
							return data.ToString(NSStringEncoding.UTF8);
					}
					return null;
				}

				set
				{
					if (textView != null)
					{
						var data = NSData.FromString(value);
						var options = new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.RTF, StringEncoding= NSStringEncoding.UTF8 };
						var astr = new NSAttributedString(data, options, out NSDictionary attributes, out NSError error);
						textView.TextStorage.SetString(astr);
					}
				} 
			}

			public virtual string Text
			{
				get { return textView != null ? textView.GetString() : initialText ?? String.Empty; }
				set
				{
					if (textView != null)
						ApplyText(value);
					else
						initialText = value;
				}
			}

			public virtual Size IntrinsicContentSize
			{
				get { return textView?.IntrinsicContentSize.ToSDSize() ?? new Size(100, 20); }
			}

			public virtual void ApplyReadOnly(bool value)
			{
				if (textView != null)
					textView.Editable = !value && owner.Enabled;
			}

			public virtual void ApplyEnabled(bool value)
			{
				if (textView != null)
				{
					textView.Selectable = value;
					textView.Editable = value && !owner.ReadOnly;
					textView.TextColor = value ? NSColor.ControlText : NSColor.DisabledControlText;
				}
			}

			public virtual void ApplyFont(Font value)
			{
				if (textView != null)
					textView.Font = value.ToNSFont();
			}

			public virtual void ApplyBorderStyle(BorderStyle value)
			{
				if (scrollView != null)
					scrollView.BorderType = value.ToNSBorderType();
			}

			public virtual void ApplyForeColor(Color value, bool apply)
			{
				if (textView != null && apply)
					textView.TextColor = value.ToNSColor();
			}

			public virtual void ApplyBackColor(Color value, bool apply)
			{
				if (textView != null && apply)
					textView.BackgroundColor = value.ToNSColor();
			}

			public virtual void ApplyAlignment(HorizontalAlignment value)
			{
				if (textView != null)
					textView.Alignment = value.ToNSTextAlignment();
			}

			public virtual void ApplyScrollbars(RichTextBoxScrollBars scrollbars)
			{
				if (scrollView == null)
					return;
					
				switch (scrollbars)
				{
					case RichTextBoxScrollBars.None:
						scrollView.HasVerticalScroller = false;
						scrollView.HasHorizontalScroller = false;
						scrollView.AutohidesScrollers = true;
						break;
					case RichTextBoxScrollBars.Vertical:
						scrollView.HasVerticalScroller = true;
						scrollView.HasHorizontalScroller = false;
						scrollView.AutohidesScrollers = true;
						break;
					case RichTextBoxScrollBars.Horizontal:
						scrollView.HasVerticalScroller = false;
						scrollView.HasHorizontalScroller = true;
						scrollView.AutohidesScrollers = true;
						break;
					case RichTextBoxScrollBars.Both:
						scrollView.HasVerticalScroller = true;
						scrollView.HasHorizontalScroller = true;
						scrollView.AutohidesScrollers = true;
						break;
					case RichTextBoxScrollBars.ForcedVertical:
						scrollView.HasVerticalScroller = true;
						scrollView.HasHorizontalScroller = false;
						scrollView.AutohidesScrollers = false;
						break;
					case RichTextBoxScrollBars.ForcedHorizontal:
						scrollView.HasVerticalScroller = false;
						scrollView.HasHorizontalScroller = true;
						scrollView.AutohidesScrollers = false;
						break;
					case RichTextBoxScrollBars.ForcedBoth:
						scrollView.HasVerticalScroller = true;
						scrollView.HasHorizontalScroller = true;
						scrollView.AutohidesScrollers = false;
						break;
				}
			}

			public string SelectedText
			{
				get
				{
					return Text.Substring(SelectionStart, SelectionLength);
				}
				set
				{
					if (textView == null)
						return;

					NSRange selectedRange = textView.SelectedRange;
					if (selectedRange.Length > 0)
						textView.TextStorage.DeleteRange(selectedRange);

					textView.TextStorage.Insert(new NSAttributedString(value), selectedRange.Location);
					Select((int)selectedRange.Location, value.Length);
				}
			}

			public Color SelectionColor
			{
				get
				{
					if (textView == null)
						return owner.ForeColor;

					NSDictionary attributes = textView.SelectedTextAttributes;
					foreach (NSObject key in attributes.Keys)
					{
						if (key.ToString() == NSForegroundColorAttributeName)
						{
							return ((NSColor)attributes[key]).ToSDColor();
						}
					}
					return textView.TextColor.ToSDColor();
				}
				set
				{
					if (textView == null)
						return;

					NSColor nsColor = NSColor.FromRgba(value.R, value.G, value.B, value.A);
					textView.SetTextColor(nsColor, textView.SelectedRange);
				}
			}

			public virtual int SelectionStart
			{
				get
				{
					if (textView != null)
						return (int)textView.SelectedRange.Location;

					return 0;
				}
			}

			public virtual int SelectionLength
			{
				get
				{
					if (textView != null)
						return (int)textView.SelectedRange.Length;

					return 0;
				}
			}

			public virtual void Select(int start, int length)
			{
				if (textView != null)
				{
					NSRange range;
					range.Location = start;
					range.Length = length;
					textView.SelectedRange = range;
				}
			}

			public virtual void SelectAll()
			{
				if (textView != null)
					textView.SelectAll(textView);
			}

			public virtual void SelectAllNoScroll()
			{
				// FIXME
				if (textView != null)
					textView.SelectAll(textView);
			}

			public virtual void Copy()
			{
				if (textView != null)
					textView.Copy(textView);
			}

			public virtual void Cut()
			{
				if (textView != null)
					textView.Cut(textView);
			}

			public virtual void Paste()
			{
				if (textView != null)
					textView.Paste(textView);
			}

			public virtual bool CanUndo()
			{
				return textView != null && textView.AllowsUndo;
			}

			public virtual bool CanRedo()
			{
				return textView != null && textView.AllowsUndo;
			}

			public virtual bool Undo()
			{
				if (textView != null && textView.Window.FirstResponder == textView)
					NSApplication.SharedApplication.SendAction(new Selector("undo:"), textView, NSApplication.SharedApplication);
				return true;
			}

			public virtual bool Redo()
			{
				if (textView != null && textView.Window.FirstResponder == textView)
					NSApplication.SharedApplication.SendAction(new Selector("redo:"), textView, NSApplication.SharedApplication);
				return true;
			}

			// Delegate ------------

			internal virtual void TextViewTextDidChange(object sender, EventArgs e)
			{
				owner.OnTextUpdate();
				owner.OnTextChanged(EventArgs.Empty);
			}

			internal virtual bool TextViewDoCommandBySelector(NSTextView textView, Selector commandSelector)
			{
				switch (commandSelector.Name)
				{
					case "insertTab:":
						if (owner.accepts_tab)
							return false;
						SendWmKey(VirtualKeys.VK_TAB, IntPtr.Zero);
						return true;
					case "insertNewline:":
						if (owner.Multiline && owner.accepts_return)
							return false;
						SendWmKey(VirtualKeys.VK_RETURN, IntPtr.Zero);
						return true;
					case "insertBacktab:":
						SendWmKey(VirtualKeys.VK_TAB, IntPtr.Zero);
						return true;
					case "cancelOperation:":
						SendWmKey(VirtualKeys.VK_ESCAPE, IntPtr.Zero);
						return true;
				}
				return false;
			}

			// This prevents crash caused by the default xamarin's delegate that returns null.
			internal string[] TextViewUpdateTouchBarItemIdentifiers(NSTextView textView, string[] identifiers)
			{
				return identifiers;
			}

			internal virtual bool TextViewLinkClicked(NSTextView textView, NSObject link, nuint charIndex)
			{
				owner.HandleLinkClicked(textView, link, charIndex);
				return true;
			}

			// Internals -----------

			internal virtual void SendWmKey(VirtualKeys key, IntPtr lParam)
			{
				XplatUI.SendMessage(owner.Handle, Msg.WM_KEYDOWN, (IntPtr)key, lParam);

				if (owner.IsHandleCreated) // The previous line could have caused disposing the control (esc, enter, ...)
					XplatUI.SendMessage(owner.Handle, Msg.WM_KEYUP, (IntPtr)key, lParam);
			}

			internal virtual void ApplyText(string value)
			{
				if (textView != null && value != null)
					textView.SetString((NSString)value);
			}
		}
	}
}
#endif //MACOS_THEME

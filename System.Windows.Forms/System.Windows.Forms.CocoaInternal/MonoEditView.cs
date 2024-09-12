using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms.Mac;
using Foundation;
using AppKit;
using CoreGraphics;
using ObjCRuntime;

namespace System.Windows.Forms.CocoaInternal
{
	//[ExportClass("MonoEditView", "MonoView")]
	class MonoEditView : MonoView, INSTextInputClient
	{
		internal NSAttributedString markedText = new NSMutableAttributedString();
		internal string insertText = String.Empty;
		internal bool isComposing;

		public MonoEditView(NativeHandle instance) : base(instance)
		{
		}

		public MonoEditView(XplatUICocoa driver, CGRect frameRect, WindowStyles style, WindowExStyles exStyle) : base(driver, frameRect, style, exStyle)
		{
		}

		#region INSTextInputClient

		public bool HasMarkedText
		{
			// Until this returns false, interpretKeyEvents continues to call insertText !
			get
			{
				var result = markedText.Length != 0;
				//Debug.WriteLine($"hasMarkedText -> {result}");
				return result;
			}
		}

		public NSRange MarkedRange
		{
			get
			{
				GetSelection(out var start, out var _);
				start = Math.Max(0, start - (int)markedText.Length);
				var result = new NSRange(start, markedText.Length);
				//Debug.WriteLine($"markedRange -> {result}");
				return result;
			}
		}

		public NSRange SelectedRange
		{
			get
			{
				GetSelection(out var start, out var end);
				var result = new NSRange(start, Math.Max(0, end - start));
				//Debug.WriteLine($"selectedRange -> {result}");
				return result;
			}
			set 
			{
				SetSelection((int)value.Location, (int)(value.Location + value.Length));
			}
		}

		public NSString[] ValidAttributesForMarkedText
		{
			get { return validAttributesForMarkedText; }
		}

		public NSWindowLevel WindowLevel
		{
			get { return Window?.Level ?? NSWindowLevel.Normal; }
		}

		public NSAttributedString GetAttributedSubstring(NSRange proposedRange, out NSRange actualRange)
		{
			var text = GetText();

			NSAttributedString result;
			if (proposedRange.Location >= text.Length)
			{
				actualRange = new NSRange(text.Length, 0);
				result = null;
			}
			else
			{
				var to = Math.Min(proposedRange.Location + proposedRange.Length, text.Length);
				actualRange = new NSRange(proposedRange.Location, (nint)to - proposedRange.Location);
				result = new NSAttributedString(text.Substring((int)actualRange.Location, (int)actualRange.Length));
			}

			//Debug.WriteLine($"attributedSubstringForProposedRange:{proposedRange}, actualRange:{actualRange} -> '{result}'");
			return result;
		}

		public nuint GetCharacterIndex(CGPoint point)
		{
			this.NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public CGRect GetFirstRect(NSRange characterRange, out NSRange actualRange)
		{
			actualRange = new NSRange();

			// SWF textboxes
			if (XplatUICocoa.CaretView != null && XplatUICocoa.CaretView.Superview == this)
				return Window.ConvertRectToScreen(XplatUICocoa.CaretView.ConvertRectToView(Bounds, null));

			// EMC textboxes - teporary solution
			var first = new CGRect(Bounds.Location, CGSize.Empty);
			var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(XplatUIWin32.RECT)));
			var result = driver.SendMessage(Handle, Msg.WM_IME_GETCURRENTPOSITION, IntPtr.Zero, ptr);
			if (result == (IntPtr)1 && Marshal.PtrToStructure<XplatUIWin32.RECT>(ptr) is XplatUIWin32.RECT r)
				first = new CGRect(first.Location.X + r.left, first.Location.Y + r.top, r.right - r.left, r.bottom - r.top);
			Marshal.FreeHGlobal(ptr);
			return Window.ConvertRectToScreen(ConvertRectToView(first, null));
		}

		public void InsertText(NSObject text, NSRange replacementRange)
		{
			//Console.WriteLine($"insertText:{text}, replacementRange:{replacementRange}, isComposing:{isComposing}");

			if (isComposing)
			{
				isComposing = false;
				eventResponder.OnImeCompositionEnd();

				// Commit IME session
				insertText = text.ToString();
				markedText = new NSAttributedString();
				var lParam = new IntPtr((int)(GCS.GCS_RESULTSTR | GCS.GCS_COMPATTR | GCS.GCS_COMPCLAUSE | GCS.GCS_CURSORPOS));
				Application.SendMessage(Handle, Msg.WM_IME_COMPOSITION, IntPtr.Zero, lParam);
				Application.SendMessage(Handle, Msg.WM_IME_ENDCOMPOSITION, IntPtr.Zero, lParam);

				// There's probably no notification in macOS text input system about closing the candidate window, so here we emulate it. lParam is not set, however.
				Application.SendMessage(Handle, Msg.WM_IME_NOTIFY, new IntPtr((int)Imn.IMN_CLOSECANDIDATE), IntPtr.Zero);
			}
			else
			{
				eventResponder.InsertText(text, replacementRange);
			}
		}

		public void SetMarkedText(NSObject text, NSRange selectedRange, NSRange replacementRange)
		{
			//Console.WriteLine($"setMarkedText:{text}, selectedRange:{selectedRange}, replacementRange:{replacementRange}");

			var value = text is NSAttributedString str ? str : new NSAttributedString(text.ToString());

			// Start session
			if (markedText.Length == 0 && value.Length != 0)
			{
				eventResponder.OnImeCompositionBegin();

				// There's probably no notification in macOS text input system about opening the candidate window, so here we emulate it. lParam is not set, however.
				Application.SendMessage(Handle, Msg.WM_IME_NOTIFY, new IntPtr((int)Imn.IMN_OPENCANDIDATE), IntPtr.Zero);

				Application.SendMessage(Handle, Msg.WM_IME_STARTCOMPOSITION, IntPtr.Zero, IntPtr.Zero);
				insertText = string.Empty;
				isComposing = true;
			}
			else
			{
				eventResponder.OnImeCompositionContinue();
			}

			markedText = value;

			// Continue session
			var lParam = new IntPtr((int)(GCS.GCS_COMPSTR | GCS.GCS_COMPATTR | GCS.GCS_COMPCLAUSE | GCS.GCS_CURSORPOS));
			if (markedText.Length != 0)
				Application.SendMessage(Handle, Msg.WM_IME_COMPOSITION, IntPtr.Zero, lParam);
		}

		public void UnmarkText()
		{
			markedText = new NSMutableAttributedString();
		}

		static readonly NSString[] validAttributesForMarkedText = new NSString[] {
			(NSString)"NSFont",
			(NSString)"NSUnderline",
			(NSString)"NSColor",
			(NSString)"NSBackgroundColor",
			(NSString)"NSUnderlineColor",
		};

		#endregion INSTextInputView

		unsafe public virtual string GetText()
		{
			var length = Math.Max(0, (int)Application.SendMessage(Handle, Msg.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero));
			var buffer = new char[length + 1];
			fixed (char* ptr = &buffer[0])
			{
				int n = (int)Application.SendMessage(Handle, Msg.WM_GETTEXT, (IntPtr)buffer.Length, new IntPtr(ptr));
				return new string(buffer, 0, Math.Min(n, length));
			}
		}

		unsafe public virtual void SetText(string value)
		{
			this.NotImplemented(MethodBase.GetCurrentMethod());
		}

		unsafe public void GetSelection(out int from, out int to)
		{
			var range = new int[] { 0, 0 };
			fixed (int* start = &range[0], end = &range[1])
				Application.SendMessage(Handle, Msg.EM_GETSEL, new IntPtr(start), new IntPtr(end));
			
			if (range[0] < range[1])
			{
				from = range[0];
				to = range[1];
			}
			else
			{
				from = range[1];
				to = range[0];
			}
		}

		unsafe public void SetSelection(int from, int to)
		{
			var range = new int[] { Math.Min(from, to), Math.Max(from, to) };
			fixed (int* start = &range[0], end = &range[1])
				Application.SendMessage(Handle, Msg.EM_SETSEL, new IntPtr(start), new IntPtr(end));
		}

		// Accessibility

		public override string AccessibilityRole => "AXTextArea"; // Using Area instead of Field makes grammarly to react for some reason

		public override bool IsAccessibilitySelectorAllowed(Selector selector)
		{
			switch (selector.Name)
			{
				case "accessibilityValue": return true;
			}

			return base.IsAccessibilitySelectorAllowed(selector);
		}

		public override bool AccessibilityEnabled => Control.FromHandle(Handle)?.Enabled ?? false;
		
		[Export("accessibilityIsValueAttributeSettable")]
		public virtual bool AccessibilityIsValueAttributeSettable() => true;

		public override NSObject AccessibilityValue
		{
			get => new NSString(GetText());
			set => SetText(value?.ToString() ?? string.Empty);
		}

		[Export("accessibilitySetValue:forAttribute:")]
		public virtual void AccessibilitySetValue(NSObject value, NSString attribute)
		{
			this.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public override nint AccessibilityNumberOfCharacters => GetText().Length;

		public override string AccessibilitySelectedText
		{
			get
			{
				var range = SelectedRange;
				var text = GetText();
				return text.Substring((int)range.Location, (int)range.Length);
			}
			set
			{
				this.NotImplemented(MethodBase.GetCurrentMethod());
			}
		}

		public virtual bool AccessibilityIsSelectedTextRangeAttributeSettable
		{
			[Export("accessibilityIsSelectedTextRangeAttributeSettable")]
			get => true;
		}

		[Export("accessibilitySetSelectedTextRangeAttribute:")]
		public virtual void AccessibilitySetSelectedTextRangeAttribute(NSRange value)
		{
			AccessibilitySelectedTextRange = value;
		}

		public override NSRange AccessibilitySelectedTextRange
		{
			get => SelectedRange;
			set => SelectedRange = value;
		}

		public override nint AccessibilityInsertionPointLineNumber
		{
			get
			{
				this.NotImplemented(MethodBase.GetCurrentMethod());
				return 0;
			}
			set
			{
				this.NotImplemented(MethodBase.GetCurrentMethod());
			}
		}

		[Export("accessibilityFrameForRange:")]
		public virtual CGRect AccessibilityFrameForRange(NSRange range)
		{
			// TODO: Fix for multiline text box and for longer texts that don't fit the box
			var p = GetCharacterPosition((int)range.Location);
			var q = GetCharacterPosition((int)range.Location + (int)range.Length);
			var rect = new CGRect(p.X, p.Y, q.X - p.X, GetLineHeight());
			//Console.WriteLine($"AccessibilityFrameForRange({range})->{rect}");
			return rect;
		}

		unsafe CGPoint GetCharacterPosition(int index)
		{
			var pt = new POINT[1];
			fixed (POINT* ptr = &pt[0])
				Application.SendMessage(Handle, Msg.EM_POSFROMCHAR, new IntPtr(ptr), new IntPtr(index));

			var frame = AccessibilityFrame;
			return new CGPoint(frame.Location.X + pt[0].x, frame.Location.Y + pt[0].y);
		}

		int GetLineHeight(int defaultValue = 0)
		{
			if (Control.FromHandle(Handle) is Control c)
				return c.Font.Height;
			return defaultValue;
		}
	}
}

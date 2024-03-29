﻿using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
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
			//Debug.WriteLine($"characterIndexForPoint:{point}");
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
			//Debug.WriteLine($"insertText:{text}, replacementRange:{replacementRange}");

			if (markedText.Length != 0)
			{
				// Commit IME session
				insertText = text.ToString();
				markedText = new NSAttributedString();
				var lParam = new IntPtr((int)(GCS.GCS_RESULTSTR | GCS.GCS_COMPATTR | GCS.GCS_COMPCLAUSE | GCS.GCS_CURSORPOS));
				Application.SendMessage(Handle, Msg.WM_IME_COMPOSITION, IntPtr.Zero, lParam);
				Application.SendMessage(Handle, Msg.WM_IME_ENDCOMPOSITION, IntPtr.Zero, lParam);
			}
			else
			{
				eventResponder.InsertText(text, replacementRange);
			}
		}

		public void SetMarkedText(NSObject text, NSRange selectedRange, NSRange replacementRange)
		{
			//Debug.WriteLine($"setMarkedText:{text}, selectedRange:{selectedRange}, replacementRange:{replacementRange}");

			var value = text is NSAttributedString str ? str : new NSAttributedString(text.ToString());

			// Start session
			if (markedText.Length == 0 && value.Length != 0)
			{
				Application.SendMessage(Handle, Msg.WM_IME_STARTCOMPOSITION, IntPtr.Zero, IntPtr.Zero);
				insertText = string.Empty;
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

		unsafe public string GetText()
		{
			var length = Math.Max(0, (int)Application.SendMessage(Handle, Msg.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero));
			var buffer = new char[length + 1];
			fixed (char* ptr = &buffer[0])
			{
				int n = (int)Application.SendMessage(Handle, Msg.WM_GETTEXT, (IntPtr)buffer.Length, new IntPtr(ptr));
				return new string(buffer, 0, Math.Min(n, length));
			}
		}

		unsafe public void GetSelection(out int from, out int to)
		{
			var range = new int[] { 0, 0 };
			fixed (int* start = &range[0], end = &range[1])
				Application.SendMessage(Handle, Msg.EM_GETSEL, new IntPtr(start), new IntPtr(end));
			from = range[0];
			to = range[1];
		}
	}
}

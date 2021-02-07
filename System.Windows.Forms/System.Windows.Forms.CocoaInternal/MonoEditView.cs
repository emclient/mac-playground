using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
#if XAMARINMAC
using Foundation;
using AppKit;
using CoreGraphics;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using nuint = System.UInt32;
using nfloat = System.Single;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	//[ExportClass("MonoEditView", "MonoView")]
	class MonoEditView : MonoView
#if XAMARINMAC
	, INSTextInputClient
	{
#elif MONOMAC
	{
		public override bool ConformsToProtocol(IntPtr protocol)
		{
			if ("NSTextInputClient" == NSString.FromHandle(Mac.Extensions.NSStringFromProtocol(protocol)))
				return true;

			return base.ConformsToProtocol(protocol);
		}
#endif

		internal NSAttributedString markedText = new NSMutableAttributedString();
		internal string insertText = String.Empty;

		public MonoEditView(IntPtr instance) : base(instance)
		{
		}

		public MonoEditView(XplatUICocoa driver, CGRect frameRect, WindowStyles style, WindowExStyles exStyle) : base(driver, frameRect, style, exStyle)
		{
		}

		#region INSTextInputView

		public virtual bool HasMarkedText
		{
			// Until this returns false, interpretKeyEvents continues to call insertText !
			[Export("hasMarkedText")]
			get
			{
				var result = markedText.Length != 0;
				//Debug.WriteLine($"hasMarkedText -> {result}");
				return result;
			}
		}

		public virtual NSRange MarkedRange
		{
			[Export("markedRange")]
			get
			{
				GetSelection(out var start, out var _);
				start = Math.Max(0, start - (int)markedText.Length);
				var result = new NSRange(start, markedText.Length);
				//Debug.WriteLine($"markedRange -> {result}");
				return result;
			}
		}

		public virtual NSRange SelectedRange
		{
			[Export("selectedRange")]
			get
			{
				GetSelection(out var start, out var end);
				var result = new NSRange(start, Math.Max(0, end - start));
				//Debug.WriteLine($"selectedRange -> {result}");
				return result;
			}
		}

		public virtual NSString[] ValidAttributesForMarkedText
		{
			[Export("validAttributesForMarkedText")]
			get { return validAttributesForMarkedText; }
		}

		public virtual NSWindowLevel WindowLevel
		{
			[Export("windowLevel")]
			get { return Window?.Level ?? NSWindowLevel.Normal; }
		}

		[Export("attributedSubstringForProposedRange:actualRange:")]
		public virtual NSAttributedString GetAttributedSubstring(NSRange proposedRange, out NSRange actualRange)
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

		[Export("characterIndexForPoint:")]
		public virtual nuint GetCharacterIndex(CGPoint point)
		{
			//Debug.WriteLine($"characterIndexForPoint:{point}");
			return 0;
		}

		[Export("firstRectForCharacterRange:actualRange:")]
		public virtual CGRect GetFirstRect(NSRange characterRange, out NSRange actualRange)
		{
			actualRange = new NSRange();

			// SWF textboxes
			if (XplatUICocoa.CaretView != null && XplatUICocoa.CaretView.Superview == this)
				return Window.ConvertRectToScreen(XplatUICocoa.CaretView.ConvertRectToView(Bounds, null));

			// EMC textboxes - teporary solution
			var r = new Rectangle[1] { new Rectangle() };
			var handle = GCHandle.Alloc(r, GCHandleType.Pinned);
			if (handle.IsAllocated)
			{
				var result = driver.SendMessage(Handle, Msg.WM_IME_GETCURRENTPOSITION, IntPtr.Zero, GCHandle.ToIntPtr(handle));
				if (result == (IntPtr)1)
				{
					var rect = new CGRect(Bounds.Left + r[0].X, Bounds.Top + r[0].Y, r[0].Width, r[0].Height);
					return Window.ConvertRectToScreen(ConvertRectToView(rect, null));
				}
				handle.Free();
			}

			// fallback
			return Window.ConvertRectToScreen(ConvertRectToView(Bounds, null));
		}

		[Export("insertText:replacementRange:")]
		public virtual void InsertText(NSObject text, NSRange replacementRange)
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

		[Export("setMarkedText:selectedRange:replacementRange:")]
		public virtual void SetMarkedText(NSObject text, NSRange selectedRange, NSRange replacementRange)
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

		[Export("unmarkText")]
		public virtual void UnmarkText()
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

		public virtual string GetText()
		{
			var length = (int)Application.SendMessage(Handle, Msg.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
			var buffer = new char[length];
			var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Application.SendMessage(Handle, Msg.WM_GETTEXT, (IntPtr)length, GCHandle.ToIntPtr(bufferHandle));
			var result = new String(buffer);
			bufferHandle.Free();
			return result;
		}

		public virtual void GetSelection(out int from, out int to)
		{
			int[] start = new int[] { 0 }, end = new int[] { 0 };
			var hStart = GCHandle.Alloc(start, GCHandleType.Pinned);
			var hEnd = GCHandle.Alloc(start, GCHandleType.Pinned);
			Application.SendMessage(Handle, Msg.EM_GETSEL, GCHandle.ToIntPtr(hStart), GCHandle.ToIntPtr(hEnd));
			hStart.Free();
			hEnd.Free();
			from = start[0];
			to = end[0];
		}
	}
}

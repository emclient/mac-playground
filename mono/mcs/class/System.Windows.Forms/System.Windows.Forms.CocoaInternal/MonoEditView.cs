using System.Drawing;
using System.Runtime.InteropServices;
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
			get { return false; }
			//get { return markedText.Length != 0; }
		}

		public virtual NSRange MarkedRange
		{
			[Export("markedRange")]
			get { return (markedText.Length > 0) ? new NSRange(0, markedText.Length - 1) : new NSRange(); }
		}

		public virtual NSRange SelectedRange
		{
			[Export("selectedRange")]
			get { return new NSRange(); }
		}

		public virtual NSString[] ValidAttributesForMarkedText
		{
			[Export("validAttributesForMarkedText")]
			get { return new NSString[] { }; }
		}

		public virtual NSWindowLevel WindowLevel
		{
			[Export("windowLevel")]
			get { return Window?.Level ?? NSWindowLevel.Normal; }
		}

		[Export("attributedSubstringForProposedRange:actualRange:")]
		public virtual NSAttributedString GetAttributedSubstring(NSRange proposedRange, out NSRange actualRange)
		{
			actualRange = new NSRange();
			return null;
		}

		[Export("characterIndexForPoint:")]
		public virtual nuint GetCharacterIndex(CGPoint point)
		{
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
			eventResponder.InsertText(text, replacementRange);
		}

		[Export("setMarkedText:selectedRange:replacementRange:")]
		public virtual void SetMarkedText(NSObject text, NSRange selectedRange, NSRange replacementRange)
		{
			if (text is NSAttributedString a)
				markedText = (NSAttributedString)a.MutableCopy();
			else
				markedText = new NSAttributedString((NSString)text);
		}

		[Export("unmarkText")]
		public virtual void UnmarkText()
		{
			markedText = new NSMutableAttributedString();
		}

		#endregion INSTextInputView
	}
}

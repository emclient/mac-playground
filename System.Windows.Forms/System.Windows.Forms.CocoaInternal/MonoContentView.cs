//
//MonoView.cs
// 
//Author:
//	Lee Andrus <landrus2@by-rite.net>
//
//Copyright (c) 2009-2010 Lee Andrus
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//

using AppKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;
using System.Windows.Forms.Mac;

namespace System.Windows.Forms.CocoaInternal
{
	partial class MonoContentView : MonoView, INSTextInputClient
	{
		public MonoContentView (NativeHandle instance) : base (instance)
		{
		}

		public MonoContentView (XplatUICocoa driver, CGRect frameRect, WindowStyles style, WindowExStyles exStyle)
			: base(driver, frameRect, style, exStyle)
		{
		}

		public override bool IsOpaque {
			get {
				return Window == null || Window.IsOpaque;
			}
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			// NOTE: When using custom title bar (NSWindowStyle.FullSizeContentView),
			// the NSWindow.IsMovable property can be set to false,
			// otherwise the window could be dragged by the *whole* former titlebar area,
			// even if there was an active edit box under the mouse pointer.
			// So, the following lines allow the window to be dragged by the exposed part
			// of the content view, which is probably the custom title bar.

			if (!Window.IsMovable
				&& Window.StyleMask.HasFlag(NSWindowStyle.FullSizeContentView)
				&& XplatUICocoa.LastMouseDown != null
				&& XplatUICocoa.LastMouseDown.Window == Window
				&& XplatUICocoa.LastMouseDown.ClickCount == 1
			)
				Window.PerformWindowDrag(theEvent);
			else
				eventResponder.MouseDragged(theEvent);
		}

		public override void MouseUp(NSEvent theEvent)
		{
			if (theEvent.IsLeftDoubleClick() && Window.StyleMask.HasFlag(NSWindowStyle.FullSizeContentView))
				if (Window.PerformDoubleClickInCaption(theEvent))
					return;

			base.MouseUp(theEvent);
		}

		// Deriving from INSTextInputClient allows for dispatching Text Input events to edit views with embed popup windows,
		// such as suggestion lists, when these popups are displayed and handle key strokes.
		// The implementation is not important, the only thing that matters is adopting the NSTextInputClient protocol,
		// so that the system gets tricked in the right moment.

		#region INSTextInputClient

		public bool HasMarkedText => false;

		public NSRange MarkedRange => new NSRange(0, 0);

		public NSRange SelectedRange => new NSRange(0, 0);

		public NSString[] ValidAttributesForMarkedText => new NSString[] {};

		public NSWindowLevel WindowLevel => Window?.Level ?? NSWindowLevel.Normal;

		public NSAttributedString GetAttributedSubstring(NSRange proposedRange, out NSRange actualRange)
		{
			actualRange = proposedRange;
			return new NSAttributedString();
		}

		public nuint GetCharacterIndex(CGPoint point) => 0;

		public CGRect GetFirstRect(NSRange characterRange, out NSRange actualRange)
		{
			actualRange = characterRange;
			return new CGRect();
		}

		public void InsertText(NSObject text, NSRange replacementRange) {}

		public void SetMarkedText(NSObject text, NSRange selectedRange, NSRange replacementRange) {}

		public void UnmarkText() {}

		#endregion INSTextInputView
	}
}

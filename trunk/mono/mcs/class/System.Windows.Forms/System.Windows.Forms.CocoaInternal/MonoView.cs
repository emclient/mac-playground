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

using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using System.Runtime.InteropServices;

#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
#else
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
#endif
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;


namespace System.Windows.Forms.CocoaInternal
{

	//[ExportClass("MonoView", "NSView")]
	internal partial class MonoView : NSView
	{
		protected XplatUICocoa driver;
		protected Hwnd hwnd;
		protected NSTrackingArea clientArea;

		public MonoView (IntPtr instance) : base (instance)
		{
		}

		public MonoView (XplatUICocoa driver, NSRect frameRect, Hwnd hwnd) : base(frameRect)
		{
			this.driver = driver;
			this.hwnd = hwnd;
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override bool IsOpaque {
			get {
				return !(Superview is MonoContentView);
			}
		}

		public override bool AcceptsFirstResponder ()
		{
			return false;
		}

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			return true;
		}

		public override void ViewDidMoveToWindow ()
		{
			base.ViewDidMoveToWindow ();
			UpdateTrackingAreas ();
		}

		public override void UpdateTrackingAreas()
		{
			if (Handle == hwnd.ClientWindow)
			{
				if (clientArea != null)
					RemoveTrackingArea(clientArea);

				clientArea = new NSTrackingArea(
					Bounds,
					NSTrackingAreaOptions.ActiveInActiveApp |
					NSTrackingAreaOptions.ActiveWhenFirstResponder |
					NSTrackingAreaOptions.MouseEnteredAndExited |
					NSTrackingAreaOptions.InVisibleRect,
					this,
					new NSDictionary());
				AddTrackingArea(clientArea);
			}

			base.UpdateTrackingAreas();
		}

		public override void MouseEntered(NSEvent e)
		{
			if (e.TrackingArea == clientArea && clientArea != null)
			{
				var wm = ToMSG(e, Msg.WM_MOUSE_ENTER);
				driver.EnqueueMessage(wm);
			}

			base.MouseEntered(e);
		}

		public override void MouseExited(NSEvent e)
		{
			if (e.TrackingArea == clientArea && clientArea != null)
			{
				var wm = ToMSG(e, Msg.WM_MOUSELEAVE);
				driver.EnqueueMessage(wm);
			}

			base.MouseExited(e);
		}

		public override void DrawRect (NSRect dirtyRect)
		{
			Rectangle bounds = driver.NativeToMonoFramed (dirtyRect, Frame.Size.Height);
			if (hwnd.ClientWindow != Handle) {
				DrawBorders ();
				hwnd.AddNcInvalidArea (bounds);
				driver.SendMessage (hwnd.Handle, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
			}
			else {
				// FIXME: Use getRectsBeingDrawn		
				hwnd.AddInvalidArea (bounds);
				driver.SendMessage (hwnd.Handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			}
		}

		private void DrawBorders ()
		{
			Graphics g;

			switch (hwnd.BorderStyle) {
			case FormBorderStyle.Fixed3D:
				using (g = Graphics.FromHwnd (hwnd.WholeWindow)) {
					if (hwnd.border_static)
						ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Border3DStyle.SunkenOuter);
					else
						ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Border3DStyle.Sunken);
				}
				break;

			case FormBorderStyle.FixedSingle:
				using (g = Graphics.FromHwnd (hwnd.WholeWindow))
					ControlPaint.DrawBorder (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Color.Black, ButtonBorderStyle.Solid);
				break;
			}
		}

		internal MSG ToMSG(NSEvent e, Msg type)
		{
			var nspoint = ConvertPointFromView(e.LocationInWindow, null);
			var localMonoPoint = driver.NativeToMonoFramed(nspoint, Frame.Height);
			var mousePosition = driver.NativeToMonoScreen(NSEvent.CurrentMouseLocation);

			int wParam = 0;
			var mouseButtons = NSEvent.CurrentPressedMouseButtons;
			if ((mouseButtons & 1) != 0)
				wParam |= (int)MsgButtons.MK_LBUTTON;
			if ((mouseButtons & 2) != 0)
				wParam |= (int)MsgButtons.MK_RBUTTON;
			if ((mouseButtons & 4) != 0)
				wParam |= (int)MsgButtons.MK_MBUTTON;
			if ((mouseButtons & 8) != 0)
				wParam |= (int)MsgButtons.MK_XBUTTON1;
			if ((mouseButtons & 16) != 0)
				wParam |= (int)MsgButtons.MK_XBUTTON2;
			var modifierFlags = NSEvent.CurrentModifierFlags;
			if ((modifierFlags & NSEventModifierMask.ControlKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_CONTROL;
			if ((modifierFlags & NSEventModifierMask.ShiftKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_SHIFT;

			var hWnd = Hwnd.ObjectFromHandle(Handle);
			return new MSG
			{
				hwnd = hWnd?.Handle ?? IntPtr.Zero,
				message = type,
				wParam = (IntPtr)wParam,
				lParam = (IntPtr)((ushort)localMonoPoint.Y << 16 | (ushort)localMonoPoint.X),
				refobject = hwnd,
				pt = { x = mousePosition.X, y = mousePosition.Y }
			};
		}

		public bool PointInRect(NSPoint p, NSRect r)
		{
			return IsFlipped
				? !(p.X < r.X || p.Y < r.Top || p.X > r.Right || p.Y > r.Bottom)
					: !(p.X < r.X || p.Y < r.Bottom || p.X > r.Right || p.Y > r.Top);
		}
	}
}

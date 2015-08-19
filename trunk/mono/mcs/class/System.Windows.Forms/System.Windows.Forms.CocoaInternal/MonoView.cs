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
	}
}

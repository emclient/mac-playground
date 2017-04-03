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

using System.Drawing;
using System.Drawing.Mac;

#if XAMARINMAC
using Foundation;
using AppKit;
using NSRect = CoreGraphics.CGRect;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using NMath = System.Math;
using NSRect = MonoMac.CoreGraphics.CGRect;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	//[ExportClass("MonoView", "NSView")]
	internal partial class MonoView : NSView, System.Drawing.IClientView
	{
		protected XplatUICocoa driver;
		protected Hwnd hwnd;
		protected NSTrackingArea clientArea;
		protected WindowsEventResponder eventReponder;
		internal bool inSetFocus;

		public MonoView(IntPtr instance) : base(instance)
		{
		}

		public MonoView(XplatUICocoa driver, NSRect frameRect, Hwnd hwnd) : base(frameRect)
		{
			this.driver = driver;
			this.hwnd = hwnd;
			this.eventReponder = new WindowsEventResponder(driver, hwnd, this);
			base.NextResponder = eventReponder;
		}

		public override NSResponder NextResponder
		{
			get
			{
				return base.NextResponder;
			}
			set
			{
				eventReponder.NextResponder = value;
			}
		}

		public override bool IsFlipped
		{
			get
			{
				return true;
			}
		}

		public override bool IsOpaque
		{
			get
			{
				if (Superview is MonoContentView)
					return Superview.IsOpaque;
				return true;
			}
		}

		public override bool AcceptsFirstResponder()
		{
			// Skip the normal processing to bypass setting it as first responder. Our
			// controls will do that by themselves by calling SetFocus.
			return hwnd.Enabled && inSetFocus;
		}

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			System.Diagnostics.Debug.WriteLine("AcceptsFirstMouse");
			return true;
		}

		public override void ViewDidMoveToWindow()
		{
			base.ViewDidMoveToWindow();
			UpdateTrackingAreas();
		}

		public override void UpdateTrackingAreas()
		{
			if (clientArea != null)
			{
				RemoveTrackingArea(clientArea);
				clientArea = null;
			}

			clientArea = new NSTrackingArea(
				Bounds,
				NSTrackingAreaOptions.ActiveInActiveApp |
				NSTrackingAreaOptions.MouseEnteredAndExited |
				NSTrackingAreaOptions.MouseMoved |
				NSTrackingAreaOptions.InVisibleRect,
				this,
				new NSDictionary());
			AddTrackingArea(clientArea);

			base.UpdateTrackingAreas();
		}

		public override void DrawRect(NSRect dirtyRect)
		{
			Rectangle bounds = driver.NativeToMonoFramed(dirtyRect, Frame.Size.Height);
			MSG msg;

			DrawBorders();
			hwnd.AddNcInvalidArea(bounds);
			//driver.SendMessage (hwnd.Handle, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
			msg = new MSG { hwnd = hwnd.Handle, message = Msg.WM_NCPAINT };
			driver.DispatchMessage(ref msg);
			hwnd.ClearNcInvalidArea();

			bounds.X -= hwnd.ClientRect.X;
			bounds.Y -= hwnd.ClientRect.Y;
			hwnd.AddInvalidArea(bounds);
			//driver.SendMessage (hwnd.Handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			msg = new MSG { hwnd = hwnd.Handle, message = Msg.WM_PAINT };
			driver.DispatchMessage(ref msg);
			hwnd.ClearInvalidArea();
		}

		public NSRect ClientBounds
		{
			get
			{
				return new NSRect(
					NMath.Max(this.hwnd.ClientRect.Top, 0),
					NMath.Max(this.hwnd.ClientRect.Left, 0),
					NMath.Max(this.hwnd.ClientRect.Width, 0),
					NMath.Max(this.hwnd.ClientRect.Height, 0));
			}
		}

		private void DrawBorders()
		{
			switch (hwnd.BorderStyle)
			{
				case FormBorderStyle.Fixed3D:
					using (var g = Graphics.FromHwnd(Handle, false))
					{
						if (hwnd.border_static)
							ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Border3DStyle.SunkenOuter);
						else
							ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Border3DStyle.Sunken);
					}
					break;

				case FormBorderStyle.FixedSingle:
					Color color = NSColor.Grid.ToSDColor(); // Color.Black
					using (var g = Graphics.FromHwnd(Handle, false))
						ControlPaint.DrawBorder(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), color, ButtonBorderStyle.Solid);
					break;
			}
		}
	}
}

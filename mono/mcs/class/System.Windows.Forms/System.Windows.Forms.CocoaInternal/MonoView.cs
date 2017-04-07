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
using CoreGraphics;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using NMath = System.Math;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	//[ExportClass("MonoView", "NSView")]
	partial class MonoView : NSView, System.Drawing.IClientView
	{
		protected XplatUICocoa driver;
		protected NSTrackingArea clientArea;
		protected WindowsEventResponder eventReponder;
		protected CGRect clientBounds;
		internal bool inSetFocus;

		public MonoView(IntPtr instance) : base(instance)
		{
		}

		public MonoView(XplatUICocoa driver, CGRect frameRect, WindowStyles style, WindowExStyles exStyle) : base(frameRect)
		{
			this.driver = driver;
			this.Style = style;
			this.ExStyle = exStyle;
			this.eventReponder = new WindowsEventResponder(driver, this);
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
				if (!(Superview is MonoView))
				    return false;
				if (Superview is MonoContentView)
					return Superview.IsOpaque;
				if (UserClip != null)
					return false;
				return true;
			}
		}

		public override bool AcceptsFirstResponder()
		{
			// Skip the normal processing to bypass setting it as first responder. Our
			// controls will do that by themselves by calling SetFocus.
			return Enabled && inSetFocus;
		}

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
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
				NSTrackingAreaOptions.InVisibleRect |
				NSTrackingAreaOptions.CursorUpdate,
				this,
				new NSDictionary());
			AddTrackingArea(clientArea);

			base.UpdateTrackingAreas();
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			MSG msg;

			this.DirtyRectangle = driver.NativeToMonoFramed(dirtyRect, this);

			DrawBorders();

			msg = new MSG { hwnd = this.Handle, message = Msg.WM_NCPAINT };
			driver.DispatchMessage(ref msg);
			msg = new MSG { hwnd = this.Handle, message = Msg.WM_PAINT };
			driver.DispatchMessage(ref msg);

			this.DirtyRectangle = null;
		}

		public CGRect ClientBounds
		{
			get
			{
				return new CGRect(
					NMath.Max(clientBounds.Left, 0),
					NMath.Max(clientBounds.Top, 0),
					NMath.Max(clientBounds.Width, 0),
					NMath.Max(clientBounds.Height, 0));
			}
			set
			{
				this.clientBounds = value;
			}
		}

		private void DrawBorders()
		{
			if (ExStyle.HasFlag(WindowExStyles.WS_EX_CLIENTEDGE) || ExStyle.HasFlag(WindowExStyles.WS_EX_STATICEDGE))
			{
				using (var g = Graphics.FromHwnd(Handle, false))
				{
					if (ExStyle.HasFlag(WindowExStyles.WS_EX_STATICEDGE))
						ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, (int)Frame.Width, (int)Frame.Height), Border3DStyle.SunkenOuter);
					else
						ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, (int)Frame.Width, (int)Frame.Height), Border3DStyle.Sunken);
				}
			}
			else if (Style.HasFlag(WindowStyles.WS_BORDER))
			{
				Color color = NSColor.Grid.ToSDColor(); // Color.Black
				using (var g = Graphics.FromHwnd(Handle, false))
					ControlPaint.DrawBorder(g, new Rectangle(0, 0, (int)Frame.Width, (int)Frame.Height), color, ButtonBorderStyle.Solid);
			}
		}

		public override void CursorUpdate(NSEvent theEvent)
		{
			driver.OverrideCursor(this.Cursor);
		}

		public Rectangle? DirtyRectangle { get; private set; }
		public WindowStyles Style { get; set; }
		public WindowExStyles ExStyle { get; set; }
		public Region UserClip { get; set; }
		public IntPtr Cursor { get; set; }

		public bool Enabled
		{
			get { return !Style.HasFlag(WindowStyles.WS_DISABLED); }
			set
			{
				if (value)
					Style &= ~WindowStyles.WS_DISABLED;
				else
					Style |= WindowStyles.WS_DISABLED;
			}
		}
	}
}

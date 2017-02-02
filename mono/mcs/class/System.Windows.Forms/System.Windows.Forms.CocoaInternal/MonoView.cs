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
using System.Runtime.InteropServices;

#if XAMARINMAC
using Foundation;
using AppKit;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using NMath = System.Math;
using System.Drawing.Mac;
#endif

#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
#else
#if XAMARINMAC
using NSRect = CoreGraphics.CGRect;
using NSPoint = CoreGraphics.CGPoint;
using nuint = System.UInt64;
using nint = System.Int64;
#elif MONOMAC
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
using nuint = System.UInt32;
using nint = System.Int32;
#endif
#endif
using System.Drawing;

namespace System.Windows.Forms.CocoaInternal
{
	//[ExportClass("MonoView", "NSView")]
	internal partial class MonoView : NSView, System.Drawing.IClientView
	{
		protected XplatUICocoa driver;
		protected Hwnd hwnd;

		protected NSTrackingArea clientArea;
		static MonoView mouseView; // A view that is currently under the mouse cursor.

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
				if (Superview is MonoContentView)
					return Superview.IsOpaque;
				return true;
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
			//if (Handle == hwnd.WholeWindow)
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
					NSTrackingAreaOptions.InVisibleRect,
					this,
					new NSDictionary());
				AddTrackingArea(clientArea);
			}

			base.UpdateTrackingAreas();
		}

		public override void MouseEntered(NSEvent e)
		{
			base.MouseEntered(e);
			MouseEnteredOrExited(e);
		}

		public override void MouseExited(NSEvent e)
		{
			base.MouseExited(e);
			MouseEnteredOrExited(e);
		}

		public virtual void MouseEnteredOrExited(NSEvent e)
		{
			if (e.TrackingArea == clientArea && clientArea != null)
			{
				if (XplatUICocoa.Grab.Hwnd == IntPtr.Zero)
				{
					var view = Window.ContentView.HitTest(e.LocationInWindow) as MonoView;
					if (view != mouseView && mouseView != null)
					{
						driver.EnqueueMessage(ToMSG(mouseView, e, Msg.WM_MOUSELEAVE));
						mouseView = null;
					}

					if (view != null && mouseView != view)
					{
						driver.EnqueueMessage(ToMSG(view, e, Msg.WM_MOUSE_ENTER));
						mouseView = view;
					}
				}
			}
		}

		public bool IsGrabbed
		{
			get { return XplatUICocoa.Grab.Hwnd == hwnd.Handle; }
		}

		public override void DrawRect (NSRect dirtyRect)
		{
			Rectangle bounds = driver.NativeToMonoFramed (dirtyRect, Frame.Size.Height);
			MSG msg;

			DrawBorders();
			hwnd.AddNcInvalidArea(bounds);
			//driver.SendMessage (hwnd.Handle, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
			msg = new MSG { hwnd = hwnd.Handle, message = Msg.WM_NCPAINT };
			driver.DispatchMessage(ref msg);
			hwnd.ClearNcInvalidArea();

			bounds.X -= hwnd.ClientRect.X;
			bounds.Y -= hwnd.ClientRect.Y;
			hwnd.AddInvalidArea (bounds);
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

		private void DrawBorders ()
		{
			switch (hwnd.BorderStyle) {
			case FormBorderStyle.Fixed3D:
				using (var g = Graphics.FromHwnd (Handle, false)) {
					if (hwnd.border_static)
						ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Border3DStyle.SunkenOuter);
					else
						ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Border3DStyle.Sunken);
				}
				break;

			case FormBorderStyle.FixedSingle:
				Color color = NSColor.Grid.ToSDColor(); // Color.Black
				using (var g = Graphics.FromHwnd (Handle, false))
					ControlPaint.DrawBorder (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), color, ButtonBorderStyle.Solid);
				break;
			}
		}

		internal MSG ToMSG(NSEvent e, Msg type)
		{
			return ToMSG(this, e, type);
		}

		internal static MSG ToMSG(MonoView view, NSEvent e, Msg type)
		{
			var nspoint = view.ConvertPointFromView(e.LocationInWindow, null);
			var localMonoPoint = view.driver.NativeToMonoFramed(nspoint, view.Frame.Height);
			var mousePosition = view.driver.NativeToMonoScreen(NSEvent.CurrentMouseLocation);

			var hWnd = Hwnd.ObjectFromHandle(view.Handle)?.Handle ?? IntPtr.Zero;
			return new MSG
			{
				hwnd = hWnd,
				message = type,
				wParam = ToWParam(e),
				lParam = (IntPtr)((ushort)localMonoPoint.Y << 16 | (ushort)localMonoPoint.X),
				refobject = hWnd,
				pt = { x = mousePosition.X, y = mousePosition.Y }
			};
		}

		public static IntPtr ToWParam(NSEvent e)
		{
			switch (e.Type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseDown:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseDown:
				case NSEventType.OtherMouseUp:
					return (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonNumberToWParam(e.ButtonNumber));
				default:
					return (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonMaskToWParam(NSEvent.CurrentPressedMouseButtons));
			}
		}

		public static nuint ButtonMaskToWParam(nuint mouseButtons)
		{
			uint wParam = 0;

			if ((mouseButtons & 1) != 0)
				wParam |= (uint)MsgButtons.MK_LBUTTON;
			if ((mouseButtons & 2) != 0)
				wParam |= (uint)MsgButtons.MK_RBUTTON;
			if ((mouseButtons & 4) != 0)
				wParam |= (uint)MsgButtons.MK_MBUTTON;
			if ((mouseButtons & 8) != 0)
				wParam |= (uint)MsgButtons.MK_XBUTTON1;
			if ((mouseButtons & 16) != 0)
				wParam |= (uint)MsgButtons.MK_XBUTTON2;

			return wParam;
		}

		public static nuint ButtonNumberToWParam(nint buttonNumber)
		{
			switch (buttonNumber)
			{
				case 0: return (uint)MsgButtons.MK_LBUTTON;
				case 1: return (uint)MsgButtons.MK_RBUTTON;
				case 2: return (uint)MsgButtons.MK_MBUTTON;
				case 3: return (uint)MsgButtons.MK_XBUTTON1;
				case 4: return (uint)MsgButtons.MK_XBUTTON2;
			}
			return 0;
		}

		public static uint ModifiersToWParam(NSEventModifierMask modifierFlags)
		{
			uint wParam = 0;

			if ((modifierFlags & NSEventModifierMask.ControlKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_CONTROL;
			if ((modifierFlags & NSEventModifierMask.ShiftKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_SHIFT;

			return wParam;
		}
	}
}

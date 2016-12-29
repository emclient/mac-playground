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
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if XAMARINMAC
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac;
using MonoMac.Foundation;
using MonoMac.AppKit;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif

#if SDCOMPAT
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
#else
#if XAMARINMAC
using NSRect = CoreGraphics.CGRect;
using NSPoint = CoreGraphics.CGPoint;
#elif MONOMAC
using NSRect = MonoMac.CoreGraphics.CGRect;
using NSPoint = MonoMac.CoreGraphics.CGPoint;
#endif
#endif

namespace System.Windows.Forms.CocoaInternal
{
	internal partial class MonoContentView : MonoView
	{
		NSTrackingArea trackingArea;
		internal IntPtr FocusHandle;
		internal static int repeatCount = 0;
		internal static bool altDown;
		internal static bool cmdDown;
		internal static bool shiftDown;
		internal static bool ctrlDown;
		internal NSView hitTestResult; //Helper for detecting clicks in the title bar & perf. optimisation

		public MonoContentView (IntPtr instance) : base (instance)
		{
		}

		public MonoContentView (XplatUICocoa driver, NSRect frameRect, Hwnd hwnd) : base(driver, frameRect, hwnd)
		{
		}

		public override bool IsOpaque {
			get {
				return Window == null || Window.IsOpaque;
			}
		}

		public override bool AcceptsFirstResponder()
		{
			return true;
		}

		public override bool BecomeFirstResponder()
		{
			// Getting focus to non-SWF control
			if (FocusHandle != IntPtr.Zero)
				driver.SendMessage(FocusHandle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
			return base.BecomeFirstResponder();
		}

		public override bool ResignFirstResponder()
		{
			// Losing focus to non-SWF control
			if (FocusHandle != IntPtr.Zero)
			{
				driver.SendMessage(FocusHandle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
				FocusHandle = IntPtr.Zero;
			}
			return base.ResignFirstResponder();
		}

		#region Mouse

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null)
			{
				RemoveTrackingArea(trackingArea);
				trackingArea = null;
			}
			
			trackingArea = new NSTrackingArea(
				Bounds,
				NSTrackingAreaOptions.ActiveInActiveApp |
				NSTrackingAreaOptions.MouseEnteredAndExited |
				NSTrackingAreaOptions.InVisibleRect,
				this,
				new NSDictionary());
			AddTrackingArea(trackingArea);

			base.UpdateTrackingAreas();
		}

		protected bool IsKeyInParentChain(NSEvent e)
		{
			for (var w = e.Window; w != null; w = w.ParentWindow)
				if (w.IsKeyWindow)
					return true;

			return false;
		}

		public override void MouseDown (NSEvent theEvent)
		{
			base.MouseDown(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			base.RightMouseDown(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void OtherMouseDown (NSEvent theEvent)
		{
			base.OtherMouseDown(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void MouseUp (NSEvent theEvent)
		{
			base.MouseUp(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			base.RightMouseUp(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void OtherMouseUp (NSEvent theEvent)
		{
			base.OtherMouseUp(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			base.MouseMoved(theEvent);

			if (IsKeyInParentChain(theEvent))
				ProcessMouseEvent (theEvent);
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			base.MouseDragged(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void RightMouseDragged (NSEvent theEvent)
		{
			base.RightMouseDragged(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void OtherMouseDragged (NSEvent theEvent)
		{
			base.OtherMouseDragged(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void ScrollWheel (NSEvent theEvent)
		{
			base.ScrollWheel(theEvent);
			ProcessMouseEvent (theEvent);
		}

		public override void MouseEntered(NSEvent e)
		{
			if (e.TrackingArea == trackingArea && trackingArea != null)
			{
				Window.AcceptsMouseMovedEvents = true;
				driver.OverrideCursor(hwnd.Cursor);
			}

			base.MouseEntered(e);
		}

		public override void MouseExited(NSEvent e)
		{
			if (e.TrackingArea == trackingArea && trackingArea != null)
			{
				Window.AcceptsMouseMovedEvents = false;
				driver.OverrideCursor(IntPtr.Zero);
			}
	
			base.MouseExited(e);
		}

		public override NSView HitTest(NSPoint aPoint)
		{
			return HitTest(aPoint, false, true);
		}

		public virtual NSView HitTest(NSPoint aPoint, bool ignoreGrab, bool saveResult)
		{
			NSView hit = null;
			if (!ignoreGrab)
			{
				var grabbed = XplatUICocoa.Grab.Hwnd;
				if (grabbed != IntPtr.Zero)
					hit = (NSView)ObjCRuntime.Runtime.GetNSObject(grabbed);
			}

			if (hit == null)
				hit = base.HitTest(aPoint);

			if (saveResult)
				hitTestResult = hit;

			return hit;
		}

		public void ProcessMouseEvent (NSEvent eventref)
		{
			NSPoint nspoint = eventref.LocationInWindow;
			Point localMonoPoint;
			Hwnd currentHwnd;
			bool client = false;

			if (hwnd.zombie)
				return;

			NSView vuWrap;
			if (XplatUICocoa.Grab.Hwnd != IntPtr.Zero) {
				//DebugUtility.WriteInfoIfChanged(XplatUICocoa.Grab.Hwnd);

				currentHwnd = Hwnd.ObjectFromHandle (XplatUICocoa.Grab.Hwnd);
				if (null == currentHwnd || currentHwnd.zombie)
				{
					//DebugUtility.WriteInfoIfChanged(HitTest(nspoint, true, false));
					return;
				}

				vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(currentHwnd.Handle);
				if (vuWrap.Window != Window)
					nspoint = vuWrap.Window.ConvertScreenToBase(Window.ConvertBaseToScreen(nspoint));
				nspoint = vuWrap.ConvertPointFromView(nspoint, null);
				//DebugUtility.WriteInfoIfChanged(vuWrap.Window.ContentView.HitTest(nspoint));

				localMonoPoint = driver.NativeToMonoFramed(nspoint, (int) vuWrap.Frame.Size.Height);
				client = true;
			}
			else {
				vuWrap = HitTest(nspoint, false, false);
				//DebugUtility.WriteInfoIfChanged(vuWrap);

				// Embedded native control? => Find MonoView parent
				while (vuWrap != null && !(vuWrap is MonoView))
					vuWrap = vuWrap.Superview;

				if (!(vuWrap is MonoView))
					return;
				currentHwnd = Hwnd.ObjectFromHandle(vuWrap.Handle);
				nspoint = vuWrap.ConvertPointFromView(nspoint, null);
				localMonoPoint = driver.NativeToMonoFramed(nspoint, Frame.Height);
				if (currentHwnd.ClientRect.Contains(localMonoPoint))
				{
					client = true;//currentHwnd.ClientWindow == vuWrap.Handle; // currentHwnd.Handle;
					localMonoPoint.X -= currentHwnd.ClientRect.X;
					localMonoPoint.Y -= currentHwnd.ClientRect.Y;
				}
			}

			int button = (int) eventref.ButtonNumber;
			if (button >= (int) NSMouseButtons.Excessive)
				button = (int) NSMouseButtons.X;
			else if (button == (int) NSMouseButtons.Left && ((driver.ModifierKeys & Keys.Control) != 0))
				button = (int) NSMouseButtons.Right;
			int msgOffset4Button = 3 * (button - (int) NSMouseButtons.Left);
			if (button >= (int) NSMouseButtons.X)
				++msgOffset4Button;

			MSG msg = new MSG ();
			msg.hwnd = currentHwnd.Handle;
			msg.lParam = (IntPtr)(localMonoPoint.Y << 16 | (localMonoPoint.X & 0xFFFF));
			msg.pt = driver.NativeToMonoScreen(Window.ConvertBaseToScreen(eventref.LocationInWindow)).ToPOINT();
			msg.refobject = hwnd;
			msg.wParam = ToWParam(eventref);

			switch (eventref.Type) {
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
					// FIXME: Should be elsewhere
					if (eventref.ClickCount > 1)
						msg.message = (client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK) + msgOffset4Button;
					else
						msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN) + msgOffset4Button;
					break;

				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseUp:
					msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP) + msgOffset4Button;
					break;

				case NSEventType.MouseMoved:
				case NSEventType.LeftMouseDragged:
				case NSEventType.RightMouseDragged:
				case NSEventType.OtherMouseDragged:
					if (XplatUICocoa.Grab.Hwnd == IntPtr.Zero) {
						IntPtr ht = IntPtr.Zero;
						if (client) {
							ht = (IntPtr)Forms.HitTest.HTCLIENT;
							NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, 
								(IntPtr)Forms.HitTest.HTCLIENT);
						} else {
							ht = (IntPtr) NativeWindow.WndProc (hwnd.ClientWindow, Msg.WM_NCHITTEST, 
								IntPtr.Zero, msg.lParam).ToInt32 ();
							NativeWindow.WndProc(hwnd.ClientWindow, Msg.WM_SETCURSOR, msg.hwnd, ht);
						}
					}

					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
					break;

				case NSEventType.ScrollWheel:

					int delta = ScaleAndQuantizeDelta((float)eventref.ScrollingDeltaY, eventref.HasPreciseScrollingDeltas);
					if (delta == 0)
						return;

					msg.message = Msg.WM_MOUSEWHEEL;
					msg.wParam = (IntPtr)(((int)msg.wParam & 0xFFFF) | (delta << 16));
					msg.lParam = (IntPtr)((msg.pt.x & 0xFFFF)| (msg.pt.y << 16));
					break;

				//case NSEventType.TabletPoint:
				//case NSEventType.TabletProximity:
				default:
					return;
			}

			driver.EnqueueMessage(msg);
		}

		int nprecise = 0;
		int ScaleAndQuantizeDelta(float delta, bool precise)
		{
			if (precise)
			{
				if (++nprecise % 3 != 0)
					return 0;
					
				const double scale = 10.0;
				int step = delta >= 0 ? 60 : -60;
				return ((int)((delta * scale + step) / step)) * step;
			}
			else
			{
				const double scale = 40.0;
				int step = delta >= 0 ? 60 : -60;
				return ((int)((delta * scale + step) / step)) * step;
			}
		}

		#endregion

		#region Keyboard

		public override void KeyDown (NSEvent theEvent)
		{
			var flags = theEvent.ModifierFlags;
			shiftDown = 0 != (flags & NSEventModifierMask.ShiftKeyMask);
			ctrlDown = 0 != (flags & NSEventModifierMask.ControlKeyMask);
			altDown = 0 != (flags & NSEventModifierMask.AlternateKeyMask);
			cmdDown = 0 != (flags & NSEventModifierMask.CommandKeyMask);

			ProcessKeyPress(theEvent);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			ProcessKeyPress(theEvent);
		}

		public override void FlagsChanged (NSEvent theEvent)
		{
			ProcessModifiers(theEvent);
		}

		public void ProcessKeyPress (NSEvent e)
		{
			repeatCount = e.IsARepeat ? 1 + repeatCount : 0;

			if (FocusHandle == IntPtr.Zero)
				return;

			ushort charCode = 0x0;
			char c = '\0';

			var chars = e.Type == NSEventType.KeyDown ? KeysConverter.GetCharactersForKeyPress (e) : "";

			chars = chars ?? e.Characters;
			if (chars.Length > 0) {
				c = chars [0];
				charCode = chars [0];
			}

			var keyCode = (ushort)e.KeyCode;

			Keys key = KeysConverter.GetKeys (e);
			if (key == Keys.None)
				return;

			bool isExtendedKey = 0 != (uint)(e.ModifierFlags & (NSEventModifierMask.ControlKeyMask | NSEventModifierMask.AlternateKeyMask));

			IntPtr wParam = (IntPtr) key;
			ulong lp = 0;
			lp |= ((ulong)(uint)repeatCount);
			lp |= ((ulong)keyCode) << 16; // OEM-dependent scanCode
			lp |= ((ulong)(isExtendedKey ? 1 : 0)) << 24;
			lp |= ((ulong)(e.IsARepeat ? 1 : 0)) << 30;
			IntPtr lParam = (IntPtr)lp;

			Msg msg = altDown && !cmdDown
				? (e.Type == NSEventType.KeyDown ? Msg.WM_SYSKEYDOWN : Msg.WM_SYSKEYUP)
				: (e.Type == NSEventType.KeyDown ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);

			if (e.Type == NSEventType.KeyDown && !string.IsNullOrEmpty (chars)) {

				if (KeysConverter.IsChar (c, key)) {
					XplatUICocoa.PushChars (chars); // XplatUICocoa pops them
					//driver.SendMessage (FocusHandle, Msg.WM_IME_COMPOSITION, IntPtr.Zero, IntPtr.Zero);
				}
			}

			//Debug.WriteLine ("keyCode={0}, characters=\"{1}\", key='{2}', chars='{3}'", e.KeyCode, chars, key, chars);
			driver.PostMessage(FocusHandle, msg, wParam, lParam);
		}

		public void ProcessModifiers (NSEvent eventref)
		{
			// we get notified when modifiers change, but not specifically what changed
			NSEventModifierMask flags = eventref.ModifierFlags;
			NSEventModifierMask diff = flags ^ XplatUICocoa.key_modifiers;
			XplatUICocoa.key_modifiers = flags;

			if (FocusHandle == IntPtr.Zero)
				return;

			if ((NSEventModifierMask.ShiftKeyMask & diff) != 0)
				driver.SendMessage(FocusHandle, (NSEventModifierMask.ShiftKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_SHIFT, IntPtr.Zero);
			if ((NSEventModifierMask.ControlKeyMask & diff) != 0)
				driver.SendMessage(FocusHandle, (NSEventModifierMask.ControlKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_CONTROL, IntPtr.Zero);
			if ((NSEventModifierMask.AlternateKeyMask & diff) != 0)
				driver.SendMessage(FocusHandle, (NSEventModifierMask.AlternateKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_MENU, IntPtr.Zero);
			//if ((NSEventModifierMask.CommandKeyMask & diff) != 0)
			//	driver.SendMessage(FocusHandle, (NSEventModifierMask.AlternateKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_LWIN, IntPtr.Zero);
		}

		#endregion
	}
}

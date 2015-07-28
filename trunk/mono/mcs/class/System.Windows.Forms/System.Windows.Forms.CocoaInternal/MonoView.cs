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
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
using System.Drawing;
using System.Collections.Generic;

namespace System.Windows.Forms.CocoaInternal
{

	//[ExportClass("MonoView", "NSView")]
	internal class MonoView : NSView
	{
		XplatUICocoa driver;

		public MonoView (IntPtr instance) : base (instance)
		{
		}

		public MonoView (XplatUICocoa driver, NSRect frameRect) : base(frameRect)
		{
			this.driver = driver;
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override bool IsOpaque {
			get {
				return true;
			}
		}

		public override bool AcceptsFirstResponder ()
		{
			return true;
		}

		public override void DrawRect (NSRect dirtyRect)
		{
			Hwnd hwnd = Hwnd.ObjectFromWindow (Handle);
			NSRect nsbounds = dirtyRect;
			bool client = hwnd.ClientWindow == Handle;
			Rectangle bounds = driver.NativeToMonoFramed (nsbounds, Frame.Size.Height);
			Rectangle clientBounds = bounds;
			bool nonclient = ! client;

			if (!hwnd.visible) {
				if (client)
					hwnd.expose_pending = false;
				if (nonclient)
					hwnd.nc_expose_pending = false;
				return;
			}

			if (nonclient) {
				DrawBorders (hwnd);
			}

			if (nonclient) {
				hwnd.AddNcInvalidArea (bounds);
				driver.SendMessage (hwnd.Handle, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
			}
			if (client) {
				// FIXME: Use getRectsBeingDrawn		
				hwnd.AddInvalidArea (clientBounds);
				driver.SendMessage (hwnd.Handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			}
		}

		private void DrawBorders (Hwnd hwnd) {
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

		#region Mouse

		public override void MouseDown (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void OtherMouseDown (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void MouseUp (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void OtherMouseUp (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void RightMouseDragged (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void OtherMouseDragged (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void ScrollWheel (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		/*public override void MouseEntered (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}

		public override void MouseExited (NSEvent theEvent)
		{
			ProcessMouseEvent (theEvent);
		}*/

		public void ProcessMouseEvent (NSEvent eventref)
		{
			NSPoint nspoint = eventref.LocationInWindow, globalNSPoint = nspoint;
			// Insure the location is still valid.
			NSWindow winWrap = eventref.Window;
			if (winWrap != null) {
				globalNSPoint = winWrap.ConvertBaseToScreen (nspoint);
				driver.mouse_position = driver.NativeToMonoScreen (globalNSPoint);
				IntPtr contentHandle = driver.WindowToHandle ((IntPtr) winWrap.Handle);
				if (IntPtr.Zero == contentHandle)
					return;
				Hwnd contentHwnd = Hwnd.ObjectFromWindow (contentHandle);
				if (null == contentHwnd || contentHwnd.zombie)
					return;
			} else {
				driver.mouse_position = driver.NativeToMonoScreen (globalNSPoint);
			}

			Point localMonoPoint = driver.mouse_position;
			NSView vuWrap = null;
			bool client = true;
			int button = (int) eventref.ButtonNumber;
			Hwnd hwnd =null;

			IntPtr hwndHandle = XplatUICocoa.Grab.Hwnd;
			bool grabbed = IntPtr.Zero != hwndHandle;

			if (button >= (int) NSMouseButtons.Excessive)
				button = (int) NSMouseButtons.X;
			else if (button == (int) NSMouseButtons.Left && ((driver.ModifierKeys & Keys.Control) != 0))
				button = (int) NSMouseButtons.Right;

			int msgOffset4Button = 3 * (button - (int) NSMouseButtons.Left);
			if (button >= (int) NSMouseButtons.X)
				++msgOffset4Button;

			if (grabbed) {
				hwnd = Hwnd.ObjectFromHandle (hwndHandle); 
				if (null == hwnd || hwnd.zombie)
					return;
				vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);

				if (vuWrap.Window != winWrap) {
					// Over different or no window: adjust coordinates.
					// Now in screen coordinates.
					winWrap = vuWrap.Window;
					nspoint = winWrap.ConvertScreenToBase (globalNSPoint);
					// Now in grabber window coordinates.
				}

				nspoint = vuWrap.ConvertPointFromView (nspoint, null);
				localMonoPoint = driver.NativeToMonoFramed (nspoint, (int) vuWrap.Frame.Size.Height);
				client = true;
			} else if (winWrap != null) {
				NSView contentVuWrap = winWrap.ContentView;
				vuWrap = contentVuWrap.HitTest (nspoint);

				if (vuWrap == null)
					return;	// Part of window Mono is not handling (title bar).

				IntPtr vuHandle = vuWrap.Handle;
				hwnd = Hwnd.ObjectFromWindow (vuHandle);
				if (null == hwnd || hwnd.zombie)
					return;

				hwndHandle = hwnd.Handle;
				nspoint = vuWrap.ConvertPointFromView (nspoint, null);
				localMonoPoint = driver.NativeToMonoFramed (nspoint, (int) vuWrap.Frame.Size.Height);
				client = vuHandle == hwnd.ClientWindow;
			}

			if (hwnd == null)
				return;

			MSG msg = new MSG ();
			msg.hwnd = hwnd.Handle;
			msg.lParam = (IntPtr) ((ushort) localMonoPoint.Y << 16 | (ushort) localMonoPoint.X);
			msg.pt.x = driver.mouse_position.X;
			msg.pt.y = driver.mouse_position.Y;
			msg.refobject = hwnd;

			switch (eventref.Type) {
			case NSEventType.LeftMouseDown:
			case NSEventType.RightMouseDown:
			case NSEventType.OtherMouseDown:
				UpdateMouseState (button, true);
				// FIXME: Should be elsewhere
				if (eventref.ClickCount > 1)
					msg.message = (client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK) + msgOffset4Button;
				else
					msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN) + msgOffset4Button;
				msg.wParam = driver.GetMousewParam (0);
				break;

			case NSEventType.LeftMouseUp:
			case NSEventType.RightMouseUp:
			case NSEventType.OtherMouseUp:
				UpdateMouseState (button, false);
				msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP) + msgOffset4Button;
				msg.wParam = driver.GetMousewParam (0);
				break;

			case NSEventType.MouseMoved:
			case NSEventType.LeftMouseDragged:
			case NSEventType.RightMouseDragged:
			case NSEventType.OtherMouseDragged:
				if (XplatUICocoa.Grab.Hwnd == IntPtr.Zero) {
					IntPtr ht = IntPtr.Zero;
					if (client) {
						ht = (IntPtr) System.Windows.Forms.HitTest.HTCLIENT;
						NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, 
							(IntPtr) System.Windows.Forms.HitTest.HTCLIENT);
					} else {
						ht = (IntPtr) NativeWindow.WndProc (hwnd.ClientWindow, Msg.WM_NCHITTEST, 
							IntPtr.Zero, msg.lParam).ToInt32 ();
						NativeWindow.WndProc(hwnd.ClientWindow, Msg.WM_SETCURSOR, msg.hwnd, ht);
					}
				}

				msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
				msg.wParam = driver.GetMousewParam (0);
				break;

			case NSEventType.ScrollWheel:
				int delta = (int) ( eventref.DeltaY * 40.0);
				if (0 == delta || IntPtr.Zero == XplatUICocoa.FocusWindow)
					return;

				msg.hwnd = XplatUICocoa.FocusWindow;
				msg.message = Msg.WM_MOUSEWHEEL;
				msg.wParam = driver.GetMousewParam (delta);
				break;

			case NSEventType.MouseEntered:
				msg.message = Msg.WM_MOUSE_ENTER;
				break;

			case NSEventType.MouseExited:
				msg.message = Msg.WM_MOUSELEAVE;
				break;
			
			case NSEventType.TabletPoint:
			case NSEventType.TabletProximity:
			default:
				return;
			}

			driver.EnqueueMessage(msg);
		}

		private void UpdateMouseState (int button, bool down)
		{
			switch (button) {
			case (int) NSMouseButtons.None:
				break;
			case (int) NSMouseButtons.Left:
				if (down) XplatUICocoa.MouseState |= MouseButtons.Left;
				else XplatUICocoa.MouseState &= ~MouseButtons.Left;
				break;
			case (int) NSMouseButtons.Right:
				if (down) XplatUICocoa.MouseState |= MouseButtons.Right;
				else XplatUICocoa.MouseState &= ~MouseButtons.Right;
				break;
			case (int) NSMouseButtons.Middle:
				if (down) XplatUICocoa.MouseState |= MouseButtons.Middle;
				else XplatUICocoa.MouseState &= ~MouseButtons.Middle;
				break;
			case (int) NSMouseButtons.X:
				if (down) XplatUICocoa.MouseState |= MouseButtons.XButton1;
				else XplatUICocoa.MouseState &= ~MouseButtons.XButton1;
				break;
			default:
				if (down) XplatUICocoa.MouseState |= MouseButtons.XButton2;
				else XplatUICocoa.MouseState &= ~MouseButtons.XButton2;
				break;
			}
		}

		#endregion

		#region Keyboard

		public override void KeyDown (NSEvent theEvent)
		{
			ProcessKeyPress (theEvent, Msg.WM_KEYDOWN);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			ProcessKeyPress (theEvent, Msg.WM_KEYUP);
		}

		public override void FlagsChanged (NSEvent theEvent)
		{
			ProcessModifiers (theEvent);
		}

		public void ProcessKeyPress (NSEvent eventref, Msg msg)
		{
			Hwnd hwnd = Hwnd.ObjectFromWindow (Handle);
			ushort charCode = 0x0;
			byte keyCode = 0x0;

			string chars = eventref.CharactersIgnoringModifiers;
			if (chars.Length > 0)
				charCode = chars[0];

			keyCode = (byte) eventref.KeyCode;

			Keys key;
			IntPtr lParam = (IntPtr) (byte)charCode;
			IntPtr wParam;
			if (keyNames.TryGetValue ((NSKey)charCode, out key))
				wParam = (IntPtr) key;
			else
				wParam = charCode == 0x10 ? (IntPtr) key_translation_table [keyCode] : (IntPtr) char_translation_table [(byte)charCode];
			driver.PostMessage (hwnd.Handle, msg, wParam, lParam);
		}

		public void ProcessModifiers (NSEvent eventref)
		{
			Hwnd hwnd = Hwnd.ObjectFromWindow (Handle);
			// we get notified when modifiers change, but not specifically what changed
			NSEventModifierMask modifiers = eventref.ModifierFlags;
			NSEventModifierMask diff = modifiers ^ XplatUICocoa.key_modifiers;

			if ((NSEventModifierMask.ShiftKeyMask & diff) != 0) {
				driver.PostMessage (hwnd.Handle, (NSEventModifierMask.ShiftKeyMask & modifiers) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_SHIFT, IntPtr.Zero);
			} else if ((NSEventModifierMask.ControlKeyMask & diff) != 0) {
				driver.PostMessage (hwnd.Handle, (NSEventModifierMask.ShiftKeyMask & modifiers) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_CONTROL, IntPtr.Zero);
			} else if ((NSEventModifierMask.CommandKeyMask & diff) != 0) {
				driver.PostMessage (hwnd.Handle, (NSEventModifierMask.ShiftKeyMask & modifiers) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_MENU, IntPtr.Zero);
			}
			XplatUICocoa.key_modifiers = modifiers;
		}

		#region Keyboard translation tables

		private static byte [] key_translation_table = new byte [256] {
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 
			16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 
			32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 
			48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 
			64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 
			80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 
			0x74, 0x75, 0x76, 0x72, 0x77, 0x78, 0x79, 103, 104, 105, 106, 107, 108, 109, 0x7a, 0x7b, 
			112, 113, 114, 115, 116, 117, 0x73, 119, 0x71, 121, 0x70, 123, 124, 125, 126, 127, 
			128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 
			144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 
			160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 
			176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 
			192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 
			208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 
			224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 
			240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255 
		};

		private static byte [] char_translation_table = new byte [256] {
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 
			16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 0x25, 0x27, 0x26, 0x28, 
			32, 49, 34, 51, 52, 53, 55, 222, 57, 48, 56, 187, 188, 189, 190, 191, 
			48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 186, 60, 61, 62, 63, 
			50, 65, 66, 67, 68, 187, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 
			80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 219, 220, 221, 54, 189, 
			192, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 
			80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 123, 124, 125, 126, 0x2e, 
			128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 
			144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 
			160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 
			176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 
			192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 
			208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 
			224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 
			240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
		};

		private static Dictionary<NSKey, Keys> keyNames;

		static MonoView () {
			keyNames = new Dictionary<NSKey, Keys> ();
			keyNames.Add (NSKey.Backslash, Keys.OemBackslash);
			keyNames.Add (NSKey.CapsLock, Keys.Capital);
			keyNames.Add (NSKey.Comma, Keys.Oemcomma);
			keyNames.Add (NSKey.Command, Keys.LWin);
			keyNames.Add (NSKey.Delete, Keys.Back);
			keyNames.Add (NSKey.DownArrow, Keys.Down);
			keyNames.Add (NSKey.Equal, Keys.Oemplus);
			keyNames.Add (NSKey.ForwardDelete, Keys.Delete);
			keyNames.Add (NSKey.Keypad0, Keys.D0);
			keyNames.Add (NSKey.Keypad1, Keys.D1);
			keyNames.Add (NSKey.Keypad2, Keys.D2);
			keyNames.Add (NSKey.Keypad3, Keys.D3);
			keyNames.Add (NSKey.Keypad4, Keys.D4);
			keyNames.Add (NSKey.Keypad5, Keys.D5);
			keyNames.Add (NSKey.Keypad6, Keys.D6);
			keyNames.Add (NSKey.Keypad7, Keys.D7);
			keyNames.Add (NSKey.Keypad8, Keys.D8);
			keyNames.Add (NSKey.Keypad9, Keys.D9);
			keyNames.Add (NSKey.KeypadDecimal, Keys.Decimal);
			keyNames.Add (NSKey.KeypadDivide, Keys.Divide);
			keyNames.Add (NSKey.KeypadEnter, Keys.Enter);
			keyNames.Add (NSKey.KeypadEquals, Keys.Oemplus);
			keyNames.Add (NSKey.KeypadMinus, Keys.OemMinus);
			keyNames.Add (NSKey.KeypadMultiply, Keys.Multiply);
			keyNames.Add (NSKey.KeypadPlus, Keys.Oemplus | Keys.Shift);
			keyNames.Add (NSKey.LeftArrow, Keys.Left);
			keyNames.Add (NSKey.LeftBracket, Keys.OemOpenBrackets);
			keyNames.Add (NSKey.Minus, Keys.OemMinus);
			keyNames.Add (NSKey.Mute, Keys.VolumeMute);
			keyNames.Add (NSKey.Next, Keys.MediaNextTrack);
			keyNames.Add (NSKey.Option, Keys.Alt);
			keyNames.Add (NSKey.Pause, Keys.MediaPlayPause);
			keyNames.Add (NSKey.Prev, Keys.MediaPreviousTrack);
			keyNames.Add (NSKey.Quote, Keys.OemQuotes);
			keyNames.Add (NSKey.RightArrow, Keys.Right);
			keyNames.Add (NSKey.RightBracket, Keys.OemCloseBrackets);
			keyNames.Add (NSKey.RightControl, Keys.RControlKey);
			keyNames.Add (NSKey.RightOption, Keys.Alt);
			keyNames.Add (NSKey.RightShift, Keys.RShiftKey);
			keyNames.Add (NSKey.ScrollLock, Keys.Scroll);
			keyNames.Add (NSKey.Semicolon, Keys.OemSemicolon);
			keyNames.Add (NSKey.Slash, Keys.OemQuestion);
			keyNames.Add (NSKey.UpArrow, Keys.Up);
		}

		#endregion
		#endregion
	}
}

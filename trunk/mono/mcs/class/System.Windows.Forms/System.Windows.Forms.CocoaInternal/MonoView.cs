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
		XplatUICocoa driver;
		NSTrackingArea trackingArea;
		Hwnd hwnd;
		static Dictionary<Keys, Keys> nonchars;

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
				return Window != null && Window.ContentView.Handle == hwnd.WholeWindow ? Window.IsOpaque : true;
			}
		}

		public override bool AcceptsFirstResponder ()
		{
			return hwnd.Enabled;
		}

		public override bool BecomeFirstResponder ()
		{
			driver.SendMessage (hwnd.Handle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
			return base.BecomeFirstResponder ();
		}

		public override bool ResignFirstResponder ()
		{
			driver.SendMessage (hwnd.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
			return base.ResignFirstResponder ();
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

		public override void ViewDidMoveToWindow ()
		{
			UpdateTrackingAreas ();
			base.ViewDidMoveToWindow ();
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null)
				RemoveTrackingArea (trackingArea);	
			trackingArea = new NSTrackingArea (
				Frame,
				NSTrackingAreaOptions.ActiveInKeyWindow |
				NSTrackingAreaOptions.MouseMoved |
				NSTrackingAreaOptions.InVisibleRect,
				this,
				new NSDictionary());
			AddTrackingArea (trackingArea);
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
				Hwnd contentHwnd = Hwnd.ObjectFromWindow (Handle);
				if (null == contentHwnd || contentHwnd.zombie)
					return;
			} else {
				driver.mouse_position = driver.NativeToMonoScreen (globalNSPoint);
			}

			// FIXME: There has to be a better way. Obscured windows are still receiving MouseMoved events.
			var windowNumber = NSWindow.WindowNumberAtPoint (
				globalNSPoint,
				0);
			if (windowNumber != winWrap.WindowNumber)
				return;

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
				int delta = (int)(eventref.DeltaY * 40.0);
				MonoView focusedView = winWrap.FirstResponder as MonoView;
				if (0 == delta || focusedView == null)
					return;

				msg.hwnd = focusedView.Handle;
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

		public void ProcessKeyPress (NSEvent e, Msg msg)
		{
			ushort charCode = 0x0;
			char c = '\0';

			var chars = msg == Msg.WM_KEYDOWN ? GetCharactersForKeyPress (e) : "";
			Debug.WriteLine ("keyCode={0}, characters=\"{1}\"", e.KeyCode, chars);

			chars = chars ?? e.Characters;
			if (chars.Length > 0) {
				c = chars [0];
				charCode = chars [0];
			}

			byte keyCode = (byte) e.KeyCode;

			Keys key = GetKeys (e);
			IntPtr wParam = (IntPtr) key;
			IntPtr lParam = (IntPtr) (byte)charCode;
			// Test
			if (key == Keys.ShiftKey)
				lParam = (IntPtr)0x2a0001;

			Hwnd hwnd = Hwnd.ObjectFromWindow (Handle);
			driver.PostMessage (hwnd.Handle, msg, wParam, lParam);

			if (msg == Msg.WM_KEYDOWN && !string.IsNullOrEmpty (chars)) {

				if (IsChar (c, key)) {
					XplatUICocoa.PushChars (chars);
					driver.PostMessage (hwnd.Handle, Msg.WM_IME_COMPOSITION, IntPtr.Zero, IntPtr.Zero);
				}
			}
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

		private static Dictionary<NSKey, Keys> keyNames;
		private static Dictionary<string, Keys> modifiers;


		static MonoView () {
			keyNames = new Dictionary<NSKey, Keys> ();
			modifiers = new Dictionary<string, Keys> ();

			keyNames.Add (NSKey.Backslash, Keys.OemBackslash);
			keyNames.Add (NSKey.CapsLock, Keys.CapsLock);
			keyNames.Add (NSKey.Comma, Keys.Oemcomma);
			keyNames.Add (NSKey.Command, Keys.LWin);
			keyNames.Add (NSKey.Delete, Keys.Back);
			keyNames.Add (NSKey.DownArrow, Keys.Down);
			keyNames.Add (NSKey.Equal, Keys.Oemplus); // Should be "="
			keyNames.Add (NSKey.ForwardDelete, Keys.Delete);
			keyNames.Add (NSKey.Keypad0, Keys.NumPad0);
			keyNames.Add (NSKey.Keypad1, Keys.NumPad1);
			keyNames.Add (NSKey.Keypad2, Keys.NumPad2);
			keyNames.Add (NSKey.Keypad3, Keys.NumPad3);
			keyNames.Add (NSKey.Keypad4, Keys.NumPad4);
			keyNames.Add (NSKey.Keypad5, Keys.NumPad5);
			keyNames.Add (NSKey.Keypad6, Keys.NumPad6);
			keyNames.Add (NSKey.Keypad7, Keys.NumPad7);
			keyNames.Add (NSKey.Keypad8, Keys.NumPad8);
			keyNames.Add (NSKey.Keypad9, Keys.NumPad9);
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
			keyNames.Add (NSKey.Period, Keys.OemPeriod);
			keyNames.Add (NSKey.Return, Keys.Enter);
			keyNames.Add (NSKey.Grave, Keys.Oemtilde);

			// Modifiers
			modifiers.Add ("524576", Keys.Alt); //LeftAlt);
			modifiers.Add ("65792", Keys.CapsLock);			
			modifiers.Add ("524608", Keys.LWin);// .LeftWindows);
			modifiers.Add ("262401", Keys.LControlKey); //LeftControl);
			modifiers.Add ("131332", Keys.RShiftKey);// RightShift);
			modifiers.Add ("131330", Keys.LShiftKey);// LeftShift);
			modifiers.Add ("655650", Keys.RShiftKey);
		}

		internal static bool IsChar(char c, Keys k)
		{
			return c == '\b' || !char.IsControl (c) && !NonChars.ContainsKey (k);
		}

		public static Keys GetKeys (NSEvent theEvent)
		{
			var nskey = (NSKey)Enum.ToObject (typeof(NSKey), theEvent.KeyCode);
			if ((theEvent.ModifierFlags & NSEventModifierMask.FunctionKeyMask) > 0) {
				var chars = theEvent.Characters.ToCharArray ();
				var thekey = chars [0];
				if (theEvent.KeyCode != (char)NSKey.ForwardDelete)
					nskey = (NSKey)Enum.ToObject (typeof(NSKey), thekey);
			}

			Keys key;
			if (keyNames.TryGetValue(nskey,out key))
				return key;

			if (Enum.TryParse<Keys>(nskey.ToString(), out key))
				return key;

			return Keys.None;	
		}

		static Dictionary<Keys, Keys> NonChars {
			get {
				if (nonchars == null) {
					nonchars = new Dictionary<Keys, Keys> ();
					foreach (var key in noncharsArray)
						nonchars [key] = key;
				}
				return nonchars;
			}
		}

		static Keys[] noncharsArray = {
			Keys.None,
			Keys.LButton,		
			Keys.RButton,		
			Keys.Cancel,		
			Keys.MButton,		
			Keys.XButton1,	
			Keys.XButton2,	
			//Keys.Back,		
//			Keys.Tab,
			//Keys.LineFeed,
			Keys.Clear,
			Keys.Return,		
			Keys.Enter,
			Keys.ShiftKey,
			Keys.ControlKey,
			Keys.Menu,
			Keys.Pause,
			Keys.CapsLock,
			Keys.Capital,	
			Keys.KanaMode,	
//			Keys.HanguelMoe,
//			Keys.HangulMod,
			Keys.JunjaMode,
			Keys.FinalMode,
			Keys.KanjiMode,
			Keys.HanjaMode,
			Keys.Escape,
			Keys.IMEConvert,
			Keys.IMENonconvert,
			Keys.IMEAceept,
			Keys.IMEModeChange,
//			Keys.Space,
			Keys.PageUp,
			Keys.Prior,
			Keys.PageDown,
			Keys.Next,
			Keys.End,
			Keys.Home,
			Keys.Left,
			Keys.Up,
			Keys.Right,
			Keys.Down,
			Keys.Select,
			Keys.Print,
			Keys.Execute,
			Keys.PrintScreen,
			Keys.Snapshot,
			Keys.Insert,
			Keys.Delete,
			Keys.Help,
//			Keys.D0,
//			Keys.D1,
//			Keys.D2,
//			Keys.D3,
//			Keys.D4,
//			Keys.D5,
//			Keys.D6,
//			Keys.D7,
//			Keys.D8,
//			Keys.D9,
//			Keys.A,
//			Keys.B,
//			Keys.C,
//			Keys.D,
//			Keys.E,
//			Keys.F,
//			Keys.G,
//			Keys.H,
//			Keys.I,
//			Keys.J,
//			Keys.K,
//			Keys.L,
//			Keys.M,
//			Keys.N,
//			Keys.O,
//			Keys.P,
//			Keys.Q,
//			Keys.R,
//			Keys.S,
//			Keys.T,
//			Keys.U,
//			Keys.V,
//			Keys.W,
//			Keys.X,
//			Keys.Y,
//			Keys.Z,
//			Keys.LWin,
//			Keys.RWin,
//			Keys.Apps,
//			Keys.NumPad0,
//			Keys.NumPad1,
//			Keys.NumPad2,
//			Keys.NumPad3,
//			Keys.NumPad4,
//			Keys.NumPad5,
//			Keys.NumPad6,
//			Keys.NumPad7,
//			Keys.NumPad8,
//			Keys.NumPad9,
//			Keys.Multiply,
//			Keys.Add,
//			Keys.Separator,
//			Keys.Subtract,
//			Keys.Decimal,
//			Keys.Divide,
			Keys.F1,
			Keys.F2,
			Keys.F3,
			Keys.F4,
			Keys.F5,
			Keys.F6,
			Keys.F7,
			Keys.F8,
			Keys.F9,
			Keys.F10,
			Keys.F11,
			Keys.F12,
			Keys.F13,
			Keys.F14,
			Keys.F15,
			Keys.F16,
			Keys.F17,
			Keys.F18,
			Keys.F19,
			Keys.F20,
			Keys.F21,
			Keys.F22,
			Keys.F23,
			Keys.F24,
			Keys.NumLock,
			Keys.Scroll,
			Keys.LShiftKey,
			Keys.RShiftKey,
			Keys.LControlKey,
			Keys.RControlKey,
			Keys.LMenu,
			Keys.RMenu,
			Keys.BrowserBack,
			Keys.BrowserForward,
			Keys.BrowserRefresh,
			Keys.BrowserStop,
			Keys.BrowserSearch,
			Keys.BrowserFavorites,
			Keys.BrowserHome,
			Keys.VolumeMute,
			Keys.VolumeDown,
			Keys.VolumeUp,
			Keys.MediaNextTrack,
			Keys.MediaPreviousTrack,
			Keys.MediaStop,
			Keys.MediaPlayPause,
			Keys.LaunchMail,
			Keys.SelectMedia,
			Keys.LaunchApplication1,
			Keys.LaunchApplication2,
//			Keys.OemSemicolon,
//			Keys.Oemplus,
//			Keys.Oemcomma,
//			Keys.OemMinus,
//			Keys.OemPeriod,
//			Keys.OemQuestion,
//			Keys.Oemtilde,
//			Keys.OemOpenBrackets,
//			Keys.OemPipe,
//			Keys.OemCloseBrackets,
//			Keys.OemQuotes,
//			Keys.Oem8,
//			Keys.OemBackslash,
			Keys.ProcessKey,
			Keys.Attn,
			Keys.Crsel,
			Keys.Exsel,
			Keys.EraseEof,
			Keys.Play,
			Keys.Zoom,
			Keys.NoName,
			Keys.Pa1,
			Keys.OemClear,
			Keys.KeyCode,
			Keys.Shift,
			Keys.Control,
			Keys.Alt,
			Keys.Modifiers,
			Keys.IMEAccept,
//			Keys.Oem1,
//			Keys.Oem102,
//			Keys.Oem2,
//			Keys.Oem3,
//			Keys.Oem4,
//			Keys.Oem5,
//			Keys.Oem6,
//			Keys.Oem7,
			Keys.Packet,
			Keys.Sleep
		};		
		
		#endregion // Keyboard translation tables

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr TISCopyCurrentKeyboardInputSource ();

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr TISGetInputSourceProperty(IntPtr inputSource, IntPtr propertyKey);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static Int32 UCKeyTranslate(
			IntPtr keyLayoutPtr,
			UInt16 virtualKeyCode,
			UInt16 keyAction,
			UInt32 modifierKeyState,
			UInt32 keyboardType,
			UInt32 keyTranslateOptions,
			ref UInt32 deadKeyState,
			UInt32 maxStringLength,
			ref UInt32 actualStringLength,
			IntPtr unicodeString);

		const UInt16 kUCKeyActionDown = 0;
		const UInt16 kUCKeyActionUp = 1;
		const UInt16 kUCKeyActionAutoKey = 2;
		const UInt16 kUCKeyActionDisplay = 3;

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern byte LMGetKbdType();
		//static extern static Int32 KBGetLayoutType(Int16 iKeyboardType);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr __CFStringMakeConstantString (string cString);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern IntPtr CFDataCreate (IntPtr allocator, ref IntPtr buf, Int32 length);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern IntPtr CFDataGetBytePtr (IntPtr data);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern IntPtr CFStringCreateWithCharacters(IntPtr alloc, IntPtr chars, long numChars);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int CFRelease (IntPtr ptr);

		const UInt16 kUCKeyTranslateNoDeadKeysBit = 0;
		static UInt32 deadKeyState = 0;

		// http://stackoverflow.com/questions/12547007/convert-key-code-into-key-equivalent-string
		string GetCharactersForKeyPress(NSEvent e)
		{
			var currentKeyboard = TISCopyCurrentKeyboardInputSource();
			var layoutData = TISGetInputSourceProperty(currentKeyboard, __CFStringMakeConstantString("TISPropertyUnicodeKeyLayoutData"));
			var keyboardLayout = CFDataGetBytePtr (layoutData);

			var flags = (uint)e.ModifierFlags;
			var modifierKeyState = (flags >> 16) & 0xFF;
			var chars = new char[4];
			UInt32 realLength = 0;

			int size = sizeof(UInt16) * chars.Length;
			IntPtr buffer = IntPtr.Zero;

			try
			{
				buffer = Marshal.AllocCoTaskMem (size);
				UCKeyTranslate(
					keyboardLayout,
					e.KeyCode,
					kUCKeyActionDown,
					modifierKeyState,
					LMGetKbdType(),
					0,
					ref deadKeyState,
					(uint)chars.Length,
					ref realLength,
					buffer);

				if (realLength != 0)
					Marshal.Copy(buffer, chars, 0, chars.Length);

				Debug.WriteLine("DeadKeyState = {0:X8}", deadKeyState);
				return new string(chars, 0, (int)realLength);
			}
			finally
			{
				if (buffer != IntPtr.Zero)
					Marshal.FreeCoTaskMem (buffer);

				CFRelease(currentKeyboard);
			}
		}

		#endregion // Keyboard
	}
}

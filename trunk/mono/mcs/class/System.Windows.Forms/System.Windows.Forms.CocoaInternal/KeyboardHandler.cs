//
//KeyboardHandler.cs
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

//
//This document was originally created as a copy of a document in 
//System.Windows.Forms.CarbonInternal and retains many features thereof.
//

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//  Geoff Norton (gnorton@novell.com)
//
//
using System;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Collections.Generic;

namespace System.Windows.Forms.CocoaInternal {
	internal class KeyboardHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventRawKeyDown = 1;
		internal const uint kEventRawKeyRepeat = 2;
		internal const uint kEventRawKeyUp = 3;
		internal const uint kEventRawKeyModifiersChanged = 4;
		internal const uint kEventHotKeyPressed = 5;
		internal const uint kEventHotKeyReleased = 6;

		internal const uint kEventTextInputUnicodeForKeyEvent = 2;
//		internal const uint kEventParamTextInputSendText = 1953723512;

//		internal const uint typeChar = 1413830740;
//		internal const uint typeUInt32 = 1835100014;
//		internal const uint typeUnicodeText = 1970567284;

//		internal static byte [] key_filter_table;
//		internal static byte [] key_modifier_table;
		internal static byte [] key_translation_table;
		internal static byte [] char_translation_table;
		private static Dictionary<NSKey, Keys> keyNames;

		internal static bool translate_modifier = false;
		internal static NSEventModifierMask key_modifiers = 0;

		static KeyboardHandler () {
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


			// our key filter table is a 256 byte array - if the corresponding byte 
			// is set the key should be filtered from WM_CHAR (apple pushes unicode events
			// for some keys which win32 only handles as KEYDOWN
			// currently filtered:
			//	fn+f* == 16
			//	left == 28
			// 	right == 29
			// 	up == 30
			//	down == 31
//			// Please update this list as well as the table as more keys are found
//			key_filter_table = new byte [256] {
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
//							};

			// our char translation table is a set of translations from mac char codes
			// to win32 vkey codes
			// most things map directly
			char_translation_table = new byte [256] {
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
			key_translation_table = new byte [256] {
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
			// the key modifier table is a state table of the possible modifier keys
			// apple currently only goes up to 1 << 14 keys, we've extended this to 32
			// bytes as thats the size that apple uses
//			key_modifier_table = new byte [32];
		}

		internal KeyboardHandler (XplatUICocoa driver) : base (driver) {}

//		private void ModifierToVirtualKey (int i, ref MSG msg, bool down) {
//			msg.hwnd = XplatUICocoa.FocusWindow;
//
//			if (i == 9 || i == 13) {
//				msg.message = (down ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);
//				msg.wParam = (IntPtr) VirtualKeys.VK_SHIFT;
//				msg.lParam = IntPtr.Zero;
//				return;
//			}
//			if (i == 12 || i == 14) {
//				msg.message = (down ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);
//				msg.wParam = (IntPtr) VirtualKeys.VK_CONTROL;
//				msg.lParam = IntPtr.Zero;
//				return;
//			}
//			if (i == 8) {
//				msg.message = (down ? Msg.WM_SYSKEYDOWN : Msg.WM_SYSKEYUP);
//				msg.wParam = (IntPtr) VirtualKeys.VK_MENU;
//				msg.lParam = new IntPtr (0x20000000);
//				return;
//			}
//			
//			return;
//		}

		public EventHandledBy ProcessModifiers (NSEvent eventref, ref MSG msg)
		{
			// we get notified when modifiers change, but not specifically what changed
			NSEventModifierMask modifiers = eventref.ModifierFlags;
			NSEventModifierMask diff = modifiers ^ key_modifiers;
			EventHandledBy result = EventHandledBy.NativeOS;

			if ((NSEventModifierMask.ShiftKeyMask & diff) != 0) {
				msg.message = (NSEventModifierMask.ShiftKeyMask & modifiers) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP;
				msg.wParam = (IntPtr) VirtualKeys.VK_SHIFT;
				msg.lParam = IntPtr.Zero;
				result |= EventHandledBy.PostMessage;
			} else if ((NSEventModifierMask.ControlKeyMask & diff) != 0) {
				msg.message = (NSEventModifierMask.ControlKeyMask & modifiers) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP;
				msg.wParam = (IntPtr) VirtualKeys.VK_CONTROL;
				msg.lParam = IntPtr.Zero;
				result |= EventHandledBy.PostMessage;
			} else if ((NSEventModifierMask.CommandKeyMask & diff) != 0) {
				msg.message = (NSEventModifierMask.CommandKeyMask & modifiers) != 0 ? Msg.WM_SYSKEYDOWN : Msg.WM_SYSKEYUP;
				msg.wParam = (IntPtr) VirtualKeys.VK_MENU;
				msg.lParam = new IntPtr (0x20000000);
				result |= EventHandledBy.PostMessage;
			}

			key_modifiers = modifiers;

			return result;
		}

//		public void ProcessText (IntPtr eventref, ref MSG msg) {
//			UInt32 size = 0;
//			IntPtr buffer = IntPtr.Zero;
//			byte [] bdata;
//
//			// get the size of the unicode buffer
//			GetEventParameter (eventref, kEventParamTextInputSendText, typeUnicodeText, IntPtr.Zero, 0, ref size, IntPtr.Zero);
//
//			buffer = Marshal.AllocHGlobal ((int) size);
//			bdata = new byte [size];
//
//			// get the actual text buffer
//			GetEventParameter (eventref, kEventParamTextInputSendText, typeUnicodeText, IntPtr.Zero, size, IntPtr.Zero, buffer);
//
//			Marshal.Copy (buffer, bdata, 0, (int) size);
//			Marshal.FreeHGlobal (buffer);
//
//			if (key_filter_table [bdata [0]] == 0x00) {
//				// TODO: We support 2byte/4byte unicode? how does this get packed
//				msg.message = Msg.WM_CHAR;
//				msg.wParam = BitConverter.IsLittleEndian ? (IntPtr) bdata [0] : (IntPtr) bdata [size-1];
//				msg.lParam = IntPtr.Zero;
//				msg.hwnd = XplatUICocoa.FocusWindow;
//			}
//		}

		public void ProcessKeyPress (NSEvent eventref, ref MSG msg) {
			ushort charCode = 0x0;
			byte keyCode = 0x0;

			string chars = eventref.CharactersIgnoringModifiers;
			if (chars.Length > 0)
				charCode = chars[0];

			keyCode = (byte) eventref.KeyCode;

			Keys key;
			msg.lParam = (IntPtr) (byte)charCode;
			if (keyNames.TryGetValue ((NSKey)charCode, out key))
				msg.wParam = (IntPtr) key;
			else
				msg.wParam = charCode == 0x10 ? (IntPtr) key_translation_table [keyCode] : (IntPtr) char_translation_table [(byte)charCode];
			msg.hwnd = XplatUICocoa.FocusWindow;
		}

		public EventHandledBy ProcessEvent (NSObject callref, NSEvent eventref, MonoView handle, uint kind, ref MSG msg)
		{
//			uint klass = EventHandler.GetEventClass (eventref);
			EventHandledBy result = EventHandledBy.PostMessage;

//			if (klass == EventHandler.kEventClassTextInput) {
//				switch (kind) {
//				case kEventTextInputUnicodeForKeyEvent:
//					ProcessText (eventref, ref msg);
//					break;
//				default:
//					Console.WriteLine ("WARNING: KeyboardHandler.ProcessEvent default handler for kEventClassTextInput should not be reached");
//					break;
//				}
//			} else if (klass == EventHandler.kEventClassKeyboard) {
				switch (eventref.Type) {
				case NSEventType.KeyDown:
//					case kEventRawKeyRepeat:
					msg.message = Msg.WM_KEYDOWN;
					ProcessKeyPress (eventref, ref msg);
					break;

				case NSEventType.KeyUp:
					msg.message = Msg.WM_KEYUP;
					ProcessKeyPress (eventref, ref msg);
					break;

				case NSEventType.FlagsChanged:
					result = ProcessModifiers (eventref, ref msg);
					break;

				default:
					Console.WriteLine ("WARNING: KeyboardHandler.ProcessEvent default handler for " +
						"kEventClassKeyboard should not be reached");
					result = EventHandledBy.None;
					break;
				}
//			} else {
//				Console.WriteLine ("WARNING: KeyboardHandler.ProcessEvent default handler for kEventClassTextInput should not be reached");
//			}

			return result;
		}

		public bool TranslateMessage (ref MSG msg) {
			bool res = false;
 
			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;
			
			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN && msg.message != Msg.WM_KEYUP && msg.message != Msg.WM_SYSKEYUP && msg.message != Msg.WM_CHAR && msg.message != Msg.WM_SYSCHAR) 
				return res;

			if (0 != (NSEventModifierMask.CommandKeyMask & key_modifiers) && 0 == (NSEventModifierMask.ControlKeyMask & key_modifiers)) {
				if (msg.message == Msg.WM_KEYDOWN) {
					msg.message = Msg.WM_SYSKEYDOWN;
				} else if (msg.message == Msg.WM_CHAR) {
					msg.message = Msg.WM_SYSCHAR;
					translate_modifier = true;
				} else if (msg.message == Msg.WM_KEYUP) {
					msg.message = Msg.WM_SYSKEYUP;
				} else {
					return res;
				}

				msg.lParam = new IntPtr (0x20000000);
			} else if (msg.message == Msg.WM_SYSKEYUP && translate_modifier && msg.wParam == (IntPtr) 18) {
				msg.message = Msg.WM_KEYUP;
				
				msg.lParam = IntPtr.Zero;
				translate_modifier = false;
			}

			return res;
		}

		internal Keys ModifierKeys {
			get {
				Keys keys = Keys.None;
				if ((NSEventModifierMask.ShiftKeyMask & key_modifiers) != 0)        { keys |= Keys.Shift; }
				if ((NSEventModifierMask.CommandKeyMask & key_modifiers) != 0)      { keys |= Keys.Alt; }
				if ((NSEventModifierMask.ControlKeyMask & key_modifiers) != 0)      { keys |= Keys.Control; }
				return keys;
			}
		}
	}

//	internal enum KeyboardModifiers : uint {
//		activeFlag = 1 << 0,
//		btnState = 1 << 7,
//		cmdKey = 1 << 8,
//		shiftKey = 1 << 9,
//		alphaLock = 1 << 10,
//		optionKey = 1 << 11,
//		controlKey = 1 << 12,
//		rightShiftKey = 1 << 13,
//		rightOptionKey = 1 << 14,
//		rightControlKey = 1 << 14,
//	}
}

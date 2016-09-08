using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using ObjCRuntime = MonoMac.ObjCRuntime;

#elif XAMARINMAC
using AppKit;
using CoreGraphics;
using Foundation;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	internal class KeysConverter
	{
		public KeysConverter()
		{
		}

		#region Keyboard translation tables

		private static Dictionary<NSKey, Keys> keyNames;
		private static Dictionary<string, Keys> modifiers;


		static KeysConverter()
		{
			keyNames = new Dictionary<NSKey, Keys>();
			modifiers = new Dictionary<string, Keys>();

			keyNames.Add(NSKey.Backslash, Keys.OemBackslash);
			keyNames.Add(NSKey.CapsLock, Keys.CapsLock);
			keyNames.Add(NSKey.Comma, Keys.Oemcomma);
			keyNames.Add(NSKey.Command, Keys.LWin);
			keyNames.Add(NSKey.Delete, Keys.Back);
			keyNames.Add(NSKey.DownArrow, Keys.Down);
			keyNames.Add(NSKey.Equal, Keys.Oemplus); // Should be "="
			keyNames.Add(NSKey.ForwardDelete, Keys.Delete);
			keyNames.Add(NSKey.Keypad0, Keys.NumPad0);
			keyNames.Add(NSKey.Keypad1, Keys.NumPad1);
			keyNames.Add(NSKey.Keypad2, Keys.NumPad2);
			keyNames.Add(NSKey.Keypad3, Keys.NumPad3);
			keyNames.Add(NSKey.Keypad4, Keys.NumPad4);
			keyNames.Add(NSKey.Keypad5, Keys.NumPad5);
			keyNames.Add(NSKey.Keypad6, Keys.NumPad6);
			keyNames.Add(NSKey.Keypad7, Keys.NumPad7);
			keyNames.Add(NSKey.Keypad8, Keys.NumPad8);
			keyNames.Add(NSKey.Keypad9, Keys.NumPad9);
			keyNames.Add(NSKey.KeypadDecimal, Keys.Decimal);
			keyNames.Add(NSKey.KeypadDivide, Keys.Divide);
			keyNames.Add(NSKey.KeypadEnter, Keys.Enter);
			keyNames.Add(NSKey.KeypadEquals, Keys.Oemplus);
			keyNames.Add(NSKey.KeypadMinus, Keys.OemMinus);
			keyNames.Add(NSKey.KeypadMultiply, Keys.Multiply);
			keyNames.Add(NSKey.KeypadPlus, Keys.Oemplus | Keys.Shift);
			keyNames.Add(NSKey.LeftArrow, Keys.Left);
			keyNames.Add(NSKey.LeftBracket, Keys.OemOpenBrackets);
			keyNames.Add(NSKey.Minus, Keys.OemMinus);
			keyNames.Add(NSKey.Mute, Keys.VolumeMute);
			keyNames.Add(NSKey.Option, Keys.Alt);
			keyNames.Add(NSKey.Quote, Keys.OemQuotes);
			keyNames.Add(NSKey.RightArrow, Keys.Right);
			keyNames.Add(NSKey.RightBracket, Keys.OemCloseBrackets);
			keyNames.Add(NSKey.RightControl, Keys.RControlKey);
			keyNames.Add(NSKey.RightOption, Keys.Alt);
			keyNames.Add(NSKey.RightShift, Keys.RShiftKey);
			keyNames.Add(NSKey.Semicolon, Keys.OemSemicolon);
			keyNames.Add(NSKey.Slash, Keys.OemQuestion);
			keyNames.Add(NSKey.UpArrow, Keys.Up);
			keyNames.Add(NSKey.Period, Keys.OemPeriod);
			keyNames.Add(NSKey.Return, Keys.Enter);
			keyNames.Add(NSKey.Grave, Keys.Oemtilde);

			//keyNames.Add(NSKey.Next, Keys.MediaNextTrack);
			//keyNames.Add(NSKey.Pause, Keys.MediaPlayPause);
			//keyNames.Add(NSKey.Prev, Keys.MediaPreviousTrack);
			//keyNames.Add(NSKey.ScrollLock, Keys.Scroll);

			// Modifiers
			modifiers.Add("524576", Keys.Alt); //LeftAlt);
			modifiers.Add("65792", Keys.CapsLock);
			modifiers.Add("524608", Keys.LWin);// .LeftWindows);
			modifiers.Add("262401", Keys.LControlKey); //LeftControl);
			modifiers.Add("131332", Keys.RShiftKey);// RightShift);
			modifiers.Add("131330", Keys.LShiftKey);// LeftShift);
			modifiers.Add("655650", Keys.RShiftKey);
		}

		internal static bool IsChar(char c, Keys k)
		{
			return c == '\b' || !char.IsControl(c) && !NonChars.ContainsKey(k);
		}

		public static Keys GetKeys(NSEvent theEvent)
		{
			var nskey = (NSKey)Enum.ToObject(typeof(NSKey), theEvent.KeyCode);
			if ((theEvent.ModifierFlags & NSEventModifierMask.FunctionKeyMask) > 0)
			{
				var chars = theEvent.Characters.ToCharArray();
				var thekey = chars[0];
				if (theEvent.KeyCode != (char)NSKey.ForwardDelete)
					nskey = (NSKey)Enum.ToObject(typeof(NSKey), thekey);
			}

			Keys key;
			if (keyNames.TryGetValue(nskey, out key))
				return key;

			if (Enum.TryParse<Keys>(nskey.ToString(), out key))
				return key;

			return Keys.None;
		}

		static Dictionary<Keys, Keys> nonchars;
		static Dictionary<Keys, Keys> NonChars
		{
			get
			{
				if (nonchars == null)
				{
					nonchars = new Dictionary<Keys, Keys>();
					foreach (var key in noncharsArray)
						nonchars[key] = key;
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

		const string Carbon = "/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon";

		[DllImport(Carbon)]
		extern static IntPtr TISCopyCurrentKeyboardInputSource();

		[DllImport(Carbon)]
		extern static IntPtr TISGetInputSourceProperty(IntPtr inputSource, IntPtr propertyKey);

		[DllImport(Carbon)]
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

		[DllImport(Carbon)]
		static extern byte LMGetKbdType();
		//static extern static Int32 KBGetLayoutType(Int16 iKeyboardType);

		[DllImport(Carbon)]
		internal extern static IntPtr __CFStringMakeConstantString(string cString);

		[DllImport(Carbon)]
		static extern IntPtr CFDataCreate(IntPtr allocator, ref IntPtr buf, Int32 length);

		[DllImport(Carbon)]
		static extern IntPtr CFDataGetBytePtr(IntPtr data);

		[DllImport(Carbon)]
		static extern IntPtr CFStringCreateWithCharacters(IntPtr alloc, IntPtr chars, long numChars);

		[DllImport(Carbon)]
		internal extern static int CFRelease(IntPtr ptr);

		const UInt16 kUCKeyTranslateNoDeadKeysBit = 0;
		static UInt32 deadKeyState = 0;

		// http://stackoverflow.com/questions/12547007/convert-key-code-into-key-equivalent-string
		public static string GetCharactersForKeyPress(NSEvent e)
		{
			var currentKeyboard = TISCopyCurrentKeyboardInputSource();
			var layoutData = TISGetInputSourceProperty(currentKeyboard, __CFStringMakeConstantString("TISPropertyUnicodeKeyLayoutData"));
			var keyboardLayout = CFDataGetBytePtr(layoutData);

			var flags = (uint)e.ModifierFlags;
			var modifierKeyState = (flags >> 16) & 0xFF;
			var chars = new char[4];
			UInt32 realLength = 0;

			int size = sizeof(UInt16) * chars.Length;
			IntPtr buffer = IntPtr.Zero;

			try
			{
				buffer = Marshal.AllocCoTaskMem(size);
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
					Marshal.FreeCoTaskMem(buffer);

				CFRelease(currentKeyboard);
			}
		}

		public static NSEvent ConvertKeyEvent(IntPtr hwnd, MSG msg)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd);
			var windowNumber = vuWrap?.Window?.WindowNumber ?? 0;

			var key = msg.wParam.ToInt32();
			var keyCode = NSKeyFromKeys((Keys)key);

			Debug.WriteLine("Sending key: " + keyCode);

			NSEvent e = null;
			switch (msg.message)
			{
				case Msg.WM_KEYDOWN:
					e = NSEvent.KeyEvent(NSEventType.KeyDown, CGPoint.Empty, (NSEventModifierMask)0, NSDate.Now.SecondsSinceReferenceDate, windowNumber, null, "", "", false, (ushort)keyCode);
					break;
				case Msg.WM_KEYUP:
					e = NSEvent.KeyEvent(NSEventType.KeyUp, CGPoint.Empty, (NSEventModifierMask)0, NSDate.Now.SecondsSinceReferenceDate, windowNumber, null, "", "", false, (ushort)keyCode);
					break;
				default:
					break;
			}

			return e;
		}

		public static NSKey NSKeyFromKeys(Keys key)
		{
			//FIXME: Add support for all key, replace switch statement with some look-up table.
			switch (key)
			{
				case Keys.Escape: return NSKey.Escape;
				case Keys.Enter: return NSKey.Return;
				case Keys.Space: return NSKey.Space;
				case Keys.F4: return NSKey.F4;
				case Keys.Down: return NSKey.DownArrow;
				case Keys.Up: return NSKey.UpArrow;
			}

			Debug.WriteLine("NSKeyFromKeys not implemented for {0}", key);
			return (NSKey)(int)key;
		}
	}
}

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
		private static Dictionary<ulong, Keys> keys;

		static KeysConverter()
		{
			keys = new Dictionary<ulong, Keys>();

			keys.Add((ulong)NSKeyEx.GtLt, Keys.Oem7);

			keys.Add((ulong)NSKeyEx.Home, Keys.Home);
			keys.Add((ulong)NSKeyEx.End, Keys.End);
			keys.Add((ulong)NSKeyEx.PageUp, Keys.PageUp);
			keys.Add((ulong)NSKeyEx.PageDown, Keys.PageDown);
			keys.Add((ulong)NSKeyEx.LeftArrow, Keys.Left);
			keys.Add((ulong)NSKeyEx.RightArrow, Keys.Right);
			keys.Add((ulong)NSKeyEx.UpArrow, Keys.Up);
			keys.Add((ulong)NSKeyEx.DownArrow, Keys.Down);

			keys.Add((ulong)NSKey.Escape, Keys.Escape);
			keys.Add((ulong)NSKey.Tab, Keys.Tab);
			keys.Add((ulong)NSKey.CapsLock, Keys.CapsLock);

			keys.Add((ulong)NSKey.Delete, Keys.Back);
			//keys.Add((ulong)NSKey.Insert, Keys.Insert);
			keys.Add((ulong)NSKey.Help, Keys.Help);
			//keys.Add((ulong)NSKey.Pause, Keys.Pause);
			//keys.Add((ulong)NSKey.ScrollLock, Keys.Scroll);
			//keys.Add((ulong)NSKey.Print, Keys.Print);
			//keys.Add((ulong)NSKey.Execute, Keys.Execute);

			keys.Add((ulong)NSKey.Shift, Keys.ShiftKey);
			keys.Add((ulong)NSKey.Control, Keys.ControlKey);
			keys.Add((ulong)NSKey.Option, Keys.Menu);
			keys.Add((ulong)NSKey.Command, Keys.LWin);

			keys.Add((ulong)NSKey.KeypadClear, Keys.NumLock);
			keys.Add((ulong)NSKey.KeypadEquals, Keys.Oemplus);
			keys.Add((ulong)NSKey.ForwardDelete, Keys.Delete);
			keys.Add((ulong)NSKey.Backslash, Keys.OemBackslash);
			keys.Add((ulong)NSKey.Comma, Keys.Oemcomma);
			keys.Add((ulong)NSKey.Equal, Keys.Oemplus);

			keys.Add((ulong)NSKey.Keypad0, Keys.NumPad0);
			keys.Add((ulong)NSKey.Keypad1, Keys.NumPad1);
			keys.Add((ulong)NSKey.Keypad2, Keys.NumPad2);
			keys.Add((ulong)NSKey.Keypad3, Keys.NumPad3);
			keys.Add((ulong)NSKey.Keypad4, Keys.NumPad4);
			keys.Add((ulong)NSKey.Keypad5, Keys.NumPad5);
			keys.Add((ulong)NSKey.Keypad6, Keys.NumPad6);
			keys.Add((ulong)NSKey.Keypad7, Keys.NumPad7);
			keys.Add((ulong)NSKey.Keypad8, Keys.NumPad8);
			keys.Add((ulong)NSKey.Keypad9, Keys.NumPad9);
			keys.Add((ulong)NSKey.KeypadDecimal, Keys.Decimal);
			keys.Add((ulong)NSKey.KeypadEnter, Keys.Enter);
			keys.Add((ulong)NSKey.KeypadMinus, Keys.Subtract);
			keys.Add((ulong)NSKey.KeypadMultiply, Keys.Multiply);
			keys.Add((ulong)NSKey.KeypadPlus, Keys.Add);
			keys.Add((ulong)NSKey.KeypadDivide, Keys.Divide);

			keys.Add((ulong)NSKey.LeftBracket, Keys.OemOpenBrackets);
			keys.Add((ulong)NSKey.Minus, Keys.OemMinus);
			keys.Add((ulong)NSKey.Mute, Keys.VolumeMute);
			keys.Add((ulong)NSKey.Quote, Keys.OemQuotes);
			keys.Add((ulong)NSKey.RightBracket, Keys.OemCloseBrackets);
			keys.Add((ulong)NSKey.RightControl, Keys.RControlKey);
			keys.Add((ulong)NSKey.RightOption, Keys.Alt);
			keys.Add((ulong)NSKey.RightShift, Keys.RShiftKey);
			keys.Add((ulong)NSKey.Semicolon, Keys.OemSemicolon);
			keys.Add((ulong)NSKey.Slash, Keys.OemQuestion);
			keys.Add((ulong)NSKey.Period, Keys.OemPeriod);
			keys.Add((ulong)NSKey.Return, Keys.Enter);
			keys.Add((ulong)NSKey.Grave, Keys.Oemtilde);

			keys.Add((ulong)NSKeyEx.F1, Keys.F1);
			keys.Add((ulong)NSKeyEx.F2, Keys.F2);
			keys.Add((ulong)NSKeyEx.F3, Keys.F3);
			keys.Add((ulong)NSKeyEx.F4, Keys.F4);
			keys.Add((ulong)NSKeyEx.F5, Keys.F5);
			keys.Add((ulong)NSKeyEx.F6, Keys.F6);
			keys.Add((ulong)NSKeyEx.F7, Keys.F7);
			keys.Add((ulong)NSKeyEx.F8, Keys.F8);
			keys.Add((ulong)NSKeyEx.F9, Keys.F9);
			keys.Add((ulong)NSKeyEx.F10, Keys.F10);
			keys.Add((ulong)NSKeyEx.F11, Keys.F11);
			keys.Add((ulong)NSKeyEx.F12, Keys.F12);
			keys.Add((ulong)NSKeyEx.F13, Keys.F13);
			keys.Add((ulong)NSKeyEx.F14, Keys.F14);
			keys.Add((ulong)NSKeyEx.F15, Keys.F15);
			keys.Add((ulong)NSKeyEx.F16, Keys.F16);
			keys.Add((ulong)NSKeyEx.F17, Keys.F17);
			keys.Add((ulong)NSKeyEx.F18, Keys.F18);
			keys.Add((ulong)NSKeyEx.F19, Keys.F19);
			keys.Add((ulong)NSKeyEx.F20, Keys.F20);

			keys.Add((ulong)NSKey.D0, Keys.D0);
			keys.Add((ulong)NSKey.D1, Keys.D1);
			keys.Add((ulong)NSKey.D2, Keys.D2);
			keys.Add((ulong)NSKey.D3,Keys.D3);
			keys.Add((ulong)NSKey.D4,Keys.D4);
			keys.Add((ulong)NSKey.D5,Keys.D5);
			keys.Add((ulong)NSKey.D6,Keys.D6);
			keys.Add((ulong)NSKey.D7,Keys.D7);
			keys.Add((ulong)NSKey.D8,Keys.D8);
			keys.Add((ulong)NSKey.D9,Keys.D9);

			keys.Add((ulong)NSKey.Space, Keys.Space);

			keys.Add((ulong)NSKey.A,Keys.A);
			keys.Add((ulong)NSKey.B,Keys.B);
			keys.Add((ulong)NSKey.C,Keys.C);
			keys.Add((ulong)NSKey.D,Keys.D);
			keys.Add((ulong)NSKey.E,Keys.E);
			keys.Add((ulong)NSKey.F,Keys.F);
			keys.Add((ulong)NSKey.G,Keys.G);
			keys.Add((ulong)NSKey.H,Keys.H);
			keys.Add((ulong)NSKey.I,Keys.I);
			keys.Add((ulong)NSKey.J,Keys.J);
			keys.Add((ulong)NSKey.K,Keys.K);
			keys.Add((ulong)NSKey.L,Keys.L);
			keys.Add((ulong)NSKey.M,Keys.M);
			keys.Add((ulong)NSKey.N,Keys.N);
			keys.Add((ulong)NSKey.O,Keys.O);
			keys.Add((ulong)NSKey.P,Keys.P);
			keys.Add((ulong)NSKey.Q,Keys.Q);
			keys.Add((ulong)NSKey.R,Keys.R);
			keys.Add((ulong)NSKey.S,Keys.S);
			keys.Add((ulong)NSKey.T,Keys.T);
			keys.Add((ulong)NSKey.U,Keys.U);
			keys.Add((ulong)NSKey.V,Keys.V);
			keys.Add((ulong)NSKey.W,Keys.W);
			keys.Add((ulong)NSKey.X,Keys.X);
			keys.Add((ulong)NSKey.Y,Keys.Y);
			keys.Add((ulong)NSKey.Z,Keys.Z);
		}

		internal static bool IsChar(char c, Keys k)
		{
			return c == '\t' || c == '\b' || k == Keys.Back || k == Keys.Escape || !char.IsControl(c) && !NonChars.ContainsKey(k);
		}

		public static Keys GetKeys(NSEvent e)
		{
			Keys key;
			if (keys.TryGetValue(e.KeyCode, out key))
				return key;

			//DebugUtility.WriteLine("KeyCode '{0}' not found in keys dictionary", e.KeyCode);
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

					//DumpKeys();
				}
				return nonchars;
			}
		}

		static UInt32 deadKeyState = 0;
		public static UInt32 DeadKeyState
		{
			get { return deadKeyState; }
		}

		internal static void DumpKeys()
		{
			var keyboard = TISCopyCurrentKeyboardInputSource();

			var modifiers = new NSEventModifierMask[] {
				0,
				NSEventModifierMask.ShiftKeyMask,
				NSEventModifierMask.AlternateKeyMask,
				NSEventModifierMask.ShiftKeyMask | NSEventModifierMask.AlternateKeyMask
			};

			var deadKeyStates = new uint[] { 0, 1, 3 };

			foreach (var deadKeyState in deadKeyStates)
			{
				foreach (var flags in modifiers)
				{
					foreach (NSKey key in Enum.GetValues(typeof(NSKey)))
					{
						var deadKeyStateCopy = deadKeyState;
						string chars = GetCharactersForKeyPress((ushort)key, flags, ref deadKeyStateCopy, keyboard);

						bool deadKeyStateUsedOrZero = deadKeyState == (deadKeyStateCopy >> 16);
						bool isDeadKey = deadKeyState == deadKeyStateCopy && deadKeyState != 0;

						if (isDeadKey)
							Console.WriteLine($"DeadKey! \"{chars}\", NSKey.{key}, {(NSEventModifierMask)flags}, {deadKeyState}");

						if (!String.IsNullOrEmpty(chars) && deadKeyStateUsedOrZero)
							Console.WriteLine($"\"{chars}\", NSKey.{key}, {(NSEventModifierMask)flags}, {deadKeyState}");
					}
				}
			}

			CFRelease(keyboard);
		}

		// http://stackoverflow.com/questions/12547007/convert-key-code-into-key-equivalent-string
		public static string GetCharactersForKeyPress(NSEvent e)
		{
			return GetCharactersForKeyPress(e.KeyCode, e.ModifierFlags, ref deadKeyState);
		}

		public static string GetCharactersForKeyPress(ushort keyCode, NSEventModifierMask modifierFlags, ref uint deadKeyState)
		{
			var keyboard = TISCopyCurrentKeyboardLayoutInputSource();
			var characters = GetCharactersForKeyPress(keyCode, modifierFlags, ref deadKeyState, keyboard);
			CFRelease(keyboard);
			return characters;
		}

		public static string GetCharactersForKeyPress(ushort keyCode, NSEventModifierMask modifierFlags, ref uint deadKeyState, IntPtr keyboard)
		{
			var layoutData = TISGetInputSourceProperty(keyboard, __CFStringMakeConstantString("TISPropertyUnicodeKeyLayoutData"));
			var keyboardLayout = CFDataGetBytePtr(layoutData);

			var flags = (uint)modifierFlags;
			var modifierKeyState = (flags >> 16) & 0xFF;
			var chars = new char[4];
			UInt32 realLength = 0;

			int size = sizeof(UInt16) * chars.Length;
			IntPtr buffer = IntPtr.Zero;

			try
			{
				buffer = Marshal.AllocCoTaskMem(size);
				UCKeyTranslate(keyboardLayout, keyCode, kUCKeyActionDown, modifierKeyState, LMGetKbdType(), 0,
					ref deadKeyState, (uint)chars.Length, ref realLength, buffer);

				if (realLength != 0)
					Marshal.Copy(buffer, chars, 0, chars.Length);

				//Debug.WriteLine("DeadKeyState = {0:X8}", deadKeyState);
				return new string(chars, 0, (int)realLength);
			}
			finally
			{
				if (buffer != IntPtr.Zero)
					Marshal.FreeCoTaskMem(buffer);
			}
		}

		public static NSEvent ConvertKeyEvent(IntPtr hwnd, MSG msg, ref NSEventModifierMask flags)
		{
			NSView vuWrap = (NSView)ObjCRuntime.Runtime.GetNSObject(hwnd);
			var windowNumber = vuWrap?.Window?.WindowNumber ?? 0;

			var key = (Keys)msg.wParam.ToInt32();
			var keyCode = NSKeyFromKeys((Keys)key);
			if (!keyCode.HasValue)
				return null;

			bool down = msg.message == Msg.WM_KEYDOWN;
			if (down)
			{
				switch (key)
				{
					case Keys.ShiftKey: flags |= NSEventModifierMask.ShiftKeyMask; break;
					case Keys.ControlKey: flags |= NSEventModifierMask.ControlKeyMask; break;
					case Keys.Menu: flags |= NSEventModifierMask.AlternateKeyMask; break;
				}
			}
			else
			{
				switch (key)
				{
					case Keys.ShiftKey: flags &= ~NSEventModifierMask.ShiftKeyMask; break;
					case Keys.ControlKey: flags &= ~NSEventModifierMask.ControlKeyMask; break;
					case Keys.Menu: flags &= ~NSEventModifierMask.AlternateKeyMask; break;
				}
			}

			Debug.WriteLine($"Converting message: {msg.message}, {keyCode}");

			string keys = GetCharactersForKeyPress((ushort)keyCode.Value, flags, ref deadKeyState);

			uint dummy = 0;
			string keysIgnoringFlags = GetCharactersForKeyPress((ushort)keyCode.Value, 0, ref dummy);
				
			NSEvent e = null;
			switch (msg.message)
			{
				case Msg.WM_KEYDOWN:
					e = NSEvent.KeyEvent(NSEventType.KeyDown, CGPoint.Empty, flags, NSDate.Now.SecondsSinceReferenceDate, windowNumber, null, keys, keysIgnoringFlags, false, (ushort)keyCode.Value);
					break;
				case Msg.WM_KEYUP:
					e = NSEvent.KeyEvent(NSEventType.KeyUp, CGPoint.Empty, flags, NSDate.Now.SecondsSinceReferenceDate, windowNumber, null, keys, keysIgnoringFlags, false, (ushort)keyCode.Value);
					break;
				default:
					break;
			}

			return e;
		}

		public static NSKey? NSKeyFromKeys(Keys key)
		{
			//FIXME: Optimize using dictionary. Take modifiers into account.
			foreach (var entry in entries)
				if (entry.wfKey == key)
					return entry.nsKey;

			Debug.WriteLine("NSKeyFromKeys not implemented for {0}", key);
			return null;
		}

		#region Native methods

		const string Carbon = "/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon";

		[DllImport(Carbon)]
		extern static IntPtr TISCopyCurrentKeyboardInputSource();

		[DllImport(Carbon)]
		extern static IntPtr TISCopyCurrentKeyboardLayoutInputSource();

		[DllImport(Carbon)]
		extern static IntPtr TISGetInputSourceProperty(IntPtr inputSource, IntPtr propertyKey);

		[DllImport(Carbon)]
		extern static IntPtr TISCreateInputSourceList(IntPtr cfDictionary, bool b); // -> Unmanaged<CFArray>!

		[DllImport(Carbon)]
		extern static IntPtr TISSelectInputSource(IntPtr inputSource);

		[DllImport(Carbon)]
		extern static Int32 UCKeyTranslate(IntPtr keyLayoutPtr, UInt16 virtualKeyCode, UInt16 keyAction, UInt32 modifierKeyState, UInt32 keyboardType, UInt32 keyTranslateOptions, ref UInt32 deadKeyState, UInt32 maxStringLength, ref UInt32 actualStringLength, IntPtr unicodeString);

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

		#endregion Native methods

		#region Native constants

		const UInt16 kUCKeyActionDown = 0;
		const UInt16 kUCKeyActionUp = 1;
		const UInt16 kUCKeyActionAutoKey = 2;
		const UInt16 kUCKeyActionDisplay = 3;

		//https://developer.apple.com/library/content/releasenotes/Carbon/HIToolbox.html
		//kTISPropertyInputSourceCategory
		//kTISPropertyInputSourceType
		//kTISPropertyInputSourceIsASCIICapable
		//kTISPropertyInputSourceIsSelectable
		//kTISPropertyInputSourceIsEnabled
		//kTISPropertyInputSourceIsSelected
		//kTISPropertyInputSourceID
		//kTISPropertyBundleID
		//kTISPropertyInputModeID
		//kTISPropertyLocalizedName

		const UInt16 kUCKeyTranslateNoDeadKeysBit = 0;

		#endregion Native constants

		#region Keyboard translation tables

		static Keys[] noncharsArray = {
			Keys.None,
			Keys.LButton,
			Keys.RButton,
			Keys.Cancel,
			Keys.MButton,
			Keys.XButton1,
			Keys.XButton2,	
			//Keys.Back,		
			//Keys.Tab,
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
			//Keys.HanguelMoe,
			//Keys.HangulMod,
			Keys.JunjaMode,
			Keys.FinalMode,
			Keys.KanjiMode,
			Keys.HanjaMode,
			//Keys.Escape,
			Keys.IMEConvert,
			Keys.IMENonconvert,
			Keys.IMEAceept,
			Keys.IMEModeChange,
			//Keys.Space,
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
			//Keys.D0,
			//Keys.D1,
			//Keys.D2,
			//Keys.D3,
			//Keys.D4,
			//Keys.D5,
			//Keys.D6,
			//Keys.D7,
			//Keys.D8,
			//Keys.D9,
			//Keys.A,
			//Keys.B,
			//Keys.C,
			//Keys.D,
			//Keys.E,
			//Keys.F,
			//Keys.G,
			//Keys.H,
			//Keys.I,
			//Keys.J,
			//Keys.K,
			//Keys.L,
			//Keys.M,
			//Keys.N,
			//Keys.O,
			//Keys.P,
			//Keys.Q,
			//Keys.R,
			//Keys.S,
			//Keys.T,
			//Keys.U,
			//Keys.V,
			//Keys.W,
			//Keys.X,
			//Keys.Y,
			//Keys.Z,
			//Keys.LWin,
			//Keys.RWin,
			//Keys.Apps,
			//Keys.NumPad0,
			//Keys.NumPad1,
			//Keys.NumPad2,
			//Keys.NumPad3,
			//Keys.NumPad4,
			//Keys.NumPad5,
			//Keys.NumPad6,
			//Keys.NumPad7,
			//Keys.NumPad8,
			//Keys.NumPad9,
			//Keys.Multiply,
			//Keys.Add,
			//Keys.Separator,
			//Keys.Subtract,
			//Keys.Decimal,
			//Keys.Divide,
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
			//Keys.OemSemicolon,
			//Keys.Oemplus,
			//Keys.Oemcomma,
			//Keys.OemMinus,
			//Keys.OemPeriod,
			//Keys.OemQuestion,
			//Keys.Oemtilde,
			//Keys.OemOpenBrackets,
			//Keys.OemPipe,
			//Keys.OemCloseBrackets,
			//Keys.OemQuotes,
			//Keys.Oem8,
			//Keys.OemBackslash,
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
			//Keys.Oem1,
			//Keys.Oem102,
			//Keys.Oem2,
			//Keys.Oem3,
			//Keys.Oem4,
			//Keys.Oem5,
			//Keys.Oem6,
			//Keys.Oem7,
			Keys.Packet,
			Keys.Sleep
		};

		internal struct Entry
		{
			public NSKey nsKey;
			public Keys wfKey;
			public bool isChar;
		}

		internal class Entries : List<Entry>
		{
			public void Add(NSKey a, Keys b, bool c, string d)
			{
				Add(new Entry { nsKey = a, wfKey = b, isChar = c });
			}
		}

		static readonly Entries entries = new Entries
		{
			{ NSKey.Shift,			Keys.ShiftKey,		false, "SHIFT" },
			{ NSKey.RightShift,		Keys.RShiftKey,		false, "RSHIFT" },
			{ NSKey.Control,		Keys.ControlKey,	false, "CONTROL" },
			{ NSKey.RightControl,	Keys.RControlKey,	false, "RCONTROL" },
			{ NSKey.Option,			Keys.Menu,			false, "ALT" },

			{ NSKey.F1, Keys.F1, false, "F1" },
			{ NSKey.F2, Keys.F2, false, "F2" },
			{ NSKey.F3, Keys.F3, false, "F3" },
			{ NSKey.F4, Keys.F4, false, "F4" },
			{ NSKey.F5, Keys.F5, false, "F5" },
			{ NSKey.F6, Keys.F6, false, "F6" },
			{ NSKey.F7, Keys.F7, false, "F7" },
			{ NSKey.F8, Keys.F8, false, "F8" },
			{ NSKey.F9, Keys.F9, false, "F9" },
			{ NSKey.F10, Keys.F10, false, "F10" },
			{ NSKey.F11, Keys.F11, false, "F11" },
			{ NSKey.F12, Keys.F12, false, "F12" },
			{ NSKey.F13, Keys.F13, false, "F13" },
			{ NSKey.F14, Keys.F14, false, "F14" },
			{ NSKey.F15, Keys.F15, false, "F15" },
			{ NSKey.F16, Keys.F16, false, "F16" },
			//{ NSKey.F17, Keys.F17, false, "F17" },
			{ NSKey.F16 + 1, Keys.F17, false, "F17" }, // F17 is currently missing in Xamarin.Mac (9/19/2016).
			{ NSKey.F18, Keys.F18, false, "F18" },
			{ NSKey.F19, Keys.F19, false, "F19" },
			{ NSKey.F20, Keys.F20, false, "F20" },
			//{ NSKey.F21, Keys.F21, false, "F21" },
			//{ NSKey.F22, Keys.F22, false, "F22" },
			//{ NSKey.F23, Keys.F23, false, "F23" },
			//{ NSKey.F24, Keys.F24, false, "F24" },

			{ NSKey.D0, Keys.D0, false, "D0" },
			{ NSKey.D1, Keys.D1, false, "D1" },
			{ NSKey.D2, Keys.D2, false, "D2" },
			{ NSKey.D3, Keys.D3, false, "D3" },
			{ NSKey.D4, Keys.D4, false, "D4" },
			{ NSKey.D5, Keys.D5, false, "D5" },
			{ NSKey.D6, Keys.D6, false, "D6" },
			{ NSKey.D7, Keys.D7, false, "D7" },
			{ NSKey.D8, Keys.D8, false, "D8" },
			{ NSKey.D9, Keys.D9, false, "D9" },

			{ NSKey.A, Keys.A, true, "A" },
			{ NSKey.B, Keys.B, true, "B" },
			{ NSKey.C, Keys.C, true, "C" },
			{ NSKey.D, Keys.D, true, "D" },
			{ NSKey.E, Keys.E, true, "E" },
			{ NSKey.F, Keys.F, true, "F" },
			{ NSKey.G, Keys.G, true, "G" },
			{ NSKey.H, Keys.H, true, "H" },
			{ NSKey.I, Keys.I, true, "I" },
			{ NSKey.J, Keys.J, true, "J" },
			{ NSKey.K, Keys.K, true, "K" },
			{ NSKey.L, Keys.L, true, "L" },
			{ NSKey.M, Keys.M, true, "M" },
			{ NSKey.N, Keys.N, true, "N" },
			{ NSKey.O, Keys.O, true, "O" },
			{ NSKey.P, Keys.P, true, "P" },
			{ NSKey.Q, Keys.Q, true, "Q" },
			{ NSKey.R, Keys.R, true, "R" },
			{ NSKey.S, Keys.S, true, "S" },
			{ NSKey.T, Keys.T, true, "T" },
			{ NSKey.U, Keys.U, true, "U" },
			{ NSKey.V, Keys.V, true, "V" },
			{ NSKey.W, Keys.W, true, "W" },
			{ NSKey.X, Keys.X, true, "X" },
			{ NSKey.Y, Keys.Y, true, "Y" },
			{ NSKey.Z, Keys.Z, true, "Z" },

			{ NSKey.Backslash,		Keys.OemBackslash,		true, "\\" },
			{ NSKey.CapsLock,		Keys.CapsLock,			false, "CAPSLOCK" },
			{ NSKey.Comma,			Keys.Oemcomma,			true,  "," },
			{ NSKey.Command,		Keys.LWin,				false, "LWIN" },
			{ NSKey.Delete,			Keys.Back,				false, "BACK" },
			{ NSKey.DownArrow,		Keys.Down,				false, "DOWN" },
			{ NSKey.Equal,			Keys.Oemplus,			false, "=" }, // Should be "="

			// Keys.Oemplus + Ctrl + Alt ... '=' both on NumericPad and the keyboard

			{ NSKey.ForwardDelete,	Keys.Delete,			false, "DEL" },
                                                     	
			{ NSKey.Keypad0,		Keys.NumPad0,			false, "NUM0" },
			{ NSKey.Keypad1,		Keys.NumPad1,			false, "NUM1" },
			{ NSKey.Keypad2,		Keys.NumPad2,			false, "NUM2" },
			{ NSKey.Keypad3,		Keys.NumPad3,			false, "NUM3" },
			{ NSKey.Keypad4,		Keys.NumPad4,			false, "NUM4" },
			{ NSKey.Keypad5,		Keys.NumPad5,			false, "NUM5" },
			{ NSKey.Keypad6,		Keys.NumPad6,			false, "NUM6" },
			{ NSKey.Keypad7,		Keys.NumPad7,			false, "NUM7" },
			{ NSKey.Keypad8,		Keys.NumPad8,			false, "NUM8" },
			{ NSKey.Keypad9,		Keys.NumPad9,			false, "NUM9" },

			{ NSKey.KeypadClear,	Keys.Clear,				false, "CLEAR" },
			{ NSKey.KeypadDecimal,	Keys.Decimal,			false, "DECIMAL" },
			{ NSKey.KeypadDivide,	Keys.Divide,			false, "DIVIDE" },
			{ NSKey.KeypadEnter,	Keys.Execute,			false, "EXECUTE" },
			{ NSKey.KeypadEquals,	Keys.Oemplus,			false, "" },
			{ NSKey.KeypadMinus,	Keys.Subtract,			false, "SUBTRACT" },
			{ NSKey.KeypadMultiply,	Keys.Multiply,			false, "MULTIPLY" },
			{ NSKey.KeypadPlus,		Keys.Add,				false, "ADD" },

			{ NSKey.Escape,			Keys.Escape,            false, "ESC" },
			{ NSKey.LeftArrow,		Keys.Left,				false, "LEFT" },
			{ NSKey.LeftBracket,	Keys.OemOpenBrackets,	true, "(" },
			{ NSKey.Minus,			Keys.OemMinus,			true, "-" },
			{ NSKey.Mute,			Keys.VolumeMute,		false, "MUTE" },
			{ NSKey.Option,			Keys.Alt,				false, "" },
			{ NSKey.Quote,			Keys.OemQuotes,			true, "\"" },
			{ NSKey.RightArrow,		Keys.Right,				false, "RIGHT" },
			{ NSKey.RightBracket,	Keys.OemCloseBrackets,	true, ")" },
			{ NSKey.RightControl,	Keys.RControlKey,		false, "RCONTROL" },
			{ NSKey.RightOption,	Keys.Alt,				false, "ALT" },
			{ NSKey.RightShift,		Keys.RShiftKey,			false, "RSHIFT" },
			{ NSKey.Semicolon,		Keys.OemSemicolon,		true, ";" },
			{ NSKey.Slash,			Keys.OemQuestion,		true, "?" },
			{ NSKey.Tab,			Keys.Tab,       		false, "TAB" },
			{ NSKey.UpArrow,		Keys.Up,				false, "UP" },
			{ NSKey.Period,			Keys.OemPeriod,			true,  "." },
			{ NSKey.Return,			Keys.Enter,				false, "" },
			{ NSKey.Space,			Keys.Space,         	true, " " },
			{ NSKey.Grave,			Keys.Oemtilde,			true,  "~" },

		};

		#endregion // Keyboard translation tables

	}
}

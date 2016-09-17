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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//  Andreia Gaita	(avidigal@novell.com)
//
//

using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms {
	

	public class SendKeys {
		private struct Keyword {
			public Keyword(string keyword, int vk) {
				this.keyword = keyword;
				this.vk = vk;
			}
			internal string keyword;
			internal int vk;
		}

		#region Local variables
		private static Queue keys = new Queue();
		private static Hashtable keywords;
		#endregion

		static SendKeys() {
			SendKeys.keywords = new Hashtable();
			
			keywords.Add("BACKSPACE", (int)Keys.Back);
			keywords.Add("BS", (int)Keys.Back);
			keywords.Add("BKSP", (int)Keys.Back);
			keywords.Add("BREAK", (int)Keys.Cancel);
			keywords.Add("CAPSLOCK", (int)Keys.CapsLock);
			keywords.Add("DELETE", (int)Keys.Delete);
			keywords.Add("DEL", (int)Keys.Delete);
			keywords.Add("DOWN", (int)Keys.Down);
			keywords.Add("END", (int)Keys.End);
			keywords.Add("ENTER", (int)Keys.Enter);
			keywords.Add("ESC", (int)Keys.Escape);
			keywords.Add("HELP", (int)Keys.Help);
			keywords.Add("HOME", (int)Keys.Home);
			keywords.Add("INSERT", (int)Keys.Insert);
			keywords.Add("INS", (int)Keys.Insert);
			keywords.Add("LEFT", (int)Keys.Left);
			keywords.Add("NUMLOCK", (int)Keys.NumLock);
			keywords.Add("PGDN", (int)Keys.PageDown);
			keywords.Add("PGUP", (int)Keys.PageUp);
			keywords.Add("PRTSC", (int)Keys.PrintScreen);
			keywords.Add("RIGHT", (int)Keys.Right);
			keywords.Add("SCROLLLOCK", (int)Keys.Scroll);
			keywords.Add("TAB", (int)Keys.Tab);
			keywords.Add("UP", (int)Keys.Up);
			keywords.Add("F1", (int)Keys.F1);
			keywords.Add("F2", (int)Keys.F2);
			keywords.Add("F3", (int)Keys.F3);
			keywords.Add("F4", (int)Keys.F4);
			keywords.Add("F5", (int)Keys.F5);
			keywords.Add("F6", (int)Keys.F6);
			keywords.Add("F7", (int)Keys.F7);
			keywords.Add("F8", (int)Keys.F8);
			keywords.Add("F9", (int)Keys.F9);
			keywords.Add("F10", (int)Keys.F10);
			keywords.Add("F11", (int)Keys.F11);
			keywords.Add("F12", (int)Keys.F12);
			keywords.Add("F13", (int)Keys.F13);
			keywords.Add("F14", (int)Keys.F14);
			keywords.Add("F15", (int)Keys.F15);
			keywords.Add("F16", (int)Keys.F16);
			keywords.Add("ADD", (int)Keys.Add);
			keywords.Add("SUBTRACT", (int)Keys.Subtract);
			keywords.Add("MULTIPLY", (int)Keys.Multiply);
			keywords.Add("DIVIDE", (int)Keys.Divide);
		}

		#region Private methods

		private SendKeys() {
		}


		private static void AddVKey(int vk, bool down) 
		{
			MSG msg = new MSG();
			msg.message = down ? Msg.WM_KEYDOWN : Msg.WM_KEYUP;
			msg.wParam = new IntPtr(vk);
			msg.lParam = IntPtr.Zero;
			keys.Enqueue(msg);
		}

		private static void AddVKey(int vk, int repeat_count) 
		{
			for (int i = 0; i < repeat_count; i++ ) {
				keys.Enqueue(new MSG { message = Msg.WM_KEYDOWN, wParam = new IntPtr(vk), lParam = (IntPtr)1 });
				keys.Enqueue(new MSG { message = Msg.WM_KEYUP, wParam = new IntPtr(vk), lParam = (IntPtr)0 });
			}
		}

		private static void AddKey(char key, int repeat_count) {

			Keys k;
			bool shift = false;
			bool ctrl = false;
			bool alt = false;

			switch (key)
			{
				case '!': k = Keys.D1; shift = true; break;
				case '@': k = Keys.D2; shift = true; break;
				case '#': k = Keys.D3; shift = true; break;
				case '$': k = Keys.D4; shift = true; break;
				case '%': k = Keys.D5; shift = true; break;
				case '^': k = Keys.D6; shift = true; break;
				case '&': k = Keys.D7; shift = true; break;
				case '*': k = Keys.D8; shift = true; break;
				case '(': k = Keys.D9; shift = true; break;
				case ')': k = Keys.D0; shift = true; break;
				case '_': k = Keys.OemMinus; shift = true; break;
				case '+': k = Keys.Oemplus; shift = true; break;
				case '{': k = Keys.OemOpenBrackets; shift = true; break;
				case '}': k = Keys.OemCloseBrackets; shift = true; break;
				case '|': k = Keys.OemBackslash; shift = true; break;
				case ':': k = Keys.OemSemicolon; shift = true; break;
				case '\"': k = Keys.OemQuotes; shift = true; break;
				case '<': k = Keys.Oemcomma; shift = true; break;
				case '>': k = Keys.OemPeriod; shift = true; break;
				case '?': k = Keys.OemQuestion; shift = true; break;
				case '~': k = Keys.Oemtilde; shift = true; break;
				case '`': k = Keys.Oemtilde; break;
				case ',': k = Keys.Oemcomma; break;
				case '.': k = Keys.OemPeriod; break;
				case '-': k = Keys.OemMinus; break;
				case '=': k = Keys.Oemplus; break;
				case '[': k = Keys.OemOpenBrackets; break;
				case ']': k = Keys.OemCloseBrackets; break;
				case '\\': k = Keys.OemBackslash; break;
				case ';': k = Keys.OemSemicolon; break;
				case '\'': k = Keys.OemQuotes; break;
				case '/': k = Keys.Divide; break;
				default: k = (Keys)key; break;
			}

			if (shift) keys.Enqueue(new MSG { message = Msg.WM_KEYDOWN, wParam = new IntPtr((int)Keys.ShiftKey), lParam = IntPtr.Zero });
			if (ctrl) keys.Enqueue(new MSG { message = Msg.WM_KEYDOWN, wParam = new IntPtr((int)Keys.ControlKey), lParam = IntPtr.Zero });
			if (alt) keys.Enqueue(new MSG { message = Msg.WM_KEYDOWN, wParam = new IntPtr((int)Keys.Menu), lParam = IntPtr.Zero });

			for (int i = 0; i < repeat_count; i++ ) {
				keys.Enqueue(new MSG { message = Msg.WM_KEYDOWN, wParam = new IntPtr((int)k), lParam = IntPtr.Zero });
				keys.Enqueue(new MSG { message = Msg.WM_KEYUP, wParam = new IntPtr((int)k), lParam = IntPtr.Zero });
			}

			if (shift) keys.Enqueue(new MSG { message = Msg.WM_KEYUP, wParam = new IntPtr((int)Keys.ShiftKey), lParam = IntPtr.Zero });
			if (ctrl) keys.Enqueue(new MSG { message = Msg.WM_KEYUP, wParam = new IntPtr((int)Keys.ControlKey), lParam = IntPtr.Zero });
			if (alt) keys.Enqueue(new MSG { message = Msg.WM_KEYUP, wParam = new IntPtr((int)Keys.Menu), lParam = IntPtr.Zero });
		}

		private static void Parse(string key_string) {
			bool isBlock = false;
			bool isVkey = false;
			bool isRepeat = false;
			bool isShift = false;
			bool isCtrl = false;
			bool isAlt = false;

			StringBuilder repeats = new StringBuilder();
			StringBuilder group_string = new StringBuilder();
			
			int key_len = key_string.Length;
			for (int i = 0; i < key_len; i++) {
				switch(key_string[i]) {
					case '{':

						group_string.Remove(0, group_string.Length);
						repeats.Remove(0, repeats.Length);
						int start = i + 1;
						for (; start < key_len; start++) {
							if (start > i + 1 && key_string[start] == '}') // allow right parenthesis char "{}}"
								break;

							if (Char.IsWhiteSpace(key_string[start])) {
								if (isRepeat)
									throw new ArgumentException("SendKeys string {0} is not valid.", key_string);

								isRepeat = true;
								continue;
							}
							if (isRepeat) {
								if (!Char.IsDigit(key_string[start]))
									throw new ArgumentException("SendKeys string {0} is not valid.", key_string);
								
								repeats.Append(key_string[start]);
								continue;
							}

							group_string.Append(key_string[start]);
						}
						if (start == key_len || start == i + 1)
						{
							break;
							//throw new ArgumentException("SendKeys string {0} is not valid.", key_string);
						}

						else if (SendKeys.keywords.Contains(group_string.ToString().ToUpper())) {
							isVkey = true;
						}
						else {
							//throw new ArgumentException("SendKeys string {0} is not valid.", key_string);
						}

						int repeat = 1;
						if (repeats.Length > 0)
							repeat = Int32.Parse(repeats.ToString());
						if (isVkey)
							AddVKey((int)keywords[group_string.ToString().ToUpper()], repeats.Length == 0 ? 1 : repeat);
						else {
							if (Char.IsUpper(Char.Parse(group_string.ToString()))) {
								if (!isShift)
									AddVKey((int)Keys.ShiftKey, true);
								AddKey(Char.Parse(group_string.ToString()), 1);
								if (!isShift)
									AddVKey((int)Keys.ShiftKey, false);
							}
							else
								AddKey(Char.Parse(group_string.ToString().ToUpper()), repeats.Length == 0 ? 1 : repeat);
						}

						i = start;
						isRepeat = isVkey = false;
						if (isShift)
							AddVKey((int)Keys.ShiftKey, false);
						if (isCtrl)
							AddVKey((int)Keys.ControlKey, false);
						if (isAlt)
							AddVKey((int)Keys.Menu, false);
						isShift = isCtrl = isAlt = false;
						break;
					
					case '+': {
						AddVKey((int)Keys.ShiftKey, true);
						isShift = true;
						break;
					}

					case '^': {
							AddVKey((int)Keys.ControlKey, true);
						isCtrl = true;
						break;
					}

					case '%': {
						AddVKey((int)Keys.Menu, true);
						isAlt = true;
						break;
					}

					case '~': {
						AddVKey((int)Keys.Enter, 1);
						break;
					}

					case '(':
						isBlock = true;
						break;

					case ')': {
						if (isShift)
							AddVKey((int)Keys.ShiftKey, false);
						if (isCtrl)
							AddVKey((int)Keys.ControlKey, false);
						if (isAlt)
							AddVKey((int)Keys.Menu, false);
						isShift = isCtrl = isAlt = isBlock = false;
						break;
					}

					default: {
						if (Char.IsUpper(key_string[i])) {
							if (!isShift)
								AddVKey((int)Keys.ShiftKey, true);
							AddKey(key_string[i], 1);
							if (!isShift)
								AddVKey((int)Keys.ShiftKey, false);
						}
						else
							AddKey(Char.Parse(key_string[i].ToString().ToUpper()), 1);
						
						if (!isBlock) {
							if (isShift)
								AddVKey((int)Keys.ShiftKey, false);
							if (isCtrl)
								AddVKey((int)Keys.ControlKey, false);
							if (isAlt)
								AddVKey((int)Keys.Menu, false);
							isShift = isCtrl = isAlt = isBlock = false;
						}
						break;
					}
				}
			}

			if (isBlock)
				throw new ArgumentException("SendKeys string {0} is not valid.", key_string);

			if (isShift)
				AddVKey((int)Keys.ShiftKey, false);
			if (isCtrl)
				AddVKey((int)Keys.ControlKey, false);
			if (isAlt)
				AddVKey((int)Keys.Menu, false);

		}

		private static void SendInput() {
			IntPtr hwnd = XplatUI.GetActive ();
			
			if (hwnd != IntPtr.Zero) {
				Form active = ((Form) Control.FromHandle (hwnd));
				if (active != null && active.ActiveControl != null)
					hwnd = active.ActiveControl.Handle;
				else if (active != null)
					hwnd = active.Handle;
			}
			XplatUI.SendInput(hwnd, keys);
			keys.Clear();
		}

		#endregion	// Private Methods

		#region Public Static Methods
		public static void Flush() {
			Application.DoEvents();
		}

		public static void Send(string keys) {
			Parse(keys);
			SendInput();
		}

		private static object lockobj = new object();
		public static void SendWait(string keys) {
			lock(lockobj) {
				Send(keys);
			}
			//Flush();
		}

		#endregion	// Public Static Methods
	}
}
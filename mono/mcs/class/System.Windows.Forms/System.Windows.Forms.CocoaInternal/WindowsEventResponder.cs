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
using nint = System.Int32;
using nuint = System.UInt32;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	/// <summary>
	/// Implementation of NSResponder that forwards Cocoa events as WM_ window messages.
	/// </summary>
	class WindowsEventResponder : NSResponder
	{
		XplatUICocoa driver;
		NSView view;

		internal double preciseDeltaScale = 5.0;
		internal double rawDeltaScale = 40.0;

		public WindowsEventResponder(IntPtr instance) : base(instance)
		{
		}

		public WindowsEventResponder(XplatUICocoa driver, NSView view)
		{
			this.driver = driver;
			this.view = view;
		}

		#region Focus

		public override bool BecomeFirstResponder()
		{
			// If the view is hosted by MonoWindow then the window is responsible
			// to send WM_SETFOCUS since it can provide more accurate parameters.
			if (!(view.Window is MonoWindow))
				driver.SendMessage(view.Handle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
			return base.BecomeFirstResponder();
		}

		public override bool ResignFirstResponder()
		{
			// If the view is hosted by MonoWindow then the window is responsible
			// to send WM_KILLFOCUS since it can provide more accurate parameters.
			if (!(view.Window is MonoWindow))
				driver.SendMessage(view.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
			return base.ResignFirstResponder();
		}

		#endregion

		#region Mouse events

		public override void MouseEntered(NSEvent theEvent)
		{
			TranslateMouseEvent(theEvent);
		}

		public override void MouseExited(NSEvent theEvent)
		{
			TranslateMouseEvent(theEvent);
		}

		public override void MouseDown(NSEvent theEvent)
		{
			ProcessModifiers((theEvent));
			TranslateMouseEvent(theEvent);
		}

		public override void RightMouseDown(NSEvent theEvent)
		{
			ProcessModifiers((theEvent));
			TranslateMouseEvent(theEvent);
		}

		public override void OtherMouseDown(NSEvent theEvent)
		{
			ProcessModifiers((theEvent));
			TranslateMouseEvent(theEvent);
		}

		public override void MouseUp(NSEvent theEvent)
		{
			ProcessModifiers((theEvent));
			TranslateMouseEvent(theEvent);
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			ProcessModifiers((theEvent));
			TranslateMouseEvent(theEvent);
		}

		public override void OtherMouseUp(NSEvent theEvent)
		{
			ProcessModifiers((theEvent));
			TranslateMouseEvent(theEvent);
		}

		public override void MouseMoved(NSEvent theEvent)
		{
			if (driver.Grab.Hwnd == view.Handle || mouseView == view)
				TranslateMouseEvent(theEvent);
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			TranslateMouseEvent(theEvent);
		}

		public override void RightMouseDragged(NSEvent theEvent)
		{
			TranslateMouseEvent(theEvent);
		}

		public override void OtherMouseDragged(NSEvent theEvent)
		{
			TranslateMouseEvent(theEvent);
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			TranslateMouseEvent(theEvent);
		}

		internal static NSView mouseView; // A view that is currently under the mouse cursor.

		static nuint ButtonMaskToWParam(nuint mouseButtons)
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

		static nuint ButtonNumberToWParam(nint buttonNumber)
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

		static uint ModifiersToWParam(NSEventModifierMask modifierFlags)
		{
			uint wParam = 0;

			if ((modifierFlags & NSEventModifierMask.ControlKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_CONTROL;
			if ((modifierFlags & NSEventModifierMask.ShiftKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_SHIFT;

			return wParam;
		}

		void TranslateMouseEvent(NSEvent e)
		{
			var clientView = view as IClientView;
			CGPoint nspoint = e.LocationInWindow;
			Point localMonoPoint;
			bool client = clientView == null;

			nspoint = view.ConvertPointFromView(nspoint, null);
			localMonoPoint = driver.NativeToMonoFramed(nspoint, view);

			if ((clientView != null && clientView.ClientBounds.ToRectangle().Contains(localMonoPoint)) ||
			    driver.Grab.Hwnd != IntPtr.Zero)
			{
				client = true;
				localMonoPoint.X -= (int)clientView.ClientBounds.X;
				localMonoPoint.Y -= (int)clientView.ClientBounds.Y;
			}

			int button = (int)e.ButtonNumber;
			if (button >= (int)NSMouseButtons.Excessive)
				button = (int)NSMouseButtons.X;
			else if (button == (int)NSMouseButtons.Left && ((driver.ModifierKeys & Keys.Control) != 0))
				button = (int)NSMouseButtons.Right;
			int msgOffset4Button = 3 * (button - (int)NSMouseButtons.Left);
			if (button >= (int)NSMouseButtons.X)
				++msgOffset4Button;

			MSG msg = new MSG();
			msg.hwnd = view.Handle;
			msg.lParam = (IntPtr)(localMonoPoint.Y << 16 | (localMonoPoint.X & 0xFFFF));
			if (e.Window == null)
				msg.pt = driver.NativeToMonoScreen(NSEvent.CurrentMouseLocation).ToPOINT();
			else
				msg.pt = driver.NativeToMonoScreen(e.Window.ConvertBaseToScreen(e.LocationInWindow)).ToPOINT();
			msg.refobject = view;

			switch (e.Type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
					// FIXME: Should be elsewhere
					if (e.ClickCount > 1)
						msg.message = (client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK) + msgOffset4Button;
					else
						msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN) + msgOffset4Button;
					msg.wParam = (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonNumberToWParam(e.ButtonNumber));
					break;

				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseUp:
					msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP) + msgOffset4Button;
					msg.wParam = (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonNumberToWParam(e.ButtonNumber));
					break;

				case NSEventType.MouseMoved:
				case NSEventType.LeftMouseDragged:
				case NSEventType.RightMouseDragged:
				case NSEventType.OtherMouseDragged:
					msg.wParam = (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonMaskToWParam(NSEvent.CurrentPressedMouseButtons));
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
					break;

				case NSEventType.ScrollWheel:
					int delta = ScaleAndQuantizeDelta((float)e.ScrollingDeltaY, e.HasPreciseScrollingDeltas);
					if (delta == 0)
						return;

					if (e.Phase == NSEventPhase.None && e.MomentumPhase == NSEventPhase.None || e.Phase == NSEventPhase.Changed || e.MomentumPhase == NSEventPhase.Changed)
					{
						msg.message = Msg.WM_MOUSEWHEEL;
						msg.wParam = (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonMaskToWParam(NSEvent.CurrentPressedMouseButtons));
						msg.wParam = (IntPtr)(((int)msg.wParam & 0xFFFF) | (delta << 16));
						msg.lParam = (IntPtr)((msg.pt.x & 0xFFFF) | (msg.pt.y << 16));
						break;
					}
					return;

				case NSEventType.MouseEntered:
				case NSEventType.MouseExited:
					if (driver.Grab.Hwnd == IntPtr.Zero && e.Window != null)
					{
						var contentView = e.Window.ContentView;
						var newMouseView = (contentView.Superview ?? contentView).HitTest(e.LocationInWindow);
						if (mouseView != newMouseView)
						{
							if (mouseView != null)
							{
								var msgLeave = new MSG();
								msgLeave.hwnd = mouseView.Handle;
								msgLeave.lParam = msg.lParam;
								msgLeave.wParam = (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonMaskToWParam(NSEvent.CurrentPressedMouseButtons));
								msgLeave.pt = msg.pt;
								msgLeave.refobject = msg.refobject;
								msgLeave.message = Msg.WM_MOUSELEAVE;
								driver.EnqueueMessage(msgLeave);
								mouseView = null;
							}
							if (newMouseView != null)
							{
								msg.hwnd = newMouseView.Handle;
								msg.wParam = (IntPtr)(ModifiersToWParam(e.ModifierFlags) | ButtonMaskToWParam(NSEvent.CurrentPressedMouseButtons));
								msg.message = Msg.WM_MOUSE_ENTER;
								driver.EnqueueMessage(msg);
								mouseView = newMouseView;
							}
						}
					}
					return;

				//case NSEventType.TabletPoint:
				//case NSEventType.TabletProximity:
				default:
					return;
			}

			driver.EnqueueMessage(msg);
		}

		internal int ScaleAndQuantizeDelta(float delta, bool precise)
		{
			return precise ? (int)(preciseDeltaScale * delta) : (int)(rawDeltaScale * delta);
		}

		#endregion

		#region Keyboard

		static int repeatCount = 0;
		static bool altDown;
		static bool cmdDown;
		static bool shiftDown;
		static bool ctrlDown;
		IntPtr wmCharLParam;

		public override void KeyDown(NSEvent theEvent)
		{
			ProcessModifiers(theEvent);
			TranslateKeyboardEvent(theEvent);
		}

		public override void KeyUp(NSEvent theEvent)
		{
			ProcessModifiers(theEvent);
			TranslateKeyboardEvent(theEvent);
		}

		public override void FlagsChanged(NSEvent theEvent)
		{
			ProcessModifiers(theEvent);
		}

		void ProcessModifiers(NSEvent eventref)
		{
			// we get notified when modifiers change, but not specifically what changed
			NSEventModifierMask flags = eventref.ModifierFlags;
			NSEventModifierMask diff = flags ^ XplatUICocoa.key_modifiers;
			XplatUICocoa.key_modifiers = flags;

			shiftDown = 0 != (flags & NSEventModifierMask.ShiftKeyMask);
			ctrlDown = 0 != (flags & NSEventModifierMask.ControlKeyMask);
			altDown = 0 != (flags & NSEventModifierMask.AlternateKeyMask);
			cmdDown = 0 != (flags & NSEventModifierMask.CommandKeyMask);

			if ((NSEventModifierMask.ShiftKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.ShiftKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_SHIFT, IntPtr.Zero);
			if ((NSEventModifierMask.ControlKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.ControlKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_CONTROL, IntPtr.Zero);
			if ((NSEventModifierMask.AlternateKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.AlternateKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_MENU, IntPtr.Zero);
			if ((NSEventModifierMask.CommandKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.CommandKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_LWIN, IntPtr.Zero);
		}

		void TranslateKeyboardEvent(NSEvent e)
		{
			repeatCount = e.IsARepeat ? 1 + repeatCount : 0;

			Keys key = KeysConverter.GetKeys(e);
			if (key == Keys.None)
				return;

			bool isExtendedKey = ctrlDown || cmdDown || e.Characters.Length == 0 || !KeysConverter.IsChar(e.Characters[0], key) && KeysConverter.DeadKeyState == 0;

			ulong lp = 0;
			lp |= ((ulong)(uint)repeatCount);
			lp |= ((ulong)e.KeyCode) << 16; // OEM-dependent scanCode
			lp |= ((ulong)(isExtendedKey ? 1 : 0)) << 24;
			lp |= ((ulong)(e.IsARepeat ? 1 : 0)) << 30;
			IntPtr lParam = (IntPtr)lp;
			IntPtr wParam = (IntPtr)key;

			bool isSysKey = false;// altDown && !cmdDown
			Msg msg = isSysKey
				? (e.Type == NSEventType.KeyDown ? Msg.WM_SYSKEYDOWN : Msg.WM_SYSKEYUP)
				: (e.Type == NSEventType.KeyDown ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);

			//Debug.WriteLine ("keyCode={0}, characters=\"{1}\", key='{2}', chars='{3}'", e.KeyCode, chars, key, chars);
			driver.PostMessage(view.Handle, msg, wParam, lParam);

			// On Windows, this would normally be done in TranslateMessage
			if (e.Type == NSEventType.KeyDown && !isExtendedKey)
			{
				wmCharLParam = lParam;
				view.InterpretKeyEvents(new[] { e });
				wmCharLParam = IntPtr.Zero;
			}
		}


		#endregion

		[Export("selectAll:")]
		public virtual void SelectAll(NSObject sender)
		{
			SendCmdKey(view.Handle, VirtualKeys.VK_A);
			//driver.SendMessage(view.Handle, Msg.WM_SELECT_ALL, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("copy:")]
		public virtual void Copy(NSObject sender)
		{
			SendCmdKey(view.Handle, VirtualKeys.VK_C);
			//driver.SendMessage(view.Handle, Msg.WM_COPY, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("paste:")]
		public virtual void Paste(NSObject sender)
		{
			SendCmdKey(view.Handle, VirtualKeys.VK_V);
			//driver.SendMessage(view.Handle, Msg.WM_PASTE, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("cut:")]
		public virtual void Cut(NSObject sender)
		{
			SendCmdKey(view.Handle, VirtualKeys.VK_X);
			//driver.SendMessage(view.Handle, Msg.WM_CUT, IntPtr.Zero, IntPtr.Zero);
		}

		[Export("insertTab:")]
		public virtual void InsertTab(NSObject sender)
		{
			driver.PostMessage(view.Handle, Msg.WM_CHAR, (IntPtr)'\t', wmCharLParam);
		}

		[Export("insertText:")]
		public virtual void InsertText(NSObject text)
		{
			string str = text.ToString();
			if (!String.IsNullOrEmpty(str))
				foreach (var c in str)
					driver.PostMessage(view.Handle, Msg.WM_CHAR, (IntPtr)c, wmCharLParam);
		}

		void SendCmdKey(IntPtr hwnd, VirtualKeys key)
		{
			if (!cmdDown)
				driver.SendMessage(hwnd, Msg.WM_KEYDOWN, (IntPtr)VirtualKeys.VK_LWIN, IntPtr.Zero);
			driver.SendMessage(hwnd, Msg.WM_KEYDOWN, (IntPtr)key, (IntPtr)0x1080000);
			driver.SendMessage(hwnd, Msg.WM_KEYUP, (IntPtr)key, (IntPtr)0x1080000);
			if (!cmdDown)
				driver.SendMessage(hwnd, Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_LWIN, IntPtr.Zero);
		}
	}
}

using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;
using Foundation;
using AppKit;
using CoreGraphics;
using ObjCRuntime;

namespace System.Windows.Forms.CocoaInternal
{
	/// <summary>
	/// Implementation of NSResponder that forwards Cocoa events as WM_ window messages.
	/// </summary>
	class WindowsEventResponder : NSResponder
	{
		XplatUICocoa driver;
		internal NSView view;

		// For emulating MouseUp in ToolStripDropDown when using native menu (that swallows it):
		internal static MSG LastMouseDownMsg;

		public WindowsEventResponder(NativeHandle instance) : base(instance)
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
			TranslateMouseEnterExit(theEvent);
		}

		public override void MouseExited(NSEvent theEvent)
		{
			TranslateMouseEnterExit(theEvent);
		}

		internal void TranslateMouseEnterExit(NSEvent e)
		{
			// We use overlapping track rectanges on every control, so the Entered/Exited events
			// are sent not only to the top-level visible view, but also to its superviews.
			// This has to be converted to the the model where only the top-level control receives
			// the messages.
			//
			// The conversion is by performing the hit test. Note that e.LocationInWindow is NOT
			// updated by the time the tracking events are received and thus e.Window.
			// MouseLocationOutsideOfEventStream has to be used instead.
			//
			// The Exited event has to be handled the same way as the entered event to cover the
			// following scenario:
			//  ___________________________
			// | Panel                     |
			// |  ______________           |
			// | |    Button    |          |
			// 
			// When you move the mouse over the Button you will receive the Entered event for
			// it. At the same time the Panel already had Entered called for its own tracking area.
			// Now moving the mouse away from the Button into the Panel area will cause an
			// Exited event on the Button's tracking rectangle, but it will NOT generate any
			// Entered event. In the System.Windows.Forms world we want to receive WM_MOUSELEAVE
			// on the Button and WM_MOUSE_ENTERED on the Panel.
			//
			// Further reading:
			// https://stackoverflow.com/questions/9986267/mouse-enter-exit-events-on-partially-hidden-nsviews

			// While dragging, both e.Window and e.WindowNumber relate to the window in which the dragging session started,
			// but e.LocationInWindow always contains the location relative to the window under the cursor!
			var location = NSEvent.CurrentMouseLocation;
			var number = NSWindow.WindowNumberAtPoint(location, 0);
			var window = NSApplication.SharedApplication.WindowWithWindowNumber(number) ?? e.Window;
			if (window == null)
				return;

			var locationInWindow = window.ConvertPointFromScreenSafe(location);

			var msg = TranslateMouseCore(e, out bool client);
			var newMouseView = window?.ContentView.HitTest(locationInWindow) ?? window?.ContentView.Superview?.HitTest(locationInWindow);
			var newMouseViewHandle = newMouseView is MonoView || newMouseView is IMacNativeControl ? (IntPtr)newMouseView.Handle : IntPtr.Zero;
			if (newMouseViewHandle == IntPtr.Zero && newMouseView != null)
			{
				var c = Control.FromChildHandle(newMouseView.Handle);
				if (c != null)
					newMouseViewHandle = c.Handle;
			}

#if DEBUG_MOUSE_ENTER_EXIT
			{
				// DEBUG
				Console.Write(e.Type == NSEventType.MouseEntered ? "ENTER " : "EXIT ");
				if (newMouseView != null)
					Console.Write("NEW: " + DebugUtility.ControlInfo(newMouseView));
				else
					Console.Write("NO NEW");
			}
#endif

			if (newMouseViewHandle == driver.LastEnteredHwnd)
				return;

			// Keep track of the last entered window during grab, but don't send messages. The
			// messages are sent by XplatUICocoa.GrabWindow and XplatUICocoa.UngrabWindow when
			// the grabbing state is entered/exited.
			if (driver.Grab.Hwnd != IntPtr.Zero)
			{
				driver.LastEnteredHwnd = newMouseViewHandle;
				return;
			}

			msg.wParam = (IntPtr)(e.ModifiersToWParam() | Mac.Extensions.ButtonMaskToWParam(NSEvent.CurrentPressedMouseButtons));
			// First notify the old window that mouse left it
			if (driver.LastEnteredHwnd != IntPtr.Zero)
			{
				msg.hwnd = driver.LastEnteredHwnd;
				msg.message = Msg.WM_MOUSELEAVE;
				driver.LastEnteredHwnd = IntPtr.Zero;

				if (HitTest.HTNOWHERE != (HitTest)Application.SendMessage(msg.hwnd, Msg.WM_NCHITTEST, msg.wParam, msg.lParam))
					Application.SendMessage(ref msg);
			}
			// And then notify the new window that mouse entered it
			if (newMouseViewHandle != IntPtr.Zero)
			{
				msg.hwnd = newMouseViewHandle;
				msg.message = Msg.WM_MOUSE_ENTER;
				driver.LastEnteredHwnd = newMouseViewHandle;

				if (HitTest.HTNOWHERE != (HitTest)Application.SendMessage(msg.hwnd, Msg.WM_NCHITTEST, msg.wParam, msg.lParam))
					Application.SendMessage(ref msg);
			}
		}

		public override void MouseDown(NSEvent theEvent)
		{
			TranslateMouseDown(theEvent);
		}

		public override void RightMouseDown(NSEvent theEvent)
		{
			TranslateMouseDown(theEvent);
		}

		public override void OtherMouseDown(NSEvent theEvent)
		{
			TranslateMouseDown(theEvent);
		}

		NSEvent prevMouseDown = null;

		internal void TranslateMouseDown(NSEvent e)
		{
			var msg = TranslateMouseCore(e, out bool client, view.FirstEnabledParentOrSelf());

			if (HitTest.HTNOWHERE == (HitTest)Application.SendMessage(msg.hwnd, Msg.WM_NCHITTEST, msg.wParam, msg.lParam))
				return;
			
			msg.wParam = (IntPtr)(e.ModifiersToWParam() | e.ButtonNumberToWParam());

			if (e.ClickCount > 1 && (e.ClickCount & 1) == 0 && prevMouseDown?.Type == e.Type)
				msg.message = (client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK).AdjustForButton(e);
			else
			{
				msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN).AdjustForButton(e);
				LastMouseDownMsg = msg;
			}

			prevMouseDown = e;
			Application.SendMessage(ref msg);
		}

		public override void MouseUp(NSEvent theEvent)
		{
			TranslateMouseUp(theEvent);
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			TranslateMouseUp(theEvent);
		}

		public override void OtherMouseUp(NSEvent theEvent)
		{
			TranslateMouseUp(theEvent);
		}

		internal void TranslateMouseUp(NSEvent e)
		{
			var msg = TranslateMouseCore(e, out bool client, MouseTarget().FirstEnabledParentOrSelf());

			if (HitTest.HTNOWHERE == (HitTest)Application.SendMessage(msg.hwnd, Msg.WM_NCHITTEST, msg.wParam, msg.lParam))
				return;

			msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP).AdjustForButton(e);
			msg.wParam = (IntPtr)(e.ModifierFlags.ToWParam() | e.ButtonNumberToWParam());
			LastMouseDownMsg.hwnd = IntPtr.Zero;
			Application.SendMessage(ref msg);
		}

		internal NSView MouseTarget()
		{
			if (ObjCRuntime.Runtime.TryGetNSObject(driver.Grab.Hwnd) is NSView grab)
				return grab;
			if (ObjCRuntime.Runtime.TryGetNSObject(driver.LastEnteredHwnd) is NSView hover)
				return hover;
			return null;
		}

		public override void MouseMoved(NSEvent theEvent)
		{
			var msg = TranslateMouseCore(theEvent, out bool client, MouseTarget());

			if (HitTest.HTNOWHERE == (HitTest)Application.SendMessage(msg.hwnd, Msg.WM_NCHITTEST, msg.wParam, msg.lParam))
				return;

			msg.wParam = (IntPtr)theEvent.ModifierFlags.ToWParam();
			msg.message = client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE;
			Application.SendMessage(ref msg);
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			TranslateMouseDragged(theEvent, MsgButtons.MK_LBUTTON);
		}

		public override void RightMouseDragged(NSEvent theEvent)
		{
			TranslateMouseDragged(theEvent, MsgButtons.MK_RBUTTON);
		}

		public override void OtherMouseDragged(NSEvent theEvent)
		{
			TranslateMouseDragged(theEvent, MsgButtons.MK_MBUTTON);
		}

		internal void TranslateMouseDragged(NSEvent e, MsgButtons button)
		{
			// Dragging events get delivered to the view where dragging started.
			// We have to redirect them to the view under the cursor (maybe in another window).
			var msg = TranslateMouseCore(e, out bool client, MouseTarget());
			msg.wParam = (IntPtr)(e.ModifierFlags.ToWParam() | (uint)button);
			msg.message = client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE;
			Application.SendMessage(ref msg);
		}

		public override void ScrollWheel(NSEvent e)
		{
			var msg = TranslateMouseCore(e, out bool _);

			var handled = false;
			var auxiliary = e.Phase != NSEventPhase.Changed;
			if (e.ScrollingDeltaY != 0 || auxiliary)
			{
				int delta = e.ScaledAndQuantizedDeltaY();
				msg.message = Msg.WM_MOUSEWHEEL;
				msg.wParam = (IntPtr)(((int)e.ModifiersToWParam() & 0xFFFF) | (delta << 16));
				msg.lParam = (IntPtr)((msg.pt.x & 0xFFFF) | (msg.pt.y << 16));
				handled = Application.SendMessage(ref msg).ToInt64() == 0;
			}

			if (e.ScrollingDeltaX != 0 || auxiliary)
			{
				int delta = e.ScaledAndQuantizedDeltaX();
				msg.message = Msg.WM_MOUSEHWHEEL;
				msg.wParam = (IntPtr)(((int)e.ModifiersToWParam() & 0xFFFF) | (delta << 16));
				msg.lParam = (IntPtr)((msg.pt.x & 0xFFFF) | (msg.pt.y << 16));
				handled = Application.SendMessage(ref msg).ToInt64() == 0;
			}

			if (!handled)
				base.ScrollWheel(e);
		}

		internal MSG TranslateMouseCore(NSEvent e, out bool isClient, NSView target = null)
		{
			var window = e.Window;
			var locationInWindow = e.LocationInWindow;
			if (target != null && target.Window != null)
			{
				locationInWindow = target.Window.ConvertPointFromWindow(locationInWindow, window);
				window = target.Window;
			}
			else target = view;

			var nspoint = target.ConvertPointFromView(locationInWindow, null);
			var localMonoPoint = driver.NativeToMonoFramed(nspoint, target);

			isClient = false;
			if (target is IClientView client)
			{
				var clientRect = client.ClientBounds.ToRectangle();
				if (clientRect.Contains(localMonoPoint) || driver.Grab.Hwnd != IntPtr.Zero)
				{
					isClient = true;
					localMonoPoint.Offset(-clientRect.X, -clientRect.Y);
				}
			}
			else
			{
				if (!(target is MonoView))
					isClient = true;
			}

			return new MSG
			{
				hwnd = target.Handle,
				lParam = (IntPtr)(localMonoPoint.Y << 16 | (localMonoPoint.X & 0xFFFF)),
				pt = window == null
					? driver.NativeToMonoScreen(NSEvent.CurrentMouseLocation).ToPOINT()
					: driver.NativeToMonoScreen(window.ConvertPointToScreenSafe(locationInWindow)).ToPOINT(),
				refobject = target
			};
		}

		#endregion

		#region Keyboard

		internal IntPtr wmCharLParam;
		
		MSG pendingWmKeyDown;

		internal void OnImeCompositionBegin()
		{
			pendingWmKeyDown.wParam = (IntPtr)(int)Keys.ProcessKey;
		}

		internal void OnImeCompositionContinue()
		{
			pendingWmKeyDown.wParam = (IntPtr)(int)Keys.ProcessKey;
		}

		internal void OnImeCompositionEnd()
		{
			pendingWmKeyDown.wParam = (IntPtr)(int)Keys.ProcessKey;
		}

		bool DeferWmKey(Msg msg, IntPtr wParam, IntPtr lParam)
		{
			// WM_KEYDOWN should be sent with Keys.ProcessKey if IME composition is being constructed.
			// Therefore, we have to wait until we see if the IME composition is in progress or not, if the view is editable.
			if (view is MonoEditView)
			{
				pendingWmKeyDown = new MSG { hwnd = view.Handle, message = msg, wParam = wParam, lParam = lParam };
				return true;
			}
			pendingWmKeyDown.hwnd = IntPtr.Zero;
			return false;
		}

		public override void KeyDown(NSEvent e)
		{
			var isKnownKey = e.ToKeyMsg(out Msg msg, out IntPtr wParam, out IntPtr lParam);
			if (isKnownKey && !DeferWmKey(msg, wParam, lParam))
				driver.SendMessage(view.Handle, msg, wParam, lParam);

			wmCharLParam = isKnownKey ? lParam : IntPtr.Zero;
			view.InterpretKeyEvents(new[] { e });
			wmCharLParam = IntPtr.Zero;

			if (isKnownKey)
				SendPendingWmKeyDown();
		}

		public override void KeyUp(NSEvent e)
		{
			if (e.ToKeyMsg(out Msg msg, out IntPtr wParam, out IntPtr lParam))
				driver.SendMessage(view.Handle, msg, wParam, lParam);
			else
				base.KeyUp(e);
		}

		public override void FlagsChanged(NSEvent theEvent)
		{
			ProcessModifiers(theEvent.ModifierFlags);
		}

		void ProcessModifiers(NSEventModifierMask flags)
		{
			var diff = XplatUICocoa.key_modifiers_mask;
			if ((NSEventModifierMask.ShiftKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.ShiftKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_SHIFT, IntPtr.Zero);
			if ((NSEventModifierMask.ControlKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.ControlKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_CONTROL, IntPtr.Zero);
			if ((NSEventModifierMask.AlternateKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.AlternateKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_MENU, IntPtr.Zero);
			if ((NSEventModifierMask.CommandKeyMask & diff) != 0)
				driver.SendMessage(view.Handle, (NSEventModifierMask.CommandKeyMask & flags) != 0 ? Msg.WM_KEYDOWN : Msg.WM_KEYUP, (IntPtr)VirtualKeys.VK_LWIN, IntPtr.Zero);
		}

		/// <summary>
		/// This handles a problem with control keys - like Esc, Enter, Arrows, Tab etc:
		/// If we sent WM_KEYDOWN regardless of InterpretKeyEvents, the event would afect both IME and the control/dialog.
		/// (For example, the Esc or Enter key would close not only IME, but the dialog itself, too)
		/// We have to wait for IME to see if it eats the key event or not.
		/// </summary>
		internal virtual void SendPendingWmKeyDown()
		{
			if (pendingWmKeyDown.hwnd != IntPtr.Zero)
			{
				driver.SendMessage(pendingWmKeyDown.hwnd, pendingWmKeyDown.message, pendingWmKeyDown.wParam, pendingWmKeyDown.lParam);
				pendingWmKeyDown.hwnd = IntPtr.Zero;
			}
		}

		// Forwarded from MonoEditView
		internal virtual void InsertText(NSObject text, NSRange replacementRange)
		{
			// Until we fully support marked text and replacing characters in edit box while IME is shown,
			// emulate replacement by sending backspace and then typing.
			// Not doing this would result in not deleting the originally typed character, event if it should have been replaced by IME.
			if (replacementRange.Location == 0)
				SendKey(view.Handle, (IntPtr)VirtualKeys.VK_BACK, IntPtr.Zero);

			SendPendingWmKeyDown();

			string str = text.ToString();
			if (!String.IsNullOrEmpty(str))
				foreach (var c in str)
					driver.SendMessage(view.Handle, Msg.WM_CHAR, (IntPtr)c, wmCharLParam);
		}

		[Export("doCommandBySelector:")]
		public virtual void DoCommandBySelector(Selector selector)
		{
			SendPendingWmKeyDown();

			switch (selector.Name)
			{
				case "insertNewline:":
					// although '\n' is the new line char on Mac, .net controls are bound to '\r'
					driver.SendMessage(view.Handle, Msg.WM_CHAR, (IntPtr)'\r', wmCharLParam);
					break;
				case "insertTab:":
					driver.SendMessage(view.Handle, Msg.WM_CHAR, (IntPtr)'\t', wmCharLParam);
					break;
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
			if (NSPasteboard.GeneralPasteboard.PasteboardItems.Length != 0)
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
			driver.SendMessage(view.Handle, Msg.WM_CHAR, (IntPtr)'\t', wmCharLParam);
		}

		[Export("insertText:")]
		public virtual void InsertText(NSObject text)
		{
			string str = text.ToString();
			if (!String.IsNullOrEmpty(str))
				foreach (var c in str)
					driver.SendMessage(view.Handle, Msg.WM_CHAR, (IntPtr)c, wmCharLParam);
		}

		[Export("undo:")]
		public virtual void Undo(NSObject sender)
		{
			if (view.Window.FirstResponder is NSTextView textView) // Make use of NSTextView's undo/redo caps.
				SuppressAndResendAction("undo:", sender);
			else
				SendCmdKey(view.Handle, VirtualKeys.VK_Z);
		}

		[Export("redo:")]
		public virtual void Redo(NSObject sender)
		{
			if (view.Window.FirstResponder is NSTextView textView) // Make use of NSTextView's undo/redo caps.
				SuppressAndResendAction("redo:", sender);
			else
				SendCmdKey(view.Handle, VirtualKeys.VK_Z, NSEventModifierMask.ShiftKeyMask);
		}

		static string suppressedAction = null;
	
		virtual internal void SuppressAndResendAction(string actionName, NSObject sender)
		{
			var sel = new Selector(suppressedAction = actionName);
			NSApplication.SharedApplication.SendAction(sel, null, sender);
			suppressedAction = null;
		}

		public override bool RespondsToSelector(Selector sel)
		{
			if (suppressedAction == sel.Name)
				return false;
			
			return base.RespondsToSelector(sel);
		}

		void SendCmdKey(IntPtr hwnd, VirtualKeys key, NSEventModifierMask moreModifiers = 0)
		{
			// Emulate (and then restore) only modifiers of not currently pressed keys
			var affected = XplatUICocoa.SetModifiers(NSEventModifierMask.CommandKeyMask | moreModifiers, true);
			if (affected != 0)
			{
				ProcessModifiers(XplatUICocoa.key_modifiers);
			}

			SendKey(hwnd, (IntPtr)key, (IntPtr)0x1080000);

			if (affected != 0)
			{
				XplatUICocoa.SetModifiers(affected, false); 
				ProcessModifiers(XplatUICocoa.key_modifiers);
			}
		}

		void SendKey(IntPtr hwnd, IntPtr key, IntPtr lParam)
		{
			driver.SendMessage(hwnd, Msg.WM_KEYDOWN, (IntPtr)key, lParam);
			driver.SendMessage(hwnd, Msg.WM_KEYUP, (IntPtr)key, lParam);
		}

		#region Support for embedding native controls

		internal bool snatchCommands = true;

		public virtual IntPtr SwfControlHandle
		{
			[Export("swfControlHandle")]
			get { return view.Handle; }
		}

		[Export("embeddedControl:doCommandBySelector:")]
		public virtual bool EmbeddedControlDoCommandBySelector(NSResponder target, Selector selector)
		{
			// This method allows you to choose which [menu] commands should be processed by embedded native control itself,
			// and which commands should be handled in the form of WM_KEYDOWN/WM_KEYUP message pair by it's WinForms parent control.

			// By default, the parent control performs all commands (copy:, paste:, etc..).
			return (snatchCommands ? this : target).TryToPerformwith(selector, this);
		}

		#endregion
	}
}

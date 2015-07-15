//
//MouseHandler.cs
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
//	Geoff Norton  <gnorton@novell.com>
//
//

using System;
using System.Runtime.InteropServices;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using NSPoint = System.Drawing.PointF;

namespace System.Windows.Forms.CocoaInternal {
	internal class MouseHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventMouseDown = 1;
		internal const uint kEventMouseUp = 2;
		internal const uint kEventMouseMoved = 5;
		internal const uint kEventMouseDragged = 6;
		internal const uint kEventMouseEntered = 8;
		internal const uint kEventMouseExited = 9;
		internal const uint kEventMouseWheelMoved = 10;
		internal const uint kEventMouseScroll = 11;

		internal const uint kEventParamMouseLocation = 1835822947;
		internal const uint kEventParamMouseButton = 1835168878;
		internal const uint kEventParamMouseWheelAxis = 1836540280;
		internal const uint kEventParamMouseWheelDelta = 1836541036;
		internal const uint typeLongInteger = 1819242087;
		internal const uint typeMouseWheelAxis = 1836540280;
		internal const uint typeMouseButton = 1835168878;
		internal const uint typeQDPoint = 1363439732;

		internal const uint kEventMouseWheelAxisX = 0;
		internal const uint kEventMouseWheelAxisY = 1;

		internal const uint DoubleClickInterval = 7500000;
		internal static ClickStruct ClickPending;
		
		internal MouseHandler (XplatUICocoa driver) : base (driver) {}

		public EventHandledBy ProcessEvent (NSObject callref, NSEvent eventref, MonoView handle, uint kind, ref MSG msg)
		{
			NSPoint nspoint = eventref.LocationInWindow, globalNSPoint = nspoint;
			// Insure the location is still valid.
			NSWindow winWrap = eventref.Window;
			if (winWrap != null) {
				globalNSPoint = winWrap.ConvertBaseToScreen (nspoint);
				Driver.mouse_position = Driver.NativeToMonoScreen (globalNSPoint);
				IntPtr contentHandle = Driver.WindowToHandle ((IntPtr) winWrap.Handle);
				if (IntPtr.Zero == contentHandle)
					return EventHandledBy.NativeOS;
				Hwnd contentHwnd = Hwnd.ObjectFromWindow (contentHandle);
				if (null == contentHwnd || contentHwnd.zombie)
					return EventHandledBy.NativeOS;
			} else {
				Driver.mouse_position = Driver.NativeToMonoScreen (globalNSPoint);
			}

			Point localMonoPoint = Driver.mouse_position;
			NSView vuWrap = null;
//			NSRect window_bounds = winWrap.frame ();
//			IntPtr window_handle = (IntPtr) winWrap;
			bool client = true;
			int button = (int) eventref.ButtonNumber;
			Hwnd hwnd =null;

			IntPtr hwndHandle = XplatUICocoa.Grab.Hwnd;
			bool grabbed = IntPtr.Zero != hwndHandle;

//			GetEventParameter (eventref, kEventParamMouseLocation, typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref qdpoint);
//			GetEventParameter (eventref, kEventParamMouseButton, typeMouseButton, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref button);
			
			if (button >= (int) NSMouseButtons.Excessive)
				button = (int) NSMouseButtons.X;
			else if (button == (int) NSMouseButtons.Left && ((Driver.ModifierKeys & Keys.Control) != 0))
				button = (int) NSMouseButtons.Right;

			int msgOffset4Button = 3 * (button - (int) NSMouseButtons.Left);
			if (button >= (int) NSMouseButtons.X)
				++msgOffset4Button;

//			point.x = qdpoint.x;
//			point.y = qdpoint.y;

//			if (FindWindow (qdpoint, ref window_handle) == 5)
//				return EventHandledBy.PostMessage;

//			GetWindowBounds (handle, 33, ref window_bounds);
//			HIViewFindByID (HIViewGetRoot (handle), new HIViewID (EventHandler.kEventClassWindow, 1), ref window_handle);

//			point.x -= window_bounds.left;
//			point.y -= window_bounds.top;

//			HIViewGetSubviewHit (window_handle, ref point, true, ref view_handle);
//			HIViewConvertPoint (ref point, window_handle, view_handle);

			if (grabbed) {
				hwnd = Hwnd.ObjectFromHandle (hwndHandle); 
				if (null == hwnd || hwnd.zombie)
					return EventHandledBy.NativeOS;
				vuWrap = (NSView) MonoMac.ObjCRuntime.Runtime.GetNSObject (hwnd.ClientWindow);

				if (vuWrap.Window != winWrap) {
					// Over different or no window: adjust coordinates.
					// Now in screen coordinates.
					winWrap = vuWrap.Window;
					nspoint = winWrap.ConvertScreenToBase (globalNSPoint);
					// Now in grabber window coordinates.
				}

				nspoint = vuWrap.ConvertPointFromView (nspoint, null);
				localMonoPoint = Driver.NativeToMonoFramed (nspoint, (int) vuWrap.Frame.Size.Height);
				client = true;

//				localMonoPoint -= (Size) hwnd.ClientRect.Location;
			} else if (winWrap != null) {
				NSView contentVuWrap = winWrap.ContentView;
				vuWrap = contentVuWrap.HitTest (nspoint);

				if (vuWrap == null)
					return EventHandledBy.NativeOS;	// Part of window Mono is not handling (title bar).

				IntPtr vuHandle = vuWrap.Handle;
				hwnd = Hwnd.ObjectFromWindow (vuHandle);
				if (null == hwnd || hwnd.zombie)
					return EventHandledBy.NativeOS;

				hwndHandle = hwnd.Handle;
//				Rectangle clientRect = hwnd.ClientRect;
				nspoint = vuWrap.ConvertPointFromView (nspoint, null);
				localMonoPoint = Driver.NativeToMonoFramed (nspoint, (int) vuWrap.Frame.Size.Height);
//				client = clientRect.Contains (localMonoPoint);
				client = vuHandle == hwnd.ClientWindow;

//				if (client)
//					localMonoPoint -= (Size) clientRect.Location;
			}

			if (hwnd == null)
				return EventHandledBy.NativeOS;

			msg.hwnd = hwnd.Handle;
			msg.lParam = (IntPtr) ((ushort) localMonoPoint.Y << 16 | (ushort) localMonoPoint.X);
			msg.pt.x = Driver.mouse_position.X;
			msg.pt.y = Driver.mouse_position.Y;
			msg.refobject = hwnd;

			switch (eventref.Type) {
//			case kEventMouseDown:
			case NSEventType.LeftMouseDown:
			case NSEventType.RightMouseDown:
			case NSEventType.OtherMouseDown:
				if ((uint)NSEventMouseSubtype.Mouse != eventref.Subtype)
					return EventHandledBy.NativeOS;

				UpdateMouseState (button, true);
				msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN) + msgOffset4Button;
				msg.wParam = Driver.GetMousewParam (0);
				if (ClickPending.Pending && (((DateTime.Now.Ticks - ClickPending.Time) < DoubleClickInterval) && 
						(msg.hwnd == ClickPending.Hwnd) && (msg.wParam == ClickPending.wParam) && 
						(msg.lParam == ClickPending.lParam) && (msg.message == ClickPending.Message))) {
					msg.message = (client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK) + msgOffset4Button;
					ClickPending.Pending = false;
				} else {
					ClickPending.Pending = true;
					ClickPending.Hwnd = msg.hwnd;
					ClickPending.Message = msg.message;
					ClickPending.wParam = msg.wParam;
					ClickPending.lParam = msg.lParam;
					ClickPending.Time = DateTime.Now.Ticks;
				}
				break;

//			case kEventMouseUp:
			case NSEventType.LeftMouseUp:
			case NSEventType.RightMouseUp:
			case NSEventType.OtherMouseUp:
				if ((uint)NSEventMouseSubtype.Mouse != eventref.Subtype)
					return EventHandledBy.NativeOS;

				UpdateMouseState (button, false);
				msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP) + msgOffset4Button;
				msg.wParam = Driver.GetMousewParam (0);
				break;

//			case kEventMouseDragged:
//			case kEventMouseMoved:
			case NSEventType.MouseMoved:
			case NSEventType.LeftMouseDragged:
			case NSEventType.RightMouseDragged:
			case NSEventType.OtherMouseDragged:
				if ((uint)NSEventMouseSubtype.Mouse != eventref.Subtype)
					return EventHandledBy.NativeOS;

				if (XplatUICocoa.Grab.Hwnd == IntPtr.Zero) {
					IntPtr ht = IntPtr.Zero;
					if (client) {
						ht = (IntPtr) HitTest.HTCLIENT;
						NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, 
									(IntPtr) HitTest.HTCLIENT);
					} else {
						ht = (IntPtr) NativeWindow.WndProc (hwnd.ClientWindow, Msg.WM_NCHITTEST, 
											IntPtr.Zero, msg.lParam).ToInt32 ();
						NativeWindow.WndProc(hwnd.ClientWindow, Msg.WM_SETCURSOR, msg.hwnd, ht);
					}
				}

				msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
				msg.wParam = Driver.GetMousewParam (0);
				break;

//			case kEventMouseWheelMoved:
//			case kEventMouseScroll:
			case NSEventType.ScrollWheel:
#if DriverDebug
				Console.Error.WriteLine ("NSScrollWheel {0} 0x{1:X}", eventref.Subtype, XplatUICocoa.FocusWindow);
#endif
//				if (Enums.NSMouseEventSubtype != eventref.subtype ())
//					return EventHandledBy.NativeOS;

//				UInt16 axis = 0;
				int delta = (int) (deviceDeltaY (eventref) * 40.0);

//				GetEventParameter (eventref, kEventParamMouseWheelAxis, typeMouseWheelAxis, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (UInt16)), IntPtr.Zero, ref axis);
//				GetEventParameter (eventref, kEventParamMouseWheelDelta, typeLongInteger, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (Int32)), IntPtr.Zero, ref delta);

				if (0 == delta || IntPtr.Zero == XplatUICocoa.FocusWindow)
					return EventHandledBy.NativeOS;

				msg.hwnd = XplatUICocoa.FocusWindow;
				msg.message = Msg.WM_MOUSEWHEEL;
				msg.wParam = Driver.GetMousewParam (delta);
				break;

			case NSEventType.MouseEntered:
			case NSEventType.MouseExited:
			case NSEventType.TabletPoint:
			case NSEventType.TabletProximity:
			default:
				return EventHandledBy.NativeOS;
			}

//			Driver.mouse_position.X = (int) nspoint.x;
//			Driver.mouse_position.Y = (int) nspoint.y;
			return EventHandledBy.PostMessage;
		}
		
		internal bool TranslateMessage (ref MSG msg)
		{
			if (msg.message == Msg.WM_MOUSEMOVE || msg.message == Msg.WM_NCMOUSEMOVE) {
				Hwnd hwnd = Hwnd.ObjectFromHandle (msg.hwnd);
				if (XplatUICocoa.MouseHwnd == null) { 
					Driver.PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
					Cursor.SetCursor (hwnd.Cursor);
				} else if (XplatUICocoa.MouseHwnd.Handle != hwnd.Handle) {
					Driver.PostMessage (XplatUICocoa.MouseHwnd.Handle, Msg.WM_MOUSELEAVE, IntPtr.Zero, 
								IntPtr.Zero);
					Driver.PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
					Cursor.SetCursor (hwnd.Cursor);
				}
				XplatUICocoa.MouseHwnd = hwnd;
			}
			
			return false;
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

		/// <summary>Allows access to smooth-scrolling methods.</summary>
		internal static float deviceDeltaX (NSEvent instance)
		{
			return instance.DeltaX;
		}
		
		/// <summary>Allows access to smooth-scrolling methods.</summary>
		internal static float deviceDeltaY (NSEvent instance)
		{
			return instance.DeltaY;
		}
		
		/// <summary>Allows access to smoth-scrolling methods.</summary>
		internal static float deviceDeltaZ (NSEvent instance)
		{
			return instance.DeltaZ;
		}
	}
}

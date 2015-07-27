//
//ApplicationHandler.cs
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
//using MObjc;
//using MCocoa;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace System.Windows.Forms.CocoaInternal {
	internal class ApplicationHandler : EventHandlerBase, IEventHandler {
		internal ApplicationHandler (XplatUICocoa driver) : base (driver) {}

		public EventHandledBy ProcessEvent (NSObject callref, NSEvent eventref, MonoView handle, uint kind, ref MSG msg)
		{			
			switch (kind) {
			case (uint)NSEventSubtype.ApplicationActivated:
				foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
					if (!utility_window.IsVisible)
						utility_window.OrderFront (callref);

				msg.hwnd = XplatUI.GetActive ();
				msg.message = Msg.WM_ACTIVATEAPP;
				msg.wParam = (IntPtr) WindowActiveFlags.WA_ACTIVE;
				break;

			case (uint)NSEventSubtype.ApplicationDeactivated:
				if (XplatUICocoa.FocusWindow != IntPtr.Zero) {
					Driver.SendMessage (XplatUICocoa.FocusWindow, Msg.WM_KILLFOCUS, 
								IntPtr.Zero, IntPtr.Zero);
				} 

				if (XplatUICocoa.Grab.Hwnd != IntPtr.Zero) {
					Driver.SendMessage (Hwnd.ObjectFromHandle (XplatUICocoa.Grab.Hwnd).Handle, 
						Msg.WM_LBUTTONDOWN, (IntPtr)MsgButtons.MK_LBUTTON, 
						(IntPtr) (Driver.MousePosition.X << 16 | Driver.MousePosition.Y));
				}

				foreach (NSWindow utility_window in XplatUICocoa.UtilityWindows)
					if (utility_window.IsVisible)
						utility_window.OrderOut (callref);

				msg.hwnd = XplatUI.GetActive ();
				msg.message = Msg.WM_ACTIVATEAPP;
				msg.wParam = (IntPtr) WindowActiveFlags.WA_INACTIVE;
				break;

			default:
				return EventHandledBy.NativeOS;
			}

			return EventHandledBy.PostMessage | EventHandledBy.Handler | EventHandledBy.NativeOS;
		}
	}
}

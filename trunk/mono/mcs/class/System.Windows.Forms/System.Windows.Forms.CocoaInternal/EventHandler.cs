//
//EventHandler.cs
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
using System.Diagnostics;
using System.Runtime.InteropServices;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace System.Windows.Forms.CocoaInternal {
	internal delegate EventHandledBy EventDelegate (NSObject callref, NSEvent eventref, MonoView handle);

	internal class EventHandler {
		internal static EventDelegate EventHandlerDelegate = new EventDelegate (EventCallback);
		internal static XplatUICocoa Driver;

		internal const int EVENT_NOT_HANDLED = 0;
		internal const int EVENT_HANDLED = -9874;

		internal static  EventHandledBy EventCallback (NSObject callref, NSEvent eventref, MonoView handle)
		{
			uint kind = (uint)eventref.Type;
			MSG msg = new MSG ();
			IEventHandler handler = null;

			try {
				switch (eventref.Type) {
				case NSEventType.KeyDown:
				case NSEventType.KeyUp:
				case NSEventType.FlagsChanged:
					handler = (IEventHandler) Driver.KeyboardHandler;
					break;

//				case kEventClassWindow:
//					handler = (IEventHandler) Driver.WindowHandler;
//					break;

				case NSEventType.LeftMouseDown:
				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseDown:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseDown:
				case NSEventType.OtherMouseUp:
				case NSEventType.MouseMoved:
				case NSEventType.LeftMouseDragged:
				case NSEventType.RightMouseDragged:
				case NSEventType.OtherMouseDragged:
				case NSEventType.MouseEntered:
				case NSEventType.MouseExited:
				case NSEventType.ScrollWheel:
				case NSEventType.TabletPoint:
				case NSEventType.TabletProximity:
					handler = (IEventHandler) Driver.MouseHandler;
					break;

//				case kEventClassControl:
//					handler = (IEventHandler) Driver.ControlHandler;
//					break;

				case NSEventType.AppKitDefined:
					kind = (uint) eventref.Subtype;
					if ((uint)NSEventSubtype.ApplicationActivated == kind || (uint)NSEventSubtype.ApplicationDeactivated == kind)
						handler = (IEventHandler) Driver.ApplicationHandler;
					else if ((uint)NSEventSubtype.WindowExposed == kind || (uint)NSEventSubtype.WindowMoved == kind || 
							(uint)NSEventSubtype.ScreenChanged == kind)
						handler = (IEventHandler) Driver.WindowHandler;
//					else
//						return EVENT_NOT_HANDLED;
					break;

					/*
				case NSEventType.ApplicationDefined:
					uint klass = (uint) eventref.Data1;
					kind = (uint) eventref.Data2;
#if DriverDebug
					Console.Error.WriteLine ("NSApplicationDefined");
#endif

					switch (klass) {
//					case kEventClassHIObject:
//						handler = (IEventHandler) Driver.HIObjectHandler;
//						break;
					case kEventClassKeyboard:
					case kEventClassTextInput:
						handler = (IEventHandler) Driver.KeyboardHandler;
						break;
					case kEventClassWindow:
						handler = (IEventHandler) Driver.WindowHandler;
#if DriverDebug
						Console.Error.WriteLine ("kEventClassWindow handler {0}", handler);
#endif
						break;
					case kEventClassMouse:
						handler = (IEventHandler) Driver.MouseHandler;
						break;
					case kEventClassControl:
						handler = (IEventHandler) Driver.ControlHandler;
						break;
					case kEventClassApplication:
						handler = (IEventHandler) Driver.ApplicationHandler;
						break;
//					default:
//						return EVENT_NOT_HANDLED;
					}
					break;*/

				case NSEventType.SystemDefined:
					if ((uint)NSSystemDefinedEvents.NSPowerOffEventType == eventref.Subtype) {
						Driver.PostQuitMessage (0);
#if DriverDebug
						Console.Error.WriteLine ("Quit {0}", eventref.Description);
#endif
						return  EventHandledBy.PostMessage;
					}
					break;

//				default:
//					return EVENT_NOT_HANDLED;
				}

				if (null != handler) {
					EventHandledBy results = handler.ProcessEvent (callref, eventref, handle, kind, ref msg);
					if (EventHandledBy.PostMessage == (EventHandledBy.PostMessage & results)) {
						msg.time = (uint) (eventref.Timestamp * 1000.0);
						Driver.EnqueueMessage (msg);
#if DriverDebug
						Console.WriteLine ("{0} {1} {2}", msg, XplatUI.Window (msg.hwnd), 
									eventref.Description);
#endif
					}
					return results;
				}
			}
			catch (Exception e)
			{
				StackTrace st = new StackTrace (1, true);
				Console.Error.WriteLine ("Exception while handling {0}", eventref.Description);
				Console.Error.WriteLine ("{0}", e);
				Console.Error.WriteLine ("{0}", st.ToString ());
				throw;
			}

#if DriverDebug
			Console.Error.WriteLine ("Unhandled {0}", eventref.Description);
#endif
			return EventHandledBy.NativeOS;
		}

		internal static bool TranslateMessage (ref MSG msg) {
			bool result = false;
 
			if (!result)
				result = Driver.KeyboardHandler.TranslateMessage (ref msg);
			if (!result)
				result = Driver.MouseHandler.TranslateMessage (ref msg);

 			return result;
		}

		internal static void InstallApplicationHandler () {
//			InstallEventHandler (GetApplicationEventTarget (), EventHandlerDelegate, (uint)ApplicationEvents.Length, ApplicationEvents, IntPtr.Zero, IntPtr.Zero);
//			NSApplication.sharedApplication ().setDelegate (new Cocoa.MonoNSDelegate ());
		}

//		internal static void InstallControlHandler (IntPtr control) {
//			InstallEventHandler (GetControlEventTarget (control), EventHandlerDelegate, (uint)ControlEvents.Length, ControlEvents, control, IntPtr.Zero);
//		}
		
//		internal static void InstallWindowHandler (IntPtr window) {
//			InstallEventHandler (GetWindowEventTarget (window), EventHandlerDelegate, (uint)WindowEvents.Length, WindowEvents, window, IntPtr.Zero);
//		}

//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern IntPtr GetApplicationEventTarget ();
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		internal static extern IntPtr GetControlEventTarget (IntPtr control);
//		[DllImport("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		internal static extern IntPtr GetWindowEventTarget (IntPtr window);

//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		internal static extern uint GetEventClass (IntPtr eventref);
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern uint GetEventKind (IntPtr eventref);
		
//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int InstallEventHandler (IntPtr window, EventDelegate event_handler, uint count, EventTypeSpec [] types, IntPtr user_data, IntPtr handlerref);
	}
}

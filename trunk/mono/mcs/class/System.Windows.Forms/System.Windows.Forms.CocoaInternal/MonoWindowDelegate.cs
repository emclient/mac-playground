//
//MonoWindow.cs
// 
//Author:
//	Filip Navara <navara@emclient.com>
//
//Copyright (c) 2015 Filip Navara
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
using NSRect = System.Drawing.RectangleF;
using System.Drawing;

namespace System.Windows.Forms.CocoaInternal
{
	internal class MonoWindowDelegate : NSWindowDelegate
	{
		XplatUICocoa driver;

		private MonoWindowDelegate (IntPtr instance) : base (instance)
		{
		}

		public MonoWindowDelegate (XplatUICocoa driver)
		{
			this.driver = driver;
		}

		public override SizeF WillResize (NSWindow sender, SizeF toFrameSize)
		{
			return base.WillResize (sender, toFrameSize);
		}

		public override bool WindowShouldClose (NSObject sender)
		{
			// FIXME: Send WM_CLOSING/WM_CLOSE
			return true;
			/*
			if (null == EventHandler.EventHandlerDelegate)
				return true;

			NSWindow winWrap = Window;
			NSEvent evtRef = NSApplication.SharedApplication.CurrentEvent;
			evtRef = NSEvent.OtherEvent(
				NSEventType.ApplicationDefined, winWrap.Frame.Location, evtRef.ModifierFlags, evtRef.Timestamp, 
					winWrap.WindowNumber, winWrap.GraphicsContext, 0, (int) EventHandler.kEventClassWindow, 
					(int) WindowHandler.kEventWindowClose);

//			Console.Error.WriteLine ("Close window {0}", evtRef.Description);
			return EventHandledBy.NativeOS == (EventHandledBy.NativeOS & 
						EventHandler.EventHandlerDelegate (sender, evtRef, this));*/
		}
	}
}

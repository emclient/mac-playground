//
//MonoView.cs
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

using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using NSRect = System.Drawing.RectangleF;
using System.Drawing;

namespace System.Windows.Forms.CocoaInternal
{

	//[ExportClass("MonoView", "NSView")]
	internal class MonoView : NSView
	{
		XplatUICocoa driver;

		private MonoView (IntPtr instance) : base (instance)
		{
		}

		public MonoView (XplatUICocoa driver, NSRect frameRect) : base(frameRect)
		{
			this.driver = driver;
		}

//		public new bool isFlipped ()
//		{
//			return true;
//		}

		public override bool TryToPerformwith (MonoMac.ObjCRuntime.Selector anAction, NSObject anObject)
		{
			NSEvent theEvent = NSApplication.SharedApplication.CurrentEvent;
			Console.Error.WriteLine ("{3}: Action {0}, Object {1}, Event {2}", anAction, anObject, theEvent, this);
			if (null != EventHandler.EventHandlerDelegate)
				if (EventHandledBy.NativeOS != (EventHandledBy.NativeOS & 
						EventHandler.EventHandlerDelegate (anObject, theEvent, this)))
					return true;

			return base.TryToPerformwith (anAction, anObject);
		}

		public override void DrawRect (NSRect dirtyRect)
		{
			Hwnd hwnd = Hwnd.ObjectFromWindow (Handle);
			NSRect nsbounds = dirtyRect;
			bool client = hwnd.ClientWindow == Handle;
			Rectangle bounds = driver.NativeToMonoFramed (nsbounds, Frame.Size.Height);
			Rectangle clientBounds = bounds;
			bool nonclient = ! client;

			if (!hwnd.visible) {
				if (client)
					hwnd.expose_pending = false;
				if (nonclient)
					hwnd.nc_expose_pending = false;
				return;
			}

			if (nonclient) {
				DrawBorders (hwnd);
			}

			if (nonclient)
				driver.AddExpose (hwnd, false, bounds);
			if (client)
				driver.AddExpose (hwnd, true, clientBounds);
		}

		private void DrawBorders (Hwnd hwnd) {
			Graphics g;

			switch (hwnd.BorderStyle) {
			case FormBorderStyle.Fixed3D:
				using (g = Graphics.FromHwnd (hwnd.WholeWindow)) {
					if (hwnd.border_static)
						ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Border3DStyle.SunkenOuter);
					else
						ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Border3DStyle.Sunken);
				}
				break;

			case FormBorderStyle.FixedSingle:
				using (g = Graphics.FromHwnd (hwnd.WholeWindow))
					ControlPaint.DrawBorder (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Color.Black, ButtonBorderStyle.Solid);
				break;
			}
		}
	}
}

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
		private MonoView (IntPtr instance) : base (instance)
		{
		}

		public MonoView () : base ()
		{
		}

		public MonoView (NSRect frameRect) : base(frameRect)
		{
		}

		public static MonoView CreateWithFrame (NSRect frameRect)
		{
			return new MonoView (frameRect);
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

		public bool windowShouldClose (NSObject sender)
		{
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
						EventHandler.EventHandlerDelegate (sender, evtRef, this));
		}


/*		public override void DrawRect (NSRect dirtyRect)
		{
			if (null == EventHandler.EventHandlerDelegate)
				return;

			NSWindow winWrap = Window;
			NSEvent evtRef = NSApplication.SharedApplication.CurrentEvent;
			evtRef = NSEvent.OtherEvent(
				NSEventType.ApplicationDefined, winWrap.Frame.Location, KeyboardHandler.key_modifiers, 
					((UInt32) Environment.TickCount) / 1000.0, winWrap.WindowNumber, winWrap.GraphicsContext, 0, 
					(int) EventHandler.kEventClassControl, (int) ControlHandler.kEventControlDraw);

//			Console.Error.WriteLine ("drawRect ({0}) = {1}", dirtyRect, evtRef.Description);
			EventHandler.EventHandlerDelegate (NSValue.FromRectangleF(dirtyRect), evtRef, this);
		}
*/
	}
}

//
//MonoNSDelegate.cs
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

namespace System.Windows.Forms.CocoaInternal
{

	//[ExportClass("MonoNSDelegate", "NSResponder")]
	internal sealed class MonoNSDelegate : NSResponder
	{

		private MonoNSDelegate (IntPtr instance) : base (instance)
		{
		}

		public MonoNSDelegate () //: base (NSObject.AllocAndInitInstance ("MonoNSDelegate"))
		{
		}


		public override bool TryToPerformwith (MonoMac.ObjCRuntime.Selector anAction, NSObject anObject)
		{
			NSEvent theEvent = NSApplication.SharedApplication.CurrentEvent;
			Console.Error.WriteLine ("{3}: Action {0}, Object {1}, Event {2}", anAction, anObject, theEvent, this);
			if (null != EventHandler.EventHandlerDelegate)
				if (EventHandledBy.Handler == EventHandler.EventHandlerDelegate (anObject, theEvent, null))
					return true;

			return base.TryToPerformwith (anAction, anObject);
		}
	}
}

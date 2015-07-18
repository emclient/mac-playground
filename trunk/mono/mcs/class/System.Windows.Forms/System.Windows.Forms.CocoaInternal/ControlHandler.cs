//
//ControlHandler.cs
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
using System.Drawing;
using System.Runtime.InteropServices;
using MonoMac.Foundation;
using MonoMac.AppKit;
using NSRect = System.Drawing.RectangleF;

namespace System.Windows.Forms.CocoaInternal {
	internal class ControlHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventControlInitialize = 1000;
		internal const uint kEventControlDispose = 1001;
		internal const uint kEventControlGetOptimalBounds = 1003;
		internal const uint kEventControlDefInitialize = kEventControlInitialize;
		internal const uint kEventControlDefDispose = kEventControlDispose;
		internal const uint kEventControlHit = 1;
		internal const uint kEventControlSimulateHit = 2;
		internal const uint kEventControlHitTest = 3;
		internal const uint kEventControlDraw = 4;
		internal const uint kEventControlApplyBackground = 5;
		internal const uint kEventControlApplyTextColor = 6;
		internal const uint kEventControlSetFocusPart = 7;
		internal const uint kEventControlGetFocusPart = 8;
		internal const uint kEventControlActivate = 9;
		internal const uint kEventControlDeactivate = 10;
		internal const uint kEventControlSetCursor = 11;
		internal const uint kEventControlContextualMenuClick = 12;
		internal const uint kEventControlClick = 13;
		internal const uint kEventControlGetNextFocusCandidate = 14;
		internal const uint kEventControlGetAutoToggleValue = 15;
		internal const uint kEventControlInterceptSubviewClick = 16;
		internal const uint kEventControlGetClickActivation = 17;
		internal const uint kEventControlDragEnter = 18;
		internal const uint kEventControlDragWithin = 19;
		internal const uint kEventControlDragLeave = 20;
		internal const uint kEventControlDragReceive = 21;
		internal const uint kEventControlInvalidateForSizeChange = 22;
		internal const uint kEventControlTrackingAreaEntered = 23;
		internal const uint kEventControlTrackingAreaExited = 24;
		internal const uint kEventControlTrack = 51;
		internal const uint kEventControlGetScrollToHereStartPoint = 52;
		internal const uint kEventControlGetIndicatorDragConstraint = 53;
		internal const uint kEventControlIndicatorMoved = 54;
		internal const uint kEventControlGhostingFinished = 55;
		internal const uint kEventControlGetActionProcPart = 56;
		internal const uint kEventControlGetPartRegion = 101;
		internal const uint kEventControlGetPartBounds = 102;
		internal const uint kEventControlSetData = 103;
		internal const uint kEventControlGetData = 104;
		internal const uint kEventControlGetSizeConstraints= 105;
		internal const uint kEventControlGetFrameMetrics = 106;
		internal const uint kEventControlValueFieldChanged = 151;
		internal const uint kEventControlAddedSubControl = 152;
		internal const uint kEventControlRemovingSubControl = 153;
		internal const uint kEventControlBoundsChanged = 154;
		internal const uint kEventControlVisibilityChanged = 157;
		internal const uint kEventControlTitleChanged = 158;
		internal const uint kEventControlOwningWindowChanged = 159;
		internal const uint kEventControlHiliteChanged = 160;
		internal const uint kEventControlEnabledStateChanged = 161;
		internal const uint kEventControlLayoutInfoChanged = 162;
		internal const uint kEventControlArbitraryMessage = 201;

		internal const uint kEventParamCGContextRef = 1668183160;
		internal const uint kEventParamDirectObject = 757935405;
		internal const uint kEventParamControlPart = 1668313716;
		internal const uint kEventParamControlLikesDrag = 1668047975;
		internal const uint kEventParamRgnHandle = 1919381096;
		internal const uint typeControlRef = 1668575852;
		internal const uint typeCGContextRef = 1668183160;
		internal const uint typeQDPoint = 1363439732;
		internal const uint typeQDRgnHandle = 1919381096;
		internal const uint typeControlPartCode = 1668313716;
		internal const uint typeBoolean = 1651470188;

		internal ControlHandler (XplatUICocoa driver) : base (driver) {}

		public EventHandledBy ProcessEvent (NSObject callref, NSEvent eventref, MonoView vuWrap, uint kind, ref MSG msg)
		{
			IntPtr handle = vuWrap.Handle;
			Hwnd hwnd;
			bool client;

//			GetEventParameter (eventref, kEventParamDirectObject, typeControlRef, IntPtr.Zero, (uint) Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref handle);
			hwnd = Hwnd.ObjectFromWindow (handle);

			if (hwnd == null)
				return EventHandledBy.NativeOS;

			msg.hwnd = hwnd.Handle;
			client = hwnd.ClientWindow == handle;

			switch (kind) {
			case kEventControlVisibilityChanged: {
				if (client) {
					msg.message = Msg.WM_SHOWWINDOW;
					msg.lParam = (IntPtr) 0;
					msg.wParam = (vuWrap.IsHiddenOrHasHiddenAncestor ? (IntPtr) 0 : (IntPtr) 1);
					return EventHandledBy.PostMessage;
				}
				return EventHandledBy.NativeOS;
			}
			case kEventControlBoundsChanged: {
				Driver.HwndPositionFromNative (hwnd);
				if (!client)
					Driver.PerformNCCalc (hwnd);

				msg.message = Msg.WM_WINDOWPOSCHANGED;
				msg.hwnd = hwnd.Handle;

				return EventHandledBy.PostMessage | EventHandledBy.Handler;
			}
			case kEventControlGetFocusPart: {
				short pcode = 0;
				//SetEventParameter (eventref, kEventParamControlPart, typeControlPartCode, (uint)Marshal.SizeOf (typeof (short)), ref pcode);
				return EventHandledBy.Handler | EventHandledBy.NativeOS;
			}
			case kEventControlDragEnter: 
			case kEventControlDragWithin: 
			case kEventControlDragLeave: 
			case kEventControlDragReceive: 
				return Dnd.HandleEvent (callref, eventref, vuWrap, kind, ref msg) ? 
						EventHandledBy.PostMessage : EventHandledBy.NativeOS;
			}

			return EventHandledBy.NativeOS;
		}
	}
}

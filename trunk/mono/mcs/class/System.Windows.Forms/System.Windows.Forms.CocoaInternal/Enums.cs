//
//Enums.cs
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
// "Software",, to deal in the Software without restriction, including
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
//	Geoff Norton  <gnorton@customerdna.com>
//

using System;

namespace System.Windows.Forms.CocoaInternal
{
	internal enum WindowClass : uint
	{
		kAlertWindowClass = 1,
		kMovableAlertWindowClass = 2,
		kModalWindowClass = 3,
		kMovableModalWindowClass = 4,
		kFloatingWindowClass = 5,
		kDocumentWindowClass = 6,
		kUtilityWindowClass = 8,
		kHelpWindowClass = 10,
		kSheetWindowClass = 11,
		kToolbarWindowClass = 12,
		kPlainWindowClass = 13,
		kOverlayWindowClass = 14,
		kSheetAlertWindowClass = 15,
		kAltPlainWindowClass = 16,
		kDrawerWindowClass = 20,
		kAllWindowClasses = 0xffffffffu
	}

	internal enum WindowAttributes : uint
	{
		kWindowNoAttributes = 0,
		kWindowCloseBoxAttribute = (1u << 0),
		kWindowHorizontalZoomAttribute = (1u << 1),
		kWindowVerticalZoomAttribute = (1u << 2),
		kWindowFullZoomAttribute = (kWindowVerticalZoomAttribute | kWindowHorizontalZoomAttribute),
		kWindowCollapseBoxAttribute = (1u << 3),
		kWindowResizableAttribute = (1u << 4),
		kWindowSideTitlebarAttribute = (1u << 5),
		kWindowToolbarButtonAttribute = (1u << 6),
		kWindowMetalAttribute = (1u << 8),
		kWindowNoUpdatesAttribute = (1u << 16),
		kWindowNoActivatesAttribute = (1u << 17),
		kWindowOpaqueForEventsAttribute = (1u << 18),
		kWindowCompositingAttribute = (1u << 19),
		kWindowNoShadowAttribute = (1u << 21),
		kWindowHideOnSuspendAttribute = (1u << 24),
		kWindowStandardHandlerAttribute = (1u << 25),
		kWindowHideOnFullScreenAttribute = (1u << 26),
		kWindowInWindowMenuAttribute = (1u << 27),
		kWindowLiveResizeAttribute = (1u << 28),
		kWindowIgnoreClicksAttribute = (1u << 29),
		kWindowNoConstrainAttribute = (1u << 31),
		kWindowStandardDocumentAttributes = (kWindowCloseBoxAttribute | kWindowFullZoomAttribute | kWindowCollapseBoxAttribute | kWindowResizableAttribute),
		kWindowStandardFloatingAttributes = (kWindowCloseBoxAttribute | kWindowCollapseBoxAttribute)
	}

//	internal enum WindowStyle : uint
//	{
//		NSBorderlessWindowMask = 0,
//		NSTitledWindowMask = 1 << 0,
//		NSClosableWindowMask = 1 << 1,
//		NSMiniaturizableWindowMask = 1 << 2,
//		NSResizableWindowMask = 1 << 3,
//		NSTexturedBackgroundWindowMask = 1 << 8
//	}

	internal enum ThemeCursor : uint
	{
		kThemeArrowCursor = 0,
		kThemeCopyArrowCursor = 1,
		kThemeAliasArrowCursor = 2,
		kThemeContextualMenuArrowCursor = 3,
		kThemeIBeamCursor = 4,
		kThemeCrossCursor = 5,
		kThemePlusCursor = 6,
		kThemeWatchCursor = 7,
		kThemeClosedHandCursor = 8,
		kThemeOpenHandCursor = 9,
		kThemePointingHandCursor = 10,
		kThemeCountingUpHandCursor = 11,
		kThemeCountingDownHandCursor = 12,
		kThemeCountingUpAndDownHandCursor = 13,
		kThemeSpinningCursor = 14,
		kThemeResizeLeftCursor = 15,
		kThemeResizeRightCursor = 16,
		kThemeResizeLeftRightCursor = 17,
		kThemeNotAllowedCursor = 18
	}

	internal enum MouseTrackingResult : ushort
	{
		kMouseTrackingMouseDown = 1,
		kMouseTrackingMouseUp = 2,
		kMouseTrackingMouseExited = 3,
		kMouseTrackingMouseEntered = 4,
		kMouseTrackingMouseDragged = 5,
		kMouseTrackingKeyModifiersChanged = 6,
		kMouseTrackingUserCancelled = 7,
		kMouseTrackingTimedOut = 8,
		kMouseTrackingMouseMoved = 9
	}

	internal enum NSWindowLevel : int
	{
		NSBaseWindowLevel = 0,
		NSMinimumWindowLevel,
		NSDesktopWindowLevel,
		NSBackstopMenuLevel,
		NSNormalWindowLevel,
		NSFloatingWindowLevel,
		NSTornOffMenuWindowLevel,
		NSDockWindowLevel,
		NSMainMenuWindowLevel,
		NSStatusWindowLevel,
		NSModalPanelWindowLevel,
		NSPopUpMenuWindowLevel,
		NSDraggingWindowLevel,
		NSScreenSaverWindowLevel,
		NSMaximumWindowLevel,
		NSOverlayWindowLevel,
		NSHelpWindowLevel,
		NSUtilityWindowLevel,
		NSDesktopIconWindowLevel,
		NSCursorWindowLevel,
		NSNumberOfWindowLevels
	}

	internal enum NSMouseButtons : long {
		None		= -1,
		Left		= 0,
		Right		= 1,
		Middle		= 2,
		X		= 3,
		Excessive	= 4
	}
}

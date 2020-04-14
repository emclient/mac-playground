//
//System.Drawing.cocoaFunctions.cs
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
//This document was originally created as a copy of carbonFunctions.cs 
// and retains many features thereof.
//

//
// System.Drawing.carbonFunctions.cs
//
// Authors:
//      Geoff Norton (gnorton@customerdna.com>
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
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

#undef DEBUG_CLIPPING

using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace System.Drawing {

	using NSRect = RectangleF;

	internal interface INativeContex : IDisposable
	{
		IntPtr CreateGdipGraphics();
		void Flush ();
	}

	[SuppressUnmanagedCodeSecurity]
#if NET_2_0
	static
#else
	sealed
#endif
	internal class Cocoa {
//		internal static Hashtable contextReference = new Hashtable ();
//		internal static object lockobj = new object ();

		internal static Delegate hwnd_delegate;

#if DEBUG_CLIPPING
		internal static float red = 1.0f;
		internal static float green = 0.0f;
		internal static float blue = 0.0f;
		internal static int debug_threshold = 1;
#endif

		static Cocoa () {
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				if (String.Equals (asm.GetName ().Name, "System.Windows.Forms")) {
					Type driver_type = asm.GetType ("System.Windows.Forms.XplatUICocoa");
					if (driver_type != null) {
						hwnd_delegate = (Delegate) driver_type.GetField ("HwndDelegate", BindingFlags.NonPublic | BindingFlags.Static).GetValue (null);
					}
				}
			}
		}

		internal static INativeContex GetCGContextForView (IntPtr handle) {
//			IntPtr context = IntPtr.Zero;
			NSView focusWindow = null;
			// JV - je to OK??
			//NSObject whoWrapper = NSObject.Lookup (handle);
			NSObject whoWrapper = MonoMac.ObjCRuntime.Runtime.GetNSObject(handle);
			NSView viewWrapper = whoWrapper as NSView;
			NSWindow windowWrapper = null;

			if (null == viewWrapper)
				windowWrapper = whoWrapper as NSWindow;
			else
				windowWrapper = viewWrapper.Window;

			IntPtr window = windowWrapper.Handle;
			NSGraphicsContext gcw = null;

			if (handle == IntPtr.Zero || window == IntPtr.Zero) {
				// FIXME: Can we actually get a CGContextRef for the desktop?  this makes context IntPtr.Zero
				gcw = NSApplication.SharedApplication.Context;
//				context = gcw.graphicsPort();

				var desktop_bounds = NSRect.Empty;
				// NSScreen.mainScreen () returns the screen the the user is currently interacting with.
				// To get the screen identified by CGMainDisplayID (), get the 0th element of this array.
				NSScreen[] screens = NSScreen.Screens;

				if (null != screens && 0 < screens.Length) {
					NSScreen screenWrap = screens[0];
					desktop_bounds = screenWrap.Frame;
				}

				return new CocoaContext (focusWindow, gcw, (int) desktop_bounds.Width, 
							 (int) desktop_bounds.Height);
			}

			if (null != viewWrapper && viewWrapper != NSView.FocusView()) {
				if (! viewWrapper.LockFocusIfCanDraw())
					return null;

				focusWindow = viewWrapper;
			}

			gcw = windowWrapper.GraphicsContext;
			gcw.SaveGraphicsState();
//			context = gcw.graphicsPort();

//			NSRect winRect = windowWrapper.frame();
//			QDRect window_bounds = new QDRect (winRect.Top, winRect.Left, winRect.Bottom, winRect.Right);

			var vuRect = windowWrapper.Frame;
			if (null != viewWrapper) {
				vuRect = viewWrapper.Bounds;
				vuRect = viewWrapper.ConvertRectToView(vuRect, null);
			}
//			Rect view_bounds = new Rect (vuRect.origin.x, vuRect.origin.y, vuRect.size.width, vuRect.size.height);
			
			if (vuRect.Height < 0)
				vuRect.Height = 0;
			if (vuRect.Width < 0)
				vuRect.Width = 0;

//ASSUMPTION! lockFocus did the translating and clipping.
//ASSUMPTION! The NSView isFlipped.
//			CGContextTranslateCTM (context, view_bounds.origin.x, (window_bounds.bottom - window_bounds.top) - (view_bounds.origin.y + view_bounds.size.height));
//
//			// Create the original rect path and clip to it
//			Rect rc_clip = new Rect (0, 0, view_bounds.size.width, view_bounds.size.height);
//
//
//			Rectangle [] clip_rectangles = (Rectangle []) hwnd_delegate.DynamicInvoke (new object [] {handle});
//			if (clip_rectangles != null && clip_rectangles.Length > 0) {
//				int length = clip_rectangles.Length;
//				
//				CGContextBeginPath (context);
//				CGContextAddRect (context, rc_clip);
//
//				for (int i = 0; i < length; i++) {
//					CGContextAddRect (context, new Rect (clip_rectangles [i].X, view_bounds.size.height - clip_rectangles [i].Y - clip_rectangles [i].Height, clip_rectangles [i].Width, clip_rectangles [i].Height));
//				}
//				CGContextClosePath (context);
//				CGContextEOClip (context);
//#if DEBUG_CLIPPING
//				if (clip_rectangles.Length >= debug_threshold) {
//					CGContextSetRGBFillColor (context, red, green, blue, 0.5f);
//					CGContextFillRect (context, rc_clip);
//					CGContextFlush (context);
//					System.Threading.Thread.Sleep (500);
//					if (red == 1.0f) { red = 0.0f; blue = 1.0f; } 
//					else if (blue == 1.0f) { blue = 0.0f; green = 1.0f; } 
//					else if (green == 1.0f) { green = 0.0f; red = 1.0f; } 
//				}
//#endif
//			} else {
//				CGContextBeginPath (context);
//				CGContextAddRect (context, rc_clip);
//				CGContextClosePath (context);
//				CGContextClip (context);
//			}

			return new CocoaContext (focusWindow, gcw, (int) vuRect.Width, (int) vuRect.Height);
		}

//		internal static IntPtr GetContext (IntPtr port) {
//			IntPtr context = IntPtr.Zero;
//
//			lock (lockobj) { 
//#if FALSE
//				if (contextReference [port] != null) {
//					CreateCGContextForPort (port, ref context);
//				} else {
//					QDBeginCGContext (port, ref context);
//					contextReference [port] = context;
//				}
//#else
//				CreateCGContextForPort (port, ref context);
//#endif
//			}
//
//			return context;
//		}

//		internal static void ReleaseContext (CocoaContext context) {
//			NSGraphicsContext.restoreGraphicsState ();
//
//			if (0 != context.focusWindow) {
//				NSView viewWrapper = (NSView) NSObject.Lookup (handle);
//				viewWrapper.unlockFocus();
//			}
//
////			lock (lockobj) { 
////#if FALSE
////				if (contextReference [port] != null && context == (IntPtr) contextReference [port]) { 
////					QDEndCGContext (port, ref context);
////					contextReference [port] = null;
////				} else {
////					CFRelease (context);
////				}
////#else
////				CFRelease (context);
////#endif
////			}
//		}

		#region Cocoa Methods
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_getClass(string className); 
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector, string argument);  
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern void objc_msgSend_stret(ref Rect arect, IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern IntPtr sel_registerName(string selectorName);         
		#endregion

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr CGMainDisplayID ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern Rect CGDisplayBounds (IntPtr display);

//		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern int HIViewGetBounds (IntPtr vHnd, ref Rect r);
//		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern int HIViewConvertRect (ref Rect r, IntPtr a, IntPtr b);

//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern IntPtr GetControlOwner (IntPtr aView);

//		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern int GetWindowBounds (IntPtr wHnd, uint reg, ref QDRect rect);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern IntPtr GetWindowPort (IntPtr hWnd);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern IntPtr GetQDGlobalsThePort ();
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CreateCGContextForPort (IntPtr port, ref IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CFRelease (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void QDBeginCGContext (IntPtr port, ref IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void QDEndCGContext (IntPtr port, ref IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern int CGContextClipToRect (IntPtr context, Rect clip);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern int CGContextClipToRects (IntPtr context, Rect [] clip_rects, int count);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextTranslateCTM (IntPtr context, float tx, float ty);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextScaleCTM (IntPtr context, float x, float y);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextFlush (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextSynchronize (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern IntPtr CGPathCreateMutable ();
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGPathAddRects (IntPtr path, IntPtr _void, Rect [] rects, int count);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGPathAddRect (IntPtr path, IntPtr _void, Rect rect);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextAddRects (IntPtr context, Rect [] rects, int count);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextAddRect (IntPtr context, Rect rect);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextBeginPath (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextClosePath (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextAddPath (IntPtr context, IntPtr path);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextClip (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextEOClip (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextEOFillPath (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextSaveGState (IntPtr context);
//		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
//		internal static extern void CGContextRestoreGState (IntPtr context);

#if DEBUG_CLIPPING
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextSetRGBFillColor (IntPtr context, float red, float green, float blue, float alpha);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextFillRect (IntPtr context, Rect rect);
#endif
		private struct CocoaContext : INativeContex
		{
			public NSView focusWindow;
			public NSGraphicsContext ctx;
			public int width;
			public int height;

			public CocoaContext (NSView focusWindow, NSGraphicsContext ctx, int width, int height)
			{
				this.focusWindow = focusWindow;
				this.ctx = ctx;
				this.width = width;
				this.height = height;
			}

			public IntPtr CreateGdipGraphics()
			{
				IntPtr graphics = IntPtr.Zero;
				//GDIPlus.GdipCreateFromContext_macosx (ctx.GraphicsPort, width, height, out graphics);
				GDIPlus.GdipCreateFromContext_macosx (ctx.GraphicsPort.Handle, width, height, out graphics);
				return graphics;
			}

			public void Flush ()
			{
				ctx.FlushGraphics();
			}

			public void Dispose ()
			{
				if (null == ctx)
					return;

				Flush ();
				NSGraphicsContext.GlobalRestoreGraphicsState(); //   RestoreGraphicsState_c ();

				if (null != focusWindow)
					focusWindow.UnlockFocus();

				ctx = null;
				focusWindow = null;
			}
		}
	}  // end class Cocoa
}  // end namespace

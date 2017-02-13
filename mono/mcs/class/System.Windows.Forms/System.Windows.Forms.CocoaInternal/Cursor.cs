//
//Cursor.cs
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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Geoff Norton (gnorton@novell.com)
//
//


using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms.CocoaInternal {
	internal class Cursor {
		internal static CocoaCursor defcur = new CocoaCursor (StdCursor.Default);

                internal static Bitmap DefineStdCursorBitmap (StdCursor id) {
			// FIXME
			return new Bitmap (16, 16);
                }
                internal static IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			CocoaCursor cc = new CocoaCursor (bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);

			return (IntPtr) GCHandle.Alloc (cc);
                }
		internal static IntPtr DefineStdCursor (StdCursor id) {
			CocoaCursor cc = new CocoaCursor (id);
		
			return (IntPtr) GCHandle.Alloc (cc);
		}
		internal static void SetCursor (IntPtr cursor) {
			if (cursor == IntPtr.Zero) {
				defcur.SetCursor ();
				return;
			}

			CocoaCursor cc = (CocoaCursor) ((GCHandle) cursor).Target;

			cc.SetCursor ();
		}
	}

	internal struct CocoaCursor {
		private Bitmap bmp;
		private Bitmap mask;
		private Color cursor_color;
		private Color mask_color;
		private int hot_x;
		private int hot_y;
		private StdCursor id;
		private bool standard;

                public CocoaCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			this.id = StdCursor.Default;
			this.bmp = bitmap;
			this.mask = mask;
			this.cursor_color = cursor_pixel;
			this.mask_color = mask_pixel;
			this.hot_x = xHotSpot;
			this.hot_y = yHotSpot;
			standard = true;
		}

		public CocoaCursor (StdCursor id) {
			this.id = id;
			this.bmp = null;
			this.mask = null;
			this.cursor_color = Color.Black;
			this.mask_color = Color.Black;
			this.hot_x = 0;
			this.hot_y = 0;
			standard = true;
		}

		public StdCursor StdCursor {
			get {
				return id;
			}
		}

		public Bitmap Bitmap {
			get { 
				return bmp;
			}
		}

		public Bitmap Mask {
			get { 
				return mask;
			}
		}

		public Color CursorColor {
			get {
				return cursor_color;
			}
		}

		public Color MaskColor {
			get {
				return mask_color;
			}
		}

		public int HotSpotX {
			get { 
				return hot_x;
			}
		}

		public int HotSpotY {
			get { 
				return hot_y;
			}
		}

		public void SetCursor () {
			if (standard)
				SetStandardCursor ();
			else	
				SetCustomCursor ();
		}

		public void SetCustomCursor () {
			throw new NotImplementedException ("We dont support custom cursors yet");
		}

		public void SetStandardCursor () {
			switch (id) {
				case StdCursor.AppStarting:
//					NSCursor.spinningCursor().set();
					break;
				case StdCursor.Arrow:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.Cross:
					NSCursor.CrosshairCursor.Set();
					break;
				case StdCursor.Default:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.Hand:
					NSCursor.OpenHandCursor.Set();
					break;
				case StdCursor.Help:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.HSplit:
					NSCursor.ResizeUpDownCursor.Set();
					break;
				case StdCursor.IBeam:
					NSCursor.IBeamCursor.Set();
					break;
				case StdCursor.No:
					NSCursor.DisappearingItemCursor.Set();
					break;
				case StdCursor.NoMove2D:
					NSCursor.DisappearingItemCursor.Set();
					break;
				case StdCursor.NoMoveHoriz:
					NSCursor.DisappearingItemCursor.Set();
					break;
				case StdCursor.NoMoveVert:
					NSCursor.DisappearingItemCursor.Set();
					break;
				case StdCursor.PanEast:
					NSCursor.ResizeRightCursor.Set();
					break;
				case StdCursor.PanNE:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.PanNorth:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.PanNW:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.PanSE:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.PanSouth:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.PanSW:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.PanWest:
					NSCursor.ResizeLeftCursor.Set();
					break;
				case StdCursor.SizeAll:
					NSCursor.ResizeLeftRightCursor.Set();
					break;
				case StdCursor.SizeNESW:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.SizeNS:
					NSCursor.ResizeUpDownCursor.Set();
					break;
				case StdCursor.SizeNWSE:
					NSCursor.ArrowCursor.Set();
					break;
				case StdCursor.SizeWE:
					NSCursor.ResizeLeftRightCursor.Set();
					break;
				case StdCursor.UpArrow:
					NSCursor.ResizeUpCursor.Set();
					break;
				case StdCursor.VSplit:
					NSCursor.ResizeLeftRightCursor.Set();
					break;
				case StdCursor.WaitCursor:
//					NSCursor.SpinningCursor().set();
					break;
				default:
					NSCursor.ArrowCursor.Set();
					break;
			}
			return;
		}

//		[DllImport ("/System/Library/Frameworks/Cocoa.framework/Versions/Current/Cocoa")]
//		static extern int SetThemeCursor (ThemeCursor cursor);

	}
}

// 
// CGGeometry.cs: CGGeometry.h helpers
//
// Authors: Mono Team
//     
// Copyright 2010 Novell, Inc
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

using System;
using System.Runtime.InteropServices;

using MonoMac;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;

#if MAC64
using nint = System.Int64;
using nuint = System.UInt64;
using nfloat = System.Double;
#else
using nint = System.Int32;
using nuint = System.UInt32;
using nfloat = System.Single;
#if SDCOMPAT
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using CGRect = System.Drawing.RectangleF;
#endif
#endif

namespace MonoMac.CoreGraphics {

	[Since (3,2)]
	public enum NSRectEdge {
		MinXEdge,
		MinYEdge,
		MaxXEdge,
		MaxYEdge,
	}

	[Since (3,2)]
	public static class RectangleFExtensions {

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern nfloat CGRectGetMinX (CGRect rect);
		public static nfloat GetMinX (this CGRect self)
		{
			return CGRectGetMinX (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern nfloat CGRectGetMidX (CGRect rect);
		public static nfloat GetMidX (this CGRect self)
		{
			return CGRectGetMidX (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern nfloat CGRectGetMaxX (CGRect rect);
		public static nfloat GetMaxX (this CGRect self)
		{
			return CGRectGetMaxX (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern nfloat CGRectGetMinY (CGRect rect);
		public static nfloat GetMinY (this CGRect self)
		{
			return CGRectGetMinY (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern nfloat CGRectGetMidY (CGRect rect);
		public static nfloat GetMidY (this CGRect self)
		{
			return CGRectGetMidY (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern nfloat CGRectGetMaxY (CGRect rect);
		public static nfloat GetMaxY (this CGRect self)
		{
			return CGRectGetMaxY (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern CGRect CGRectStandardize (CGRect rect);
		public static CGRect Standardize (this CGRect self)
		{
			return CGRectStandardize (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern bool CGRectIsNull (CGRect rect);
		public static bool IsNull (this CGRect self)
		{
			return CGRectIsNull (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern bool CGRectIsInfinite (CGRect rect);
		public static bool IsInfinite (this CGRect self)
		{
			return CGRectIsNull (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern CGRect CGRectInset (CGRect rect, nfloat dx, nfloat dy);
		public static CGRect Inset (this CGRect self, nfloat dx, nfloat dy)
		{
			return CGRectInset (self, dx, dy);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern CGRect CGRectIntegral (CGRect rect);
		public static CGRect Integral (this CGRect self)
		{
			return CGRectIntegral (self);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern CGRect CGRectUnion (CGRect r1, CGRect r2);
		public static CGRect UnionWith (this CGRect self, CGRect other)
		{
			return CGRectUnion (self, other);
		}

		[DllImport (Constants.CoreGraphicsLibrary)]
		static extern void CGRectDivide (CGRect rect, out CGRect slice, out CGRect remainder, nfloat amount, NSRectEdge edge);
		public static void Divide (this CGRect self, nfloat amount, NSRectEdge edge, out CGRect slice, out CGRect remainder)
		{
			CGRectDivide (self, out slice, out remainder, amount, edge);
		}
	}
}


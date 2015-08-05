// 
// CVImageBuffer.cs: Implements the managed CVImageBuffer
//
// Authors: Mono Team
//     
// Copyright 2010 Novell, Inc
// Copyright 2011, 2012 Xamarin Inc
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
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;

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

namespace MonoMac.CoreVideo {

	[Since (4,0)]
	public class CVImageBuffer : CVBuffer {
		public static readonly NSString CGColorSpaceKey;
		public static readonly NSString GammaLevelKey;
		public static readonly NSString CleanApertureKey;
		public static readonly NSString PreferredCleanApertureKey;
		public static readonly NSString CleanApertureWidthKey;
		public static readonly NSString CleanApertureHeightKey;
		public static readonly NSString CleanApertureHorizontalOffsetKey;
		public static readonly NSString CleanApertureVerticalOffsetKey;
		public static readonly NSString FieldCountKey;
		public static readonly NSString FieldDetailKey;
		public static readonly NSString FieldDetailTemporalTopFirst;
		public static readonly NSString FieldDetailTemporalBottomFirst;
		public static readonly NSString FieldDetailSpatialFirstLineEarly;
		public static readonly NSString FieldDetailSpatialFirstLineLate;
		public static readonly NSString PixelAspectRatioKey;
		public static readonly NSString PixelAspectRatioHorizontalSpacingKey;
		public static readonly NSString PixelAspectRatioVerticalSpacingKey;
		public static readonly NSString DisplayDimensionsKey;
		public static readonly NSString DisplayWidthKey;
		public static readonly NSString DisplayHeightKey;
		public static readonly NSString YCbCrMatrixKey;
		public static readonly NSString YCbCrMatrix_ITU_R_709_2;
		public static readonly NSString YCbCrMatrix_ITU_R_601_4;
		public static readonly NSString YCbCrMatrix_SMPTE_240M_1995;

		public static readonly NSString ChromaSubsamplingKey;
		public static readonly NSString ChromaSubsampling_420;
		public static readonly NSString ChromaSubsampling_422;
		public static readonly NSString ChromaSubsampling_411;

		public static readonly NSString TransferFunctionKey;
		public static readonly NSString TransferFunction_ITU_R_709_2;
		public static readonly NSString TransferFunction_SMPTE_240M_1995;
		public static readonly NSString TransferFunction_UseGamma;

		public static readonly NSString ChromaLocationTopFieldKey;
		public static readonly NSString ChromaLocationBottomFieldKey;
		public static readonly NSString ChromaLocation_Left;
		public static readonly NSString ChromaLocation_Center;
		public static readonly NSString ChromaLocation_TopLeft;
		public static readonly NSString ChromaLocation_Top;
		public static readonly NSString ChromaLocation_BottomLeft;
		public static readonly NSString ChromaLocation_Bottom;
		public static readonly NSString ChromaLocation_DV420;

		static CVImageBuffer ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreVideoLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				CGColorSpaceKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferCGColorSpaceKey");
				GammaLevelKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferGammaLevelKey");
				CleanApertureKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferCleanApertureKey");
				PreferredCleanApertureKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferPreferredCleanApertureKey");
				CleanApertureWidthKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferCleanApertureWidthKey");
				CleanApertureHeightKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferCleanApertureHeightKey");
				CleanApertureHorizontalOffsetKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferCleanApertureHorizontalOffsetKey");
				CleanApertureVerticalOffsetKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferCleanApertureVerticalOffsetKey");
				FieldCountKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferFieldCountKey");
				FieldDetailKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferFieldDetailKey");
				FieldDetailTemporalTopFirst = Dlfcn.GetStringConstant (handle, "kCVImageBufferFieldDetailTemporalTopFirst");
				FieldDetailTemporalBottomFirst = Dlfcn.GetStringConstant (handle, "kCVImageBufferFieldDetailTemporalBottomFirst");
				FieldDetailSpatialFirstLineEarly = Dlfcn.GetStringConstant (handle, "kCVImageBufferFieldDetailSpatialFirstLineEarly");
				FieldDetailSpatialFirstLineLate = Dlfcn.GetStringConstant (handle, "kCVImageBufferFieldDetailSpatialFirstLineLate");
				PixelAspectRatioKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferPixelAspectRatioKey");
				PixelAspectRatioHorizontalSpacingKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferPixelAspectRatioHorizontalSpacingKey");
				PixelAspectRatioVerticalSpacingKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferPixelAspectRatioVerticalSpacingKey");
				DisplayDimensionsKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferDisplayDimensionsKey");
				DisplayWidthKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferDisplayWidthKey");
				DisplayHeightKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferDisplayHeightKey");
				YCbCrMatrixKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferYCbCrMatrixKey");
				YCbCrMatrix_ITU_R_709_2 = Dlfcn.GetStringConstant (handle, "kCVImageBufferYCbCrMatrix_ITU_R_709_2");
				YCbCrMatrix_ITU_R_601_4 = Dlfcn.GetStringConstant (handle, "kCVImageBufferYCbCrMatrix_ITU_R_601_4");
				YCbCrMatrix_SMPTE_240M_1995 = Dlfcn.GetStringConstant (handle, "kCVImageBufferYCbCrMatrix_SMPTE_240M_1995");

				ChromaSubsamplingKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaSubsamplingKey");
				ChromaSubsampling_420 = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaSubsampling_420");
				ChromaSubsampling_422 = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaSubsampling_422");
				ChromaSubsampling_411 = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaSubsampling_411");

				TransferFunctionKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferTransferFunctionKey");
				TransferFunction_ITU_R_709_2 = Dlfcn.GetStringConstant (handle, "kCVImageBufferTransferFunction_ITU_R_709_2");
				TransferFunction_SMPTE_240M_1995 = Dlfcn.GetStringConstant (handle, "kCVImageBufferTransferFunction_SMPTE_240M_1995");
				TransferFunction_UseGamma = Dlfcn.GetStringConstant (handle, "kCVImageBufferTransferFunction_UseGamma");

				ChromaLocationTopFieldKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocationTopFieldKey");
				ChromaLocationBottomFieldKey = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocationBottomFieldKey");
				ChromaLocation_Left = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_Left");
				ChromaLocation_Center = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_Center");
				ChromaLocation_TopLeft = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_TopLeft");
				ChromaLocation_Top = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_Top");
				ChromaLocation_BottomLeft = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_BottomLeft");
				ChromaLocation_Bottom = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_Bottom");
				ChromaLocation_DV420 = Dlfcn.GetStringConstant (handle, "kCVImageBufferChromaLocation_DV420");
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}

		internal CVImageBuffer (IntPtr handle) : base (handle)
		{
		}

		internal CVImageBuffer () {}
		
		[Preserve (Conditional=true)]
		internal CVImageBuffer (IntPtr handle, bool owns) : base (handle, owns)
		{
		}
		
		[DllImport (Constants.CoreVideoLibrary)]
		extern static CGRect CVImageBufferGetCleanRect (IntPtr imageBuffer);
		public CGRect CleanRect {
			get {
				return CVImageBufferGetCleanRect (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CGSize CVImageBufferGetDisplaySize (IntPtr imageBuffer);
		public CGSize DisplaySize {
			get {
				return CVImageBufferGetDisplaySize (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CGSize CVImageBufferGetEncodedSize (IntPtr imageBuffer);
		public CGSize EncodedSize {
			get {
				return CVImageBufferGetDisplaySize (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static bool CVImageBufferIsFlipped (IntPtr imageBuffer);
		
		public bool IsFlipped {
			get {
				return CVImageBufferIsFlipped (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVImageBufferGetColorSpace (IntPtr handle);
		
		public CGColorSpace ColorSpace {
			get {
				return new CGColorSpace (CVImageBufferGetColorSpace (handle));
			}
		}
	}
}

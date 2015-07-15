//
// System.Drawing.Font-CoreText.cs
//
// Author:
//   Kenneth J. Pouncey (kjpou@pt.lu)
//
// Copyright 2011-2013 Xamarin Inc.
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

#if MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.CoreText;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;
#endif

namespace System.Drawing
{
	public partial class Font
	{
		internal CTFont nativeFont;
		bool bold = false;
		bool italic = false;


		private void CreateNativeFont (FontFamily familyName, float emSize, FontStyle style,
			GraphicsUnit unit, byte gdiCharSet, bool  gdiVerticalFont )
		{
			// convert to 96 Dpi to be consistent with Windows
			var dpiSize = emSize * dpiScale;

			try 
			{
				nativeFont = new CTFont(familyName.NativeDescriptor,dpiSize);
			}
			catch
			{
				nativeFont = new CTFont("Helvetica",dpiSize);
			}

			CTFontSymbolicTraits tMask = CTFontSymbolicTraits.None;

			if ((style & FontStyle.Bold) == FontStyle.Bold)
				tMask |= CTFontSymbolicTraits.Bold;
			if ((style & FontStyle.Italic) == FontStyle.Italic)
				tMask |= CTFontSymbolicTraits.Italic;
			strikeThrough = (style & FontStyle.Strikeout) == FontStyle.Strikeout;
			underLine = (style & FontStyle.Underline) == FontStyle.Underline;

			var nativeFont2 = nativeFont.WithSymbolicTraits(dpiSize,tMask,tMask);

			if (nativeFont2 != null)
				nativeFont = nativeFont2;

			bold = (nativeFont.SymbolicTraits & CTFontSymbolicTraits.Bold) == CTFontSymbolicTraits.Bold; 
			italic = (nativeFont.SymbolicTraits & CTFontSymbolicTraits.Italic) == CTFontSymbolicTraits.Italic;
			sizeInPoints = emSize;
			this.unit = unit;

			// FIXME
			// I do not like the hard coded 72 but am just trying to boot strap the Font class right now
			size = ConversionHelpers.GraphicsUnitConversion(GraphicsUnit.Point, unit, 72.0f, sizeInPoints); 

		}

		/**
		 * 
		 * Returns: The line spacing, in pixels, of this font.
		 * 
		 * The line spacing of a Font is the vertical distance between the base lines of 
		 * two consecutive lines of text. Thus, the line spacing includes the blank space 
		 * between lines along with the height of the character itself.
		 * 
		 * If the Unit property of the font is set to anything other than GraphicsUnit.Pixel, 
		 * the height (in pixels) is calculated using the vertical resolution of the 
		 * screen display. For example, suppose the font unit is inches and the font size 
		 * is 0.3. Also suppose that for the corresponding font family, the em-height 
		 * is 2048 and the line spacing is 2355. For a screen display that has a vertical 
		 * resolution of 96 dots per inch, you can calculate the height as follows:
		 * 
		 * 2355*(0.3/2048)*96 = 33.11719
		 * 
		 **/
		private float GetNativeheight()
		{
			// Documentation for Accessing Font Metrics
			// http://developer.apple.com/library/ios/#documentation/StringsTextFonts/Conceptual/CoreText_Programming/Operations/Operations.html
			float lineHeight = 0;
			lineHeight += nativeFont.AscentMetric;
			lineHeight += nativeFont.DescentMetric;
			lineHeight += nativeFont.LeadingMetric;


			// Still have not figured this out yet!!!!
			return lineHeight;

		}
	}
}


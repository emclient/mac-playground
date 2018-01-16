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

#if XAMARINMAC
using Foundation;
using CoreGraphics;
using CoreText;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using MonoMac.CoreText;
using MonoMac.AppKit;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;
using MonoTouch.UIKit;
#endif

#if MAC64
using nfloat = System.Double;
#else
using nfloat = System.Single;
#endif

namespace System.Drawing
{
	public partial class Font
	{
		internal CTFont nativeFont;
		bool bold = false;
		bool italic = false;

		internal Font(NSFont font)
		{
			var traits = font.FontDescriptor.SymbolicTraits;

			fontFamily = new FontFamily(font.FamilyName, true);
			fontStyle =
				((traits & NSFontSymbolicTraits.BoldTrait) == NSFontSymbolicTraits.BoldTrait ? FontStyle.Bold : 0) |
				((traits & NSFontSymbolicTraits.ItalicTrait) == NSFontSymbolicTraits.ItalicTrait ? FontStyle.Italic : 0);
			gdiVerticalFont = false;
			gdiCharSet = DefaultCharSet;
			sizeInPoints = (float)(font.PointSize * 72f / 96f);
			size = (float)font.PointSize;
			unit = GraphicsUnit.Pixel;

			// CTFont and NSFont are toll-free bridged
			this.nativeFont = (CTFont)Activator.CreateInstance(
				typeof(CTFont),
				Reflection.BindingFlags.NonPublic | Reflection.BindingFlags.Instance,
				null,
				new object[] { font.Handle },
				null);
		}

		private void CreateNativeFont (FontFamily familyName, float emSize, FontStyle style,
			GraphicsUnit unit, byte gdiCharSet, bool  gdiVerticalFont )
		{
			sizeInPoints = ConversionHelpers.GraphicsUnitConversion(unit, GraphicsUnit.Point, 96.0f, emSize); 

			// convert to 96 Dpi to be consistent with Windows
			var dpiSize = sizeInPoints * 96f / 72f;

			try 
			{
				if (IsFontInstalled(familyName.NativeDescriptor, dpiSize) && IsFontSafe(familyName.Name))
					nativeFont = new CTFont(familyName.NativeDescriptor, dpiSize);
				else
					nativeFont = new CTFont(CTFontUIFontType.System, dpiSize, PreferredLanguage);
			}
			catch
			{
				nativeFont = new CTFont(CTFontUIFontType.System, dpiSize, PreferredLanguage);
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
			this.size = emSize;
			this.unit = unit;
		}

		internal static string PreferredLanguage
		{
			get
			{
				var langauges = NSLocale.PreferredLanguages;
				return langauges.Length > 0 ? langauges[0] : "en-US";
			}
		}

		internal static bool IsFontInstalled(CTFontDescriptor descriptor, nfloat size)
		{
			var matching = descriptor.GetMatchingFontDescriptors((NSSet)null);
			return matching != null && matching.Length > 0;
		}

		internal static bool IsFontSafe(string name)
		{
			switch (name)
			{
				// For some reason, Helvetica causes problems on the Mac:
				// The metric looks good, but glyphs get rendered just at the top of the bounding rectangle.
				// The layout then seems broken. Until determining the real reason, let's disable this font.
				// case "Helvetica": return false;
				default: return true;
			}
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
			return (float)nativeFont.BoundingBox.Height;
		}
	}
}


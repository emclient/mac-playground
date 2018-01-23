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
using System.Drawing.Mac;

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
		const int RegularWidth = 5;

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

			nativeFont = font.ToCTFont();
		}

		void CreateNativeFont(FontFamily familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
		{
			this.sizeInPoints = ConversionHelpers.GraphicsUnitConversion(unit, GraphicsUnit.Point, 96f, emSize);
			this.bold = FontStyle.Bold == (style & FontStyle.Bold);
			this.italic = FontStyle.Italic == (style & FontStyle.Italic);
			this.underLine = FontStyle.Underline == (style & FontStyle.Underline);
			this.size = emSize;
			this.unit = unit;

			var size = sizeInPoints * 96f / 72f;
			var traits = (NSFontTraitMask)0;
			if (bold) traits |= NSFontTraitMask.Bold;
			if (italic) traits |= NSFontTraitMask.Italic;

			this.nativeFont = NSFontWithFamily(familyName.Name, traits, RegularWidth, size).ToCTFont();
		}

		static NSFont NSFontWithFamily(string name, NSFontTraitMask traits, int width, float size)
		{
			if (NSThread.Current.IsMainThread)
				return NSFontManager.SharedFontManager.FontWithFamily(name, traits, width, size);

			NSFont font = null;
			NSThread.MainThread.InvokeOnMainThread(() => { font = NSFontManager.SharedFontManager.FontWithFamily(name, traits, width, size); });
			return font;
		}

		internal static string PreferredLanguage
		{
			get
			{
				var langauges = NSLocale.PreferredLanguages;
				return langauges.Length > 0 ? langauges[0] : "en-US";
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


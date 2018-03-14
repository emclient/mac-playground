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
using CoreText;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.CoreText;
using MonoMac.AppKit;
#else
using MonoTouch.Foundation;
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

		internal Font(CTFont font)
		{
			var traits = font.GetTraits().SymbolicTraits.GetValueOrDefault();
			fontFamily = new FontFamily(font.FamilyName, true);
			fontStyle |= traits.IsBold() ? FontStyle.Bold : 0;
			fontStyle |= traits.IsItalic() ? FontStyle.Italic : 0;
			gdiVerticalFont = false;
			gdiCharSet = DefaultCharSet;
			sizeInPoints = (float)(font.Size * 72f / 96f);
			size = (float)font.Size;
			unit = GraphicsUnit.Pixel;
			nativeFont = font;
		}

		void CreateNativeFont(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
		{
			this.sizeInPoints = ConversionHelpers.GraphicsUnitConversion(unit, GraphicsUnit.Point, 96f, emSize);
			this.underLine = 0 != (style & FontStyle.Underline);
			this.strikeThrough = 0 != (style & FontStyle.Strikeout);

			this.size = emSize;
			this.unit = unit;

			var size = sizeInPoints * 96f / 72f;

			var traits = CTFontSymbolicTraits.None;
			traits |= style.IsBold() ? CTFontSymbolicTraits.Bold : 0;
			traits |= style.IsItalic() ? CTFontSymbolicTraits.Italic : 0;

			this.nativeFont = CTFontWithFamily(family, traits, size);
		}

		internal static CTFont CTFontWithFamily(FontFamily family, CTFontSymbolicTraits traits, float size)
		{
			// Semibold font hack
			if (FontFamily.RemoveSemiboldSuffix(family.Name, out string familyName))
			    if (CTFontWithFamilyName(familyName, traits, size, CTFontWeight.Semibold) is CTFont semibold)
					return semibold;
			
			var	font = CTFontWithFamily(family, size);
			var mask = (CTFontSymbolicTraits)uint.MaxValue;
			var fontWithTraits = font.WithSymbolicTraits(size, traits, mask);
			return fontWithTraits ?? font;
		}

		internal static CTFont CTFontWithFamily(FontFamily family, float size)
		{
			return IsFontInstalled(family.NativeDescriptor)
				? new CTFont(family.NativeDescriptor, size)
				: new CTFont(CTFontUIFontType.System, size, PreferredLanguage);
		}

		internal static bool IsFontInstalled(CTFontDescriptor descriptor)
		{
			var matching = descriptor.GetMatchingFontDescriptors((NSSet)null);
			return matching != null && matching.Length > 0;
		}

		internal static string PreferredLanguage
		{
			get
			{
				var langauges = NSLocale.PreferredLanguages;
				return langauges.Length > 0 ? langauges[0] : "en-US";
			}
		}

		internal static CTFont CTFontWithFamilyName(string family, CTFontSymbolicTraits straits, float size, float weight)
		{
			var bold = Math.Abs(weight - CTFontWeight.Bold) < 0.01f;
			var traits = new NSMutableDictionary();

			if (Math.Abs(weight) > 0.01 && !bold)
				traits[CTFontTraitKey.Weight] = NSNumber.FromFloat(weight);

			if (bold)
				straits |= CTFontSymbolicTraits.Bold;

			if (0 != (straits & (CTFontSymbolicTraits.Bold | CTFontSymbolicTraits.Italic)))
				traits[CTFontTraitKey.Symbolic] = NSNumber.FromUInt32((UInt32)straits);

			var attrs = new NSMutableDictionary();
			attrs[CTFontDescriptorAttributeKey.FamilyName] = (NSString)family;
			attrs[CTFontDescriptorAttributeKey.Traits] = traits;

			var desc = new CTFontDescriptor(new CTFontDescriptorAttributes(attrs));
			var font = new CTFont(desc, size);

			return font;
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
			return nativeFont.GetLineHeight();
		}
	}
}


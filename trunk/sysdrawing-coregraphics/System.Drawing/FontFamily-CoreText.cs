//
// System.Drawing.FontFamily-CoreText.cs
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
using System.Drawing.Text;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using MonoMac.CoreText;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;
#endif

namespace System.Drawing
{
	public sealed partial class FontFamily
	{
		const string MONO_SPACE = "Courier New";
		// Wikipedia - On October 16, 2007, Apple announced on their website that the next version of their flagship operating system, 
		// Mac OS X v10.5 ("Leopard"), would include Microsoft Sans Serif.
		// Just because it ships does not mean we have to use it though.
		const string SANS_SERIF = "Microsoft Sans Serif";  // "Arial";  or even "Helvetica";
		const string SERIF = "Times New Roman";

		enum Metric
		{
			EMHeight,
			CellAscent,
			CellDescent,
			LineSpacing
		}

		CTFontDescriptor nativeFontDescriptor;

		internal CTFontDescriptor NativeDescriptor
		{
			get { return nativeFontDescriptor; }
		}

		private void CreateNativeFontFamily(string name, bool createDefaultIfNotExists)
		{
			CreateNativeFontFamily (name, null, createDefaultIfNotExists);
		}

		private void CreateNativeFontFamily(string name, FontCollection fontCollection, bool createDefaultIfNotExists)
		{
			if (fontCollection != null) 
			{
				if (fontCollection.nativeFontDescriptors.ContainsKey (name))
					nativeFontDescriptor = fontCollection.nativeFontDescriptors [name];

				if (nativeFontDescriptor == null && createDefaultIfNotExists) 
				{
					nativeFontDescriptor = new CTFontDescriptor (SANS_SERIF, 0);
				}
			} 
			else 
			{
				nativeFontDescriptor = new CTFontDescriptor (name, 0);

				if (nativeFontDescriptor == null && createDefaultIfNotExists) 
				{
					nativeFontDescriptor = new CTFontDescriptor (SANS_SERIF, 0);
				}
			}

			if (nativeFontDescriptor == null)
				throw new ArgumentException ("name specifies a font that is not installed on the computer running the application.");
			else 
			{
				var attrs = nativeFontDescriptor.GetAttributes ();
				familyName = attrs.FamilyName;
				// if the font description attributes do not contain a value for FamilyName then we
				// need to try and create the font to get the family name from the actual font.
				if (string.IsNullOrEmpty (familyName)) 
				{
					var font = new CTFont (nativeFontDescriptor, 0);
					familyName = font.FamilyName;
				}
			}

		}

		private bool NativeStyleAvailable(FontStyle style)
		{

			// we are going to actually have to create a font object here
			// will not create an actual variable for this yet.  We may
			// want to do this in the future so that we do not have to keep 
			// creating it over and over again.
			var font = new CTFont (nativeFontDescriptor,0);

			switch (style) 
			{
			case FontStyle.Bold:
				var tMaskBold = CTFontSymbolicTraits.None;
				tMaskBold |= CTFontSymbolicTraits.Bold;
				var bFont = font.WithSymbolicTraits (0, tMaskBold, tMaskBold);
				if (bFont == null)
					return false;
				var bold = (bFont.SymbolicTraits & CTFontSymbolicTraits.Bold) == CTFontSymbolicTraits.Bold;
				return bold;

			case FontStyle.Italic:
				//return (font.SymbolicTraits & CTFontSymbolicTraits.Italic) == CTFontSymbolicTraits.Italic; 
				var tMaskItalic = CTFontSymbolicTraits.None;
				tMaskItalic |= CTFontSymbolicTraits.Italic;
				var iFont = font.WithSymbolicTraits (0, tMaskItalic, tMaskItalic);
				if (iFont == null)
					return false;
				var italic = (iFont.SymbolicTraits & CTFontSymbolicTraits.Italic) == CTFontSymbolicTraits.Italic;
				return italic;

			case FontStyle.Regular:

				// Verify if this is correct somehow - we may need to add Bold here as well not sure
				if ((font.SymbolicTraits & CTFontSymbolicTraits.Condensed) == CTFontSymbolicTraits.Condensed
					||  (font.SymbolicTraits & CTFontSymbolicTraits.Expanded) == CTFontSymbolicTraits.Expanded)
					return false;
				else
					return true;
			case FontStyle.Underline:
				return font.UnderlineThickness > 0;
			case FontStyle.Strikeout:
				// not implemented yet
				return false;

			}
			return false;
		}

		private int GetNativeMetric(Metric metric, FontStyle style)
		{

			// we are going to actually have to create a font object here
			// will not create an actual variable for this yet.  We may
			// want to do this in the future so that we do not have to keep 
			// creating it over and over again.
			CTFontSymbolicTraits tMask = CTFontSymbolicTraits.None;

			if ((style & FontStyle.Bold) == FontStyle.Bold)
				tMask |= CTFontSymbolicTraits.Bold;
			if ((style & FontStyle.Italic) == FontStyle.Italic)
				tMask |= CTFontSymbolicTraits.Italic;

			var font = new CTFont (nativeFontDescriptor,0);
			font = font.WithSymbolicTraits (0, tMask, tMask);

			switch (metric)
			{
			case Metric.EMHeight:
				return (int)font.UnitsPerEmMetric;
			case Metric.CellAscent:
				return (int)Math.Round(font.AscentMetric / font.Size * font.UnitsPerEmMetric);
			case Metric.CellDescent:
				return (int)Math.Round(font.DescentMetric / font.Size * font.UnitsPerEmMetric);
			case  Metric.LineSpacing:
				float lineHeight = 0;
				lineHeight += font.AscentMetric;
				lineHeight += font.DescentMetric;
				lineHeight += font.LeadingMetric;
				return (int)Math.Round(lineHeight / font.Size * font.UnitsPerEmMetric);
			}

			return 0;
		}

	}
}


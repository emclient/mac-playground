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
using System.Drawing.Text;
using CoreText;
using ObjCRuntime;

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

		// Semibold font hack support (the MS-Windows way)
		internal const string semiboldSuffix = " Semibold";

		internal CTFontDescriptor NativeDescriptor
		{
			get { return nativeFontDescriptor; }
		}

		private void CreateNativeFontFamily(string name, bool createDefaultIfNotExists)
		{
			CreateNativeFontFamily (name, null, createDefaultIfNotExists);
		}

		private void CreateNativeFontFamily(string extendedName, FontCollection fontCollection, bool createDefaultIfNotExists)
		{
			RemoveSemiboldSuffix(extendedName, out string name);

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
				if (string.IsNullOrEmpty (familyName)) 
					familyName = extendedName; //font.FamilyName
			}
		}

		private bool NativeStyleAvailable(FontStyle style)
		{
			// we are going to actually have to create a font object here
			// will not create an actual variable for this yet.  We may
			// want to do this in the future so that we do not have to keep 
			// creating it over and over again.
			using (var baseFont = new CTFont(nativeFontDescriptor, 0))
			{
				var traits = CTFontSymbolicTraits.None;
				traits |= style.HasFlag(FontStyle.Bold) ? CTFontSymbolicTraits.Bold : 0;
				traits |= style.HasFlag(FontStyle.Italic) ? CTFontSymbolicTraits.Italic : 0;

				using (var font = baseFont.WithSymbolicTraits(0, traits, traits))
				{
					if (font == null)
						return false;

					if (style.HasFlag(FontStyle.Bold) && !font.SymbolicTraits.HasFlag(CTFontSymbolicTraits.Bold))
						return false;

					if (style.HasFlag(FontStyle.Italic) && !font.SymbolicTraits.HasFlag(CTFontSymbolicTraits.Italic))
						return false;

					if (style.HasFlag(FontStyle.Regular))
						if (font.SymbolicTraits.HasFlag(CTFontSymbolicTraits.Condensed) || font.SymbolicTraits.HasFlag(CTFontSymbolicTraits.Expanded))
							return false;

					if (style.HasFlag(FontStyle.Underline) && font.UnderlineThickness == 0)
						return false;

					// TODO:
					//if (font.HasFlag(FontStyle.Strikeout))
					//	return false;
				}
				return true;
			}
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

			using (var baseFont = new CTFont(nativeFontDescriptor, 0))
			using (var font = baseFont.WithSymbolicTraits(0, tMask, tMask))
			{
				switch (metric)
				{
					case Metric.EMHeight:
						return (int)font.UnitsPerEmMetric;
					case Metric.CellAscent:
						return (int)Math.Round(font.AscentMetric / font.Size * font.UnitsPerEmMetric);
					case Metric.CellDescent:
						return (int)Math.Round(font.DescentMetric / font.Size * font.UnitsPerEmMetric);
					case Metric.LineSpacing:
						nfloat lineHeight = 0;
						lineHeight += font.AscentMetric;
						lineHeight += font.DescentMetric;
						lineHeight += font.LeadingMetric;
						return (int)NMath.Round(lineHeight / font.Size * font.UnitsPerEmMetric);
				}
			}
			return 0;
		}

		// Semibold font hack support (the MS-Windows way)
		internal static bool RemoveSemiboldSuffix(string name, out string plain)
		{
			if (name.EndsWith(semiboldSuffix, StringComparison.InvariantCulture))
			{
				plain = name.Substring(0, name.Length - semiboldSuffix.Length);
				return true;
			}

			plain = name;
			return false;
		}

	}
}


//
// System.Drawing.Text.FontCollection-CoreText.cs
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
using System.Collections.Generic;

#if MONOMAC
using MonoMac.CoreText;
#else
using MonoTouch.CoreText;
#endif

namespace System.Drawing.Text 
{
	public abstract partial class FontCollection
	{
		internal CTFontCollection nativeFontCollection = null;
		internal Dictionary<string, CTFontDescriptor> nativeFontDescriptors = null;

		List<string> NativeFontFamilies ()
		{
			if (nativeFontCollection == null) 
			{
				var collectionOptions = new CTFontCollectionOptions ();
				collectionOptions.RemoveDuplicates = true;
				nativeFontCollection = new CTFontCollection (collectionOptions);
				nativeFontDescriptors = new Dictionary<string, CTFontDescriptor> ();
			}

			var fontdescs = nativeFontCollection.GetMatchingFontDescriptors ();

			foreach (var fontdesc in fontdescs) 
			{
				var font = new CTFont (fontdesc, 0);

				// Just in case RemoveDuplicates collection option is not working
				if (!nativeFontDescriptors.ContainsKey (font.FamilyName)) 
				{
					nativeFontDescriptors.Add (font.FamilyName, fontdesc);
				}

			}
			var fontFamilies = new List<string> (nativeFontDescriptors.Keys);

			return fontFamilies;
		}

	}
}


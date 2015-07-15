//
// System.Drawing.Text.PrivateFontCollection-CoreText.cs
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
using System.IO;

#if MONOMAC
using MonoMac.CoreText;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreText;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
#endif

namespace System.Drawing.Text
{
	public sealed partial class PrivateFontCollection 
	{

		void LoadFontFile (string fileName)
		{
			CTFont nativeFont;
			var dpiSize = 0;
			var ext = Path.GetExtension(fileName);

			if (!String.IsNullOrEmpty(ext))
			{

				if (nativeFontDescriptors == null)
					nativeFontDescriptors = new Dictionary<string, CTFontDescriptor> ();

				//Try loading from Bundle first
				var fontName = fileName.Substring (0, fileName.Length - ext.Length);
				var pathForResource = NSBundle.MainBundle.PathForResource (fontName, ext.Substring(1));

				NSUrl url;

 				if (!string.IsNullOrEmpty(pathForResource))
					url = NSUrl.FromFilename (pathForResource);
				else
					url = NSUrl.FromFilename (fileName);

				// We will not use CTFontManager.RegisterFontsForUrl (url, CTFontManagerScope.Process);
				// here.  The reason is that there is no way we can be sure that the font can be created to
				// to identify the family name afterwards.  So instead we will create a CGFont from a data provider.
				// create CTFont to obtain the CTFontDescriptor, store family name and font descriptor to be accessed
				// later.
				try {
					var dataProvider = new CGDataProvider (url.Path);
					var cgFont = CGFont.CreateFromProvider (dataProvider);

					try {
						nativeFont = new CTFont(cgFont, dpiSize, null);
						if (!nativeFontDescriptors.ContainsKey(nativeFont.FamilyName))
						{
							nativeFontDescriptors.Add(nativeFont.FamilyName, nativeFont.GetFontDescriptor());
							NSError error;
							var registered = CTFontManager.RegisterGraphicsFont(cgFont, out error);
							if (!registered)
							{
								// If the error code is 105 then the font we are trying to register is already registered
								// We will not report this as an error.
								if (error.Code != 105)
									throw new ArgumentException("Error registering: " + Path.GetFileName(fileName));
							}
						}
					}
					catch
					{
						// note: MS throw the same exception FileNotFoundException if the file exists but isn't a valid font file
						throw new System.IO.FileNotFoundException (fileName);
					}
				}
				catch (Exception)
				{
					// note: MS throw the same exception FileNotFoundException if the file exists but isn't a valid font file
					throw new System.IO.FileNotFoundException (fileName);
				}
			}
		}
	}
}


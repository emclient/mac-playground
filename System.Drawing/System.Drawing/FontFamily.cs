//
// System.Drawing.FontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//   Peter Dennis Bartok (pbartok@novell.com)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//   Kenneth J. Pouncey (kjpou@pt.lu)
//
// Copyright (C) 2002/2004 Ximian, Inc http://www.ximian.com
// Copyright (C) 2004 - 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing 
{

	public sealed partial class FontFamily : MarshalByRefObject, IDisposable 
	{

		string familyName;

		public FontFamily(GenericFontFamilies genericFamily)
		{
			switch (genericFamily) 
			{
			case GenericFontFamilies.Monospace:
				familyName = MONO_SPACE;
				break;
			case GenericFontFamilies.SansSerif:
				familyName = SANS_SERIF;
				break;
			case GenericFontFamilies.Serif:
				familyName = SERIF;
				break;
			}

			CreateNativeFontFamily (familyName, true);
		}

		public FontFamily (string name)
			: this (name, false)
		{			}

		internal FontFamily (string name, bool createDefaultIfNotExists)
		{			
			if (string.IsNullOrEmpty (name))
				throw new ArgumentException ("name can not be null or empty");

			CreateNativeFontFamily (name, createDefaultIfNotExists);

		}

		public FontFamily(string name, FontCollection fontCollection)
		{
			CreateNativeFontFamily (name, fontCollection, false);

		}

		public string Name
		{
			get { return familyName; }
		}

		public bool IsStyleAvailable (FontStyle fontStyle)
		{
			return NativeStyleAvailable (fontStyle);
		}

		public int GetEmHeight(FontStyle style)
		{
			return GetNativeMetric (Metric.EMHeight, style);
		}

		public int GetCellDescent(FontStyle style)
		{
			return GetNativeMetric (Metric.CellDescent, style);
		}

		public int GetCellAscent(FontStyle style)
		{
			return GetNativeMetric (Metric.CellAscent, style);
		}

		public int GetLineSpacing(FontStyle style)
		{
			return GetNativeMetric (Metric.LineSpacing, style);
		}

		public static FontFamily[] Families
		{
			get
			{
				return new InstalledFontCollection().Families;
			}
		}

		public static FontFamily GenericMonospace
		{
			get
			{
				return new FontFamily(MONO_SPACE);
			}
		}

		public static FontFamily GenericSansSerif
		{
			get
			{
				return new FontFamily(SANS_SERIF);
			}
		}

		public static FontFamily GenericSerif
		{
			get
			{
				return new FontFamily(SERIF);
			}
		}

		~FontFamily ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
		}

		public override string ToString ()
		{
			return string.Format ("[FontFamily: Name={0}]", Name);
		}
	}
}
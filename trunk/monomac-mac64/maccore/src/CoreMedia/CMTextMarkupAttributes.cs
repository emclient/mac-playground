// 
// CMTextMarkupAttributes.cs: Implements CMTextMarkup Attributes
//
// Authors: Marek Safar (marek.safar@gmail.com)
//     
// Copyright 2012 Xamarin Inc
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
//

using System;
using System.Runtime.InteropServices;

using MonoMac;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.CoreMedia {

	public struct TextMarkupColor
	{
		public TextMarkupColor (float red, float green, float blue, float alpha)
			: this ()
		{
			if (red < 0 || red > 1.0)
				throw new ArgumentOutOfRangeException ("red");
			if (green < 0 || green > 1.0)
				throw new ArgumentOutOfRangeException ("green");
			if (blue < 0 || blue > 1.0)
				throw new ArgumentOutOfRangeException ("blue");
			if (alpha < 0 || alpha > 1.0)
				throw new ArgumentOutOfRangeException ("alpha");

			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}

		public float Red { get; private set; }
		public float Green { get; private set; }
		public float Blue { get; private set; }
		public float Alpha { get; private set; }
	}

	[Since (6,0)]
	public class CMTextMarkupAttributes : DictionaryContainer
	{
		static class Keys
		{
			public static readonly NSString ForegroundColorARGB;
			public static readonly NSString BackgroundColorARGB;
			public static readonly NSString BoldStyle;
			public static readonly NSString ItalicStyle;
			public static readonly NSString UnderlineStyle;
			public static readonly NSString FontFamilyName;
			public static readonly NSString RelativeFontSize;

			static Keys ()
			{
				var handle = Dlfcn.dlopen (Constants.CoreMediaLibrary, 0);
				if (handle == IntPtr.Zero)
					return;
				try {
					ForegroundColorARGB  = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_ForegroundColorARGB");
					BackgroundColorARGB  = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_BackgroundColorARGB");
					BoldStyle            = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_BoldStyle");
					ItalicStyle          = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_ItalicStyle");
					UnderlineStyle       = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_UnderlineStyle");
					FontFamilyName       = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_FontFamilyName");
					RelativeFontSize     = Dlfcn.GetStringConstant (handle, "kCMTextMarkupAttribute_RelativeFontSize");
				} finally {
					Dlfcn.dlclose (handle);
				}
			}
		}

		public CMTextMarkupAttributes ()
		{
		}

#if !COREBUILD
		public CMTextMarkupAttributes (NSDictionary dictionary)
			: base (dictionary)
		{
		}

		public TextMarkupColor? ForegroundColor {
			get {
				var array = GetArray<NSNumber> (Keys.ForegroundColorARGB);
				if (array == null)
					return null;

				return new TextMarkupColor (array [1].FloatValue, array [2].FloatValue, array [3].FloatValue, array [0].FloatValue);
			}
			set {
				if (value != null) {
					var v = value.Value;
					SetArrayValue (Keys.ForegroundColorARGB, new [] {
						NSNumber.FromFloat (v.Alpha),
						NSNumber.FromFloat (v.Red),
						NSNumber.FromFloat (v.Green),
						NSNumber.FromFloat (v.Blue)
					});
				} else {
					RemoveValue (Keys.ForegroundColorARGB);
				}
			}
		}

		public TextMarkupColor? BackgroundColor {
			get {
				var array = GetArray<NSNumber> (Keys.BackgroundColorARGB);
				if (array == null)
					return null;

				return new TextMarkupColor (array [1].FloatValue, array [2].FloatValue, array [3].FloatValue, array [0].FloatValue);
			}
			set {
				if (value != null) {
					var v = value.Value;
					SetArrayValue (Keys.BackgroundColorARGB, new [] {
						NSNumber.FromFloat (v.Alpha),
						NSNumber.FromFloat (v.Red),
						NSNumber.FromFloat (v.Green),
						NSNumber.FromFloat (v.Blue)
					});
				} else {
					RemoveValue (Keys.BackgroundColorARGB);
				}
			}
		}

		public bool? Bold {
			get {
				return GetBoolValue (Keys.BoldStyle);
			}
			set {
				SetBooleanValue (Keys.BoldStyle, value);
			}
		}

		public bool? Italic {
			get {
				return GetBoolValue (Keys.ItalicStyle);
			}
			set {
				SetBooleanValue (Keys.ItalicStyle, value);
			}
		}

		public bool? Underline {
			get {
				return GetBoolValue (Keys.UnderlineStyle);
			}
			set {
				SetBooleanValue (Keys.UnderlineStyle, value);
			}
		}

		public string FontFamilyName {
			get {
				return GetStringValue (Keys.FontFamilyName);
			}
			set {
				SetStringValue (Keys.FontFamilyName, value);
			}
		}

		public int? RelativeFontSize {
			get {
				return GetInt32Value (Keys.RelativeFontSize);
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");

				SetNumberValue (Keys.RelativeFontSize, value);
			}
		}
#endif
	}
}

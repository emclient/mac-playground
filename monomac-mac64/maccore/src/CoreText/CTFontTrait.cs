// 
// CTFontTrait.cs: CTFont Traits
//
// Authors: Mono Team
//     
// Copyright 2010 Novell, Inc
// Copyright 2011, 2012 Xamarin Inc
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
using System.Runtime.InteropServices;

using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.CoreText {

	[Since (3,2)]
	public static class CTFontTraitKey {
		public static readonly NSString Symbolic;
		public static readonly NSString Weight;
		public static readonly NSString Width;
		public static readonly NSString Slant;

		static CTFontTraitKey ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreTextLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				Symbolic  = Dlfcn.GetStringConstant (handle, "kCTFontSymbolicTrait");
				Weight    = Dlfcn.GetStringConstant (handle, "kCTFontWeightTrait");
				Width     = Dlfcn.GetStringConstant (handle, "kCTFontWidthTrait");
				Slant     = Dlfcn.GetStringConstant (handle, "kCTFontSlantTrait");
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}
	}

	[Since (3,2)]
	[Flags]
	public enum CTFontSymbolicTraits : uint {
		None        = 0,
		Italic      = (1 << 0),
		Bold        = (1 << 1),
		Expanded    = (1 << 5),
		Condensed   = (1 << 6),
		MonoSpace   = (1 << 10),
		Vertical    = (1 << 11),
		UIOptimized = (1 << 12),
		ColorGlyphs = (1 << 13),
		Composite   = (1 << 14),
		
		Mask        = ((uint) 15 << CTFontTraits.ClassMaskShift)
	}

	[Since (3,2)]
	public enum CTFontStylisticClass : uint {
		None                = 0,
		Unknown             = ((uint) 0 << CTFontTraits.ClassMaskShift),
		OldStyleSerifs      = ((uint) 1 << CTFontTraits.ClassMaskShift),
		TransitionalSerifs  = ((uint) 2 << CTFontTraits.ClassMaskShift),
		ModernSerifs        = ((uint) 3 << CTFontTraits.ClassMaskShift),
		ClarendonSerifs     = ((uint) 4 << CTFontTraits.ClassMaskShift),
		SlabSerifs          = ((uint) 5 << CTFontTraits.ClassMaskShift),
		FreeformSerifs      = ((uint) 7 << CTFontTraits.ClassMaskShift),
		SansSerif           = ((uint) 8 << CTFontTraits.ClassMaskShift),
		Ornamentals         = ((uint) 9 << CTFontTraits.ClassMaskShift),
		Scripts             = ((uint) 10 << CTFontTraits.ClassMaskShift),
		Symbolic            = ((uint) 12 << CTFontTraits.ClassMaskShift),
	}

	[Since (3,2)]
	public class CTFontTraits {

		public CTFontTraits ()
			: this (new NSMutableDictionary ())
		{
		}

		public CTFontTraits (NSDictionary dictionary)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			Dictionary = dictionary;
		}

		public NSDictionary Dictionary {get; private set;}

		public uint? Symbolic {
			get {return Adapter.GetUInt32Value (Dictionary, CTFontTraitKey.Symbolic);}
			set {Adapter.SetValue (Dictionary, CTFontTraitKey.Symbolic, value);}
		}

		public CTFontSymbolicTraits? SymbolicTraits {
			get {
				var v = Symbolic;
				return !v.HasValue ? null : (CTFontSymbolicTraits?) (v.Value & ~StylisticClassMask);
			}
			set {
				var c = StylisticClass;
				Symbolic = Adapter.BitwiseOr (
						c.HasValue ? (uint?) c.Value : null, 
						value.HasValue ? (uint?) value.Value : null);
			}
		}

		public CTFontStylisticClass? StylisticClass {
			get {
				var v = Symbolic;
				return !v.HasValue ? null : (CTFontStylisticClass?) (v.Value & StylisticClassMask);
			}
			set {
				var t = SymbolicTraits;
				Symbolic = Adapter.BitwiseOr (
						t.HasValue ? (uint?) t.Value : null,
						value.HasValue ? (uint?) value.Value : null);
			}
		}

		public float? Weight {
			get {return Adapter.GetSingleValue (Dictionary, CTFontTraitKey.Weight);}
			set {Adapter.SetValue (Dictionary, CTFontTraitKey.Weight, value);}
		}

		public float? Width {
			get {return Adapter.GetSingleValue (Dictionary, CTFontTraitKey.Width);}
			set {Adapter.SetValue (Dictionary, CTFontTraitKey.Width, value);}
		}

		public float? Slant {
			get {return Adapter.GetSingleValue (Dictionary, CTFontTraitKey.Slant);}
			set {Adapter.SetValue (Dictionary, CTFontTraitKey.Slant, value);}
		}

		internal const int  ClassMaskShift      = 28;
		internal const uint StylisticClassMask  = ((uint) 15 << ClassMaskShift);
	}
}


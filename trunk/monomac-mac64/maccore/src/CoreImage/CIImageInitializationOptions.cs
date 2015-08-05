// 
// CIImageInitializationOptions.cs: Implements settings for CIImage initialization
//
// Authors: Marek Safar (marek.safar@gmail.com)
//     
// Copyright 2012, Xamarin Inc.
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

using MonoMac.Foundation;
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;
using MonoMac.CoreGraphics;

namespace MonoMac.CoreImage {

	public class CIImageInitializationOptions : DictionaryContainer
	{
#if !COREBUILD
		public CIImageInitializationOptions ()
			: base (new NSMutableDictionary ())
		{
		}

		public CIImageInitializationOptions (NSDictionary dictionary)
			: base (dictionary)
		{
		}

		public CGColorSpace ColorSpace {
			set {
				SetNativeValue (CIImage.CIImageColorSpaceKey, value == null ? null : value);
			}
		}
#endif
	}

	public class CIImageInitializationOptionsWithMetadata : CIImageInitializationOptions
	{
#if !COREBUILD
		public CIImageInitializationOptionsWithMetadata ()
		{
		}

		public CIImageInitializationOptionsWithMetadata (NSDictionary dictionary)
			: base (dictionary)
		{
		}

		public CGImageProperties Properties {
			set {
				SetNativeValue (CIImage.CIImagePropertiesKey, value == null ? null : value.Dictionary, false);
			}
		}
#endif
	}
}

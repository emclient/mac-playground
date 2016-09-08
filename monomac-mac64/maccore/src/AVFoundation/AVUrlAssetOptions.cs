// 
// AVUrlAssetOptions.cs: Implements strongly typed access for AV video settings
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

namespace MonoMac.AVFoundation {

	public class AVUrlAssetOptions : DictionaryContainer
	{
#if !COREBUILD
		public AVUrlAssetOptions ()
			: base (new NSMutableDictionary ())
		{
		}

		public AVUrlAssetOptions (NSDictionary dictionary)
			: base (dictionary)
		{
		}

		public bool? PreferPreciseDurationAndTiming {
			set {
				SetBooleanValue (AVUrlAsset.PreferPreciseDurationAndTimingKey, value);
			}
			get {
				return GetBoolValue (AVUrlAsset.PreferPreciseDurationAndTimingKey);
			}
		}

		[Since (5,0)]
		public AVAssetReferenceRestrictions? ReferenceRestrictions {
			set {
				SetNumberValue (AVUrlAsset.ReferenceRestrictionsKey, (int?) value);
			}
			get {
				return (AVAssetReferenceRestrictions?) GetInt32Value (AVUrlAsset.ReferenceRestrictionsKey);
			}
		}
#endif
	}
}


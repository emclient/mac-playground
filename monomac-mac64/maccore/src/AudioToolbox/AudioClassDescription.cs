// 
// AudioClassDescription.cs:
//
// Authors:
//    Marek Safar (marek.safar@gmail.com)
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

using System;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.AudioUnit;

namespace MonoMac.AudioToolbox {

	[StructLayout (LayoutKind.Sequential)]
	public struct AudioClassDescription
	{
		public AudioCodecComponentType Type;
		public AudioFormatType SubType;
		public AudioCodecManufacturer Manufacturer;

		public AudioClassDescription (AudioCodecComponentType type, AudioFormatType subType, AudioCodecManufacturer manufacturer)
		{
			Type = type;
			SubType = subType;
			Manufacturer = manufacturer;
		}

		public bool IsHardwareCodec {
			get {
				return Manufacturer == AudioCodecManufacturer.AppleHardware;
			}
		}

/*
		// TODO: Fails with 'prop', so probably Apple never implemented it
		// The documentation is wrong too
		public unsafe static uint? HardwareCodecCapabilities (AudioClassDescription[] descriptions)
		{
			if (descriptions == null)
				throw new ArgumentNullException ("descriptions");

			fixed (AudioClassDescription* item = &descriptions[0]) {
				uint successfulCodecs;
				int size = sizeof (uint);
				var ptr_size = Marshal.SizeOf (typeof (AudioClassDescription)) * descriptions.Length;
				var res = AudioFormatPropertyNative.AudioFormatGetProperty (AudioFormatProperty.HardwareCodecCapabilities, ptr_size, item, ref size, out successfulCodecs);
				if (res != 0)
					return null;

				return successfulCodecs;
			}
		}
*/
	}

	public enum AudioCodecComponentType
	{
		Decoder	= 0x61646563,	// 'adec'	
		Encoder	= 0x61656e63,	// 'aenc'
	}
}
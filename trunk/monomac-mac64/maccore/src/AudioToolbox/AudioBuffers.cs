//
// AudioBuffers: AudioBufferList wrapper class
//
// Authors:
//   Miguel de Icaza
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2011, 2012 Xamarin Inc.
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
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.AudioToolbox
{
	public class AudioBuffers : IDisposable
	{
		IntPtr address;
		readonly bool owns;

		public AudioBuffers (IntPtr address)
			: this (address, false)
		{
		}

		public AudioBuffers (IntPtr address, bool owns)
		{
			if (address == IntPtr.Zero)
				throw new ArgumentException ("address");

			this.address = address;
			this.owns = owns;
		}

		public unsafe AudioBuffers (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			var size = sizeof (int) + count * sizeof (AudioBuffer);
			address = Marshal.AllocHGlobal (size);
			owns = true;

			Marshal.WriteInt32 (address, 0, count);
			AudioBuffer *ptr = (AudioBuffer *) (((byte *) address) + sizeof (int));
			for (int i = 0; i < count; i++){
				ptr->NumberChannels = 0;
				ptr->DataByteSize = 0;
				ptr->Data = IntPtr.Zero;
				ptr++;
			}
		}

		~AudioBuffers ()
		{
			Dispose (false);
			GC.SuppressFinalize (this);
		}

		public unsafe int Count {
			get {
				return *(int *) address;
			}
		}

		public AudioBuffer this [int index] {
			get {
				if (index >= Count)
					throw new ArgumentOutOfRangeException ("index");

				//
				// Decodes
				//
				// struct AudioBufferList
				// {
				//    UInt32      mNumberBuffers;
				//    AudioBuffer mBuffers[1]; // this is a variable length array of mNumberBuffers elements
				// }
				//
				unsafe {
					byte *baddress = (byte *) address;
					
					var ptr = baddress + sizeof (int) + index * sizeof (AudioBuffer);
					return *(AudioBuffer *) ptr;
				}
			}
			set {
				if (index >= Count)
					throw new ArgumentOutOfRangeException ("index");

				unsafe {
					byte *baddress = (byte *) address;
					var ptr = (AudioBuffer *) (baddress + sizeof (int) + index * sizeof (AudioBuffer));
					*ptr = value;
				}
			}
		}

		public static explicit operator IntPtr (AudioBuffers audioBuffers)
		{
			return audioBuffers.address;
		}

		public void SetData (int index, IntPtr data)
		{
			if (index >= Count)
				throw new ArgumentOutOfRangeException ("index");

			unsafe {
				byte * baddress = (byte *) address;
				var ptr = (IntPtr *)(baddress + sizeof (int) + index * sizeof (AudioBuffer) + sizeof (int) + sizeof (int));
				*ptr = data;
			}
		}

		public void SetData (int index, IntPtr data, int dataByteSize)
		{
			if (index >= Count)
				throw new ArgumentOutOfRangeException ("index");

			unsafe {
				byte *baddress = (byte *) address;
				var ptr = (int *)(baddress + sizeof (int) + index * sizeof (AudioBuffer) + sizeof (int));
				*ptr = dataByteSize;
				ptr++;
				IntPtr *iptr = (IntPtr *) ptr;
				*iptr = data;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (owns && address != IntPtr.Zero) {
				Marshal.FreeHGlobal (address);
				address = IntPtr.Zero;
			}
		}
	}
}

//
// MonoMac.CoreFoundation.CFDataBuffer
//
// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
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

namespace MonoMac.CoreFoundation {

	class CFDataBuffer : IDisposable {
		byte[] buffer;
		CFData data;
		bool owns;

		public CFDataBuffer (byte[] buffer)
		{
			this.buffer = buffer;

			/*
			 * Copy the buffer to allow the native side to take ownership.
			 */

			var gch = GCHandle.Alloc (buffer, GCHandleType.Pinned);
			data = CFData.FromData (gch.AddrOfPinnedObject (), buffer.Length);
			gch.Free ();
			owns = true;
		}

		public CFDataBuffer (IntPtr ptr)
		{
			data = new CFData (ptr, false);
			buffer = data.GetBuffer ();
			owns = false;
		}

		~CFDataBuffer ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public IntPtr Handle {
			get { return data.Handle; }
		}

		public byte[] Data {
			get { return buffer; }
		}

		public byte this [int idx] {
			get { return buffer [idx]; }
		}

		protected virtual void Dispose (bool disposing)
		{
			if (data != null) {
				data.Dispose ();
				data = null;
			}
		}
	}
}


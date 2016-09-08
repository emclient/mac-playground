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
// Copyright 2011, 2012 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.Foundation {

	public partial class NSKeyedArchiver {

		public static void GlobalSetClassName (string name, Class kls)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (kls == null)
				throw new ArgumentNullException ("kls");

			var nsname = new NSString (name);
			MonoMac.ObjCRuntime.Messaging.void_objc_msgSend_IntPtr_IntPtr (class_ptr, Selector.GetHandle (selSetClassNameForClass_), nsname.Handle, kls.Handle);
			nsname.Dispose ();
		}

		public static string GlobalGetClassName (Class kls)
		{
			if (kls == null)
				throw new ArgumentNullException ("kls");
			return NSString.FromHandle (MonoMac.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr (class_ptr, Selector.GetHandle (selClassNameForClass_), kls.Handle));
		}

	}
}

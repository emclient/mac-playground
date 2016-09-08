//
// CFBoolean.cs: Contains base types
//
// Authors:
//    Jonathan Pryor (jpryor@novell.com)
// Copyright 2011, 2012 Xamarin Inc
//
// The class can be either constructed from a string (from user code)
// or from a handle (from iphone-sharp.dll internal calls).  This
// delays the creation of the actual managed string until actually
// required
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

using MonoMac.ObjCRuntime;
using MonoMac.Foundation;

namespace MonoMac.CoreFoundation {
	[Since (3,2)]
	class CFBoolean : INativeObject, IDisposable {
		IntPtr handle;

		public static readonly CFBoolean True;
		public static readonly CFBoolean False;

		static CFBoolean ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				True  = new CFBoolean (Dlfcn.GetIntPtr (handle, "kCFBooleanTrue"), false);
				False = new CFBoolean (Dlfcn.GetIntPtr (handle, "kCFBooleanFalse"), false);
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}

		[Preserve (Conditional = true)]
		internal CFBoolean (IntPtr handle, bool owns)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		~CFBoolean ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		[DllImport (Constants.CoreFoundationLibrary, EntryPoint="CFBooleanGetTypeID")]
		public extern static int GetTypeID ();

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

		public static implicit operator bool (CFBoolean value)
		{
			return value.Value;
		}

		public static explicit operator CFBoolean (bool value)
		{
			return FromBoolean (value);
		}

		public static CFBoolean FromBoolean (bool value)
		{
			return value ? True : False;
		}

		[DllImport (Constants.CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static bool CFBooleanGetValue (IntPtr boolean);

		public bool Value {
			get {return CFBooleanGetValue (handle);}
		}

		public static bool GetValue (IntPtr boolean)
		{
			return CFBooleanGetValue (boolean);
		}
	}
}

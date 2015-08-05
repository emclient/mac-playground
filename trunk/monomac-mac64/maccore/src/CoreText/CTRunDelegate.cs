// 
// CTRunDelegate.cs: Implements the managed CTRunDelegate
//
// Authors: Mono Team
//     
// Copyright 2010 Novell, Inc
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
using System.Reflection;
using System.Runtime.InteropServices;

using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;
using MonoMac.CoreGraphics;

namespace MonoMac.CoreText {

#region Run Delegate Callbacks
	delegate void CTRunDelegateDeallocateCallback (IntPtr refCon);
	delegate float CTRunDelegateGetAscentCallback (IntPtr refCon);
	delegate float CTRunDelegateGetDescentCallback (IntPtr refCon);
	delegate float CTRunDelegateGetWidthCallback (IntPtr refCon);

	[StructLayout (LayoutKind.Sequential)]
	class CTRunDelegateCallbacks {
		public CTRunDelegateVersion             version;
		public CTRunDelegateDeallocateCallback  dealloc;
		public CTRunDelegateGetAscentCallback   getAscent;
		public CTRunDelegateGetDescentCallback  getDescent;
		public CTRunDelegateGetWidthCallback    getWidth;
	}
#endregion

#region Run Delegate Versions
	enum CTRunDelegateVersion {
		Version1        = 1,
		CurrentVersion  = Version1,
	}
#endregion

	[Since (3,2)]
	public class CTRunDelegateOperations : IDisposable {

		internal GCHandle handle;

		protected CTRunDelegateOperations ()
		{
			handle = GCHandle.Alloc (this);
		}

		public virtual void Dispose ()
		{
		}

		public virtual float GetAscent ()
		{
			return 0.0f;
		}

		public virtual float GetDescent ()
		{
			return 0.0f;
		}

		public virtual float GetWidth ()
		{
			return 0.0f;
		}

		internal CTRunDelegateCallbacks GetCallbacks ()
		{
			var callbacks = new CTRunDelegateCallbacks () {
				version = CTRunDelegateVersion.Version1,
				dealloc = Deallocate,
			};

			var flags = BindingFlags.Public | BindingFlags.Instance;
			MethodInfo m;

			if ((m = this.GetType ().GetMethod ("GetAscent", flags)) != null &&
					m.DeclaringType != typeof (CTRunDelegateOperations)) {
				callbacks.getAscent = GetAscent;
			}
			if ((m = this.GetType ().GetMethod ("GetDescent", flags)) != null &&
					m.DeclaringType != typeof (CTRunDelegateOperations)) {
				callbacks.getDescent = GetDescent;
			}
			if ((m = this.GetType ().GetMethod ("GetWidth", flags)) != null &&
					m.DeclaringType != typeof (CTRunDelegateOperations)) {
				callbacks.getWidth = GetWidth;
			}

			return callbacks;
		}

		[MonoPInvokeCallback (typeof (CTRunDelegateDeallocateCallback))]
		static void Deallocate (IntPtr refCon)
		{
			var self = GetOperations (refCon);
			if (self == null)
				return;

			self.Dispose ();

			if (self.handle.IsAllocated)
				self.handle.Free ();
			self.handle = new GCHandle ();
		}

		internal static CTRunDelegateOperations GetOperations (IntPtr refCon)
		{
			GCHandle c = GCHandle.FromIntPtr (refCon);

			return c.Target as CTRunDelegateOperations;
		}

		[MonoPInvokeCallback (typeof (CTRunDelegateGetAscentCallback))]
		static float GetAscent (IntPtr refCon)
		{
			var self = GetOperations (refCon);
			if (self == null)
				return 0.0f;
			return self.GetAscent ();
		}

		[MonoPInvokeCallback (typeof (CTRunDelegateGetDescentCallback))]
		static float GetDescent (IntPtr refCon)
		{
			var self = GetOperations (refCon);
			if (self == null)
				return 0.0f;
			return self.GetDescent ();
		}

		[MonoPInvokeCallback (typeof (CTRunDelegateGetWidthCallback))]
		static float GetWidth (IntPtr refCon)
		{
			var self = GetOperations (refCon);
			if (self == null)
				return 0.0f;
			return self.GetWidth ();
		}
	}

	[Since (3,2)]
	public class CTRunDelegate : INativeObject, IDisposable {
		internal IntPtr handle;

		internal CTRunDelegate (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentNullException ("handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}
		
		public IntPtr Handle {
			get {return handle;}
		}

		~CTRunDelegate ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

#region RunDelegate Creation
		[DllImport (Constants.CoreTextLibrary)]
		static extern IntPtr CTRunDelegateCreate (CTRunDelegateCallbacks callbacks, IntPtr refCon);

		public CTRunDelegate (CTRunDelegateOperations operations)
		{
			if (operations == null)
				throw ConstructorError.ArgumentNull (this, "operations");

			handle = CTRunDelegateCreate (operations.GetCallbacks (), GCHandle.ToIntPtr (operations.handle));
			if (handle == IntPtr.Zero)
				throw ConstructorError.Unknown (this);
		}
#endregion

#region Run Delegate Access
		[DllImport (Constants.CoreTextLibrary)]
		static extern IntPtr CTRunDelegateGetRefCon (IntPtr runDelegate);

		public CTRunDelegateOperations Operations {
			get {
				return CTRunDelegateOperations.GetOperations (CTRunDelegateGetRefCon (handle));
			}
		}
#endregion
	}
}


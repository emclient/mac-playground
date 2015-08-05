// 
// CFException.cs: Convert CFError into an CFException
//
// Authors: Mono Team
//     
// Copyright (C) 2009 Novell, Inc
// Copyright 2012 Xamarin Inc.
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

namespace MonoMac.CoreFoundation {

	public static class CFErrorDomain {

		public static readonly NSString Cocoa;
		public static readonly NSString Mach;
		public static readonly NSString OSStatus;
		public static readonly NSString Posix;

		static CFErrorDomain ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				Cocoa     = Dlfcn.GetStringConstant (handle, "kCFErrorDomainCocoa");
				Mach      = Dlfcn.GetStringConstant (handle, "kCFErrorDomainMach");
				OSStatus  = Dlfcn.GetStringConstant (handle, "kCFErrorDomainOSStatus");
				Posix     = Dlfcn.GetStringConstant (handle, "kCFErrorDomainPosix");
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}
	}

	public static class CFExceptionDataKey {

		public static readonly NSString Description;
		public static readonly NSString LocalizedDescription;
		public static readonly NSString LocalizedFailureReason;
		public static readonly NSString LocalizedRecoverySuggestion;
		public static readonly NSString UnderlyingError;

		static CFExceptionDataKey ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				Description                 = Dlfcn.GetStringConstant (handle, "kCFErrorDescriptionKey");
				LocalizedDescription        = Dlfcn.GetStringConstant (handle, "kCFErrorLocalizedDescriptionKey");
				LocalizedFailureReason      = Dlfcn.GetStringConstant (handle, "kCFErrorLocalizedFailureReasonKey");
				LocalizedRecoverySuggestion = Dlfcn.GetStringConstant (handle, "kCFErrorLocalizedRecoverySuggestionKey");
				UnderlyingError             = Dlfcn.GetStringConstant (handle, "kCFErrorUnderlyingErrorKey");
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}
	}

	public class CFException : Exception {

		public CFException (string description, NSString domain, int code, string failureReason, string recoverySuggestion)
			: base (description)
		{
			Code                = code;
			Domain              = domain;
			FailureReason       = failureReason;
			RecoverySuggestion  = recoverySuggestion;
		}

		public static CFException FromCFError (IntPtr cfErrorHandle)
		{
			return FromCFError (cfErrorHandle, true);
		}

		public static CFException FromCFError (IntPtr cfErrorHandle, bool release)
		{
			if (cfErrorHandle == IntPtr.Zero)
				throw new ArgumentException ("cfErrorHandle must not be null.", "cfErrorHandle");

			var e = new CFException (
					ToString (CFErrorCopyDescription (cfErrorHandle)),
					(NSString) Runtime.GetNSObject (CFErrorGetDomain (cfErrorHandle)),
					CFErrorGetCode (cfErrorHandle),
					ToString (CFErrorCopyFailureReason (cfErrorHandle)),
					ToString (CFErrorCopyRecoverySuggestion (cfErrorHandle)));

			var cfUserInfo = CFErrorCopyUserInfo (cfErrorHandle);
			if (cfUserInfo != IntPtr.Zero) {
				using (var userInfo = new NSDictionary (cfUserInfo)) {
					foreach (var i in userInfo)
						e.Data.Add (i.Key, i.Value);
				}
			}
			if (release)
				CFObject.CFRelease (cfErrorHandle);
			return e;
		}

		public int Code {get; private set;}
		public NSString Domain {get; private set;}
		public string FailureReason {get; private set;}
		public string RecoverySuggestion {get; private set;}

		static string ToString (IntPtr cfStringRef)
		{
			return ToString (cfStringRef, true);
		}

		static string ToString (IntPtr cfStringRef, bool release)
		{
			var r = CFString.FetchString (cfStringRef);
			if (release && (cfStringRef != IntPtr.Zero))
				CFObject.CFRelease (cfStringRef);
			return r;
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		static extern IntPtr CFErrorCopyDescription (IntPtr err);

		[DllImport (Constants.CoreFoundationLibrary)]
		static extern IntPtr CFErrorCopyFailureReason (IntPtr err);

		[DllImport (Constants.CoreFoundationLibrary)]
		static extern IntPtr CFErrorCopyRecoverySuggestion (IntPtr err);

		[DllImport (Constants.CoreFoundationLibrary)]
		static extern IntPtr CFErrorCopyUserInfo (IntPtr err);

		[DllImport (Constants.CoreFoundationLibrary)]
		static extern int CFErrorGetCode (IntPtr err);

		[DllImport (Constants.CoreFoundationLibrary)]
		static extern IntPtr CFErrorGetDomain (IntPtr err);
	}
}


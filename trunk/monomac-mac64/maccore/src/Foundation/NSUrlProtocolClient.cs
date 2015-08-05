//
// NSUrlProtocol support
//
// Author:
//   Rolf Bjarne Kvinge
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

using MonoMac.ObjCRuntime;

#if MAC64
using nint = System.Int64;
using nuint = System.UInt64;
using nfloat = System.Double;
#else
using nint = System.Int32;
using nuint = System.UInt32;
using nfloat = System.Single;
#if SDCOMPAT
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using CGRect = System.Drawing.RectangleF;
#endif
#endif

namespace MonoMac.Foundation {
	public sealed class NSUrlProtocolClient : NSObject
	{
		public NSUrlProtocolClient (IntPtr handle)
			: base (handle)
		{
		}
		
		const string selUrlProtocolWasRedirectedToRequestRedirectResponse_ = "URLProtocol:wasRedirectedToRequest:redirectResponse:";
		const string selUrlProtocolCachedResponseIsValid_ = "URLProtocol:cachedResponseIsValid:";
		const string selUrlProtocolDidReceiveResponseCacheStoragePolicy_ = "URLProtocol:didReceiveResponse:cacheStoragePolicy:";
		const string selUrlProtocolDidLoadData_ = "URLProtocol:didLoadData:";
		const string selUrlProtocolDidFinishLoading_ = "URLProtocolDidFinishLoading:";
		const string selUrlProtocolDidFailWithError_ = "URLProtocol:didFailWithError:";
		const string selUrlProtocolDidReceiveAuthenticationChallenge_ = "URLProtocol:didReceiveAuthenticationChallenge:";
		const string selUrlProtocolDidCancelAuthenticationChallenge_ = "URLProtocol:didCancelAuthenticationChallenge:";

		public void Redirected (NSUrlProtocol protocol, NSUrlRequest redirectedToEequest, NSUrlResponse redirectResponse)
		{
			Messaging.void_objc_msgSend_IntPtr_IntPtr_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolWasRedirectedToRequestRedirectResponse_), protocol.Handle, redirectedToEequest.Handle, redirectResponse.Handle);
		}

		public void CachedResponseIsValid (NSUrlProtocol protocol, NSCachedUrlResponse cachedResponse)
		{
			Messaging.void_objc_msgSend_IntPtr_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolCachedResponseIsValid_), protocol.Handle, cachedResponse.Handle);
		}

		public void ReceivedResponse (NSUrlProtocol protocol, NSUrlResponse response, NSUrlCacheStoragePolicy policy)
		{
			Messaging.void_objc_msgSend_intptr_intptr_nuint (this.Handle, Selector.GetHandle (selUrlProtocolDidReceiveResponseCacheStoragePolicy_), protocol.Handle, response.Handle, (nuint)policy);
		}

		public void DataLoaded (NSUrlProtocol protocol, NSData data)
		{
			Messaging.void_objc_msgSend_IntPtr_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolDidLoadData_), protocol.Handle, data.Handle);
		}

		public void FinishedLoading (NSUrlProtocol protocol)
		{
			Messaging.void_objc_msgSend_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolDidFinishLoading_), protocol.Handle);
		}

		public void FailedWithError (NSUrlProtocol protocol, NSError error)
		{
			Messaging.void_objc_msgSend_IntPtr_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolDidFailWithError_), protocol.Handle, error.Handle);
		}

		public void ReceivedAuthenticationChallenge (NSUrlProtocol protocol, NSUrlAuthenticationChallenge challenge)
		{
			Messaging.void_objc_msgSend_IntPtr_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolDidReceiveAuthenticationChallenge_), protocol.Handle, challenge.Handle);
		}

		public void CancelledAuthenticationChallenge (NSUrlProtocol protocol, NSUrlAuthenticationChallenge challenge)
		{
			Messaging.void_objc_msgSend_IntPtr_IntPtr (this.Handle, Selector.GetHandle (selUrlProtocolDidCancelAuthenticationChallenge_), protocol.Handle, challenge.Handle);
		}
	}
}

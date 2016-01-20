using System;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using System.IO;
using MonoMac.AppKit;
using System.Collections.Generic;

namespace FormsTest
{
	[Register("UrlProtocol")]
	public class UrlProtocol : NSUrlProtocol
	{
		NSUrlConnection connection;
		static NSObject BeingHandledValue = new NSString("YES");
		static string BeingHandledKey = "BeingHandledByUrlProtocol";

		[Export("canInitWithRequest:")]
		public static bool canInitWithRequest(NSUrlRequest request)
		{
			Console.WriteLine("canInitWithRequest: " +  request.Url);

			if (NSUrlProtocol.GetProperty(BeingHandledKey, request) != null)
				return false;

			return true;
		}

		[Export("canonicalRequestForRequest:")]
		public static new NSUrlRequest GetCanonicalRequest(NSUrlRequest request)   //create mutable version of it to be able to add custom headers
		{           
			return request;
		}

		public UrlProtocol(IntPtr handle)
			: base(handle)
		{
//			var initSelector = new Selector("init");
//			this.Handle = MonoMac.ObjCRuntime.Messaging.IntPtr_objc_msgSend(this.Handle, initSelector.Handle);
		}

		public override void StartLoading()
		{
			Console.WriteLine("StartLoading: " +  this.Request.Url.AbsoluteString);

			var request = (this.Request as NSMutableUrlRequest) ?? (NSMutableUrlRequest)this.Request.MutableCopy();
			NSUrlProtocol.SetProperty(BeingHandledValue, BeingHandledKey, request);
			connection = new NSUrlConnection(request, new UrlConnectionDelegate(this), true);
		}

		public override void StopLoading()
		{
			if (connection != null)
			{
				connection.Cancel();
				connection = null;
			}
		}
	}

	public class UrlConnectionDelegate : NSUrlConnectionDelegate 
	{
		UrlProtocol handler;

		public UrlConnectionDelegate(UrlProtocol handler)
			: base()
		{
			this.handler = handler;
		}

		public override void ReceivedResponse(NSUrlConnection connection, NSUrlResponse response)
		{
			Console.WriteLine("ReceivedResponse");
			handler.Client.ReceivedResponse(handler, response, NSUrlCacheStoragePolicy.NotAllowed);
		}

		public override void ReceivedData(NSUrlConnection connection, NSData data)
		{
			Console.WriteLine("ReceivedData");
			handler.Client.DataLoaded(handler, data);
		}

		public override void FinishedLoading(NSUrlConnection connection)
		{
			Console.WriteLine("FinishedLoading");
			handler.Client.FinishedLoading(handler);
		}

		public override void FailedWithError(NSUrlConnection connection, NSError error)
		{
			Console.WriteLine("FailedWithError");
			handler.Client.FailedWithError(handler, error);
		}
	}
}

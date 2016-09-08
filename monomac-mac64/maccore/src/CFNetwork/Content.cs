//
// MonoMac.CFNetwork.Content
//
// Authors:
//      Martin Baulig (martin.baulig@gmail.com)
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MonoMac.Foundation;

namespace MonoMac.CFNetwork {

	class Content : StreamContent {
		WebResponseStream responseStream;
		long? contentLength;

		internal Content (WebResponseStream stream)
			: base (stream)
		{
			this.responseStream = stream;
		}

		protected override bool TryComputeLength (out long length)
		{
			length = contentLength ?? 0;
			return contentLength != null;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (responseStream != null)
					responseStream.Dispose ();
				responseStream = null;
			}
		}

		#region Headers

		internal bool DecodeHeader (string key, string value)
		{
			if (key.Equals ("Content-Type")) {
				SetContentType (value);
				return true;
			} else if (key.EndsWith ("Content-Length")) {
				SetContentLength (value);
				return true;
			} else if (key.EndsWith ("Content-Language")) {
				Headers.ContentLanguage.Add (value);
				return true;
			} else if (key.Equals ("Content-Location")) {
				Headers.ContentLocation = new Uri (value);
				return true;
			} else if (key.Equals ("Allow")) {
				DecodeAllow (value);
				return true;
			} else if (key.Equals ("Expires")) {
				DecodeExpires (value);
				return true;
			} else if (key.Equals ("Last-Modified")) {
				DecodeLastModified (value);
				return true;
			} else {
				return false;
			}
		}

		void SetContentType (string value)
		{
			int pos = value.IndexOf (";");

			string type;
			if (pos < 0)
				type = value.Trim ();
			else
				type = value.Substring (0, pos).Trim ();
			Headers.ContentType = new MediaTypeHeaderValue (type);

			if (pos < 0)
				return;

			value = value.Substring (pos+1).Trim ();
			if (value.StartsWith ("charset=")) {
				var charset = value.Substring (8);
				Headers.ContentEncoding.Add (charset);
			}
		}

		void SetContentLength (string value)
		{
			contentLength = long.Parse (value);
			Headers.ContentLength = contentLength;
		}

		void DecodeAllow (string value)
		{
			foreach (var method in value.Split (','))
				Headers.Allow.Add (method);
		}

		void DecodeExpires (string value)
		{
			Headers.Expires = DateTimeOffset.Parse (value);
		}

		void DecodeLastModified (string value)
		{
			Headers.LastModified = DateTimeOffset.Parse (value);
		}

		#endregion
	}
}

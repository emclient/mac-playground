// 
// CFProxySupport.cs: Implements the managed binding for CFProxySupport.h
//
// Authors: Jeffrey Stedfast <jeff@xamarin.com>
//     
// Copyright (C) 2011 Xamarin, Inc.
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
using System.Net;
using System.Runtime.InteropServices;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;

namespace MonoMac.CoreFoundation {
	public enum CFProxyType {
		None,
		AutoConfigurationUrl,
		AutoConfigurationJavaScript,
		FTP,
		HTTP,
		HTTPS,
		SOCKS
	}
	
	public class CFProxy {
		NSDictionary settings;
		
		internal CFProxy (NSDictionary settings)
		{
			this.settings = settings;
		}
		
		#region Property Keys
		static NSString kCFProxyAutoConfigurationHTTPResponseKey;
		static NSString AutoConfigurationHTTPResponseKey {
			get {
				if (kCFProxyAutoConfigurationHTTPResponseKey == null)
					kCFProxyAutoConfigurationHTTPResponseKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyAutoConfigurationHTTPResponseKey");
				
				return kCFProxyAutoConfigurationHTTPResponseKey;
			}
		}
		
		static NSString kCFProxyAutoConfigurationJavaScriptKey;
		static NSString AutoConfigurationJavaScriptKey {
			get {
				if (kCFProxyAutoConfigurationJavaScriptKey == null)
					kCFProxyAutoConfigurationJavaScriptKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyAutoConfigurationJavaScriptKey");
				
				return kCFProxyAutoConfigurationJavaScriptKey;
			}
		}
		
		static NSString kCFProxyAutoConfigurationURLKey;
		static NSString AutoConfigurationURLKey {
			get {
				if (kCFProxyAutoConfigurationURLKey == null)
					kCFProxyAutoConfigurationURLKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyAutoConfigurationURLKey");
				
				return kCFProxyAutoConfigurationURLKey;
			}
		}
		
		static NSString kCFProxyHostNameKey;
		static NSString HostNameKey {
			get {
				if (kCFProxyHostNameKey == null)
					kCFProxyHostNameKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyHostNameKey");
				
				return kCFProxyHostNameKey;
			}
		}
		
		static NSString kCFProxyPasswordKey;
		static NSString PasswordKey {
			get {
				if (kCFProxyPasswordKey == null)
					kCFProxyPasswordKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyPasswordKey");
				
				return kCFProxyPasswordKey;
			}
		}
		
		static NSString kCFProxyPortNumberKey;
		static NSString PortNumberKey {
			get {
				if (kCFProxyPortNumberKey == null)
					kCFProxyPortNumberKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyPortNumberKey");
				
				return kCFProxyPortNumberKey;
			}
		}
		
		static NSString kCFProxyTypeKey;
		static NSString ProxyTypeKey {
			get {
				if (kCFProxyTypeKey == null)
					kCFProxyTypeKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeKey");
				
				return kCFProxyTypeKey;
			}
		}
		
		static NSString kCFProxyUsernameKey;
		static NSString UsernameKey {
			get {
				if (kCFProxyUsernameKey == null)
					kCFProxyUsernameKey = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyUsernameKey");
				
				return kCFProxyUsernameKey;
			}
		}
		#endregion Property Keys
		
		#region Proxy Types
		static NSString kCFProxyTypeNone;
		static NSString CFProxyTypeNone {
			get {
				if (kCFProxyTypeNone == null)
					kCFProxyTypeNone = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeNone");
				
				return kCFProxyTypeNone;
			}
		}
		
		static NSString kCFProxyTypeAutoConfigurationURL;
		static NSString CFProxyTypeAutoConfigurationURL {
			get {
				if (kCFProxyTypeAutoConfigurationURL == null)
					kCFProxyTypeAutoConfigurationURL = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeAutoConfigurationURL");
				
				return kCFProxyTypeAutoConfigurationURL;
			}
		}
		
		static NSString kCFProxyTypeAutoConfigurationJavaScript;
		static NSString CFProxyTypeAutoConfigurationJavaScript {
			get {
				if (kCFProxyTypeAutoConfigurationJavaScript == null)
					kCFProxyTypeAutoConfigurationJavaScript = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeAutoConfigurationJavaScript");
				
				return kCFProxyTypeAutoConfigurationJavaScript;
			}
		}
		
		static NSString kCFProxyTypeFTP;
		static NSString CFProxyTypeFTP {
			get {
				if (kCFProxyTypeFTP == null)
					kCFProxyTypeFTP = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeFTP");
				
				return kCFProxyTypeFTP;
			}
		}
		
		static NSString kCFProxyTypeHTTP;
		static NSString CFProxyTypeHTTP {
			get {
				if (kCFProxyTypeHTTP == null)
					kCFProxyTypeHTTP = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeHTTP");
				
				return kCFProxyTypeHTTP;
			}
		}
		
		static NSString kCFProxyTypeHTTPS;
		static NSString CFProxyTypeHTTPS {
			get {
				if (kCFProxyTypeHTTPS == null)
					kCFProxyTypeHTTPS = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeHTTPS");
				
				return kCFProxyTypeHTTPS;
			}
		}
		
		static NSString kCFProxyTypeSOCKS;
		static NSString CFProxyTypeSOCKS {
			get {
				if (kCFProxyTypeSOCKS == null)
					kCFProxyTypeSOCKS = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFProxyTypeSOCKS");
				
				return kCFProxyTypeSOCKS;
			}
		}
		#endregion Proxy Types
		
		static CFProxyType CFProxyTypeToEnum (NSString type)
		{
			if (CFProxyTypeAutoConfigurationJavaScript != null) {
				if (type.Handle == CFProxyTypeAutoConfigurationJavaScript.Handle)
					return CFProxyType.AutoConfigurationJavaScript;
			}
			
			if (CFProxyTypeAutoConfigurationURL != null) {
				if (type.Handle == CFProxyTypeAutoConfigurationURL.Handle)
					return CFProxyType.AutoConfigurationUrl;
			}
			
			if (CFProxyTypeFTP != null) {
				if (type.Handle == CFProxyTypeFTP.Handle)
					return CFProxyType.FTP;
			}
			
			if (CFProxyTypeHTTP != null) {
				if (type.Handle == CFProxyTypeHTTP.Handle)
					return CFProxyType.HTTP;
			}
			
			if (CFProxyTypeHTTP != null) {
				if (type.Handle == CFProxyTypeHTTPS.Handle)
					return CFProxyType.HTTPS;
			}
			
			if (CFProxyTypeSOCKS != null) {
				if (type.Handle == CFProxyTypeSOCKS.Handle)
					return CFProxyType.SOCKS;
			}
			
			return CFProxyType.None;
		}
		
#if false
		// AFAICT these get used with CFNetworkExecuteProxyAutoConfiguration*()
		
		// TODO: bind CFHTTPMessage so we can return the proper type here.
		public NSObject AutoConfigurationHTTPResponse {
			get { return settings[AutoConfigurationHTTPResponseKey]; }
		}
#endif
		
		[Since (4, 0)][Lion]
		public NSString AutoConfigurationJavaScript {
			get {
				if (AutoConfigurationJavaScriptKey == null)
					return null;
				
				return (NSString) settings[AutoConfigurationJavaScriptKey];
			}
		}
		
		public NSUrl AutoConfigurationUrl {
			get {
				if (AutoConfigurationURLKey == null)
					return null;
				
				return (NSUrl) settings[AutoConfigurationURLKey];
			}
		}
		
		public string HostName {
			get {
				if (HostNameKey == null)
					return null;
				
				NSString v = (NSString) settings[HostNameKey];
				
				return v != null ? v.ToString () : null;
			}
		}
		
		public string Password {
			get {
				if (PasswordKey == null)
					return null;
				
				NSString v = (NSString) settings[PasswordKey];
				
				return v != null ? v.ToString () : null;
			}
		}
		
		public int Port {
			get {
				if (PortNumberKey == null)
					return 0;
				
				NSNumber v = (NSNumber) settings[PortNumberKey];
				
				return v != null ? v.Int32Value : 0;
			}
		}
		
		public CFProxyType ProxyType {
			get {
				if (ProxyTypeKey == null)
					return CFProxyType.None;
				
				return CFProxyTypeToEnum ((NSString) settings[ProxyTypeKey]);
			}
		}
		
		public string Username {
			get {
				if (UsernameKey == null)
					return null;
				
				NSString v = (NSString) settings[UsernameKey];
				
				return v != null ? v.ToString () : null;
			}
		}
	}
	
	public class CFProxySettings {
		NSDictionary settings;
		
		internal CFProxySettings (NSDictionary settings)
		{
			this.settings = settings;
		}
		
		public NSDictionary Dictionary {
			get { return settings; }
		}
		
		#region Global Proxy Setting Constants
		static NSString kCFNetworkProxiesHTTPEnable;
		static NSString CFNetworkProxiesHTTPEnable {
			get {
				if (kCFNetworkProxiesHTTPEnable == null)
					kCFNetworkProxiesHTTPEnable = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFNetworkProxiesHTTPEnable");
				
				return kCFNetworkProxiesHTTPEnable;
			}
		}
		
		static NSString kCFNetworkProxiesHTTPPort;
		static NSString CFNetworkProxiesHTTPPort {
			get {
				if (kCFNetworkProxiesHTTPPort == null)
					kCFNetworkProxiesHTTPPort = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFNetworkProxiesHTTPPort");
				
				return kCFNetworkProxiesHTTPPort;
			}
		}
		
		static NSString kCFNetworkProxiesHTTPProxy;
		static NSString CFNetworkProxiesHTTPProxy {
			get {
				if (kCFNetworkProxiesHTTPProxy == null)
					kCFNetworkProxiesHTTPProxy = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFNetworkProxiesHTTPProxy");
				
				return kCFNetworkProxiesHTTPProxy;
			}
		}
		
		static NSString kCFNetworkProxiesProxyAutoConfigEnable;
		static NSString CFNetworkProxiesProxyAutoConfigEnable {
			get {
				if (kCFNetworkProxiesProxyAutoConfigEnable == null)
					kCFNetworkProxiesProxyAutoConfigEnable = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFNetworkProxiesProxyAutoConfigEnable");
				
				return kCFNetworkProxiesProxyAutoConfigEnable;
			}
		}
		
		static NSString kCFNetworkProxiesProxyAutoConfigJavaScript;
		static NSString CFNetworkProxiesProxyAutoConfigJavaScript {
			get {
				if (kCFNetworkProxiesProxyAutoConfigJavaScript == null)
					kCFNetworkProxiesProxyAutoConfigJavaScript = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFNetworkProxiesProxyAutoConfigJavaScript");
				
				return kCFNetworkProxiesProxyAutoConfigJavaScript;
			}
		}
		
		static NSString kCFNetworkProxiesProxyAutoConfigURLString;
		static NSString CFNetworkProxiesProxyAutoConfigURLString {
			get {
				if (kCFNetworkProxiesProxyAutoConfigURLString == null)
					kCFNetworkProxiesProxyAutoConfigURLString = Dlfcn.GetStringConstant (CFNetwork.CFNetworkLibraryHandle, "kCFNetworkProxiesProxyAutoConfigURLString");
				
				return kCFNetworkProxiesProxyAutoConfigURLString;
			}
		}
		#endregion Global Proxy Setting Constants
		
		public bool HTTPEnable {
			get {
				if (CFNetworkProxiesHTTPEnable == null)
					return false;
				
				NSNumber v = (NSNumber) settings[CFNetworkProxiesHTTPEnable];
				
				return v != null ? v.BoolValue : false;
			}
		}
		
		public int HTTPPort {
			get {
				if (CFNetworkProxiesHTTPPort == null)
					return 0;
				
				NSNumber v = (NSNumber) settings[CFNetworkProxiesHTTPPort];
				
				return v != null ? v.Int32Value : 0;
			}
		}
		
		public string HTTPProxy {
			get {
				if (CFNetworkProxiesHTTPProxy == null)
					return null;
				
				NSString v = (NSString) settings[CFNetworkProxiesHTTPProxy];
				
				return v != null ? v.ToString () : null;
			}
		}
		
		public bool ProxyAutoConfigEnable {
			get {
				if (CFNetworkProxiesProxyAutoConfigEnable == null)
					return false;
				
				NSNumber v = (NSNumber) settings[CFNetworkProxiesProxyAutoConfigEnable];
				
				return v != null ? v.BoolValue : false;
			}
		}
		
		public string ProxyAutoConfigJavaScript {
			get {
				if (CFNetworkProxiesProxyAutoConfigJavaScript == null)
					return null;
				
				NSString v = (NSString) settings[CFNetworkProxiesProxyAutoConfigJavaScript];
				
				return v != null ? v.ToString () : null;
			}
		}
		
		public string ProxyAutoConfigURLString {
			get {
				if (CFNetworkProxiesProxyAutoConfigURLString == null)
					return null;
				
				NSString v = (NSString) settings[CFNetworkProxiesProxyAutoConfigURLString];
				
				return v != null ? v.ToString () : null;
			}
		}
	}
	
	public static class CFNetwork {
		internal static IntPtr CFNetworkLibraryHandle = Dlfcn.dlopen (Constants.CFNetworkLibrary, 0);
		
		[DllImport (Constants.CFNetworkLibrary)]
		// CFArrayRef CFNetworkCopyProxiesForAutoConfigurationScript (CFStringRef proxyAutoConfigurationScript, CFURLRef targetURL);
		extern static IntPtr CFNetworkCopyProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, IntPtr targetURL);
		
		static NSArray CopyProxiesForAutoConfigurationScript (NSString proxyAutoConfigurationScript, NSUrl targetURL)
		{
			IntPtr native = CFNetworkCopyProxiesForAutoConfigurationScript (proxyAutoConfigurationScript.Handle, targetURL.Handle);
			
			if (native == IntPtr.Zero)
				return null;
			
			return new NSArray (native);
		}
		
		public static CFProxy[] GetProxiesForAutoConfigurationScript (NSString proxyAutoConfigurationScript, NSUrl targetURL)
		{
			if (proxyAutoConfigurationScript == null)
				throw new ArgumentNullException ("proxyAutoConfigurationScript");
			
			if (targetURL == null)
				throw new ArgumentNullException ("targetURL");
			
			NSArray array = CopyProxiesForAutoConfigurationScript (proxyAutoConfigurationScript, targetURL);
			
			if (array == null)
				return null;
			
			NSDictionary[] dictionaries = NSArray.ArrayFromHandle<NSDictionary> (array.Handle);
			array.Dispose ();
			
			if (dictionaries == null)
				return null;
			
			CFProxy[] proxies = new CFProxy [dictionaries.Length];
			for (int i = 0; i < dictionaries.Length; i++)
				proxies[i] = new CFProxy (dictionaries[i]);
			
			return proxies;
		}
		
		public static CFProxy[] GetProxiesForAutoConfigurationScript (NSString proxyAutoConfigurationScript, Uri targetUri)
		{
			if (proxyAutoConfigurationScript == null)
				throw new ArgumentNullException ("proxyAutoConfigurationScript");
			
			if (targetUri == null)
				throw new ArgumentNullException ("targetUri");
			
			NSUrl targetURL = new NSUrl (targetUri.AbsoluteUri);
			CFProxy[] proxies = GetProxiesForAutoConfigurationScript (proxyAutoConfigurationScript, targetURL);
			targetURL.Dispose ();
			
			return proxies;
		}
		
		[DllImport (Constants.CFNetworkLibrary)]
		// CFArrayRef CFNetworkCopyProxiesForURL (CFURLRef url, CFDictionaryRef proxySettings);
		extern static IntPtr CFNetworkCopyProxiesForURL (IntPtr url, IntPtr proxySettings);
		
		static NSArray CopyProxiesForURL (NSUrl url, NSDictionary proxySettings)
		{
			IntPtr native = CFNetworkCopyProxiesForURL (url.Handle, proxySettings != null ? proxySettings.Handle : IntPtr.Zero);
			
			if (native == IntPtr.Zero)
				return null;
			
			return new NSArray (native);
		}
		
		public static CFProxy[] GetProxiesForURL (NSUrl url, CFProxySettings proxySettings)
		{
			if (url == null || (url.Handle == IntPtr.Zero))
				throw new ArgumentNullException ("url");
			
			if (proxySettings == null)
				proxySettings = GetSystemProxySettings ();
			
			NSArray array = CopyProxiesForURL (url, proxySettings.Dictionary);
			
			if (array == null)
				return null;
			
			NSDictionary[] dictionaries = NSArray.ArrayFromHandle<NSDictionary> (array.Handle);
			array.Dispose ();
			
			if (dictionaries == null)
				return null;
			
			CFProxy[] proxies = new CFProxy [dictionaries.Length];
			for (int i = 0; i < dictionaries.Length; i++)
				proxies[i] = new CFProxy (dictionaries[i]);
			
			return proxies;
		}
		
		public static CFProxy[] GetProxiesForUri (Uri uri, CFProxySettings proxySettings)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			NSUrl url = NSUrl.FromString (uri.AbsoluteUri);
			if (url == null)
				return null;
			
			CFProxy[] proxies = GetProxiesForURL (url, proxySettings);
			url.Dispose ();
			
			return proxies;
		}
		
		[DllImport (Constants.CFNetworkLibrary)]
		// CFDictionaryRef CFNetworkCopySystemProxySettings (void);
		extern static IntPtr CFNetworkCopySystemProxySettings ();
		
		public static CFProxySettings GetSystemProxySettings ()
		{
			IntPtr native = CFNetworkCopySystemProxySettings ();
			
			if (native == IntPtr.Zero)
				return null;
			
			var dict = new NSDictionary (native);
			// Must release since the IntPtr constructor calls Retain and
			// CFNetworkCopySystemProxySettings return value is already retained
			dict.Release ();
			return new CFProxySettings (dict);
		}
		
#if notyet
		// FIXME: These require bindings for CFRunLoopSource and CFStreamClientContext
		
		public delegate void CFProxyAutoConfigurationResultCallback (IntPtr client, NSArray proxyList, NSError error);
		
		[DllImport (Constants.CFNetworkLibrary)]
		// CFRunLoopSourceRef CFNetworkExecuteProxyAutoConfigurationScript (CFStringRef proxyAutoConfigurationScript, CFURLRef targetURL, CFProxyAutoConfigurationResultCallback cb, CFStreamClientContext *clientContext);
		extern static IntPtr CFNetworkExecuteProxyAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, IntPtr targetURL, IntPtr cb, IntPtr clientContext);
		
		public static CFRunLoopSource ExecuteProxyAutoConfigurationScript (NSString proxyAutoConfigurationScript, NSUrl targetURL, CFProxyAutoConfigurationResultCallback resultCallback, CFStreamClientContext clientContext)
		{
			if (proxyAutoConfigurationScript == null)
				throw new ArgumentNullException ("proxyAutoConfigurationScript");

			if (targetURL == null)
				throw new ArgumentNullException ("targetURL");

			if (resultCallback == null)
				throw new ArgumentNullException ("resultCallback");

			if (clientContext == null)
				throw new ArgumentNullException ("clientContext");

			IntPtr source = CFNetworkExecuteProxyAutoConfigurationScript (proxyAutoConfigurationScript.Handle, targetURL.Handle, resultCallback, clientContext);

			if (source != IntPtr.Zero)
				return new CFRunLoopSource (source);

			return null;
		}
		
		[DllImport (Constants.CFNetworkLibrary)]
		// CFRunLoopSourceRef CFNetworkExecuteProxyAutoConfigurationURL (CFURLRef proxyAutoConfigurationURL, CFURLRef targetURL, CFProxyAutoConfigurationResultCallback cb, CFStreamClientContext *clientContext);
		extern static IntPtr CFNetworkExecuteProxyAutoConfigurationURL (IntPtr proxyAutoConfigurationURL, IntPtr targetURL, IntPtr cb, IntPtr clientContext);
		
		public static CFRunLoopSource ExecuteProxyAutoConfigurationURL (NSUrl proxyAutoConfigurationURL, NSUrl targetURL, CFProxyAutoConfigurationResultCallback resultCallback, CFStreamClientContext clientContext)
		{
			if (proxyAutoConfigurationURL == null)
				throw new ArgumentNullException ("proxyAutoConfigurationURL");

			if (targetURL == null)
				throw new ArgumentNullException ("targetURL");

			if (resultCallback == null)
				throw new ArgumentNullException ("resultCallback");

			if (clientContext == null)
				throw new ArgumentNullException ("clientContext");

			IntPtr source = CFNetworkExecuteProxyAutoConfigurationURL (proxyAutoConfigurationURL.Handle, targetURL.Handle, resultCallback, clientContext);

			if (source != IntPtr.Zero)
				return new CFRunLoopSource (source);

			return null;
		}
#endif
		
		class CFWebProxy : IWebProxy {
			ICredentials credentials;
			bool userSpecified;
			
			public CFWebProxy ()
			{
			}
			
			public ICredentials Credentials {
				get { return credentials; }
				set {
					userSpecified = true;
					credentials = value;
				}
			}
			
			static Uri GetProxyUri (CFProxy proxy, out NetworkCredential credentials)
			{
				string protocol;
				
				switch (proxy.ProxyType) {
				case CFProxyType.FTP:
					protocol = "ftp://";
					break;
				case CFProxyType.HTTP:
				case CFProxyType.HTTPS:
					protocol = "http://";
					break;
				default:
					credentials = null;
					return null;
				}
				
				string username = proxy.Username;
				string password = proxy.Password;
				string hostname = proxy.HostName;
				int port = proxy.Port;
				string uri;
				
				if (username != null)
					credentials = new NetworkCredential (username, password);
				else
					credentials = null;
				
				uri = protocol + hostname + (port != 0 ? ':' + port.ToString () : string.Empty);
				
				return new Uri (uri, UriKind.Absolute);
			}
			
			static Uri GetProxyUriFromScript (NSString script, Uri targetUri, out NetworkCredential credentials)
			{
				CFProxy[] proxies = CFNetwork.GetProxiesForAutoConfigurationScript (script, targetUri);
				
				if (proxies == null) {
					credentials = null;
					return targetUri;
				}
				
				for (int i = 0; i < proxies.Length; i++) {
					switch (proxies[i].ProxyType) {
					case CFProxyType.HTTPS:
					case CFProxyType.HTTP:
					case CFProxyType.FTP:
						// create a Uri based on the hostname/port/etc info
						return GetProxyUri (proxies[i], out credentials);
					case CFProxyType.SOCKS:
					default:
						// unsupported proxy type, try the next one
						break;
					case CFProxyType.None:
						// no proxy should be used
						credentials = null;
						return targetUri;
					}
				}
				
				credentials = null;
				
				return null;
			}
			
			public Uri GetProxy (Uri targetUri)
			{
				NetworkCredential credentials = null;
				Uri proxy = null;
				
				if (targetUri == null)
					throw new ArgumentNullException ("targetUri");
				
				try {
					CFProxySettings settings = CFNetwork.GetSystemProxySettings ();
					CFProxy[] proxies = CFNetwork.GetProxiesForUri (targetUri, settings);
					
					if (proxies != null) {
						for (int i = 0; i < proxies.Length && proxy == null; i++) {
							switch (proxies[i].ProxyType) {
							case CFProxyType.AutoConfigurationJavaScript:
								proxy = GetProxyUriFromScript (proxies[i].AutoConfigurationJavaScript, targetUri, out credentials);
								break;
							case CFProxyType.AutoConfigurationUrl:
								// unsupported proxy type (requires fetching script from remote url)
								break;
							case CFProxyType.HTTPS:
							case CFProxyType.HTTP:
							case CFProxyType.FTP:
								// create a Uri based on the hostname/port/etc info
								proxy = GetProxyUri (proxies[i], out credentials);
								break;
							case CFProxyType.SOCKS:
								// unsupported proxy type, try the next one
								break;
							case CFProxyType.None:
								// no proxy should be used
								proxy = targetUri;
								break;
							}
						}
						
						if (proxy == null) {
							// no supported proxies for this Uri, fall back to trying to connect to targetUri directly
							proxy = targetUri;
						}
					} else {
						proxy = targetUri;
					}
				} catch {
					// ignore errors while retrieving proxy data
					proxy = targetUri;
				}
				
				if (!userSpecified)
					this.credentials = credentials;
				
				return proxy;
			}
			
			public bool IsBypassed (Uri targetUri)
			{
				if (targetUri == null)
					throw new ArgumentNullException ("targetUri");
				
				return GetProxy (targetUri) == targetUri;
			}
		}
		
		public static IWebProxy GetDefaultProxy ()
		{
			return new CFWebProxy ();
		}
	}
}

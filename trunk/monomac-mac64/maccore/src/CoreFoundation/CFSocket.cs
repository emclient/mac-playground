//
// MonoMac.CoreFoundation.CFSocket
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
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.CoreFoundation {

	[Flags]
	public enum CFSocketCallBackType {
		NoCallBack = 0,
		ReadCallBack = 1,
		AcceptCallBack = 2,
		DataCallBack = 3,
		ConnectCallBack = 4,
		WriteCallBack = 8
	}

	public enum CFSocketError {
		Success = 0,
		Error = -1,
		Timeout = -2
	}

	[Flags]
	public enum CFSocketFlags {
		AutomaticallyReenableReadCallBack = 1,
		AutomaticallyReenableAcceptCallBack = 2,
		AutomaticallyReenableDataCallBack = 3,
		AutomaticallyReenableWriteCallBack = 8,
		LeaveErrors = 64,
		CloseOnInvalidate = 128
	}

	public struct CFSocketNativeHandle {
		internal readonly int handle;

		internal CFSocketNativeHandle (int handle)
		{
			this.handle = handle;
		}

		public override string ToString ()
		{
			return string.Format ("[CFSocketNativeHandle {0}]", handle);
		}
	}

	public class CFSocketException : Exception {
		public CFSocketError Error {
			get;
			private set;
		}

		public CFSocketException (CFSocketError error)
		{
			this.Error = error;
		}
	}

#if !COREFX
	struct CFSocketSignature {
		int protocolFamily;
		int socketType;
		int protocol;
		IntPtr address;

		public CFSocketSignature (AddressFamily family, SocketType type,
		                          ProtocolType proto, CFSocketAddress address)
		{
			this.protocolFamily = AddressFamilyToInt (family);
			this.socketType = SocketTypeToInt (type);
			this.protocol = ProtocolToInt (proto);
			this.address = address.Handle;
		}

		internal static int AddressFamilyToInt (AddressFamily family)
		{
			switch (family) {
			case AddressFamily.Unspecified:
				return 0;
			case AddressFamily.Unix:
				return 1;
			case AddressFamily.InterNetwork:
				return 2;
			case AddressFamily.AppleTalk:
				return 16;
			case AddressFamily.InterNetworkV6:
				return 30;
			default:
				throw new ArgumentException ();
			}
		}

		internal static int SocketTypeToInt (SocketType type)
		{
			if ((int) type == 0)
				return 0;

			switch (type) {
			case SocketType.Unknown:
				return 0;
			case SocketType.Stream:
				return 1;
			case SocketType.Dgram:
				return 2;
			case SocketType.Raw:
				return 3;
			case SocketType.Rdm:
				return 4;
			case SocketType.Seqpacket:
				return 5;
			default:
				throw new ArgumentException ();
			}
		}

		internal static int ProtocolToInt (ProtocolType type)
		{
			return (int) type;
		}

	}

	class CFSocketAddress : CFDataBuffer {
		public CFSocketAddress (IPEndPoint endpoint)
			: base (CreateData (endpoint))
		{
		}

		internal static IPEndPoint EndPointFromAddressPtr (IntPtr address)
		{
			using (var buffer = new CFDataBuffer (address)) {
				if (buffer [1] == 30) { // AF_INET6
					int port = (buffer [2] << 8) + buffer [3];
					var bytes = new byte [16];
					Buffer.BlockCopy (buffer.Data, 8, bytes, 0, 16);
					return new IPEndPoint (new IPAddress (bytes), port);
				} else if (buffer [1] == 2) { // AF_INET
					int port = (buffer [2] << 8) + buffer [3];
					var bytes = new byte [4];
					Buffer.BlockCopy (buffer.Data, 4, bytes, 0, 4);
					return new IPEndPoint (new IPAddress (bytes), port);
				} else {
					throw new ArgumentException ();
				}
			}
		}

		static byte[] CreateData (IPEndPoint endpoint)
		{
			if (endpoint.AddressFamily == AddressFamily.InterNetwork) {
				var buffer = new byte [16];
				buffer [0] = 16;
				buffer [1] = 2; // AF_INET
				buffer [2] = (byte)(endpoint.Port >> 8);
				buffer [3] = (byte)(endpoint.Port & 0xff);
				Buffer.BlockCopy (endpoint.Address.GetAddressBytes (), 0, buffer, 4, 4);
				return buffer;
			} else if (endpoint.AddressFamily == AddressFamily.InterNetworkV6) {
				var buffer = new byte [28];
				buffer [0] = 32;
				buffer [1] = 30; // AF_INET6
				buffer [2] = (byte)(endpoint.Port >> 8);
				buffer [3] = (byte)(endpoint.Port & 0xff);
				Buffer.BlockCopy (endpoint.Address.GetAddressBytes (), 0, buffer, 8, 16);
				return buffer;
			} else {
				throw new ArgumentException ();
			}
		}
	}

	public class CFSocket : CFType, INativeObject, IDisposable {
		IntPtr handle;
		GCHandle gch;

		~CFSocket ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public IntPtr Handle {
			get { return handle; }
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (gch.IsAllocated)
					gch.Free ();
			}
			if (handle != IntPtr.Zero) {
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

		delegate void CFSocketCallBack (IntPtr s, int type, IntPtr address, IntPtr data, IntPtr info);

		[MonoPInvokeCallback (typeof(CFSocketCallBack))]
		static void OnCallback (IntPtr s, int type, IntPtr address, IntPtr data, IntPtr info)
		{
			var socket = GCHandle.FromIntPtr (info).Target as CFSocket;
			CFSocketCallBackType cbType = (CFSocketCallBackType)type;

			if (cbType == CFSocketCallBackType.AcceptCallBack) {
				var ep = CFSocketAddress.EndPointFromAddressPtr (address);
				var handle = new CFSocketNativeHandle (Marshal.ReadInt32 (data));
				socket.OnAccepted (new CFSocketAcceptEventArgs (handle, ep));
			} else if (cbType == CFSocketCallBackType.ConnectCallBack) {
				CFSocketError result;
				if (data == IntPtr.Zero)
					result = CFSocketError.Success;
				else
					result = (CFSocketError)Marshal.ReadInt32 (data);
				socket.OnConnect (new CFSocketConnectEventArgs (result));
			}
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static IntPtr CFSocketCreate (IntPtr allocator, int family, int type, int proto,
		                                     CFSocketCallBackType callBackTypes,
		                                     CFSocketCallBack callout, IntPtr ctx);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static IntPtr CFSocketCreateWithNative (IntPtr allocator, CFSocketNativeHandle sock,
		                                               CFSocketCallBackType callBackTypes,
		                                               CFSocketCallBack callout, IntPtr ctx);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static IntPtr CFSocketCreateRunLoopSource (IntPtr allocator, IntPtr socket, int order);

		public CFSocket ()
			: this (0, 0, 0)
		{
		}

		public CFSocket (AddressFamily family, SocketType type, ProtocolType proto)
			: this (family, type, proto, CFRunLoop.Current)
		{
		}

		public CFSocket (AddressFamily family, SocketType type, ProtocolType proto, CFRunLoop loop)
			: this (CFSocketSignature.AddressFamilyToInt (family),
			        CFSocketSignature.SocketTypeToInt (type),
			        CFSocketSignature.ProtocolToInt (proto), loop)
		{
		}

		CFSocket (int family, int type, int proto, CFRunLoop loop)
		{
			var cbTypes = CFSocketCallBackType.DataCallBack | CFSocketCallBackType.ConnectCallBack;

			gch = GCHandle.Alloc (this);
			var ctx = new CFStreamClientContext ();
			ctx.Info = GCHandle.ToIntPtr (gch);

			var ptr = Marshal.AllocHGlobal (Marshal.SizeOf (typeof(CFStreamClientContext)));
			try {
				Marshal.StructureToPtr (ctx, ptr, false);
				handle = CFSocketCreate (
					IntPtr.Zero, family, type, proto, cbTypes, OnCallback, ptr);
			} finally {
				Marshal.FreeHGlobal (ptr);
			}

			if (handle == IntPtr.Zero)
				throw new CFSocketException (CFSocketError.Error);
			gch = GCHandle.Alloc (this);

			var source = new CFRunLoopSource (CFSocketCreateRunLoopSource (IntPtr.Zero, handle, 0));
			loop.AddSource (source, CFRunLoop.CFDefaultRunLoopMode);
		}

		internal CFSocket (CFSocketNativeHandle sock)
		{
			var cbTypes = CFSocketCallBackType.DataCallBack | CFSocketCallBackType.WriteCallBack;

			gch = GCHandle.Alloc (this);
			var ctx = new CFStreamClientContext ();
			ctx.Info = GCHandle.ToIntPtr (gch);

			var ptr = Marshal.AllocHGlobal (Marshal.SizeOf (typeof(CFStreamClientContext)));
			try {
				Marshal.StructureToPtr (ctx, ptr, false);
				handle = CFSocketCreateWithNative (
					IntPtr.Zero, sock, cbTypes, OnCallback, ptr);
			} finally {
				Marshal.FreeHGlobal (ptr);
			}

			if (handle == IntPtr.Zero)
				throw new CFSocketException (CFSocketError.Error);

			var source = new CFRunLoopSource (CFSocketCreateRunLoopSource (IntPtr.Zero, handle, 0));
			var loop = CFRunLoop.Current;
			loop.AddSource (source, CFRunLoop.CFDefaultRunLoopMode);
		}

		CFSocket (IntPtr handle)
		{
			this.handle = handle;
			gch = GCHandle.Alloc (this);

			var source = new CFRunLoopSource (CFSocketCreateRunLoopSource (IntPtr.Zero, handle, 0));
			var loop = CFRunLoop.Current;
			loop.AddSource (source, CFRunLoop.CFDefaultRunLoopMode);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static IntPtr CFSocketCreateConnectedToSocketSignature (IntPtr allocator, ref CFSocketSignature signature,
		                                                               CFSocketCallBackType callBackTypes,
		                                                               CFSocketCallBack callout,
		                                                               IntPtr context, double timeout);

		public static CFSocket CreateConnectedToSocketSignature (AddressFamily family, SocketType type,
		                                                         ProtocolType proto, IPEndPoint endpoint,
		                                                         double timeout)
		{
			var cbTypes = CFSocketCallBackType.ConnectCallBack | CFSocketCallBackType.DataCallBack;
			using (var address = new CFSocketAddress (endpoint)) {
				var sig = new CFSocketSignature (family, type, proto, address);
				var handle = CFSocketCreateConnectedToSocketSignature (
					IntPtr.Zero, ref sig, cbTypes, OnCallback, IntPtr.Zero, timeout);
				if (handle == IntPtr.Zero)
					throw new CFSocketException (CFSocketError.Error);

				return new CFSocket (handle);
			}
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSocketNativeHandle CFSocketGetNative (IntPtr handle);

		internal CFSocketNativeHandle GetNative ()
		{
			return CFSocketGetNative (handle);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSocketError CFSocketSetAddress (IntPtr handle, IntPtr address);

		public void SetAddress (IPAddress address, int port)
		{
			SetAddress (new IPEndPoint (address, port));
		}

		public void SetAddress (IPEndPoint endpoint)
		{
			EnableCallBacks (CFSocketCallBackType.AcceptCallBack);
			var flags = GetSocketFlags ();
			flags |= CFSocketFlags.AutomaticallyReenableAcceptCallBack;
			SetSocketFlags (flags);
			using (var address = new CFSocketAddress (endpoint)) {
				var error = CFSocketSetAddress (handle, address.Handle);
				if (error != CFSocketError.Success)
					throw new CFSocketException (error);
			}
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSocketFlags CFSocketGetSocketFlags (IntPtr handle);

		public CFSocketFlags GetSocketFlags ()
		{
			return CFSocketGetSocketFlags (handle);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSocketSetSocketFlags (IntPtr handle, CFSocketFlags flags);

		public void SetSocketFlags (CFSocketFlags flags)
		{
			CFSocketSetSocketFlags (handle, flags);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSocketDisableCallBacks (IntPtr handle, CFSocketCallBackType types);

		public void DisableCallBacks (CFSocketCallBackType types)
		{
			CFSocketDisableCallBacks (handle, types);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSocketEnableCallBacks (IntPtr handle, CFSocketCallBackType types);

		public void EnableCallBacks (CFSocketCallBackType types)
		{
			CFSocketEnableCallBacks (handle, types);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSocketError CFSocketSendData (IntPtr handle, IntPtr address, IntPtr data, double timeout);

		public void SendData (byte[] data, double timeout)
		{
			using (var buffer = new CFDataBuffer (data)) {
				var error = CFSocketSendData (handle, IntPtr.Zero, buffer.Handle, timeout);
				if (error != CFSocketError.Success)
					throw new CFSocketException (error);
			}
		}

		public class CFSocketAcceptEventArgs : EventArgs {
			internal CFSocketNativeHandle SocketHandle {
				get;
				private set;
			}

			public IPEndPoint RemoteEndPoint {
				get;
				private set;
			}

			public CFSocketAcceptEventArgs (CFSocketNativeHandle handle, IPEndPoint remote)
			{
				this.SocketHandle = handle;
				this.RemoteEndPoint = remote;
			}

			public CFSocket CreateSocket ()
			{
				return new CFSocket (SocketHandle);
			}

			public override string ToString ()
			{
				return string.Format ("[CFSocketAcceptEventArgs: RemoteEndPoint={0}]", RemoteEndPoint);
			}
		}

		public class CFSocketConnectEventArgs : EventArgs {
			public CFSocketError Result {
				get;
				private set;
			}

			public CFSocketConnectEventArgs (CFSocketError result)
			{
				this.Result = result;
			}

			public override string ToString ()
			{
				return string.Format ("[CFSocketConnectEventArgs: Result={0}]", Result);
			}
		}

		public event EventHandler<CFSocketAcceptEventArgs> AcceptEvent;
		public event EventHandler<CFSocketConnectEventArgs> ConnectEvent;

		void OnAccepted (CFSocketAcceptEventArgs args)
		{
			if (AcceptEvent != null)
				AcceptEvent (this, args);
		}

		void OnConnect (CFSocketConnectEventArgs args)
		{
			if (ConnectEvent != null)
				ConnectEvent (this, args);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSocketError CFSocketConnectToAddress (IntPtr handle, IntPtr address, double timeout);

		public void Connect (IPAddress address, int port, double timeout)
		{
			Connect (new IPEndPoint (address, port), timeout);
		}

		public void Connect (IPEndPoint endpoint, double timeout)
		{
			using (var address = new CFSocketAddress (endpoint)) {
				var error = CFSocketConnectToAddress (handle, address.Handle, timeout);
				if (error != CFSocketError.Success)
					throw new CFSocketException (error);
			}
		}
	}
#endif
}

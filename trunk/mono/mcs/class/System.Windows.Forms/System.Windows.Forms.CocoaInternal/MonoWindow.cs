using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace System.Windows.Forms.CocoaInternal
{
	internal class MonoWindow : NSWindow
	{
		public MonoWindow (IntPtr handle) : base(handle)
		{
		}

		//[Export ("initWithContentRect:styleMask:backing:defer:"), CompilerGenerated]
		internal MonoWindow (RectangleF contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) 
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
		}

		[Export("windowShouldClose:")]
		internal virtual bool shouldClose (NSObject sender)
		{
			var contentView = this.ContentView;
			var hwnd = Hwnd.GetObjectFromWindow (contentView.Handle);

			NativeWindow.WndProc (hwnd.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

			return false;
		}

		[Export("windowWillClose:")]
		internal virtual bool willClose (NSObject sender)
		{
			// TODO: Send WillClose .NET event?
			return true;
		}
	}
}


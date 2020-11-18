using System;
#if XAMARINMAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms
{
	public interface IMacNativeControl
	{
		NSView CreateView();
	}
}

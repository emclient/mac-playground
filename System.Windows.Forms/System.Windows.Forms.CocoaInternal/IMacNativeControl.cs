using System;
using AppKit;

namespace System.Windows.Forms
{
	public interface IMacNativeControl
	{
		NSView CreateView();
	}
}

#if MAC

using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.CocoaInternal;
using AppKit;

namespace System.Windows.Forms {
	public partial class ScrollableControl : IMacNativeControl {

		NSScrollView scrollView;
		Control content;

		public bool UseNativeControl = false;

		public NSView CreateView()
		{
			if (!UseNativeControl)
				return null;

			scrollView = new NSScrollView(this.Bounds.ToCGRect());
			scrollView.AutohidesScrollers = true;

			content = new Control();
			content.Bounds = Bounds;
			//content.BackColor = BackColor;

			var docView = ObjCRuntime.Runtime.GetNSObject(content.Handle) as MonoView;
			docView.AutoresizingMask = NSViewResizingMask.NotSizable;
			scrollView.DocumentView = docView;

			scrollView.AutohidesScrollers = true;
			scrollView.ScrollerStyle = NSScrollerStyle.Legacy;
				
			scrollView.HasVerticalScroller = true;
			scrollView.HasHorizontalScroller = true;

			return scrollView;
		}

		internal virtual void ResizeCanvas(Size size)
		{
			if (content != null)
				content.Size = size;
		}
	}
}

#endif

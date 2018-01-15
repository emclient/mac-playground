//
//MonoView.cs
// 
//Author:
//	Lee Andrus <landrus2@by-rite.net>
//
//Copyright (c) 2009-2010 Lee Andrus
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//

using System.Drawing;
using System.Drawing.Mac;
using System.Runtime.InteropServices;
using System.Windows.Forms.Mac;
using System.Collections.Specialized;
using System.Collections.Generic;

#if XAMARINMAC
using Foundation;
using AppKit;
using CoreGraphics;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using NMath = System.Math;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	//[ExportClass("MonoView", "NSView")]
	partial class MonoView : NSView, System.Drawing.IClientView
	{
		protected XplatUICocoa driver;
		protected NSTrackingArea clientArea;
		protected NSTrackingArea clientAreaEnterExit;
		internal WindowsEventResponder eventReponder;
		protected CGRect clientBounds;
		internal bool inSetFocus;

		public MonoView(IntPtr instance) : base(instance)
		{
		}

		public MonoView(XplatUICocoa driver, CGRect frameRect, WindowStyles style, WindowExStyles exStyle) : base(frameRect)
		{
			this.driver = driver;
			this.Style = style;
			this.ExStyle = exStyle;
			this.eventReponder = new WindowsEventResponder(driver, this);
			base.NextResponder = eventReponder;
		}

		public override NSResponder NextResponder
		{
			get
			{
				return base.NextResponder;
			}
			set
			{
				eventReponder.NextResponder = value;
			}
		}

		public override bool IsFlipped
		{
			get
			{
				return true;
			}
		}

		public override bool IsOpaque
		{
			get
			{
				if (!(Superview is MonoView))
					return false;
				if (Superview != null)
					return Superview.IsOpaque;
				if (UserClip != null)
					return false;
				return true;
			}
		}

		public override bool AcceptsFirstResponder()
		{
			// Skip the normal processing to bypass setting it as first responder. Our
			// controls will do that by themselves by calling SetFocus.
			return Enabled && inSetFocus;
		}

		public override bool CanBecomeKeyView
		{
			get
			{
				return Style.HasFlag(WindowStyles.WS_TABSTOP) && base.CanBecomeKeyView;
			}
		}

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			return true;
		}

		public override bool MouseDownCanMoveWindow
		{
			get
			{
				// FIXME: Another ugly hack - until we find a good solution
				var typeName = (Control.FromHandle(Handle))?.GetType().Name ?? String.Empty;
				if (typeName.Contains("Button") || typeName.Contains("TextBox") || typeName.Contains("Label") || typeName.Contains("Search"))
					return false;

				return base.MouseDownCanMoveWindow;
			}
		}

		public override void ViewDidMoveToWindow()
		{
			base.ViewDidMoveToWindow();
			UpdateTrackingAreas();
		}

		public override void UpdateTrackingAreas()
		{
			if (clientArea != null)
			{
				RemoveTrackingArea(clientArea);
				clientArea = null;
			}
			if (clientAreaEnterExit != null)
			{
				RemoveTrackingArea(clientAreaEnterExit);
				clientAreaEnterExit = null;
			}

			if (Window != null && Window.IgnoresMouseEvents)
				return;

			clientArea = new NSTrackingArea(
				Bounds,
				NSTrackingAreaOptions.ActiveInActiveApp |
				NSTrackingAreaOptions.MouseMoved |
				NSTrackingAreaOptions.InVisibleRect |
				NSTrackingAreaOptions.CursorUpdate |
				NSTrackingAreaOptions.EnabledDuringMouseDrag,
				this,
				new NSDictionary());
			AddTrackingArea(clientArea);
			clientAreaEnterExit = new NSTrackingArea(
				Bounds,
				NSTrackingAreaOptions.ActiveInActiveApp |
				NSTrackingAreaOptions.MouseEnteredAndExited |
				NSTrackingAreaOptions.EnabledDuringMouseDrag,
				this,
				new NSDictionary());
			AddTrackingArea(clientAreaEnterExit);

			base.UpdateTrackingAreas();
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			this.DirtyRectangle = driver.NativeToMonoFramed(dirtyRect, this);

			var msg = new MSG { hwnd = this.Handle, message = Msg.WM_NCPAINT };
			driver.DispatchMessage(ref msg);
			msg = new MSG { hwnd = this.Handle, message = Msg.WM_PAINT };
			driver.DispatchMessage(ref msg);

			this.DirtyRectangle = null;
		}

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                var oldFrame = base.Frame;
                base.Frame = value;
				if (oldFrame.Size != value.Size)
					PerformNCCalc(value.Size);
            }
        }

        public override void SetFrameSize(CGSize newSize)
        {
			base.SetFrameSize(newSize);
			PerformNCCalc(newSize);
		}

		public void PerformNCCalc(CGSize newSize)
		{
			//FIXME! Should not reference Win32 variant here or NEED to do so.
			XplatUIWin32.NCCALCSIZE_PARAMS ncp = new XplatUIWin32.NCCALCSIZE_PARAMS ();
			IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(ncp));

			Rectangle rect = new Rectangle(0, 0, (int)newSize.Width, (int)newSize.Height);
			ncp.rgrc1.left = rect.Left;
			ncp.rgrc1.top = rect.Top;
			ncp.rgrc1.right = rect.Right;
			ncp.rgrc1.bottom = rect.Bottom;

			Marshal.StructureToPtr(ncp, ptr, true);
			NativeWindow.WndProc(Handle, Msg.WM_NCCALCSIZE, (IntPtr) 1, ptr);
			ncp = (XplatUIWin32.NCCALCSIZE_PARAMS) Marshal.PtrToStructure (ptr, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));
			Marshal.FreeHGlobal(ptr);

			var clientRect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			var savedBounds = ClientBounds;
			ClientBounds = clientRect.ToCGRect();

			// Update subview locations
			var offset = new CGPoint(ClientBounds.X - savedBounds.X, ClientBounds.Y - savedBounds.Y);
			if (offset.X != 0 || offset.Y != 0)
			{
				foreach (var subView in Subviews)
				{
					var frameOrigin = subView.Frame.Location;
					frameOrigin.X += offset.X;
					frameOrigin.Y += offset.Y;
					subView.SetFrameOrigin(frameOrigin);
				}
			}
		}

		public CGRect ClientBounds
		{
			get
			{
				return new CGRect(
					NMath.Max(clientBounds.Left, 0),
					NMath.Max(clientBounds.Top, 0),
					NMath.Max(clientBounds.Width, 0),
					NMath.Max(clientBounds.Height, 0));
			}
			set
			{
				this.clientBounds = value;
			}
		}

		internal virtual void DrawBorders()
		{
			if (ExStyle.HasFlag(WindowExStyles.WS_EX_CLIENTEDGE) || ExStyle.HasFlag(WindowExStyles.WS_EX_STATICEDGE))
			{
				using (var g = Graphics.FromHwnd(Handle, false))
				{
					if (ExStyle.HasFlag(WindowExStyles.WS_EX_STATICEDGE))
						ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, (int)Frame.Width, (int)Frame.Height), Border3DStyle.SunkenOuter);
					else
						ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, (int)Frame.Width, (int)Frame.Height), Border3DStyle.Sunken);
				}
			}
			else if (Style.HasFlag(WindowStyles.WS_BORDER))
			{
				Color color = NSColor.Grid.ToSDColor(); // Color.Black
				using (var g = Graphics.FromHwnd(Handle, false))
					ControlPaint.DrawBorder(g, new Rectangle(0, 0, (int)Frame.Width, (int)Frame.Height), color, ButtonBorderStyle.Solid);
			}
		}

		public override void CursorUpdate(NSEvent theEvent)
		{
			// Caching cursor (this.Cursor) does not work well, because of logic in Control.Cursor getter.
			var cursor = Control.FromHandle(Handle)?.Cursor?.Handle ?? IntPtr.Zero;
			driver.OverrideCursor(cursor);
		}

		public Rectangle? DirtyRectangle { get; private set; }
		public WindowStyles Style { get; set; }
		public WindowExStyles ExStyle { get; set; }
		public Region UserClip { get; set; }
		public IntPtr Cursor { get; set; }

		public bool Enabled
		{
			get { return !Style.HasFlag(WindowStyles.WS_DISABLED); }
			set
			{
				if (value)
					Style &= ~WindowStyles.WS_DISABLED;
				else
					Style |= WindowStyles.WS_DISABLED;
			}
		}

		// Experimental
		public override void DrawFocusRingMask()
		{
			var control = Control.FromHandle(Handle);
			if (control != null && control.Enabled && control.Focused && control.ShowFocusCues)
			{
				NSBezierPath.FillRect(Bounds);
			}
		}

		public override CGRect FocusRingMaskBounds
		{
			get { return Bounds; }
		}

		public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
		{
			var control = Control.FromHandle(Handle);
			if (null != control && control.AllowDrop)
			{
				var e = ToDragEventArgs(sender);
				control.DndEnter(e);
				if (e.Effect != UnusedDndEffect)
					return (XplatUICocoa.DraggingEffects = e.Effect).ToDragOperation();

				XplatUICocoa.DraggingEffects = DragDropEffects.None;
			}
			return NSDragOperation.Generic;
		}

		public override NSDragOperation DraggingUpdated(NSDraggingInfo sender)
		{
			var control = Control.FromHandle(Handle);
			if (null != control && control.AllowDrop)
			{
				var e = ToDragEventArgs(sender);
				control.DndOver(e);
				if (e.Effect != UnusedDndEffect)
					XplatUICocoa.DraggingEffects = e.Effect;

				return XplatUICocoa.DraggingEffects.ToDragOperation();
			}
			return NSDragOperation.Generic;
		}

		public override void DraggingExited(NSDraggingInfo sender)
		{
			var control = Control.FromHandle(Handle);
			if (null != control && control.AllowDrop)
			{
				var e = ToDragEventArgs(sender);
				control.DndLeave(e);
			}
		}

		const DragDropEffects UnusedDndEffect = unchecked((DragDropEffects)0xffffffff);

		DragEventArgs ToDragEventArgs(NSDraggingInfo sender, DragDropEffects effect = UnusedDndEffect)
		{
			var q = ToMonoScreen(sender.DraggingLocation, null);
			var allowed = XplatUICocoa.DraggingAllowedEffects;
			var modifiers = NSEvent.CurrentModifierFlags.ToKeys();
			var idata = XplatUICocoa.DraggedData as IDataObject ?? (IDataObject)(XplatUICocoa.DraggedData = ToIDataObject(sender.DraggingPasteboard));
			return new DragEventArgs(idata, (int)modifiers, q.X, q.Y, allowed, effect);
		}

		public override void DraggingEnded(NSDraggingInfo sender)
		{
			XplatUICocoa.DraggedData = null; // Clear data box for next dragging session
		}

		public override bool PrepareForDragOperation(NSDraggingInfo sender)
		{
			foreach(var type in sender.DraggingPasteboard.Types)
			{
				switch(type)
				{
					case XplatUICocoa.UTTypeUTF8PlainText:
					case XplatUICocoa.NSStringPboardType:
					case XplatUICocoa.IDataObjectPboardType:
					case XplatUICocoa.UTTypeFileUrl:
						return true;
				}
			}
			return false;
		}

		public override bool PerformDragOperation(NSDraggingInfo sender)
		{
			var c = Control.FromHandle(Handle);
			if (c is IDropTarget dt)
			{
				var e = ToDragEventArgs(sender, XplatUICocoa.DraggingEffects);
				if (e != null)
				{
					dt.OnDragDrop(e);
					return true;
				}
			}
			return false;
		}

		public Point ToMonoScreen(CGPoint p, NSView view)
		{
			if (view != null)
				p = ConvertPointToView(p, null);
			var r = Window.ConvertRectToScreen(new CGRect(p, CGSize.Empty));
			return driver.NativeToMonoScreen(r.Location);
		}

		private IDataObject ToIDataObject(NSPasteboard pboard)
		{
			var types = pboard.Types;
			if (Array.IndexOf(types, XplatUICocoa.IDataObjectPboardType) != -1)
				if (XplatUICocoa.DraggedData is IDataObject idata)
					return idata;

			var s = pboard.GetStringForType(XplatUICocoa.NSStringPboardType);
			if (s != null)
				return new DataObject(s);

			s = pboard.GetStringForType(XplatUICocoa.UTTypeFileUrl);
			if (s != null)
			{
				var paths = new List<string>();
				foreach (var item in pboard.PasteboardItems)
				{
					var url = item.GetStringForType(XplatUICocoa.UTTypeFileUrl);
					paths.Add(NSUrl.FromString(url).Path);
				}

				if (paths.Count != 0)
					return new DataObject(DataFormats.FileDrop, paths.ToArray());
			}

			// TODO: Add more conversions/wrappers - for files, images etc.

			return null;
		}
	}

	static class NSViewExtensions
	{
		public static T ClosestParentOfType<T>(this NSView view) where T : NSView
		{
			while (view != null && !(view is T))
				view = view.Superview;
			return view as T;
		}
	}
}

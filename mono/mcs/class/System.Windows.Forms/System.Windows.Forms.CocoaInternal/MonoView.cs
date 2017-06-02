﻿//
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
		protected WindowsEventResponder eventReponder;
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

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			return true;
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

			clientArea = new NSTrackingArea(
				Bounds,
				NSTrackingAreaOptions.ActiveInActiveApp |
				NSTrackingAreaOptions.MouseEnteredAndExited |
				NSTrackingAreaOptions.MouseMoved |
				NSTrackingAreaOptions.InVisibleRect |
				NSTrackingAreaOptions.CursorUpdate,
				this,
				new NSDictionary());
			AddTrackingArea(clientArea);

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
			driver.OverrideCursor(this.Cursor);
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
				var q = ToMonoScreen(sender.DraggingLocation, null);
				var allowed = XplatUICocoa.DraggingAllowedEffects;
				var modifiers = NSEvent.CurrentModifierFlags.ToKeys();
				var e = new DragEventArgs(XplatUICocoa.DraggedData as IDataObject, (int)modifiers, q.X, q.Y, allowed, 0);
				control.DndEnter(e);
				//XplatUICocoa.DraggingEffects = e.Effect;
			}
			return NSDragOperation.Generic;
		}

		public override NSDragOperation DraggingUpdated(NSDraggingInfo sender)
		{
			var control = Control.FromHandle(Handle);
			if (null != control && control.AllowDrop)
			{
				var q = ToMonoScreen(sender.DraggingLocation, null);
				var allowed = XplatUICocoa.DraggingAllowedEffects;
				var modifiers = NSEvent.CurrentModifierFlags.ToKeys();
				var e = new DragEventArgs(XplatUICocoa.DraggedData as IDataObject, (int)modifiers, q.X, q.Y, allowed, 0);
				control.DndOver(e);

				XplatUICocoa.DraggingEffects = e.Effect;
				return e.Effect.ToDragOperation();
			}
			return NSDragOperation.None;
		}

		public override void DraggingExited(NSDraggingInfo sender)
		{
			var control = Control.FromHandle(Handle);
			if (null != control && control.AllowDrop)
			{
				var q = ToMonoScreen(sender.DraggingLocation, null);
				var allowed = XplatUICocoa.DraggingAllowedEffects;
				var modifiers = NSEvent.CurrentModifierFlags.ToKeys();
				var e = new DragEventArgs(XplatUICocoa.DraggedData as IDataObject, (int)modifiers, q.X, q.Y, allowed, 0);
				control.DndLeave(e);
			}
		}

		public override void DraggingEnded(NSDraggingInfo sender)
		{
		}

		public override bool PrepareForDragOperation(NSDraggingInfo sender)
		{
			foreach(var type in sender.DraggingPasteboard.Types)
			{
				switch(type)
				{
					case XplatUICocoa.PublicUtf8PlainText:
					case XplatUICocoa.NSStringPboardType:
					case XplatUICocoa.IDataObjectPboardType:
						return true;
				}
			}
			return false;
		}

		public Point ToMonoScreen(CGPoint p, NSView view)
		{
			if (view != null)
				p = ConvertPointToView(p, null);
			var r = Window.ConvertRectToScreen( new CGRect(p, CGSize.Empty));
			return driver.NativeToMonoScreen(r.Location);
		}

		public override bool PerformDragOperation(NSDraggingInfo sender)
		{
			var c = Control.FromHandle(Handle);
			if (c is IDropTarget dt)
			{
				var effects = XplatUICocoa.DraggingEffects; //ToDragDropEffects(sender.DraggingSourceOperationMask);
				var types = sender.DraggingPasteboard.Types;
				var allowed = XplatUICocoa.DraggingAllowedEffects;

				foreach (var type in types)
				{
					switch (type)
					{
						case XplatUICocoa.PublicUtf8PlainText:
						case XplatUICocoa.NSStringPboardType:
						{
							var str = sender.DraggingPasteboard.GetStringForType(type);
							var data = new DataObject(DataFormats.Text, str);
							var q = ToMonoScreen(sender.DraggingLocation, null);
							var modifiers = (int)NSEvent.CurrentModifierFlags.ToKeys();
							var e = new DragEventArgs(data, modifiers, q.X, q.Y, allowed, effects);
							dt.OnDragDrop(e);
							return true;
						}
						case XplatUICocoa.IDataObjectPboardType:
						{
							if (XplatUICocoa.DraggedData is IDataObject idata)
							{
								var q = ToMonoScreen(sender.DraggingLocation, null);
								var modifiers = (int)NSEvent.CurrentModifierFlags.ToKeys();
								var e = new DragEventArgs(idata, modifiers, q.X, q.Y, allowed, effects);
								dt.OnDragDrop(e);
								return true;
							}
							break;
						}
					}
				}
			}
			return false;
		}
	}
}

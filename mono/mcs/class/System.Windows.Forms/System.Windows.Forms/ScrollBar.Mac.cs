#if MACOS_THEME

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing.Mac;

#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[DefaultEvent("Scroll")]
	[DefaultProperty("Value")]
	public abstract partial class ScrollBar : Control, IMacNativeControl
	{
		NSScroller scroller;

		int minimum = 0;
		int maximum = 100;
		int large_change = 10;
		int small_change = 1;
		int position = 0;

		internal int manual_thumb_size;
		internal bool use_manual_thumb_size;
		internal bool implicit_control;
		internal bool vert;

		public NSView CreateView()
		{
			var scrollView = new NSScrollView();
			if (vert)
			{
				scrollView.HasVerticalScroller = true;
				scroller = scrollView.VerticalScroller;
			}
			else
			{
				scrollView.HasHorizontalScroller = true;
				scroller = scrollView.HorizontalScroller;
			}
			scroller.RemoveFromSuperview();
			scroller.Frame = Bounds.ToCGRect();

			scroller.DoubleValue = 0.0;
			scroller.Activated += HandleScroller;

			UpdateScroller();
			return scroller;
		}

		internal virtual void HandleScroller(object sender, EventArgs e)
		{
			double val = scroller.DoubleValue;
			double scale = maximum - minimum - large_change;
			double pos = scale * val + minimum;

			int newPosition = (int)Math.Round(pos);

			Value = newPosition;

			OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, position));
			OnScroll(new ScrollEventArgs(ScrollEventType.EndScroll, position));
		}

		internal virtual void UpdateScroller()
		{
			if (scroller != null)
			{
				double scale = maximum - minimum - large_change;
				double pos = position - minimum;
				double val = (scale != 0.0) ? pos / scale : 0.0;

				scroller.KnobProportion = ((float)(large_change)) / (float)(maximum - minimum);

				scroller.DoubleValue = val;
			}
		}

		#region events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged
		{
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged
		{
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged
		{
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Click
		{
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick
		{
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler FontChanged
		{
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged
		{
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged
		{
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseClick
		{
			add { base.MouseClick += value; }
			remove { base.MouseClick -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick
		{
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDown
		{
			add { base.MouseDown += value; }
			remove { base.MouseDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove
		{
			add { base.MouseMove += value; }
			remove { base.MouseMove -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseUp
		{
			add { base.MouseUp += value; }
			remove { base.MouseUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint
		{
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		static object ScrollEvent = new object();
		static object ValueChangedEvent = new object();

		public event ScrollEventHandler Scroll
		{
			add { Events.AddHandler(ScrollEvent, value); }
			remove { Events.RemoveHandler(ScrollEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged
		{
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public event EventHandler ValueChanged
		{
			add { Events.AddHandler(ValueChangedEvent, value); }
			remove { Events.RemoveHandler(ValueChangedEvent, value); }
		}
		#endregion Events

		public ScrollBar()
		{
			base.TabStop = false;
			base.Cursor = Cursors.Default;

			SetStyle(ControlStyles.UserPaint | ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);
		}

		#region Public Properties
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool AutoSize
		{
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				if (base.BackColor == value)
					return;
				base.BackColor = value;
				Refresh();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set
			{
				if (base.BackgroundImage == value)
					return;

				base.BackgroundImage = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override ImageLayout BackgroundImageLayout
		{
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		protected override CreateParams CreateParams
		{
			get { return base.CreateParams; }
		}

		protected override Padding DefaultMargin
		{
			get { return Padding.Empty; }
		}

		protected override ImeMode DefaultImeMode
		{
			get { return ImeMode.Disable; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Font Font
		{
			get { return base.Font; }
			set
			{
				if (base.Font.Equals(value))
					return;

				base.Font = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set
			{
				if (base.ForeColor == value)
					return;

				base.ForeColor = value;
				Refresh();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new ImeMode ImeMode
		{
			get { return base.ImeMode; }
			set
			{
				if (base.ImeMode == value)
					return;

				base.ImeMode = value;
			}
		}

		[DefaultValue(10)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFDescription("Scroll amount when clicking in the scroll area"), MWFCategory("Behaviour")]
		public int LargeChange
		{
			get { return Math.Min(large_change, maximum - minimum + 1); }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("LargeChange", string.Format("Value '{0}' must be greater than or equal to 0.", value));

				if (large_change != value)
				{
					large_change = value;

					UpdateScroller();

					// UIA Framework: Generate UIA Event to indicate LargeChange change
					OnUIAValueChanged(new ScrollEventArgs(ScrollEventType.LargeIncrement, value));
				}
			}
		}

		[DefaultValue(100)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFDescription("Highest value for scrollbar"), MWFCategory("Behaviour")]
		public int Maximum
		{
			get { return maximum; }
			set
			{
				if (maximum == value)
					return;

				maximum = value;

				// UIA Framework: Generate UIA Event to indicate Maximum change
				OnUIAValueChanged(new ScrollEventArgs(ScrollEventType.Last, value));

				if (maximum < minimum)
					minimum = maximum;
				if (Value > maximum)
					Value = maximum;

				UpdateScroller();
			}
		}

		internal void SetValues(int maximum, int large_change)
		{
			SetValues(-1, maximum, -1, large_change);
		}

		internal void SetValues(int minimum, int maximum, int small_change, int large_change)
		{
			bool update = false;

			if (-1 != minimum && this.minimum != minimum)
			{
				this.minimum = minimum;

				if (minimum > this.maximum)
					this.maximum = minimum;
				update = true;

				// change the position if it is out of range now
				position = Math.Max(position, minimum);
			}

			if (-1 != maximum && this.maximum != maximum)
			{
				this.maximum = maximum;

				if (maximum < this.minimum)
					this.minimum = maximum;
				update = true;

				// change the position if it is out of range now
				position = Math.Min(position, maximum);
			}

			if (-1 != small_change && this.small_change != small_change)
			{
				this.small_change = small_change;
			}

			if (this.large_change != large_change)
			{
				this.large_change = large_change;
				update = true;
			}

			if (update)
			{
				UpdateScroller();
			}
		}

		[DefaultValue(0)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFDescription("Smallest value for scrollbar"), MWFCategory("Behaviour")]
		public int Minimum
		{
			get { return minimum; }
			set
			{
				if (minimum == value)
					return;

				minimum = value;

				// UIA Framework: Generate UIA Event to indicate Minimum change
				OnUIAValueChanged(new ScrollEventArgs(ScrollEventType.First, value));

				if (minimum > maximum)
					maximum = minimum;

				UpdateScroller();
			}
		}

		[DefaultValue(1)]
		[MWFDescription("Scroll amount when clicking scroll arrows"), MWFCategory("Behaviour")]
		public int SmallChange
		{
			get { return small_change > LargeChange ? LargeChange : small_change; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("SmallChange", string.Format("Value '{0}' must be greater than or equal to 0.", value));

				if (small_change != value)
				{
					small_change = value;

					UpdateScroller();

					// UIA Framework: Generate UIA Event to indicate SmallChange change
					OnUIAValueChanged(new ScrollEventArgs(ScrollEventType.SmallIncrement, value));
				}
			}
		}

		[DefaultValue(false)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Bindable(true)]
		[DefaultValue(0)]
		[MWFDescription("Current value for scrollbar"), MWFCategory("Behaviour")]
		public int Value
		{
			get { return position; }
			set
			{
				if (value < minimum || value > maximum)
					throw new ArgumentOutOfRangeException("Value", string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				if (position != value)
				{
					position = value;

					OnValueChanged(EventArgs.Empty);

					if (IsHandleCreated)
					{
						UpdateScroller();
					}
				}
			}
		}

		#endregion //Public Properties

		#region Public Methods
		protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
		{
			// Basically, we want to keep our small edge and scale the long edge
			// ie: if we are vertical, don't scale our width
			if (vert)
				return base.GetScaledBounds(bounds, factor, (specified & BoundsSpecified.Height) | (specified & BoundsSpecified.Location));
			else
				return base.GetScaledBounds(bounds, factor, (specified & BoundsSpecified.Width) | (specified & BoundsSpecified.Location));
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
		}

		protected override void OnHandleCreated(System.EventArgs e)
		{
			base.OnHandleCreated(e);
		}

		protected virtual void OnScroll(ScrollEventArgs se)
		{
			ScrollEventHandler eh = (ScrollEventHandler)(Events[ScrollEvent]);
			if (eh == null)
				return;

			if (se.NewValue < Minimum)
			{
				se.NewValue = Minimum;
			}

			if (se.NewValue > Maximum)
			{
				se.NewValue = Maximum;
			}

			eh(this, se);
		}

		private void SendWMScroll(ScrollBarCommands cmd)
		{
			if ((Parent != null) && Parent.IsHandleCreated)
			{
				if (vert)
				{
					XplatUI.SendMessage(Parent.Handle, Msg.WM_VSCROLL, (IntPtr)cmd, implicit_control ? IntPtr.Zero : Handle);
				}
				else
				{
					XplatUI.SendMessage(Parent.Handle, Msg.WM_HSCROLL, (IntPtr)cmd, implicit_control ? IntPtr.Zero : Handle);
				}
			}
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[ValueChangedEvent]);
			if (eh != null)
				eh(this, e);
		}

		public override string ToString()
		{
			return string.Format("{0}, Minimum: {1}, Maximum: {2}, Value: {3}",
						GetType().FullName, minimum, maximum, position);
		}

		protected void UpdateScrollInfo()
		{
			UpdateScroller();
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
		}

		/// <include file='doc\ScrollBar.uex' path='docs/doc[@for="ScrollBar.OnMouseWheel"]/*' />
        /// <devdoc>
        ///     Converts mouse wheel movements into scrolling, when scrollbar has the focus.
        ///     Typically one wheel step will cause one small scroll increment, in either
        ///     direction. A single wheel message could represent either one wheel step, multiple
        ///     wheel steps (fast wheeling), or even a fraction of a step (smooth-wheeled mice).
        ///     So we accumulate the total wheel delta, and consume it in whole numbers of steps.
        /// </devdoc>
		private int wheelDelta = 0;
		const int WHEEL_DELTA = 120;

		protected override void OnMouseWheel(MouseEventArgs e) {
            wheelDelta += e.Delta;
 
            bool scrolled = false;
 
            while (Math.Abs(wheelDelta) >= WHEEL_DELTA) {
                if (wheelDelta > 0) {
                    wheelDelta -= WHEEL_DELTA;
                    DoScroll(ScrollEventType.SmallDecrement);
                    scrolled = true;
                }
                else {
                    wheelDelta += WHEEL_DELTA;
                    DoScroll(ScrollEventType.SmallIncrement);
                    scrolled = true;
                }
            }
 
            if (scrolled) {
                DoScroll(ScrollEventType.EndScroll);
            }
 
            if (e is HandledMouseEventArgs) {
                ((HandledMouseEventArgs) e).Handled = true;
            }
 
            // The base implementation should be called before the implementation above,
            // but changing the order in Whidbey would be too much of a breaking change
            // for this particular class.
            base.OnMouseWheel(e);
        }

		private void DoScroll(ScrollEventType type) 
		{
            // For Rtl systems we need to swap increment and decrement
            //
            if (RightToLeft == RightToLeft.Yes) {
                switch (type) {
                    case ScrollEventType.First:
                        type = ScrollEventType.Last;
                        break;
 
                    case ScrollEventType.Last:
                        type = ScrollEventType.First;
                        break;
 
                    case ScrollEventType.SmallDecrement:
                        type = ScrollEventType.SmallIncrement;
                        break;
 
                    case ScrollEventType.SmallIncrement:
                        type = ScrollEventType.SmallDecrement;
                        break;
 
                    case ScrollEventType.LargeDecrement:
                        type = ScrollEventType.LargeIncrement;
                        break;
 
                    case ScrollEventType.LargeIncrement:
                        type = ScrollEventType.LargeDecrement;
                        break;
                }
            }
            
			int newValue = position;
			int oldValue = position;
 
            // The ScrollEventArgs constants are defined in terms of the windows
            // messages..  this eliminates confusion between the VSCROLL and
            // HSCROLL constants, which are identical.
            //
            switch (type) {
                case ScrollEventType.First:
                    newValue = minimum;
                    break;
 
                case ScrollEventType.Last:
                    newValue = maximum - LargeChange + 1; // si.nMax - si.nPage + 1;
                    break;
 
                case ScrollEventType.SmallDecrement:
					newValue = Math.Max(position - SmallChange, minimum);
                    break;
 
                case ScrollEventType.SmallIncrement:
					newValue = Math.Min(position + SmallChange, maximum - LargeChange + 1); // max - lChange + 1);
                    break;
 
                case ScrollEventType.LargeDecrement:
					newValue = Math.Max(position - LargeChange, minimum);
                    break;
 
                case ScrollEventType.LargeIncrement:
					newValue = Math.Min(position + LargeChange, maximum - LargeChange + 1); // si.nPos + si.nPage,si.nMax - si.nPage + 1);
                    break;
 
                //case ScrollEventType.ThumbPosition:
                //case ScrollEventType.ThumbTrack:
                    //NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO();
                    //si.fMask = NativeMethods.SIF_TRACKPOS;
                    //SafeNativeMethods.GetScrollInfo(new HandleRef(this, Handle), NativeMethods.SB_CTL, si);
                    
                    //if (RightToLeft == RightToLeft.Yes) {
                    //    newValue = ReflectPosition(si.nTrackPos);
                    //}
                    //else {
                    //    newValue = si.nTrackPos;
                    //}
                    
                    //break;
            }
 
			ScrollEventArgs se = new ScrollEventArgs(type, oldValue, newValue, vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll);
            OnScroll(se);
            Value = se.NewValue;
        }


		#endregion //Public Methods

		#region Private Methods

		// I hate to do this, but we don't have the resources to track
		// down everything internal that is setting a value outside the
		// correct range, so we'll clamp it to the acceptable values.
		internal void SafeValueSet(int value)
		{
			value = Math.Min(value, maximum);
			value = Math.Max(value, minimum);

			Value = value;
		}

		#endregion //Private Methods

		#region UIA Framework Section: Events, Methods and Properties.

		//NOTE:
		//	We are using Reflection to add/remove internal events.
		//	Class ScrollBarButtonInvokePatternInvokeEvent uses the events.
		//
		// Types used to generate UIA InvokedEvent
		// * args.Type = ScrollEventType.LargeIncrement. Space between Thumb and bottom/right Button
		// * args.Type = ScrollEventType.LargeDecrement. Space between Thumb and top/left Button
		// * args.Type = ScrollEventType.SmallIncrement. Small increment UIA Button (bottom/right Button)
		// * args.Type = ScrollEventType.SmallDecrement. Small decrement UIA Button (top/left Button)
		// Types used to generate RangeValue-related events
		// * args.Type = ScrollEventType.LargeIncrement. LargeChange event
		// * args.Type = ScrollEventType.Last. Maximum event
		// * args.Type = ScrollEventType.First. Minimum event
		// * args.Type = ScrollEventType.SmallIncrement. SmallChange event
		static object UIAScrollEvent = new object();
		static object UIAValueChangeEvent = new object();

		internal void OnUIAValueChanged(ScrollEventArgs args)
		{
			ScrollEventHandler eh = (ScrollEventHandler)Events[UIAValueChangeEvent];
			if (eh != null)
				eh(this, args);
		}

		#endregion UIA Framework Section: Events, Methods and Properties.
	}
}

#endif

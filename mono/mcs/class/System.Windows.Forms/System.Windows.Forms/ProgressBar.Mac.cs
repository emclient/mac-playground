#if MACOS_THEME

using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.AppKit;
#elif XAMARINMAC
using AppKit;
#endif

namespace System.Windows.Forms
{
	[DefaultProperty("Value")]
	[DefaultBindingProperty("Value")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class ProgressBar : Control, IMacNativeControl
	{
		NSProgressIndicator indicator;

		int minimum = 0;
		int maximum = 100;
		bool right_to_left_layout = false;

		internal int step = 10;
		internal int val = 0;
		internal ProgressBarStyle style;


		public NSView CreateView()
		{
			indicator = new NSProgressIndicator();
			indicator.Style = NSProgressIndicatorStyle.Bar;
			indicator.ControlSize = NSControlSize.Regular;
			indicator.IsDisplayedWhenStopped = true;
			indicator.Indeterminate = style == ProgressBarStyle.Marquee;

			indicator.MinValue = minimum;
			indicator.MaxValue = maximum;

			if (style == ProgressBarStyle.Marquee)
				indicator.StartAnimation(indicator);

			return indicator;
		}

		static object RightToLeftLayoutChangedEvent = new object();

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
		public new event EventHandler CausesValidationChanged
		{
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
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
		public new event EventHandler Enter
		{
			add { base.Enter += value; }
			remove { base.Enter -= value; }
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
		public new event EventHandler ImeModeChanged
		{
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown
		{
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress
		{
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp
		{
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Leave
		{
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new event MouseEventHandler MouseDoubleClick
		{
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged
		{
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint
		{
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public event EventHandler RightToLeftLayoutChanged
		{
			add { Events.AddHandler(RightToLeftLayoutChangedEvent, value); }
			remove { Events.RemoveHandler(RightToLeftLayoutChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged
		{
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged
		{
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public ProgressBar()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable | ControlStyles.ResizeRedraw | ControlStyles.Opaque | ControlStyles.UseTextForAccessibility, false);
			force_double_buffer = true;
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AllowDrop
		{
			get { return base.AllowDrop; }
			set
			{
				base.AllowDrop = value;
			}
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a BackgroundImageChanged event
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout
		{
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool CausesValidation
		{
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}

		protected override CreateParams CreateParams
		{
			get { return base.CreateParams; }
		}

		protected override ImeMode DefaultImeMode
		{
			get { return base.DefaultImeMode; }
		}

		protected override Size DefaultSize
		{
			get { return ThemeEngine.Current.ProgressBarDefaultSize; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool DoubleBuffered
		{
			get { return base.DoubleBuffered; }
			set { base.DoubleBuffered = value; }
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a FontChanged event
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode
		{
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(100)]
		public int Maximum
		{
			get { return maximum; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' must be greater than or equal to 0.");

				maximum = value;
				minimum = Math.Min(minimum, maximum);
				val = Math.Min(val, maximum);

				UpdateIndicator(minimum, maximum, val);
			}
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(0)]
		public int Minimum
		{
			get
			{
				return minimum;
			}
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' must be greater than or equal to 0.");

				minimum = value;
				maximum = Math.Max(maximum, minimum);
				val = Math.Max(val, minimum);

				UpdateIndicator(minimum, maximum, val);
			}
		}

		internal void UpdateIndicator(int min, int max, int val)
		{
			if (indicator != null)
			{
				indicator.MinValue = min;
				indicator.MaxValue = max;
				indicator.DoubleValue = val;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Padding Padding
		{
			get { return base.Padding; }
			set { base.Padding = value; }
		}

		[Localizable(true)]
		[DefaultValue(false)]
		[MonoTODO("RTL is not supported")]
		public virtual bool RightToLeftLayout
		{
			get { return right_to_left_layout; }
			set
			{
				if (right_to_left_layout != value)
				{
					right_to_left_layout = value;
					OnRightToLeftLayoutChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(10)]
		public int Step
		{
			get { return step; }
			set
			{
				step = value;
			}
		}

		[Browsable(true)]
		[DefaultValue(ProgressBarStyle.Blocks)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		public ProgressBarStyle Style
		{
			get
			{
				return style;
			}

			set
			{
				if (value != ProgressBarStyle.Blocks && value != ProgressBarStyle.Continuous && value != ProgressBarStyle.Marquee)
					throw new InvalidEnumArgumentException(nameof(value), unchecked((int)value), typeof(ProgressBarStyle));

				if (style != value)
				{
					style = value;
					if (indicator == null)
						return;

					switch(style)
					{
						case ProgressBarStyle.Marquee:
							indicator.Indeterminate = true;
							indicator.StartAnimation(indicator);
							break;
						default:
							indicator.Indeterminate = false;
							indicator.StopAnimation(indicator);
							break;
					}
				}
			}
		}

		[DefaultValue(100)]
		public int MarqueeAnimationSpeed
		{
			get; set;
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Bindable(false)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Bindable(true)]
		[DefaultValue(0)]
		public int Value
		{
			get
			{
				return val;
			}
			set
			{
				if (value < Minimum || value > Maximum)
					throw new ArgumentOutOfRangeException(nameof(Value), $"'{value}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'");

				val = value;
				if (indicator != null)
					indicator.DoubleValue = value;
			}
		}


		public void Increment(int value)
		{
			if (Style == ProgressBarStyle.Marquee)
				throw new InvalidOperationException("Increment should not be called if the style is Marquee.");

			int newValue = Value + value;

			if (newValue < Minimum)
				newValue = Minimum;

			if (newValue > Maximum)
				newValue = Maximum;

			Value = newValue;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
		{
			var eh = (EventHandler)Events[RightToLeftLayoutChangedEvent];
			if (eh != null)
				eh(this, e);
		}

		public void PerformStep()
		{
			if (Style == ProgressBarStyle.Marquee)
				throw new InvalidOperationException("PerformStep should not be called if the style is Marquee.");

			Increment(Step);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void ResetForeColor()
		{
		}

		public override string ToString()
		{
			return $"{GetType().FullName}, Minimum: {Minimum}, Maximum: {Maximum}, Value: {Value}";
		}
	}
}
#endif //MACOS_THEME

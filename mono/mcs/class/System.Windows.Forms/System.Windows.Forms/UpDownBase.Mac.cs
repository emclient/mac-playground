#if MACOS_THEME

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;

#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms
{
	public abstract partial class UpDownBase
	{
		public UpDownBase()
		{
			_UpDownAlign = LeftRightAlignment.Right;
			InternalBorderStyle = BorderStyle.None;

			spnSpinner = new UpDownSpinner(this);

			txtView = new UpDownTextBox(this);
			txtView.ModifiedChanged += OnChanged;
			txtView.AcceptsReturn = true;
			txtView.AutoSize = false;
			txtView.BorderStyle = BorderStyle.Fixed3D;
			txtView.TabIndex = TabIndex;

			spnSpinner.Size = spnSpinner.PreferredSize;
			spnSpinner.Dock = DockStyle.Right;

			txtView.Dock = DockStyle.Left; 

			SuspendLayout();
			Controls.Add(spnSpinner);
			Controls.Add(txtView);
			ResumeLayout();

			SuspendLayout();
			txtView.Size = new Size(Width - spnSpinner.Width - 4, txtView.PreferredHeight);
			txtView.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			spnSpinner.Size = spnSpinner.PreferredSize;
			spnSpinner.Anchor = AnchorStyles.Right;
			ResumeLayout();

			Height = PreferredHeight;

			TabIndexChanged += TabIndexChangedHandler;

			txtView.KeyDown += OnTextBoxKeyDown;
			txtView.KeyPress += OnTextBoxKeyPress;
//			txtView.LostFocus += OnTextBoxLostFocus;
			txtView.Resize += OnTextBoxResize;
			txtView.TextChanged += OnTextBoxTextChanged;

			// So the child controls don't get auto selected when the updown is selected
			auto_select_child = false;
			SetStyle(ControlStyles.FixedHeight | ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			base.BackColor = Color.Transparent;
		}

		protected virtual void OnTextBoxResize(object source, EventArgs e)
		{
			Height = PreferredHeight;
			txtView.Top = (ClientSize.Height - txtView.Height) / 2;
			spnSpinner.Top = -1 + (ClientSize.Height - spnSpinner.Height) / 2;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public int PreferredHeight
		{
			get
			{
				int hTextView = txtView != null ? txtView.PreferredHeight : 0;
				int hSpinner = spnSpinner != null ? spnSpinner.PreferredSize.Height : 0;
				return Math.Max(hTextView, hSpinner);
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			spnSpinner.Enabled = Enabled;
			base.OnEnabledChanged(e);
		}

		internal sealed class UpDownSpinner : Control, IMacNativeControl
		{
			UpDownBase owner;
			NSStepper stepper;

			public UpDownSpinner(UpDownBase owner)
			{
				this.owner = owner;

				SetStyle(ControlStyles.AllPaintingInWmPaint
					   | ControlStyles.DoubleBuffer
					   | ControlStyles.Opaque
					   | ControlStyles.ResizeRedraw
					   | ControlStyles.UserPaint, true);
					 //| ControlStyles.FixedHeight, true);
				SetStyle(ControlStyles.Selectable, false);

				stepper = new NSStepper();
				stepper.SizeToFit();
			}

			public override Size GetPreferredSize(Size proposedSize)
			{
				if (stepper != null)
					return stepper.SizeThatFits(proposedSize.ToCGSize()).ToSDSize();
				
				return base.GetPreferredSize(proposedSize);
			}

			public NSView CreateView()
			{
				stepper.IntValue = 0;
				stepper.MinValue = int.MinValue;
				stepper.MaxValue = int.MaxValue;
				stepper.ValueWraps = false;
				stepper.Activated += StepperActivated;
				return stepper;
			}

			internal void StepperActivated(object sender, EventArgs e)
			{
				int value = stepper.IntValue;
				stepper.IntValue = 0;

				if (value > 0)
					owner.UpButton();
				else if (value < 0)
					owner.DownButton();
			}

			protected override void OnEnabledChanged(EventArgs e)
			{
				stepper.Enabled = Enabled;
				base.OnEnabledChanged(e);
			}
		}
	}
}

#endif

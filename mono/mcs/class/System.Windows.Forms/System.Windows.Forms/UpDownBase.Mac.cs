#if MACOS_THEME

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.CocoaInternal;

#if XAMARINMAC
using AppKit;
using CoreGraphics;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
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
			txtView.ModifiedChanged += new EventHandler(OnChanged);
			txtView.AcceptsReturn = true;
			txtView.AutoSize = false;
			txtView.BorderStyle = BorderStyle.Fixed3D;
			txtView.TabIndex = TabIndex;

			spnSpinner.Width = spnSpinner.PreferredSize.Width;
			spnSpinner.Dock = DockStyle.Right;

			txtView.Dock = DockStyle.Fill;

			SuspendLayout();
			Controls.Add(spnSpinner);
			Controls.Add(txtView);
			ResumeLayout();

			SuspendLayout();
			txtView.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
			txtView.Size = new Size(txtView.Width - spnSpinner.Width - 4, txtView.Height);
			ResumeLayout();

			Height = PreferredHeight;

			TabIndexChanged += new EventHandler(TabIndexChangedHandler);

			txtView.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
			txtView.KeyPress += new KeyPressEventHandler(OnTextBoxKeyPress);
//			txtView.LostFocus += new EventHandler(OnTextBoxLostFocus);
			txtView.Resize += new EventHandler(OnTextBoxResize);
			txtView.TextChanged += new EventHandler(OnTextBoxTextChanged);

			// So the child controls don't get auto selected when the updown is selected
			auto_select_child = false;
			SetStyle(ControlStyles.FixedHeight | ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			base.BackColor = Color.Transparent;
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

		internal sealed class UpDownSpinner : Control, IMacNativeControl
		{
			private UpDownBase owner;
			NSStepper stepper;

			public UpDownSpinner(UpDownBase owner)
			{
				this.owner = owner;

				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.DoubleBuffer, true);
				SetStyle(ControlStyles.Opaque, true);
				SetStyle(ControlStyles.ResizeRedraw, true);
				SetStyle(ControlStyles.UserPaint, true);
				//SetStyle(ControlStyles.FixedHeight, true);
				SetStyle(ControlStyles.Selectable, false);

				stepper = new NSStepper();
				stepper.SizeToFit();
			}

			public override Size GetPreferredSize(Size proposedSize)
			{
				if (stepper != null)
				{
					var s = stepper.FittingSize;
					var i = stepper.AlignmentRectInsets;
					return new Size((int)(s.Width - i.Left - i.Right), (int)(s.Height - i.Top - i.Bottom));
				}
				
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
		}
	}
}

#endif

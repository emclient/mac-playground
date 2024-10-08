﻿using System.ComponentModel;
using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.Mac;
using AppKit;
using CoreGraphics;

namespace System.Windows.Forms
{
	public abstract partial class UpDownBase
	{
		public UpDownBase()
		{
			_UpDownAlign = LeftRightAlignment.Right;
			InternalBorderStyle = BorderStyle.None;

			spnSpinner = new UpDownStepper();
			spnSpinner.UpButton += (sender, e) => { UpButton(); };
			spnSpinner.DownButton += (sender, e) => { DownButton(); };

			txtView = new UpDownTextBox(this);
			txtView.ModifiedChanged += OnChanged;
			txtView.AcceptsReturn = true;
			//txtView.AutoSize = false;
			txtView.BorderStyle = BorderStyle.Fixed3D;
			txtView.TabIndex = TabIndex;
			txtView.SetStyle(ControlStyles.Selectable, true);

			spnSpinner.Size = spnSpinner.PreferredSize;

			Controls.Add(spnSpinner);
			Controls.Add(txtView);

			Height = PreferredHeight;

			TabIndexChanged += TabIndexChangedHandler;

			txtView.KeyDown += OnTextBoxKeyDown;
			txtView.KeyPress += OnTextBoxKeyPress;
//			txtView.LostFocus += OnTextBoxLostFocus;
			txtView.Resize += OnTextBoxResize;
			txtView.TextChanged += OnTextBoxTextChanged;

			// So the child controls don't get auto selected when the updown is selected
			auto_select_child = false;
			SetStyle(ControlStyles.FixedHeight | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			base.BackColor = Color.Transparent;
		}

		protected virtual void OnTextBoxResize(object source, EventArgs e)
		{
		}
		
		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout(e);

			if (txtView != null && spnSpinner != null)
			{
				const int gap = 4;
				spnSpinner.Size = new Size(spnSpinner.PreferredSize.Width, Height);
				txtView.Size = new Size(Width - spnSpinner.Width - gap, Height);
				txtView.Top = (ClientSize.Height - txtView.Height) / 2;
				spnSpinner.Top = (ClientSize.Height - spnSpinner.Height) / 2;

				if (RtlTranslateLeftRight(UpDownAlign) == LeftRightAlignment.Right)
				{
					txtView.Left = 0;
					spnSpinner.Left = txtView.Right + gap;
				}
				else
				{
					spnSpinner.Left = 0;
					txtView.Left = spnSpinner.Right + gap;
				}
			}
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
	}

	internal class Stepper : NSStepper
	{
		internal Size Offset { get; set; }

		public override CGRect Frame
		{
			get
			{
				return base.Frame.Move(-Offset.Width, -Offset.Height);
			}
			set
			{
				base.Frame = value.Move(Offset.Width, Offset.Height);
			}
		}
	}

	internal class UpDownStepper : Control, IMacNativeControl
	{
		Stepper stepper;

		public event EventHandler UpButton;
		public event EventHandler DownButton;

		public Size Offset
		{
			get { return stepper.Offset; }
			set { stepper.Offset = value; }
		}

		public UpDownStepper()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.DoubleBuffer
				   | ControlStyles.Opaque
				   | ControlStyles.ResizeRedraw
				   | ControlStyles.UserPaint, true);
				 //| ControlStyles.FixedHeight, true);
			SetStyle(ControlStyles.Selectable, false);

			stepper = new Stepper();
			stepper.SizeToFit();
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			if (stepper != null)
				return stepper.SizeThatFits(proposedSize.ToCGSize()).Inflate(2,2).ToSDSize();
			
			return new Size(13, 23);
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
				UpButton(this, new EventArgs());
			else if (value < 0)
				DownButton(this, new EventArgs());
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			stepper.Enabled = Enabled;
			base.OnEnabledChanged(e);
		}
	}
}

#if !MACOS_THEME

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	public abstract partial class UpDownBase
	{
		public UpDownBase()
		{
			_UpDownAlign = LeftRightAlignment.Right;
			InternalBorderStyle = BorderStyle.Fixed3D;

			spnSpinner = new UpDownStepper(this);

			txtView = new UpDownTextBox (this);
			txtView.ModifiedChanged += new EventHandler(OnChanged);
			txtView.AcceptsReturn = true;
			txtView.AutoSize = false;
			txtView.BorderStyle = BorderStyle.None;
			txtView.Location = new System.Drawing.Point(17, 17);
			txtView.TabIndex = TabIndex;

			spnSpinner.Width = 16;
			spnSpinner.Dock = DockStyle.Right;
			
			txtView.Dock = DockStyle.Fill;
			
			SuspendLayout ();
			Controls.Add (spnSpinner);
			Controls.Add (txtView);	
			ResumeLayout ();

			SuspendLayout();
			txtView.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
			txtView.Size = new Size(txtView.Width - spnSpinner.Width, txtView.Height);
			ResumeLayout();

			Height = PreferredHeight;
			base.BackColor = txtView.BackColor;

			TabIndexChanged += new EventHandler (TabIndexChangedHandler);
			
			txtView.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
			txtView.KeyPress += new KeyPressEventHandler(OnTextBoxKeyPress);
//			txtView.LostFocus += new EventHandler(OnTextBoxLostFocus);
			txtView.Resize += new EventHandler(OnTextBoxResize);
			txtView.TextChanged += new EventHandler(OnTextBoxTextChanged);

			// So the child controls don't get auto selected when the updown is selected
			auto_select_child = false;
			SetStyle(ControlStyles.FixedHeight, true);
			SetStyle(ControlStyles.Selectable, true);
			SetStyle(ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public int PreferredHeight {
			get {
				// For some reason, the TextBox's PreferredHeight does not
				// change when the Font property is assigned. Without a
				// border, it will always be Font.Height anyway.
				//int text_box_preferred_height = (txtView != null) ? txtView.PreferredHeight : Font.Height;
				int text_box_preferred_height = Font.Height;

				switch (border_style) {
					case BorderStyle.FixedSingle:
					case BorderStyle.Fixed3D:
						text_box_preferred_height += 3; // magic number? :-)

						return text_box_preferred_height + 4;

					case BorderStyle.None:
					default:
						return text_box_preferred_height;
				}
			}
		}

		protected virtual void OnTextBoxResize(object source, EventArgs e)
		{
			Height = PreferredHeight;
			txtView.Top = (ClientSize.Height - txtView.Height) / 2;
		}

		#region UpDownSpinner Sub-class
		internal sealed class UpDownStepper : Control
		{
#region	Local Variables
			private const int InitialRepeatDelay = 50;
			private UpDownBase owner;
			private Timer tmrRepeat;
			private Rectangle top_button_rect;
			private Rectangle bottom_button_rect;
			private int mouse_pressed;
			private int mouse_x;
			private int mouse_y;
			private int repeat_delay;
			private int repeat_counter;
			bool top_button_entered;
			bool bottom_button_entered;
#endregion   // Local Variables

#region Constructors
			public UpDownStepper(UpDownBase owner)
			{
				this.owner = owner;

				mouse_pressed = 0;

				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.DoubleBuffer, true);
				SetStyle(ControlStyles.Opaque, true);
				SetStyle(ControlStyles.ResizeRedraw, true);
				SetStyle(ControlStyles.UserPaint, true);
				SetStyle(ControlStyles.FixedHeight, true);
				SetStyle(ControlStyles.Selectable, false);

				tmrRepeat = new Timer();

				tmrRepeat.Enabled = false;
				tmrRepeat.Interval = 10;
				tmrRepeat.Tick += new EventHandler(tmrRepeat_Tick);

				compute_rects();
			}
#endregion // Constructors

#region Private & Internal Methods
			private void compute_rects()
			{
				int top_button_height;
				int bottom_button_height;

				top_button_height = ClientSize.Height / 2;
				bottom_button_height = ClientSize.Height - top_button_height;

				top_button_rect = new Rectangle(0, 0, ClientSize.Width, top_button_height);
				bottom_button_rect = new Rectangle(0, top_button_height, ClientSize.Width, bottom_button_height);
			}

			private void redraw(Graphics graphics)
			{
				VisualStyles.PushButtonState top_button_state = VisualStyles.PushButtonState.Normal;
				VisualStyles.PushButtonState bottom_button_state = VisualStyles.PushButtonState.Normal;

				if (owner.Enabled)
				{
					if (mouse_pressed != 0)
					{
						if (mouse_pressed == 1 && top_button_rect.Contains(mouse_x, mouse_y))
							top_button_state = VisualStyles.PushButtonState.Pressed;

						if (mouse_pressed == 2 && bottom_button_rect.Contains(mouse_x, mouse_y))
							bottom_button_state = VisualStyles.PushButtonState.Pressed;
					}
					else
					{
						if (top_button_entered)
							top_button_state = VisualStyles.PushButtonState.Hot;
						if (bottom_button_entered)
							bottom_button_state = VisualStyles.PushButtonState.Hot;
					}
				}
				else
				{
					top_button_state = VisualStyles.PushButtonState.Disabled;
					bottom_button_state = VisualStyles.PushButtonState.Disabled;
				}
				ThemeEngine.Current.UpDownBaseDrawButton(graphics, top_button_rect, true, top_button_state);
				ThemeEngine.Current.UpDownBaseDrawButton(graphics, bottom_button_rect, false, bottom_button_state);
			}

			private void tmrRepeat_Tick(object sender, EventArgs e)
			{
				if (repeat_delay > 1)
				{
					repeat_counter++;

					if (repeat_counter < repeat_delay)
					{
						return;
					}

					repeat_counter = 0;
					repeat_delay = (repeat_delay * 3 / 4);
				}

				if (mouse_pressed == 0)
				{
					tmrRepeat.Enabled = false;
				}

				if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y))
				{
					owner.UpButton();
				}

				if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y))
				{
					owner.DownButton();
				}
			}
#endregion // Private & Internal Methods

#region Protected Instance Methods
			protected override void OnMouseDown(MouseEventArgs e)
			{
				if (e.Button != MouseButtons.Left)
				{
					return;
				}

				if (top_button_rect.Contains(e.X, e.Y))
				{
					mouse_pressed = 1;
					owner.UpButton();
				}
				else if (bottom_button_rect.Contains(e.X, e.Y))
				{
					mouse_pressed = 2;
					owner.DownButton();
				}

				mouse_x = e.X;
				mouse_y = e.Y;
				Capture = true;

				tmrRepeat.Enabled = true;
				repeat_counter = 0;
				repeat_delay = InitialRepeatDelay;

				Refresh();
			}

			protected override void OnMouseMove(MouseEventArgs e)
			{
				ButtonState before, after;

				before = ButtonState.Normal;
				if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y))
					before = ButtonState.Pushed;
				if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y))
					before = ButtonState.Pushed;

				mouse_x = e.X;
				mouse_y = e.Y;

				after = ButtonState.Normal;
				if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y))
					after = ButtonState.Pushed;
				if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y))
					after = ButtonState.Pushed;

				bool new_top_button_entered = top_button_rect.Contains(e.Location);
				bool new_bottom_button_entered = bottom_button_rect.Contains(e.Location);

				if (before != after)
				{
					if (after == ButtonState.Pushed)
					{
						tmrRepeat.Enabled = true;
						repeat_counter = 0;
						repeat_delay = InitialRepeatDelay;

						// fire off one right now too for good luck
						if (mouse_pressed == 1)
							owner.UpButton();
						if (mouse_pressed == 2)
							owner.DownButton();
					}
					else
						tmrRepeat.Enabled = false;

					top_button_entered = new_top_button_entered;
					bottom_button_entered = new_bottom_button_entered;

					Refresh();
				}
				else
				{
					if (ThemeEngine.Current.UpDownBaseHasHotButtonStyle)
					{
						Region area_to_invalidate = new Region();
						bool dirty = false;
						area_to_invalidate.MakeEmpty();
						if (top_button_entered != new_top_button_entered)
						{
							top_button_entered = new_top_button_entered;
							area_to_invalidate.Union(top_button_rect);
							dirty = true;
						}
						if (bottom_button_entered != new_bottom_button_entered)
						{
							bottom_button_entered = new_bottom_button_entered;
							area_to_invalidate.Union(bottom_button_rect);
							dirty = true;
						}
						if (dirty)
							Invalidate(area_to_invalidate);
						area_to_invalidate.Dispose();
					}
					else
					{
						top_button_entered = new_top_button_entered;
						bottom_button_entered = new_bottom_button_entered;
					}
				}
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				mouse_pressed = 0;
				Capture = false;

				Refresh();
			}

			protected override void OnMouseWheel(MouseEventArgs e)
			{
				HandledMouseEventArgs hme = e as HandledMouseEventArgs;
				if (hme != null)
				{
					if (hme.Handled)
						return;
					hme.Handled = true;
				}

				if (e.Delta > 0)
					owner.UpButton();
				else if (e.Delta < 0)
					owner.DownButton();
			}

			protected override void OnMouseLeave(EventArgs e)
			{
				if (top_button_entered)
				{
					top_button_entered = false;
					if (ThemeEngine.Current.UpDownBaseHasHotButtonStyle)
						Invalidate(top_button_rect);
				}
				if (bottom_button_entered)
				{
					bottom_button_entered = false;
					if (ThemeEngine.Current.UpDownBaseHasHotButtonStyle)
						Invalidate(bottom_button_rect);
				}
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				redraw(e.Graphics);
			}

			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
				compute_rects();
			}
#endregion // Protected Instance Methods
		}
#endregion // UpDownSpinner Sub-class
	}
}

#endif
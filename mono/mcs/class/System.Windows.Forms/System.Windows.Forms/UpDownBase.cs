// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jonathan Gilbert	<logic@deltaq.org>
//
// Integration into MWF:
//	Peter Bartok		<pbartok@novell.com>
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Designer("System.Windows.Forms.Design.UpDownBaseDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract partial class UpDownBase : ContainerControl {

		internal class UpDownTextBox : TextBox {

			private UpDownBase owner;

			public UpDownTextBox (UpDownBase owner)
			{
				this.owner = owner;

				SetStyle (ControlStyles.FixedWidth, false);
				SetStyle (ControlStyles.Selectable, false);
			}


			// The following can be shown to be present by
			// adding events to both the UpDown and the
			// internal textbox.  the textbox doesn't
			// generate any, and the updown generates them
			// all instead.
			protected override void OnGotFocus (EventArgs e)
			{
				ShowSelection = true;
				owner.OnGotFocus (e);
				// doesn't chain up
			}

			protected override void OnLostFocus (EventArgs e)
			{
				ShowSelection = false;
				owner.OnLostFocus (e);
				// doesn't chain up
			}

			protected override void OnMouseDown (MouseEventArgs e)
			{
				// XXX look into whether or not the
				// mouse event args are altered in
				// some way.

				owner.OnMouseDown (e);
				base.OnMouseDown (e);
			}

			protected override void OnMouseUp (MouseEventArgs e)
			{
				// XXX look into whether or not the
				// mouse event args are altered in
				// some way.

				owner.OnMouseUp (e);
				base.OnMouseUp (e);
			}

			// XXX there are likely more events that forward up to the UpDown
		}

		#region Local Variables
		internal UpDownTextBox		txtView;
		private UpDownStepper		spnSpinner;
		private bool			_InterceptArrowKeys = true;
		private LeftRightAlignment	_UpDownAlign;
		private bool			changing_text;
		private bool			user_edit;
		#endregion	// Local Variables

		#region UIA Framework Events
		static object UIAUpButtonClickEvent = new object ();

		internal event EventHandler UIAUpButtonClick {
			add { Events.AddHandler (UIAUpButtonClickEvent, value); }
			remove { Events.RemoveHandler (UIAUpButtonClickEvent, value); }
		}

		internal void OnUIAUpButtonClick (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIAUpButtonClickEvent];
			if (eh != null)
				eh (this, e);
		}

		static object UIADownButtonClickEvent = new object ();

		internal event EventHandler UIADownButtonClick {
			add { Events.AddHandler (UIADownButtonClickEvent, value); }
			remove { Events.RemoveHandler (UIADownButtonClickEvent, value); }
		}

		internal void OnUIADownButtonClick (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIADownButtonClickEvent];
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Private Methods
		private void TabIndexChangedHandler (object sender, EventArgs e)
		{
			txtView.TabIndex = TabIndex;
		}

		internal override void OnPaintInternal (PaintEventArgs e)
		{
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), ClientRectangle);
		}

		#endregion	// Private Methods

		#region Public Instance Properties
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AutoScroll {
			get {
				return base.AutoScroll;
			}

			set {
				base.AutoScroll = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		public override Color BackColor {
			get {
				return base.BackColor;
			}

			set {
				base.BackColor = value;
				txtView.BackColor = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
				txtView.BackgroundImage = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		public override ContextMenu ContextMenu {
			get {
				return base.ContextMenu;
			}
			set {
				base.ContextMenu = value;
				txtView.ContextMenu = value;
				spnSpinner.ContextMenu = value;
			}
		}

		public override ContextMenuStrip ContextMenuStrip {
			get { return base.ContextMenuStrip; }
			set {
				base.ContextMenuStrip = value;
				txtView.ContextMenuStrip = value;
				spnSpinner.ContextMenuStrip = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new DockPaddingEdges DockPadding {
			get { return base.DockPadding; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool Focused {
			get {
				return txtView.Focused;
			}
		}

		public override Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
				txtView.ForeColor = value;
			}
		}

		[DefaultValue(true)]
		public bool InterceptArrowKeys {
			get {
				return _InterceptArrowKeys;
			}
			set {
				_InterceptArrowKeys = value;
			}
		}

		public override Size MaximumSize {
			get { return base.MaximumSize; }
			set { base.MaximumSize = new Size (value.Width, 0); }
		}
		
		public override Size MinimumSize {
			get { return base.MinimumSize; }
			set { base.MinimumSize = new Size (value.Width, 0); }
		}

		[DefaultValue(false)]
		public bool ReadOnly {
			get {
				return txtView.ReadOnly;
			}
			set {
				txtView.ReadOnly = value;
			}
		}

		[Localizable(true)]
		public override string Text {
			get {
				if (txtView != null) {
					return txtView.Text;
				}
				return "";
			}
			set {
				txtView.Text = value;
				if (this.UserEdit)
					ValidateEditText();

				txtView.SelectionLength = 0;
			}
		}

		[DefaultValue(HorizontalAlignment.Left)]
		[Localizable(true)]
		public HorizontalAlignment TextAlign {
			get {
				return txtView.TextAlign;
			}
			set{
				txtView.TextAlign = value;
			}
		}

		[DefaultValue(LeftRightAlignment.Right)]
		[Localizable(true)]
		public LeftRightAlignment UpDownAlign {
			get {
				return _UpDownAlign;
			}
			set {
				if (_UpDownAlign != value) {
					_UpDownAlign = value;
					
					if (value == LeftRightAlignment.Left)
						spnSpinner.Dock = DockStyle.Left;
					else
						spnSpinner.Dock = DockStyle.Right;
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected bool ChangingText {
			get {
				return changing_text;
			}
			set {
				changing_text = value;
			}
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(120, this.PreferredHeight);
			}
		}

		protected bool UserEdit {
			get {
				return user_edit;
			}
			set {
				user_edit = value;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public abstract void DownButton ();
		public void Select(int start, int length)
		{
			txtView.Select(start, length);
		}

		public abstract void UpButton ();
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void OnChanged (object source, EventArgs e)
		{
		}

		protected override void OnFontChanged (EventArgs e)
		{
			txtView.Font = this.Font;
			Height = PreferredHeight;
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout(e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseUp (MouseEventArgs mevent)
		{
			base.OnMouseUp (mevent);
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			HandledMouseEventArgs hme = e as HandledMouseEventArgs;
			if (hme != null) {
				if (hme.Handled)
					return;
				hme.Handled = true;
			}

			if (!Enabled)
				return;
			
			if (e.Delta > 0)
				UpButton();
			else if (e.Delta < 0)
				DownButton();
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		protected virtual void OnTextBoxKeyDown (object source, KeyEventArgs e)
		{
			if (_InterceptArrowKeys) {
				if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down)) {
					e.Handled = true;

					if (e.KeyCode == Keys.Up)
						UpButton();
					if (e.KeyCode == Keys.Down)
						DownButton();
				}
			}

			OnKeyDown(e);
		}

		protected virtual void OnTextBoxKeyPress (object source, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r') {
				e.Handled = true;
				ValidateEditText();
			}
			OnKeyPress(e);
		}

		protected virtual void OnTextBoxLostFocus (object source, EventArgs e)
		{
			if (UserEdit) {
				ValidateEditText();
			}
		}

		protected virtual void OnTextBoxTextChanged (object source, EventArgs e)
		{
			if (changing_text)
				ChangingText = false;
			else
				UserEdit = true;

			OnTextChanged(e);
		}

		internal override int OverrideHeight (int height)
		{
			return Math.Min (height, PreferredHeight);
		}

		protected abstract void UpdateEditText ();

		protected virtual void ValidateEditText ()
		{
			// to be overridden by subclassers
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc (ref Message m)
		{
			switch((Msg) m.Msg) {
			case Msg.WM_KEYUP:
			case Msg.WM_KEYDOWN:
			case Msg.WM_CHAR:
				XplatUI.SendMessage (txtView.Handle, (Msg) m.Msg, m.WParam, m.LParam);
				break;
			case Msg.WM_SETFOCUS:
				ActiveControl = txtView;
				break;
			case Msg.WM_KILLFOCUS:
				ActiveControl = null;
				break;
			case Msg.WM_SELECT_ALL:
				SelectAll();
				break;
			default:
				base.WndProc (ref m);
				break;
			}
		}

		internal virtual void SelectAll()
		{
			if (txtView != null && txtView.Visible)
				txtView.SelectAllNoScroll();
		}

		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseEnter {
			add { base.MouseEnter += value; }
			remove { base.MouseEnter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseHover {
			add { base.MouseHover += value; }
			remove { base.MouseHover -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseLeave {
			add { base.MouseLeave += value; }
			remove { base.MouseLeave -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove {
			add { base.MouseMove += value; }
			remove { base.MouseMove -= value; }
		}
		#endregion	// Events
	}
}

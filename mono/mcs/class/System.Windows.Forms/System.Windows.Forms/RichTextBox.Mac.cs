#if MACOS_THEME

using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARINMAC
using AppKit;
using Foundation;
#endif

namespace System.Windows.Forms
{
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[Docking(DockingBehavior.Ask)]
	[ComVisible(true)]
	[Designer("System.Windows.Forms.Design.RichTextBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class RichTextBox : TextBoxBase
	{
		#region Public Constructors

		public RichTextBox()
		{
			richtext = true;
			accepts_return = true;
			multiline = true;

			DetectUrls = true;
			ScrollBars = RichTextBoxScrollBars.Both;
		}

		#endregion // Public Constructors

		#region Public Instance Properties
		[Browsable(false)]
		public override bool AllowDrop
		{
			get { return base.AllowDrop; }
			set { base.AllowDrop = value; }
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override bool AutoSize
		{
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[MonoTODO("Value not respected, always true")]
		[DefaultValue(false)]
		public bool AutoWordSelection
		{
			get; set;
		}

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

		[DefaultValue(0)]
		[Localizable(true)]
		public int BulletIndent
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return 0; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanRedo
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return false; }
		}

		[DefaultValue(true)]
		public bool DetectUrls
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return false; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[MonoTODO("Stub, does nothing")]
		[DefaultValue(false)]
		public bool EnableAutoDragDrop
		{
			get; set;
		}

		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[MonoTODO("Stub, does nothing")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RichTextBoxLanguageOptions LanguageOption
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return RichTextBoxLanguageOptions.AutoFont; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[DefaultValue(Int32.MaxValue)]
		public override int MaxLength
		{
			get { return base.MaxLength; }
			set { base.MaxLength = value; }
		}

		[DefaultValue(true)]
		public override bool Multiline
		{
			get { return base.Multiline; }
			set { base.Multiline = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string RedoActionName
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return String.Empty; }
		}

		[MonoTODO("Stub, does nothing")]
		[Browsable(false)]
		[DefaultValue(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool RichTextShortcutsEnabled
		{
			get; set;
		}

		[DefaultValue(0)]
		[Localizable(true)]
		[MonoTODO("Stub, does nothing")]
		[MonoInternalNote("Teach TextControl.RecalculateLine to consider the right margin as well")]
		public int RightMargin
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return 0; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[RefreshProperties(RefreshProperties.All)]
		public string Rtf
		{
			get { return Imp.Rtf; }
			set { Imp.Rtf = value; }
		}

		[DefaultValue(RichTextBoxScrollBars.Both)]
		[Localizable(true)]
		public RichTextBoxScrollBars ScrollBars
		{
			get { return scrollbars; }
			set { Imp.ApplyScrollbars(scrollbars = value); }
		}

		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedRtf
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return String.Empty; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[Browsable(false)]
		[DefaultValue(HorizontalAlignment.Left)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public HorizontalAlignment SelectionAlignment
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return HorizontalAlignment.Left; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[MonoTODO("Stub, does nothing")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionBackColor
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return BackColor; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[Browsable(false)]
		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public bool SelectionBullet
		{
			get; set;
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public int SelectionCharOffset
		{
			get; set;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionColor
		{
			get { return Imp.SelectionColor; }
			set { Imp.SelectionColor = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font SelectionFont
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return Font; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public int SelectionHangingIndent
		{
			get; set;
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public int SelectionIndent
		{
			get; set;
		}

		[Browsable(false)]
		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public bool SelectionProtected
		{
			get; set;
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public int SelectionRightIndent
		{
			get; set;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("Stub, does nothing")]
		public int[] SelectionTabs
		{
			get; set;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RichTextBoxSelectionTypes SelectionType
		{
			get { NotImplemented(MethodBase.GetCurrentMethod()); return RichTextBoxSelectionTypes.Empty; }
			set { NotImplemented(MethodBase.GetCurrentMethod(), value); }
		}

		[DefaultValue(false)]
		[MonoTODO("Stub, does nothing")]
		public bool ShowSelectionMargin
		{
			get; set;
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		public override int TextLength
		{
			get { return base.TextLength; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string UndoActionName
		{
			get { return String.Empty; }
		}

		[Localizable(true)]
		[DefaultValue(1)]
		public float ZoomFactor
		{
			get; set;
		}
		#endregion // Public Instance Properties

		#region Protected Instance Properties

		protected override Size DefaultSize
		{
			get
			{
				return new Size(100, 96);
			}
		}

		#endregion // Protected Instance Properties

		#region Public Instance Methods

		public bool CanPaste(DataFormats.Format clipFormat)
		{
			if ((clipFormat.Name == DataFormats.Rtf) ||
				(clipFormat.Name == DataFormats.Text) ||
				(clipFormat.Name == DataFormats.UnicodeText))
			{
				return true;
			}
			return false;
		}

		public int Find(char[] characterSet)
		{
			return Find(characterSet, -1, -1);
		}

		public int Find(char[] characterSet, int start)
		{
			return Find(characterSet, start, -1);
		}

		public int Find(char[] characterSet, int start, int end)
		{
			return -1;
		}

		public int Find(string str)
		{
			return Find(str, -1, -1, RichTextBoxFinds.None);
		}

		public int Find(string str, int start, int end, RichTextBoxFinds options)
		{
			return -1;
		}

		public int Find(string str, int start, RichTextBoxFinds options)
		{
			return Find(str, start, -1, options);
		}

		public int Find(string str, RichTextBoxFinds options)
		{
			return Find(str, -1, -1, options);
		}

		public void LoadFile(Stream data, RichTextBoxStreamType fileType)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void LoadFile(string path)
		{
			LoadFile(path, RichTextBoxStreamType.RichText);
		}

		public void LoadFile(string path, RichTextBoxStreamType fileType)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void Paste(DataFormats.Format clipFormat)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void Redo()
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SaveFile(Stream data, RichTextBoxStreamType fileType)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SaveFile(string path)
		{
			if (path.EndsWith(".rtf"))
			{
				SaveFile(path, RichTextBoxStreamType.RichText);
			}
			else
			{
				SaveFile(path, RichTextBoxStreamType.PlainText);
			}
		}

		public void SaveFile(string path, RichTextBoxStreamType fileType)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		#endregion // Public Instance Methods

		#region Protected Instance Methods
		protected virtual object CreateRichEditOleCallback()
		{
			throw new NotImplementedException();
		}

		protected virtual void OnContentsResized(ContentsResizedEventArgs e)
		{
			ContentsResizedEventHandler eh = (ContentsResizedEventHandler)(Events[ContentsResizedEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected virtual void OnHScroll(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[HScrollEvent]);
			if (eh != null)
				eh(this, e);
		}

		[MonoTODO("Stub, never called")]
		protected virtual void OnImeChange(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[ImeChangeEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected virtual void OnLinkClicked(LinkClickedEventArgs e)
		{
			LinkClickedEventHandler eh = (LinkClickedEventHandler)(Events[LinkClickedEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected virtual void OnProtected(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[ProtectedEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
		}

		protected virtual void OnSelectionChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[SelectionChangedEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected virtual void OnVScroll(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[VScrollEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
		}

		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			return base.ProcessCmdKey(ref m, keyData);
		}

		internal override void HandleLinkClicked(NSTextView textView, NSObject link, nuint charIndex)
		{
			var e = new LinkClickedEventArgs(link.ToString());
			OnLinkClicked(e);
		}
		#endregion // Protected Instance Methods

		#region Events
		static object ContentsResizedEvent = new object();
		static object HScrollEvent = new object();
		static object ImeChangeEvent = new object();
		static object LinkClickedEvent = new object();
		static object ProtectedEvent = new object();
		static object SelectionChangedEvent = new object();
		static object VScrollEvent = new object();

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

		public event ContentsResizedEventHandler ContentsResized
		{
			add { Events.AddHandler(ContentsResizedEvent, value); }
			remove { Events.RemoveHandler(ContentsResizedEvent, value); }
		}

		[Browsable(false)]
		public new event DragEventHandler DragDrop
		{
			add { base.DragDrop += value; }
			remove { base.DragDrop -= value; }
		}

		[Browsable(false)]
		public new event DragEventHandler DragEnter
		{
			add { base.DragEnter += value; }
			remove { base.DragEnter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler DragLeave
		{
			add { base.DragLeave += value; }
			remove { base.DragLeave -= value; }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event DragEventHandler DragOver
		{
			add { base.DragOver += value; }
			remove { base.DragOver -= value; }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback
		{
			add { base.GiveFeedback += value; }
			remove { base.GiveFeedback -= value; }
		}

		public event EventHandler HScroll
		{
			add { Events.AddHandler(HScrollEvent, value); }
			remove { Events.RemoveHandler(HScrollEvent, value); }
		}

		public event EventHandler ImeChange
		{
			add { Events.AddHandler(ImeChangeEvent, value); }
			remove { Events.RemoveHandler(ImeChangeEvent, value); }
		}

		public event LinkClickedEventHandler LinkClicked
		{
			add { Events.AddHandler(LinkClickedEvent, value); }
			remove { Events.RemoveHandler(LinkClickedEvent, value); }
		}

		public event EventHandler Protected
		{
			add { Events.AddHandler(ProtectedEvent, value); }
			remove { Events.RemoveHandler(ProtectedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event QueryContinueDragEventHandler QueryContinueDrag
		{
			add { base.QueryContinueDrag += value; }
			remove { base.QueryContinueDrag -= value; }
		}

		[MonoTODO("Event never raised")]
		public event EventHandler SelectionChanged
		{
			add { Events.AddHandler(SelectionChangedEvent, value); }
			remove { Events.RemoveHandler(SelectionChangedEvent, value); }
		}

		public event EventHandler VScroll
		{
			add { Events.AddHandler(VScrollEvent, value); }
			remove { Events.RemoveHandler(VScrollEvent, value); }
		}
		#endregion // Events
	}
}
#endif

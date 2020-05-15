using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MailClient.UI.Controls.WizardControls
{
	public class ControlExpandablePanel : UserControl
	{
		private Color expandedColor = Color.FromArgb(220, 235, 252);
		private Color collapsedColor = Color.FromArgb(247, 247, 247);

		protected int headerHeight = 38;
		protected Image image;
		protected Common.UI.ImageList.ImageList imageList;
		protected bool expanded = false;
		protected bool showArrow = true;
		protected Pen grayBorderPen;
		private System.ComponentModel.IContainer components = null;
		private Font headerFont;
		protected Brush expandedBrush, collapsedBrush;
		private Image cachedInvertedUp, cachedInvertedImage;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font HeaderFont
		{
			get { return headerFont; }
			set { headerFont = value; }
		}


		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Localizable(true)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				Invalidate();
			}
		}

		[DefaultValue(null)]
		public Image Image
		{
			get { return image; }
			set
			{
				if (image != value)
				{
					image = value;
					cachedInvertedImage = null;
				}
			}

		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Common.UI.ImageList.ImageList ImageList
		{
			get
			{
				if (imageList == null)
					imageList = new Common.UI.ImageList.ImageList();
				return imageList;
			}
		}

		[DefaultValue(false)]
		public bool Expanded
		{
			get { return expanded; }
			set
			{
				if (expanded != value)
				{
					//ExpandedChangedEventArgs ea = new ExpandedChangedEventArgs();
					//OnBeforeExpandedChanged(ea);

					//if (ea.Cancel)
					//	return;

					expanded = value;

					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		public bool ShowArrow
		{
			get { return showArrow; }
			set
			{
				if (showArrow != value)
				{
					showArrow = value;
					Invalidate();
				}
			}
		}

		public int CollapsedHeight
		{
			get { return headerHeight + 1; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color ExpandedColor
		{
			get { return expandedColor; }
			set
			{
				if (expandedColor != value)
				{
					expandedColor = value;
					if (expandedBrush != null)
						expandedBrush.Dispose();
					expandedBrush = new SolidBrush(expandedColor);
				}
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color CollapsedColor
		{
			get { return collapsedColor; }
			set
			{
				if (collapsedColor != value)
				{
					collapsedColor = value;
					if (collapsedBrush != null)
						collapsedBrush.Dispose();
					collapsedBrush = new SolidBrush(collapsedColor);
				}
			}
		}


		private bool allowImageInvert = true;

		[DefaultValue(true)]
		public bool AllowImageInvert
		{
			get { return allowImageInvert; }
			set { allowImageInvert = value; }
		}


		//public event EventHandler<ExpandedChangedEventArgs> BeforeExpandedChanged;
		//protected virtual void OnBeforeExpandedChanged(ExpandedChangedEventArgs e)
		//{
			//if (BeforeExpandedChanged != null)
			//	BeforeExpandedChanged(this, e);
		//}


		public ControlExpandablePanel()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.Selectable, true);

			InitializeComponent();

			//if (!UIUtils.DesignMode)
			{
				this.BackColor = Color.White;
				headerFont = MailClient.Common.UI.FontCache.CreateFont(SystemFonts.MessageBoxFont.FontFamily, 10f);
				Font = MailClient.Common.UI.FontCache.CreateFont(SystemFonts.MessageBoxFont.FontFamily, SystemFonts.MessageBoxFont.SizeInPoints);

				grayBorderPen = new Pen(MailClient.Common.UI.Themes.ThemeManager.Instance.ActiveTheme.LightLine);
			}
			//else
			//	grayBorderPen = Pens.Gray;	//design-time

			expandedBrush = new SolidBrush(expandedColor);
			collapsedBrush = new SolidBrush(collapsedColor);
			headerHeight = MailClient.Common.UI.DisplaySettingsManager.Instance.Scale(38);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Size scaled16 = Common.UI.DisplaySettingsManager.Instance.ScaleSize(new Size(16, 16));
			int scaled10 = Common.UI.DisplaySettingsManager.Instance.Scale(10);
			int scaled33 = Common.UI.DisplaySettingsManager.Instance.Scale(33);

			if (expanded)
			{
				e.Graphics.FillRectangle(expandedBrush, 1, 0, this.Width - 1, headerHeight + 1);

				if (showArrow)
				{
					Image img = Common.UI.ImageResources.Less.GetSuitableImageForSize(scaled16);
					if (Common.UI.CommonPaintUtils.IsDarkColor(expandedColor))
					{
						if (cachedInvertedUp == null)
							cachedInvertedUp = Common.UI.CommonPaintUtils.MakeInvertedImage(img);
						img = cachedInvertedUp;
					}

					e.Graphics.DrawImage(img, new Rectangle(this.Width - scaled16.Width - scaled10, (headerHeight - scaled16.Height) / 2, scaled16.Width, scaled16.Height));
				}

				e.Graphics.DrawLines(grayBorderPen, new Point[] { new Point(0, headerHeight + 1), new Point(0, Height - 1), new Point(Width - 1, Height - 1), new Point(Width - 1, headerHeight + 1) });
			}
			else
			{
				e.Graphics.FillRectangle(collapsedBrush, 1, 0, this.Width - 1, headerHeight + 1);

				if (showArrow)
				{
					Image img = Common.UI.CommonPaintUtils.IsDarkColor(expandedColor) ?
						Common.UI.ImageResources.MoreWhite.GetSuitableImageForSize(scaled16) :
						Common.UI.ImageResources.More.GetSuitableImageForSize(scaled16);
					
					e.Graphics.DrawImage(img, new Rectangle(this.Width - scaled16.Width - scaled10, (headerHeight - scaled16.Height) / 2, scaled16.Width, scaled16.Height));
				}
			}

			Pen penToUse = grayBorderPen;
			e.Graphics.DrawLine(penToUse, new Point(0, 0), new Point(0, headerHeight));
			e.Graphics.DrawLine(penToUse, new Point(Width - 1, headerHeight), new Point(Width - 1, 0));

			if (image != null)
			{
				Image img = image;
				if (Common.UI.CommonPaintUtils.IsDarkColor(expandedColor) && allowImageInvert)
				{
					if (cachedInvertedImage == null)
						cachedInvertedImage = Common.UI.CommonPaintUtils.MakeInvertedImage(img);
					img = cachedInvertedImage;
				}
				e.Graphics.DrawImage(img, scaled10, (headerHeight - img.Height) / 2);
			}
			else if (imageList != null)
			{
				Image img = imageList.GetSuitableImageForSize(scaled16);
				if (img != null)
				{
					if (Common.UI.CommonPaintUtils.IsDarkColor(expandedColor) && allowImageInvert)
					{
						if (cachedInvertedImage == null)
							cachedInvertedImage = Common.UI.CommonPaintUtils.MakeInvertedImage(img);
						img = cachedInvertedImage;
					}
					e.Graphics.DrawImage(img, new Rectangle(scaled10, (headerHeight - scaled16.Height) / 2, scaled16.Width, scaled16.Height));
				}
			}

			MailClient.Common.UI.TextRendererEx.DrawText(e.Graphics, Text, headerFont, new Rectangle(scaled33, 0, Width - scaled33, headerHeight - 2), ForeColor, TextFormatFlags.Default | TextFormatFlags.VerticalCenter);

			base.OnPaint(e);

		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			if (!expanded && e.Location.Y <= headerHeight)
			{
				Expanded = !expanded;
			}

			base.OnMouseClick(e);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ExpandablePanel
			// 
			this.Name = "ExpandablePanel";
			this.ResumeLayout(false);

		}
	}
}

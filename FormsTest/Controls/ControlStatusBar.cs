using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MailClient.UI.Controls
{
	public static class ImageExtensions
	{
		public static Image GetSuitableImageForSize(this ImageList images, Graphics graphics, Size size)
		{
			return images.Images[0];
		}
	}

	public static class ScaleUtils
	{
		public static SizeF GetScaleFactor(Control c) { return new SizeF(1, 1); }
		public static int Scale(SizeF scaleFactor, int v) { return v; }
		public static int Scale(Control control, int v) { return v; }
	}

	public static class CommonPaintUtils
	{
		public static bool IsDarkColor(Color c) { return false; }
		public static Bitmap MakeTintedImage(Image image, Color foreColor) { return image is Bitmap b ? b : new Bitmap(image); }
	}

	public partial class ControlStatusBar : UserControl
	{
		public bool InvalidationEnabled { get; set; } = true;

		public Color StatusBarForeground = Color.Black;
		public Color StatusBarBackground = Color.White;
		public Color StatusBarBackgroundOffline = Color.DarkGray;
		public Color StatusBarForegroundOffline = Color.Black;

		public Color StatusBarInfiniteProgressBackground = Color.DarkGray;
		public Color StatusBarInfiniteProgress = Color.LightBlue;

		public Color LightLine = Color.LightBlue;
		public Color StatusBarBackgroundOver = Color.LightSalmon;

		public Color StatusBarProgress = Color.Aqua;

		public ImageList UpImage;
		public ImageList DownImage;
		public ImageList InfoImage;

		public string OfflineText = "Offline";
		public string NeverSyncedText = "Never synced";

		private const int COLLAPSED_HEIGHT = 18;
		private const int EXPANDED_HEIGHT = 43;

		private int progressLineThickness = 4;
		private int infiniteProgressPart1 = 10;
		private int infiniteProgressPart2 = 20;
		private bool mouseOver;
		private bool collapsed;
		private Rectangle arrowImgRect;
		private Rectangle arrowMouseRect;
		private Rectangle infoImgRect;
		private Rectangle infoMouseRect;
		private Rectangle textRect;
		private string actionName;
		private int progressPercentage;
		private DateTime? lastCompletedOperationUtcTime;
		private bool infiniteOperation;
		private int infiniteOperationProgress;
		private bool infiniteOperationStopPending;
		private bool inProgress;
		private int infiniteStopTicksCount;
		private bool mouseOverArrow;
		private Brush infiniteBrush;
		private bool offline;
		private Bitmap invertedImage;

		int infiniteBrushWidth = 1;
		int infiniteBarSpeed = 60; // pixels per second
		DateTime infiniteBarStartTime = DateTime.MinValue;

		[DefaultValue(false)]
		public bool Collapsed
		{
			get { return collapsed; }
			set
			{
				if (collapsed != value)
				{
					collapsed = value;
					setSize();

					setColors();

					Invalidate();

					if (CollapsedChanged != null)
						CollapsedChanged(this, EventArgs.Empty);
				}
			}
		}


		public int ProgressPercentage
		{
			get { return progressPercentage; }
		}

		public bool InfiniteOperationActive
		{
			get { return infiniteOperation; }
		}

		public event EventHandler CollapsedChanged;


		public ControlStatusBar()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			InitializeComponent();
			setSize();
			setColors();
			createInfiniteBrush();

			SystemEvents.TimeChanged += SystemEvents_TimeChanged;
		}

		private void SystemEvents_TimeChanged(object sender, EventArgs e)
		{
			this.Invalidate();
		}

		private void setColors()
		{
			this.ForeColor = offline && !collapsed ? StatusBarForegroundOffline : StatusBarForeground;
			this.BackColor = offline && !collapsed ? StatusBarBackgroundOffline : StatusBarBackground;
		}

		void ThemeManager_ThemeChanged(object sender, EventArgs e)
		{
			this.ForeColor = StatusBarForeground;
			this.BackColor = StatusBarBackground;

			if (invertedImage != null)
			{
				invertedImage.Dispose();
				invertedImage = null;
			}

			createInfiniteBrush();
		}

		private void createInfiniteBrush()
		{
			if (infiniteBrush != null)
			{
				((TextureBrush)infiniteBrush).Image.Dispose();
				infiniteBrush.Dispose();
			}

			Bitmap bmp = new Bitmap(infiniteProgressPart1 + infiniteProgressPart2, progressLineThickness);
			using (Graphics gr = Graphics.FromImage(bmp))
			{
				using (Brush br = new SolidBrush(StatusBarInfiniteProgressBackground))
					gr.FillRectangle(br, new Rectangle(0, 0, bmp.Width, bmp.Height));

				gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

				using (Pen p = new Pen(StatusBarInfiniteProgress, infiniteProgressPart1))
				{
					int half = bmp.Width / 2;
					gr.DrawLine(p, half - infiniteProgressPart1 / 2 - progressLineThickness / 2, progressLineThickness + infiniteProgressPart1 / 2, half + infiniteProgressPart1 / 2 + progressLineThickness, -(infiniteProgressPart1 / 2) - progressLineThickness);
				}
			}
			infiniteBrush = new TextureBrush(bmp);
			infiniteBrushWidth = bmp.Width;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (invertedImage != null)
				{
					invertedImage.Dispose();
					invertedImage = null;
				}
				if(components != null)
				{
					components.Dispose();
					components = null;
				}

				SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);


			// draw upper line
			using (Pen p = new Pen(LightLine))
				e.Graphics.DrawLine(p, 0, 0, Width, 0);


			if (collapsed)
			{
				if (mouseOver)
				{
					using (Brush br = new SolidBrush(StatusBarBackgroundOver))
						e.Graphics.FillRectangle(br, ClientRectangle);
				}
				if (UpImage != null && UpImage.Images.Count > 0)
					e.Graphics.DrawImage(UpImage.GetSuitableImageForSize(e.Graphics, arrowImgRect.Size), arrowImgRect);
			}
			else if (e.ClipRectangle.IntersectsWith(new Rectangle(0, progressLineThickness, Width, Height - progressLineThickness)))
			{
				if (!infoImgRect.IntersectsWith(arrowImgRect))
				{
					if (mouseOver)
					{
						if (mouseOverArrow)
						{
							using (Brush br = new SolidBrush(StatusBarBackgroundOver))
								e.Graphics.FillRectangle(br, arrowMouseRect);
						}
						if (DownImage != null && DownImage.Images.Count > 0)
							e.Graphics.DrawImage(DownImage.GetSuitableImageForSize(e.Graphics, arrowImgRect.Size), arrowImgRect);
					}
				}

				if (InfoImage != null && InfoImage.Images.Count > 0)
				{
					if (!CommonPaintUtils.IsDarkColor(ForeColor))
					{
						if (invertedImage == null)
							invertedImage = CommonPaintUtils.MakeTintedImage((InfoImage.GetSuitableImageForSize(e.Graphics, infoImgRect.Size)), ForeColor);
						e.Graphics.DrawImage(invertedImage, infoImgRect);
					}
					else
						e.Graphics.DrawImage(InfoImage.GetSuitableImageForSize(e.Graphics, infoImgRect.Size), infoImgRect);
				}

				string text = actionName;
				if (offline)
					text = OfflineText;
				else if (!infiniteOperation && !inProgress)
				{
					if (lastCompletedOperationUtcTime.HasValue)
						text = GetLastSyncedText();
					else
						text = NeverSyncedText;
				}
				TextRenderer.DrawText(e.Graphics, text, this.Font, textRect, this.ForeColor, TextFormatFlags.Default | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
			}

			if (infiniteOperation)
			{
				double dt = (DateTime.Now - infiniteBarStartTime).TotalSeconds;
				int position = ((int)(dt * infiniteBarSpeed)) % infiniteBrushWidth;

				e.Graphics.TranslateTransform(position, 0);
				e.Graphics.FillRectangle(infiniteBrush, new Rectangle(-position, 0, this.Width, progressLineThickness));
				e.Graphics.ResetTransform();
			}
			else if (progressPercentage > 0 && progressPercentage < 100)
			{
				using (Brush br = new SolidBrush(StatusBarProgress))
					e.Graphics.FillRectangle(br, new Rectangle(0, 0, (progressPercentage * Width) / 100, progressLineThickness));
			}
		}

		private string GetLastSyncedText()
		{
			//return string.Format(Resources.UI.Controls_base.LastSyncAt, DateTimeUtils.ToLongTimeString(lastCompletedOperationUtcTime.Value.ToLocalTime()));
			return "Last Synced:";
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			setSize();

			SizeF scaleFactor = ScaleUtils.GetScaleFactor(this);

			if (collapsed)
			{
				int scaledImgSize = ScaleUtils.Scale(scaleFactor, 16);
				arrowImgRect = new Rectangle((this.Width - scaledImgSize) / 2, (this.Height - scaledImgSize) / 2 + 1, scaledImgSize, scaledImgSize);
			}
			else
			{
				int scaledImgSize = ScaleUtils.Scale(scaleFactor, 20);
				arrowImgRect = new Rectangle(this.Width - scaledImgSize - 8, (int)Math.Round((this.Height - scaledImgSize) / 2d), scaledImgSize, scaledImgSize);

				infoImgRect = new Rectangle(ScaleUtils.Scale(scaleFactor, 9), (int)Math.Round((this.Height - scaledImgSize) / 2d), scaledImgSize, scaledImgSize);

				arrowMouseRect = arrowImgRect;
				arrowMouseRect.Inflate(4, 4);
				arrowMouseRect.Width++; // to take into account the size of the arrow in the image


				infoMouseRect = infoImgRect;
				infoMouseRect.Inflate(4, 4);
				//infoMouseRect.Width++; // to take into account the size of the arrow in the image

				textRect = new Rectangle(infoImgRect.Right + 4, 0, arrowMouseRect.Left - infoImgRect.Right, this.Height);
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			mouseOver = true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			mouseOver = false;
			Invalidate();
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			if (Collapsed)
				Collapsed = !Collapsed;
			else
			{
				if (arrowMouseRect.Contains(e.Location) && !infoImgRect.IntersectsWith(arrowImgRect))
					Collapsed = !Collapsed;
				else
					ShowOperations();
			}
		}

		private void ShowOperations()
		{
			//GUIStatus.Instance.ShowOperations()
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!collapsed)
			{
				bool oldOver = mouseOverArrow;
				mouseOverArrow = arrowMouseRect.Contains(e.Location);

				if (oldOver != mouseOverArrow)
					Invalidate();
			}
		}

		private void setSize()
		{
			this.Height = ScaleUtils.Scale(this, Collapsed ? COLLAPSED_HEIGHT : EXPANDED_HEIGHT);
		}


		public void ReportException(Exception e)
		{
			//timerClear.Enabled = false;
			//actionLabel.Text = e.Message;
			//actionProgress.Visible = false;
			//actionProgress.Value = 0;
		}

		public void SetOnlineState(bool online)
		{
			offline = !online;
			setColors();

			Invalidate();
		}

		public void SetActionProgress(string actionName, int progressPercentage)
		{
			if (offline)
				return;

			StopInfiniteProgress();
			this.actionName = actionName.Replace("\r\n", "\n").Replace("\n", "  ");
			this.progressPercentage = progressPercentage;

			if (progressPercentage >= 100)
			{
				lastCompletedOperationUtcTime = DateTime.UtcNow;
				inProgress = false;
			}
			else
				inProgress = true;


			Invalidate();
		}

		public void SetInfiniteProgress(string actionName)
		{
			if (offline)
				return;

			bool invalidate = (this.actionName != actionName);
			progressPercentage = 0;

			this.actionName = actionName;
			infiniteOperationStopPending = false;

			if (!infiniteOperation)
			{
				infiniteOperation = true;
				timerInfinite.Start();
				invalidate = true;

				infiniteBarStartTime = DateTime.Now;
			}

			if (invalidate)
				Invalidate();
		}

		public void StopInfiniteProgress()
		{
			if (infiniteOperation)
			{
				infiniteOperationStopPending = true;

				// allow 3 timer ticks before stopping
				infiniteStopTicksCount = 3;
			}
		}

		private void timerInfinite_Tick(object sender, EventArgs e)
		{
			if (infiniteOperationStopPending)
			{
				if ((--infiniteStopTicksCount) <= 0)
				{
					infiniteOperationStopPending = false;
					infiniteOperationProgress = 0;
					infiniteOperation = false;
					lastCompletedOperationUtcTime = DateTime.UtcNow;

					timerInfinite.Stop();

					Invalidate();
				}
			}
			else
			{
				infiniteOperationProgress = (infiniteOperationProgress + 1) % 100;
				if (InvalidationEnabled)
					Invalidate(new Rectangle(0, 0, Width, progressLineThickness));
			}
		}

	}
}

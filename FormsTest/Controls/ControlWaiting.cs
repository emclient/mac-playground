using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace MailClient.UI.Controls
{
	public partial class ControlWaiting : UserControl
	{
		public bool InvalidationEnabled { get; set; } = true;

		static class UIUtils
		{
			public static bool DesignMode { get { return false; } }
		}

		public enum ImageAlignmentType
		{
			Center,
			Left
		}

		public enum WaitingStyle
		{
			Circle,
			Line,
			CircleGradient
		}

		private ImageAlignmentType imageAlignment = ImageAlignmentType.Center;
		public ImageAlignmentType ImageAlignment
		{
			get
			{
				return imageAlignment;
			}
			set
			{
				imageAlignment = value;
			}
		}

		public WaitingStyle waitingStyle = WaitingStyle.CircleGradient;

		[DefaultValue(WaitingStyle.CircleGradient)]
		public WaitingStyle Style
		{
			get
			{
				return waitingStyle;
			}
			set
			{
				waitingStyle = value; setTimerInterval();
			}
		}

		int value;

		[DefaultValue(typeof(Color), "White")]
		public override Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[DefaultValue(typeof(Color), "ControlDarkDark")]
		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		public ControlWaiting()
		{
			this.BackColor = Color.White;
			InitializeComponent();
			TabStop = false;

			if (!UIUtils.DesignMode)
			{
				setTimerInterval();
			}
		}

		public static void DrawWaitingCircle(Graphics g, Rectangle rect, ImageAlignmentType imageAlignment, Color foreColor, Pen borderPen, int value)
		{
			int baseSize = Math.Min(Math.Min(rect.Width, rect.Height), 28);
			float size = baseSize / 5.0F;
			float halfSize = size / 2.0F;
			float radius = (baseSize / 2.0F) - halfSize;
			float centerX;
			float centerY = rect.Top + (float)rect.Height / 2;

			value %= 8;

			if (imageAlignment == ImageAlignmentType.Center)
				centerX = rect.Left + (float)rect.Width / 2;
			else
				centerX = rect.Top + radius + halfSize;
		
			SmoothingMode oldSmoothingMode = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			for (int i = 7; i >= 0; i--)
			{
				int iAdjusted = ((16 - value - i) % 8);
				int alpha = (int)(255.0F * ((i + 1) / 8.0F));
				Color drawColor = Color.FromArgb(alpha, foreColor);
				using (SolidBrush brush = new SolidBrush(drawColor))
				{
					g.FillEllipse(
						brush,
						centerX - (float)Math.Sin(iAdjusted * Math.PI / 4) * radius - halfSize,
						centerY - (float)Math.Cos(iAdjusted * Math.PI / 4) * radius - halfSize,
						size,
						size);
				}

				if (borderPen != null)
				{
					g.DrawEllipse(
						borderPen,
						centerX - (float)Math.Sin(iAdjusted * Math.PI / 4) * radius - halfSize,
						centerY - (float)Math.Cos(iAdjusted * Math.PI / 4) * radius - halfSize,
						size,
						size);
				}

				halfSize *= 0.85f;
				size *= 0.85f;
			}

			g.SmoothingMode = oldSmoothingMode;
		}

		public static void DrawWaitingCircleWithGradient(Graphics g, Rectangle rect, ImageAlignmentType imageAlignment, Color foreColor, Pen borderPen, int value)
		{
			int baseSize = Math.Min(Math.Min(rect.Width, rect.Height), 28);
			float innerRadius = baseSize * 2 / 3 + (Math.Max(0, baseSize - 20) * 0.5f);
			float size = (baseSize - innerRadius) / 2;

			if (Math.Min(rect.Width, rect.Height) <= 28 && innerRadius > 20)
			{
				innerRadius -= 2;
				size--;
			}

			float halfRadius = innerRadius / 2;
			int numOfLines = 22 + (int)(Math.Max(0, baseSize - 14) * 1.4f);	// more lines for bigger circle
			int lineThickness = 2 + Math.Max(0, baseSize - 14) / 6;	// and thicker lines for bigger circles

			value %= numOfLines;

			float centerX;
			float centerY = rect.Top + (float)rect.Height / 2;

			if (imageAlignment == ImageAlignmentType.Center)
				centerX = rect.Left + (float)rect.Width / 2;
			else
				centerX = rect.Left + baseSize / 2;

			SmoothingMode oldSmoothingMode = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			for (int i = numOfLines - 1; i >= 0; i--)
			{
				int adjusted = (i - value + numOfLines) % numOfLines;
				int alpha = (int)(255.0F * ((float)(adjusted + 1) / numOfLines));

				Color drawColor = Color.FromArgb(alpha, foreColor);

				double angle = (Math.PI * 2 * i) / numOfLines;

				// because of StartCap and EndCap, we have to subtract 1px
				PointF startPoint = new PointF(
					centerX + (float)((halfRadius + 1) * Math.Cos(angle)),
					centerY + (float)((halfRadius + 1) * Math.Sin(angle)));

				PointF endPoint = new PointF(
					centerX + (float)((halfRadius + size - 1) * Math.Cos(angle)),
					centerY + (float)((halfRadius + size - 1) * Math.Sin(angle)));

				using (Pen pen = new Pen(drawColor, lineThickness))
				{
					pen.StartCap = LineCap.Round;
					pen.EndCap = LineCap.Round;

					g.DrawLine(pen, startPoint, endPoint);
				}
			}

			g.SmoothingMode = oldSmoothingMode;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (!Enabled)
				return;

			switch (this.waitingStyle)
			{
				case WaitingStyle.Circle:
					DrawWaitingCircle(e.Graphics, this.ClientRectangle, imageAlignment, this.ForeColor, Pens.Gray, this.value);
					break;
				case WaitingStyle.CircleGradient:
					DrawWaitingCircleWithGradient(e.Graphics, this.ClientRectangle, imageAlignment, this.ForeColor, Pens.Gray, this.value);
					break;
				case WaitingStyle.Line:
					using (Brush brush = new SolidBrush(this.ForeColor))
					{
						for (int i = 0; i < 5; i++)
						{
							double tt = ((this.value + (i * 3)) % 130) / 80f;
							if (tt <= 1.0)
							{
								double xx;
								if (tt < 0.5)
									xx = Math.Pow(2 * tt, 0.5) / 2;
								else
									xx = 1 - (Math.Pow(2 * (1.0 - tt), 0.5) / 2);
								int x = (int)(ClientSize.Width * xx);
								e.Graphics.FillRectangle(brush, new Rectangle(x - 1, (ClientSize.Height / 2) - 1, 2, 2));
							}
						}
					}
					break;
			}

		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			timer.Enabled = this.Enabled && this.Visible;
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			timer.Enabled = this.Enabled && this.Visible;
			if (!Enabled)
				value = 0;
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (!UIUtils.DesignMode)
			{
				this.value++;

				if (InvalidationEnabled)
					Invalidate();
			}
		}

		private void setTimerInterval()
		{

			switch (this.waitingStyle)
			{
				case WaitingStyle.Circle:
					timer.Interval = 100;
					break;
				case WaitingStyle.CircleGradient:
					timer.Interval = 30;
					break;
				case WaitingStyle.Line:
					timer.Interval = 30;
					break;
			}
		}

	}
}

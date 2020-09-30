using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormsTest.Controls
{
	public class TextureBrushControl : Control
	{
		public TextureBrushControl()
		{
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			_ = Image;
		}

		Image image;
		public Image Image
		{
			get
			{
				if (image == null)
					image = CreateImage();
				return image;
			}
			set
			{
				image = value;
			}
		}

		public System.Drawing.Drawing2D.WrapMode WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;

		protected virtual Image CreateImage(Size? size = null)
		{
			var dim = size ?? new Size(48, 32);
			var bitmap = new Bitmap(dim.Width, dim.Height);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.FillRectangle(Brushes.White, 0, 0, dim.Width, dim.Height);
				g.FillEllipse(Brushes.DarkBlue, dim.Width / 2, dim.Height / 2, dim.Width / 2, dim.Height / 2);
				g.DrawString($"{WrapMode}", Font, Brushes.Black, ClientRectangle);
			}
			return bitmap;
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			e.Graphics.FillRectangle(Brushes.Gray, e.ClipRectangle);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			using (var solidbr = new SolidBrush(Color.Yellow))
			using (var brush = new TextureBrush(Image))
			{
				//brush.RotateTransform(10);

				brush.WrapMode = this.WrapMode;
				e.Graphics.FillRectangle(solidbr, ClientRectangle);
				e.Graphics.FillRectangle(brush, ClientRectangle);
			}
		}
	}
}

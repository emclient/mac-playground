
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

#if MAC
using AppKit;
using CoreGraphics;
#endif

namespace FormsTest.Controls
{
	public class TextureBrushControl2 : Control
	{
		public TextureBrushControl2()
		{
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			image = CreateImage(size, $"{mode}");
		}

		int gap = 8;
		Size size = new Size(48, 32);
		//System.Drawing.Drawing2D.WrapMode mode = System.Drawing.Drawing2D.WrapMode.Clamp;
		System.Drawing.Drawing2D.WrapMode mode = System.Drawing.Drawing2D.WrapMode.Tile;
		Image image;

		protected virtual Image CreateImage(Size size, string text)
		{
			var image = CreateImageSingleRes(size, text);

			var addResolution = typeof(Image).GetMethod("AddResolution", BindingFlags.Instance | BindingFlags.NonPublic);
			addResolution?.Invoke(image, new object[] { CreateImageSingleRes(new Size(2 * size.Width, 2 * size.Height), text) });
			return image;
		}

		protected virtual Image CreateImageSingleRes(Size size, string text)
		{
			Size dim = new Size(size.Width, size.Height);
			var bitmap = new Bitmap(dim.Width, dim.Height);
			var font = new Font(this.Font.FontFamily.Name, size.Height / 3, GraphicsUnit.Pixel);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.FillRectangle(Brushes.White, 0, 0, dim.Width, dim.Height);
				g.FillEllipse(Brushes.DarkBlue, dim.Width / 2, dim.Height / 2, dim.Width / 2, dim.Height / 2);
				g.DrawString($"{text}", font, Brushes.Black, new Rectangle(0, 0, dim.Width, dim.Height));
			}
			return bitmap;
		}

#if MAC
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var ctx = NSGraphicsContext.CurrentContext.CGContext;
			var ctm = ctx.GetCTM();
			var tile = new Size(512, 512);
			var offset = new Point(0,0);
			//offset.X = tile.Width - (int) ctx.GetCTM().x0 % tile.Width;
			//offset.Y = tile.Height - (int) ctx.GetCTM().y0 % tile.Height;

			var size = new Size(2 * this.size.Width, 2 *this.size.Height);

			int k = 12;
			for (int i = 0; i < k; ++i)
			{
				for (int j = 0; j < k; ++j)
				{
					var x = i * (size.Width + gap);
					var y = j * (size.Height + gap);
					var rect = new Rectangle(x, y, size.Width, size.Height);

					using (var solidbr = new SolidBrush(Color.Beige))
					using (var brush = new TextureBrush(image))
					{
						//brush.RotateTransform(10);
						brush.WrapMode = mode;
						brush.TranslateTransform(x + offset.X, y + offset.Y);
						e.Graphics.FillRectangle(solidbr, rect);
						e.Graphics.FillRectangle(brush, rect);
					}
				}
			}
		}
#endif
		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			Invalidate();
		}
	}
}

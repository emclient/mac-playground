using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Extensions.Drawing
{
	internal static class DrawingExtensions
	{
		public static GraphicsPath AddRoundRect(this GraphicsPath path, RectangleF bounds, float radius)
		{
			return AddRoundRect(path, bounds, radius, radius, radius, radius);
		}

		public static GraphicsPath AddRoundRect(this GraphicsPath path, RectangleF bounds, float topLeftRadius, float topRightRadius, float botRightRadius, float botLeftRadius)
		{
			var d = 2 * topLeftRadius;
			var corner = new RectangleF(bounds.X, bounds.Y, d, d);
			path.AddArc(corner, 180, 90);

			d = 2 * topRightRadius;
			corner = new RectangleF(bounds.Right - d, bounds.Y, d, d);
			path.AddArc(corner, 270, 90);

			d = 2 * botRightRadius;
			corner = new RectangleF(bounds.Right - d, bounds.Bottom - d, d, d);
			path.AddArc(corner, 0, 90);

			d = 2 * botLeftRadius;
			corner = new RectangleF(bounds.X, bounds.Bottom - d, d, d);
			path.AddArc(corner, 90, 90);

			path.CloseFigure();
			return path;
		}

		public static Graphics DrawRoundRect(this Graphics g, Pen pen, Rectangle bounds, int radius)
		{
			return DrawRoundRect(g, pen, bounds.ToRectangleF(), radius, radius, radius, radius);
		}

		public static Graphics DrawRoundRect(this Graphics g, Pen pen, RectangleF bounds, float radius)
		{
			return DrawRoundRect(g, pen, bounds, radius, radius, radius, radius);
		}

		public static Graphics DrawRoundRect(this Graphics graphics, Pen pen, RectangleF bounds, float topLeftRadius, float topRightRadius, float botRightRadius, float botLeftRadius)
		{
			if (graphics == null)
				throw new ArgumentNullException(nameof(graphics));

			if (pen != null)
				using (var path = new GraphicsPath().AddRoundRect(bounds, topLeftRadius, topRightRadius, botRightRadius, botLeftRadius))
					graphics.DrawPath(pen, path);

			return graphics;
		}

		public static Graphics FillRoundRect(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
		{
			return graphics.FillRoundRect(brush, bounds.ToRectangleF(), radius, radius, radius, radius);
		}

		public static Graphics FillRoundRect(this Graphics graphics, Brush brush, Rectangle bounds, int topLeftRadius, int topRightRadius, int botRightRadius, int botLeftRadius)
		{
			return graphics.FillRoundRect(brush, bounds.ToRectangleF(), topLeftRadius, topRightRadius, botRightRadius, botLeftRadius);
		}

		public static Graphics FillRoundRect(this Graphics graphics, Brush brush, RectangleF bounds, float topLeftRadius, float topRightRadius, float botRightRadius, float botLeftRadius)
		{
			if (graphics == null)
				throw new ArgumentNullException(nameof(graphics));

			if (brush != null)
				using (var path = new GraphicsPath().AddRoundRect(bounds, topLeftRadius, topRightRadius, botRightRadius, botLeftRadius))
					graphics.FillPath(brush, path);

			return graphics;
		}

		public static Graphics FillEquilateralTriangle(this Graphics graphics, Brush brush, Rectangle bounds, int angle)
		{
			return FillEquilateralTriangle(graphics, brush, bounds.ToRectangleF(), angle);
		}

		public static Graphics FillEquilateralTriangle(this Graphics graphics, Brush brush, RectangleF bounds, float angle)
		{
			var mid = bounds.Mid();
			var a = (float)Math.Min(bounds.Width, bounds.Height);
			var h = (float)Math.Sqrt(a * a * 3 / 4);
			var r = new RectangleF(mid.X - a / 2, mid.Y - h / 2, a, h);
			return graphics.FillTriangle(brush, r, angle);
		}

		public static Graphics FillTriangle(this Graphics graphics, Brush brush, RectangleF bounds, float angle)
		{
			var mid = bounds.Mid();

			var state = graphics.Save();
			graphics.TranslateTransform(mid.X, mid.Y);
			graphics.RotateTransform(angle);
			graphics.TranslateTransform(-mid.X, -mid.Y);

			using (var path = new GraphicsPath())
			{
				path.AddLine(bounds.Left, bounds.Bottom, mid.X, bounds.Top);
				path.AddLine(mid.X, bounds.Top, bounds.Right, bounds.Bottom);
				path.CloseFigure();

				graphics.FillPath(brush, path);
			}

			graphics.Restore(state);
			return graphics;
		}

		public static Graphics DrawArrow(this Graphics graphics, Pen pen, Rectangle bounds, int angle)
		{
			return graphics.DrawArrow(pen, bounds.ToRectangleF(), angle);
		}

		public static Graphics DrawArrow(this Graphics graphics, Pen pen, RectangleF bounds, float angle)
		{
			var mid = bounds.Mid();

			var state = graphics.Save();
			graphics.TranslateTransform(mid.X, mid.Y);
			graphics.RotateTransform(angle);
			graphics.TranslateTransform(-mid.X, -mid.Y);

			var a = bounds.Width;
			var h = bounds.Height;
			var r = new RectangleF(mid.X - a / 2, mid.Y - h / 2, a, h);

			graphics.DrawLine(pen, r.Left, r.Bottom, mid.X, r.Top);
			graphics.DrawLine(pen, mid.X, r.Top, r.Right, r.Bottom);

			graphics.Restore(state);
			return graphics;
		}

		internal static RectangleF ToRectangleF(this Rectangle r)
		{
			return new RectangleF(r.X, r.Y, r.Width, r.Height);
		}

		internal static PointF Mid(this RectangleF r)
		{
			return new PointF((r.Left + r.Right) / 2, (r.Top + r.Bottom) / 2);
		}
	}
}

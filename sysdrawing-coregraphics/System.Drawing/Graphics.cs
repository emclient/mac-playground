//
// Graphics.cs: The graphics context API from System.Drawing, MonoTouch implementation
//
// Authors:
//   Sebastien Pouliot  <sebastien@xamarin.com>
//   Alexandre Pigolkine (pigolkine@gmx.de)
//   Duncan Mak (duncan@xamarin.com)
//   Miguel de Icaza (miguel@xamarin.com)
//   Kenneth J. Pouncey (kjpou@pt.lu)
//
// Copyright 2011-2013 Xamarin Inc
// Copyright 2003-2009 Novell, Inc.
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Mac;

#if XAMARINMAC
using CoreGraphics;
using AppKit;
using NSRuntime = ObjCRuntime.Runtime;
#elif MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using NSRuntime = MonoMac.ObjCRuntime.Runtime;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreText;
#endif

using MatrixOrder = System.Drawing.Drawing2D.MatrixOrder;

namespace System.Drawing {

	public sealed partial class Graphics : MarshalByRefObject, IDisposable, IDeviceContext {
		internal CGContext context;
		private NSView focusedView;
		private bool hasClientTransform;
		internal Pen LastPen;
		internal Brush LastBrush;
		internal CGRect boundingBox;
		internal bool isFlipped;
		internal InterpolationMode interpolationMode;
		internal SmoothingMode smoothingMode;

		// Text Layout
		internal Color lastBrushColor;

		// User Space variables
		internal Matrix modelMatrix;
		internal Matrix viewMatrix;
		internal Matrix modelViewMatrix;
		float userspaceScaleX = 1, userspaceScaleY = 1;
		private GraphicsUnit graphicsUnit = GraphicsUnit.Display;
		private float pageScale = 1;
		private PointF renderingOrigin = PointF.Empty;
		private static Region infinitRegion = new Region();
		private Region clip;
		internal nfloat screenScale;

		internal Graphics (CGContext context, bool flipped = true)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			
			this.isFlipped = flipped;
			this.context = context;

			context.SaveState();

			this.screenScale = 1f;

			modelMatrix = new Matrix();

			boundingBox = context.GetClipBoundingBox();

			graphicsUnit = GraphicsUnit.Pixel;
			pageScale = 1;

			// Set anti-aliasing
			SmoothingMode = SmoothingMode.Default;

			setupView();
		}

#if MONOTOUCH
		private Graphics() :
			this (UIGraphics.GetCurrentContext(), UIScreen.MainScreen.Scale)
		{
		}

		private Graphics(CGContext context) :
			this (context, UIScreen.MainScreen.Scale)
		{	}

		private Graphics(CGContext context, float screenScale)
		{
			var gc = context;
			nativeObject = gc;
			this.screenScale = screenScale;
			InitializeContext(gc);

		}

		public static Graphics FromContext(CGContext context)
		{
			return new Graphics (context, UIScreen.MainScreen.Scale);
		}

		public static Graphics FromContext(CGContext context, float screenScale)
		{
			return new Graphics (context, screenScale);
		}

#endif

#if MONOMAC || XAMARINMAC
		private Graphics() :
			this (NSGraphicsContext.CurrentContext)
		{
		}

		private Graphics (NSGraphicsContext context) :
			this (context.CGContext(), context.IsFlipped)
		{
		}

		public static Graphics FromContext (NSGraphicsContext context)
		{
			return new Graphics (context);
		}

		public static Graphics FromHwnd (IntPtr hwnd, bool client)
		{
			if (hwnd == IntPtr.Zero) {
				var scaleFactor = NSScreen.MainScreen.BackingScaleFactor;
				var context = new CGBitmapContext(IntPtr.Zero, 1, 1, 8, 4, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedFirst);
				context.ScaleCTM(scaleFactor, -scaleFactor);
				return new Graphics(context);
			}

			Graphics g;
			var obj = NSRuntime.GetNSObject(hwnd);
			var view = obj as NSView;
			if (view == null && obj is NSWindow && ((NSWindow)obj).GraphicsContext != null) {
				g = new Graphics(((NSWindow)obj).GraphicsContext);
			} else if (NSView.FocusView () == view) {
				if (NSGraphicsContext.CurrentContext == null)
					return FromHwnd(IntPtr.Zero, false);
				g = Graphics.FromCurrentContext();
			} else if (view.LockFocusIfCanDraw ()) {
				if (NSGraphicsContext.CurrentContext == null)
					return FromHwnd(IntPtr.Zero, false);
				g = Graphics.FromCurrentContext();
				g.focusedView = view;
			} else if (view.Window != null && view.Window.GraphicsContext != null) {
				g = new Graphics (view.Window.GraphicsContext);
			} else {
				return Graphics.FromImage (new Bitmap (1, 1));
			}

			if (client) {
				if (view is IClientView clientView) {
					var clientBounds = clientView.ClientBounds;
					g.context.ClipToRect(clientBounds);
					g.context.TranslateCTM(clientBounds.Left, clientBounds.Top);
					g.context.SaveState();
					g.hasClientTransform = true;
				}
			}

			g.screenScale = g.context.GetCTM().xx; // view.Layer?.ContentsScale ?? 1f;

			return g;
		}

		public static Graphics FromHwnd(IntPtr hwnd)
		{
			return FromHwnd(hwnd, true);
		}
#endif

		public static Graphics FromCurrentContext()
		{
			return new Graphics ();
		}

		internal float GraphicsUnitConvertX (float x)
		{
			return ConversionHelpers.GraphicsUnitConversion(PageUnit, GraphicsUnit.Pixel, DpiX, x);
		}

		internal float GraphicsUnitConvertY (float y)
		{
			return ConversionHelpers.GraphicsUnitConversion(PageUnit, GraphicsUnit.Pixel, DpiY, y);
		}

		~Graphics ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		internal void Dispose (bool disposing)
		{
			if (disposing) {
				if (context != null)
				{
					context.RestoreState();
					if (hasClientTransform)
						context.RestoreState();
					context = null;
				}

				if (focusedView != null) {
					focusedView.UnlockFocus ();
					focusedView = null;
				}
			}
		}

		public IntPtr GetHdc()
		{
			return context.Handle;	
			//throw new NotSupportedException ();
		}

		public void ReleaseHdc()
		{
			//throw new NotSupportedException ();
		}

		public void ReleaseHdc(IntPtr hdc)
		{
			//throw new NotSupportedException ();
		}

		public static Graphics FromHdc(IntPtr hdc)
		{
			if (hdc != IntPtr.Zero)
			{
				var context = new CGContext(hdc);
				return new Graphics(context);
			}
			return null;
		}

		public static Graphics FromHdcInternal(IntPtr hdc)
		{
			throw new NotSupportedException ();		
		}

		// from: gdip_cairo_move_to, inlined to assume converts_unit=true, antialias=true
		void MoveTo (float x, float y)
		{
			context.MoveTo (x, y);
		}
		
		void MoveTo (PointF point)
		{
			context.MoveTo (point.X, point.Y);
		}

		void LineTo (PointF point)
		{
			context.AddLineToPoint (point.X, point.Y);
		}

		void LineTo (float x, float y)
		{
			context.AddLineToPoint (x, y);
		}
		
		void CurveTo (float x1, float y1, float x2, float y2, float x3, float y3)
		{
			context.AddCurveToPoint (x1, y1, x2, y2, x3, y3);
		}

		void PreparePen (Pen pen)
		{
			if (pen.Width == 1f)
				context.TranslateCTM (.5f, .5f);
		}

		void Stroke (Pen pen)
		{
			// First we call the Pen with a fill of false so the brush can setup the stroke 
			// For LinearGradientBrush this will setup a TransparentLayer so the gradient can
			// be filled at the end.  See comments.
			pen.Setup (this, false);
			context.DrawPath(CGPathDrawingMode.Stroke);
			// Next we call the Pen with a fill of true so the brush can draw the Gradient. 
			// For LinearGradientBrush this will draw the Gradient and end the TransparentLayer.
			// See comments.
			pen.Setup (this, true);
			if (pen.Width == 1f)
				context.TranslateCTM (-.5f, -.5f);
		}

		void StrokePen (Pen pen)
		{
			Stroke (pen);
			// FIXME: draw custom start/end caps
		}

		public void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		
		public void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		public void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			PreparePen (pen);
			DrawEllipticalArc(x, y, width, height, startAngle, sweepAngle, false);
			StrokePen (pen);
		}

		// Microsoft documentation states that the signature for this member should be
		// public void DrawArc( Pen pen,  int x,  int y,  int width,  int height,   int startAngle,
   		// int sweepAngle. However, GdipDrawArcI uses also float for the startAngle and sweepAngle params
   		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			
			PreparePen (pen);
			DrawEllipticalArc(x, y, width, height, startAngle, sweepAngle, false);
			StrokePen (pen);
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			// DrawLine is throwing an assertion error on MonoTouch
			// Assertion failed: (CGFloatIsValid(x) && CGFloatIsValid(y))
			// , function void CGPathAddLineToPoint(CGMutablePathRef, const CGAffineTransform *, CGFloat, CGFloat)
			// What we will do here is not draw the line at all if any of the points are Single.NaN
			if (!float.IsNaN(pt1.X) && !float.IsNaN(pt1.Y) &&
			    !float.IsNaN(pt2.X) && !float.IsNaN(pt2.Y)) 
			{
				PreparePen (pen);
				MoveTo (pt1.X, pt1.Y);
				LineTo (pt2.X, pt2.Y);
				StrokePen (pen);
			}
		}
		
		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			PreparePen (pen);
			MoveTo (pt1.X, pt1.Y);
			CurveTo (pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
			StrokePen (pen);
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			PreparePen (pen);
			MoveTo (pt1.X, pt1.Y);
			CurveTo (pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
			StrokePen (pen);
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			PreparePen (pen);
			MoveTo (x1, y1);
			CurveTo (x2, y2, x3, y3, x4, y4);
			StrokePen (pen);
		}
		
		public void DrawBeziers (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
            int length = points.Length;
            if (length < 4)
	            return;

			for (int i = 0; i < length - 1; i += 3) {
	            Point p1 = points [i];
	            Point p2 = points [i + 1];
	            Point p3 = points [i + 2];
	            Point p4 = points [i + 3];

				DrawBezier (pen, p1, p2, p3, p4);
			}
		}

		public void DrawBeziers (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
            int length = points.Length;
            if (length < 4)
	            return;

			for (int i = 0; i < length - 1; i += 3) {
	            var p1 = points [i];
	            var p2 = points [i + 1];
	            var p3 = points [i + 2];
	            var p4 = points [i + 3];

				DrawBezier (pen, p1, p2, p3, p4);
			}
		}
		
		public void DrawLine (Pen pen, PointF pt1, PointF pt2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			// DrawLine is throwing an assertion error on MonoTouch
			// Assertion failed: (CGFloatIsValid(x) && CGFloatIsValid(y))
			// , function void CGPathAddLineToPoint(CGMutablePathRef, const CGAffineTransform *, CGFloat, CGFloat)
			// What we will do here is not draw the line at all if any of the points are Single.NaN
			if (!float.IsNaN(pt1.X) && !float.IsNaN(pt1.Y) &&
			    !float.IsNaN(pt2.X) && !float.IsNaN(pt2.Y)) 
			{
				PreparePen (pen);
				MoveTo (pt1.X, pt1.Y);
				LineTo (pt2.X, pt2.Y);
				StrokePen (pen);
			}
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			PreparePen (pen);
			MoveTo (x1, y1);
			LineTo (x2, y2);
			StrokePen (pen);

		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			PreparePen (pen);
			MoveTo (x1, y1);
			LineTo (x2, y2);
			StrokePen (pen);
		}
		
		public void DrawLines (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			if (points == null)
				throw new ArgumentNullException ("points");
			int count = points.Length;
			if (count < 2)
				return;

			PreparePen (pen);
			MoveTo (points [0]);
			for (int i = 1; i < count; i++)
				LineTo (points [i]);
			StrokePen (pen);
		}
		
		public void DrawLines (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			if (points == null)
				throw new ArgumentNullException ("points");
			int count = points.Length;
			if (count < 2)
				return;

			PreparePen (pen);
			MoveTo (points [0]);
			for (int i = 1; i < count; i++)
				LineTo (points [i]);
			StrokePen (pen);

		}

		internal void RectanglePath (float x1, float y1, float x2, float y2)
		{
			MoveTo (x1, y1);
			LineTo (x1, y2);
			LineTo (x2, y2);
			LineTo (x2, y1);
			context.ClosePath ();
		}
			
		internal void RectanglePath (CGRect rectangle) 
		{
			//context.AddRect(rectangle);
			RectanglePath((float)rectangle.Left, (float)rectangle.Top, (float)rectangle.Right, (float)rectangle.Bottom);
		}

		public void DrawRectangle (Pen pen, float x1, float y1, float x2, float y2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			PreparePen (pen);
			RectanglePath (new CGRect(x1, y1, x2, y2));
			StrokePen (pen);
		}
		
		public void DrawRectangle (Pen pen, int x1, int y1, int x2, int y2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			
			PreparePen (pen);
			RectanglePath (new CGRect(x1, y1, x2, y2));
			StrokePen (pen);
		}
		
		public void DrawRectangle (Pen pen, Rectangle rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			PreparePen (pen);
			RectanglePath (new CGRect(rect.X, rect.Y, rect.Width, rect.Height));
			StrokePen (pen);
		}

		public void FillRectangle (Brush brush, float x, float y, float w, float h)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			brush.FillRect(this, new CGRect(x, y, w, h));
		}

		public void FillRectangle (Brush brush, Rectangle rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			brush.FillRect(this, new CGRect(rect.X, rect.Y, rect.Width, rect.Height));
		}
		
		public void FillRectangle (Brush brush, RectangleF rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			brush.FillRect(this, new CGRect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public void FillRectangle (Brush brush, int x, int y, int w, int h)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			brush.FillRect(this, new CGRect(x, y, w, h));
		}

		public void FillRegion (Brush brush, Region region)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (region == null)
				throw new ArgumentNullException ("region");

			// We will clear the rectangle of our clipping bounds for Empty
			if (region.regionPath == null) 
			{
				// This may set the rectangle to Black depending on the context
				// passed.  On a NSView set WantsLayers and the Layer Background color.
				context.ClearRect (context.GetClipBoundingBox ());
				return;
			}

			context.AddPath (region.regionPath);
			brush.FillPath(this);
		}

		public void DrawEllipse (Pen pen, RectangleF rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			PreparePen (pen);
			context.AddEllipseInRect(new CGRect(rect.X, rect.Y, rect.Width, rect.Height));
			StrokePen (pen);
		}

		public void DrawEllipse (Pen pen, int x1, int y1, int x2, int y2)
		{
			DrawEllipse (pen, new RectangleF (x1, y1, x2, y2));
		}

		public void DrawEllipse (Pen pen, Rectangle rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			
			DrawEllipse (pen, new RectangleF (rect.X, rect.Y, rect.Width, rect.Height));
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");

			DrawEllipse (pen, new RectangleF (x, y, width, height));

		}

		public void FillEllipse (Brush brush, RectangleF rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");

			context.AddEllipseInRect(new CGRect(rect.X, rect.Y, rect.Width, rect.Height));
			brush.FillPath(this);
		}

		public void FillEllipse (Brush brush, int x1, int y1, int x2, int y2)
		{
			FillEllipse (brush, new RectangleF (x1, y1, x2, y2));
		}
		
		public void FillEllipse (Brush brush, float x1, float y1, float x2, float y2)
		{
			FillEllipse (brush, new RectangleF (x1, y1, x2, y2));
		}

		private void applyModelView() {
			
			// Since there is no context.SetCTM, only ConcatCTM
			// get the current transform, invert it, and concat this to
			// obtain the identity.   Then we concatenate the value passed
			if (modelViewMatrix != null && !modelViewMatrix.IsIdentity) {
				modelViewMatrix.Invert();
				context.ConcatCTM (modelViewMatrix.transform);
			}

			modelViewMatrix = viewMatrix.Clone();
			modelViewMatrix.Multiply (modelMatrix);

//			Console.WriteLine("------------ apply Model View ------");
//			Console.WriteLine("Model: " + modelMatrix.transform);
//			Console.WriteLine("View: " + viewMatrix.transform);
//			Console.WriteLine("ModelView: " + modelViewMatrix.transform);
//			Console.WriteLine("------------ end apply Model View ------\n\n");
			// we apply the Model View matrix passed to the context
			context.ConcatCTM (modelViewMatrix.transform);

		} 

		public void ResetTransform ()
		{
			modelMatrix.Reset();
			applyModelView();
		}
		
		public Matrix Transform {
			get {
				return modelMatrix;
			}
			set {
				modelMatrix = value;
				applyModelView();
			}
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			modelMatrix.Rotate(angle, order);
			applyModelView();
		}
		
		public void TranslateTransform (float tx, float ty)
		{
			TranslateTransform (tx, ty, MatrixOrder.Prepend);
		}
		
		public void TranslateTransform (float tx, float ty, MatrixOrder order)
		{
			//Console.WriteLine ("Currently does not support anything but prepend mode");
			modelMatrix.Translate(tx, ty, order);
			applyModelView();
		}
		
		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}
		
		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			modelMatrix.Scale(sx,sy,order);
			applyModelView();
		}
		
		void MakeCurve (PointF [] points, PointF [] tangents, int offset, int length, CurveType type)
		{
			MoveTo (points [offset].X, points [offset].Y);
			int i = offset;
			
			for (; i < offset + length; i++) {
				int j = i + 1;

				float x1 = points [i].X + tangents [i].X;
				float y1 = points [i].Y + tangents [i].Y;

				float x2 = points [j].X - tangents [j].X;
				float y2 = points [j].Y - tangents [j].Y;

				float x3 = points [j].X;
				float y3 = points [j].Y;

				context.AddCurveToPoint (x1, y1, x2, y2, x3, y3);
			}

			if (type == CurveType.Close) {
				/* complete (close) the curve using the first point */
				float x1 = points [i].X + tangents [i].X;
				float y1 = points [i].Y + tangents [i].Y;

				float x2 = points [0].X - tangents [0].X;
				float y2 = points [0].Y - tangents [0].Y;

				float x3 = points [0].X;
				float y3 = points [0].Y;

				context.AddCurveToPoint (x1, y1, x2, y2, x3, y3);

				context.ClosePath ();
			}
		}
		
		internal PointF [] ConvertPoints (Point [] points)
		{
			if (points == null)
				return null;
			int len = points.Length;
			var result = new PointF [len];
			for (int i = 0; i < len; i++)
				result [i] = new PointF (points [i].X, points [i].Y);
			return result;
		}
		
		
		public void DrawCurve (Pen pen, PointF[] points, int offset, int numberOfSegments, float tension = 0.5f)
		{
			if (points == null)
				throw new ArgumentNullException ("points");
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (numberOfSegments < 1)
				throw new ArgumentException ("numberOfSegments");

			int count = points.Length;
			// we need 3 points for the first curve, 2 more for each curves 
			// and it's possible to use a point prior to the offset (to calculate) 
			if (offset == 0 && numberOfSegments == 1 && count < 3)
				throw new ArgumentException ("invalid parameters");
			if (numberOfSegments >= points.Length - offset)
				throw new ArgumentException ("offset");

			var tangents = GeomUtilities.GetCurveTangents (GraphicsPath.CURVE_MIN_TERMS, points, count, tension, CurveType.Open);
			PreparePen (pen);
			MakeCurve (points, tangents, offset, numberOfSegments, CurveType.Open);
			StrokePen (pen);
		}

		public void DrawCurve (Pen pen, Point[] points, int offset, int numberOfSegments, float tension = 0.5f)
		{
			DrawCurve (pen, ConvertPoints (points), offset, numberOfSegments, tension);
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension = 0.5f)
		{
			DrawCurve (pen, ConvertPoints (points), tension);
		}

		public void DrawCurve (Pen pen, PointF [] points, float tension = 0.5f)
		{
			if (points == null)
				throw new ArgumentNullException ("points");
			int count = points.Length;
			if (count == 2)
				DrawLines (pen, points);
			else {
				int segments = (count > 3) ? (count-1) : (count-2);
				
				DrawCurve (pen, points, 0, segments, tension);
			}
		}

		void PlotPath (GraphicsPath path)
		{
			float x1 = 0, y1 = 0, x2 = 0, y2 = 0, x3 = 0, y3 = 0;
			var points = path.PathPoints;
			var types = path.PathTypes;
			int bidx = 0;
			
			for (int i = 0; i < points.Length; i++){
				var point = points [i];
				var type = (PathPointType) types [i];

				switch (type & PathPointType.PathTypeMask){
				case PathPointType.Start:
					MoveTo (point.X, point.Y);
					break;
					
				case PathPointType.Line:
					LineTo (point.X, point.Y);
					break;
					
				case PathPointType.Bezier3:
					// collect 3 points
					switch (bidx++){
					case 0:
						x1 = point.X;
						y1 = point.Y;
						break;
					case 1:
						x2 = point.X;
						y2 = point.Y;
						break;
					case 2:
						x3 = point.X;
						y3 = point.Y;
						break;
					}
					if (bidx == 3){
						context.AddCurveToPoint (x1, y1, x2, y2, x3, y3);
						bidx = 0;
					}
					break;
				default:
					throw new Exception ("Inconsistent internal state, path type=" + type);
				}
				if ((type & PathPointType.CloseSubpath) != 0)
					context.ClosePath ();
			}
		}
		
		public void DrawPath (Pen pen, GraphicsPath path)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (path == null)
				throw new ArgumentNullException ("path");

			PreparePen (pen);
			PlotPath (path);
			StrokePen (pen);
		}

		public void FillPath (Brush brush, GraphicsPath path)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (path == null)
				throw new ArgumentNullException ("path");
			PlotPath (path);

			var fillMode = path.FillMode;
			if (path.isReverseWindingOnFill)
				fillMode = (fillMode == FillMode.Alternate) ? FillMode.Winding : FillMode.Alternate;

			brush.FillPath(this, fillMode);
		}
		
		CompositingMode compositing_mode;
		public CompositingMode CompositingMode {
			get {
				return compositing_mode;
			}
			set {
				compositing_mode = value;
				switch (compositing_mode) 
				{
				case CompositingMode.SourceCopy:
					context.SetBlendMode (CGBlendMode.Copy);
					break;
				case CompositingMode.SourceOver:
					context.SetBlendMode (CGBlendMode.Overlay);
					break;
				}
			}
		}

		public Color GetNearestColor(Color color)
		{
			// this uses a color pallette which we really do not implement
			// so just return back the color passed for now.
			return Color.FromArgb (color.ToArgb());
		}

		void setupView() 
		{
			if (!isFlipped) {
				viewMatrix = new Matrix(1, 0, 0, -1, 0, (float)boundingBox.Height);
			} else {
				viewMatrix = new Matrix();
			}
			userspaceScaleX = GraphicsUnitConvertX(1) * pageScale;
			userspaceScaleY = GraphicsUnitConvertY(1) * pageScale;
			viewMatrix.Scale(userspaceScaleX, userspaceScaleY);
			viewMatrix.Translate(renderingOrigin.X * userspaceScaleX, -renderingOrigin.Y * userspaceScaleY, MatrixOrder.Append);
			applyModelView();
			
		}

		public GraphicsUnit PageUnit { 
			get {
				return graphicsUnit;
			}
			set {
				graphicsUnit = value;

				setupView();
			} 
		}

		public float PageScale 
		{ 
			get { return pageScale; }
			set {
				// TODO: put some validation in here maybe?  Need to 
				pageScale = value;
				setupView();			
			}
		}

		public TextRenderingHint TextRenderingHint { get; set; }
		
		public static Graphics FromImage (Image image)
		{
			if (image == null) 
				throw new ArgumentNullException ("image");

			if ((image.PixelFormat & PixelFormat.Indexed) != 0)
				throw new Exception ("Cannot create Graphics from an indexed bitmap.");
			
			Bitmap b = image as Bitmap;
			if (b == null)
				throw new Exception ("Can not create Graphics contexts from " + image.GetType () + " Images, only Bitmaps are supported");

			var bitmapContext = b.GetRenderableContext ();

			return new Graphics (bitmapContext, false);
		}
		
		public void SetClip (RectangleF rect)
		{
			SetClip (rect, CombineMode.Replace);
		}

		public void SetClip (Rectangle rect)
		{
			SetClip (rect, CombineMode.Replace);
		}

		public void SetClip (GraphicsPath graphicsPath)
		{
			SetClip (graphicsPath, CombineMode.Replace);
		}
		
		public void SetClip (Graphics g)
		{
			SetClip (g, CombineMode.Replace);	
		}

		public void SetClip (RectangleF rect, CombineMode combineMode)
		{
			if (combineMode == CombineMode.Intersect) {
				if (clip == null || clip.IsInfinite(this)) {
					clip = new Region(rect);
				} else {
					clip.Intersect(rect);
				}
				context.ClipToRect(rect.ToCGRect());
			} else if (combineMode == CombineMode.Replace) {
				if (combineMode == CombineMode.Replace && clip != null && !clip.IsInfinite(this)) {
					ResetNativeClip();
				}
				clip = new Region(rect);
				context.ClipToRect(rect.ToCGRect());
			} else {
				SetClip (new Region (rect), combineMode);
			}
		}

		public void SetClip (Rectangle rect, CombineMode combineMode)
		{
			SetClip ((RectangleF)rect, combineMode);
		}

		public void SetClip (GraphicsPath graphicsPath, CombineMode combineMode)
		{
			SetClip (new Region (graphicsPath), combineMode);
		}
		
		public void SetClip (Graphics g, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		public void SetClip (Region region, CombineMode combineMode)
		{
			// We need to reset the clip that is active now so that the graphic
			// states are correct when we set them.
			ResetNativeClip();

			switch (combineMode) {
				case CombineMode.Replace:
					// Set our clip region by cloning the region that is passed for now
					clip = region.Clone ();
					break;
				case CombineMode.Intersect:
					if (clip == null) {
						clip = region.Clone();
					} else {
						clip.Intersect(region);
					}
					break;
				case CombineMode.Union:
					if (clip != null) {
						clip.Union(region);
					}
					break;
				case CombineMode.Exclude:
					if (clip == null) {
						clip = new Region();
					}
					clip.Exclude(region);
					break;
				case CombineMode.Xor:
					if (clip == null) {
						clip = new Region();
					}
					clip.Xor(region);
					break;
				default:
					throw new NotImplementedException ("SetClip for CombineMode " + combineMode + " not implemented");
			}

			//Unlike the current path, the current clipping path is part of the graphics state. 
			//Therefore, to re-enlarge the paintable area by restoring the clipping path to a 
			//prior state, you must save the graphics state before you clip and restore the graphics 
			//state after you’ve completed any clipped drawing.
			if (Clip.IsEmpty(this)) {
				context.ClipToRect (CGRect.Empty);
			} else {
				context.AddPath (Clip.regionPath);
				context.ClosePath ();
				context.Clip ();
			}
		}
		
		public GraphicsContainer BeginContainer ()
		{
			throw new NotImplementedException ();
		}
		
		public GraphicsContainer BeginContainer (Rectangle dstRect, Rectangle srcRect, GraphicsUnit unit)
		{
			throw new NotImplementedException ();		
		}

		public GraphicsContainer BeginContainer (RectangleF dstRect, RectangleF srcRect, GraphicsUnit unit)
		{
			throw new NotImplementedException ();		
		}

		public void EndContainer (GraphicsContainer container)
		{
			throw new NotImplementedException ();
		}	

		// CGContext Anti-Alias:
		// A Boolean value that specifies whether anti-aliasing should be turned on. 
		// Anti-aliasing is turned on by default when a window or bitmap context is created. 
		// It is turned off for other types of contexts.

		// Default, None, and HighSpeed are equivalent and specify rendering without smoothing applied.
		// AntiAlias and HighQuality are equivalent and specify rendering with smoothing applied.
		public SmoothingMode SmoothingMode { 
			get { return smoothingMode; } 
			set 
			{
                if (smoothingMode != value) {
	                // Quartz performs antialiasing for a graphics context if both the allowsAntialiasing parameter 
	                // and the graphics state parameter shouldAntialias are true.
					switch (value)
					{
					case SmoothingMode.AntiAlias:
					case SmoothingMode.HighQuality:
					case SmoothingMode.Default:
                        if (smoothingMode != SmoothingMode.AntiAlias &&
                            smoothingMode != SmoothingMode.HighQuality &&
                            smoothingMode != SmoothingMode.Default) {
						    //context.SetAllowsAntialiasing(true);  // This parameter is not part of the graphics state.
						    context.SetShouldAntialias(true);
                        }
						break;
					default:
                        if (smoothingMode == SmoothingMode.AntiAlias &&
                            smoothingMode == SmoothingMode.HighQuality &&
                            smoothingMode == SmoothingMode.Default) {
    						//context.SetAllowsAntialiasing(false); // This parameter is not part of the graphics state.
	    					context.SetShouldAntialias(false);
                        }
						break;
					}
					smoothingMode = value;
				}
			}
		}
		
		public bool IsClipEmpty {
			get {
				return Clip.IsEmpty(this);
			}
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
				//throw new NotImplementedException ();
				return PixelOffsetMode.None;					
			}
			set {
				//throw new NotImplementedException ();
			}
		}
		
		public Region Clip {
			get {
				return clip ?? infinitRegion;
			}
			set {
				SetClip (value, CombineMode.Replace);
			}
		}
		
		public RectangleF ClipBounds {
			get {
				return Clip.GetBounds ();
				//return context.GetClipBoundingBox ();
			}
			set {
				SetClip(value);
			}
		}
		
		public RectangleF VisibleClipBounds {
			get {
				// FIXME
				return Clip.GetBounds ();
				//throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets or sets the interpolation mode.  The interpolation mode determines how intermediate values between two endpoints are calculated.
		/// </summary>
		/// <value>The interpolation mode.</value>
		public InterpolationMode InterpolationMode {
			get {
				return interpolationMode;
			}
			set {
				interpolationMode = value;
				switch (value) 
				{
				case InterpolationMode.Low:
					context.InterpolationQuality = CGInterpolationQuality.Low;
					break;
				case InterpolationMode.High:
				case InterpolationMode.HighQualityBicubic:
				case InterpolationMode.HighQualityBilinear:
					context.InterpolationQuality = CGInterpolationQuality.High;
					break;
				case InterpolationMode.NearestNeighbor:
				case InterpolationMode.Bicubic:
				case InterpolationMode.Bilinear:
					context.InterpolationQuality = CGInterpolationQuality.Medium;
					break;
				case InterpolationMode.Invalid:
					context.InterpolationQuality = CGInterpolationQuality.None;
					break;
				default:
					context.InterpolationQuality = CGInterpolationQuality.Default;
					break;
				}

			}
		}

		public PointF RenderingOrigin {
			get {
				return renderingOrigin;
			}
			set {

				renderingOrigin = value;
				setupView();
			}
		}
		
		public int TextContrast {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public float DpiX { 
			get {
				// We should probably read the NSScreen attributes and get the resolution
				//    but there are problems getting the value from NSValue to a Rectangle
				// We will set this to a fixed value for now
				return 96.0f;
			}
		}

		public float DpiY { 
			get {
				// We should probably read the NSScreen attributes and get the resolution
				//    but there are problems getting the value from NSValue to a Rectangle
				// We will set this to a fixed value for now
				return 96.0f;
			}
		}


		public CompositingQuality CompositingQuality {
			// There is no support for CompositingQuality in CoreGraphics.
			// Instead of throwing a NotImplementedException we will just let
			// things fall through when setting and return Default always.
			get {
				return CompositingQuality.Default;
			}
			set {

			}
		}
		
		public bool IsVisibleClipEmpty { 
			get {
				return Clip.IsEmpty(this);
			}
		}

		public void TranslateClip (int dx, int dy)
		{
			TranslateClip ((float)dx, (float)dy);
		}
		
		public void TranslateClip (float dx, float dy)
		{
			if (clip != null) {
				Clip.Translate (dx, dy);
				SetClip (Clip, CombineMode.Replace);
			}
		}

		public void ResetClip ()
		{
			ResetNativeClip();
			clip = null;
		}

		public void ResetNativeClip()
		{
			LastPen = null;
			LastBrush = null;

			//Unlike the current path, the current clipping path is part of the graphics state. 
			//Therefore, to re-enlarge the paintable area by restoring the clipping path to a 
			//prior state, you must save the graphics state before you clip and restore the graphics 
			//state after you’ve completed any clipped drawing.
			context.RestoreState();
			context.SaveState();
			modelViewMatrix = null;
			applyModelView();
		}

		public void ExcludeClip (Rectangle rect)
		{
			SetClip ((RectangleF)rect, CombineMode.Exclude);
		}
		
		public void ExcludeClip (RectangleF rect)
		{
			SetClip (rect, CombineMode.Exclude);
		}

		public void ExcludeClip (Region region)
		{
			SetClip (region, CombineMode.Exclude);
		}

		public void IntersectClip (Rectangle rect)
		{
			SetClip ((RectangleF)rect, CombineMode.Intersect);
		}
		
		public void IntersectClip (RectangleF rect)
		{
			SetClip (rect, CombineMode.Intersect);
		}

		public void IntersectClip (Region region)
		{
			SetClip (region, CombineMode.Intersect);
		}

		public void Clear (Color color)
		{
			context.SaveState ();
			context.SetFillColor(color);
			context.FillRect(context.GetClipBoundingBox());
			context.RestoreState ();
		}
		
		public void Restore (GraphicsState gstate)
		{
			LastPen = null;
			LastBrush = null;
			//LastPen = gstate.lastPen;
			//LastBrush = gstate.lastBrush;
			modelMatrix = gstate.model;
			viewMatrix = gstate.view;
			renderingOrigin = gstate.renderingOrigin;
			graphicsUnit = gstate.pageUnit;
			pageScale = gstate.pageScale;
			SmoothingMode = gstate.smoothingMode;
			clip = gstate.clipRegion;
			applyModelView();
		}
		
		public GraphicsState Save ()
		{
			var currentState = new GraphicsState();
			//currentState.lastPen = LastPen;
			//currentState.lastBrush = LastBrush;
			// Make sure we clone the Matrices or we will still modify
			// them after the save as they are the same objects.  Woops!!
			currentState.model = modelMatrix.Clone();
			currentState.view = viewMatrix.Clone();
			currentState.renderingOrigin = renderingOrigin;
			currentState.pageUnit = graphicsUnit;
			currentState.pageScale = pageScale;
			currentState.smoothingMode = smoothingMode;
			currentState.clipRegion = clip == null ? null : clip.Clone();
			return currentState;
		}

		public void DrawClosedCurve (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			DrawClosedCurve (pen, points, 0.5f, FillMode.Winding);
		}
		
		public void DrawClosedCurve (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			DrawClosedCurve (pen, ConvertPoints (points), 0.5f, FillMode.Winding);
		}
 			
		// according to MSDN fillmode "is required but ignored" which makes _some_ sense since the unmanaged 
		// GDI+ call doesn't support it (issue spotted using Gendarme's AvoidUnusedParametersRule)
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");

			DrawClosedCurve (pen, ConvertPoints (points), tension, fillmode);
		}

		// according to MSDN fillmode "is required but ignored" which makes _some_ sense since the unmanaged 
		// GDI+ call doesn't support it (issue spotted using Gendarme's AvoidUnusedParametersRule)
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");

			int count = points.Length;
			if (count == 2)
				DrawPolygon (pen, points);
			else {
				int segments = (count > 3) ? (count-1) : (count-2);

				var tangents = GeomUtilities.GetCurveTangents (GraphicsPath.CURVE_MIN_TERMS, points, count, tension, CurveType.Close);
				PreparePen (pen);
				MakeCurve (points, tangents, 0, segments, CurveType.Close);
				StrokePen (pen);
			}
		}
		
		public void FillClosedCurve (Brush brush, PointF [] points)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			FillClosedCurve(brush,points,FillMode.Alternate);
		}
		
		public void FillClosedCurve (Brush brush, Point [] points)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			FillClosedCurve(brush,ConvertPoints(points),FillMode.Alternate);
		}
 			
		// according to MSDN fillmode "is required but ignored" which makes _some_ sense since the unmanaged 
		// GDI+ call doesn't support it (issue spotted using Gendarme's AvoidUnusedParametersRule)
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension = 0.5f)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			FillClosedCurve(brush,points,FillMode.Alternate);
		}

		// according to MSDN fillmode "is required but ignored" which makes _some_ sense since the unmanaged 
		// GDI+ call doesn't support it (issue spotted using Gendarme's AvoidUnusedParametersRule)
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension = 0.5f)
		{

			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			int count = points.Length;
			if (count == 2)
				FillPolygon(brush, points, FillMode.Alternate);
			else {
				int segments = (count > 3) ? (count-1) : (count-2);
				
				var tangents = GeomUtilities.GetCurveTangents (GraphicsPath.CURVE_MIN_TERMS, points, count, tension, CurveType.Close);
				MakeCurve (points, tangents, 0, segments, CurveType.Close);
				brush.FillPath(this);
			}
		}

		public void DrawIcon (Icon icon, Rectangle targetRect)
		{
			if (icon == null)
				throw new ArgumentNullException ("icon");

			var scaledSize = context.ConvertSizeToDeviceSpace(new CGSize(targetRect.Width, targetRect.Height));
			icon = new Icon(icon, new Size((int)scaledSize.Width, (int)scaledSize.Height));

			DrawImage (icon.ToBitmap (), targetRect);
		}

		public void DrawIcon (Icon icon, int x, int y)
		{
			if (icon == null)
				throw new ArgumentNullException ("icon");

			DrawImage (icon.ToBitmap (), x, y);
		}
		
		public void DrawIconUnstretched (Icon icon, Rectangle targetRect)
		{
			if (icon == null)
				throw new ArgumentNullException ("icon");

			DrawImageUnscaled (icon.ToBitmap (), targetRect);
		}

		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		static double radians (float degrees) {
			return degrees * Math.PI/180;
		
		}

		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			PreparePen (pen);
			DrawEllipticalArc(x,y,width,height, startAngle, sweepAngle, true);
			StrokePen (pen);

		}
		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			DrawEllipticalArc(rect, startAngle, sweepAngle, true);
			brush.FillPath(this);
		}

		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			DrawEllipticalArc(x,y,width,height, startAngle, sweepAngle, true);
			brush.FillPath(this);
		}

		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			DrawEllipticalArc(x,y,width,height, startAngle, sweepAngle, true);
			brush.FillPath(this);
		}

		void PolygonSetup (PointF [] points)
		{
			if (points == null)
				throw new ArgumentNullException ("points");
			if (points.Length < 2)
				throw new ArgumentException ("Needs at least two points");
			MoveTo (points [0]);
			for (int i = 0; i < points.Length - 0; i++)
				LineTo (points [i]);
		}
		
		public void DrawPolygon (Pen pen, Point [] points)
		{
			DrawPolygon (pen, ConvertPoints (points));
		}

		public void DrawPolygon (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			PreparePen (pen);
			PolygonSetup (points);
			context.ClosePath ();
			StrokePen (pen);
		}	

		public void FillPolygon (Brush brush, Point [] points)
		{
			FillPolygon (brush, ConvertPoints (points), FillMode.Alternate);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode)
		{
			FillPolygon (brush, ConvertPoints (points), fillMode);
		}

		public void FillPolygon (Brush brush, PointF [] points)
		{
			FillPolygon (brush, points, FillMode.Alternate);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode = FillMode.Alternate)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");

			PolygonSetup (points);
			brush.FillPath(this, fillMode);
		}
		
		public void DrawRectangles (Pen pen, RectangleF [] rects)
		{
			if (pen == null)
				throw new ArgumentNullException ("image");
			if (rects == null)
				throw new ArgumentNullException ("rects");

			PreparePen (pen);
			foreach (var rect in rects)
				RectanglePath (rect.X, rect.Y, rect.Right, rect.Bottom);
			StrokePen (pen);
		}

		public void DrawRectangles (Pen pen, Rectangle [] rects)
		{
			if (pen == null)
				throw new ArgumentNullException ("image");
			if (rects == null)
				throw new ArgumentNullException ("rects");

			PreparePen (pen);
			foreach (var rect in rects)
				RectanglePath (rect.X, rect.Y, rect.Right, rect.Bottom);
			StrokePen (pen);
		}

		public void FillRectangles (Brush brush, Rectangle [] rects)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (rects == null)
				throw new ArgumentNullException ("rects");

			foreach (var rect in rects)
				RectanglePath (rect.X, rect.Y, rect.Right, rect.Bottom);
			brush.FillPath(this);
		}

		public void FillRectangles (Brush brush, RectangleF [] rects)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (rects == null)
				throw new ArgumentNullException ("rects");

			foreach (var rect in rects)
				RectanglePath (rect.X, rect.Y, rect.Right, rect.Bottom);

			brush.FillPath(this);
		}


		public void Flush ()
		{
			Flush (FlushIntention.Flush);
		}

		
		public void Flush (FlushIntention intention)
		{
			if (intention == FlushIntention.Flush) {
				context.Flush ();
			} else {
				context.Synchronize ();
			}
		}
		
		public bool IsVisible (Point point)
		{
			return Clip.IsVisible (point);
		}
		
		public bool IsVisible (RectangleF rect)
		{
			return Clip.IsVisible (rect);
		}

		public bool IsVisible (PointF point)
		{
			return Clip.IsVisible (point);
		}
		
		public bool IsVisible (Rectangle rect)
		{
			return Clip.IsVisible (rect);
		}
		
		public bool IsVisible (float x, float y)
		{
			return Clip.IsVisible (x, y);
		}
		
		public bool IsVisible (int x, int y)
		{
			return Clip.IsVisible (x, y);
		}
		
		public bool IsVisible (float x, float y, float width, float height)
		{
			return IsVisible (new RectangleF (x, y, width, height));
		}

		public bool IsVisible (int x, int y, int width, int height)
		{
			return IsVisible (new RectangleF (x, y, width, height));
		}
		
		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			if (matrix == null)
				throw new ArgumentNullException ("matrix");

			if (order == MatrixOrder.Prepend)
				context.ConcatCTM (matrix.transform);
			else
				context.ConcatCTM (matrix.transform);
		}
		
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
		{
			if (pts == null)
				throw new ArgumentNullException ("pts");

			Matrix transform = new Matrix();
			ConversionHelpers.GetGraphicsTransform (this, destSpace, srcSpace, ref transform);
			transform.TransformPoints (pts);
		}


		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
		{						
			if (pts == null)
				throw new ArgumentNullException ("pts");

			Matrix transform = new Matrix();
			ConversionHelpers.GetGraphicsTransform (this, destSpace, srcSpace, ref transform);
			transform.TransformPoints (pts);

		}
	}
}
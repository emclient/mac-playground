//
// PathGradientBrush.cs: PathGradientBrush implementation for MonoTouch and MonoMac
//
// Authors:
//   Kenneth Pouncey (kjpou@pt.lu)
//
// Copyright 2013
//

using System.Drawing;
using System.ComponentModel;

#if XAMARINMAC
using CoreGraphics;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
#endif


namespace System.Drawing.Drawing2D {

	public sealed class PathGradientBrush : Brush {

		PointF[] pathPoints;

		// Fields
		//bool interpolationColorsWasSet;
		WrapMode wrapMode = WrapMode.Clamp;
		//bool gammaCorrection;
		//bool changed;
		Matrix gradientTransform = new Matrix();

		//Color[] colors = new Color[2];

		Blend blend;
		Color centerColor = Color.White;
		PointF focusScales = PointF.Empty;
		Color[] surroundColors = new Color[] { Color.White };
		ColorBlend colorBlend = new ColorBlend();
		RectangleF rectangle = RectangleF.Empty;
		PointF centerPoint = PointF.Empty;

		// Everything I have read on the internet shows Microsoft 
		// using a 2.2 gamma correction for colors.
		// for instance: http://msdn.microsoft.com/en-gb/library/windows/desktop/hh972627(v=vs.85).aspx
		//float gamma = 1.0f / 2.2f;

		// Shading
		//float[][] shadingColors;

		// When stroking with a gradient we have to use Transparency Layers.
		bool hasTransparencyLayer = false;

		FillMode polygonWinding = FillMode.Winding;

		public PathGradientBrush (GraphicsPath path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			var pathClone = (GraphicsPath)path.Clone ();
			pathClone.CloseAllFigures ();
			pathClone.Flatten ();
			pathPoints = pathClone.PathPoints;

			// make sure we have a closed path
			if (pathPoints[0] != pathPoints[pathPoints.Length - 1])
			{
				var first = pathPoints [0];
				var temps = new PointF[pathPoints.Length + 1];
				for (var p = 0; p < pathPoints.Length; p++)
					temps[p] = pathPoints[p];

				temps[temps.Length - 1] = first;

				pathPoints = temps;
			}


			rectangle = GeomUtilities.PolygonBoundingBox (pathPoints);
			centerPoint = GeomUtilities.PolygonCentroid (pathPoints);
			wrapMode = WrapMode.Clamp;

			// verify the winding of the polygon so that we cen calculate the 
			// edges correctly
			var vt1 = pathPoints [0];
			var vt2 = centerPoint;
			var vt3 = pathPoints [1];

			var pWinding = vt1.X * vt2.Y - vt2.X * vt1.Y;
			pWinding += vt2.X * vt3.Y - vt3.X * vt2.Y;
			pWinding += vt3.X * vt1.Y - vt1.X * vt3.Y;

			// Positive is counter clockwise
			if (pWinding < 0)
				polygonWinding = FillMode.Alternate;

			blend = new Blend(1);
			blend.Factors = new float[]{ 1.0f};
			blend.Positions = new float[] { 1.0f };
		}

		public PathGradientBrush (Point [] points) : this (points, WrapMode.Clamp)
		{	}

		public PathGradientBrush (PointF [] points) : this (points, WrapMode.Clamp)
		{	}

		public PathGradientBrush (Point [] points, WrapMode wrapMode) : this(points.ToFloat(), wrapMode)
		{	}

		public PathGradientBrush (PointF [] points, WrapMode wrapMode)
		{
			if (points == null)
				throw new ArgumentNullException ("points");
			if ((wrapMode < WrapMode.Tile) || (wrapMode > WrapMode.Clamp))
				throw new InvalidEnumArgumentException ("WrapMode");

			pathPoints = points;

			// make sure we have a closed path
			if (pathPoints[0] != pathPoints[pathPoints.Length - 1])
			{
				var first = pathPoints [0];
				var temps = new PointF[pathPoints.Length + 1];
				for (var p = 0; p < pathPoints.Length; p++)
					temps[p] = pathPoints[p];

				temps[temps.Length - 1] = first;

				pathPoints = temps;
			}


			rectangle = GeomUtilities.PolygonBoundingBox (pathPoints);
			centerPoint = GeomUtilities.PolygonCentroid (pathPoints);
			this.wrapMode = wrapMode;

			// verify the winding of the polygon so that we cen calculate the 
			// edges correctly
			var vt1 = pathPoints [0];
			var vt2 = centerPoint;
			var vt3 = pathPoints [1];

			var pWinding = vt1.X * vt2.Y - vt2.X * vt1.Y;
			pWinding += vt2.X * vt3.Y - vt3.X * vt2.Y;
			pWinding += vt3.X * vt1.Y - vt1.X * vt3.Y;

			if (pWinding < 0)
				polygonWinding = FillMode.Alternate;

			blend = new Blend(1);
			blend.Factors = new float[]{ 1.0f};
			blend.Positions = new float[] { 1.0f };

		}

		// Properties

		public Blend Blend
		{
			get
			{
				return blend;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Blend");
				}
				blend = value;
				changed = true;
			}
		}

		public Color CenterColor {
			get {
				return centerColor;
			}
			set {
				centerColor = value;
			}
		}

		public PointF CenterPoint {
			get {

				return centerPoint;
			}
			set {
				centerPoint = value;
			}
		}

		public PointF FocusScales {
			get {
				return focusScales;
			}
			set {
				focusScales = value;
			}
		}

		public ColorBlend InterpolationColors {
			get {
				return colorBlend;
			}
			set {
				// no null check, MS throws a NullReferenceException here
				int count;
				Color [] colors = value.Colors;
				float [] positions = value.Positions;
				count = colors.Length;

				if (count == 0 || positions.Length == 0)
					throw new ArgumentException ("Invalid ColorBlend object. It should have at least 2 elements in each of the colors and positions arrays.");

				if (count != positions.Length)
					throw new ArgumentException ("Invalid ColorBlend object. It should contain the same number of positions and color values.");

				if (positions [0] != 0.0F)
					throw new ArgumentException ("Invalid ColorBlend object. The positions array must have 0.0 as its first element.");

				if (positions [count - 1] != 1.0F)
					throw new ArgumentException ("Invalid ColorBlend object. The positions array must have 1.0 as its last element.");

				int [] blend = new int [colors.Length];
				for (int i = 0; i < colors.Length; i++)
					blend [i] = colors [i].ToArgb ();

				colorBlend = value;
			}
		}

		public RectangleF Rectangle {
			get {
				return rectangle;
			}
		}

		public Color [] SurroundColors {
			get {
				return surroundColors;
			}
			set {
				surroundColors = value;
				changed = true;
			}
		}

		public Matrix Transform
		{
			get
			{
				return gradientTransform;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Transform");
				}
				gradientTransform = value.Clone();
				changed = true;
			}
		}

		public WrapMode WrapMode {
			get {
				return wrapMode;
			}
			set {
				if ((value < WrapMode.Tile) || (value > WrapMode.Clamp))
					throw new InvalidEnumArgumentException ("WrapMode");

				wrapMode = value;
				changed = true;
			}
		}

		// Methods

		public void MultiplyTransform(Matrix matrix)
		{
			this.MultiplyTransform(matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform(Matrix matrix, MatrixOrder order)
		{
			if (matrix == null)
			{
				throw new ArgumentNullException("matrix");
			}
			gradientTransform.Multiply(matrix, order);
			changed = true;
		}

		public void ResetTransform()
		{
			gradientTransform.Reset();
			changed = true;
		}

		public void RotateTransform(float angle)
		{
			this.RotateTransform(angle, MatrixOrder.Prepend);
		}

		public void RotateTransform(float angle, MatrixOrder order)
		{
			gradientTransform.Rotate(angle, order);
			changed = true;
		}

		public void ScaleTransform(float sx, float sy)
		{
			this.ScaleTransform(sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform(float sx, float sy, MatrixOrder order)
		{
			gradientTransform.Scale(sx, sy, order);
			changed = true;
		}

		public void TranslateTransform(float dx, float dy)
		{
			this.TranslateTransform(dx, dy, MatrixOrder.Prepend);
		}

		public void TranslateTransform(float dx, float dy, MatrixOrder order)
		{
			gradientTransform.Translate(dx, dy, order);
			changed = true;
		}

		public void SetBlendTriangularShape (float focus)
		{
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale)
		{
			if (focus < 0 || focus > 1 || scale < 0 || scale > 1)
				throw new ArgumentException ("Invalid parameter passed.");

			blend = new Blend(3);
			blend.Positions[1] = focus;
			blend.Positions[2] = 1.0f;

			blend.Factors[1] = scale;

			changed = true;
		}

		public void SetSigmaBellShape (float focus)
		{
			SetSigmaBellShape (focus, 1.0F);
		}

		public void SetSigmaBellShape (float focus, float scale)
		{
			if (focus < 0 || focus > 1 || scale < 0 || scale > 1)
				throw new ArgumentException ("Invalid parameter passed.");

			this.Blend = GeomUtilities.SigmaBellShape(focus, scale);

		}

		public override object Clone ()
		{
			object clone = null;
			return clone;
		}

		// http://developer.apple.com/library/mac/#documentation/GraphicsImaging/Conceptual/drawingwithquartz2d/dq_shadings/dq_shadings.html#//apple_ref/doc/uid/TP30001066-CH207-BBCECJBF
		internal override void Setup (Graphics graphics, bool fill)
		{

			CGContext context = graphics.context;

			// if fill is false then we are being called from a Pen stroke so
			// we need to setup a transparency layer
			// http://developer.apple.com/library/mac/#documentation/GraphicsImaging/Conceptual/drawingwithquartz2d/dq_shadings/dq_shadings.html#//apple_ref/doc/uid/TP30001066-CH207-BBCECJBF
			if (!fill) 
			{
				context.BeginTransparencyLayer();
				hasTransparencyLayer = true;
				// Make sure we set a color here so that the gradient shows up
				graphics.lastBrushColor = Color.Black;
				return;
			}

			// if this is the same as the last that was set and no changes have been made
			// then return.
			if (graphics.LastBrush != this || changed)
			{
				//setupShadingColors();
			}

			// Transform the start and end points using the brush's transformation matrix
			gradientTransform.TransformPoints(pathPoints);

			RasterizePolygon (context, centerPoint, pathPoints, surroundColors, centerColor);

			// If we are in a Transparency layer then we need to end the transparency
			if (hasTransparencyLayer) {
				context.EndTransparencyLayer();	
			}

			changed = false;

			graphics.LastBrush = this;
			// We will reset the last pen so that it can be setup again
			// so that we do not loose the settings after stroking the gradient
			// not sure where the setting are being reset so this may be a hack
			// and things are just not being restored correctly.
			graphics.LastPen = null;
			// I am setting this to be used for Text coloring in DrawString
			graphics.lastBrushColor = surroundColors[surroundColors.Length - 1];
		}


		internal void RasterizePolygon(CGContext context, PointF center, PointF[] pathPoints,
		                                      Color[] surroundColors, Color centerColor)
		{

			var last = pathPoints[0];

			Color start = Color.Empty;
			Color end = Color.Empty;
			var count = pathPoints.Length - 1;
			var colorCount = surroundColors.Length;
			var startIndex = 0;
			var endIndex = 1;

//			// Create new stopwatch
//			var stopwatch = new System.Diagnostics.Stopwatch ();
//
//			// Begin timing
//			stopwatch.Start();

			for (int p = 1; p <= count; p++)
			{

				var next = pathPoints[p];

				if (startIndex >= colorCount)
				{
					start = surroundColors[colorCount - 1];
					end = surroundColors[colorCount - 1];
				}
				else
				{
					start = surroundColors[startIndex++];
					if (startIndex == colorCount)
					{
						end = surroundColors[0];
					}
					else
					{
						if (endIndex >= colorCount)
						{
							end = surroundColors[colorCount - 1];
						}
						else
						{
							end = surroundColors[endIndex++];
						}
					}
				}

				//Console.WriteLine("triangle {0} P1 {1} P2 {2} P3 {3} color {4}", p, last, next, center, start);
				if (polygonWinding == FillMode.Winding)
					RasterizeTriangle(context, center, last, next, centerColor, start, end);
				else
					RasterizeTriangle(context, last, center, next, start, centerColor, end);

				last = next;

			}

//			// Stop timing
//			stopwatch.Stop();
//
//			// Write result
//			Console.WriteLine("Time elapsed: {0}",
//				stopwatch.Elapsed);
		}

		struct Edge {

			// Dimensions of our pixel group
			public const int StepXSize = 1;
			public const int StepYSize = 1;

			// Step deltas
			public int StepX;
			public int StepY;

			// Edge function values at origin
			public int EdgeOrigin;

			// Barycentric interpolation factor for each vertex
			public float VertexFactor;

			// Vertex Color
			public Color Color;

			public int A;
			public int B;
			public int C;

			public Edge(PointF v1, PointF v2, PointF v3, PointF origin, Color color)
			{

				var ax = (int)v1.X;
				var ay = (int)v1.Y;
				var bx = (int)v2.X;
				var by = (int)v2.Y;
				var cx = (int)origin.X;
				var cy = (int)origin.Y;

				// Edge setup
				A = (int)Math.Floor((decimal)ay) - (int)Math.Floor((decimal)by);
				B = (int)Math.Floor((decimal)bx) - (int)Math.Floor((decimal)ax);
				C = ax * by - ay * bx;

				// Step deltas - This is setup here in case we want to process more than 1 x 1 pixel groups
				StepX = A * StepXSize;
				StepY = B * StepYSize;
				VertexFactor = 1.0f / ((bx - ax) * ((int)v3.Y - ay) - (by - ay) * ((int)v3.X - ax));
				// x/y values for initial pixel block
				int x = (int)origin.X;
				int y = (int)origin.Y;

				EdgeOrigin = A * x + B * y + C;

				Color = color;
			}

		};


		int edge32Red = 0;
		int edge32Green = 0;
		int edge32Blue = 0;
		int edge32Alpha = 0;

		int edge13Red = 0;
		int edge13Green = 0;
		int edge13Blue = 0;
		int edge13Alpha = 0;

		int edge21Red = 0;
		int edge21Green = 0;
		int edge21Blue = 0;
		int edge21Alpha = 0;



		/// <summary>
		/// Rasterizes the triangle specified by the vector / points and their associated colors
		/// using barycentric coordinates.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="vt1"></param>
		/// <param name="vt2"></param>
		/// <param name="vt3"></param>
		/// <param name="colorV1"></param>
		/// <param name="colorV2"></param>
		/// <param name="colorV3"></param>
		internal void RasterizeTriangle(CGContext context, PointF vt1, PointF vt2, PointF vt3, Color colorV1, Color colorV2, Color colorV3)
		{
			// get the bounding box of the triangle 
			int maxX = (int)Math.Max(vt1.X, Math.Max(vt2.X, vt3.X));
			int minX = (int)Math.Min(vt1.X, Math.Min(vt2.X, vt3.X));
			int maxY = (int)Math.Max(vt1.Y, Math.Max(vt2.Y, vt3.Y));
			int minY = (int)Math.Min(vt1.Y, Math.Min(vt2.Y, vt3.Y));

			// Barycentric coordinates at minX/minY corner
			PointF pm = new PointF( minX, minY );

			var edge32 = new Edge(vt3, vt2, vt1, pm, colorV1);
			var edge13 = new Edge (vt1, vt3, vt2, pm, colorV2);
			var edge21 = new Edge (vt2, vt1, vt3, pm, colorV3);


			int span32 = edge32.EdgeOrigin;
			int span13 = edge13.EdgeOrigin;
			int span21 = edge21.EdgeOrigin;

			edge32Red = colorV1.R;
			edge32Green = colorV1.G;
			edge32Blue = colorV1.B;
			edge32Alpha = colorV1.A;

			edge13Red = colorV2.R;
			edge13Green = colorV2.G;
			edge13Blue = colorV2.B;
			edge13Alpha = colorV2.A;

			edge21Red = colorV3.R;
			edge21Green = colorV3.G;
			edge21Blue = colorV3.B;
			edge21Alpha = colorV3.A;

			int span32XOffset = 0;
			int span13XOffset = 0;
			int span21XOffset = 0;

			bool inside = false;
			int mask = 0;
			//  Iterate over each pixel of bounding box and check if it's inside
			//  the triangle using the barycentirc approach.
			for (int y = minY; y <= maxY; y += Edge.StepYSize)
			{
				// Barycentric coordinates at start of row
				span32XOffset = span32;
				span13XOffset = span13;
				span21XOffset = span21;

				inside = false;
				for (int x = minX; x <= maxX; x += Edge.StepXSize)
				{

					mask = span32XOffset | span13XOffset | span21XOffset;

					// If p is on or inside all edges for any pixels,
					// render those pixels.
					if (mask >= 0)
					{
						if (!inside)
						{
							inside = true;
						}
						RenderPixels(context, x, y, edge32, edge13, edge21, span32XOffset, span13XOffset, span21XOffset);
					}

					// Step to the right
					span32XOffset += edge32.StepX;
					span13XOffset += edge13.StepX;
					span21XOffset += edge21.StepX;
					if (mask < 0 && inside)
					{
						inside = false;
						break;
					}

				}


				// Row step
				span32 += edge32.StepY;
				span13 += edge13.StepY;
				span21 += edge21.StepY;
			}
		}

		CGRect pixelRect = new CGRect (0, 0, 1, 1);
		void RenderPixels(CGContext context, int x, int y, Edge edge32, Edge edge13, Edge edge21, int w1, int w2, int w3)
		{

			//VertexInterpoliation = A * x + B * y + C;
//			float alpha = (edge32.A * x + edge32.B * y + edge32.C) * edge32.VertexFactor;
//			float beta = (edge13.A * x + edge13.B * y + edge13.C)  * edge13.VertexFactor;
//			float gamma = (edge21.A * x + edge21.B * y + edge21.C) * edge21.VertexFactor;

			// Determine barycentric coordinates
			float alpha = (float)(w1 * edge32.VertexFactor);
			float beta = (float)(w2 * edge13.VertexFactor);
			float gamma = (float)(w3 * edge21.VertexFactor);

			GradientLerp3 (alpha, beta, gamma);
			// Set the color
			context.SetFillColor (colorOutput [0], colorOutput [1], colorOutput [2], colorOutput [3]);

			// Set our pixel location
			pixelRect.X = x;
			pixelRect.Y = y;

			// Fill the pixel
			context.FillRect (pixelRect);

		}

		float[] colorOutput = new float[4];
		private void GradientLerp3(float alpha, float beta, float gamma)
		{

			var resRed = (alpha * edge32Red) + ((beta * edge13Red) + (gamma * edge21Red));
			var resGreen = (alpha * edge32Green) + ((beta * edge13Green) + (gamma  * edge21Green));
			var resBlue = (alpha * edge32Blue) + ((beta * edge13Blue) + (gamma * edge21Blue));
			var resAlpha = (alpha * edge32Alpha) + ((beta * edge13Alpha) + (gamma * edge21Alpha));

			colorOutput [0] = resRed/ 255f;
			colorOutput [1] = resGreen / 255;
        	colorOutput [2] = resBlue / 255f; 
        	colorOutput [3] = resAlpha / 255f;
		}
		public override bool Equals(object obj)
		{
			return (obj is PathGradientBrush b)
				&& pathPoints.Equals(b.pathPoints)
				&& wrapMode.Equals(b.wrapMode)
				&& gradientTransform.Equals(b.gradientTransform)
	            && centerColor.Equals(b.centerColor)
	            && focusScales.Equals(b.focusScales)
	            && surroundColors.Equals(b.surroundColors)
	            && colorBlend.Equals(b.colorBlend)
	            && rectangle.Equals(b.rectangle)
	            && centerPoint.Equals(b.centerPoint)
	            && polygonWinding.Equals(b.polygonWinding)
				&& blend.Equals(b.blend);
		}
	}
}

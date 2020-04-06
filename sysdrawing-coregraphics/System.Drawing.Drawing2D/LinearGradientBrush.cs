//
// LinearGradientBrush.cs: LinearGradientBrush implementation for MonoTouch and MonoMac
//
// Authors:
//   Kenneth Pouncey (kjpou@pt.lu)
//
// Copyright 2012
//
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;

#if XAMARINMAC
using CoreGraphics;
using System.Drawing.Mac;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
#endif

#if MAC64
using nfloat = System.Double;
#else
using nfloat = System.Single;
#endif
using NMath = System.Math;

namespace System.Drawing.Drawing2D 
{
	/// <summary>
	/// Summary description for LinearGradientBrush.
	/// </summary>
	public sealed class LinearGradientBrush : Brush
	{
		// Fields
		bool interpolationColorsWasSet;
		WrapMode wrapMode = WrapMode.Tile;
		bool gammaCorrection;
		//bool changed;
		Matrix gradientTransform = new Matrix();

		PointF startPoint;
		PointF endPoint;
		Color[] colors = new Color[2];
		Blend blend;
		ColorBlend colorBlend;
		RectangleF rectangle = RectangleF.Empty;
		float angle = 0;
		bool angleIsScalable = false;

		// Everything I have read on the internet shows Microsoft 
		// using a 2.2 gamma correction for colors.
		// for instance: http://msdn.microsoft.com/en-gb/library/windows/desktop/hh972627(v=vs.85).aspx
		float gamma = 1.0f / 2.2f;

		// Shading
		nfloat[,] shadingColors;

		// When stroking with a gradient we have to use Transparency Layers.
		bool hasTransparencyLayer = false;
		
		public LinearGradientBrush(Point point1, Point point2, Color color1, Color color2)
			: this(new PointF(point1.X, point1.Y),new PointF(point2.X, point2.Y), color1, color2)
		{

		}
		
		public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2)
		{
			startPoint = point1;
			endPoint = point2;

			colors[0] = color1;
			colors[1] = color2;

			rectangle.Size = new SizeF(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
			rectangle.X = (rectangle.Width < 0) ? endPoint.X : startPoint.X;
			rectangle.Y = (rectangle.Height < 0) ? endPoint.Y : startPoint.Y;

			if (rectangle.Width < 0)
			{
				rectangle.Width = -rectangle.Width;
			}

			if (rectangle.Height < 0)
			{
				rectangle.Height = -rectangle.Height;
			}

			if (rectangle.Height == 0)
			{
				rectangle.Height = rectangle.Width;
				rectangle.Y = rectangle.Y - rectangle.Height / 2.0f;
			}

			else if (rectangle.Width == 0)
			{
				rectangle.Width = rectangle.Height;
				rectangle.X = rectangle.X - rectangle.Width / 2.0f;
			}

			blend = new Blend(1);
			blend.Factors = new float[]{ 1.0f};
			blend.Positions = new float[] { 1.0f };
		}
		
		public LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
			: this(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), color1, color2, linearGradientMode)
		{
		}
		
		public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle) 
			: this(rect, color1, color2, angle, false)
		{
		}
		
		public LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
			: this(rect, color1, color2, linearGradientMode.ToAngle(), true)
		{
		}
		
		public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle) 
			: this(rect, color1, color2, angle, true)
		{

		}
		
		public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable)
			: this(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), color1, color2, angle, isAngleScaleable)
		{
		}
		
		public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
		{

			GeomUtilities.ComputeOrientationLine (rect, angle, out startPoint, out endPoint);	
			this.angle = angle;
			angleIsScalable = isAngleScaleable;
			colors[0] = color1;
			colors[1] = color2;
			
			rectangle = rect;
			
			blend = new Blend(1);
			blend.Factors = new float[]{ 1.0f};
			blend.Positions = new float[] { 1.0f };
			
		}
		
		public override object Clone()
		{
			var clone = new LinearGradientBrush(this.rectangle, colors[0], colors[1],
			                                    angle, angleIsScalable);
			clone.Blend = blend;
			if (interpolationColorsWasSet)
				clone.InterpolationColors = colorBlend;
			clone.Transform = gradientTransform;
			clone.GammaCorrection = gammaCorrection;

			return clone;
		}

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
		
		public bool GammaCorrection
		{
			get
			{
				return gammaCorrection;
			}
			set
			{
				if (value != gammaCorrection) 
				{
					gammaCorrection = value;
					changed = true;
				}
			}
		}
		
		public ColorBlend InterpolationColors
		{
			get
			{
				return colorBlend;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("ColorBlend");
				}
				colorBlend = value;
				interpolationColorsWasSet = true;
				changed = true;
			}
		}
		
		public Color[] LinearColors
		{
			get
			{
				return new Color[] {colors[0], colors[1]};
			}
			set
			{
				// TODO: do some sanity tests?
//				if (value == null || value[0] == null || value[1] == null)
//				{
//					throw new ArgumentNullException("LinearColors");
//				}

				colors[0] = value[0];
				colors[1] = value[1];
				changed = true;
			}
		}
		
		public RectangleF Rectangle
		{
			get
			{
				return rectangle;
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
		
		public WrapMode WrapMode
		{
			get
			{
				return wrapMode;
			}
			set
			{
				// Clamp is not valid
				if ((value < WrapMode.Tile) || (value >= WrapMode.Clamp))
					throw new InvalidEnumArgumentException ("WrapMode");
				wrapMode = value;
				changed = true;
			}
		}

		float[] positions;
		float[] factors;
		unsafe public void GradientLerp (nfloat *data, nfloat *outData) 
		{
			nfloat lerpDist = *(nfloat*)data;
		
			int i = 0;

			int numPositions = positions.Length;

			// Make sure we put the linear distance value back into the 0.0 .. 1.0 range
			// depending on the wrap mode
			if (wrapMode == WrapMode.Tile || wrapMode == WrapMode.TileFlipY)
			{
				// Repeat
				lerpDist = lerpDist - (nfloat)NMath.Floor(lerpDist);
			}
			else 
			{
				// Reflect
				lerpDist = (nfloat)NMath.Abs(lerpDist) % 2.0f;
				if (lerpDist > 1.0f) {
					lerpDist = 2.0f - lerpDist;
				}
			}

			for (i = 0; i < numPositions; i++)
			{
				if (positions[i] > lerpDist)
					break;
			}

			nfloat prevPosition = 0;
			nfloat dist = 0;
			nfloat normalized = 0;

			if (i == 0 || i == numPositions) {
				if (i == numPositions)
					--i;

				// When we have multiple positions we need to interpolate the colors
				// between the two positions.  
				// normalized will be the normalized [0,1] amount
				// of the gradiant area between the two positions.
				//
				// The shading colors have already
				// been setup with the color factors taken into account.

				// Get the distance between current position and last position
				dist = factors[i] - prevPosition;
				// normalized value between the two shading colors
				normalized = (nfloat)((lerpDist - prevPosition)/dist);
//				Console.WriteLine("prev {0} dist {1} normal {2} i {3} t {4}", 
//				                  prevPosition, dist, normalized, i, t);
				for(ushort ctr = 0; ctr < 4; ctr++) {
					
					outData[ctr] = GeomUtilities.Lerp(shadingColors[0,ctr], 
					                    shadingColors[1,ctr],
					                    normalized);
				}
			} 
			else 
			{
				// When we have multiple positions we need to interpolate the colors
				// between the two positions.  
				// normalized will be the normalized [0,1] amount
				// of the gradiant area between the two positions.
				//
				// The shading colors have already
				// been setup with the color factors taken into account.
				prevPosition = positions[i-1];
				// Get the distance between current position and last position
				dist = positions[i] - prevPosition;
				// normalized value between the two shading colors
				normalized = (nfloat)((lerpDist - prevPosition)/dist);

				for(ushort ctr = 0; ctr < 4; ctr++) {

					outData[ctr] = GeomUtilities.Lerp(shadingColors[i-1,ctr], 
					                    shadingColors[i,ctr],
					                    normalized);
				}
			}


			if (gammaCorrection) 
			{
				// * NOTE * Here I am only computing the gamma correction for RGB values not alpha
				// I am really not sure if this is correct or not but from my reading on this topic
				// it is really never mentioned that alpha is included.
				for(ushort ctr = 0; ctr < 3; ctr++) {
					
					outData[ctr] = (nfloat)Math.Pow(outData[ctr], gamma);
				}

			}

//			Console.WriteLine("R: {0}, G: {1}, B: {2}, A: {3}", 
//			                  outData[0],
//			                  outData[1],
//			                  outData[2],
//			                  outData[3]);

		}

		void setupShadingColors()
		{

			if (interpolationColorsWasSet) {
				setColorsUsingColorBlend();
			}
			else {
				setColorsUsingBlend();
			}

		}

		void setColorsUsingColorBlend() 
		{
			int size = colorBlend.Positions.Length;
			
			shadingColors = new nfloat[size,4];
			positions = colorBlend.Positions;

			float[] cgColor;
			for (int s = 0; s < size; s++)
			{
				cgColor = colorBlend.Colors[s].ElementsCGRGBA();
				//Console.WriteLine("shadingIndex {0} position {1} factor {2}",s, positions[s], factor);
				for (int c = 0; c < 4; c++)
				{
					shadingColors[s,c] = cgColor[c];
				}
			}
		}

		void setColorsUsingBlend() 
		{
			int size = blend.Positions.Length;
			
			if (size == 1) 
			{
				shadingColors = new nfloat[2,4];
				factors = new float[2];
				positions = new float[2];
				
				// check for default Blend to setup the shading colors
				// appropriately.
				if (blend.Factors[0] == 1 && blend.Positions[0] == 1)
				{
					factors[0] = 0f;
					positions[0] = 0;
				}
				else 
				{
					// This is a special case where a blend was set
					// with a factor.  It still does not give exact 
					// results with windows.  Need to look at this as
					// still not sure what it should do.
					// Example:
					//	float[] myFactors = { 0.2f };
					//	float[] myPositions = { 0.0f };
					//	Blend myBlend = new Blend();
					//	myBlend.Factors = myFactors;
					//	myBlend.Positions = myPositions;
					//	lgBrush2.Blend = myBlend;
					factors[0] = blend.Factors[0];
					positions[0] = 1.0f;
				}
				
				// Close off the color shadings 
				factors[1] = 1.0f;
				positions[1] = 1.0f;
				
			}
			else 
			{
				shadingColors = new nfloat[size,4];
				factors = blend.Factors;
				positions = blend.Positions;
			}
			
			float[] sc = colors[0].ElementsCGRGBA();
			float[] ec = colors[1].ElementsCGRGBA();
			
			float factor = 0;
			for (int s = 0; s < positions.Length; s++)
			{
				factor = factors[s];
				//Console.WriteLine("shadingIndex {0} position {1} factor {2}",s, positions[s], factor);
				
				for (int c = 0; c < 4; c++)
				{
					shadingColors[s,c] = GeomUtilities.Lerp (sc[c], ec[c], factor);
				}
			}
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
				setupShadingColors();
			}

			CGSize gradientRegionCg = context.GetClipBoundingBox().Size;
			SizeF gradientRegion = new SizeF ((float)gradientRegionCg.Width, (float)gradientRegionCg.Height);
//			SizeF gradientRegionPath = context.GetPathBoundingBox().Size;
			PointF sp = startPoint;
			PointF ep = endPoint;
			PointF[] points = {sp, ep};

			// Transform the start and end points using the brush's transformation matrix
			gradientTransform.TransformPoints(points);

			sp = points[0];
			ep = points[1];

			var cgf = CyclicGradientFunction(gradientRegion,
			                             ref sp, ref ep);

			var colorSpace = CGColorSpace.CreateDeviceRGB();
			CGPoint cgsp = new CGPoint(sp.X, sp.Y), cgep = new CGPoint(ep.X, ep.Y);
			var shading = CGShading.CreateAxial(colorSpace, cgsp, cgep, cgf, false, false);
			
			colorSpace.Dispose();

			context.SaveState();

			// if path is empty here then we are being called from a Pen stroke so
			// we set the blend mode for strokes and clip the path for fills
			if (!context.IsPathEmpty()) 
			{
				context.Clip();
			}
			else
			{
				context.SetBlendMode(CGBlendMode.SourceIn);
			}

			context.DrawShading(shading);
			context.RestoreState();

			// If we are in a Transparency layer then we need to end the transparency
			if (hasTransparencyLayer) {
				context.EndTransparencyLayer();	
			}

			cgf.Dispose();
			shading.Dispose();
			shading = null;

			changed = false;
			
			graphics.LastBrush = this;
			// We will reset the last pen so that it can be setup again
			// so that we do not loose the settings after stroking the gradient
			// not sure where the setting are being reset so this may be a hack
			// and things are just not being restored correctly.
			graphics.LastPen = null;
			// I am setting this to be used for Text coloring in DrawString
			graphics.lastBrushColor = colors[0];
		}

		internal void SetupSolid(Graphics graphics, bool fill)
		{
			if (graphics.LastBrush == this && !changed)
				return;

			if (fill)
				graphics.context.SetFillColor(colors[0]);
			else
				graphics.context.SetStrokeColor(colors[0]);

			graphics.LastBrush = this;
			changed = false;

			graphics.LastPen = null;
			// I am setting this to be used for Text coloring in DrawString
			graphics.lastBrushColor = colors[0];
		}

		internal override void FillRect(Graphics graphics, CGRect rect)
		{
			if (colors[0] == colors[1])
			{
				SetupSolid(graphics, true);
				graphics.context.FillRect(rect);
				return;
			}

			graphics.RectanglePath(rect);
			Setup(graphics, true);
			graphics.context.EOFillPath();
		}

		// Based on CreateRepetingGradientFunction from cairo-quartz-surface.c
		CGFunction CyclicGradientFunction(SizeF regionSize,
		                                     ref PointF start, ref PointF end)
		{
			
			PointF mstart, mend;

			double dx, dy;
			int x_rep_start = 0, x_rep_end = 0;
			int y_rep_start = 0, y_rep_end = 0;

			int rep_start, rep_end;
			
			// figure out how many times we'd need to repeat the gradient pattern
			// to cover the whole (transformed) surface area
			mstart = start;
			mend = end;

			dx = Math.Abs(mend.X - mstart.X);
			dy = Math.Abs(mend.Y - mstart.Y);
			
			//if (dx > 1e-6)
			// Changed this to 1e-4 because of the floating point precision with 1e-6 was causing
			// problems.
			if (dx > 1e-4)
			{
				x_rep_start = (int)Math.Ceiling(Math.Min(mstart.X, mend.X) / dx);
				x_rep_end = (int)Math.Ceiling((regionSize.Width - Math.Max(mstart.X, mend.X)) / dx);
				
				if (mend.X < mstart.X)
				{
					int swap = x_rep_end;
					x_rep_end = x_rep_start;
					x_rep_start = swap;
				}
			}
			
			//if (dy > 1e-6)
			// Changed this to 1e-4 because of the floating point precision with 1e-6 was causing
			// problems.
			if (dy > 1e-4)
			{
				y_rep_start = (int)Math.Ceiling(Math.Min(mstart.Y, mend.Y) / dy);
				y_rep_end = (int)Math.Ceiling((regionSize.Height - Math.Max(mstart.Y, mend.Y)) / dy);

				if (mend.Y < mstart.Y)
				{
					int swap = y_rep_end;
					y_rep_end = y_rep_start;
					y_rep_start = swap;
				}
			}
			
			rep_start = Math.Max(x_rep_start, y_rep_start);
			rep_end = Math.Max(x_rep_end, y_rep_end);
			
			// extend the line between start and end by rep_start times from the start
			// and rep_end times from the end
			dx = end.X - start.X;
			dy = end.Y - start.Y;
			
			start.X = start.X - (float)dx * rep_start;
			start.Y = start.Y - (float)dy * rep_start;
			
			end.X = end.X + (float)dx * rep_end;
			end.Y = end.Y + (float)dy * rep_end;

			// set the input range for the function -- the function knows how to
			// map values outside of 0.0 .. 1.0 to that range for the type of wrap mode.
			// See the CGFunctionEvaluate funtion for the mapping of values.
			nfloat[] validDomain = { 0, 1 };
			validDomain[0] = 0.0f - 1.0f * rep_start;
			validDomain[1] = 1.0f + 1.0f * rep_end;
			
			//Console.WriteLine("start point [0] : {0} end point [1] : {1}", start, end);
		
			nfloat[] validRange = new nfloat[8] 
			{ 0, 1.0f, 0, 1.0f, 0, 1.0f, 0, 1.0f };  // R, G, B, A
			
			CGFunction.CGFunctionEvaluate eval;
			CGFunction cgf;
			
			unsafe {
				eval = GradientLerp;
				//eval = DrawGradient;
				cgf = new CGFunction(validDomain, validRange, 
				                     eval);
			}

			return cgf;
		}

		public override bool Equals(object obj)
		{
			return (obj is LinearGradientBrush b)
				&& wrapMode.Equals(b.wrapMode)
				&& gammaCorrection.Equals(b.gammaCorrection)
				&& gradientTransform.Equals(b.gradientTransform)
				&& startPoint.Equals(b.startPoint)
				&& endPoint.Equals(b.endPoint)
				&& colors.Equals(b.colors)
				&& blend.Equals(b.blend)
				&& colorBlend.Equals(b.colorBlend)
				&& blend.Equals(b.blend)
				&& rectangle.Equals(b.rectangle)
				&& angle.Equals(b.angle)
				&& angleIsScalable == b.angleIsScalable;
		}
	}
}


//
// Pen.cs: The Pen code
//
// Authors:
//   Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2011-2013 Xamarin Inc
//
using System;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using CoreGraphics;
using MatrixOrder = System.Drawing.Drawing2D.MatrixOrder;

namespace System.Drawing
{
	public sealed partial class Pen : MarshalByRefObject, IDisposable, ICloneable
	{
		Brush brush;
		Color color;
		bool changed = true;
		internal bool isModifiable;
		float width;
		
		public Pen (Brush brush) : this (brush, 1f)
		{
		}

		public Pen (Color color) : this (color, 1f)
		{
		}

		public Pen (Brush brush, float width)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			this.brush = (Brush)brush.Clone ();
			var sb = brush as SolidBrush;
			if (sb != null)
				color = sb.Color;
			else
				color = Color.Black;
			this.width = width;
		}

		public Pen (Color color, float width)
		{
			brush = new SolidBrush (color);
			this.color = color;
			this.width = width;
		}

		public Brush Brush 
		{ 
			get {
				return brush;
			}
			set {
				brush = (Brush)value.Clone ();
				var sb = brush as SolidBrush;
				if (sb != null)
					color = sb.Color;
				else
					color = Color.Black;
				changed = true;
			}
		}

		public Color Color 
		{ 
			get {
				return color;
			}
			set {
				if (value != color) 
				{
					color = value;
					brush = new SolidBrush (color);
					changed = true;
				}
			}
		}


		~Pen ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		public void Dispose (bool disposing)
		{
			if (disposing) {
			}
		}

		public object Clone ()
		{
			if (brush != null)
				return new Pen (brush, width);
			else
				return new Pen (color, width);
		}

		public float Width {
			get {
				return width;
			}

			set {
				width = value;
				changed = true;
			}
		}

		Matrix transform;

		public Matrix Transform {
			get {
				return transform;
			}

			set {
				transform = value;
				changed = true;

			}
		}

		LineCap startCap = 0;

		public LineCap StartCap {
			get {
				return (LineCap)startCap;
			}
			set {
				if (Enum.IsDefined(typeof(LineCap), value)) {
					startCap = value;
					changed = true;

				}
				else
					throw new InvalidEnumArgumentException ("value", (int)value, typeof(LineCap));
			}
		}

		LineCap endCap = 0;
		
		public LineCap EndCap {
			get {
				return (LineCap)startCap;
			}
			set {
				if (Enum.IsDefined(typeof(LineCap), value)) {
					endCap = value;
					changed = true;

				}
				else
					throw new InvalidEnumArgumentException ("value", (int)value, typeof(LineCap));
			}
		}

		DashStyle dashStyle = 0;
		
		public DashStyle DashStyle {
			get {
				return (DashStyle)dashStyle;
			}
			set {
				if (Enum.IsDefined(typeof(DashStyle), value)) {
					dashStyle = value;
					changed = true;
					if (dashStyle != DashStyle.Custom)
						dashPattern = null;
				}
				else
					throw new InvalidEnumArgumentException ("value", (int)value, typeof(DashStyle));
			}
		}

		float dashOffset = 0;
		
		public float DashOffset {
			get {
				return dashOffset;
			}
			set {
				// fixme for error checking and range
				dashOffset = value;
				changed = true;
			}
		}

		float[] dashPattern = null;

		public float[] DashPattern {
			get {
				return dashPattern;
			}
			set {
				if (value != null) {
					dashStyle = DashStyle.Custom;
					dashPattern = value;
					changed = true;
				} else {
					dashStyle = DashStyle.Solid;
					dashPattern = null;
					changed = true;
				}
			}
		}

		public void SetLineCap (LineCap startCap, LineCap endCap, DashCap dashCap)
		{
			StartCap = startCap;
		}

		LineJoin lineJoin = LineJoin.Miter;
		public LineJoin LineJoin 
		{ 
			get { return lineJoin; }
			set { 
				lineJoin = value;
				changed = true;
			}
		}

		float miterLimit = 10f;
		public float MiterLimit 
		{ 
			get { return miterLimit; }
			set {
				miterLimit = value;
				changed = true;
			}
		}

		PenAlignment alignment;
		public PenAlignment Alignment {
			get {
				return alignment;
			}

			set {
				alignment = value;
				changed = true;
			}
		}

		/// <summary>
		/// Gets or sets an array of custom dashes and spaces. The dashes are made up of line segments.
		/// </summary>
		public float[] CompoundArray
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}		


		/// <summary>
		/// Gets or sets a custom cap style to use at the beginning of lines drawn with this <see cref='Pen'/>.
		/// </summary>
		public CustomLineCap CustomStartCap
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets or sets a custom cap style to use at the end of lines drawn with this <see cref='Pen'/>.
		/// </summary>
		public CustomLineCap CustomEndCap
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets or sets the cap style used at the beginning or end of dashed lines drawn with this <see cref='Pen'/>.
		/// </summary>
		public DashCap DashCap
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}		

		/// <summary>
		/// Gets the style of lines drawn with this <see cref='Pen'/>.
		/// </summary>
		public PenType PenType
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Resets the geometric transform for this <see cref='Pen'/> to identity.
		/// </summary>
		public void ResetTransform() => transform.Reset ();

		/// <summary>
		/// Multiplies the transform matrix for this <see cref='Pen'/> by the specified <see cref='Matrix'/>.
		/// </summary>
		public void MultiplyTransform(Matrix matrix) => MultiplyTransform(matrix, MatrixOrder.Prepend);

		/// <summary>
		/// Multiplies the transform matrix for this <see cref='Pen'/> by the specified <see cref='Matrix'/> in the specified order.
		/// </summary>
		public void MultiplyTransform(Matrix matrix, MatrixOrder order) => transform.Multiply (matrix, order);

		/// <summary>
		/// Translates the local geometrical transform by the specified dimensions. This method prepends the translation
		/// to the transform.
		/// </summary>
		public void TranslateTransform(float dx, float dy) => TranslateTransform(dx, dy, MatrixOrder.Prepend);

		/// <summary>
		/// Translates the local geometrical transform by the specified dimensions in the specified order.
		/// </summary>
		public void TranslateTransform(float dx, float dy, MatrixOrder order) => transform.Translate (dx, dy, order);

		/// <summary>
		/// Scales the local geometric transform by the specified amounts. This method prepends the scaling matrix to the transform.
		/// </summary>
		public void ScaleTransform(float sx, float sy) => ScaleTransform(sx, sy, MatrixOrder.Prepend);

		/// <summary>
		/// Scales the local geometric transform by the specified amounts in the specified order.
		/// </summary>
		public void ScaleTransform (float sx, float sy, MatrixOrder order) => transform.Scale (sx, sy, order);

		/// <summary>
		/// Rotates the local geometric transform by the specified amount. This method prepends the rotation to the transform.
		/// </summary>
		public void RotateTransform (float angle) => RotateTransform(angle, MatrixOrder.Prepend);

		/// <summary>
		/// Rotates the local geometric transform by the specified amount in the specified order.
		/// </summary>
		public void RotateTransform (float angle, MatrixOrder order) => transform.Rotate (angle, order);

		static float[] Dot = { 1.0f, 1.0f };
		static float[] Dash = { 3.0f, 1.0f };
		static float[] DashDot = { 3.0f, 1.0f, 1.0f, 1.0f };
		static float[] DashDotDot = { 3.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

		internal void Setup (Graphics graphics, bool fill)
		{

			CGContext context = graphics.context;

			brush.Setup (graphics, fill);
			// TODO: apply matrix

			if (graphics.LastPen == this && !changed)
				return;


			//  A Width of 0 will result in the Pen drawing as if the Width were 1.
			width = width == 0 ? 1 : width;
			//width = graphics.GraphicsUnitConvertFloat (width);

			context.SetLineWidth (width);

			switch (startCap) 
			{
			case LineCap.Flat:
				context.SetLineCap(width > 1f || dashStyle != DashStyle.Solid ? CGLineCap.Butt : CGLineCap.Square);
				break;
			case LineCap.Square:
				context.SetLineCap(CGLineCap.Square);
				break;
			case LineCap.Round:
				context.SetLineCap(CGLineCap.Round);
				break;
//			case LineCap.Triangle:
//			case LineCap.NoAnchor:
//			case LineCap.SquareAnchor:
//			case LineCap.RoundAnchor:
//			case LineCap.DiamondAnchor:
//			case LineCap.ArrowAnchor:
//			case LineCap.AnchorMask:
//			case LineCap.Custom:
			default:
				context.SetLineCap(CGLineCap.Butt);
				break;

			}

			switch (dashStyle) 
			{
			case DashStyle.Custom:
				context.SetLineDash(dashOffset,setupMorseCode(dashPattern));
				break;
			case DashStyle.Dash:
				context.SetLineDash(dashOffset,setupMorseCode(Dash));
				break;
			case DashStyle.Dot:
				context.SetLineDash(dashOffset,setupMorseCode(Dot));
				break;
			case DashStyle.DashDot:
				context.SetLineDash(dashOffset,setupMorseCode(DashDot));
				break;
			case DashStyle.DashDotDot:
				context.SetLineDash(dashOffset,setupMorseCode(DashDotDot));
				break;
			default:
				context.SetLineDash(0, new nfloat[0]);
				break;
			}
			// miter limit
			// join
			// cap
			// dashes

			changed = false;
			graphics.LastPen = this;
		}

		nfloat[] setupMorseCode (float[] morse) 
		{
			nfloat[] dashdots = new nfloat[morse.Length];
			for (int x = 0; x < dashdots.Length; x++) 
			{
				dashdots[x] = morse[x] * width;
			}

			return dashdots;
		}
	}
}
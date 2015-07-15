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

#if MONOMAC
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreGraphics;
#endif

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

				}
				else
					throw new InvalidEnumArgumentException ("value", (int)value, typeof(DashStyle));
			}
		}

		int dashOffset = 0;
		
		public int DashOffset {
			get {
				return dashOffset;
			}
			set {
				// fixme for error checking and range
				dashOffset = value;
				changed = true;
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
				context.SetLineCap(CGLineCap.Butt);
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
				context.SetLineDash(0, new float[0]);
				break;
			}
			// miter limit
			// join
			// cap
			// dashes

			changed = false;
			graphics.LastPen = this;
		}

		float[] setupMorseCode (float[] morse) 
		{
			float[] dashdots = new float[morse.Length];
			for (int x = 0; x < dashdots.Length; x++) 
			{
				dashdots[x] = morse[x] * width;
			}

			return dashdots;
		}
	}
}
//
// SolidBrush.cs: SolidBrush implementation for MonoTouch
//
// Authors:
//   Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2011-2013 Xamarin Inc
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Mac;
#if XAMARINMAC
using CoreGraphics;
#elif MONOMAC
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreGraphics;
#endif

namespace System.Drawing {

	public partial class SolidBrush : Brush {
		Color color;
		internal bool isModifiable;
		bool isModified;

		public SolidBrush (Color color)
		{
			this.color = color;
			isModifiable = true;
		}

		internal SolidBrush (Color color, bool isModifiable)
		{
			this.color = color;
			this.isModifiable = isModifiable;
		}
		
		public Color Color {
			get {
				return color;
			}
			set {
				if (value != color) 
				{
					color = value;
					isModified = true;
				}
			}
		}
		
		public override void Dispose (bool disposing)
		{
			if (disposing){
			}
		}

		public override object Clone ()
		{
			return new SolidBrush (color);
		}

		internal override void Setup (Graphics graphics, bool fill)
		{
			if (graphics.LastBrush == this && !isModified)
				return;

			if (fill) {
				graphics.context.SetFillColor(color);
				
			} else {
				graphics.context.SetStrokeColor(color);
			}

			graphics.LastBrush = this;
			isModified = false;

			// I am setting this to be used for Text coloring in DrawString
			graphics.lastBrushColor = color;
		}

		internal override void FillRect(Graphics graphics, CGRect rect)
		{
			Setup(graphics, true);
			graphics.context.FillRect(rect);
		}

		public override bool Equals(object obj)
		{
			return (obj is SolidBrush sb) && color.Equals(sb.Color);
		}

		public override string ToString()
		{
			return $"[SolidBrush color={Color}]";
		}
	}
}
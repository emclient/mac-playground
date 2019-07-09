//
// Brush.cs: The Brush setup code
//
// Authors:
//   Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2011-2013 Xamarin Inc
//
using System;
using System.Drawing.Drawing2D;
#if XAMARINMAC
using CoreGraphics;
#elif MONOMAC
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreGraphics;
#endif

namespace System.Drawing {

	public abstract partial class Brush : MarshalByRefObject, IDisposable, ICloneable {
		protected bool changed = true;
		
		~Brush ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		public virtual void Dispose (bool disposing)
		{
			if (disposing){
			}
		}

		abstract public object Clone ();

		internal abstract void Setup(Graphics graphics, bool fill);

		internal virtual void FillRect(Graphics graphics, CGRect rect)
		{
			// Use path by default, because this is how the default implementation works.
			// Some brushes override this to improve performance (SolidBrush, LinearGradientBrush)
			graphics.RectanglePath(rect);
			FillPath(graphics);
		}

		internal virtual void FillPath(Graphics graphics, FillMode mode = FillMode.Alternate)
		{
			Setup(graphics, true);
			if (mode == FillMode.Alternate)
				graphics.context.EOFillPath();
			else
				graphics.context.FillPath();
		}
	}
}
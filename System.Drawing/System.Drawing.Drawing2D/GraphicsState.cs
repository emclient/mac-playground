using System;
using System.Drawing;

namespace System.Drawing.Drawing2D
{
	public class GraphicsState {

		// TODO: set the rest of the states
		// These are just off the top of my head for right now as am sure there are
		// many more
		internal Pen lastPen { get; set; }
		internal Brush lastBrush { get; set; }
		internal Matrix model { get; set; }
		internal Matrix view { get; set; }
		internal PointF renderingOrigin { get; set; }
		internal GraphicsUnit pageUnit {get;set;}
		internal float pageScale { get; set; }
		internal SmoothingMode smoothingMode { get; set; }
		internal Region clipRegion { get; set; }

	}

}


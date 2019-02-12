//
// System.Drawing.Region.cs
//
// Authors:
//	Miguel de Icaza (miguel@ximian.com)
//	Jordi Mas i Hernandez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@xamarin.com>
//	Kenneth J. Pouncey  <kenneth.pouncey@xamarin.com>
//
// Copyright (C) 2003 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004,2006 Novell, Inc. http://www.novell.com
// Copyright 2011-2013 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Drawing.Drawing2D;
using System.Collections.Generic;

#if XAMARINMAC
using CoreGraphics;
#elif MONOMAC
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreGraphics;
#endif

// Polygon Clipping Library
using ClipperLib;

#if MAC64
using nfloat = System.Double;
#else
using nfloat = System.Single;
#endif

namespace System.Drawing 
{
	using System.Diagnostics;
	using Reflection;

	// Clipper lib definitions
	using Path = List<IntPoint>;
	using Paths = List<List<IntPoint>>;

	public sealed class Region : MarshalByRefObject, IDisposable 
	{

		const PolyFillType SUBJ_FILL_TYPE = PolyFillType.pftNonZero;
		const PolyFillType CLIP_FILL_TYPE = PolyFillType.pftNonZero;
		const bool EVEN_ODD_FILL = false;

		internal static CGRect infinite = new CGRect(-4194304, -4194304, 8388608, 8388608);
		internal object regionObject; 
		internal List<RegionEntry> regionList = new List<RegionEntry>();
		internal CGPath regionPath;
		internal CGRect regionBounds;


		//Here we are scaling all coordinates up by 100 when they're passed to Clipper 
		//via Polygon (or Polygons) objects because Clipper no longer accepts floating  
		//point values. Likewise when Clipper returns a solution in a Polygons object, 
		//we need to scale down these returned values by the same amount before displaying.
		private static float scale = 10000; //or 1 or 10 or 10000 etc for lesser or greater precision.

		internal Paths solution = new Paths();


		internal struct RegionEntry
		{
			public RegionType regionType;
			public Paths regionPath;
			public RegionClipType regionClipType;

			public RegionEntry (RegionType type) :
				this (type, new Paths(), RegionClipType.None)
			{   }

			public RegionEntry (RegionType type, Path path) :
				this (type, path, RegionClipType.None)
			{ 	}

			public RegionEntry (RegionType type, Path path, RegionClipType clipType)
			{	
				regionType = type;
				regionPath = new Paths() { path };
				regionClipType = clipType;
			}

			public RegionEntry (RegionType type, Paths path, RegionClipType clipType)
			{
				regionType = type;
				regionPath = path;
				regionClipType = clipType;
			}
		}
		
		internal enum RegionType
		{
			Rectangle = 10000,
			Infinity = 10001,
			Empty = 10002,
			Path = 10003,
		}

		internal enum RegionClipType { 
			Intersection = ClipType.ctIntersection, 
			Union = ClipType.ctUnion, 
			Difference = ClipType.ctDifference, 
			Xor = ClipType.ctXor,
			None = -1
		};


		// An infinite region would cover the entire device region which is the same as
		// not having a clipping region. Note that this is not the same as having an
		// empty region, which when clipping to it has the effect of excluding the entire
		// device region.
		public Region ()
		{
			// We set the default region to a very large 
			regionObject = infinite;

			var path = RectangleToPath (new RectangleF((float)infinite.X, (float)infinite.Y, (float)infinite.Width, (float)infinite.Height));
			solution.Add (path);

			regionList.Add (new RegionEntry (RegionType.Infinity, path));

			regionPath = new CGPath ();
			regionPath.MoveToPoint (infinite.Left, infinite.Top);
			regionPath.AddLineToPoint (infinite.Right, infinite.Top);
			regionPath.AddLineToPoint (infinite.Right, infinite.Bottom);
			regionPath.AddLineToPoint (infinite.Left, infinite.Bottom);

			regionBounds = infinite;
		}
		
		public Region (Rectangle rect) : 
			this ((RectangleF)rect)
		{ }
		
		public Region (RectangleF rect)
		{
			regionObject = rect;
			var path = RectangleToPath (rect);
			solution.Add (path);
			regionList.Add (new RegionEntry (RegionType.Rectangle, path));
			regionPath = new CGPath ();
			regionPath.MoveToPoint (rect.Left, rect.Top);
			regionPath.AddLineToPoint (rect.Right, rect.Top);
			regionPath.AddLineToPoint (rect.Right, rect.Bottom);
			regionPath.AddLineToPoint (rect.Left, rect.Bottom);

			regionBounds = new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public Region (GraphicsPath path)
		{
			var clonePath = (GraphicsPath)path.Clone(); 
			regionObject = clonePath;
			regionPath = new CGPath ();

			PlotPath (clonePath);

			clonePath.Flatten ();
			var flatPath = PointFArrayToIntArray (clonePath.PathPoints, scale);
			solution.Add (flatPath);
			regionList.Add (new RegionEntry (RegionType.Path, flatPath));
			regionBounds = regionPath.BoundingBox;
		}

		public IntPtr GetHrgn(Graphics g)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static Region FromHrgn(IntPtr hrgn)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return null;
		}

		internal static Path PointFArrayToIntArray(PointF[] points, float scale)
		{
			Path result = new Path();
			for (int i = 0; i < points.Length; ++i)
			{
				result.Add(new IntPoint((int)points[i].X * scale, (int)points[i].Y * scale)); 
			}
			return result;
		}

		
		internal static PointF[] PathToPointFArray(Path pg, float scale)
		{
			PointF[] result = new PointF[pg.Count];
			for (int i = 0; i < pg.Count; ++i)
			{
				result[i].X = (float)pg[i].X / scale;
				result[i].Y = (float)pg[i].Y / scale;
			}
			return result;
		}

		void PlotPath (GraphicsPath path)
		{
			nfloat x1 = 0, y1 = 0, x2 = 0, y2 = 0, x3 = 0, y3 = 0;
			var points = path.PathPoints;
			var types = path.PathTypes;
			int bidx = 0;

			for (int i = 0; i < points.Length; i++){
				var point = new CGPoint(points [i].X, points [i].Y);
				var type = (PathPointType) types [i];

				switch (type & PathPointType.PathTypeMask){
				case PathPointType.Start:
					regionPath.MoveToPoint (point);
					break;

				case PathPointType.Line:
					regionPath.AddLineToPoint (point);
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
						regionPath.AddCurveToPoint (x1, y1, x2, y2, x3, y3);
						bidx = 0;
					}
					break;
					default:
					throw new Exception ("Inconsistent internal state, path type=" + type);
				}
				if ((type & PathPointType.CloseSubpath) != 0)
					regionPath.CloseSubpath ();
			}
		}


		~Region ()
		{
			Dispose (false);
		}
		
		public bool Equals(Region region, Graphics g)
		{
			if (region == null)
				throw new ArgumentNullException ("region");
			if (g == null)
				throw new ArgumentNullException ("g");

			//FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
			return ReferenceEquals(this, region);
		}
		
		internal Region(Region region)
		{
			solution = Copy(region.solution);
			regionPath = new CGPath(region.regionPath);
			regionList = Copy(region.regionList);
			regionBounds = region.regionBounds;
			regionObject = CopyRegionObject(region.regionObject);
		}

		public Region Clone()
		{
			return new Region(this);
		}

		internal static Paths Copy(Paths src)
		{
			var dst = new Paths(src.Capacity);
			foreach (var path in src)
				dst.Add(new Path(path));
			return dst;
		}

		internal static List<RegionEntry> Copy(List<RegionEntry> src)
		{
			var dst = new List<RegionEntry>(src.Capacity);
			foreach (var r in src)
				dst.Add(new RegionEntry
				{
					regionType = r.regionType,
					regionClipType = r.regionClipType,
					regionPath = Copy(r.regionPath)
				});
			return dst;
		}

		internal static object CopyRegionObject(object src)
		{
			if (src is CGRect rect)
				return rect;
			if (src is CGPath path)
				return new CGPath(path);
			if (src is RectangleF rectf)
				return rectf;
			if (src is GraphicsPath graphicsPath)
				return graphicsPath.Clone(); 

			Console.WriteLine($"Unexpected type of regionObject ({src.GetType().Name})");
			return src;
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
		}
		
		public RectangleF GetBounds (Graphics g)
		{
			if (g == null)
				throw new ArgumentNullException ();

			// FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
			return GetBounds();
		}

//		public bool IsInfinite(Graphics g)
//		{
//		}

		public void MakeInfinite() 
		{
			regionObject = infinite;

			var path = RectangleToPath (new RectangleF((float)infinite.X, (float)infinite.Y, (float)infinite.Width, (float)infinite.Height));

			// clear out our containers.
			regionList.Clear ();
			solution.Clear ();

			solution.Add (path);
			regionList.Add (new RegionEntry (RegionType.Infinity, path));

			regionPath = new CGPath ();
			regionPath.MoveToPoint (infinite.Left, infinite.Top);
			regionPath.AddLineToPoint (infinite.Right, infinite.Top);
			regionPath.AddLineToPoint (infinite.Right, infinite.Bottom);
			regionPath.AddLineToPoint (infinite.Left, infinite.Bottom);

			regionBounds = regionPath.BoundingBox;
		}

		public void MakeEmpty() 
		{
			regionObject = RectangleF.Empty;

			var path = RectangleToPath (RectangleF.Empty);

			// clear out our containers.
			regionList.Clear ();
			solution.Clear ();

			solution.Add (path);
			regionList.Add (new RegionEntry (RegionType.Empty, path));

			regionPath = new CGPath ();

			regionBounds = CGRect.Empty;
		}

		public void Transform(Matrix matrix)
		{
			if (!regionPath.IsEmpty && !regionBounds.Equals(infinite)) 
			{
				foreach (var path in solution) 
				{
					for (int p = 0; p < path.Count; p++) 
					{
						var point = path [p];
						TransformIntPoint (ref point, matrix);
						path [p] = point;
					}
				}

				PathsToInternalPath (solution);

			}
		}

		/// <summary>
		/// Transform the specified Rectangle by the matrix that is passed.
		/// </summary>
		/// <param name="matrix">Matrix.</param>
		private static void TransformIntPoint (ref IntPoint point, Matrix matrix) 
		{
			var transform = matrix.transform;
			var x = point.X / scale;
			var y = point.Y / scale;

			point.X = (long)((transform.xx * x + transform.xy * y + transform.x0) * scale);
			point.Y = (long)((transform.yx * x + transform.yy * y + transform.y0) * scale);

		}


		public void Translate(int dx,int dy)
		{
			Translate ((float)dx, (float)dy);
		}

		public void Translate(float dx, float dy)
		{
			var translateMatrix = new Matrix(CGAffineTransform.MakeTranslation(dx, dy));
			Transform (translateMatrix);
		}

		public void Intersect(Rectangle rect)
		{
			Intersect ((RectangleF)rect);
		}

		public void Intersect(RectangleF rect)
		{
			regionList.Add(new RegionEntry(RegionType.Rectangle, RectangleToPath(rect), RegionClipType.Intersection));
			calculateRegionPath (ClipType.ctIntersection);
		}

		public void Intersect(Region region)
		{
			regionList.Add(new RegionEntry(RegionType.Path, region.solution, RegionClipType.Intersection));
			calculateRegionPath (ClipType.ctIntersection);
		}

		public void Union(Rectangle rect)
		{
			Union ((RectangleF)rect);
		}

		public void Union(RectangleF rect)
		{
			regionList.Add(new RegionEntry(RegionType.Rectangle, RectangleToPath(rect), RegionClipType.Union));
			calculateRegionPath (ClipType.ctUnion);
		}

		public void Union(Region region)
		{
			regionList.Add(new RegionEntry(RegionType.Path, region.solution, RegionClipType.Union));
			calculateRegionPath (ClipType.ctUnion);
		}

		public void Xor(Rectangle rect)
		{
			Xor ((RectangleF)rect);
		}

		public void Xor(RectangleF rect)
		{
			regionList.Add(new RegionEntry(RegionType.Rectangle, RectangleToPath(rect), RegionClipType.Xor));
			calculateRegionPath (ClipType.ctXor);
		}

		public void Xor(Region region)
		{
			regionList.Add(new RegionEntry(RegionType.Path, region.solution, RegionClipType.Xor));
			calculateRegionPath (ClipType.ctXor);
		}

		public void Exclude(Rectangle rect)
		{
			Exclude ((RectangleF)rect);
		}

		public void Exclude(RectangleF rect)
		{
			regionList.Add(new RegionEntry(RegionType.Rectangle, RectangleToPath(rect), RegionClipType.Difference));
			calculateRegionPath (ClipType.ctDifference);
		}

		public void Exclude(Region region)
		{
			regionList.Add(new RegionEntry(RegionType.Path, region.solution, RegionClipType.Difference));
			calculateRegionPath (ClipType.ctDifference);
		}

		void calculateRegionPath (ClipType clipType)
		{
			Clipper c = new Clipper();

			var subjects = solution;
			//subjects.Add (solution);

			var clips = new Paths ();

			foreach (var path in regionList [regionList.Count - 1].regionPath)
				clips.Add (path);


			c.AddPolygons(subjects, PolyType.ptSubject);
			c.AddPolygons(clips, PolyType.ptClip);

			solution.Clear();

			bool succeeded = c.Execute(clipType, solution, SUBJ_FILL_TYPE, CLIP_FILL_TYPE);

			if (succeeded) 
			{
				PathsToInternalPath (solution);

				// Not sure what this is returning
//				var bounds = c.GetBounds ();
//				regionBounds.X = bounds.left / scale;
//				regionBounds.Y = bounds.top / scale;
//				regionBounds.Width = (bounds.right - bounds.left) / scale;
//				regionBounds.Height = (bounds.bottom - bounds.top) / scale;

				if (regionPath.IsEmpty)
					regionBounds = CGRect.Empty;
				else
					regionBounds = regionPath.BoundingBox;

			}

		}

		void PathsToInternalPath(Paths paths)
		{

			regionPath = new CGPath ();

			foreach (var poly in solution)
			{
				regionPath.MoveToPoint(IntPointToCGPoint(poly[0]));

				for (var p =1; p < poly.Count; p++) 
				{
					regionPath.AddLineToPoint (IntPointToCGPoint (poly [p]));
				}
			}

		}

		internal RectangleF GetBounds()
		{
			return new RectangleF((float)regionBounds.X, (float)regionBounds.Y, (float)regionBounds.Width, (float)regionBounds.Height);
		}

		public bool IsInfinite(Graphics g)
		{
			return regionBounds.Equals (infinite);
		}

		public bool IsVisible(Point point)
		{
			return IsVisible ((PointF)point);
		}

		public bool IsVisible(PointF point)
		{
			// eoFill - A Boolean value that, if true, specifies to use the even-odd fill rule to evaluate 
			// the painted region of the path. If false, the winding fill rule is used.
			return regionPath.ContainsPoint (new CGPoint(point.X, point.Y), EVEN_ODD_FILL);
		}

		public bool IsVisible(Rectangle rectangle)
		{
			return IsVisible ((RectangleF)rectangle);
		}

		public bool IsVisible(RectangleF rectangle)
		{
			// eoFill - A Boolean value that, if true, specifies to use the even-odd fill rule to evaluate 
			// the painted region of the path. If false, the winding fill rule is used.
			var topLeft = new CGPoint (rectangle.Left, rectangle.Top);
			var topRight = new CGPoint (rectangle.Right, rectangle.Top);
			var bottomRight = new CGPoint (rectangle.Right, rectangle.Bottom);
			var bottomLeft = new CGPoint (rectangle.Left, rectangle.Bottom);

			return regionPath.ContainsPoint (topLeft, EVEN_ODD_FILL) || regionPath.ContainsPoint (topRight, EVEN_ODD_FILL)
				|| regionPath.ContainsPoint (bottomRight, EVEN_ODD_FILL) || regionPath.ContainsPoint (bottomLeft, EVEN_ODD_FILL);

		}

		public bool IsVisible(float x, float y)
		{
			// eoFill - A Boolean value that, if true, specifies to use the even-odd fill rule to evaluate 
			// the painted region of the path. If false, the winding fill rule is used.
			return regionPath.ContainsPoint (new CGPoint(x,y), EVEN_ODD_FILL);
		}

		public bool IsVisible(int x, int y)
		{
			// eoFill - A Boolean value that, if true, specifies to use the even-odd fill rule to evaluate 
			// the painted region of the path. If false, the winding fill rule is used.
			return regionPath.ContainsPoint (new CGPoint(x,y), EVEN_ODD_FILL);
		}

		public bool IsEmpty(Graphics g)
		{
			return regionPath.IsEmpty;
		}

		static Path RectangleToPath (RectangleF rect)
		{
			Path path = new Path ();

			path.Add(new IntPoint(rect.Left * scale, rect.Top * scale));
			path.Add(new IntPoint(rect.Right * scale, rect.Top * scale));
			path.Add(new IntPoint(rect.Right * scale, rect.Bottom * scale));
			path.Add(new IntPoint(rect.Left * scale, rect.Bottom * scale));
			path.Add(new IntPoint(rect.Left * scale, rect.Top * scale));

			return path;
		}
						
		static CGPoint IntPointToCGPoint (IntPoint point)
		{
			return new CGPoint (point.X / scale, point.Y / scale);
		}

		public RectangleF[] GetRegionScans(Matrix matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException("matrix");

			// FIXME
			NotImplemented(MethodBase.GetCurrentMethod());
			return new RectangleF[] { this.GetBounds() };
		}		

		static void NotImplemented(MethodBase methodBase)
		{
			System.Diagnostics.Debug.WriteLine("Not implemented:" + methodBase.Name);
		}
	}
}
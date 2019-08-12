//
// System.Drawing.Color.cs
//
// Authors:
// 	Dennis Hayes (dennish@raytek.com)
// 	Ben Houston  (ben@exocortex.org)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
// 	Juraj Skripsky (juraj@hotfeet.ch)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Dennis Hayes
// (c) 2002 Ximian, Inc. (http://www.ximiam.com)
// (C) 2005 HotFeet GmbH (http://www.hotfeet.ch)
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Drawing.Mac;
#if XAMARINMAC
using AppKit;
using CoreGraphics;
#else
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using nfloat = System.Single;
#endif

namespace System.Drawing 
{
	[Serializable]
	[TypeConverter(typeof(ColorConverter))]
	public struct Color {

		[Flags]
		internal enum Flags
		{
			HasValue = 1,
			IsKnown = 2,
			IsSystem = 4,
		}

		int value;
		Flags flags;
		NSColor nsColor;
		string name;

		internal Color(int value, string name = null) {
			value.ToArgb(out int a, out int r, out int g, out int b);
			this.nsColor = NSColor.FromRgba(r, g, b, a);
			this.name = name;
			this.value = 0;
			this.flags = 0;
		}

		internal Color(int a, int r, int g, int b, string name = null)
		{
			this.nsColor = NSColor.FromRgba(r, g, b, a);
			this.name = name;
			this.value = 0;
			this.flags = 0;
		}

	internal Color(NSColor color, string name = null)
		{
			value = 0;
			flags = 0;
			nsColor = color;
			this.name = name;
		}

		internal bool IsNSColor { get { return nsColor != null; } }

		#region Unimplemented bloated properties
		//
		// These properties were implemented very poorly on Mono, this
		// version will only store the int32 value and any helper properties
		// like Name, IsKnownColor, IsSystemColor, IsNamedColor are not
		// currently implemented, and would be implemented in the future
		// using external tables/hastables/dictionaries, without bloating
		// the Color structure
		//
		public string Name {
			get {
				if (name != null)
					return name;

				if (nsColor != null && nsColor.ColorSpaceName.Equals(NSColorSpace.Named.ToString(), StringComparison.Ordinal))
					return nsColor.ColorNameComponent;

				return KnownColors.NameByArgb.TryGetValue((uint)Value, out string s) ? s: String.Empty;
			}
		}

		public bool IsKnownColor {
			get {
				return KnownColors.NameByArgb.ContainsKey((uint)Value);
			}
		}

		public bool IsSystemColor {
			get {
				return 0 != (Flags.IsSystem & flags);
			}
		}

		public bool IsNamedColor {
			get {
				return name != null;
			}
		}
#endregion
	
		public static Color FromArgb (int red, int green, int blue)
		{
			return FromArgb (255, red, green, blue);
		}
		
		public static Color FromArgb (int alpha, int red, int green, int blue)
		{
			if((red > 255) || (red < 0))
				throw CreateColorArgumentException(red, "red");
			if((green > 255) || (green < 0))
				throw CreateColorArgumentException (green, "green");
			if((blue > 255) || (blue < 0))
				throw CreateColorArgumentException (blue, "blue");
			if((alpha > 255) || (alpha < 0))
				throw CreateColorArgumentException (alpha, "alpha");

			return new Color(alpha, red, green, blue);
		}

		internal NSColor NSColor
		{
			get { return nsColor ?? value.ToNSColor(); }
		}

		internal CGColor CGColor
		{
			get { return nsColor?.CGColor ?? value.ToCGColor(); }
		}

		public int ToArgb()
		{
			return Value;
		} 

		public static Color FromArgb (int alpha, Color baseColor)
		{
			return FromArgb (alpha, baseColor.R, baseColor.G, baseColor.B);
		}

		public static Color FromArgb (int argb)
		{
			return new Color(argb);
		}

		public static Color FromKnownColor (KnownColor color)
		{
			return KnownColors.FromKnownColor (color);
		}

		public static Color FromName(string name)
		{
			if (KnownColors.ArgbByName.TryGetValue(name, out uint argb))
				return new Color((int)argb);
		
			return new Color();
		}

	
		public static readonly Color Empty;
		
		public static bool operator == (Color left, Color right)
		{
			return left.Equals(right);
		}
		
		public static bool operator != (Color left, Color right)
		{
			return !left.EqualsTo(right);
		}

		public float GetBrightness ()
		{
			Value.ToArgb(out int _, out int r, out int g, out int b);
			byte minval = (byte)Math.Min(r, Math.Min (g, b));
			byte maxval = (byte)Math.Max(r, Math.Max (g, b));
	
			return (float)(maxval + minval) / 510;
		}

		public float GetSaturation ()
		{
			Value.ToArgb(out int _, out int r, out int g, out int b);
			byte minval = (byte)Math.Min(r, Math.Min(g, b));
			byte maxval = (byte)Math.Max(r, Math.Max(g, b));

			if (maxval == minval)
				return 0.0f;

			int sum = maxval + minval;
			if (sum > 255)
				sum = 510 - sum;

			return (float)(maxval - minval) / sum;
		}

		public float GetHue ()
		{
			Value.ToArgb(out int _, out int r, out int g, out int b);
			byte minval = (byte) Math.Min(r, Math.Min (g, b));
			byte maxval = (byte) Math.Max(r, Math.Max (g, b));
			
			if (maxval == minval)
				return 0.0f;
			
			float diff = (float)(maxval - minval);
			float rnorm = (maxval - r) / diff;
			float gnorm = (maxval - g) / diff;
			float bnorm = (maxval - b) / diff;
	
			float hue = 0.0f;
			if (r == maxval) 
				hue = 60.0f * (6.0f + bnorm - gnorm);
			if (g == maxval) 
				hue = 60.0f * (2.0f + rnorm - bnorm);
			if (b  == maxval) 
				hue = 60.0f * (4.0f + gnorm - rnorm);
			if (hue > 360.0f) 
				hue = hue - 360.0f;

			return hue;
		}
		
		public KnownColor ToKnownColor ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEmpty 
		{
			get {
				return nsColor == null && value == 0;
			}
		}

		internal int Value
		{
			get
			{
				if ((Flags.HasValue & flags) != 0)
					return value;
				if (nsColor != null) {
					return nsColor.ToArgb();
				}
				return value;
			}
		}

		public byte A {
			get { return (byte) (Value >> 24); }
		}

		public byte R {
			get { return (byte) (Value >> 16); }
		}

		public byte G {
			get { return (byte) (Value >> 8); }
		}

		public byte B {
			get { return (byte) Value; }
		}

		public override bool Equals (object obj)
		{
			return (obj is Color c) && EqualsTo(c);
		}

		internal bool EqualsTo(Color c)
		{
			if (nsColor != null && c.nsColor != null)
				return nsColor.Equals(c.nsColor);

			return Value == c.Value;
		}

		public override int GetHashCode ()
		{
			return nsColor != null ? nsColor.GetHashCode() : value;
		}

		public override string ToString ()
		{
			if (IsEmpty)
				return "Color [Empty]";
			if (nsColor != null)
				return $"Color [NSColor = {nsColor}]";

			return String.Format ("Color [A={0}, R={1}, G={2}, B={3}, Name={4}, Named={5}, System={6}]", A, R, G, B, Name, IsNamedColor, IsSystemColor);
		}
 
		private static ArgumentException CreateColorArgumentException (int value, string color)
		{
			return new ArgumentException (string.Format ("'{0}' is not a valid"
				+ " value for '{1}'. '{1}' should be greater or equal to 0 and"
				+ " less than or equal to 255.", value, color));
		}

		static public Color Transparent {
			get { return KnownColors.Get(KnownColor.Transparent); }
		}

		static public Color AliceBlue {
			get { return KnownColors.Get(KnownColor.AliceBlue); }
		}

		static public Color AntiqueWhite {
			get { return KnownColors.Get(KnownColor.AntiqueWhite); }
		}

		static public Color Aqua {
			get { return KnownColors.Get(KnownColor.Aqua); }
		}

		static public Color Aquamarine {
			get { return KnownColors.Get(KnownColor.Aquamarine); }
		}

		static public Color Azure {
			get { return KnownColors.Get(KnownColor.Azure); }
		}

		static public Color Beige {
			get { return KnownColors.Get(KnownColor.Beige); }
		}

		static public Color Bisque {
			get { return KnownColors.Get(KnownColor.Bisque); }
		}

		static public Color Black {
			get { return KnownColors.Get(KnownColor.Black); }
		}

		static public Color BlanchedAlmond {
			get { return KnownColors.Get(KnownColor.BlanchedAlmond); }
		}

		static public Color Blue {
			get { return KnownColors.Get(KnownColor.Blue); }
		}

		static public Color BlueViolet {
			get { return KnownColors.Get(KnownColor.BlueViolet); }
		}

		static public Color Brown {
			get { return KnownColors.Get(KnownColor.Brown); }
		}

		static public Color BurlyWood {
			get { return KnownColors.Get(KnownColor.BurlyWood); }
		}

		static public Color CadetBlue {
			get { return KnownColors.Get(KnownColor.CadetBlue); }
		}

		static public Color Chartreuse {
			get { return KnownColors.Get(KnownColor.Chartreuse); }
		}

		static public Color Chocolate {
			get { return KnownColors.Get(KnownColor.Chocolate); }
		}

		static public Color Coral {
			get { return KnownColors.Get(KnownColor.Coral); }
		}

		static public Color CornflowerBlue {
			get { return KnownColors.Get(KnownColor.CornflowerBlue); }
		}

		static public Color Cornsilk {
			get { return KnownColors.Get(KnownColor.Cornsilk); }
		}

		static public Color Crimson {
			get { return KnownColors.Get(KnownColor.Crimson); }
		}

		static public Color Cyan {
			get { return KnownColors.Get(KnownColor.Cyan); }
		}

		static public Color DarkBlue {
			get { return KnownColors.Get(KnownColor.DarkBlue); }
		}

		static public Color DarkCyan {
			get { return KnownColors.Get(KnownColor.DarkCyan); }
		}

		static public Color DarkGoldenrod {
			get { return KnownColors.Get(KnownColor.DarkGoldenrod); }
		}

		static public Color DarkGray {
			get { return KnownColors.Get(KnownColor.DarkGray); }
		}

		static public Color DarkGreen {
			get { return KnownColors.Get(KnownColor.DarkGreen); }
		}

		static public Color DarkKhaki {
			get { return KnownColors.Get(KnownColor.DarkKhaki); }
		}

		static public Color DarkMagenta {
			get { return KnownColors.Get(KnownColor.DarkMagenta); }
		}

		static public Color DarkOliveGreen {
			get { return KnownColors.Get(KnownColor.DarkOliveGreen); }
		}

		static public Color DarkOrange {
			get { return KnownColors.Get(KnownColor.DarkOrange); }
		}

		static public Color DarkOrchid {
			get { return KnownColors.Get(KnownColor.DarkOrchid); }
		}

		static public Color DarkRed {
			get { return KnownColors.Get(KnownColor.DarkRed); }
		}

		static public Color DarkSalmon {
			get { return KnownColors.Get(KnownColor.DarkSalmon); }
		}

		static public Color DarkSeaGreen {
			get { return KnownColors.Get(KnownColor.DarkSeaGreen); }
		}

		static public Color DarkSlateBlue {
			get { return KnownColors.Get(KnownColor.DarkSlateBlue); }
		}

		static public Color DarkSlateGray {
			get { return KnownColors.Get(KnownColor.DarkSlateGray); }
		}

		static public Color DarkTurquoise {
			get { return KnownColors.Get(KnownColor.DarkTurquoise); }
		}

		static public Color DarkViolet {
			get { return KnownColors.Get(KnownColor.DarkViolet); }
		}

		static public Color DeepPink {
			get { return KnownColors.Get(KnownColor.DeepPink); }
		}

		static public Color DeepSkyBlue {
			get { return KnownColors.Get(KnownColor.DeepSkyBlue); }
		}

		static public Color DimGray {
			get { return KnownColors.Get(KnownColor.DimGray); }
		}

		static public Color DodgerBlue {
			get { return KnownColors.Get(KnownColor.DodgerBlue); }
		}

		static public Color Firebrick {
			get { return KnownColors.Get(KnownColor.Firebrick); }
		}

		static public Color FloralWhite {
			get { return KnownColors.Get(KnownColor.FloralWhite); }
		}

		static public Color ForestGreen {
			get { return KnownColors.Get(KnownColor.ForestGreen); }
		}

		static public Color Fuchsia {
			get { return KnownColors.Get(KnownColor.Fuchsia); }
		}

		static public Color Gainsboro {
			get { return KnownColors.Get(KnownColor.Gainsboro); }
		}

		static public Color GhostWhite {
			get { return KnownColors.Get(KnownColor.GhostWhite); }
		}

		static public Color Gold {
			get { return KnownColors.Get(KnownColor.Gold); }
		}

		static public Color Goldenrod {
			get { return KnownColors.Get(KnownColor.Goldenrod); }
		}

		static public Color Gray {
			get { return KnownColors.Get(KnownColor.Gray); }
		}

		static public Color Green {
			get { return KnownColors.Get(KnownColor.Green); }
		}

		static public Color GreenYellow {
			get { return KnownColors.Get(KnownColor.GreenYellow); }
		}

		static public Color Honeydew {
			get { return KnownColors.Get(KnownColor.Honeydew); }
		}

		static public Color HotPink {
			get { return KnownColors.Get(KnownColor.HotPink); }
		}

		static public Color IndianRed {
			get { return KnownColors.Get(KnownColor.IndianRed); }
		}

		static public Color Indigo {
			get { return KnownColors.Get(KnownColor.Indigo); }
		}

		static public Color Ivory {
			get { return KnownColors.Get(KnownColor.Ivory); }
		}

		static public Color Khaki {
			get { return KnownColors.Get(KnownColor.Khaki); }
		}

		static public Color Lavender {
			get { return KnownColors.Get(KnownColor.Lavender); }
		}

		static public Color LavenderBlush {
			get { return KnownColors.Get(KnownColor.LavenderBlush); }
		}

		static public Color LawnGreen {
			get { return KnownColors.Get(KnownColor.LawnGreen); }
		}

		static public Color LemonChiffon {
			get { return KnownColors.Get(KnownColor.LemonChiffon); }
		}

		static public Color LightBlue {
			get { return KnownColors.Get(KnownColor.LightBlue); }
		}

		static public Color LightCoral {
			get { return KnownColors.Get(KnownColor.LightCoral); }
		}

		static public Color LightCyan {
			get { return KnownColors.Get(KnownColor.LightCyan); }
		}

		static public Color LightGoldenrodYellow {
			get { return KnownColors.Get(KnownColor.LightGoldenrodYellow); }
		}

		static public Color LightGreen {
			get { return KnownColors.Get(KnownColor.LightGreen); }
		}

		static public Color LightGray {
			get { return KnownColors.Get(KnownColor.LightGray); }
		}

		static public Color LightPink {
			get { return KnownColors.Get(KnownColor.LightPink); }
		}

		static public Color LightSalmon {
			get { return KnownColors.Get(KnownColor.LightSalmon); }
		}

		static public Color LightSeaGreen {
			get { return KnownColors.Get(KnownColor.LightSeaGreen); }
		}

		static public Color LightSkyBlue {
			get { return KnownColors.Get(KnownColor.LightSkyBlue); }
		}

		static public Color LightSlateGray {
			get { return KnownColors.Get(KnownColor.LightSlateGray); }
		}

		static public Color LightSteelBlue {
			get { return KnownColors.Get(KnownColor.LightSteelBlue); }
		}

		static public Color LightYellow {
			get { return KnownColors.Get(KnownColor.LightYellow); }
		}

		static public Color Lime {
			get { return KnownColors.Get(KnownColor.Lime); }
		}

		static public Color LimeGreen {
			get { return KnownColors.Get(KnownColor.LimeGreen); }
		}

		static public Color Linen {
			get { return KnownColors.Get(KnownColor.Linen); }
		}

		static public Color Magenta {
			get { return KnownColors.Get(KnownColor.Magenta); }
		}

		static public Color Maroon {
			get { return KnownColors.Get(KnownColor.Maroon); }
		}

		static public Color MediumAquamarine {
			get { return KnownColors.Get(KnownColor.MediumAquamarine); }
		}

		static public Color MediumBlue {
			get { return KnownColors.Get(KnownColor.MediumBlue); }
		}

		static public Color MediumOrchid {
			get { return KnownColors.Get(KnownColor.MediumOrchid); }
		}

		static public Color MediumPurple {
			get { return KnownColors.Get(KnownColor.MediumPurple); }
		}

		static public Color MediumSeaGreen {
			get { return KnownColors.Get(KnownColor.MediumSeaGreen); }
		}

		static public Color MediumSlateBlue {
			get { return KnownColors.Get(KnownColor.MediumSlateBlue); }
		}

		static public Color MediumSpringGreen {
			get { return KnownColors.Get(KnownColor.MediumSpringGreen); }
		}

		static public Color MediumTurquoise {
			get { return KnownColors.Get(KnownColor.MediumTurquoise); }
		}

		static public Color MediumVioletRed {
			get { return KnownColors.Get(KnownColor.MediumVioletRed); }
		}

		static public Color MidnightBlue {
			get { return KnownColors.Get(KnownColor.MidnightBlue); }
		}

		static public Color MintCream {
			get { return KnownColors.Get(KnownColor.MintCream); }
		}

		static public Color MistyRose {
			get { return KnownColors.Get(KnownColor.MistyRose); }
		}

		static public Color Moccasin {
			get { return KnownColors.Get(KnownColor.Moccasin); }
		}

		static public Color NavajoWhite {
			get { return KnownColors.Get(KnownColor.NavajoWhite); }
		}

		static public Color Navy {
			get { return KnownColors.Get(KnownColor.Navy); }
		}

		static public Color OldLace {
			get { return KnownColors.Get(KnownColor.OldLace); }
		}

		static public Color Olive {
			get { return KnownColors.Get(KnownColor.Olive); }
		}

		static public Color OliveDrab {
			get { return KnownColors.Get(KnownColor.OliveDrab); }
		}

		static public Color Orange {
			get { return KnownColors.Get(KnownColor.Orange); }
		}

		static public Color OrangeRed {
			get { return KnownColors.Get(KnownColor.OrangeRed); }
		}

		static public Color Orchid {
			get { return KnownColors.Get(KnownColor.Orchid); }
		}

		static public Color PaleGoldenrod {
			get { return KnownColors.Get(KnownColor.PaleGoldenrod); }
		}

		static public Color PaleGreen {
			get { return KnownColors.Get(KnownColor.PaleGreen); }
		}

		static public Color PaleTurquoise {
			get { return KnownColors.Get(KnownColor.PaleTurquoise); }
		}

		static public Color PaleVioletRed {
			get { return KnownColors.Get(KnownColor.PaleVioletRed); }
		}

		static public Color PapayaWhip {
			get { return KnownColors.Get(KnownColor.PapayaWhip); }
		}

		static public Color PeachPuff {
			get { return KnownColors.Get(KnownColor.PeachPuff); }
		}

		static public Color Peru {
			get { return KnownColors.Get(KnownColor.Peru); }
		}

		static public Color Pink {
			get { return KnownColors.Get(KnownColor.Pink); }
		}

		static public Color Plum {
			get { return KnownColors.Get(KnownColor.Plum); }
		}

		static public Color PowderBlue {
			get { return KnownColors.Get(KnownColor.PowderBlue); }
		}

		static public Color Purple {
			get { return KnownColors.Get(KnownColor.Purple); }
		}

		static public Color Red {
			get { return KnownColors.Get(KnownColor.Red); }
		}

		static public Color RosyBrown {
			get { return KnownColors.Get(KnownColor.RosyBrown); }
		}

		static public Color RoyalBlue {
			get { return KnownColors.Get(KnownColor.RoyalBlue); }
		}

		static public Color SaddleBrown {
			get { return KnownColors.Get(KnownColor.SaddleBrown); }
		}

		static public Color Salmon {
			get { return KnownColors.Get(KnownColor.Salmon); }
		}

		static public Color SandyBrown {
			get { return KnownColors.Get(KnownColor.SandyBrown); }
		}

		static public Color SeaGreen {
			get { return KnownColors.Get(KnownColor.SeaGreen); }
		}

		static public Color SeaShell {
			get { return KnownColors.Get(KnownColor.SeaShell); }
		}

		static public Color Sienna {
			get { return KnownColors.Get(KnownColor.Sienna); }
		}

		static public Color Silver {
			get { return KnownColors.Get(KnownColor.Silver); }
		}

		static public Color SkyBlue {
			get { return KnownColors.Get(KnownColor.SkyBlue); }
		}

		static public Color SlateBlue {
			get { return KnownColors.Get(KnownColor.SlateBlue); }
		}

		static public Color SlateGray {
			get { return KnownColors.Get(KnownColor.SlateGray); }
		}

		static public Color Snow {
			get { return KnownColors.Get(KnownColor.Snow); }
		}

		static public Color SpringGreen {
			get { return KnownColors.Get(KnownColor.SpringGreen); }
		}

		static public Color SteelBlue {
			get { return KnownColors.Get(KnownColor.SteelBlue); }
		}

		static public Color Tan {
			get { return KnownColors.Get(KnownColor.Tan); }
		}

		static public Color Teal {
			get { return KnownColors.Get(KnownColor.Teal); }
		}

		static public Color Thistle {
			get { return KnownColors.Get(KnownColor.Thistle); }
		}

		static public Color Tomato {
			get { return KnownColors.Get(KnownColor.Tomato); }
		}

		static public Color Turquoise {
			get { return KnownColors.Get(KnownColor.Turquoise); }
		}

		static public Color Violet {
			get { return KnownColors.Get(KnownColor.Violet); }
		}

		static public Color Wheat {
			get { return KnownColors.Get(KnownColor.Wheat); }
		}

		static public Color White {
			get { return KnownColors.Get(KnownColor.White); }
		}

		static public Color WhiteSmoke {
			get { return KnownColors.Get(KnownColor.WhiteSmoke); }
		}

		static public Color Yellow {
			get { return KnownColors.Get(KnownColor.Yellow); }
		}

		static public Color YellowGreen {
			get { return KnownColors.Get(KnownColor.YellowGreen); }
		}
	}
}

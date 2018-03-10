using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{

	[Serializable]
	[ComVisible (true)]
//	[Editor ("System.Drawing.Design.FontEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter (typeof (FontConverter))]
	public sealed partial class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable 
	{
	
		const byte DefaultCharSet = 1;

		float sizeInPoints = 0;
		GraphicsUnit unit = GraphicsUnit.Point;
		float size;
		bool underLine = false;
		bool strikeThrough = false;
		FontFamily fontFamily;
		FontStyle fontStyle;
		byte gdiCharSet = 1;
		bool  gdiVerticalFont;

		public Font (Font prototype, FontStyle newStyle)
			: this (prototype.FontFamily, prototype.size, newStyle, prototype.unit, prototype.gdiCharSet, prototype.gdiVerticalFont)
		{
		}

		public Font (FontFamily family, float emSize,  GraphicsUnit unit)
			: this (family, emSize, FontStyle.Regular, unit, DefaultCharSet, false)
		{
		}

		public Font (string familyName, float emSize,  GraphicsUnit unit)
			: this (new FontFamily (familyName, true), emSize, FontStyle.Regular, unit, DefaultCharSet, false)
		{
		}

		public Font (FontFamily family, float emSize)
			: this (family, emSize, FontStyle.Regular, GraphicsUnit.Point, DefaultCharSet, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style)
			: this (family, emSize, style, GraphicsUnit.Point, DefaultCharSet, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			: this (family, emSize, style, unit, DefaultCharSet, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
			: this (family, emSize, style, unit, gdiCharSet, false)
		{
		}

		public Font (string familyName, float emSize)
			: this (new FontFamily (familyName, true), emSize, FontStyle.Regular, GraphicsUnit.Point, DefaultCharSet, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style)
			: this (new FontFamily (familyName, true), emSize, style, GraphicsUnit.Point, DefaultCharSet, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style, GraphicsUnit unit)
			: this (new FontFamily (familyName, true), emSize, style, unit, DefaultCharSet, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
			: this (new FontFamily (familyName, true), emSize, style, unit, gdiCharSet, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style,
		           GraphicsUnit unit, byte gdiCharSet, bool  gdiVerticalFont)
			: this (new FontFamily (familyName, true), emSize, style, unit, gdiCharSet, gdiVerticalFont)
		{
		}

		public Font (FontFamily familyName, float emSize, FontStyle style,
		             GraphicsUnit unit, byte gdiCharSet, bool  gdiVerticalFont )
		{


			if (emSize <= 0)
				throw new ArgumentException("emSize is less than or equal to 0, evaluates to infinity, or is not a valid number.","emSize");

			fontFamily = familyName;
			fontStyle = style;
			this.gdiVerticalFont = gdiVerticalFont;
			this.gdiCharSet = gdiCharSet;

			CreateNativeFont (familyName, emSize, style, unit, gdiCharSet, gdiVerticalFont);
		}

		internal Font (string familyName, float emSize, string systemName)
			: this (familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, DefaultCharSet, false)
		{
		}

		private Font(SerializationInfo info, StreamingContext context)
			: this(
				(string)info.GetValue("Name", typeof(string)),
				(float)info.GetValue("Size", typeof(float)),
				(FontStyle)info.GetValue("Style", typeof(FontStyle)),
				(GraphicsUnit)info.GetValue("Unit", typeof(GraphicsUnit)))
		{
		}

		#region ISerializable implementation
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Name", Name);
			info.AddValue("Size", Size);
			info.AddValue("Style", Style);
			info.AddValue("Unit", Unit);
		}
		#endregion

		#region ICloneable implementation
		public object Clone ()
		{
			return new Font (fontFamily, size, fontStyle, unit, gdiCharSet, gdiVerticalFont);
		}
		#endregion

		#region IDisposable implementation
		~Font ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		internal void Dispose (bool disposing)
		{
			if (disposing){
				if (nativeFont != null){
					nativeFont.Dispose ();
					nativeFont = null;
				}
			}
		}
		#endregion

		public IntPtr ToHfont()
		{
			return nativeFont != null ? nativeFont.Handle : IntPtr.Zero;
		}

		public static Font FromHfont(IntPtr hfont)
		{
			throw new NotImplementedException ();
		}

		public static Font FromLogFont(object logFont)
		{
			throw new NotImplementedException ();
		}

		public void ToLogFont(object logFont)
		{
			throw new NotImplementedException ();
		}

		public void ToLogFont(object logFont, Graphics g)
		{
			throw new NotImplementedException ();
		}

		public float SizeInPoints 
		{
			get { return sizeInPoints; }
		}
		
		public GraphicsUnit Unit 
		{
			get { return unit; }
			
		}
		
		public float Size 
		{
			get { 
				return size; 
			}
			
		}

		public bool Bold 
		{ 
			get { return 0 != (fontStyle & FontStyle.Bold); }
		}

		public bool Italic
		{ 
			get { return 0 != (fontStyle & FontStyle.Italic); }
		}

		public bool Underline
		{ 
			get { return underLine; }
		}

		public bool Strikeout
		{ 
			get { return strikeThrough; }
		}

		public int Height {
			get { return (int)Math.Round (GetNativeheight ()); }
		}

		public FontFamily FontFamily
		{
			get { return fontFamily; }
		}

		public FontStyle Style 
		{ 
			get { return fontStyle; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Name
		{
			get { return fontFamily.Name; }
		}

		public float GetHeight()
		{
			return GetNativeheight();
		}

		public float GetHeight(Graphics g) 
		{
			return GetNativeheight ();
		}

		/// <devdoc>
		///     Returns the GDI char set for this instance of a font. This will only
		///     be valid if this font was created from a classic GDI font definition,
		///     like a LOGFONT or HFONT, or it was passed into the constructor.
		///
		///     This is here for compatability with native Win32 intrinsic controls
		///     on non-Unicode platforms.
		/// </devdoc>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public byte GdiCharSet
		{
			get
			{
				return gdiCharSet;
			}
		}

		[BrowsableAttribute(false)]
		public bool IsSystemFont
		{
			get
			{
				return !String.IsNullOrEmpty(SystemFontName);
			}
		}

		public String SystemFontName
		{
			get
			{
				if (this.Equals(SystemFonts.CaptionFont))
					return "CaptionFont";

				if (this.Equals(SystemFonts.DefaultFont))
					return "DefaultFont";

				if (this.Equals(SystemFonts.DialogFont))
					return "DialogFont";

				if (this.Equals(SystemFonts.IconTitleFont))
					return "IconTitleFont";

				if (this.Equals(SystemFonts.MenuFont))
					return "MenuFont";

				if (this.Equals(SystemFonts.MessageBoxFont))
					return "MessageBoxFont";

				if (this.Equals(SystemFonts.SmallCaptionFont))
					return "SmallCaptionFont";

				if (this.Equals(SystemFonts.StatusFont))
					return "StatusFont";

				return String.Empty;
			}
		}

		internal static void NotImplemented(System.Reflection.MethodBase method, object details = null)
		{
			System.Diagnostics.Debug.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name + (details == null ? String.Empty : " (" + details.ToString() + ")"));
		}

		public bool Equals(Font f)
		{
			return f != null
				&& this.Name == f.Name
				&& (int)Math.Round(10.0 * this.Size) == (int)Math.Round(10.0 * f.Size)
				&& this.Underline == f.Underline
				&& this.Bold == f.Bold
				&& this.Strikeout == f.Strikeout
				&& this.FontFamily.Name == f.FontFamily.Name
				&& this.Style == f.Style;
		}

		public override string ToString()
		{
			return $"{Name}, {SizeInPoints}pt, {Style}{(Strikeout ? ", Strikeout" : "")}, Height {Height}";
		}
	}
}


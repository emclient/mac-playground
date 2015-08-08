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

		static float dpiScale = 96f / 72f;

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

		#region ISerializable implementation
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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
			get { return bold; }
		}

		public bool Italic
		{ 
			get { return italic; }
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

		public float GetHeight(Graphics g) 
		{
			return GetNativeheight ();
		}
	}
}


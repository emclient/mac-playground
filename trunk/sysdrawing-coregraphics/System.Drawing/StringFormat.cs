//
// System.Drawing.StringFormat.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing {

	public sealed class StringFormat : MarshalByRefObject, IDisposable, ICloneable {


		StringFormatFlags formatFlags = 0;

		public StringFormat ()
		{
			Alignment = StringAlignment.Near;
		}

		public StringFormat (StringFormat format)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			Alignment = format.Alignment;
			LineAlignment = format.LineAlignment;
			FormatFlags = format.FormatFlags;
		}

		public StringFormat(StringFormatFlags options) : this()
		{
			formatFlags = options;
		}

		public StringFormat(StringFormatFlags options, int language) : this(options)
		{

		}

		~StringFormat ()
		{
			Dispose (false);
		}
		
		
		public StringAlignment Alignment {
			get; set;
		}

		public StringAlignment LineAlignment { get; set; }
				
		public StringTrimming Trimming { get; set; }

		public object Clone()
		{
			return new StringFormat (this);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
		}

		public static StringFormat GenericDefault
		{
			get {
				return new StringFormat () { FormatFlags = 0 };
			}
		}

		public static StringFormat GenericTypographic 
		{
			get {
				return new StringFormat () { FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit | StringFormatFlags.NoClip };
			}
		}


		public StringFormatFlags FormatFlags {
			get {				
				return formatFlags;
			}

			set {
				formatFlags = value;
			}
		}
		
  		public void SetMeasurableCharacterRanges (CharacterRange [] ranges)
		{					
			throw new NotImplementedException ();
		}
		
	}
}
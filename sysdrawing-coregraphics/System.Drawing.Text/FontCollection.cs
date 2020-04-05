//
// System.Drawing.Text.FontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//		Sanjay Gupta (gsanjay@novell.com)
//		Peter Dennis Bartok (pbartok@novell.com)
//
//
// Copyright (C) 2004 - 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.Drawing;

namespace System.Drawing.Text 
{

	public abstract partial class FontCollection : IDisposable 
	{
		
		internal FontCollection ()
		{
		}
        
		// methods
		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (nativeFontCollection != null) 
			{
				nativeFontCollection.Dispose();
				nativeFontCollection = null;
			}
		}

		// properties
		public FontFamily[] Families
		{
			get { 
				var families = new List<FontFamily> ();

				var familyNames = NativeFontFamilies ();

				// Lets sort the family names
				familyNames.Sort ();

				if (nativeFontCollection == null)
					throw new ArgumentException ("Collection was disposed or can not be created.");

				foreach (var family in familyNames) 
				{
					families.Add(new FontFamily (family));
				}
           
				return families.ToArray ();;               
			}
		}

		~FontCollection()
		{
			Dispose (false);
		}
	}
}

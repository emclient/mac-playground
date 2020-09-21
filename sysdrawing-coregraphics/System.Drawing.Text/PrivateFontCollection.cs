//
// System.Drawing.Text.PrivateFontCollection.cs
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
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System.Drawing.Text 
{

#if !NET_2_0
	[ComVisible(false)]
#endif
	public sealed partial class PrivateFontCollection : FontCollection 
	{

		// constructors
		public PrivateFontCollection ()
		{	}
		
		// methods
		public void AddFontFile (string filename) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			// note: MS throw the same exception FileNotFoundException if the file exists but isn't a valid font file
			LoadFontFile (filename);
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public void AddMemoryFont (IntPtr memory, int length) 
		{
			// note: MS throw FileNotFoundException if something is bad with the data (except for a null pointer)
		}

		// properties
		public new FontFamily[] Families
		{
			get { 
				var families = new List<FontFamily> ();

				var familyNames = new List<string>(nativeFontDescriptors.Keys);
				// Lets sort the family names
				familyNames.Sort ();

				foreach (var family in familyNames) 
				{
					families.Add(new FontFamily (family, this));
				}

				return families.ToArray ();;               
			}
		}

		// methods	
		protected override void Dispose (bool disposing)
		{
			if (nativeFontCollection != null) {
				nativeFontCollection = null;
			}
			
			base.Dispose (disposing);
		}		
	}
}

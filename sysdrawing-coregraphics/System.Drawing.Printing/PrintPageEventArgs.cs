//
// System.Drawing.PrintPageEventArgs.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//
// (C) 2002 Ximian, Inc
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing.Printing {

	public class PrintPageEventArgs : EventArgs {

		public PrintPageEventArgs (Graphics graphics, Rectangle marginBounds, Rectangle pageBounds, PageSettings pageSettings)
		{
			Graphics = graphics;
			MarginBounds = marginBounds;
			PageBounds = pageBounds;
			PageSettings = pageSettings;
		}

		public bool Cancel { get; set; }

		public Graphics Graphics { get; private set; }

		public bool HasMorePages { get; set; }

		public Rectangle MarginBounds { get; private set; }

		public Rectangle PageBounds { get; private set; }

		public PageSettings PageSettings { get; private set; }

		public bool CopySettingsToDevMode { get; internal set; }

		internal void SetGraphics(Graphics graphics)
		{
			this.Graphics = graphics;
		}

		internal void Dispose()
		{
			this.Graphics.Dispose();
			this.Graphics = null;
		}
	}
}
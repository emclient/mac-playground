//
// VisualStylesNative.cs: IVisualStyles that uses the Visual Styles feature of
// Windows XP and later.
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;
using System.Diagnostics;
namespace System.Windows.Forms.VisualStyles
{
	class VisualStylesUnsupported : IVisualStyles
	{
		public int UxThemeCloseThemeData (IntPtr hTheme) => throw new InvalidOperationException();
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds) => throw new InvalidOperationException();
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Rectangle clipRectangle) => throw new InvalidOperationException();
		public int UxThemeDrawThemeEdge (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects, out Rectangle result) => throw new InvalidOperationException();
		public int UxThemeDrawThemeParentBackground (IDeviceContext dc, Rectangle bounds, Control childControl) => throw new InvalidOperationException();
		public int UxThemeDrawThemeText (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string text, TextFormatFlags textFlags, Rectangle bounds) => throw new InvalidOperationException();
		public int UxThemeGetThemeBackgroundContentRect (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Rectangle result) => throw new InvalidOperationException();
		public int UxThemeGetThemeBackgroundExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle contentBounds, out Rectangle result) => throw new InvalidOperationException();
		public int UxThemeGetThemeBackgroundRegion (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Region result) => throw new InvalidOperationException();
		public int UxThemeGetThemeBool (IntPtr hTheme, int iPartId, int iStateId, BooleanProperty prop, out bool result) => throw new InvalidOperationException();
		public int UxThemeGetThemeColor (IntPtr hTheme, int iPartId, int iStateId, ColorProperty prop, out Color result) => throw new InvalidOperationException();
		public int UxThemeGetThemeEnumValue (IntPtr hTheme, int iPartId, int iStateId, EnumProperty prop, out int result) => throw new InvalidOperationException();
		public int UxThemeGetThemeFilename (IntPtr hTheme, int iPartId, int iStateId, FilenameProperty prop, out string result) => throw new InvalidOperationException();
		public int UxThemeGetThemeInt (IntPtr hTheme, int iPartId, int iStateId, IntegerProperty prop, out int result) => throw new InvalidOperationException();
		public int UxThemeGetThemeMargins (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, MarginProperty prop, out Padding result) => throw new InvalidOperationException();
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, ThemeSizeType type, out Size result) => throw new InvalidOperationException();
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, ThemeSizeType type, out Size result) => throw new InvalidOperationException();
		public int UxThemeGetThemePosition (IntPtr hTheme, int iPartId, int iStateId, PointProperty prop, out Point result) => throw new InvalidOperationException();
		public int UxThemeGetThemeString (IntPtr hTheme, int iPartId, int iStateId, StringProperty prop, out string result) => throw new InvalidOperationException();
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, Rectangle bounds, out Rectangle result) => throw new InvalidOperationException();
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, out Rectangle result) => throw new InvalidOperationException();
		public int UxThemeGetThemeTextMetrics (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, out TextMetrics result) => throw new InvalidOperationException();
		public int UxThemeHitTestThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, HitTestOptions options, Rectangle backgroundRectangle, IntPtr hrgn, Point pt, out HitTestCode result) => throw new InvalidOperationException();
		public bool UxThemeIsAppThemed () => false;
		public bool UxThemeIsThemeActive () => false;
		public bool UxThemeIsThemePartDefined (IntPtr hTheme, int iPartId) => false;
		public bool UxThemeIsThemeBackgroundPartiallyTransparent (IntPtr hTheme, int iPartId, int iStateId) => false;
		public IntPtr UxThemeOpenThemeData (IntPtr hWnd, string classList) => throw new InvalidOperationException();

		public string VisualStyleInformationAuthor => throw new InvalidOperationException();
		public string VisualStyleInformationColorScheme => throw new InvalidOperationException();
		public string VisualStyleInformationCompany => throw new InvalidOperationException();
		public Color VisualStyleInformationControlHighlightHot => throw new InvalidOperationException();
		public string VisualStyleInformationCopyright => throw new InvalidOperationException();
		public string VisualStyleInformationDescription => throw new InvalidOperationException();
		public string VisualStyleInformationDisplayName => throw new InvalidOperationException();
		public string VisualStyleInformationFileName => throw new InvalidOperationException();
		public bool VisualStyleInformationIsSupportedByOS => false;
		public int VisualStyleInformationMinimumColorDepth => throw new InvalidOperationException();
		public string VisualStyleInformationSize => throw new InvalidOperationException();
		public bool VisualStyleInformationSupportsFlatMenus => throw new InvalidOperationException();
		public Color VisualStyleInformationTextControlBorder => throw new InvalidOperationException();
		public string VisualStyleInformationUrl => throw new InvalidOperationException();
		public string VisualStyleInformationVersion => throw new InvalidOperationException();

		public void VisualStyleRendererDrawBackgroundExcludingArea (IntPtr theme, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle excludedArea) => throw new InvalidOperationException();
	}
}

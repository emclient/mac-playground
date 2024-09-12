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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


/// Win32 Version
namespace System.Windows.Forms {
	internal partial class XplatUIWin32 {
		#region Private Structs
		[StructLayout(LayoutKind.Sequential)]
		internal struct SIZE {
			internal int		width;
			internal int		height;

			public SIZE (int width, int height)
			{
				this.width = width;
				this.height = height;
			}

			public int Width {
				get { return width; }
				set { width = value; }
			}

			public int Height { 
				get { return height; }
				set { height = value; }
			}

			public Size ToSize() {
				return new Size (width, height);
			}

			public static SIZE FromSize (Size size) {
				return new SIZE (size.Width, size.Height);
			}
			public override int GetHashCode ()
			{
				return ((Width << 0x1a) | (Width >> 6)) ^ ((Height << 7) | (Height >> 0x19));
			}
			public override string ToString () {
				return String.Format("SIZE width={0}, height={1}", width, height);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT {
			internal int		left;
			internal int		top;
			internal int		right;
			internal int		bottom;

			public RECT (int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			#region Instance Properties
			public int Height { get { return bottom - top; } }
			public int Width { get { return right - left; } }
			public Size Size { get { return new Size (Width, Height); } }
			public Point Location { get { return new Point (left, top); } }
			#endregion

			#region Instance Methods
			public Rectangle ToRectangle ()
			{
				return Rectangle.FromLTRB (left, top, right, bottom);
			}

			public static RECT FromRectangle (Rectangle rectangle)
			{
				return new RECT (rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			public override int GetHashCode ()
			{
				return left ^ ((top << 13) | (top >> 0x13))
				  ^ ((Width << 0x1a) | (Width >> 6))
				  ^ ((Height << 7) | (Height >> 0x19));
			}
			
			public override string ToString ()
			{
				return String.Format("RECT left={0}, top={1}, right={2}, bottom={3}, width={4}, height={5}", left, top, right, bottom, right-left, bottom-top);
			}
			#endregion

			#region Operator overloads
			public static implicit operator Rectangle (RECT rect)
			{
				return Rectangle.FromLTRB (rect.left, rect.top, rect.right, rect.bottom);
			}

			public static implicit operator RECT (Rectangle rect)
			{
				return new RECT (rect.Left, rect.Top, rect.Right, rect.Bottom);
			}
			#endregion
		}

		internal enum SPIAction {
			SPI_GETACTIVEWINDOWTRACKING = 0x1000,
			SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,
			SPI_GETANIMATION = 0x0048,
			SPI_GETCARETWIDTH = 0x2006,
			SPI_GETCOMBOBOXANIMATION = 0x1004,
			SPI_GETDRAGFULLWINDOWS	= 0x0026,
			SPI_GETDROPSHADOW = 0x1024,
			SPI_GETFONTSMOOTHING = 0x004A,
			SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
			SPI_GETFONTSMOOTHINGTYPE = 0x200A,
			SPI_GETGRADIENTCAPTIONS = 0x1008,
			SPI_GETHOTTRACKING = 0x100E,
			SPI_GETICONTITLEWRAP = 0x0019,
			SPI_GETKEYBOARDSPEED = 0x000A,
			SPI_GETKEYBOARDDELAY	= 0x0016,
			SPI_GETKEYBOARDCUES		= 0x100A,
			SPI_GETKEYBOARDPREF = 0x0044,
			SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,
			SPI_GETMENUANIMATION = 0x1002,
			SPI_GETMENUDROPALIGNMENT = 0x001B,
			SPI_GETMENUFADE = 0x1012,
			SPI_GETMENUSHOWDELAY = 0x006A,
			SPI_GETMOUSESPEED = 0x0070,
			SPI_GETSELECTIONFADE = 0x1014,
			SPI_GETSNAPTODEFBUTTON = 0x005F,
			SPI_GETTOOLTIPANIMATION = 0x1016,
			SPI_GETWORKAREA = 0x0030,
			SPI_GETMOUSEHOVERWIDTH	= 0x0062,
			SPI_GETMOUSEHOVERHEIGHT	= 0x0064,
			SPI_GETMOUSEHOVERTIME	= 0x0066,
			SPI_GETUIEFFECTS = 0x103E,
			SPI_GETWHEELSCROLLLINES = 0x0068
		}

		internal enum WindowPlacementFlags {
			SW_HIDE			= 0,
			SW_SHOWNORMAL       	= 1,
			SW_NORMAL           	= 1,
			SW_SHOWMINIMIZED    	= 2,
			SW_SHOWMAXIMIZED    	= 3,
			SW_MAXIMIZE         	= 3,
			SW_SHOWNOACTIVATE   	= 4,
			SW_SHOW             	= 5,
			SW_MINIMIZE         	= 6,
			SW_SHOWMINNOACTIVE  	= 7,
			SW_SHOWNA           	= 8,
			SW_RESTORE          	= 9,
			SW_SHOWDEFAULT      	= 10,
			SW_FORCEMINIMIZE    	= 11,
			SW_MAX              	= 11
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT {
			internal uint			length;
			internal uint			flags;
			internal WindowPlacementFlags	showCmd;
			internal POINT			ptMinPosition;
			internal POINT			ptMaxPosition;
			internal RECT			rcNormalPosition;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NCCALCSIZE_PARAMS {
			internal RECT		rgrc1;
			internal RECT		rgrc2;
			internal RECT		rgrc3;
			internal IntPtr		lppos;
		}

		[Flags]
		private enum TMEFlags {
			TME_HOVER		= 0x00000001,
			TME_LEAVE		= 0x00000002,
			TME_NONCLIENT		= 0x00000010,
			TME_QUERY		= unchecked((int)0x40000000),
			TME_CANCEL		= unchecked((int)0x80000000)
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			internal int		size;
			internal TMEFlags	dwFlags;
			internal IntPtr		hWnd;
			internal int		dwHoverTime;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct PAINTSTRUCT {
			internal IntPtr		hdc;
			internal int		fErase;
			internal RECT		rcPaint;
			internal int		fRestore;
			internal int		fIncUpdate;
			internal int		Reserved1;
			internal int		Reserved2;
			internal int		Reserved3;
			internal int		Reserved4;
			internal int		Reserved5;
			internal int		Reserved6;
			internal int		Reserved7;
			internal int		Reserved8;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct KEYBDINPUT {
			internal short wVk;
			internal short wScan;
			internal Int32 dwFlags;
			internal Int32 time;
			internal UIntPtr dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MOUSEINPUT {
			internal Int32 dx;
			internal Int32 dy;
			internal Int32 mouseData;
			internal Int32 dwFlags;
			internal Int32 time;
			internal UIntPtr dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct HARDWAREINPUT {
			internal Int32 uMsg;
			internal short wParamL;
			internal short wParamH;
		}

		[StructLayout (LayoutKind.Sequential)]
		internal struct ICONINFO {
			internal bool fIcon;
			internal Int32 xHotspot;
			internal Int32 yHotspot;
			internal IntPtr hbmMask;
			internal IntPtr hbmColor;
		}    
		
		[StructLayout(LayoutKind.Explicit)]
		internal struct INPUT {
			[FieldOffset(0)]
			internal Int32 type;

			[FieldOffset(4)]
			internal MOUSEINPUT mi;

			[FieldOffset(4)]
			internal KEYBDINPUT ki;

			[FieldOffset(4)]
			internal HARDWAREINPUT hi;
		}

		[StructLayout (LayoutKind.Sequential)]
		public struct ANIMATIONINFO {
			internal uint cbSize;
			internal int iMinAnimate;
		}
		
		internal enum InputFlags {
			KEYEVENTF_EXTENDEDKEY	= 0x0001,
			KEYEVENTF_KEYUP			= 0x0002,
			KEYEVENTF_SCANCODE		= 0x0003,
			KEYEVENTF_UNICODE		= 0x0004,
		}

		internal enum ClassStyle {
			CS_VREDRAW			= 0x00000001,
			CS_HREDRAW			= 0x00000002,
			CS_KEYCVTWINDOW			= 0x00000004,
			CS_DBLCLKS			= 0x00000008,
			CS_OWNDC			= 0x00000020,
			CS_CLASSDC			= 0x00000040,
			CS_PARENTDC			= 0x00000080,
			CS_NOKEYCVT			= 0x00000100,
			CS_NOCLOSE			= 0x00000200,
			CS_SAVEBITS			= 0x00000800,
			CS_BYTEALIGNCLIENT		= 0x00001000,
			CS_BYTEALIGNWINDOW		= 0x00002000,
			CS_GLOBALCLASS			= 0x00004000,
			CS_IME				= 0x00010000,
			// Windows XP+
			CS_DROPSHADOW			= 0x00020000
		}

		internal enum SetWindowPosZOrder {
			HWND_TOP			= 0,
			HWND_BOTTOM			= 1,
			HWND_TOPMOST			= -1,
			HWND_NOTOPMOST			= -2
		}

		[Flags]
		internal enum SetWindowPosFlags {
			SWP_ASYNCWINDOWPOS		= 0x4000, 
			SWP_DEFERERASE			= 0x2000,
			SWP_DRAWFRAME			= 0x0020,
			SWP_FRAMECHANGED		= 0x0020,
			SWP_HIDEWINDOW			= 0x0080,
			SWP_NOACTIVATE			= 0x0010,
			SWP_NOCOPYBITS			= 0x0100,
			SWP_NOMOVE			= 0x0002,
			SWP_NOOWNERZORDER		= 0x0200,
			SWP_NOREDRAW			= 0x0008,
			SWP_NOREPOSITION		= 0x0200,
			SWP_NOENDSCHANGING		= 0x0400,
			SWP_NOSIZE			= 0x0001,
			SWP_NOZORDER			= 0x0004,
			SWP_SHOWWINDOW			= 0x0040
		}

		internal enum GetSysColorIndex {
			COLOR_SCROLLBAR			= 0,
			COLOR_BACKGROUND		= 1,
			COLOR_ACTIVECAPTION		= 2,
			COLOR_INACTIVECAPTION		= 3,
			COLOR_MENU			= 4,
			COLOR_WINDOW			= 5,
			COLOR_WINDOWFRAME		= 6,
			COLOR_MENUTEXT			= 7,
			COLOR_WINDOWTEXT		= 8,
			COLOR_CAPTIONTEXT		= 9,
			COLOR_ACTIVEBORDER		= 10,
			COLOR_INACTIVEBORDER		= 11,
			COLOR_APPWORKSPACE		= 12,
			COLOR_HIGHLIGHT			= 13,
			COLOR_HIGHLIGHTTEXT		= 14,
			COLOR_BTNFACE			= 15,
			COLOR_BTNSHADOW			= 16,
			COLOR_GRAYTEXT			= 17,
			COLOR_BTNTEXT			= 18,
			COLOR_INACTIVECAPTIONTEXT	= 19,
			COLOR_BTNHIGHLIGHT		= 20,
			COLOR_3DDKSHADOW		= 21,
			COLOR_3DLIGHT			= 22,
			COLOR_INFOTEXT			= 23,
			COLOR_INFOBK			= 24,
			
			COLOR_HOTLIGHT			= 26,
			COLOR_GRADIENTACTIVECAPTION	= 27,
			COLOR_GRADIENTINACTIVECAPTION	= 28,
			COLOR_MENUHIGHLIGHT		= 29,
			COLOR_MENUBAR			= 30,

			COLOR_DESKTOP			= 1,
			COLOR_3DFACE			= 16,
			COLOR_3DSHADOW			= 16,
			COLOR_3DHIGHLIGHT		= 20,
			COLOR_3DHILIGHT			= 20,
			COLOR_BTNHILIGHT		= 20,
			COLOR_MAXVALUE			= 24,/* Maximum value */
		}       

		private enum LoadCursorType {
			First				= 32512,
			IDC_ARROW			= 32512,
			IDC_IBEAM			= 32513,
			IDC_WAIT			= 32514,
			IDC_CROSS			= 32515,
			IDC_UPARROW			= 32516,
			IDC_SIZE			= 32640,
			IDC_ICON			= 32641,
			IDC_SIZENWSE			= 32642,
			IDC_SIZENESW			= 32643,
			IDC_SIZEWE			= 32644,
			IDC_SIZENS			= 32645,
			IDC_SIZEALL			= 32646,
			IDC_NO				= 32648,
			IDC_HAND			= 32649,
			IDC_APPSTARTING			= 32650,
			IDC_HELP			= 32651,
			Last				= 32651
		}

		private enum AncestorType {
			GA_PARENT = 1,
			GA_ROOT = 2, 
			GA_ROOTOWNER = 3
		}

		[Flags]
		private enum WindowLong {
			GWL_WNDPROC     		= -4,
			GWL_HINSTANCE			= -6,
			GWL_HWNDPARENT      		= -8,
			GWL_STYLE           		= -16,
			GWL_EXSTYLE         		= -20,
			GWL_USERDATA			= -21,
			GWL_ID				= -12
		}

		[Flags]
		private enum LogBrushStyle {
			BS_SOLID			= 0,
			BS_NULL             		= 1,
			BS_HATCHED          		= 2,
			BS_PATTERN          		= 3,
			BS_INDEXED          		= 4,
			BS_DIBPATTERN       		= 5,
			BS_DIBPATTERNPT     		= 6,
			BS_PATTERN8X8       		= 7,
			BS_DIBPATTERN8X8    		= 8,
			BS_MONOPATTERN      		= 9
		}

		[Flags]
		private enum LogBrushHatch {
			HS_HORIZONTAL			= 0,       /* ----- */
			HS_VERTICAL         		= 1,       /* ||||| */
			HS_FDIAGONAL        		= 2,       /* \\\\\ */
			HS_BDIAGONAL        		= 3,       /* ///// */
			HS_CROSS            		= 4,       /* +++++ */
			HS_DIAGCROSS        		= 5,       /* xxxxx */
		}

		internal struct COLORREF {
			internal byte			R;
			internal byte			G;
			internal byte			B;
			internal byte			A;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LOGBRUSH {
			internal LogBrushStyle		lbStyle;
			internal COLORREF		lbColor;
			internal LogBrushHatch		lbHatch;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct TEXTMETRIC { 
			internal int			tmHeight;
			internal int			tmAscent;
			internal int			tmDescent;
			internal int			tmInternalLeading;
			internal int			tmExternalLeading;
			internal int			tmAveCharWidth;
			internal int			tmMaxCharWidth;
			internal int			tmWeight;
			internal int			tmOverhang;
			internal int			tmDigitizedAspectX;
			internal int			tmDigitizedAspectY;
			internal short			tmFirstChar; 
			internal short			tmLastChar; 
			internal short			tmDefaultChar; 
			internal short			tmBreakChar; 
			internal byte			tmItalic; 
			internal byte			tmUnderlined; 
			internal byte			tmStruckOut; 
			internal byte			tmPitchAndFamily; 
			internal byte			tmCharSet; 
		}

		public enum TernaryRasterOperations : uint
		{
			SRCCOPY = 0x00CC0020,
			SRCPAINT = 0x00EE0086,
			SRCAND = 0x008800C6,
			SRCINVERT = 0x00660046,
			SRCERASE = 0x00440328,
			NOTSRCCOPY = 0x00330008,
			NOTSRCERASE = 0x001100A6,
			MERGECOPY = 0x00C000CA,
			MERGEPAINT = 0x00BB0226,
			PATCOPY = 0x00F00021,
			PATPAINT = 0x00FB0A09,
			PATINVERT = 0x005A0049,
			DSTINVERT = 0x00550009,
			BLACKNESS = 0x00000042,
			WHITENESS = 0x00FF0062
		}

		[Flags]
		private enum ScrollWindowExFlags {
			SW_NONE				= 0x0000,
			SW_SCROLLCHILDREN		= 0x0001,
			SW_INVALIDATE			= 0x0002,
			SW_ERASE			= 0x0004,
			SW_SMOOTHSCROLL			= 0x0010
		}

		internal enum SystemMetrics {
			SM_CXSCREEN			= 0,
			SM_CYSCREEN             	= 1,
			SM_CXVSCROLL            	= 2,
			SM_CYHSCROLL            	= 3,
			SM_CYCAPTION            	= 4,
			SM_CXBORDER             	= 5,
			SM_CYBORDER             	= 6,
			SM_CXDLGFRAME           	= 7,
			SM_CYDLGFRAME           	= 8,
			SM_CYVTHUMB             	= 9,
			SM_CXHTHUMB             	= 10,
			SM_CXICON               	= 11,
			SM_CYICON               	= 12,
			SM_CXCURSOR             	= 13,
			SM_CYCURSOR             	= 14,
			SM_CYMENU               	= 15,
			SM_CXFULLSCREEN         	= 16,
			SM_CYFULLSCREEN         	= 17,
			SM_CYKANJIWINDOW        	= 18,
			SM_MOUSEPRESENT         	= 19,
			SM_CYVSCROLL            	= 20,
			SM_CXHSCROLL            	= 21,
			SM_DEBUG                	= 22,
			SM_SWAPBUTTON           	= 23,
			SM_RESERVED1            	= 24,
			SM_RESERVED2            	= 25,
			SM_RESERVED3            	= 26,
			SM_RESERVED4            	= 27,
			SM_CXMIN                	= 28,
			SM_CYMIN                	= 29,
			SM_CXSIZE               	= 30,
			SM_CYSIZE               	= 31,
			SM_CXFRAME              	= 32,
			SM_CYFRAME              	= 33,
			SM_CXMINTRACK			= 34,
			SM_CYMINTRACK           	= 35,
			SM_CXDOUBLECLK          	= 36,
			SM_CYDOUBLECLK          	= 37,
			SM_CXICONSPACING        	= 38,
			SM_CYICONSPACING        	= 39,
			SM_MENUDROPALIGNMENT    	= 40,
			SM_PENWINDOWS           	= 41,
			SM_DBCSENABLED          	= 42,
			SM_CMOUSEBUTTONS        	= 43,
			SM_CXFIXEDFRAME			= SM_CXDLGFRAME,
			SM_CYFIXEDFRAME			= SM_CYDLGFRAME,
			SM_CXSIZEFRAME			= SM_CXFRAME,
			SM_CYSIZEFRAME			= SM_CYFRAME,
			SM_SECURE               	= 44,
			SM_CXEDGE               	= 45,
			SM_CYEDGE               	= 46,
			SM_CXMINSPACING         	= 47,
			SM_CYMINSPACING         	= 48,
			SM_CXSMICON             	= 49,
			SM_CYSMICON             	= 50,
			SM_CYSMCAPTION          	= 51,
			SM_CXSMSIZE             	= 52,
			SM_CYSMSIZE             	= 53,
			SM_CXMENUSIZE           	= 54,
			SM_CYMENUSIZE           	= 55,
			SM_ARRANGE              	= 56,
			SM_CXMINIMIZED          	= 57,
			SM_CYMINIMIZED          	= 58,
			SM_CXMAXTRACK           	= 59,
			SM_CYMAXTRACK           	= 60,
			SM_CXMAXIMIZED          	= 61,
			SM_CYMAXIMIZED          	= 62,
			SM_NETWORK              	= 63,
			SM_CLEANBOOT            	= 67,
			SM_CXDRAG               	= 68,
			SM_CYDRAG               	= 69,
			SM_SHOWSOUNDS           	= 70,
			SM_CXMENUCHECK          	= 71,
			SM_CYMENUCHECK          	= 72,
			SM_SLOWMACHINE          	= 73,
			SM_MIDEASTENABLED       	= 74,
			SM_MOUSEWHEELPRESENT    	= 75,
			SM_XVIRTUALSCREEN       	= 76,
			SM_YVIRTUALSCREEN       	= 77,
			SM_CXVIRTUALSCREEN      	= 78,
			SM_CYVIRTUALSCREEN      	= 79,
			SM_CMONITORS            	= 80,
			SM_SAMEDISPLAYFORMAT    	= 81,
			SM_IMMENABLED           	= 82,
			SM_CXFOCUSBORDER        	= 83,
			SM_CYFOCUSBORDER        	= 84,
			SM_TABLETPC             	= 86,
			SM_MEDIACENTER          	= 87,
			SM_CMETRICS             	= 88
		}

		// We'll only support _WIN32_IE < 0x0500 for now
		internal enum NotifyIconMessage {
			NIM_ADD				= 0x00000000,
			NIM_MODIFY			= 0x00000001,
			NIM_DELETE			= 0x00000002,
		}

		[Flags]
		internal enum NotifyIconFlags {
			NIF_MESSAGE			= 0x00000001,
			NIF_ICON			= 0x00000002,
			NIF_TIP				= 0x00000004,
			NIF_STATE			= 0x00000008,
			NIF_INFO			= 0x00000010			
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct NOTIFYICONDATA {
			internal uint				cbSize;
			internal IntPtr				hWnd;
			internal uint				uID;
			internal NotifyIconFlags	uFlags;
			internal uint				uCallbackMessage;
			internal IntPtr				hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			internal string				szTip;
			internal int				dwState;
			internal int				dwStateMask;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
			internal string				szInfo;
			internal int				uTimeoutOrVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
			internal string				szInfoTitle;
			internal ToolTipIcon		dwInfoFlags;
		}

		[Flags]
		internal enum DCExFlags {
			DCX_WINDOW			= 0x00000001,
			DCX_CACHE			= 0x00000002,
			DCX_NORESETATTRS     		= 0x00000004,
			DCX_CLIPCHILDREN     		= 0x00000008,
			DCX_CLIPSIBLINGS     		= 0x00000010,
			DCX_PARENTCLIP       		= 0x00000020,
			DCX_EXCLUDERGN       		= 0x00000040,
			DCX_INTERSECTRGN     		= 0x00000080,
			DCX_EXCLUDEUPDATE    		= 0x00000100,
			DCX_INTERSECTUPDATE  		= 0x00000200,
			DCX_LOCKWINDOWUPDATE 		= 0x00000400,
			DCX_USESTYLE			= 0x00010000,
			DCX_VALIDATE         		= 0x00200000
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct CLIENTCREATESTRUCT {
			internal IntPtr			hWindowMenu;
			internal uint			idFirstChild;
		}

		private enum ClassLong : int {
			GCL_MENUNAME			= -8,
			GCL_HBRBACKGROUND		= -10,
			GCL_HCURSOR         		= -12,
			GCL_HICON            		= -14,
			GCL_HMODULE          		= -16,
			GCL_CBWNDEXTRA       		= -18,
			GCL_CBCLSEXTRA       		= -20,
			GCL_WNDPROC          		= -24,
			GCL_STYLE            		= -26,
			GCW_ATOM             		= -32,
			GCL_HICONSM			= -34
		}

		[Flags]
		internal enum GAllocFlags : uint {
			GMEM_FIXED			= 0x0000,
			GMEM_MOVEABLE			= 0x0002,
			GMEM_NOCOMPACT			= 0x0010,
			GMEM_NODISCARD			= 0x0020,
			GMEM_ZEROINIT			= 0x0040,
			GMEM_MODIFY			= 0x0080,
			GMEM_DISCARDABLE		= 0x0100,
			GMEM_NOT_BANKED			= 0x1000,
			GMEM_SHARE          		= 0x2000,
			GMEM_DDESHARE			= 0x2000,
			GMEM_NOTIFY			= 0x4000,
			GMEM_LOWER			= GMEM_NOT_BANKED,
			GMEM_VALID_FLAGS		= 0x7F72,
			GMEM_INVALID_HANDLE 		= 0x8000,
			GHND                		= (GMEM_MOVEABLE | GMEM_ZEROINIT),
			GPTR                		= (GMEM_FIXED | GMEM_ZEROINIT)
		}

		internal enum ROP2DrawMode : int {
			R2_BLACK			= 1,
			R2_NOTMERGEPEN      		= 2,
			R2_MASKNOTPEN       		= 3,
			R2_NOTCOPYPEN       		= 4,
			R2_MASKPENNOT       		= 5,
			R2_NOT              		= 6,
			R2_XORPEN           		= 7,
			R2_NOTMASKPEN       		= 8,
			R2_MASKPEN          		= 9,
			R2_NOTXORPEN        		= 10,
			R2_NOP              		= 11,
			R2_MERGENOTPEN      		= 12,
			R2_COPYPEN          		= 13,
			R2_MERGEPENNOT      		= 14,
			R2_MERGEPEN         		= 15,
			R2_WHITE            		= 16,
			R2_LAST             		= 16
		}

		internal enum PenStyle : int {
			PS_SOLID			= 0,
			PS_DASH             		= 1,
			PS_DOT              		= 2,
			PS_DASHDOT          		= 3,
			PS_DASHDOTDOT       		= 4,
			PS_NULL             		= 5,
			PS_INSIDEFRAME      		= 6,
			PS_USERSTYLE        		= 7,
			PS_ALTERNATE        		= 8
		}

		internal enum PatBltRop : int {
			PATCOPY   = 0xf00021,
			PATINVERT = 0x5a0049,
			DSTINVERT = 0x550009,
			BLACKNESS = 0x000042,
			WHITENESS = 0xff0062,
		}

		internal enum StockObject : int {
			WHITE_BRUSH			= 0,
			LTGRAY_BRUSH        		= 1,
			GRAY_BRUSH          		= 2,
			DKGRAY_BRUSH        		= 3,
			BLACK_BRUSH         		= 4,
			NULL_BRUSH          		= 5,
			HOLLOW_BRUSH        		= NULL_BRUSH,
			WHITE_PEN   			= 6,
			BLACK_PEN           		= 7,
			NULL_PEN            		= 8,
			OEM_FIXED_FONT      		= 10,
			ANSI_FIXED_FONT     		= 11,
			ANSI_VAR_FONT       		= 12,
			SYSTEM_FONT         		= 13,
			DEVICE_DEFAULT_FONT 		= 14,
			DEFAULT_PALETTE     		= 15,
			SYSTEM_FIXED_FONT  		= 16
		}

		internal enum HatchStyle : int {
			HS_HORIZONTAL			= 0,
			HS_VERTICAL         		= 1,
			HS_FDIAGONAL        		= 2,
			HS_BDIAGONAL        		= 3,
			HS_CROSS            		= 4,
			HS_DIAGCROSS        		= 5
		}

		[Flags]
		internal enum SndFlags : int {
			SND_SYNC			= 0x0000,
			SND_ASYNC			= 0x0001,
			SND_NODEFAULT			= 0x0002,
			SND_MEMORY			= 0x0004,
			SND_LOOP			= 0x0008,
			SND_NOSTOP			= 0x0010,
			SND_NOWAIT     			= 0x00002000,
			SND_ALIAS			= 0x00010000,
			SND_ALIAS_ID			= 0x00110000,
			SND_FILENAME			= 0x00020000,
			SND_RESOURCE			= 0x00040004,
			SND_PURGE			= 0x0040,
			SND_APPLICATION			= 0x0080,
		}

		[Flags]
		internal enum LayeredWindowAttributes : int {
			LWA_COLORKEY		= 0x1,
			LWA_ALPHA			= 0x2,
		}

		public enum ACLineStatus : byte {
			Offline = 0,
			Online = 1,
			Unknown = 255
		}

		public enum BatteryFlag : byte {
			High = 1,
			Low = 2,
			Critical = 4,
			Charging = 8,
			NoSystemBattery = 128,
			Unknown = 255
		}

		[StructLayout (LayoutKind.Sequential)]
		public class SYSTEMPOWERSTATUS {
			public ACLineStatus _ACLineStatus;
			public BatteryFlag _BatteryFlag;
			public Byte _BatteryLifePercent;
			public Byte _Reserved1;
			public Int32 _BatteryLifeTime;
			public Int32 _BatteryFullLifeTime;
		}
		#endregion
	}
}

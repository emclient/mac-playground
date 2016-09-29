using System;

#if XAMARINMAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
#else
using MonoTouch.UIKit;
#endif

namespace System.Drawing {

	public sealed class SystemFonts {
		static SystemFonts ()
		{
		}

		public static Font GetFontByName (string systemFontName)
		{
			if (systemFontName == "CaptionFont")
				return CaptionFont;

			if (systemFontName == "DefaultFont")
				return DefaultFont;

			if (systemFontName == "DialogFont")
				return DialogFont;	

			if (systemFontName == "IconTitleFont")
				return IconTitleFont;

			if (systemFontName == "MenuFont")
				return MenuFont;

			if (systemFontName == "MessageBoxFont")
				return MessageBoxFont;

			if (systemFontName == "SmallCaptionFont")
				return SmallCaptionFont;

			if (systemFontName == "StatusFont")
				return StatusFont;			

			return null;
		}

		public static Font CaptionFont { 
			get { return new Font (NSFont.TitleBarFontOfSize(NSFont.SystemFontSize)); }
		}

		public static Font DefaultFont  { 
			get { return new Font (NSFont.LabelFontOfSize(NSFont.LabelFontSize)); }
		}

		public static Font DialogFont  { 
			get { return new Font (NSFont.LabelFontOfSize(NSFont.LabelFontSize)); }
		}

		public static Font IconTitleFont  { 
			get { return new Font (NSFont.LabelFontOfSize(NSFont.LabelFontSize)); }
		}

		public static Font MenuFont  { 
			get { return new Font (NSFont.MenuFontOfSize(NSFont.SystemFontSize)); }
		}

		public static Font MessageBoxFont  { 
			get { return new Font (NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize)); }
		}

		public static Font SmallCaptionFont  { 
			get { return new Font (NSFont.TitleBarFontOfSize(NSFont.SmallSystemFontSize)); }
		}

		public static Font StatusFont  { 
			get { return new Font (NSFont.LabelFontOfSize(NSFont.LabelFontSize)); }
		}
	}
}

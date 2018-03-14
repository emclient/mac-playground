using System;

#if XAMARINMAC
using CoreText;
using Foundation;
#elif MONOMAC
using MonoMac.CoreText;
using MonoMac.Foundation;
#else
using CoreText;
using Foundation;
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

		static Font captionFont;
		public static Font CaptionFont { 
			get {
				if (captionFont == null)
					captionFont = new Font(new CTFont(CTFontUIFontType.WindowTitle, 0, null));
				return captionFont;
			}
		}

		static Font defaultFont;
		public static Font DefaultFont  { 
			get {
				if (defaultFont == null)
					defaultFont = new Font(new CTFont(CTFontUIFontType.Label, 0, null));
				return defaultFont;
			}
		}

		static Font dialogFont;
		public static Font DialogFont  { 
			get
			{
				if (dialogFont == null)
					dialogFont = new Font(new CTFont(CTFontUIFontType.Label, 0, null));
				return dialogFont;
			}
		}

		static Font iconTitleFont;
		public static Font IconTitleFont  { 
			get
			{
				if (iconTitleFont == null)
					iconTitleFont = new Font(new CTFont(CTFontUIFontType.Label, 0, null));
				return iconTitleFont;
			}
		}

		static Font menuFont;
		public static Font MenuFont { 
			get
			{
				if (menuFont == null)
					menuFont = new Font(new CTFont(CTFontUIFontType.MenuItem, 0, null));
				return menuFont;
			}
		}

		static Font messageBoxFont;
		public static Font MessageBoxFont  { 
			get
			{
				if (messageBoxFont == null)
					messageBoxFont = new Font(new CTFont(CTFontUIFontType.System, 0, null));
				return messageBoxFont;
			}
		}

		static Font smallCaptionFont;
		public static Font SmallCaptionFont  { 
			get
			{
				if (smallCaptionFont == null)
					smallCaptionFont = new Font(new CTFont(CTFontUIFontType.UtilityWindowTitle, 0, null));
				return smallCaptionFont;
			}
		}

		static Font statusFont;
		public static Font StatusFont  { 
			get
			{
				if (statusFont == null)
					statusFont = new Font(new CTFont(CTFontUIFontType.Label, 0, null));
				return statusFont;
			}
		}
	}
}

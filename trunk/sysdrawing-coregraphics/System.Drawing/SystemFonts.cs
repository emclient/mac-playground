using System;

#if MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreText;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreText;
#endif

namespace System.Drawing {

	public sealed class SystemFonts {
		static readonly string messageFontFamily;
		static readonly string systemFontFamily;
		static readonly string menuFontFamily;
		static readonly string labelFontFamily;

		static SystemFonts ()
		{
			messageFontFamily = NSFont.MessageFontOfSize (NSFont.SystemFontSize).FamilyName;
			systemFontFamily = "Helvetica Neue"; //NSFont.SystemFontOfSize (NSFont.SystemFontSize).FamilyName;
			menuFontFamily = NSFont.MenuFontOfSize (NSFont.SystemFontSize).FamilyName;
			labelFontFamily = NSFont.LabelFontOfSize (NSFont.LabelFontSize).FamilyName;
		}

		private SystemFonts()
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
			get { return new Font (systemFontFamily, (float)NSFont.SmallSystemFontSize, "CaptionFont"); }
		}

		public static Font DefaultFont  { 
			get { return new Font (systemFontFamily, 9f, "DefaultFont"); }
		}

		public static Font DialogFont  { 
			get { return new Font (labelFontFamily, (float)NSFont.LabelFontSize, "DialogFont"); }
		}

		public static Font IconTitleFont  { 
			get { return new Font (systemFontFamily, (float)NSFont.SmallSystemFontSize, "IconTitleFont"); }
		}

		public static Font MenuFont  { 
			get { return new Font (menuFontFamily, (float)NSFont.SmallSystemFontSize, "MenuFont"); }
		}

		public static Font MessageBoxFont  { 
			get { return new Font (messageFontFamily, 9f, "MessageBoxFont"); }
		}

		public static Font SmallCaptionFont  { 
			get { return new Font (systemFontFamily, (float)NSFont.SmallSystemFontSize, "SmallCaptionFont"); }
		}

		public static Font StatusFont  { 
			get { return new Font (systemFontFamily, (float)NSFont.SmallSystemFontSize, "StatusFont"); }
		}	      
	}
}

using System;

#if XAMARINMAC
using AppKit;
using Foundation;
#elif MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
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

		static Font captionFont;
		public static Font CaptionFont { 
			get {
				if (captionFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { captionFont = new Font(NSFont.TitleBarFontOfSize(NSFont.SystemFontSize)); });
				return captionFont;
			}
		}

		static Font defaultFont;
		public static Font DefaultFont  { 
			get {
				if (defaultFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { defaultFont = new Font(NSFont.LabelFontOfSize(NSFont.SmallSystemFontSize)); });
				return defaultFont;
			}
		}

		static Font dialogFont;
		public static Font DialogFont  { 
			get
			{
				if (dialogFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { dialogFont = new Font(NSFont.LabelFontOfSize(NSFont.LabelFontSize)); });
				return dialogFont;
			}
		}

		static Font iconTitleFont;
		public static Font IconTitleFont  { 
			get
			{
				if (iconTitleFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { iconTitleFont = new Font(NSFont.LabelFontOfSize(NSFont.LabelFontSize)); });
				return iconTitleFont;
			}
		}

		static Font menuFont;
		public static Font MenuFont { 
			get
			{
				if (menuFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { menuFont = new Font(NSFont.MenuFontOfSize(NSFont.SystemFontSize)); });
				return menuFont;
			}
		}

		static Font messageBoxFont;
		public static Font MessageBoxFont  { 
			get
			{
				if (messageBoxFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { messageBoxFont = new Font(NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize)); });
				return messageBoxFont;
			}
		}

		static Font smallCaptionFont;
		public static Font SmallCaptionFont  { 
			get
			{
				if (smallCaptionFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { smallCaptionFont = new Font(NSFont.TitleBarFontOfSize(NSFont.SmallSystemFontSize)); });
				return smallCaptionFont;
			}
		}

		static Font statusFont;
		public static Font StatusFont  { 
			get
			{
				if (statusFont == null)
					NSApplication.SharedApplication.InvokeOnMainThread(() => { statusFont = new Font(NSFont.LabelFontOfSize(NSFont.LabelFontSize)); });
				return statusFont;
			}
		}
	}
}

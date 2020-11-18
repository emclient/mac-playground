using System;

#if XAMARINMAC
namespace AppKit
#elif MONOMAC 
namespace MonoMac.AppKit
#endif
{
#if MONOMAC || XAMARINMAC
	public enum NSKeyEx
	{
		GtLt = 0x0A, // The key placed on the left of the '1' key, on the Czech keyboard: [</>]

		F17 = 0x40, // Missing in Xamarin

		F18 = 0x4F,
		F19 = 0x50,
		F20 = 0x5A,
		F5 = 0x60,
		F6 = 0x61,
		F7 = 0x62,
		F3 = 0x63,
		F8 = 0x64,
		F9 = 0x65,
		F11 = 0x67,
		F13 = 0x69,
		F16 = 0x6A,
		F14 = 0x6B,
		F10 = 0x6D,
		F12 = 0x6F,
		F15 = 0x71,
		Help = 0x72,
		Home = 0x73,
		PageUp = 0x74,
		F4 = 0x76,
		End = 0x77,
		F2 = 0x78,
		PageDown = 0x79,
		F1 = 0x7A,
		LeftArrow = 0x7B,
		RightArrow = 0x7C,
		DownArrow = 0x7D,
		UpArrow = 0x7E
	}
}

#endif
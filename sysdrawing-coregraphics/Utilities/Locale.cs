using System;

namespace System.Drawing
{
	public class Locale
	{
		public static string GetText (string format)
		{
			return format;
		}
		
		public static string GetText (string format, params object [] args)
		{
			return string.Format (format, args);
		}
	}
}


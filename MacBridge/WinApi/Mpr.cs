using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WinApi
{
	public static partial class Mpr
	{
		public static int WNetGetConnection(
			string localName,
			StringBuilder remoteName,
			ref int length)
		{
			return Mpr.ERROR_NOT_CONNECTED;
		}			
	}
}

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WinApi
{
	public static partial class Win32
	{
		public static int RevokeDragDrop(IntPtr hwnd)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int OleGetClipboard(ref System.Runtime.InteropServices.ComTypes.IDataObject data)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}
	}
}


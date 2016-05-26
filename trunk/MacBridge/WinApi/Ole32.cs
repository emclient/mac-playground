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

		public static IStream CreateStreamOnHGlobal(IntPtr hGlobal, [MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return null;
		}

		public static void DoDragDrop(IDataObject dataObject, IDropSource dropSource, int allowedEffects, int[] finalEffect)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
		}

		public static int OleGetClipboard(ref System.Runtime.InteropServices.ComTypes.IDataObject data)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}
	}
}


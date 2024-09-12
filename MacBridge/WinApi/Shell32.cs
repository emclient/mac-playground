using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WinApi
{
    public static partial class Win32
    {
		public static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv)
		{
			// TODO
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags)
        {
            // TODO
            NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

        public static int SHQueryUserNotificationState(out int qns)
        {
            // TODO
            NotImplemented(MethodBase.GetCurrentMethod());
            qns = 0;
            return 0;
        }

        public static int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path)
        {
            // TODO
            NotImplemented(MethodBase.GetCurrentMethod());
            path = IntPtr.Zero;
            return 0;
        }

        public static IntPtr SHAppBarMessage(uint dwMessage, [In] ref APPBARDATA pData)
        {
            // TODO
            NotImplemented(MethodBase.GetCurrentMethod());
            return IntPtr.Zero;
        }

		public static int SHCreateStdEnumFmtEtc(uint cfmt, FORMATETC[] afmt, out IEnumFORMATETC ppenumFormatEtc)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			ppenumFormatEtc = null;
			return 0;
		}

		public static void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out()] out IntPtr pidl, uint sfgaoIn, [Out()] out uint psfgaoOut)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			pidl = IntPtr.Zero;
			psfgaoOut = 0;
		}

		public static int SHGetMalloc(out IMalloc ppMalloc)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			ppMalloc = null;
			return 0;
		}

		public static void SHBindToParent([In] IntPtr pidl, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv, out IntPtr ppidlLast)
		{
			ppv = null;
			ppidlLast = IntPtr.Zero;
			NotImplemented(MethodBase.GetCurrentMethod());
		}
	}
}


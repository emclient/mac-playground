using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinApi
{
    public static partial class Win32
    {
        public static IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags)
        {
            // TODO
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }

        public static int SHQueryUserNotificationState(out int qns)
        {
            // TODO
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            qns = 0;
            return 0;
        }

        public static int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path)
        {
            // TODO
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            path = IntPtr.Zero;
            return 0;
        }

        public static IntPtr SHAppBarMessage(uint dwMessage, [In] ref APPBARDATA pData)
        {
            // TODO
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return IntPtr.Zero;
        }
    }
}


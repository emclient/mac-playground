using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WinApi
{
    public static partial class Win32
    {
        public static void GetStartupInfo([In, Out] STARTUPINFO lpStartupInfo)
        {
            // TODO
            NotImplemented(MethodBase.GetCurrentMethod());
        }

		public static int WideCharToMultiByte(int codePage, int flags, string wideStr, int chars, byte[] pOutBytes, int bufferBytes, IntPtr defaultChar, IntPtr pDefaultUsed)
		{
			// TODO
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes)
		{
			// TODO
			NotImplemented(MethodBase.GetCurrentMethod());

			//NSDictionary* fileAttributes = [[NSFileManager defaultManager] attributesOfFileSystemForPath: @"/"
			//unsigned long long freeSpace = [[fileAttributes objectForKey: NSFileSystemFreeSize] longLongValue];
			//NSLog(@"free disk space: %dGB", (int)(freeSpace / 1073741824));

			lpFreeBytesAvailable = 0;
			lpTotalNumberOfBytes = 0;
			lpTotalNumberOfFreeBytes = 0;
			return false;
		}

		public static IntPtr GlobalReAlloc(HandleRef handle, IntPtr bytes, int flags)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static IntPtr GlobalLock(IntPtr hMem)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static bool GlobalUnlock(IntPtr hMem)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return true;
		}

		public static IntPtr GlobalSize(IntPtr handle)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static int GetCurrentThreadId()
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int GetLongPathName(string path, StringBuilder pszPath, int cchPath)
		{
			pszPath.Append(path);
			return path.Length;
		}
	}
}

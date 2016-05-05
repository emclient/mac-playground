using System;
using System.Reflection;
using System.Runtime.InteropServices;

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
	}
}

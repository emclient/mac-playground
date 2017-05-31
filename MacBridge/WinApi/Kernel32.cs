using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WinApi
{
	public static partial class Win32
	{
		static Dictionary<IntPtr, UIntPtr> localHeap = new Dictionary<IntPtr, UIntPtr>();
		static Dictionary<IntPtr, UIntPtr> globalHeap = new Dictionary<IntPtr, UIntPtr>();

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

		public static IntPtr LocalAlloc(uint uFlags, UIntPtr uBytes)
		{
			IntPtr ptr = Marshal.AllocHGlobal((int)uBytes.ToUInt32());
			localHeap[ptr] = uBytes;
			return ptr;
		}

		public static IntPtr LocalReAlloc(IntPtr hMem, IntPtr bytes, int flags)
		{
			if (localHeap.ContainsKey(hMem))
			{
				localHeap.Remove((hMem));
				hMem = Marshal.ReAllocHGlobal(hMem, bytes);
				localHeap[hMem] = new UIntPtr((ulong)bytes);

				if (0 != (flags & Win32.GMEM_ZEROINIT))
				{
					//FIXME
				}
				return hMem;
			}
			return IntPtr.Zero;
		}

		public static IntPtr LocalFree(IntPtr hMem)
		{
			return localHeap.ContainsKey(hMem) ? IntPtr.Zero : hMem;
		}

		public static UIntPtr LocalSize(IntPtr hMem)
		{
			return localHeap.TryGetValue(hMem, out UIntPtr length) ? length : UIntPtr.Zero;
		}

		public static IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes)
		{
			IntPtr ptr = Marshal.AllocHGlobal((int)dwBytes.ToUInt32());
			globalHeap[ptr] = dwBytes;
			return ptr;
		}

		public static IntPtr GlobalReAlloc(IntPtr hMem, IntPtr bytes, int flags)
		{
			if (globalHeap.ContainsKey(hMem))
			{
				globalHeap.Remove((hMem));
				hMem = Marshal.ReAllocHGlobal(hMem, bytes);
				globalHeap[hMem] = new UIntPtr((ulong)bytes);

				if (0 != (flags & Win32.GMEM_ZEROINIT))
				{
					//FIXME
				}
				return hMem;
			}
			return IntPtr.Zero;
		}

		public static IntPtr GlobalFree(IntPtr hMem)
		{
			return globalHeap.ContainsKey(hMem) ? IntPtr.Zero : hMem;
		}

		public static UIntPtr GlobalSize(IntPtr hMem)
		{
			return globalHeap.TryGetValue(hMem, out UIntPtr length) ? length : UIntPtr.Zero;
		}

		public static IntPtr GlobalLock(IntPtr hMem)
		{
			return hMem;
		}

		public static bool GlobalUnlock(IntPtr hMem)
		{
			return true;
		}
	}
}

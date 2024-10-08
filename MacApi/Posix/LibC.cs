﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MacApi.Posix
{
    public static class LibC
    {
        #region Helpers

        public static UInt64 GetMaxNumberOfOpenFiles()
        {
            var limit = new rlimit();
            getrlimit((int)RLimit.NoFile, ref limit);
            return limit.cur;
        }

		public static bool SetMaxNumberOfOpenFiles(UInt64 value)
		{
			var limit = new rlimit();
			getrlimit((int)RLimit.NoFile, ref limit);
			limit.cur = value;
			return 0 != setrlimit((int)RLimit.NoFile, ref limit);
		}

		public static void SetMaxNumberOfOpenFilesClosestTo(UInt64 value)
		{
			var limit = new rlimit();
			getrlimit((int)RLimit.NoFile, ref limit);

			UInt64 min = limit.cur, max = value, mid = 0;
			while (min <= max)
			{
				mid = (min + max) / 2;
				limit.cur = mid;
				if (0 == setrlimit((int)RLimit.NoFile, ref limit))
					min = 1 + mid;
				else
					max = mid - 1;
			}
		}

		public class Dup2 : IDisposable
		{
			IntPtr file;
			int dup;

			public Dup2(string path, FileNo fno, string mode)
			{
				file = fopen(path, mode);
				if (file != IntPtr.Zero)
				{
					var desc = fileno(file);
					if (desc != -1)
						dup = dup2(desc, (int)fno);
				}
			}

			public void Dispose()
			{
				if (file != IntPtr.Zero)
					fflush(file);

				if (dup != -1) 
				{
					close(dup);
					dup = -1;
				}

				if (file != IntPtr.Zero)
				{
					fclose(file);
					file = IntPtr.Zero;
				} 
			}
		}

		unsafe public static string GetCPUBrand(string defaultValue = "N/A")
		{
			const string key = "machdep.cpu.brand_string";
			var size = IntPtr.Zero;
			if (sysctlbyname(key, IntPtr.Zero, ref size, IntPtr.Zero, IntPtr.Zero) >= 0 && size.ToInt32() > 0)
				fixed (byte* ptr = new byte[size.ToInt32()])
					if (sysctlbyname(key, new IntPtr(ptr), ref size, IntPtr.Zero, IntPtr.Zero) >= 0 && size.ToInt32() > 0)
						return Encoding.ASCII.GetString(ptr, size.ToInt32() - 1);
			return defaultValue;
		}

		#endregion

		#region Native wrappers

		const UInt64 RLimInfinity = (1ul << 63) - 1; // no limit

        public enum RLimit
        {
            Cpu = 0,      // cpu time per process
            FSize = 1,    // file size 
            Data = 2,     // data segment size
            Stack = 3,    // stack size
            Core = 4,     // core file size
            AS = 5,       // address space (resident set size)
            MemLock = 6,  // locked-in-memory address space 
            NProc = 7,    // number of processes
            NoFile = 8,   // number of open files
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct rlimit
        {
            public UInt64 cur;
            public UInt64 max;
        }

		[DllImport(Constants.libcLibrary, SetLastError = true)]
        public static extern int getrlimit(int resource, ref rlimit l);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
        public static extern int setrlimit(int resource, ref rlimit l);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
        public static extern int getpid();

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int waitpid(int pid, int[] status, int options);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		static extern int sysctlbyname(string name, IntPtr buffer, ref IntPtr buflen, IntPtr newp, IntPtr newlen);

		[Flags]
		public enum FileMode
		{
			/* Read, write, execute/search by owner */
			S_IRWXU = 0000700,      /* [XSI] RWX mask for owner */
			S_IRUSR = 0000400,      /* [XSI] R for owner */
			S_IWUSR = 0000200,      /* [XSI] W for owner */
			S_IXUSR = 0000100,      /* [XSI] X for owner */
									/* Read, write, execute/search by group */
			S_IRWXG = 0000070,      /* [XSI] RWX mask for group */
			S_IRGRP = 0000040,      /* [XSI] R for group */
			S_IWGRP = 0000020,      /* [XSI] W for group */
			S_IXGRP = 0000010,      /* [XSI] X for group */
									/* Read, write, execute/search by others */
			S_IRWXO = 0000007,      /* [XSI] RWX mask for other */
			S_IROTH = 0000004,      /* [XSI] R for other */
			S_IWOTH = 0000002,      /* [XSI] W for other */
			S_IXOTH = 0000001,      /* [XSI] X for other */

			S_ISUID = 0004000,      /* [XSI] set user id on execution */
			S_ISGID = 0002000,      /* [XSI] set group id on execution */
			S_ISVTX = 0001000,      /* [XSI] directory restrcted delete */

			S_ISTXT	 = S_ISVTX,		/* sticky bit: not supported */
			S_IREAD	 = S_IRUSR,		/* backward compatability */
			S_IWRITE = S_IWUSR,		/* backward compatability */
			S_IEXEC	 = S_IXUSR,		/* backward compatability */
		}

		public enum SeekFlags : int
		{
			SEEK_SET = 0,
			SEEK_CUR = 1,
			SEEK_END = 2
		}

		public enum FileNo {
			StdIn = 0,
			StdOut = 1,
			StdErr = 2
		}

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern IntPtr fopen(string path, string mode);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int fclose(IntPtr stream);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int fseek(IntPtr stream, long offset, SeekFlags origin);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern ulong fread(byte[] ptr, ulong size, ulong nmemb, IntPtr stream);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern ulong fwrite(byte[] ptr, ulong size, ulong nmemb, IntPtr stream);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int fflush(IntPtr stream);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int open(string path, FileMode mode);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int close(int path);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int fileno(IntPtr file);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern int dup2(int oldFd, int newFd);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern IntPtr fdopen(int fildes, string mode);

		[DllImport(Constants.libcLibrary, SetLastError = true)]
		public static extern IntPtr freopen(string path, string mode, IntPtr file);

		[DllImport(Constants.libcLibrary)]
		public static extern void exit(int status);

		#endregion

		#region pthreads


		[DllImport(Constants.libcLibrary)]
		public static extern long pthread_self();

		[DllImport(Constants.libcLibrary)]
		public static extern long pthread_mach_thread_np(long thread);

		#endregion
	}
}

using System;
using System.Runtime.InteropServices;

namespace MacBridge.Posix
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

        public static void SetMaxNumberOfOpenFiles(UInt64 value)
        {
            var limit = new rlimit();
            getrlimit((int)RLimit.NoFile, ref limit);
            limit.cur = value;
            setrlimit((int)RLimit.NoFile, ref limit);
        }

        #endregion

        #region Native wrappers

        const UInt64 RLimInfinity = (1ul << 63) - 1; // no limit

        enum RLimit
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

        #endregion
    }
}

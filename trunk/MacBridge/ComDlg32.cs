using System;
using System.Diagnostics;
using System.Reflection;

namespace WinApi
{
    public static partial class Win32
    {
        public static bool ChooseFont(IntPtr lpcf)
        {
            // TODO:
            Debug.WriteLine(NotImplemented + MethodBase.GetCurrentMethod().Name);
            return false;
        }
    }
}


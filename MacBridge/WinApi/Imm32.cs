using System;
using System.Runtime.InteropServices;
using System.Reflection;
#if XAMARINMAC
using Foundation;
#else
using MonoMac.Foundation;
#endif

namespace WinApi
{
	public static partial class Win32
	{
		public static IntPtr ImmGetContext(IntPtr hWnd)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static IntPtr ImmReleaseContext(IntPtr hWnd, IntPtr hIMC)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static IntPtr ImmAssociateContextEx(IntPtr hWnd, IntPtr hIMC, int dwFlags)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen)
		{
			// if lpBuf is null or dwBufLen is 0, only size is returned, but no data gets copied.

			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, uint[] lpBuf, int dwBufLen)
		{
			// if lpBuf is null or dwBufLen is 0, only size is returned, but no data gets copied.

			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int ImmSetCandidateWindow(IntPtr himc, ref CANDIDATEFORM lpCandidateForm)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		internal class ImmContex : NSObject
		{
			internal IntPtr hwnd = IntPtr.Zero;
		}
	}
}
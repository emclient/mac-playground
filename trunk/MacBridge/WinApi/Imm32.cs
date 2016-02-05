using System;
using System.Runtime.InteropServices;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		public static IntPtr ImmGetContext(IntPtr hWnd)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return IntPtr.Zero;
		}

		public static int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, uint[] lpBuf, int dwBufLen)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int ImmSetCandidateWindow(IntPtr himc, ref CANDIDATEFORM lpCandidateForm)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

	}
}
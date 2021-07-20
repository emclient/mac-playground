using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms.CocoaInternal;
using System.Text;
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
			return hWnd;
		}

		public static bool ImmGetOpenStatus(IntPtr hWnd)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
		}

		public static bool ImmSetOpenStatus(IntPtr hWnd, bool b)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return false;
		}

		public static IntPtr ImmReleaseContext(IntPtr hWnd, IntPtr hIMC)
		{
			// No action needed, because we're using hWnd as hIMC
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
			if (ObjCRuntime.Runtime.GetNSObject(hIMC) is MonoEditView view)
			{
				if (dwIndex == Win32.GCS_RESULTSTR || dwIndex == Win32.GCS_COMPSTR)
				{
					var str = dwIndex == Win32.GCS_RESULTSTR ? view.insertText : view.markedText.Value;

					if (lpBuf == null || dwBufLen == 0)
						return 2 * str.Length;

					var bytes = Encoding.Unicode.GetBytes(str);
					var count = Math.Min(bytes.Length, Math.Min(lpBuf.Length, dwBufLen));
					Array.Copy(bytes, lpBuf, count);
					return count;
				}

				if (view.markedText.Length == 0)
					return 0;

				if (dwIndex == Win32.GCS_COMPCLAUSE)
				{
					if (lpBuf != null && dwBufLen != 0)
						Debug.Assert(false, "Use ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, uint[] lpBuf, int dwBufLen) to get indexes.");
					return 2 * sizeof(uint);
				}

				if (dwIndex == Win32.GCS_COMPATTR)
				{
					var count = (int)view.markedText.Length;
					if (lpBuf != null && dwBufLen != 0)
						for (int i = 0; i < count; i++)
							lpBuf[i] = Win32.ATTR_INPUT;
					return count;
				}

				if (dwIndex == Win32.GCS_CURSORPOS)
					return (int)view.markedText.Length;
			}
			return 0;
		}

		public static int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, uint[] lpBuf, int dwBufLen)
		{
			// if lpBuf is null or dwBufLen is 0, only size is returned, but no data gets copied.

			if (ObjCRuntime.Runtime.GetNSObject(hIMC) is MonoEditView view)
			{
				if (view.markedText.Length == 0)
					return 0;

				if (dwIndex == Win32.GCS_COMPCLAUSE)
				{
					if (lpBuf != null && dwBufLen >= 2 * sizeof(uint))
					{
						lpBuf[0] = 0;
						lpBuf[1] = (uint)view.markedText.Length;
					}
					return 2 * sizeof(uint);
				}
			}
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

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinApi
{
	// Dummy base interface for CommonFileDialog coclasses
	public interface NativeCommonFileDialog
	{
	}

	// ---------------------------------------------------------
	// Coclass interfaces - designed to "look like" the object 
	// in the API, so that the 'new' operator can be used in a 
	// straightforward way. Behind the scenes, the C# compiler
	// morphs all 'new CoClass()' calls to 'new CoClassWrapper()'
	public class NativeFileOpenDialog : IFileOpenDialog
	{
		public NativeFileOpenDialog()
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, Win32.FDAP fdap)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie)
		{
			pdwCookie = 0;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void ClearClientData()
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void Close([MarshalAs(UnmanagedType.Error)] int hr)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi)
		{
			ppsi = null;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName)
		{
			pszName = null;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetFileTypeIndex(out uint piFileType)
		{
			piFileType = 0;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi)
		{
			ppsi = null;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetOptions(out Win32.FOS pfos)
		{
			pfos = new Win32.FOS();
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi)
		{
			ppsi = null;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetResults([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppenum)
		{
			ppenum = null;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsai)
		{
			ppsai = null;
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetClientGuid([In] ref Guid guid)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFileTypeIndex([In] uint iFileType)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFileTypes([In] uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] Win32.COMDLG_FILTERSPEC[] rgFilterSpec)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFileTypes([In] uint cFileTypes, [In] ref Win32.COMDLG_FILTERSPEC rgFilterSpec)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetOptions([In] Win32.FOS fos)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}

		public int Show([In] IntPtr parent)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public void Unadvise([In] uint dwCookie)
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}
	}

	// ---------------------------------------------------
	// .NET classes representing runtime callable wrappers
	public class FileOpenDialogRCW
	{
		public FileOpenDialogRCW()
		{
			Win32.NotImplemented(MethodBase.GetCurrentMethod());
		}
	}
}

using System;
using System.Reflection;

namespace WinApi
{
	public static partial class Win32
	{
		public static uint SetSecurityInfo(IntPtr handle, SE_OBJECT_TYPE ObjectType, Int32 SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static Boolean ConvertStringSecurityDescriptorToSecurityDescriptorW(String strSecurityDescriptor, UInt32 sDRevision, ref IntPtr securityDescriptor, ref UInt32 securityDescriptorSize)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return true;
		}

		public static Boolean GetSecurityDescriptorSacl(IntPtr pSecurityDescriptor, out IntPtr lpbSaclPresent, out IntPtr pSacl, out IntPtr lpbSaclDefaulted)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			lpbSaclPresent = IntPtr.Zero;
			pSacl = IntPtr.Zero;
			lpbSaclDefaulted = IntPtr.Zero;
			return true;
		}

		public static UInt32 SetNamedSecurityInfoW(String pObjectName, SE_OBJECT_TYPE objectType, Int32 securityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

	}
}

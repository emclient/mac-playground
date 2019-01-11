using System;
using System.Text;
using System.Reflection;
using MacApi;

namespace WinApi
{
	public static partial class Win32
	{
		public static bool GetUserNameEx(ExtendedNameFormat nameFormat, StringBuilder userName, ref uint userNameSize)
		{
			string value = null;
			switch (nameFormat)
			{
				case ExtendedNameFormat.NameDisplay:
					value = FoundationStatic.FullUserName;
					break;

				case ExtendedNameFormat.NameUnknown:
				case ExtendedNameFormat.NameFullyQualifiedDN:
				case ExtendedNameFormat.NameSamCompatible:
				case ExtendedNameFormat.NameUniqueId:
				case ExtendedNameFormat.NameCanonical:
				case ExtendedNameFormat.NameUserPrincipal:
				case ExtendedNameFormat.NameCanonicalEx:
				case ExtendedNameFormat.NameServicePrincipal:
				case ExtendedNameFormat.NameDnsDomain:
				default:
					NotImplemented(MethodBase.GetCurrentMethod());
					userNameSize = 0;
					return false;
			}

			userName.Append(value);
			userNameSize = (uint)value.Length;
			return true;
		}
	}
}


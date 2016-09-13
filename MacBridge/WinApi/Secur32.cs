using System;
using System.Text;
using System.Reflection;
using System.Security.Principal;
using MacBridge;

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

		public static uint AcquireCredentialsHandle(
			string pszPrincipal, //SEC_CHAR*
			string pszPackage, //SEC_CHAR* //"Kerberos","NTLM","Negotiative"
			int fCredentialUse,
			IntPtr PAuthenticationID,//_LUID AuthenticationID,//pvLogonID, //PLUID
			ref SEC_WINNT_AUTH_IDENTITY pAuthData,//PVOID
			int pGetKeyFn, //SEC_GET_KEY_FN
			IntPtr pvGetKeyArgument, //PVOID
			ref SECURITY_HANDLE phCredential, //SecHandle //PCtxtHandle ref
			ref SECURITY_INTEGER ptsExpiry) //PTimeStamp //TimeStamp ref
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static uint AcquireCredentialsHandle(
			string pszPrincipal, //SEC_CHAR*
			string pszPackage, //SEC_CHAR* //"Kerberos","NTLM","Negotiative"
			int fCredentialUse,
			IntPtr PAuthenticationID,//_LUID AuthenticationID,//pvLogonID, //PLUID
			IntPtr pAuthData,//PVOID
			int pGetKeyFn, //SEC_GET_KEY_FN
			IntPtr pvGetKeyArgument, //PVOID
			ref SECURITY_HANDLE phCredential, //SecHandle //PCtxtHandle ref
			ref SECURITY_INTEGER ptsExpiry) //PTimeStamp //TimeStamp ref
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static uint InitializeSecurityContext(
			ref SECURITY_HANDLE phCredential,//PCredHandle
			IntPtr phContext, //PCtxtHandle
			string pszTargetName,
			int fContextReq,
			int Reserved1,
			int TargetDataRep,
			IntPtr pInput, //PSecBufferDesc SecBufferDesc
			int Reserved2,
			out SECURITY_HANDLE phNewContext, //PCtxtHandle
			out SecBufferDesc pOutput, //PSecBufferDesc SecBufferDesc
			out uint pfContextAttr, //managed ulong == 64 bits!!!
			out SECURITY_INTEGER ptsExpiry) //PTimeStamp
		{
			NotImplemented(MethodBase.GetCurrentMethod());

			phNewContext = new SECURITY_HANDLE {};
			pOutput = new SecBufferDesc {};
			pfContextAttr = 0;
			ptsExpiry = new SECURITY_INTEGER {};

			return 0;
		}

		public static uint InitializeSecurityContext(
			ref SECURITY_HANDLE phCredential,//PCredHandle
			ref SECURITY_HANDLE phContext, //PCtxtHandle
			string pszTargetName,
			int fContextReq,
			int Reserved1,
			int TargetDataRep,
			ref SecBufferDesc SecBufferDesc, //PSecBufferDesc SecBufferDesc
			int Reserved2,
			out SECURITY_HANDLE phNewContext, //PCtxtHandle
			out SecBufferDesc pOutput, //PSecBufferDesc SecBufferDesc
			out uint pfContextAttr, //managed ulong == 64 bits!!!
			out SECURITY_INTEGER ptsExpiry) //PTimeStamp
		{
			NotImplemented(MethodBase.GetCurrentMethod());

			phNewContext = new SECURITY_HANDLE {};
			pOutput = new SecBufferDesc {};
			pfContextAttr = 0;
			ptsExpiry = new SECURITY_INTEGER {};

			return 0;
		}

		/*
		public static int AcceptSecurityContext(ref SECURITY_HANDLE phCredential,
												IntPtr phContext,
												ref SecBufferDesc pInput,
												uint fContextReq,
												uint TargetDataRep,
												out SECURITY_HANDLE phNewContext,
												out SecBufferDesc pOutput,
												out uint pfContextAttr,    //managed ulong == 64 bits!!!
												out SECURITY_INTEGER ptsTimeStamp);

		public static int AcceptSecurityContext(ref SECURITY_HANDLE phCredential,
												ref SECURITY_HANDLE phContext,
												ref SecBufferDesc pInput,
												uint fContextReq,
												uint TargetDataRep,
												out SECURITY_HANDLE phNewContext,
												out SecBufferDesc pOutput,
												out uint pfContextAttr,    //managed ulong == 64 bits!!!
												out SECURITY_INTEGER ptsTimeStamp);
		*/

		public static int ImpersonateSecurityContext(ref SECURITY_HANDLE phContext)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int QueryContextAttributes(ref SECURITY_HANDLE phContext,
														uint ulAttribute,
														out SecPkgContext_Sizes pContextAttributes)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			pContextAttributes = new SecPkgContext_Sizes {};
			return 0;
		}
		
		public static int EncryptMessage(ref SECURITY_HANDLE phContext,
												uint fQOP,        //managed ulong == 64 bits!!!
												ref SecBufferDesc pMessage,
												uint MessageSeqNo)    //managed ulong == 64 bits!!!
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}

		public static int DecryptMessage(ref SECURITY_HANDLE phContext,
												 ref SecBufferDesc pMessage,
												 uint MessageSeqNo,
												 out uint pfQOP)
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			pfQOP = 0;
			return 0;
		}

		public static int MakeSignature(ref SECURITY_HANDLE phContext,          // Context to use
												uint fQOP,         // Quality of Protection
												ref SecBufferDesc pMessage,        // Message to sign
												uint MessageSeqNo)      // Message Sequence Num.
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			return 0;
		}


		public static int VerifySignature(ref SECURITY_HANDLE phContext,          // Context to use
												ref SecBufferDesc pMessage,        // Message to sign
												uint MessageSeqNo,            // Message Sequence Num.
												out uint pfQOP)      // Quality of Protection
		{
			NotImplemented(MethodBase.GetCurrentMethod());
			pfQOP = 0;
			return 0;
		}
	}
}


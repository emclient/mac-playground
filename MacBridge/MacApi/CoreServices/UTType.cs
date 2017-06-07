#if !XAMARINMAC

using System;
using System.Runtime.InteropServices;
using MonoMac.Foundation;

namespace MacBridge.LaunchServices
{
	public class UTType
	{
		public const string Item = "public.item";
		public const string PublicUtf8PlainText = "public.utf8-plain-text";
		public const string NSStringPboardType = "NSStringPboardType";

		public const string TagClassFilenameExtension = "public.filename-extension";

		public static string CreatePreferredIdentifier(string tagClass, string tag, string conformingToUti)
		{
			var a = NSString.CreateNative(tagClass);
			var b = NSString.CreateNative(tag);
			var c = NSString.CreateNative(conformingToUti);
			var ret = NSString.FromHandle(UTTypeCreatePreferredIdentifierForTag(a, b, c));
			NSString.ReleaseNative(a);
			NSString.ReleaseNative(b);
			NSString.ReleaseNative(c);
			return ret;
		}

		public static string GetPreferredTag(string uti, string tagClass)
		{
			if (uti == null)
				throw new ArgumentNullException(nameof(uti));
			if (tagClass == null)
				throw new ArgumentNullException(nameof(tagClass));

			var a = NSString.CreateNative(uti);
			var b = NSString.CreateNative(tagClass);
			var ret = NSString.FromHandle(UTTypeCopyPreferredTagWithClass(a, b));
			NSString.ReleaseNative(a);
			NSString.ReleaseNative(b);
			return ret;
		}

		[DllImport(Constants.CoreServicesLibrary)]
		extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
		[DllImport(Constants.CoreServicesLibrary)]
		extern static IntPtr UTTypeCopyPreferredTagWithClass(IntPtr uti, IntPtr tagClass);
	}
}

#endif // !XAMARINMAC

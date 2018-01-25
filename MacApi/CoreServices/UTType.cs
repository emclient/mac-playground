using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.Foundation;
#elif XAMARINMAC
using Foundation;
using MobileCoreServices;
#endif

namespace MacBridge.LaunchServices
{
 #if MONOMAC

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
 
#endif

	public class UTTypeEx
	{
		public const string kUTTagClassMIMEType = "public.mime-type";
		public const string kUTTagClassFilenameExtension = "public.filename-extension";
		public const string kApplicationOctetStream = "application/octet-stream";

		public static string MimeTypeFromExtension(string extension)
		{
			return UTTagFromTag(extension.StartsWith(".") ? extension.Substring(1) : extension, kUTTagClassFilenameExtension, kUTTagClassMIMEType, kApplicationOctetStream);
		}

		public static string ExtensionFromMimeType(string mimeType, string defaultValue = kApplicationOctetStream)
		{
			return UTTagFromTag(mimeType, kUTTagClassMIMEType, kUTTagClassFilenameExtension, defaultValue);
		}

		internal static string UTTagFromTag(string value, string sourceTag, string destinationTag, string defaultValue)
		{
			var uti = UTType.CreatePreferredIdentifier(sourceTag, value, null);
			if (uti != null)
				return UTType.GetPreferredTag(uti, destinationTag) ?? defaultValue;
			return defaultValue;
		}
	}
}

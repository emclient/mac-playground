using System;
using System.Runtime.InteropServices;
using CoreFoundation;
using Foundation;

namespace MacApi.LaunchServices
{
	public class UTType
	{
		private const string kApplicationOctetStream = "application/octet-stream";

		public static string MimeTypeFromExtension(string extension)
		{
			return UTTagFromTag(
				extension.StartsWith(".") ? extension.Substring(1) : extension,
				MobileCoreServices.UTType.TagClassFilenameExtension,
				MobileCoreServices.UTType.TagClassMIMEType,
				kApplicationOctetStream);
		}

		public static string ExtensionFromMimeType(string mimeType, string defaultValue = kApplicationOctetStream)
		{
			return UTTagFromTag(
				mimeType,
				MobileCoreServices.UTType.TagClassMIMEType,
				MobileCoreServices.UTType.TagClassFilenameExtension,
				defaultValue);
		}

		internal static string UTTagFromTag(string value, string sourceTag, string destinationTag, string defaultValue)
		{
			var uti = MobileCoreServices.UTType.CreatePreferredIdentifier(sourceTag, value, null);
			if (uti != null)
				return MobileCoreServices.UTType.GetPreferredTag(uti, destinationTag) ?? defaultValue;
			return defaultValue;
		}
	}
}

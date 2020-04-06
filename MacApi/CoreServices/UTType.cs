using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.Foundation;
#elif XAMARINMAC
using Foundation;
#endif

namespace MacApi.LaunchServices
{
	public class UTType
	{
		public const string kUTTypeData = "public.data";
		public const string kUTTypeUTF8PlainText = "public.utf8-plain-text";
		public const string kUTTypeText = "public.text";
		public const string kUTTypeEmailMessage = "public.email-message";
		public const string kUTTypeItem = "public.item";
		public const string kUTTypeImage = "public.image";
		public const string kUTTypeContent = "public.content";
		public const string kUTTypeAudio = "public.audio";
		public const string kUTTypeVideo = "public.video";
		public const string kUTTypeContact = "public.contact";
		public const string kUTTypeRTF = "public.rtf";
		public const string NSFilesPromisePboardType = "Apple files promise pasteboard type";
		public const string kUTTypePDF = "com.adobe.pdf";
		public const string kUTTypeVCard = "public.vcard";
		public const string kUTTypeToDoItem = "public.to-do-item";
		public const string kUTTypeCalendarEvent = "public.calendar-event";

		public const string kPasteboardTypeFileURLPromise = "com.apple.pasteboard.promised-file-url";
		public const string kPasteboardTypeFilePromiseContent = "com.apple.pasteboard.promised-file-content-type";
		public const string NSPasteboardTypeHTML = "public.html";
		public const string NSDocumentTypeDocumentAttribute = "DocumentType";
		public const string NSRTFTextDocumentType = "NSRTF";

		public const string kUTTagClassNSPboardType = "com.apple.nspboard-type";
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

		public static string CreatePreferredIdentifier(string tagClass, string tag, string conformingToUti)
		{
			return NSString.FromHandle(UTTypeCreatePreferredIdentifierForTag(Handle(tagClass), Handle(tag), Handle(conformingToUti)));
		}

		public static string GetPreferredTag(string uti, string tagClass)
		{
			if (uti == null)
				throw new ArgumentNullException(nameof(uti));
			if (tagClass == null)
				throw new ArgumentNullException(nameof(tagClass));

			return NSString.FromHandle(UTTypeCopyPreferredTagWithClass(((NSString)uti).Handle, ((NSString)tagClass).Handle));
		}

		internal static string UTTagFromTag(string value, string sourceTag, string destinationTag, string defaultValue)
		{
			var uti = UTType.CreatePreferredIdentifier(sourceTag, value, null);
			if (uti != null)
				return UTType.GetPreferredTag(uti, destinationTag) ?? defaultValue;
			return defaultValue;
		}

		internal static IntPtr Handle(string s)
		{
			return s != null ? ((NSString)s).Handle : IntPtr.Zero;
		}

		[DllImport(Constants.CoreServicesLibrary)]
		internal extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
		[DllImport(Constants.CoreServicesLibrary)]
		internal extern static IntPtr UTTypeCopyPreferredTagWithClass(IntPtr uti, IntPtr tagClass);

	}
}

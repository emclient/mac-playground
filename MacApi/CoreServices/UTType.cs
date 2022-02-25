using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CoreFoundation;
using CoreServices;
using Foundation;

namespace MacApi.CoreServices
{
	public partial class UTType
    {
        private string identifier;

        internal UTType(string identifier)
        {
            this.identifier = identifier;
        }

        public static UTType? CreateFromIdentifier(string identifier)
        {
            return string.IsNullOrEmpty(identifier) ? null : new UTType(identifier);
        }

        public static UTType? CreateFromExtension(string extension)
        {
            var identifier = CreatePreferredIdentifier(UTType.kUTTagClassFilenameExtension, extension, null);
            return identifier != null ? new UTType(identifier) : null;
        }

        public string Identifier => identifier;

        public bool ConformsTo(UTType type)
        {
            return UTTypeConformsTo(Handle(identifier), Handle(type.Identifier));
        }

        public string? PreferredExtension => GetPreferredTag(identifier!, UTType.kUTTagClassFilenameExtension);

        public string? PreferredMimeType => GetPreferredTag(identifier!, UTType.kUTTagClassMIMEType);

        public static implicit operator string(UTType type) => type.Identifier;

        // ? typeWithTag:tagClass:conformingToType:
		public static string? CreatePreferredIdentifier(string tagClass, string tag, string? conformingToUti)
		{
            var handle = UTTypeCreatePreferredIdentifierForTag(Handle(tagClass), Handle(tag), Handle(conformingToUti));
		    return handle != IntPtr.Zero ? CFString.FromHandle(handle) : null;
		}

		internal static string? GetPreferredTag(string identifier, string tagClass)
		{
            var handle = UTTypeCopyPreferredTagWithClass(Handle(identifier), Handle(tagClass));
			return NSString.FromHandle(handle);
		}

		internal static IntPtr Handle(string s)
		{
			return s != null ? ((NSString)s).Handle : IntPtr.Zero;
		}

        public override string ToString()
        {
            return $"identifier='{identifier}', prefMimeType:{PreferredMimeType}, prefExtension:{PreferredExtension}";
        }
    }

    public partial class UTType
    {
		[DllImport(Constants.CoreServicesLibrary)]
		internal extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
		[DllImport(Constants.CoreServicesLibrary)]
		internal extern static IntPtr UTTypeCopyPreferredTagWithClass(IntPtr uti, IntPtr tagClass);
		[DllImport(Constants.CoreServicesLibrary)]
        internal extern static bool UTTypeConformsTo(IntPtr uti, IntPtr conformsToUti);
    }

    public partial class UTType
    {
        static UTType? mimeType;
        public static UTType MimeType => mimeType ??= new UTType(kUTTagClassMIMEType);

        static UTType? filenameExtension;
        public static UTType FilenameExtension => filenameExtension ??= new UTType(kUTTagClassFilenameExtension);

        static UTType? pboardType;
        public static UTType NSPboardType => pboardType ??= new UTType(kUTTagClassNSPboardType);

        static UTType? data;
        public static UTType Data => data ??= new UTType(kUTTypeData);

        static UTType? pdf;
        public static UTType Pdf => pdf ??= new UTType(kUTTypePDF);

        static UTType? emailMessage;
        public static UTType EmailMessage => emailMessage ??= new UTType(kUTTypeEmailMessage);

        static UTType? vcard;
        public static UTType VCard => vcard ??= new UTType(kUTTypeVCard);

        static UTType? calendarEvent;
        public static UTType CalendarEvent => calendarEvent ??= new UTType(kUTTypeCalendarEvent);
    }

    public partial class UTType
    {
		internal const string kApplicationOctetStream = "application/octet-stream";

		internal const string kUTTagClassFilenameExtension = "public.filename-extension";
		internal const string kUTTagClassNSPboardType = "com.apple.nspboard-type";
		internal const string kUTTagClassMIMEType = "public.mime-type";

		internal const string kUTTypeData = "public.data";
		internal const string kUTTypeUTF8PlainText = "public.utf8-plain-text";
		internal const string kUTTypeText = "public.text";
		internal const string kUTTypeEmailMessage = "public.email-message";
		internal const string kUTTypeItem = "public.item";
		internal const string kUTTypeImage = "public.image";
		internal const string kUTTypeContent = "public.content";
		internal const string kUTTypeAudio = "public.audio";
		internal const string kUTTypeVideo = "public.video";
		internal const string kUTTypeContact = "public.contact";
		internal const string kUTTypeRTF = "public.rtf";
		internal const string NSFilesPromisePboardType = "Apple files promise pasteboard type";
		internal const string kUTTypePDF = "com.adobe.pdf";
		internal const string kUTTypeVCard = "public.vcard";
		internal const string kUTTypeToDoItem = "public.to-do-item";
		internal const string kUTTypeCalendarEvent = "public.calendar-event";
    }
}

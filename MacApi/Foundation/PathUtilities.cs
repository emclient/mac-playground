using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
#if XAMARINMAC
using Foundation;
#else
using MonoMac.Foundation;
#endif

namespace MacApi
{
	public static partial class FoundationStatic
	{
		public static string UserName
		{
			get { return NSString.FromHandle(NSUserName()); }
		}

		public static string FullUserName
		{
			get { return NSString.FromHandle(NSFullUserName()); }
		}

		public static string HomeDirectory
		{
			get { return NSString.FromHandle(NSHomeDirectory()); }
		}

		public static string HomeDirectoryForUser(string userName)
		{
			return NSString.FromHandle(NSHomeDirectoryForUser(new NSString(userName).Handle));
		}

		public static string TemporaryDirectory
		{
			get { return NSString.FromHandle(NSTemporaryDirectory()); }
		}

		public static string OpenStepRootDirectory
		{
			get { return NSString.FromHandle(NSOpenStepRootDirectory()); }
		}

		public static string LibraryDirectory
		{
			get { return FirstSearchPathFor(NSSearchPathDirectory.NSLibraryDirectory) ?? Path.Combine(HomeDirectory, "Library"); }
		}

		public static string GlobalLibraryDirectory
		{
			get { return FirstSearchPathFor(NSSearchPathDirectory.NSLibraryDirectory, NSSearchPathDomainMask.NSLocalDomainMask) ?? Path.Combine(OpenStepRootDirectory, "Library"); }
		}

		public static string CachesDirectory(bool shouldCreate = true)
		{
			var url = NSFileManager.DefaultManager.GetUrl(global::Foundation.NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User, null, shouldCreate, out NSError _);
			return url?.Path ?? Path.Combine(LibraryDirectory, "Caches");
		}

		public static string GlobalCachesDirectory(bool shouldCreate = true)
		{
			var url = NSFileManager.DefaultManager.GetUrl(global::Foundation.NSSearchPathDirectory.CachesDirectory,  NSSearchPathDomain.Local, null, shouldCreate, out NSError _);
			return url?.Path ?? CachesDirectory(shouldCreate);
		}

		public static string CookiesDirectory
		{
			get { return Path.Combine(LibraryDirectory, "Cookies"); }
		}

		public static string ApplicationSupportDirectory
		{
			get { return FirstSearchPathFor(NSSearchPathDirectory.NSApplicationSupportDirectory) ?? Path.Combine(LibraryDirectory, "Application Support"); }
		}

		internal static string FirstSearchPathFor(NSSearchPathDirectory directory, NSSearchPathDomainMask domainMask = NSSearchPathDomainMask.NSUserDomainMask, bool expandTilde = true)
		{
			var paths = SearchPathForDirectoriesInDomains(directory, domainMask, expandTilde);
			return paths.Length > 0 ? paths[0] : null;
		}

		public static string[] SearchPathForDirectoriesInDomains(NSSearchPathDirectory directory, NSSearchPathDomainMask domainMask, bool expandTilde)
		{
			var handle = NSSearchPathForDirectoriesInDomains(directory, domainMask, expandTilde);
			return NSArray.StringArrayFromHandle(handle);
		}

		[Serializable]
		public enum NSSearchPathDirectory : uint
		{
			NSApplicationDirectory = 1,				// supported applications (Applications)
			NSDemoApplicationDirectory = 2,			// unsupported applications, demonstration versions (Demos)
			NSDeveloperApplicationDirectory = 3,	// developer applications (Developer/Applications)
			NSAdminApplicationDirectory = 4,		// system and network administration applications (Administration)
			NSLibraryDirectory = 5, 				// various user-visible documentation, support, and configuration files, resources (Library)
			NSDeveloperDirectory = 6,				// developer resources (Developer)
			NSUserDirectory = 7,					// user home directories (Users)
			NSDocumentationDirectory = 8,			// documentation (Documentation)
			NSDocumentDirectory = 9,				// documents (Documents)
			NSCoreServiceDirectory = 10,			// locate of core services (System/Library/CoreServices)
			NSDesktopDirectory = 12,            	// location of user's desktop
			NSCachesDirectory = 13,             	// location of discardable cache files (Library/Caches)
			NSApplicationSupportDirectory = 14, 	// location of application support files (plug-ins, etc) (Library/Application Support)
			NSDownloadsDirectory = 15,          	// location of the user's "Downloads" directory
			NSAllApplicationsDirectory = 100,		// all directories where applications can occur
			NSAllLibrariesDirectory = 101,			// all directories where resources can occur
		}

		[Flags]
		[Serializable]
		public enum NSSearchPathDomainMask : uint
		{
			NSUserDomainMask = 1,					// user's home directory --- place to install user's personal items (~)
			NSLocalDomainMask = 2,					// local to the current machine --- place to install items available to everyone on this machine (/Library)
			NSNetworkDomainMask = 4, 				// publicly available location in the local area network --- place to install items available on the network (/Network)
			NSSystemDomainMask = 8,					// provided by Apple, unmodifiable (/System)
			NSAllDomainsMask = 0x0ffff,				// all domains: all of the above and future items
		}

		[DllImport(Dll)]
		internal extern static IntPtr NSUserName();

		[DllImport(Dll)]
		internal extern static IntPtr NSFullUserName();

		[DllImport(Dll)]
		internal extern static IntPtr NSHomeDirectory();

		[DllImport(Dll)]
		internal extern static IntPtr NSHomeDirectoryForUser(IntPtr userName);

		[DllImport(Dll)]
		internal extern static IntPtr NSTemporaryDirectory();

		[DllImport(Dll)]
		internal extern static IntPtr NSOpenStepRootDirectory();

		[DllImport(Dll)]
		internal extern static IntPtr NSSearchPathForDirectoriesInDomains(NSSearchPathDirectory directory, NSSearchPathDomainMask domainMask, [MarshalAs(UnmanagedType.U1)] bool expandTilde);
	}
}


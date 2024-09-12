using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CoreFoundation;
using Foundation;

namespace MacApi
{
	public static partial class FoundationStatic
	{
		public static string HomeDirectory => NSFileManager.HomeDirectory;

		public static string LibraryDirectory
		{
			get { return FirstSearchPathFor(NSSearchPathDirectory.LibraryDirectory) ?? Path.Combine(HomeDirectory, "Library"); }
		}

		public static string CachesDirectory(bool shouldCreate = true)
		{
			var url = NSFileManager.DefaultManager.GetUrl(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User, null, shouldCreate, out NSError _);
			return url?.Path ?? Path.Combine(LibraryDirectory, "Caches");
		}

		public static string CookiesDirectory
		{
			get { return Path.Combine(LibraryDirectory, "Cookies"); }
		}

		private static string? FirstSearchPathFor(NSSearchPathDirectory directory, NSSearchPathDomain domainMask = NSSearchPathDomain.User, bool expandTilde = true)
		{
			var paths = NSSearchPath.GetDirectories(directory, domainMask, expandTilde);
			return paths.Length > 0 ? paths[0] : null;
		}
	}
}


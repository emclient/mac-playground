using System;
using System.IO;
using Foundation;

namespace MacApi.Foundation
{
	public class NotificationCenterPreferences
	{
		public static NotificationCenterPreferences Shared { get; } = new NotificationCenterPreferences();
		public static string DefaultPath {  get { return Path.Combine(FoundationStatic.LibraryDirectory, "Preferences", "com.apple.ncprefs.plist"); } }

		public enum AlertStyle
		{
			None = 0 << 3,
			Banners = 1 << 3,
			Alerts = 2 << 3,
			Mask = 3 << 3
		}

		[Flags]
		public enum Options
		{
			DontShowInNotificationCenter = 1 << 0,
			BadgeAppIcon = 1 << 1,
			PlaySound = 1 << 2,
			DontShowOnLockScreen = 1 << 12,
			Mask = DontShowInNotificationCenter | BadgeAppIcon | PlaySound | DontShowOnLockScreen
		}

		public class Item 
		{
			public string BundleId { get; internal set; }
			public string Path { get; internal set; }
			public AlertStyle Style { get; internal set; }
			public Options Flags { get; internal set; }

			public override string ToString()
			{
				return $"bundle-id:{BundleId}, path:{Path}, style:{Style}, flags:{Flags}";
			}
		}

		protected string path;
		protected NSDictionary cache;
		protected long lastModifiedUtc = 0;

		public NotificationCenterPreferences(string path = null)
		{
			this.path = path ?? DefaultPath;
		}

		public virtual bool ReadPreferencesForBundleId(string bundleId, out Item prefs)
		{
			Item found = null;
			Iterate((id, path, style, flags) => {
				if (bundleId == id)
				{
					found = new Item { BundleId = id, Path = path, Style = style, Flags = flags};
					return false;
				}
				return true;
			});

			prefs = found;
			return found != null;
		}

		public delegate bool IterationDelegate(string bundleId, string path, AlertStyle style, Options options);

		public virtual void Iterate(IterationDelegate handler)
		{
			var plist = Read();
			var apps = (NSArray)plist["apps"];
			var shouldContinue = true;
			for (nuint i = 0; i < apps.Count && shouldContinue; ++i)
			{
				var item = apps.GetItem<NSDictionary>(i);
				var flagsItem = item["flags"];
				var flags = flagsItem != null ? ((NSNumber)flagsItem).UInt32Value : 0;
				shouldContinue = handler(item["bundle-id"]?.ToString(), item["path"]?.ToString(), (AlertStyle)(flags & (uint)AlertStyle.Mask), (Options)(flags) & Options.Mask);
			}
		}

		protected virtual NSDictionary Read()
		{
			var t = File.GetLastWriteTimeUtc(path).ToFileTimeUtc();
			if (cache == null || lastModifiedUtc < t)
			{
				lastModifiedUtc = t;
				cache = NSDictionary.FromFile(path);
			}
			return cache;
		}
	}
}

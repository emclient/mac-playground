#if MAC
using System;
using System.IO;
using Foundation;
using MacApi.CoreGraphics;
using NCP = MacApi.Foundation.NotificationCenterPreferences;
namespace FormsTest
{
	public partial class MainForm
	{
		const string emcBundleId = "com.emclient.mail.client";

		void ReadNotificationCenterPreferences()
		{
			var plistPath = Path.Combine(MacApi.FoundationStatic.LibraryDirectory, "Preferences", "com.apple.ncprefs.plist");
			var plist = NSDictionary.FromFile(plistPath);
			var apps = (NSArray)plist["apps"];
			for (nuint i = 0; i < apps.Count; ++i)
			{
				var d = apps.GetItem<NSDictionary>(i);
				var id = (NSString)d["bundle-id"];
				var path = (NSString)d["path"];
				var flags = ((NSNumber)d["flags"]).UInt32Value;

				if (id.ToString() == emcBundleId)
				{
					Console.WriteLine("Notification Center Preferences:");
					Console.WriteLine($"id:{id}");
					Console.WriteLine($"path:{path}");
					Console.WriteLine($"flags:{flags}, {flags:x}, {Convert.ToString(flags, 2).PadLeft(16, '0')}");
				}
			}

			if (NCP.Shared.ReadPreferencesForBundleId(emcBundleId, out NCP.Item prefs))
				Console.WriteLine(prefs);
		}

		void CheckIfScreenIsLocked()
		{
			Console.WriteLine($"Testing locked screen in 3 seconds...");
			var t = new System.Windows.Forms.Timer();
			t.Interval = 3000;
			t.Tick += (sender, e) => {
				t.Stop();
				var locked = Quartz.IsScreenLocked();
				Console.WriteLine($"IsScreenLocked:{locked}");
			};
			t.Start();
		}
	}
}
#endif

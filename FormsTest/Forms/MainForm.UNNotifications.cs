#if MAC
using System;
using System.Windows.Forms;
using Foundation;
using ObjCRuntime;
using UserNotifications;

namespace FormsTest
{
	public partial class MainForm
	{
		public void UNNotificationsTest()
		{
			Console.WriteLine("UNNotificationsTest");

			var center = UNUserNotificationCenter.Current;
			center.GetNotificationSettings((settings) => {
				Console.WriteLine($"GetNotificationSettings => {settings}");

				if (settings.AuthorizationStatus == UNAuthorizationStatus.NotDetermined)
				{
					var options = UNAuthorizationOptions.Badge | UNAuthorizationOptions.Alert;
					center.RequestAuthorization(options, (granted, error) => {
						Console.WriteLine($"RequestAuthorization => {granted}, {error}");
					});
				}
			});

		}
	}
}
#endif

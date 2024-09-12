using System;
using System.Threading.Tasks;
using Foundation;
using AppKit;

namespace MacApi.AppKit
{
	public static class NSWorkspaceEx
	{
		public static int LaunchApplicationForPath(string path, string[] args, bool activate = true)
		{
			var url = NSUrl.FromFilename(path);
			NSRunningApplication? application = null;
			NSError? error = null;

			if (OperatingSystem.IsMacOSVersionAtLeast(10, 15))
			{
				var tcs = new TaskCompletionSource();
				var configuration = NSWorkspaceOpenConfiguration.Create();
				configuration.Arguments = args;
				configuration.Activates = activate;
				NSWorkspace.SharedWorkspace.OpenUrl(
					url, configuration, (_application, _error) => {
						application = _application;
						error = _error;
						tcs.SetResult();
					});
				tcs.Task.GetAwaiter().GetResult();
			}
			else
			{
				var options = NSWorkspaceLaunchOptions.Default;
				var arguments = NSArray.FromStrings(args);
				var configuration = NSDictionary.FromObjectAndKey(arguments, NSWorkspace.LaunchConfigurationArguments);
				application = NSWorkspace.SharedWorkspace.OpenUrl(url, options, configuration, out error);
			}

			if (error != null)
				throw new ApplicationException("NSWorkspace failed to open URL: " + url);
			if (application == null)
				return 0;
			if (activate)
				application.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

			return application.ProcessIdentifier;
		}

		public static NSTask LaunchTaskForPath(string path, string[] args, bool activate = true)
		{
			var task = new NSTask();
			var pipe = new NSPipe();

			task.StandardOutput = pipe;
			task.LaunchPath = path;
			task.Arguments = args;
			task.Launch();

			if (activate)
			{
				NSRunningApplication app = null;
				for (int wait = 5000; wait > 0; wait -= 100)
				{
					if (app == null)
						app = NSRunningApplication.GetRunningApplication(task.ProcessIdentifier);

					if (app != null && app.FinishedLaunching)
						if (app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows))
							break;

					NSThread.SleepFor(0.1);
				}
			}

			return task;
		}
	}
}

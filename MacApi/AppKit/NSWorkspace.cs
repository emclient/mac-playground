using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Threading;
#if MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#elif XAMARINMAC
using Foundation;
using AppKit;
#endif

namespace MacBridge.AppKit
{
    public static class NSWorkspaceEx
    {
		readonly static NSString NSWorkspaceLaunchConfigurationArguments = (NSString)"NSWorkspaceLaunchConfigurationArguments"; // NSWorkspace.LaunchConfigurationArguments;

		public static int LaunchApplicationForPath(string path, string args, bool activate = true)
		{
			return LaunchApplicationForPath(path, SplitArgs(args), activate);
		}

		static int LaunchApplicationForPath(string path, string[] args, bool activate = true)
        {
            var options = new NSWorkspaceLaunchOptions();
			var arguments = NSArray.FromObjects(args);
			var configuration = NSDictionary.FromObjectAndKey(arguments, NSWorkspaceLaunchConfigurationArguments);
			var url = new NSUrl(path, false);
            
			NSError error = new NSError();
#if MONOMAC
            var app = NSWorkspace.SharedWorkspace.LaunchApplication(url, options, configuration, error);
#else
            var app = NSWorkspace.SharedWorkspace.OpenURL(url, options, configuration, out error);
#endif
            if (error != null)
                throw new ApplicationException("NSWorkspace failed to open URL: " + url);

            if (app == null)
                return 0;

            if (activate)
                app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            return app.ProcessIdentifier;
        }

		public static int LaunchAppAndWaitForExit(string path, string args, bool activate = true)
		{
			return LaunchAppAndWaitForExit(path, SplitArgs(args), activate);
		}

		static int LaunchAppAndWaitForExit(string path, string[] args, bool activate = true)
		{
			var pid = LaunchApplicationForPath(path, args, activate);
			return WaitForExit(pid);
		}

		public static int WaitForExit(int pid, double timeout = -1)
		{
			DateTime limit = timeout >= 0 ? DateTime.Now.AddSeconds(timeout) : DateTime.MaxValue;

			var process = System.Diagnostics.Process.GetProcessById(pid);
			while (process != null)
			{
				if (process.HasExited)
					return 0; // process.ExitCode; // process.ExitCode not implemented in MONO
				if (DateTime.Now >= limit)
					return 0;
				
				Thread.Sleep(10);
				process = System.Diagnostics.Process.GetProcessById(pid);
			}

			return 0;
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
				NSApplicationEx.Activate(task.ProcessIdentifier);

			return task;
		}

		public static int LaunchTaskAndWaitForExit(string path, string args)
		{
			return LaunchTaskAndWaitForExit(path, SplitArgs(args));
	
		}

		public static bool TerminateApplication(int pid, bool waitForExit = true)
		{
			var app = NSRunningApplication.GetRunningApplication(pid);
			if (app == null)
				return false;

			if (app.Terminate())
			{
				while (null != NSRunningApplication.GetRunningApplication(pid))
				{
				}
			}
			return true;
		}

		static int LaunchTaskAndWaitForExit(string path, string[] args)
		{
			var task = LaunchTaskForPath(path, args);
			task.WaitUntilExit();
			return task.TerminationStatus;
		}

		static Regex SplitArgsRegex = new Regex("(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

		static string[] SplitArgs(string args)
		{
			var split = SplitArgsRegex.Split(args);
			var list = new List<String>();
			foreach (var arg in split)
			{
				var unwrapped = Unwrap(arg);
				if (unwrapped.Length != 0)
					list.Add(unwrapped);
			}
			return list.ToArray();
		}

		static string Unwrap(string s, string prefix="\"", string suffix = "\"")
		{
			if (s.Length > prefix.Length+suffix.Length && s.StartsWith(prefix) && s.EndsWith(suffix))
			    return s.Substring(prefix.Length, s.Length - prefix.Length - suffix.Length);
			return s;
		}
    }
}

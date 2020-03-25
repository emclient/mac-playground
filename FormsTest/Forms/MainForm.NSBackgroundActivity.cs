#if MAC
using System;
using System.Windows.Forms;
using MacApi.Foundation;

namespace FormsTest
{
	public partial class MainForm : Form
	{
		int round = 5;
		int counter = 0;
		NSBackgroundActivityScheduler activity;

		void ToggleBgActivity()
		{
			if (activity != null)
			{
				Console.WriteLine($"Invalidating background activity");
				activity.Invalidate();
				activity = null;
				return;
			}

			Console.WriteLine($"Scheduling background activity");
			activity = new NSBackgroundActivityScheduler("com.emclient.FormsTest.BgActivity");
			activity.QualityOfService = NSBackgroundActivityScheduler.QoS.Utility;
			activity.Interval = 2.0;
			activity.Repeats = true;

			activity.Schedule((completion) => {
				Console.WriteLine($"Background activity - round:{counter}!");
				counter = (counter + 1) % round;
				completion(counter == 0 ? NSBackgroundActivityScheduler.Result.Finished : NSBackgroundActivityScheduler.Result.Deferred);
			});
		}
	}
}
#endif
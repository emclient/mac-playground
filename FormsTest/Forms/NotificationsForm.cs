#if __UNIFIED__
using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Foundation;
using UserNotifications;

namespace FormsTest
{
	partial class NotificationsForm : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		FlowLayoutPanel panel1;

		Controller controller;

		public NotificationsForm()
		{
			controller = new Controller(this);
			UNUserNotificationCenter.Current.Delegate = controller;

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			components = new Container();

			this.panel1 = new FlowLayoutPanel();
            this.SuspendLayout();

			// panel1
			panel1.SuspendLayout();
			panel1.AutoSize = true;
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Dock = DockStyle.Fill;
			panel1.Name = "panel1";
			panel1.FlowDirection = FlowDirection.TopDown;
			panel1.Padding = new Padding(1);

			AddButton("Print Settings", controller.PrintSettings);
			AddButton("Request Authorization", controller.RequestAuthorization);
			AddButton("Timer Notification", controller.AddTimerNotification);
			AddButton("Calendar Notification", controller.AddCalendarNotification);
			AddButton("One Action Notification", controller.AddOneButtonNotification);
			AddButton("Twoo Actions Notification", controller.AddTwoActionsNotification);
			AddButton("Three Actions Notification", controller.AddThreeActionsNotification);
			AddButton("Four Actions Notification", controller.AddFourActionsNotification);

			// TablesForm
			MinimumSize = new Size(100, 100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(480, 320);
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "NotificationsForm";
			Padding = new Padding(6);
			Text = "Notifications";

			panel1.ResumeLayout(true);
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		protected Button AddButton(string title, Action action)
		{
			var button = new Button();
			button.AutoSize = true;
			button.Click += (a, b) => action();
			button.Text = title;
			panel1.Controls.Add(button);
			return button;
		}

		protected void grid_ColumnHeaderDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
		{
			Console.WriteLine($"{e}");
		}
	}

	class CategoryID
	{
		public const string Plain = "Plain";
		public const string Simple = "Simple";
		public const string Double = "Double";
		public const string Triple = "Triple";
		public const string Quadruple = "Quadruple";
	}

	class ActionID
	{
		public const string Snooze = "Snooze";
		public const string Open = "Open";
		public const string Delete = "Delete";
		public const string Reply = "Reply";
	}

	class IntentID
	{
		public const string Snooze = "Default";
	}

	class Controller : UNUserNotificationCenterDelegate
	{
		NotificationsForm form;
		
		object locker = new Object();
		HashSet<string> ids = new HashSet<string>();

		public Controller(NotificationsForm form)
		{
			this.form = form;
			InitializeCategories();
		}

		public void InitializeCategories()
		{
			var snoozeAction = UNNotificationAction.FromIdentifier(ActionID.Snooze, "Snooze", UNNotificationActionOptions.None);
			var openAction = UNNotificationAction.FromIdentifier(ActionID.Open, "Open", UNNotificationActionOptions.Foreground);
			var deleteAction = UNNotificationAction.FromIdentifier(ActionID.Delete, "Delete", UNNotificationActionOptions.Destructive);
			var replyAction = UNNotificationAction.FromIdentifier(ActionID.Reply, "Reply", UNNotificationActionOptions.Foreground);

			var cats = new UNNotificationCategory[] {
				UNNotificationCategory.FromIdentifier(
					CategoryID.Plain,
					new UNNotificationAction[] { },
					new string[] {},
					UNNotificationCategoryOptions.None),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Simple,
					new UNNotificationAction[] { openAction },
					new string[] {},
					UNNotificationCategoryOptions.None),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Double,
					new UNNotificationAction[] { openAction, replyAction },
					new string[] {},
					UNNotificationCategoryOptions.None),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Triple,
					new UNNotificationAction[] { openAction, replyAction, deleteAction },
					new string[] {},
					UNNotificationCategoryOptions.None),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Quadruple, 
					new UNNotificationAction[] { snoozeAction, openAction, replyAction, deleteAction }, 
					new string[] {},
					UNNotificationCategoryOptions.None),
			};
			UNUserNotificationCenter.Current.SetNotificationCategories(new NSSet<UNNotificationCategory>(cats));
		}

		public void PrintSettings()
		{
			Console.WriteLine($"PrintSettings");

			var center = UNUserNotificationCenter.Current;
			center.GetNotificationSettings((settings) => {
				Console.WriteLine($"GetNotificationSettings => {settings}");
			});
		}

		public void RequestAuthorization()
		{
			Console.WriteLine($"RequestAuthorization");

			var center = UNUserNotificationCenter.Current;
			center.GetNotificationSettings((settings) => {
				if (settings.AuthorizationStatus == UNAuthorizationStatus.NotDetermined)
				{
					var options = UNAuthorizationOptions.Badge | UNAuthorizationOptions.Alert;
					center.RequestAuthorization(options, (granted, error) => {
						Console.WriteLine($"RequestAuthorization => {granted}, {error}");
					});
				}
				else
				{
					Console.WriteLine($"Authorization already granted");
				}
			});
		}

		public void AddTimerNotification()
		{
			Console.WriteLine($"AddTimerNotification");
			AddRequest(CategoryID.Plain, "Plain (Timer Trigger)", "No actions", NewTimerTrigger());
		}

		public void AddCalendarNotification()
		{
			Console.WriteLine($"AddCalendarNotification");
			AddRequest(CategoryID.Plain, "Plain (Calendar Trigger)", "No actions", NewCalendarTrigger());
		}

		public void AddOneButtonNotification()
		{
			Console.WriteLine($"AddOneButtonNotification");
			AddRequest(CategoryID.Simple, "Simple", "One action");
		}

		public void AddTwoActionsNotification()
		{
			Console.WriteLine($"AddTwoActionsNotification");
			AddRequest(CategoryID.Double, "Double", "Two actions");
		}

		public void AddThreeActionsNotification()
		{
			Console.WriteLine($"AddThreeActionsNotification");
			AddRequest(CategoryID.Triple, "Triple", "Three actions");
		}

		public void AddFourActionsNotification()
		{
			Console.WriteLine($"AddFourActionsNotification");
			AddRequest(CategoryID.Quadruple, "Quadruple", "Four actions");
		}

		public void AddRequest(string category, string title, string? subtitle = null, UNNotificationTrigger? trigger = null)
		{
			var center = UNUserNotificationCenter.Current;
			var content = new UNMutableNotificationContent
			{
				Title = title,
				Subtitle = subtitle ?? "",
				CategoryIdentifier = category
			};
			
			string identifier = Guid.NewGuid().ToString();
			lock(locker)
				ids.Add(identifier);

			var request = UNNotificationRequest.FromIdentifier(identifier, content, trigger ?? NewTimerTrigger(2));

			Console.WriteLine($"AddNotificationRequest({identifier}, {content.Title}, {content.Subtitle})");
			UNUserNotificationCenter.Current.AddNotificationRequest(request, (error) => {
				Console.WriteLine( error != null ? $"- FAILED: {error}" : " - ADDED");
			});
		}

		UNNotificationTrigger NewTimerTrigger(double seconds = 2.0)
		{
			return UNTimeIntervalNotificationTrigger.CreateTrigger(seconds, false);
		}

		UNNotificationTrigger NewCalendarTrigger(double seconds = 2.0)
		{
			var date = (NSDate)DateTime.Now.AddSeconds(2);
			var components =  NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Year | NSCalendarUnit.Month | NSCalendarUnit.Day | NSCalendarUnit.Hour | NSCalendarUnit.Minute | NSCalendarUnit.Second, date);
			return  UNCalendarNotificationTrigger.CreateTrigger(components, false);
		}

		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			Console.WriteLine($"WillPresentNotification: {notification}");

			var identifier = notification.Request.Identifier;
			Console.WriteLine($"Identifier: {identifier}");

			completionHandler(UNNotificationPresentationOptions.Banner);
		}

		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			Console.WriteLine($"DidReceiveNotificationResponse: {response}");

			var identifier = response.Notification.Request.Identifier;
			Console.WriteLine($"Identifier: {identifier}");

			lock(locker)
			 	ids.Remove(identifier);

			completionHandler();
		}
	}
}

#endif

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
		TextBox textbox1;

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
			this.textbox1 = new TextBox();
            this.SuspendLayout();

			// panel1
			panel1.SuspendLayout();
			panel1.AutoSize = true;
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Dock = DockStyle.Left;
			panel1.Name = "panel1";
			panel1.FlowDirection = FlowDirection.TopDown;
			panel1.Padding = new Padding(1);

			textbox1.Dock = DockStyle.Fill;
			textbox1.Text = "Nazdar";
			textbox1.AutoSize = false;
			textbox1.Multiline = true;
			textbox1.AcceptsTab = true;
			textbox1.AcceptsReturn = true;
			textbox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;

			AddButton("Print Settings", controller.PrintSettings);
			AddButton("Request Authorization", controller.RequestAuthorization);
			AddButton("Timer Notification", controller.AddTimerNotification);
			AddButton("Calendar Notification", controller.AddCalendarNotification);
			AddButton("One Action Notification", controller.AddOneButtonNotification);
			AddButton("Twoo Actions Notification", controller.AddTwoActionsNotification);
			AddButton("Three Actions Notification", controller.AddThreeActionsNotification);
			AddButton("Four Actions Notification", controller.AddFourActionsNotification);
			AddButton("Remove All notifications", controller.RemoveAllNotifications);
			AddButton("Clear Console", () => textbox1.Clear() );

			// TablesForm
			MinimumSize = new Size(100, 100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(640, 360);
			Controls.Add(textbox1);
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
			WriteLine($"{e}");
		}

		public void WriteLine(string line)
		{
			this.BeginInvoke(() =>
			{
				textbox1.AppendText(line);
				textbox1.AppendText(Environment.NewLine);
				textbox1.SelectionStart = textbox1.TextLength;
				textbox1.ScrollToCaret();
			});
			
			Console.WriteLine(line);
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
			//InitializeCategories();
		}

		public void InitializeCategories()
		{
			var snoozeAction = UNNotificationAction.FromIdentifier(ActionID.Snooze, "Snooze", UNNotificationActionOptions.None);
			var openAction = UNNotificationAction.FromIdentifier(ActionID.Open, "Open", UNNotificationActionOptions.Foreground);
			var deleteAction = UNNotificationAction.FromIdentifier(ActionID.Delete, "Delete", UNNotificationActionOptions.Destructive);
			var replyAction = UNNotificationAction.FromIdentifier(ActionID.Reply, "Reply", UNNotificationActionOptions.Foreground);

			var options = UNNotificationCategoryOptions.CustomDismissAction;

			var cats = new UNNotificationCategory[] {
				UNNotificationCategory.FromIdentifier(
					CategoryID.Plain,
					new UNNotificationAction[] { },
					new string[] {},
					options),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Simple,
					new UNNotificationAction[] { openAction },
					new string[] {},
					options),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Double,
					new UNNotificationAction[] { openAction, replyAction },
					new string[] {},
					options),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Triple,
					new UNNotificationAction[] { openAction, replyAction, deleteAction },
					new string[] {},
					options),
				UNNotificationCategory.FromIdentifier(
					CategoryID.Quadruple, 
					new UNNotificationAction[] { snoozeAction, openAction, replyAction, deleteAction }, 
					new string[] {},
					options),
			};
			UNUserNotificationCenter.Current.SetNotificationCategories(new NSSet<UNNotificationCategory>(cats));
		}

		void RegisterCategory(string categoryID)
		{
			var actions = CreateActions(categoryID);
			var options = UNNotificationCategoryOptions.CustomDismissAction;
			var category = UNNotificationCategory.FromIdentifier(categoryID, actions, new string[] {}, options);
			var cats = new UNNotificationCategory[] { category };
			UNUserNotificationCenter.Current.SetNotificationCategories(new NSSet<UNNotificationCategory>(cats));
		}

		UNNotificationAction[] CreateActions(string categoryID)
		{
			switch (categoryID)
			{
				default:
				case CategoryID.Plain:
					return new UNNotificationAction[] { };
				case CategoryID.Simple:
					return new UNNotificationAction[] { 
						UNNotificationAction.FromIdentifier(ActionID.Open, "Open", UNNotificationActionOptions.Foreground)
					};
				case CategoryID.Double:
					return new UNNotificationAction[] {
						UNNotificationAction.FromIdentifier(ActionID.Open, "Open", UNNotificationActionOptions.Foreground),
						UNNotificationAction.FromIdentifier(ActionID.Delete, "Delete", UNNotificationActionOptions.Destructive)
					};
				case CategoryID.Triple:
					return new UNNotificationAction[] {
						UNNotificationAction.FromIdentifier(ActionID.Open, "Open", UNNotificationActionOptions.Foreground),
						UNNotificationAction.FromIdentifier(ActionID.Delete, "Delete", UNNotificationActionOptions.Destructive),
						UNNotificationAction.FromIdentifier(ActionID.Reply, "Reply", UNNotificationActionOptions.Foreground)
					};
				case CategoryID.Quadruple:
					return new UNNotificationAction[] {
						UNNotificationAction.FromIdentifier(ActionID.Snooze, "Snooze", UNNotificationActionOptions.None),
						UNNotificationAction.FromIdentifier(ActionID.Open, "Open", UNNotificationActionOptions.Foreground),
						UNNotificationAction.FromIdentifier(ActionID.Delete, "Delete", UNNotificationActionOptions.Destructive),
						UNNotificationAction.FromIdentifier(ActionID.Reply, "Reply", UNNotificationActionOptions.Foreground)
					};
			}
		}

		public void PrintSettings()
		{
			var n = NSLocale.CurrentLocaleDidChangeNotification;
			form.WriteLine($"PrintSettings");

			var center = UNUserNotificationCenter.Current;
			center.GetNotificationSettings((settings) => {
				form.WriteLine($"GetNotificationSettings => {settings}");
			});
		}

		public void RequestAuthorization()
		{
			form.WriteLine($"RequestAuthorization");

			var center = UNUserNotificationCenter.Current;
			center.GetNotificationSettings((settings) => {
				if (settings.AuthorizationStatus == UNAuthorizationStatus.NotDetermined)
				{
					var options = UNAuthorizationOptions.Badge | UNAuthorizationOptions.Alert;
					center.RequestAuthorization(options, (granted, error) => {
						form.WriteLine($"RequestAuthorization => {granted}, {error}");
					});
				}
				else
				{
					form.WriteLine($"Authorization already granted");
				}
			});
		}

		public void AddTimerNotification()
		{
			form.WriteLine($"AddTimerNotification");
			AddRequest(CategoryID.Plain, "Plain (Timer Trigger)", "No actions", NewTimerTrigger());
		}

		public void AddCalendarNotification()
		{
			form.WriteLine($"AddCalendarNotification");
			AddRequest(CategoryID.Plain, "Plain (Calendar Trigger)", "No actions", NewCalendarTrigger());
		}

		public void AddOneButtonNotification()
		{
			form.WriteLine($"AddOneButtonNotification");
			AddRequest(CategoryID.Simple, "Simple", "One action");
		}

		public void AddTwoActionsNotification()
		{
			form.WriteLine($"AddTwoActionsNotification");
			AddRequest(CategoryID.Double, "Double", "Two actions");
		}

		public void AddThreeActionsNotification()
		{
			form.WriteLine($"AddThreeActionsNotification");
			AddRequest(CategoryID.Triple, "Triple", "Three actions");
		}

		public void AddFourActionsNotification()
		{
			form.WriteLine($"AddFourActionsNotification");
			AddRequest(CategoryID.Quadruple, "Quadruple", "Four actions");
		}

		public void RemoveAllNotifications()
		{
			form.WriteLine($"RemoveAllNotifications");
			UNUserNotificationCenter.Current.GetPendingNotificationRequests((requests) => {
				form.WriteLine($"Removing {requests.Length} pending request(s)");
				UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
			});

			UNUserNotificationCenter.Current.GetDeliveredNotifications((notifications) => {
				form.WriteLine($"Removing {notifications.Length} delivered notification(s)");
				UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
			});
		}

		public void AddRequest(string category, string title, string? subtitle = null, UNNotificationTrigger? trigger = null)
		{
			// Try to register category just in time (since we do not have notification categories in emc)
			RegisterCategory(category);

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

			form.WriteLine($"AddNotificationRequest({identifier}, {content.Title}, {content.Subtitle})");
			UNUserNotificationCenter.Current.AddNotificationRequest(request, (error) => {
				form.WriteLine( error != null ? $"- FAILED: {error}" : " - ADDED");
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
			form.WriteLine($"WillPresentNotification: {notification}");

			var identifier = notification.Request.Identifier;
			form.WriteLine($"Identifier: {identifier}");

			completionHandler(UNNotificationPresentationOptions.Banner);
		}

		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			form.WriteLine($"DidReceiveNotificationResponse: {response}");

			var identifier = response.Notification.Request.Identifier;
			form.WriteLine($"Identifier: {identifier}, IsDefault:{response.IsDefaultAction}, IsCustom:{response.IsCustomAction}, IsDismiss:{response.IsDismissAction}");

			lock(locker)
			 	ids.Remove(identifier);

			completionHandler();
		}
	}
}

#endif

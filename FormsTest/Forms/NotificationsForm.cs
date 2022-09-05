#if MAC
using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
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
			InitializeComponent();

			UNUserNotificationCenter.Current.Delegate = controller = new Controller(this);
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

			var button1 = new Button();
			button1.AutoSize = true;
			button1.Click += button1_Click;
			button1.Text = "Print Settings";
			panel1.Controls.Add(button1);

			var button2 = new Button();
			button2.AutoSize = true;
			button2.Click += button2_Click;
			button2.Text = "Request Authorization";
			panel1.Controls.Add(button2);

			var button3 = new Button();
			button3.AutoSize = true;
			button3.Click += button3_Click;
			button3.Text = "Timer Notification";
			panel1.Controls.Add(button3);

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

		protected void button1_Click(object? sender, EventArgs e)
		{
			controller.PrintSettings();
		}

		protected void button2_Click(object? sender, EventArgs e)
		{
			controller.RequestAuthorization();
		}

		protected void button3_Click(object? sender, EventArgs e)
		{
			controller.AddTimerNotification();
		}
	}

	class Controller : UNUserNotificationCenterDelegate
	{
		NotificationsForm form;
		
		object locker = new Object();
		HashSet<string> ids = new HashSet<string>();

		public Controller(NotificationsForm form)
		{
			this.form = form;
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

			var center = UNUserNotificationCenter.Current;

			var content = new UNMutableNotificationContent
			{
				Title = "Timer Notification",
				Subtitle = "Subtitle"
			};
			
			var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

			string identifier = Guid.NewGuid().ToString();
			lock(locker)
				ids.Add(identifier);

			var request = UNNotificationRequest.FromIdentifier(identifier, content, trigger);

			Console.WriteLine($"AddNotificationRequest({identifier}, {content.Title}, {content.Subtitle})");

			UNUserNotificationCenter.Current.AddNotificationRequest(request, (error) => {
				if (error == null)
					Console.WriteLine($" - ADDED");
				else
					Console.WriteLine($" - FAILED:{error}");
			});
		}

		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			Console.WriteLine($"WillPresentNotification: {notification}");

			var identifier = notification.Request.Identifier;
			Console.WriteLine($"Identifier: {identifier}");

			completionHandler(UNNotificationPresentationOptions.Alert);
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

#endif //MAC

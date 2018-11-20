using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace XibLessApp
{
	class AppDelegate : NSApplicationDelegate
	{
		public override void WillFinishLaunching(NSNotification notification)
		{
			Console.WriteLine("WillFinishLaunching");
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			Console.WriteLine("DidFinishLaunching");
		}
	}

	class Program
	{
		static NSApplication app;

		static void Main(string[] args)
		{
			NSApplication.Init();

			app = NSApplication.SharedApplication;
			app.Delegate = new AppDelegate();
			app.ActivationPolicy = NSApplicationActivationPolicy.Regular;
			app.Menu = Program.CreateMenu();

            app.FinishLaunching();
			//app.Run()

			//var dialog = Program.WindowWithTitle("Modal");
			//Program.RunModalWindow(dialog);

			var window = Program.WindowWithTitle("Normal");
			window.MakeKeyAndOrderFront(app);
			Program.Run();
		}

		static void Run()
		{
			while (true)
			{
				var mask = NSEventMask.AnyEvent;
				var @event = NSApplication.SharedApplication.NextEvent(mask, NSDate.DistantFuture, NSRunLoop.NSDefaultRunLoopMode, true);

				if (@event == null)
					break;

				app.SendEvent(@event);
			}
		}

		static void RunModalWindow(NSWindow window)
		{
			var session = app.BeginModalSession(window);
			while (true)
			{
				var mask = NSEventMask.AnyEvent;
				var @event = app.NextEvent(mask, NSDate.DistantFuture, NSRunLoop.NSDefaultRunLoopMode, true);

				if (@event != null)
				{
					app.SendEvent(@event);

					if (!window.IsVisible)
						break;
				}
			}
			app.EndModalSession(session);
		}

		static NSWindow WindowWithTitle(String title)
		{
			var style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable;
			var window = new Window(new RectangleF(0, 0, 480, 320), style, NSBackingStore.Buffered, false);
			window.Title = title;
			return window;
		}

		static NSMenu CreateMenu()
		{
			var root = new NSMenu("MainMenu");

			var appMenu = new NSMenu("AppMenu");
			//appMenu.AddItem(new NSMenuItem("Hide", "h", (sender, e) => { Console.WriteLine("Hide"); }));
			//appMenu.AddItem(new NSMenuItem("Hide others", "o", (sender, e) => { Console.WriteLine("Hide Others"); }));
			//appMenu.AddItem(new NSMenuItem("Quit", "q", (sender, e) => { Console.WriteLine("Quit"); app.Terminate(app); }));
			appMenu.AddItem(new NSMenuItem("Hide", new Selector("hide:"), "h"));
			appMenu.AddItem(new NSMenuItem("Hide Others", new Selector("hideOtherApplications:"), "o"));
			appMenu.AddItem(new NSMenuItem("Quit XibLessApp", new Selector("terminate:"), "q"));

			var appMenuItem = root.AddItem("AppMenuItem", null, "");
			root.SetSubmenu(appMenu, appMenuItem);

			return root;
		}
	}
}


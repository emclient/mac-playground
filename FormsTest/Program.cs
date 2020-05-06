// #define INCLUDE_CEF_TEST

using System;
using System.Windows.Forms;
using System.Threading;
using FormsTest;
using System.Diagnostics;
#if INCLUDE_CEF_TEST
using MailClient.Common.UI;
using Xilium.CefGlue;
#endif
using System.IO;
using System.Runtime.InteropServices;

#if MAC
using MacApi.Posix;
#if XAMARINMAC
using AppKit;
#else //MONOMAC
using MonoMac.AppKit;
#endif

#endif

namespace FormsTest
{
	public static class Program
	{
		//public static Settings Settings { get; private set; }

#if INCLUDE_CEF_TEST
		static CefApp a = new MyCefApp();

		class MyCefRenderProcessHandler : CefRenderProcessHandler
		{
		}

		class MyCefApp : CefApp
		{
			protected override CefRenderProcessHandler GetRenderProcessHandler()
			{
				return new MyCefRenderProcessHandler();
			}
		}
#endif

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static int Main(string[] args)
		{
			MaxOpenFiles();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.ApplicationExit += (object sender, EventArgs e) =>
			{
				//UrlProtocol.Unregister();
			};

			//UrlProtocol.Register();

			//Marshalling.Initialize();
			//CefApp.

			CreateMacMenu();

			var f = new MainForm();
			f.Show();
			Application.Run();

			var threads = Process.GetCurrentProcess().Threads;

			return 0;
		}

#if MAC

		static void CreateMacMenu()
		{
			var menuBar = new NSMenu("");

			var appMenuItem = new NSMenuItem("");
			var appMenu = new NSMenu("FormsTest");
			var quitItem = new NSMenuItem("Quit") { KeyEquivalent = "q" };
			quitItem.Activated += (sender, e) => { Terminate(); };
			appMenu.AddItem(quitItem);
			menuBar.AddItem(appMenuItem);
			menuBar.SetSubmenu(appMenu, appMenuItem);

			var editMenuItem = new NSMenuItem();
			var editMenu = new NSMenu("Edit");
			var selectAllItem = new NSMenuItem("Select All") { Action = new ObjCRuntime.Selector("selectAll:"), KeyEquivalent = "a" };
			editMenu.AddItem(selectAllItem);
			var copyItem = new NSMenuItem("Copy") { Action = new ObjCRuntime.Selector("copy:"), KeyEquivalent="c" };
			editMenu.AddItem(copyItem);
			var pasteItem = new NSMenuItem("Paste") { Action = new ObjCRuntime.Selector("paste:") , KeyEquivalent="v" };
			editMenu.AddItem(pasteItem);
			menuBar.AddItem(editMenuItem);
			menuBar.SetSubmenu(editMenu, editMenuItem);

			NSApplication.SharedApplication.Menu = menuBar;
		}

		static void Terminate()
		{
			// Workaround for not exiting the app because of pending threads related to UrlProtocol subclass.
			var app = NSApplication.SharedApplication;
			if (app.Running)
				app.Terminate(NSApplication.SharedApplication);
			else
				LibC.exit(0);
		}

		static void MaxOpenFiles()
		{
			var lim = new LibC.rlimit();
			var ok = LibC.getrlimit((int)LibC.RLimit.NoFile, ref lim);
			Console.WriteLine($"getrlimit({lim.cur}, {lim.max}) => {ok}");

			int n = 9000;
			lim.cur = lim.max = (UInt64)n;
			ok = LibC.setrlimit((int)LibC.RLimit.NoFile, ref lim);
			Console.WriteLine($"setrlimit({lim.cur}, {lim.max}) => {ok}");
			ok = LibC.getrlimit((int)LibC.RLimit.NoFile, ref lim);
			Console.WriteLine($"getrlimit({lim.cur}, {lim.max}) => {ok}");

			lim.cur = (UInt64)4000;
			ok = LibC.setrlimit((int)LibC.RLimit.NoFile, ref lim);
			Console.WriteLine($"setrlimit({lim.cur}, {lim.max}) => {ok}");
			ok = LibC.getrlimit((int)LibC.RLimit.NoFile, ref lim);
			Console.WriteLine($"getrlimit({lim.cur}, {lim.max}) => {ok}");

			var a = new Stream[n];
			var f = Path.GetTempFileName();
			int i = 0;
			try
			{
				for (i = 0; i < n; ++i)
				{
					a[i] = new FileStream(f, FileMode.Open, FileAccess.Read);
					//var result = LibC.open(f, LibC.FileMode.S_IRUSR);
					//var result = LibC.fopen(f, "rb");
					//if (result == IntPtr.Zero)
						//break;
				}
			}
			catch
			{
			}
			finally
			{
				for (int j = i - 1; j >= 0; --j) 					a[j].Close(); 			}
			Console.WriteLine($"#{i}");
		}
#else
		static void CreateMacMenu()
		{
		}

		static void Terminate()
		{
		}

		static void MaxOpenFiles()
		{
		}
#endif
	}
}

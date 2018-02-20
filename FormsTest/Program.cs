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
using MacBridge.Posix;
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
		public static void Main(string[] args)
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

			Application.Run(new MainForm());

			var threads = Process.GetCurrentProcess().Threads;

			Terminate();
		}

#if MAC
		static void Terminate()
		{
			// Workaround for not exiting the app because of pending threads related to UrlProtocol subclass.
			NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
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
		static void Terminate()
		{
		}

		static void MaxOpenFiles()
		{
		}
#endif
	}
}

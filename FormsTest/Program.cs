using System;
using System.Windows.Forms;
using System.Threading;
using FormsTest;
using System.Diagnostics;
using MailClient.Common.UI;
using Xilium.CefGlue;
using MacBridge.Posix;
using System.IO;
using System.Runtime.InteropServices;

#if MAC

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

			Marshalling.Initialize();
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
			LibC.getrlimit((int)LibC.RLimit.NoFile, ref lim);

			int n = 9000;
			lim.cur = lim.max = (UInt64)n;
			LibC.setrlimit((int)LibC.RLimit.NoFile, ref lim);

			LibC.getrlimit((int)LibC.RLimit.NoFile, ref lim);

			lim.cur = (UInt64)4000;
			LibC.setrlimit((int)LibC.RLimit.NoFile, ref lim);

			var a = new FileStream[n];
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
			Console.WriteLine($"#{i}");
		}

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

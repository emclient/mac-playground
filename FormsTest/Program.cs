using System;
using System.Windows.Forms;
using System.Threading;
using FormsTest;
using System.Diagnostics;
using MailClient.Common.UI;
using Xilium.CefGlue;

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

#if MAC
			// Workaround for not exiting the app because of pending threads related to UrlProtocol subclass.
            NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
#endif
        }
	}
}

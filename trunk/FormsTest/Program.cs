using System;
using System.Windows.Forms;
using System.Threading;
using FormsTest;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using System.Diagnostics;

namespace FormsTest
{
	public static class Program
	{
        //public static Settings Settings { get; private set; }
        
		static Class urlProtocolClass;

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.ApplicationExit += (object sender, EventArgs e) =>
			{
				if (urlProtocolClass != null)
					NSUrlProtocol.UnregisterClass(urlProtocolClass);
			};

			urlProtocolClass = new Class(typeof(UrlProtocol));
			NSUrlProtocol.RegisterClass(urlProtocolClass);

			Application.Run(new MainForm());

			var threads = Process.GetCurrentProcess().Threads;

			// Workaround for not exiting the app because of pending threads related to UrlProtocol subclass.
			//MonoMac.AppKit.NSApplication.SharedApplication.Terminate(MonoMac.AppKit.NSApplication.SharedApplication);
		}
	}
}

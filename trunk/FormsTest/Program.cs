using System;
using System.Windows.Forms;
using System.Threading;
using FormsTest;
using System.Diagnostics;

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
                UrlProtocol.Unregister();
			};

            UrlProtocol.Register();

			Application.Run(new MainForm());

			var threads = Process.GetCurrentProcess().Threads;

#if MAC
			// Workaround for not exiting the app because of pending threads related to UrlProtocol subclass.
            NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
#endif
        }
	}
}

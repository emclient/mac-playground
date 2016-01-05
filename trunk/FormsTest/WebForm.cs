using System.Windows.Forms;
using MailClient.UI.Controls;
using System;

#if MAC

using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.WebKit;

#endif

namespace FormsTest
{
	partial class WebForm : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#if MAC 
		NSView container;
		WebView webView;
		#endif

		public WebForm()
		{
			InitializeComponent();
			InstallWebview();

			Load += LayoutForm_Load;
		}

		private void InitializeComponent()
		{
		}

		protected void InstallWebview()
		{
			#if MAC

			this.container = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(this.Handle);
			this.webView = new WebView();

			webView.Frame = container.Bounds;
			container.AddSubview(webView);

			var url = new NSUrl("http://www.seznam.cz");
			var request = new NSUrlRequest(url);
			webView.MainFrame.LoadRequest(request);

			#endif
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			#if MAC
			webView.Frame = container.Bounds;
			#endif
		}

		void LayoutForm_Load(object sender, EventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

	}
}
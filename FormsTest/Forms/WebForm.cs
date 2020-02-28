using System.Windows.Forms;
//using MailClient.UI.Controls;
using System;

#if MAC

#if XAMARINMAC
using AppKit;
using Foundation;
using WebKit;
#else
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.WebKit;
using ObjCRuntime = MonoMac.ObjCRuntime;
#endif

#endif // MAC

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

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		protected void InstallWebview()
		{
			#if MAC

			this.container = (NSView)ObjCRuntime.Runtime.GetNSObject(this.Handle);
			this.webView = new WebView();
			webView.Frame = container.Bounds;
			container.AddSubview(webView);

			webView.OnSendRequest = this.OnSendRequest;
			webView.DecidePolicyForNavigation += new EventHandler<WebNavigationPolicyEventArgs>(DecidePolicyForNavigation);
			webView.StartedProvisionalLoad += new EventHandler<WebFrameEventArgs>(WebViewStartedProvisionalLoad);
			webView.CommitedLoad += new EventHandler<WebFrameEventArgs>(WebViewCommitedLoad);
			webView.FinishedLoad += new EventHandler<WebFrameEventArgs>(WebViewFinishedLoad);
			webView.FailedLoadWithError += new EventHandler<WebFrameErrorEventArgs>(WebViewFailedLoadWithError);
			webView.WillCloseFrame += new EventHandler<WebFrameEventArgs>(WebViewWillCloseFrame);
			webView.ClearedWindowObject += new EventHandler<WebFrameScriptFrameEventArgs>(WebViewClearedWindowObject);
			webView.ReceivedTitle += new EventHandler<WebFrameTitleEventArgs>(WebViewReceivedTitle);

			//var url = new NSUrl("http://jetencurakjesteprezidentem.cz");
			var url = new NSUrl("http://idnes.cz");
			var request = new NSUrlRequest(url);
			webView.MainFrame.LoadRequest(request);

			#endif
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			#if MAC
			if (webView != null && container != null)
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

		// ===============

        #if MAC

		private NSUrlRequest OnSendRequest(WebView sender, NSObject identifier, NSUrlRequest request, NSUrlResponse redirectResponse, WebDataSource dataSource)
		{
			return request;
		}

		private void DecidePolicyForNavigation(object sender, WebNavigationPolicyEventArgs e)
		{
			WebView.DecideUse(e.DecisionToken);
		}

		private void WebViewClearedWindowObject(object sender, WebFrameScriptFrameEventArgs e)
		{
//			var jsStubName = "__cefFrameStub";
//			var stub = new JSStub(this.mainFrame, jsStubName);
//			e.WindowObject.SetValueForKey(stub, new NSString(jsStubName));
		}

		private void WebViewStartedProvisionalLoad(object sender, WebFrameEventArgs e)
		{
		}

		private void WebViewCommitedLoad(object sender, WebFrameEventArgs e)
		{
		}

		private void WebViewFinishedLoad(object sender, WebFrameEventArgs e)
		{
		}

		private void WebViewFailedLoadWithError(object sender, WebFrameErrorEventArgs e)
		{
		}

		private void WebViewWillCloseFrame(object sender, WebFrameEventArgs e)
		{
		}

		private void WebViewReceivedTitle(object sender, WebFrameTitleEventArgs e)
		{
		}

		// ==============

		internal class JSStub : NSObject {

			const string WrapperSelector = "execute:obj:args:";

			WebFrame frame;
			string name;

			internal JSStub(WebFrame frame, string name)
			{
				this.frame = frame;
				this.name = name;
			}
		}

        #endif // MAC
	}
}
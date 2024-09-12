using System;
using System.Drawing;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

#if MAC
using AppKit;
using Foundation;
#endif

namespace FormsTest
{
	partial class CancellationForm : Form
	{
		System.ComponentModel.IContainer components = null;
		Panel mainPanel;
		Button startButton, cancelButton;
		Label messageLabel;

		ManualResetEvent finishedEvent = new(true);

		HttpClient? httpClient;

		bool isRunning;
		object isRunningLock = new();

		System.Threading.CancellationTokenSource cancellationTokenSource = new();

		public CancellationForm()
		{
			InitializeComponent();
			
			NetworkUtility.NetworkAvailabilityChanged += NetworkUtility_NetworkAvailabilityChanged;
		}

		private void InitializeComponent()
		{
			components = new Container();

			this.mainPanel = new Panel();
            this.SuspendLayout();

			// mainPanel
			mainPanel.SuspendLayout();
			mainPanel.AutoSize = true;
			mainPanel.BorderStyle = BorderStyle.FixedSingle;
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Name = "mainPanel";
			mainPanel.Padding = new Padding(1);

			messageLabel = new Label();
			messageLabel.Dock = DockStyle.Top;
			messageLabel.AutoSize = true;
			mainPanel.Controls.Add(messageLabel);

			startButton = new Button();
			startButton.AutoSize = true;
			startButton.Dock = DockStyle.Top;
			startButton.Text = "Start";
			startButton.Click += StartButton_Click;
			mainPanel.Controls.Add(startButton);

			cancelButton = new Button();
			cancelButton.AutoSize = true;
			cancelButton.Dock = DockStyle.Top;
			cancelButton.Text = "Cancel";
			cancelButton.Click += CancelButton_Click;
			cancelButton.Enabled = false;
			mainPanel.Controls.Add(cancelButton);

			Controls.Add(mainPanel);

			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "CancellationForm";
			Padding = new Padding(6);
			Text = "Cancellation";

			mainPanel.ResumeLayout(true);
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void Dispose(bool disposing)
		{
			Cancel();
			NetworkUtility.NetworkAvailabilityChanged -= NetworkUtility_NetworkAvailabilityChanged;

			if (disposing && (components != null))
				components.Dispose();

			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		async void StartButton_Click(object sender, EventArgs e)
		{
			await Start().ConfigureAwait(false);
		}
		void CancelButton_Click(object sender, EventArgs e)
		{
			Cancel();
		}

		async Task Start()
		{
			lock(isRunningLock)
			{
				if (isRunning)
					return;
				isRunning = true;
			}

			Report("Start");

			cancellationTokenSource = new();
			finishedEvent.Reset();

			BeginInvoke(() => {
			 	startButton.Enabled = false;
			 	cancelButton.Enabled = true;
			});

			try
			{
				await LoadAvatars(cancellationTokenSource.Token).ConfigureAwait(false);
				await SendReceive(cancellationTokenSource.Token).ConfigureAwait(false);
				Report("Finished");
			}
			catch (Exception ex)
			{
				Report(cancellationTokenSource.IsCancellationRequested ? "Cancelled" : "Failed: " + ex.ToString());
			}

			finishedEvent.Set();

			BeginInvoke(() => {
			 	startButton.Enabled = true;
			 	cancelButton.Enabled = false;
			});

			lock(isRunningLock)
				isRunning = false;
		}

		void Cancel()
		{
			lock (isRunningLock)
				if (!isRunning)
					return;

			Report("Cancel");
			cancellationTokenSource.Cancel();
			finishedEvent.WaitOne();
		}

		void NetworkUtility_NetworkAvailabilityChanged(object sender, EventArgs e)
		{
			if (NetworkUtility.IsNetworkAvailable)
				GoOnline();
			else	
				GoOffline();
		}

		void GoOffline()
		{
			Console.WriteLine("+GoOffline");
			Cancel();
			Console.WriteLine("-GoOffline");
		}

		void GoOnline()
		{
			_ = Task.Run(() => Start());
		}

		async Task<string> SendReceive(CancellationToken ct)
		{
			var url = "https://www.google.com";
			return await SendReceive(url, ct).ConfigureAwait(false);
		}

		async Task<string> SendReceive(string url, CancellationToken ct)
		{
			var client = GetHttpClient();

			using var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.Content = new StringContent(new string ('x', 100000));

			Report("Sending data...");
			var sendTask = client.SendAsync(request, ct).ConfigureAwait(false);
			var sent = await sendTask;
			Console.WriteLine("...data sent");

			await Task.Delay(2000, ct).ConfigureAwait(false);

			Report("Waiting for response...");
			var getTask = client.GetAsync(url, ct).ConfigureAwait(false);
			var response = await getTask;
			Report("...response received");

			await Task.Delay(2000, ct).ConfigureAwait(false);

			Console.WriteLine("Reading data...");
			var readTask = response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			var content = await readTask;
			Console.WriteLine("...Data read");

			Report($"received:{content.Length} bytes");

			return $"{content.Length}";
		}

		async Task LoadAvatars(CancellationToken ct)
		{
			for (int i = 0; i < 10; ++i)
			foreach (var url in avatars)
			{
				ct.ThrowIfCancellationRequested();
				Report($"+Downloading avatar: {url}");
				byte[] bytes = await LoadAvatar(url, ct).ConfigureAwait(false);
				Report($"-Downloading avatar: {url} ({bytes.Length} bytes)");
			}
		}

		async Task<byte[]> LoadAvatar(string url, CancellationToken ct)
		{
			var client = GetHttpClient();
			var response = await client.GetAsync(url, ct).ConfigureAwait(false);
			return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
		}

		void Report(string message)
		{
			Console.WriteLine("message");
			BeginInvoke(() => { messageLabel.Text = message; });
		}

		HttpClient GetHttpClient()
		{
			if (httpClient == null)
			{
				httpClient = new HttpClient(GetHandler());
				httpClient.Timeout = TimeSpan.FromSeconds(60);
			}

			return httpClient;
		}

		HttpMessageHandler GetHandler()
		{
			HttpMessageHandler handler =
#if MAC
				new NSUrlSessionHandler(NSUrlSessionConfiguration.EphemeralSessionConfiguration);
#else
				new HttpClientHandler();
#endif
			return handler;
		}

		string[] avatars = [
			"https://avatars.slack-edge.com/2023-10-25/6092362819522_8285c426a943f70aec6d_48.jpg",
			"https://avatars.slack-edge.com/2023-03-17/4952147317287_c92e2b60a1ac8af29a96_48.jpg",
			"https://avatars.slack-edge.com/2015-05-19/4971928231_03744fc1b8c4d6d6a66e_48.jpg",
			"https://avatars.slack-edge.com/2024-07-04/7387803155585_32f0b72a9d0ca9812f13_48.png",
			"https://avatars.slack-edge.com/2020-03-15/1003674990998_1da5dfc4dccc027892ea_48.png",
			"https://avatars.slack-edge.com/2015-07-01/7095899671_dd9cb85d3bff59fa8d3e_48.jpg",
		 	"https://avatars.slack-edge.com/2018-07-10/397245895878_1c3f489e77690fbc894d_48.jpg",
			"https://avatars.slack-edge.com/2021-06-30/2249803705920_5fe2d9812e8f26a1b93d_48.jpg",
			"https://avatars.slack-edge.com/2024-06-24/7323755870836_5a458db67236e6217240_48.png",
			"https://avatars.slack-edge.com/2016-05-06/40781400358_5959c74f55f73a361636_48.png",
			"https://avatars.slack-edge.com/2017-01-30/133478059345_d63c8b35a5cf03a4c52d_48.jpg",
			"https://avatars.slack-edge.com/2016-08-11/68400788736_9867728078d19a36388c_48.jpg",
			"https://secure.gravatar.com/avatar/55894f5d61da5fc711b3ef2884cb9346.jpg?s=48",
			"https://secure.gravatar.com/avatar/9805a9a5c85e5033a3153d75c512cb7e.jpg?s=48",
			"https://secure.gravatar.com/avatar/30bc0a34f5be583a50533213c7ae553e.jpg?s=48",
			"https://secure.gravatar.com/avatar/022b0228cb8246573bf467fe3357dc3b.jpg?s=48",
			"https://secure.gravatar.com/avatar/86b42baa2f44e38986a221710c644fde.jpg?s=48",
			"https://secure.gravatar.com/avatar/2cdcf8d2ea8a19436333ee852fc77580.jpg?s=48",
		];
	}
}

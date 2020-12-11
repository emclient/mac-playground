using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

#if MAC
using Foundation;
#endif

namespace FormsTest.Experiments
{
	// This is to test a problem with http client connections related to the number of open file handles.
	// It creates a temporary file and then opens it again and again (using different methods) in batches, until opening fails.
	// After each batch of openings, it attempts to make an http connection using different types of message handlers.
	// One can se that different message handlers fail at different numbers of file openings.
	// You can disable setting rlimit in Program.cs to see how it affects the issue.

	public class FileDescriptors
	{
		const int batchSize = 200;
		const string url = "http://www.fermion.cz/blank.html";

		public interface FileOpener
		{
			bool Open(string path);
			void Close();
		}

#if MAC
		class NSFileHandlerOpener : FileOpener
		{
			NSFileHandle handle;
			public bool Open(string path) { return null != (handle = NSFileHandle.OpenRead(path)); }
			public void Close() { handle?.CloseFile(); }
		}
#endif

		class StreamOpener : FileOpener
		{
			FileStream stream;
			public bool Open(string path) { try { stream = new FileStream(path, FileMode.Open, FileAccess.Read); } catch { } return stream != null; }
			public void Close() { stream?.Close(); }
		}

		class Client : HttpClient
		{
			public string HandlerTypeName { get; set; }
			public Client(HttpMessageHandler handler, string handlerTypeName) : base(handler) { HandlerTypeName = handlerTypeName; }
		}

		public void RunTest()
		{
			Task.Run(async () => await OpenFilesAndMakeHttpRequests());
		}

		async Task OpenFilesAndMakeHttpRequests()
		{
			Console.WriteLine("FileDesriptors Test Begin");

#if MAC
			await OpenFilesAndMakeHttpRequests(() => new NSFileHandlerOpener(), "NSFileHandle");
#endif
			await OpenFilesAndMakeHttpRequests(() => new StreamOpener(), "FileStream");

			Console.WriteLine("FileDesriptors Test End");
		}

		async Task OpenFilesAndMakeHttpRequests(Func<FileOpener> makeOpener, string openerName)
		{
			var path = Path.GetTempFileName();

			var makers = new Func<Client>[] {
#if MAC
				() => { return new Client(new CFNetworkHandler(), "CFNetworkHandler"); },
				() => { return new Client(new NSUrlSessionHandler(), "NSUrlSessionHandler"); },
#endif
				() => { return new Client(CreateMonoWebRequestHandler(), "MonoWebRequestHandler"); },
			};

			var openers = new List<FileOpener>();

			Console.WriteLine($"Opening files with {openerName} ---------------");
			while (true)
			{
				Console.WriteLine($"Opening {batchSize} files");
				OpenFiles(openers, makeOpener, batchSize, path);

				Console.WriteLine($"{openers.Count} files. Testing connections...");
				if (!await MakeRequests(makers))
					break;
			}

			Console.WriteLine($"Closing {openers.Count} file(s)");
			CloseFiles(openers);

			File.Delete(path);
			GC.Collect();
		}

		void CloseFiles(List<FileOpener> openers)
		{
			foreach (var opener in openers)
				opener.Close();
			openers.Clear();
		}

		void OpenFiles(List<FileOpener> openers, Func<FileOpener> makeOpener, int count, string path)
		{
			for (int i = 0; i < count; ++i)
				if (makeOpener() is FileOpener opener)
					if (opener.Open(path))
						openers.Add(opener);
					else
						return;
		}

		async Task<bool> MakeRequests(Func<Client>[] makers)
		{
			int succ = 0;
			for (int i = 0; i < makers.Length; ++i)
				if (makers[i] != null)
					if (await MakeRequest(makers[i], url))
						++succ;
					else
						makers[i] = null;

			return succ != 0;
		}

		async Task<bool> MakeRequest(Func<Client> maker, string url)
		{
			Client client = null;
			try
			{
				using (client = maker())
				using (var response = await client.GetAsync(url))
					return response.StatusCode == HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{client?.HandlerTypeName} Connection Failed: {ex}");
				return false;
			}
		}

		public static HttpClientHandler CreateMonoWebRequestHandler()
		{
			var monoHandlerType = Type.GetType("System.Net.Http.MonoWebRequestHandler, System.Net.Http");
			if (monoHandlerType != null)
			{
				var internalMonoHandlerCtors = monoHandlerType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				if (internalMonoHandlerCtors.Length < 1)
					throw new InvalidOperationException("Internal parameter-less constructor for `System.Net.Http.MonoWebRequestHandler` was not found.");

				object internalMonoHandler = internalMonoHandlerCtors[0].Invoke(null);
				var httpClientHandlerCtors = typeof(HttpClientHandler).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

				if (httpClientHandlerCtors.Length < 1)
					throw new InvalidOperationException("`internal HttpClientHandler(IMonoHttpClientHandler)` constructor was not found in `System.Net.Http.HttpClientHandler`.");

				return (HttpClientHandler)httpClientHandlerCtors[0].Invoke(new[] { internalMonoHandler });
			}
			else
			{
				return new HttpClientHandler();
			}
		}
	}
}

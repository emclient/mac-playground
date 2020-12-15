using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
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

		public enum MessageHandlerType
		{
			SocketsHttpHandler,
			MonoWebRequestHandler,
			CFNetworkHandler,
			NSUrlSessionHandler,
		}

		class Client : HttpClient
		{
			public static bool PoolingEnabled { get; set; }
			public static int Step { get; set; }

			protected static Dictionary<MessageHandlerType, ConcurrentQueue<Client>> pools;

			public MessageHandlerType HandlerType { get; private set; }
			public string HandlerTypeName { get => HandlerType.ToString(); }
			public int Id { get => id; }

			static int counter;
			protected int id;

			static Client()
			{
				pools = new Dictionary<MessageHandlerType, ConcurrentQueue<Client>>();
				pools.Add(MessageHandlerType.SocketsHttpHandler, new ConcurrentQueue<Client>());
				pools.Add(MessageHandlerType.MonoWebRequestHandler, new ConcurrentQueue<Client>());
			}

			public Client(MessageHandlerType type)
				: this(CreateMessageHandler(type))
			{
				HandlerType = type;
			}

			protected Client(HttpMessageHandler handler) : base(handler)
			{
				id = Interlocked.Increment(ref counter);
				Console.WriteLine($"Created: {id}");
			}

			public static HttpMessageHandler CreateMessageHandler(MessageHandlerType type)
			{
				switch (type)
				{
					default:
#if MAC
					case MessageHandlerType.SocketsHttpHandler: return new SocketsHttpHandler();
					case MessageHandlerType.CFNetworkHandler: return new CFNetworkHandler();
					case MessageHandlerType.NSUrlSessionHandler: return new NSUrlSessionHandler();
#endif
					case MessageHandlerType.MonoWebRequestHandler: return CreateMonoWebRequestHandler();
				}
			}

			protected override void Dispose(bool disposing)
			{
				Console.WriteLine($"Disposing: {id}");

				if (IsPooled(HandlerType))
				{
					if (PoolingEnabled && disposing)
						pools[HandlerType].Enqueue(this);
					else
						base.Dispose(disposing);
				}
				else
					base.Dispose(disposing);
			}

			public static void Imbue(int count, MessageHandlerType type)
			{
				if (!PoolingEnabled || !IsPooled(type))
					return;

				Console.WriteLine($"Imbue({count}), existing:{pools[type].Count}");
				for (int i = 0; i < count; ++i)
				{
					var client = new Client(type);
					using (var response = client.GetAsync(url))
						response.Wait();
					pools[type].Enqueue(client);
				}
			}

			public static Client Create(MessageHandlerType type)
			{
				if (!PoolingEnabled || !IsPooled(type))
					return new Client(type);

				if (pools[type].Count == 0)
					Imbue(Step, type);

				if (pools[type].TryDequeue(out var client))
				{
					Console.WriteLine($"Dequeued: {client.Id}");
					return client;
				}

				Console.WriteLine($"Dequeue failed, creating");

				return new Client(type);
			}

			public static bool IsPooled(MessageHandlerType type)
			{
				switch (type)
				{
					case MessageHandlerType.SocketsHttpHandler:
					case MessageHandlerType.MonoWebRequestHandler:
						return true;
				}
				return false;
			}
		}

		public void RunTest()
		{
			Task.Run(async () => await OpenFilesAndMakeHttpRequests());
		}

		async Task OpenFilesAndMakeHttpRequests()
		{
			Console.WriteLine("FileDesriptors Test Begin");

			Client.PoolingEnabled = true;
			Client.Imbue(20, MessageHandlerType.MonoWebRequestHandler);
			Client.Imbue(20, MessageHandlerType.SocketsHttpHandler);

			var path = Path.GetTempFileName();

			var makers = new Func<Client>[] {
#if MAC
				() => { return new Client(MessageHandlerType.CFNetworkHandler); },
				() => { return new Client(MessageHandlerType.NSUrlSessionHandler); },
#endif
				() => { return new Client(MessageHandlerType.SocketsHttpHandler); },
				() => { return new Client(MessageHandlerType.MonoWebRequestHandler); },
			};

			var files = new List<FileStream>();

			Console.WriteLine($"Opening files ---------------");
			while (true)
			{
				Console.WriteLine($"Opening {batchSize} files");
				OpenFiles(files, batchSize, path);

				Console.WriteLine($"{files.Count} files. Testing connections...");
				if (!await MakeRequests(makers))
					break;
			}

			Console.WriteLine($"Closing {files.Count} file(s)");
			CloseFiles(files);

			File.Delete(path);
			GC.Collect();

			Console.WriteLine("FileDesriptors Test End");
		}

		void CloseFiles(List<FileStream> files)
		{
			foreach (var fle in files)
				fle.Close();
			files.Clear();
		}

		void OpenFiles(List<FileStream> streams, int count, string path)
		{
			for (int i = 0; i < count; ++i)
				try { streams.Add(new FileStream(path, FileMode.Open, FileAccess.Read));
				} catch { return; }
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

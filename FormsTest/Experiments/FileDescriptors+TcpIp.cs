/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FormsTest.Experiments
{
	public class FileDescriptors_TcpIp
	{
		const int batchSize = 200;
		const string url = "http://www.fermion.cz/blank.html";

		public FileDescriptors_TcpIp()
		{
		}

		public void RunTest()
		{
			Task.Run(async () => await OpenFilesAndMakeHttpRequests());
		}

		async Task OpenFilesAndMakeHttpRequests()
		{
			Console.WriteLine("FileDesriptors Test Begin");

			var path = Path.GetTempFileName();

			var files = new List<FileStream>();

			Console.WriteLine($"Opening files ---------------");
			while (true)
			{
				Console.WriteLine($"Opening {batchSize} files");
				OpenFiles(files, batchSize, path);

				Console.WriteLine($"{files.Count} files. Testing connections...");
				if (!await MakeRequests())
					break;
			}

			Console.WriteLine($"Closing {files.Count} file(s)");
			CloseFiles(files);

			File.Delete(path);
			GC.Collect();

			Console.WriteLine("FileDesriptors Test End");
		}

		private Task<bool> MakeRequests()
		{

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
				try
				{
					streams.Add(new FileStream(path, FileMode.Open, FileAccess.Read));
				}
				catch { return; }
		}

	}
}
*/
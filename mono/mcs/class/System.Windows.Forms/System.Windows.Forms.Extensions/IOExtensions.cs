using System;
using System.IO;
using System.Text;

namespace System.Windows.Forms.Extensions.IO
{
	public static class IOExtensions
	{
		public static byte[] ToArray(this Stream stream)
		{
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static string ToString(this Stream stream, Encoding encoding)
		{
			var bytes = stream.ToArray();
			return encoding.GetString(bytes);
		}

		public static MemoryStream ToStream(this string text, Encoding encoding)
		{
			var bytes = encoding.GetBytes(text);
			return new MemoryStream(bytes);
		}
	}
}

using System;
using System.IO;
using System.Text;
using System.Windows.Forms.Extensions.IO;

namespace System.Windows.Forms
{
	internal class DataObjectWrapper : IDataObject
	{
		IDataObject innerData;

		public DataObjectWrapper(IDataObject data)
		{
			innerData = data;
		}

		#region IDataObject

		public object GetData(string format)
		{
			var data = innerData.GetData(format);
			return Unwrap(format, data);
		}

		public object GetData(string format, bool autoConvert)
		{
			var data = innerData.GetData(format, autoConvert);
			return Unwrap(format, data);
		}

		public object GetData(Type format)
		{
			return innerData.GetData(format);
		}

		public bool GetDataPresent(string format)
		{
			return innerData.GetDataPresent(format);
		}

		public bool GetDataPresent(string format, bool autoConvert)
		{
			return innerData.GetDataPresent(format, autoConvert);
		}

		public bool GetDataPresent(Type format)
		{
			return innerData.GetDataPresent(format);
		}

		public string[] GetFormats()
		{
			return innerData.GetFormats();
		}

		public string[] GetFormats(bool autoConvert)
		{
			return innerData.GetFormats(autoConvert);
		}

		public void SetData(object data)
		{
			innerData.SetData(data);
		}

		public void SetData(string format, bool autoConvert, object data)
		{
			innerData.SetData(format, autoConvert, data);
		}

		public void SetData(string format, object data)
		{
			innerData.SetData(format, data);
		}

		public void SetData(Type format, object data)
		{
			innerData.SetData(format, data);
		}

		#endregion

		#region internals

		object Unwrap(string format, object data)
		{
			switch (format)
			{
				case DataFormats.Rtf:
				case DataFormats.Html:
				case DataFormats.Text:
					{ return data is Stream stream ? stream.ToString(Encoding.UTF8).Replace("\0", "") : data as string; }
				case DataFormats.UnicodeText:
					{ return data is Stream stream ? stream.ToString(Encoding.Unicode).Replace("\0", "") : data as string; }
			}

			return data;
		}

		#endregion
	}
}

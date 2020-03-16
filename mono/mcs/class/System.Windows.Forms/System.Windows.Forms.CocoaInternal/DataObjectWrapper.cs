using System.IO;
using System.Text;
using System.Windows.Forms.Extensions.IO;

namespace System.Windows.Forms.CocoaInternal
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
			var data = innerData.GetData(AlterFormat(format));
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

		public void Wipe()
		{
			// Get rid of possible references to whatever that cannot be read during app terminaion.
			if (!(innerData is DataObject))
				innerData = new DataObject();
		}

		#region internals

		string AlterFormat(string format)
		{
			switch (format)
			{
				case DataFormats.HtmlStream: return DataFormats.Html;
				default: return format;
			}
		}

		object Unwrap(string format, object data)
		{
			switch (format)
			{
				case DataFormats.Rtf:
				case DataFormats.Html:
				case DataFormats.Text:
					return UnwrapText(format, data, Encoding.UTF8);
				case DataFormats.UnicodeText:
					return UnwrapText(format, data, Encoding.Unicode);
				case DataFormats.HtmlStream:
					return data is string s ? s.ToStream(Encoding.UTF8) : null;
			}
			return data;
		}

		string UnwrapText(string format, object data, Encoding encoding)
		{
			if (data is Stream stream)
			{
				data = stream.ToString(encoding).Replace("\0", "");
				innerData.SetData(format, data);
			}
			return data as String;
		}
		#endregion
	}
}

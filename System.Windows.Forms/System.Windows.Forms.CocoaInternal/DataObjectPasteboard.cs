using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms.Extensions.IO;
using System.Windows.Forms.CocoaInternal;
using CoreGraphics;
using System.Windows.Forms.Mac;

#if XAMARINMAC
using Foundation;
using AppKit;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
#endif

namespace System.Windows.Forms.CocoaInternal
{
	internal class DataObjectPasteboard : IDataObject
	{
		NSPasteboard pboard;

		public DataObjectPasteboard(NSPasteboard pboard)
		{
			this.pboard = pboard;
		}

		#region IDataObject

		public object GetData(string format)
		{
			return pboard.GetData(format, true);
		}

		public object GetData(string format, bool autoConvert)
		{
			return pboard.GetData(format, true);
		}

		public object GetData(Type format)
		{
			return pboard.GetData(format.FullName, true);
		}

		public bool GetDataPresent(string format)
		{
			return -1 != Array.IndexOf(GetFormats(), format);
		}

		public bool GetDataPresent(string format, bool autoConvert)
		{
			return -1 != Array.IndexOf(GetFormats(), format);
		}

		public bool GetDataPresent(Type format)
		{
			return false;
		}

		public string[] GetFormats()
		{
			return pboard.GetFormats();
		}

		public string[] GetFormats(bool autoConvert)
		{
			return pboard.GetFormats();
		}

		public void SetData(object data)
		{
		}

		public void SetData(string format, bool autoConvert, object data)
		{
		}

		public void SetData(string format, object data)
		{
		}

		public void SetData(Type format, object data)
		{
		}

		#endregion
	}
}

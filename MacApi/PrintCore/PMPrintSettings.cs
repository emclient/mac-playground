/*using System;
using System.Runtime.InteropServices;

namespace MacApi.PrintCore
{
	public class PMPrintSettings : PMPrintCoreBase
	{
		public bool Collate
		{
			get;
			set;
		}

		public uint Copies
		{
			get;
			set;
		}

		public PMDuplexMode DuplexMode
		{
			get;
			set;
		}

		public uint FirstPage
		{
			get;
			set;
		}

		public uint LastPage
		{
			get;
			set;
		}

		public double Scale
		{
			get;
			set;
		}

		//
		// Constructors
		//
		internal PMPrintSettings(IntPtr handle, bool owns)
		{
		}

		public PMPrintSettings()
		{
		}

		//
		// Static Methods
		//
		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMCopyPrintSettings(IntPtr source, IntPtr dest);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMCreatePrintSettings(out IntPtr session);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetCollate(IntPtr handle, out byte collate);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetCopies(IntPtr handle, out uint copies);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetDuplex(IntPtr handle, out PMDuplexMode mode);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetFirstPage(IntPtr handle, out uint first);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetLastPage(IntPtr handle, out uint last);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetPageRange(IntPtr handle, out uint minPage, out uint maxPage);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMGetScale(IntPtr handle, out double mode);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetCollate(IntPtr handle, byte collate);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetCopies(IntPtr handle, uint copies, byte elock);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetDuplex(IntPtr handle, PMDuplexMode mode);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetFirstPage(IntPtr handle, uint first, byte lockb);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetLastPage(IntPtr handle, uint last, byte lockb);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetPageRange(IntPtr handle, uint minPage, uint maxPage);

		[DllImport(Constants.PrintCoreLibrary)]
		private static extern PMStatusCode PMSetScale(IntPtr handle, double scale);

		public static PMStatusCode TryCreate(out PMPrintSettings settings)
		{
			settings = null;
			return (PMStatusCode)0;
		}

		public PMStatusCode CopySettings(PMPrintSettings destination)
		{
			return (PMStatusCode)0;
		}

		public PMStatusCode GetPageRange(out uint minPage, out uint maxPage)
		{
			minPage = 0;
			maxPage = 0;
			return (PMStatusCode)0;
		}

		public PMStatusCode SetPageRange(uint minPage, uint maxPage)
		{
			return (PMStatusCode)0;
		}
	}
}
*/
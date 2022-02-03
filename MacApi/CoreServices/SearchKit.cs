using System;
using System.Runtime.InteropServices;
using CoreFoundation;
using Foundation;
using ObjCRuntime;
using SearchKit;

namespace MacApi.CoreServices.SearchKit
{

	public static class SearchKitExtensions
	{
		public static long GetDocumentID(this SKIndex index, SKDocument document)
		{
			return SKIndexGetDocumentID(index.Handle, document.Handle);
		}

		public static int GetDocumentState(this SKIndex index, SKDocument document)
		{
			return SKIndexGetDocumentState(index.Handle, document.Handle);
		}

		public static string[] GetTerms(this SKIndex index, SKDocument document)
		{
			var docID = index.GetDocumentID(document);
			var termIDs = SKIndexCopyTermIDArrayForDocumentID(index.Handle, docID);
			return CFArray.ArrayFromHandleFunc(termIDs, (handle) => 
			{
				using var termId = Runtime.GetNSObject<NSNumber>(handle, false);
				var term = SKIndexCopyTermStringForTermID(index.Handle, termId.Int64Value);
				return CFString.FromHandle(term, true);
			}, false);
		}

		[DllImport (Constants.SearchKitLibrary)]
		public static extern void SKLoadDefaultExtractorPlugIns();

		[DllImport (Constants.SearchKitLibrary)]
		public static extern long SKIndexGetDocumentID(IntPtr index, IntPtr document);

		[DllImport (Constants.SearchKitLibrary)]
		public static extern int SKIndexGetDocumentState(IntPtr index, IntPtr document);

		[DllImport (Constants.SearchKitLibrary)]
		public static extern IntPtr SKIndexCopyTermIDArrayForDocumentID(IntPtr index, long id);

		[DllImport (Constants.SearchKitLibrary)]
		public static extern IntPtr SKIndexCopyTermStringForTermID(IntPtr index, long id);
	}
}

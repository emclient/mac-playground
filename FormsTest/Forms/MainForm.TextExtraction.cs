#if MAC
using System;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CoreFoundation;
using Foundation;
using UniformTypeIdentifiers;
using ObjCRuntime;
using MacApi;
using SearchKit;
#endif

namespace FormsTest
{
#if MAC
	public partial class MainForm
	{
		void TextExtractionTest()
		{
			using var dlg = new OpenFileDialog();
			dlg.Multiselect = true;
			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			foreach (var path in  dlg.FileNames)
			{
				ExtractTextWithSearchKit(path);
				ExtractTextWithAttributedString(path);
			}
		}

		void ExtractTextWithAttributedString(string path)
		{
			var ids = GetTextTypes();
			var utis = ids.Select(x => UTType.CreateFromIdentifier(x));
			Console.WriteLine($"textTypes: {string.Join('\n', ids)}");

			var ext = System.IO.Path.GetExtension(path).TrimStart('.');
			Console.WriteLine($"--- {ext} -------------");
			Console.WriteLine($"{path}");

			var type = UTType.CreateFromExtension(ext);
			Console.WriteLine($"UTI:{type.Identifier}");

			var found = null != utis.FirstOrDefault(x => type.ConformsTo(x));
			Console.WriteLine($"### {ext} SUPPORTED:{found}");

			if (!found)
				return;

			var data = NSData.FromFile(path);
			if (data == null)
				return;

			var options = new NSAttributedStringDocumentAttributes();
			var str = new NSAttributedString(data, options.Dictionary, out var attributes, out var error);

			if (str == null || error != null)
				return;
			
			var keys = attributes.Keys;
			foreach(var key in keys)
			{
				var value = attributes[key];
				Console.WriteLine($"{key}:{value}");
			}
		}
		
		public static string[] GetTextTypes()
		{
			var cls = Class.GetHandle(typeof(NSAttributedString));
			var sel = Selector.GetHandle("textTypes");
			var ptr = LibObjc.IntPtr_objc_msgSend(cls, sel);
			var arr = NSArray.ArrayFromHandle<NSString>(ptr);
			var types = new string[arr.Length];
			for (var i = 0; i < arr.Length; ++i)
				types[(int)i] = arr.GetValue(i).ToString();
			return types;
		}

		static bool defaultExtractorsLoaded;
		void ExtractTextWithSearchKit(string path)
		{
			if (!defaultExtractorsLoaded)
			{
				SearchKitExtensions.SKLoadDefaultExtractorPlugIns();
				defaultExtractorsLoaded = true;				
			}

			var terms = ExtractTermsFromFile(path);
			Console.WriteLine($"--- Terms from: {path} ");
			Console.WriteLine(string.Join(" ", terms));
		}

		string[] ExtractTermsFromFile(string path)
		{
			var name = System.IO.Path.GetFileName(path);
			var url = new NSUrl(path, false);
			using var document = new SKDocument(url);
			using var data = new NSMutableData();
			var properties = new SKTextAnalysis { MinTermLength = 3 };
			using var index = SKIndex.CreateWithMutableData(data, name, SKIndexType.Inverted, properties);
			var docId = index.GetDocumentID(document);
			var hint = string.Empty;
			bool added = index.AddDocument(document, hint, true);
			var state = index.GetDocumentState(document);
			index.Flush();
			return index.GetTerms(document);
		}

	}

	public static class SearchKitExtensions
	{
		struct Constants
		{
			public const string SearchKitLibrary = "/System/Library/Frameworks/CoreServices.framework/Versions/Current/Frameworks/SearchKit.framework/Versions/Current/SearchKit";
		}

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
				using var termId = ObjCRuntime.Runtime.GetNSObject<NSNumber>(handle, false);
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
#endif
}

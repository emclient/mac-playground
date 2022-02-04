#if MAC
using System;
using System.Linq;
using System.Windows.Forms;
using CoreData;
using Foundation;
using UniformTypeIdentifiers;
using ObjCRuntime;
using MacApi;
using MacApi.CoreServices.SearchKit;
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

		void ExtractTextWithSearchKit(string path)
		{
			SearchKitExtensions.LoadDefaultExtractorPlugIns();
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

		void CoreDataTest()
		{
			var entity = new NSEntityDescription { 
				Name = "Wrapper",
				ManagedObjectClassName = "Wrapper",
				Properties = new NSPropertyDescription[] {
					new NSAttributeDescription { Name = "FileName", AttributeType = NSAttributeType.String, Optional = true },
					new NSAttributeDescription { Name = "FileData", AttributeType = NSAttributeType.Binary, Optional = true },
				}
			};

			var model = new NSManagedObjectModel() { Entities = new NSEntityDescription[] { entity } };
			var coordinator = new NSPersistentStoreCoordinator(model);
			var store = coordinator.AddPersistentStoreWithType(NSPersistentStoreCoordinator.InMemoryStoreType, null, null, null, out var error);
			
			var context = new NSManagedObjectContext { PersistentStoreCoordinator = coordinator };

			var wrapper = NSEntityDescription.InsertNewObjectForEntityForName("Wrapper", context) as Wrapper;
			wrapper.FileName = new NSString("Attachment.txt");
			wrapper.FileData = NSData.FromString("Attachment content");

			context.Save(out error);

			using var uri = wrapper.ObjectID.URIRepresentation;
			Console.WriteLine($"CoreData URI: {uri}");
		}

		[Register("Wrapper")]
		public class Wrapper : NSManagedObject 
		{
			public NSString? FileName { get; set; }
			public NSData? FileData { get; set; }
			public Wrapper(IntPtr ptr) : base(ptr) {}
			public Wrapper(NSEntityDescription description, NSManagedObjectContext context) : base(description, context) {}
		}
	}
#endif
}

using Foundation;
using MobileCoreServices;
using ObjCRuntime;
using System.Linq;

namespace MacApi.AppKit
{

	public static class NSAttributedStringExtensions
	{
		public static string[] GetNSAttributedStringTextTypes()
		{
			var cls = Class.GetHandle(typeof(NSAttributedString));
			var sel = Selector.GetHandle("textTypes")!;
			var ptr = LibObjc.IntPtr_objc_msgSend(cls, sel);
			var arr = NSArray.ArrayFromHandle<NSString>(ptr)!;
			var types = new string[arr.Length];
			for (var i = 0; i < arr.Length; ++i)
				if (arr.GetValue(i) is NSString identifier)
                    types[i] = identifier.ToString();
			return types;
		}

		static string[]? supportedTypes;
		public static string[] SupportedTypes
		{
			get
			{
				if (supportedTypes == null)
					supportedTypes = GetNSAttributedStringTextTypes();
				return supportedTypes;
			}
		}

		public static bool IsSupportedByNSAttributedString(this string uti)
		{
			return SupportedTypes.FirstOrDefault(x => UTType.ConformsTo(uti, x)) != null;
		}
	}
}
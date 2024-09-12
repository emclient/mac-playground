using Foundation;
using ObjCRuntime;
using System.Linq;
using UniformTypeIdentifiers;

namespace MacApi.Foundation
{
	public static class NSAttributedStringExtensions
	{
		public static UTType[] GetNSAttributedStringTextTypes()
		{
			var cls = Class.GetHandle(typeof(NSAttributedString));
			var sel = Selector.GetHandle("textTypes")!;
			var ptr = LibObjc.IntPtr_objc_msgSend(cls, sel);
			var arr = NSArray.ArrayFromHandle<NSString>(ptr)!;
			var types = new UTType[arr.Length];
			for (var i = 0; i < arr.Length; ++i)
				if (arr.GetValue(i) is NSString identifier)
                    types[i] = UTType.CreateFromIdentifier(identifier.ToString())!;
			return types;
		}

		static UTType[]? supportedTypes;
		public static UTType[] SupportedTypes
		{
			get
			{
				if (supportedTypes == null)
					supportedTypes = GetNSAttributedStringTextTypes();
				return supportedTypes;
			}
		}

		public static bool IsSupportedByNSAttributedString(this UTType type)
		{
			return SupportedTypes.FirstOrDefault(x => type.ConformsTo(x)) != null;
		}
	}
}

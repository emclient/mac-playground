using MacApi;
using System.Diagnostics;
using System.Windows.Forms.Mac;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace System.Windows.Forms.CocoaInternal
{
	[Register("MonoApplication")]
	internal partial class MonoApplication : NSApplication
	{
		internal delegate IntPtr CreateSharedApplicationDelegate();
		internal static Swizzle<CreateSharedApplicationDelegate> shared;
		internal static NSApplication instance;

		public static NSApplication CreateShared()
		{
			if (instance == null)
			{
				instance = CreateFromInfoPlist() ?? new MonoApplication(New(typeof(MonoApplication)));
				shared = new Swizzle<CreateSharedApplicationDelegate>(typeof(NSApplication), "sharedApplication", GetSharedApplication, true);
			}
			return instance;
		}

		public MonoApplication(NativeHandle handle) : base(handle)
		{
			SetupDelegate();
		}

		internal static IntPtr New(Type type)
		{
			return LibObjc.IntPtr_objc_msgSend(Class.GetHandle(type), Selector.GetHandle("new"));
		}

		public static IntPtr GetSharedApplication()
		{
			return instance.Handle;
		}

		internal static NSApplication CreateFromInfoPlist()
		{
			string name = null;
			Exception exception = null;
			try
			{
				name = (NSBundle.MainBundle.InfoDictionary["NSPrincipalClass"] as NSString)?.ToString();
				if (string.IsNullOrEmpty(name) || name == "NSApplication" || name == "MonoApplication" )
					return null;

				if (Type.GetType(name) is Type type)
					if (Activator.CreateInstance(type, new object[] { New(type) }) is NSApplication app)
						return app;
			}
			catch (Exception e)
			{
				exception = e;
			}

			Debug.Assert(false, $"Failed creating principal class from Info.plist: \"{name}\". Exception: {exception}");
			return null;
		}

		// This is necessary for proper window behavior if we do not use the "run" method, which is our case
		public override bool Running
		{
			get { return Application.MessageLoop; }
		}
	}
}

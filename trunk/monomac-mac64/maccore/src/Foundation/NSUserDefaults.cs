namespace MonoMac.Foundation {

	public partial class NSUserDefaults {

                public void SetString (string value, string defaultName)
                {
			NSString str = new NSString (value);

			SetObjectForKey (str, defaultName);
			
			str.Dispose ();
		}

		public NSObject this [string key] {
			get {
				return ObjectForKey (key);
			}

			set {
				SetObjectForKey (value, key);
			}
		}
	}
	
}

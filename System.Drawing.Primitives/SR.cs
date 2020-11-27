namespace System
{
	class SR
	{
		public static string InvalidEx2BoundArgument = "Value of '{1}' is not valid for '{0}'. '{0}' should be greater than or equal to {2} and less than or equal to {3}.";
		public static string InvalidColor = "Color '{0}' is not valid.";
		public static string ConvertInvalidPrimitive = "{0} is not a valid value for {1}.";
	        internal static string Format(string resourceFormat, params object[] args) => string.Format(resourceFormat, args);
	}
}
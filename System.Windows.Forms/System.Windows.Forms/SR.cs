namespace System
{
	class SR
	{
		public static string InvalidResXBasePathOperation = "Property can be changed only before the enumeration.";
		public static string SerializationException = "Type {0} could not be read from the data in line {1}, position {2}.  The type's internal structure may have changed.  Either implement ISerializable on the type or provide a type converter that can provide a more reliable conversion format, such as text or an array of bytes.  The conversion exception was: {3}";
		public static string InvocationException = "Type {0} in the data at line {1}, position {2}, cannot be loaded because it threw the following exception during construction: {3}";
		public static string InvalidResXFile = "ResX file {0} cannot be parsed.";
		public static string InvalidResXFileReaderWriterTypes = "ResX input is not valid. Cannot find valid \"resheader\" tags for the ResX reader and writer type names.";
		public static string InvalidResXNoType = "Could not find a type for a name.  The type name was '{0}'.";
		public static string InvalidResXResourceNoName = "Cannot find a name for the resource with the value '{0}'.";
		public static string ResXResourceWriterSaved = "Resource writer has been saved.  You may not edit it.";
		public static string NotSerializableType = "Item named '{0}' of type '{1}' cannot be added to the resource file because it is not serializable.";
		public static string TypeLoadException = "Type {0} in the data at line {1}, position {2} cannot be located.";
		public static string TypeLoadExceptionShort = "Type {0} cannot be located.";
		public static string NotSupported = "The type {0} on line {1}, position {2} threw the following exception while being converted: {3}";
	}
}
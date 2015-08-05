//
// MonoMac defines for docfixer
//
using System;
using System.Reflection;
using MonoMac.Foundation;

public partial class DocGenerator {
	static Assembly assembly = typeof (MonoMac.Foundation.NSObject).Assembly;
	const string BaseNamespace = "MonoMac";
	const string ArchiveName = "monomac-lib.zip";
	
	static string GetMostRecentDocBase ()
	{
		return "/Developer/Documentation/DocSets/com.apple.adc.documentation.AppleSnowLeopard.CoreReference.docset/Contents/Resources/Documents/documentation";
	}

	public static string GetSelector (object attr)
	{
		return ((ExportAttribute) attr).Selector;
	}
}
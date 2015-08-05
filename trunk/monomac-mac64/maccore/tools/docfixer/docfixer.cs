//
// docfixer
//
// TODO:
//   Remove <h2...> Overview</h2> from merged docs
//
// Copyright 2012 Xamarin Inc
//
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using macdoc;
using Mono.Options;
using Mono.Cecil;

public partial class DocGenerator
{
	public static int Main (string[] args)
	{
		try {
			return Main2 (args);
		} catch (Exception ex) {
			Console.WriteLine ("Unexpected exception: {0}", ex);
			return 1;
		}
	}

	public static int Main2 (string[] args)
	{
		bool show_help = false;
		bool useRawDoc = false;
		var mergerOptions = new AppleDocMerger.Options ();

		var options = new OptionSet () {
			{ "apple-doc-dir=",
			  "{DIRECTORY} for the Apple documentation.",
			  v => mergerOptions.DocBase = Path.Combine (v, "Contents/Resources/Documents/documentation") },
			{ "reference-assembly=",
			  "{ASSEMBLY} file containing the types to find documentation for.",
			  v => mergerOptions.Assembly = AssemblyDefinition.ReadAssembly (v) },
			{ "ns-prefix=",
			  "{NAMESPACE} prefix for the types to parse.",
			  v => mergerOptions.BaseAssemblyNamespace = v },
			{ "import-samples",
			  "If set, the tool will import samples from an external repository",
			  v => mergerOptions.ImportSamples = v != null },
			{ "samples-repository=",
			  "Tell where to find the sample .zip repository",
			  v => mergerOptions.SamplesRepositoryPath = v},
			{ "use-raw-doc",
			  "Process uncompiled mdoc documentation rather than a bundle",
			  v => useRawDoc = v != null},
			{ "v|verbose",
			  "Verbose output",
			  v => mergerOptions.MergingPathCallback = v != null ? p => Console.WriteLine (p) : (Action<string>)null },
			{ "h|?|help",
			  "Show this help message and exit.",
			  v => show_help = v != null },
		};

		string monodocPath = null;
		try {
			monodocPath = options.Parse (args).FirstOrDefault ();
		}
		catch (Exception e) {
			Console.Error.WriteLine (e.ToString ());
			Console.Error.WriteLine ("Use '--help' for more information.");
			return 1;
		}

		if (string.IsNullOrEmpty (monodocPath) || show_help) {
			ShowHelp (options);
			return 0;
		}

		IMdocArchive mdocArchive = null;
		if (useRawDoc)
			mdocArchive = new MDocDirectoryArchive (Path.Combine (monodocPath, "en"));
		else
			mdocArchive = MDocZipArchive.ExtractAndLoad (Path.Combine (monodocPath, ArchiveName));
		mergerOptions.MonodocArchive = mdocArchive;
		var merger = new AppleDocMerger (mergerOptions);
		merger.MergeDocumentation ();
		mdocArchive.CommitChanges ();

		return 0;
	}
	
	static void ShowHelp (OptionSet options)
	{
		Console.WriteLine ("Usage: docfixer [OPTIONS]+ MONODOC_LIB_PATH");
		Console.WriteLine ();
		Console.WriteLine ("Merge Apple documentation into mdoc(5) documentation.");
		Console.WriteLine ();
		Console.WriteLine ("Available Options:");
		options.WriteOptionDescriptions (Console.Out);
		Console.WriteLine ();
		Console.WriteLine ("Copyright (C) 2012, Novell, Inc.");
	}
}

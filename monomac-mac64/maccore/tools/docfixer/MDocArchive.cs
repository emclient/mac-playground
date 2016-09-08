// Copyright 2012 Xamarin Inc

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using Ionic.Zip;
using Mono.Cecil;

namespace macdoc
{
	public interface IMdocArchive
	{
		string GetPathForType (TypeDefinition t);
		void CommitChanges ();
	}
	
	// This is an uncompiled 'en' directory of ECMA documentation
	public class MDocDirectoryArchive : IMdocArchive
	{
		string baseDir;
		
		public MDocDirectoryArchive (string baseDir)
		{
			this.baseDir = baseDir;
		}
		
		public string GetPathForType (TypeDefinition t)
		{
			return Path.Combine (baseDir, t.Namespace, t.Name + ".xml");
		}
		
		public void CommitChanges ()
		{
			
		}
	}
	
	// This represent a .zip archive of a ECMA API doc set
	public class MDocZipArchive : IMdocArchive
	{
		const string originSuffix = ".origin";
		
		string baseDir;
		string originalArchivePath;
		// Provide a mapping between a type full name (e.g. System.String) and the file name of its documentation (in ecma doc case, a number)
		Dictionary<string, string> typeMapping = new Dictionary<string, string> ();
		
		private MDocZipArchive (string originalArchivePath, string baseDir)
		{
			this.baseDir = baseDir;
			this.originalArchivePath = originalArchivePath;
			BuildTypeMapping ();
		}
		
		void BuildTypeMapping ()
		{
			int id;
			foreach (var file in Directory.EnumerateFiles (baseDir)) {
				var name = Path.GetFileName (file);
				if (!int.TryParse (name, out id))
					continue;
				using (var reader = XmlReader.Create (file)) {
					if (!reader.Read ())
						continue;
					if (!reader.MoveToAttribute ("FullName"))
						continue;
					var typeFullName = reader.ReadContentAsString ();
					if (string.IsNullOrEmpty (typeFullName))
						continue;
					typeMapping[typeFullName] = file;
				}
			}
		}
		
		public string GetPathForType (TypeDefinition t)
		{
			string path;
			bool result = typeMapping.TryGetValue (t.FullName, out path);
			return result ? path : null;
		}
		
		public static MDocZipArchive ExtractAndLoad (string archivePath)
		{
			if (!File.Exists (archivePath))
				throw new ArgumentException ("Archive file doesn't exists", "archivePath");
			// If there is an existing .origin file it means some merging operation has already happened
			// and so we need to start at a clean state
			if (File.Exists (archivePath + originSuffix))
				File.Copy (archivePath + originSuffix, archivePath, true);
			
			var extractionDir = Path.Combine (Path.GetTempPath (), Path.GetFileNameWithoutExtension (archivePath));
			if (Directory.Exists (extractionDir))
				Directory.Delete (extractionDir, true);
			Directory.CreateDirectory (extractionDir);
			using (var zip = ZipFile.Read (archivePath))
				zip.ExtractAll (extractionDir);
			
			return new MDocZipArchive (archivePath, extractionDir);
		}
		
		public void CommitChanges ()
		{
			File.Copy (originalArchivePath, originalArchivePath + originSuffix, true);
			File.Delete (originalArchivePath);
			using (var zip = new ZipFile (originalArchivePath)) {
				zip.AddDirectory (baseDir);
				zip.Save ();
			}
		}
	}
}


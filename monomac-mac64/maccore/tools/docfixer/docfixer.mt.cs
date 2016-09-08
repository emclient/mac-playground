//
// MonoTouch defines for docfixer
//
// Copyright 2012 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
using System;
using System.IO;
using System.Reflection;

public partial class DocGenerator
{
	const string BaseNamespace = "MonoTouch";
	const string ArchiveName = "MonoTouch-lib.zip";

	static string GetMostRecentDocBase ()
	{
		var versions = new [] { "5_0" };
		var base_paths = new [] {
			"/Library/Developer/Shared/Documentation/DocSets/com.apple.adc.documentation.AppleiOS{0}.iOSLibrary.docset/Contents/Resources/Documents/documentation",
			"/Developer/Platforms/iPhoneOS.platform/Developer/Documentation/DocSets/com.apple.adc.documentation.AppleiOS{0}.iOSLibrary.docset/Contents/Resources/Documents/documentation",
		};

		foreach (var p in base_paths){
			foreach (var v in versions) {
				var d = string.Format (p, v);
				if (Directory.Exists (d)) {
					return d;
					break;
				}
			}
		}
		return null;
	}
}
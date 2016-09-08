// MobileCoreServices.UTType
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
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

using System;
using System.Runtime.InteropServices;
using MonoMac.ObjCRuntime;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;

namespace MonoMac.MobileCoreServices {
	
	public class UTType {
		
		public static NSString ExportedTypeDeclarationsKey;
		public static NSString ImportedTypeDeclarationsKey;
		public static NSString IdentifierKey;
		public static NSString TagSpecificationKey;
		public static NSString ConformsToKey;
		public static NSString DescriptionKey;
		public static NSString IconFileKey;
		public static NSString ReferenceURLKey;
		public static NSString VersionKey;

		public static NSString TagClassFilenameExtension;
		public static NSString TagClassMIMEType;
#if MONOMAC
		public static NSString TagClassNSPboardType;
		public static NSString TagClassOSType;
#endif
		public static NSString Item;
		public static NSString Content;
		public static NSString CompositeContent;
		public static NSString Application;
		public static NSString Message;
		public static NSString Contact;
		public static NSString Archive;
		public static NSString DiskImage;

		public static NSString Data;
		public static NSString Directory;
		public static NSString Resolvable;
		public static NSString SymLink;
		public static NSString MountPoint;
		public static NSString AliasFile;
		public static NSString AliasRecord;
		public static NSString URL;
		public static NSString FileURL;

		public static NSString Text;
		public static NSString PlainText;
		public static NSString UTF8PlainText;
		public static NSString UTF16ExternalPlainText;
		public static NSString UTF16PlainText;
		public static NSString RTF;
		public static NSString HTML;
		public static NSString XML;
		public static NSString SourceCode;
		public static NSString CSource;
		public static NSString ObjectiveCSource;
		public static NSString CPlusPlusSource;
		public static NSString ObjectiveCPlusPlusSource;
		public static NSString CHeader;
		public static NSString CPlusPlusHeader;
		public static NSString JavaSource;

		public static NSString PDF;
		public static NSString RTFD;
		public static NSString FlatRTFD;
		public static NSString TXNTextAndMultimediaData;
		public static NSString WebArchive;

		public static NSString Image;
		public static NSString JPEG;
		public static NSString JPEG2000;
		public static NSString TIFF;
		public static NSString PICT;
		public static NSString GIF;
		public static NSString PNG;
		public static NSString QuickTimeImage;
		public static NSString AppleICNS;
		public static NSString BMP;
		public static NSString ICO;

		public static NSString AudiovisualContent;
		public static NSString Movie;
		public static NSString Video;
		public static NSString Audio;
		public static NSString QuickTimeMovie;
		public static NSString MPEG;
		public static NSString MPEG4;
		public static NSString MP3;
		public static NSString MPEG4Audio;
		public static NSString AppleProtectedMPEG4Audio;

		public static NSString Folder;
		public static NSString Volume;
		public static NSString Package;
		public static NSString Bundle;
		public static NSString Framework;

		public static NSString ApplicationBundle;
		public static NSString ApplicationFile;

		public static NSString VCard;

		public static NSString InkText;
		
		static UTType ()
		{
			var handle = Dlfcn.dlopen (Constants.MobileCoreServicesLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			
			try {
				ExportedTypeDeclarationsKey = Dlfcn.GetStringConstant (handle, "kUTExportedTypeDeclarationsKey");
				ImportedTypeDeclarationsKey = Dlfcn.GetStringConstant (handle, "kUTImportedTypeDeclarationsKey");
				IdentifierKey = Dlfcn.GetStringConstant (handle, "kUTTypeIdentifierKey");
				TagSpecificationKey = Dlfcn.GetStringConstant (handle, "kUTTypeTagSpecificationKey");
				ConformsToKey = Dlfcn.GetStringConstant (handle, "kUTTypeConformsToKey");
				DescriptionKey = Dlfcn.GetStringConstant (handle, "kUTTypeDescriptionKey");
				IconFileKey = Dlfcn.GetStringConstant (handle, "kUTTypeIconFileKey");
				ReferenceURLKey = Dlfcn.GetStringConstant (handle, "kUTTypeReferenceURLKey");
				VersionKey = Dlfcn.GetStringConstant (handle, "kUTTypeVersionKey");
				
				TagClassFilenameExtension = Dlfcn.GetStringConstant (handle, "kUTTagClassFilenameExtension");
				TagClassMIMEType = Dlfcn.GetStringConstant (handle, "kUTTagClassMIMEType");
#if MONOMAC
				TagClassNSPboardType = Dlfcn.GetStringConstant (handle, "kUTTagClassNSPboardType");
				TagClassOSType = Dlfcn.GetStringConstant (handle, "kUTTagClassOSType");
#endif			
				Item = Dlfcn.GetStringConstant (handle, "kUTTypeItem");
				Content = Dlfcn.GetStringConstant (handle, "kUTTypeContent");
				CompositeContent = Dlfcn.GetStringConstant (handle, "kUTTypeCompositeContent");
				Application = Dlfcn.GetStringConstant (handle, "kUTTypeApplication");
				Message = Dlfcn.GetStringConstant (handle, "kUTTypeMessage");
				Contact = Dlfcn.GetStringConstant (handle, "kUTTypeContact");
				Archive = Dlfcn.GetStringConstant (handle, "kUTTypeArchive");
				DiskImage = Dlfcn.GetStringConstant (handle, "kUTTypeDiskImage");
				
				Data = Dlfcn.GetStringConstant (handle, "kUTTypeData");
				Directory = Dlfcn.GetStringConstant (handle, "kUTTypeDirectory");
				Resolvable = Dlfcn.GetStringConstant (handle, "kUTTypeResolvable");
				SymLink = Dlfcn.GetStringConstant (handle, "kUTTypeSymLink");
				MountPoint = Dlfcn.GetStringConstant (handle, "kUTTypeMountPoint");
				AliasFile = Dlfcn.GetStringConstant (handle, "kUTTypeAliasFile");
				AliasRecord = Dlfcn.GetStringConstant (handle, "kUTTypeAliasRecord");
				URL = Dlfcn.GetStringConstant (handle, "kUTTypeURL");
				FileURL = Dlfcn.GetStringConstant (handle, "kUTTypeFileURL");
				
				Text = Dlfcn.GetStringConstant (handle, "kUTTypeText");
				PlainText = Dlfcn.GetStringConstant (handle, "kUTTypePlainText");
				UTF8PlainText = Dlfcn.GetStringConstant (handle, "kUTTypeUTF8PlainText");
				UTF16ExternalPlainText = Dlfcn.GetStringConstant (handle, "kUTTypeUTF16ExternalPlainText");
				UTF16PlainText = Dlfcn.GetStringConstant (handle, "kUTTypeUTF16PlainText");
				RTF = Dlfcn.GetStringConstant (handle, "kUTTypeRTF");
				HTML = Dlfcn.GetStringConstant (handle, "kUTTypeHTML");
				XML = Dlfcn.GetStringConstant (handle, "kUTTypeXML");
				SourceCode = Dlfcn.GetStringConstant (handle, "kUTTypeSourceCode");
				CSource = Dlfcn.GetStringConstant (handle, "kUTTypeCSource");
				ObjectiveCSource = Dlfcn.GetStringConstant (handle, "kUTTypeObjectiveCSource");
				CPlusPlusSource = Dlfcn.GetStringConstant (handle, "kUTTypeCPlusPlusSource");
				ObjectiveCPlusPlusSource = Dlfcn.GetStringConstant (handle, "kUTTypeObjectiveCPlusPlusSource");
				CHeader = Dlfcn.GetStringConstant (handle, "kUTTypeCHeader");
				CPlusPlusHeader = Dlfcn.GetStringConstant (handle, "kUTTypeCPlusPlusHeader");
				JavaSource = Dlfcn.GetStringConstant (handle, "kUTTypeJavaSource");
				
				PDF = Dlfcn.GetStringConstant (handle, "kUTTypePDF");
				RTFD = Dlfcn.GetStringConstant (handle, "kUTTypeRTFD");
				FlatRTFD = Dlfcn.GetStringConstant (handle, "kUTTypeFlatRTFD");
				TXNTextAndMultimediaData = Dlfcn.GetStringConstant (handle, "kUTTypeTXNTextAndMultimediaData");
				WebArchive = Dlfcn.GetStringConstant (handle, "kUTTypeWebArchive");
				
				Image = Dlfcn.GetStringConstant (handle, "kUTTypeImage");
				JPEG = Dlfcn.GetStringConstant (handle, "kUTTypeJPEG");
				JPEG2000 = Dlfcn.GetStringConstant (handle, "kUTTypeJPEG2000");
				TIFF = Dlfcn.GetStringConstant (handle, "kUTTypeTIFF");
				PICT = Dlfcn.GetStringConstant (handle, "kUTTypePICT");
				GIF = Dlfcn.GetStringConstant (handle, "kUTTypeGIF");
				PNG = Dlfcn.GetStringConstant (handle, "kUTTypePNG");
				QuickTimeImage = Dlfcn.GetStringConstant (handle, "kUTTypeQuickTimeImage");
				AppleICNS = Dlfcn.GetStringConstant (handle, "kUTTypeAppleICNS");
				BMP = Dlfcn.GetStringConstant (handle, "kUTTypeBMP");
				ICO = Dlfcn.GetStringConstant (handle, "kUTTypeICO");
				
				AudiovisualContent = Dlfcn.GetStringConstant (handle, "kUTTypeAudiovisualContent");
				Movie = Dlfcn.GetStringConstant (handle, "kUTTypeMovie");
				Video = Dlfcn.GetStringConstant (handle, "kUTTypeVideo");
				Audio = Dlfcn.GetStringConstant (handle, "kUTTypeAudio");
				QuickTimeMovie = Dlfcn.GetStringConstant (handle, "kUTTypeQuickTimeMovie");
				MPEG = Dlfcn.GetStringConstant (handle, "kUTTypeMPEG");
				MPEG4 = Dlfcn.GetStringConstant (handle, "kUTTypeMPEG4");
				MP3 = Dlfcn.GetStringConstant (handle, "kUTTypeMP3");
				MPEG4Audio = Dlfcn.GetStringConstant (handle, "kUTTypeMPEG4Audio");
				AppleProtectedMPEG4Audio = Dlfcn.GetStringConstant (handle, "kUTTypeAppleProtectedMPEG4Audio");
				
				Folder = Dlfcn.GetStringConstant (handle, "kUTTypeFolder");
				Volume = Dlfcn.GetStringConstant (handle, "kUTTypeVolume");
				Package = Dlfcn.GetStringConstant (handle, "kUTTypePackage");
				Bundle = Dlfcn.GetStringConstant (handle, "kUTTypeBundle");
				Framework = Dlfcn.GetStringConstant (handle, "kUTTypeFramework");
				
				ApplicationBundle = Dlfcn.GetStringConstant (handle, "kUTTypeApplicationBundle");
				ApplicationFile = Dlfcn.GetStringConstant (handle, "kUTTypeApplicationFile");
				
				VCard = Dlfcn.GetStringConstant (handle, "kUTTypeVCard");
				
				InkText = Dlfcn.GetStringConstant (handle, "kUTTypeInkText");
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}
	}
}
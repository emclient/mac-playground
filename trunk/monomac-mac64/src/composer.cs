//
// Copyright 2010, Kenneth Pouncey
//
// composer.cs: Definitions for binding QuartzComposer.
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
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MonoMac.CoreGraphics;
using MonoMac.CoreAnimation;
using MonoMac.CoreImage;
using MonoMac.CoreVideo;

namespace MonoMac.QuartzComposer {

	[BaseType (typeof (NSObject))]
	interface QCComposition {
		[Static]
		[Export ("compositionWithFile:")]
		QCComposition GetComposition (string path);

		[Static]
		[Export ("compositionWithData:")]
		QCComposition GetComposition (NSData data);

		[Export ("protocols")]
		string [] Protocols { get; }

		[Export ("attributes")]
		NSDictionary Attributes { get; }

		[Export ("inputKeys")]
		string[] InputKeys { get; }

		[Export ("outputKeys")]
		string[] OutputKeys { get; }

		[Export ("identifier")]
		string Identifier { get; }
		
		[Field ("QCCompositionAttributeNameKey")]
		NSString AttributeNameKey { get; }

		[Field ("QCCompositionAttributeDescriptionKey")]
		NSString AttributeDescriptionKey { get; }

		[Field ("QCCompositionAttributeCopyrightKey")]
		NSString AttributeCopyrightKey { get; }
		
		[Field ("QCCompositionAttributeBuiltInKey")]
		NSString AttributeBuiltInKey { get; }
		
		[Field ("QCCompositionAttributeIsTimeDependentKey")]
		NSString AttributeIsTimeDependentKey { get; }
		
		[Field ("QCCompositionAttributeHasConsumersKey")]
		NSString AttributeHasConsumersKey { get; }
		
		[Field ("QCCompositionAttributeCategoryKey")]
		NSString AttributeCategoryKey { get; }
		
		[Field ("QCCompositionCategoryDistortion")]
		NSString CategoryDistortion { get; }
				
		[Field ("QCCompositionCategoryStylize")]
		NSString CategoryStylize { get; }
		
		[Field ("QCCompositionCategoryUtility")]
		NSString CategoryUtility { get; }

		[Field ("QCCompositionInputImageKey")]
		NSString InputImageKey { get; }

		[Field ("QCCompositionInputSourceImageKey")]
		NSString InputSourceImageKey { get; }

		[Field ("QCCompositionInputDestinationImageKey")]
		NSString InputDestinationImageKey { get; }

		[Field ("QCCompositionInputRSSFeedURLKey")]
		NSString InputRSSFeedURLKey { get; }

		[Field ("QCCompositionInputRSSArticleDurationKey")]
		NSString InputRSSArticleDurationKey { get; }

		[Field ("QCCompositionInputPreviewModeKey")]
		NSString InputPreviewModeKey { get; }

		[Field ("QCCompositionInputXKey")]
		NSString InputXKey { get; }

		[Field ("QCCompositionInputYKey")]
		NSString InputYKey { get; }

		[Field ("QCCompositionInputScreenImageKey")]
		NSString InputScreenImageKey { get; }

		[Field ("QCCompositionInputAudioPeakKey")]
		NSString InputAudioPeakKey { get; }

		[Field ("QCCompositionInputAudioSpectrumKey")]
		NSString InputAudioSpectrumKey { get; }

		[Field ("QCCompositionInputTrackPositionKey")]
		NSString InputTrackPositionKey { get; }

		[Field ("QCCompositionInputTrackInfoKey")]
		NSString InputTrackInfoKey { get; }

		[Field ("QCCompositionInputTrackSignalKey")]
		NSString InputTrackSignalKey { get; }

		[Field ("QCCompositionInputPrimaryColorKey")]
		NSString InputPrimaryColorKey { get; }

		[Field ("QCCompositionInputSecondaryColorKey")]
		NSString InputSecondaryColorKey { get; }

		[Field ("QCCompositionInputPaceKey")]
		NSString InputPaceKey { get; }

		[Field ("QCCompositionOutputImageKey")]
		NSString OutputImageKey { get; }

		[Field ("QCCompositionOutputWebPageURLKey")]
		NSString OutputWebPageURLKey { get; }

		[Field ("QCCompositionProtocolGraphicAnimation")]
		NSString ProtocolGraphicAnimation { get; }

		[Field ("QCCompositionProtocolGraphicTransition")]
		NSString ProtocolGraphicTransition { get; }

		[Field ("QCCompositionProtocolImageFilter")]
		NSString ProtocolImageFilter { get; }

		[Field ("QCCompositionProtocolScreenSaver")]
		NSString ProtocolScreenSaver { get; }

		[Field ("QCCompositionProtocolRSSVisualizer")]
		NSString ProtocolRSSVisualizer { get; }

		[Field ("QCCompositionProtocolMusicVisualizer")]
		NSString ProtocolMusicVisualizer { get; }
	}


	[BaseType (typeof (CAOpenGLLayer))]
	[DisableDefaultCtor] // return invalid handle
	interface QCCompositionLayer {
		
		[Static]
		[Export ("compositionLayerWithFile:")]
		QCCompositionLayer Create (string path);

		[Static]
		[Export ("compositionLayerWithComposition:")]
		QCCompositionLayer Create (QCComposition composition);

		[Export ("initWithFile:")]
		IntPtr Constructor (string path);

		[Export ("initWithComposition:")]
		IntPtr Constructor (QCComposition composition);

		[Export ("composition")]
		QCComposition Composition { get; }

	}

	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor] // crash when used (e.g. description) meant to be used thru sharedCompositionRepository
	interface QCCompositionRepository {
		[Static]
		[Export ("sharedCompositionRepository")]
		QCCompositionRepository SharedCompositionRepository { get; }

		[Export ("compositionWithIdentifier:")]
		QCComposition GetComposition (string identifier);

		[Export ("compositionsWithProtocols:andAttributes:")]
		QCComposition[] GetCompositions (NSArray protocols, NSDictionary attributes);

		[Export ("allCompositions")]
		QCComposition[] AllCompositions { get; }

	}

}
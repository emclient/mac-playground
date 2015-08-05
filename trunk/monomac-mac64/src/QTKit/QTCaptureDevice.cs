//
// Copyright 2010, Novell, Inc.
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
using MonoMac.Foundation;
using System;

namespace MonoMac.QTKit {

	public partial class QTCaptureDevice {

		static NSString FromMediaType (QTMediaType mediaType)
		{
			switch (mediaType){
			case QTMediaType.Video:
				return QTMedia.TypeVideo;
			case QTMediaType.Sound:
				return QTMedia.TypeSound;
			case QTMediaType.Text:
				return QTMedia.TypeText;
			case QTMediaType.Base:
				return QTMedia.TypeBase;
			case QTMediaType.Mpeg:
				return QTMedia.TypeMpeg;
			case QTMediaType.Music:
				return QTMedia.TypeMusic;
			case QTMediaType.TimeCode:
				return QTMedia.TypeTimeCode;
			case QTMediaType.Sprite:
				return QTMedia.TypeSprite;
			case QTMediaType.Flash:
				return QTMedia.TypeFlash;
			case QTMediaType.Movie:
				return QTMedia.TypeMovie;
			case QTMediaType.Tween:
				return QTMedia.TypeTween;
			case QTMediaType.Type3D:
				return QTMedia.Type3D;
			case QTMediaType.Skin:
				return QTMedia.TypeSkin;
			case QTMediaType.Qtvr:
				return QTMedia.TypeQTVR;
			case QTMediaType.Hint:
				return QTMedia.TypeHint;
			case QTMediaType.Stream:
				return QTMedia.TypeStream;
			case QTMediaType.Muxed:
				return QTMedia.TypeMuxed;
			case QTMediaType.QuartzComposer:
				return QTMedia.TypeQuartzComposer;
			default:
				return null;
			}
		}

		public static QTCaptureDevice [] GetInputDevices (QTMediaType mediaType)
		{
			var t = FromMediaType (mediaType);
			if (t == null)
				return null;
			return _GetInputDevices (t);
		}

		public static QTCaptureDevice GetDefaultInputDevice (QTMediaType mediaType)
		{
			var t = FromMediaType (mediaType);
			if (t == null)
				return null;
			return _GetDefaultInputDevice (t);
		}

		public bool HasMediaType (QTMediaType mediaType)
		{
			return _HasMediaType (FromMediaType (mediaType));
		}

		public bool IsAvcTransportControlReadOnly {
			get {
				return IsAttributeReadOnly (AVCTransportControlsAttribute);
			}
		}
		
		public  QTCaptureDeviceTransportControl AvcTransportControl {
			get {
				var dict = (NSDictionary) GetAttribute (AVCTransportControlsAttribute);
				if (dict == null)
					return null;
				return new QTCaptureDeviceTransportControl (dict);
			}
			set {
				var dict = new NSMutableDictionary ();
				if (value.Speed.HasValue)
					dict [QTCaptureDevice.AVCTransportControlsSpeedKey] = NSNumber.FromInt32 ((int) value.Speed.Value);
				if (value.PlaybackMode.HasValue)
					dict [QTCaptureDevice.AVCTransportControlsSpeedKey] = NSNumber.FromInt32 ((int) value.PlaybackMode.Value);
				SetAttribute (dict, AVCTransportControlsAttribute);
			}
		}

		public bool IsSuspended {
			get {
				var val = (NSNumber) GetAttribute (SuspendedAttribute);
				return (val != null && val.BoolValue);
			}
		}
	}

	public class QTCaptureDeviceTransportControl {
		public QTCaptureDeviceTransportControl () {}
		internal QTCaptureDeviceTransportControl (NSDictionary dict)
		{
			var number = (NSNumber) dict [QTCaptureDevice.AVCTransportControlsSpeedKey];
			if (number != null)
				Speed = (QTCaptureDeviceControlsSpeed) number.Int32Value;
			number = (NSNumber) dict [QTCaptureDevice.AVCTransportControlsPlaybackModeKey];
			if (number != null)
				PlaybackMode = (QTCaptureDevicePlaybackMode) number.Int32Value;
		}
		
		public QTCaptureDeviceControlsSpeed? Speed { get; set; }
		public QTCaptureDevicePlaybackMode? PlaybackMode { get; set; }
	}
}
//
// C#-like events for AVFoundation classes
//
// Author:
//   Miguel de Icaza (miguel@novell.com)
// Copyright 2009, Novell, Inc.
// Copyright 2010, Novell, Inc.
// Copyright 2011, 2012 Xamarin Inc.
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
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.AVFoundation {

	public class AVErrorEventArgs : EventArgs {
		public AVErrorEventArgs (NSError error)
		{
			Error = error;
		}

		public NSError Error { get; private set; }
	}
	
	public class AVStatusEventArgs : EventArgs {
		public AVStatusEventArgs (bool status)
		{
			Status = status;
		}

		public bool Status { get; private set; }
	}

	#pragma warning disable 672
	sealed class InternalAVAudioPlayerDelegate : AVAudioPlayerDelegate {
		internal EventHandler cbEndInterruption, cbBeginInterruption;
		internal EventHandler<AVStatusEventArgs> cbFinishedPlaying;
		internal EventHandler<AVErrorEventArgs> cbDecoderError;
		
		[Preserve (Conditional = true)]
		public override void FinishedPlaying (AVAudioPlayer player, bool flag)
		{
			if (cbFinishedPlaying != null)
				cbFinishedPlaying (player, new AVStatusEventArgs (flag));
			if (player.Handle == IntPtr.Zero)
				throw new ObjectDisposedException ("player", "the player object was Dispose()d during the callback, this has corrupted the state of the program");
		}
	
		[Preserve (Conditional = true)]
		public override void DecoderError (AVAudioPlayer player, NSError  error)
		{
			if (cbDecoderError != null)
				cbDecoderError (player, new AVErrorEventArgs (error));
		}
#if !MONOMAC	
		[Preserve (Conditional = true)]
		public override void BeginInterruption (AVAudioPlayer  player)
		{
			if (cbBeginInterruption != null)
				cbBeginInterruption (player, EventArgs.Empty);
		}
	
		[Preserve (Conditional = true)]
		public override void EndInterruption (AVAudioPlayer player)
		{
			if (cbEndInterruption != null)
				cbEndInterruption (player, EventArgs.Empty);
		}
#endif
	}
	#pragma warning restore 672
	
	public partial class AVAudioPlayer {
		InternalAVAudioPlayerDelegate EnsureEventDelegate ()
		{
			var del = WeakDelegate as InternalAVAudioPlayerDelegate;
			if (del == null){
				del = new InternalAVAudioPlayerDelegate ();
				WeakDelegate = del;
			}
			return del;
		}

		public event EventHandler<AVStatusEventArgs> FinishedPlaying {
			add {
				EnsureEventDelegate ().cbFinishedPlaying += value;
			}

			remove {
				EnsureEventDelegate ().cbFinishedPlaying -= value;
			}
		}

		public event EventHandler<AVErrorEventArgs> DecoderError {
			add {
				EnsureEventDelegate ().cbDecoderError += value;
			}

			remove {
				EnsureEventDelegate ().cbDecoderError -= value;
			}
		}

		public event EventHandler BeginInterruption {
			add {
				EnsureEventDelegate ().cbBeginInterruption += value;
			}

			remove {
				EnsureEventDelegate ().cbBeginInterruption -= value;
			}
		}

		public event EventHandler EndInterruption {
			add {
				EnsureEventDelegate ().cbEndInterruption += value;
			}

			remove {
				EnsureEventDelegate ().cbEndInterruption -= value;
			}
		}
	}

	internal class InternalAVAudioRecorderDelegate : AVAudioRecorderDelegate {
		internal EventHandler cbEndInterruption, cbBeginInterruption;
		internal EventHandler<AVStatusEventArgs> cbFinishedRecording;
		internal EventHandler<AVErrorEventArgs> cbEncoderError;
		
		[Preserve (Conditional = true)]
		public override void FinishedRecording (AVAudioRecorder recorder, bool flag)
		{
			if (cbFinishedRecording != null)
				cbFinishedRecording (recorder, new AVStatusEventArgs (flag));
		}
	
		[Preserve (Conditional = true)]
		public override void EncoderError (AVAudioRecorder recorder, NSError error)
		{
			if (cbEncoderError != null)
				cbEncoderError (recorder, new AVErrorEventArgs (error));
		}
#if !MONOMAC	
		[Preserve (Conditional = true)]
		public override void BeginInterruption (AVAudioRecorder  recorder)
		{
			if (cbBeginInterruption != null)
				cbBeginInterruption (recorder, EventArgs.Empty);
		}
	
		[Preserve (Conditional = true)]
		[Obsolete ("Deprecated in iOS 6.0")]		
		public override void EndInterruption (AVAudioRecorder recorder)
		{
			if (cbEndInterruption != null)
				cbEndInterruption (recorder, EventArgs.Empty);
		}
#endif
	}

	public partial class AVAudioRecorder {
		InternalAVAudioRecorderDelegate EnsureEventDelegate ()
		{
			var del = WeakDelegate as InternalAVAudioRecorderDelegate;
			if (del == null){
				del = new InternalAVAudioRecorderDelegate ();
				WeakDelegate = del;
			}
			return del;
		}

		public event EventHandler<AVStatusEventArgs> FinishedRecording {
			add {
				EnsureEventDelegate ().cbFinishedRecording += value;
			}

			remove {
				EnsureEventDelegate ().cbFinishedRecording -= value;
			}
		}

		public event EventHandler<AVErrorEventArgs> EncoderError {
			add {
				EnsureEventDelegate ().cbEncoderError += value;
			}

			remove {
				EnsureEventDelegate ().cbEncoderError -= value;
			}
		}

		public event EventHandler BeginInterruption {
			add {
				EnsureEventDelegate ().cbBeginInterruption += value;
			}

			remove {
				EnsureEventDelegate ().cbBeginInterruption -= value;
			}
		}

		public event EventHandler EndInterruption {
			add {
				EnsureEventDelegate ().cbEndInterruption += value;
			}

			remove {
				EnsureEventDelegate ().cbEndInterruption -= value;
			}
		}
	}

	public class AVSampleRateEventArgs : EventArgs {
		public AVSampleRateEventArgs (double sampleRate)
		{
			SampleRate = sampleRate;
		}
		public double SampleRate { get; private set; }
	}

	public class AVChannelsEventArgs : EventArgs {
		public AVChannelsEventArgs (int numberOfChannels)
		{
			NumberOfChannels = numberOfChannels;
		}
		public int NumberOfChannels { get; private set; }
	}

	public class AVCategoryEventArgs : EventArgs {
		public AVCategoryEventArgs (string category)
		{
			Category = category;
		}

		public string Category { get; private set; }
	}
	
#if !MONOMAC
	internal class InternalAVAudioSessionDelegate : AVAudioSessionDelegate {
		internal EventHandler cbEndInterruption, cbBeginInterruption;
		internal EventHandler<AVCategoryEventArgs> cbCategoryChanged;
		internal EventHandler<AVStatusEventArgs> cbInputAvailabilityChanged;
		internal EventHandler<AVSampleRateEventArgs> cbSampleRateChanged;
		internal EventHandler<AVChannelsEventArgs> cbInputChanged;
		internal EventHandler<AVChannelsEventArgs> cbOutputChanged;

		AVAudioSession session;
		
		[Preserve (Conditional = true)]
		public InternalAVAudioSessionDelegate (AVAudioSession session)
		{
			this.session = session;
		}
		
		[Preserve (Conditional = true)]
		public override void BeginInterruption ()
		{
			if (cbBeginInterruption != null)
				cbBeginInterruption (session, EventArgs.Empty);
		}
	
		[Preserve (Conditional = true)]
		public override void EndInterruption ()
		{
			if (cbEndInterruption != null)
				cbEndInterruption (session, EventArgs.Empty);
		}

		[Preserve (Conditional = true)]
		public override void InputIsAvailableChanged (bool isInputAvailable)
		{
			if (cbInputAvailabilityChanged != null)
				cbInputAvailabilityChanged (session, new AVStatusEventArgs (isInputAvailable));
		}		
	
	}

	public partial class AVAudioSession {
		InternalAVAudioSessionDelegate EnsureEventDelegate ()
		{
			var del = WeakDelegate as InternalAVAudioSessionDelegate;
			if (del == null){
				del = new InternalAVAudioSessionDelegate (this);
				WeakDelegate = del;
			}
			return del;
		}

		public event EventHandler BeginInterruption {
			add {
				EnsureEventDelegate ().cbBeginInterruption += value;
			}
			remove {
				EnsureEventDelegate ().cbBeginInterruption -= value;
			}
		}

		public event EventHandler EndInterruption {
			add {
				EnsureEventDelegate ().cbEndInterruption += value;
			}
			remove {
				EnsureEventDelegate ().cbBeginInterruption -= value;
			}
		}

		public event EventHandler<AVCategoryEventArgs> CategoryChanged {
			add {
				EnsureEventDelegate ().cbCategoryChanged += value;
			}
			remove {
				EnsureEventDelegate ().cbCategoryChanged -= value;
			}
		}

		public event EventHandler<AVStatusEventArgs> InputAvailabilityChanged {
			add {
				EnsureEventDelegate ().cbInputAvailabilityChanged += value;
			}
			remove {
				EnsureEventDelegate ().cbInputAvailabilityChanged -= value;
			}
		}

		public event EventHandler<AVSampleRateEventArgs> SampleRateChanged {
			add {
				EnsureEventDelegate ().cbSampleRateChanged += value;
			}

			remove {
				EnsureEventDelegate ().cbSampleRateChanged -= value;
			}
		}

		public event EventHandler<AVChannelsEventArgs> InputChannelsChanged {
			add {
				EnsureEventDelegate ().cbInputChanged += value;
			}

			remove {
				EnsureEventDelegate ().cbOutputChanged += value;
			}
		}

		public event EventHandler<AVChannelsEventArgs> OutputChannelsChanged {
			add {
				EnsureEventDelegate ().cbOutputChanged += value;
			}

			remove {
				EnsureEventDelegate ().cbOutputChanged -= value;
			}
		}
	}
#endif
}

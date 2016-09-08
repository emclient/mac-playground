// 
// SystemSound.cs: AudioServices system sound
//
// Authors: Mono Team
//          Marek Safar (marek.safar@gmail.com)
//     
// Copyright 2009 Novell, Inc
// Copyright 2012 Xamarin Inc.
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

using MonoMac.Foundation;
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.AudioToolbox {

	enum SystemSoundId : uint {
		Vibrate = 0x00000FFF,
	}

	public class SystemSound : INativeObject, IDisposable {
#if MONOMAC
		// TODO:
#else
		public static readonly SystemSound Vibrate = new SystemSound ((uint) SystemSoundId.Vibrate, false);
#endif

		uint soundId;
		bool ownsHandle;

		Action completionRoutine;
		GCHandle gc_handle;
		static readonly Action<SystemSoundId, IntPtr> SoundCompletionCallback = SoundCompletionShared;

		internal SystemSound (uint soundId, bool ownsHandle)
		{
			this.soundId = soundId;
			this.ownsHandle = ownsHandle;
		}

		~SystemSound ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				AssertNotDisposed ();
				return (IntPtr) soundId;
			}
		}

		public bool IsUISound {
			get {
				uint out_size = sizeof (uint);
				uint data;

				var res = AudioServices.AudioServicesGetProperty (AudioServicesPropertyKey.IsUISound, sizeof (AudioServicesPropertyKey), ref soundId, out out_size, out data);
				if (res != AudioServicesError.None)
					throw new ArgumentException (res.ToString ());

				return data == 1;
			}

			set {
				uint data = value ? (uint)1 : 0;

				var res = AudioServices.AudioServicesSetProperty (AudioServicesPropertyKey.IsUISound, sizeof (AudioServicesPropertyKey), ref soundId, sizeof (uint), ref data);
				if (res != AudioServicesError.None)
					throw new ArgumentException (res.ToString ());
			}
		}

		public bool CompletePlaybackIfAppDies {
			get {
				uint out_size = sizeof (uint);
				uint data;

				var res = AudioServices.AudioServicesGetProperty (AudioServicesPropertyKey.CompletePlaybackIfAppDies, sizeof (AudioServicesPropertyKey), ref soundId, out out_size, out data);
				if (res != AudioServicesError.None)
					throw new ArgumentException (res.ToString ());

				return data == 1;
			}

			set {
				uint data = value ? (uint)1 : 0;

				var res = AudioServices.AudioServicesSetProperty (AudioServicesPropertyKey.CompletePlaybackIfAppDies, sizeof (AudioServicesPropertyKey), ref soundId, sizeof (uint), ref data);
				if (res != AudioServicesError.None)
					throw new ArgumentException (res.ToString ());
			}
		}

		void AssertNotDisposed ()
		{
			if (soundId == 0)
				throw new ObjectDisposedException ("SystemSound");
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			Cleanup (false);
		}

		[DllImport (Constants.AudioToolboxLibrary)]
		static extern AudioServicesError AudioServicesDisposeSystemSoundID (uint soundId);

		void Cleanup (bool checkForError)
		{
			if (soundId == 0 || !ownsHandle)
				return;

			if (gc_handle.IsAllocated) {
				gc_handle.Free ();
			}

			if (completionRoutine != null) {
				RemoveSystemSoundCompletion ();
			}

			var error = AudioServicesDisposeSystemSoundID (soundId);
			var oldId = soundId;
			soundId = 0;
			if (checkForError && error != AudioServicesError.None) {
				throw new InvalidOperationException (string.Format ("Error while disposing SystemSound with ID {0}: {1}",
							oldId, error.ToString()));
			}
		}

		public void Close ()
		{
			Cleanup (true);
		}

		[DllImport (Constants.AudioToolboxLibrary)]
		static extern void AudioServicesPlayAlertSound (uint inSystemSoundID);
		public void PlayAlertSound ()
		{
			AssertNotDisposed ();
			AudioServicesPlayAlertSound (soundId);
		}

		[DllImport (Constants.AudioToolboxLibrary)]
		static extern void AudioServicesPlaySystemSound(uint inSystemSoundID);
		public void PlaySystemSound ()
		{
			AssertNotDisposed ();
			AudioServicesPlaySystemSound (soundId);
		}

		[DllImport (Constants.AudioToolboxLibrary)]
		static extern AudioServicesError AudioServicesCreateSystemSoundID (IntPtr fileUrl, out uint soundId);

		public SystemSound (NSUrl fileUrl)
		{
			var error = AudioServicesCreateSystemSoundID (fileUrl.Handle, out soundId);
			if (error != AudioServicesError.None)
				throw new InvalidOperationException (string.Format ("Could not create system sound ID for url {0}; error={1}",
							fileUrl, error));
			ownsHandle = true;
		}
			
		public static SystemSound FromFile (NSUrl fileUrl)
		{
			uint soundId;
			var error = AudioServicesCreateSystemSoundID (fileUrl.Handle, out soundId);
			if (error != AudioServicesError.None)
				return null;
			return new SystemSound (soundId, true);
		}

		public static SystemSound FromFile (string filename)
		{
			using (var url = new NSUrl (filename)){
				uint soundId;
				var error = AudioServicesCreateSystemSoundID (url.Handle, out soundId);
				if (error != AudioServicesError.None)
					return null;
				return new SystemSound (soundId, true);
			}
		}

		[DllImport (Constants.AudioToolboxLibrary)]
		static extern AudioServicesError AudioServicesAddSystemSoundCompletion (uint soundId, IntPtr runLoop, IntPtr runLoopMode, Action<SystemSoundId, IntPtr> completionRoutine, IntPtr clientData);

		[MonoPInvokeCallback (typeof (Action<SystemSoundId, IntPtr>))]
		static void SoundCompletionShared (SystemSoundId id, IntPtr clientData)
		{
			GCHandle gch = GCHandle.FromIntPtr (clientData);
			var ss = (SystemSound) gch.Target;

			ss.completionRoutine ();
		}

		public AudioServicesError AddSystemSoundCompletion (Action routine, CFRunLoop runLoop = null)
		{
			if (gc_handle.IsAllocated)
				throw new ArgumentException ("Only single completion routine is supported");

			gc_handle = GCHandle.Alloc (this);
			completionRoutine = routine;

			return AudioServicesAddSystemSoundCompletion (soundId,
			                                              runLoop == null ? IntPtr.Zero : runLoop.Handle,
			                                              IntPtr.Zero, // runLoopMode should be enum runLoopMode == null ? IntPtr.Zero : runLoopMode.Handle,
			                                              SoundCompletionCallback, GCHandle.ToIntPtr (gc_handle));
		}

		[DllImport (Constants.AudioToolboxLibrary)]
		static extern void AudioServicesRemoveSystemSoundCompletion (uint soundId);

		public void RemoveSystemSoundCompletion ()
		{
			completionRoutine = null;
			AudioServicesRemoveSystemSoundCompletion (soundId);
		}
	}
}

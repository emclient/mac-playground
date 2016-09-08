#if DEBUGFOCUS

// This extension helps with tracking issues with unpaired lockFocus/unlockFocus calls

using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace System.Windows.Forms.CocoaInternal
{
	internal partial class MonoView : NSView
	{
		public class TraceInfo
		{
			public MonoView view;
			public string stackTrace;
			public int lockFocusCounter;
			public int unlockFocusCounter;
		}

		private Dictionary<MonoView, TraceInfo> lockFocusTrace = new Dictionary<MonoView, TraceInfo>();

		public override void LockFocus ()
		{
			var traceInfo = GetTraceInfo ();

			if (!NSThread.IsMain)
				Console.WriteLine ("Calling LockFocus from non UI thread!");

			++traceInfo.lockFocusCounter;

			if (!NSThread.IsMain)
				Console.WriteLine ("Calling LockFocus from non UI thread!");

			base.LockFocus ();
		}

		public override bool LockFocusIfCanDraw ()
		{
			var traceInfo = GetTraceInfo ();

			if (!NSThread.IsMain)
				Console.WriteLine ("Calling LockFocusIfCanDraw from non UI thread!");

			if (base.LockFocusIfCanDraw ()) {
				++traceInfo.lockFocusCounter;
				return true;
			}

			return false;
		}

		public override bool LockFocusIfCanDrawInContext (NSGraphicsContext context)
		{
			if (!NSThread.IsMain)
				Console.WriteLine ("Calling LockFocusIfCanDrawInContext from non UI thread!");

			var traceInfo = GetTraceInfo ();
			if (base.LockFocusIfCanDrawInContext (context)) {
				++traceInfo.lockFocusCounter;
				return true;
			}
			return false;
		}

		public override void UnlockFocus ()
		{
			if (!NSThread.IsMain)
				Console.WriteLine ("Calling UnlockFocus from non UI thread!");

			TraceInfo traceInfo;
			if (!lockFocusTrace.TryGetValue (this, out traceInfo)) {
				Console.WriteLine ("Calling UnlockFocus without calling LockFocus!");
				return;
			} else {
				++traceInfo.unlockFocusCounter;
				if (traceInfo.unlockFocusCounter > traceInfo.lockFocusCounter) {
					Console.WriteLine ("Calling UnlockFocus more times than LockFocus! ({0} = {1} - {2})",
						traceInfo.unlockFocusCounter - traceInfo.lockFocusCounter, traceInfo.unlockFocusCounter, traceInfo.lockFocusCounter);
					return;
				}
			}

			if (NSThread.IsMain)
				base.UnlockFocus ();
			else 
				PrintStackTrace(traceInfo);
		}

		void PrintStackTrace(TraceInfo traceInfo) {
			if (traceInfo != null && traceInfo.stackTrace != null) {
				Console.WriteLine (traceInfo.stackTrace);
			}
		}

		TraceInfo GetTraceInfo() {
			TraceInfo traceInfo;
			if (!lockFocusTrace.TryGetValue (this, out traceInfo)) {
				traceInfo = new TraceInfo {
					view = this,
					stackTrace = System.Environment.StackTrace.ToString (),
					lockFocusCounter = 0,
					unlockFocusCounter = 0
				};
				lockFocusTrace.Add (this, traceInfo);
			}
			return traceInfo;
		}
	}
}

#endif

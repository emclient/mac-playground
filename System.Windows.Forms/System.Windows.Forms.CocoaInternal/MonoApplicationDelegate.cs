using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms.Mac;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace System.Windows.Forms.CocoaInternal
{
	internal partial class MonoApplication
	{
		XplatUICocoa driver;
		const long ENDSESSION_LOGOFF = 0x80000000;

		internal static bool IsGoingToPowerOff = false;
		internal static DateTime IsGoingToPowerOfTime = DateTime.MinValue;
		internal const double IsGoingToPowerOfMaxDelay = 30; // To pseudo-handle the case when shutdown is cancelled by another app.
		internal bool DeactivateAppOnDraggingEnded = false;
		internal NSMenu dockMenu;

		internal void SetupDelegate()
		{
			this.driver = XplatUICocoa.GetInstance();
			this.dockMenu = new NSMenu();

			XplatUICocoa.DraggingEnded += XplatUICocoa_DraggingEnded;

			NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.WillPowerOffNotification, WillPowerOff, null);
			// FIXME: NSMenuDidBeginTrackingNotification should send WM_CANCELMODE

			this.WillFinishLaunching += MonoApplication_WillFinishLaunching;
			this.WillTerminate += MonoApplication_WillTerminate;
			this.WillBecomeActive += MonoApplication_WillBecomeActive;
			this.DidBecomeActive += MonoApplication_DidBecomeActive;
			this.WillResignActive += MonoApplication_WillResignActive;
			this.ApplicationDockMenu = MonoApplication_ApplicationDockMenu;
			this.ApplicationShouldHandleReopen = MonoApplication_ApplicationShouldHandleReopen;
			this.OpenUrls += MonoApplication_OpenUrls;
			this.OpenFiles += MonoApplication_OpenFiles;
			this.ApplicationShouldTerminate = MonoApplication_ShouldTerminate;
		}

		protected virtual void WillPowerOff(NSNotification n)
		{
			IsGoingToPowerOff = true;
			IsGoingToPowerOfTime = DateTime.Now;
		}

		private NSApplicationTerminateReply MonoApplication_ShouldTerminate(NSApplication sender)
		{
			bool shouldTerminate = true;

			var forms = new Collections.ArrayList(Application.OpenForms);
			forms.Reverse();

			if (IsGoingToPowerOff && (DateTime.Now - IsGoingToPowerOfTime).TotalSeconds < IsGoingToPowerOfMaxDelay)
			{
				IsGoingToPowerOff = false; // For the case the shutdown is going to be cancelled

				foreach(Form form in forms)
					if (IntPtr.Zero == XplatUI.SendMessage(form.Handle, Msg.WM_QUERYENDSESSION, (IntPtr)1, (IntPtr)ENDSESSION_LOGOFF))
						shouldTerminate = false;

				if (!shouldTerminate)
					return NSApplicationTerminateReply.Cancel;

				foreach (Form form in forms)
					XplatUI.SendMessage(form.Handle, Msg.WM_ENDSESSION, (IntPtr)1, (IntPtr)ENDSESSION_LOGOFF);
			}

		   if (Application.MessageLoop)
		   {
			   foreach (Form form in forms)
			   {
					form.CloseReason = CloseReason.TaskManagerClosing;
					XplatUI.SendMessage(form.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
					if (form.IsHandleCreated)
						return NSApplicationTerminateReply.Cancel;
			   }
		   }

		   return NSApplicationTerminateReply.Now;
	   }

		private void MonoApplication_WillFinishLaunching(object sender, EventArgs e)
		{
			if (!NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(10, 13, 0)))
				NSAppleEventManager.SharedAppleEventManager.SetEventHandler(this, new Selector(selHandleUrl), AEEventClass.Internet, AEEventID.GetUrl);
		}

		private void MonoApplication_WillTerminate(object sender, EventArgs e)
		{
			Pasteboard.Wipe();
		}

		private void MonoApplication_WillBecomeActive(object sender, EventArgs e)
		{
			// This is to work around the following problem with Expose:
			// When you activate another app by clicking it's dock icon and then activate em Client window with Expose (F3, gesture),
			// then the em Client app becomes active, but the clicked em Client window goes *behind* the main window of the previously
			// active app.
			// With this workaround, the clicked windiw flashes back and forth (under the foreign window and back),
			// but remains in front at last.

			MainWindow?.OrderFront(this);
		}

		private void MonoApplication_DidBecomeActive(object sender, EventArgs e)
		{
			if (XplatUICocoa.DraggedData == null)
				DoActivateApp();
			else
				DeactivateAppOnDraggingEnded = false;
		}

		private void MonoApplication_WillResignActive(object sender, EventArgs e)
		{
			if (XplatUICocoa.DraggedData == null) // Do not perform deactivation if dragging in progress
				DoDeactivateApp();
			else
				DeactivateAppOnDraggingEnded = true;
		}

		void DoActivateApp()
		{
			XplatUICocoa.UpdateModifiers(NSEvent.CurrentModifierFlags);
			driver.SendMessage(XplatUI.GetActive(), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
		}

		void DoDeactivateApp()
		{
			if (driver.Grab.Hwnd != IntPtr.Zero)
			{
				driver.SendMessage(driver.Grab.Hwnd, Msg.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
				driver.Grab.Hwnd = IntPtr.Zero;
			}
			driver.SendMessage(XplatUI.GetActive(), Msg.WM_ACTIVATEAPP, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
		}

		void XplatUICocoa_DraggingEnded(object sender, EventArgs e)
		{
			if (DeactivateAppOnDraggingEnded)
			{
				DeactivateAppOnDraggingEnded = false;
				DoDeactivateApp();
			}
		}

		const string selHandleUrl = "handleURLEvent:withReplyEvent:";
		[Export(selHandleUrl)]
		public void MonoApplication_HandleURLEvent(NSAppleEventDescriptor value, NSAppleEventDescriptor reply)
		{
			var url = value.ParamDescriptorForKeyword(AEKeyword.DirectObject)?.StringValue;
			if (url != null)
				TryOpen(new string[] { url }, Msg.WM_OPEN_URLS);
		}

		private void MonoApplication_OpenUrls(object sender, NSApplicationOpenUrlsEventArgs e)
		{
			var urls = new List<string>();
			foreach (var url in e.Urls)
				urls.Add(url.AbsoluteString);

			TryOpen(urls.ToArray(), Msg.WM_OPEN_URLS);
		}

		private void MonoApplication_OpenFiles(object sender, NSApplicationFilesEventArgs e)
		{
			TryOpen(e.Filenames);
		}

		internal bool TryOpen(string[] filenames, Msg msg = Msg.WM_OPEN_FILES)
		{
			GCHandle gch = new GCHandle();
			try
			{
				gch = GCHandle.Alloc(filenames);
				var result = driver.SendMessage(XplatUI.GetActive(), msg, IntPtr.Zero, GCHandle.ToIntPtr(gch));
				return result == IntPtr.Zero;
			}
			catch (Exception e)
			{
				DebugHelper.WriteLine($"Failed opening file(s) or URL(s) [{string.Join(",", filenames)}]: {e}");
				return false;
			}
			finally
			{
				if (gch.IsAllocated)
					gch.Free();
			}
		}

		internal NSMenu MonoApplication_ApplicationDockMenu(NSApplication sender)
		{
			var gch = new GCHandle();
			try
			{
				gch = GCHandle.Alloc(dockMenu);
				driver.SendMessage(XplatUI.GetActive(), Msg.WM_DOCK_MENU, IntPtr.Zero, GCHandle.ToIntPtr(gch));
			}
			catch (Exception e)
			{
				DebugHelper.WriteLine($"Exception in WM_DOCK_MENU handler: {e}");
			}
			finally
			{
				if (gch.IsAllocated)
					gch.Free();
			}
			return dockMenu;
		}

		internal bool MonoApplication_ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
		{
			XplatUICocoa.UpdateModifiers(NSEvent.CurrentModifierFlags);
			var result = driver.SendMessage(XplatUI.GetActive(), Msg.WM_APP_REOPEN, new IntPtr(hasVisibleWindows ? 1 : 0), IntPtr.Zero);
			return result.ToInt32() != 0;
		}
	}
}


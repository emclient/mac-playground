using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace XibLessApp
{
	public class Window : NSWindow
	{
		public Window(RectangleF contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
		}

		[Export("close")]
		public virtual void MyClose()
		{
			Console.WriteLine("Close");

			PrintWindows();
			OrderOut(NSApplication.SharedApplication);
			PrintWindows();
		}

		public override void ResignKeyWindow()
		{
			Console.WriteLine("ResignKeyWindow");
			base.ResignKeyWindow();
		}

		public override void ResignMainWindow()
		{
			Console.WriteLine("ResignMainWindow");
			base.ResignMainWindow();
		}

		void PrintWindows()
		{
			var wins = NSApplication.SharedApplication.Windows;
			foreach(var win in wins) {
				Console.WriteLine($"title:{win.Title}, isVisible:{win.IsVisible}, isKey:{win.IsKeyWindow}, isMain:{IsMainWindow}");
			}
		}
	}
}

using System.Runtime.InteropServices;
using System.Windows.Forms.CocoaInternal;

using MacApi;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing.Mac;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms.Extensions.IO;
using System.Text;
using CoreFoundation;
using System.Reflection;
using System.Collections.Specialized;

using System;
using AppKit;
using Foundation;
using ObjCRuntime;
using CoreGraphics;
using UniformTypeIdentifiers;
using System.Diagnostics;

namespace System.Windows.Forms.Mac
{
	using KeysConverter = CocoaInternal.KeysConverter;

	public static class Extensions
	{
		static readonly DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		static readonly Selector swfMarkSel = new Selector("thisIsSwfControl");
		static double preciseDeltaScale = 1.0;
		static double rawDeltaScale = 40.0;

		public static NSMethodSignature GetMethodSignature(this NSObject obj, Selector selector)
		{
			var sig = LibObjc.IntPtr_objc_msgSend_IntPtr(obj.Handle, Selector.GetHandle("methodSignatureForSelector:"), selector.Handle);
			return ObjCRuntime.Runtime.GetNSObject<NSMethodSignature>(sig);
		}

		public static NSInvocation ToInvocation(this NSMethodSignature signature)
		{
			var cls = Class.GetHandle(typeof(NSInvocation));
			var ctor = Selector.GetHandle("invocationWithMethodSignature:");
			var inv = LibObjc.IntPtr_objc_msgSend_IntPtr(cls, ctor, signature.Handle);
			return ObjCRuntime.Runtime.GetNSObject<NSInvocation>(inv);
		}

		public static void SetArgument(this NSInvocation invocation, NSObject arg, int index)
		{
			LibObjc.void_objc_msgSend_IntPtr_nint(invocation.Handle, Selector.GetHandle("setArgument:atIndex:"), arg?.Handle ?? IntPtr.Zero, index);
		}

		public static uint ToFourCC(this string s)
		{
			return (((uint)s[0]) << 24 | ((uint)s[1]) << 16 | ((uint)s[2]) << 8 | ((uint)s[3]));
		}

		public static NSString ToNSString(this string value)
		{
			return (NSString)value;
		}

		internal static bool ToKeyMsg(this NSEvent e, out Msg msg, out IntPtr wParam, out IntPtr lParam)
		{
			var key = KeysConverter.GetKeys(e);
			var isExtendedKey = XplatUICocoa.IsCtrlDown || XplatUICocoa.IsCmdDown || e.Characters.Length == 0 || !KeysConverter.IsChar(e.Characters[0], key) && KeysConverter.DeadKeyState == 0;

			ulong lp = 0;
			lp |= e.IsARepeat ? 1ul : 0ul;
			lp |= ((ulong)e.KeyCode) << 16; // OEM-dependent scanCode
			lp |= (isExtendedKey ? 1ul : 0ul) << 24;
			lp |= (e.IsARepeat ? 1ul : 0ul) << 30;
			lParam = (IntPtr)lp;
			wParam = (IntPtr)key;

			var isSysKey = false;// altDown && !cmdDown
			msg = isSysKey ? (e.Type == NSEventType.KeyDown ? Msg.WM_SYSKEYDOWN : Msg.WM_SYSKEYUP) : (e.Type == NSEventType.KeyDown ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);

			return key != Keys.None;
		}

		public static bool IsMouse(this NSEvent e, out NSMouseFlags flags)
		{
			switch (e.Type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.RightMouseDown:
				case NSEventType.OtherMouseDown:
					flags = NSMouseFlags.ClickCount | NSMouseFlags.Pressure | NSMouseFlags.Down;
					return true;
				case NSEventType.LeftMouseUp:
				case NSEventType.RightMouseUp:
				case NSEventType.OtherMouseUp:
					flags = NSMouseFlags.ClickCount | NSMouseFlags.Pressure | NSMouseFlags.Up;
					return true;
				case NSEventType.LeftMouseDragged:
				case NSEventType.RightMouseDragged:
				case NSEventType.OtherMouseDragged:
					flags = NSMouseFlags.Drag;
					return true;
				case NSEventType.ScrollWheel:
				case NSEventType.MouseMoved:
					flags = NSMouseFlags.None;
					return true;
			}
			flags = NSMouseFlags.None;
			return false;
		}

		public static NSEvent RetargetMouseEvent(this NSEvent e, NSView target, NSMouseFlags props)
		{
			if (target.Window == null)
				return e;

			var l = e.Window != null ? e.Window.ConvertPointToScreen(e.LocationInWindow) : e.LocationInWindow;
			var p = target.Window.ConvertPointFromScreen(l);
			var clickCount = props.HasFlag(NSMouseFlags.ClickCount) ? e.ClickCount : 0;
			var pressure = props.HasFlag(NSMouseFlags.Pressure) ? e.Pressure : 0;
			return e.Type == NSEventType.ScrollWheel
				? e // Creating ScrollWheel fails, we need to use CGEvent() or derive custom event from NSEvent
				: NSEvent.MouseEvent(e.Type, p, e.ModifierFlags, e.Timestamp, target.Window.WindowNumber, null, 0, clickCount, pressure);
		}

		public static NSEvent RetargetMouseEvent(this NSEvent e, CGPoint locationOnScreen, NSMouseFlags props)
		{
			var wnum = NSWindow.WindowNumberAtPoint(locationOnScreen, 0);
			var target = NSApplication.SharedApplication.WindowWithWindowNumber(wnum);
			if (target != null)
			{
				var location = target.ConvertPointFromScreen(locationOnScreen);
				var clickCount = props.HasFlag(NSMouseFlags.ClickCount) ? e.ClickCount : 0;
				var pressure = props.HasFlag(NSMouseFlags.Pressure) ? e.Pressure : 0;
				return e.Type == NSEventType.ScrollWheel
					? e // Creating ScrollWheel fails, we need to use CGEvent() or derive custom event from NSEvent
					: NSEvent.MouseEvent(e.Type, location, e.ModifierFlags, e.Timestamp, target.WindowNumber, null, 0, clickCount, pressure);
			}
			return e;
		}

		public static void DispatchMouseEvent(this NSView view, NSEvent e)
		{
			switch (e.Type)
			{
				case NSEventType.LeftMouseDown: view.MouseDown(e); break;
				case NSEventType.RightMouseDown: view.RightMouseDown(e); break;
				case NSEventType.OtherMouseDown: view.OtherMouseDown(e); break;
				case NSEventType.LeftMouseUp: view.MouseUp(e); break;
				case NSEventType.RightMouseUp: view.RightMouseUp(e); break;
				case NSEventType.OtherMouseUp: view.OtherMouseUp(e); break;
				case NSEventType.LeftMouseDragged: view.MouseDragged(e); break;
				case NSEventType.RightMouseDragged: view.RightMouseDragged(e); break;
				case NSEventType.OtherMouseDragged: view.OtherMouseDragged(e); break;
				case NSEventType.ScrollWheel: view.ScrollWheel(e); break;
				case NSEventType.BeginGesture: view.BeginGestureWithEvent(e); break;
				case NSEventType.EndGesture: view.EndGestureWithEvent(e); break;
				case NSEventType.MouseMoved: view.MouseMoved(e); break;
			}
		}

		public static int ScaledAndQuantizedDeltaY(this NSEvent e)
		{
			return ScaleAndQuantizeDelta((float)e.ScrollingDeltaY, e.HasPreciseScrollingDeltas);
		}

		public static int ScaledAndQuantizedDeltaX(this NSEvent e)
		{
			return ScaleAndQuantizeDelta((float)e.ScrollingDeltaX, e.HasPreciseScrollingDeltas);
		}

		private static int ScaleAndQuantizeDelta(float delta, bool precise)
		{
			return precise ? (int)(preciseDeltaScale * delta) : (int)(rawDeltaScale * delta);
		}

		public static uint ToWParam(this NSEvent e)
		{
			uint wParam = 0;

			if ((e.ModifierFlags & NSEventModifierMask.ControlKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_CONTROL;
			if ((e.ModifierFlags & NSEventModifierMask.ShiftKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_SHIFT;

			return wParam;
		}

		public static uint ButtonMaskToWParam(this NSEvent e)
		{
			return ButtonMaskToWParam(e.ButtonMask);
		}

		public static uint ButtonMaskToWParam(nuint mask)
		{
			uint wParam = 0;

			if ((mask & 1) != 0)
				wParam |= (uint)MsgButtons.MK_LBUTTON;
			if ((mask & 2) != 0)
				wParam |= (uint)MsgButtons.MK_RBUTTON;
			if ((mask & 4) != 0)
				wParam |= (uint)MsgButtons.MK_MBUTTON;
			if ((mask & 8) != 0)
				wParam |= (uint)MsgButtons.MK_XBUTTON1;
			if ((mask & 16) != 0)
				wParam |= (uint)MsgButtons.MK_XBUTTON2;

			return wParam;
		}

		public static uint ButtonNumberToWParam(this NSEvent e)
		{
			switch (e.ButtonNumberEx())
			{
				case 0: return (uint)MsgButtons.MK_LBUTTON;
				case 1: return (uint)MsgButtons.MK_RBUTTON;
				case 2: return (uint)MsgButtons.MK_MBUTTON;
				case 3: return (uint)MsgButtons.MK_XBUTTON1;
				case 4: return (uint)MsgButtons.MK_XBUTTON2;
			}
			return 0;
		}

		public static uint ToWParam(this NSEventModifierMask mask)
		{
			uint wParam = 0;

			if ((mask & NSEventModifierMask.ControlKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_CONTROL;
			if ((mask & NSEventModifierMask.ShiftKeyMask) != 0)
				wParam |= (int)MsgButtons.MK_SHIFT;

			return wParam;
		}

		public static uint ModifiersToWParam(this NSEvent e)
		{
			return e.ModifierFlags.ToWParam();
		}

		public static int ButtonNumberEx(this NSEvent e)
		{
			switch (e.Type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.LeftMouseUp:
					return 0;
				case NSEventType.RightMouseDown:
				case NSEventType.RightMouseUp:
					return 1;
				case NSEventType.OtherMouseDown:
				case NSEventType.OtherMouseUp:
					return 2;
				default:
					return 0;
			}
		}

		// 
		internal static Msg AdjustForButton(this Msg msg, NSEvent e)
		{
			int button = (int)e.ButtonNumberEx();
			if (button >= (int)NSMouseButtons.Excessive)
				button = (int)NSMouseButtons.X;
			int offset = 3 * (button - (int)NSMouseButtons.Left);
			if (button >= (int)NSMouseButtons.X)
				++offset;
			return msg + offset;
		}

		public static void TraverseSubviews(this NSView view, Action<NSView> action)
		{
			foreach (var v in view.Subviews)
			{
				action(v);
				TraverseSubviews(v, action);
			}
		}

		public static bool Contains(this NSView self, NSView view)
		{
			for (var v = view; v != null; v = v.Superview)
				if (v == self)
					return true;
			return false;
		}

		public static bool IsEnabled(this NSView view) {
			if (view is MonoView miew)
				return miew.Enabled;
			if (view is NSControl control)
				return control.Enabled;
			return true;
		}

		public static NSView FirstEnabledParentOrSelf(this NSView self)
		{
			for (var v = self; v != null; v = v.Superview)
				if (v.IsEnabled())
					return v;
			return null;
		}

		static IntPtr enclosingMenuItem = Selector.GetHandle("enclosingMenuItem");
		public static NSMenuItem EnclosingMenuItem(this NSView view)
		{
			var ptr = LibObjc.IntPtr_objc_msgSend(view.Handle, enclosingMenuItem);
			return ObjCRuntime.Runtime.GetNSObject(ptr) as NSMenuItem;
		}

		public static bool IsCopyPaste(this NSMenuItem item)
		{
			switch (item.KeyEquivalent)
			{
				case "x":
				case "c":
				case "v":
				case "a":
					if (item.KeyEquivalentModifierMask == NSEventModifierMask.CommandKeyMask)
						return true;
					break;
			}
			
			switch (item.Action?.Name)
			{
				case "cut:":
				case "copy:":
				case "paste:":
				case "selectAll:":
					return true;
			}
			
			return false;
		}

		public static IDisposable HideCopyPasteMenuItems(NSMenu? menu = null)
		{
			return MenuItemIterator.IterateItems(
				menu ?? NSApplication.SharedApplication.Menu,
				x => !x.Hidden && x.IsCopyPaste() ? x.Hidden = true : false,
				x => x.Hidden = false
			);
		}

		public static Control ToControl(this NSView view)
		{
			return view.IsSwfControl() ? Control.FromHandle(view.Handle) : Control.FromChildHandle(view.Handle);
		}

		static IntPtr getRectsBeingDrawnHandle = Selector.GetHandle("getRectsBeingDrawn:count:");

		public unsafe static CGRect[] GetRectsBeingDrawn(this NSView view)
		{
			var ptrs = new IntPtr[] { (IntPtr)0, (IntPtr)0 };
			fixed (IntPtr* rectsPtr = &ptrs[0], countPtr = &ptrs[1])
			{
				LibObjc.void_objc_msgSend_IntPtr_IntPtr(view.Handle, getRectsBeingDrawnHandle, (IntPtr)rectsPtr, (IntPtr)countPtr);
				var rects = (CGRect*)ptrs[0];
				var count = (int)ptrs[1];
				var result = new CGRect[count];
				for (int i = 0; i < count; ++i)
					result[i] = rects[i];
				return result;
			}
		}

		static IntPtr getClipsToBoundsHandle = Selector.GetHandle("clipsToBounds");
		public static bool GetClipsToBounds(this NSView view)
		{
			return LibObjc.bool_objc_msgSend(view.Handle, getClipsToBoundsHandle);
		}

		static IntPtr setClipsToBoundsHandle = Selector.GetHandle("setClipsToBounds:");
		public static void SetClipsToBounds(this NSView view, bool value)
		{
			LibObjc.void_objc_msgSend_Bool(view.Handle, setClipsToBoundsHandle, value);
		}

		public static string GetString(this NSTextView self)
		{
			var selector = new ObjCRuntime.Selector("string");
			var handle = LibObjc.IntPtr_objc_msgSend(self.Handle, selector.Handle);
			return handle != IntPtr.Zero ? CFString.FromHandle(handle) : (string)null;
		}

		public static void SetString(this NSTextView self, NSString value)
		{
			var selector = new ObjCRuntime.Selector("setString:");
			LibObjc.void_objc_msgSend_IntPtr(self.Handle, selector.Handle, value.Handle);
		}

		public static bool AllowsUndo(this NSTextView self)
		{
			var selector = new ObjCRuntime.Selector("allowsUndo");
			return LibObjc.bool_objc_msgSend(self.Handle, selector.Handle);
		}

		public static void AllowsUndo(this NSTextView self, bool value)
		{
			var selector = new ObjCRuntime.Selector("setAllowsUndo:");
			LibObjc.void_objc_msgSend_Bool(self.Handle, selector.Handle, value);
		}

		public static NSBorderType ToNSBorderType(this BorderStyle self)
		{
			switch (self)
			{
				case BorderStyle.None: return NSBorderType.NoBorder;
				case BorderStyle.FixedSingle: return NSBorderType.LineBorder;
				case BorderStyle.Fixed3D: return NSBorderType.BezelBorder;
				default: return NSBorderType.BezelBorder;
			}
		}

		public static BorderStyle ToBorderStyle(this NSBorderType self)
		{
			switch (self)
			{
				case NSBorderType.LineBorder: return BorderStyle.FixedSingle;
				case NSBorderType.BezelBorder: return BorderStyle.Fixed3D;
				case NSBorderType.GrooveBorder: return BorderStyle.Fixed3D;
				default: return BorderStyle.None;
			}
		}

		public static NSTextAlignment ToNSTextAlignment(this HorizontalAlignment self)
		{
			switch (self)
			{
				case HorizontalAlignment.Center: return NSTextAlignment.Center;
				case HorizontalAlignment.Right: return NSTextAlignment.Right;
				default: return NSTextAlignment.Left;
			}
		}

		public static HorizontalAlignment ToHorizontalAlignment(this NSTextAlignment self)
		{
			switch (self)
			{
				case NSTextAlignment.Center: return HorizontalAlignment.Center;
				case NSTextAlignment.Right: return HorizontalAlignment.Right;
				default: return HorizontalAlignment.Left;
			}
		}

		public static NSCellStateValue ToCellState(this CheckState checkState)
		{
			switch (checkState)
			{
				case CheckState.Checked: return NSCellStateValue.On;
				case CheckState.Unchecked: return NSCellStateValue.Off;
				case CheckState.Indeterminate: return NSCellStateValue.Mixed;
			}
			return NSCellStateValue.Off;
		}

		internal static Selector classSel = new Selector("class");
		internal static Selector cellClassSel = new Selector("cellClass");
		internal static Selector setCellClassSel = new Selector("setCellClass:");

		public static Class SetCellClass(this Class ctrlClass, Class cellClass)
		{
			var cellClassHandle = LibObjc.IntPtr_objc_msgSend(cellClass.Handle, classSel.Handle);
			var ctrlClassHandle = LibObjc.IntPtr_objc_msgSend(ctrlClass.Handle, classSel.Handle);
			var prevCellClassHandle = LibObjc.IntPtr_objc_msgSend(ctrlClassHandle, cellClassSel.Handle);
			LibObjc.void_objc_msgSend_IntPtr(ctrlClassHandle, setCellClassSel.Handle, cellClassHandle);
			return prevCellClassHandle == IntPtr.Zero ? (Class)null : new Class(prevCellClassHandle);
		}

		public static Class SetCellClass(this Type ctrlType, Type cellType)
		{
			return GetClass(ctrlType)?.SetCellClass(GetClass(cellType));
		}

		public static Class SetCellClass(this Type ctrlType, Class cellClass)
		{
			return GetClass(ctrlType)?.SetCellClass(cellClass);
		}

		public static Class GetClass(this Type type)
		{
			var handle = Class.GetHandle(type);
			return handle != IntPtr.Zero ? new Class(handle) : null;
		}

		[System.Diagnostics.Conditional("DEBUG")]
        public static void NotImplemented(MethodBase method)
        {
            Console.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name);
        }

		[System.Diagnostics.Conditional("DEBUG")]
        public static void NotImplemented(this NSObject o, MethodBase method)
        {
            Console.WriteLine("Not Implemented: " + method.ReflectedType.Name + "." + method.Name);
        }

		public static bool RespondsToSelector(this Type type, string selector)
		{
			return type.GetClass()?.RespondsToSelector(new Selector(selector)) ?? false;
		}

		public static bool RespondsToSelector(this Class @class, Selector selector)
		{
			var obj = ObjCRuntime.Runtime.GetNSObject(@class.Handle);
			return obj?.RespondsToSelector(selector) ?? false;
		}

		public static bool IsAccessibilitySelector(this Selector sel)
		{
			var name = sel.Name;
			return name.StartsWith("accessibility", StringComparison.InvariantCulture)
				 || name.StartsWith("setAccessibility", StringComparison.InvariantCulture);
		}

		public static bool AddMethod(this NSObject obj, Selector selector, Delegate imp)
		{
			if (obj.RespondsToSelector(selector))
				return false;

			var classHandle = obj.Class.Handle;
			return LibObjc.class_addMethod(classHandle, selector.Handle, imp, "v@:");
		}

		public static bool AddMethod(this NSObject obj, string selector, Delegate imp)
		{
			return AddMethod(obj, new Selector(selector), imp);
		}

		public static bool MarkAsSwfControl(this NSObject obj)
		{
			return obj.AddMethod(swfMarkSel, (Action)delegate { });
		}

		public static bool IsSwfControl(this NSObject obj)
		{
			return obj.RespondsToSelector(swfMarkSel);
		}

		public static bool IsSwfControl(this IntPtr handle)
		{
			return handle.ToNSObject()?.IsSwfControl() ?? false;
		}

		public static NSObject ToNSObject(this IntPtr handle)
		{
			return ObjCRuntime.Runtime.GetNSObject(handle);
		}

		internal static T ToNSObject<T>(this IntPtr handle) where T : NSObject
		{
			return (T)ObjCRuntime.Runtime.GetNSObject(handle);
		}

		internal static T AsNSObject<T>(this IntPtr handle) where T : NSObject
		{
			return ObjCRuntime.Runtime.GetNSObject(handle) as T;
		}

		internal static NSView ToNSView(this IntPtr handle)
		{
			return (NSView)ObjCRuntime.Runtime.GetNSObject(handle);
		}

		internal static NSView AsNSView(this IntPtr handle)
		{
			return ObjCRuntime.Runtime.GetNSObject(handle) as NSView;
		}

		internal static MonoView AsMonoView(this IntPtr handle)
		{
			return ObjCRuntime.Runtime.GetNSObject(handle) as MonoView;
		}

		public static NSWindow[] OrderedWindows(this NSApplication self)
		{
			var selector = new ObjCRuntime.Selector("orderedWindows");
			var ptr = LibObjc.IntPtr_objc_msgSend(self.Handle, selector.Handle);
			var array = NSArray.ArrayFromHandle<NSWindow>(ptr);
			return array;
		}

		public static bool PerformDoubleClickInCaption(this NSWindow window, NSEvent e)
		{
			if (window.StyleMask.HasFlag(NSWindowStyle.FullSizeContentView))
			{
				var action = NSUserDefaults.StandardUserDefaults.StringForKey("AppleActionOnDoubleClick");
				
				if (string.IsNullOrEmpty(action))
				{
					var miniaturize = NSUserDefaults.StandardUserDefaults.IntForKey("AppleMiniaturizeOnDoubleClick");
					action = miniaturize != 0 ? "Minimize" : "Maximize";
				}

				switch (action)
				{
					case "None":
						return true;
					case "Maximize":
						if (window.StyleMask.HasFlag(NSWindowStyle.Resizable))
						{
							window.PerformZoom(window);
							return true;
						}
						break;
					case "Minimize":
						if (window.StyleMask.HasFlag(NSWindowStyle.Miniaturizable))
						{
							window.PerformMiniaturize(window);
							return true;
						}
						break;
				}
			}
			return false;
		}

		public static bool IsLeftDoubleClick(this NSEvent e)
		{
			return e.Type == NSEventType.LeftMouseUp && e.ClickCount == 2;
		}

		public static bool IsChildOf(this NSWindow window, NSWindow parent)
		{
			for (; window != null; window = window.ParentWindow)
				if (window.ParentWindow == parent)
					return true;
			return false;
		}

		public static bool IsFullscreen(this NSWindow self)
		{
			return 0 != (self.StyleMask & NSWindowStyle.FullScreenWindow);
		}

		// The following two methods are backward compatible (10.7)

		public static CGPoint ConvertPointFromScreenSafe(this NSWindow window, CGPoint point)
		{
			return window.ConvertRectFromScreen(new CGRect(point, CGSize.Empty)).Location;
		}

		public static CGPoint ConvertPointToScreenSafe(this NSWindow window, CGPoint point)
		{
			return window.ConvertRectToScreen(new CGRect(point, CGSize.Empty)).Location;
		}

		public static CGPoint ConvertPointFromWindow(this NSWindow window, CGPoint point, NSWindow source)
		{
			return window.ConvertPointFromScreenSafe(source?.ConvertPointToScreenSafe(point) ?? point);
		}

		public static CGPoint ConvertPointToWindow(this NSWindow window, CGPoint point, NSWindow target)
		{
			var spoint = window.ConvertPointToScreenSafe(point);
			return target?.ConvertPointFromScreenSafe(spoint) ?? spoint;
		}

		public static NSDragOperation ToDragOperation(this DragDropEffects e)
		{
			var o = NSDragOperation.None;
			if ((e & DragDropEffects.Copy) != 0)
				o |= NSDragOperation.Copy;
			if ((e & DragDropEffects.Link) != 0)
				o |= NSDragOperation.Link;
			if ((e & DragDropEffects.Move) != 0)
				o |= NSDragOperation.Move;
			return o;
		}

		public static DragDropEffects ToDragDropEffects(this NSDragOperation o)
		{
			var e = DragDropEffects.None;
			if ((o & NSDragOperation.Copy) != 0)
				e |= DragDropEffects.Copy;
			if ((o & NSDragOperation.Link) != 0)
				e |= DragDropEffects.Link;
			if ((o & NSDragOperation.Move) != 0)
				e |= DragDropEffects.Move;
			return e;
		}

		public static Keys ToKeys(this NSEventModifierMask modifiers)
		{
			Keys keys = Keys.None;
			if ((NSEventModifierMask.ShiftKeyMask & modifiers) != 0) { keys |= Keys.Shift; }
			if ((NSEventModifierMask.CommandKeyMask & modifiers) != 0) { keys |= Keys.Cmd; }
			if ((NSEventModifierMask.AlternateKeyMask & modifiers) != 0) { keys |= Keys.Alt; }
			if ((NSEventModifierMask.ControlKeyMask & modifiers) != 0) { keys |= Keys.Control; }
			return keys;
		}

		public static NSEventModifierMask ToNSEventModifierMask(this Keys keys)
		{
			var modifiers = (NSEventModifierMask)0;
			if (keys.HasFlag(Keys.Shift))
				modifiers |= NSEventModifierMask.ShiftKeyMask;
			if (keys.HasFlag(Keys.Control))
				modifiers |= NSEventModifierMask.ControlKeyMask;
			if (keys.HasFlag(Keys.Cmd))
				modifiers |= NSEventModifierMask.CommandKeyMask;
			if (keys.HasFlag(Keys.Alt))
				modifiers |= NSEventModifierMask.AlternateKeyMask;
			return modifiers;
		}

		static Forms.KeysConverter keysConverter;
		static Forms.KeysConverter GetKeysConverter()
		{
			if (keysConverter == null)
				keysConverter = new Forms.KeysConverter();
			return keysConverter;
		}

		public static bool ToKeyEquivalentAndModifiers(this Keys keys, out string keyEquivalent, out NSEventModifierMask mask, bool requireModifier = true)
		{
			var mods = keys & Keys.Modifiers;
			mask = mods.ToNSEventModifierMask();

			var parts = (GetKeysConverter().ConvertToString(keys) ?? "").Split('+');
			keyEquivalent = (mods != 0 && parts.Length > 1) || (mods == 0 && parts.Length > 0) ? parts[parts.Length - 1].ToLowerInvariant() : null;
			keyEquivalent = keyEquivalent?.KeysAsStringToSymbol(true);

			return mask != 0 || !requireModifier && keyEquivalent != null;
		}

		public static bool ToKeyEquivalentAndModifiers(this string shortcutKeyDisplayString, out string keyEquivalent, out NSEventModifierMask mask, bool requireModifier = true)
		{
			try
			{
				if (!string.IsNullOrEmpty(shortcutKeyDisplayString))
				{
					var expanded = shortcutKeyDisplayString.ExpandAppleMenuShortcut();
					if (keysConverter.ConvertFromString(expanded) is Keys scut)
					{
						if (scut.ToKeyEquivalentAndModifiers(out var e, out var m, false))
						{
							keyEquivalent = e;
							mask = m;
							return true;
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}
			keyEquivalent = string.Empty;
			mask = 0;
			return false;
		}		

		public static string ToSymbol(this Keys keys, bool includeApplePrivateUnicodeSymbols = false)
		{
			return KeysAsStringToSymbol(keys.ToString(), includeApplePrivateUnicodeSymbols);
		}

		public static string KeysAsStringToSymbol(this string keysAsString, bool includeApplePrivateUnicodeSymbols = false)
		{
			var subst = ShortcutSubst[keysAsString] ?? keysAsString;
			if (includeApplePrivateUnicodeSymbols)
				subst = ApplePrivateShortcutSubst[subst] ?? subst;
			return subst;
		}

		// Converts "⌘⇧J" to Cmd+Shift+J, for example
		public static string ExpandAppleMenuShortcut(this string shortcut, string separator = "+")
		{
			var sepIndex = shortcut.IndexOf(separator);
			if (sepIndex > 0 && sepIndex < shortcut.Length - 1 || shortcut.Length == 0)
				return shortcut; // If there is a separator in the middle of the shortcut, then it's not a NSMenuItem shortcut.

			bool prefix = true;
			string result = "", sep = "";
			for (int i = 0; i < shortcut.Length; ++i)
			{
				var c = shortcut[i];

				// Modifiers can be only in the prefix (if any)
				bool isAppleModifierSymbol = c.IsAppleModifierSymbol();
				if (!prefix && isAppleModifierSymbol)
				{
					Debug.Assert(false, $"Unsupported shortcut form: {shortcut}");
					return shortcut;
				}

				prefix &= isAppleModifierSymbol;
				
				if (prefix)
				{
					var s = c.ToString();
					result += $"{sep}{SubstNames[s] ?? s}";
					sep = separator;
				}
				else
				{
					var s = shortcut.Substring(i);
					result += $"{sep}{SubstNames[s] ?? s}";
					break;
				}
			}

			return result;
		}

		const string appleModifiers = "⌃⌘⌥⇧";
		public static bool IsAppleModifierSymbol(this char c)
		{
			return appleModifiers.IndexOf(c) >= 0;
		}

		static StringDictionary StringDictionaryFromKeysAndValuesInterlaced(string[] namesAndValues, bool inversed = false)
		{
			int a = 0, b = 0;
			if (inversed) a = 1; else b = 1;

			var sd = new StringDictionary();
			for (int i = 0; i < namesAndValues.Length - 1; i += 2)
			{
				var key = namesAndValues[i + a];
				if (!sd.ContainsKey(key))
					sd[key] = namesAndValues[i + b];
			}
			return sd;
		}

		static StringDictionary subst;
		static StringDictionary ShortcutSubst
		{
			get
			{
				if (subst == null)
					subst = StringDictionaryFromKeysAndValuesInterlaced(keyNames);
				return subst;
			}
		}

		static StringDictionary names;
		static StringDictionary SubstNames
		{
			get
			{
				if (names == null)
					names = StringDictionaryFromKeysAndValuesInterlaced(keyNames, true);
				return names;
			}
		}

		static readonly string[] keyNames = {
			"Ctrl", "⌃",
			"Cmd", "⌘",
			"Alt", "⌥",
			"Shift", "⇧",

			"OemSemicolon", ";",
			"Oemplus", "+",
			"Oemcomma", ",",
			"OemMinus", "-",
			"OemPeriod", ".",
			"OemQuestion", "?",
			"Oemtilde", "~",
			"OemOpenBrackets", "[",
			"OemPipe", "|",
			"OemCloseBrackets", "]",
			"OemQuotes", "\"",
			"OemBackslash", "\\",

			"D0", "0",
			"D1", "1",
			"D2", "2",
			"D3", "3",
			"D4", "4",
			"D5", "5",
			"D6", "6",
			"D7", "7",
			"D8", "8",
			"D9", "9",

			//"Oem0", "???",
			"Oem1", ";",
			"Oem2", "/",
         	"Oem3", "~",
         	"Oem4", "[",
         	"Oem5", "|",
         	"Oem6", "]",
         	"Oem7", "'",
         	//"Oem8", "???",
         	//"Oem9", "",
         	"Oem102", "\\",
			"OemPlus", "=",

			"NumLock", "⌧",
			"NumPad0", "0",
			"NumPad1", "1",
			"NumPad2", "2",
			"NumPad3", "3",
			"NumPad4", "4",
			"NumPad5", "5",
			"NumPad6", "6",
			"NumPad7", "7",
			"NumPad8", "8",
			"NumPad9", "9",

			"Multiply", "*",
			"Add", "+",
			//"Separator", "-"
			"Subtract", "-",
			//"Decimal", "."
			"Divide", "/",

			//"OemClear", "⌧",
			"CapsLock", "⇪",
			"Enter", "⌤",
			"Return", "↩",
			"Delete", "⌦",
			"Back", "⌫",
			//"Backspace", "⌫",
			"PageUp", "⇞",
			"PageDown", "⇟",
			"Next", "⇟", // Why?
			"Eject", "⏏",
			"Escape", "⎋",
			"Home", "↖",
			"End", "↘",
			"Tab", "⇥",
			"End", "↘",
			"Space", "␣",
			"Left", "←",
			"Up", "↑",
			"Right", "→",
			"Down", "↓",

			"PrintScreen", "\xF72E",
			"Insert", "\xF727",
			"Execute", "\xF742",
			"Help", "\xF746",
			"Scroll", "\xF72F",
		};

		static StringDictionary? applePrivateShortcutSubst;
		static StringDictionary ApplePrivateShortcutSubst
		{
			get
			{
				if (applePrivateShortcutSubst == null)
					applePrivateShortcutSubst = StringDictionaryFromKeysAndValuesInterlaced(applePrivateUnicodeSymbols);
				return applePrivateShortcutSubst;
			}
		}

		static StringDictionary? applePrivateShortcutNames;
		static StringDictionary ApplePrivateShortcutNames
		{
			get
			{
				if (applePrivateShortcutNames == null)
					applePrivateShortcutNames = StringDictionaryFromKeysAndValuesInterlaced(applePrivateUnicodeSymbols, true);
				return applePrivateShortcutNames;
			}
		}

		static readonly string[] applePrivateUnicodeSymbols = {
			 "F1", "\xF704",
			 "F2", "\xF705",
			 "F3", "\xF706",
			 "F4", "\xF707",
			 "F5", "\xF708",
			 "F6", "\xF709",
			 "F7", "\xF70A",
			 "F8", "\xF70B",
			 "F9", "\xF70C",
			 "F10", "\xF70D",
			 "F11", "\xF70E",
			 "F12", "\xF70F",
			 "F13", "\xF710",
			 "F14", "\xF711",
			 "F15", "\xF712",
			 "F16", "\xF713",
			 "F17", "\xF714",
			 "F18", "\xF715",
			 "F19", "\xF716",
			 "F20", "\xF717",
			 "F21", "\xF718",
			 "F22", "\xF719",
			 "F23", "\xF71A",
			 "F24", "\xF71B",
			 "F25", "\xF71C",
			 "F26", "\xF71D",
			 "F27", "\xF71E",
			 "F28", "\xF71F",
			 "F29", "\xF720",
			 "F30", "\xF721",
			 "F31", "\xF722",
			 "F32", "\xF723",
			 "F33", "\xF724",
			 "F34", "\xF725",
			 "F35", "\xF726",
		};

		public static int GetUnicodeStringLength(this byte[] self, int max = -1)
		{
			max = max < 0 ? self.Length : Math.Min(max, self.Length);
			for (int n = 0; n < max; n += 2)
				if (self[n] == 0 && self[1 + n] == 0)
					return n;
			return max;
		}

		unsafe internal static void CopyTo(this Runtime.InteropServices.ComTypes.IStream input, System.IO.Stream output, int bufferSize = 32768)
		{
			byte[] buffer = new byte[bufferSize];
			while (true)
			{
				ulong read;
				input.Read(buffer, bufferSize, (IntPtr)(&read));
				if (read == 0)
					return;
				output.Write(buffer, 0, (int)read);
			}
		}

		public unsafe static NSData ToNSData(this IStream stream)
		{
			var data = new NSMutableData();
			byte[] buffer = new byte[32768];
			while (true)
			{
				ulong read;
				stream.Read(buffer, buffer.Length, (IntPtr)(&read));
				if (read == 0)
					return data;
				fixed (byte* value = &buffer[0])
					data.AppendBytes((IntPtr)(void*)value, (nuint)read);
			}
		}

		public static bool SupportsAllowedContentTypes(this NSSavePanel panel)
		{
			var sel = new Selector("setAllowedContentTypes:");
			return panel.RespondsToSelector(sel);
		}

		public static bool IsMojaveOrHigher(this NSProcessInfo info)
		{
			var version = info.OperatingSystemVersion;
			return (version.Major == 10 && version.Minor >= 14) || version.Major > 10;
		}

		public static bool IsCatalinaOrHigher(this NSProcessInfo info)
		{
			var version = info.OperatingSystemVersion;
			return (version.Major == 10 && version.Minor >= 15) || version.Major > 10;
		}

		public static bool IsMontereyOrHigher(this NSProcessInfo info)
		{
			return info.OperatingSystemVersion.Major >= 12;
		}

		public static bool IsSonomaOrHigher(this NSProcessInfo info)
		{
			return info.OperatingSystemVersion.Major >= 14;
		}

		public static Size GetDeviceDpi(this Control control)
		{
			var form = control?.FindForm() ?? Form.ActiveForm;
			var view = ObjCRuntime.Runtime.GetNSObject(form?.Handle ?? IntPtr.Zero) as NSView;
			return view?.Window?.DeviceDPI().ToSDSize() ?? new Size(72, 72);
		}

		#region NSPasteboard Extensions

		public static string[] GetFormats(this NSPasteboard pboard)
		{
			var types = new List<string>();
			foreach (var type in pboard.Types)
			{
				switch (type)
				{
					case Pasteboard.NSPasteboardTypeText:
						types.Add(DataFormats.Text);
						types.Add(DataFormats.UnicodeText);
						break;
					case Pasteboard.NSPasteboardTypeURL:
						if (Uri.TryCreate(pboard.GetStringForType(type), UriKind.Absolute, out Uri uri))
							types.Add(Pasteboard.UniformResourceLocatorW);
						break;
					case Pasteboard.NSPasteboardTypeHTML:
						types.Add(DataFormats.Html);
						break;
					case Pasteboard.NSPasteboardTypeRTF:
						types.Add(DataFormats.Rtf);
						break;
					case Pasteboard.NSPasteboardTypeImage:
					case Pasteboard.NSPasteboardTypePNG:
					case Pasteboard.NSPasteboardTypeTIFF:
					case Pasteboard.NSPasteboardTypeJPEG:
						types.Add(DataFormats.Bitmap);
						break;
					case Pasteboard.NSPasteboardTypeFileURL:
						types.Add(DataFormats.FileDrop);
						break;
					case Pasteboard.NSPasteboardTypeFileURLPromise:
						types.Add("FilePromise");
						break;
				}
			}

			// Special rules that decrease chance for misinterpretation of data in SWF apps
			if (types.Contains(DataFormats.FileDrop))
				types.Remove(DataFormats.Bitmap);

			return types.ToArray();
		}

		public static IDataObject GetDataObject(this NSPasteboard pboard)
		{
			return new DataObjectPasteboard(pboard);
		}

		public static object GetData(this NSPasteboard pboard, string format, bool autoConvert)
		{
			switch (format)
			{
				case DataFormats.Text:
				case DataFormats.UnicodeText:
					return pboard.GetStringForType(Pasteboard.NSPasteboardTypeText);
				case DataFormats.Rtf:
					return pboard.GetRtf();
				case DataFormats.Html:
					return pboard.GetHtml();
				case DataFormats.HtmlStream:
					return pboard.GetHtml()?.ToStream(Encoding.UTF8);
				case Pasteboard.UniformResourceLocatorW:
					return pboard.GetUri();
				case DataFormats.Bitmap:
					return pboard.GetBitmap();
				case DataFormats.FileDrop:
					return pboard.GetFileDrop();
				case "FilePromise":
					return pboard.GetFilePromise();
			}

			return null;
		}

		public static Uri GetUri(this NSPasteboard pboard)
		{
			if (Uri.TryCreate(pboard.GetStringForType(Pasteboard.NSPasteboardTypeText), UriKind.Absolute, out Uri uri))
				return uri;

			return null;
		}

		public static string GetRtf(this NSPasteboard pboard)
		{
			var data = pboard.GetDataForType(Pasteboard.NSPasteboardTypeRTF);
			if (data != null)
				return NSString.FromData(data, NSStringEncoding.ASCIIStringEncoding)?.ToString();

			return null;
		}

		public static Bitmap GetBitmap(this NSPasteboard pboard)
		{
			return new NSImage(pboard).ToCGImage()?.ToBitmap();
		}

		public static string GetHtml(this NSPasteboard pboard)
		{
			string html = pboard.GetStringForType(Pasteboard.NSPasteboardTypeHTML);

			if (html != null)
				return HtmlClip.AddMetadata(html);

			return null;
		}

		public static string[] GetFileDrop(this NSPasteboard pboard)
		{
			var paths = new List<string>();

			foreach (var item in pboard.PasteboardItems)
				if (item.GetStringForType(Pasteboard.NSPasteboardTypeFileURL) is string itemUrlString)
					if (ResolveAlias(itemUrlString) is string itemUrlResolved)
						paths.Add(itemUrlResolved);

			return paths.ToArray();
		}

		public static FilePromise GetFilePromise(this NSPasteboard pboard)
		{
			var promises = pboard.ReadObjectsForClasses(new Class[] { typeof(NSFilePromiseReceiver).GetClass() }, null);
			if (promises != null && promises.Length != 0)
				return new FilePromise(promises);
			return null;
		}

		public static string ResolveAlias(string url)
		{
			const string FilePrefix = "file://";
			if (url.StartsWith(FilePrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				// Convert Mac OS file reference URL (file:///.file/id=) to a file URL (necessary in Xamarin.Mac)
				url = new NSUrl(url).FilePathUrl.AbsoluteString;

				url = ResolveBookmark(url);
				url = Net.WebUtility.UrlDecode(url);
				url = url.Substring(FilePrefix.Length);
				url = new NSString(url).ResolveSymlinksInPath().ToString();
			}
			return url;
		}

		internal static string ResolveBookmark(string url)
		{
			var nsurl = NSUrl.FromString(url);
			if (nsurl != null)
			{
				var bookmark = NSUrl.GetBookmarkData(nsurl, out _);
				if (bookmark != null)
				{
					var resolved = new NSUrl(bookmark, NSUrlBookmarkResolutionOptions.WithoutUI, null, out var bookmarkIsStale, out _);
					if (resolved != null && !bookmarkIsStale && resolved.IsFileUrl)
						return resolved.FilePathUrl.AbsoluteString;
				}
			}
			return url;
		}

		#endregion

		#region Accessibility

		public static string AccessibilityRole(this Control control)
		{
			var role = (control?.AccessibilityObject is AccessibleObject obj) ? obj.Role : control.AccessibleRole;
			//Console.WriteLine($"AccessibilityRole({control.GetType().Name}: {role}->{role.ToNSAccessibilityRole()})");
			return role.ToNSAccessibilityRole();
		}

		public static string AccessibilityTitle(this Control control)
		{
			string text = null;
			if (control.GetStyle(ControlStyles.UseTextForAccessibility))
				text = control.Text?.RemoveMnemonic();

			return !string.IsNullOrEmpty(text) ? text : control.AccessibleTextLabel();
		}

		public static string AccessibilityLabel(this Control control)
		{
			return (control?.AccessibilityObject is AccessibleObject obj) ? obj.Description : control.AccessibleDescription;
		}

		public static NSObject AccessibilityValue(this Control control)
		{
			var obj = control.AccessibilityObject;
			if (obj.Value != null)
				return obj.Value.ToNSString();

			if (obj.State.ToAccessibilityValue() is NSObject value)
				return value;
			return null;
		}

		public static bool SupportsAccessibilityValue(this Control control)
		{
			var obj = control?.AccessibilityObject;
			if (obj?.Value is string s && String.IsNullOrWhiteSpace(s))
				return false;

			if (obj?.Value != null)
				return true;

			var role = obj != null ? obj.Role : control.AccessibleRole;
			switch (role)
			{
				case AccessibleRole.CheckButton:
				case AccessibleRole.RadioButton:
				case AccessibleRole.ComboBox:
				case AccessibleRole.Slider:
				case AccessibleRole.SpinButton:
				case AccessibleRole.ProgressBar:
					return true;
			}
			return false;
		}

		private static MethodInfo getStyleMethod;
		public static bool GetStyle(this Control control, ControlStyles style)
		{
			getStyleMethod ??= typeof(Control).GetMethod("GetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
			return (bool)(getStyleMethod?.Invoke(control, new object[] { style }) ?? false);
		}

		public static string AccessibleTextLabel(this Control control)
		{
			return (control?.AccessiblePreviousLabel()?.Text ?? control.AccessibleName)?.RemoveMnemonic();
		}

		public static Label AccessiblePreviousLabel(this Control control)
		{
			if (control?.Parent is Control parent && parent.GetContainerControl() is ContainerControl container)
				while (null != (control = container.GetNextControl(control, false)))
					if (control is Label label)
						return label;
					else if (control.Visible && control.TabStop)
						break;

			return null;
		}

		public static string RemoveMnemonic(this string s)
		{
			return s.Replace("&&", "\0").Replace("&", "").Replace("\0", "&"); // "&" => "", "&&" => "&"
		}

		public static string ToNSAccessibilityRole(this AccessibleRole role)
		{
			var result = (int)role > 0 && (int)role < accessibilityRoles.Length ? accessibilityRoles[(int)role] : null;
			//Console.WriteLine($"{role} -> {result}");
			return result;
		}
		
		readonly static string[] accessibilityRoles = new string[] {
			NSAccessibilityRoles.UnknownRole, //None		= 0,
			NSAccessibilityRoles.WindowRole, //TitleBar	= 1,
			NSAccessibilityRoles.MenuRole, //MenuBar		= 2,
			NSAccessibilityRoles.ScrollBarRole, //ScrollBar	= 3,
			NSAccessibilityRoles.HandleRole, //Grip		= 4,
			NSAccessibilityRoles.UnknownRole, //Sound		= 5,
			NSAccessibilityRoles.UnknownRole, //Cursor		= 6,
			NSAccessibilityRoles.UnknownRole, //Caret		= 7,
			NSAccessibilityRoles.SheetRole, //Alert		= 8,
			NSAccessibilityRoles.WindowRole, //Window		= 9,
			NSAccessibilityRoles.UnknownRole, //Client		= 10,
			NSAccessibilityRoles.MenuRole, //MenuPopup	= 11,
			NSAccessibilityRoles.MenuItemRole, //MenuItem	= 12,
			NSAccessibilityRoles.HelpTagRole, //ToolTip		= 13,
			NSAccessibilityRoles.ApplicationRole, //Application	= 14,
			NSAccessibilityRoles.UnknownRole, //Document	= 15,
			NSAccessibilityRoles.LayoutItemRole, //Pane		= 16,
			NSAccessibilityRoles.UnknownRole, //Chart		= 17,
			NSAccessibilitySubroles.DialogSubrole, //Dialog		= 18,
			NSAccessibilityRoles.UnknownRole, //Border		= 19,
			NSAccessibilityRoles.GroupRole, //Grouping	= 20,
			NSAccessibilityRoles.SplitGroupRole, //Separator	= 21,
			NSAccessibilityRoles.ToolbarRole,//ToolBar		= 22,
			NSAccessibilityRoles.StaticTextRole, //StatusBar	= 23,
			NSAccessibilityRoles.TableRole, //Table		= 24,
			NSAccessibilityRoles.ColumnRole, //ColumnHeader	= 25,
			NSAccessibilityRoles.RowRole, //RowHeader	= 26,
			NSAccessibilityRoles.ColumnRole, //Column		= 27,
			NSAccessibilityRoles.RowRole, //Row		= 28,
			NSAccessibilityRoles.CellRole, //Cell		= 29,
			NSAccessibilityRoles.LinkRole, //Link		= 30,
			NSAccessibilityRoles.HelpTagRole, //HelpBalloon	= 31,
			NSAccessibilityRoles.UnknownRole, //Character	= 32,
			NSAccessibilityRoles.ListRole, //List		= 33,
			NSAccessibilityRoles.ListRole, //ListItem	= 34,
			NSAccessibilityRoles.OutlineRole, //Outline		= 35,
			NSAccessibilityRoles.OutlineRole, //OutlineItem	= 36,
			NSAccessibilityRoles.TabGroupRole, //PageTab		= 37,
			NSAccessibilityRoles.PageRole, //PropertyPage	= 38,
			NSAccessibilityRoles.ValueIndicatorRole, //Indicator	= 39,
			NSAccessibilityRoles.ImageRole, //Graphic		= 40,
			NSAccessibilityRoles.StaticTextRole, //StaticText	= 41,
			NSAccessibilityRoles.TextAreaRole, //Text		= 42,
			NSAccessibilityRoles.ButtonRole, //PushButton	= 43,
			NSAccessibilityRoles.CheckBoxRole, //CheckButton	= 44,
			NSAccessibilityRoles.RadioButtonRole, //RadioButton	= 45,
			NSAccessibilityRoles.ComboBoxRole, //ComboBox	= 46,
			NSAccessibilityRoles.PopUpButtonRole, //DropList	= 47,
			NSAccessibilityRoles.ProgressIndicatorRole, //ProgressBar	= 48,
			NSAccessibilityRoles.ValueIndicatorRole, //Dial		= 49,
			NSAccessibilityRoles.UnknownRole, //HotkeyField	= 50,
			NSAccessibilityRoles.SliderRole, //Slider		= 51,
			NSAccessibilityRoles.ValueIndicatorRole, //SpinButton	= 52,
			NSAccessibilityRoles.ImageRole, //Diagram		= 53,
			NSAccessibilityRoles.UnknownRole, //Animation	= 54,
			NSAccessibilityRoles.UnknownRole, //Equation	= 55,
			NSAccessibilityRoles.PopUpButtonRole, //ButtonDropDown	= 56,
			"AXMenuButton", //ButtonMenu	= 57,
			NSAccessibilityRoles.PopUpButtonRole, //ButtonDropDownGrid= 58,
			NSAccessibilityRoles.UnknownRole, //WhiteSpace	= 59,
			NSAccessibilityRoles.TabGroupRole, //PageTabList	= 60,
			NSAccessibilityRoles.UnknownRole, //Clock		= 61,
			null,// NSAccessibilityRoles.UnknownRole, //Default		= -1,
			NSAccessibilityRoles.SplitterRole, //SplitButton	= 62,
			NSAccessibilityRoles.TextAreaRole,  //IpAddress	= 63,
			NSAccessibilityRoles.ButtonRole, //OutlineButton	= 64
		};

		public static NSObject ToAccessibilityValue(this AccessibleStates state)
		{
			switch (state)
			{
				case AccessibleStates.None:
					return new NSNumber(0);
				case AccessibleStates.Checked:
					return new NSNumber(1);
				//case AccessibleStates.Mixed:
				//	return new NSNumber(2);
			}
			return null;
		}

		#endregion // Accessibility

		#region Drag and Drop

		public static DragEventArgs ToDragEventArgs(this NSView view, INSDraggingInfo sender, DragDropEffects effect = UnusedDndEffect)
		{
			var q = view.ToMonoScreen(sender.DraggingLocation, null);
			var allowed = XplatUICocoa.DraggingAllowedEffects;
			var modifiers = NSEvent.CurrentModifierFlags;

			// translate mac modifiers to win modifiers
			// 1(bit 0)  - The left mouse button.
			// 2(bit 1)  - The right mouse button.
			// 4(bit 2)  - The SHIFT key.
			// 8(bit 3)  - The CTRL key.
			// 16(bit 4) - The middle mouse button.
			// 32(bit 5) - The ALT key.

			int keystate = 0;
			if (0 != (modifiers & NSEventModifierMask.ShiftKeyMask))
				keystate |= 4;
			if (0 != (modifiers & NSEventModifierMask.AlternateKeyMask)) // alt (mac) => ctrl (win)
				keystate |= 8;

			var idata = XplatUICocoa.DraggedData as IDataObject ?? view.ToIDataObject(sender);
			return new DragEventArgs(idata, keystate, q.X, q.Y, allowed, effect);
		}

		public static IDataObject ToIDataObject(this NSView view, INSDraggingInfo draginfo)
		{
			var idata = view.ToIDataObject(draginfo.DraggingPasteboard);
			if (idata != null)
				return idata;

			// The following code takes place when dropping images on macOS >= Ventura,
			// where real NSImage instances are transferred via the pasteboard,
			// instead of file promises or file URLs.
			// It seems that only one class can be in the following array,
			// and it must not be the NSPasteboardItem's class.
			// If it were NSPasteboardItem, then the item received
			// would be empty.
			var classes = CoreFoundation.CFArray.Create(
				typeof(NSImage).GetClass()
				//,typeof(NSString).GetClass()
				//,typeof(NSUrl).GetClass()
				//,typeof(NSColor).GetClass(),
				//,typeof(NSAttributedString).GetClass()
			);

			draginfo.EnumerateDraggingItems(NSDraggingItemEnumerationOptions.Concurrent, view, classes.Handle, new NSDictionary(), (NSDraggingItem item, nint index, ref bool stop) => {
				if (item.Item is NSImage image) {
					idata = new DataObject();
					idata.SetData(DataFormats.FileDrop, new NSImageFileDropProvider(image));
					idata.SetData(DataFormats.Bitmap, new NSImageBitmapProvider(image));
					stop = true;
				}
			});

			return idata;
		}

		class NSImageBitmapProvider : DataObject.IDropDataProvider
		{
			NSImage image;
			public NSImageBitmapProvider(NSImage image) => this.image = image;
			public object GetData(string format) => image.ToBitmap();
		}

		class NSImageFileDropProvider : DataObject.IDropDataProvider
		{
			NSImage image;
			public NSImageFileDropProvider(NSImage image) => this.image = image;
			public object GetData(string format)
			{
				System.Diagnostics.Debug.Assert(format == DataFormats.FileDrop);

				foreach (var rep in image.Representations())
					if (rep.BitsPerSample == 0)
						if (SaveAsPdf() is string pdf)
							return new string[] { pdf };

				if (SaveAsPng() is string png)
					return new string[] { png };

				return null;
			}

			string SaveAsPng()
			{
				if (image.AsTiff() is NSData tiffData)
				{
					var rep = new NSBitmapImageRep(tiffData);
				    var data = rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png);
					var path = CreateTempFileName("png");
					if (data.Save(path, (NSDataWritingOptions)0, out var _))
						return path;
				}
				return null;
			}

			string SaveAsPdf()
			{
				var frame = new CGRect(CGPoint.Empty, image.Size);
				var view = new NSImageView(frame);
				view.Image = image;
				var data = view.DataWithPdfInsideRect(frame);
				var path = CreateTempFileName("pdf");
				if (data.Save(path, (NSDataWritingOptions)0, out var _))
					return path;
				return null;
			}

			string CreateTempFileName(string extension)
			{
				var folder = NSFileManager.TemporaryDirectory;
				var now = DateTime.Now.ToString("HHmmss");
				var fm = NSFileManager.DefaultManager;
				var suffix = "";
				for (int i = 1; i < 100000; ++i)
				{
					var path = new NSUrl(folder, true).Append($"{now}{suffix}", false).AppendPathExtension(extension).Path;
					if (!fm.FileExists(path))
						return path;
					suffix = $"-{i}";
				}
				return new NSUrl(folder, true).Append($"{new Guid().ToString()}", false).AppendPathExtension(extension).Path;
			}
		}

		public static IDataObject ToIDataObject(this NSView view, NSPasteboard pboard)
		{
			var types = pboard.Types;
			if (Array.IndexOf(types, XplatUICocoa.IDataObjectPboardType) != -1)
				if (XplatUICocoa.DraggedData is IDataObject idata)
					return idata;

			var s = pboard.GetStringForType(XplatUICocoa.NSStringPboardType);
			if (s != null)
				return new DataObject(s);

			s = pboard.GetStringForType(XplatUICocoa.UTTypeFileUrl);
			if (s != null)
			{
				var paths = new List<string>();
				foreach (var item in pboard.PasteboardItems)
				{
					var url = item.GetStringForType(XplatUICocoa.UTTypeFileUrl);
					paths.Add(NSUrl.FromString(url).Path);
				}

				if (paths.Count != 0)
					return new DataObject(DataFormats.FileDrop, paths.ToArray());
			}

			// TODO: Add more conversions/wrappers - for files, images etc.
			// See DataObjectPasteboard - merge somehow?

			return null;
		}

		const DragDropEffects UnusedDndEffect = unchecked((DragDropEffects)0xffffffff);

		public static Control GetDropControl(this NSView view)
		{
			for (var v = view; v != null; v = v.Superview)
				if (v is MonoView mv && mv.flags.HasFlag(MonoView.Flags.AllowDrop))
					return Control.FromHandle(mv.Handle);
			return null;
		}

		public static NSDragOperation DraggingEnteredInternal(this NSView view, INSDraggingInfo sender)
		{
			try
			{
				var control = view.GetDropControl();
				if (null != control)
				{
					var e = view.ToDragEventArgs(sender);
					using var _ = XplatUICocoa.ToggleDraggedData(e);
					control.DndEnter(e);
					if (e.Effect != UnusedDndEffect)
						return (XplatUICocoa.DraggingEffects = e.Effect.BestOf()).ToDragOperation();

					XplatUICocoa.DraggingEffects = DragDropEffects.None;
				}
			}
			catch
			{
				return NSDragOperation.None;
			}
			return NSDragOperation.Generic;
		}

		public static NSDragOperation DraggingUpdatedInternal(this NSView view, INSDraggingInfo sender)
		{
			try
			{
				var driver = XplatUICocoa.GetInstance();
				if (!driver.draggingSource.Cancelled)
				{
					var source = Control.FromHandle(driver.draggingSource.ViewHandle);
					var args = new QueryContinueDragEventArgs(0, false, DragAction.Continue);
					source?.DndContinueDrag(args);
					if (args.Action == DragAction.Cancel)
					{
						// It seems there is no way to cancel dragging on macOS.
						// Anyway, we have to stop sending QueryContinue events.
						driver.draggingSource.Cancelled = true;
					}
				}

				var control = view.GetDropControl();
				if (null != control)
				{
					// TODO: Prevent expensive data conversions here!
					var e = view.ToDragEventArgs(sender);
					using var _ = XplatUICocoa.ToggleDraggedData(e);
					control.DndOver(e);
					if (e.Effect != UnusedDndEffect)
						XplatUICocoa.DraggingEffects = e.Effect = e.Effect.BestOf();

					return XplatUICocoa.DraggingEffects.ToDragOperation();
				}
			}
			catch
			{
				return NSDragOperation.None;
			}
			return NSDragOperation.Generic;
		}

		public static void DraggingExitedInternal(this NSView view, INSDraggingInfo sender)
		{
			try
			{
				var control = view.GetDropControl();
				if (null != control)
				{
					var e = view.ToDragEventArgs(sender);
					using var _ = XplatUICocoa.ToggleDraggedData(e);
					control.DndLeave(e);
				}
			}
			catch
			{
			}
		}

		public static DragDropEffects BestOf(this DragDropEffects effect)
		{
			switch (effect)
			{
				case DragDropEffects.All: return DragDropEffects.Copy;
				case (DragDropEffects.Copy | DragDropEffects.Move): return DragDropEffects.Copy;
				case (DragDropEffects.Move | DragDropEffects.Scroll): return DragDropEffects.Move;
				default: return effect;
			}
		}

		public static Point ToMonoScreen(this NSView src, CGPoint p, NSView view)
		{
			if (view != null)
				p = src.ConvertPointToView(p, null);
			var r = src.Window.ConvertRectToScreen(new CGRect(p, CGSize.Empty));
			return XplatUICocoa.GetInstance().NativeToMonoScreen(r.Location);
		}

		public static void RegisterForGenericDraggedTypes(this NSView view, bool value = true)
		{
			try
			{
				if (value)
					view.RegisterForDraggedTypes(new string[] { XplatUICocoa.IDataObjectPboardType, XplatUICocoa.UTTypeItem });
				else
					view.UnregisterDraggedTypes();
			}
			catch (Exception e)
			{
				Diagnostics.Debug.Assert(false, $"Failed to register for dragged type: {e}");
			}
		}

		public static bool PrepareForDragOperationInternal(this NSView view, INSDraggingInfo sender)
		{
			foreach (var type in sender.DraggingPasteboard.Types)
			{
				switch (type)
				{
					case XplatUICocoa.UTTypeUTF8PlainText:
					case XplatUICocoa.NSStringPboardType:
					case XplatUICocoa.IDataObjectPboardType:
					case XplatUICocoa.UTTypeFileUrl:
						return true;
					default:
						var utt = UTType.CreateFromIdentifier(type);
						if (utt != null && utt.ConformsTo(UTTypes.Image))
							return true;
						break;
				}
			}
			return false;
		}

		public static bool PerformDragOperationInternal(this NSView view, INSDraggingInfo sender)
		{
			try
			{
				XplatUICocoa.PerformDragOperationCalled = true;
				if (view.GetDropControl() is IDropTarget dt)
				{
					var e = view.ToDragEventArgs(sender, XplatUICocoa.DraggingEffects);
					if (e != null)
					{
						using var _ = XplatUICocoa.ToggleDraggedData(e);
						dt.OnDragDrop(e);
						if (e.Effect != UnusedDndEffect)
							XplatUICocoa.DraggingEffects = e.Effect;

						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public static void DraggingEndedInternal(this NSView view, INSDraggingInfo sender)
		{
			sender.DraggingPasteboard.ClearContents();
			XplatUICocoa.OnDraggingEnded(view, sender);
		}
		
		public static void InvokeMenuWillOpenDeep(this NSMenu? menu)
		{
			menu?.Delegate?.MenuWillOpen(menu);

			foreach (var item in menu.Items)
				if (item.HasSubmenu)
					InvokeMenuWillOpenDeep(item.Submenu);
		}

		#endregion


		public static void WriteObject(this NSPasteboard pboard, INSPasteboardWriting pasteboardWriting)
		{
			pboard.WriteObjects(new INSPasteboardWriting[] { pasteboardWriting });
		}

		public static INSPasteboardWriting AsPasteboardWriting(this NSObject self)
		{
			return (INSPasteboardWriting)self;
		}

		public static INSPasteboardWriting AsPasteboardWriting(this String self)
		{
			return (INSPasteboardWriting)(NSString)self;
		}
	}

	public class NSControlSetCellClass : IDisposable
	{
		Class ctrl, cell;

		public NSControlSetCellClass(Type ctrlType, Type cellType)
		{
			this.ctrl = ctrlType.GetClass();
			this.cell = ctrl?.SetCellClass(cellType.GetClass());
		}

		public void Dispose()
		{
			ctrl?.SetCellClass(cell);
		}
	}

	public static class AEKeyword {
		public static uint DirectObject { get { return "----".ToFourCC(); } }
		public static uint ErrorNumber { get { return "errn".ToFourCC(); } }
		public static uint ErrorString { get { return "errs".ToFourCC(); } }
		public static uint ProcessSerialNumber { get { return "psn ".ToFourCC(); } }
		public static uint PreDispatch { get { return "phac".ToFourCC(); } }
		public static uint SelectProc { get { return "selh".ToFourCC(); } }
		public static uint AERecorderCount { get { return "recr".ToFourCC(); } }
		public static uint AEVersion { get { return "vers".ToFourCC(); } }
	}

	public class FilePromise
	{
		protected static NSOperationQueue queue;
		protected NSObject[] promises;

		public FilePromise(NSObject[] promises)
		{
			this.promises = promises;
		}

		public virtual void Serialize(Action<string> callback)
		{
			foreach (NSFilePromiseReceiver promise in promises)
				Serialize(promise, callback);
		}

		protected virtual void Serialize(NSFilePromiseReceiver promise, Action<string> callback)
		{
			if (queue == null) queue = new NSOperationQueue();

			var tempPath = IO.Path.GetTempPath();
			var tempPathUrl = NSUrl.FromFilename(tempPath);

			promise.ReceivePromisedFiles(tempPathUrl, new NSDictionary(), queue, (destUrl, error) => {
				if (error == null && callback != null)
					NSApplication.SharedApplication.BeginInvokeOnMainThread(() => callback(destUrl.Path));
			});
		}
	}

	[Flags]
	public enum NSMouseFlags
	{
		None = 0x00,
		ClickCount = 0x01,
		Pressure = 0x02,
		Down = 0x04,
		Up = 0x08,
		Drag = 0x10,
	}

	public delegate bool SelectMenuItemCallback(NSMenuItem item);
	public delegate void RestoreMenuItemCallback(NSMenuItem item);

	class MenuItemIterator : IDisposable
	{
		RestoreMenuItemCallback restoreCallback;
		List<NSMenuItem> items = new();

		public static MenuItemIterator IterateItems(NSMenu menu, SelectMenuItemCallback selectCallback, RestoreMenuItemCallback restoreCallback)
		{
			return new MenuItemIterator(menu, selectCallback, restoreCallback);
		}

		MenuItemIterator(NSMenu menu, SelectMenuItemCallback selectCallback, RestoreMenuItemCallback restoreCallback)
		{
			this.restoreCallback = restoreCallback;
			Iterate(menu, selectCallback);
		}

		void Iterate(NSMenu menu, SelectMenuItemCallback callback)
		{
			foreach (var item in menu.Items)
			{
				if (callback(item))
					items.Add(item);
				if (item.HasSubmenu)
					Iterate(item.Submenu, callback);
			}
		}

		void Iterate(List<NSMenuItem> items, RestoreMenuItemCallback callback, bool clearItems)
		{
			foreach (var item in items)
				callback(item);

			if (clearItems)
				items.Clear();
		}

		public void Dispose()
		{
			Iterate(items, restoreCallback, true);
		}
	}
}

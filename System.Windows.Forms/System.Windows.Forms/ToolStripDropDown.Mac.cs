using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.CocoaInternal;
using System.Windows.Forms.Mac;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace System.Windows.Forms
{
	public partial class ToolStripDropDown
	{
		NSMenu currentMenu;

		private void CancelGrab()
		{
			IntPtr grabHandle;
			bool grabConfined;
			Drawing.Rectangle grabArea;
			XplatUI.GrabInfo(out grabHandle, out grabConfined, out grabArea);
			if (grabHandle != IntPtr.Zero)
				XplatUI.SendMessage(grabHandle, Msg.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
		}

		NSMenu nsMenu;
		internal NSMenu ToNSMenu()
		{
			if (nsMenu == null)
			{
				nsMenu = new NSMenu();
				nsMenu.Delegate = new MonoMenuDelegate(this, nsMenu);
				nsMenu.AutoEnablesItems = false;
			}
			return nsMenu;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (nsMenu != null)
				{
					nsMenu.Delegate?.Dispose();
					nsMenu.Delegate = null;
					nsMenu.RemoveAllItems();
					nsMenu.Dispose();
					nsMenu = null;
				}
			}

			base.Dispose(disposing);
		}

		[Register("__xmonomac_internal_ActionDispatcher")]
		internal class ActionDispatcher : NSObject
		{
			const string skey = "__xmonomac_internal_ActionDispatcher_activated:";
			const string dkey = "__xmonomac_internal_ActionDispatcher_doubleActivated:";
			public static Selector Action = new Selector(skey);
			public static Selector DoubleAction = new Selector(dkey);
			public EventHandler Activated;
			public EventHandler DoubleActivated;
			//public Func<NSMenuItem, bool> ValidateMenuItemFunc;

			[Preserve, Export(skey)]
			public void OnActivated(NSObject sender)
			{
				EventHandler handler = Activated;
				if (handler != null)
					handler(sender, EventArgs.Empty);
			}

			[Preserve, Export(dkey)]
			public void OnActivated2(NSObject sender)
			{
				EventHandler handler = DoubleActivated;
				if (handler != null)
					handler(sender, EventArgs.Empty);
			}

			public ActionDispatcher(EventHandler handler)
			{
				IsDirectBinding = false;
				Activated = handler;
			}

			public ActionDispatcher()
			{
				IsDirectBinding = false;
			}

			public static NSObject SetupAction(NSObject target, EventHandler handler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
				{
					ctarget = new ActionDispatcher();
				}
				ctarget.Activated += handler;
				return ctarget;
			}

			public static void RemoveAction(NSObject target, EventHandler handler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
					return;
				ctarget.Activated -= handler;
			}

			public static NSObject SetupDoubleAction(NSObject target, EventHandler doubleHandler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
				{
					ctarget = new ActionDispatcher();
				}
				ctarget.DoubleActivated += doubleHandler;
				return ctarget;
			}

			public static void RemoveDoubleAction(NSObject target, EventHandler doubleHandler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
					return;
				ctarget.DoubleActivated -= doubleHandler;
			}

			/*public bool ValidateMenuItem(NSMenuItem menuItem)
			{
				if (ValidateMenuItemFunc != null)
					return ValidateMenuItemFunc(menuItem);

				return true;
			}*/

			public virtual Boolean WorksWhenModal
			{
				[Export("worksWhenModal")]
				get { return true; }
			}
		}

		class MonoMenuDelegate : NSMenuDelegate
		{
			ToolStripDropDown owner;
			NSMenu menu;
			bool beforePopupCalled;

			public MonoMenuDelegate(ToolStripDropDown owner, NSMenu menu)
			{
				this.owner = owner;
				this.menu = menu;
			}

			public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
			{
				//base.MenuWillHighlightItem(menu, item);
			}

			public void BeforePopup()
			{
				// Early opening before we call NSMenu.PopUpMenu
				beforePopupCalled = true;
				RecreateNativeItems();
			}

			internal virtual void RecreateNativeItems()
			{
				menu.RemoveAllItems();
				foreach (ToolStripItem item in owner.Items)
				{
					var menuItem = item.ToNSMenuItem();
					var actionObj = new ActionDispatcher((sender, e) =>
						{
							owner.OnItemClicked(new ToolStripItemClickedEventArgs(item));
							item.PerformClick();
						});
					menuItem.Target = actionObj;
					menuItem.Action = ActionDispatcher.Action;
					menuItem.Menu?.RemoveItem(menuItem);
					menu.AddItem(menuItem);
				}
			}

			public override void MenuWillOpen(NSMenu menu)
			{
				if (owner.currentMenu != menu)
				{
					var cancelEventArgs = new CancelEventArgs(); 
					if (owner.owner_item != null && owner.owner_item is ToolStripDropDownItem)
					{
						ToolStripDropDownItem dropdown_owner = (ToolStripDropDownItem)owner.owner_item;
						dropdown_owner.OnDropDownShow(cancelEventArgs);
					}
					if (!cancelEventArgs.Cancel)
						owner.OnOpening(cancelEventArgs);
					if (cancelEventArgs.Cancel)
					{
						menu.CancelTrackingWithoutAnimation();
						return;
					}
				}

				owner.is_visible = true;
				owner.currentMenu = menu;
				owner.OnVisibleChanged(EventArgs.Empty);

				// Send WM_CANCELMODE to cancel any grabs
				owner.CancelGrab();

				// Convert all the menu items to NSMenuItems (w/ embedded views if necessary)
				if (!beforePopupCalled)
					BeforePopup();

				// Get rid of expansion triangles when there is no visible menu in the submenu
				for (int i = 0; i < menu.Items.Length && i < owner.Items.Count; ++i)
				  	RemoveSubmenuIfEmpty(menu.Items[i], owner.Items[i]);

				owner.OnOpened(EventArgs.Empty);
			}

			void RemoveSubmenuIfEmpty(NSMenuItem item, ToolStripItem ownerItem)
			{
				if (item.HasSubmenu && ownerItem is ToolStripDropDownItem dropDownItem)
					if (!ContainsVisibleItems(dropDownItem))
						item.Submenu = null;
			}

			bool ContainsVisibleItems(ToolStripDropDownItem item)
			{
				foreach(ToolStripItem sub in item.DropDownItems)
					if (sub.InternalVisible)
						return true;
				return false;
			}

			public override void MenuDidClose(NSMenu menu)
			{
				// Send WM_CANCELMODE to cancel any grabs
				menu.RemoveAllItems();
				owner.CancelGrab();
				owner.currentMenu = null;
				owner.Close();
				beforePopupCalled = false;

				XplatUICocoa.UpdateModifiers(NSEvent.CurrentModifierFlags);

				if (Application.KeyboardCapture == owner)
					Application.KeyboardCapture.KeyboardActive = false;
			}	
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (currentMenu == null)
				base.OnPaintBackground(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (currentMenu == null)
				base.OnPaint(e);
		}

		internal void PostMouseUp(Control control, Point p)
		{
			var msg = WindowsEventResponder.LastMouseDownMsg;
			if (msg.hwnd != IntPtr.Zero)
				XplatUI.PostMessage(msg.hwnd,(Msg)( ((int)msg.message) - (int)Msg.WM_LBUTTONDOWN + (int)Msg.WM_LBUTTONUP), msg.wParam, msg.lParam);
		}
	}
}

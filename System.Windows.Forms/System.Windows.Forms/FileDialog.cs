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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//
//  Alexander Olk	alex.olk@googlemail.com
//  Gert Driesen (drieseng@users.sourceforge.net)
//  Eric Petit (surfzoid2002@yahoo.fr)
//
// TODO:
// Keyboard shortcuts (DEL, F5, F2)
// ??

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Windows.Forms
{

	#region PopupButtonPanel
	internal class PopupButtonPanel : Control, IUpdateFolder
	{
		#region PopupButton
		internal class PopupButton : Control
		{
			internal enum PopupButtonState
			{ Normal, Down, Up}
			
			private Image image = null;
			private PopupButtonState popupButtonState = PopupButtonState.Normal;
			private StringFormat text_format = new StringFormat();
			private Rectangle text_rect = Rectangle.Empty;
			
			public PopupButton ()
			{
				text_format.Alignment = StringAlignment.Center;
				text_format.LineAlignment = StringAlignment.Near;
				
				SetStyle (ControlStyles.DoubleBuffer, true);
				SetStyle (ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.UserPaint, true);
				SetStyle (ControlStyles.Selectable, false);
			}
			
			public Image Image {
				set {
					image = value;
					Invalidate ();
				}
				
				get {
					return image;
				}
			}
			
			public PopupButtonState ButtonState {
				set {
					popupButtonState = value;
					Invalidate ();
				}
				
				get {
					return popupButtonState;
				}
			}

			#region UIA Framework Members
			internal void PerformClick ()
			{
				OnClick (EventArgs.Empty);
			}
			#endregion

			protected override void OnPaint (PaintEventArgs pe)
			{
				Draw (pe);
				
				base.OnPaint (pe);
			}
			
			private void Draw (PaintEventArgs pe)
			{
				Graphics gr = pe.Graphics;
				
				gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
				
				// draw image
				if (image != null) {
					int i_x = (ClientSize.Width - image.Width) / 2;
					int i_y = 4;
					gr.DrawImage (image, i_x, i_y);
				}
				
				if (Text != String.Empty) {
					if (text_rect == Rectangle.Empty)
						text_rect = new Rectangle (0, Height - 30, Width, Height - 30); 
					
					gr.DrawString (Text, Font, Brushes.White, text_rect, text_format);
				}
				
				switch (popupButtonState) {
					case PopupButtonState.Up:
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), 0, 0, ClientSize.Width - 1, 0);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), 0, 0, 0, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1);
						break;
						
					case PopupButtonState.Down:
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), 0, 0, ClientSize.Width - 1, 0);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), 0, 0, 0, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1);
						break;
				}
			}
			
			protected override void OnMouseEnter (EventArgs e)
			{
				if (popupButtonState != PopupButtonState.Down)
					popupButtonState = PopupButtonState.Up;
				
				PopupButtonPanel panel = Parent as PopupButtonPanel;
				
				if (panel.focusButton != null && panel.focusButton.ButtonState == PopupButtonState.Up) {
					panel.focusButton.ButtonState = PopupButtonState.Normal;
					panel.SetFocusButton (null);
				}
				Invalidate ();
				base.OnMouseEnter (e);
			}
			
			protected override void OnMouseLeave (EventArgs e)
			{
				if (popupButtonState == PopupButtonState.Up)
					popupButtonState = PopupButtonState.Normal;
				Invalidate ();
				base.OnMouseLeave (e);
			}
			
			protected override void OnClick (EventArgs e)
			{
				popupButtonState = PopupButtonState.Down;
				Invalidate ();
				base.OnClick (e);
			}
		}
		#endregion
		
		private PopupButton recentlyusedButton;
		private PopupButton desktopButton;
		private PopupButton personalButton;
		private PopupButton mycomputerButton;
		private PopupButton networkButton;
		
		private PopupButton lastPopupButton = null;
		private PopupButton focusButton = null;
		
		private string currentPath;
		
		private int currentFocusIndex;
		
		public PopupButtonPanel ()
		{
			SuspendLayout ();
			
			BackColor = Color.FromArgb (128, 128, 128);
			Size = new Size (85, 336);
			InternalBorderStyle = BorderStyle.Fixed3D;
			
			recentlyusedButton = new PopupButton ();
			desktopButton = new PopupButton ();
			personalButton = new PopupButton ();
			mycomputerButton = new PopupButton ();
			networkButton = new PopupButton ();
			
			recentlyusedButton.Size = new Size (81, 64);
			recentlyusedButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesRecentDocuments, 32);
			recentlyusedButton.BackColor = BackColor;
			recentlyusedButton.ForeColor = Color.Black;
			recentlyusedButton.Location = new Point (2, 2);
			recentlyusedButton.Text = "Recently\nused";
			recentlyusedButton.Click += new EventHandler (OnClickButton);
			
			desktopButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesDesktop, 32);
			desktopButton.BackColor = BackColor;
			desktopButton.ForeColor = Color.Black;
			desktopButton.Size = new Size (81, 64);
			desktopButton.Location = new Point (2, 66);
			desktopButton.Text = "Desktop";
			desktopButton.Click += new EventHandler (OnClickButton);
			
			personalButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesPersonal, 32);
			personalButton.BackColor = BackColor;
			personalButton.ForeColor = Color.Black;
			personalButton.Size = new Size (81, 64);
			personalButton.Location = new Point (2, 130);
			personalButton.Text = "Personal";
			personalButton.Click += new EventHandler (OnClickButton);
			
			mycomputerButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesMyComputer, 32);
			mycomputerButton.BackColor = BackColor;
			mycomputerButton.ForeColor = Color.Black;
			mycomputerButton.Size = new Size (81, 64);
			mycomputerButton.Location = new Point (2, 194);
			mycomputerButton.Text = "My Computer";
			mycomputerButton.Click += new EventHandler (OnClickButton);
			
			networkButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesMyNetwork, 32);
			networkButton.BackColor = BackColor;
			networkButton.ForeColor = Color.Black;
			networkButton.Size = new Size (81, 64);
			networkButton.Location = new Point (2, 258);
			networkButton.Text = "My Network";
			networkButton.Click += new EventHandler (OnClickButton);
			
			Controls.Add (recentlyusedButton);
			Controls.Add (desktopButton);
			Controls.Add (personalButton);
			Controls.Add (mycomputerButton);
			Controls.Add (networkButton);
			
			ResumeLayout (false);
			
			KeyDown += new KeyEventHandler (Key_Down);
			
			SetStyle (ControlStyles.StandardClick, false);
		}

		void OnClickButton (object sender, EventArgs e)
		{
			if (lastPopupButton != null && lastPopupButton != sender as PopupButton)
				lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
			lastPopupButton = sender as PopupButton;
			
			if (sender == recentlyusedButton) {
				currentPath = MWFVFS.RecentlyUsedPrefix;
			} else
			if (sender == desktopButton) {
				currentPath = MWFVFS.DesktopPrefix;
			} else
			if (sender == personalButton) {
				currentPath = MWFVFS.PersonalPrefix;
			} else
			if (sender == mycomputerButton) {
				currentPath = MWFVFS.MyComputerPrefix;
			} else
			if (sender == networkButton) {
				currentPath = MWFVFS.MyNetworkPrefix;
			}
			
			EventHandler eh = (EventHandler)(Events [PDirectoryChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		static object UIAFocusedItemChangedEvent = new object ();

		internal event EventHandler UIAFocusedItemChanged {
			add { Events.AddHandler (UIAFocusedItemChangedEvent, value); }
			remove { Events.RemoveHandler (UIAFocusedItemChangedEvent, value); }
		}

		internal void OnUIAFocusedItemChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAFocusedItemChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal PopupButton UIAFocusButton {
			get {
				return focusButton;
			}
		}

		public string CurrentFolder {
			set {
				string currentPath = value;
				if (currentPath == MWFVFS.RecentlyUsedPrefix) {
					if (lastPopupButton != recentlyusedButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						recentlyusedButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = recentlyusedButton;
					}
				} else
				if (currentPath == MWFVFS.DesktopPrefix) {
					if (lastPopupButton != desktopButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						desktopButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = desktopButton;
					}
				} else
				if (currentPath == MWFVFS.PersonalPrefix) {
					if (lastPopupButton != personalButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						personalButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = personalButton;
					}
				} else
				if (currentPath == MWFVFS.MyComputerPrefix) {
					if (lastPopupButton != mycomputerButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						mycomputerButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = mycomputerButton;
					}
				} else
				if (currentPath == MWFVFS.MyNetworkPrefix) {
					if (lastPopupButton != networkButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						networkButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = networkButton;
					}
				} else {
					if (lastPopupButton != null) {
						lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						lastPopupButton = null;
					}
				}
			}
			get {
				return currentPath;
			}
		}
		
		protected override void OnGotFocus (EventArgs e)
		{
			if (lastPopupButton != recentlyusedButton) {
				recentlyusedButton.ButtonState = PopupButton.PopupButtonState.Up;
				SetFocusButton (recentlyusedButton);
			}
			currentFocusIndex = 0;
			
			base.OnGotFocus (e);
		}
		
		protected override void OnLostFocus (EventArgs e)
		{
			if (focusButton != null && focusButton.ButtonState != PopupButton.PopupButtonState.Down)
				focusButton.ButtonState = PopupButton.PopupButtonState.Normal;
			base.OnLostFocus (e);
		}
		
		protected override bool IsInputKey (Keys key)
		{
			switch (key) {
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Left:
				case Keys.Enter:
					return true;
			}
			return base.IsInputKey (key);
		}
		
		private void Key_Down (object sender, KeyEventArgs e)
		{
			bool update_focus = false;
			
			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) {
				currentFocusIndex --;
				
				if (currentFocusIndex < 0)
					currentFocusIndex = Controls.Count - 1;
				
				update_focus = true;
			} else
			if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Right) {
				currentFocusIndex++;
				
				if (currentFocusIndex == Controls.Count)
					currentFocusIndex = 0;
				
				update_focus = true;
			} else
			if (e.KeyCode == Keys.Enter) {
				focusButton.ButtonState = PopupButton.PopupButtonState.Down;
				OnClickButton (focusButton, EventArgs.Empty);
			}
			
			if (update_focus) {
				PopupButton newfocusButton = Controls [currentFocusIndex] as PopupButton;
				if (focusButton != null && focusButton.ButtonState != PopupButton.PopupButtonState.Down)
					focusButton.ButtonState = PopupButton.PopupButtonState.Normal;
				if (newfocusButton.ButtonState != PopupButton.PopupButtonState.Down)
					newfocusButton.ButtonState = PopupButton.PopupButtonState.Up;
				SetFocusButton (newfocusButton);
			}
			
			e.Handled = true;
		}
		
		static object PDirectoryChangedEvent = new object ();
		
		public event EventHandler DirectoryChanged {
			add { Events.AddHandler (PDirectoryChangedEvent, value); }
			remove { Events.RemoveHandler (PDirectoryChangedEvent, value); }
		}

		internal void SetFocusButton (PopupButton button)
		{
			if (button == focusButton)
			return;

			focusButton = button;
				OnUIAFocusedItemChanged ();
		}
	}
	#endregion
	
	#region DirComboBox
	internal class DirComboBox : ComboBox, IUpdateFolder
	{
		#region DirComboBoxItem
		internal class DirComboBoxItem
		{
			private int imageIndex;
			private string name;
			private string path;
			private int xPos;
			private ImageList imageList;
			
			public DirComboBoxItem (ImageList imageList, int imageIndex, string name, string path, int xPos)
			{
				this.imageList = imageList;
				this.imageIndex = imageIndex;
				this.name = name;
				this.path = path;
				this.xPos = xPos;
			}
			
			public int ImageIndex {
				get {
					return imageIndex;
				}
			}
			
			public string Name {
				get {
					return name;
				}
			}
			
			public string Path {
				get {
					return path;
				}
			}
			
			public int XPos {
				get {
					return xPos;
				}
			}
			
			public ImageList ImageList {
				set {
					imageList = value;
				}
				
				get {
					return imageList;
				}
			}
			#region UIA Framework Members
			public override string ToString ()
			{
				return name;
			}
			#endregion
		}
		#endregion
		
		private ImageList imageList = new ImageList();
		
		private string currentPath;
		
		private Stack folderStack = new Stack();
		
		private static readonly int indent = 6;
		
		private DirComboBoxItem recentlyUsedDirComboboxItem;
		private DirComboBoxItem desktopDirComboboxItem;
		private DirComboBoxItem personalDirComboboxItem;
		private DirComboBoxItem myComputerDirComboboxItem;
		private DirComboBoxItem networkDirComboboxItem;
		
		private ArrayList myComputerItems = new ArrayList ();
		
		private DirComboBoxItem mainParentDirComboBoxItem = null;
		private DirComboBoxItem real_parent = null;
		
		private MWFVFS vfs;
		
		public DirComboBox (MWFVFS vfs)
		{
			this.vfs = vfs;

			SuspendLayout ();
			
			DrawMode = DrawMode.OwnerDrawFixed;
			
			imageList.ColorDepth = ColorDepth.Depth32Bit;
			imageList.ImageSize = new Size (16, 16);
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesRecentDocuments, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesDesktop, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesPersonal, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyComputer, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyNetwork, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.NormalFolder, 16));
			imageList.TransparentColor = Color.Transparent;
			
			recentlyUsedDirComboboxItem = new DirComboBoxItem (imageList, 0, "Recently used", MWFVFS.RecentlyUsedPrefix, 0);
			desktopDirComboboxItem = new DirComboBoxItem (imageList, 1, "Desktop", MWFVFS.DesktopPrefix, 0);
			personalDirComboboxItem = new DirComboBoxItem (imageList, 2, "Personal folder", MWFVFS.PersonalPrefix, indent);
			myComputerDirComboboxItem = new DirComboBoxItem (imageList, 3, "My Computer", MWFVFS.MyComputerPrefix, indent);
			networkDirComboboxItem = new DirComboBoxItem (imageList, 4, "My Network", MWFVFS.MyNetworkPrefix, indent);
			
			ArrayList al = this.vfs.GetMyComputerContent ();
			
			foreach (FSEntry fsEntry in al) {
				myComputerItems.Add (new DirComboBoxItem (MimeIconEngine.LargeIcons, fsEntry.IconIndex, fsEntry.Name, fsEntry.FullName, indent * 2));
			}
			
			al.Clear ();
			al = null;
			
			mainParentDirComboBoxItem = myComputerDirComboboxItem;
			
			ResumeLayout (false);
		}
		
		public string CurrentFolder {
			set {
				currentPath = value;
				
				CreateComboList ();
			}
			get {
				return currentPath;
			}
		}
		
		private void CreateComboList ()
		{
			real_parent = null;
			DirComboBoxItem selection = null;
			
			if (currentPath == MWFVFS.RecentlyUsedPrefix) {
				mainParentDirComboBoxItem = recentlyUsedDirComboboxItem;
				selection = recentlyUsedDirComboboxItem;
			} else if (currentPath == MWFVFS.DesktopPrefix) {
				selection = desktopDirComboboxItem;
				mainParentDirComboBoxItem = desktopDirComboboxItem;
			} else if (currentPath == MWFVFS.PersonalPrefix) {
				selection = personalDirComboboxItem;
				mainParentDirComboBoxItem = personalDirComboboxItem;
			} else if (currentPath == MWFVFS.MyComputerPrefix) {
				selection = myComputerDirComboboxItem;
				mainParentDirComboBoxItem = myComputerDirComboboxItem;
			} else if (currentPath == MWFVFS.MyNetworkPrefix) {
				selection = networkDirComboboxItem;
				mainParentDirComboBoxItem = networkDirComboboxItem;
			} else {
				foreach (DirComboBoxItem dci in myComputerItems) {
					if (dci.Path == currentPath) {
						mainParentDirComboBoxItem = selection = dci;
						break;
					}
				}
			}
			
			BeginUpdate ();
			
			Items.Clear ();
			
			Items.Add (recentlyUsedDirComboboxItem);
			Items.Add (desktopDirComboboxItem);
			Items.Add (personalDirComboboxItem);
			Items.Add (myComputerDirComboboxItem);
			Items.AddRange (myComputerItems);
			Items.Add (networkDirComboboxItem);
			
			if (selection == null)
				real_parent = CreateFolderStack ();
			
			if (real_parent != null) {
				int local_indent = 0;
				
				if (real_parent == desktopDirComboboxItem)
					local_indent = 1;
				else
				if (real_parent == personalDirComboboxItem || real_parent == networkDirComboboxItem)
					local_indent = 2;
				else
					local_indent = 3;
				
				selection = AppendToParent (local_indent, real_parent);
			}
			
			EndUpdate ();
			
			if (selection != null)
				SelectedItem = selection;
		}
		
		private DirComboBoxItem CreateFolderStack ()
		{
			folderStack.Clear ();
			
			DirectoryInfo di = new DirectoryInfo (currentPath);
			
			folderStack.Push (di);

			bool ignoreCase = !XplatUI.RunningOnUnix;

			while (di.Parent != null) {
				di = di.Parent;

				if (mainParentDirComboBoxItem != personalDirComboboxItem && string.Compare (di.FullName, ThemeEngine.Current.Places (UIIcon.PlacesDesktop), ignoreCase) == 0)
					return desktopDirComboboxItem;
				else
				if (mainParentDirComboBoxItem == personalDirComboboxItem) {
					if (string.Compare (di.FullName, ThemeEngine.Current.Places (UIIcon.PlacesPersonal), ignoreCase) == 0)
						return personalDirComboboxItem;
				} else
					foreach (DirComboBoxItem dci in myComputerItems) {
						if (string.Compare (dci.Path, di.FullName, ignoreCase) == 0) {
							return dci;
						}
					}

				folderStack.Push (di);
			}
			
			return null;
		}
		
		private DirComboBoxItem AppendToParent (int nr_indents, DirComboBoxItem parentDirComboBoxItem)
		{
			DirComboBoxItem selection = null;
			
			int index = Items.IndexOf (parentDirComboBoxItem) + 1;
			
			int xPos = indent * nr_indents;
			
			while (folderStack.Count != 0) {
				DirectoryInfo dii = folderStack.Pop () as DirectoryInfo;
				
				DirComboBoxItem dci = new DirComboBoxItem (imageList, 5, dii.Name, dii.FullName, xPos);
				
				Items.Insert (index, dci);
				index++;
				selection = dci;
				xPos += indent;
			}
			
			return selection;
		}
		
		protected override void OnDrawItem (DrawItemEventArgs e)
		{
			if (e.Index == -1)
				return;
			
			DirComboBoxItem dcbi = Items [e.Index] as DirComboBoxItem;
			
			Bitmap bmp = new Bitmap (e.Bounds.Width, e.Bounds.Height, e.Graphics);
			Graphics gr = Graphics.FromImage (bmp);
			
			Color backColor = e.BackColor;
			Color foreColor = e.ForeColor;
			
			int xPos = dcbi.XPos;

			if ((e.State & DrawItemState.ComboBoxEdit) != 0)
				xPos = 0;

			gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (backColor),
					new Rectangle (0, 0, bmp.Width, bmp.Height));
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected &&
					(!DroppedDown || (e.State & DrawItemState.ComboBoxEdit) != DrawItemState.ComboBoxEdit)) {
				foreColor = ThemeEngine.Current.ColorHighlightText;

				int w = (int) gr.MeasureString (dcbi.Name, e.Font).Width;

				gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHighlight),
						new Rectangle (xPos + 23, 1, w + 3, e.Bounds.Height - 2));
				if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
					ControlPaint.DrawFocusRectangle (gr, new Rectangle (xPos + 22, 0, w + 5,
							e.Bounds.Height), foreColor, ThemeEngine.Current.ColorHighlight);
				}
			}

			gr.DrawString (dcbi.Name, e.Font , ThemeEngine.Current.ResPool.GetSolidBrush (foreColor), new Point (24 + xPos, (bmp.Height - e.Font.Height) / 2));
			gr.DrawImage (dcbi.ImageList.Images [dcbi.ImageIndex], new Rectangle (new Point (xPos + 2, 0), new Size (16, 16)));
			
			e.Graphics.DrawImage (bmp, e.Bounds.X, e.Bounds.Y);
			gr.Dispose ();
			bmp.Dispose ();
		}
		
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			if (Items.Count > 0) {
				DirComboBoxItem dcbi = Items [SelectedIndex] as DirComboBoxItem;
				
				currentPath = dcbi.Path;
			}
		}
		
		protected override void OnSelectionChangeCommitted (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CDirectoryChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		static object CDirectoryChangedEvent = new object ();
		
		public event EventHandler DirectoryChanged {
			add { Events.AddHandler (CDirectoryChangedEvent, value); }
			remove { Events.RemoveHandler (CDirectoryChangedEvent, value); }
		}
	}
	#endregion
	
	#region FilterStruct
	internal struct FilterStruct
	{
		public string filterName;
		public StringCollection filters;
		
		public FilterStruct (string filterName, string filter)
		{
			this.filterName = filterName;
			
			filters =  new StringCollection ();
			
			SplitFilters (filter);
		}
		
		private void SplitFilters (string filter)
		{
			string[] split = filter.Split (new char [] {';'});
			foreach (string s in split) {
				filters.Add (s.Trim ());
			}
		}
	}
	#endregion
	
	#region FileFilter
	internal class FileFilter
	{
		private ArrayList filterArrayList = new ArrayList();
		
		private string filter;
		
		public FileFilter ()
		{}
		
		public FileFilter (string filter) : base ()
		{
			this.filter = filter;
			
			SplitFilter ();
		}
		
		public static bool CheckFilter (string val)
		{
			if (val.Length == 0)
				return true;
			
			string[] filters = val.Split (new char [] {'|'});
			
			if ((filters.Length % 2) != 0)
				return false;
			
			return true;
		}
		
		public ArrayList FilterArrayList {
			set {
				filterArrayList = value;
			}
			
			get {
				return filterArrayList;
			}
		}
		
		public string Filter {
			set {
				filter = value;
				
				SplitFilter ();
			}
			
			get {
				return filter;
			}
		}
		
		private void SplitFilter ()
		{
			filterArrayList.Clear ();
			
			if (filter.Length == 0)
				return;
			
			string[] filters = filter.Split (new char [] {'|'});
			
			for (int i = 0; i < filters.Length; i += 2) {
				FilterStruct filterStruct = new FilterStruct (filters [i], filters [i + 1]);
				
				filterArrayList.Add (filterStruct);
			}
		}
	}
	#endregion
	
	#region MWFFileView

	internal class MWFFileView : ListView
	{
		public delegate void ThumbnailDelegate(FileViewListViewItem fi);
		private ThumbnailCreator thumbCreator;

		private ArrayList filterArrayList;
		
		private bool showHiddenFiles = false;
		
		private string selectedFilesString;
		
		private int filterIndex = 1;
		
		private ToolTip toolTip;
		private int oldItemIndexForToolTip = -1;
		
		private ContextMenuStrip contextMenu;
		
		private ToolStripMenuItem menuItemView;
		private ToolStripMenuItem menuItemNew;
		
		private ToolStripMenuItem smallIconMenutItem;
		private ToolStripMenuItem tilesMenutItem;
		private ToolStripMenuItem largeIconMenutItem;
		private ToolStripMenuItem listMenutItem;
		private ToolStripMenuItem detailsMenutItem;
		
		private ToolStripMenuItem newFolderMenuItem; 
		private ToolStripMenuItem showHiddenFilesMenuItem;
		
		private ToolStripMenuItem previousCheckedMenuItem;
		
		private ArrayList viewMenuItemClones = new ArrayList ();
		
		private FSEntry currentFSEntry = null;
		
		private string currentFolder;
		private string currentRealFolder;
		private FSEntry currentFolderFSEntry;
		
		// store DirectoryInfo for a back button for example
		private Stack directoryStack = new Stack();
		
		// list of controls(components to enable or disable depending on current directoryStack.Count
		private ArrayList dirStackControlsOrComponents = new ArrayList ();
		
		private ToolStripButton folderUpToolBarButton;
		
		private ArrayList registered_senders = new ArrayList ();
		
		private bool should_push = true;
		
		private MWFVFS vfs;
		
		private View old_view;
		
		private ToolStripMenuItem old_menuitem;
		private bool do_update_view = false;

		private ColumnHeader [] columns;
		
		public MWFFileView (MWFVFS vfs)
		{
			this.vfs = vfs;
			this.vfs.RegisterUpdateDelegate (new MWFVFS.UpdateDelegate (RealFileViewUpdate), this);
			
			SuspendLayout ();
			
			contextMenu = new ContextMenuStrip ();
			
			toolTip = new ToolTip ();
			toolTip.InitialDelay = 300;
			toolTip.ReshowDelay = 0; 
			
			// contextMenu
			
			// View menu item
			menuItemView = new ToolStripMenuItem (Locale.GetText("View"));
			
			smallIconMenutItem = new ToolStripMenuItem (Locale.GetText("Small Icon"), null, new EventHandler (OnClickViewMenuSubItem));
			menuItemView.DropDownItems.Add (smallIconMenutItem);
			
			tilesMenutItem = new ToolStripMenuItem (Locale.GetText("Tiles"), null, new EventHandler (OnClickViewMenuSubItem));
			menuItemView.DropDownItems.Add (tilesMenutItem);
			
			largeIconMenutItem = new ToolStripMenuItem (Locale.GetText("Large Icon"), null, new EventHandler (OnClickViewMenuSubItem));
			menuItemView.DropDownItems.Add (largeIconMenutItem);
			
			listMenutItem = new ToolStripMenuItem (Locale.GetText("List"), null, new EventHandler (OnClickViewMenuSubItem));
			listMenutItem.Checked = true;
			menuItemView.DropDownItems.Add (listMenutItem);
			previousCheckedMenuItem = listMenutItem;
			
			detailsMenutItem = new ToolStripMenuItem (Locale.GetText("Details"), null, new EventHandler (OnClickViewMenuSubItem));
			menuItemView.DropDownItems.Add (detailsMenutItem);
			
			contextMenu.Items.Add (menuItemView);
			
			contextMenu.Items.Add (new ToolStripSeparator ());
			
			// New menu item
			menuItemNew = new ToolStripMenuItem (Locale.GetText("New"));
			
			newFolderMenuItem = new ToolStripMenuItem (Locale.GetText("New Folder"), null, new EventHandler (OnClickNewFolderMenuItem));
			menuItemNew.DropDownItems.Add (newFolderMenuItem);
			
			contextMenu.Items.Add (menuItemNew);
			
			contextMenu.Items.Add (new ToolStripSeparator ());
			
			// Show hidden files menu item
			showHiddenFilesMenuItem = new ToolStripMenuItem (Locale.GetText("Show hidden files"), null, new EventHandler (OnClickContextMenu));
			showHiddenFilesMenuItem.Checked = showHiddenFiles;
			contextMenu.Items.Add (showHiddenFilesMenuItem);
			
			LabelWrap = true;
			
			SmallImageList = MimeIconEngine.SmallIcons;
			LargeImageList = MimeIconEngine.LargeIcons;
			
			View = old_view = View.List;
			LabelEdit = true;
			
			ContextMenuStrip = contextMenu;

			// Create columns, but only add them when view changes to Details
			columns = new ColumnHeader [4];
			columns [0] = CreateColumnHeader (Locale.GetText(" Name"), 170, HorizontalAlignment.Left);
			columns [1] = CreateColumnHeader (Locale.GetText("Size "), 80, HorizontalAlignment.Right);
			columns [2] = CreateColumnHeader (Locale.GetText(" Type"), 100, HorizontalAlignment.Left);
			columns [3] = CreateColumnHeader (Locale.GetText(" Last Access"), 150, HorizontalAlignment.Left);

			AllowColumnReorder = true;
			
			ResumeLayout (false);
			
			KeyDown += new KeyEventHandler (MWF_KeyDown);
		}

		ColumnHeader CreateColumnHeader (string text, int width, HorizontalAlignment alignment)
		{
			ColumnHeader col = new ColumnHeader ();
			col.Text = text;
			col.Width = width;
			col.TextAlign = alignment;

			return col;
		}

		public string CurrentFolder {
			get {
				return currentFolder;
			}
			set {
				currentFolder = value;
			}
		}
		
		public string CurrentRealFolder {
			get {
				return currentRealFolder;
			}
		}
		
		public FSEntry CurrentFSEntry {
			get {
				return currentFSEntry;
			}
		}
		
		public ToolStripMenuItem[] ViewMenuItems {
			get {
				/*ToolStripMenuItem[] menuItemClones = new ToolStripMenuItem [] {
					smallIconMenutItem.CloneMenu (),
					tilesMenutItem.CloneMenu (),
					largeIconMenutItem.CloneMenu (),
					listMenutItem.CloneMenu (),
					detailsMenutItem.CloneMenu ()
				};
				
				viewMenuItemClones.Add (menuItemClones);
				
				return menuItemClones;*/
				// FIXME
				return Array.Empty<ToolStripMenuItem>();
			}
		}
		
		public ArrayList FilterArrayList {
			set {
				filterArrayList = value;
			}
			
			get {
				return filterArrayList;
			}
		}
		
		public bool ShowHiddenFiles {
			set {
				showHiddenFiles = value;
			}
			
			get {
				return showHiddenFiles;
			}
		}
		
		public int FilterIndex {
			set {
				filterIndex = value;
				if (Visible)
					UpdateFileView ();
			}
			
			get {
				return filterIndex;
			}
		}
		
		public string SelectedFilesString {
			set {
				selectedFilesString = value;
			}
			
			get {
				return selectedFilesString;
			}
		}
		
		public void PushDir ()
		{
			if (currentFolder != null)
				directoryStack.Push (currentFolder);
			
			EnableOrDisableDirstackObjects ();
		}

		public void PopDir ()
		{
			PopDir (null);
		}
		
		public void PopDir (string filter)
		{
			if (directoryStack.Count == 0)
				return;
			
			string new_folder = directoryStack.Pop () as string;
			
			EnableOrDisableDirstackObjects ();
			
			should_push = false;
			
			ChangeDirectory (null, new_folder, filter);
		}
		
		public void RegisterSender (IUpdateFolder iud)
		{
			registered_senders.Add (iud);
		}
		
		public void CreateNewFolder ()
		{
			if (currentFolder == MWFVFS.MyComputerPrefix ||
			    currentFolder == MWFVFS.RecentlyUsedPrefix)
				return;
			
			FSEntry fsEntry = new FSEntry ();
			fsEntry.Attributes = FileAttributes.Directory;
			fsEntry.FileType = FSEntry.FSEntryType.Directory;
			fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("inode/directory");
			fsEntry.LastAccessTime = DateTime.Now;
			
			// FIXME: when ListView.LabelEdit is available use it
//			listViewItem.BeginEdit();
			
			TextEntryDialog ted = new TextEntryDialog ();
			ted.IconPictureBoxImage = MimeIconEngine.LargeIcons.Images.GetImage (fsEntry.IconIndex);
			
			string folder = String.Empty;
			
			if (currentFolderFSEntry.RealName != null)
				folder = currentFolderFSEntry.RealName;
			else
				folder = currentFolder;
			
			string tmp_filename = Locale.GetText("New Folder");
			
			if (Directory.Exists (Path.Combine (folder, tmp_filename))) {
				int i = 1;
				
				if (XplatUI.RunningOnUnix) {
					tmp_filename = tmp_filename + "-" + i;
				} else {
					tmp_filename = tmp_filename + " (" + i + ")";
				}
				
				while (Directory.Exists (Path.Combine (folder, tmp_filename))) {
					i++;
					if (XplatUI.RunningOnUnix) {
						tmp_filename = Locale.GetText("New Folder") + "-" + i;
					} else {
						tmp_filename = Locale.GetText("New Folder") + " (" + i + ")";
					}
				}
			}
			
			ted.FileName = tmp_filename;
			
			if (ted.ShowDialog () == DialogResult.OK) {
				string new_folder = Path.Combine (folder, ted.FileName);
				
				if (vfs.CreateFolder (new_folder)) {
					fsEntry.FullName = new_folder;
					fsEntry.Name = ted.FileName;
					
					FileViewListViewItem listViewItem = new FileViewListViewItem (fsEntry);
					
					BeginUpdate ();
					Items.Add (listViewItem);
					EndUpdate ();
					
					listViewItem.EnsureVisible ();
				}
			}
		}
		
		public void SetSelectedIndexTo (string fname)
		{
			foreach (FileViewListViewItem item in Items) {
				if (item.Text == fname) {
					BeginUpdate ();
					SelectedItems.Clear ();
					item.Selected = true;
					EndUpdate ();
					break;
				}
			}
		}

		public void OneDirUp ()
		{
			OneDirUp (null);
		}
		
		public void OneDirUp (string filter)
		{
			string parent_folder = vfs.GetParent ();
			if (parent_folder != null)
				ChangeDirectory (null, parent_folder, filter);
		}

		public void ChangeDirectory (object sender, string folder)
		{
			ChangeDirectory (sender, folder, null);
		}
		
		public void ChangeDirectory (object sender, string folder, string filter)
		{
			if (folder == MWFVFS.DesktopPrefix || folder == MWFVFS.RecentlyUsedPrefix)
				folderUpToolBarButton.Enabled = false;
			else
				folderUpToolBarButton.Enabled = true;
			
			foreach (IUpdateFolder iuf in registered_senders) {
				iuf.CurrentFolder = folder;
			}
			
			if (should_push)
				PushDir ();
			else
				should_push = true;
			
			currentFolderFSEntry = vfs.ChangeDirectory (folder);
			
			currentFolder = folder;
			
			if (currentFolder.IndexOf ("://") != -1)
				currentRealFolder = currentFolderFSEntry.RealName;
			else
				currentRealFolder = currentFolder;
			
			BeginUpdate ();
			
			Items.Clear ();
			SelectedItems.Clear ();
			
			if (folder == MWFVFS.RecentlyUsedPrefix) {
				old_view = View;
				View = View.Details;
				old_menuitem = previousCheckedMenuItem;
				UpdateMenuItems (detailsMenutItem);
				do_update_view = true;
			} else
			if (View != old_view && do_update_view) {
				UpdateMenuItems (old_menuitem);
				View = old_view;
				do_update_view = false;
			}
			EndUpdate ();

			try {
				UpdateFileView (filter);
			} catch (Exception e) {
				if (should_push)
					PopDir ();
				MessageBox.Show (e.Message, Locale.GetText("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void UpdateFileView ()
		{
			UpdateFileView (null);
		}

		internal void StopThumbnailCreation()
		{
			if (thumbCreator != null) {
				thumbCreator.Stop();
				thumbCreator = null;
			}
		}

		public void UpdateFileView (string custom_filter)
		{
			StopThumbnailCreation();

			if (custom_filter != null) {
				StringCollection custom_filters = new StringCollection ();
				custom_filters.Add (custom_filter);

				vfs.GetFolderContent (custom_filters);
			} else if (filterArrayList != null && filterArrayList.Count != 0) {
				FilterStruct fs = (FilterStruct)filterArrayList [filterIndex - 1];
				
				vfs.GetFolderContent (fs.filters);
			} else
				vfs.GetFolderContent ();
		}
		
		public void RealFileViewUpdate (ArrayList directoriesArrayList, ArrayList fileArrayList)
		{
			BeginUpdate ();
			
			DeleteOldThumbnails ();	// any existing thumbnail images need to be Dispose()d.
			Items.Clear ();
			SelectedItems.Clear ();
			
			foreach (FSEntry directoryFSEntry in directoriesArrayList) {
				if (!ShowHiddenFiles)
					if (directoryFSEntry.Name.StartsWith (".") || (directoryFSEntry.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
						continue;
				
				FileViewListViewItem listViewItem = new FileViewListViewItem (directoryFSEntry);
				
				Items.Add (listViewItem);
			}
			
			StringCollection collection = new StringCollection ();
			
			foreach (FSEntry fsEntry in fileArrayList) {
				
				// remove duplicates. that can happen when you read recently used files for example
				if (collection.Contains (fsEntry.Name)) {
					
					string fileName = fsEntry.Name;
					
					if (collection.Contains (fileName)) {
						int i = 1;
						
						while (collection.Contains (fileName + "(" + i + ")")) {
							i++;
						}
						
						fileName = fileName + "(" + i + ")";
					}
					
					fsEntry.Name = fileName;
				}
				
				collection.Add (fsEntry.Name);
				
				DoOneFSEntry (fsEntry);
			}
			
			EndUpdate ();
			
			collection.Clear ();
			collection = null;
			
			directoriesArrayList.Clear ();
			fileArrayList.Clear ();

			// Create thumbnail images for Image type files.  This greatly facilitates
			// choosing pictures whose names mean nothing.
			// See https://bugzilla.xamarin.com/show_bug.cgi?id=28025 for details.
			thumbCreator = new ThumbnailCreator(new ThumbnailDelegate(RedrawTheItem), this);
			var makeThumbnails = new Thread(new ThreadStart(thumbCreator.MakeThumbnails));
			makeThumbnails.IsBackground = true;
			makeThumbnails.Start();
		}

		private void RedrawTheItem(FileViewListViewItem fi)
		{
			this.RedrawItems(fi.Index, fi.Index, false);
		}

		public void AddControlToEnableDisableByDirStack (object control)
		{
			dirStackControlsOrComponents.Add (control);
		}
		
		public void SetFolderUpToolBarButton (ToolStripButton tb)
		{
			folderUpToolBarButton = tb;
		}
		
		public void WriteRecentlyUsed (string fullfilename)
		{
			vfs.WriteRecentlyUsedFiles (fullfilename);
		}
		
		private void EnableOrDisableDirstackObjects ()
		{
			foreach (object o in dirStackControlsOrComponents) {
				if (o is Control) {
					Control c = o as Control;
					c.Enabled = (directoryStack.Count > 1);
				} else
				if (o is ToolStripButton) {
					ToolStripButton t = o as ToolStripButton;
					t.Enabled = (directoryStack.Count > 0);
				}
			}
		}
		
		private void DoOneFSEntry (FSEntry fsEntry) 
		{
			if (!ShowHiddenFiles)
				if (fsEntry.Name.StartsWith (".") || (fsEntry.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					return;
			
			FileViewListViewItem listViewItem = new FileViewListViewItem (fsEntry);
			
			Items.Add (listViewItem);
		}
		
		private void MWF_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Back) {
				OneDirUp ();
			} else if (e.Control && e.KeyCode == Keys.A && MultiSelect) {
				foreach (ListViewItem lvi in Items)
					lvi.Selected = true;
			}
		}
		
		#region UIA Framework Members
		internal void PerformClick ()
		{
			OnClick (EventArgs.Empty);
		}

		internal void PerformDoubleClick ()
		{
			OnDoubleClick (EventArgs.Empty);
		}
		#endregion

		protected override void OnClick (EventArgs e)
		{
			if (!MultiSelect) {
				if (SelectedItems.Count > 0) {
					FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
					
					FSEntry fsEntry = listViewItem.FSEntry;
					
					if (fsEntry.FileType == FSEntry.FSEntryType.File) {
						currentFSEntry = fsEntry;
						
						EventHandler eh = (EventHandler)(Events [MSelectedFileChangedEvent]);
						if (eh != null)
							eh (this, EventArgs.Empty);
					}
				}
			}
			
			base.OnClick (e);
		}
		
		protected override void OnDoubleClick (EventArgs e)
		{
			if (SelectedItems.Count > 0) {
				FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
				
				FSEntry fsEntry = listViewItem.FSEntry;

				if ((fsEntry.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
					
					ChangeDirectory (null, fsEntry.FullName);
					
					EventHandler eh = (EventHandler)(Events [MDirectoryChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				} else {
					currentFSEntry = fsEntry;
					
					EventHandler eh = (EventHandler)(Events [MSelectedFileChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
					
					eh = (EventHandler)(Events [MForceDialogEndEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
					
					return;
				}
			}
			
			base.OnDoubleClick (e);
		}
		
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			if (SelectedItems.Count > 0) {
				selectedFilesString = String.Empty;
				
				if (SelectedItems.Count == 1) {
					FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
					
					FSEntry fsEntry = listViewItem.FSEntry;

					if ((fsEntry.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
						selectedFilesString = SelectedItems [0].Text;
				} else {
					foreach (FileViewListViewItem lvi in SelectedItems) {
						FSEntry fsEntry = lvi.FSEntry;

						if ((fsEntry.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
							selectedFilesString = selectedFilesString + "\"" + lvi.Text + "\" ";
					}
				}

				EventHandler eh = (EventHandler)(Events [MSelectedFilesChangedEvent]);
				if (eh != null)
					eh (this, EventArgs.Empty);
			}

			base.OnSelectedIndexChanged (e);
		}
		
		protected override void OnMouseMove (MouseEventArgs e)
		{
			FileViewListViewItem item = GetItemAt (e.X, e.Y) as FileViewListViewItem;
			
			if (item != null) {
				int currentItemIndex = item.Index;
				
				if (currentItemIndex != oldItemIndexForToolTip) {
					oldItemIndexForToolTip = currentItemIndex;
					
					if (toolTip != null && toolTip.Active)
						toolTip.Active = false;
					
					FSEntry fsEntry = item.FSEntry;
					
					string output = String.Empty;
					
					if (fsEntry.FileType == FSEntry.FSEntryType.Directory)
						output = Locale.GetText("Directory: {0}", fsEntry.FullName);
					else if (fsEntry.FileType == FSEntry.FSEntryType.Device)
						output = Locale.GetText("Device: {0}", fsEntry.FullName);
					else if (fsEntry.FileType == FSEntry.FSEntryType.Network)
						output = Locale.GetText("Network: {0}", fsEntry.FullName);
					else
						output = Locale.GetText("File: {0}", fsEntry.FullName);
					
					toolTip.SetToolTip (this, output);	
					
					toolTip.Active = true;
				}
			} else
				toolTip.Active = false;
			
			base.OnMouseMove (e);
		}
		
		void OnClickContextMenu (object sender, EventArgs e)
		{
			ToolStripMenuItem senderMenuItem = sender as ToolStripMenuItem;
			
			if (senderMenuItem == showHiddenFilesMenuItem) {
				senderMenuItem.Checked = !senderMenuItem.Checked;
				showHiddenFiles = senderMenuItem.Checked;
				UpdateFileView ();
			}
		}
		
		void OnClickViewMenuSubItem (object sender, EventArgs e)
		{
			ToolStripMenuItem senderMenuItem = (ToolStripMenuItem)sender;
			
			UpdateMenuItems (senderMenuItem);
			
			// update me - call BeginUpdate/EndUpdate to avoid flicker when columns change
			
			BeginUpdate ();
			switch (senderMenuItem.Parent.Items.IndexOf (senderMenuItem)) {
				case 0:
					View = View.SmallIcon;
					break;
				case 1:
					View = View.Tile;
					break;
				case 2:
					View = View.LargeIcon;
					break;
				case 3:
					View = View.List;
					break;
				case 4:
					View = View.Details;
					break;
				default:
					break;
			}

			if (View == View.Details)
				Columns.AddRange (columns);
			else {
				ListViewItemSorter = null;
				Columns.Clear ();
			}

			EndUpdate ();
		}

		protected override void OnBeforeLabelEdit (LabelEditEventArgs e)
		{
			FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
			FSEntry fsEntry = listViewItem.FSEntry;

			// only allow editing of files or directories
			if (fsEntry.FileType != FSEntry.FSEntryType.Directory &&
				fsEntry.FileType != FSEntry.FSEntryType.File)
				e.CancelEdit = true;

			base.OnBeforeLabelEdit (e);
		}

		protected override void OnAfterLabelEdit (LabelEditEventArgs e)
		{
			base.OnAfterLabelEdit (e);

			// no changes were made
			if (e.Label == null || Items [e.Item].Text == e.Label)
				return;

			FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
			FSEntry fsEntry = listViewItem.FSEntry;

			string folder = (currentFolderFSEntry.RealName != null) ?
				currentFolderFSEntry.RealName : currentFolder;

			switch (fsEntry.FileType) {
			case FSEntry.FSEntryType.Directory:
				string sourceDir = (fsEntry.RealName != null) ? fsEntry.RealName : fsEntry.FullName;
				string destDir = Path.Combine (folder, e.Label);
				if (!vfs.MoveFolder (sourceDir, destDir)) {
					e.CancelEdit = true;
				} else {
					if (fsEntry.RealName != null)
						fsEntry.RealName = destDir;
					else
						fsEntry.FullName = destDir;
				}
				break;
			case FSEntry.FSEntryType.File:
				string sourceFile = (fsEntry.RealName != null) ? fsEntry.RealName : fsEntry.FullName;
				string destFile = Path.Combine (folder, e.Label);
				if (!vfs.MoveFile (sourceFile, destFile)) {
					e.CancelEdit = true;
				} else {
					if (fsEntry.RealName != null)
						fsEntry.RealName = destFile;
					else
						fsEntry.FullName = destFile;
				}
				break;
			}
		}

		private void UpdateMenuItems (ToolStripMenuItem senderMenuItem)
		{
			previousCheckedMenuItem.Checked = false;
			senderMenuItem.Checked = true;
			
			foreach (ToolStripMenuItem[] items in viewMenuItemClones) {
				previousCheckedMenuItem.Checked = false;
				senderMenuItem.Checked = true;
			}
			
			previousCheckedMenuItem = senderMenuItem;
		}
		
		void OnClickNewFolderMenuItem (object sender, EventArgs e)
		{
			CreateNewFolder ();
		}
		
		static object MSelectedFileChangedEvent = new object ();
		static object MSelectedFilesChangedEvent = new object ();
		static object MDirectoryChangedEvent = new object ();
		static object MForceDialogEndEvent = new object ();
		
		public event EventHandler SelectedFileChanged {
			add { Events.AddHandler (MSelectedFileChangedEvent, value); }
			remove { Events.RemoveHandler (MSelectedFileChangedEvent, value); }
		}
		
		public event EventHandler SelectedFilesChanged {
			add { Events.AddHandler (MSelectedFilesChangedEvent, value); }
			remove { Events.RemoveHandler (MSelectedFilesChangedEvent, value); }
		}
		
		public event EventHandler DirectoryChanged {
			add { Events.AddHandler (MDirectoryChangedEvent, value); }
			remove { Events.RemoveHandler (MDirectoryChangedEvent, value); }
		}
		
		public event EventHandler ForceDialogEnd {
			add { Events.AddHandler (MForceDialogEndEvent, value); }
			remove { Events.RemoveHandler (MForceDialogEndEvent, value); }
		}

		internal class ThumbnailCreator
		{
			private ThumbnailDelegate thumbnailDelegate;
			private ListView control;
			private readonly object lockobject = new object();
			private bool stopped = false;

			public ThumbnailCreator(ThumbnailDelegate thumbnailDelegate, ListView listView)
			{
				this.thumbnailDelegate = thumbnailDelegate;
				this.control = listView;
			}

			public void MakeThumbnails()
			{
				foreach (var item in control.Items) {
					var fi = item as FileViewListViewItem;
					if (fi == null || fi.FSEntry == null || !fi.FSEntry.IsImageFile())
						continue;
					fi.FSEntry.SetImage();
					if (stopped)
						return;
					if (thumbnailDelegate != null) {
						lock (lockobject) {
							object[] objectArray = new object[1];
							objectArray[0] = fi;
							control.Invoke(thumbnailDelegate, objectArray);
						}
					}
				}
			}

			public void Stop()
			{
				lock (lockobject) {
					stopped = true;
				}
			}
		}

		private void DeleteOldThumbnails()
		{
			foreach (var item in Items) {
				var fi = item as FileViewListViewItem;
				if (fi == null || fi.FSEntry == null)
					continue;
				fi.FSEntry.Dispose();
				fi.FSEntry = null;
			}
		}

		protected override void Dispose(bool disposing)
		{
			DeleteOldThumbnails();
			base.Dispose(disposing);
		}
	}
	#endregion

	#region FileListViewItem
	internal class FileViewListViewItem : ListViewItem
	{
		private FSEntry fsEntry;
		
		public FileViewListViewItem (FSEntry fsEntry)
		{
			this.fsEntry = fsEntry;
			
			ImageIndex = fsEntry.IconIndex;
			
			Text = fsEntry.Name;
			
			switch (fsEntry.FileType) {
				case FSEntry.FSEntryType.Directory:
					SubItems.Add (String.Empty);
					SubItems.Add ("Directory");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.File:
					long fileLen = 1;
					try {
						if (fsEntry.FileSize > 1024)
							fileLen = fsEntry.FileSize / 1024;
					} catch (Exception) {
						fileLen = 1;
					}
					
					SubItems.Add (fileLen.ToString () + " KB");
					SubItems.Add ("File");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.Device:
					SubItems.Add (String.Empty);
					SubItems.Add ("Device");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.RemovableDevice:
					SubItems.Add (String.Empty);
					SubItems.Add ("RemovableDevice");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				default:
					break;
			}
		}

		public FSEntry FSEntry {
			set {
				fsEntry = value;
			}
			
			get {
				return fsEntry;
			}
		}
	}

	#endregion
	
	#region MwfFileViewItemComparer
	class MwfFileViewItemComparer : IComparer
	{
		int column_index;
		bool asc;

		public MwfFileViewItemComparer (bool asc)
		{
			this.asc = asc;
		}

		public int ColumnIndex {
			get {
				return column_index;
			}
			set {
				column_index = value;
			}
		}

		public bool Ascendent {
			get {
				return asc;
			}
			set {
				asc = value;
			}
		}

		public int Compare (object a, object b)
		{
			ListViewItem item_a = (ListViewItem)a;
			ListViewItem item_b = (ListViewItem)b;

			int retval;
			if (asc)
				retval = String.Compare (item_a.SubItems [column_index].Text, 
						item_b.SubItems [column_index].Text);
			else
				retval = String.Compare (item_b.SubItems [column_index].Text,
						item_a.SubItems [column_index].Text);

			return retval;
		}
	}
	#endregion

	#region IUpdateFolder
	internal interface IUpdateFolder
	{
		string CurrentFolder {get; set;}
	}
	#endregion
	
	#region TextEntryDialog
	// FIXME: When ListView.LabelEdit is implemented remove me
	internal class TextEntryDialog : Form
	{
		private Label label1;
		private Button okButton;
		private TextBox newNameTextBox;
		private PictureBox iconPictureBox;
		private Button cancelButton;
		private GroupBox groupBox1;
		
		public TextEntryDialog ()
		{
			groupBox1 = new GroupBox ();
			cancelButton = new Button ();
			iconPictureBox = new PictureBox ();
			newNameTextBox = new TextBox ();
			okButton = new Button ();
			label1 = new Label ();
			groupBox1.SuspendLayout ();
			SuspendLayout ();
			
			// groupBox1
			groupBox1.Controls.Add (newNameTextBox);
			groupBox1.Controls.Add (label1);
			groupBox1.Controls.Add (iconPictureBox);
			groupBox1.Location = new Point (8, 8);
			groupBox1.Size = new Size (232, 160);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			groupBox1.Text = "New Name";
			
			// cancelButton
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.Location = new Point (168, 176);
			cancelButton.TabIndex = 4;
			cancelButton.Text = "Cancel";
			
			// iconPictureBox
			iconPictureBox.BorderStyle = BorderStyle.Fixed3D;
			iconPictureBox.Location = new Point (86, 24);
			iconPictureBox.Size = new Size (60, 60);
			iconPictureBox.TabIndex = 3;
			iconPictureBox.TabStop = false;
			iconPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
			
			// newNameTextBox
			newNameTextBox.Location = new Point (16, 128);
			newNameTextBox.Size = new Size (200, 20);
			newNameTextBox.TabIndex = 5;
			newNameTextBox.Text = String.Empty;
			
			// okButton
			okButton.DialogResult = DialogResult.OK;
			okButton.Location = new Point (80, 176);
			okButton.TabIndex = 3;
			okButton.Text = "OK";
			
			// label1
			label1.Location = new Point (16, 96);
			label1.Size = new Size (200, 23);
			label1.TabIndex = 4;
			label1.Text = "Enter Name:";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			
			// MainForm
			AcceptButton = okButton;
			AutoScaleBaseSize = new Size (5, 13);
			CancelButton = cancelButton;
			ClientSize = new Size (248, 205);
			Controls.Add (groupBox1);
			Controls.Add (cancelButton);
			Controls.Add (okButton);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Text = "New Folder or File";
			groupBox1.ResumeLayout (false);
			ResumeLayout (false);
			
			newNameTextBox.Select ();
		}
		
		public Image IconPictureBoxImage {
			set {
				iconPictureBox.Image = value;
			}
		}
		
		public string FileName {
			get {
				return newNameTextBox.Text;
			}
			set {
				newNameTextBox.Text = value;
			}
		}
	}
	#endregion
	
	#region MWFVFS	
	internal class MWFVFS
	{
		private FileSystem fileSystem;
		
		public static readonly string DesktopPrefix = "Desktop://";
		public static readonly string PersonalPrefix = "Personal://";
		public static readonly string MyComputerPrefix = "MyComputer://";
		public static readonly string RecentlyUsedPrefix = "RecentlyUsed://";
		public static readonly string MyNetworkPrefix = "MyNetwork://";
		public static readonly string MyComputerPersonalPrefix = "MyComputerPersonal://";
		
		public static Hashtable MyComputerDevicesPrefix = new Hashtable ();
		
		public delegate void UpdateDelegate (ArrayList folders, ArrayList files);
		private UpdateDelegate updateDelegate;
		private Control calling_control;
		
		private ThreadStart get_folder_content_thread_start;
		private Thread worker;
		private WorkerThread workerThread = null;
		private StringCollection the_filters;
		
		public MWFVFS ()
		{
			if (XplatUI.RunningOnUnix) {
				fileSystem = new UnixFileSystem ();
			} else {
				fileSystem = new WinFileSystem ();
			}
		}
		
		public FSEntry ChangeDirectory (string folder)
		{
			return fileSystem.ChangeDirectory (folder);
		}
		
		public void GetFolderContent ()
		{
			GetFolderContent (null);
		}
		
		public void GetFolderContent (StringCollection filters)
		{
			the_filters = filters;

			if (workerThread != null) {
				workerThread.Stop ();
				workerThread = null;
			}

			// Added next line to ensure the control is created before BeginInvoke is called on it
			calling_control.CreateControl();
			workerThread = new WorkerThread (fileSystem, the_filters, updateDelegate, calling_control);
			
			get_folder_content_thread_start = new ThreadStart (workerThread.GetFolderContentThread);
			worker = new Thread (get_folder_content_thread_start);
			worker.IsBackground = true;
			worker.Start();
		}
		
		internal class WorkerThread
		{
			private FileSystem fileSystem;
			private StringCollection the_filters;
			private UpdateDelegate updateDelegate;
			private Control calling_control;
			private readonly object lockobject = new object ();
			private bool stopped = false;
			
			public WorkerThread (FileSystem fileSystem, StringCollection the_filters, UpdateDelegate updateDelegate, Control calling_control)
			{
				this.fileSystem = fileSystem;
				this.the_filters = the_filters;
				this.updateDelegate = updateDelegate;
				this.calling_control = calling_control;
			}
			
			public void GetFolderContentThread()
			{
				ArrayList folders;
				ArrayList files;
				
				fileSystem.GetFolderContent (the_filters, out folders, out files);
				
				if (stopped)
					return;
				
				if (updateDelegate != null) {
					lock (this) {
						object[] objectArray = new object[2];
						
						objectArray[0] = folders;
						objectArray[1] = files;
						
						calling_control.BeginInvoke (updateDelegate, objectArray);
					}
				}
			}
			
			public void Stop ()
			{
				lock (lockobject) {
					stopped = true;
				}
			}
		}
		
		public ArrayList GetFoldersOnly ()
		{
			return fileSystem.GetFoldersOnly ();
		}
		
		public void WriteRecentlyUsedFiles (string filename)
		{
			fileSystem.WriteRecentlyUsedFiles (filename);
		}
		
		public ArrayList GetRecentlyUsedFiles ()
		{
			return fileSystem.GetRecentlyUsedFiles ();
		}
		
		public ArrayList GetMyComputerContent ()
		{
			return fileSystem.GetMyComputerContent ();
		}
		
		public ArrayList GetMyNetworkContent ()
		{
			return fileSystem.GetMyNetworkContent ();
		}
		
		public bool CreateFolder (string new_folder)
		{
			try {
				if (Directory.Exists (new_folder)) {
					string message = "Folder \"" + new_folder + "\" already exists.";
					MessageBox.Show (message, new_folder, MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return false;
				} else
					Directory.CreateDirectory (new_folder);
			} catch (Exception e) {
				MessageBox.Show (e.Message, new_folder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			return true;
		}

		public bool MoveFolder (string sourceDirName, string destDirName)
		{
			try {
				if (Directory.Exists (destDirName)) {
					string message = "Cannot rename " + Path.GetFileName (sourceDirName)
						+ ": A folder with the name you specified already exists."
						+ " Specify a different folder name.";
					MessageBox.Show (message, "Error Renaming Folder", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return false;
				} else
					Directory.Move (sourceDirName, destDirName);
			} catch (Exception e) {
				MessageBox.Show (e.Message, "Error Renaming Folder", 
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public bool MoveFile (string sourceFileName, string destFileName)
		{
			try {
				if (File.Exists (destFileName)) {
					string message = "Cannot rename " + Path.GetFileName (sourceFileName)
						+ ": A file with the name you specified already exists."
						+ " Specify a different file name.";
					MessageBox.Show (message, "Error Renaming File",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				} else
					File.Move (sourceFileName, destFileName);
			} catch (Exception e) {
				MessageBox.Show (e.Message, "Error Renaming File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public string GetParent ()
		{
			return fileSystem.GetParent ();
		}
		
		public void RegisterUpdateDelegate(UpdateDelegate updateDelegate, Control control)
		{
			this.updateDelegate = updateDelegate;
			calling_control = control;
		}
	}
	#endregion
	
	#region FileSystem
	internal abstract class FileSystem
	{
		protected string currentTopFolder = String.Empty;
		protected FSEntry currentFolderFSEntry = null;
		protected FSEntry currentTopFolderFSEntry = null;
		private FileInfoComparer fileInfoComparer = new FileInfoComparer ();
		private FSEntryComparer fsEntryComparer = new FSEntryComparer ();
		
		public FSEntry ChangeDirectory (string folder)
		{
			if (folder == MWFVFS.DesktopPrefix) {
				currentTopFolder = MWFVFS.DesktopPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetDesktopFSEntry ();
			} else
			if (folder == MWFVFS.PersonalPrefix) {
				currentTopFolder = MWFVFS.PersonalPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetPersonalFSEntry ();
			} else
			if (folder == MWFVFS.MyComputerPersonalPrefix) {
				currentTopFolder = MWFVFS.MyComputerPersonalPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetMyComputerPersonalFSEntry ();
			} else
			if (folder == MWFVFS.RecentlyUsedPrefix) {
				currentTopFolder = MWFVFS.RecentlyUsedPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetRecentlyUsedFSEntry ();
			} else
			if (folder == MWFVFS.MyComputerPrefix) {
				currentTopFolder = MWFVFS.MyComputerPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetMyComputerFSEntry ();
			} else
			if (folder == MWFVFS.MyNetworkPrefix) {
				currentTopFolder = MWFVFS.MyNetworkPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetMyNetworkFSEntry ();
			} else {
				bool found = false;
				
				foreach (DictionaryEntry entry in MWFVFS.MyComputerDevicesPrefix) {
					FSEntry fsEntry = entry.Value as FSEntry;
					if (folder == fsEntry.FullName) {
						currentTopFolder = entry.Key as string;
						currentTopFolderFSEntry = currentFolderFSEntry = fsEntry;
						found = true;
						break;
					}
				}
				
				if (!found) {
					currentFolderFSEntry = GetDirectoryFSEntry (new DirectoryInfo (folder), currentTopFolderFSEntry);
				}
			}
			
			return currentFolderFSEntry;
		}
		
		public string GetParent ()
		{
			return currentFolderFSEntry.Parent;
		}
		
		// directories_out and files_out contain FSEntry objects
		public void GetFolderContent (StringCollection filters, out ArrayList directories_out, out ArrayList files_out)
		{
			directories_out = new ArrayList ();
			files_out = new ArrayList ();
			
			if (currentFolderFSEntry.FullName == MWFVFS.DesktopPrefix) {
				FSEntry personalFSEntry = GetPersonalFSEntry ();
				
				directories_out.Add (personalFSEntry);
				
				FSEntry myComputerFSEntry = GetMyComputerFSEntry ();
				
				directories_out.Add (myComputerFSEntry);
				
				FSEntry myNetworkFSEntry = GetMyNetworkFSEntry ();
				
				directories_out.Add (myNetworkFSEntry);
				
				ArrayList d_out = null;
				ArrayList f_out = null;
				GetNormalFolderContent (ThemeEngine.Current.Places (UIIcon.PlacesDesktop), filters, out d_out, out f_out);
				directories_out.AddRange (d_out);
				files_out.AddRange (f_out);
				
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.RecentlyUsedPrefix) {
				files_out = GetRecentlyUsedFiles ();
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyComputerPrefix) {
				directories_out.AddRange (GetMyComputerContent ());
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.PersonalPrefix || currentFolderFSEntry.FullName == MWFVFS.MyComputerPersonalPrefix) {
				ArrayList d_out = null;
				ArrayList f_out = null;
				GetNormalFolderContent (ThemeEngine.Current.Places (UIIcon.PlacesPersonal), filters, out d_out, out f_out);
				directories_out.AddRange (d_out);
				files_out.AddRange (f_out);
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyNetworkPrefix) {
				directories_out.AddRange (GetMyNetworkContent ());
			} else {
				GetNormalFolderContent (currentFolderFSEntry.FullName, filters, out directories_out, out files_out);
			}
		}
		
		public ArrayList GetFoldersOnly ()
		{
			ArrayList directories_out = new ArrayList ();
			
			if (currentFolderFSEntry.FullName == MWFVFS.DesktopPrefix) {
				FSEntry personalFSEntry = GetPersonalFSEntry ();
				
				directories_out.Add (personalFSEntry);
				
				FSEntry myComputerFSEntry = GetMyComputerFSEntry ();
				
				directories_out.Add (myComputerFSEntry);
				
				FSEntry myNetworkFSEntry = GetMyNetworkFSEntry ();
				
				directories_out.Add (myNetworkFSEntry);
				
				ArrayList d_out = GetNormalFolders (ThemeEngine.Current.Places (UIIcon.PlacesDesktop));
				directories_out.AddRange (d_out);
				
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.RecentlyUsedPrefix) {
				//files_out = GetRecentlyUsedFiles ();
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyComputerPrefix) {
				directories_out.AddRange (GetMyComputerContent ());
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.PersonalPrefix || currentFolderFSEntry.FullName == MWFVFS.MyComputerPersonalPrefix) {
				ArrayList d_out = GetNormalFolders (ThemeEngine.Current.Places (UIIcon.PlacesPersonal));
				directories_out.AddRange (d_out);
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyNetworkPrefix) {
				directories_out.AddRange (GetMyNetworkContent ());
			} else {
				directories_out = GetNormalFolders (currentFolderFSEntry.FullName);
			}
			return directories_out;
		}
		
		protected void GetNormalFolderContent (string from_folder, StringCollection filters, out ArrayList directories_out, out ArrayList files_out)
		{
			DirectoryInfo dirinfo = new DirectoryInfo (from_folder);
			
			directories_out = new ArrayList ();
			
			DirectoryInfo[] dirs = null;

			try {
				dirs = dirinfo.GetDirectories ();
			} catch (Exception) {}

			if (dirs != null)
				for (int i = 0; i < dirs.Length; i++) {
					directories_out.Add (GetDirectoryFSEntry (dirs [i], currentTopFolderFSEntry));
				}

			directories_out.Sort (fsEntryComparer);
			
			files_out = new ArrayList ();
			
			ArrayList files = new ArrayList ();

			try {
				if (filters == null) {
					files.AddRange (dirinfo.GetFiles ());
				} else {
					foreach (string s in filters)
						files.AddRange (dirinfo.GetFiles (s));
					
					files.Sort (fileInfoComparer);
				}
			} catch (Exception) {}

			for (int i = 0; i < files.Count; i++) {
				FSEntry fs = GetFileFSEntry (files [i] as FileInfo);
				if (fs != null)
					files_out.Add (fs);
			}
		}

		protected ArrayList GetNormalFolders (string from_folder)
		{
			DirectoryInfo dirinfo = new DirectoryInfo (from_folder);
			
			ArrayList directories_out = new ArrayList ();
			
			DirectoryInfo[] dirs = null;
			
			try {
				dirs = dirinfo.GetDirectories ();
			} catch (Exception) {}
			
			if (dirs != null)
				for (int i = 0; i < dirs.Length; i++) {
					directories_out.Add (GetDirectoryFSEntry (dirs [i], currentTopFolderFSEntry));
				}
			
			return directories_out;
		}
		
		protected virtual FSEntry GetDirectoryFSEntry (DirectoryInfo dirinfo, FSEntry topFolderFSEntry)
		{
			FSEntry fs = new FSEntry ();
			
			fs.Attributes = dirinfo.Attributes;
			fs.FullName = dirinfo.FullName;
			fs.Name = dirinfo.Name;
			fs.MainTopNode = topFolderFSEntry;
			fs.FileType = FSEntry.FSEntryType.Directory;
			fs.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("inode/directory");
			fs.LastAccessTime = dirinfo.LastAccessTime;
			
			return fs;
		}
		
		protected virtual FSEntry GetFileFSEntry (FileInfo fileinfo)
		{
			// *sigh* FileInfo gives us no usable information for links to directories
			// so, return null
			if ((fileinfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
				return null;
			
			FSEntry fs = new FSEntry ();
			
			fs.Attributes = fileinfo.Attributes;
			fs.FullName = fileinfo.FullName;
			fs.Name = fileinfo.Name;
			fs.FileType = FSEntry.FSEntryType.File;
			fs.IconIndex = MimeIconEngine.GetIconIndexForFile (fileinfo.FullName);
			fs.FileSize = fileinfo.Length;
			fs.LastAccessTime = fileinfo.LastAccessTime;
			
			return fs;
		}
		
		internal class FileInfoComparer : IComparer
		{
			public int Compare (object fileInfo1, object fileInfo2)
			{
				return String.Compare (((FileInfo)fileInfo1).Name, ((FileInfo)fileInfo2).Name);
			}
		}

		internal class FSEntryComparer : IComparer
		{
			public int Compare (object fileInfo1, object fileInfo2)
			{
				return String.Compare (((FSEntry)fileInfo1).Name, ((FSEntry)fileInfo2).Name);
			}
		}
	
		protected abstract FSEntry GetDesktopFSEntry ();
		
		protected abstract FSEntry GetRecentlyUsedFSEntry ();
		
		protected abstract FSEntry GetPersonalFSEntry ();
		
		protected abstract FSEntry GetMyComputerPersonalFSEntry ();
		
		protected abstract FSEntry GetMyComputerFSEntry ();
		
		protected abstract FSEntry GetMyNetworkFSEntry ();
		
		public abstract void WriteRecentlyUsedFiles (string fileToAdd);
		
		public abstract ArrayList GetRecentlyUsedFiles ();
		
		public abstract ArrayList GetMyComputerContent ();
		
		public abstract ArrayList GetMyNetworkContent ();
	}
	#endregion
	
	#region UnixFileSystem
	internal class UnixFileSystem : FileSystem
	{
		private MasterMount masterMount = new MasterMount ();
		private FSEntry desktopFSEntry = null;
		private FSEntry recentlyusedFSEntry = null;
		private FSEntry personalFSEntry = null;
		private FSEntry mycomputerpersonalFSEntry = null;
		private FSEntry mycomputerFSEntry = null;
		private FSEntry mynetworkFSEntry = null;
		
		private string personal_folder;
		private string recently_used_path;
		private string full_kde_recent_document_dir;
		
		public UnixFileSystem ()
		{
			personal_folder = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			recently_used_path = Path.Combine (personal_folder, ".recently-used");
			
			full_kde_recent_document_dir = personal_folder + "/.kde/share/apps/RecentDocuments";
			
			desktopFSEntry = new FSEntry ();
			
			desktopFSEntry.Attributes = FileAttributes.Directory;
			desktopFSEntry.FullName = MWFVFS.DesktopPrefix;
			desktopFSEntry.Name = "Desktop";
			desktopFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
			desktopFSEntry.FileType = FSEntry.FSEntryType.Directory;
			desktopFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("desktop/desktop");
			desktopFSEntry.LastAccessTime = DateTime.Now;
			
			recentlyusedFSEntry = new FSEntry ();
			
			recentlyusedFSEntry.Attributes = FileAttributes.Directory;
			recentlyusedFSEntry.FullName = MWFVFS.RecentlyUsedPrefix;
			recentlyusedFSEntry.Name = "Recently Used";
			recentlyusedFSEntry.FileType = FSEntry.FSEntryType.Directory;
			recentlyusedFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("recently/recently");
			recentlyusedFSEntry.LastAccessTime = DateTime.Now;
			
			personalFSEntry = new FSEntry ();
			
			personalFSEntry.Attributes = FileAttributes.Directory;
			personalFSEntry.FullName = MWFVFS.PersonalPrefix;
			personalFSEntry.Name = "Personal";
			personalFSEntry.MainTopNode = GetDesktopFSEntry ();
			personalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			personalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			personalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			personalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerpersonalFSEntry = new FSEntry ();
			
			mycomputerpersonalFSEntry.Attributes = FileAttributes.Directory;
			mycomputerpersonalFSEntry.FullName = MWFVFS.MyComputerPersonalPrefix;
			mycomputerpersonalFSEntry.Name = "Personal";
			mycomputerpersonalFSEntry.MainTopNode = GetMyComputerFSEntry ();
			mycomputerpersonalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			mycomputerpersonalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerpersonalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			mycomputerpersonalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerFSEntry = new FSEntry ();
			
			mycomputerFSEntry.Attributes = FileAttributes.Directory;
			mycomputerFSEntry.FullName = MWFVFS.MyComputerPrefix;
			mycomputerFSEntry.Name = "My Computer";
			mycomputerFSEntry.MainTopNode = GetDesktopFSEntry ();
			mycomputerFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("workplace/workplace");
			mycomputerFSEntry.LastAccessTime = DateTime.Now;
			
			mynetworkFSEntry = new FSEntry ();
			
			mynetworkFSEntry.Attributes = FileAttributes.Directory;
			mynetworkFSEntry.FullName = MWFVFS.MyNetworkPrefix;
			mynetworkFSEntry.Name = "My Network";
			mynetworkFSEntry.MainTopNode = GetDesktopFSEntry ();
			mynetworkFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mynetworkFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
			mynetworkFSEntry.LastAccessTime = DateTime.Now;
		}
		
		public override void WriteRecentlyUsedFiles (string fileToAdd)
		{
			if (File.Exists (recently_used_path) && new FileInfo (recently_used_path).Length > 0) {
				XmlDocument xml_doc = new XmlDocument ();
				xml_doc.Load (recently_used_path);
				
				XmlNode grand_parent_node = xml_doc.SelectSingleNode ("RecentFiles");
				
				if (grand_parent_node != null) {
					// create a new element
					XmlElement new_recent_item_node = xml_doc.CreateElement ("RecentItem");
					
					XmlElement new_child = xml_doc.CreateElement ("URI");
					UriBuilder ub = new UriBuilder ();
					ub.Path = fileToAdd;
					ub.Host = null;
					ub.Scheme = "file";
					XmlText new_text_child = xml_doc.CreateTextNode (ub.ToString ());
					new_child.AppendChild (new_text_child);
					
					new_recent_item_node.AppendChild (new_child);
					
					new_child = xml_doc.CreateElement ("Mime-Type");
					new_text_child = xml_doc.CreateTextNode (Mime.GetMimeTypeForFile (fileToAdd));
					new_child.AppendChild (new_text_child);
					
					new_recent_item_node.AppendChild (new_child);
					
					new_child = xml_doc.CreateElement ("Timestamp");
					long seconds = (long)(DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalSeconds;
					new_text_child = xml_doc.CreateTextNode (seconds.ToString ());
					new_child.AppendChild (new_text_child);
					
					new_recent_item_node.AppendChild (new_child);
					
					new_child = xml_doc.CreateElement ("Groups");
					
					new_recent_item_node.AppendChild (new_child);
					
					// now search the nodes in grand_parent_node for another instance of the new uri and if found remove it
					// so that the new node is the first one
					foreach (XmlNode n in grand_parent_node.ChildNodes) {
						XmlNode uri_node = n.SelectSingleNode ("URI");
						if (uri_node != null) {
							XmlNode uri_node_child = uri_node.FirstChild;
							if (uri_node_child is XmlText)
								if (ub.ToString () == ((XmlText)uri_node_child).Data) {
									grand_parent_node.RemoveChild (n);
									break;
								}
						}
					}
					
					// prepend the new recent item to the grand parent node list
					grand_parent_node.PrependChild (new_recent_item_node);
					
					// limit the # of RecentItems to 10
					if (grand_parent_node.ChildNodes.Count > 10) {
						while (grand_parent_node.ChildNodes.Count > 10)
							grand_parent_node.RemoveChild (grand_parent_node.LastChild);
					}
					
					try {
						xml_doc.Save (recently_used_path);
					} catch (Exception) {
					}
				}
			} else {
				XmlDocument xml_doc = new XmlDocument ();
				xml_doc.AppendChild (xml_doc.CreateXmlDeclaration ("1.0", String.Empty, String.Empty));
				
				XmlElement recentFiles_element = xml_doc.CreateElement ("RecentFiles");
				
				XmlElement new_recent_item_node = xml_doc.CreateElement ("RecentItem");
				
				XmlElement new_child = xml_doc.CreateElement ("URI");
				UriBuilder ub = new UriBuilder ();
				ub.Path = fileToAdd;
				ub.Host = null;
				ub.Scheme = "file";
				XmlText new_text_child = xml_doc.CreateTextNode (ub.ToString ());
				new_child.AppendChild (new_text_child);
				
				new_recent_item_node.AppendChild (new_child);
				
				new_child = xml_doc.CreateElement ("Mime-Type");
				new_text_child = xml_doc.CreateTextNode (Mime.GetMimeTypeForFile (fileToAdd));
				new_child.AppendChild (new_text_child);
				
				new_recent_item_node.AppendChild (new_child);
				
				new_child = xml_doc.CreateElement ("Timestamp");
				long seconds = (long)(DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalSeconds;
				new_text_child = xml_doc.CreateTextNode (seconds.ToString ());
				new_child.AppendChild (new_text_child);
				
				new_recent_item_node.AppendChild (new_child);
				
				new_child = xml_doc.CreateElement ("Groups");
				
				new_recent_item_node.AppendChild (new_child);
				
				recentFiles_element.AppendChild (new_recent_item_node);
				
				xml_doc.AppendChild (recentFiles_element);
				
				try {
					xml_doc.Save (recently_used_path);
				} catch (Exception) {
				}
			}
		}
		
		// return an ArrayList with FSEntry objects
		public override ArrayList GetRecentlyUsedFiles ()
		{
			// check for GNOME and KDE
			
			ArrayList files_al = new ArrayList ();
			
			// GNOME
			if (File.Exists (recently_used_path)) {
				try {
					XmlTextReader xtr = new XmlTextReader (recently_used_path);
					while (xtr.Read ()) {
						if (xtr.NodeType == XmlNodeType.Element && xtr.Name.ToUpper () == "URI") {
							xtr.Read ();
							Uri uri = new Uri (xtr.Value);
							if (!files_al.Contains (uri.LocalPath))
								if (File.Exists (uri.LocalPath)) {
									FSEntry fs = GetFileFSEntry (new FileInfo (uri.LocalPath));
									if (fs != null)
										files_al.Add (fs);
								}
						}
					}
					xtr.Close ();
				} catch (Exception) {
					
				}
			}
			
			// KDE
			if (Directory.Exists (full_kde_recent_document_dir)) {
				string[] files = Directory.GetFiles (full_kde_recent_document_dir, "*.desktop");
				
				foreach (string file_name in files) {
					StreamReader sr = new StreamReader (file_name);
					
					string line = sr.ReadLine ();
					
					while (line != null) {
						line = line.Trim ();
						
						if (line.StartsWith ("URL=")) {
							line = line.Replace ("URL=", String.Empty);
							line = line.Replace ("$HOME", personal_folder);
							
							Uri uri = new Uri (line);
							if (!files_al.Contains (uri.LocalPath))
								if (File.Exists (uri.LocalPath)) {
									FSEntry fs = GetFileFSEntry (new FileInfo (uri.LocalPath));
									if (fs != null)
										files_al.Add (fs);
								}
							break;
						}
						
						line = sr.ReadLine ();
					}
					
					sr.Close ();
				}
			}
			
			
			return files_al;
		}
		
		// return an ArrayList with FSEntry objects
		public override ArrayList GetMyComputerContent ()
		{
			ArrayList my_computer_content_arraylist = new ArrayList ();
			
			if (masterMount.ProcMountAvailable) {
				masterMount.GetMounts ();
				
				foreach (MasterMount.Mount mount in masterMount.Block_devices) {
					FSEntry fsEntry = new FSEntry ();
					fsEntry.FileType = FSEntry.FSEntryType.Device;
					
					fsEntry.FullName = mount.mount_point;
					
					fsEntry.Name = "HDD (" +  mount.fsType + ", " + mount.device_short + ")";
					
					fsEntry.FsType = mount.fsType;
					fsEntry.DeviceShort = mount.device_short;
					
					fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("harddisk/harddisk");
					
					fsEntry.Attributes = FileAttributes.Directory;
					
					fsEntry.MainTopNode = GetMyComputerFSEntry ();
					
					my_computer_content_arraylist.Add (fsEntry);
					
					if (!MWFVFS.MyComputerDevicesPrefix.Contains (fsEntry.FullName + "://"))
						MWFVFS.MyComputerDevicesPrefix.Add (fsEntry.FullName + "://", fsEntry);
				}
				
				foreach (MasterMount.Mount mount in masterMount.Removable_devices) {
					FSEntry fsEntry = new FSEntry ();
					fsEntry.FileType = FSEntry.FSEntryType.RemovableDevice;
					
					fsEntry.FullName = mount.mount_point;
					
					bool is_dvd_cdrom = mount.fsType == MasterMount.FsTypes.usbfs ? false : true;
					string type_name = is_dvd_cdrom ? "DVD/CD-Rom" : "USB";
					string mime_type = is_dvd_cdrom ? "cdrom/cdrom" : "removable/removable";
					
					fsEntry.Name = type_name +" (" + mount.device_short + ")";
					
					fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType (mime_type);
					
					fsEntry.FsType = mount.fsType;
					fsEntry.DeviceShort = mount.device_short;
					
					fsEntry.Attributes = FileAttributes.Directory;
					
					fsEntry.MainTopNode = GetMyComputerFSEntry ();
					
					my_computer_content_arraylist.Add (fsEntry);
					
					string contain_string = fsEntry.FullName + "://";
					if (!MWFVFS.MyComputerDevicesPrefix.Contains (contain_string))
						MWFVFS.MyComputerDevicesPrefix.Add (contain_string, fsEntry);
				}
			}
			
			my_computer_content_arraylist.Add (GetMyComputerPersonalFSEntry ());
			
			return my_computer_content_arraylist;
		}
		
		public override ArrayList GetMyNetworkContent ()
		{
			ArrayList fsEntries = new ArrayList ();
			
			foreach (MasterMount.Mount mount in masterMount.Network_devices) {
				FSEntry fsEntry = new FSEntry ();
				fsEntry.FileType = FSEntry.FSEntryType.Network;
				
				fsEntry.FullName = mount.mount_point;
				
				fsEntry.FsType = mount.fsType;
				fsEntry.DeviceShort = mount.device_short;
				
				fsEntry.Name = "Network (" + mount.fsType + ", " + mount.device_short + ")";
				
				switch (mount.fsType) {
					case MasterMount.FsTypes.nfs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("nfs/nfs");
						break;
					case MasterMount.FsTypes.smbfs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("smb/smb");
						break;
					case MasterMount.FsTypes.ncpfs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
						break;
					case MasterMount.FsTypes.cifs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
						break;
					default:
						break;
				}
				
				fsEntry.Attributes = FileAttributes.Directory;
				
				fsEntry.MainTopNode = GetMyNetworkFSEntry ();
				
				fsEntries.Add (fsEntry);
			}
			return fsEntries;
		}
		
		protected override FSEntry GetDesktopFSEntry ()
		{
			return desktopFSEntry;
		}
		
		protected override FSEntry GetRecentlyUsedFSEntry ()
		{
			return recentlyusedFSEntry;
		}
		
		protected override FSEntry GetPersonalFSEntry ()
		{
			return personalFSEntry;
		}
		
		protected override FSEntry GetMyComputerPersonalFSEntry ()
		{
			return mycomputerpersonalFSEntry;
		}
		
		protected override FSEntry GetMyComputerFSEntry ()
		{
			return mycomputerFSEntry;
		}
		
		protected override FSEntry GetMyNetworkFSEntry ()
		{
			return mynetworkFSEntry;
		}
	}
	#endregion
	
	#region WinFileSystem
	internal class WinFileSystem : FileSystem
	{
		private FSEntry desktopFSEntry = null;
		private FSEntry recentlyusedFSEntry = null;
		private FSEntry personalFSEntry = null;
		private FSEntry mycomputerpersonalFSEntry = null;
		private FSEntry mycomputerFSEntry = null;
		private FSEntry mynetworkFSEntry = null;
		
		public WinFileSystem ()
		{
			desktopFSEntry = new FSEntry ();
			
			desktopFSEntry.Attributes = FileAttributes.Directory;
			desktopFSEntry.FullName = MWFVFS.DesktopPrefix;
			desktopFSEntry.Name = "Desktop";
			desktopFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
			desktopFSEntry.FileType = FSEntry.FSEntryType.Directory;
			desktopFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("desktop/desktop");
			desktopFSEntry.LastAccessTime = DateTime.Now;
			
			recentlyusedFSEntry = new FSEntry ();
			
			recentlyusedFSEntry.Attributes = FileAttributes.Directory;
			recentlyusedFSEntry.FullName = MWFVFS.RecentlyUsedPrefix;
			recentlyusedFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesRecentDocuments);
			recentlyusedFSEntry.Name = "Recently Used";
			recentlyusedFSEntry.FileType = FSEntry.FSEntryType.Directory;
			recentlyusedFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("recently/recently");
			recentlyusedFSEntry.LastAccessTime = DateTime.Now;
			
			personalFSEntry = new FSEntry ();
			
			personalFSEntry.Attributes = FileAttributes.Directory;
			personalFSEntry.FullName = MWFVFS.PersonalPrefix;
			personalFSEntry.Name = "Personal";
			personalFSEntry.MainTopNode = GetDesktopFSEntry ();
			personalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			personalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			personalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			personalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerpersonalFSEntry = new FSEntry ();
			
			mycomputerpersonalFSEntry.Attributes = FileAttributes.Directory;
			mycomputerpersonalFSEntry.FullName = MWFVFS.MyComputerPersonalPrefix;
			mycomputerpersonalFSEntry.Name = "Personal";
			mycomputerpersonalFSEntry.MainTopNode = GetMyComputerFSEntry ();
			mycomputerpersonalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			mycomputerpersonalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerpersonalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			mycomputerpersonalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerFSEntry = new FSEntry ();
			
			mycomputerFSEntry.Attributes = FileAttributes.Directory;
			mycomputerFSEntry.FullName = MWFVFS.MyComputerPrefix;
			mycomputerFSEntry.Name = "My Computer";
			mycomputerFSEntry.MainTopNode = GetDesktopFSEntry ();
			mycomputerFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("workplace/workplace");
			mycomputerFSEntry.LastAccessTime = DateTime.Now;
			
			mynetworkFSEntry = new FSEntry ();
			
			mynetworkFSEntry.Attributes = FileAttributes.Directory;
			mynetworkFSEntry.FullName = MWFVFS.MyNetworkPrefix;
			mynetworkFSEntry.Name = "My Network";
			mynetworkFSEntry.MainTopNode = GetDesktopFSEntry ();
			mynetworkFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mynetworkFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
			mynetworkFSEntry.LastAccessTime = DateTime.Now;
		}
		
		public override void WriteRecentlyUsedFiles (string fileToAdd)
		{
			// TODO: Implement this method
			// use SHAddToRecentDocs ?
		}
		
		public override ArrayList GetRecentlyUsedFiles ()
		{
			ArrayList al = new ArrayList ();
			
			DirectoryInfo di = new DirectoryInfo (recentlyusedFSEntry.RealName);
			
			FileInfo[] fileinfos = di.GetFiles ();
			
			foreach (FileInfo fi in fileinfos) {
				FSEntry fs = GetFileFSEntry (fi);
				if (fs != null)
					al.Add (fs);
			}
			
			return al;
		}
		
		public override ArrayList GetMyComputerContent ()
		{
			string[] logical_drives = Directory.GetLogicalDrives ();
			
			ArrayList my_computer_content_arraylist = new ArrayList ();
			
			foreach (string drive in logical_drives) {
				FSEntry fsEntry = new FSEntry ();
				fsEntry.FileType = FSEntry.FSEntryType.Device;
				
				fsEntry.FullName = drive;
				
				fsEntry.Name = drive;
				
				fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("harddisk/harddisk");
				
				fsEntry.Attributes = FileAttributes.Directory;
				
				fsEntry.MainTopNode = GetMyComputerFSEntry ();
				
				my_computer_content_arraylist.Add (fsEntry);
				
				string contain_string = fsEntry.FullName + "://";
				if (!MWFVFS.MyComputerDevicesPrefix.Contains (contain_string))
					MWFVFS.MyComputerDevicesPrefix.Add (contain_string, fsEntry);
			}
			
			my_computer_content_arraylist.Add (GetMyComputerPersonalFSEntry ());
			
			return my_computer_content_arraylist;
		}
		
		public override ArrayList GetMyNetworkContent ()
		{
			// TODO: Implement this method
			return new ArrayList ();
		}
		protected override FSEntry GetDesktopFSEntry ()
		{
			return desktopFSEntry;
		}
		
		protected override FSEntry GetRecentlyUsedFSEntry ()
		{
			return recentlyusedFSEntry;
		}
		
		protected override FSEntry GetPersonalFSEntry ()
		{
			return personalFSEntry;
		}
		
		protected override FSEntry GetMyComputerPersonalFSEntry ()
		{
			return mycomputerpersonalFSEntry;
		}
		
		protected override FSEntry GetMyComputerFSEntry ()
		{
			return mycomputerFSEntry;
		}
		
		protected override FSEntry GetMyNetworkFSEntry ()
		{
			return mynetworkFSEntry;
		}
	}
	#endregion
	
	#region FSEntry
	internal class FSEntry : IDisposable
	{
		public enum FSEntryType
		{
			Desktop,
			RecentlyUsed,
			MyComputer,
			File,
			Directory,
			Device,
			RemovableDevice,
			Network
		}
		
		private MasterMount.FsTypes fsType;
		private string device_short;
		private string fullName;
		private string name;
		private string realName = null;
		private FileAttributes attributes = FileAttributes.Normal;
		private long fileSize;
		private FSEntryType fileType;
		private DateTime lastAccessTime;
		private FSEntry mainTopNode = null;
		
		private int iconIndex;

		private string parent;
		
		public MasterMount.FsTypes FsType {
			set {
				fsType = value;
			}
			
			get {
				return fsType;
			}
		}
		
		public string DeviceShort {
			set {
				device_short = value;
			}
			
			get {
				return device_short;
			}
		}
		
		public string FullName {
			set {
				fullName = value;
			}
			
			get {
				return fullName;
			}
		}
		
		public string Name {
			set {
				name = value;
			}
			
			get {
				return name;
			}
		}
		
		public string RealName {
			set {
				realName = value;
			}
			
			get {
				return realName;
			}
		}
		
		public FileAttributes Attributes {
			set {
				attributes = value;
			}
			
			get {
				return attributes;
			}
		}
		
		public long FileSize {
			set {
				fileSize = value;
			}
			
			get {
				return fileSize;
			}
		}
		
		public FSEntryType FileType {
			set {
				fileType = value;
			}
			
			get {
				return fileType;
			}
		}
		
		public DateTime LastAccessTime {
			set {
				lastAccessTime = value;
			}
			
			get {
				return lastAccessTime;
			}
		}
		
		public int IconIndex {
			set {
				iconIndex = value;
			}
			
			get {
				return iconIndex;
			}
		}
		
		public FSEntry MainTopNode {
			set {
				mainTopNode = value;
			}
			
			get {
				return mainTopNode;
			}
		}
		
		public string Parent {
			set {
				parent = value;
			}
			
			get {
				parent = GetParent ();
				
				return parent;
			}
		}
		
		private string GetParent ()
		{
			if (fullName == MWFVFS.PersonalPrefix) {
				return MWFVFS.DesktopPrefix;
			} else
			if (fullName == MWFVFS.MyComputerPersonalPrefix) {
				return MWFVFS.MyComputerPrefix;
			} else
			if (fullName == MWFVFS.MyComputerPrefix) {
				return MWFVFS.DesktopPrefix;
			} else
			if (fullName == MWFVFS.MyNetworkPrefix) {
				return MWFVFS.DesktopPrefix;
			} else
			if (fullName == MWFVFS.DesktopPrefix) {
				return null;
			} else
			if (fullName == MWFVFS.RecentlyUsedPrefix) {
				return null;
			} else {
				foreach (DictionaryEntry entry in MWFVFS.MyComputerDevicesPrefix) {
					FSEntry fsEntry = entry.Value as FSEntry;
					if (fullName == fsEntry.FullName) {
						return fsEntry.MainTopNode.FullName;
					}
				}
				
				DirectoryInfo dirInfo = new DirectoryInfo (fullName);
				
				DirectoryInfo dirInfoParent = dirInfo.Parent;
				
				if (dirInfoParent != null) {
					FSEntry fsEntry = MWFVFS.MyComputerDevicesPrefix [dirInfoParent.FullName + "://"] as FSEntry;
					
					if (fsEntry != null) {
						return fsEntry.FullName;
					}
					
					if (mainTopNode != null) {
						if (dirInfoParent.FullName == ThemeEngine.Current.Places (UIIcon.PlacesDesktop) &&
						    mainTopNode.FullName == MWFVFS.DesktopPrefix) {
							return mainTopNode.FullName;
						} else
						if (dirInfoParent.FullName == ThemeEngine.Current.Places (UIIcon.PlacesPersonal) &&
						    mainTopNode.FullName == MWFVFS.PersonalPrefix) {
							return mainTopNode.FullName;
						} else
						if (dirInfoParent.FullName == ThemeEngine.Current.Places (UIIcon.PlacesPersonal) &&
						    mainTopNode.FullName == MWFVFS.MyComputerPersonalPrefix) {
							return mainTopNode.FullName;
						}
					}
					
					return dirInfoParent.FullName;
				}
			}
			
			return null;
		}

		internal bool IsImageFile()
		{
			var fileExtension = Path.GetExtension(FullName);
			if (String.IsNullOrEmpty(fileExtension))
				return false;
			var extension = fileExtension.ToLowerInvariant();
			return extension == ".bmp" ||
				extension == ".gif" ||
				extension == ".jpg" || extension == ".jpeg" ||
				extension == ".png" ||
				extension == ".tif" || extension == ".tiff";
		}

		internal Image Image { get; set; }
		private bool fMustDisposeImage;

		internal void SetImage()
		{
			try {
				Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
				using (Bitmap myBitmap = new Bitmap(FullName))
				{
					this.Image = myBitmap.GetThumbnailImage(48, 48, myCallback, IntPtr.Zero);
					fMustDisposeImage = true;
				}
			} catch (Exception) {
				// cannot handle this image format?  not an image file?
				this.Image = null;
			}
		}

		private bool ThumbnailCallback()
		{
			return false;
		}

		#region IDisposable implementation
		public void Dispose()
		{
			if (this.Image != null && fMustDisposeImage) {
				this.Image.Dispose();
				this.Image = null;
			}
		}
		#endregion
	}
	#endregion
	
	#region MasterMount
	// Alexsantas little *nix helper
	internal class MasterMount
	{
		// add more...
		internal enum FsTypes
		{
			none,
			ext2,
			ext3,
			hpfs,
			iso9660,
			jfs,
			minix,
			msdos,
			ntfs,
			reiserfs,
			ufs,
			umsdos,
			vfat,
			sysv,
			xfs,
			ncpfs,
			nfs,
			smbfs,
			usbfs,
			cifs
		}
		
		internal struct Mount
		{
			public string device_or_filesystem;
			public string device_short;
			public string mount_point;
			public FsTypes fsType;
		}
		
		bool proc_mount_available = false;
		
		ArrayList block_devices = new ArrayList ();
		ArrayList network_devices = new ArrayList ();
		ArrayList removable_devices = new ArrayList ();
		
		private MountComparer mountComparer = new MountComparer ();
		
		public MasterMount ()
		{
			// maybe check if the current user can access /proc/mounts
			if (XplatUI.RunningOnUnix)
				if (File.Exists ("/proc/mounts"))
					proc_mount_available = true;
		}
		
		public ArrayList Block_devices {
			get {
				return block_devices;
			}
		}
		
		public ArrayList Network_devices {
			get {
				return network_devices;
			}
		}
		
		public ArrayList Removable_devices {
			get {
				return removable_devices;
			}
		}
		
		public bool ProcMountAvailable {
			get {
				return proc_mount_available;
			}
		}
		
		public void GetMounts ()
		{
			if (!proc_mount_available)
				return;
			
			block_devices.Clear ();
			network_devices.Clear ();
			removable_devices.Clear ();
			
			try {
				StreamReader sr = new StreamReader ("/proc/mounts");
				
				string line = sr.ReadLine ();
				
				ArrayList lines = new ArrayList ();
 				while (line != null) {
					if (lines.IndexOf (line) == -1) { // Avoid duplicates
						ProcessProcMountLine (line);
						lines.Add (line);
					}
 					line = sr.ReadLine ();
 				}
				
				sr.Close ();
				
				block_devices.Sort (mountComparer);
				network_devices.Sort (mountComparer);
				removable_devices.Sort (mountComparer);
			} catch {
				// bla
			}
		}
		
		private void ProcessProcMountLine (string line)
		{
			string[] split = line.Split (new char [] {' '});
			
			if (split != null && split.Length > 0) {
				Mount mount = new Mount ();
				
				if (split [0].StartsWith ("/dev/"))
					mount.device_short = split [0].Replace ("/dev/", String.Empty);
				else 
					mount.device_short = split [0];
				
				mount.device_or_filesystem = split [0];
				mount.mount_point = split [1];
				
				// TODO: other removable devices, floppy
				// ssh
				
				// network mount
				if (split [2] == "nfs") {
					mount.fsType = FsTypes.nfs;
					network_devices.Add (mount);
				} else if (split [2] == "smbfs") {
					mount.fsType = FsTypes.smbfs;
					network_devices.Add (mount);
				} else if (split [2] == "cifs") {
					mount.fsType = FsTypes.cifs;
					network_devices.Add (mount);
				} else if (split [2] == "ncpfs") {
					mount.fsType = FsTypes.ncpfs;
					network_devices.Add (mount);
					
				} else if (split [2] == "iso9660") { //cdrom
					mount.fsType = FsTypes.iso9660;
					removable_devices.Add (mount);
				} else if (split [2] == "usbfs") { //usb ? not tested
					mount.fsType = FsTypes.usbfs;
					removable_devices.Add (mount);
					
				} else if (split [0].StartsWith ("/")) { //block devices
					if (split [1].StartsWith ("/dev/"))  // root static, do not add
						return;
					
					if (split [2] == "ext2")
						mount.fsType = FsTypes.ext2;
					else if (split [2] == "ext3")
						mount.fsType = FsTypes.ext3;
					else if (split [2] == "reiserfs")
						mount.fsType = FsTypes.reiserfs;
					else if (split [2] == "xfs")
						mount.fsType = FsTypes.xfs;
					else if (split [2] == "vfat")
						mount.fsType = FsTypes.vfat;
					else if (split [2] == "ntfs")
						mount.fsType = FsTypes.ntfs;
					else if (split [2] == "msdos")
						mount.fsType = FsTypes.msdos;
					else if (split [2] == "umsdos")
						mount.fsType = FsTypes.umsdos;
					else if (split [2] == "hpfs")
						mount.fsType = FsTypes.hpfs;
					else if (split [2] == "minix")
						mount.fsType = FsTypes.minix;
					else if (split [2] == "jfs")
						mount.fsType = FsTypes.jfs;
					
					block_devices.Add (mount);
				}
			}
		}
		
		public class MountComparer : IComparer
		{
			public int Compare (object mount1, object mount2)
			{
				return String.Compare (((Mount)mount1).device_short, ((Mount)mount2).device_short);
			}
		}
	}
	#endregion
		
	#region MWFConfig
	// easy to use class to store and read internal MWF config settings.
	// the config values are stored in the users home dir as a hidden xml file called "mwf_config".
	// currently supports int, string, byte, color and arrays (including byte arrays)
	// don't forget, when you read a value you still have to cast this value.
	//
	// usage:
	// MWFConfig.SetValue ("SomeClass", "What", value);
	// object o = MWFConfig.GetValue ("SomeClass", "What");
	//
	// example:
	// 
	// string[] configFileNames = (string[])MWFConfig.GetValue ("FileDialog", "FileNames");
	// MWFConfig.SetValue ("FileDialog", "LastFolder", "/home/user");
	
	internal class MWFConfig
	{
		private static MWFConfigInstance Instance = new MWFConfigInstance ();
		
		private static object lock_object = new object();
		
		public static object GetValue (string class_name, string value_name)
		{
			lock (lock_object) {
				return Instance.GetValue (class_name, value_name);
			}
		}
		
		public static void SetValue (string class_name, string value_name, object value)
		{
			lock (lock_object) {
				Instance.SetValue (class_name, value_name, value);
			}
		}
		
		public static void Flush ()
		{
			lock (lock_object) {
				Instance.Flush ();
			}
		}
		
		public static void RemoveClass (string class_name)
		{
			lock (lock_object) {
				Instance.RemoveClass (class_name);
			}
		}
		
		public static void RemoveClassValue (string class_name, string value_name)
		{
			lock (lock_object) {
				Instance.RemoveClassValue (class_name, value_name);
			}
		}
		
		public static void RemoveAllClassValues (string class_name)
		{
			lock (lock_object) {
				Instance.RemoveAllClassValues (class_name);
			}
		}
	
		internal class MWFConfigInstance
		{
			Hashtable classes_hashtable = new Hashtable ();
			static string full_file_name;
			static string default_file_name;
			readonly string configName = "MWFConfig";

			static MWFConfigInstance ()
			{
				string b = "mwf_config";
				string dir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);

				if (XplatUI.RunningOnUnix) {
					dir = Path.Combine (dir, ".mono");
					try {
						Directory.CreateDirectory (dir);
					} catch {}
				} 

				default_file_name = Path.Combine (dir, b);
				full_file_name = default_file_name;
			}
			
			public MWFConfigInstance ()
			{
				Open (default_file_name);
			}
			
			// only for testing
			public MWFConfigInstance (string filename)
			{
				string path = Path.GetDirectoryName (filename);
				if (path == null || path == String.Empty) {
					path = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					
					full_file_name = Path.Combine (path, filename);
				}  else 
					full_file_name = filename;

				Open (full_file_name);
			}
			
			~MWFConfigInstance ()
			{
				Flush ();
			}
			
			public object GetValue (string class_name, string value_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null)
					return class_entry.GetValue (value_name);
				
				return null;
			}
			
			public void SetValue (string class_name, string value_name, object value)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry == null) {
					class_entry = new ClassEntry ();
					class_entry.ClassName = class_name;
					classes_hashtable [class_name] = class_entry;
				}
				
				class_entry.SetValue (value_name, value);
			}
			
			private void Open (string filename)
			{
				try {
					XmlTextReader xtr = new XmlTextReader (filename);
					
					ReadConfig (xtr);
					
					xtr.Close ();
				} catch (Exception) {
				}
			}
			
			public void Flush ()
			{
				try {
					XmlTextWriter xtw = new XmlTextWriter (full_file_name, null);
					xtw.Formatting = Formatting.Indented;
					
					WriteConfig (xtw);
					
					xtw.Close ();

					if (!XplatUI.RunningOnUnix)
						File.SetAttributes (full_file_name, FileAttributes.Hidden);
				} catch (Exception){
				}
			}
			
			public void RemoveClass (string class_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null) {
					class_entry.RemoveAllClassValues ();
					
					classes_hashtable.Remove (class_name);
				}
			}
			
			public void RemoveClassValue (string class_name, string value_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null) {
					class_entry.RemoveClassValue (value_name);
				}
			}
			
			public void RemoveAllClassValues (string class_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null) {
					class_entry.RemoveAllClassValues ();
				}
			}
			
			private void ReadConfig (XmlTextReader xtr)
			{
				if (!CheckForMWFConfig (xtr))
					return;
				
				while (xtr.Read ()) {
					switch (xtr.NodeType) {
						case XmlNodeType.Element:
							ClassEntry class_entry = classes_hashtable [xtr.Name] as ClassEntry;
							
							if (class_entry == null) {
								class_entry = new ClassEntry ();
								class_entry.ClassName = xtr.Name;
								classes_hashtable [xtr.Name] = class_entry;
							}
							
							class_entry.ReadXml (xtr);
							break;
					}
				}
			}
			
			private bool CheckForMWFConfig (XmlTextReader xtr)
			{
				if (xtr.Read ()) {
					if (xtr.NodeType == XmlNodeType.Element) {
						if (xtr.Name == configName)
							return true;
					}
				}
				
				return false;
			}
			
			private void WriteConfig (XmlTextWriter xtw)
			{
				if (classes_hashtable.Count == 0)
					return;
				
				xtw.WriteStartElement (configName);
				foreach (DictionaryEntry entry in classes_hashtable) {
					ClassEntry class_entry = entry.Value as ClassEntry;
					
					class_entry.WriteXml (xtw);
				}
				xtw.WriteEndElement ();
			}
			
			internal class ClassEntry
			{
				private Hashtable classvalues_hashtable = new Hashtable ();
				private string className;
				
				public string ClassName {
					set {
						className = value;
					}
					
					get {
						return className;
					}
				}
				
				public void SetValue (string value_name, object value)
				{
					ClassValue class_value = classvalues_hashtable [value_name] as ClassValue;
					
					if (class_value == null) {
						class_value = new ClassValue ();
						class_value.Name = value_name;
						classvalues_hashtable [value_name] = class_value;
					}
					
					class_value.SetValue (value);
				}
				
				public object GetValue (string value_name)
				{
					ClassValue class_value = classvalues_hashtable [value_name] as ClassValue;
					
					if (class_value == null) {
						return null;
					}
					
					return class_value.GetValue ();
				}
				
				public void RemoveAllClassValues ()
				{
					classvalues_hashtable.Clear ();
				}
				
				public void RemoveClassValue (string value_name)
				{
					ClassValue class_value = classvalues_hashtable [value_name] as ClassValue;
					
					if (class_value != null) {
						classvalues_hashtable.Remove (value_name);
					}
				}
				
				public void ReadXml (XmlTextReader xtr)
				{
					while (xtr.Read ()) {
						switch (xtr.NodeType) {
							case XmlNodeType.Element:
								string name = xtr.GetAttribute ("name");
								
								ClassValue class_value = classvalues_hashtable [name] as ClassValue;
								
								if (class_value == null) {
									class_value = new ClassValue ();
									class_value.Name = name;
									classvalues_hashtable [name] = class_value;
								}
								
								class_value.ReadXml (xtr);
								break;
								
							case XmlNodeType.EndElement:
								return;
						}
					}
				}
				
				public void WriteXml (XmlTextWriter xtw)
				{
					if (classvalues_hashtable.Count == 0)
						return;
					
					xtw.WriteStartElement (className);
					
					foreach (DictionaryEntry entry in classvalues_hashtable) {
						ClassValue class_value = entry.Value as ClassValue;
						
						class_value.WriteXml (xtw);
					}
					
					xtw.WriteEndElement ();
				}
			}
			
			internal class ClassValue
			{
				private object value;
				private string name;
				
				public string Name {
					set {
						name = value;
					}
					
					get {
						return name;
					}
				}
				
				public void SetValue (object value)
				{
					this.value = value;
				}
				public object GetValue ()
				{
					return value;
				}
				
				public void ReadXml (XmlTextReader xtr)
				{
					string type;
					string single_value;
					
					type = xtr.GetAttribute ("type");
					
					if (type == "byte_array" || type.IndexOf ("-array") == -1) {
						single_value = xtr.ReadString ();
						
						if (type == "string") {
							value = single_value;
						} else
						if (type == "int") {
							value = Int32.Parse (single_value);
						} else
						if (type == "byte") {
							value = Byte.Parse (single_value);
						} else
						if (type == "color") {
							int color = Int32.Parse (single_value);
							value = Color.FromArgb (color);
						} else
						if (type == "byte-array") {
							byte[] b_array = Convert.FromBase64String (single_value);
							value = b_array;
						}
					} else {
						ReadXmlArrayValues (xtr, type);
					}
				}
				
				private void ReadXmlArrayValues (XmlTextReader xtr, string type)
				{
					ArrayList al = new ArrayList ();
					
					while (xtr.Read ()) {
						switch (xtr.NodeType) {
							case XmlNodeType.Element:
								string single_value = xtr.ReadString ();
								
								if (type == "int-array") {
									int int_val = Int32.Parse (single_value);
									al.Add (int_val);
								} else
								if (type == "string-array") {
									string str_val = single_value;
									al.Add (str_val);
								}
								break;
								
							case XmlNodeType.EndElement:
								if (xtr.Name == "value") {
									if (type == "int-array") {
										value = al.ToArray (typeof(int));
									} else
									if (type == "string-array") {
										value = al.ToArray (typeof(string));
									} 
									return;
								}
								break;
						}
					}
				}
				
				public void WriteXml (XmlTextWriter xtw)
				{
					xtw.WriteStartElement ("value");
					xtw.WriteAttributeString ("name", name);
					if (value is Array) {
						WriteArrayContent (xtw);
					} else {
						WriteSingleContent (xtw);
					}
					xtw.WriteEndElement ();
				}
				
				private void WriteSingleContent (XmlTextWriter xtw)
				{
					string type_string = String.Empty;
					
					if (value is string)
						type_string = "string";
					else
					if (value is int)
						type_string = "int";
					else
					if (value is byte)
						type_string = "byte";
					else
					if (value is Color)
						type_string = "color";
					
					xtw.WriteAttributeString ("type", type_string);
					
					if (value is Color)
						xtw.WriteString (((Color)value).ToArgb ().ToString ());
					else
						xtw.WriteString (value.ToString ());
				}
				
				private void WriteArrayContent (XmlTextWriter xtw)
				{
					string type_string = String.Empty;
					string type_name = String.Empty;
					
					if (value is string[]) {
						type_string = "string-array";
						type_name = "string";
					} else
					if (value is int[]) {
						type_string = "int-array";
						type_name = "int";
					} else
					if (value is byte[]) {
						type_string = "byte-array";
						type_name = "byte";
					}
					
					xtw.WriteAttributeString ("type", type_string);
					
					if (type_string != "byte-array") {
						Array array = value as Array;
						
						foreach (object o in array) {
							xtw.WriteStartElement (type_name);
							xtw.WriteString (o.ToString ());
							xtw.WriteEndElement ();
						}
					} else {
						byte[] b_array = value as byte [];
						
						xtw.WriteString (Convert.ToBase64String (b_array, 0, b_array.Length));
					}
				}
			}
		}
	}
	#endregion
}

//#endif // MONOMAC
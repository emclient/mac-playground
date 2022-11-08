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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Windows.Forms
{
	#region FileDialog
	[DefaultProperty ("FileName")]
	[DefaultEvent ("FileOk")]
	public abstract class FileDialog : CommonDialog
	{
		protected static readonly object EventFileOk = new object ();
		private static int MaxFileNameItems = 10;
		
		internal enum FileDialogType
		{
			OpenFileDialog,
			SaveFileDialog
		}
		
		private bool addExtension = true;
		private bool checkFileExists;
		private bool checkPathExists = true;
		private string defaultExt;
		private bool dereferenceLinks = true;
		private string[] fileNames;
		private string filter = "";
		private int filterIndex = 1;
		private string initialDirectory;
		private bool restoreDirectory;
		private bool showHelp;
		private string title;
		private bool validateNames = true;
		private bool auto_upgrade_enable = true;
		private FileDialogCustomPlacesCollection custom_places;
		private bool supportMultiDottedExtensions;
		private bool checkForIllegalChars = true;
		private Button cancelButton;
		private ToolStripButton upToolBarButton;
		private PopupButtonPanel popupButtonPanel;
		private Button openSaveButton;
		private Button helpButton;
		private Label fileTypeLabel;
		private ToolStripDropDownButton menueToolBarButton;
		private ToolStrip smallButtonToolBar;
		private DirComboBox dirComboBox;
		private ComboBox fileNameComboBox;
		private Label fileNameLabel;
		private MWFFileView mwfFileView;
		private MwfFileViewItemComparer file_view_comparer;
		private Label searchSaveLabel;
		private ToolStripButton newdirToolBarButton;
		private ToolStripButton backToolBarButton;
		private ComboBox fileTypeComboBox;
		private ImageList imageListTopToolbar;
		private CheckBox readonlyCheckBox;
		
		private bool multiSelect;
		
		private string restoreDirectoryString = String.Empty;
		
		internal FileDialogType fileDialogType;
		
		private bool do_not_call_OnSelectedIndexChangedFileTypeComboBox;
		
		private bool showReadOnly;
		private bool readOnlyChecked;
		internal bool createPrompt;
		internal bool overwritePrompt = true;
		
		private FileFilter fileFilter;
		private string[] configFileNames = null;		
		private string lastFolder = String.Empty;
		
		private MWFVFS vfs;
		
		private const string filedialog_string = "FileDialog";
		private const string lastfolder_string = "LastFolder";
		private const string width_string = "Width";
		private const string height_string = "Height";
		private const string filenames_string = "FileNames";
		private const string x_string = "X";
		private const string y_string = "Y";
		
		private readonly char [] wildcard_chars = new char [] { '*', '?' };

		private bool disable_form_closed_event;
		
		internal FileDialog ()
		{
			form = new DialogForm (this);
			vfs = new MWFVFS ();
			
			Size formConfigSize = Size.Empty;
			Point formConfigLocation = Point.Empty;
			
			object formWidth = MWFConfig.GetValue (filedialog_string, width_string);
			
			object formHeight = MWFConfig.GetValue (filedialog_string, height_string);
			
			if (formHeight != null && formWidth != null)
				formConfigSize = new Size ((int)formWidth, (int)formHeight);
			
			object formLocationX = MWFConfig.GetValue (filedialog_string, x_string);
			object formLocationY = MWFConfig.GetValue (filedialog_string, y_string);
			
			if (formLocationX != null && formLocationY != null)
				formConfigLocation = new Point ((int)formLocationX, (int)formLocationY);
			
			configFileNames = (string[])MWFConfig.GetValue (filedialog_string, filenames_string);
			
			fileTypeComboBox = new ComboBox ();
			backToolBarButton = new ToolStripButton ();
			newdirToolBarButton = new ToolStripButton ();
			searchSaveLabel = new Label ();
			mwfFileView = new MWFFileView (vfs);
			fileNameLabel = new Label ();
			fileNameComboBox = new ComboBox ();
			dirComboBox = new DirComboBox (vfs);
			smallButtonToolBar = new ToolStrip ();
			menueToolBarButton = new ToolStripDropDownButton ();
			fileTypeLabel = new Label ();
			openSaveButton = new Button ();
			helpButton = new Button ();
			popupButtonPanel = new PopupButtonPanel ();
			upToolBarButton = new ToolStripButton ();
			cancelButton = new Button ();
			form.CancelButton = cancelButton;
			imageListTopToolbar = new ImageList ();
			readonlyCheckBox = new CheckBox ();
			
			form.SuspendLayout ();
			
			//imageListTopToolbar
			imageListTopToolbar.ColorDepth = ColorDepth.Depth32Bit;
			imageListTopToolbar.ImageSize = new Size (16, 16); // 16, 16
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("go-previous.png"));
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("go-top.png"));
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("folder-new.png"));
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("preferences-system-windows.png"));
			imageListTopToolbar.TransparentColor = Color.Transparent;
			
			// searchLabel
			searchSaveLabel.FlatStyle = FlatStyle.System;
			searchSaveLabel.Location = new Point (6, 6);
			searchSaveLabel.Size = new Size (86, 22);
			searchSaveLabel.TextAlign = ContentAlignment.MiddleRight;
			
			// dirComboBox
			dirComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
			dirComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			dirComboBox.Location = new Point (99, 6);
			dirComboBox.Size = new Size (261, 22);
			dirComboBox.TabIndex = 7;
			
			// smallButtonToolBar
			smallButtonToolBar.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			smallButtonToolBar.AutoSize = false;
			smallButtonToolBar.Items.AddRange (new ToolStripItem [] {
								     backToolBarButton,
								     upToolBarButton,
								     newdirToolBarButton,
								     menueToolBarButton});
			smallButtonToolBar.Dock = DockStyle.None;
			smallButtonToolBar.ImageList = imageListTopToolbar;
			smallButtonToolBar.Location = new Point (372, 6);
			smallButtonToolBar.Size = new Size (140, 28);
			smallButtonToolBar.TabIndex = 8;
			
			// buttonPanel
			popupButtonPanel.Dock = DockStyle.None;
			popupButtonPanel.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left))));
			popupButtonPanel.Location = new Point (6, 35);
			popupButtonPanel.Size = new Size (87, 338);
			popupButtonPanel.TabIndex = 9;
			
			// mwfFileView
			mwfFileView.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
			mwfFileView.Location = new Point (99, 35);
			mwfFileView.Size = new Size (450, 283);
			mwfFileView.MultiSelect = false;
			mwfFileView.TabIndex = 10;
			mwfFileView.RegisterSender (dirComboBox);
			mwfFileView.RegisterSender (popupButtonPanel);
			
			// fileNameLabel
			fileNameLabel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			fileNameLabel.FlatStyle = FlatStyle.System;
			fileNameLabel.Location = new Point (101, 326);
			fileNameLabel.Size = new Size (70, 21);
			fileNameLabel.Text = "File name:";
			fileNameLabel.TextAlign = ContentAlignment.MiddleLeft;
			
			// fileNameComboBox
			fileNameComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			fileNameComboBox.Location = new Point (195, 326);
			fileNameComboBox.Size = new Size (246, 22);
			fileNameComboBox.TabIndex = 1;
			fileNameComboBox.MaxDropDownItems = MaxFileNameItems;
			UpdateRecentFiles ();
			
			// fileTypeLabel
			fileTypeLabel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			fileTypeLabel.FlatStyle = FlatStyle.System;
			fileTypeLabel.Location = new Point (101, 355);
			fileTypeLabel.Size = new Size (90, 21);
			fileTypeLabel.Text = "Files of type:";
			fileTypeLabel.TextAlign = ContentAlignment.MiddleLeft;
			
			// fileTypeComboBox
			fileTypeComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			fileTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			fileTypeComboBox.Location = new Point (195, 355);
			fileTypeComboBox.Size = new Size (246, 22);
			fileTypeComboBox.TabIndex = 2;
			
			// backToolBarButton
			backToolBarButton.ImageIndex = 0;
			backToolBarButton.Enabled = false;
			mwfFileView.AddControlToEnableDisableByDirStack (backToolBarButton);
			
			// upToolBarButton
			upToolBarButton.ImageIndex = 1;
			mwfFileView.SetFolderUpToolBarButton (upToolBarButton);
			
			// newdirToolBarButton
			newdirToolBarButton.ImageIndex = 2;
			
			// menueToolBarButton
			menueToolBarButton.ImageIndex = 3;
			
			// menueToolBarButtonContextMenu
			menueToolBarButton.DropDownItems.AddRange (mwfFileView.ViewMenuItems);
			
			// openSaveButton
			openSaveButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			openSaveButton.FlatStyle = FlatStyle.System;
			openSaveButton.Location = new Point (474, 326);
			openSaveButton.Size = new Size (75, 23);
			openSaveButton.TabIndex = 4;
			openSaveButton.FlatStyle = FlatStyle.System;
			
			// cancelButton
			cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancelButton.FlatStyle = FlatStyle.System;
			cancelButton.Location = new Point (474, 353);
			cancelButton.Size = new Size (75, 23);
			cancelButton.TabIndex = 5;
			cancelButton.Text = "Cancel";
			cancelButton.FlatStyle = FlatStyle.System;
			
			// helpButton
			helpButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			helpButton.FlatStyle = FlatStyle.System;
			helpButton.Location = new Point (474, 353);
			helpButton.Size = new Size (75, 23);
			helpButton.TabIndex = 6;
			helpButton.Text = "Help";
			helpButton.FlatStyle = FlatStyle.System;
			helpButton.Visible = false;
			
			// checkBox
			readonlyCheckBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			readonlyCheckBox.Text = "Open Readonly";
			readonlyCheckBox.Location = new Point (195, 350);
			readonlyCheckBox.Size = new Size (245, 21);
			readonlyCheckBox.TabIndex = 3;
			readonlyCheckBox.FlatStyle = FlatStyle.System;
			readonlyCheckBox.Visible = false;
			
			form.SizeGripStyle = SizeGripStyle.Show;
			form.AcceptButton = openSaveButton;
			form.MaximizeBox = true;
			form.MinimizeBox = true;
			form.FormBorderStyle = FormBorderStyle.Sizable;
			form.ClientSize =  new Size (555, 385);
			form.MinimumSize = form.Size;

			form.Controls.Add (smallButtonToolBar);
			form.Controls.Add (cancelButton);
			form.Controls.Add (openSaveButton);
			form.Controls.Add (mwfFileView);
			form.Controls.Add (fileTypeLabel);
			form.Controls.Add (fileNameLabel);
			form.Controls.Add (fileTypeComboBox);
			form.Controls.Add (fileNameComboBox);
			form.Controls.Add (dirComboBox);
			form.Controls.Add (searchSaveLabel);
			form.Controls.Add (popupButtonPanel);
			form.Controls.Add (helpButton);
			form.Controls.Add (readonlyCheckBox);
			
			form.ResumeLayout (true);

			if (formConfigSize != Size.Empty) {
				form.ClientSize = formConfigSize;
			}
			
			if (formConfigLocation != Point.Empty) {
				form.Location = formConfigLocation;
			}
			
			openSaveButton.Click += new EventHandler (OnClickOpenSaveButton);
			cancelButton.Click += new EventHandler (OnClickCancelButton);
			helpButton.Click += new EventHandler (OnClickHelpButton);
			
			smallButtonToolBar.ItemClicked += new ToolStripItemClickedEventHandler (OnClickSmallButtonToolBar);
			
			fileTypeComboBox.SelectedIndexChanged += new EventHandler (OnSelectedIndexChangedFileTypeComboBox);
			
			mwfFileView.SelectedFileChanged += new EventHandler (OnSelectedFileChangedFileView);
			mwfFileView.ForceDialogEnd += new EventHandler (OnForceDialogEndFileView);
			mwfFileView.SelectedFilesChanged += new EventHandler (OnSelectedFilesChangedFileView);
			mwfFileView.ColumnClick += new ColumnClickEventHandler(OnColumnClickFileView);
			
			dirComboBox.DirectoryChanged += new EventHandler (OnDirectoryChangedDirComboBox);
			popupButtonPanel.DirectoryChanged += new EventHandler (OnDirectoryChangedPopupButtonPanel);

			readonlyCheckBox.CheckedChanged += new EventHandler (OnCheckCheckChanged);
			form.FormClosed += new FormClosedEventHandler (OnFileDialogFormClosed);
			custom_places = new FileDialogCustomPlacesCollection ();
		}
		
		[DefaultValue(true)]
		public bool AddExtension {
			get {
				return addExtension;
			}
			
			set {
				addExtension = value;
			}
		}
		
		[MonoTODO ("Stub, value not respected")]
		[DefaultValue (true)]
		public bool AutoUpgradeEnabled {
			get { return auto_upgrade_enable; }
			set { auto_upgrade_enable = value; }
		}

		[DefaultValue(false)]
		public virtual bool CheckFileExists {
			get {
				return checkFileExists;
			}
			
			set {
				checkFileExists = value;
			}
		}
		
		[DefaultValue(true)]
		public bool CheckPathExists {
			get {
				return checkPathExists;
			}
			
			set {
				checkPathExists = value;
			}
		}
		
		[MonoTODO ("Stub, collection not used")]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public FileDialogCustomPlacesCollection CustomPlaces {
			get { return custom_places; }
		}

		[DefaultValue("")]
		[AllowNull]
		public string DefaultExt {
			get {
				if (defaultExt == null)
					return string.Empty;
				return defaultExt;
			}
			set {
				if (value != null && value.Length > 0) {
					// remove leading dot
					if (value [0] == '.')
						value = value.Substring (1);
				}
				defaultExt = value;
			}
		}
		
		// in MS.NET it doesn't make a difference if
		// DerefenceLinks is true or false
		// if the selected file is a link FileDialog
		// always returns the link
		[DefaultValue(true)]
		public bool DereferenceLinks {
			get {
				return dereferenceLinks;
			}
			
			set {
				dereferenceLinks = value;
			}
		}
		
		[DefaultValue("")]
		[AllowNull]
		public string FileName {
			get {
				if (fileNames == null || fileNames.Length == 0)
					return string.Empty;

				if (fileNames [0].Length == 0)
					return string.Empty;

				// skip check for illegal characters if the filename was set
				// through FileDialog API
				if (!checkForIllegalChars)
					return fileNames [0];

				// ensure filename contains only valid characters
				Path.GetFullPath (fileNames [0]);
				// but return filename as is
				return fileNames [0];

			}
			
			set {
				if (value != null) {
					fileNames = new string [1] { value };
				} else {
					fileNames = new string [0];
				}

				// skip check for illegal characters if the filename was set
				// through FileDialog API
				checkForIllegalChars = false;
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] FileNames {
			get {
				if (fileNames == null || fileNames.Length == 0) {
					return new string [0];
				}
				
				string[] new_filenames = new string [fileNames.Length];
				fileNames.CopyTo (new_filenames, 0);

				// skip check for illegal characters if the filename was set
				// through FileDialog API
				if (!checkForIllegalChars)
					return new_filenames;

				foreach (string fileName in new_filenames) {
					// ensure filename contains only valid characters
					Path.GetFullPath (fileName);
				}
				return new_filenames;
			}
		}
		
		[DefaultValue("")]
		[Localizable(true)]
		[AllowNull]
		public string Filter {
			get {
				return filter;
			}
			
			set {
				if (value == null) {
					filter = "";
					if (fileFilter != null)
						fileFilter.FilterArrayList.Clear ();
				} else {
					if (FileFilter.CheckFilter (value)) {
						filter = value;
						
						fileFilter = new FileFilter (filter);
					} else
						throw new ArgumentException ("The provided filter string"
							+ " is invalid. The filter string should contain a"
							+ " description of the filter, followed by the "
							+ " vertical bar (|) and the filter pattern. The"
							+ " strings for different filtering options should"
							+ " also be separated by the vertical bar. Example:"
							+ " Text files (*.txt)|*.txt|All files (*.*)|*.*");
				}
				
				UpdateFilters ();
			}
		}
		
		[DefaultValue(1)]
		public int FilterIndex {
			get {
				return filterIndex;
			}
			set {
				filterIndex = value;
			}
		}
		
		[DefaultValue("")]
		[AllowNull]
		public string InitialDirectory {
			get {
				if (initialDirectory == null)
					return string.Empty;
				return initialDirectory;
			}
			set {
				initialDirectory = value;
			}
		}
		
		[DefaultValue(false)]
		public bool RestoreDirectory {
			get {
				return restoreDirectory;
			}
			
			set {
				restoreDirectory = value;
			}
		}
		
		[DefaultValue(false)]
		public bool ShowHelp {
			get {
				return showHelp;
			}
			
			set {
				showHelp = value;
				ResizeAndRelocateForHelpOrReadOnly ();
			}
		}
		
		[DefaultValue(false)]
		public bool SupportMultiDottedExtensions {
			get {
				return supportMultiDottedExtensions;
			}

			set {
				supportMultiDottedExtensions = value;
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		public string Title {
			get {
				if (title == null)
					return string.Empty;
				return title;
			}
			set {
				title = value;
			}
		}
		
		// this one is a hard one ;)
		// Win32 filename:
		// - up to MAX_PATH characters (windef.h) = 260
		// - no trailing dots or spaces
		// - case preserving
		// - etc...
		// NTFS/Posix filename:
		// - up to 32,768 Unicode characters
		// - trailing periods or spaces
		// - case sensitive
		// - etc...
		[DefaultValue(true)]
		public bool ValidateNames {
			get {
				return validateNames;
			}
			
			set {
				validateNames = value;
			}
		}
		
		public override void Reset ()
		{
			addExtension = true;
			checkFileExists = false;
			checkPathExists = true;
			DefaultExt = null;
			dereferenceLinks = true;
			FileName = null;
			Filter = String.Empty;
			FilterIndex = 1;
			InitialDirectory = null;
			restoreDirectory = false;
			SupportMultiDottedExtensions = false;
			ShowHelp = false;
			Title = null;
			validateNames = true;
			
			UpdateFilters ();
		}
		
		public override string ToString ()
		{
			return String.Format("{0}: Title: {1}, FileName: {2}", base.ToString (), Title, FileName);
		}
		
		public event CancelEventHandler FileOk {
			add { Events.AddHandler (EventFileOk, value); }
			remove { Events.RemoveHandler (EventFileOk, value); }
		}
		
		protected virtual IntPtr Instance {
			get {
				if (form == null)
					return IntPtr.Zero;
				return form.Handle;
			}
		}
		
		// This is just for internal use with MSs version, so it doesn't need to be implemented
		// as it can't really be accessed anyways
		protected int Options {
			get { return -1; }
		}

		internal virtual string DialogTitle {
			get {
				return Title;
			}
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		protected override IntPtr HookProc (IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			throw new NotImplementedException ();
		}
		
		protected void OnFileOk (CancelEventArgs e)
		{	
			CancelEventHandler fo = (CancelEventHandler) Events [EventFileOk];
			if (fo != null)
				fo (this, e);
		}
		
		private void CleanupOnClose ()
		{
			mwfFileView.StopThumbnailCreation();

			WriteConfigValues ();
			
			Mime.CleanFileCache ();
			
			disable_form_closed_event = true;
		}
		
		protected override bool RunDialog (IntPtr hWndOwner)
		{
			ReadConfigValues ();
			form.Text = DialogTitle;

			// avoid using the FileNames property to skip the invalid characters
			// check
			string fileName = null;
			if (fileNames != null && fileNames.Length != 0)
				fileName = fileNames [0];
			else
				fileName = string.Empty;

			SelectFilter ();

			form.Refresh ();

			SetFileAndDirectory (fileName);
			fileNameComboBox.Select ();
			
			return true;
		}
		
		internal virtual bool ShowReadOnly {
			set {
				showReadOnly = value;
				ResizeAndRelocateForHelpOrReadOnly ();
			}
			
			get {
				return showReadOnly;
			}
		}
		
		internal virtual bool ReadOnlyChecked {
			set {
				readOnlyChecked = value;
				readonlyCheckBox.Checked = value;
			}
			
			get {
				return readOnlyChecked;
			}
		}
		
		internal bool BMultiSelect {
			set {
				multiSelect = value;
				mwfFileView.MultiSelect = value;
			}
			
			get {
				return multiSelect;
			}
		}
		
		internal string OpenSaveButtonText {
			set {
				openSaveButton.Text = value;
			}
		}
		
		internal string SearchSaveLabel {
			set {
				searchSaveLabel.Text = value;
			}
		}

		internal string FileTypeLabel {
			set {
				fileTypeLabel.Text = value;
			}
		}

		internal string CustomFilter {
			get {
				string fname = fileNameComboBox.Text;
				if (fname.IndexOfAny (wildcard_chars) == -1)
					return null;

				return fname;
			}
		}

		private void SelectFilter ()
		{
			int filter_to_select = (filterIndex - 1);

			if (mwfFileView.FilterArrayList == null || mwfFileView.FilterArrayList.Count == 0) {
				filter_to_select = -1;
			} else {
				if (filter_to_select < 0 || filter_to_select >= mwfFileView.FilterArrayList.Count)
					filter_to_select = 0;
			}

			do_not_call_OnSelectedIndexChangedFileTypeComboBox = true;
			fileTypeComboBox.BeginUpdate ();
			fileTypeComboBox.SelectedIndex = filter_to_select;
			fileTypeComboBox.EndUpdate ();
			do_not_call_OnSelectedIndexChangedFileTypeComboBox = false;
			mwfFileView.FilterIndex = filter_to_select + 1;
		}

		private void SetFileAndDirectory (string fname)
		{
			if (fname.Length != 0) {
				bool rooted = Path.IsPathRooted (fname);
				if (!rooted) {
					mwfFileView.ChangeDirectory (null, lastFolder);
					fileNameComboBox.Text = fname;
				} else {
					string dirname = Path.GetDirectoryName (fname);
					if (dirname != null && dirname.Length > 0 && Directory.Exists (dirname)) {
						fileNameComboBox.Text = Path.GetFileName (fname);
						mwfFileView.ChangeDirectory (null, dirname);
					} else {
						fileNameComboBox.Text = fname;
						mwfFileView.ChangeDirectory (null, lastFolder);
					}
				}
			} else {
				mwfFileView.ChangeDirectory (null, lastFolder);
				fileNameComboBox.Text = null;
			}
		}
		
		void OnClickOpenSaveButton (object sender, EventArgs e)
		{
			// for filenames typed or selected by user, enable check for 
			// illegal characters in filename(s)
			checkForIllegalChars = true;

			if (fileDialogType == FileDialogType.OpenFileDialog) {
				ListView.SelectedListViewItemCollection sl = mwfFileView.SelectedItems;
				if (sl.Count > 0 && sl [0] != null) {
					if (sl.Count == 1) {
						FileViewListViewItem item = sl [0] as FileViewListViewItem;
						FSEntry fsEntry = item.FSEntry;

						if ((fsEntry.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
							mwfFileView.ChangeDirectory (null, fsEntry.FullName, CustomFilter);
							return;
						}
					} else {
						foreach (FileViewListViewItem item in sl) {
							FSEntry fsEntry = item.FSEntry;
							if ((fsEntry.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
								mwfFileView.ChangeDirectory (null, fsEntry.FullName, CustomFilter);
								return;
							}
						}
					}
				}
			}

			// Custom filter, typed by the user, ignoring the stored filters
			if (fileNameComboBox.Text.IndexOfAny (wildcard_chars) != -1) {
				mwfFileView.UpdateFileView (fileNameComboBox.Text);
				return;
			}

			ArrayList files = new ArrayList ();
			FileNamesTokenizer tokenizer = new FileNamesTokenizer (
				fileNameComboBox.Text, multiSelect);
			tokenizer.GetNextFile ();
			while (tokenizer.CurrentToken != TokenType.EOF) {
				string fileName = tokenizer.TokenText;
				string internalfullfilename;

				if (!Path.IsPathRooted (fileName)) {
					// on unix currentRealFolder for "Recently used files" is null,
					// because recently used files don't get saved as links in a directory
					// recently used files get saved in a xml file
					if (mwfFileView.CurrentRealFolder != null)
						fileName = Path.Combine (mwfFileView.CurrentRealFolder, fileName);
					else
						if (mwfFileView.CurrentFSEntry != null) {
							fileName = mwfFileView.CurrentFSEntry.FullName;
						}
				}

				FileInfo fileInfo = new FileInfo (fileName);

				if (fileInfo.Exists || fileDialogType == FileDialogType.SaveFileDialog) {
					internalfullfilename = fileName;
				} else {
					DirectoryInfo dirInfo = new DirectoryInfo (fileName);
					if (dirInfo.Exists) {
						mwfFileView.ChangeDirectory (null, dirInfo.FullName, CustomFilter);
						fileNameComboBox.Text = null;
						return;
					} else {
						internalfullfilename = fileName;
					}
				}

				if (addExtension) {
					string current_extension = Path.GetExtension (fileName);
					if (current_extension.Length == 0) {
						string filter_extension = string.Empty;

						if (AddFilterExtension (internalfullfilename))
							filter_extension = GetExtension (internalfullfilename);

						if (filter_extension.Length == 0 && DefaultExt.Length > 0) {
							filter_extension = "." + DefaultExt;

							if (checkFileExists) {
								// ignore DefaultExt if file not exist
								if (!File.Exists (internalfullfilename + filter_extension))
									filter_extension = string.Empty;
							}
						}

						internalfullfilename += filter_extension;
					}
				}

				if (checkFileExists) {
					if (!File.Exists (internalfullfilename)) {
						string message = "\"" + internalfullfilename + "\" does not exist. Please verify that you have entered the correct file name.";
						MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}
				}

				if (fileDialogType == FileDialogType.SaveFileDialog) {
					if (overwritePrompt) {
						if (File.Exists (internalfullfilename)) {
							string message = "\"" + internalfullfilename + "\" already exists. Do you want to overwrite it?";
							DialogResult dr = MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
							if (dr == DialogResult.Cancel)
								return;
						}
					}

					if (createPrompt) {
						if (!File.Exists (internalfullfilename)) {
							string message = "\"" + internalfullfilename + "\" does not exist. Do you want to create it?";
							DialogResult dr = MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
							if (dr == DialogResult.Cancel)
								return;
						}
					}
				}

				files.Add (internalfullfilename);
				tokenizer.GetNextFile ();
			}

			if (files.Count > 0) {
				fileNames = new string [files.Count];
				for (int i = 0; i < files.Count; i++) {
					string fileName = (string) files [i];
					fileNames [i] = fileName;
					mwfFileView.WriteRecentlyUsed (fileName);

					if (!File.Exists (fileName))
						// ignore files that do not exist
						continue;

					if (fileNameComboBox.Items.IndexOf (fileName) == -1)
						fileNameComboBox.Items.Insert (0, fileName);
				}

				// remove items above the maximum items that we want to display
				while (fileNameComboBox.Items.Count > MaxFileNameItems)
					fileNameComboBox.Items.RemoveAt (MaxFileNameItems);
			} else {
				// If a directory is selected, navigate into it
				foreach (FileViewListViewItem item in mwfFileView.SelectedItems) {
					FSEntry fsEntry = item.FSEntry;
					
					if ((fsEntry.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
						mwfFileView.ChangeDirectory (null, fsEntry.FullName, CustomFilter);
						return;
					}
				}

				return;
			}

			if (checkPathExists && mwfFileView.CurrentRealFolder != null) {
				if (!Directory.Exists (mwfFileView.CurrentRealFolder)) {
					string message = "\"" + mwfFileView.CurrentRealFolder + "\" does not exist. Please verify that you have entered the correct directory name.";
					MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

					if (InitialDirectory.Length == 0 || !Directory.Exists (InitialDirectory))
						mwfFileView.ChangeDirectory (null, lastFolder, CustomFilter);
					else
						mwfFileView.ChangeDirectory (null, InitialDirectory, CustomFilter);
					return;
				}
			}

			if (restoreDirectory) {
				lastFolder = restoreDirectoryString;
			} else {
				lastFolder = mwfFileView.CurrentFolder;
			}

			// update value of FilterIndex with user-selected filter
			filterIndex = fileTypeComboBox.SelectedIndex + 1;

			CancelEventArgs cancelEventArgs = new CancelEventArgs ();

			OnFileOk (cancelEventArgs);

			if (cancelEventArgs.Cancel)
				return;
				
			CleanupOnClose ();
			form.DialogResult = DialogResult.OK;
		}

		bool AddFilterExtension (string fileName)
		{
			if (fileDialogType == FileDialogType.OpenFileDialog) {
				if (DefaultExt.Length == 0)
					return true;

				if (checkFileExists) {
					// if CheckFileExists is true, only add filter extension if
					// file with DefaultExt does not exist
					string fullFileName = fileName + "." + DefaultExt;
					return !File.Exists (fullFileName);
				} else {
					// if CheckFileExists is false, only add filter extension
					// if specified file does not exist
					return !File.Exists (fileName);
				}
			}

			return true;
		}

		string GetExtension (string fileName)
		{
			string filter_extension = String.Empty;

			if (fileFilter == null || fileTypeComboBox.SelectedIndex == -1)
				return filter_extension;

			FilterStruct filterstruct = (FilterStruct) fileFilter.FilterArrayList
				[fileTypeComboBox.SelectedIndex];

			for (int i = 0; i < filterstruct.filters.Count; i++) {
				string extension = filterstruct.filters [i];

				if (extension.StartsWith ("*"))
					extension = extension.Remove (0, 1);

				if (extension.IndexOf ('*') != -1)
					continue;

				if (!supportMultiDottedExtensions) {
					int lastdot = extension.LastIndexOf('.');
					if (lastdot > 0) {
						if (extension.LastIndexOf('.', lastdot - 1) != -1) {
							extension = extension.Remove(0, lastdot);
						}
					}
				}

				if (!checkFileExists) {
					filter_extension = extension;
					break;
				}

				if (fileDialogType == FileDialogType.SaveFileDialog) {
					// when DefaultExt is set, only consider first filter
					// extension (and do not check if file exists)
					if (DefaultExt.Length > 0) {
						filter_extension = extension;
						break;
					}
				}

				// MSDN: If the CheckFileExists property is true,
				// the dialog box adds the first extension from the
				// current file filter that matches an existing file
				string fullfilename = fileName + extension;
				if (File.Exists (fullfilename)) {
					filter_extension = extension;
					break;
				} else {
					if (fileDialogType == FileDialogType.SaveFileDialog) {
						// when DefaultExt is set, only consider first filter
						// extension
						if (DefaultExt.Length > 0) {
							filter_extension = extension;
							break;
						}
					}
				}
			}

			return filter_extension;
		}

		void OnClickCancelButton (object sender, EventArgs e)
		{
			if (restoreDirectory)
				mwfFileView.CurrentFolder = restoreDirectoryString;

			CleanupOnClose ();
			
			form.DialogResult = DialogResult.Cancel;
		}
		
		void OnClickHelpButton (object sender, EventArgs e)
		{
			OnHelpRequest (e);
		}
		
		void OnClickSmallButtonToolBar (object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == upToolBarButton) {
				mwfFileView.OneDirUp (CustomFilter);
			} else
			if (e.ClickedItem == backToolBarButton) {
				mwfFileView.PopDir (CustomFilter);
			} else
			if (e.ClickedItem == newdirToolBarButton) {
				mwfFileView.CreateNewFolder ();
			}
		}
		
		void OnSelectedIndexChangedFileTypeComboBox (object sender, EventArgs e)
		{
			if (do_not_call_OnSelectedIndexChangedFileTypeComboBox) {
				do_not_call_OnSelectedIndexChangedFileTypeComboBox = false;
				return;
			}

			UpdateRecentFiles ();

			mwfFileView.FilterIndex = fileTypeComboBox.SelectedIndex + 1;
		}
		
		void OnSelectedFileChangedFileView (object sender, EventArgs e)
		{
			fileNameComboBox.Text = mwfFileView.CurrentFSEntry.Name;
		}
		
		void OnSelectedFilesChangedFileView (object sender, EventArgs e)
		{
			string selectedFiles = mwfFileView.SelectedFilesString;
			if (selectedFiles != null && selectedFiles.Length != 0)
				fileNameComboBox.Text = selectedFiles;
		}
		
		void OnForceDialogEndFileView (object sender, EventArgs e)
		{
			OnClickOpenSaveButton (this, EventArgs.Empty);
		}
		
		void OnDirectoryChangedDirComboBox (object sender, EventArgs e)
		{
			mwfFileView.ChangeDirectory (sender, dirComboBox.CurrentFolder, CustomFilter);
		}
		
		void OnDirectoryChangedPopupButtonPanel (object sender, EventArgs e)
		{
			mwfFileView.ChangeDirectory (sender, popupButtonPanel.CurrentFolder, CustomFilter);
		}
		
		void OnCheckCheckChanged (object sender, EventArgs e)
		{
			ReadOnlyChecked = readonlyCheckBox.Checked;
		}

		void OnFileDialogFormClosed (object sender, FormClosedEventArgs e)
		{
			HandleFormClosedEvent (sender);
		}

		private void OnColumnClickFileView (object sender, ColumnClickEventArgs e)
		{
			if (file_view_comparer == null)
				file_view_comparer = new MwfFileViewItemComparer (true);

			file_view_comparer.ColumnIndex = e.Column;
			file_view_comparer.Ascendent = !file_view_comparer.Ascendent;

			if (mwfFileView.ListViewItemSorter == null)
				mwfFileView.ListViewItemSorter = file_view_comparer;
			else
				mwfFileView.Sort ();
		}

		void HandleFormClosedEvent (object sender)
		{
			if (!disable_form_closed_event)
				OnClickCancelButton (sender, EventArgs.Empty);
			
			disable_form_closed_event = false;
		}
		
		private void UpdateFilters ()
		{
			if (fileFilter == null)
				fileFilter = new FileFilter ();
			
			ArrayList filters = fileFilter.FilterArrayList;
			
			fileTypeComboBox.BeginUpdate ();
			
			fileTypeComboBox.Items.Clear ();
			
			foreach (FilterStruct fs in filters) {
				fileTypeComboBox.Items.Add (fs.filterName);
			}
			
			fileTypeComboBox.EndUpdate ();
			
			mwfFileView.FilterArrayList = filters;
		}

		private void UpdateRecentFiles ()
		{
			fileNameComboBox.Items.Clear ();
			if (configFileNames != null) {
				foreach (string configFileName in configFileNames) {
					if (configFileName == null || configFileName.Trim ().Length == 0)
						continue;
					// add no more than 10 items
					if (fileNameComboBox.Items.Count >= MaxFileNameItems)
						break;
					fileNameComboBox.Items.Add (configFileName);
				}
			}
		}
		
		private void ResizeAndRelocateForHelpOrReadOnly ()
		{
			form.SuspendLayout ();

			int fx = form.Size.Width - form.MinimumSize.Width;
			int fy = form.Size.Height - form.MinimumSize.Height;

			if (!ShowHelp && !ShowReadOnly)
				fy += 29;

			mwfFileView.Size = new Size (450 + fx, 254 + fy);
			fileNameLabel.Location = new Point (101, 298 + fy);
			fileNameComboBox.Location = new Point (195, 298 + fy);
			fileTypeLabel.Location = new Point (101, 326 + fy);
			fileTypeComboBox.Location = new Point (195, 326 + fy);
			openSaveButton.Location = new Point (474 + fx, 298 + fy);
			cancelButton.Location = new Point (474 + fx, 324 + fy);
			helpButton.Location = new Point (474 + fx, 353 + fy);
			readonlyCheckBox.Location = new Point (195, 350 + fy);

			helpButton.Visible = ShowHelp;
			readonlyCheckBox.Visible = ShowReadOnly;
			
			form.ResumeLayout ();
		}
		
		private void WriteConfigValues ()
		{
			MWFConfig.SetValue (filedialog_string, width_string, form.ClientSize.Width);
			MWFConfig.SetValue (filedialog_string, height_string, form.ClientSize.Height);
			MWFConfig.SetValue (filedialog_string, x_string, form.Location.X);
			MWFConfig.SetValue (filedialog_string, y_string, form.Location.Y);
			
			MWFConfig.SetValue (filedialog_string, lastfolder_string, lastFolder);
				
			string[] fileNameCBItems = new string [fileNameComboBox.Items.Count];
				
			fileNameComboBox.Items.CopyTo (fileNameCBItems, 0);
				
			MWFConfig.SetValue (filedialog_string, filenames_string, fileNameCBItems);
		}
		
		private void ReadConfigValues ()
		{
			lastFolder = (string)MWFConfig.GetValue (filedialog_string, lastfolder_string);
			
			if (lastFolder != null && lastFolder.IndexOf ("://") == -1) {
				if (!Directory.Exists (lastFolder)) {
					lastFolder = MWFVFS.DesktopPrefix;
				}
			}
			
			if (InitialDirectory.Length > 0 && Directory.Exists (InitialDirectory))
				lastFolder = InitialDirectory;
			else
				if (lastFolder == null || lastFolder.Length == 0)
					lastFolder = Environment.CurrentDirectory;
			
			if (RestoreDirectory)
				restoreDirectoryString = lastFolder;
		}

		class FileNamesTokenizer
		{
			public FileNamesTokenizer (string text, bool allowMultiple)
			{
				_text = text;
				_position = 0;
				_tokenType = TokenType.BOF;
				_allowMultiple = allowMultiple;
			}

			public TokenType CurrentToken {
				get { return _tokenType; }
			}

			public string TokenText {
				get { return _tokenText; }
			}

			public bool AllowMultiple {
				get { return _allowMultiple; }
			}

			private int ReadChar ()
			{
				if (_position < _text.Length) {
					return _text [_position++];
				} else {
					return -1;
				}
			}

			private int PeekChar ()
			{
				if (_position < _text.Length) {
					return _text [_position];
				} else {
					return -1;
				}
			}

			private void SkipWhitespaceAndQuotes ()
			{
				int ch;

				while ((ch = PeekChar ()) != -1) {
					if ((char) ch != '"' && !char.IsWhiteSpace ((char) ch))
						break;
					ReadChar ();
				}
			}

			public void GetNextFile ()
			{
				if (_tokenType == TokenType.EOF)
					throw new Exception ("");

				int ch;

				SkipWhitespaceAndQuotes ();

				if (PeekChar () == -1) {
					_tokenType = TokenType.EOF;
					return;
				}

				_tokenType = TokenType.FileName;
				StringBuilder sb = new StringBuilder ();

				while ((ch = PeekChar ()) != -1) {
					if ((char) ch == '"') {
						ReadChar ();
						if (AllowMultiple)
							break;
						int pos = _position;

						SkipWhitespaceAndQuotes ();
						if (PeekChar () == -1) {
							break;
						}
						_position = ++pos;
						sb.Append ((char) ch);
					} else {
						sb.Append ((char) ReadChar ());
					}
				}

				_tokenText = sb.ToString ();
			}

			private readonly bool _allowMultiple;
			private int _position;
			private readonly string _text;
			private TokenType _tokenType;
			private string _tokenText;
		}

		internal enum TokenType
		{
			BOF,
			EOF,
			FileName,
		}
	}
	#endregion
}

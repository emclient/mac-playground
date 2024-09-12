using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using Foundation;

namespace Microsoft.WindowsAPICodePack.Taskbar
{
	public interface IJumpListItem
	{
		string Path { get; set; }
	}

    public class JumpListCustomCategory
    {
        private string name;

        internal List<IJumpListItem> JumpListItems
        {
            get;
            private set;
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                    name = value;
            }
        }

        public void AddJumpListItems(params IJumpListItem[] items)
        {
            if (items != null)
                foreach (IJumpListItem item in items)
                    JumpListItems.Add(item);
        }

        public JumpListCustomCategory(string categoryName)
        {
            Name = categoryName;

            JumpListItems = new List<IJumpListItem>();
        }
    }

    public class JumpList
    {
        public static JumpList CreateJumpList()
        {
            return new JumpList();
        }

        public static JumpList CreateJumpListForIndividualWindow(string appId, IntPtr windowHandle)
        {
            return new JumpList();
        }

		private JumpList()
		{
		}

        private readonly object syncLock = new Object();

        #region Properties

		private List<JumpListCustomCategory> customCategoriesCollection;

        public void AddCustomCategories(params JumpListCustomCategory[] customCategories)
        {
            lock (syncLock)
                if (customCategoriesCollection == null)
                    customCategoriesCollection = new List<JumpListCustomCategory>();

            if (customCategories != null)
                foreach (JumpListCustomCategory category in customCategories)
                    customCategoriesCollection.Add(category);
        }

        private List<JumpListTask> userTasks;
        public void AddUserTasks(params JumpListTask[] tasks)
        {
            if (userTasks == null)
                lock (syncLock)
                    if (userTasks == null)
                        userTasks = new List<JumpListTask>();

            if (tasks != null)
                foreach (JumpListTask task in tasks)
                    userTasks.Add(task);
        }

        public void ClearAllUserTasks()
        {
            if (userTasks != null)
                userTasks.Clear();
        }

        public uint MaxSlotsInList
        {
            get
            {
                uint maxSlotsInList = 10; // default
                return maxSlotsInList;
            }
        }

        public JumpListKnownCategoryType KnownCategoryToDisplay { get; set; }

        private int knownCategoryOrdinalPosition;

        public int KnownCategoryOrdinalPosition
        {
            get { return knownCategoryOrdinalPosition; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                knownCategoryOrdinalPosition = value;
            }

        }

        public string ApplicationId { get; private set; }

        #endregion

        internal JumpList(string appID) : this(appID, IntPtr.Zero)
        {
        }

        private JumpList(string appID, IntPtr windowHandle)
        {
        }

        public static void AddToRecent(string destination)
        {
        }

        public void Refresh()
        {
            PostJumplistChanged();
        }

        public event EventHandler<UserRemovedJumpListItemsEventArgs> JumpListItemsRemoved = delegate { };

        public IEnumerable RemovedDestinations
        {
            get { return new ArrayList(); }
        }

		void PostJumplistChanged()
		{
            var jumplist = new NSMutableDictionary();

            var items = new NSMutableArray();
            foreach (var task in this.userTasks)
                if (task is JumpListLink link)
                    AddItem(items, link.Path, link.Title, link.Arguments);


            if (this.customCategoriesCollection != null && customCategoriesCollection.Count != 0)
            {
                foreach (var cat in this.customCategoriesCollection)
                {
                    var catItems = new NSMutableArray();
                    foreach (var si in cat.JumpListItems)
                        if (si is JumpListLink catLink)
                            AddItem(catItems, catLink.Path, catLink.Title, catLink.Arguments);
                    
                    if (items.Count != 0)
                        AddItem(items, "", "-", "");

                    AddItem(items, "", cat.Name, "", catItems);
                }
            }

            jumplist.Add((NSString)"items", items);


            // Let's tell our docktile plugin to update the item list.
            var nc = (NSDistributedNotificationCenter)NSDistributedNotificationCenter.DefaultCenter;
            var identifier = NSBundle.MainBundle.BundleIdentifier + ".JumplistChanged";
            nc.PostNotificationName(identifier, null, jumplist, false);
		}

		void AddItem(NSMutableArray items, string path, string title, string arg, NSMutableArray subItems = null)
		{
			var args = new NSMutableArray();
			args.Add((NSString)arg);

			var item = new NSMutableDictionary();
			item.Add((NSString)"title", (NSString)title);
			item.Add((NSString)"path", (NSString)path);
			item.Add((NSString)"args", args);
            if (subItems != null)
    			item.Add((NSString)"items", subItems);

			items.Add(item);
		}
    }

	public abstract class JumpListTask
	{
	}

    public class JumpListLink : JumpListTask, IJumpListItem, IDisposable
    {
        public JumpListLink(string pathValue, string titleValue)
        {
            if (string.IsNullOrEmpty(pathValue))
                throw new ArgumentNullException("pathValue");

            if (string.IsNullOrEmpty(titleValue))
                throw new ArgumentNullException("titleValue");

            Path = pathValue;
            Title = titleValue;
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                title = value;
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                path = value;
            }
        }

        public IconReference IconReference { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        public WindowShowCommand ShowCommand { get; set; }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                title = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~JumpListLink()
        {
            Dispose(false);
        }

        #endregion
    }

    public class TaskbarManager
    {
        // Hide the default constructor
        private TaskbarManager()
        {
        }

        // Best practice recommends defining a private object to lock on
        private static object _syncLock = new object();

        private static TaskbarManager _instance;
        public static TaskbarManager Instance
        {
            get
            {
                if (_instance == null)
                    lock (_syncLock)
                        if (_instance == null)
                            _instance = new TaskbarManager();

                return _instance;
            }
        }
        public void SetOverlayIcon(System.Drawing.Icon icon, string accessibilityText)
        {
        }

        public void SetOverlayIcon(IntPtr windowHandle, System.Drawing.Icon icon, string accessibilityText)
        {
        }

        public void SetProgressValue(int currentValue, int maximumValue)
        {
        }

        public void SetProgressValue(int currentValue, int maximumValue, IntPtr windowHandle)
        {
        }

        public void SetProgressState(TaskbarProgressBarState state)
        {
        }
        public void SetProgressState(TaskbarProgressBarState state, IntPtr windowHandle)
        {
        }

        private TabbedThumbnailManager _tabbedThumbnail;
        public TabbedThumbnailManager TabbedThumbnail
        {
            get
            {
                if (_tabbedThumbnail == null)
                    _tabbedThumbnail = new TabbedThumbnailManager();
                return _tabbedThumbnail;
            }
        }

        private ThumbnailToolBarManager _thumbnailToolBarManager;
        public ThumbnailToolBarManager ThumbnailToolBars
        {
            get
            {
                if (_thumbnailToolBarManager == null)
                    _thumbnailToolBarManager = new ThumbnailToolBarManager();

                return _thumbnailToolBarManager;
            }
        }

        public string ApplicationId
        {
            get { return GetCurrentProcessAppId(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                SetCurrentProcessAppId(value);
                ApplicationIdSetProcessWide = true;
            }
        }

        private IntPtr _ownerHandle;
        internal IntPtr OwnerHandle
        {
            get
            {
                if (_ownerHandle == IntPtr.Zero)
                {
                    Process currentProcess = Process.GetCurrentProcess();

                    if (currentProcess == null || currentProcess.MainWindowHandle == IntPtr.Zero)
                        throw new InvalidOperationException();

                    _ownerHandle = currentProcess.MainWindowHandle;
                }

                return _ownerHandle;
            }
        }
        public void SetApplicationIdForSpecificWindow(IntPtr windowHandle, string appId)
        {
        }

        private void SetCurrentProcessAppId(string appId)
        {
        }

        private string GetCurrentProcessAppId()
        {
            string appId = string.Empty;
            return appId;
        }

        internal bool ApplicationIdSetProcessWide { get; private set; }

        public static bool IsPlatformSupported
        {
            get { return true; }
        }
    }

    public class TabbedThumbnailManager
    {
        public void AddThumbnailPreview(TabbedThumbnail preview)
        {
        }

        public TabbedThumbnail GetThumbnailPreview(IntPtr windowHandle)
        {
            return null;
        }

        public TabbedThumbnail GetThumbnailPreview(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            return GetThumbnailPreview(control.Handle);
        }
        public void RemoveThumbnailPreview(TabbedThumbnail preview)
        {
        }

        public void RemoveThumbnailPreview(IntPtr windowHandle)
        {
        }

        public void RemoveThumbnailPreview(Control control)
        {
        }

        public void SetActiveTab(TabbedThumbnail preview)
        {
        }

        public void SetActiveTab(IntPtr windowHandle)
        {
        }

        public void SetActiveTab(Control control)
        {
        }

        public bool IsThumbnailPreviewAdded(TabbedThumbnail preview)
        {
            return false;
        }
        public bool IsThumbnailPreviewAdded(IntPtr windowHandle)
        {
            return false;
        }

        public bool IsThumbnailPreviewAdded(Control control)
        {
            return false;
        }
        public void InvalidateThumbnails()
        {
        }

        public static void ClearThumbnailClip(IntPtr windowHandle)
        {
        }

        public void SetThumbnailClip(IntPtr windowHandle, Rectangle? clippingRectangle)
        {
        }

        public static void SetTabOrder(TabbedThumbnail previewToChange, TabbedThumbnail insertBeforePreview)
        {
        }
    }

    public class TabbedThumbnail
    {
    }

    public class ThumbnailToolBarManager
    {
    }

    public enum FileDialogAddPlaceLocation
    {
        Bottom = 0x00000000,
        Top = 0x00000001,
    }

    public class UserRemovedJumpListItemsEventArgs : EventArgs
    {
        private readonly IEnumerable _removedItems;

        internal UserRemovedJumpListItemsEventArgs(IEnumerable RemovedItems)
        {
            _removedItems = RemovedItems;
        }

        public IEnumerable RemovedItems
        {
            get { return _removedItems; }
        }
    }

    public enum DisplayNameType
    {
        Default = 0x00000000,
        RelativeToParent = unchecked((int)0x80018001),
        RelativeToParentAddressBar = unchecked((int)0x8007c001),
        RelativeToDesktop = unchecked((int)0x80028000),
        RelativeToParentEditing = unchecked((int)0x80031001),
        RelativeToDesktopEditing = unchecked((int)0x8004c000),
        FileSystemPath = unchecked((int)0x80058000),
        Url = unchecked((int)0x80068000),
    }

    public enum LibraryFolderType
    {
        Generic = 0,
        Documents,
        Music,
        Pictures,
        Videos
    }

    public enum WindowShowCommand
    {
        Hide = 0,
        Normal = 1,
        Minimized = 2,
        Maximized = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinimizedNoActivate = 7,
        ShowNA = 8,
        Restore = 9,
        Default = 10,
        ForceMinimize = 11
    }
    public enum SearchConditionOperation
    {
        Implicit = 0,
        Equal = 1,
        NotEqual = 2,
        LessThan = 3,
        GreaterThan = 4,
        LessThanOrEqual = 5,
        GreaterThanOrEqual = 6,
        ValueStartsWith = 7,
        ValueEndsWith = 8,
        ValueContains = 9,
        ValueNotContains = 10,
        DosWildcards = 11,
        WordEqual = 12,
        WordStartsWith = 13,
        ApplicationSpecific = 14
    }

    public enum SearchConditionType
    {
        And = 0,
        Or = 1,
        Not = 2,
        Leaf = 3,
    }

    public enum FolderLogicalViewMode
    {
        Unspecified = -1,
        None = 0,
        First = 1,
        Details = 1,
        Tiles = 2,
        Icons = 3,
        List = 4,
        Content = 5,
        Last = 5
    }

    public enum SortDirection
    {
        Default = 0,
        Descending = -1,
        Ascending = 1,
    }
    public enum StructuredQuerySingleOption
    {
        Schema,
        Locale,
        WordBreaker,
        NaturalSyntax,
        AutomaticWildcard,
        TraceLevel,
        LanguageKeywords,
        Syntax,
        TimeZone,
        ImplicitConnector,
        ConnectorCase,
    }

    public enum StructuredQueryMultipleOption
    {
        VirtualProperty,
        DefaultProperty,
        GeneratorForType,
        MapProperty,
    }
    public enum QueryParserManagerOption
    {
        SchemaBinaryName = 0,
        PreLocalizedSchemaBinaryPath = 1,
        UnlocalizedSchemaBinaryPath = 2,
        LocalizedSchemaBinaryPath = 3,
        AppendLCIDToLocalizedPath = 4,
        LocalizerSupport = 5
    }
    public enum JumpListKnownCategoryType
    {
        Neither = 0,
        Recent,
        Frequent,
    }
    public enum TaskbarProgressBarState
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }
}

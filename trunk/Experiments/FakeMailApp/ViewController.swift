//
//  ViewController.swift
//  FakeMailApp
//
//  Created by Jiri Volejnik on 06/10/15.
//  Copyright (c) 2015 eM Client. All rights reserved.
//

import Cocoa
import ServiceManagement

class ViewController: NSViewController, NSTableViewDataSource, NSTableViewDelegate {

    @IBOutlet var urlTextField:NSTextField!
    @IBOutlet var availableAppsTableView:NSTableView!
    @IBOutlet var defaultAppTextField:NSTextField!
    @IBOutlet var newDefaultAppTextField:NSTextField!

    @IBOutlet var startupAppsTableView:NSTableView!
    @IBOutlet var startupAppLaunchAgentFilename:NSTextField!
    @IBOutlet var startupAppExecutablePath:NSTextField!
    
    
    var urls = [NSURL]()
    var defaultAppUrl:NSURL?

    var startupApps = [String]()
    
    var firstLoad = true
    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view.
        if firstLoad {
            firstLoad = false
            
            checkIsDefault(self)
            reloadStartupApps(self)
        }
    }

    override var representedObject: AnyObject? {
        didSet {
        // Update the view, if already loaded.
        }
    }
    
    @IBAction func quit(sender:AnyObject?) {
        NSApplication.sharedApplication().terminate(sender)
    }

    @IBAction func checkIsDefault(sender:AnyObject?) {

        // Default mailto: app
        let url = NSURL(string: self.urlTextField.stringValue) // NSURL(string: "mailto:")

        self.defaultAppUrl = defaultApplicationUrlForUrl(url!)
        self.defaultAppTextField.stringValue = defaultAppUrl != nil ? defaultAppUrl!.absoluteString : ""
        
        // all mailto: apps

        self.urls = applicationURLsForURL(url!)
        self.availableAppsTableView.reloadData()
    }

    @IBAction func setDefault(sender:AnyObject?) {
        
        let urlString = self.urlTextField.stringValue
        let url = NSURL(string: urlString)
        let newAppUrlString = newDefaultAppTextField.stringValue
        let appUrl = NSURL(string:newAppUrlString)
        
        setDefaultApplicationURLForURL(url!, appURL: appUrl!)
    }
    
    @IBAction func register(sender:AnyObject?) {
        registerThisApp()
    }

    @IBAction func launchDefaultApp(sender:AnyObject?) {
        let url = NSURL(string: self.urlTextField.stringValue)
        launchDefaultAppForURL(url!)
    }
    
    @IBAction func this(sender:AnyObject?) {
        self.newDefaultAppTextField.stringValue = NSBundle.mainBundle().bundleURL.absoluteString
    }
    
    func numberOfRowsInTableView(tableView: NSTableView) -> Int
    {
        switch tableView.identifier ?? "" {
            case "DefaultApps": return numberOfRowsInTableView_DefaultApps(tableView)
            case "StartupApps": return numberOfRowsInTableView_StartupApps(tableView)
            default: return 0
        }
    }
    
    func tableView(tableView: NSTableView, viewForTableColumn tableColumn: NSTableColumn?, row: Int) -> NSView? {
        switch tableView.identifier ?? "" {
        case "DefaultApps": return tableView_DefaultApps(tableView, viewForTableColumn:tableColumn, row: row)
        case "StartupApps": return tableView_StartupApps(tableView, viewForTableColumn:tableColumn, row: row)
        default: return nil
        }
    }

    func numberOfRowsInTableView_DefaultApps(tableView: NSTableView) -> Int {
        return urls.count
    }

    func tableView_DefaultApps(tableView: NSTableView, viewForTableColumn tableColumn: NSTableColumn?, row: Int) -> NSView? {
        let result = tableView.makeViewWithIdentifier("MyView", owner:self) as! NSTableCellView
        result.textField!.stringValue = self.urls[row].absoluteString
        return result
    }

    
    func tableViewSelectionDidChange(notification: NSNotification) {
        switch (notification.object! as! NSTableView).identifier ?? "" {
        case "DefaultApps": tableViewSelectionDidChange_DefaultApps(notification)
        case "StartupApps": tableViewSelectionDidChange_StartupApps(notification)
        default: break
        }
    }

    func tableViewSelectionDidChange_DefaultApps(notification: NSNotification) {
        
        let tableView = notification.object as! NSTableView
        let index = tableView.selectedRow
        if index >= 0 && index < urls.count {
            self.newDefaultAppTextField.stringValue = urls[index].absoluteString
        } else {
            self.newDefaultAppTextField.stringValue = ""
        }
    }
    
    func defaultApplicationUrlForUrl(url:NSURL) -> NSURL? {
        let error = UnsafeMutablePointer<Unmanaged<CFError>?>()
        let result = LSCopyDefaultApplicationURLForURL(url, LSRolesMask.All, error)
        return result != nil ? result!.takeRetainedValue() as NSURL : nil
    }
    
    func applicationURLsForURL(url:NSURL) -> Array<NSURL> {
        var result = Array<NSURL>()
        if let array = LSCopyApplicationURLsForURL(url, LSRolesMask.All)?.takeRetainedValue() {
            for var i = 0; i < CFArrayGetCount(array); ++i {
                let ptr = CFArrayGetValueAtIndex(array, i)
                let url:NSURL = unsafeBitCast(ptr, NSURL.self)
                result.append(url)
            }
        }
        return result
    }
    
    func setDefaultApplicationURLForURL(url:NSURL, appURL:NSURL)
    {
        if let bundle = NSBundle(URL: appURL) {
            let result = LSSetDefaultHandlerForURLScheme(url.scheme, bundle.bundleIdentifier!)
            if result == 0 {
                checkIsDefault(nil)
            } else {
                print("Failed setting default handler for \(url.scheme) to \(appURL.absoluteString)")
            }
            
        }
    }
    
    func registerAppURL(url:NSURL) {
        let status = LSRegisterURL(url, true)
        print("LSRegisterURL(\(url.absoluteString)) -> \(status)")
    }
    
    func registerThisApp() {
        registerAppURL(NSBundle.mainBundle().bundleURL)
    }

    func launchDefaultAppForURL(url:NSURL) {
        let ptr = UnsafeMutablePointer<Unmanaged<CFURL>?>.alloc(1)
        let status = LSOpenCFURLRef(url, ptr)
        let launchedURL:NSURL? = ptr.memory?.takeRetainedValue()
        let launchedURLStr:String = launchedURL != nil ? launchedURL!.absoluteString : "-"
        print("LSOpenCFURLRef(\(url.absoluteString)) -> \(status), \(launchedURLStr)")
    }

    
    //https://developer.apple.com/library/mac/documentation/MacOSX/Conceptual/BPSystemStartup/Chapters/CreatingLoginItems.html#//apple_ref/doc/uid/10000172i-SW5-SW1
    
    // -- Startup Apps -----------------------

    @IBAction func reloadStartupApps(sender:AnyObject?) {
        var apps = [String]()
        enumerateLaunchAgents { (path) -> Bool in
            if self.isStartupLaunchAgent(path) {
                apps.append(path)
            }
            return true
        }
        self.startupApps = apps
        
        startupAppsTableView.reloadData()
    }
    
    @IBAction func addStartupApp(sender:AnyObject?) {
        addStartupLaunchAgentForAppPath(self.startupAppExecutablePath.stringValue, fileName:self.startupAppLaunchAgentFilename.stringValue)
        reloadStartupApps(self)
    }

    @IBAction func removeSelectedStartupApp(sender:AnyObject?) {
        let row = startupAppsTableView.selectedRow
        if row >= 0 && row < startupApps.count {
            let path = startupApps[row]
            startupApps.removeAtIndex(row)
            
            let fileManager = NSFileManager.defaultManager()
            if fileManager.fileExistsAtPath(path) {
                do {
                    try fileManager.removeItemAtPath(path)
                } catch {
                    // failed deleting file
                }
            }
        }
        
        startupAppsTableView.reloadData()
    }
    
    @IBAction func enterParamsForThisStartupApp(sender:AnyObject?) {
        self.startupAppExecutablePath.stringValue = NSBundle.mainBundle().executablePath!
    }
    
    @IBAction func chooseStartupApp(sender:AnyObject?) {
        let dlg = NSOpenPanel()
        dlg.allowsMultipleSelection = false
        dlg.showsHiddenFiles = true
        dlg.resolvesAliases = true
        dlg.treatsFilePackagesAsDirectories = true
        if NSFileHandlingPanelOKButton == dlg.runModal() {
            self.startupAppExecutablePath.stringValue = dlg.URL!.path!
        }
    }

    func numberOfRowsInTableView_StartupApps(tableView: NSTableView) -> Int {
        return startupApps.count
    }

    func tableView_StartupApps(tableView: NSTableView, viewForTableColumn tableColumn: NSTableColumn?, row: Int) -> NSView? {
        let result = tableView.makeViewWithIdentifier("MyView", owner:self) as! NSTableCellView
        result.textField!.stringValue = self.startupApps[row]
        return result
    }
    
    func tableViewSelectionDidChange_StartupApps(notification: NSNotification) {
//        let tableView = notification.object as! NSTableView
//        let index = tableView.selectedRow
//        if index >= 0 && index < urls.count {
//            self.newDefaultAppTextField.stringValue = urls[index].absoluteString
//        } else {
//            self.newDefaultAppTextField.stringValue = ""
//        }
    }
    
    func addStartupLaunchAgentForAppPath(appPath:String, fileName:String, label:String? = nil) {
        let d = createLaunchAgentDictionaryForAppPath(appPath, label: label ?? fileName)
        if let launchAgentPath = createLaunchAgentPathForFilename(fileName) {
            (d as NSDictionary).writeToFile(launchAgentPath, atomically: true)
        }
    }

    func createLaunchAgentPathForFilename(filename:String, var maxAttempts:Int = 1000) -> String? {
        let folders = NSSearchPathForDirectoriesInDomains(.LibraryDirectory, .UserDomainMask, true)
        maxAttempts = maxAttempts <= 0 ? 1000 : maxAttempts
        for folder in folders {
            let folderURL = NSURL(fileURLWithPath: folder).URLByAppendingPathComponent("LaunchAgents")
            var suffix = ""
            for var i = 1; i <= maxAttempts; ++i {
                let agentPath = folderURL.URLByAppendingPathComponent(filename + suffix + ".plist").path!
                if !NSFileManager.defaultManager().fileExistsAtPath(agentPath) {
                    return agentPath
                }
                suffix = " \(i)"
            }
        }
        
        return nil
    }

    func isStartupApp(appPath:String) -> Bool {

        // Look for .plist file in the ~/Library/LaunchAgents/ that points to this app.
        var found = false
        enumerateLaunchAgents { (path) -> Bool in
            if self.isStartupLaunchAgent(path) {
                found = true
            }
            return !found
        }
        return found
    }
    
    func isStartupLaunchAgent(path:String, forExecutable:String? = nil) -> Bool {

        if !path.endsWithIgnoreCase(".plist") {
            return false
        }
        
        if let d = NSDictionary(contentsOfFile: path) as? [String:AnyObject] {
        
            if !(d["RunAtLoad"] as? Bool == true) {
                return false
            }
            
            if (forExecutable != nil) {
                if let args = d["ProgramArguments"] as? [String] {
                    if args[0] != forExecutable {
                        return false
                    }
                }
            }
            
            // ..
            return true
        }
        return false
    }

    func createLaunchAgentDictionaryForAppPath(executablePath:String, label:String) -> [String:AnyObject] {

        var args = [String]()
        args.append(executablePath)
        
        var d = [String:AnyObject]()
        d["Label"] = label
        d["ProgramArguments"] = args
        d["LimitLoadToSessionType"] = "Aqua"
        d["RunAtLoad"] = true
        //d["StandardErrorPath"] = "/dev/null"
        //d["StandardOutPath"] = "/dev/null"
        
        return d
    }
    
    func enumerateLaunchAgents(callback: ((_:String) -> Bool)) {
        
        let fileManager = NSFileManager.defaultManager()
        let folders = NSSearchPathForDirectoriesInDomains(.LibraryDirectory, .UserDomainMask, true)
        for folder in folders {
            let launchAgentsFolderURL = NSURL(fileURLWithPath: folder).URLByAppendingPathComponent("LaunchAgents")
            if let paths = fileManager.enumeratorAtPath(launchAgentsFolderURL.path!) {
                while let subpath = (paths.nextObject() as? String) {
                    if !callback(launchAgentsFolderURL.URLByAppendingPathComponent(subpath).path!) {
                        return
                    }
                }
            }
        }
    }
    
}
        
// -----------------------

extension String {
    init (fourCharCode:UInt32) {
        self.init()
        for var i = 3; i >= 0; --i {
            self.append(UnicodeScalar(UInt32((fourCharCode >> (UInt32(i) * 8)) & 0xff)))
        }
    }
    
    init(lsItemInfoFlags value:LSItemInfoFlags) {
        
        self.init()

        let flags = [
            LSItemInfoFlags.IsPlainFile,
            LSItemInfoFlags.IsPackage,
            LSItemInfoFlags.IsApplication,
            LSItemInfoFlags.IsContainer,
            LSItemInfoFlags.IsAliasFile,
            LSItemInfoFlags.IsSymlink,
            LSItemInfoFlags.IsInvisible,
            LSItemInfoFlags.IsNativeApp,
            LSItemInfoFlags.IsClassicApp,
            LSItemInfoFlags.AppPrefersNative,
            LSItemInfoFlags.AppPrefersClassic,
            LSItemInfoFlags.AppIsScriptable,
            LSItemInfoFlags.IsVolume,
            LSItemInfoFlags.ExtensionIsHidden,
        ]
        
        let names = [
            "IsPlainFile",
            "IsPackage",
            "IsApplication",
            "IsContainer",
            "IsAliasFile",
            "IsSymlink",
            "IsInvisible",
            "IsNativeApp",
            "IsClassicApp",
            "AppPrefersNative",
            "AppPrefersClassic",
            "AppIsScriptable",
            "IsVolume",
            "ExtensionIsHidden",
        ]
        
        var sep = ""
        for var i = 0; i < flags.count; ++i {
            if value.contains(flags[i]) {
                self.appendContentsOf(sep + names[i])
                sep = "|"
            }
        }
    }

    func beginsWith (str: String) -> Bool {
        if let range = self.rangeOfString(str) {
            return range.startIndex == self.startIndex
        }
        return false
    }
    
    func endsWith (str: String) -> Bool {
        if let range = self.rangeOfString(str, options:NSStringCompareOptions.BackwardsSearch) {
            return range.endIndex == self.endIndex
        }
        return false
    }
    
    func endsWithIgnoreCase (str: String) -> Bool {
        let options = NSStringCompareOptions.BackwardsSearch.rawValue | NSStringCompareOptions.CaseInsensitiveSearch.rawValue
        if let range = self.rangeOfString(str, options:NSStringCompareOptions(rawValue: options)) {
            return range.endIndex == self.endIndex
        }
        return false
    }
}


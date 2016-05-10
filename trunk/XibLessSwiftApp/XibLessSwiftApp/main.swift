import Cocoa


class AppDelegate: NSObject, NSApplicationDelegate {

    var createWindowWhenLaunched:Bool
    var window:NSWindow? = nil // prevents destroying the window
    
    init(createWindowWhenLaunched:Bool)
    {
        self.createWindowWhenLaunched = createWindowWhenLaunched
    }
    
    func applicationDidFinishLaunching(notification: NSNotification) {
        print("applicationDidFinishLaunching")
        if createWindowWhenLaunched {
            self.window = Program.windowWithTitle("Window")
            window?.makeKeyAndOrderFront(window)
        }
    }
}

class Program {

    static let app:NSApplication = NSApplication.sharedApplication()
    
    static func runNative()
    {
        let delegate = AppDelegate(createWindowWhenLaunched: true)
        app.delegate = delegate

        if app.activationPolicy() != .Regular {
            atexit_b { app.setActivationPolicy(app.activationPolicy()); return }
            app.setActivationPolicy(.Regular)
            app.activateIgnoringOtherApps(true)
        }
        
        app.menu = Program.CreateMenu()

        app.run()
    }
    
    static func runCustom()
    {
        let delegate = AppDelegate(createWindowWhenLaunched: false)
        app.delegate = delegate

        if app.activationPolicy() != .Regular {
            atexit_b { app.setActivationPolicy(app.activationPolicy()); return }
            app.setActivationPolicy(.Regular)
            app.activateIgnoringOtherApps(true)
        }
        
        app.menu = Program.CreateMenu()

        app.finishLaunching()
        
        Program.runModalWindow(Program.windowWithTitle("Modal"))
        
        Program.windowWithTitle("Normal").makeKeyAndOrderFront(app)
        Program.run()
    }

    static func windowWithTitle(title:String) -> NSWindow {
        let style = NSTitledWindowMask | NSClosableWindowMask | NSResizableWindowMask
        let window = NSWindow(contentRect:NSMakeRect(0, 0, 480, 320), styleMask:style, backing:.Buffered, `defer`:false)
        window.title = title
        return window
    }

    static func CreateMenu() -> NSMenu {
        let tree = [
            "Apple": [
                NSMenuItem(title: "Hide",  action: "hide:", keyEquivalent:"h"),
                NSMenuItem(title: "Hide Others",  action: "hideOtherApplications:", keyEquivalent:"o"),
                NSMenuItem(title: "Quit",  action: "terminate:", keyEquivalent:"q"),
            ],
        ]
        let result = NSMenu(title: "MainMenu")
        for (title, items) in tree {
            let menu = NSMenu(title: title)
            if let item = result.addItemWithTitle(title, action: nil, keyEquivalent:"") {
                result.setSubmenu(menu, forItem: item)
                for item in items {
                    menu.addItem(item)
                }
            }
        }
        return result
    }
    
    static func run() {
        repeat
        {
            let mask = Int.init(truncatingBitPattern: NSEventMask.AnyEventMask.rawValue)
            let event = app.nextEventMatchingMask(mask, untilDate: NSDate.distantFuture(), inMode: NSDefaultRunLoopMode, dequeue: true)
            if event == nil {
                break
            }
            app.sendEvent(event!)
        } while true
    }
    
    static func runModalWindow(theWindow: NSWindow) {

        let session = app.beginModalSessionForWindow(theWindow)

        repeat
        {
            let mask = Int.init(truncatingBitPattern: NSEventMask.AnyEventMask.rawValue)
            let event = app.nextEventMatchingMask(mask, untilDate: NSDate.distantFuture(), inMode: NSDefaultRunLoopMode, dequeue: true)
            if event != nil {
                app.sendEvent(event!)
                if !theWindow.visible {
                    break
                }
            }
        } while true

        app.endModalSession(session)
    }
    
}

//Program.runNative()
Program.runCustom()

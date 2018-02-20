import Cocoa


class AppDelegate: NSObject, NSApplicationDelegate {

    var createWindowWhenLaunched:Bool
    var window:NSWindow? = nil // prevents destroying the window
    
    init(createWindowWhenLaunched:Bool)
    {
        self.createWindowWhenLaunched = createWindowWhenLaunched
    }
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        print("applicationDidFinishLaunching")
        if createWindowWhenLaunched {
            self.window = Program.windowWithTitle("Window")
            window?.makeKeyAndOrderFront(window)
        }
    }
}

class Program {

    static let app:NSApplication = NSApplication.shared()
    
    static func runNative()
    {
        let delegate = AppDelegate(createWindowWhenLaunched: true)
        app.delegate = delegate

        if app.activationPolicy() != .regular {
            atexit_b { app.setActivationPolicy(app.activationPolicy()); return }
            app.setActivationPolicy(.regular)
            app.activate(ignoringOtherApps: true)
        }
        
        app.menu = Program.CreateMenu()

        app.run()
    }
    
    static func runCustom()
    {
        let delegate = AppDelegate(createWindowWhenLaunched: false)
        app.delegate = delegate

        if app.activationPolicy() != .regular {
            atexit_b { app.setActivationPolicy(app.activationPolicy()); return }
            app.setActivationPolicy(.regular)
            app.activate(ignoringOtherApps: true)
        }
        
        app.menu = Program.CreateMenu()

        app.finishLaunching()
        
        Program.runModalWindow(Program.windowWithTitle("Modal"))
        
        Program.windowWithTitle("Normal").makeKeyAndOrderFront(app)
        Program.run()
    }

    static func windowWithTitle(_ title:String) -> NSWindow {
        let style = NSTitledWindowMask.rawValue | NSClosableWindowMask.rawValue | NSResizableWindowMask.rawValue
        let window = NSWindow(contentRect:NSMakeRect(0, 0, 480, 320), styleMask:NSWindowStyleMask(rawValue: style), backing:.buffered, defer:false)
        window.title = title
        return window
    }

    static func CreateMenu() -> NSMenu {
        let tree = [
            "Apple": [
                NSMenuItem(title: "Hide",  action: #selector(NSApplication.hide(_:)), keyEquivalent:"h"),
                NSMenuItem(title: "Hide Others",  action: #selector(NSApplication.hideOtherApplications(_:)), keyEquivalent:"o"),
                //NSMenuItem(title: "Quit",  action: #selector(NSInputServiceProvider.terminate(_:)), keyEquivalent:"q"),
                NSMenuItem(title: "Quit",  action: #selector(NSApp.terminate(_:)), keyEquivalent:"q"),
            ],
        ]
        let result = NSMenu(title: "MainMenu")
        for (title, items) in tree {
            let menu = NSMenu(title: title)
            let item = result.addItem(withTitle: title, action: nil, keyEquivalent:"")
            result.setSubmenu(menu, for: item)
            for item in items {
                menu.addItem(item)
            }
        }
        return result
    }
    
    static func run() {
        repeat
        {
            let mask = UInt.init(truncatingBitPattern: NSEventMask.any.rawValue)
            let event = app.nextEvent(matching: NSEventMask(rawValue: UInt64(mask)), until: Date.distantFuture, inMode: RunLoopMode.defaultRunLoopMode, dequeue: true)
            if event == nil {
                break
            }
            app.sendEvent(event!)
        } while true
    }
    
    static func runModalWindow(_ theWindow: NSWindow) {

        let session = app.beginModalSession(for: theWindow)

        repeat
        {
            let mask = UInt.init(truncatingBitPattern: NSEventMask.any.rawValue)
            let event = app.nextEvent(matching: NSEventMask(rawValue: UInt64(mask)), until: Date.distantFuture, inMode: RunLoopMode.defaultRunLoopMode, dequeue: true)
            if event != nil {
                app.sendEvent(event!)
                if !theWindow.isVisible {
                    break
                }
            }
        } while true

        app.endModalSession(session)
    }
    
}

//Program.runNative()
Program.runCustom()

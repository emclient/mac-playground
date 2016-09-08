//
//  AppDelegate.swift
//  FakeMailApp
//
//  Created by Jiri Volejnik on 06/10/15.
//  Copyright (c) 2015 eM Client. All rights reserved.
//

import Cocoa

@NSApplicationMain
class AppDelegate: NSObject, NSApplicationDelegate {

    func applicationWillFinishLaunching(aNotification: NSNotification) {
        registerUrlHandler()
    }
    
    func registerUrlHandler() {
        let appleEventManager = NSAppleEventManager.sharedAppleEventManager()
        appleEventManager.setEventHandler(self, andSelector: Selector("handleGetURLEvent:replyEvent:"), forEventClass: AEEventClass(kInternetEventClass), andEventID: AEEventID(kAEGetURL))
    }
    
    @objc func handleGetURLEvent(event: NSAppleEventDescriptor?, replyEvent: NSAppleEventDescriptor?) {
        NSLog("%@", (event?.descriptorForKeyword(AEKeyword(keyDirectObject))?.stringValue)!)
    }
    
}


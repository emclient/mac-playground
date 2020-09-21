//
//  main.m
//  Backup
//
//  Created by Jiri Volejnik on 12/03/2020.
//  Copyright © 2020 eM Client. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "AppDelegate.h"

int main(int argc, const char * argv[]) {
    @autoreleasepool {
        // Setup code that might create autoreleased objects goes here.
    }

    AppDelegate* delegate = [[AppDelegate alloc] init];
    NSApplication.sharedApplication.delegate = delegate;

    return NSApplicationMain(argc, argv);
}

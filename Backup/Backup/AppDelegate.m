//
//  AppDelegate.m
//  Backup
//
//  Created by Jiri Volejnik on 12/03/2020.
//  Copyright © 2020 eM Client. All rights reserved.
//

#import "AppDelegate.h"

@interface AppDelegate ()

@end

@implementation AppDelegate

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    
    if ([self isEmClientRunning]) {
        [self tellEmClientToBackup];
    } else {
        [self launchEmClient];
    }
}

-(BOOL)isEmClientRunning {
    NSArray* emclients = [NSRunningApplication runningApplicationsWithBundleIdentifier:@"com.emclient.mail.client"];
    return emclients != nil && emclients.count != 0;
}

-(void)tellEmClientToBackup {
    NSURL* url = [NSURL URLWithString:@"emclient:backup"];
    [self openURL:url args:nil terminate:true];
}

- (void)launchEmClient {
    NSMutableArray* arguments = [[[NSProcessInfo processInfo] arguments] mutableCopy];
    [arguments removeObjectAtIndex:0];
    [arguments insertObject:@"/dbbackup" atIndex:0];
    
    NSString* backup = [[NSBundle mainBundle] bundlePath];
    NSString* helpers = [backup stringByDeletingLastPathComponent];
    NSString* contents = [helpers stringByDeletingLastPathComponent];
    NSString* emclient = [contents stringByDeletingLastPathComponent];
    
//    emclient = @"/Users/jirkav/Projects/emclient/bin/Xamarin - Debug/eM Client.app";
//    arguments = @[@"/dbbackup", @"-backup", @"-silent" ];
    
    NSURL* url = [NSURL fileURLWithPath:emclient];
    [self openURL:url args:arguments terminate:true];
}

-(void)openURL:(NSURL*)url args:(NSArray*)args terminate:(BOOL)terminate {
    if (@available(macOS 10.15, *)) {
        NSWorkspaceOpenConfiguration* conf = [NSWorkspaceOpenConfiguration configuration];
        if (args != nil) {
            conf.arguments = args;
        }
        [NSWorkspace.sharedWorkspace openURL:url configuration:conf completionHandler:^(NSRunningApplication *app, NSError *error) {
//            NSLog(@"%@", error);
            dispatch_async(dispatch_get_main_queue(), ^{
                [NSApplication.sharedApplication terminate:self];
            });
        }];
    } else {
        NSError* error;
        NSDictionary* conf = nil;
        if (args != nil) {
            conf = [NSDictionary dictionaryWithObject:args forKey:NSWorkspaceLaunchConfigurationArguments];
        }
        [NSWorkspace.sharedWorkspace openURL:url options:NSWorkspaceLaunchDefault configuration:conf error:&error];
//        NSLog(@"%@", error);
        [NSApplication.sharedApplication terminate:self];
    }
}

@end

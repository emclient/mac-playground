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
    
    [self initLogging];
    
    if ([self isEmClientRunning]) {
        [self tellEmClientToBackup];
    } else {
        [self launchEmClient];
    }
}

-(BOOL)isEmClientRunning {

    NSArray* args = [[NSProcessInfo processInfo] arguments];
    NSString* bundleIdentifier = [self getValueFromArray:args following:@"-bundleIdentifier" defaultValue:@"com.emclient.mail.client"];

    NSArray* emclients = [NSRunningApplication runningApplicationsWithBundleIdentifier:bundleIdentifier];
    return emclients != nil && emclients.count != 0;
}

-(void)tellEmClientToBackup {
    // If emclient/iwdc is running, invoke backup by opening url with registerd scheme.
    NSArray* args = [[NSProcessInfo processInfo] arguments];
    NSString* scheme = [self getValueFromArray:args following:@"-scheme" defaultValue:@"emclient"];
    NSString* str = [NSString stringWithFormat:@"%@:backup", scheme];
    NSURL* url = [NSURL URLWithString:str];
    NSLog(@"Starting backup by opening URL with custom registered scheme: %@", url);
    [self openURL:url args:nil terminate:true];
}

- (void)launchEmClient {
    // If emclient/iwdc is not running, invoke backup by launching the app with command line arguments.
    NSMutableArray* arguments = [[[NSProcessInfo processInfo] arguments] mutableCopy];
    
    NSInteger indexOfArgsOption = [arguments indexOfObject:@"-args"];
    if (indexOfArgsOption == NSNotFound) {
        NSLog(@"Failed to locate \"-args\" option. Forwarding arguments starting at index 1");
        indexOfArgsOption = 0;
    }
    
    [arguments removeObjectsInRange:NSMakeRange(0, 1 + indexOfArgsOption)];
    [arguments insertObject:@"/dbbackup" atIndex:0];
    
    NSString* backup = [[NSBundle mainBundle] bundlePath];
    NSString* helpers = [backup stringByDeletingLastPathComponent];
    NSString* contents = [helpers stringByDeletingLastPathComponent];
    NSString* emclient = [contents stringByDeletingLastPathComponent];
    
//    emclient = @"/Users/jirkav/Projects/emclient/bin/Xamarin - Debug/eM Client.app";
//    arguments = @[@"/dbbackup", @"-backup", @"-silent" ];
    
    NSURL* url = [NSURL fileURLWithPath:emclient];
    NSLog(@"Launching app by opening URL: %@, args:%@", url, arguments);
    [self openURL:url args:arguments terminate:true];
}

-(void)openURL:(NSURL*)url args:(NSArray*)args terminate:(BOOL)terminate {
    if (@available(macOS 10.15, *)) {
        NSWorkspaceOpenConfiguration* conf = [NSWorkspaceOpenConfiguration configuration];
        if (args != nil) {
            conf.arguments = args;
        }
        [NSWorkspace.sharedWorkspace openURL:url configuration:conf completionHandler:^(NSRunningApplication *app, NSError *error) {
            NSLog(@"%@", error);
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
        NSLog(@"%@", error);
        [NSApplication.sharedApplication terminate:self];
    }
}

-(NSString*)getValueFromArray:(NSArray*)array following:(NSString*)prev defaultValue:(NSString*)def
{
    NSInteger index = [array indexOfObject:prev];
    if (index != NSNotFound && array.count > 1 + index)
        return [array objectAtIndex:1 + index];
    return def;
}

-(void)initLogging
{
    NSArray* args = [[NSProcessInfo processInfo] arguments];
    NSString* path = [self getValueFromArray:args following:@"-logFile" defaultValue:nil];

    if ([path isEqualToString:@"__default__"]) {
        path = [self defaultLogFile];
    }
    
    if (path != nil) {
        freopen([path cStringUsingEncoding:NSASCIIStringEncoding], "a+", stderr);
    }
    
    NSLog(@"----------------");
    NSLog(@"Launched with arguments: %@", args);
}

-(NSString*)defaultLogFile
{
    NSArray *allPaths = NSSearchPathForDirectoriesInDomains(NSDownloadsDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [allPaths objectAtIndex:0];
    return [documentsDirectory stringByAppendingPathComponent:@"Backup.app.txt"];
}
@end

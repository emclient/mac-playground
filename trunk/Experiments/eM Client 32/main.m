//
//  main.m
//  eM Client 32
//
//  Created by Jiri Volejnik on 23/10/15.
//  Copyright (c) 2015 eM Client. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "CStringArray.h"
#import "AutofreePool.h"
#import "App.h"

int main(int argc, const char * argv[]) {

    AutofreePool* fpool = [[AutofreePool alloc] init];
    NSAutoreleasePool* pool = [[NSAutoreleasePool alloc] init];
    
    NSLogUTF8Array(@"------- Launched with args:", argc, argv);
    
    App* app = [[App new] autorelease];
    [app registerGetURLHandler];
    [app launchWithArgc:argc argv:argv];
    
    [pool release];
    [fpool release];

    NSLog(@"Finished");
    return 0;
}


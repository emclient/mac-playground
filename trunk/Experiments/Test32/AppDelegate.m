//
//  AppDelegate.m
//  TestApp
//
//  Created by Jiri Volejnik on 21/10/15.
//  Copyright © 2015 eM Client. All rights reserved.
//

#import "AppDelegate.h"
#import <AppKit/AppKit.h>

//extern int launchApp(int argc, const char * argv[], NSString*);

@interface AppDelegate ()

//@property (weak) IBOutlet NSWindow *window;
@end

@implementation AppDelegate

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {

//    [self performSelectorOnMainThread:@selector(launch:) withObject:nil waitUntilDone:YES];
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

- (void)launch:(id)something {
 
//    NSArray *arguments = [[NSProcessInfo processInfo] arguments];
//    char** args = [self getArray:arguments];
//    launchApp(arguments.count, (const char**)args, @"formstest");
    //[self freeArray:args];
}

- (char**)getArray:(NSArray*)a_array
{
    unsigned count = [a_array count];
    char **array = (char **)malloc((count + 1) * sizeof(char*));
    
    for (unsigned i = 0; i < count; i++)
    {
        array[i] = strdup([[a_array objectAtIndex:i] UTF8String]);
    }
    array[count] = NULL;
    return array;
}

- (void)freeArray:(char **)array
{
    if (array != NULL)
    {
        for (unsigned index = 0; array[index] != NULL; index++)
        {
            free(array[index]);
        }
        free(array);
    }
}

@end

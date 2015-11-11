//
//  main.m
//  eM Client 32
//
//  Created by Jiri Volejnik on 23/10/15.
//  Copyright (c) 2015 eM Client. All rights reserved.
//

// If you uncomment the following line, you always have to start remote debugging session in Xamarin Studio
// before launching this app, and you have to launch this app without debugging in Xcode!

#import <Cocoa/Cocoa.h>

#include <glib-2.0/glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-config.h>
//#include <mono/metadata/debug-helpers.h>
//#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>

#import "AutofreePool.h"
#import "CStringArray.h"

int launchFormsTest(int argc, const char * argv[]);

// -------

int main(int argc, const char * argv[]) {

    AutofreePool* fpool = [[AutofreePool alloc] init];
    NSAutoreleasePool* pool = [[NSAutoreleasePool alloc] init];
    
    launchFormsTest(argc, argv);
    //launchMailClient(argc, argv);
    //invokeMethod(argc, argv);
    //NSApplicationMain(argc, argv);
    
    [pool release];
    [fpool release];
    return 0;
}

int launchFormsTest(int argc, const char * argv[])
{
    NSBundle* mainBundle = NSBundle.mainBundle;
    
    NSString* contents = [mainBundle.bundlePath stringByAppendingString:@"/Contents"];
//    NSString* resources = [contents stringByAppendingString:@"/Resources"];
    NSString* base = contents;
    NSString* lib = [base stringByAppendingString:@"/lib"];
    NSString* etc = [base stringByAppendingString:@"/etc"];
    NSString* config = [etc stringByAppendingString:@"/mono/config"]; // contains dll remapping
    
//    NSString* exeFolderParent = [lib stringByAppendingString:@"/formstest"];
    NSString* exeFolderParent = [lib stringByAppendingString:@"/emclient"];
    NSString* exeFolder = [exeFolderParent stringByAppendingString:@"/Mono - Debug"];
//    NSString* exe = [exeFolder stringByAppendingString:@"/FormsTest.exe"];
    NSString* exe = [exeFolder stringByAppendingString:@"/MailClient.exe"];
//    NSString* exeConfig = [exe stringByAppendingString:@".config"];
    
    setenv("MONO_ENABLE_SHM", "1", true); // Enable System Mutexes
    setenv ("MONO_DEBUG", "explicit-null-checks", true); // Prevents crashing on null reference exceptions
    //setenv("MONO_LOG_LEVEL", "debug", true);
    //setenv("MONO_DEBUG", "reverse-pinvoke-exceptions", true);

    // Pokus vyresit problem s nenalezenim gdiplus.dll
    //setenv("DYLD_LIBRARY_PATH", [exeFolder UTF8String], true);

    NSMutableArray* monoOptions = [[NSMutableArray new] autorelease];

    // Setup remote debugging if Ctrl-Alt-Shift buttons are pressed on startup
    NSUInteger flags = [NSEvent modifierFlags] & NSDeviceIndependentModifierFlagsMask;
    int remoteDebuggingMask = NSShiftKeyMask | NSControlKeyMask | NSAlternateKeyMask; // | NSCommandKeyMask;
    bool remoteDebugging = flags == remoteDebuggingMask;
    if (remoteDebugging) {
        [monoOptions addObject:@"--soft-breakpoints"];
        [monoOptions addObject:@"--debugger-agent=transport=dt_socket,address=127.0.0.1:10000"];
    }
    //[monoOptions addObject:@"--trace=N:MailClient,E:all"];
    //[monoOptions addObject:@"--trace=N:FormsTest,E:all"];
    
    //mono_jit_parse_options(sizeof(options)/sizeof(char*), (char**)options);
    mono_jit_parse_options(monoOptions.count, [monoOptions UTF8StringArray]);
    
    //http://mono.1490590.n4.nabble.com/teste-exe-mscorlib-dll-not-found-td1503296.html
    mono_set_dirs([lib UTF8String], [etc UTF8String]);
    
    // Load our own modified configuration with dll maps
    mono_config_parse([config UTF8String]);
    
    MonoDomain* domain = mono_jit_init("Domain");

    // Remote debugging using Xamarin Studio:
    if (remoteDebugging) {
        mono_debug_init(MONO_DEBUG_FORMAT_MONO);
        mono_debug_domain_create(domain);
    }
    
    //mono_jit_init_version("FormsTest", "v4.5");
    // Bez tohoto dojde k padu v mono_runtime_class_init, ale zda se ze nezalezi na druhem a tretim parametru
    mono_domain_set_config(domain, [base UTF8String], [config UTF8String]);
    //mono_domain_set_config(domain, [lib UTF8String], [exeConfig UTF8String]);
    
    NSMutableArray* appArgs = [[NSMutableArray new] autorelease];
    [appArgs addObject:@"--args"];
    [appArgs addUTF8Strings:argv from:0 count:argc];
    
    MonoAssembly* assembly = mono_domain_assembly_open(domain, [exe UTF8String]);
    return mono_jit_exec (domain, assembly, appArgs.count, [appArgs UTF8StringArray]);
}

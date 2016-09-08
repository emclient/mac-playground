//
//  App.m
//  FakeMailApp
//
//  Created by Jiri Volejnik on 12/11/15.
//  Copyright © 2015 eM Client. All rights reserved.
//

#import <Cocoa/Cocoa.h>

#include <glib-2.0/glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>

#import "CStringArray.h"
#import "AutofreePool.h"
#import "App.h"

@implementation App

- (int) launchWithArgc:(int)argc argv:(const char**)argv {

    NSBundle* mainBundle = NSBundle.mainBundle;
    NSString* contents = [mainBundle.bundlePath stringByAppendingString:@"/Contents"];
    NSString* base = contents;
    NSString* lib = [base stringByAppendingString:@"/lib"];
    NSString* etc = [base stringByAppendingString:@"/etc"];
    NSString* config = [etc stringByAppendingString:@"/mono/config"]; // contains dll remapping
    NSString* exeFolder = [lib stringByAppendingString:@"/emclient"];
    NSString* exe = [exeFolder stringByAppendingString:@"/MailClient.exe"];
    
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
    if (argc > 0) {
        //[appArgs addObject:@"--args"];
        [appArgs addUTF8Strings:argv from:0 count:argc];
    }
    
    self->assembly = mono_domain_assembly_open(domain, [exe UTF8String]);
    //return mono_jit_exec (domain, assembly, argc, argv);
    return mono_jit_exec (domain, assembly, appArgs.count, [appArgs UTF8StringCopyArray]);
}

// internals ----------------------------------------------

// Sets up handling mailto: and possibly other registered schemes
-(void) registerGetURLHandler {
    NSAppleEventManager* appleEventManager = NSAppleEventManager.sharedAppleEventManager;
    [appleEventManager setEventHandler:self andSelector: @selector(handleGetURLEvent:replyEvent:) forEventClass:kInternetEventClass andEventID:kAEGetURL];
}

// Forwards mailto: and other URLs of registered schemes to the .net app
- (void) handleGetURLEvent:(NSAppleEventDescriptor*)event replyEvent:(NSAppleEventDescriptor*)reply {
    NSString* url = [[event descriptorForKeyword:keyDirectObject] stringValue];
    NSLog(@"GURL:%@", url);

    MonoImage* image = mono_assembly_get_image(self->assembly);
    MonoClass* klass = mono_class_from_name(image, "MailClient", "Program");
    MonoMethod* method = mono_class_get_method_from_name(klass, "GetUrl", 1);

    MonoString* arg0 = mono_string_new(mono_domain_get(), [url UTF8String]);
    void* args[1];
    args[0] = arg0;
    MonoObject* exception = nil;
    mono_runtime_invoke(method, nil, args, &exception);
    
    if (exception != nil)
        NSLog(@"%s", mono_string_to_utf8(mono_object_to_string(exception, nil)));
}

@end

// Alternative way of getting method handle:
//const char* descAsString = "MailClient.Program:GetUrl(string)";
//MonoMethodDesc* description = mono_method_desc_new(descAsString, true);
//MonoMethod* method = mono_method_desc_search_in_class(description, classHandle);

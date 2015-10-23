//
//  main.m
//  eM Client 32
//
//  Created by Jiri Volejnik on 23/10/15.
//  Copyright (c) 2015 eM Client. All rights reserved.
//

#import <Cocoa/Cocoa.h>

#include <glib-2.0/glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>

#include <mono/metadata/mono-config.h>

int launchFormsTest(int argc, const char * argv[]);


int main(int argc, const char * argv[]) {
    NSAutoreleasePool* pool = [[NSAutoreleasePool alloc] init];
    
    launchFormsTest(argc, argv);
    //launchMailClient(argc, argv);
    //invokeMethod(argc, argv);
    //NSApplicationMain(argc, argv);
    
    [pool release];
    return 0;
}

int launchFormsTest(int argc, const char * argv[])
{
    NSBundle* mainBundle = NSBundle.mainBundle;
    
    NSString* contents = [mainBundle.bundlePath stringByAppendingString:@"/Contents"];
    NSString* resources = [contents stringByAppendingString:@"/Resources"];
    NSString* base = contents;
    NSString* lib = [base stringByAppendingString:@"/lib"];
    NSString* etc = [base stringByAppendingString:@"/etc"];
    NSString* config = [etc stringByAppendingString:@"/config"]; // contains dll remapping
    
    NSString* exeFolderParent = [lib stringByAppendingString:@"/formstest"];
    NSString* exeFolder = [exeFolderParent stringByAppendingString:@"/Mono - Debug"];
    NSString* exe = [exeFolder stringByAppendingString:@"/FormsTest.exe"];
    NSString* exeConfig = [exe stringByAppendingString:@".config"];
    
    setenv("MONO_ENABLE_SHM", "1", true); // Enable System Mutexes
    setenv("MONO_LOG_LEVEL", "debug", true);
    
    // Pokus vyresit problem s nenalezenim gdiplus.dll
    //setenv("DYLD_LIBRARY_PATH", [exeFolder UTF8String], true);
    
    //http://mono.1490590.n4.nabble.com/teste-exe-mscorlib-dll-not-found-td1503296.html
    mono_set_dirs([lib UTF8String], [etc UTF8String]);
    
    // Load our own modified configuration with dll maps
    mono_config_parse([config UTF8String]);
    
    MonoDomain* domain = mono_jit_init("Domain");
    
    //mono_jit_init_version("FormsTest", "v4.5");
    // Bez tohoto dojde k padu v mono_runtime_class_init, ale zda se ze nezalezi na druhem a tretim parametru
    mono_domain_set_config(domain, [base UTF8String], [config UTF8String]);
    //mono_domain_set_config(domain, [lib UTF8String], [exeConfig UTF8String]);
    
    MonoAssembly* assembly = mono_domain_assembly_open(domain, [exe UTF8String]);
    return mono_jit_exec (domain, assembly, argc, (char**)argv);
}

//
//  main.m
//  Test32
//
//  Created by Jiri Volejnik on 16/10/15.
//  Copyright © 2015 eM Client. All rights reserved.
//

#import <Cocoa/Cocoa.h>

#include <glib-2.0/glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>

#include <mono/metadata/mono-config.h>

int launchFormsTest(int argc, const char * argv[]);
int launchMailClient(int argc, const char * argv[]);
int invokeMethod(int argc, const char * argv[]);
char* concat(char *s1, char *s2);

int main(int argc, const char * argv[])
{
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
    NSString* config = [resources stringByAppendingString:@"/config"]; // contains dll remapping

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

int launchMailClient(int argc, const char * argv[])
{
    NSBundle* mainBundle = NSBundle.mainBundle;
    
    NSString* contents = [mainBundle.bundlePath stringByAppendingString:@"/Contents"];
    NSString* resources = [contents stringByAppendingString:@"/Resources"];
    NSString* base = contents;
    NSString* lib = [base stringByAppendingString:@"/lib"];
    NSString* etc = [base stringByAppendingString:@"/etc"];
    NSString* config = [resources stringByAppendingString:@"/config"]; // contains dll remapping
    
    NSString* exeFolderParent = [lib stringByAppendingString:@"/emclient"];
    NSString* exeFolder = [exeFolderParent stringByAppendingString:@"/Mono - Debug"];
    NSString* exe = [exeFolder stringByAppendingString:@"/MailClient.exe"];
    //NSString* exeConfig = [exe stringByAppendingString:@".config"];
    
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
    //mono_domain_set_config(domain, [exeFolder UTF8String], [exeConfig UTF8String]); // crashes
    
    MonoAssembly* assembly = mono_domain_assembly_open(domain, [exe UTF8String]);
    return mono_jit_exec (domain, assembly, argc, (char**)argv);
}

int invokeMethod(int argc, const char * argv[]) {
    
    NSBundle* mainBundle = NSBundle.mainBundle;
    
    NSString* contents = [mainBundle.bundlePath stringByAppendingString:@"/Contents"];
    NSString* resources = [contents stringByAppendingString:@"/Resources"];
    NSString* base = contents;
    NSString* lib = [base stringByAppendingString:@"/lib"];
    NSString* etc = [base stringByAppendingString:@"/etc"];
    NSString* config = [resources stringByAppendingString:@"/config"]; // contains dll remapping
    
    NSString* exeFolderParent = [lib stringByAppendingString:@"/emclient"];
    NSString* exeFolder = [exeFolderParent stringByAppendingString:@"/Mono - Debug"];
    NSString* exe = [exeFolder stringByAppendingString:@"/FormsTest.exe"];
    
    // create an app domain
    // http://en.wikipedia.org/wiki/Application_Domain
    MonoDomain* domain = mono_jit_init("Domain");
    
    // mandatory Cocoa call to show that Mono and ObjC work together
    NSString* dll = [mainBundle pathForResource:@"MailClient" ofType:@"exe"];
    
    // load the referenced assembly in our domain
    MonoAssembly* assembly = mono_domain_assembly_open(domain, [dll UTF8String]);
    MonoImage* image = mono_assembly_get_image(assembly);
    
    // find the class we want to wrap and create an uninitialized instance
    MonoClass* classHandle = mono_class_from_name(image, "MailClient", "Program");
    //MonoObject* object = mono_object_new(domain, classHandle);
    MonoObject* object = nil;
    
    // this calls the default, argument-less ctor
    // for more complex constructors, you need to find the method handle and call it
    // (helpful hint: constructors are internally called ".ctor", so the description
    // string will look like "Namespace.Class..ctor()")

    //mono_runtime_object_init(object);
    
    // get a method handle to whatever you like
    const char* descAsString = "MailClient.Program:Main(string[])";

    //Main
    MonoMethodDesc* description = mono_method_desc_new(descAsString, true);
    MonoMethod* method = mono_method_desc_search_in_class(description, classHandle);
    
    // call it
    void* args[0];
    mono_runtime_invoke(method, object, args, NULL);
    
    // when you're done, shutdown the runtime by destroying the app domain
    mono_jit_cleanup(domain);
    
    return 0;
}

char* concat(char *s1, char *s2)
{
    char *result = malloc(strlen(s1)+strlen(s2)+1);//+1 for the zero-terminator
    //in real code you would check for errors in malloc here
    strcpy(result, s1);
    strcat(result, s2);
    return result;
}

// scrapbook

//    NSString* exeFolder = [lib stringByAppendingString:@"/emclient/Mono - Debug"];
//    NSString* exe = [exeFolder stringByAppendingString:@"/MailClient.exe"];
//    NSString* path = [NSString stringWithCString:getenv("PATH") encoding:NSUTF8StringEncoding];
//    path = [path stringByAppendingString:@":"];
//    path = [path stringByAppendingString:bin];
//    setenv("PATH", [path UTF8String], true);

//    NSFileManager* filemgr = [[NSFileManager alloc] init];
//    NSString* cdir = [filemgr currentDirectoryPath]; //getcwd(<#char *#>, <#size_t#>)
//    char cwd[1024];
//    getcwd(cwd, sizeof(cwd));
//chdir("")

//    setenv("DYLD_LIBRARY_PATH", "PREFIX/lib/APPLICATION", 1);
//    setenv("LD_LIBRARY_PATH", "PREFIX/lib/APPLICATION", 1);
//    setenv("DYLD_LIBRARY_PATH", [lib UTF8String], 1);
//    setenv("LD_LIBRARY_PATH", [lib UTF8String], 1);
//    $ export PATH=/opt/mono/bin:$PATH
//    $ export LD_LIBRARY_PATH=/opt/mono/lib:$LD_LIBRARY_PATH # Linux only
//    $ export DYLD_LIBRARY_PATH=/opt/mono/lib:$DYLD_LIBRARY_PATH # Mac OS X
//    $ export PKG_CONFIG_PATH=/opt/mono/lib/pkgconfig
//    $ export MONO_PATH=/opt/mono/lib

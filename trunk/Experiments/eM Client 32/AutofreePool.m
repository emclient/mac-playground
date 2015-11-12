//
//  AutofreePool.m
//  FakeMailApp
//
//  Created by Jiri Volejnik on 10/11/15.
//  Copyright © 2015 eM Client. All rights reserved.
//

#import <Foundation/NSDebug.h>
#import <CoreFoundation/CFBase.h>

#import "AutofreePool.h"
#import <objc/runtime.h>
#import <pthread.h>

static pthread_key_t poolKey;

static void _PFDrain(const void *key, const void *value, void *context)
{
    if (key != nil)
        free((void*)key);
}

@implementation AutofreePool

+ (void)initialize
{
    if( self == [AutofreePool class]) {
        pthread_key_create(&poolKey, NULL);
    }
}

+ (const void*)addObject:(const void*)anObject
{
    if( anObject != nil ) {
        AutofreePool *pool = pthread_getspecific(poolKey);
        if( pool != nil )
            [pool addObject: anObject];
        else
            if (NSDebugEnabled)
                NSLog(@"No AutofreePool");
    }
    return anObject;
}

+ (char * _Nullable)addUTF8String:(const void* _Nonnull)aString {
    [AutofreePool addObject:aString];
    return (char*) aString;
}

+ (char * _Nullable)addUTF8StringCopy:(const void* _Nonnull)aString {
    int n = 1 + strlen(aString);
    char* copy = malloc(n * sizeof(char));
    strcpy(copy, aString);
    copy[n - 1] = '\0';
    return [AutofreePool addUTF8String:copy];
}


-(id)init
{
    if (self = [super init]) {
        _reserved2 = pthread_getspecific(poolKey);
        pthread_setspecific(poolKey, self);
        //[NSAutoreleasePool __hello: self];
        _reserved3 = CFDictionaryCreateMutable( kCFAllocatorDefault, 0, NULL, NULL );
    }
    return self;
}

- (const void*)addObject:(const void*)anObject
{
    if (anObject != nil) {
        NSUInteger count = (NSUInteger)CFDictionaryGetCount(_reserved3);
        count = (NSUInteger)CFDictionaryGetValue( _reserved3, anObject );
        CFDictionarySetValue( (CFMutableDictionaryRef)_reserved3, anObject, (const void *)(++count) );
    }
    return anObject;
}

- (void)drain
{
    pthread_setspecific(poolKey, _reserved2);
    _reserved2 = nil;
    
    NSUInteger count = (NSUInteger)CFDictionaryGetCount( (CFDictionaryRef)_reserved3 );
    
    // apply the _PFDrain function to each object in the dictionary/pool
    if (count != 0)
        CFDictionaryApplyFunction( (CFDictionaryRef)_reserved3, _PFDrain, NULL );
}

-(void)dealloc
{
    CFRelease(_reserved3);
    //[(id)_reserved3 release];
    _reserved3 = nil;
    
    [super dealloc];
}

- (oneway void)release
{
    [self drain];
    [super release];        // which will call dealloc
}

-(id)retain
{
    // raise an exception... better find out which one
    return self; // and don't do this
}

@end

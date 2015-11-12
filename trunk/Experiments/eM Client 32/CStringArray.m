#import "AutofreePool.h"
#import "CStringArray.h"

@implementation NSArray (CStringExtensions)

-(char**)UTF8StringArray {
    int size = self.count * sizeof(char*);
    const char** a = malloc(size);
    for(int i = 0; i < self.count; ++i) {
        NSString* s = [self objectAtIndex:i];
        a[i] = [s UTF8String];
    }

    [AutofreePool addObject:a];
    return (char**)a;
}

-(char**)UTF8StringCopyArray {
    int size = self.count * sizeof(char*);
    const char** a = malloc(size);
    for(int i = 0; i < self.count; ++i) {
        NSString* s = [self objectAtIndex:i];
        a[i] = [AutofreePool addUTF8StringCopy:[s UTF8String]];
    }
    [AutofreePool addObject:a];
    return (char**)a;
}

@end

// ----

@implementation NSMutableArray (CStringExtensions)

+(NSMutableArray*)mutableArrayWithUTF8Strings:(const char**)utf8Strings from:(int)from count:(int)count {
    return [[[NSMutableArray new] autorelease] addUTF8Strings:utf8Strings from:from count:count];
}

-(NSMutableArray*)addUTF8Strings:(const char**)utf8Strings from:(int)from count:(int)count {
    for (int i = 0; i < count; ++i) {
        [self addObject:[NSString stringWithUTF8String:utf8Strings[from + i]]];
    }
    return self;
}


@end

// --------------------------------------------------------------------------------------

void NSLogUTF8Array(NSString* prefix, int n, const char** array) {
    if (prefix != nil)
        NSLog(@"%@", prefix);
    for (int i = 0; i < n; ++i) {
        NSLog(@"%s", array[i]);
    }
}

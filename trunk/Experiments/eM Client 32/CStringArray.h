#import <Foundation/Foundation.h>

@interface NSArray (CStringExtensions)
-(char**)UTF8StringArray;
@end

@interface NSMutableArray (CStringExtensions)
+(NSMutableArray*)mutableArrayWithUTF8Strings:(const char**)utf8Strings from:(int)from count:(int)count;
-(NSMutableArray*)addUTF8Strings:(const char**)utf8Strings from:(int)from count:(int)count;
@end

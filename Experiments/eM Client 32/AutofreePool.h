//
//  AutofreePool.h
//  FakeMailApp
//
//  Created by Jiri Volejnik on 10/11/15.
//  Copyright © 2015 eM Client. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

NS_AUTOMATED_REFCOUNT_UNAVAILABLE

@interface AutofreePool : NSObject {
@private
    void	*_token;
    void	*_reserved3;
    void	*_reserved2;
    void	*_reserved;
}

+ (const void*)addObject:(const void*)anObject;
- (const void*)addObject:(const void*)anObject;
- (void)drain;

+ (char * _Nullable)addUTF8String:(const void* _Nonnull)aString;
+ (char * _Nullable)addUTF8StringCopy:(const void* _Nonnull)aString;

@end

NS_ASSUME_NONNULL_END

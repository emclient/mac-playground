//
//  App.h
//  FakeMailApp
//
//  Created by Jiri Volejnik on 12/11/15.
//  Copyright © 2015 eM Client. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef struct _MonoAssembly MonoAssembly;

@interface App : NSObject {
    MonoAssembly* assembly;
}

- (void) registerGetURLHandler;
- (int) launchWithArgc:(int)argc argv:(const char**)argv;

@end

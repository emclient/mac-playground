//
//  IndexerTests.m
//  IndexerTests
//
//  Created by Jiri Volejnik on 08.03.2022.
//

#import <XCTest/XCTest.h>
#import "Indexer.h"

@interface IndexerTests : XCTestCase

@end

@implementation IndexerTests

- (void)setUp {
    // Put setup code here. This method is called before the invocation of each test method in the class.
}

- (void)tearDown {
    // Put teardown code here. This method is called after the invocation of each test method in the class.
}

- (void)testExample {
    @autoreleasepool {
        NSLog(@"ObjC Term Extractor");

        NSString* main = @__FILE__;
        NSArray* filenames = [NSArray arrayWithObjects: @"good.xlsx", @"corrupted.xlsx", nil];

        NSURL* folder = [[NSURL fileURLWithPath:main] URLByDeletingLastPathComponent];
        for (NSString* filename in filenames) {
            NSURL* url = [folder URLByAppendingPathComponent: filename];
            NSArray* terms = extractTermsFromUrl(url);
            NSLog(@"Terms: %@", terms);
        }
    }
}

- (void)testPerformanceExample {
    // This is an example of a performance test case.
    [self measureBlock:^{
        // Put the code you want to measure the time of here.
    }];
}

@end

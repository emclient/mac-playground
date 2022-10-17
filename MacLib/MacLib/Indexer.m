//
//  Indexer.m
//  Indexer
//
//  Created by Jiri Volejnik on 07.03.2022.
//

#import "Indexer.h"

bool defaultExtractorPlugInsLoaded = false;

NSArray* extractTermsFromUrl(NSURL* url) {
    @autoreleasepool {
        @try {
            if (!defaultExtractorPlugInsLoaded) {
                SKLoadDefaultExtractorPlugIns();
            }

            SKDocumentRef document = SKDocumentCreateWithURL((__bridge CFURLRef)(url));
            if (document != nil) {
                NSString *mime = nil, *name = @"Temporary";
                NSMutableData *data = [NSMutableData new];
                NSMutableDictionary *properties = [NSMutableDictionary dictionaryWithObjectsAndKeys: [NSNumber numberWithInt:3], (__bridge NSString *)kSKTermChars, nil];

                SKIndexRef index = SKIndexCreateWithMutableData((__bridge CFMutableDataRef)(data), (__bridge CFStringRef)(name), kSKIndexInverted, (__bridge CFDictionaryRef)properties);
                if (index != nil) {
                    BOOL added = SKIndexAddDocument(index, document, (__bridge CFStringRef)(mime), true);
                    if (added) {
                        SKIndexFlush(index);
                        SKDocumentID docId = SKIndexGetDocumentID(index, document);
                        CFArrayRef termIDs = SKIndexCopyTermIDArrayForDocumentID(index, docId);
                        if (termIDs != nil) {
                            NSMutableArray* terms = [NSMutableArray new];
                            for (NSNumber* termID in (__bridge NSMutableArray*)termIDs) {
                                CFStringRef term = SKIndexCopyTermStringForTermID(index, termID.intValue);
                                [terms addObject:(__bridge id _Nonnull)(term)];
                            }
                            return terms;
                        }
                    }
                }
            }
        } @catch (NSException *exception) {
            NSLog(@"Failed indexing file '%@' ", url.lastPathComponent);
            NSLog(@"Exception name: %@, reason: %@ ", exception.name, exception.reason);
        }
    }
    return NULL;
}

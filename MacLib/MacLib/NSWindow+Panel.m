//
//  NSWindow+Panel.m
//  Indexer
//
//  Created by Jiří Volejník on 17.10.2022.
//

#import <AppKit/AppKit.h>
#import <objc/runtime.h>
#import "NSWindow+Panel.h"

@interface NSWindow (Panel)
- (BOOL)isPanel;
- (void)setIsPanel:(BOOL)value;
+ (void)replaceIsKeyWindow;
@end

@implementation NSWindow (Panel)

BOOL _isPanel = NO;

- (BOOL)isPanel {
    return _isPanel;
}

- (void)setIsPanel:(BOOL) value {
    _isPanel = value;
}

- (BOOL)isKeyWindowReplacement {
    if (self.isPanel)
        return YES;
    
    // call original
    return self.isKeyWindowReplacement;
}

+ (void)replaceIsKeyWindow {
    
    SEL origSel = @selector(isKeyWindow);
    SEL replSel = @selector(isKeyWindowReplacement);

    Class cls = NSWindow.class;
    Method origMet = class_getInstanceMethod(cls, origSel);
    Method replMet = class_getInstanceMethod(cls, replSel);
    
    IMP origImp = method_getImplementation(origMet);
    IMP replImp = method_getImplementation(replMet);
    
    class_replaceMethod(cls, replSel,origImp, method_getTypeEncoding(origMet));
    class_replaceMethod(cls, origSel, replImp, method_getTypeEncoding(replMet));
}

@end

// Methods to be p/invoked

void initNSWindowPanelCategory(void) {
    [NSWindow replaceIsKeyWindow];
}

void setIsPanel(NSWindow* window, BOOL value) {
    [window setIsPanel:value];
}

BOOL isPanel(NSWindow* window) {
    return [window isPanel];
}

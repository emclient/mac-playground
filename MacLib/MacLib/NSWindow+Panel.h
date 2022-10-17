//
//  NSWindow+Panel.h
//  Indexer
//
//  Created by Jiří Volejník on 17.10.2022.
//

#import <Cocoa/Cocoa.h>

NS_ASSUME_NONNULL_BEGIN

void initNSWindowPanelCategory(void);
void setIsPanel(NSWindow*, BOOL);
BOOL isPanel(NSWindow*);

NS_ASSUME_NONNULL_END

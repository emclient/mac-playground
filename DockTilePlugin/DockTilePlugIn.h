#import <Cocoa/Cocoa.h>
#import <AppKit/AppKit.h>

@interface DockTilePlugIn : NSObject <NSDockTilePlugIn> {
    id jumplistObserver;
    NSMenu* tileMenu;
    NSDockTile* tile;
}

- (void)setDockTile:(NSDockTile *)inDockTile;

@property(retain) id jumplistObserver;
@property(retain) NSMenu* tileMenu;
@property(retain) NSDockTile* tile;

@end

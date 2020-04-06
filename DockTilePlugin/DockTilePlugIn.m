
#import "DockTilePlugIn.h"

@implementation DockTilePlugIn

@synthesize jumplistObserver;
@synthesize tileMenu;
@synthesize tile;

#define kJumplistChangedNotificationName "com.emclient.mail.JumplistChanged"
#define NSLog(args...) _log(@"DEBUG ", __FILE__,__LINE__,__PRETTY_FUNCTION__,args);

bool jumplistModified = false;

void append(NSString* msg)
{
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSCachesDirectory, NSUserDomainMask, YES);
    NSString* folder = [paths objectAtIndex:0];
    NSString *path = [NSString stringWithFormat:@"%@/com.emclient.mail.docktileplugin.txt", folder];
    NSFileHandle *handle = [NSFileHandle fileHandleForWritingAtPath:path];
    
    // create file
    if (![[NSFileManager defaultManager] fileExistsAtPath:path]){
        [[NSData data] writeToFile:path atomically:YES];
    }
    
    // append text
    [handle truncateFileAtOffset:[handle seekToEndOfFile]];
    [handle writeData:[msg dataUsingEncoding:NSUTF8StringEncoding]];
    [handle closeFile];
}

void _log(NSString *prefix, const char *file, int lineNumber, const char *funcName, NSString *format,...) {
    va_list ap;
    va_start (ap, format);
    format = [format stringByAppendingString:@"\n"];
    NSString *msg = [[NSString alloc] initWithFormat:[NSString stringWithFormat:@"%@",format] arguments:ap];
    va_end (ap);
    fprintf(stderr,"%s%50s:%3d - %s",[prefix UTF8String], funcName, lineNumber, [msg UTF8String]);
    append(msg);
    [msg release];
}

- (void)setDockTile:(NSDockTile*)dockTile {
    NSLog(@"setDockTile");
    
    if (dockTile != nil) {
        [self addObservers];
        [self loadJumplist];
    } else {
        [self removeObservers];
    }
    self.tile = dockTile;
}

- (nullable NSMenu*)dockMenu {
    NSLog(@"dockMenu");

    if (tileMenu == nil) {
        tileMenu = [self defaultMenu];
    }
    return tileMenu;
}

- (NSMenu*)defaultMenu {
    NSLog(@"defaultMenu");

    NSMutableArray* args = [[NSMutableArray alloc] init];
    [args addObject:@"/newmail"];
    NSMutableDictionary* item = [[NSMutableDictionary alloc] init];
    [item setObject:@"Message" forKey:@"title"];
    [item setObject:args forKey:@"args"];
    NSMutableArray* items = [[NSMutableArray alloc] init];
    [items addObject:item];
    NSMutableDictionary* jumplist = [[NSMutableDictionary alloc] init];
    [jumplist setObject:items forKey:@"items"];

    return [self createMenuWithJumplist:jumplist];
    
//    NSMenu* menu = [[NSMenu alloc] init];
//    NSMenuItem* item = [[NSMenuItem alloc] initWithTitle:@"New Message" action:@selector(menuItemClicked:) keyEquivalent:@""];
//    item.target = self;
//    item.tag = 0;
//    [menu addItem:item];
//    return menu;
}

- (void)jumplistChanged:(NSNotification*)notification {
    NSLog(@"jumplistChanged:");
    NSDictionary* jumplist = notification.userInfo;
    if (jumplist != nil)
    {
        [self saveJumplist:jumplist];
        self.tileMenu = [self createMenuWithJumplist:jumplist];
    }
}

- (void)loadJumplist {
    NSLog(@"loadJumplist");
    NSDictionary* jumplist = [[NSUserDefaults standardUserDefaults] objectForKey:@"jumplist"];
    self.tileMenu = [self createMenuWithJumplist:jumplist];
}

- (void)saveJumplist:(NSDictionary*)jumplist {
    NSLog(@"saveJumplist:");
    [[NSUserDefaults standardUserDefaults] setObject:jumplist forKey:@"jumplist"];
}

- (NSMenu*)createMenuWithJumplist:(NSDictionary*)jumplist {
    NSLog(@"createMenuWithJumplist:");
    
    NSMenu* menu = [[NSMenu alloc] init];
    if (jumplist != nil) {
        NSArray* items = [jumplist objectForKey:@"items"];
        if (items != nil) {
            for (id d in items) {
                NSMenuItem* item = [self createMenuItemWithDictionary:d];
                if (item != nil) {
                    [menu addItem:item];
                }
            }
        }
    }
    return menu;
}

- (NSMenuItem*) createMenuItemWithDictionary:(NSDictionary*)d {
    NSLog(@"createMenuItemWithDictionary:");

    NSMenuItem* item = nil;
    if (d != nil) {
        NSString* title = [d objectForKey:@"title"];
        if (title != nil) {
            item = [[NSMenuItem alloc] initWithTitle:title action:@selector(menuItemClicked:) keyEquivalent:@""];
            [item setTarget:self];
            [item setRepresentedObject:d];
        }
    }
    return item;
}

- (void) addObservers {
    NSLog(@"addObservers");

    if (self.jumplistObserver == nil) {
        self.jumplistObserver = [[NSDistributedNotificationCenter defaultCenter] addObserverForName:@kJumplistChangedNotificationName object:nil queue:nil usingBlock:^(NSNotification *notification) {
                [self jumplistChanged:notification];
        }];
    }
}

- (void) removeObservers {
    NSLog(@"removeObservers");

    if (self.jumplistObserver != nil) {
        [[NSDistributedNotificationCenter defaultCenter] removeObserver:self.jumplistObserver];
        self.jumplistObserver = nil;
    }
}

- (void)menuItemClicked:(id)sender {
    NSLog(@"menuItemClicked:");
    
    NSMenuItem* item = sender;
    NSDictionary* d = item.representedObject;
    if (d != nil) {
        NSArray<NSString *> * args = [d objectForKey:@"args"];
        [self launchAppWithArgs:args != nil ? args : [[NSArray alloc] init]];
    }
}

- (void)launchAppWithArgs:(NSArray*)args {
    NSLog(@"launchAppWithArgs");
    
    NSString *bundlePath = [[NSBundle bundleForClass:[self class]] bundlePath];
    NSMutableArray *pathComponents = [[bundlePath pathComponents] mutableCopy];
    
    [pathComponents removeLastObject];
    [pathComponents removeLastObject];
    
    NSString* appName = @"eM Client";
    NSString *path = [[[NSString pathWithComponents:pathComponents] stringByAppendingPathComponent:@"MacOS"] stringByAppendingPathComponent:appName];
    NSTask *task = [[NSTask alloc] init];
    [task setLaunchPath:path];
    [task setArguments:args];
    [task launch];
}

- (void)dealloc {
    NSLog(@"dealloc");
    [self removeObservers];
    [super dealloc];
}

@end

#import <Foundation/Foundation.h>
#import <MessageUI/MessageUI.h>
#import <MessageUI/MFMailComposeViewController.h>

@interface ConsoleUtilities : NSObject<MFMailComposeViewControllerDelegate>

+ (ConsoleUtilities*)instance;

- (void)beginEmail:(NSString*)toAddress subject:(NSString*)subject body:(NSString*)body isHTML:(BOOL)isHTML;

- (void)addAttachment:(NSData*)data mimeType:(NSString*)mimeType filename:(NSString*)filename;

- (void)finishEmail;

@end

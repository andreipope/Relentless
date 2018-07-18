#import "ConsoleUtilities.h"

void UnityPause( int pause );

@implementation ConsoleUtilities

MFMailComposeViewController* mMailer;

+ (ConsoleUtilities*)instance
{
    static ConsoleUtilities *instance = nil;
    
    if( !instance )
        instance = [[ConsoleUtilities alloc] init];
    
    return instance;
}

- (void)beginEmail:(NSString*)toAddress subject:(NSString*)subject body:(NSString*)body isHTML:(BOOL)isHTML
{
    mMailer = [[MFMailComposeViewController alloc] init];
    
    if(mMailer == nil)
        return;
    
    [mMailer setSubject:subject];
    [mMailer setMessageBody:body isHTML:isHTML];
    
    mMailer.mailComposeDelegate = self;
    
    if( toAddress && toAddress.length && [toAddress rangeOfString:@"@"].location != NSNotFound )
        [mMailer setToRecipients:[NSArray arrayWithObject:toAddress]];
}


- (void)addAttachment:(NSData*)data mimeType:(NSString*)mimeType filename:(NSString*)filename
{
    if(mMailer == nil)
        return;
    
    if( data && filename && mimeType )
        [mMailer addAttachmentData:data mimeType:mimeType fileName:filename];
}

- (void)finishEmail
{
    if(mMailer != nil)
    {
        [self showViewControllerModallyInWrapper:mMailer];
    }
    
    mMailer = nil;
}

- (void)showViewControllerModallyInWrapper:(UIViewController*)viewController
{
    // pause the game
    UnityPause( true );
    
    // cancel the previous delayed call to dismiss the view controller if it exists
    [NSObject cancelPreviousPerformRequestsWithTarget:self];
    
    UIViewController *vc = UnityGetGLViewController();
    
    // show the mail composer on iPad in a form sheet
    if( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad && [viewController isKindOfClass:[MFMailComposeViewController class]] )
        viewController.modalPresentationStyle = UIModalPresentationFormSheet;
    
    // show the view controller
    [vc presentModalViewController:viewController animated:YES];
}

- (void)dismissWrappedController
{
    UnityPause( false );
    
    UIViewController *vc = UnityGetGLViewController();
    
    if( !vc )
        return;
    
    [vc dismissModalViewControllerAnimated:YES];
}

- (void)mailComposeController:(MFMailComposeViewController*)controller didFinishWithResult:(MFMailComposeResult)result error:(NSError*)error
{
    [self dismissWrappedController];
    
    [controller performSelector:@selector(autorelease) withObject:nil afterDelay:2.0];
}

@end

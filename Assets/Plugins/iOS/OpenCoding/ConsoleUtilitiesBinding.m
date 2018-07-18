#import "ConsoleUtilities.h"
#include "DisplayManager.h"

#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

void _opencodingConsoleBeginEmail(const char * toAddress, const char * subject, const char * body, bool isHTML)
{
    [[ConsoleUtilities instance] beginEmail:GetStringParam( toAddress )
                                    subject:GetStringParam( subject )
                                       body:GetStringParam( body )
                                     isHTML:isHTML];
}

void _opencodingConsoleAddAttachment(UInt8 *bytes, int length, const char * attachmentMimeType, const char * attachmentFilename)
{
    NSData *data = [[NSData alloc] initWithBytes:(void*)bytes length:length];
    [[ConsoleUtilities instance] addAttachment: data
                                    mimeType:GetStringParam( attachmentMimeType )
                                    filename:GetStringParam( attachmentFilename )];
}

void _opencodingConsoleFinishEmail()
{
    [[ConsoleUtilities instance] finishEmail];
}

void _opencodingConsoleCopyToClipboard(const char* text)
{
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    pasteboard.string = GetStringParam(text);
}

int _opencodingConsoleGetNativeScreenWidth()
{
    return [DisplayManager Instance].mainDisplay.screenSize.width;
}
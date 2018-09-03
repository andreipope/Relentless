Thank you for purchasing TouchConsole Pro!

I really hope you enjoy using TouchConsole Pro. Please do send me any questions, suggestions or feedback at support@opencoding.net!

The documentation for TouchConsole Pro is available here: http://opencoding.net/TouchConsolePro/getting_started.php

If you just want to quickly have a play around with the console, open the ConsoleDemoScene scene in the Demo folder and press Play!

=== Changelog ===
== Version 2.5.0 == 
New features
- Added support for iPhone X safe area, the console will be inset in from the left and right edges.

Bug fixes
- Fixed an issue with UI clicks/taps falling through the console to the Unity UI
- Moved Plugins back to their own folder as this broke some features (emailing logs primarily) on Android.

== Version 2.4.0 ==
New features
- Added DebugConsole.Instance.ConsoleWidth property which allows you to change the width of the console. If set to 0 (the default) it'll fill the screen width.
- Added support for Linux.
- Changed layout of folders, everything is now in one folder rather than split between Plugins and the Opencoding folder.
- Now captures log messages from threads that aren't the main thread using the Unity logMessageReceivedThreaded callback rather than via the AppDomain.
- Added support for UWP platforms - they're recongised as desktop platforms so require a keyboard.

Bug fixes
- Fixes to support Unity 2017
- Fixed stacktraces not being escaped in the exported/emailed log files. This caused some characters (such as < and >) to cause the log file to be corrupted.
- Fixed compilation error on Unity 2017.
- Changed behaviour of keyboard on Android, this makes it usable (but not quite ideal) on recent Android versions where the keyboard covers more of the screen and behaves slightly differently. Many thanks to Heiko Schmitt at Big Point for his help with this.

Removed features
- Removed support for Unity 4.x

== Version 2.3.1 ==
Bug fixes
- Fixed gradle builds failing with an error related to a BuildConfig class in the jar file
- Fixed a warning related to Unity 4 that sometimes appeared on Unity 5

== Version 2.3.0 ==
Bug fixes
- Calling DebugConsole.Instance.EmailLog when the console isn’t open will cause the console to open itself.
- Fix for exception when emailing a log on some Android devices.
- Fix for warning about having multiple android-support-v4.jar files not properly checking filenames (it was detecting files called android-support-v4 with any file extension).
- Fixed log emails not including log messages that were emitted during the last frame.

== Version 2.2.0 ==
New features
- Added API for exporting the log to a file - DebugConsole.Instance.ExportLog.
- Multiple save files (or other data files that you have) can now be attached to log emails/exports. An example of how to do this can be seen in BallGameController.SetupDebugConsole. If you’ve used DebugConsole.SaveFileProvider already, you’ll need to make a small modification to fix compile errors as this API has changed.
- Added setting that allows you to disable showing the list of recent commands.
- Added setting to allow you to specify that the console should only be included in a build if a certain define is set. This is the opposite of the existing setting. The existing 'disable if defined' setting has precedence over the new 'enable if defined' setting, to err on the side of caution.

Changes
- Removed the device unique identifier from the log file as this was causing Unity to add a new permission for it on Android. You can add the identifier yourself if you wish, using the log file customisation API.
- Log messages are now trimmed to 10,000 characters long when displayed, as there were issues on Android where warnings were being output when very long messages were displayed.
- Reduced per-frame memory allocation when the console is hidden. This should now be 0 on mobile devices, and a few bytes on desktop platforms (due to an unavoidable Unity function being used).

Bug fixes
- Fixed device/OS specific issue with sending emails on Android.

== Version 2.1.0 ==
New features
- Added support for params array arguments on methods. These can be of any type that the console supports.
- Added CommandHandlers.RegisterCommandHandler. This has a number of overrides that will allow you to register a single method or property as a command handler. It'll also allow you to specify your own types for param arrays - see the documentation and the file AdvancedCommandHandlerDemo.cs for a full explanation.
- Added CommandHandlers.UnregisterCommandHandler, for unregistering command handlers registered with the above method.
- Added CommandHandlers.CurrentExecutingCommand method that will tell you the name of the currently executing command as you can now register multiple commands to a single method.
- Added CommandHandlers.DefaultCommandHandler which will allow you to register a method to be called if the command hasn't been handled by any registered command. Avoid using this too much!
- Added detection of missing or incorrect link.xml file. If the file is missing, a default version will be generated, if it's present but invalid, an exception will be thrown during the build.
- You can now disable the recent command list in the settings.

Changes
- To fix the Android email issue (below) the Android integration code is now provided as an Android Library Project rather than a .jar file. The first time you update to this version, the old Android .jar file will be deleted.
- Related to this the android support library (android-support-v4) is now included with the console. This isn't ideal, but it's now required. If you already have a copy of this library in your project you may want to remove this, or if you have a different SDK version you may want to replace it with the one from your SDK.
- Removed the Unity 5.2 workaround code that added the Toggle TouchConsole Pro menu item to the Window menu. If you found this really useful, let me know!

Bug fixes
- Fixes for two issues that occurred when using Unity 5.3.
- Fixed being unable to send emails on Android using the Gmail 5.x app.
- When using the up/down keys to go through the console history, the suggestions and command parameter information wasn't updated correctly.
- Fixed optional string parameters not showing the default value.

== Version 2.0.1 == 
Changes
- Application.version is used to set the game version on non-iOS/Android platforms (rather than PlayerSettings.bundleVersion).

Bug fixes
- Workaround for Unity 5.2.x bug (see below) is now limited to builds before 5.2.1p3 where the bug was fixed by Unity.
- Fixed crash when trying to send an email on iOS with no email accounts set up on the device.
- Fixed the behaviour of the suggestion buttons which were somewhat inconsistent as to whether they'd automatically execute or not. They should now always automatically execute if they don't have any parameters.
- Hardware information for the logs is no longer populated via reflection, which should make this more reliable when stripping is turned on.

== Version 2.0.0 ==
New features
- Massively improved log emails:
  - Quickly jump between errors/warnings etc.
  - Filter the log
  - See hardware data about the device that sent the log
  - See game data such as the camera position, direction etc (if you provide it - see example in DemoController.cs)
  - See a screenshot of the game (optionally, this can be disabled in the settings)
  - See the save file (if you provide it - see example in BallGameController.cs)
  - See the ‘real’ device time that each log message occurred at
- Added buttons for toggling Exceptions and Asserts, rather than grouping them with Errors as Unity does. This can be disabled in the settings if you prefer the old behavior.
- Added icons for Exceptions and Asserts. Exceptions are magenta squares, asserts are blue circles (obviously!)
- You can now press and hold one of the log message type buttons (errors, warnings etc.) at the top of the screen to only show that type. Press and hold again to show all log message types.
- Added option in the settings to automatically pause the game (setting Time.timeScale) when the console is opened. This defaults to being disabled.
- Game version is now shown at the bottom of the in-console settings popup (the cog in the top right corner)

Changes
- Settings are now in their own ScriptableObject asset. This will mean you need to re-set your settings if you’ve made any changes to the defaults, but it should mean that this won’t need to happen again in the future.

Bug fixes
- Workaround for Unity 5.2.x bug (http://tinyurl.com/qje3nd7) where the console couldn’t be opened on the OS X editor or OS X standalone builds. This means that on these platforms and versions you can only open the console with the tilde/back quote key. This should be fixed in a Unity patch release very soon.
- Fixed parameter suggestions not being shown for properties with the CommandHandler attribute added.
- Fixed console command lines in the log being counted as errors
- Fixed console opening animation being affected by Time.timeScale, causing it to fail to open if the game was paused.
- Fixed parameter suggestion buttons being left showing once one of them had been used to execute a command.
- Fixed the bottom of 'y' and 'g' characters on auto-complete buttons being cut off in the editor.
- Minor fixes to the layout of the search/filter bar.

== Version 1.5.0 ==
New features
- By default the console is now automatically added to any scene in the project when you press play. This doesn’t affect builds, and can be turned off via the Unity Preferences window.
- Added support for setting the default email address that logs should be emailed to.
- Added support for changing the scale of the console, with independent settings for the editor, mobile and standalone.
- Added ability to change the log history size limit (default 3000 items) using LogHistory.Instance.ItemLimit.
- Made it easier to modify the install location: you’ll need to edit the _opencodingDirectoryLocation field in DebugConsoleEditorSettings.cs.

Bug fixes
- OS X Editor/Standalone: Temporary work around for broken keyboard input on OS X (introduced in Unity 5.2). There’s now a menu item that allows you to show/hide the console in the editor (or Command+G).
- Fixed an error related to an undocumented property (TextEditor.pos) that was renamed in Unity 5.2.
- iOS: Fixed console appearing too large when the “Target Resolution” player setting is set to something other than Native.
- Fixed exception that occurred when Unity 'hot reloaded' the project due to the console not being able to completely serialize its state. A better fix for this will come in the future.
- Fixes exporting the console log on Windows where invalid characters in the filename caused it to fail.
- Fixed exporting the console log failing on standalone builds.
- Fixed exception occurring when using Debug.Assert.

== Version 1.4.0 ==
New features
- Automatically disables Unity GUI input when the console is opened. This can be disabled in the Settings if you have your own way of doing this. This works by setting the EventSystem.current property to null while the console is open and restoring it once the console is closed.
- Added two new callbacks - DebugConsole.Instance.ConsoleAboutToOpen and DebugConsole.Instance.ConsoleAboutToClose. These allow you to be notified when the console is going to open or close and prevent it, if you wish.

Bug fixes
- DebugConsole game objects will now destroy themselves if there is already an instance of DebugConsole loaded.
- Fixed the Instance field for the DebugConsole class not being set to null when it was Destroyed.

== Version 1.3.0 ==
New features
- Added Run button to the end of the input line on mobile devices. This is makes the console at least partly useable in landscape on Nexus and Xperia devices (where a Unity bug causes the touch screen keyboard to be non-interactive).
- Suggestion buttons now automatically execute the command if it takes no parameters.
- Made the keys that open/close the console configurable. By default this is one of ~, \, `, |, § or ±.

Bug fixes
- Fixed Android log emails not including the attachment sometimes.
- Fixed a very infrequent issue where an exception would sometimes be thrown when log messages were emitted from a non-main thread.
- Fixed error that sometimes occurred when running the Demo scene.
- Fixed an issue on Windows where the console would flash up for a single frame when opened.
- Fixed compilation errors with Unity 5 on iOS.

== Version 1.2.0 ==

IMPORTANT: If upgrading to this version, make sure you rebuild your Xcode project from scratch or you will get compile errors.

New Features
- Added CommandHandlers.BeforeCommandExecutedHook that allows you to prevent a command from being executed.
- Added a demo scene and code that shows how you can use this to ask the user for a password before certain commands are executed - useful for public betas.

Changes
- Switched to a different method for modifying the Xcode project. This should be more compatible with other Unity plugins, most notably the Facebook SDK. On upgrade, the old code for this will be automatically deleted to avoid the unnecessary code hanging around - you may notice this in your version control system.
- The filter bar is now automatically closed when the console is.
- On Mobile: Opening the filter bar with the console maximized now temporarily minimizes the console so the keyboard doesn’t overlay the console.

Bug Fixes
- Worked around a bug in Unity 4.6.1 that caused a crash on iOS and Android (thanks to the multiple users who noticed this!)
- Fixed copying text not working in the web player (thanks jerotas!)
- Fixed an exception that occurred when the filter bar was closed using the Done/Return button on the Touch Screen Keyboard.

== Version 1.1.1 ==

New Features
- Added a new method for opening the console - holding down three fingers for about half a second. This can be enabled in the settings.
- Added hook to allow you to customise the email that is sent - extra attachments can be added and the message modified or replaced. This is useful for adding your save file or screenshots etc.
- Added method for triggering the log email to be sent, if you want to provide another method for sending it.

Bug Fixes
- Fixed the log being blank on Unity 5.
- Fixed WebGL builds on Unity 5.
- Fixed an error when the console was used in a game with stripping enabled (added a link.xml file).
- Fix for builds failing when the console was used in a game with the Facebook SDK included.
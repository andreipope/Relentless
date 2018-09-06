ReUniter is a Unity Editor extension inspired by ReSharper, the C# plugin for Visual Studio from JetBrains. Works with both Unity Free and Pro. Similar to Launchy, Spotlight, Alfred, Quicksilver.

Features:
 - Navigate to Game Object (Ctrl-G or Command-G) - search as you type through entities in the Hierarchy window
 - Navigate to Asset (Ctrl-T or Command-T) - search as you type through assets in the Project window
 - Recent Items (Ctrl-E or Command-E) - quick navigation to recently selected items (regardless of whether they were selected via ReUniter or not - this keeps track of the Unity current selection as it appears in the Inspector window)
 - using the Shift key when selecting something will add it to the current selection
 - using the Control/Command key when selecting something will "execute" it (the equivalent of double clicking it or pressing Enter when it is selected) - for example: a scene will be opened, a script file will bring up the external editor
 - using the Alt key when selecting something will not select it, rather only highlight it in its corresponding window (and not dismiss the ReUniter window)
 - pressing Ctrl-A or Command-A will select all visible search results (only for navigate to asset or game object), then press Enter or Tab to select them
 - using the menu item Tools->ReUniter->Change Window location, you can change where ReUniter windows will appear

Search behavior:
 - find items that contain the searched text
 - use multiple words in the search
 - use * and ? for wildcard searches: "Re*W" matches "ReUniterWindow"
 - use CamelHump (initials) search: "RUW" or "ruw" matches "ReUniterWindow"
 - use Unity typed search, using ":material", ":shader" or shorter versions, ":ma" matches materials, ":m" matches models and material, ":a" matches audio and animation clips
   - supported asset types: AnimationClip, AudioClip, AudioMixer (Unity 5 only), Font, GUISkin, Material, Mesh, Model, PhysicMaterial, Prefab, Scene, Script, Shader, Sprite, Texture
   - ":audioclip" or ":au" (or anything in between) matches all audio files
   - ":tex red" matches all textures with "red" in their name
   - "blend :sha" or ":shader blend" matches all shaders with "blend" in their name
   - use ":material", ":script", ":texture", ":audio", ":camera", ":light", ":renderer", etc (or as few letters as needed, like ":ma" or ":sh") 

Popup window behavior:
 - incremental search (as you type), searched text will be highlighted in all search results
 - results are ordered based on the best match
 - use the up/down arrow keys or the mouse to navigate up and down in the search results
 - press Enter, Tab or left click to select a search result
 - press Escape or click anywhere ouside of the search dialog to dismiss it
 - if the recent items window is open, press Ctrl-E or Command-E again to go down that list (to allow for quick one-handed switching between two different items using the Tab key)
 
The extension supports and remembers multiple selection items. Recent item selections are persistent across editor runs - they are stored in Assets/.reuniter_data so don't forget to add it to the ignore list (.gitignore, svn:ignore) if you're using version control.
 
This extension comes with source code if you find the default keyboard shorcuts conflict with other existing assets.


Known issues:
 - The higlighting of search results once something is selected will throw a harmless error if that window (Hierarcy or Project) is not visible. Filed bug with Unity.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
#if (UNITY_5)
using UnityEngine.Audio;
#endif
using Object = UnityEngine.Object;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable CheckNamespace
// ReSharper disable ConvertClosureToMethodGroup

namespace Reuniter
{

    internal class ReUniterItemInfo
    {

        public ReUniterItemInfo(String fullPath)
        {
            var lastSlash = fullPath.LastIndexOf('/');
            ParentPath = lastSlash < 0 ? "" : fullPath.Substring(0, lastSlash);
            Name = lastSlash < 0 ? fullPath : fullPath.Substring(lastSlash+1);
            StripExtension = true;
        }

        public ReUniterItemInfo(string parentPath, string name, params Object[] unityObjects)
        {
            ParentPath = parentPath;
            Name = name;
            UnityObjects = unityObjects;
            PenaltyScore = -1;
        }

        public string ParentPath { get; set; }
        public string Name { get; set; }
        public bool StripExtension { get; private set; }

        public string NameWithoutExtension {
            get { return StripExtension ? FileNameWithoutExtension(Name) : Name; }
        }
        public Object[] UnityObjects { get; set; }
        public int PenaltyScore { get; set; }

        private static string FileNameWithoutExtension(string fileName)
        {
            var lastDot = fileName.LastIndexOf('.');
            return lastDot < 0 ? fileName : fileName.Substring(0, lastDot);
        }

    }

    internal class BuiltInAssetType
    {
        public string Name { get; set; }
        public Type UnityType { get; set; }
        public String[] Extensions { get; set; }

        public BuiltInAssetType(string name, Type unityType, params string[] extensions)
        {
            Name = name;
            UnityType = unityType;
            Extensions = extensions;
        }

        public bool Matches(Object asset, string fullAssetName)
        {
            if (asset == null || !UnityType.IsInstanceOfType(asset))
                return false;
            return Extensions == null || Extensions.Length == 0 ||
                   Extensions.Any(x => fullAssetName.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal class ReUniterMode
    {
        public Func<string, IEnumerable<ReUniterItemInfo>> RefreshAction { get; set; }
        public Func<ReUniterItemInfo, Object[]> LoadItem { get; set; }

        public string SearchLabel { get; set; }
        public bool MoveWindowMode { get; set; }
    }

    public class ReUniterWindow : EditorWindow
    {
        private static IEnumerable<Type> UnityTypes;

        private static readonly List<BuiltInAssetType> BuiltInAssetTypes = new List<BuiltInAssetType>
        {
            new BuiltInAssetType("AnimationClip",                 typeof(Animation)),
            new BuiltInAssetType("AudioClip",                     typeof(AudioClip)),
#if (UNITY_5)
            new BuiltInAssetType("AudioMixer",                    typeof(AudioMixer)),
#endif
            new BuiltInAssetType("Font",                          typeof(Font)),
            new BuiltInAssetType("GUISkin",                       typeof(GUISkin)),
            new BuiltInAssetType("Material",                      typeof(Material)),
            new BuiltInAssetType("Mesh",                          typeof(Mesh)),
            new BuiltInAssetType("Model",                         typeof(GameObject), ".fbx", ".dae", ".3ds", ".dxf", ".obj" ),
            new BuiltInAssetType("PhysicMaterial",                typeof(PhysicMaterial)),
            new BuiltInAssetType("Prefab",                        typeof(GameObject), ".prefab"),
            new BuiltInAssetType("Scene",                         typeof(Object), ".unity"),
            new BuiltInAssetType("Script",                        typeof(MonoScript)),
            new BuiltInAssetType("Shader",                        typeof(Shader)),
            new BuiltInAssetType("Sprite",                        typeof(Sprite)),
            new BuiltInAssetType("Texture",                        typeof(Texture))
        }; 

        private string itemName = "";
        private string previousItemName;

        private IEnumerable<ReUniterItemInfo> itemInfos = NoResults;
        private static readonly GUIStyle RichTextGuiStyle = new GUIStyle { richText = true, fontSize = 12, margin = new RectOffset(5, 5, 5, 5), normal = { textColor = EditorStyles.label.normal.textColor } }; 
        private static readonly GUIStyle RightAlignRichTextGuiStyle = new GUIStyle { richText = true, fontSize = 12, margin = new RectOffset(5,5,5,5), 
            alignment = TextAnchor.MiddleRight, normal = {textColor = new Color(.4f,.4f,.4f)}};
        private static readonly GUIStyle CenteredLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperCenter, wordWrap = true };

        private Texture2D selectedLineBackgroundTex;
        private Texture2D searchLineBackgroundTex;

        private GUIStyle selectedLineGuiStyle;
        private GUIStyle searchLineGuiStyle;
        private GUIStyle regularLineGuiStyle;

        private static readonly ReUniterItemInfo[] NoResults = { };
        private int selectedIndex;
        private bool selectAll;
        private const int ROW_HEIGHT = 24;
        private const int ICON_XMIN = -12;

        private const float WINDOW_WIDTH = 600;
        private const float WINDOW_HEIGHT = 70;
        private const float MOUSE_HOVER_DELTA = 54;
        private const float MOVE_WINDOW_HEIGHT = 110;
        private const string REUNITER_POSITION_X = "REUNITER_POSITION_X";
        private const string REUNITER_POSITION_Y = "REUNITER_POSITION_Y";
        private const string DARK_COLORIZE_PREFIX = "<b><color=#07c7f2>";
        private const string LIGHT_COLORIZE_PREFIX = "<b><color=#008293>";
        private const string COLORIZE_SUFFIX = "</color></b>";
        private const int MAX_ITEMS_COUNT = 14;
        private ReUniterMode mode;

        public void OnDestroy()
        {
            DestroyImmediate(selectedLineBackgroundTex);
            DestroyImmediate(searchLineBackgroundTex);
        }

        [MenuItem("Tools/ReUniter/Go To Asset %t")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static void GoToAsset()
        {
            ShowWindow(new ReUniterMode
                {
                    RefreshAction = RefreshAssetInfos,
                    SearchLabel = "Enter Asset Name:",
                    LoadItem = LoadAsset
                });
        }


        [MenuItem("Tools/ReUniter/Go To Game Object %g")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static void GoToGameObject()
        {
            ShowWindow(new ReUniterMode
                {
                    RefreshAction = RefreshGameObjectInfos,
                    SearchLabel = "Enter Game Object Name:",
                    LoadItem = LoadUnityObjects
                });
        }

        [MenuItem("Tools/ReUniter/Recent Items %e")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static void RecentItems()
        {
            var recentItemsMode = new ReUniterMode
            {
                RefreshAction = RefreshRecentItemInfos,
                SearchLabel = "Enter Recent Item Name:",
                LoadItem = LoadUnityObjects
            };

            if (PreviousWindow != null && PreviousWindow.mode.SearchLabel == recentItemsMode.SearchLabel)
            {
                PreviousWindow.selectAll = false;
                PreviousWindow.selectedIndex++;
                PreviousWindow.Repaint();
            }
            else
                ShowWindow(recentItemsMode);
        }

        [MenuItem("Tools/ReUniter/Change Window Location")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static void ChangeWindowLocation()
        {
            ShowWindow(new ReUniterMode
            {
                RefreshAction = null,
                SearchLabel = "Move window to desired location then click button below to save.\nReUniter windows will have no frame/title bar so please ignore the frame on this window.",
                LoadItem = null,
                MoveWindowMode = true
            });
        }


        [MenuItem("Tools/ReUniter/Clear Recent Items")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static void ClearRecentItems()
        {
            ReUniterSelectionHistoryTracker.PreviousSelections.Clear();   
        }

        private static IEnumerable<ReUniterItemInfo> RefreshRecentItemInfos(string itemName)
        {
            return RefreshInfos(itemName, FindRecentItemsAction, true);
        }

        private static string NameForObjects(Object[] objects)
        {
            var result = new StringBuilder();
            objects.Where(x=>x!=null)._Each(x=>result.Append(SelectedItemName(x)).Append(", "));
            result.Remove(result.Length - 2, 2);
            return result.ToString();
        }

        public static string SelectedItemName(Object obj)
        {
            if (!AssetDatabase.Contains(obj))
                return obj.name;
            var assetPath = AssetDatabase.GetAssetPath(obj);
            var lastSlash = assetPath.LastIndexOf('/');
            var selectedItemName = lastSlash < 0 ? assetPath : assetPath.Substring(lastSlash + 1);
            if (selectedItemName.Trim().Length == 0)
                selectedItemName = obj.name;
            return selectedItemName;
        }

        private static ReUniterWindow PreviousWindow;
        private bool darkSkin;

        private static void ShowWindow(ReUniterMode mode)
        {
            if (PreviousWindow != null)
            {
                if (PreviousWindow.mode.SearchLabel == mode.SearchLabel) //reusing existing window
                    return;
                PreviousWindow.Close();
            }
            var reUniter = CreateInstance<ReUniterWindow>();
            reUniter.wantsMouseMove = true;
            reUniter.mode = mode;
            if (mode.MoveWindowMode)
            {
                reUniter.minSize = new Vector2(WINDOW_WIDTH, MOVE_WINDOW_HEIGHT);
                reUniter.maxSize = new Vector2(WINDOW_WIDTH, MOVE_WINDOW_HEIGHT);
                reUniter.ShowUtility();
                reUniter.position = PositionRect(MOVE_WINDOW_HEIGHT);
            }
            else
            {                
                var positionRect = PositionRect(WINDOW_HEIGHT);
                positionRect.y -= positionRect.height;
                reUniter.ShowAsDropDown(positionRect, new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT-1));
            }
            PreviousWindow = reUniter;
        }

        private static Rect PositionRect(float windowHeight)
        {
            var positionX = EditorPrefs.GetFloat(REUNITER_POSITION_X, (float)Screen.currentResolution.width/2 - WINDOW_WIDTH/2);
            var positionY = EditorPrefs.GetFloat(REUNITER_POSITION_Y, Screen.currentResolution.height/2 - 100);
            return new Rect(positionX, positionY, WINDOW_WIDTH, windowHeight);
        }

        public static Texture2D GetIconForFile(string fileName)
        {
            var lastDot = fileName.LastIndexOf('.');
            var extension = (lastDot != -1) ? fileName.Substring(lastDot + 1).ToLower() : string.Empty;
            switch (extension)
            {
                case "boo":
                    return EditorGUIUtility.FindTexture("boo Script Icon");
                case "cginc":
                    return EditorGUIUtility.FindTexture("CGProgram Icon");
                case "cs":
                    return EditorGUIUtility.FindTexture("cs Script Icon");
                case "guiskin":
                    return EditorGUIUtility.FindTexture("GUISkin Icon");
                case "js":
                    return EditorGUIUtility.FindTexture("Js Script Icon");
                case "mat":
                    return EditorGUIUtility.FindTexture("Material Icon");
                case "prefab":
                    return EditorGUIUtility.FindTexture("PrefabNormal Icon");
                case "shader":
                    return EditorGUIUtility.FindTexture("Shader Icon");
                case "txt":
                    return EditorGUIUtility.FindTexture("TextAsset Icon");
                case "unity":
                    return EditorGUIUtility.FindTexture("SceneAsset Icon");
                case "asset":
                case "prefs":
                    return EditorGUIUtility.FindTexture("GameManager Icon");
                case "anim":
                    return EditorGUIUtility.FindTexture("Animation Icon");
                case "meta":
                    return EditorGUIUtility.FindTexture("MetaFile Icon");
                case "ttf":
                case "otf":
                case "fon":
                case "fnt":
                    return EditorGUIUtility.FindTexture("Font Icon");
                case "aac":
                case "aif":
                case "aiff":
                case "au":
                case "mid":
                case "midi":
                case "mp3":
                case "mpa":
                case "ra":
                case "ram":
                case "wma":
                case "wav":
                case "wave":
                case "ogg":
                    return EditorGUIUtility.FindTexture("AudioClip Icon");
                case "ai":
                case "apng":
                case "png":
                case "bmp":
                case "cdr":
                case "dib":
                case "eps":
                case "exif":
                case "gif":
                case "ico":
                case "icon":
                case "j":
                case "j2c":
                case "j2k":
                case "jas":
                case "jiff":
                case "jng":
                case "jp2":
                case "jpc":
                case "jpe":
                case "jpeg":
                case "jpf":
                case "jpg":
                case "jpw":
                case "jpx":
                case "jtf":
                case "mac":
                case "omf":
                case "qif":
                case "qti":
                case "qtif":
                case "tex":
                case "tfw":
                case "tga":
                case "tif":
                case "tiff":
                case "wmf":
                case "psd":
                case "exr":
                    return EditorGUIUtility.FindTexture("Texture Icon");
                case "3df":
                case "3dm":
                case "3dmf":
                case "3ds":
                case "3dv":
                case "3dx":
                case "blend":
                case "c4d":
                case "lwo":
                case "lws":
                case "ma":
                case "max":
                case "mb":
                case "mesh":
                case "obj":
                case "vrl":
                case "wrl":
                case "wrz":
                case "fbx":
                    return EditorGUIUtility.FindTexture("Mesh Icon");
                case "asf":
                case "asx":
                case "avi":
                case "dat":
                case "divx":
                case "dvx":
                case "mlv":
                case "m2l":
                case "m2t":
                case "m2ts":
                case "m2v":
                case "m4e":
                case "m4v":
                case "mjp":
                case "mov":
                case "movie":
                case "mp21":
                case "mp4":
                case "mpe":
                case "mpeg":
                case "mpg":
                case "mpv2":
                case "ogm":
                case "qt":
                case "rm":
                case "rmvb":
                case "wmw":
                case "xvid":
                    return EditorGUIUtility.FindTexture("MovieTexture Icon");
                case "colors":
                case "gradients":
                case "curves":
                case "curvesnormalized":
                case "particlecurves":
                case "particlecurvessigned":
                case "particledoublecurves":
                case "particledoublecurvessigned":
                    return EditorGUIUtility.FindTexture("ScriptableObject Icon");
            }
            return EditorGUIUtility.FindTexture("DefaultAsset Icon");
        }


        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnGUI()
        {
            if (mode==null)
                return;
            if (KeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }
            DetectMouseHover();
            if (KeyDown(KeyCode.DownArrow))
            {
                selectAll = false;
                selectedIndex++;
            }
            if (KeyDown(KeyCode.UpArrow))
            {
                selectAll = false;
                selectedIndex--;
            }
            
            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "SelectAll"){
                selectAll = !selectAll;
	            Event.current.Use();
            }
            if (string.IsNullOrEmpty(itemName))
            {
                selectAll = false;
            }

            var enterPressed = KeyDown(KeyCode.Return) || KeyDown(KeyCode.Tab);
          
            var shiftPressed = Event.current.shift;
            var controlPressed = IsOSX() ? Event.current.command : Event.current.control;
            var altPressed = Event.current.alt;

            previousItemName = itemName;
            if (mode.MoveWindowMode)
                DisplaySavePositionButton();
            else
                DisplaySearchTextField();

            RefreshFileNames();
            
            if (Event.current.type == EventType.MouseDown)
            {
                var mousePositionIndex = MousePositionIndex();
                if (mousePositionIndex >= 0 && mousePositionIndex < itemInfos.Count())
                    enterPressed = true;
            }
            selectedIndex = itemInfos.Any() ? Mathf.Clamp(selectedIndex, 0, itemInfos.Count() - 1) : -1;
            if (enterPressed)
            {
                if (selectAll)
                    SelectItems(itemInfos, shiftPressed, controlPressed, altPressed);
                else if (selectedIndex >= 0)
                    SelectItems(new[]{ itemInfos.ElementAt(selectedIndex)}, shiftPressed, controlPressed, altPressed);
                if (!altPressed)
                    Close();
                return;
            }

            DisplaySearchResults();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static bool IsOSX()
        {
            return Application.platform == RuntimePlatform.OSXEditor;
        }

        private void DetectMouseHover()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                if (Event.current.mousePosition.y > MOUSE_HOVER_DELTA)
                {
                    var hoveredIndex = MousePositionIndex();
                    if (hoveredIndex != selectedIndex)
                    {
                        selectAll = false;
                        selectedIndex = hoveredIndex;
                        Repaint();
                    }
                }
            }
        }

        private static int MousePositionIndex()
        {
            return ((int)(Event.current.mousePosition.y - MOUSE_HOVER_DELTA)) / ROW_HEIGHT;
        }

        private void RefreshFileNames()
        {
            if ((itemName == "" || previousItemName != itemName) && mode.RefreshAction != null)
            {
                itemInfos = mode.RefreshAction(itemName).ToArray();
            }
        }

        private static string MakeWildCard(string searchTerm)
        {
            var sb = new StringBuilder();
            sb.Append(" ");
            searchTerm.ToCharArray()._Each(x=>sb.Append(x).Append(' '));
            return sb.ToString();
        }

        private static IEnumerable<ReUniterItemInfo> RefreshAssetInfos(string itemName)
        {
            return RefreshInfos(itemName, FindAssetsAction);
        }

        private static void InitUnityTypes()
        {
            if (UnityTypes==null)
                UnityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(
                    t =>
                        t != null && t.IsClass && t.Namespace != null &&
                        (t.Namespace.StartsWith("UnityEngine") || t.Namespace.StartsWith("UnityEditor")) && typeof(Object).IsAssignableFrom(t)).ToList();
        }

        private static IEnumerable<ReUniterItemInfo> RefreshInfos(string itemName, Func<string[], IEnumerable<Type>, IEnumerable<ReUniterItemInfo>> findAction, bool returnAllOnEmptySearch = false)
        {
            InitUnityTypes();
            if (string.IsNullOrEmpty(itemName))
            {
                return returnAllOnEmptySearch ? findAction(new string[] {}, UnityTypes).Take(MAX_ITEMS_COUNT) : NoResults;
            }

            var searchTerms = itemName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var results = ResultsFor(searchTerms, UnityTypes, findAction);

            var reUniterItemInfos = results.Take(100)
                .OrderBy(x => x.PenaltyScore).ThenBy(x => x.Name).ThenBy(x => x.ParentPath)
                .Take(MAX_ITEMS_COUNT);

            return reUniterItemInfos;
        }

        private static IEnumerable<ReUniterItemInfo> ResultsFor(List<string> searchTerms, IEnumerable<Type> unityTypes,
            Func<string[], IEnumerable<Type>, IEnumerable<ReUniterItemInfo>> findAction)
        {
            var uniterItemInfos = findAction(searchTerms.ToArray(), unityTypes);
            var results = uniterItemInfos.ToList();
            results._Each(x =>
            {
                x.PenaltyScore = ComputePenalty(x, searchTerms);
            });
            var reUniterItemInfos = results.Where(x => x.PenaltyScore < Int32.MaxValue);

            return reUniterItemInfos;
        }



        private static IEnumerable<ReUniterItemInfo> FindAssetsAction(string[] searchTerms, IEnumerable<Type> unityTypes)
        {
            var searchFor = string.Join(" ",
                searchTerms.Select(x => x.StartsWith(":") ? TypeClauseFor(x.Substring(1)) + " " : MakeWildCard(x)).ToArray());
            if (searchFor.Trim().Length == 0)
                return Enumerable.Empty<ReUniterItemInfo>();
            var reUniterItemInfos = AssetDatabase.FindAssets(searchFor, new[] { "Assets" })
                .Select(x => new ReUniterItemInfo(AssetDatabase.GUIDToAssetPath(x).Substring("Assets/".Length)));
            return reUniterItemInfos;
        }

        private static IEnumerable<ReUniterItemInfo> FindGameObjectsAction(string[] searchTerms, IEnumerable<Type> unityTypes)
        {
            IEnumerable<Type> applicableTypes = new[] {typeof (GameObject)};
            if (searchTerms.Any(x=>x.StartsWith(":")))
                applicableTypes = ApplicableTypesFor(searchTerms, unityTypes).Where(x=>typeof(GameObject).IsAssignableFrom(x) || typeof(Component).IsAssignableFrom(x));
            return applicableTypes
                .SelectMany(x => FindObjectsOfType(x)).Where(x => x.name.Trim() != "") //returns only active game objects
//                .SelectMany(x => Resources.FindObjectsOfTypeAll(x)).Where(x => !AssetDatabase.Contains(x) && x.name.Trim() != "") // returns all game objects, including inactive ones
                .Select(x => new ReUniterItemInfo(BuildParentPath(x), x.name, x)); 
        }

        private static bool TypeNameMatches(Type x, string typeName)
        {
            return x.Name.StartsWith(typeName, StringComparison.OrdinalIgnoreCase);
        }


        private static IEnumerable<ReUniterItemInfo> FindRecentItemsAction(string[] searchTerms, IEnumerable<Type> unityTypes)
        {
            var selections = ReUniterSelectionHistoryTracker.PreviousSelections
                .Where(x=>x != null && x.Length > 0 && x.Any(y => y != null))
                .Select(x => new ReUniterItemInfo(x.Length + " item(s)", NameForObjects(x), x));

            if (searchTerms.Any(x => x.StartsWith(":")))
            {
                var applicableTypes = ApplicableTypesFor(searchTerms, unityTypes);
                var applicableBuiltInTypes = ApplicableBuiltInTypesFor(searchTerms);
                selections = selections.Where(x => x.UnityObjects.Any(y => MatchesType(y, applicableBuiltInTypes, applicableTypes)));
            }
            return selections;
        }

        private static IEnumerable<BuiltInAssetType> ApplicableBuiltInTypesFor(IEnumerable<string> searchTerms)
        {
            return searchTerms.Where(x => x.StartsWith(":") && x.Length > 1)
                .Select(x => x.Substring(1))
                .ToArray()
                .SelectMany(x => BuiltInAssetTypes.Where(y => y.Name.StartsWith(x, StringComparison.OrdinalIgnoreCase)));
        }

        private static IEnumerable<Type> ApplicableTypesFor(IEnumerable<string> searchTerms, IEnumerable<Type> unityTypes)
        {
            return searchTerms.Where(x => x.StartsWith(":") && x.Length>1)
                .Select(x => x.Substring(1)).ToArray().SelectMany(x => unityTypes.Where(y => TypeNameMatches(y, x)));
        }

        private static bool MatchesType(Object obj, IEnumerable<BuiltInAssetType> applicableBuiltInTypes, IEnumerable<Type> applicableTypes)
        {
            if (AssetDatabase.Contains(obj))
            {
                return applicableBuiltInTypes.Any(x => x.Matches(obj, SelectedItemName(obj)));
            }

            if (applicableTypes.Contains(obj.GetType()))
                return true;
            var gameObject = obj as GameObject;
            if (gameObject != null)
                return applicableTypes.Any(x => typeof(Component).IsAssignableFrom(x) && gameObject.GetComponent(x) != null);
            return false;
        }


        private static string TypeClauseFor(string potentialType)
        {
            if (potentialType.Trim().Length == 0)
                return "";
            return string.Join(" ", BuiltInAssetTypes.Where(x => x.Name.StartsWith(potentialType, StringComparison.OrdinalIgnoreCase)).Select(x => "t:"+x.Name).ToArray());
        }


        private static int ComputePenalty(ReUniterItemInfo item, List<string> searchTerms)
        {
            var fileName = item.NameWithoutExtension;

            var penalty = 0;
            var splitFileName = fileName.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var searchTerm in searchTerms)
            {
                if (searchTerm.StartsWith(":"))
                    continue;
                var penaltyFor = PenaltyFor(splitFileName, searchTerm);
                if (penaltyFor == Int32.MaxValue)
                    return Int32.MaxValue;
                penalty += penaltyFor;
            }
            return penalty;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")] //foreach more efficient than LINQ
        private static int PenaltyFor(string[] splitFileName, string searchTerm)
        {   
            foreach (var itemInPath in splitFileName)
                if (itemInPath == searchTerm) return 0;
            foreach (var itemInPath in splitFileName)
                if (String.Equals(itemInPath, searchTerm, StringComparison.OrdinalIgnoreCase)) return 1;
            foreach (var itemInPath in splitFileName)
                if (itemInPath.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase)) return 5;
            foreach (var itemInPath in splitFileName)
                if (itemInPath.ToLower().StartsWith(searchTerm.ToLower(), StringComparison.OrdinalIgnoreCase)) return 6;
            foreach (var itemInPath in splitFileName)
                if (itemInPath.ToLower().IndexOf(searchTerm.ToLower(), StringComparison.Ordinal) > 0) return 10;
            foreach (var itemInPath in splitFileName)
                if (itemInPath.ToLower().IndexOf(searchTerm.ToLower(), StringComparison.OrdinalIgnoreCase) > 0) return 11;

            return splitFileName.Min(x => PenaltyFor(x, searchTerm));
        }

        private static int PenaltyFor(string item, string searchTerm)
        {
            String initials = CamelCaseFor(item);
            if (initials == searchTerm.ToUpper())
                return 20;
            if (initials.StartsWith(searchTerm.ToUpper()))
                return 22;

            if (searchTerm.Contains("*") || searchTerm.Contains("?") && searchTerm.ToCharArray().Any(x=> x!=' ' && x!= '?' && x!='*'))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("^.*");
                RegexFor(searchTerm, sb);
                sb.Append(".*$");

                if (Regex.IsMatch(item, sb.ToString()))
                    return 30;
                if (Regex.IsMatch(item, sb.ToString(), RegexOptions.IgnoreCase))
                    return 31;
            }
            return Int32.MaxValue;
        }


        private static void RegexFor(string searchTerm, StringBuilder sb)
        {
            foreach (var character in searchTerm.ToCharArray())
            {
                if (character == '?')
                    sb.Append('.');
                else if (character == '*')
                    sb.Append(".*");
                else
                    sb.Append(Regex.Escape(character.ToString()));
            }
        }

        private static string RegexFor(string searchTerm)
        {
            StringBuilder sb = new StringBuilder();
            RegexFor(searchTerm, sb);
            return sb.ToString();
        }

        private static IEnumerable<ReUniterItemInfo> RefreshGameObjectInfos(string itemName)
        {
            return RefreshInfos(itemName, FindGameObjectsAction);
        }

        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        private static string BuildParentPath(Object obj)
        {
            var parentPath = "";
            GameObject gameObject = null;
            if (obj is GameObject)
                gameObject = (GameObject) obj;
            else if (obj is Component)
                gameObject = ((Component) obj).gameObject;
            if (gameObject == null)
                return "";
            var transform = gameObject.transform;
            while (transform.parent != null)
            {
                parentPath = transform.parent.name + "/" + parentPath;
                transform = transform.parent;
            }
            return parentPath + obj.name;
        }

        private void DisplaySearchResults()
        {
            itemInfos._Each((x, index) =>
                {
                    var path = x.ParentPath;
                    GUILayout.BeginHorizontal((selectAll || index == selectedIndex) ? selectedLineGuiStyle : regularLineGuiStyle);

                    GUILayout.Space(ROW_HEIGHT);
                    GUILayout.Label(Highlight(x, itemName), RichTextGuiStyle);

                    var lastRect = GUILayoutUtility.GetLastRect();
                    var iconRect = new Rect(lastRect);
                    iconRect.width = iconRect.height;
                    iconRect.xMin = ICON_XMIN;
                    var iconForFile = GetIconForFile(x.Name);
                    if (iconForFile!=null)
                        GUI.DrawTexture(iconRect, iconForFile, ScaleMode.ScaleToFit);

                    GUILayout.Space(20);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(path, RightAlignRichTextGuiStyle, GUILayout.ExpandWidth(false));

                    GUILayout.EndHorizontal();
                });
            ResizeWindow();
        }

        private void ResizeWindow()
        {
            if (mode.MoveWindowMode)
                return;
            var desiredHeight = WINDOW_HEIGHT + itemInfos.Count()*ROW_HEIGHT;
            if (Math.Abs(position.height - desiredHeight) > 1)
            {
                maxSize = new Vector2(position.width, desiredHeight);
                minSize = new Vector2(position.width, desiredHeight);
                maxSize = new Vector2(position.width, desiredHeight);
                position = PositionRect(desiredHeight);
            }
        }

        private void DisplaySearchTextField()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<b>" + mode.SearchLabel + "</b>", RichTextGuiStyle);
            EditorGUILayout.LabelField("<b><color=#05A4C4>Re</color><color=#fa3e00>Uniter</color></b>", RightAlignRichTextGuiStyle);
            GUILayout.EndHorizontal();
            const string controlName = "vh_asset_name";
            GUI.SetNextControlName(controlName);
            itemName = GUILayout.TextField((itemName ?? "").Replace("`", ""), searchLineGuiStyle, GUILayout.ExpandWidth(true)).Replace("`", "");
            GUI.FocusControl(controlName);
            ForceCaretToEndOfTextField();
        }

        private void DisplaySavePositionButton()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(mode.SearchLabel, CenteredLabelStyle);
            GUILayout.FlexibleSpace();
            
            DisplayCenteredButton("Save Window Position", StoreWindowPosition);
            GUILayout.FlexibleSpace();
            
            DisplayCenteredButton("Clear Window Position", ClearWindowPosition);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DisplayCenteredButton(string buttonName, Action onButtonClick)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(buttonName, GUILayout.Width(200)))
            {
                onButtonClick();
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void StoreWindowPosition()
        {
            EditorPrefs.SetFloat(REUNITER_POSITION_X, position.x);
            EditorPrefs.SetFloat(REUNITER_POSITION_Y, position.y);
        }

        private void ClearWindowPosition()
        {
            EditorPrefs.DeleteKey(REUNITER_POSITION_X);
            EditorPrefs.DeleteKey(REUNITER_POSITION_Y);
        }

        private void ForceCaretToEndOfTextField()
        {
            var te = (TextEditor) GUIUtility.GetStateObject(typeof (TextEditor), GUIUtility.keyboardControl);            
# if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
            te.pos = itemName.Length;
            te.selectPos = itemName.Length;
# else
            te.cursorIndex = itemName.Length;
            te.selectIndex = itemName.Length;
#endif
        }

        public void OnEnable()
        {
            darkSkin = IsDarkSkin();
            selectedLineBackgroundTex = MakeTex(600, 1, SelectionBackgroundColor());
            searchLineBackgroundTex = MakeTex(600, 1, new Color(.9f, .9f, .9f));

            selectedLineGuiStyle = new GUIStyle
            {
                richText = true,
                normal = { background = selectedLineBackgroundTex, textColor = new Color(0, 0, 0, 0) },
            };
            searchLineGuiStyle = new GUIStyle
            {
                richText = true,
                fontSize = 12,
                normal = { background = searchLineBackgroundTex },
                margin = new RectOffset(5, 5, 10, 10),
            };
            regularLineGuiStyle = new GUIStyle { richText = true, normal = { textColor = new Color(0, 0, 0, 0) } };

            
        }

        private static bool IsDarkSkin()
        {
            var isDarkSkin = EditorStyles.label.normal.textColor.r > .5f;
            return isDarkSkin;
        }

        private string ColorizePrefix()
        {
            return darkSkin?DARK_COLORIZE_PREFIX:LIGHT_COLORIZE_PREFIX;
        }

        private Color SelectionBackgroundColor()
        {
            return darkSkin ? new Color(.1f, .5f, .9f, .5f) : new Color(.9f, .95f, 1f, 1f);
        }

        private string Highlight(ReUniterItemInfo itemInfo, string searchTerms)
        {
            string item = itemInfo.Name;
            bool[] lettersToHighlight = new bool[item.Length];
            foreach (var term in searchTerms.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries))
                HighlightWord(item, term, lettersToHighlight);

            if (itemInfo.StripExtension)
            {
                var lastDot = item.LastIndexOf('.');
                if (lastDot != -1)
                    for (var i = lastDot; i < lettersToHighlight.Length; i++)
                        lettersToHighlight[i] = false;
            }

            var sb = new StringBuilder();
            var chars = item.ToCharArray();
            for (int index = 0; index < chars.Length; index++)
            {
                bool previousHighlighted = index != 0 && lettersToHighlight[index - 1];
                bool currentHighlighted = lettersToHighlight[index];
                if (!previousHighlighted && currentHighlighted)
                    sb.Append(ColorizePrefix());
                if (previousHighlighted && !currentHighlighted)
                    sb.Append(COLORIZE_SUFFIX);
                sb.Append(chars[index]);
                if (index == chars.Length - 1 && currentHighlighted)
                    sb.Append(COLORIZE_SUFFIX);
            }
            return sb.ToString();
        }

        private void HighlightWord(string item, string term, bool[] lettersToHighlight)
        {
            var index = item.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                for (int i = index; i < index + term.Length; i++)
                    lettersToHighlight[i] = true;
                return;
            }

            var initials = CamelCaseFor(item);
            if (initials.StartsWith(term.ToUpper()))
            {
                lettersToHighlight[0] = true;
                int count = term.Length-1;
                var chars = item.ToCharArray();
                for (int i = 1; i < chars.Length; i++)
                {
                    if (count <= 0)
                        return;
                    var character = chars[i];
                    if (char.IsUpper(character))
                    {
                        lettersToHighlight[i] = true;
                        count--;
                    }
                }
                return;
            }

            var match = Regex.Match(item, RegexFor(term), RegexOptions.IgnoreCase);
            if (match.Success)
            {
                for (int i = match.Index; i < match.Index + match.Length; i++)
                    lettersToHighlight[i] = true;
            }
        }

        private static string CamelCaseFor(string text)
        {
            var initials = new StringBuilder();
            initials.Append(text[0].ToString().ToUpper());
            for (int index = 1; index < text.ToCharArray().Length; index++)
            {
                var character = text.ToCharArray()[index];
                if (char.IsUpper(character))
                    initials.Append(character);
            }
            return initials.ToString();
        }

        private bool KeyDown(KeyCode keyCode)
        {
            return Event.current.type == EventType.KeyDown && Event.current.keyCode == keyCode;
        }

        private void SelectItems(IEnumerable<ReUniterItemInfo> items, bool appendToSelection, bool openAssets, bool onlyPing)
        {
            if (onlyPing)
            {
                EditorGUIUtility.PingObject(items.Select(mode.LoadItem).First().First());
                return;
            }

            var newSelection = items.Select(mode.LoadItem).SelectMany(x=>x);
            if (appendToSelection)
                newSelection = Selection.objects.ToList().Union(newSelection);
            Selection.objects = newSelection.ToArray();
            if (!openAssets)
            {
                var allGameObjects = Selection.objects.All(x => x is GameObject);
                var noneGameObject = !Selection.objects.Any(x => x is GameObject);

                if (allGameObjects)
                    EditorApplication.ExecuteMenuItem("Window/"+MenuItemPath("Hierarchy"));
                if (noneGameObject)
                {
                    EditorApplication.ExecuteMenuItem("Window/"+MenuItemPath("Project"));
                    EditorUtility.FocusProjectWindow();
                }

                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            if (openAssets)
                newSelection._Each(x=>AssetDatabase.OpenAsset(x));
        }

        private static string MenuItemPath(string menuItem)
        {
#if UNITY_2018_2_OR_NEWER
            return "General/"+menuItem;
#else
            return menuItem;
#endif
        }

        private static Object[] LoadAsset(ReUniterItemInfo itemInfo)
        {
            var parentPath = itemInfo.ParentPath.Length == 0 ? "" : itemInfo.ParentPath + "/";
            return new []{AssetDatabase.LoadAssetAtPath("Assets/" + parentPath + itemInfo.Name, typeof (Object))};
        }

        private static Object[] LoadUnityObjects(ReUniterItemInfo itemInfo)
        {
            return itemInfo.UnityObjects;
        }

    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class IEnumerableExtensions
    {
        public static void _Each<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
                action(item);
        }

        public static void _Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }
    }
}
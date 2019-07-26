using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor.CardLibraryEditor
{
    public class CardLibraryEditorWindow : EditorWindow
    {
        private const string CardLibraryJsonPathPrefsKey = "CardLibraryEditorWindow_CardLibraryJsonPath";

        [SerializeField]
        private Vector2 _scrollPosition;

        [SerializeField]
        private string _cardLibraryJsonPath;

        [SerializeField]
        private bool _onlyShowStandardEdition = true;

        private List<CardGuiItem> _cardGuiItems = new List<CardGuiItem>();

        [MenuItem("Window/ZombieBattleground/Open Card Library Editor Window")]
        private static void ShowWindow()
        {
            CardLibraryEditorWindow window = GetWindow<CardLibraryEditorWindow>();
            window.titleContent = new GUIContent("Card Library Editor");
        }

        private void OnDisable()
        {
            foreach (CardGuiItem cardGuiItem in _cardGuiItems)
            {
                cardGuiItem.Dispose();
            }
        }

        private void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                DrawMainGui();
            }
        }

        private void DrawMainGui()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("Must be in Play mode");
                return;
            }

            IAppStateManager appStateManager = GameClient.Get<IAppStateManager>();
            if (appStateManager.AppState == Enumerators.AppState.PlaySelection ||
                appStateManager.AppState == Enumerators.AppState.APP_INIT ||
                appStateManager.AppState == Enumerators.AppState.NONE)
            {
                GUILayout.Label("Game must finish loading");
                return;
            }

            _cardLibraryJsonPath =
                EditorSpecialGuiUtility.DrawPersistentFilePathField(
                    _cardLibraryJsonPath,
                    "zb_card_meta_data 'card_library.json' File Path: ",
                    "Select zb_card_meta_data 'card_library.json' File",
                    "json",
                    CardLibraryJsonPathPrefsKey
                );

            if (!File.Exists(_cardLibraryJsonPath))
            {
                GUILayout.Label("Select an existing file.");
                return;
            }

            if (GUILayout.Button("Load Card Library"))
            {
                LoadCardLibrary();
            }

            if (_cardGuiItems.Count == 0)
                return;

            if (GUILayout.Button("Save Card Library"))
            {
                SaveCardLibrary();
            }

            if (GUILayout.Button("Export All Cards as Images"))
            {
                ExportCardsAsImages();
            }

            _onlyShowStandardEdition = EditorGUILayout.ToggleLeft("Hide non-Standard edition cards", _onlyShowStandardEdition);

            EditorSpecialGuiUtility.DrawSeparator();

            DrawCardEditors();
        }

        private void DrawCardEditors()
        {
            foreach (CardGuiItem cardGuiItem in _cardGuiItems)
            {
                if (_onlyShowStandardEdition && cardGuiItem.Card.CardKey.Variant != Enumerators.CardVariant.Standard)
                    continue;

                cardGuiItem.IsOpened = EditorGUILayout.Foldout(cardGuiItem.IsOpened, cardGuiItem.Title);
                if (cardGuiItem.IsOpened)
                {
                    cardGuiItem.DrawGui();
                    if (cardGuiItem.IsChanged)
                    {
                        // If this is a standard card, refresh variants
                        if (cardGuiItem.Card.CardKey.Variant != Enumerators.CardVariant.Standard)
                            continue;

                        _cardGuiItems.Where(cardVariantGuiItem =>
                                cardVariantGuiItem.Card.CardKey.MouldId == cardGuiItem.Card.CardKey.MouldId &&
                                cardVariantGuiItem.Card.CardKey.Variant != Enumerators.CardVariant.Standard
                            )
                            .Cast<CardVariantGuiItem>()
                            .ToList()
                            .ForEach(item => item.StandardCard = (Card) cardGuiItem.Card);
                    }
                }
            }
        }

        private void ExportCardsAsImages()
        {
            // TODO: override card graphics with high-res variants
            string exportPath =
                UnityEditor.EditorUtility.SaveFolderPanel("Select Export Folder", Application.dataPath, "CardExports");
            if (!Directory.Exists(exportPath))
                throw new DirectoryNotFoundException(exportPath);

            try
            {
                for (int i = 0; i < _cardGuiItems.Count; i++)
                {
                    CardGuiItem cardGuiItem = _cardGuiItems[i];

                    if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                        "Exporting",
                        "Exporting card " + cardGuiItem.Card.Name,
                        i / (float) _cardGuiItems.Count
                    ))
                    {
                        GUIUtility.ExitGUI();
                    }

                    Texture2D shadowImage = cardGuiItem.RenderCard(CardGuiItem.CardImageWidth, CardGuiItem.CardImageHeight);
                    byte[] pngBytes = shadowImage.EncodeToPNG();
                    DestroyImmediate(shadowImage);

                    string imagePath = Path.Combine(exportPath, cardGuiItem.Card.CardKey.MouldId.Id + ".png");
                    File.WriteAllBytes(imagePath, pngBytes);
                }
            }
            finally
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
        }

        private void LoadCardLibrary()
        {
            List<Card> cards =
                LoadCardLibraryFromJsonString(File.ReadAllText(_cardLibraryJsonPath))
                    .OrderBy(card => card.CardKey, CardKey.Comparer)
                    .ToList();

            _cardGuiItems = new List<CardGuiItem>();
            foreach (Card card in cards)
            {
                if (card.CardKey.Variant == Enumerators.CardVariant.Standard)
                {
                    _cardGuiItems.Add(new CardGuiItem(this, card));
                }
                else
                {
                    CardKey standardCardKey = new CardKey(card.CardKey.MouldId, Enumerators.CardVariant.Standard);
                    Card standardCard = cards.First(c => c.CardKey == standardCardKey);
                    _cardGuiItems.Add(new CardVariantGuiItem(this, card, standardCard));
                }
            }
        }

        private void SaveCardLibrary()
        {
            // Put Standard variants at the start
            List<IReadOnlyCard> cards =
                _cardGuiItems
                    .Select(guiItem => guiItem.Card)
                    .Where(card => card.CardKey.Variant == Enumerators.CardVariant.Standard)
                    .OrderBy(card => card.CardKey.MouldId.Id)
                    .Concat(
                        _cardGuiItems
                            .Select(guiItem => guiItem.Card)
                            .Where(card => card.CardKey.Variant != Enumerators.CardVariant.Standard)
                            .OrderBy(card => card.CardKey.MouldId.Id)
                            .ThenBy(card => card.CardKey.Variant)
                            .ToList()
                    )
                    .ToList();

            string cardLibraryJson = SaveCardLibraryToJsonString(cards);
            string exportPath = UnityEditor.EditorUtility.SaveFilePanel(
                "Select File Path",
                Path.GetDirectoryName(_cardLibraryJsonPath),
                "card_library",
                "json"
            );
            File.WriteAllText(exportPath, cardLibraryJson);
            ShowNotification(new GUIContent("Saved!"));
        }

        private static List<Card> LoadCardLibraryFromJsonString(string json)
        {
            Protobuf.CardList protobufCardList = Protobuf.CardList.Parser.ParseJson(json);
            return
                protobufCardList.Cards
                    .Select(card => card.FromProtobuf())
                    .ToList();
        }

        private static string SaveCardLibraryToJsonString(List<IReadOnlyCard> cards)
        {
            ReadonlyCardList cardList = new ReadonlyCardList { Cards = cards };
            JsonSerializerSettings serializerSettings =
                JsonUtility.CreateProtobufSerializerSettings((sender, args) => Debug.LogError(args.ErrorContext.Error));
            serializerSettings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(cardList, Formatting.Indented, serializerSettings);
            /*Protobuf.CardList protobufCardList = new Protobuf.CardList
            {
                Cards =
                {
                    cards.Select(card => card.ToProtobuf())
                }
            };
            JsonFormatter jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(false));
            return JsonUtility.PrettyPrint(jsonFormatter.Format(protobufCardList));*/
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor.CardLibraryEditor
{
    public class CardLibraryEditorWindow : EditorWindow
    {
        private const string CardLibraryJsonPathPrefsKey =
            "CardLibraryEditorWindow_CardLibraryJsonPath";

        private const float CardCameraOrthographicSize = 7.5f;
        private const float CardPreviewDownscaleFactor = 3;

        private const int CardImageWidth = 1024;
        private const int CardImageHeight = 1500;

        private const int DeckEditingCardPreviewWidth = 512;
        private const int DeckEditingCardPreviewHeight = 512 / 4;

        [SerializeField]
        private Vector2 _scrollPosition;

        [SerializeField]
        private string _cardLibraryJsonPath;

        private List<CardGuiItem> _cardGuiItems = new List<CardGuiItem>();

        private bool _onlyShowStandardEdition = true;

        [MenuItem("Window/ZombieBattleground/Open Card Library Editor Window 2")]
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
                // TODO: override card graphics with high-res variants
                string exportPath = UnityEditor.EditorUtility.SaveFolderPanel("Select Export Folder", Application.dataPath, "CardExports");
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

                        Texture2D shadowImage = cardGuiItem.RenderCard(CardImageWidth, CardImageHeight);
                        byte[] pngBytes = shadowImage.EncodeToPNG();
                        DestroyImmediate(shadowImage);

                        string imagePath = Path.Combine(exportPath, cardGuiItem.Card.CardKey.MouldId.Id + ".png") ;
                        File.WriteAllBytes(imagePath, pngBytes);
                    }
                }
                finally
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
            }

            _onlyShowStandardEdition = EditorGUILayout.ToggleLeft("Hide non-Standard edition cards", _onlyShowStandardEdition);

            EditorSpecialGuiUtility.DrawSeparator();

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

        private static PictureTransform DrawPictureTransformFields(string labelPrefix, PictureTransform pictureTransform)
        {
            EditorGUI.BeginChangeCheck();

            FloatVector2 pictureTransformPosition = (FloatVector2) EditorGUILayout.Vector2Field(
                labelPrefix + " Picture Position",
                (Vector2) pictureTransform.Position
            );
            float pictureTransformScale =
                EditorGUILayout.Slider(
                    labelPrefix + " Picture Scale",
                    pictureTransform.Scale,
                    0,
                    3
                );

            bool isChanged = EditorGUI.EndChangeCheck();
            if (!isChanged)
                return pictureTransform;

            return new PictureTransform(
                pictureTransformPosition,
                pictureTransformScale
            );
        }

        [Serializable]
        private class CardVariantGuiItem : CardGuiItem
        {
            private Card _standardCard;

            public Card StandardCard
            {
                get => _standardCard;
                set
                {
                    if (_standardCard == value)
                        return;

                    _standardCard = value;
                    UpdatePreviewCard();
                }
            }

            public CardVariantGuiItem(EditorWindow owner, Card variant, Card standardCard) : base(owner)
            {
                Card = new NullAbilitiesCardProxy(new Card(
                    variant.CardKey,
                    variant.Set,
                    variant.Name,
                    variant.Cost,
                    variant.Description,
                    variant.FlavorText,
                    variant.Picture,
                    variant.Damage,
                    variant.Defense,
                    variant.Faction,
                    variant.Frame,
                    variant.Kind,
                    variant.Rank,
                    variant.Type,
                    variant.Abilities,
                    variant.PictureTransforms,
                    variant.UniqueAnimation,
                    variant.Hidden,
                    variant.Overrides
                ));
                _previewUtility = new PreviewRenderUtility();
                StandardCard = standardCard;
                UpdatePreviewCard();
            }

            protected override bool DrawCardEditorGui()
            {
                EditorGUILayout.HelpBox("Editing card variants not implemented, edit the Standard variant", MessageType.Warning);
                return false;
            }

            protected override void UpdatePreviewCard()
            {
                if (StandardCard == null)
                    return;

                Card variantCard = DataUtilities.ApplyCardVariantOverrides(Card, StandardCard);
                PreviewCard = new Card(
                    variantCard.CardKey,
                    variantCard.Set,
                    variantCard.Name,
                    variantCard.Cost,
                    variantCard.Description,
                    variantCard.FlavorText,
                    variantCard.Picture,
                    variantCard.Damage,
                    variantCard.Defense,
                    variantCard.Faction,
                    variantCard.Frame,
                    variantCard.Kind,
                    variantCard.Rank,
                    variantCard.Type,
                    variantCard.Abilities,
                    variantCard.PictureTransforms,
                    // Disable animation for preview
                    Enumerators.UniqueAnimation.None,
                    variantCard.Hidden,
                    variantCard.Overrides
                );

                Title = PreviewCard.ToString();
                ClearPreview();
            }

            private class NullAbilitiesCardProxy : IReadOnlyCard
            {
                private readonly IReadOnlyCard _original;

                public NullAbilitiesCardProxy(IReadOnlyCard original)
                {
                    _original = original;
                }

                public CardKey CardKey => _original.CardKey;

                public Enumerators.CardSet Set => _original.Set;

                public string Name => _original.Name;

                public int Cost => _original.Cost;

                public string Description => _original.Description;

                public string FlavorText => _original.FlavorText;

                public string Picture => _original.Picture;

                public int Damage => _original.Damage;

                public int Defense => _original.Defense;

                public Enumerators.Faction Faction => _original.Faction;

                public string Frame => _original.Frame;

                public Enumerators.CardKind Kind => _original.Kind;

                public Enumerators.CardRank Rank => _original.Rank;

                public Enumerators.CardType Type => _original.Type;

                // Returns null since abilities are ignored for variants anyway, but show up in JSON
                public IReadOnlyList<AbilityData> Abilities => null;

                public CardPictureTransforms PictureTransforms => _original.PictureTransforms;

                public Enumerators.UniqueAnimation UniqueAnimation => _original.UniqueAnimation;

                public bool Hidden => _original.Hidden;

                public CardOverrideData Overrides => _original.Overrides;
            }
        }

        private class CardGuiItem
        {
            [SerializeField]
            private Texture2D _cardPreviewTexture;

            [SerializeField]
            private Texture2D _deckEditingDeckCardPreviewTexture;

            [SerializeField]
            private Texture2D _boardUnitPreviewTexture;

            [SerializeField]
            private Texture2D _pastActionCardPreviewTexture;

            private bool _isBoardUnitPreviewRendering;

            [SerializeField] protected PreviewRenderUtility _previewUtility;

            [SerializeField]
            private bool _isOpened;

            [SerializeField] protected EditorWindow Owner;

            public bool IsOpened
            {
                get => _isOpened;
                set => _isOpened = value;
            }

            public IReadOnlyCard Card { get; protected set; }

            public Card PreviewCard { get; protected set; }

            public bool IsChanged { get; private set; }

            public string Title { get; protected set; }

            public CardGuiItem(EditorWindow owner, Card card) : this(owner)
            {

                Card = new Card(card);
                PreviewCard = new Card(card);
                _previewUtility = new PreviewRenderUtility();
                UpdatePreviewCard();
            }

            protected CardGuiItem(EditorWindow owner)
            {
                Owner = owner;
            }

            public void DrawGui()
            {
                IsChanged = false;
                if (_previewUtility == null)
                {
                    _previewUtility = new PreviewRenderUtility();
                }

                DrawInnerGui();
            }

            public void Dispose()
            {
                ClearPreview();
                _previewUtility?.Cleanup();
            }

            protected void ClearPreview()
            {
                if (_cardPreviewTexture != null)
                {
                    DestroyImmediate(_cardPreviewTexture);
                }

                if (_deckEditingDeckCardPreviewTexture != null)
                {
                    DestroyImmediate(_deckEditingDeckCardPreviewTexture);
                }

                if (_boardUnitPreviewTexture != null)
                {
                    DestroyImmediate(_boardUnitPreviewTexture);
                }

                if (_pastActionCardPreviewTexture != null)
                {
                    DestroyImmediate(_pastActionCardPreviewTexture);
                }
            }

            private void DrawInnerGui()
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool isChanged = DrawCardEditorGui();
                    IsChanged = isChanged;
                    RenderPreviews(isChanged);
                    DrawPreviewsGui();
                }
                EditorGUILayout.EndHorizontal();
            }

            protected virtual bool DrawCardEditorGui()
            {
                EditorGUI.BeginChangeCheck();

                CardPictureTransforms cardPictureTransforms = Card.PictureTransforms;
                Enumerators.CardSet cardSet = Card.Set;
                EditorGUILayout.BeginVertical();

                PictureTransform battlegroundPictureTransform = DrawPictureTransformFields("Battleground", cardPictureTransforms.Battleground);
                GUILayout.Space(10);
                PictureTransform deckUIPictureTransform = DrawPictureTransformFields("Deck UI", cardPictureTransforms.DeckUI);
                GUILayout.Space(10);
                PictureTransform pastActionPictureTransform = DrawPictureTransformFields("Past Action", cardPictureTransforms.PastAction);
                GUILayout.Space(15);

                cardSet = (Enumerators.CardSet) EditorGUILayout.EnumPopup("Set", cardSet);

                EditorGUILayout.EndVertical();
                bool isChanged = EditorGUI.EndChangeCheck();

                if (isChanged)
                {
                    cardPictureTransforms = new CardPictureTransforms(
                        battlegroundPictureTransform,
                        deckUIPictureTransform,
                        pastActionPictureTransform
                    );
                    Card = new Card(
                        Card.CardKey,
                        cardSet,
                        Card.Name,
                        Card.Cost,
                        Card.Description,
                        Card.FlavorText,
                        Card.Picture,
                        Card.Damage,
                        Card.Defense,
                        Card.Faction,
                        Card.Frame,
                        Card.Kind,
                        Card.Rank,
                        Card.Type,
                        Card.Abilities,
                        cardPictureTransforms,
                        Card.UniqueAnimation,
                        Card.Hidden,
                        Card.Overrides
                    );

                    UpdatePreviewCard();
                }

                return isChanged;
            }

            private void DrawPreviewsGui()
            {
                // Main card view
                Rect rectTexture = EditorGUILayout.GetControlRect(
                    false,
                    _cardPreviewTexture.height,
                    GUILayout.Width(_cardPreviewTexture.width));
                EditorGUI.DrawTextureTransparent(rectTexture,
                    _cardPreviewTexture,
                    ScaleMode.ScaleToFit,
                    rectTexture.width / rectTexture.height);

                // Board unit
                rectTexture = EditorGUILayout.GetControlRect(
                    false,
                    (int) (CardImageHeight / CardPreviewDownscaleFactor),
                    GUILayout.Width((int) (CardImageWidth / CardPreviewDownscaleFactor)));

                if (_boardUnitPreviewTexture != null)
                {
                    EditorGUI.DrawTextureTransparent(rectTexture,
                        _boardUnitPreviewTexture,
                        ScaleMode.ScaleToFit,
                        rectTexture.width / rectTexture.height);
                }

                EditorGUILayout.BeginVertical();
                {
                    // Deck editing card
                    rectTexture = EditorGUILayout.GetControlRect(
                        false,
                        _deckEditingDeckCardPreviewTexture.height,
                        GUILayout.Width(_deckEditingDeckCardPreviewTexture.width));

                    EditorGUI.DrawTextureTransparent(rectTexture,
                        _deckEditingDeckCardPreviewTexture,
                        ScaleMode.ScaleToFit,
                        rectTexture.width / rectTexture.height);

                    // Past action card
                    rectTexture = EditorGUILayout.GetControlRect(
                        false,
                        _pastActionCardPreviewTexture.height,
                        GUILayout.Width(_pastActionCardPreviewTexture.width));
                    EditorGUI.DrawTextureTransparent(rectTexture,
                        _pastActionCardPreviewTexture,
                        ScaleMode.ScaleToFit,
                        rectTexture.width / rectTexture.height);
                }
                EditorGUILayout.EndVertical();
            }

            protected virtual void UpdatePreviewCard()
            {
                PreviewCard = new Card(
                    Card.CardKey,
                    Card.Set,
                    Card.Name,
                    Card.Cost,
                    Card.Description,
                    Card.FlavorText,
                    Card.Picture,
                    Card.Damage,
                    Card.Defense,
                    Card.Faction,
                    Card.Frame,
                    Card.Kind,
                    Card.Rank,
                    Card.Type,
                    Card.Abilities,
                    Card.PictureTransforms,
                    // Disable animation for preview
                    Enumerators.UniqueAnimation.None,
                    Card.Hidden,
                    Card.Overrides
                );

                Title = PreviewCard.ToString();
            }

            private void RenderPreviews(bool force)
            {
                if (force || _cardPreviewTexture == null)
                {
                    _cardPreviewTexture = RenderCard(
                        PreviewCard,
                        (int) (CardImageWidth / CardPreviewDownscaleFactor),
                        (int) (CardImageHeight / CardPreviewDownscaleFactor)
                    );
                }

                if (force || _deckEditingDeckCardPreviewTexture == null)
                {
                    _deckEditingDeckCardPreviewTexture = RenderDeckEditingCard(
                        PreviewCard,
                        DeckEditingCardPreviewWidth,
                        DeckEditingCardPreviewHeight
                    );
                }

                if (force || _pastActionCardPreviewTexture == null)
                {
                    _pastActionCardPreviewTexture = RenderPastActionCard(
                        PreviewCard,
                        128,
                        128
                    );
                }

                if (Card.Kind != Enumerators.CardKind.ITEM && (force || _boardUnitPreviewTexture == null) && !_isBoardUnitPreviewRendering)
                {
                    RenderBoardUnit(
                        PreviewCard,
                        (int) (CardImageWidth / CardPreviewDownscaleFactor),
                        (int) (CardImageHeight / CardPreviewDownscaleFactor),
                        tex =>
                        {
                            _boardUnitPreviewTexture = tex;
                            _isBoardUnitPreviewRendering = false;
                            Owner.Repaint();
                        }
                    );
                }
            }

            public Texture2D RenderCard(int width, int height)
            {
                return RenderCard(PreviewCard, width, height);
            }

            protected Texture2D RenderCard(Card card, int width, int height)
            {
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
                CardsController cardsController = gameplayManager.GetController<CardsController>();

                CardModel previewModel = new CardModel(new WorkingCard(card, card, null, InstanceId.Invalid));
                BoardCardView boardCardView = cardsController.CreateBoardCardViewByModel(previewModel);

                Texture2D result;
                try
                {
                    result = RenderPreview(boardCardView.GameObject, width, height, orthographicSize: CardCameraOrthographicSize);
                }
                finally
                {
                    DestroyImmediate(boardCardView.GameObject);
                    boardCardView.Dispose();
                }

                return result;
            }

            protected void RenderBoardUnit(Card card, int width, int height, Action<Texture2D> onDone)
            {
                //_isBoardUnitPreviewRendering = true;
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
                CardsController cardsController = gameplayManager.GetController<CardsController>();

                card = new Card(
                    card.CardKey,
                    card.Set,
                    card.Name,
                    card.Cost,
                    card.Description,
                    card.FlavorText,
                    card.Picture,
                    card.Damage,
                    card.Defense,
                    card.Faction,
                    card.Frame,
                    card.Kind,
                    card.Rank,
                    card.Type,
                    card.Abilities,
                    card.PictureTransforms,
                    Enumerators.UniqueAnimation.None,
                    card.Hidden,
                    card.Overrides
                );

                CardModel previewModel = new CardModel(
                    new WorkingCard(card, card, new Player(InstanceId.Invalid, null, false, true), InstanceId.Invalid));
                BoardUnitView boardUnitView = new BoardUnitView(previewModel, null);
                boardUnitView.PlayArrivalAnimation(false, false);
                boardUnitView.battleframeAnimator.speed = 100000;

                int updateCounter = 5;
                void RenderDelegate()
                {
                    updateCounter--;
                    if (updateCounter > 0)
                        return;

                    EditorApplication.update -= RenderDelegate;

                    boardUnitView.battleframeAnimator.Play(0, -1, 1);
                    boardUnitView.battleframeAnimator.Update(1000f);
                    try
                    {
                        Texture2D result = RenderPreview(boardUnitView.GameObject, width, height, orthographicSize: 1.8f);
                        onDone(result);
                    }
                    finally
                    {
                        DestroyImmediate(boardUnitView.GameObject);
                        boardUnitView.Dispose();
                    }
                }

                EditorApplication.update += RenderDelegate;
            }

            protected Texture2D RenderPastActionCard(Card card, int width, int height)
            {
                ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();

                CardModel previewModel = new CardModel(
                    new WorkingCard(card, card, new Player(InstanceId.Invalid, null, false, true), InstanceId.Invalid));

                GameObject prefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");
                PastActionsPopup.PastActionParam pastActionParam = new PastActionsPopup.PastActionParam
                {
                    Model = previewModel,
                    Caller = previewModel
                };
                PastReportActionSmall reportActionSmall = new PastReportActionSmall(null, prefab, null, pastActionParam);

                Texture2D result;
                try
                {
                    reportActionSmall.SelfObject.GetComponent<RectTransform>().pivot = Vector2.one * 0.5f;
                    reportActionSmall.SelfObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    result = RenderPreview(reportActionSmall.SelfObject, width, height, orthographicSize: 8.8f, addCanvas: true, canvasScaleFactor: 0.9f);
                }
                finally
                {
                    DestroyImmediate(reportActionSmall.SelfObject);
                }

                return result;
            }

            protected Texture2D RenderDeckEditingCard(Card card, int width, int height)
            {
                ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();

                GameObject prefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/DeckCard_UI");

                DeckCardUI deckCardUi = new DeckCardUI();
                deckCardUi.Init(Instantiate(prefab));
                deckCardUi.FillCard(card, 1);
                Texture2D result;
                try
                {
                    result = RenderPreview(deckCardUi.GetGameObject(), width, height, orthographicSize: 10, addCanvas: true, canvasScaleFactor: 0.8f);
                }
                finally
                {
                    DestroyImmediate(deckCardUi.GetGameObject());
                }

                return result;
            }

            protected Texture2D RenderPreview(
                GameObject gameObject,
                int width,
                int height,
                float orthographicSize = 8,
                bool addCanvas = false,
                float canvasScaleFactor = 1)
            {
                int freeLayer = TagManager.GetFreeLayer(TagManager.LayerSearchDirection.LastToFirst);
                gameObject.SetLayerRecursively(freeLayer);

                // Init and set camera
                _previewUtility.StartStaticPreview(width, height);
                _previewUtility.Camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                _previewUtility.Camera.clearFlags = CameraClearFlags.SolidColor;
                _previewUtility.Camera.orthographic = true;
                _previewUtility.Camera.cullingMask = 1 << freeLayer;
                _previewUtility.Camera.orthographicSize = orthographicSize;
                _previewUtility.Camera.transform.position = new Vector3(0f, 0f, -10f);
                _previewUtility.Camera.nearClipPlane = 0.3f;
                _previewUtility.Camera.farClipPlane = 1000f;

                GameObject tempCanvasGameObject = null;
                if (addCanvas)
                {
                    tempCanvasGameObject = new GameObject("_TempPreviewCanvas");
                    tempCanvasGameObject.layer = freeLayer;
                    Canvas canvas = tempCanvasGameObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = _previewUtility.Camera;
                    canvas.scaleFactor = canvasScaleFactor;
                    canvas.pixelPerfect = false;

                    gameObject.transform.SetParent(tempCanvasGameObject.transform);
                }

                gameObject.transform.position = Vector3.zero;
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;

                try
                {
                    // Render scene
                    RenderTexture.active = _previewUtility.RenderTexture;
                    GL.Clear(true, true, _previewUtility.Camera.backgroundColor);

                    _previewUtility.Camera.aspect = width / (float) height;
                    _previewUtility.Camera.Render();

                    RenderTexture.active = null;

                    // Get result
                    Texture2D result = _previewUtility.EndStaticPreview();
                    result.name = gameObject.name + "_Preview";
                    return result;
                }
                finally
                {
                    if (tempCanvasGameObject != null)
                    {
                        DestroyImmediate(tempCanvasGameObject);
                    }
                }
            }
        }

        private class CardKeyedCollection : KeyedCollection<CardKey, Card>
        {
            public CardKeyedCollection()
            {
            }
            protected override CardKey GetKeyForItem(Card item)
            {
                return item.CardKey;
            }
        }
    }
}

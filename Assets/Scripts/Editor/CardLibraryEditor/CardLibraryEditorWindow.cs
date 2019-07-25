using System;
using System.Collections.Generic;
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

        private IList<Card> _cards;
        private List<CardGuiItem> _cardGuiItems = new List<CardGuiItem>();

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
                _cards = JsonConvert.DeserializeObject<CardList>(
                        File.ReadAllText(_cardLibraryJsonPath),
                        JsonUtility.CreateStrictSerializerSettings((sender, args) => throw args.ErrorContext.Error)
                    ).Cards;
                _cardGuiItems = _cards.Select(card => new CardGuiItem(this, card)).ToList();
            }

            if (_cards == null)
                return;

            if (GUILayout.Button("Export All Cards as Images"))
            {
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

                        Texture2D shadowImage = cardGuiItem.RenderCard(
                            cardGuiItem.Card,
                            CardImageWidth,
                            CardImageHeight
                        );
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

            EditorSpecialGuiUtility.DrawSeparator();

            foreach (CardGuiItem cardGuiItem in _cardGuiItems)
            {
                cardGuiItem.IsOpened = EditorGUILayout.Foldout(cardGuiItem.IsOpened, cardGuiItem.Card.ToString());
                if (cardGuiItem.IsOpened)
                {
                    cardGuiItem.DrawGui();
                }
            }
        }

        [Serializable]
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

            [SerializeField]
            private PreviewRenderUtility _previewUtility;

            [SerializeField]
            private bool _isOpened;

            [SerializeField]
            private EditorWindow Owner;

            public bool IsOpened
            {
                get => _isOpened;
                set => _isOpened = value;
            }

            public IReadOnlyCard OriginalCard { get; }
            public Card Card { get; private set; }

            public CardGuiItem(EditorWindow owner, Card card)
            {
                Owner = owner;
                OriginalCard = card;
                Card = new Card(card);
                _previewUtility = new PreviewRenderUtility();
            }

            public void DrawGui()
            {
                if (_previewUtility == null)
                {
                    _previewUtility = new PreviewRenderUtility();
                }

                DrawInnerGui();
            }

            private void DrawInnerGui()
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();

                    PictureTransform pictureTransform = Card.PictureTransform;
                    FloatVector3 pictureTransformPosition = pictureTransform.Position;
                    EditorGUILayout.BeginVertical();
                    {
                        pictureTransformPosition =
                            ((Vector3) EditorGUILayout.Vector2Field("Battleframe Picture Position", pictureTransformPosition.ToVector3()))
                                .ToFloatVector3();
                        float scaleFactor = Mathf.Max(pictureTransform.Scale.X, pictureTransform.Scale.Y);
                        FloatVector3 pictureTransformScale = new FloatVector3(EditorGUILayout.Slider("Battleframe Picture Scale", scaleFactor, 0, 3));
                        pictureTransform =
                            new PictureTransform(
                                pictureTransformPosition,
                                pictureTransformScale
                            );
                    }

                    EditorGUILayout.EndVertical();
                    bool isChanged = EditorGUI.EndChangeCheck();

                    if (isChanged)
                    {
                        Card = new Card(
                            Card.CardKey,
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
                            pictureTransform,
                            Enumerators.UniqueAnimation.None,
                            Card.Hidden
                        );
                    }

                    RenderPreviews(isChanged);

                    // Card
                    Rect rectTexture = EditorGUILayout.GetControlRect(
                        false, _cardPreviewTexture.height, GUILayout.Width(_cardPreviewTexture.width));
                    EditorGUI.DrawTextureTransparent(rectTexture, _cardPreviewTexture, ScaleMode.ScaleToFit, rectTexture.width / rectTexture.height);

                    // Board unit
                    if (_boardUnitPreviewTexture != null)
                    {
                        rectTexture = EditorGUILayout.GetControlRect(
                            false, _boardUnitPreviewTexture.height, GUILayout.Width(_boardUnitPreviewTexture.width));;
                        EditorGUI.DrawTextureTransparent(rectTexture, _boardUnitPreviewTexture, ScaleMode.ScaleToFit, rectTexture.width / rectTexture.height);
                    }

                    EditorGUILayout.BeginVertical();
                    {
                        // Deck editing card
                        rectTexture = EditorGUILayout.GetControlRect(
                            false, _deckEditingDeckCardPreviewTexture.height, GUILayout.Width(_deckEditingDeckCardPreviewTexture.width));;
                        EditorGUI.DrawTextureTransparent(rectTexture, _deckEditingDeckCardPreviewTexture, ScaleMode.ScaleToFit, rectTexture.width / rectTexture.height);

                        // Past action card
                        rectTexture = EditorGUILayout.GetControlRect(
                            false, _pastActionCardPreviewTexture.height, GUILayout.Width(_pastActionCardPreviewTexture.width));
                        EditorGUI.DrawTextureTransparent(rectTexture, _pastActionCardPreviewTexture, ScaleMode.ScaleToFit, rectTexture.width / rectTexture.height);
                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndHorizontal();
            }

            private void RenderPreviews(bool force)
            {
                if (force || _cardPreviewTexture == null)
                {
                    _cardPreviewTexture = RenderCard(
                        Card,
                        (int) (CardImageWidth / CardPreviewDownscaleFactor),
                        (int) (CardImageHeight / CardPreviewDownscaleFactor)
                    );
                }

                if (force || _deckEditingDeckCardPreviewTexture == null)
                {
                    _deckEditingDeckCardPreviewTexture = RenderDeckEditingCard(
                        Card,
                        DeckEditingCardPreviewWidth,
                        DeckEditingCardPreviewHeight
                    );
                }

                if (force || _pastActionCardPreviewTexture == null)
                {
                    _pastActionCardPreviewTexture = RenderPastActionCard(
                        Card,
                        128,
                        128
                    );
                }

                if (Card.Kind != Enumerators.CardKind.ITEM && (force || _boardUnitPreviewTexture == null) && !_isBoardUnitPreviewRendering)
                {
                    RenderBoardUnit(
                        Card,
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

            public void Dispose()
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

                _previewUtility?.Cleanup();
            }

            public Texture2D RenderCard(Card card, int width, int height)
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

            private void RenderBoardUnit(Card card, int width, int height, Action<Texture2D> onDone)
            {
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
                CardsController cardsController = gameplayManager.GetController<CardsController>();

                card = new Card(
                    card.CardKey,
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
                    card.PictureTransform,
                    Enumerators.UniqueAnimation.None,
                    card.Hidden
                );

                CardModel previewModel = new CardModel(
                    new WorkingCard(card, card, new Player(InstanceId.Invalid, null, false, true), InstanceId.Invalid));
                BoardUnitView boardUnitView = new BoardUnitView(previewModel, null);
                boardUnitView.PlayArrivalAnimation(false, false);
                boardUnitView.battleframeAnimator.Play(0, -1, 1);
                boardUnitView.battleframeAnimator.Update(0.1f);

                int updateCounter = 4;
                void RenderDelegate()
                {
                    updateCounter--;
                    if (updateCounter > 0)
                        return;

                    EditorApplication.update -= RenderDelegate;

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

            private Texture2D RenderPastActionCard(Card card, int width, int height)
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

            private Texture2D RenderDeckEditingCard(Card card, int width, int height)
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

            private Texture2D RenderPreview(
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
    }
}

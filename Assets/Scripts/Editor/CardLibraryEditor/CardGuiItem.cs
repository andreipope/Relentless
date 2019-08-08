using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground.Editor.CardLibraryEditor
{
    public class CardGuiItem
    {
        public const int CardImageWidth = 1024;
        public const int CardImageHeight = 1500;

        private const float CardCameraOrthographicSize = 7.5f;
        private const float CardPreviewDownscaleFactor = 3;

        private const int DeckEditingCardPreviewWidth = 512;
        private const int DeckEditingCardPreviewHeight = 512 / 4;

        private Texture2D _cardPreviewTexture;
        private Texture2D _deckEditingDeckCardPreviewTexture;
        private Texture2D _boardUnitPreviewTexture;
        private Texture2D _pastActionCardPreviewTexture;
        private bool _isBoardUnitPreviewRendering;
        private bool _isOpened;

        protected PreviewRenderUtility _previewUtility;
        protected EditorWindow Owner { get; }

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
                Object.DestroyImmediate(_cardPreviewTexture);
            }

            if (_deckEditingDeckCardPreviewTexture != null)
            {
                Object.DestroyImmediate(_deckEditingDeckCardPreviewTexture);
            }

            if (_boardUnitPreviewTexture != null)
            {
                Object.DestroyImmediate(_boardUnitPreviewTexture);
            }

            if (_pastActionCardPreviewTexture != null)
            {
                Object.DestroyImmediate(_pastActionCardPreviewTexture);
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
                Object.DestroyImmediate(boardCardView.GameObject);
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
                    Object.DestroyImmediate(boardUnitView.GameObject);
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
                Object.DestroyImmediate(reportActionSmall.SelfObject);
            }

            return result;
        }

        protected Texture2D RenderDeckEditingCard(Card card, int width, int height)
        {
            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();

            GameObject prefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/DeckCard_UI");

            DeckCardUI deckCardUi = new DeckCardUI();
            deckCardUi.Init(Object.Instantiate(prefab));
            deckCardUi.FillCard(card);
            Texture2D result;
            try
            {
                result = RenderPreview(deckCardUi.GetGameObject(), width, height, orthographicSize: 10, addCanvas: true, canvasScaleFactor: 0.8f);
            }
            finally
            {
                Object.DestroyImmediate(deckCardUi.GetGameObject());
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
                    Object.DestroyImmediate(tempCanvasGameObject);
                }
            }
        }

        protected static PictureTransform DrawPictureTransformFields(string labelPrefix, PictureTransform pictureTransform)
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Iap;
using OneOf;
using OneOf.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.UI;
using CardKey = Loom.ZombieBattleground.Data.CardKey;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class PackOpenerPageWithNavigationBar : IUIElement
    {
        private const float FadeDuration = 1f;
        private const float CardHideAnimationDuration = 0.2f;
        private const float CardAppearAnimationDuration = 0.65f;
        private const float CardFlipAnimationDuration = 0.3f;
        private const float BoardCardViewOpenedCardScale = 32.55f;
        private static readonly Vector3 CardHidePosition = new Vector3(0, -15, 0);

        private static readonly ILog Log = Logging.GetLog(nameof(PackOpenerPageWithNavigationBar));

        private readonly List<PackObject> _packObjects = new List<PackObject>();

        private readonly List<PackCard> _openedCards = new List<PackCard>();

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private PlasmaChainBackendFacade _plasmaChainBackendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private CardsController _cardsController;

        private Material _grayScaleMaterial;

        private GameObject _selfPage;

        private GameObject _openedPackPanel;

        private Image _openedPackPanelDimBackground;

        private Button _openedPackPanelCloseButton;

        private Button _openedPackPanelOpenNextPackButton;

        private GameObject _packObjectsRoot;

        private Transform[] _cardPositions;

        private TextMeshProUGUI _currentPackTypeText;

        private TextMeshProUGUI _currentPackTypeAmountText;

        private Button _openButton;

        private Enumerators.MarketplaceCardPackType? _selectedPackType;

        private PackOpenerControllerBase _controller;

        private CardInfoPopupHandler _cardInfoPopupHandler;


        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            _grayScaleMaterial = _loadObjectsManager.GetObjectByPath<Material>("Materials/UI-Default-Grayscale");
        }

        public async void Show()
        {
            _selectedPackType = null;
            _controller = new NormalPackOpenerController();
            //_controller = new TutorialPackOpenerController();

            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();

            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPageWithNavigationBar"),
                _uiManager.Canvas.transform,
                false);

            _openedPackPanel = _selfPage.transform.Find("OpenedPackPanel").gameObject;
            _openedPackPanelDimBackground = _openedPackPanel.transform.Find("DimBackground").GetComponent<Image>();
            _openedPackPanelCloseButton = _openedPackPanel.transform.Find("BottomButtons/CloseButton").GetComponent<Button>();
            _openedPackPanelOpenNextPackButton = _openedPackPanel.transform.Find("BottomButtons/OpenNextPackButton").GetComponent<Button>();

            _packObjectsRoot = _selfPage.transform.Find("PackOpener/Packs/Layout/PacksRoot").gameObject;
            _currentPackTypeText = _selfPage.transform.Find("PackOpener/CurrentPackTypeText").GetComponent<TextMeshProUGUI>();
            _currentPackTypeAmountText = _selfPage.transform.Find("PackOpener/CurrentPackTypeAmountText").GetComponent<TextMeshProUGUI>();
            _openButton = _selfPage.transform.Find("PackOpener/OpenButton").GetComponent<Button>();
            _cardPositions =
                _selfPage.transform.Find("PackOpener/CardPositionsList")
                    .gameObject
                    .GetComponent<GameObjectList>()
                    .Items
                    .Select(go => go.transform)
                    .ToArray();

            _openedPackPanel.SetActive(false);
            _openedPackPanel.transform.SetParent(_uiManager.Canvas.transform);

            _currentPackTypeText.text = "";
            _currentPackTypeAmountText.text = "";
            _openButton.onClick.AddListener(OpenButtonHandler);
            _openedPackPanelCloseButton.onClick.AddListener(OpenedPackPanelCloseButtonHandler);
            _openedPackPanelOpenNextPackButton.onClick.AddListener(OpenedPackPanelOpenNextPackHandler);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_PACKS);
            _uiManager.DrawPopup<AreaBarPopup>();
            _openedPackPanel.transform.SetAsLastSibling();

            OneOf<Success,Exception> result = await _controller.Start();
            if (result.IsT1)
            {
                Log.Warn("Failed to start pack opener: " + result.Value);
                FailAndGoToMainMenu();
                return;
            }

            CreatePackObjects();
        }

        public void Hide()
        {
            Dispose();

            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        public void Update()
        {
            if (_selfPage == null || !_selfPage.activeInHierarchy)
                return;

            _cardInfoPopupHandler.Update();
        }

        public void Dispose()
        {
            ClearPackObjects();
            ClearOpenedCards();

            Object.Destroy(_openedPackPanel);

            if (_selfPage == null)
                return;

            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        #endregion

        private void OpenedPackPanelOpenNextPackHandler()
        {
            _openedPackPanelCloseButton.interactable = false;
            _openedPackPanelOpenNextPackButton.interactable = false;

            PlayCardsHideAnimation(async () =>
            {
                ClearOpenedCards();
                await OpenSelectedPack();
            });
        }

        private void OpenedPackPanelCloseButtonHandler()
        {
            _openedPackPanelCloseButton.interactable = false;
            _openedPackPanelOpenNextPackButton.interactable = false;

            PlayCardsHideAnimation(ClearOpenedCards);
            CanvasGroup openedPackCanvasGroup = _openedPackPanel.GetComponent<CanvasGroup>();

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(CardHideAnimationDuration);
            sequence.Append(openedPackCanvasGroup.DOFade(0f, FadeDuration));
            sequence.AppendCallback(() =>
            {
                _openedPackPanel.SetActive(false);
                CreatePackObjects();
            });
        }

        private async void OpenButtonHandler()
        {
            await OpenSelectedPack();
        }

        private void PlayCardsHideAnimation(Action onEnd)
        {
            for (int i = 0; i < _openedCards.Count; i++)
            {
                PackCard packCard = _openedCards[i];
                Sequence hideSequence = DOTween.Sequence();
                hideSequence.AppendInterval(i * 0.1f);
                hideSequence.Append(packCard.GameObject.transform.DOMove(CardHidePosition, CardHideAnimationDuration));
                if (i == _openedCards.Count - 1)
                {
                    hideSequence.AppendCallback(() => onEnd());
                }
            }
        }

        private void SetSelectedPackType(Enumerators.MarketplaceCardPackType? packType)
        {
            _selectedPackType = packType;
            if (packType != null)
            {
                uint amount = _controller.GetPackTypeAmount(packType.Value);
                _currentPackTypeText.text = $"{packType.ToString().ToUpperInvariant()} PACK";
                _currentPackTypeAmountText.text = amount.ToString();
            }
            else
            {
                _currentPackTypeText.text = "";
                _currentPackTypeAmountText.text = "";
            }

            UpdateOpenButtonState();
        }

        private async Task OpenSelectedPack()
        {
            Assert.IsTrue(_selectedPackType != null);
            OneOf<IReadOnlyList<CardKey>, Exception> result = await _controller.OpenPack(_selectedPackType.Value);
            if (result.IsT1)
            {
                ExceptionReporter.LogExceptionAsWarning(Log, result.AsT1);
                FailAndGoToMainMenu("Loading cards failed.\n Please try again.");
                return;
            }

            SetSelectedPackType(_selectedPackType.Value);

            _openedPackPanelCloseButton.interactable = true;
            _openedPackPanelOpenNextPackButton.interactable = true;
            _openedPackPanelOpenNextPackButton.gameObject.SetActive(_controller.GetPackTypeAmount(_selectedPackType.Value) > 0);

            IReadOnlyList<CardKey> cardKeys = result.AsT0;
            List<Card> cards = _dataManager.CachedCardsLibraryData.GetCardsByCardKeys(cardKeys, true).ToList();
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                if (card == null)
                {
                    card = CreateEmptyPodFakeCard(cardKeys[i]);
                    cards[i] = card;
                }
            }

            _openedPackPanel.SetActive(true);
            CanvasGroup openedPackCanvasGroup = _openedPackPanel.GetComponent<CanvasGroup>();
            openedPackCanvasGroup.DOFade(1f, FadeDuration);

            CreateOpenedCards(cards);
        }

        private void CreateOpenedCards(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                PackCard packCard = new PackCard(_openedPackPanel.transform, _loadObjectsManager, _cardsController, card);
                packCard.GameObject.transform.position = _cardPositions[i].position;
                packCard.GameObject.transform.localScale = Vector3.zero;
                packCard.Button.onClick.AddListener(() =>
                {
                    if (!packCard.IsFlipped)
                    {
                        packCard.Flip();
                    }
                    else
                    {
                        if (_cardInfoPopupHandler.IsStateChanging)
                            return;

                        _cardInfoPopupHandler.SelectCard(packCard.BoardCardView);

                        // Nicely hide the clicked card to avoid duplicating it visually
                        packCard.GameObject.SetActive(false);

                        void OnClosed()
                        {
                            packCard.GameObject.SetActive(true);
                            _cardInfoPopupHandler.Closed -= OnClosed;
                        }

                        _cardInfoPopupHandler.Closed += OnClosed;
                    }
                });
                packCard.PlayAppearAnimation();
                _openedCards.Add(packCard);
            }
        }

        private void CreatePackObjects()
        {
            ClearPackObjects();

            IReadOnlyList<Enumerators.MarketplaceCardPackType> packTypes = _controller.ShownPackTypes;
            foreach (Enumerators.MarketplaceCardPackType packType in packTypes)
            {
                if (_controller.GetPackTypeAmount(packType) == 0)
                    continue;

                PackObject packObject = new PackObject(_packObjectsRoot.transform, _loadObjectsManager, packType);
                _packObjects.Add(packObject);
            }

            if (_packObjects.Count > 0)
            {
                SetSelectedPackType(_packObjects[0].PackType);
            }
            else
            {
                SetSelectedPackType(null);
            }
        }

        private static Card CreateEmptyPodFakeCard(CardKey cardKey)
        {
            return new Card(
                cardKey,
                $"Card #{cardKey.MouldId.Id}",
                0,
                "",
                "",
                "embryo_pod",
                0,
                0,
                Enumerators.Faction.AIR,
                "",
                Enumerators.CardKind.CREATURE,
                Enumerators.CardRank.MINION,
                Enumerators.CardType.UNDEFINED,
                new List<AbilityData>(),
                new PictureTransform(new FloatVector3(0), new FloatVector3(0.4f)),
                Enumerators.UniqueAnimation.None,
                true
            );
        }

        private void UpdateOpenButtonState()
        {
            _openButton.interactable = _selectedPackType != null && _controller.GetPackTypeAmount(_selectedPackType.Value) > 0;
            _openButton.GetComponent<Image>().material = _openButton.interactable ? null : _grayScaleMaterial;
        }

        private void FailAndGoToMainMenu(string customMessage = null)
        {
            _uiManager.HidePopup<LoadingOverlayPopup>();
            _uiManager.DrawPopup<WarningPopup>(customMessage ?? "Something went wrong.\n Please try again.");
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void ClearOpenedCards()
        {
            foreach (PackCard packCard in _openedCards)
            {
                packCard.Dispose();
            }

            _openedCards.Clear();
        }

        private void ClearPackObjects()
        {
            foreach (PackObject packTypeButton in _packObjects)
            {
                packTypeButton.Dispose();
            }

            _packObjects.Clear();
        }

        #region Util

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CHANGE_SCREEN,
                    Constants.SfxSoundVolume,
                    false,
                    false,
                    true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #endregion

        private class PackObject
        {
            public Enumerators.MarketplaceCardPackType PackType { get; }

            public GameObject GameObject { get; }

            public PackObject(Transform parent, ILoadObjectsManager loadObjectsManager, Enumerators.MarketplaceCardPackType packType)
            {
                PackType = packType;
                GameObject = Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/PackOpenerPack"), parent);
            }

            public void Dispose()
            {
                Object.Destroy(GameObject);
            }
        }

        private class PackCard
        {
            public GameObject GameObject { get; }

            public GameObject CardBackGameObject { get; }

            public BoardCardView BoardCardView { get; }

            public Button Button { get; }

            public bool IsFlipped { get; private set; }

            public PackCard(Transform parent, ILoadObjectsManager loadObjectsManager, CardsController cardsController, IReadOnlyCard card)
            {
                GameObject = Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/PackOpenerCard"), parent);
                CardBackGameObject = GameObject.transform.Find("CardBack").gameObject;
                Button = GameObject.transform.Find("Button").GetComponent<Button>();

                CardModel cardModel = new CardModel(new WorkingCard(card, card, null));
                BoardCardView = cardsController.CreateBoardCardViewByModel(cardModel);
                BoardCardView.Transform.SetParent(GameObject.transform);

                BoardCardView.Transform.localScale = Vector3.one * BoardCardViewOpenedCardScale;
                BoardCardView.Transform.localRotation = Quaternion.identity;
                BoardCardView.Transform.localPosition = Vector3.zero;
                BoardCardView.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
                BoardCardView.SetHighlightingEnabled(false);
                BoardCardView.GameObject.SetActive(false);
            }

            public void Flip()
            {
                if (IsFlipped)
                    return;

                IsFlipped = true;
                BoardCardView.Transform.localEulerAngles = new Vector3(0, -90, 0);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(CardBackGameObject.transform.DORotate(new Vector3(0, 90, 0), CardFlipAnimationDuration / 2f).SetEase(Ease.InSine));
                sequence.AppendCallback(() =>
                {
                    CardBackGameObject.SetActive(false);
                    BoardCardView.GameObject.SetActive(true);
                });
                sequence.Append(BoardCardView.Transform.DORotate(new Vector3(0, 0, 0), CardFlipAnimationDuration / 2f).SetEase(Ease.OutSine));
            }

            public void Dispose()
            {
                Object.Destroy(GameObject);
            }

            public void PlayAppearAnimation()
            {
                Sequence appearSequence = DOTween.Sequence();
                appearSequence.AppendInterval(Random.Range(0f, 0.2f));
                appearSequence.Append(GameObject.transform.DOScale(Vector3.one, CardAppearAnimationDuration).SetEase(Ease.InFlash));
            }
        }

        private abstract class PackOpenerControllerBase
        {
            public abstract IReadOnlyList<Enumerators.MarketplaceCardPackType> ShownPackTypes { get; }

            public abstract uint GetPackTypeAmount(Enumerators.MarketplaceCardPackType packType);

            public abstract Task<OneOf<Success, Exception>> Start();

            public abstract Task<OneOf<IReadOnlyList<CardKey>, Exception>> OpenPack(Enumerators.MarketplaceCardPackType packType);
        }

        private class TutorialPackOpenerController : PackOpenerControllerBase
        {
            public override IReadOnlyList<Enumerators.MarketplaceCardPackType> ShownPackTypes { get; } =
                (Enumerators.MarketplaceCardPackType[]) Enum.GetValues(typeof(Enumerators.MarketplaceCardPackType));

            public override uint GetPackTypeAmount(Enumerators.MarketplaceCardPackType packType)
            {
                return 1;
            }

            public override Task<OneOf<Success, Exception>> Start()
            {
                return Task.FromResult((OneOf<Success, Exception>) new Success());
            }

            public override Task<OneOf<IReadOnlyList<CardKey>, Exception>> OpenPack(Enumerators.MarketplaceCardPackType packType)
            {
                // TODO: Load cards from tutorial here
                List<CardKey> cards = new List<CardKey>
                {
                    CardKey.FromCardTokenId(500),
                    CardKey.FromCardTokenId(500),
                    CardKey.FromCardTokenId(500),
                    CardKey.FromCardTokenId(500),
                    CardKey.FromCardTokenId(500)
                };
                return Task.FromResult((OneOf<IReadOnlyList<CardKey>, Exception>) cards);
            }
        }

        private class NormalPackOpenerController : PackOpenerControllerBase
        {
            private readonly Dictionary<Enumerators.MarketplaceCardPackType, uint> _packTypeToPackAmount = new Dictionary<Enumerators.MarketplaceCardPackType, uint>();
            private readonly IUIManager _uiManager;
            private readonly ILoadObjectsManager _loadObjectsManager;
            private readonly PlasmaChainBackendFacade _plasmaChainBackendFacade;
            private readonly BackendDataControlMediator _backendDataControlMediator;
            private readonly ITutorialManager _tutorialManager;
            private readonly IDataManager _dataManager;
            private readonly BackendFacade _backendFacade;

            public NormalPackOpenerController()
            {
                _uiManager = GameClient.Get<IUIManager>();
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();
                _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                _tutorialManager = GameClient.Get<ITutorialManager>();
                _dataManager = GameClient.Get<IDataManager>();
                _backendFacade = GameClient.Get<BackendFacade>();
            }

            public override IReadOnlyList<Enumerators.MarketplaceCardPackType> ShownPackTypes { get; } = new[]
            {
                Enumerators.MarketplaceCardPackType.Booster
            };

            public override uint GetPackTypeAmount(Enumerators.MarketplaceCardPackType packType)
            {
                _packTypeToPackAmount.TryGetValue(packType, out uint amount);
                return amount;
            }

            public override async Task<OneOf<Success, Exception>> Start()
            {
                try
                {
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Checking your packs...");
                    using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
                    {
                        // Claim unclaimed packs
                        Log.Debug("Call GetPendingMintingTransactionReceipts");
                        Protobuf.GetPendingMintingTransactionReceiptsResponse mintingTransactionReceipts =
                            await _backendFacade.GetPendingMintingTransactionReceipts(_backendDataControlMediator.UserDataModel.UserId);
                        if (mintingTransactionReceipts.ReceiptCollection.Receipts.Count > 0)
                        {
                            _uiManager.DrawPopup<LoadingOverlayPopup>("Claiming packs...");
                        }

                        foreach (Protobuf.MintingTransactionReceipt receiptProtobuf in mintingTransactionReceipts.ReceiptCollection.Receipts)
                        {
                            AuthFiatApiFacade.TransactionReceipt receipt = receiptProtobuf.FromProtobuf();
                            Log.Debug($"Claiming receipt with TxId {receipt.TxId}");
                            try
                            {
                                await _plasmaChainBackendFacade.ClaimPacks(client, receipt);
                                Log.Info($"Claimed receipt with TxId {receipt.TxId} successfully!!");
                            }
                            catch (TxCommitException e)
                            {
                                // Already claimed?
                                Log.Warn("Already claimed? " + e);
                            }

                            Log.Debug($"Confirming receipt with TxId {receipt.TxId}");
                            await _backendFacade.ConfirmPendingMintingTransactionReceipt(_backendDataControlMediator.UserDataModel.UserId, receipt.TxId);
                        }

                        if (mintingTransactionReceipts.ReceiptCollection.Receipts.Count > 0)
                        {
                            _uiManager.DrawPopup<LoadingOverlayPopup>("Loading your packs...");
                        }

                        for (int i = 0; i < ShownPackTypes.Count; ++i)
                        {
                            await UpdatePackBalanceAmount(client, (Enumerators.MarketplaceCardPackType) i);
                        }
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
                finally
                {
                    _uiManager.HidePopup<LoadingOverlayPopup>();
                }

                return new Success();
            }

            public override async Task<OneOf<IReadOnlyList<CardKey>, Exception>> OpenPack(Enumerators.MarketplaceCardPackType packType)
            {
                _uiManager.DrawPopup<LoadingOverlayPopup>("Loading your cards...");
                try
                {
                    using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
                    {
                        IReadOnlyList<CardKey> cardKeys = await _plasmaChainBackendFacade.CallOpenPack(client, packType);
                        _packTypeToPackAmount[packType]--;
                        return OneOf<IReadOnlyList<CardKey>, Exception>.FromT0(cardKeys);
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
                finally
                {
                    _uiManager.HidePopup<LoadingOverlayPopup>();
                }
            }

            private async Task UpdatePackBalanceAmount(DAppChainClient client, Enumerators.MarketplaceCardPackType packType)
            {
                Log.Debug($"{nameof(UpdatePackBalanceAmount)}({nameof(packType)} = {packType})");
                try
                {
                    _packTypeToPackAmount[packType] = await _plasmaChainBackendFacade.GetPackTypeBalance(client, packType);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to get balance for pack type {packType}", e);
                }
            }
        }
    }
}

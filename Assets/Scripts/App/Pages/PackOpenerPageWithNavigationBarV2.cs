using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using UnityEngine.UI;
using CardKey = Loom.ZombieBattleground.Data.CardKey;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PackOpenerPageWithNavigationBarV2 : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PackOpenerPageWithNavigationBarV2));

        private readonly List<PackObject> _packObjects = new List<PackObject>();

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private PlasmaChainBackendFacade _plasmaChainBackendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private GameObject _openedPackPanel;

        private GameObject _packObjectsRoot;

        private Transform[] _cardPositions;

        private TextMeshProUGUI _currentPackTypeText;

        private TextMeshProUGUI _currentPackTypeAmountText;

        private Button _openButton;

        private Enumerators.MarketplaceCardPackType? _selectedPackType;

        private PackOpenerControllerBase _controller;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public async void Show()
        {
            _selectedPackType = null;
            _controller = new NormalPackOpenerController();

            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPageWithNavigationBarV3"),
                _uiManager.Canvas.transform,
                false);

            _openedPackPanel = _selfPage.transform.Find("OpenedPackPanel").gameObject;
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
            _currentPackTypeText.text = "";
            _currentPackTypeAmountText.text = "";
            _openButton.onClick.AddListener(OpenButtonHandler);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_PACKS);
            _uiManager.DrawPopup<AreaBarPopup>();

            OneOf<Success,Exception> result = await _controller.Start();
            if (result.IsT1)
            {
                Log.Warn("Failed to start pack opener: " + result.Value);
                FailAndGoToMainMenu();
                return;
            }

            CreatePackTypeButtons();
        }

        public void Hide()
        {
            Dispose();

            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        public void Update()
        {

        }

        public void Dispose()
        {
            ClearPackObjects();

            if (_selfPage == null)
                return;

            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        private void ClearPackObjects()
        {
            foreach (PackObject packTypeButton in _packObjects)
            {
                packTypeButton.Dispose();
            }

            _packObjects.Clear();
        }

        private void ChangeState(State newState)
        {
            /*Assert.IsFalse(_selfPage == null);

            if (_state == newState)
                return;

            Log.Info($"ChangeState: prev:{_state.ToString()} next:{newState.ToString()}");

            _state = newState;
            switch (_state)
            {
                case State.Undefined:
                    break;
                case State.InitializingStore:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Initializing store...");
                    break;
                case State.ClaimingPendingPurchases:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Checking for purchases...");
                    break;
                case State.WaitForInput:
                    _uiManager.HidePopup<LoadingOverlayPopup>();
                    break;
                case State.InitiatedPurchase:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Activating Purchase...");
                    break;
                case State.Purchasing:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Processing Purchase...");
                    break;
                case State.RequestFiatValidation:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Processing payment...");
                    break;
                case State.RequestFiatTransaction:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs");
                    break;
                case State.RequestPack:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs.");
                    break;
                case State.WaitForRequestPackResponse:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs..");
                    break;
                case State.RequestFiatClaim:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs...");
                    break;
                case State.TransitionToPackOpener:
                    _unfinishedState = State.Undefined;
                    OnFinishRequestPack();
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(_state), (int) _state, typeof(State));
            }*/
        }

        private void CreatePackTypeButtons()
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
        }

        private void SetSelectedPackType(Enumerators.MarketplaceCardPackType packType)
        {
            _selectedPackType = packType;
            uint amount = _controller.GetPackTypeAmount(packType);
            _currentPackTypeText.text = $"{packType.ToString().ToUpperInvariant()} PACK";
            _currentPackTypeAmountText.text = amount.ToString();

            UpdateOpenButtonState();
        }

        private async void OpenButtonHandler()
        {
            OneOf<IReadOnlyList<CardKey>, Exception> result = await _controller.OpenPack(_selectedPackType.Value);
            if (result.IsT1)
            {
                FailAndGoToMainMenu("Loading cards failed.\n Please try again.");
                return;
            }

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

            Log.Debug("cards: " + Utilites.FormatCallLogList(cards));
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
        }

        private void FailAndGoToMainMenu(string customMessage = null)
        {
            _uiManager.HidePopup<LoadingOverlayPopup>();
            _uiManager.DrawPopup<WarningPopup>(customMessage ?? "Something went wrong.\n Please try again.");
            ChangeState(State.Undefined);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
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

        private enum State
        {
            Undefined,
            ClaimingPacks,
            Ready,
            TrayInserted,
            CardEmerged
        }

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

        private abstract class PackOpenerControllerBase
        {
            public abstract IReadOnlyList<Enumerators.MarketplaceCardPackType> ShownPackTypes { get; }

            public abstract uint GetPackTypeAmount(Enumerators.MarketplaceCardPackType packType);

            public abstract Task<OneOf<Success, Exception>> Start();

            public abstract Task<OneOf<IReadOnlyList<CardKey>, Exception>> OpenPack(Enumerators.MarketplaceCardPackType packType);
        }

        private class NormalPackOpenerController : PackOpenerControllerBase
        {
            private Dictionary<Enumerators.MarketplaceCardPackType, uint> _packTypeToPackAmount = new Dictionary<Enumerators.MarketplaceCardPackType, uint>();
            private IUIManager _uiManager;
            private ILoadObjectsManager _loadObjectsManager;
            private PlasmaChainBackendFacade _plasmaChainBackendFacade;
            private BackendDataControlMediator _backendDataControlMediator;
            private ITutorialManager _tutorialManager;
            private IDataManager _dataManager;
            private BackendFacade _backendFacade;

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
                using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
                {
                    try
                    {
                        IReadOnlyList<CardKey> cardKeys = await _plasmaChainBackendFacade.CallOpenPack(client, packType);
                        return OneOf<IReadOnlyList<CardKey>, Exception>.FromT0(cardKeys);
                    }
                    catch (Exception e)
                    {
                        return e;
                    }
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

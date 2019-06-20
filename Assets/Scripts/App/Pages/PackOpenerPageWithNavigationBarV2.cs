using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Iap;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PackOpenerPageWithNavigationBarV2 : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PackOpenerPageWithNavigationBarV2));

        private readonly HashSet<Enumerators.MarketplaceCardPackType> _supportedPackTypes = new HashSet<Enumerators.MarketplaceCardPackType>
        {
            Enumerators.MarketplaceCardPackType.Booster,
            Enumerators.MarketplaceCardPackType.Booster
        };

        private readonly List<PackTypeButton> _packTypeButtons = new List<PackTypeButton>();

        private Dictionary<Enumerators.MarketplaceCardPackType, uint> _packTypeToPackAmount = new Dictionary<Enumerators.MarketplaceCardPackType, uint>();

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private PlasmaChainBackendFacade _plasmaChainBackendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private GameObject _openedPackPanel;

        private Enumerators.MarketplaceCardPackType _selectedPackType;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPageWithNavigationBarV2"),
                _uiManager.Canvas.transform,
                false);

            _openedPackPanel = _selfPage.transform.Find("OpenedPackPanel").gameObject;
            _openedPackPanel.SetActive(false);
            CreatePackTypeButtons(_supportedPackTypes);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_PACKS);
            _uiManager.DrawPopup<AreaBarPopup>();
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
            foreach (PackTypeButton packTypeButton in _packTypeButtons)
            {
                packTypeButton.Dispose();
            }

            _packTypeButtons.Clear();

            if (_selfPage == null)
                return;

            Object.Destroy(_selfPage);
            _selfPage = null;
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

        private void CreatePackTypeButtons(ICollection<Enumerators.MarketplaceCardPackType> packTypes)
        {
            Transform root = _selfPage.transform.Find("PackTypeSelectorPanel/Container/Root");
            ToggleGroup toggleGroup = root.gameObject.GetComponent<ToggleGroup>();
            foreach (Enumerators.MarketplaceCardPackType packType in packTypes)
            {
                string name = $"{packType.ToString().ToUpperInvariant()} PACK";
                PackTypeButton packTypeButton = new PackTypeButton(root, _loadObjectsManager, packType, name);
                packTypeButton.Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn)
                        return;

                    PlayClickSound();
                    ChangeSelectedPackType(packType);
                });
                toggleGroup.RegisterToggle(packTypeButton.Toggle);
                packTypeButton.Toggle.group = toggleGroup;
                _packTypeButtons.Add(packTypeButton);
            }
        }

        private void ButtonPackTypeHandler(Enumerators.MarketplaceCardPackType packType)
        {
            if (_tutorialManager.BlockAndReport("PackContent"))
                return;

            PlayClickSound();
            ChangeSelectedPackType(packType);
        }

        private void ChangeSelectedPackType(Enumerators.MarketplaceCardPackType packType)
        {
            _selectedPackType = packType;
            //SetPackToOpenAmount(_packBalanceAmounts[(int) _selectedPackType]);

            UpdateOpenButtonState();
        }

        private void UpdateOpenButtonState()
        {

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
            None,
            ClaimingPacks,
            Ready,
            TrayInserted,
            CardEmerged,
        }

        private class PackTypeButton
        {
            public Enumerators.MarketplaceCardPackType PackType { get; }

            public GameObject GameObject { get; }

            public Toggle Toggle { get; }

            public TextMeshProUGUI NameText { get; }

            public TextMeshProUGUI AmountText { get; }

            public PackTypeButton(Transform parent, ILoadObjectsManager loadObjectsManager, Enumerators.MarketplaceCardPackType packType, string name)
            {
                PackType = packType;
                GameObject = Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/OpenPackPackTypeButton"), parent);
                Toggle = GameObject.GetComponent<Toggle>();
                NameText = GameObject.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                AmountText = GameObject.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();

                NameText.text = name;
                AmountText.text = "0";
            }


            public void Dispose()
            {
                Object.Destroy(GameObject);
            }
        }
    }
}

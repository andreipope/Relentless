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
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PackOpenerPageWithNavigationBar : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PackOpenerPageWithNavigationBar));

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI[] _packTypeAmountLabels,
            _packTypeNames;

        private Button[] _packTypeButtons;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private PlasmaChainBackendFacade _plasmaChainBackendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private CardInfoPopupHandler _cardInfoPopupHandler;

        private GameObject _selfPage;

        private GameObject _gooPoolPrefab,
            _buttonOpenPackVFXPrefab,
            _cardCreaturePrefab,
            _cardItemPrefab;

        private GameObject _vfxMinionPrefab,
            _vfxOfficerPrefab,
            _vfxCommanderPrefab,
            _vfxGeneralPrefab;

        private GameObject _createdGooPool,
            _createdbuttonOpenPackVFX;

        private Image _rightPanelLight,
            _leftPanelLight;

        private List<BoardCardView> _createdBoardCards;

        private List<GameObject> _createdCardsVFX;

        private CardHighlightingVFXItem _createdHighlightingVFXItem;

        private Button _buttonPlus,
            _buttonMinus,
            _buttonMax,
            _buttonOpenPack,
            _butttonPanelCollect,
            _buttonCollect,
            _buttonBuyPack;

        private TextMeshProUGUI _packsAmountText;

        private Transform _packTray,
            _trayStart,
            _trayEnd,
            _panelCollect,
            _greenPoolVFX;

        private Transform _panelPackContent;

        private SpriteRenderer _vignetteCollectCard;

        private Sprite _packHolderSelectedSprite,
            _packHolderNormalSprite;

        private Animator _gooPoolAnimator;

        private List<Transform> _cardPositions;

        private List<Transform> _cardsToReveal;

        private List<Card> _cardsToDisplayQueqe;

        private int _packToOpenAmount;

        private int[] _packBalanceAmounts;

        private Enumerators.MarketplaceCardPackType _selectedPackType;

        private enum STATE
        {
            NONE,
            READY,
            TRAY_INSERTED,
            CARD_EMERGED,
        }

        private STATE _state;

#pragma warning disable 414
        private bool _isTransitioningState;
#pragma warning restore 414

        private bool _isWaitingForTapToReveal;

        private Enumerators.MarketplaceCardPackType _lastPackBalanceIdRequest;

        private Enumerators.MarketplaceCardPackType _lastOpenPackIdRequest;

        private int _retryPackBalanceRequestCount = 0;

        private int _retryOpenPackRequestCount = 0;

        private const int MaxRequestRetryAttempt = 2;

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();
            _cardInfoPopupHandler.StateChanging += () => ChangeStateCardInfoPopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.StateChanged += () => ChangeStateCardInfoPopup(_cardInfoPopupHandler.IsStateChanging);
            _createdBoardCards = new List<BoardCardView>();
            _cardsToDisplayQueqe = new List<Card>();
            _createdCardsVFX = new List<GameObject>();
            _cardsToReveal = new List<Transform>();

            Enumerators.MarketplaceCardPackType[] packTypes =
                (Enumerators.MarketplaceCardPackType[]) Enum.GetValues(typeof(Enumerators.MarketplaceCardPackType));
            _packBalanceAmounts = new int[packTypes.Length];
        }

        public void Update()
        {
            if (_selfPage == null || !_selfPage.activeInHierarchy)
                return;

            _cardInfoPopupHandler.Update();
            if (_isWaitingForTapToReveal)
            {
                if (_cardsToReveal.Count <= 0)
                {
                    _isWaitingForTapToReveal = false;
                    return;
                }

                if (Input.GetMouseButton(0))
                {
                    GameObject hitObject = RaycastFromMousePosition();
                    if (hitObject == null)
                        return;

                    foreach (Transform cardTran in _cardsToReveal)
                    {
                        if (hitObject == cardTran.gameObject)
                        {
                            _cardsToReveal.Remove(cardTran);
                            RevealCard(cardTran);
                            return;
                        }
                    }
                }
            }
            else if (_cardInfoPopupHandler.IsInteractable)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject hitObject = RaycastFromMousePosition();
                    if (hitObject == null)
                        return;

                    for (int i = 0; i < _createdBoardCards.Count; i++)
                    {
                        if (hitObject == _createdBoardCards[i].GameObject)
                        {
                            _createdHighlightingVFXItem.SetActiveCard(_createdBoardCards[i]);
                            _cardInfoPopupHandler.SelectCard(_createdBoardCards[i]);
                        }
                    }
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPageWithNavigationBar"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _createdHighlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"),
                _selfPage.transform,
                true));

            _gooPoolPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/OpenPackGooPool");
            _buttonOpenPackVFXPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/OpenPackButton");
            _cardCreaturePrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");

            _vfxMinionPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/ZB_ANM_MinionPackOpenerV2");
            _vfxOfficerPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/ZB_ANM_OfficerPackOpenerV2");
            _vfxCommanderPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/ZB_ANM_CommanderPackOpenerV2");
            _vfxGeneralPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/ZB_ANM_GeneralPackOpenerV2");

            _buttonBuyPack = _selfPage.transform.Find("Pack_Panel/Button_BuyPacks").GetComponent<Button>();
            _buttonPlus = _selfPage.transform.Find("Pack_Panel/pack_screen/ButtonPlus").GetComponent<Button>();
            _buttonMinus = _selfPage.transform.Find("Pack_Panel/pack_screen/ButtonMinus").GetComponent<Button>();
            _buttonMax = _selfPage.transform.Find("Pack_Panel/pack_screen/ButtonMax").GetComponent<Button>();
            _buttonOpenPack = _selfPage.transform.Find("Pack_Panel/RightPanel/ButtonOpenPacks").GetComponent<Button>();
            _butttonPanelCollect = _selfPage.transform.Find("Panel_Collect").GetComponent<Button>();
            _buttonCollect = _selfPage.transform.Find("Panel_Collect/ButtonCollect").GetComponent<Button>();

            _packsAmountText = _selfPage.transform.Find("Pack_Panel/pack_screen/TextAmount").GetComponent<TextMeshProUGUI>();
            _packTray = _selfPage.transform.Find("Pack_Panel/Pack_Tray").GetComponent<Transform>();
            _trayStart = _selfPage.transform.Find("Pack_Panel/tray_start").GetComponent<Transform>();
            _trayEnd = _selfPage.transform.Find("Pack_Panel/tray_end").GetComponent<Transform>();
            _panelCollect = _selfPage.transform.Find("Panel_Collect").GetComponent<Transform>();
            _panelCollect.SetParent(_uiManager.Canvas2.transform);

            _packHolderSelectedSprite = _selfPage.transform.Find("pack_holder_tray/sprite_pack_holder_selected").GetComponent<Image>().sprite;
            _packHolderNormalSprite = _selfPage.transform.Find("pack_holder_tray/sprite_pack_holder_normal").GetComponent<Image>().sprite;

            _rightPanelLight = _selfPage.transform.Find("Pack_Panel/Glowing/Panel_right").GetComponent<Image>();
            _leftPanelLight = _selfPage.transform.Find("Pack_Panel/Glowing/Panel_left").GetComponent<Image>();

            _buttonPlus.gameObject.SetActive(false);
            _buttonMinus.gameObject.SetActive(false);
            _buttonMax.gameObject.SetActive(false);

            InitObjects();
            InitCardPositions();
            InitState();
            InitPackTypeButtons();
            SetPackTypeButtonsAmount();

            if (_tutorialManager.IsTutorial)
            {
                _packBalanceAmounts[(int) Enumerators.MarketplaceCardPackType.Minion] =
                    _tutorialManager.CurrentTutorial.TutorialContent.TutorialReward.CardPackCount;
                SetPackTypeButtonsAmount(Enumerators.MarketplaceCardPackType.Minion);
            }
            else
            {
#pragma warning disable 4014
                UpdateAllPackBalanceAmounts();
#pragma warning restore 4014
            }

            ChangeSelectedPackType(Enumerators.MarketplaceCardPackType.Minion);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_PACKS);
            _uiManager.DrawPopup<AreaBarPopup>();
        }

        public void Hide()
        {
            Dispose();

            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        public void Dispose()
        {
            DestroyCreatedObject();
        }

        #endregion

        #region private function

        private void InitObjects()
        {
            _buttonBuyPack.onClick.AddListener(ButtonBuyPacksHandler);
            _buttonPlus.onClick.AddListener(ButtonPlusHandler);
            _buttonMinus.onClick.AddListener(ButtonMinusHandler);
            _buttonMax.onClick.AddListener(ButtonMaxHandler);
            _buttonOpenPack.onClick.AddListener(ButtonOpenPackHandler);
            _buttonCollect.onClick.AddListener(ButtonCollectHandler);

            _createdGooPool = Object.Instantiate(_gooPoolPrefab);
            Vector3 gooPoolPos = _selfPage.transform.Find("Pack_Panel/locator_goo_pool").position;
            gooPoolPos.z = 0f;
            _createdGooPool.transform.position = gooPoolPos;

            _gooPoolAnimator = _createdGooPool.transform.Find("OpenPack").GetComponent<Animator>();
            _gooPoolAnimator.enabled = true;
            _greenPoolVFX = _createdGooPool.transform.Find("OpenPack/OpenPack").GetComponent<Transform>();

            _panelPackContent = _selfPage.transform.Find("pack_holder_tray/PackContent");

            _createdGooPool.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            _createdGooPool.GetComponent<SortingGroup>().sortingOrder = 1;

            _vignetteCollectCard = _createdGooPool.transform.Find("Vignette").GetComponent<SpriteRenderer>();
            _vignetteCollectCard.enabled = false;

            _createdbuttonOpenPackVFX = Object.Instantiate(_buttonOpenPackVFXPrefab);
            _createdbuttonOpenPackVFX.transform.position = _buttonOpenPack.transform.position;
        }

        private void InitCardPositions()
        {
            _cardPositions = new List<Transform>();
            _cardPositions.Add(
                _createdGooPool.transform.Find("OpenPack/Card/CreatureCard")
            );
            _cardPositions.Add(
                _createdGooPool.transform.Find("OpenPack/Card (3)/CreatureCard")
            );
            _cardPositions.Add(
                _createdGooPool.transform.Find("OpenPack/Card (5)/CreatureCard")
            );
            _cardPositions.Add(
                _createdGooPool.transform.Find("OpenPack/Card (2)/CreatureCard")
            );
            _cardPositions.Add(
                _createdGooPool.transform.Find("OpenPack/Card (1)/CreatureCard (1)")
            );
        }

        private void InitPackTypeButtons()
        {
            Enumerators.MarketplaceCardPackType[] packTypes =
                (Enumerators.MarketplaceCardPackType[]) Enum.GetValues(typeof(Enumerators.MarketplaceCardPackType));
            _packTypeNames = new TextMeshProUGUI[packTypes.Length];
            _packTypeAmountLabels = new TextMeshProUGUI[packTypes.Length];
            _packTypeButtons = new Button[packTypes.Length];
            for (int i = 0; i < packTypes.Length; ++i)
            {
                Transform buttonParent = _selfPage.transform.Find($"pack_holder_tray/PackContent/Group/pack_holder_normal_{i}");
                _packTypeNames[i] = buttonParent.Find("text_name").GetComponent<TextMeshProUGUI>();
                _packTypeNames[i].text = $"{packTypes[i].ToString().ToUpper()} PACK";
                _packTypeAmountLabels[i] = buttonParent.Find("text_amount").GetComponent<TextMeshProUGUI>();
                _packTypeAmountLabels[i].text = "0";
                _packTypeButtons[i] = buttonParent.GetComponent<Button>();
                Enumerators.MarketplaceCardPackType id = (Enumerators.MarketplaceCardPackType) i;
                _packTypeButtons[i]
                    .onClick.AddListener(() =>
                    {
                        ButtonPackTypeHandler(id);
                    });

                // FIXME
                if (packTypes[i] != Enumerators.MarketplaceCardPackType.Booster)
                {
                    buttonParent.gameObject.SetActive(false);
                }
            }

            _selfPage.transform.Find("pack_holder_tray/PackContent").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        private void SetPackTypeButtonsAmount()
        {
            for (int i = 0; i < _packTypeAmountLabels.Length; ++i)
            {
                SetPackTypeButtonsAmount((Enumerators.MarketplaceCardPackType) i);
            }
        }

        private void SetPackTypeButtonsAmount(Enumerators.MarketplaceCardPackType typeId)
        {
            if (_selfPage == null || !_selfPage.activeInHierarchy)
                return;
            if (_packTypeAmountLabels == null || _packBalanceAmounts == null)
                return;
            _packTypeAmountLabels[(int) typeId].text = _packBalanceAmounts[(int) typeId].ToString();
        }

        private void CreateCardsToDisplay()
        {
            DestroyBoardCards();

            List<Card> cards = new List<Card>();
            for (int i = 0; i < 5 && _cardsToDisplayQueqe.Count > 0; ++i)
            {
                Card card = _cardsToDisplayQueqe[0];
                cards.Add(card);
                _cardsToDisplayQueqe.RemoveAt(0);
            }

            for (int i = 0; i < cards.Count; ++i)
            {
                Card card = cards[i];
                BoardCardView boardCard = CreateCard(card, Vector3.up * 12f);
                boardCard.Transform.parent = _cardPositions[i];
                boardCard.Transform.localPosition = Vector3.zero;
                boardCard.Transform.localRotation = Quaternion.identity;
                _createdBoardCards.Add(boardCard);
            }
        }

        private void InitState()
        {
            _state = STATE.NONE;
            _isTransitioningState = false;
            _isWaitingForTapToReveal = false;
            ChangeState(STATE.READY);
        }

        private void DestroyBoardCards()
        {
            if (_createdBoardCards != null)
            {
                foreach (BoardCardView card in _createdBoardCards)
                {
                    if (card != null)
                    {
                        Object.Destroy(card.GameObject);
                    }
                }

                _createdBoardCards.Clear();
            }
        }

        private void DestroyCardVFX()
        {
            if (_createdCardsVFX != null)
            {
                foreach (GameObject vfx in _createdCardsVFX)
                {
                    if (vfx != null)
                    {
                        Object.Destroy(vfx);
                    }
                }

                _createdCardsVFX.Clear();
            }
        }

        private void DestroyCreatedObject()
        {
            DestroyCardVFX();
            DestroyBoardCards();
            if (_panelCollect != null)
            {
                Object.Destroy(_panelCollect.gameObject);
            }

            if (_createdGooPool != null)
            {
                Object.Destroy(_createdGooPool);
            }

            if (_cardPositions != null)
            {
                _cardPositions.Clear();
            }

            if (_createdbuttonOpenPackVFX != null)
            {
                Object.Destroy(_createdbuttonOpenPackVFX);
            }
        }

        public async Task UpdateAllPackBalanceAmounts()
        {
            Enumerators.MarketplaceCardPackType[] packTypes = (Enumerators.MarketplaceCardPackType[]) Enum.GetValues(typeof(Enumerators.MarketplaceCardPackType));

            using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
            {
                for (int i = 0; i < packTypes.Length; ++i)
                {
                    await UpdatePackBalanceAmount(client, (Enumerators.MarketplaceCardPackType) i);
                }
            }
        }

        public async Task UpdatePackBalanceAmount(DAppChainClient client, Enumerators.MarketplaceCardPackType typeId)
        {
            _lastPackBalanceIdRequest = typeId;
            try
            {
                _packBalanceAmounts[(int) typeId] = await _plasmaChainBackendFacade.GetPackTypeBalance(client, typeId);
                SetPackTypeButtonsAmount(typeId);
                _retryPackBalanceRequestCount = 0;
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(UpdatePackBalanceAmount)} with typeId {typeId} failed: {e}");

                _retryPackBalanceRequestCount++;
                if (_retryPackBalanceRequestCount >= MaxRequestRetryAttempt)
                {
                    _retryPackBalanceRequestCount = 0;
                    _uiManager.DrawPopup<WarningPopup>("Something went wrong.\nWould you like to retry?");
                }
                else
                {
                    await UpdatePackBalanceAmount(client, typeId);
                }
            }
        }

        private async Task RetrieveCardsFromPack(DAppChainClient client, Enumerators.MarketplaceCardPackType packType)
        {
            _lastOpenPackIdRequest = packType;
            SetButtonInteractable(false);
            _uiManager.DrawPopup<LoadingOverlayPopup>("Loading your cards...");
            try
            {
                IReadOnlyList<CardKey> cardKeys = await _plasmaChainBackendFacade.CallOpenPack(client, packType);
                IReadOnlyList<Card> cards = _dataManager.CachedCardsLibraryData.GetCardsByCardKeys(cardKeys, true);

                _cardsToDisplayQueqe.Clear();
                _cardsToDisplayQueqe.AddRange(cards);
                _uiManager.HidePopup<LoadingOverlayPopup>();
                _retryOpenPackRequestCount = 0;
                ChangeState(STATE.CARD_EMERGED);
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(RetrieveCardsFromPack)} with packType {packType} failed: {e}");

                _retryOpenPackRequestCount++;
                if (_retryOpenPackRequestCount >= MaxRequestRetryAttempt)
                {
                    _retryOpenPackRequestCount = 0;
                    _uiManager.DrawPopup<WarningPopup>("Something went wrong.");
                }
                else
                {
                    await RetrieveCardsFromPack(client, _lastOpenPackIdRequest);
                }
            }
        }

        private async Task SimulateRetriveTutorialCardsFromPack()
        {
            _uiManager.DrawPopup<LoadingOverlayPopup>("Loading your cards...");
            _cardsToDisplayQueqe = _tutorialManager.GetCardForCardPack(5);
            _uiManager.HidePopup<LoadingOverlayPopup>();
            await Task.Delay(TimeSpan.FromSeconds(1));
            ChangeState(STATE.CARD_EMERGED);
        }

        private async void ProcessOpenPackLogic()
        {
            if (_tutorialManager.IsTutorial)
            {
                if (_tutorialManager.IsTutorial)
                {
                    if (_packBalanceAmounts[(int) _selectedPackType] > 0 && _packToOpenAmount > 0)
                    {
                        await SimulateRetriveTutorialCardsFromPack();

                        _packBalanceAmounts[(int) _selectedPackType]--;
                        _packToOpenAmount--;
                    }
                    else
                    {
                        ChangeState(STATE.CARD_EMERGED);
                        _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardPackCollected);
                    }
                }
            }
            else
            {
                using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
                {
                    await UpdatePackBalanceAmount(client, _selectedPackType);

                    SetPackToOpenAmount(_packBalanceAmounts[(int) _selectedPackType]);
                    if (_packBalanceAmounts[(int) _selectedPackType] > 0 && _packToOpenAmount > 0)
                    {
                        _packBalanceAmounts[(int) _selectedPackType]--;
                        _packToOpenAmount--;
                        await RetrieveCardsFromPack(client, _selectedPackType);
                    }
                    else
                    {
                        _cardsToDisplayQueqe.Clear();
                        ChangeState(STATE.CARD_EMERGED);
                    }
                }
            }
        }

        private void SetPackToOpenAmount(int amount)
        {
            _packToOpenAmount = amount;
            string s = amount > 1 ? "s" : "";
            _packsAmountText.text = $"{amount.ToString()} pack{s}";
            _createdbuttonOpenPackVFX.gameObject.SetActive(_packToOpenAmount > 0 && _buttonCollect.IsInteractable());
        }

        private void ChangeStateCardInfoPopup(bool isStart) { }

        private void SetButtonInteractable(bool isInteractable)
        {
            if (_buttonPlus != null)
                _buttonPlus.interactable = isInteractable;
            if (_buttonMinus != null)
                _buttonMinus.interactable = isInteractable;
            if (_buttonMax != null)
                _buttonMax.interactable = isInteractable;
            if (_buttonOpenPack != null)
            {
                _buttonOpenPack.interactable = isInteractable;
                _createdbuttonOpenPackVFX.SetActive(isInteractable && _packToOpenAmount > 0);
            }
        }

        private void RefreshAnimation()
        {
            _gooPoolAnimator.enabled = true;
            _gooPoolAnimator.Play("TubeAnim", 0, 0f);
            Sequence waitSeqence = DOTween.Sequence();
            waitSeqence.AppendInterval(.2f);
            waitSeqence.OnComplete(
                () =>
                {
                    _gooPoolAnimator.enabled = false;
                });
        }

        private void PlayCardsEmergeFromPoolAnimation()
        {
            _isTransitioningState = true;
            _gooPoolAnimator.enabled = true;
            _gooPoolAnimator.Play("OpenCardPackAnim", 0, 0f);
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.OPEN_PACK,
                    Constants.SfxSoundVolume,
                    false,
                    false,
                    true);
            _vignetteCollectCard.enabled = true;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(3.05f);
            sequence.OnComplete(
                () =>
                {
                    _gooPoolAnimator.enabled = false;
                    _isTransitioningState = false;

                    _cardsToReveal.Clear();
                    foreach (Transform cardPos in _cardPositions)
                    {
                        _cardsToReveal.Add(cardPos.parent);
                    }

                    _isWaitingForTapToReveal = true;
                });
        }

        private void CreateCardVFX(BoardCardView boardCard)
        {
            GameObject vfxPrefab;
            Enumerators.SoundType soundType;
            switch (boardCard.Model.Card.Prototype.Rank)
            {
                case Enumerators.CardRank.MINION:
                    soundType = Enumerators.SoundType.CARD_REVEAL_MINION;
                    vfxPrefab = _vfxMinionPrefab;
                    break;
                case Enumerators.CardRank.OFFICER:
                    soundType = Enumerators.SoundType.CARD_REVEAL_OFFICER;
                    vfxPrefab = _vfxOfficerPrefab;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    soundType = Enumerators.SoundType.CARD_REVEAL_COMMANDER;
                    vfxPrefab = _vfxCommanderPrefab;
                    break;
                case Enumerators.CardRank.GENERAL:
                    soundType = Enumerators.SoundType.CARD_REVEAL_GENERAL;
                    vfxPrefab = _vfxGeneralPrefab;
                    break;
                default:
                    return;
            }

            GameClient.Get<ISoundManager>().PlaySound(soundType, Constants.SfxSoundVolume, false, false, true);

            GameObject vfxParent = new GameObject("VFX");
            vfxParent.transform.parent = boardCard.GameObject.transform;
            vfxParent.transform.localPosition = Vector3.zero;
            vfxParent.transform.localScale = Vector3.one;

            GameObject vfx = Object.Instantiate(vfxPrefab);
            vfx.transform.Find("MinionPlaceHolder").gameObject.SetActive(false);
            vfx.transform.parent = vfxParent.transform;
            vfx.transform.localPosition = Vector3.zero;
            vfx.transform.localScale = Vector3.one;
            _createdCardsVFX.Add(vfx);

            vfxParent.transform.localScale = Vector3.one * 0.673f;
            vfxParent.transform.localRotation = Quaternion.identity;
        }

        private GameObject RaycastFromMousePosition()
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null)
                    {
                        return hit.collider.gameObject;
                    }
                }
            }

            return null;
        }

        private void RevealCard(Transform cardShirt)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(
                cardShirt
                    .DORotate(Vector3.up * 90f, 0.1f, RotateMode.Fast)
                    .SetEase(Ease.InCubic)
                    .OnComplete(
                        () => { }));
            Transform cardFace = cardShirt.GetChild(0).GetChild(0);
            cardFace.parent = null;
            cardFace.localEulerAngles = Vector3.up * 90f;
            sequence.Append(
                cardFace
                    .DORotate(Vector3.zero, 0.2f, RotateMode.Fast)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(
                        () =>
                        {
                            foreach (BoardCardView boardCard in _createdBoardCards)
                            {
                                if (boardCard.Transform == cardFace)
                                {
                                    CreateCardVFX(boardCard);
                                    break;
                                }
                            }

                            if (!_isWaitingForTapToReveal)
                            {
                                Sequence sequence2 = DOTween.Sequence();
                                sequence2.AppendInterval(0.2f);
                                sequence2.OnComplete(() =>
                                {
                                    RefreshAnimation();
                                    _isTransitioningState = false;
                                    _greenPoolVFX.gameObject.SetActive(true);
                                    _vignetteCollectCard.enabled = true;
                                    _panelCollect.gameObject.SetActive(true);
                                    _buttonCollect.gameObject.SetActive(true);
                                });
                            }
                        }));
        }

        #endregion

        #region Button Handler

        private void ButtonBuyPacksHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonBuyPack.name))
                return;

            PlayClickSound();
            DOTween.KillAll();
            DestroyCreatedObject();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void ButtonPlusHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonPlus.name))
                return;

            PlayClickSound();
            if (_packToOpenAmount >= _packBalanceAmounts[(int) _selectedPackType])
                return;
            SetPackToOpenAmount(_packToOpenAmount + 1);
        }

        private void ButtonMinusHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonMinus.name))
                return;

            PlayClickSound();
            if (_packToOpenAmount <= 0)
                return;
            SetPackToOpenAmount(_packToOpenAmount - 1);
        }

        private void ButtonMaxHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonMax.name))
                return;

            PlayClickSound();
            if (_packToOpenAmount >= _packBalanceAmounts[(int) _selectedPackType])
                return;
            SetPackToOpenAmount(_packBalanceAmounts[(int) _selectedPackType]);
        }

        private void ButtonOpenPackHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonOpenPack.name))
                return;

            PlayClickSound();

            if (_state == STATE.READY)
            {
                if (_tutorialManager.IsTutorial)
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardPackOpened);
                }

                SetPackToOpenAmount(_packBalanceAmounts[(int) _selectedPackType]);
                if (_packToOpenAmount <= 0)
                    return;

                ChangeState(STATE.TRAY_INSERTED);
            }
        }

        private void ButtonPackTypeHandler(Enumerators.MarketplaceCardPackType id)
        {
            if (_tutorialManager.BlockAndReport(_panelPackContent.name))
                return;

            PlayClickSound();
            ChangeSelectedPackType(id);
        }

        private void ButtonCollectHandler()
        {
            PlayClickSound();

            _buttonCollect.gameObject.SetActive(false);
            _createdHighlightingVFXItem.ChangeState(false);

            int amount = _createdBoardCards.Count;
            int lastIndex = amount - 1;
            float delayOffset = 0.1f;
            float moveDuration = 0.4f;
            Vector3 hideCardPosition = -Vector3.up * 14f;

            for (int i = 0; i < amount; ++i)
            {
                Transform displayBoardCard = _createdBoardCards[i].Transform;
                Sequence hideCardSequence = DOTween.Sequence();
                hideCardSequence.AppendInterval(delayOffset * i);
                hideCardSequence.Append(displayBoardCard.DOMove(hideCardPosition, moveDuration));

                if (i == lastIndex)
                {
                    hideCardSequence.OnComplete(ProcessOpenPackLogic);
                }
            }
        }

        #endregion

        #region STATE

        private void ChangeState(STATE newState)
        {
            _isWaitingForTapToReveal = false;
            _vignetteCollectCard.enabled = false;
            switch (_state)
            {
                case STATE.NONE:
                    if (newState == STATE.READY)
                    {
                        SetPackToOpenAmount(0);
                        SetButtonInteractable(true);
                        _isTransitioningState = false;
                        _packTray.position = _trayStart.position;
                        _rightPanelLight.color = Color.clear;
                        _leftPanelLight.color = Color.white;
                        _panelCollect.gameObject.SetActive(false);
                        RefreshAnimation();
                        _state = newState;
                    }

                    break;
                case STATE.READY:
                    if (newState == STATE.TRAY_INSERTED)
                    {
                        SetButtonInteractable(false);
                        _panelCollect.gameObject.SetActive(true);
                        _buttonCollect.gameObject.SetActive(false);
                        _isTransitioningState = true;

                        Sequence sequence = DOTween.Sequence();
                        sequence.AppendInterval(0.5f)
                            .OnComplete(
                                () =>
                                {
                                    _leftPanelLight.color = Color.clear;
                                });
                        sequence.Append(
                            _packTray
                                .DOMove(_trayEnd.position, 1f)
                                .SetEase(Ease.InCubic)
                                .OnComplete(
                                    () =>
                                    {
                                        _rightPanelLight.color = Color.red;
                                        _isTransitioningState = false;
                                        _state = newState;
                                        ProcessOpenPackLogic();
                                    }
                                ));
                    }

                    break;
                case STATE.TRAY_INSERTED:
                    if (newState == STATE.CARD_EMERGED)
                    {
                        CreateCardsToDisplay();

                        SetButtonInteractable(false);

                        PlayCardsEmergeFromPoolAnimation();

                        _state = newState;
                    }

                    break;
                case STATE.CARD_EMERGED:
                    if (newState == STATE.CARD_EMERGED)
                    {
                        _panelCollect.gameObject.SetActive(false);
                        if (_cardsToDisplayQueqe.Count <= 0)
                        {
                            ChangeState(STATE.READY);
                        }
                        else
                        {
                            CreateCardsToDisplay();

                            _panelCollect.gameObject.SetActive(false);
                            _greenPoolVFX.gameObject.SetActive(true);

                            PlayCardsEmergeFromPoolAnimation();
                        }
                    }
                    else if (newState == STATE.READY)
                    {
                        SetPackToOpenAmount(0);
                        SetPackTypeButtonsAmount();
                        SetButtonInteractable(true);
                        _isTransitioningState = false;
                        _packTray.position = _trayStart.position;
                        _rightPanelLight.color = Color.clear;
                        _leftPanelLight.color = Color.white;
                        _panelCollect.gameObject.SetActive(false);
                        _greenPoolVFX.gameObject.SetActive(true);

                        _state = newState;
                    }

                    break;
                default:
                    break;
            }
        }

        private void ChangeSelectedPackType(Enumerators.MarketplaceCardPackType id)
        {
            _selectedPackType = id;
            SetPackToOpenAmount(_packBalanceAmounts[(int) _selectedPackType]);
            for (int i = 0; i < _packTypeButtons.Length; ++i)
            {
                _packTypeButtons[i].GetComponent<Image>().sprite =
                    (i == (int) _selectedPackType ? _packHolderSelectedSprite : _packHolderNormalSprite);
            }

            UpdateOpenButtonInteractable();
        }

        private void UpdateOpenButtonInteractable()
        {
            _buttonOpenPack.interactable = _packBalanceAmounts[(int) _selectedPackType] > 0;
        }

        #endregion

        #region Util

        private BoardCardView CreateCard(IReadOnlyCard card, Vector3 worldPos)
        {
            GameObject go;
            BoardCardView boardCard;
            CardModel cardModel = new CardModel(new WorkingCard(card, card, null));
            switch (card.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(_cardCreaturePrefab);
                    boardCard = new UnitBoardCardView(go, cardModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(_cardItemPrefab);
                    boardCard = new ItemBoardCardView(go, cardModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Kind), card.Kind, null);
            }

            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = worldPos;
            boardCard.Transform.localScale = Vector3.one * 0.16f;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;

            return boardCard;
        }

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        #endregion

    }
}

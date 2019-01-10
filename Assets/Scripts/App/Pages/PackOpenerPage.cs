//#define OPEN_PACK_MANAGER_INCLUDED

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PackOpenerPage : IUIElement
    {    
        private IUIManager _uiManager;
        
        private ILoadObjectsManager _loadObjectsManager;
        
        private CardInfoPopupHandler _cardInfoPopupHandler;

        #if OPEN_PACK_MANAGER_INCLUDED
        private OpenPackPlasmaManager _openPackPlasmaManager;
        #endif
        
        private GameObject _selfPage;
        
        private GameObject _gooPoolPrefab, _buttonOpenPackVFXPrefab, _cardCreaturePrefab, _cardItemPrefab;
    
        private GameObject _createdGooPool, _createdbuttonOpenPackVFX;
    
        private Image _rightPanelLight, _leftPanelLight;
        
        private List<BoardCard> _createdBoardCards;
        
        private CardHighlightingVFXItem _createdHighlightingVFXItem;
        
        private Button _buttonBack, _buttonPlus, _buttonMinus, _buttonMax, _buttonOpenPack, _butttonPanelCollect, _buttonCollect;
        
        private TextMeshProUGUI _packsAmountText;
        
        private Transform _packTray, _trayStart, _trayEnd, _panelCollect, _greenPoolVFX;
    
        private Animator _gooPoolAnimator;
        
        private List<Transform> _cardPositions;
    
        private List<Card> _cardsToDisplayQueqe;
    
        private int _packToOpenAmount;
    
        private int _packBalanceAmount;
        
        private enum STATE
        {
            NONE,
            READY,
            TRAY_INSERTED,
            CARD_EMERGED,
        }
    
        private STATE _state;
    
        private bool _isTransitioningState;
    
#region IUIElement
    
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            
            #if OPEN_PACK_MANAGER_INCLUDED
            _openPackPlasmaManager = GameClient.Get<OpenPackPlasmaManager>();
            #endif
            
            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();
            _cardInfoPopupHandler.StateChanging += () => ChangeStateCardInfoPopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.StateChanged += () => ChangeStateCardInfoPopup(_cardInfoPopupHandler.IsStateChanging);
            _createdBoardCards = new List<BoardCard>();
            _cardsToDisplayQueqe = new List<Card>();            
        }
    
        public void Update()
        {
            if (_selfPage != null && _selfPage.activeInHierarchy)
            {
                _cardInfoPopupHandler.Update();
                if (_cardInfoPopupHandler.IsInteractable)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
                        RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);
                        if (hits.Length > 0)
                        {
                            foreach (RaycastHit2D hit in hits)
                            {
                                if (hit.collider != null)
                                {
                                    for (int i = 0; i < _createdBoardCards.Count; i++)
                                    {
                                        if (hit.collider.gameObject == _createdBoardCards[i].GameObject)
                                        {
                                            _createdHighlightingVFXItem.SetActiveCard(_createdBoardCards[i]);
                                            _cardInfoPopupHandler.SelectCard(_createdBoardCards[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    
        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            _createdHighlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));                        
            
            _gooPoolPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/OpenPackGooPool");
            _buttonOpenPackVFXPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OpenPack/OpenPackButton");
             _cardCreaturePrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            
            
            _buttonBack = _selfPage.transform.Find("Header/BackButton").GetComponent<Button>();
            _buttonPlus = _selfPage.transform.Find("BackgroundImage/LeftPanel/ButtonPlus").GetComponent<Button>();
            _buttonMinus = _selfPage.transform.Find("BackgroundImage/LeftPanel/ButtonMinus").GetComponent<Button>();
            _buttonMax = _selfPage.transform.Find("BackgroundImage/LeftPanel/ButtonMax").GetComponent<Button>();
            _buttonOpenPack = _selfPage.transform.Find("BackgroundImage/RightPanel/ButtonOpenPacks").GetComponent<Button>();
            _butttonPanelCollect = _selfPage.transform.Find("Panel_Collect").GetComponent<Button>();
            _buttonCollect = _selfPage.transform.Find("Panel_Collect/ButtonCollect").GetComponent<Button>();
            
            _packsAmountText = _selfPage.transform.Find("BackgroundImage/LeftPanel/TextAmount").GetComponent<TextMeshProUGUI>();
            _packTray = _selfPage.transform.Find("BackgroundImage/Tray").GetComponent<Transform>();
            _trayStart =  _selfPage.transform.Find("BackgroundImage/tray_start").GetComponent<Transform>();
            _trayEnd =  _selfPage.transform.Find("BackgroundImage/tray_end").GetComponent<Transform>();
            _panelCollect = _selfPage.transform.Find("Panel_Collect").GetComponent<Transform>();
    
            _rightPanelLight = _selfPage.transform.Find("BackgroundImage/Glowing/Panel_right").GetComponent<Image>();
            _leftPanelLight = _selfPage.transform.Find("BackgroundImage/Glowing/Panel_left").GetComponent<Image>();                       
            
            InitObjects();
            InitCardPositions();
            InitState();            
            RetrievePackBalanceAmount();           
        }
        
        public void Hide()
        {
            Dispose();
    
            if (_selfPage == null)
                return;
    
            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }
    
        public void Dispose()
        {
            DestroyCreatedObject();            
        }
    
#endregion
    
#region private function
    
        private void InitObjects()
        { 
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            _buttonPlus.onClick.AddListener(ButtonPlusHandler);
            _buttonMinus.onClick.AddListener(ButtonMinusHandler);
            _buttonMax.onClick.AddListener(ButtonMaxHandler);
            _buttonOpenPack.onClick.AddListener(ButtonOpenPackHandler);
            _buttonCollect.onClick.AddListener(ButtonCollectHandler);
            
            _createdGooPool = Object.Instantiate(_gooPoolPrefab);
            _gooPoolAnimator = _createdGooPool.transform.Find("OpenPack").GetComponent<Animator>();
            _gooPoolAnimator.enabled = true;
            _greenPoolVFX = _createdGooPool.transform.Find("OpenPack/OpenPack").GetComponent<Transform>();           
                        
            _createdGooPool.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            _createdGooPool.GetComponent<SortingGroup>().sortingOrder = 1;            
            
            _createdbuttonOpenPackVFX = Object.Instantiate(_buttonOpenPackVFXPrefab);
            _createdbuttonOpenPackVFX.transform.position = _buttonOpenPack.transform.position;
            _createdbuttonOpenPackVFX.SetActive(false);
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
        
        private void CreateCardsToDisplay()
        {
            DestroyBoardCards();   
            
            List<Card> cards = new List<Card>();
            for (int i = 0; i < 5 && _cardsToDisplayQueqe.Count > 0 ; ++i)
            {
                Card card = _cardsToDisplayQueqe[0];
                cards.Add(card);
                _cardsToDisplayQueqe.RemoveAt(0);
            }
                    
            for( int i=0; i<cards.Count; ++i)
            {
                Card card = cards[i];
                BoardCard boardCard = CreateCard(card, Vector3.up * 12f);
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
            ChangeState(STATE.READY);
        }
        
        private void DestroyBoardCards()
        {
            if (_createdBoardCards != null)
            {
                foreach (BoardCard card in _createdBoardCards)
                {
                    if (card != null)
                    {
                        Object.Destroy(card.GameObject);
                    }
                }
                _createdBoardCards.Clear();
            }
        }
    
        private void DestroyCreatedObject()
        {
            DestroyBoardCards();
            if (_createdGooPool != null)
            {
                Object.Destroy(_createdGooPool);
            }
            if (_cardPositions != null)
            {
                _cardPositions.Clear();
            }
            if( _createdbuttonOpenPackVFX != null)
            {
                Object.Destroy(_createdbuttonOpenPackVFX);
            }
        }

#if OPEN_PACK_MANAGER_INCLUDED
        private async void RetrievePackBalanceAmount()
        {
            _packsAmountText.text = "-";
            _packBalanceAmount = 0;
            _packBalanceAmount = await _openPackPlasmaManager.CallPackBalanceContract();
            SetPackToOpenAmount(0);
        }
        
        private async void ProcessOpenPackLogic()
        {            
            int amount = _packToOpenAmount;     
            _packBalanceAmount -= _packToOpenAmount;
            List<Card> cards = await _openPackPlasmaManager.CallOpenPack(_packToOpenAmount);
            _cardsToDisplayQueqe = cards; 
            
            ChangeState(STATE.CARD_EMERGED);       
        }
#else
        private void RetrievePackBalanceAmount()
        {
            _packsAmountText.text = "-";
            _packBalanceAmount = 5;
            SetPackToOpenAmount(0);
        }
        
        private void ProcessOpenPackLogic()
        {            
            int amount = _packToOpenAmount;     
            _packBalanceAmount -= _packToOpenAmount;
            List<Card> cards = RetrieveDummyCards();
            _cardsToDisplayQueqe = cards; 
            
            ChangeState(STATE.CARD_EMERGED);       
        }
#endif

        private List<Card> RetrieveDummyCards()
        {
            List<Card> cards = new List<Card>();            
            CardSet set = SetTypeUtility.GetCardSet(GameClient.Get<IDataManager>(), Enumerators.SetType.FIRE);
            int amount = 15; 
            foreach( Card card in set.Cards)
            {
                cards.Add(card);
                if (cards.Count >= amount)
                    break;
            }
            return cards;
        }
    
        private void SetPackToOpenAmount( int amount)
        {
            _packToOpenAmount = amount;
            _packsAmountText.text = amount.ToString();            
        }
        
        private void ChangeStateCardInfoPopup(bool isStart)
        {
            //_buttonCollect.interactable = !isStart;
        }
    
        private void SetButtonInteractable( bool isInteractable)
        {
            if(_buttonPlus != null)
                _buttonPlus.interactable = isInteractable;
            if(_buttonMinus != null)
                _buttonMinus.interactable = isInteractable;
            if(_buttonMax != null)
                _buttonMax.interactable = isInteractable;
            if(_buttonOpenPack != null)
                _buttonOpenPack.interactable = isInteractable;
        }
        
        private void RefreshAnimation()
        {
            _gooPoolAnimator.enabled = true;
            _gooPoolAnimator.Play("TubeAnim", 0, 0f);
            Sequence waitSeqence = DOTween.Sequence();
            waitSeqence.AppendInterval(.1f);
            waitSeqence.OnComplete(
            ()=>
            {
                _gooPoolAnimator.enabled = false;
            });  
        }
        
        private void PlayCardsEmergeFromPoolAnimation()
        {
            _isTransitioningState = true;
            _gooPoolAnimator.enabled = true;
            _gooPoolAnimator.Play("TubeAnim", 0, 0f);
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(5f);
            sequence.OnComplete(
            ()=>
            {   
                foreach(BoardCard boardCard in _createdBoardCards)
                {
                    boardCard.Transform.parent = null;
                }
                RefreshAnimation();
                _isTransitioningState = false;
                _greenPoolVFX.gameObject.SetActive(false);
                _panelCollect.gameObject.SetActive(true);
                _buttonCollect.gameObject.SetActive(true);
            });
        }
    
#endregion
    
#region Button Handler
    
        private void ButtonBackHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            DOTween.KillAll();            
            DestroyCreatedObject();
            GameClient.Get<IAppStateManager>().BackAppState();
        }
        
        private void ButtonPlusHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (_packToOpenAmount >= _packBalanceAmount)
                return;
            SetPackToOpenAmount( _packToOpenAmount+1 );
        }
    
        private void ButtonMinusHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (_packToOpenAmount <= 0)
                return;
            SetPackToOpenAmount( _packToOpenAmount-1 );
        }
        
        private void ButtonMaxHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (_packToOpenAmount >= _packBalanceAmount)
                return;
            SetPackToOpenAmount( _packBalanceAmount );
        }
        
        private void ButtonOpenPackHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
    
            if (_state == STATE.READY)
            {
                if (_packToOpenAmount <= 0)
                    return;
                ChangeState(STATE.TRAY_INSERTED);
            }
            else if (_state == STATE.TRAY_INSERTED)
            {
                ProcessOpenPackLogic();                
            }
            
        }
        
        private void ButtonCollectHandler()
        {
            _buttonCollect.gameObject.SetActive(false);
            _createdHighlightingVFXItem.ChangeState(false);
            int amount = _createdBoardCards.Count;
            int lastIndex = amount - 1;
            for(int i=0; i<amount ; ++i)
            {
                Transform displayBoardCard = _createdBoardCards[i].Transform;
                Sequence hideCardSequence = DOTween.Sequence();
                hideCardSequence.AppendInterval(.1f * i);
                hideCardSequence.Append(displayBoardCard.DOMove(  - Vector3.up * 12f  , .4f));
                
                if( i == lastIndex)
                {
                    hideCardSequence.OnComplete(
                    () =>
                    {
                        ChangeState(STATE.CARD_EMERGED);
                    }
                    );                        
                }
            }
        }
    
#endregion
    
#region STATE
    
        private void ChangeState( STATE newState )
        {
            switch (_state)
            {
                case STATE.NONE:
                    if( newState == STATE.READY)
                    {
                        SetPackToOpenAmount(0);
                        SetButtonInteractable(true);
                        _isTransitioningState = false;
                        _packTray.position = _trayStart.position;                        
                        _createdbuttonOpenPackVFX.SetActive(false);                        
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
                        _isTransitioningState = true;
    
                        Sequence sequence = DOTween.Sequence();
                        sequence.AppendInterval(0.5f).OnComplete(
                        ()=>
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
                                _createdbuttonOpenPackVFX.SetActive(true);
                                _buttonOpenPack.interactable = true;
                                _state = newState;                                
                            }
                        ));
                    }
                    break;                    
                case STATE.TRAY_INSERTED:
                    if (newState == STATE.CARD_EMERGED)
                    {
                        CreateCardsToDisplay();
                    
                        _createdbuttonOpenPackVFX.SetActive(false);
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
                        SetButtonInteractable(true);
                        _isTransitioningState = false;
                        _packTray.position = _trayStart.position;                        
                        _createdbuttonOpenPackVFX.SetActive(false);                        
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
    
#endregion
    
#region Util
    
        private BoardCard CreateCard(IReadOnlyCard card, Vector3 worldPos)
        {
        
            GameObject go;
            BoardCard boardCard;
            switch (card.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(_cardCreaturePrefab);
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.SPELL:
                    go = Object.Instantiate(_cardItemPrefab);
                    boardCard = new SpellBoardCard(go);
                    break;
                default:                
                    throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
            }
    
            boardCard.Init(card);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = worldPos;
            boardCard.Transform.localScale = Vector3.one * 0.32f;
            boardCard.Transform.Find("Amount").gameObject.SetActive(false);
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            
            return boardCard;
        
        }
    
#endregion
    
    }
}

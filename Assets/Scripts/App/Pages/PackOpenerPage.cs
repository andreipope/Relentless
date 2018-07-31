// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using LoomNetwork.CZB.Gameplay;

namespace LoomNetwork.CZB
{
    public class PackOpenerPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;
        private IPlayerManager _playerManager;

        private CardsController _cardsController;

        private GameObject _selfPage;

        private Button _buttonBack;

        private Button _buttonBuy,
                                _buttonCollection;

        private GameObject _packItemPrefab,
                            _packItemContent,
                            _cardCreaturePrefab,
                            _cardSpellPrefab,
                            _packOpenVFXprefab,
                           _packOpenVFX,
                            _cardPlaceholdersPrefab,
                           _cardPlaceholders,
                            _backgroundCanvasPrefab,
                           _backgroundCanvas;

        private Vector3 _centerPos;

        private bool _lock, _isCardPreview;

        private Transform _cardsContainer;

        private GameObject _packsObject;

        private TextMeshProUGUI _packsAmount;

        private int _cardsTurned = 0;

        private Transform _cardPreview, _cardPreviewOriginal;

        private bool _activatedTemporaryPack = false;

        private List<BoardCard> _createdBoardCards;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
            //_backgroundCanvasPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/BackgroundPackOpenerCanvas");
            _cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersPackOpener");


            _buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<Button>();
            _buttonCollection = _selfPage.transform.Find("Button_Collection").GetComponent<Button>();

            _packOpenVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/PackOpenerVFX");

            _buttonBack = _selfPage.transform.Find("Header/BackButton").GetComponent<Button>();
            _packsObject = _selfPage.transform.Find("PackItem").gameObject;

            _packsAmount = _packsObject.transform.Find("Amount/Value").GetComponent<TextMeshProUGUI>();

            _buttonBack.onClick.AddListener(BackButtonHandler);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonCollection.onClick.AddListener(CollectionButtonHandler);

            _createdBoardCards = new List<BoardCard>();

            Hide();
        }


        public void Update()
        {
            if (_selfPage.activeInHierarchy)
            {
                if (!_uiManager.GetPopup<CardInfoPopup>().Self.activeSelf)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (_isCardPreview)
                            CardPreview(false);
                        else
                            CardClickeCheck();
                    }

                    if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.W))
                    {
                        _activatedTemporaryPack = true;
                    }

                }
            }
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            InitObjects();

        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            Dispose();
        }

        public void Dispose()
        {
            ResetBoardCards();
            MonoBehaviour.Destroy(_backgroundCanvas);
            MonoBehaviour.Destroy(_cardPlaceholders);
        }

        private void ResetBoardCards()
        {
            foreach (var item in _createdBoardCards)
                item.Dispose();
            _createdBoardCards.Clear();
        }

        private void CardClickeCheck()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                foreach (var card in _createdBoardCards)
                {
                    if (hit.collider.gameObject == card.gameObject)
                    {
                        if (card.transform.Find("Back").gameObject.activeSelf)
                            CardSelected(card);
                        else
                        {
                            _cardPreviewOriginal = card.transform;
                            CardPreview(true);
                        }
                    }
                }
            }
            else if (_cardsTurned == 5)
                MoveCardsToBottomAndDestroy();
        }

        private void CardPreview(bool isOpen)
        {
            if (isOpen)
            {

                _cardPreview = MonoBehaviour.Instantiate(_cardPreviewOriginal.gameObject).transform;
                _cardPreview.name = "CardPreview";
                Utilites.SetLayerRecursively(_cardPreview.gameObject, 11);

                Sequence mySequence = DOTween.Sequence();
                mySequence.Append(_cardPreview.DORotate(new Vector3(-20, 30, -20), .2f));
                mySequence.Append(_cardPreview.DORotate(new Vector3(0, 0, 0), .4f));

                Sequence mySequence2 = DOTween.Sequence();
                mySequence2.Append(_cardPreview.DOMove(new Vector3(0, .3f, 5), .4f));
                mySequence2.Append(_cardPreview.DOMove(new Vector3(0, -0.3f, 5), .2f));

                Sequence mySequence3 = DOTween.Sequence();
                mySequence3.Append(_cardPreview.DOScale(new Vector3(1.1f, 1.1f, 1.1f), .4f));
                mySequence3.Append(_cardPreview.DOScale(new Vector3(1f, 1f, 1f), .2f));

                GameClient.Get<ICameraManager>().FadeIn(0.7f, 1);
                _isCardPreview = true;
            }
            else
            {
                GameClient.Get<ICameraManager>().FadeOut(null, 1);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(_cardPreview.DOScale(_cardPreviewOriginal.localScale, .3f));
                sequence.Join(_cardPreview.DOMove(_cardPreviewOriginal.position, .3f));
                sequence.Join(_cardPreview.DORotate(_cardPreviewOriginal.eulerAngles, .3f));
                sequence.OnComplete(() =>
                {
                    MonoBehaviour.Destroy(_cardPreview.gameObject);
                    _isCardPreview = false;
                });
            }
        }

        private void MoveCardsToBottomAndDestroy()
        {
            foreach (Transform cardObj in _cardsContainer)
            {
                Sequence animationSequence5 = DOTween.Sequence();
                animationSequence5.Append(cardObj.DOMove(_centerPos - Vector3.up * 9, .3f));
                animationSequence5.OnComplete(() =>
                {
                    MonoBehaviour.Destroy(cardObj.gameObject);
                });
            }
            if (_playerManager.LocalUser.packsCount > 0)
                _lock = false;
            _packsObject.GetComponent<DragableObject>().locked = _lock;
            _dataManager.SaveCache(Enumerators.CacheDataType.COLLECTION_DATA);
            _cardsTurned = 0;
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void InitObjects()
        {
            _cardsContainer = new GameObject("CardsContainer").transform;
            _centerPos = new Vector3(2.3f, -0.5f, 10);
            _cardPlaceholders = MonoBehaviour.Instantiate(_cardPlaceholdersPrefab);

            var packsCount = _playerManager.LocalUser.packsCount > 99 ? 99 : _playerManager.LocalUser.packsCount;
            _packsAmount.text = packsCount.ToString();

            _lock = false;

            if (_playerManager.LocalUser.packsCount > 0)
            {
                _packsObject.GetComponent<DragableObject>().OnItemEndDrag += PackOpenButtonHandler;
            }
            else
                _lock = true;
            _packsObject.GetComponent<DragableObject>().locked = _lock;
        }

        #region button handlers

        private void BackButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            DOTween.KillAll();
            if (_cardsContainer != null)
                MonoBehaviour.Destroy(_cardsContainer.gameObject);
            GameClient.Get<IAppStateManager>().BackAppState();
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }

        private void CollectionButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.COLLECTION);
        }

        private void PackOpenButtonHandler(GameObject go)
        {
            if (_cardsContainer != null)
                if (_cardsContainer.transform.childCount == 0 && !_lock)
                {
                    _playerManager.LocalUser.packsCount--;
                    var packsCount = _playerManager.LocalUser.packsCount > 99 ? 99 : _playerManager.LocalUser.packsCount;
                    _packsAmount.text = packsCount.ToString();
                    _lock = true;
                    _packsObject.GetComponent<DragableObject>().locked = _lock;

                    DetachAndAnimatePackItem(go);
                }
        }

        #endregion

        private void DetachAndAnimatePackItem(GameObject go)
        {
            Sequence animationSequence = DOTween.Sequence();
            animationSequence.Append(go.transform.DOMove(_centerPos, .3f));
            //animationSequence.Append(go.transform.DOShakePosition(.7f, 20f, 20, 90, false, false));

            

            animationSequence.OnComplete(() =>
            {
                _packOpenVFX = MonoBehaviour.Instantiate(_packOpenVFXprefab);
                _packOpenVFX.transform.position = Utilites.CastVFXPosition(_centerPos);
                _packOpenVFX.GetComponent<AnimationEventTriggering>().OnAnimationEvent += OnPackOpenVFXAnimationEventHandler;

                MonoBehaviour.Destroy(go);
                GameClient.Get<ITimerManager>().AddTimer((x) =>
                {
                    PackItemAnimationComplete();
                }, null, 0.4f);
            });
        }

        private void PackItemAnimationComplete()
        {
            var cardPack = new CardPack(Enumerators.CardPackType.DEFAULT);

            if (!_dataManager.CachedUserLocalData.openedFirstPack)
            {
                _activatedTemporaryPack = true;
                _dataManager.CachedUserLocalData.openedFirstPack = true;
                _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            }

            var cardsInPack = cardPack.OpenPack(_activatedTemporaryPack);

            _activatedTemporaryPack = false;

            for (int i = 0; i < Constants.CARDS_IN_PACK; i++)
            {
                var n = i;
                var card = cardsInPack[i];

                string cardSetName = _cardsController.GetSetOfCard(card);

                GameObject go = null;
                if ((Enumerators.CardKind)card.cardKind == Enumerators.CardKind.CREATURE)
                {
                    go = MonoBehaviour.Instantiate(_cardCreaturePrefab as GameObject);
                }
                else if ((Enumerators.CardKind)card.cardKind == Enumerators.CardKind.SPELL)
                {
                    go = MonoBehaviour.Instantiate(_cardSpellPrefab as GameObject);
                }

                go.transform.SetParent(_cardsContainer);
                go.transform.Find("Back").gameObject.SetActive(true);
                go.transform.Find("Amount").gameObject.SetActive(false);

                // todo imrpoveE!!!!
                var boardCard = new BoardCard(go);

                boardCard.Init(card);
                boardCard.SetHighlightingEnabled(false);
                boardCard.transform.position = _centerPos;
                boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_DEFAULT;
                boardCard.gameObject.GetComponent<SortingGroup>().sortingOrder = 1;

                Vector3 pos = _cardPlaceholders.transform.GetChild(i).position;
                Vector3 rotation = _cardPlaceholders.transform.GetChild(i).eulerAngles;

                go.transform.localScale = Vector3.one * .28f;
                go.transform.DOMove(pos, 1.0f);
                go.transform.DORotate(rotation, 1.0f);

                _createdBoardCards.Add(boardCard);
            }
        }

        private void OnPackOpenVFXAnimationEventHandler(string name)
        {
            if (_packOpenVFX == null)
                return;

            if (name == "EndPackOpen")
                MonoBehaviour.Destroy(_packOpenVFX);
        }

        private void CardSelected(BoardCard card)
        {
            var go = card.gameObject;

            if (!go.transform.Find("Back").gameObject.activeSelf)
                return;

            Vector3 rotation = go.transform.eulerAngles;
            Sequence animationSequence3 = DOTween.Sequence();
            animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 90, go.transform.eulerAngles.z), .4f));
            animationSequence3.Join(go.transform.DOScale(new Vector3(.4f, .4f, .4f), .2f));
            animationSequence3.OnComplete(() =>
            {
                go.transform.Find("Back").gameObject.SetActive(false);
                Sequence animationSequence4 = DOTween.Sequence();
                animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, go.transform.eulerAngles.z), .3f));
                animationSequence4.Join(go.transform.DOScale(new Vector3(.35f, .35f, .35f), .2f));
                animationSequence4.AppendInterval(2f);

                _cardsTurned++;
                _dataManager.CachedCollectionData.ChangeAmount(card.libraryCard.name, 1);
            });
        }
    }
}
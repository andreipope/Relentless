using System;
using System.Collections.Generic;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Gameplay;
using LoomNetwork.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class PackOpenerPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private IPlayerManager _playerManager;

        private CardsController _cardsController;

        private GameObject _selfPage;

        private Button _buttonBack;

        private Button _buttonBuy, _buttonCollection;

        private GameObject _cardCreaturePrefab;

        private GameObject _cardSpellPrefab;

        private GameObject _packOpenVfXprefab;

        private GameObject _packOpenVfx;

        private GameObject _cardPlaceholdersPrefab;

        private GameObject _cardPlaceholders;

        private Vector3 _centerPos;

        private bool _lock, _isCardPreview;

        private Transform _cardsContainer;

        private GameObject _packsObject;

        private TextMeshProUGUI _packsAmount;

        private int _cardsTurned;

        private Transform _cardPreview, _cardPreviewOriginal;

        private bool _activatedTemporaryPack;

        private List<BoardCard> _createdBoardCards;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            _createdBoardCards = new List<BoardCard>();
        }

        public void Update()
        {
            if (_selfPage != null && _selfPage.activeInHierarchy)
            {
                if (!_uiManager.GetPopup<CardInfoPopup>().Self.activeSelf)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (_isCardPreview)
                        {
                            CardPreview(false);
                        }
                        else
                        {
                            CardClickeCheck();
                        }
                    }

                    if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                        Input.GetKeyDown(KeyCode.W))
                    {
                        _activatedTemporaryPack = true;
                    }
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _cardCreaturePrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");

            _cardPlaceholdersPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersPackOpener");

            _buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<Button>();
            _buttonCollection = _selfPage.transform.Find("Button_Collection").GetComponent<Button>();

            _packOpenVfXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/PackOpenerVFX");

            _buttonBack = _selfPage.transform.Find("Header/BackButton").GetComponent<Button>();
            _packsObject = _selfPage.transform.Find("PackItem").gameObject;

            _packsAmount = _packsObject.transform.Find("Amount/Value").GetComponent<TextMeshProUGUI>();

            _buttonBack.onClick.AddListener(BackButtonHandler);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonCollection.onClick.AddListener(CollectionButtonHandler);

            InitObjects();
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
            Object.Destroy(_cardPlaceholders);
        }

        private void CardClickeCheck()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                foreach (BoardCard card in _createdBoardCards)
                {
                    if (hit.collider.gameObject == card.GameObject)
                    {
                        if (card.Transform.Find("Back").gameObject.activeSelf)
                        {
                            CardSelected(card);
                        }
                        else
                        {
                            _cardPreviewOriginal = card.Transform;
                            CardPreview(true);
                        }
                    }
                }
            }
            else if (_cardsTurned == 5)
            {
                MoveCardsToBottomAndDestroy();
            }
        }

        private void CardPreview(bool isOpen)
        {
            if (isOpen)
            {
                _cardPreview = Object.Instantiate(_cardPreviewOriginal.gameObject).transform;
                _cardPreview.name = "CardPreview";
                _cardPreview.gameObject.SetLayerRecursively(11);

                Sequence mySequence = DOTween.Sequence();
                mySequence.Append(_cardPreview.DORotate(new Vector3(-20, 30, -20), .2f));
                mySequence.Append(_cardPreview.DORotate(new Vector3(0, 0, 0), .4f));

                Sequence mySequence2 = DOTween.Sequence();
                mySequence2.Append(_cardPreview.DOMove(new Vector3(0, .3f, 5), .4f));
                mySequence2.Append(_cardPreview.DOMove(new Vector3(0, -0.3f, 5), .2f));

                Sequence mySequence3 = DOTween.Sequence();
                mySequence3.Append(_cardPreview.DOScale(new Vector3(1.1f, 1.1f, 1.1f), .4f));
                mySequence3.Append(_cardPreview.DOScale(new Vector3(1f, 1f, 1f), .2f));

                GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);
                _isCardPreview = true;
            }
            else
            {
                GameClient.Get<ICameraManager>().FadeOut(null, 1);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(_cardPreview.DOScale(_cardPreviewOriginal.localScale, .3f));
                sequence.Join(_cardPreview.DOMove(_cardPreviewOriginal.position, .3f));
                sequence.Join(_cardPreview.DORotate(_cardPreviewOriginal.eulerAngles, .3f));
                sequence.OnComplete(
                    () =>
                    {
                        Object.Destroy(_cardPreview.gameObject);
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
                animationSequence5.OnComplete(
                    () =>
                    {
                        Object.Destroy(cardObj.gameObject);
                    });
            }

            if (_playerManager.LocalUser.PacksCount > 0)
            {
                _lock = false;
            }

            _packsObject.GetComponent<DragableObject>().Locked = _lock;
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
            _cardPlaceholders = Object.Instantiate(_cardPlaceholdersPrefab);

            int packsCount = _playerManager.LocalUser.PacksCount > 99 ? 99 : _playerManager.LocalUser.PacksCount;
            _packsAmount.text = packsCount.ToString();

            _lock = false;

            if (_playerManager.LocalUser.PacksCount > 0)
            {
                _packsObject.GetComponent<DragableObject>().OnItemEndDrag += PackOpenButtonHandler;
            }
            else
            {
                _lock = true;
            }

            _packsObject.GetComponent<DragableObject>().Locked = _lock;
        }

        private void DetachAndAnimatePackItem(GameObject go)
        {
            Sequence animationSequence = DOTween.Sequence();
            animationSequence.Append(go.transform.DOMove(_centerPos, .3f));

            animationSequence.OnComplete(
                () =>
                {
                    _packOpenVfx = Object.Instantiate(_packOpenVfXprefab);
                    _packOpenVfx.transform.position = Utilites.CastVfxPosition(_centerPos);
                    _packOpenVfx.GetComponent<AnimationEventTriggering>().AnimationEventTriggered +=
                        OnPackOpenVFXAnimationEventHandler;

                    Object.Destroy(go);
                    GameClient.Get<ITimerManager>().AddTimer(
                        x =>
                        {
                            PackItemAnimationComplete();
                        },
                        null,
                        0.4f);
                });
        }

        private void PackItemAnimationComplete()
        {
            CardPack cardPack = new CardPack(Enumerators.CardPackType.DEFAULT);

            if (!_dataManager.CachedUserLocalData.OpenedFirstPack)
            {
                _activatedTemporaryPack = true;
                _dataManager.CachedUserLocalData.OpenedFirstPack = true;
                _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            }

            List<Card> cardsInPack = cardPack.OpenPack(_activatedTemporaryPack);

            _activatedTemporaryPack = false;

            for (int i = 0; i < Constants.CardsInPack; i++)
            {
                Card card = cardsInPack[i];

                GameObject go;
                switch (card.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        go = Object.Instantiate(_cardCreaturePrefab);
                        break;
                    case Enumerators.CardKind.SPELL:
                        go = Object.Instantiate(_cardSpellPrefab);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                go.transform.SetParent(_cardsContainer);
                go.transform.Find("Back").gameObject.SetActive(true);
                go.transform.Find("Amount").gameObject.SetActive(false);

                // todo imrpoveE!!!!
                BoardCard boardCard = new BoardCard(go);

                boardCard.Init(card);
                boardCard.SetHighlightingEnabled(false);
                boardCard.Transform.position = _centerPos;
                boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.Default.id;
                boardCard.GameObject.GetComponent<SortingGroup>().sortingOrder = 1;

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
            if (_packOpenVfx == null)
                return;

            if (name == "EndPackOpen")
            {
                Object.Destroy(_packOpenVfx);
            }
        }

        private void CardSelected(BoardCard card)
        {
            GameObject go = card.GameObject;

            if (!go.transform.Find("Back").gameObject.activeSelf)
                return;

            Sequence animationSequence3 = DOTween.Sequence();
            animationSequence3.Append(
                go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 90, go.transform.eulerAngles.z), .4f));
            animationSequence3.Join(go.transform.DOScale(new Vector3(.4f, .4f, .4f), .2f));
            animationSequence3.OnComplete(
                () =>
                {
                    go.transform.Find("Back").gameObject.SetActive(false);
                    Sequence animationSequence4 = DOTween.Sequence();
                    animationSequence4.Append(
                        go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, go.transform.eulerAngles.z),
                            .3f));
                    animationSequence4.Join(go.transform.DOScale(new Vector3(.35f, .35f, .35f), .2f));
                    animationSequence4.AppendInterval(2f);

                    _cardsTurned++;
                    _dataManager.CachedCollectionData.ChangeAmount(card.LibraryCard.Name, 1);
                });
        }

        #region button handlers

        private void BackButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            DOTween.KillAll();
            if (_cardsContainer != null)
            {
                Object.Destroy(_cardsContainer.gameObject);
            }

            GameClient.Get<IAppStateManager>().BackAppState();
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void CollectionButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.COLLECTION);
        }

        private void PackOpenButtonHandler(GameObject go)
        {
            if (_cardsContainer != null)
            {
                if (_cardsContainer.transform.childCount == 0 && !_lock)
                {
                    _playerManager.LocalUser.PacksCount--;
                    int packsCount = _playerManager.LocalUser.PacksCount > 99 ?
                        99 :
                        _playerManager.LocalUser.PacksCount;
                    _packsAmount.text = packsCount.ToString();
                    _lock = true;
                    _packsObject.GetComponent<DragableObject>().Locked = _lock;

                    DetachAndAnimatePackItem(go);
                }
            }
        }

        #endregion

    }
}

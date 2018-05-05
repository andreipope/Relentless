using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using FullSerializer;
using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.CZB.Helpers;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using GrandDevs.Internal;
using GrandDevs.CZB.Gameplay;

namespace GrandDevs.CZB
{
    public class PackOpenerPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;
        private IPlayerManager _playerManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonBack;

        private ScrollRect _cardsListScrollRect;

        private fsSerializer serializer = new fsSerializer();

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

        private int _cardsTurned = 0;

        private Transform _cardPreview, _cardPreviewOriginal;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PackOpenerPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/SpellCard");
            _packItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/PackItem");
            _backgroundCanvasPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/BackgroundPackOpenerCanvas");
            _cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersPackOpener");

            _packOpenVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/packOpenVFX");

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<MenuButtonNoGlow>();
            _packItemContent = _selfPage.transform.Find("Panel_PacksList/Group").gameObject;
            _cardsListScrollRect = _selfPage.transform.Find("Panel_PacksList").GetComponent<ScrollRect>();
            _buttonBack.onClickEvent.AddListener(BackButtonHandler);

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
				}
			}
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            InitObjects();
            //Debug.Log(_packItemContent);
            //InternalTools.FixVerticalLayoutGroupFitting(_packItemContent);
            //_cardsListScrollRect.verticalNormalizedPosition = 1f;
            //_cardsListScrollRect.CalculateLayoutInputVertical();
            
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            Dispose();
        }

        public void Dispose()
        {
            foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
            {
                MonoBehaviour.Destroy(card.gameObject);
            }
            //foreach (Transform child in _packItemContent.transform)
            //{
            //    MonoBehaviour.Destroy(child.gameObject);
            //}
             MonoBehaviour.Destroy(_packsObject);
            MonoBehaviour.Destroy(_backgroundCanvas);
			MonoBehaviour.Destroy(_cardPlaceholders);
            //MonoBehaviour.Destroy(_packsObject);
		}

        private void CardClickeCheck()
        {
			var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			var hit = Physics2D.Raycast(mousePos, Vector2.zero);
			if (hit.collider != null)
			{
				foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
				{
					if (hit.collider.gameObject == card.gameObject)
					{
                        if (card.transform.Find("BackgroundBack").gameObject.activeSelf)
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
            if(isOpen)
            {
                
                _cardPreview = MonoBehaviour.Instantiate(_cardPreviewOriginal.gameObject).transform;
				_cardPreview.name = "CardPreview";
				Utilites.SetLayerRecursively(_cardPreview.gameObject, 8);

				Sequence mySequence = DOTween.Sequence();
				mySequence.Append(_cardPreview.DORotate(new Vector3(-20, 30, -20), .2f));
				mySequence.Append(_cardPreview.DORotate(new Vector3(0, 0, 0), .4f));

				Sequence mySequence2 = DOTween.Sequence();
				mySequence2.Append(_cardPreview.DOMove(new Vector3(0, .3f, 5), .4f));
				mySequence2.Append(_cardPreview.DOMove(new Vector3(0, -0.3f, 5), .2f));

				Sequence mySequence3 = DOTween.Sequence();
				mySequence3.Append(_cardPreview.DOScale(new Vector3(1.1f, 1.1f, 1.1f), .4f));
				mySequence3.Append(_cardPreview.DOScale(new Vector3(1f, 1f, 1f), .2f));

				GameClient.Get<ICameraManager>().FadeIn(0.7f);
				_isCardPreview = true;
            }
            else
            {
                GameClient.Get<ICameraManager>().FadeOut(null);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(_cardPreview.DOScale(_cardPreviewOriginal.localScale, .3f));
                sequence.Join(_cardPreview.DOMove(_cardPreviewOriginal.position, .3f));
                sequence.Join(_cardPreview.DORotate(_cardPreviewOriginal.eulerAngles, .3f));
                sequence.OnComplete(() => {
                    MonoBehaviour.Destroy(_cardPreview.gameObject); 
                    _isCardPreview = false;
                });
            }
			
			//_uiManager.DrawPopup<CardInfoPopup>(card.libraryCard);
			//(_uiManager.GetPopup<CardInfoPopup>() as CardInfoPopup).cardTransform = _selectedCard.transform;
        }

        private void MoveCardsToBottomAndDestroy()
        {
			foreach (Transform cardObj in _cardsContainer)
			{
				Sequence animationSequence5 = DOTween.Sequence();
				animationSequence5.Append(cardObj.DOMove(_centerPos - Vector3.up * 7, .3f));
				animationSequence5.OnComplete(() =>
				{
					MonoBehaviour.Destroy(cardObj.gameObject);
				});
			}
			_lock = false;
			foreach (Transform item in _packItemContent.transform)
				item.GetComponent<DragableObject>().locked = _lock;
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
            _centerPos = new Vector3(2, 0, 10);
            _backgroundCanvas = MonoBehaviour.Instantiate(_backgroundCanvasPrefab);
            _backgroundCanvas.GetComponent<Canvas>().worldCamera = Camera.allCameras[0];
            _cardPlaceholders = MonoBehaviour.Instantiate(_cardPlaceholdersPrefab);

            /*for(int i = 0; i < _playerManager.LocalUser.packsCount; i++)
            {
                var go = MonoBehaviour.Instantiate(_packItemPrefab) as GameObject;
                go.transform.SetParent(_packItemContent.transform, false);
                go.GetComponent<DragableObject>().OnItemEndDrag += PackOpenButtonHandler;
            }*/

            if (_playerManager.LocalUser.packsCount > 0)
            {
                _packsObject = MonoBehaviour.Instantiate(_packItemPrefab) as GameObject;
                _packsObject.transform.SetParent(_packItemContent.transform, false);
                _packsObject.transform.Find("Amount/Value").GetComponent<Text>().text = _playerManager.LocalUser.packsCount.ToString();
                _packsObject.GetComponent<DragableObject>().OnItemEndDrag += PackOpenButtonHandler;
            }


            _lock = false;
            foreach (Transform item in _packItemContent.transform)
                item.GetComponent<DragableObject>().locked = _lock;
        }

        #region button handlers
 
        private void BackButtonHandler()
        {
            DOTween.KillAll();
            MonoBehaviour.Destroy(_cardsContainer.gameObject);
            GameClient.Get<IAppStateManager>().BackAppState();
        }

        private void PackOpenButtonHandler(GameObject go)
        {
            if (_cardsContainer.transform.childCount == 0 && !_lock)
            {
                _playerManager.LocalUser.packsCount--;
                _packsObject.transform.Find("Amount/Value").GetComponent<Text>().text = _playerManager.LocalUser.packsCount.ToString();
                _lock = true;
                foreach (Transform item in _packItemContent.transform)
                {
                    if(item != go.transform)
                        item.GetComponent<DragableObject>().locked = _lock;
                }

                DetachAndAnimatePackItem(go);
                if (_playerManager.LocalUser.packsCount == 0)
                {
                    MonoBehaviour.Destroy(_packsObject);
                }
            }
        }

        #endregion

        private void DetachAndAnimatePackItem(GameObject go)
        {
            Sequence animationSequence = DOTween.Sequence();
            animationSequence.Append(go.transform.DOMove(_centerPos, .3f));
            animationSequence.Append(go.transform.DOShakePosition(.7f, 20f, 10, 90, false, false));

            _packOpenVFX = MonoBehaviour.Instantiate(_packOpenVFXprefab);
            _packOpenVFX.transform.position = _centerPos;

            animationSequence.OnComplete(() => {
                MonoBehaviour.Destroy(go);
                PackItemAnimationComplete();
            });
        }

        private void PackItemAnimationComplete()
        {
            for (int i = 0; i < Constants.CARDS_IN_PACK; i++)
            {
                var n = i;

                UnityEngine.Debug.Log("PackItemAnimationComplete");


                var card = GenerateNewCard();

                string cardSetName = string.Empty;
                //var sets = _dataManager.CachedCardsLibraryData.sets.Where(set => set.name != "Others");
                foreach (var cardSet in _dataManager.CachedCardsLibraryData.sets)
                {
                    if (cardSet.cards.IndexOf(card) > -1)
                        cardSetName = cardSet.name;
                }
              
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
                go.transform.Find("BackgroundBack").gameObject.SetActive(true);
                go.transform.Find("Amount").gameObject.SetActive(false);
                var cardView = go.GetComponent<CardView>();
                cardView.PopulateWithLibraryInfo(card, cardSetName);
                cardView.SetHighlightingEnabled(false);
                cardView.transform.position = _centerPos;
                cardView.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                cardView.GetComponent<SpriteRenderer>().sortingOrder = 1;
                cardView.GetComponent<SortingGroup>().sortingLayerName = "Default";
                cardView.GetComponent<SortingGroup>().sortingOrder = 1;

				Vector3 pos = _cardPlaceholders.transform.GetChild(i).position;
                Vector3 rotation = _cardPlaceholders.transform.GetChild(i).eulerAngles;

                go.transform.localScale = Vector3.one * .35f;
                go.transform.DOMove(pos, 1.0f);
                go.transform.DORotate(rotation, 1.0f);
            }
        }

        private Card GenerateNewCard()
        {
            int id = 0;
            var rarity = (Enumerators.CardRarity)IsChanceFit(0);
            var cards = _dataManager.CachedCardsLibraryData.Cards.Where((item) => item.cardRarity == rarity).ToList();
            Card card = cards[UnityEngine.Random.Range(0, cards.Count)];
            return card;
        }

        private int IsChanceFit(int rarity)
        {
            int random = UnityEngine.Random.Range(0, 100);
            if (random > 90)
            {
                rarity++;
                return IsChanceFit(rarity);
            }
            else
                return rarity;
        }

		private void CardSelected(CardView card)
		{
            var go = card.gameObject;

            if (!go.transform.Find("BackgroundBack").gameObject.activeSelf)
                return;


            Vector3 rotation = go.transform.eulerAngles;
			Sequence animationSequence3 = DOTween.Sequence();
			animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 90, go.transform.eulerAngles.z), .4f));
            animationSequence3.Join(go.transform.DOScale(new Vector3(.4f, .4f, .4f), .2f));
			animationSequence3.OnComplete(() =>
			{                            
				go.transform.Find("BackgroundBack").gameObject.SetActive(false);
				Sequence animationSequence4 = DOTween.Sequence();
				animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, go.transform.eulerAngles.z), .3f));
                animationSequence4.Join(go.transform.DOScale(new Vector3(.35f, .35f, .35f), .2f));
                animationSequence4.AppendInterval(2f);

                _cardsTurned++;
                _dataManager.CachedCollectionData.ChangeAmount(card.libraryCard.id, 1);
			});
        }
    }
}

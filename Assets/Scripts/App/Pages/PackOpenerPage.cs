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

        private bool _lock;

        private Transform _cardsContainer;

        private GameObject _packsObject;

        private int _cardsTurned = 0;

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
                        if (_cardsTurned == 5)
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
                        else
                        {
                            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            var hit = Physics2D.Raycast(mousePos, Vector2.zero);
                            if (hit.collider != null)
                            {
                                foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
                                {
                                    if (hit.collider.gameObject == card.gameObject)
                                    {
                                        CardSelected(card);
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
            _selfPage.SetActive(true);
            InitObjects();
            InternalTools.FixVerticalLayoutGroupFitting(_packItemContent);
            _cardsListScrollRect.verticalNormalizedPosition = 1f;
            _cardsListScrollRect.CalculateLayoutInputVertical();
            
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
            MonoBehaviour.Destroy(_backgroundCanvas);
			MonoBehaviour.Destroy(_cardPlaceholders);
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

            Debug.Log(_playerManager.LocalUser.packsCount);
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
                if ((Enumerators.CardKind)card.cardTypeId == Enumerators.CardKind.CREATURE)
                {
                    go = MonoBehaviour.Instantiate(_cardCreaturePrefab as GameObject);
                }
                else if ((Enumerators.CardKind)card.cardTypeId == Enumerators.CardKind.SPELL)
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

                go.transform.DOMove(pos, 1.0f);
                go.transform.DORotate(rotation, 1.0f);
            }
        }

        private Card GenerateNewCard()
        {
            int id = 0;
            var rarity = (Enumerators.CardRarity)IsChanceFit(0);
            var cards = _dataManager.CachedCardsLibraryData.Cards.Where((item) => item.rarity == rarity).ToList();
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
            animationSequence3.Append(go.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), .2f));
			animationSequence3.OnComplete(() =>
			{                            
				go.transform.Find("BackgroundBack").gameObject.SetActive(false);
				Sequence animationSequence4 = DOTween.Sequence();
				animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, go.transform.eulerAngles.z), .3f));
                animationSequence4.Append(go.transform.DOScale(new Vector3(1f, 1f, 1f), .2f));
                animationSequence4.AppendInterval(2f);

                _cardsTurned++;
                _dataManager.CachedCollectionData.ChangeAmount(card.libraryCard.id, 1);
			});
        }
    }
}

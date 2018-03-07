using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CCGKit;
using UnityEngine.Rendering;
using System.IO;
using FullSerializer;
using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.CZB.Helpers;

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

            for(int i = 0; i < _playerManager.LocalUser.packsCount; i++)
            {
                var go = MonoBehaviour.Instantiate(_packItemPrefab) as GameObject;
                go.transform.SetParent(_packItemContent.transform, false);
                go.GetComponent<DragableObject>().OnItemEndDrag += PackOpenButtonHandler;
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
                _lock = true;
                foreach(Transform item in _packItemContent.transform)
                    item.GetComponent<DragableObject>().locked = _lock;


                //go.transform.SetParent(_selfPage.transform, true);
                DetachAndAnimatePackItem(go);
            }
        }

        #endregion

        private void DetachAndAnimatePackItem(GameObject go)
        {
            //go.transform.Find("OpenButton").gameObject.SetActive(false);
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
            var gameConfig = GameManager.Instance.config;

            int count = 5;
            for (int i = 0; i < count; i++)
            {
                var n = i;
                var ind = UnityEngine.Random.Range(0, gameConfig.GetNumCards());
                var card = gameConfig.cards[ind];
                var cardType = gameConfig.cardTypes.Find(x => x.id == card.cardTypeId);

                string cardSetName = string.Empty;
                foreach (var cardSet in gameConfig.cardSets)
                {
                    if (cardSet.cards.IndexOf(card) > -1)
                        cardSetName = cardSet.name;
                }

                GameObject go = null;
                if (cardType.name == "Creature")
                {
                    go = MonoBehaviour.Instantiate(_cardCreaturePrefab as GameObject);
                }
                else if (cardType.name == "Spell")
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

		private void CardSelected(CardView card)
		{
            var go = card.gameObject;

            Vector3 rotation = go.transform.eulerAngles;
			Sequence animationSequence3 = DOTween.Sequence();
			animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 90, go.transform.eulerAngles.z), .2f + .2f));
			animationSequence3.OnComplete(() =>
			{
				go.transform.Find("BackgroundBack").gameObject.SetActive(false);
				Sequence animationSequence4 = DOTween.Sequence();
				animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, go.transform.eulerAngles.z), .3f));
				animationSequence4.AppendInterval(1f);
                animationSequence4.Append(go.transform.DOMove(_centerPos - Vector3.up * 7, .3f));
				animationSequence4.OnComplete(() =>
				{
                    if (_cardsContainer.transform.childCount == 1)
                    {
                        _lock = false;
                        foreach (Transform item in _packItemContent.transform)
                            item.GetComponent<DragableObject>().locked = _lock;
                    }
                    int currentCardAmount = card.libraryCard.GetIntProperty("Amount");
                    currentCardAmount++;
                    card.libraryCard.SetIntProperty("Amount", currentCardAmount);
                    MonoBehaviour.Destroy(go);
				});
			});
        }
    }
}

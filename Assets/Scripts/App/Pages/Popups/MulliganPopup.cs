using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.BackendCommunication;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Loom.ZombieBattleground.PastActionsPopup;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MulliganPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        public event Action<List<BoardUnitModel>> MulliganCards;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;

        private GameObject _spellCardPrefab,
                           _unitCardPrefab;

        private GameObject _panelDeckObject,
                           _panelReplaceObject;

        private ButtonShiftingContent _buttonKeep;

        private List<MulliganCardItem> _mulliganCardItems;

        private MulliganCardItem _currentDragCard;

        private List<Vector3> _replacePositions;

        private List<Vector3> _basePositions;

        private bool _isDragging;


        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _spellCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Mulligan/MulliganCard_Spell");
            _unitCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Mulligan/MulliganCard_Unit");
        }

        public void Hide()
        {
            if (Self == null)
                return;

            foreach (var card in _mulliganCardItems)
            {
                card.eventHandler.DragBegan -= DragBeganEventHandler;
                card.eventHandler.DragUpdated -= DragUpdatedEventHandler;
                card.eventHandler.DragEnded -= DragEndedEventHandler;
                card.Dispose();
            }
            _mulliganCardItems.Clear();

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/MulliganPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _panelDeckObject = Self.transform.Find("Deck_Panel").gameObject;
            _panelReplaceObject = Self.transform.Find("Replace_Panel").gameObject;

            _basePositions = new List<Vector3>();
            _replacePositions = new List<Vector3>();

            Transform baseWaypointsPanel = _panelDeckObject.transform.Find("Waypoints");
            for (int i = 0; i < baseWaypointsPanel.childCount; i++)
            {
                _basePositions.Add(baseWaypointsPanel.GetChild(i).transform.position);
            }

            Transform replaceWaypointsPanel = _panelReplaceObject.transform.Find("Waypoints");
            for (int i = 0; i < replaceWaypointsPanel.childCount; i++)
            {
                _replacePositions.Add(replaceWaypointsPanel.GetChild(i).transform.position);
            }

            _buttonKeep = Self.transform.Find("Button_Keep").GetComponent<ButtonShiftingContent>();

            _buttonKeep.onClick.AddListener(KeepButtonOnClickHandler);

            _isDragging = false;

            FillCards();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void FillCards()
        {
            _mulliganCardItems = new List<MulliganCardItem>();
            GameObject prefab = null;
            MulliganCardItem item = null;
            int index = 0;
            foreach (BoardUnitModel card in _gameplayManager.CurrentPlayer.CardsPreparingToHand)
            {
                prefab = card.Prototype.CardKind == Enumerators.CardKind.CREATURE ? _unitCardPrefab : _spellCardPrefab;
                item = new MulliganCardItem(prefab, Self.transform, card);
                _mulliganCardItems.Add(item);
                item.eventHandler.DragBegan += DragBeganEventHandler;
                item.eventHandler.DragUpdated += DragUpdatedEventHandler;
                item.eventHandler.DragEnded += DragEndedEventHandler;
                item.SetPositions(_basePositions[index], _replacePositions[index]);
                index++;
            }
        }

        private void DragBeganEventHandler(PointerEventData arg1, GameObject obj)
        {
            if (_isDragging)
                return;

            _currentDragCard = _mulliganCardItems.Find((x) => x.selfObject == obj);
            if(_currentDragCard != null)
            {
                _currentDragCard.selfObject.transform.localPosition = Vector3.zero;
                _isDragging = true;
            }
        }

        private void DragUpdatedEventHandler(PointerEventData arg1, GameObject obj)
        {
            if (!_isDragging || _currentDragCard.selfObject != obj)
                return;

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _currentDragCard.UpdatePosition(position);
        }

        private void DragEndedEventHandler(PointerEventData arg1, GameObject obj)
        {

            if (!_isDragging || _currentDragCard.selfObject != obj)
                return;

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);

            bool isFinded = false;

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _panelReplaceObject)
                    {
                        _currentDragCard.SetChangedState(true);
                        isFinded = true;
                    }
                    else if(hit.collider.gameObject == _panelDeckObject)
                    {
                        _currentDragCard.SetChangedState( false);
                        isFinded = true;
                    }
                }
            }

            if (!isFinded)
            {
                _currentDragCard.SetChangedState(_currentDragCard.CardShouldBeChanged);
            }

            _currentDragCard = null;

            _isDragging = false;
        }

        public void KeepButtonOnClickHandler()
        {
            if (GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP)
            {
                _gameplayManager.GetController<CardsController>().CardsDistribution(_mulliganCardItems.FindAll((x) => x.CardShouldBeChanged).Select((k) => k.BoardUnitModel).ToList());
            }

            InvokeMulliganCardsEvent(_mulliganCardItems.FindAll((x) => !x.CardShouldBeChanged).Select((k) => k.BoardUnitModel).ToList());

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<MulliganPopup>();
        }    

        public void InvokeMulliganCardsEvent(List<BoardUnitModel> cards)
        {
            MulliganCards?.Invoke(cards);
        }    
    }

    public class MulliganCardItem
    {
        public OnBehaviourHandler eventHandler;

        public bool CardShouldBeChanged;

        public GameObject selfObject;

        public BoardUnitModel BoardUnitModel;

        public int lastIndex;

        private ActionElement _cardElement;

        private Transform _oldParentPanel;

        private GameObject _recycleObject;

        private Vector3 _normalPosition,
                        _replacePosition,
                        _deltaPosition;

        private bool _isFirstUpdatePosition = false;

        public MulliganCardItem(GameObject prefab, Transform parent, BoardUnitModel boardUnitModel)
        {
            this.BoardUnitModel = boardUnitModel;

            selfObject = Object.Instantiate(prefab, parent, false);

            _recycleObject = selfObject.transform.Find("Image_Recycle").gameObject;

            eventHandler = selfObject.transform.GetComponent<OnBehaviourHandler>();

            switch (boardUnitModel.Prototype.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    _cardElement = new UnitCardElement(selfObject);
                    break;
                case Enumerators.CardKind.ITEM:
                    _cardElement = new SpellCardElement(selfObject);
                    break;
                default:
                    break;
            }

            _cardElement.Init(boardUnitModel.Card);
        }

        public void SetChangedState(bool state)
        {
            CardShouldBeChanged = state;

            if(CardShouldBeChanged)
            {
                selfObject.transform.position = _replacePosition;
            }
            else
            {
                selfObject.transform.position = _normalPosition;
            }

            _isFirstUpdatePosition = false;

            _recycleObject.SetActive(CardShouldBeChanged);
        }

        public void UpdatePosition(Vector3 position)
        {
            if(_recycleObject.activeInHierarchy)
            {
                _recycleObject.SetActive(false);
            }

            position.z = selfObject.transform.position.z;

            if (!_isFirstUpdatePosition)
            {
                Vector3 currentPosition = CardShouldBeChanged ? _replacePosition : _normalPosition;
                _deltaPosition = position - currentPosition;
                _isFirstUpdatePosition = true;
                selfObject.transform.SetAsLastSibling();
            }
            
            selfObject.transform.position = position - _deltaPosition;
        }

        public void SetPositions(Vector3 normalPosition, Vector3 replacePosition)
        {
            _normalPosition = normalPosition;
            _replacePosition = replacePosition;
            selfObject.transform.position = _normalPosition;
        }

        public void Dispose()
        {
            Object.Destroy(selfObject);
        }
    }
}

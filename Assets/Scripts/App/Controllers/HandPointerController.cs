using System;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HandPointerController : IController
    {
        private List<HandPointerPopup> _handPointerPopups;

        public void Init()
        {
            _handPointerPopups = new List<HandPointerPopup>();
        }

        public void Update()
        {
            foreach (HandPointerPopup popup in _handPointerPopups)
            {
                popup.Update();
            }
        }

        public void ChangeVisibilitySelectHandPointer(Enumerators.TutorialObjectOwner owner, bool state)
        {
            List<HandPointerPopup> sortingPopups = _handPointerPopups.FindAll(popup => popup.Owner == owner);
            foreach (HandPointerPopup popup in sortingPopups)
            {
                if (state)
                {
                    popup.Activate();
                }
                else
                {
                    popup.Deactivate();
                }
            }
        }

        public void Dispose()
        {
            ResetAll();
        }

        public void ResetAll()
        {
            foreach (HandPointerPopup popup in _handPointerPopups)
            {
                popup.Dispose();
            }
            _handPointerPopups.Clear();
        }

        public void Reset(HandPointerPopup popup)
        {
            popup.Dispose();
            _handPointerPopups.Remove(popup);
        }

        public void DrawPointer(Enumerators.TutorialHandPointerType type,
                                Enumerators.TutorialObjectOwner owner,
                                Vector3 begin,
                                Vector3? end = null,
                                float appearDelay = 0,
                                bool appearOnce = false,
                                int tutorialObjectIdStepOwner = 0,
                                int targetTutorialObjectId = 0,
                                List<int> additionalObjectIdOwners = null,
                                List<int> additionalObjectIdTargets = null,
                                Enumerators.TutorialObjectLayer handLayer = Enumerators.TutorialObjectLayer.Default,
                                float handPointerSpeed = Constants.HandPointerSpeed,
                                string tutorialUIElementOwnerName = Constants.Empty,
                                float rotation = 0)
        {
            HandPointerPopup popup = new HandPointerPopup(type,
                                                          owner,
                                                          begin,
                                                          end,
                                                          appearDelay,
                                                          appearOnce,
                                                          tutorialObjectIdStepOwner,
                                                          targetTutorialObjectId,
                                                          additionalObjectIdOwners,
                                                          additionalObjectIdTargets, handLayer,
                                                          handPointerSpeed,
                                                          tutorialUIElementOwnerName,
                                                          rotation);
            _handPointerPopups.Add(popup);
        }
    }

    public class HandPointerPopup
    {
        private readonly ITutorialManager _tutorialManager;
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly BattlegroundController _battlegroundController;

        private readonly HandPointerController _handPointerController;

        private const float Interval = 0.3f;
        private const int SortingOrderAbovePages = -1;
        private const int SortingOrderAbovePopups = 125;

        private static Vector3 PositionForCardInHandOwner = new Vector3(0, 7, 0);

        public Enumerators.TutorialObjectOwner Owner;

        private Enumerators.TutorialHandPointerType _type;

        private GameObject _selfObject;

        private SpriteRenderer _handRenderer;

        private Sprite[] _handStates;

        private Vector3 _startPosition;

        private Vector3 _endPosition;

        private Vector3 _startPoint;

        private Vector3 _endPoint;

        private float _appearDelay;

        private bool _appearOnce;

        private bool _stayInEndPoint;

        private GameObject _ownerObject;

        private GameObject _targetObject;

        private float _maxValue = 3;
        private float _startValue = 0;
        private float _sideTurn;

        private float _speedMove;

        private float _durationMove;

        private bool _isMove = false;

        private float _sineOffset = 0;

        private bool _wasDisposed = false;

        private string _tutorialUIElementOwnerName;

        private List<Sequence> _allSequences;

        public HandPointerPopup(Enumerators.TutorialHandPointerType type,
                                Enumerators.TutorialObjectOwner owner,
                                Vector3 begin,
                                Vector3? end = null,
                                float appearDelay = 0,
                                bool appearOnce = false,
                                int tutorialObjectIdStepOwner = 0,
                                int targetTutorialObjectId = 0,
                                List<int> additionalObjectIdOwners = null,
                                List<int> additionalObjectIdTargets = null,
                                Enumerators.TutorialObjectLayer handLayer = Enumerators.TutorialObjectLayer.Default,
                                float handPointerSpeed = Constants.HandPointerSpeed,
                                string tutorialUIElementOwnerName = Constants.Empty,
                                float rotation = 0)
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _handPointerController = _gameplayManager.GetController<HandPointerController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _allSequences = new List<Sequence>();

            _startPosition = begin;
            _appearDelay = appearDelay;
            _appearOnce = appearOnce;
            _type = type;
            Owner = owner;
            _speedMove = handPointerSpeed;
            _tutorialUIElementOwnerName = tutorialUIElementOwnerName;

            _selfObject = MonoBehaviour.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/HandPointer"));

            _selfObject.transform.eulerAngles = new Vector3(0, 0, rotation);


            switch (Owner)
            {
                case Enumerators.TutorialObjectOwner.Battleframe:
                case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                    if (tutorialObjectIdStepOwner != 0)
                    {
                        CardModel ownerObjectModel =
                            _gameplayManager.CurrentPlayer.CardsOnBoard
                                .FirstOrDefault(x => String.Equals(x.Card.Prototype.Name, _tutorialManager.GetCardNameByTutorialObjectId(tutorialObjectIdStepOwner), (StringComparison) StringComparison.InvariantCultureIgnoreCase));
                        if (ownerObjectModel == null && additionalObjectIdOwners != null)
                        {
                            foreach (int id in additionalObjectIdOwners)
                            {
                                ownerObjectModel =
                                    _gameplayManager.CurrentPlayer.CardsOnBoard
                                        .FirstOrDefault(x => String.Equals(x.Card.Prototype.Name, _tutorialManager.GetCardNameByTutorialObjectId(id), StringComparison.InvariantCultureIgnoreCase));
                                if (ownerObjectModel != null)
                                    break;
                            }
                        }
                        _ownerObject = ownerObjectModel == null ? null : _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(ownerObjectModel)?.GameObject;
                        if (_ownerObject == null)
                        {
                            _handPointerController.Reset(this);
                            return;
                        }
                    }
                    break;
                case Enumerators.TutorialObjectOwner.PlayerCardInHand:
                    BoardCardView boardCard = null;
                    CardModel boardCardModel = null;
                    if (tutorialObjectIdStepOwner != 0)
                    {
                        boardCardModel =
                            _gameplayManager.CurrentPlayer.CardsInHand
                                .FirstOrDefault(card => card.Card.TutorialObjectId == tutorialObjectIdStepOwner);
                    }
                    else if(_gameplayManager.CurrentPlayer.CardsInHand.Count > 0)
                    {
                        boardCardModel = _gameplayManager.CurrentPlayer.CardsInHand[0];
                    }

                    if (boardCardModel != null)
                    {
                        boardCard = _battlegroundController.GetBoardUnitViewByModel<BoardCardView>(boardCardModel);
                    }

                    if (boardCard != null)
                    {
                        _ownerObject = boardCard.GameObject;
                        if (_startPosition == Vector3.zero)
                        {
                            _startPosition = PositionForCardInHandOwner;
                        }
                    }
                    break;
                default:
                    break;
            }
            
            if(targetTutorialObjectId != 0)
            {
                CardModel targetObjectModel =
                    _gameplayManager.OpponentPlayer.CardsOnBoard
                        .FirstOrDefault(x => String.Equals(x.Card.Prototype.Name, _tutorialManager.GetCardNameByTutorialObjectId(targetTutorialObjectId), StringComparison.InvariantCultureIgnoreCase));
                if (targetObjectModel == null && additionalObjectIdTargets != null)
                {
                    foreach (int id in additionalObjectIdTargets)
                    {
                        targetObjectModel =
                            _gameplayManager.OpponentPlayer.CardsOnBoard
                                .FirstOrDefault(x => String.Equals(x.Card.Prototype.Name, _tutorialManager.GetCardNameByTutorialObjectId(id), StringComparison.InvariantCultureIgnoreCase));
                        if (targetObjectModel != null)
                            break;
                    }
                }

                GameObject targetObjectGo = targetObjectModel == null ? null : _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(targetObjectModel)?.GameObject;
                if (targetObjectGo == null)
                {
                    _handPointerController.Reset(this);
                    return;
                }
                _targetObject = targetObjectGo;
            }

            _selfObject.SetActive(false);

            _handRenderer = _selfObject.transform.Find("Hand").GetComponent<SpriteRenderer>();

            switch(handLayer)
            {
                case Enumerators.TutorialObjectLayer.Default:
                case Enumerators.TutorialObjectLayer.AbovePages:
                    _handRenderer.sortingOrder = 0;
                    _handRenderer.sortingLayerName = SRSortingLayers.GameUI2;
                    break;
                case Enumerators.TutorialObjectLayer.AbovePopups:
                    _handRenderer.sortingOrder = SortingOrderAbovePopups;
                    _handRenderer.sortingLayerName = SRSortingLayers.GameUI3; 
                    break;
            }

            _handStates = _loadObjectsManager.GetObjectsByPath<Sprite>(new string[] {
                "Images/Tutorial/tutorial_hand_drag",
                "Images/Tutorial/tutorial_hand_pointing",
                "Images/Tutorial/tutorial_hand_pressed"
            });

            switch (type)
            {
                case Enumerators.TutorialHandPointerType.Single:
                    break;
                case Enumerators.TutorialHandPointerType.Animated:
                    _endPosition = end ?? Vector3.zero;
                    break;
                default:
                    break;
            }

            StartSequence();
        }

        private void StartSequence()
        {
            _allSequences.Add(InternalTools.DoActionDelayed(Start, _appearDelay));
        }

        private void Start()
        {
            if (_wasDisposed)
                return;

            if (Owner == Enumerators.TutorialObjectOwner.UI)
            {
                _ownerObject = GameObject.Find(_tutorialUIElementOwnerName);
                _targetObject = _ownerObject;
            }

            _selfObject.SetActive(true);

            _stayInEndPoint = false;

            _selfObject.transform.position = GetPositionByOwner(_ownerObject, _startPosition);
            _handRenderer.transform.localPosition = Vector3.zero;

            switch (_type)
            {
                case Enumerators.TutorialHandPointerType.Single:
                    ChangeHandState(Enumerators.TutorialHandState.Drag);
                    _allSequences.Add(InternalTools.DoActionDelayed(End, Interval));
                    break;
                case Enumerators.TutorialHandPointerType.Animated:
                    ChangeHandState(Enumerators.TutorialHandState.Pointing);
                    _allSequences.Add(InternalTools.DoActionDelayed(() => ChangeHandState(Enumerators.TutorialHandState.Pressed), Interval));
                    _allSequences.Add(InternalTools.DoActionDelayed(Move, Interval * 2));
                    break;
                default:
                    break;
            }
        }

        private void Move()
        {
            if (_wasDisposed)
                return;

            ChangeHandState(0);
            _isMove = true;

            _startPoint = GetPositionByOwner(_ownerObject, _startPosition);
            _endPoint = GetPositionByOwner(_targetObject, _endPosition);
            _sideTurn = _startPoint.x > _endPosition.x ? 1 : -1;
            _sineOffset = 0;

            _durationMove = Vector2.Distance(_startPoint, _endPoint) / _speedMove;

            Sequence moveSequence = DOTween.Sequence();
            _allSequences.Add(moveSequence);
            moveSequence.Append(_selfObject.transform.DOMove(_endPoint, _durationMove)
                .OnComplete(() =>
                {
                    _allSequences.Add(InternalTools.DoActionDelayed(End, Interval));
                }));
        }

        private void End()
        {
            if (_wasDisposed)
                return;

            ChangeHandState(Enumerators.TutorialHandState.Pointing);
            _stayInEndPoint = true;

            if (_type == Enumerators.TutorialHandPointerType.Single)
            {
                _allSequences.Add(InternalTools.DoActionDelayed(() => ChangeHandState(Enumerators.TutorialHandState.Pressed), Interval));
            }

            if(!_appearOnce)
            {
                _allSequences.Add(InternalTools.DoActionDelayed(Start, Interval * 2));
            }
        }

        private void ChangeHandState(Enumerators.TutorialHandState handState)
        {
            if (_wasDisposed)
                return;

            _handRenderer.sprite = _handStates[(int)handState];
        }

        public void Update()
        {
            if (_wasDisposed)
                return;

            _endPoint = GetPositionByOwner(_targetObject, _endPosition);

            if (_stayInEndPoint && _type == Enumerators.TutorialHandPointerType.Animated)
            {
                _selfObject.transform.position = _endPoint;
            }
            else if(_type == Enumerators.TutorialHandPointerType.Single)
            {
                _selfObject.transform.position = GetPositionByOwner(_ownerObject, _startPosition);
            }

            if(_isMove)
            {
                _sineOffset = _startValue + _maxValue - (_maxValue / Vector3.Distance(_startPoint, _endPoint) *
                    Vector3.Distance(_selfObject.transform.position, _endPoint));
                float angle = Mathf.Sin(_sineOffset) * _sideTurn;
                _handRenderer.transform.localPosition = new Vector3(angle, 0, 0);

                if (_selfObject.transform.position == _endPoint)
                {
                    _isMove = false;
                }
            }

            for (int i = 0; i < _allSequences.Count; i++)
            {
                if(_allSequences[i] == null || !_allSequences[i].active)
                {
                    _allSequences.Remove(_allSequences[i]);
                }
            }
        }

        public void Activate()
        {
            _wasDisposed = false;
            StartSequence();
        }

        public void Deactivate()
        {
            _wasDisposed = true;
            _isMove = false;
            ClearSequences();
            _selfObject?.SetActive(false);
        }

        private Vector3 GetPositionByOwner(GameObject owner, Vector3 position)
        {
            if (owner != null)
            {
                if (Owner == Enumerators.TutorialObjectOwner.UI)
                {
                    return _ownerObject.transform.position + position;
                }
                else
                {
                    return owner.transform.TransformPoint(_startPosition);
                }
            }
            return position;
        }

        private void ClearSequences()
        {
            if (_allSequences != null && _allSequences.Count > 0)
            {
                foreach (Sequence sequence in _allSequences)
                {
                    sequence?.Kill();
                }
            }
        }

        public void Dispose()
        {
            _wasDisposed = true;

            if (_selfObject != null)
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}

using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Loom.ZombieBattleground.SpecificBattlegroundInfo;

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

        public void DrawPointer(Enumerators.TutorialHandPointerType type, Vector3 begin, Vector3? end = null, float appearDelay = 0, bool appearOnce = false, int tutorialObjectIdStepOwner = 0, int handOrder = 0)
        {
            HandPointerPopup popup = new HandPointerPopup(type, begin, end, appearDelay, appearOnce, tutorialObjectIdStepOwner, handOrder);
            _handPointerPopups.Add(popup);
        }
    }

    public class HandPointerPopup
    {
        private readonly ITutorialManager _tutorialManager;
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly IGameplayManager _gameplayManager;

        private const float durationMove = 2f;
        private const float interval = 0.3f;

        private Enumerators.TutorialHandPointerType _type;

        private GameObject _selfObject;

        private SpriteRenderer _handRenderer;

        private Sprite[] _handStates;

        private Vector3 _startPosition;

        private Vector3 _endPosition;

        private Vector3 _startPoint;

        private float _appearDelay;

        private bool _appearOnce;

        private bool _stayInEndPoint;

        private BoardUnitView _ownerUnit;

        private float _maxValue = 3;
        private float _startValue = 0;
        private float _sideTurn;

        private bool _isMove = false;

        private float _sineOffset = 0;


        public HandPointerPopup(Enumerators.TutorialHandPointerType type, Vector3 begin, Vector3? end = null, float appearDelay = 0, bool appearOnce = false, int tutorialObjectIdStepOwner = 0, int handOrder = 0)
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _startPosition = begin;
            _appearDelay = appearDelay;
            _appearOnce = appearOnce;
            _type = type;

            _selfObject = MonoBehaviour.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/HandPointer"));

            if(tutorialObjectIdStepOwner != 0)
            {
                _selfObject.transform.SetParent(_ownerUnit.Transform, false);
            }

            _selfObject.SetActive(false);

            _handRenderer = _selfObject.transform.Find("Hand").GetComponent<SpriteRenderer>();
            if (handOrder != 0)
            {
                _handRenderer.sortingOrder = handOrder;
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
                    _endPosition = (Vector3)end;
                    break;
                default:
                    break;
            }

            StartSequence();
        }

        private void StartSequence()
        {
            InternalTools.DoActionDelayed(Start, _appearDelay);
        }

        private void Start()
        {
            if (_selfObject == null)
                return;

            _selfObject.SetActive(true);

            _stayInEndPoint = false;
            if (_ownerUnit != null)
            {
                _selfObject.transform.localPosition = _startPosition;
            }
            else
            {
                _selfObject.transform.position = _startPosition;
            }
            _handRenderer.transform.localPosition = Vector3.zero;

            switch (_type)
            {
                case Enumerators.TutorialHandPointerType.Single:
                    ChangeHandState(Enumerators.TutorialHandState.Drag);
                    InternalTools.DoActionDelayed(End, interval);
                    break;
                case Enumerators.TutorialHandPointerType.Animated:
                    ChangeHandState(Enumerators.TutorialHandState.Pointing);
                    InternalTools.DoActionDelayed(() => ChangeHandState(Enumerators.TutorialHandState.Pressed), interval);                   
                    InternalTools.DoActionDelayed(Move, interval * 2);
                    break;
                default:
                    break;
            }
        }

        private void Move()
        {
            if (_selfObject == null)
                return;

            ChangeHandState(0);
            _isMove = true;
            _startPoint = _ownerUnit != null ? _ownerUnit.Transform.position + _startPosition : _startPosition;           
            _sideTurn = _startPoint.x > _endPosition.x ? 1 : -1;
            _sineOffset = 0;
            _selfObject.transform.DOMove(_endPosition, durationMove)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    InternalTools.DoActionDelayed(End, interval);
                });
        }

        private void End()
        {
            if (_selfObject == null)
                return;

            ChangeHandState(Enumerators.TutorialHandState.Pointing);
            _stayInEndPoint = true;

            if (_type == Enumerators.TutorialHandPointerType.Single)
            {
                InternalTools.DoActionDelayed(() => ChangeHandState(Enumerators.TutorialHandState.Pressed), interval);
            }

            if(!_appearOnce)
            {
                InternalTools.DoActionDelayed(Start, interval * 2);
            }
        }

        private void ChangeHandState(Enumerators.TutorialHandState handState)
        {
            if (_selfObject == null)
                return;

            _handRenderer.sprite = _handStates[(int)handState];
        }

        public void Update()
        {
            if (_selfObject == null)
                return;

            if(_stayInEndPoint && _type == Enumerators.TutorialHandPointerType.Animated)
            {
                _selfObject.transform.position = _endPosition;
            }
            else if(_type == Enumerators.TutorialHandPointerType.Single)
            {
                _selfObject.transform.position = _startPosition;
            }

            if(_isMove)
            {
                _sineOffset = _startValue + _maxValue - (_maxValue / Vector3.Distance(_startPoint, _endPosition) *
                    Vector3.Distance(_selfObject.transform.position, _endPosition));
                float angle = Mathf.Sin(_sineOffset) * _sideTurn;
                _handRenderer.transform.localPosition = new Vector3(angle, 0, 0);

                if (_selfObject.transform.position == _endPosition)
                {
                    _isMove = false;
                }
            }
        }

        public void Dispose()
        {
            if (_selfObject != null)
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using System.Linq;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class InputController : IController
    {
        public event Action<BoardUnitView> UnitSelectedEvent;

        public event Action<BoardUnitView> UnitDeselectedEvent;

        public event Action<BoardUnitView> UnitSelectingEvent;

        public event Action<BoardUnitView> UnitPointerEnteredEvent;

        public event Action<Player> PlayerSelectedEvent;

        public event Action<Player> PlayerPointerEnteredEvent;

        public event Action<Player> PlayerSelectingEvent;

        public event Action<GameObject> ManaBarSelected;

        public event Action<GameObject> ManaBarPointerEntered;

        public event Action NoObjectsSelectedEvent;

        public event Action<BoardObject> ClickedOnBoardObjectEvent;

        public event Action<BoardObject> DragOnBoardObjectEvent;

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private Camera _raysCamera;

        private List<BoardUnitView> _selectedUnitsList;

        private GameObject _hoveringObject;

        private float _timeHovering;

        private bool _isHovering;

        private PointerEventSolver _pointerEventSolver;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _selectedUnitsList = new List<BoardUnitView>();

            _pointerEventSolver = new PointerEventSolver();
            _pointerEventSolver.Clicked += PointerClickedHandler;
            _pointerEventSolver.Ended += PointerEndedHandler;
            _pointerEventSolver.DragStarted += PointerDragStartedHandler;
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameplayReady() && !_tutorialManager.IsTutorial)
                return;

            HandleInput();
        }

        public void ResetAll()
        {
            _selectedUnitsList.Clear();
        }

        private void HandleInput()
        {
            _pointerEventSolver.Update();

            if (Application.isMobilePlatform)
            {
                foreach (Touch touch in Input.touches)
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.TapOnScreen);

                            CastRay(touch.position, SRLayerMask.Battleground);
                            _pointerEventSolver.PushPointer(PointerEventSolver.DefaultDelta);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            CastRay(touch.position, SRLayerMask.Battleground, true);
                            break;
                        case TouchPhase.Canceled:
                        case TouchPhase.Ended:
                            _pointerEventSolver.PopPointer();

                            foreach (BoardUnitView unit in _selectedUnitsList)
                            {
                                UnitDeselectedEvent?.Invoke(unit);
                            }

                            _selectedUnitsList.Clear();
                            break;
                    }
                }
            }
            else
            {
                if (_gameplayManager.IsTutorial && !_gameplayManager.GetController<BoardArrowController>().IsBoardArrowNowInTheBattle)
                {
                    CastRay(Input.mousePosition, SRLayerMask.Battleground, isHovering: true);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.TapOnScreen);

                    CastRay(Input.mousePosition, SRLayerMask.Battleground);
                    _pointerEventSolver.PushPointer(PointerEventSolver.DefaultDelta);
                }
                else if (Input.GetMouseButton(0))
                {
                    CastRay(Input.mousePosition, SRLayerMask.Battleground, true);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _pointerEventSolver.PopPointer();

                    foreach (BoardUnitView unit in _selectedUnitsList)
                    {
                        UnitDeselectedEvent?.Invoke(unit);
                    }

                    _selectedUnitsList.Clear();
                }

                if (_gameplayManager.GetController<BoardArrowController>().IsBoardArrowNowInTheBattle)
                {
                    if (_gameplayManager.GetController<BoardArrowController>().CurrentBoardArrow is AbilityBoardArrow)
                    {
                        CastRay(Input.mousePosition, SRLayerMask.Battleground, true);
                    }
                }
            }
        }

        private void CastRay(Vector3 origin, int layerMask, bool permanent = false, bool isHovering = false)
        {
            _raysCamera = Camera.main;

            Vector3 point = _raysCamera.ScreenToWorldPoint(origin);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, layerMask);
            hits = hits.Where(hit => !hit.collider.name.Equals(Constants.BattlegroundTouchZone)).ToArray();

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    CheckColliders(hit.collider, permanent, isHovering);
                }
            }
            else
            {
                NoObjectsSelectedEvent?.Invoke();
                ClearHovering();
            }
        }

        private void CheckColliders(Collider2D collider, bool permanent = false, bool isHovering = false)
        {
            if (collider.name.Equals(Constants.PlayerBoard) ||
                collider.name.Equals(Constants.OpponentBoard))
            {
                ClearHovering();
                NoObjectsSelectedEvent?.Invoke();
                return;
            }

            bool hasPointerTarget = false;
            bool hasTarget = false;

            hasPointerTarget = ProcessTutorialActions(ref hasPointerTarget, collider, isHovering);

            ProcessUnits(ref hasTarget, ref hasPointerTarget, _gameplayManager.CurrentPlayer.BoardCards, collider, permanent, isHovering);
            ProcessUnits(ref hasTarget, ref hasPointerTarget, _gameplayManager.OpponentPlayer.BoardCards, collider, permanent, isHovering);

            ProcessPlayers(ref hasTarget, ref hasPointerTarget, _gameplayManager.CurrentPlayer, collider, permanent, isHovering);
            ProcessPlayers(ref hasTarget, ref hasPointerTarget, _gameplayManager.OpponentPlayer, collider, permanent, isHovering);

            if (!hasTarget)
            {
                NoObjectsSelectedEvent?.Invoke();
            }
            if (!hasPointerTarget)
            {
                ClearHovering();
            }
        }

        private void UpdateHovering(GameObject obj, Player player = null, BoardUnitView unit = null, BoardCard boardCard = null, bool isManaBar = false)
        {
            if (_hoveringObject != obj)
            {
                _isHovering = false;
                _hoveringObject = obj;
                _timeHovering = 0;
            }
            else if (!_isHovering)
            {
                _timeHovering += Time.deltaTime;
                if (_timeHovering >= Constants.MaxTimeForHovering)
                {
                    if (unit != null)
                    {
                        UnitPointerEnteredEvent?.Invoke(unit);
                    }
                    else if (player != null)
                    {
                        PlayerPointerEnteredEvent?.Invoke(player);
                    }
                    else if (boardCard != null)
                    {
                        GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.PlayerCardInHandSelected);
                    }
                    else if (isManaBar)
                    {
                        ManaBarPointerEntered?.Invoke(obj);
                    }
                    _isHovering = true;
                }
            }
        }

        private void ClearHovering()
        {
            _isHovering = false;
            _hoveringObject = null;
            _timeHovering = 0;
        }

        private (bool, bool) ProcessUnits(
                ref bool hasTarget,
                ref bool hasPoinerTarget,
                UniquePositionedList<BoardUnitView> BoardCards,
                Collider2D collider,
                bool permanent = false,
                bool isHovering = false)
        {
            foreach (BoardUnitView unit in BoardCards)
            {
                if (unit.GameObject.GetInstanceID() == collider.gameObject.GetInstanceID())
                {
                    hasTarget = true;
                    hasPoinerTarget = true;

                    if (isHovering)
                    {
                        UpdateHovering(collider.gameObject, unit: unit);
                    }
                    else
                    {
                        if (!permanent)
                        {
                            if (!_selectedUnitsList.Contains(unit))
                            {
                                _selectedUnitsList.Add(unit);
                            }

                            UnitSelectedEvent?.Invoke(unit);
                        }
                        else
                        {
                            UnitSelectingEvent?.Invoke(unit);
                        }
                    }
                    break;
                }
            }

            return (hasTarget, hasPoinerTarget);
        }

        private bool ProcessTutorialActions(
                ref bool hasPoinerTarget,
                Collider2D collider,
                bool isHovering = false)
        {
            if (_gameplayManager.IsTutorial)
            {
                if (collider.name.Equals(Constants.PlayerManaBar))
                {
                    hasPoinerTarget = true;

                    if (isHovering)
                    {
                        UpdateHovering(collider.gameObject, isManaBar: true);
                    }
                    else
                    {
                        ManaBarSelected?.Invoke(collider.gameObject);
                    }
                }
                else
                {
                    BoardCard boardCard = _gameplayManager.GetController<BattlegroundController>().GetBoardCardFromHisObject(collider.gameObject);

                    if (boardCard != null)
                    {
                        hasPoinerTarget = true;

                        if (isHovering)
                        {
                            UpdateHovering(collider.gameObject, boardCard: boardCard);
                        }
                    }
                }
            }

            return hasPoinerTarget;
        }

        private (bool, bool) ProcessPlayers(
                ref bool hasTarget,
                ref bool hasPoinerTarget,
                Player player,
                Collider2D collider,
                bool permanent = false,
                bool isHovering = false)
        {
            if (player.AvatarObject == collider.gameObject)
            {
                hasTarget = true;
                hasPoinerTarget = true;

                if (isHovering)
                {
                    UpdateHovering(collider.gameObject, player);
                }
                else
                {
                    if (!permanent)
                    {
                        PlayerSelectedEvent?.Invoke(player);
                    }
                    else
                    {
                        PlayerSelectingEvent?.Invoke(player);
                    }
                }
            }

            return (hasTarget, hasPoinerTarget);
        }

        private void PointerClickedHandler()
        {
            BoardObject boardObject = null;

            if (_selectedUnitsList.Count > 0)
            {
                boardObject = _selectedUnitsList[0].Model;
            }

            if (boardObject != null)
            {
                InternalTools.DoActionDelayed(() =>
                {
                    ClickedOnBoardObjectEvent?.Invoke(boardObject);
                }, Time.deltaTime);
            }
        }

        private void PointerEndedHandler()
        {
        }

        private void PointerDragStartedHandler()
        {
            BoardObject boardObject = null;

            if (_selectedUnitsList.Count > 0)
            {
                boardObject = _selectedUnitsList[0].Model;
            }

            if (boardObject != null)
            {
                DragOnBoardObjectEvent?.Invoke(boardObject);
            }
        }
    }
}

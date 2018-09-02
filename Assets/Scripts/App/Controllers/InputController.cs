using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class InputController : IController
    {
        private readonly int _unitsLayerMask = 9;

        public Action<BoardUnit> UnitSelectedEvent;

        public Action<BoardUnit> UnitDeselectedEvent;

        public Action<BoardUnit> UnitSelectingEvent;

        public Action<Player> PlayerSelectedEvent;

        public Action<Player> PlayerSelectingEvent;

        public Action NoObjectsSelectedEvent;

        private IGameplayManager _gameplayManager;

        private Camera _raysCamera;

        private List<BoardUnit> _selectedUnitsList;

        private int _playersLayerMask = 9;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _selectedUnitsList = new List<BoardUnit>();
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameplayReady())

                return;

            HandleInput();
        }

        public void ResetAll()
        {
            _selectedUnitsList.Clear();
        }

        private void HandleInput()
        {
            if (Application.isMobilePlatform)
            {
                foreach (Touch touch in Input.touches)
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            CastRay(touch.position, _unitsLayerMask);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            CastRay(touch.position, _unitsLayerMask, true);
                            break;
                        case TouchPhase.Canceled:
                        case TouchPhase.Ended:
                            foreach (BoardUnit unit in _selectedUnitsList)
                            {
                                UnitDeselectedEvent?.Invoke(unit);
                            }

                            _selectedUnitsList.Clear();
                            break;
                    }
                }
            } else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CastRay(Input.mousePosition, _unitsLayerMask);
                } else if (Input.GetMouseButton(0))
                {
                    CastRay(Input.mousePosition, _unitsLayerMask, true);
                } else if (Input.GetMouseButtonUp(0))
                {
                    foreach (BoardUnit unit in _selectedUnitsList)
                    {
                        UnitDeselectedEvent?.Invoke(unit);
                    }

                    _selectedUnitsList.Clear();
                }
            }
        }

        private void CastRay(Vector3 origin, int layerMask, bool permanent = false)
        {
            _raysCamera = Camera.main;

            Vector3 point = _raysCamera.ScreenToWorldPoint(origin);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, 1 << layerMask);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    CheckColliders(hit.collider, permanent);
                }
            } else
            {
                NoObjectsSelectedEvent?.Invoke();
            }
        }

        private void CheckColliders(Collider2D collider, bool permanent = false)
        {
            if (collider.name.Equals(Constants.PLAYER_BOARD) || collider.name.Equals(Constants.OPPONENT_BOARD))
            {
                NoObjectsSelectedEvent?.Invoke();

                return;
            }

            // check on units
            bool hasTarget = false;

            foreach (BoardUnit unit in _gameplayManager.CurrentPlayer.BoardCards)
            {
                if (unit.gameObject == collider.gameObject)
                {
                    hasTarget = true;

                    if (!permanent)
                    {
                        if (!_selectedUnitsList.Contains(unit))
                        {
                            _selectedUnitsList.Add(unit);
                        }

                        UnitSelectedEvent?.Invoke(unit);
                    } else
                    {
                        UnitSelectingEvent?.Invoke(unit);
                    }

                    break;
                }
            }

            foreach (BoardUnit unit in _gameplayManager.OpponentPlayer.BoardCards)
            {
                if (unit.gameObject == collider.gameObject)
                {
                    hasTarget = true;

                    if (!permanent)
                    {
                        if (!_selectedUnitsList.Contains(unit))
                        {
                            _selectedUnitsList.Add(unit);
                        }

                        UnitSelectedEvent?.Invoke(unit);
                    } else
                    {
                        UnitSelectingEvent?.Invoke(unit);
                    }

                    break;
                }
            }

            // check on players
            if (_gameplayManager.CurrentPlayer.AvatarObject == collider.gameObject)
            {
                hasTarget = true;

                if (!permanent)
                {
                    PlayerSelectedEvent?.Invoke(_gameplayManager.CurrentPlayer);
                } else
                {
                    PlayerSelectingEvent?.Invoke(_gameplayManager.CurrentPlayer);
                }
            }

            if (_gameplayManager.OpponentPlayer.AvatarObject == collider.gameObject)
            {
                hasTarget = true;

                if (!permanent)
                {
                    PlayerSelectedEvent?.Invoke(_gameplayManager.OpponentPlayer);
                } else
                {
                    PlayerSelectingEvent?.Invoke(_gameplayManager.OpponentPlayer);
                }
            }

            if (!hasTarget)
            {
                NoObjectsSelectedEvent?.Invoke();
            }
        }
    }
}

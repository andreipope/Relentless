using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class InputController : IController
    {
        public Action<BoardUnitView> UnitSelectedEvent;

        public Action<BoardUnitView> UnitDeselectedEvent;

        public Action<BoardUnitView> UnitSelectingEvent;

        public Action<Player> PlayerSelectedEvent;

        public Action<Player> PlayerSelectingEvent;

        public Action NoObjectsSelectedEvent;

        private IGameplayManager _gameplayManager;

        private Camera _raysCamera;

        private List<BoardUnitView> _selectedUnitsList;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _selectedUnitsList = new List<BoardUnitView>();
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
                            CastRay(touch.position, SRLayerMask.Battleground);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            CastRay(touch.position, SRLayerMask.Battleground, true);
                            break;
                        case TouchPhase.Canceled:
                        case TouchPhase.Ended:
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
                if (Input.GetMouseButtonDown(0))
                {
                    CastRay(Input.mousePosition, SRLayerMask.Battleground);
                }
                else if (Input.GetMouseButton(0))
                {
                    CastRay(Input.mousePosition, SRLayerMask.Battleground, true);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    foreach (BoardUnitView unit in _selectedUnitsList)
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

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, layerMask);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    CheckColliders(hit.collider, permanent);
                }
            }
            else
            {
                NoObjectsSelectedEvent?.Invoke();
            }
        }

        private void CheckColliders(Collider2D collider, bool permanent = false)
        {
            if (collider.name.Equals(Constants.PlayerBoard) || collider.name.Equals(Constants.OpponentBoard))
            {
                NoObjectsSelectedEvent?.Invoke();
                return;
            }

            // check on units
            bool hasTarget = false;

            foreach (BoardUnitView unit in _gameplayManager.CurrentPlayer.BoardCards)
            {
                if (unit.GameObject == collider.gameObject)
                {
                    hasTarget = true;

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

                    break;
                }
            }

            foreach (BoardUnitView unit in _gameplayManager.OpponentPlayer.BoardCards)
            {
                if (unit.GameObject == collider.gameObject)
                {
                    hasTarget = true;

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
                }
                else
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
                }
                else
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

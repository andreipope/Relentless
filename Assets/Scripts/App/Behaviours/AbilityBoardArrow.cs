using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AbilityBoardArrow : BoardArrow
    {
        public List<Enumerators.AbilityTargetType> PossibleTargets = new List<Enumerators.AbilityTargetType>();

        public BoardUnit SelfBoardCreature;

        public Enumerators.CardType TargetUnitType;

        public Enumerators.UnitStatusType TargetUnitStatusType;

        private IInputManager _inputManager;

        private int _onMouseDownInputIndex;

        private int _onRightMouseDownInputIndex;

        private int _onEscapeInputIndex;

        public event Action<BoardUnit> OnCardSelectedEvent;

        public event Action<BoardUnit> OnCardUnselectedevent;

        public event Action<Player> OnPlayerSelectedEvent;

        public event Action<Player> OnPlayerUnselectedEvent;

        public event Action OnInputEndEvent;

        public event Action OnInputCancelEvent;

        public override void OnCardSelected(BoardUnit unit)
        {
            if (unit.CurrentHp <= 0)
                return;

            if (PossibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER_CARD) &&
                unit.GameObject.CompareTag(SRTags.PlayerOwned) ||
                PossibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD) &&
                unit.GameObject.CompareTag(SRTags.OpponentOwned) ||
                PossibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                if ((TargetUnitType == Enumerators.CardType.NONE) || (unit.InitialUnitType == TargetUnitType))
                {
                    if ((TargetUnitStatusType == Enumerators.UnitStatusType.NONE) || (unit.UnitStatus == TargetUnitStatusType))
                    {
                        if (SelfBoardCreature != unit)
                        {
                            SelectedCard?.SetSelectedUnit(false);

                            SelectedCard = unit;
                            SelectedPlayer?.SetGlowStatus(false);

                            SelectedPlayer = null;
                            CreateTarget(unit.Transform.position);
                            SelectedCard.SetSelectedUnit(true);

                            OnCardSelectedEvent?.Invoke(unit);
                        }
                    }
                }
            }
        }

        public override void OnCardUnselected(BoardUnit creature)
        {
            if (SelectedCard == creature)
            {
                SelectedCard.SetSelectedUnit(false);

                // _targetObjectsGroup.SetActive(false);
                OnCardUnselectedevent?.Invoke(creature);
            }

            SelectedCard = null;
        }

        public override void OnPlayerSelected(Player player)
        {
            if (player.Hp <= 0)
                return;

            if ((PossibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER) && player.AvatarObject.CompareTag(SRTags.PlayerOwned)) || (PossibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT) && player.AvatarObject.CompareTag(SRTags.OpponentOwned)) || PossibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                SelectedPlayer = player;
                SelectedCard?.SetSelectedUnit(false);

                SelectedCard = null;
                CreateTarget(player.AvatarObject.transform.position);
                SelectedPlayer.SetGlowStatus(true);
                OnPlayerSelectedEvent?.Invoke(player);
            }
        }

        public override void OnPlayerUnselected(Player player)
        {
            if (SelectedPlayer == player)
            {
                SelectedCard?.SetSelectedUnit(false);

                SelectedCard = null;

                SelectedPlayer.SetGlowStatus(false);

                // _targetObjectsGroup.SetActive(false);
                OnPlayerUnselectedEvent?.Invoke(player);
            }

            SelectedPlayer = null;
        }

        protected void Awake()
        {
            _inputManager = GameClient.Get<IInputManager>();

            Init();

            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    _onMouseDownInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 0, OnMouseButtonUpHandler, null, null);
                    _onRightMouseDownInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 1, OnRightMouseButtonUpHandler, null, null);
                    _onEscapeInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.KEYBOARD, (int)KeyCode.Escape, null, OnRightMouseButtonUpHandler, null);
                },
                null,
                Time.fixedDeltaTime);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _inputManager.UnregisterInputHandler(_onMouseDownInputIndex);
            _inputManager.UnregisterInputHandler(_onRightMouseDownInputIndex);
            _inputManager.UnregisterInputHandler(_onEscapeInputIndex);
        }

        protected override void Update()
        {
            base.Update();
        }

        protected void OnMouseButtonUpHandler()
        {
            if (StartedDrag)
            {
                StartedDrag = false;
                OnInputEndEvent?.Invoke();
            }
        }

        protected void OnRightMouseButtonUpHandler()
        {
            if (StartedDrag)
            {
                StartedDrag = false;
                OnInputCancelEvent?.Invoke();
            }
        }
    }
}

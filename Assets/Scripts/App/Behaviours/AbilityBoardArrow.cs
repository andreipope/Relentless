using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AbilityBoardArrow : BoardArrow
    {
        public List<Enumerators.Target> PossibleTargets = new List<Enumerators.Target>();

        public BoardUnitView SelfBoardCreature;

        public Enumerators.CardType TargetUnitType;

        public Enumerators.UnitSpecialStatus TargetUnitSpecialStatusType;

        public int UnitDefense = 0;

        public int UnitCost = 0;

        public AbilityBase OwnerOfThis;

        public Enumerators.AbilitySubTrigger SubTrigger;

        private IInputManager _inputManager;

        private int _onMouseDownInputIndex;

        private int _onRightMouseDownInputIndex;

        private int _onEscapeInputIndex;

        public event Action<BoardUnitView> CardSelected;

        public event Action<BoardUnitView> CardUnselected;

        public event Action<Player> PlayerSelected;

        public event Action<Player> PlayerUnselected;

        public event Action InputEnded;

        public event Action InputCanceled;

        public override void OnCardSelected(BoardUnitView unit)
        {
            if (unit.Model.CurrentDefense <= 0 || unit.Model.IsDead || !unit.Model.IsUnitActive)
                return;

            if (TutorialManager.IsTutorial)
            {
                if ((!unit.Model.OwnerPlayer.IsLocalPlayer &&
                    !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTarget.OPPONENT_CARD)) ||
                    (unit.Model.OwnerPlayer.IsLocalPlayer &&
                    !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTarget.PLAYER_CARD)))
                    return;
            }

            if (PossibleTargets.Contains(Enumerators.Target.PLAYER_CARD) &&
                unit.GameObject.CompareTag(SRTags.PlayerOwned) ||
                PossibleTargets.Contains(Enumerators.Target.OPPONENT_CARD) &&
                unit.GameObject.CompareTag(SRTags.OpponentOwned) ||
                PossibleTargets.Contains(Enumerators.Target.ALL))
            {
                if(SubTrigger == Enumerators.AbilitySubTrigger.CardCostMoreThanCostOfThis)
                {
                    if (unit.Model.Card.InstanceCard.Cost <= OwnerOfThis.CardModel.InstanceCard.Cost)
                        return;
                }

                if (TargetUnitType == Enumerators.CardType.UNDEFINED || unit.Model.InitialUnitType == TargetUnitType)
                {
                    if (TargetUnitSpecialStatusType == Enumerators.UnitSpecialStatus.NONE ||
                        unit.Model.UnitSpecialStatus == TargetUnitSpecialStatusType)
                    {
                        if ((UnitDefense > 0 && unit.Model.CurrentDefense <= UnitDefense) || UnitDefense == 0)
                        {
                            if (unit.Model.Card.InstanceCard.Cost <= UnitCost || UnitCost == 0)
                            {
                                if (SelfBoardCreature != unit)
                                {
                                    SelectedCard?.SetSelectedUnit(false);

                                    SelectedCard = unit;
                                    SelectedPlayer?.SetGlowStatus(false);

                                    SelectedPlayer = null;
                                    SelectedCard.SetSelectedUnit(true);

                                    CardSelected?.Invoke(unit);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void OnCardUnselected(BoardUnitView creature)
        {
            if (SelectedCard == creature)
            {
                SelectedCard.SetSelectedUnit(false);
                CardUnselected?.Invoke(creature);
            }

            SelectedCard = null;
        }

        public override void OnPlayerSelected(Player player)
        {
            if (player.Defense <= 0)
                return;

            if (TutorialManager.IsTutorial)
            {
                if ((!player.IsLocalPlayer &&
                    !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTarget.OPPONENT)) ||
                    (player.IsLocalPlayer &&
                    !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTarget.PLAYER)))
                    return;
            }

            if (PossibleTargets.Contains(Enumerators.Target.PLAYER) &&
                player.AvatarObject.CompareTag(SRTags.PlayerOwned) ||
                PossibleTargets.Contains(Enumerators.Target.OPPONENT) &&
                player.AvatarObject.CompareTag(SRTags.OpponentOwned) ||
                PossibleTargets.Contains(Enumerators.Target.ALL))
            {
                SelectedPlayer = player;
                SelectedCard?.SetSelectedUnit(false);

                SelectedCard = null;
                SelectedPlayer.SetGlowStatus(true);
                PlayerSelected?.Invoke(player);
            }
        }

        public override void OnPlayerUnselected(Player player)
        {
            if (SelectedPlayer == player)
            {
                SelectedCard?.SetSelectedUnit(false);
                SelectedCard = null;
                SelectedPlayer.SetGlowStatus(false);

                PlayerUnselected?.Invoke(player);
            }

            SelectedPlayer = null;
        }

        protected void Awake()
        {
            _inputManager = GameClient.Get<IInputManager>();

            Init();

            InternalTools.DoActionDelayed(() =>
            {
                _onMouseDownInputIndex =
                    _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 0, OnMouseButtonUpHandler);
                _onRightMouseDownInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 1,
                    OnRightMouseButtonUpHandler);
                _onEscapeInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.KEYBOARD,
                    (int)KeyCode.Escape, null, OnRightMouseButtonUpHandler);
            }, 0.75f);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _inputManager.UnregisterInputHandler(_onMouseDownInputIndex);
            _inputManager.UnregisterInputHandler(_onRightMouseDownInputIndex);
            _inputManager.UnregisterInputHandler(_onEscapeInputIndex);
        }

        protected void OnMouseButtonUpHandler()
        {
            if (StartedDrag)
            {
                StartedDrag = false;
                InputEnded?.Invoke();
            }
        }

        protected void OnRightMouseButtonUpHandler()
        {
            if (StartedDrag)
            {
                StartedDrag = false;
                InputCanceled?.Invoke();
            }
        }
    }
}

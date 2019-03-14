using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground
{
    public class BattleBoardArrow : BoardArrow
    {
        public UniqueList<BoardObject> IgnoreBoardObjectsList;

        public IReadOnlyList<BoardUnitModel> BoardCards;

        public BoardUnitModel Owner;

        public bool IgnoreHeavy;

        public Enumerators.UnitStatusType TargetUnitStatusType;

        public List<Enumerators.UnitStatusType> BlockedUnitStatusTypes;

        public void End(BoardUnitView creature)
        {
            if (!StartedDrag)
                return;

            StartedDrag = false;

            BoardObject target = null;

            if (SelectedCard != null)
            {
                target = SelectedCard.BoardUnitModel;
            }
            else if (SelectedPlayer != null)
            {
                target = SelectedPlayer;
            }

            if (target != null)
            {
                creature.BoardUnitModel.DoCombat(target);

                if (target == SelectedPlayer)
                {
                    creature.BoardUnitModel.OwnerPlayer.ThrowCardAttacked(creature.BoardUnitModel, SelectedPlayer.InstanceId);
                }
                else
                {
                    creature.BoardUnitModel.OwnerPlayer.ThrowCardAttacked(creature.BoardUnitModel, SelectedCard.BoardUnitModel.Card.InstanceId);
                }
            }
            else
            {
                if(TutorialManager.IsTutorial)
                {
                    TutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerBattleframe);
                }
            }

            Dispose();
        }

        public override void OnCardSelected(BoardUnitView unit)
        {
            SelectedPlayer = null;
            SelectedPlayer?.SetGlowStatus(false);

            if (TutorialManager.IsTutorial &&
                !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTargetType.OPPONENT_CARD))
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(unit.BoardUnitModel))
                return;

            if (unit.BoardUnitModel.CurrentHp <= 0 || unit.BoardUnitModel.IsDead)
                return;

            if (ElementType.Count > 0 && !ElementType.Contains(unit.BoardUnitModel.Card.Prototype.CardSetType))
                return;

            if (BlockedUnitStatusTypes == null) 
            {
                BlockedUnitStatusTypes = new List<Enumerators.UnitStatusType>();
            }

            if (TargetsType.Contains(Enumerators.SkillTargetType.ALL_CARDS) ||
                TargetsType.Contains(Enumerators.SkillTargetType.PLAYER_CARD) &&
                unit.Transform.CompareTag(SRTags.PlayerOwned) ||
                TargetsType.Contains(Enumerators.SkillTargetType.OPPONENT_CARD) &&
                unit.Transform.CompareTag(SRTags.OpponentOwned))
            {
                bool opponentHasProvoke = OpponentHasHeavyUnits();
                if (!opponentHasProvoke || opponentHasProvoke && unit.BoardUnitModel.IsHeavyUnit || IgnoreHeavy)
                {
                    if ((TargetUnitStatusType == Enumerators.UnitStatusType.NONE ||
                        unit.BoardUnitModel.UnitStatus == TargetUnitStatusType) &&
                        !BlockedUnitStatusTypes.Contains(unit.BoardUnitModel.UnitStatus))
                    {
                        SelectedCard?.SetSelectedUnit(false);

                        SelectedCard = unit;
                        SelectedCard.SetSelectedUnit(true);
                    }
                }
            }
        }

        public override void OnCardUnselected(BoardUnitView creature)
        {
            if (SelectedCard == creature)
            {
                SelectedCard.SetSelectedUnit(false);
                SelectedCard = null;
            }

            SelectedPlayer?.SetGlowStatus(false);
            SelectedPlayer = null;
        }

        public override void OnPlayerSelected(Player player)
        {
            SelectedCard?.SetSelectedUnit(false);
            SelectedCard = null;

            if (TutorialManager.IsTutorial &&
                !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTargetType.OPPONENT))
                return;

            if (player.Defense <= 0)
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(player))
                return;

            if (Owner != null && !Owner.HasFeral && Owner.HasBuffRush)
                return;

            if (TargetsType.Contains(Enumerators.SkillTargetType.OPPONENT) &&
                player.AvatarObject.CompareTag(SRTags.OpponentOwned) ||
                TargetsType.Contains(Enumerators.SkillTargetType.PLAYER) &&
                player.AvatarObject.CompareTag(SRTags.PlayerOwned))
            {
                if (!OpponentHasHeavyUnits() || IgnoreHeavy)
                {
                    SelectedPlayer = player;
                    SelectedPlayer.SetGlowStatus(true);
                }
            }
        }

        public override void OnPlayerUnselected(Player player)
        {
            if (SelectedPlayer == player)
            {
                SelectedPlayer.SetGlowStatus(false);
                SelectedPlayer = null;
            }

            SelectedCard?.SetSelectedUnit(false);
            SelectedCard = null;
        }

        protected bool OpponentHasHeavyUnits()
        {
            return BoardCards?.FindAll(x => x.IsHeavyUnit && x.CurrentHp > 0).Count > 0;
        }

        private void Awake()
        {
            Init();
        }
    }
}

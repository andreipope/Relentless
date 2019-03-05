using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground
{
    public class BattleBoardArrow : BoardArrow
    {
        public UniqueList<BoardObject> IgnoreBoardObjectsList;

        public UniquePositionedList<BoardUnitView> BoardCards;

        public BoardUnitView Owner;

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
                target = SelectedCard.Model;
            }
            else if (SelectedPlayer != null)
            {
                target = SelectedPlayer;
            }

            if (target != null)
            {
                creature.Model.DoCombat(target);

                if (target == SelectedPlayer)
                {
                    creature.Model.OwnerPlayer.ThrowCardAttacked(creature.Model.Card, SelectedPlayer.InstanceId);
                }
                else
                {
                    creature.Model.OwnerPlayer.ThrowCardAttacked(creature.Model.Card, SelectedCard.Model.Card.InstanceId);
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

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(unit.Model))
                return;

            if (unit.Model.CurrentHp <= 0 || unit.Model.IsDead)
                return;

            if (ElementType.Count > 0 && !ElementType.Contains(unit.Model.Card.LibraryCard.Faction))
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
                if (!opponentHasProvoke || opponentHasProvoke && unit.Model.IsHeavyUnit || IgnoreHeavy)
                {
                    if ((TargetUnitStatusType == Enumerators.UnitStatusType.NONE ||
                        unit.Model.UnitStatus == TargetUnitStatusType) &&
                        !BlockedUnitStatusTypes.Contains(unit.Model.UnitStatus))
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

            if (Owner != null && !Owner.Model.HasFeral && Owner.Model.HasBuffRush)
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
            return BoardCards?.FindAll(x => x.Model.IsHeavyUnit && x.Model.CurrentHp > 0).Count > 0;
        }

        private void Awake()
        {
            Init();
        }
    }
}

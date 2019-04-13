using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground
{
    public class BattleBoardArrow : BoardArrow
    {
        public UniqueList<IBoardObject> IgnoreBoardObjectsList;

        public IReadOnlyList<CardModel> BoardCards;

        public CardModel Owner;

        public bool IgnoreHeavy;

        public Enumerators.UnitSpecialStatus _targetUnitSpecialStatusType;

        public List<Enumerators.UnitSpecialStatus> BlockedUnitStatusTypes;

        public void End(BoardUnitView creature)
        {
            if (!StartedDrag)
                return;

            StartedDrag = false;

            IBoardObject target = null;

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
                    creature.Model.OwnerPlayer.ThrowCardAttacked(creature.Model, SelectedPlayer.InstanceId);
                }
                else
                {
                    creature.Model.OwnerPlayer.ThrowCardAttacked(creature.Model, SelectedCard.Model.Card.InstanceId);
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
                !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTarget.OPPONENT_CARD))
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(unit.Model))
                return;

            if (unit.Model.CurrentDefense <= 0 || unit.Model.IsDead || !unit.Model.IsUnitActive)
                return;

            if (ElementType.Count > 0 && !ElementType.Contains(unit.Model.Card.Prototype.Faction))
                return;

            if (BlockedUnitStatusTypes == null) 
            {
                BlockedUnitStatusTypes = new List<Enumerators.UnitSpecialStatus>();
            }

            if (TargetsType.Contains(Enumerators.SkillTarget.ALL_CARDS) ||
                TargetsType.Contains(Enumerators.SkillTarget.PLAYER_CARD) &&
                unit.Transform.CompareTag(SRTags.PlayerOwned) ||
                TargetsType.Contains(Enumerators.SkillTarget.OPPONENT_CARD) &&
                unit.Transform.CompareTag(SRTags.OpponentOwned))
            {
                bool opponentHasProvoke = OpponentHasHeavyUnits();
                if (!opponentHasProvoke || opponentHasProvoke && unit.Model.IsHeavyUnit || IgnoreHeavy)
                {
                    if ((_targetUnitSpecialStatusType == Enumerators.UnitSpecialStatus.NONE ||
                        unit.Model.UnitSpecialStatus == _targetUnitSpecialStatusType) &&
                        !BlockedUnitStatusTypes.Contains(unit.Model.UnitSpecialStatus))
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
                !TutorialManager.CurrentTutorialStep.ToGameplayStep().SelectableTargets.Contains(Enumerators.SkillTarget.OPPONENT))
                return;

            if (player.Defense <= 0)
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(player))
                return;

            if (Owner != null && !Owner.HasFeral && Owner.HasBuffRush)
                return;

            if (TargetsType.Contains(Enumerators.SkillTarget.OPPONENT) &&
                player.AvatarObject.CompareTag(SRTags.OpponentOwned) ||
                TargetsType.Contains(Enumerators.SkillTarget.PLAYER) &&
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
            return BoardCards?.FindAll(x => x.IsHeavyUnit && x.CurrentDefense > 0).Count > 0;
        }

        private void Awake()
        {
            Init();
        }
    }
}

using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground
{
    public class BattleBoardArrow : BoardArrow
    {
        public List<BoardObject> IgnoreBoardObjectsList;

        public List<BoardUnitView> BoardCards;

        public BoardUnitView Owner;

        public bool IgnoreHeavy;

        public Enumerators.UnitStatusType TargetUnitStatusType;

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
                    creature.Model.OwnerPlayer.ThrowCardAttacked(creature.Model.Card, AffectObjectType.Player, -1);
                }
                else
                {
                    creature.Model.OwnerPlayer.ThrowCardAttacked(creature.Model.Card, AffectObjectType.Character, SelectedCard.Model.Card.Id);
                }
            }
            else
            {
                if (TutorialManager.IsTutorial)
                {
                    TutorialManager.ActivateSelectTarget();
                }
            }

            Dispose();
        }

        public override void OnCardSelected(BoardUnitView unit)
        {
            if (TutorialManager.IsTutorial && !TutorialManager.CurrentTutorialDataStep.BoardArrowCanUsableOnUnits)
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(unit.Model))
                return;

            if (unit.Model.CurrentHp <= 0)
                return;

            if (ElementType.Count > 0 && !ElementType.Contains(unit.Model.Card.LibraryCard.CardSetType))
                return;

            if (TargetsType.Contains(Enumerators.SkillTargetType.ALL_CARDS) ||
                TargetsType.Contains(Enumerators.SkillTargetType.PLAYER_CARD) &&
                unit.Transform.CompareTag("PlayerOwned") ||
                TargetsType.Contains(Enumerators.SkillTargetType.OPPONENT_CARD) &&
                unit.Transform.CompareTag("OpponentOwned"))
            {
                bool opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || opponentHasProvoke && unit.Model.IsHeavyUnit || IgnoreHeavy)
                {
                    if (TargetUnitStatusType == Enumerators.UnitStatusType.NONE ||
                        unit.Model.UnitStatus == TargetUnitStatusType)
                    {
                        SelectedCard?.SetSelectedUnit(false);

                        SelectedCard = unit;
                        SelectedPlayer?.SetGlowStatus(false);

                        SelectedPlayer = null;
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
        }

        public override void OnPlayerSelected(Player player)
        {
            if (TutorialManager.IsTutorial && !TutorialManager.CurrentTutorialDataStep.BoardArrowCanUsableOnPlayer)
                return;

            if (player.Health <= 0)
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(player))
                return;

            if (Owner != null && !Owner.Model.HasFeral && Owner.Model.HasBuffRush)
                return;

            if (TargetsType.Contains(Enumerators.SkillTargetType.OPPONENT) &&
                player.AvatarObject.CompareTag("OpponentOwned") ||
                TargetsType.Contains(Enumerators.SkillTargetType.PLAYER) &&
                player.AvatarObject.CompareTag("PlayerOwned"))
            {
                bool opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || IgnoreHeavy)
                {
                    SelectedPlayer = player;

                    SelectedPlayer.SetGlowStatus(true);
                    SelectedCard?.SetSelectedUnit(false);

                    SelectedCard = null;
                }
            }
        }

        public override void OnPlayerUnselected(Player player)
        {
            if (SelectedPlayer == player)
            {
                SelectedCard?.SetSelectedUnit(false);
                SelectedCard = null;
                SelectedPlayer.SetGlowStatus(false);
                SelectedPlayer = null;
            }
        }

        protected bool OpponentBoardContainsProvokingCreatures()
        {
            List<BoardUnitView> provokeCards = BoardCards.FindAll(x => x.Model.IsHeavyUnit);
            return provokeCards.Count > 0;
        }

        private void Awake()
        {
            Init();
        }
    }
}

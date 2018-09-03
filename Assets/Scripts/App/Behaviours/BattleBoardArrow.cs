using System.Collections.Generic;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class BattleBoardArrow : BoardArrow
    {
        public List<object> IgnoreBoardObjectsList;

        public List<BoardUnit> BoardCards;

        public BoardUnit Owner;

        public bool IgnoreHeavy;

        public void End(BoardUnit creature)
        {
            if (!StartedDrag)
                return;

            StartedDrag = false;

            creature.DoCombat(SelectedCard ?? (object)SelectedPlayer);
            Dispose();
        }

        public override void OnCardSelected(BoardUnit unit)
        {
            if (GameplayManager.IsTutorial && (GameplayManager.TutorialStep == 19 || GameplayManager.TutorialStep == 27 || GameplayManager.TutorialStep == 32))
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(unit))
                return;

            if (unit.CurrentHp <= 0)
                return;

            if (ElementType.Count > 0 && !ElementType.Contains(unit.Card.LibraryCard.CardSetType))
                return;

            if (TargetsType.Contains(Enumerators.SkillTargetType.ALL_CARDS) || TargetsType.Contains(Enumerators.SkillTargetType.PLAYER_CARD) && unit.Transform.CompareTag("PlayerOwned") || TargetsType.Contains(Enumerators.SkillTargetType.OPPONENT_CARD) && unit.Transform.CompareTag("OpponentOwned"))
            {
                bool opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || opponentHasProvoke && unit.IsHeavyUnit() || IgnoreHeavy)
                {
                    SelectedCard?.SetSelectedUnit(false);

                    SelectedCard = unit;
                    SelectedPlayer?.SetGlowStatus(false);

                    SelectedPlayer = null;
                    SelectedCard.SetSelectedUnit(true);
                }
            }
        }

        public override void OnCardUnselected(BoardUnit creature)
        {
            if (SelectedCard == creature)
            {
                SelectedCard.SetSelectedUnit(false);
                SelectedCard = null;
            }
        }

        public override void OnPlayerSelected(Player player)
        {
            if (GameplayManager.IsTutorial && GameplayManager.TutorialStep != 19 && GameplayManager.TutorialStep != 28 && GameplayManager.TutorialStep != 32)
                return;

            if (player.Health <= 0)
                return;

            if (IgnoreBoardObjectsList != null && IgnoreBoardObjectsList.Contains(player))
                return;

            if (Owner != null && !Owner.HasFeral && Owner.HasBuffRush)
                return;

            if (TargetsType.Contains(Enumerators.SkillTargetType.OPPONENT) && player.AvatarObject.CompareTag("OpponentOwned") || TargetsType.Contains(Enumerators.SkillTargetType.PLAYER) && player.AvatarObject.CompareTag("PlayerOwned"))
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
            List<BoardUnit> provokeCards = BoardCards.FindAll(x => x.IsHeavyUnit());
            return provokeCards.Count > 0;
        }

        private void Awake()
        {
            Init();
        }
    }
}

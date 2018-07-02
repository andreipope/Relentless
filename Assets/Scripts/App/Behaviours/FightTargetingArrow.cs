// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections.Generic;

namespace LoomNetwork.CZB
{
    public class FightTargetingArrow : TargetingArrow
    {
        public List<BoardCreature> BoardCards;

        public void End(BoardCreature creature)
        {
            if (!startedDrag)
            {
                return;
            }

            startedDrag = false;

            creature.ResolveCombat();
            Destroy(gameObject);
        }

        public override void OnCardSelected(BoardCreature creature)
        {
            if (_gameplayManager.IsTutorial && (_gameplayManager.TutorialStep == 19 || _gameplayManager.TutorialStep == 27))
                return;

            if (targetsType.Contains(Common.Enumerators.SkillTargetType.ALL_CARDS) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.PLAYER_CARD) && creature.transform.CompareTag("PlayerOwned")) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.OPPONENT_CARD) && creature.transform.CompareTag("OpponentOwned")) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.OPPONENT) && creature.transform.CompareTag("OpponentOwned")) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.PLAYER) && creature.transform.CompareTag("PlayerOwned")))
            {
                var opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || (opponentHasProvoke && creature.Card.libraryCard.cardType == Common.Enumerators.CardType.HEAVY))
                {
                    selectedCard = creature;
                    selectedPlayer = null;
                    CreateTarget(creature.transform.position);
                }
            }
        }

        public override void OnCardUnselected(BoardCreature creature)
        {
            if (selectedCard == creature)
            {
                Destroy(target);
                selectedCard = null;
            }
        }

        public override void OnPlayerSelected(Player player)
        {
            if (_gameplayManager.IsTutorial && (_gameplayManager.TutorialStep != 19 &&
                                                _gameplayManager.TutorialStep != 28 &&
                                                _gameplayManager.TutorialStep != 29))
                return;

            if (targetsType.Contains(Common.Enumerators.SkillTargetType.ALL_CARDS) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.PLAYER_CARD) && player.AvatarObject.CompareTag("PlayerOwned")) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.OPPONENT_CARD) && player.AvatarObject.CompareTag("OpponentOwned")) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.OPPONENT) && player.AvatarObject.CompareTag("OpponentOwned")) ||
                (targetsType.Contains(Common.Enumerators.SkillTargetType.PLAYER) && player.AvatarObject.CompareTag("PlayerOwned")))
            {
                var opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke)
                {
                    selectedPlayer = player;
                    selectedCard = null;
                    CreateTarget(player.AvatarObject.transform.position);
                }
            }
        }

        public override void OnPlayerUnselected(Player player)
        {
            if (selectedPlayer == player)
            {
                Destroy(target);
                selectedPlayer = null;
            }
        }

        protected bool OpponentBoardContainsProvokingCreatures()
        {
            var provokeCards = BoardCards.FindAll(x => x.Card.libraryCard.cardType == Common.Enumerators.CardType.HEAVY);
            return provokeCards.Count > 0;
        }
    }
}
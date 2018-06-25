using System.Collections.Generic;


namespace GrandDevs.CZB
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

            if (targetType == EffectTarget.AnyPlayerOrCreature ||
                targetType == EffectTarget.TargetCard ||
                (targetType == EffectTarget.PlayerOrPlayerCreature && creature.tag == "PlayerOwned") ||
                (targetType == EffectTarget.OpponentOrOpponentCreature && creature.tag == "OpponentOwned") ||
                (targetType == EffectTarget.PlayerCard && creature.tag == "PlayerOwned") ||
                (targetType == EffectTarget.OpponentCard && creature.tag == "OpponentOwned"))
            {
                var opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || (opponentHasProvoke && creature.Card.cardType == Common.Enumerators.CardType.HEAVY))
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

        public override void OnPlayerSelected(PlayerAvatar player)
        {
            if (_gameplayManager.IsTutorial && (_gameplayManager.TutorialStep != 19 &&
                                                _gameplayManager.TutorialStep != 28 &&
                                                _gameplayManager.TutorialStep != 29))
                return;

            if (targetType == EffectTarget.AnyPlayerOrCreature ||
                targetType == EffectTarget.TargetPlayer ||
                (targetType == EffectTarget.PlayerOrPlayerCreature && player.tag == "PlayerOwned") ||
                (targetType == EffectTarget.OpponentOrOpponentCreature && player.tag == "OpponentOwned") ||
                (targetType == EffectTarget.Player && player.tag == "PlayerOwned") ||
                (targetType == EffectTarget.Opponent && player.tag == "OpponentOwned"))
            {
                var opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke)
                {
                    selectedPlayer = player;
                    selectedCard = null;
                    CreateTarget(player.transform.position);
                }
            }
        }

        public override void OnPlayerUnselected(PlayerAvatar player)
        {
            if (selectedPlayer == player)
            {
                Destroy(target);
                selectedPlayer = null;
            }
        }

        protected bool OpponentBoardContainsProvokingCreatures()
        {
            var provokeCards = BoardCards.FindAll(x => x.Card.cardType == Common.Enumerators.CardType.HEAVY);
            return provokeCards.Count > 0;
        }
    }
}
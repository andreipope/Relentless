// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using CCGKit;

public class FightTargetingArrow : TargetingArrow
{
    public RuntimeZone opponentBoardZone;

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
        if (targetType == EffectTarget.AnyPlayerOrCreature ||
            targetType == EffectTarget.TargetCard ||
            (targetType == EffectTarget.PlayerOrPlayerCreature && creature.tag == "PlayerOwned") ||
            (targetType == EffectTarget.OpponentOrOpponentCreature && creature.tag == "OpponentOwned") ||
            (targetType == EffectTarget.PlayerCard && creature.tag == "PlayerOwned") ||
            (targetType == EffectTarget.OpponentCard && creature.tag == "OpponentOwned"))
        {
            var opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
            if (!opponentHasProvoke || (opponentHasProvoke && creature.card.HasKeyword("Provoke")))
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
        var provokeCards = opponentBoardZone.cards.FindAll(x => x.HasKeyword("Provoke"));
        return provokeCards.Count > 0;
    }
}